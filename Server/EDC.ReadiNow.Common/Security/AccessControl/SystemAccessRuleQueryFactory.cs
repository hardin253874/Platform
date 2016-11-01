// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using EDC.Database.Types;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    internal class SystemAccessRuleQueryFactory : IAccessRuleQueryFactory
    {
        /// <summary>
        ///     Entity id to allocate for in memory access rules. Note: these will be negative so that they
        ///     are in a totally separate space to actual entities.
        /// </summary>
        private static long _entityId;
        

        /// <summary>
        ///     Access rule definitions.
        /// </summary>
        private readonly Lazy<List<SystemAccessRuleDefinition>> _systemAccessRuleDefinitions = new Lazy<List<SystemAccessRuleDefinition>>(CreateSystemAccessRuleDefinitions,
            LazyThreadSafetyMode.ExecutionAndPublication);


        /// <summary>
        ///     Return a dictionary of system access rules for the current tenant.
        /// </summary>
        /// <returns></returns>
        public IDictionary<SubjectPermissionTuple, IList<AccessRuleQuery>> GetQueries()
        {
            if (!RequestContext.IsSet)
            {
                throw new InvalidOperationException("RequestContext not set.");
            }

            var result = new Dictionary<SubjectPermissionTuple, IList<AccessRuleQuery>>();

            using (new SecurityBypassContext())
            {
                foreach (var accessRuleDefinition in _systemAccessRuleDefinitions.Value)
                {
                    var subjectId = GetEntityId(accessRuleDefinition.Subject);
                    if (subjectId <= 0)
                    {
                        continue;
                    }

                    foreach (var entityTypeAlias in accessRuleDefinition.SecuredEntityTypes)
                    {
                        var entityTypeId = GetEntityId(entityTypeAlias);
                        if (entityTypeId <= 0)
                        {
                            continue;
                        }

                        foreach (var getStructuredQueryFunc in accessRuleDefinition.GetStructuredQueryFuncs)
                        {
                            var structuredQuery = getStructuredQueryFunc(entityTypeAlias);

                            ConvertArgumentAliasesToIds(structuredQuery);

                            foreach (var permissionAlias in accessRuleDefinition.Permissions)
                            {
                                var permissionId = GetEntityId(permissionAlias);

                                if (permissionId <= 0)
                                {
                                    continue;
                                }

                                var key = new SubjectPermissionTuple(subjectId, permissionId);

                                IList<AccessRuleQuery> accessRuleList;

                                if (!result.TryGetValue(key, out accessRuleList))
                                {
                                    accessRuleList = new List<AccessRuleQuery>();
                                    result[key] = accessRuleList;
                                }

                                accessRuleList.Add(new AccessRuleQuery(GetNextEntityId(), GetNextEntityId(), entityTypeId, structuredQuery, accessRuleDefinition.IgnoreForReports));
                            }
                        }                        
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the argument aliases to ids.
        /// </summary>
        /// <param name="structuredQuery">The structured query.</param>
        private void ConvertArgumentAliasesToIds(StructuredQuery structuredQuery)
        {
            if (structuredQuery?.Conditions == null || structuredQuery.Conditions.Count == 0)
            {
                return;
            }

            foreach (var condition in structuredQuery.Conditions)
            {
                if (condition.Arguments == null || condition.Arguments.Count == 0)
                {
                    continue;
                }

                foreach (var arg in condition.Arguments)
                {
                    if (string.IsNullOrWhiteSpace(arg.ValueString) || (!(arg.Type is ChoiceRelationshipType) && !(arg.Type is InlineRelationshipType))) continue;

                    long id;
                    if (EntityIdentificationCache.TryGetId(new EntityAlias(arg.ValueString), out id))
                    {
                        arg.Value = id;
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static Func<string, StructuredQuery> GetCreateStructuredQueryFunc(string methodName)
        {
            var typeAliasParam = Expression.Parameter(typeof(string), "typeAlias");

            var callExpression = Expression.Call(typeof(SystemAccessRules), methodName, null, typeAliasParam);

            var lambda = Expression.Lambda(callExpression, typeAliasParam);

            var typedExpression = (Expression<Func<string, StructuredQuery>>) lambda;

            return typedExpression.Compile();
        }

        /// <summary>
        ///     Create the access rule definitions
        /// </summary>
        /// <returns></returns>
        private static List<SystemAccessRuleDefinition> CreateSystemAccessRuleDefinitions()
        {
            var enableAppLockdown = !Factory.FeatureSwitch.Get("disableAppLockdown");      

            var thisType = typeof(SystemAccessRules);

            var fields = thisType.GetFields(BindingFlags.NonPublic | BindingFlags.Static);

            var accessRuleDefinitions = new List<SystemAccessRuleDefinition>();

            foreach (var field in fields)
            {                
                if (enableAppLockdown)
                {
                    // If app lockdown is enabled do not process the everyone has all access attribute
                    if (field.Name == "EveryoneAllFeatureSwitched")
                    {
                        continue;
                    }
                }                    

                var accessRuleQueryAttributes = field.GetCustomAttributes<AccessRuleQueryDefinitionAttribute>();

                var accessRuleQueryDefinitonAttributes = accessRuleQueryAttributes as IList<AccessRuleQueryDefinitionAttribute> ?? accessRuleQueryAttributes.ToList();
                if (accessRuleQueryAttributes == null || !accessRuleQueryDefinitonAttributes.Any())
                {
                    continue;
                }

                var entityTypesValue = field.GetValue(null) as IEnumerable<string>;
                if (entityTypesValue == null)
                {
                    continue;
                }

                var entityTypes = new List<string>(entityTypesValue);

                foreach (var accessRuleQueryAttribute in accessRuleQueryDefinitonAttributes)
                {
                    if (string.IsNullOrWhiteSpace(accessRuleQueryAttribute?.Subject) ||
                        accessRuleQueryAttribute.Permissions == null ||
                        accessRuleQueryAttribute.GetStructuredQueryMethodNames == null)
                    {
                        continue;
                    }

                    var getQueryMethodFuncs = accessRuleQueryAttribute.GetStructuredQueryMethodNames.Select(GetCreateStructuredQueryFunc);
                    var accessRuleDefinition = new SystemAccessRuleDefinition(accessRuleQueryAttribute.Subject, accessRuleQueryAttribute.Permissions, entityTypes, getQueryMethodFuncs, accessRuleQueryAttribute.IgnoreForReports);

                    accessRuleDefinitions.Add(accessRuleDefinition);
                }
            }

            return accessRuleDefinitions;
        }

        /// <summary>
        ///     Get the next entity id.
        /// </summary>
        /// <returns></returns>
        private static long GetNextEntityId()
        {
            return Interlocked.Decrement(ref _entityId);
        }

        

        /// <summary>
        ///     Gets an entity id from the specified alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        private long GetEntityId(string alias)
        {
            long id;

            if (!EntityIdentificationCache.TryGetId(new EntityAlias(alias), out id))
            {
                return -1;
            }

            return id;
        }        

        /// <summary>
        ///     This class represents an access rule definition.
        /// </summary>
        private class SystemAccessRuleDefinition
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="subject">Alias of the subject</param>
            /// <param name="permissions">List of permission aliases.</param>
            /// <param name="securedEntityTypes"></param>
            /// <param name="getStructuredQueryFuncs">Functions to create a structured queries.</param>
            /// <param name="ignoreForReports"></param>
            public SystemAccessRuleDefinition(string subject, IEnumerable<string> permissions, IEnumerable<string> securedEntityTypes,
                IEnumerable<Func<string, StructuredQuery>> getStructuredQueryFuncs, bool ignoreForReports)
            {
                if (string.IsNullOrEmpty(subject))
                {
                    throw new ArgumentNullException(nameof(subject));
                }

                if (permissions == null)
                {
                    throw new ArgumentNullException(nameof(permissions));
                }

                if (securedEntityTypes == null)
                {
                    throw new ArgumentNullException(nameof(securedEntityTypes));
                }

                if (getStructuredQueryFuncs == null)
                {
                    throw new ArgumentNullException(nameof(getStructuredQueryFuncs));
                }

                Subject = subject;
                Permissions = new List<string>(permissions);
                SecuredEntityTypes = new List<string>(securedEntityTypes);
                GetStructuredQueryFuncs = new List<Func<string, StructuredQuery>>(getStructuredQueryFuncs);
                IgnoreForReports = ignoreForReports;
            }

            /// <summary>
            ///     Gets the subject.
            /// </summary>
            public string Subject { get; }

            /// <summary>
            ///     Gets the permissions.
            /// </summary>
            public IEnumerable<string> Permissions { get; }

            /// <summary>
            ///     Gets the secured entity types.
            /// </summary>
            public IEnumerable<string> SecuredEntityTypes { get; }

            /// <summary>
            ///     Gets the structured query creation functions.
            /// </summary>
            public IEnumerable<Func<string, StructuredQuery>> GetStructuredQueryFuncs { get; }

            /// <summary>
            /// Gets or sets a value indicating whether to ignore for reports.
            /// </summary>
            /// <value>
            ///   <c>true</c> if ignore for reports; otherwise, <c>false</c>.
            /// </value>
            public bool IgnoreForReports { get; }
        }
    }
}