// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using EDC.Collections.Generic;
using EDC.Database;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using Entity = EDC.ReadiNow.Model.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Core;
using EDC.Common;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Specify whether the user has the requested access to the requested entities.
    /// </summary>
    public class EntityAccessControlChecker : IEntityAccessControlChecker
    {
        /// <summary>
        /// The maximum number of filtered entities.
        /// If the number of entities being secured is less than this number
        /// the query will be filtered with these entities, otherwise
        /// the query will return all entities.
        /// </summary>
        private const int MaximumNumberOfFilteredEntities = 500;

        /// <summary>
        /// Lazy eval if security is disabled.
        /// </summary>
        private static Lazy<bool> _securityDisabled = new Lazy<bool>(
            ( ) => EDC.ReadiNow.Configuration.ConfigurationSettings.GetServerConfigurationSection( ).Security.Disabled );

        /// <summary>
        /// Create a new <see cref="EntityAccessControlChecker"/>.
        /// </summary>
        public EntityAccessControlChecker()
            : this(new CachingUserRoleRepository(new UserRoleRepository()), new QueryRepository(), new EntityTypeRepository())
        {
            // Do nothing   
        }

        /// <summary>
        /// Default access rule checker
        /// </summary>
        protected virtual string Name => "default";

        /// <summary>
        /// Create a new <see cref="EntityAccessControlChecker"/>.
        /// </summary>
        /// <param name="roleRepository">
        /// Used to load roles for a given user. This cannot be null.
        /// </param>
        /// <param name="queryRepository">
        /// Used to load queries for a role and permission. This cannot be null.
        /// </param>
        /// <param name="entityTypeRepository">
        /// Used to load the types of each entity. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        internal EntityAccessControlChecker(IUserRoleRepository roleRepository, IQueryRepository queryRepository,
            IEntityTypeRepository entityTypeRepository)
        {
            if (roleRepository == null)
            {
                throw new ArgumentNullException("roleRepository");
            }
            if (queryRepository == null)
            {
                throw new ArgumentNullException("queryRepository");
            }
            if (entityTypeRepository == null)
            {
                throw new ArgumentNullException("entityTypeRepository");
            }

            RoleRepository = roleRepository;
            QueryRepository = queryRepository;
            EntityTypeRepository = entityTypeRepository;
        }

        /// <summary>
        /// The <see cref="IUserRoleRepository"/> used to load roles for a given user.
        /// </summary>
        public IUserRoleRepository RoleRepository { get; private set; }

        /// <summary>
        /// The <see cref="QueryRepository"/> used to load queries for the subjects and operations.
        /// </summary>
        public IQueryRepository QueryRepository { get; private set; }

        /// <summary>
        /// The <see cref="EntityTypeRepository"/> used to get the types of entities.
        /// </summary>
        public IEntityTypeRepository EntityTypeRepository { get; private set; }

        /// <summary>
        /// Check whether the user has all the specified 
        /// <paramref name="permissions">access</paramref> to the specified <paramref name="entities"/>.
        /// </summary>
        /// <param name="entities">
        ///     The entities to check. This cannot be null or contain null.
        /// </param>
        /// <param name="permissions">
        ///     The permissions or operations to check. This cannot be null or contain null.
        /// </param>
        /// <param name="user">
        ///     The user requesting access. This cannot be null.
        /// </param>
        /// <returns>
        /// A mapping of each entity ID to whether the user has access (true) or not (false).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Neither <paramref name="entities"/> nor <paramref name="permissions"/> can contain null.
        /// </exception>
        public IDictionary<long, bool> CheckAccess(IList<EntityRef> entities, IList<EntityRef> permissions, EntityRef user)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            if (permissions == null)
            {
                throw new ArgumentNullException("permissions");
            }
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            IDictionary<long, bool> result;
            ISet<long> subjects;
            IDictionary<long, ISet<EntityRef>> entityTypes;
            Dictionary<long, IDictionary<long, bool>> permissionToAccess;
            // Dictionary keyed off report id to entities
            Dictionary<long, ISet<long>> queryResultsCache;
            ISet<long> allEntities;

            if (SkipCheck(user))
            {
                result = SetAll(entities.Select(e => e.Id), true);
            }
            else if (entities.Count == 0)
            {
                result = new Dictionary<long, bool>();
            }
            else
            {
                using (new SecurityBypassContext())
                {
                    subjects = GetSubjects(user);

                    using (MessageContext messageContext = new MessageContext(EntityAccessControlService.MessageName))
                    {
                        messageContext.Append(() => "Checking access rules:");

                        entityTypes = null;
                        permissionToAccess = new Dictionary<long, IDictionary<long, bool>>();
                        queryResultsCache = new Dictionary<long, ISet<long>>();
                        allEntities = null;

                        foreach (EntityRef permission in permissions)
                        {
                            permissionToAccess[permission.Id] = SetAll(entities.Select(e => e.Id), false);

                            if (allEntities == null &&
                                !permission.Equals(Permissions.Create))
                            {
                                // Add all the entities to a set
                                allEntities = new HashSet<long>(entities.Select(e => e.Id));
                            }

                            foreach (long subject in subjects)
                            {
                                if (entityTypes == null)
                                {
                                    entityTypes = EntityTypeRepository.GetEntityTypes(entities);
                                }

                                bool accessGrantedToAll = false;

                                // entityType maps a sorted list of type ids to instance entity Refs
                                foreach (KeyValuePair<long, ISet<EntityRef>> entityType in entityTypes)
                                {
                                    using (MessageContext perTypeMessageContext = new MessageContext(EntityAccessControlService.MessageName))
                                    {
                                        perTypeMessageContext.Append(() =>
                                            ConstructCheckMessage(entities, subjects, entityTypes, subject, entityType));

                                        using (new MessageContext(EntityAccessControlService.MessageName))
                                        {
                                            // Automatically allow read access (only) to fields, relationships and types
                                            CheckAutomaticAccess(permission, entityType, permissionToAccess);

                                            if (EntityRefComparer.Instance.Equals(permission, Permissions.Create))
                                            {
                                                CheckAccessControlByRelationship(
                                                    subject,
                                                    permission,
                                                    entityType.Key,
                                                    entityType.Value.ToList(),
                                                    permissionToAccess[permission.Id]);
                                            }
                                            else
                                            {
                                                CheckAccessControlByQuery(
                                                    subject,
                                                    permission,
                                                    entityType.Key,
                                                    entityType.Value.ToList(),
                                                    allEntities, queryResultsCache,
                                                    permissionToAccess[permission.Id]);
                                            }

                                            // Skip remaining checks if access is granted to all requested entities 
                                            // for the current permission.
                                            if (permissionToAccess[permission.Id].All(kvp => kvp.Value))
                                            {
                                                messageContext.Append(
                                                    () => "Access granted to all entities. Not checking additional access rules.");
                                                accessGrantedToAll = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                // Skip remaining checks if access is granted to all requested entities 
                                // for the current permission.
                                if (accessGrantedToAll || permissionToAccess[permission.Id].All(kvp => kvp.Value))
                                {
                                    if (!accessGrantedToAll)
                                    {
                                        messageContext.Append(() => "Access granted to all entities. Not checking additional access rules.");
                                    }
                                    break;
                                }
                            }
                        }

                        result = CollateAccess(entities, permissionToAccess);
                        result = AllowAccessToTypelessIds(result, entityTypes);

                        // Add all the containing roles to the cache so a role change invalidates this
                        // user's security cache entries.
                        using (CacheContext cacheContext = CacheContext.GetContext())
                        {
                            cacheContext.Entities.Add(subjects);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Construct the message written to the <see cref="MessageContext"/>.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="subjects"></param>
        /// <param name="entityTypes"></param>
        /// <param name="subject"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private string ConstructCheckMessage(IList<EntityRef> entities, ISet<long> subjects, IDictionary<long, ISet<EntityRef>> entityTypes,
            long subject, KeyValuePair<long, ISet<EntityRef>> entityType)
        {
            EntityRef nameField;

            nameField = new EntityRef("core", "name");

            // Preload names of referenced entities, subjects and types
            Entity.GetField<string>(subjects.Select(e => new EntityRef(e)), nameField);
            // This was failing because the entities were instances of EntityTypeOnly which were causing failures when passes into Entity.GetField
            Entity.GetField<string>(entities.Where(e => !EntityId.IsTemporary(e.Id)).Select(e => new EntityRef(e.Id)), nameField);
            Entity.GetField<string>(entityTypes.Keys.Select(e => new EntityRef(e)), nameField);

            return string.Format(
                "Checking access rules for {0} to entities {1} of type {2}:",
                string.Format("'{0}' ({1})", Entity.GetName(subject), subject),
                string.Join(
                    ", ",
                    entityType.Value.Select(er => string.Format("'{0}' ({1})", Entity.GetName(er.Id), er.Id))),
                string.Join(
                    ", ",
                    string.Format("'{0}' ({1})", Entity.GetName(entityType.Key), entityType.Key)));
        }

        /// <summary>
        /// Is there an access rule for the specified type(s) that includes the requested permission? E.g. create.
        /// </summary>
        /// <param name="entityTypes">The <see cref="EntityType"/>s to check. This cannot be null or contain null.</param>
        /// <param name="permission">The permission being sought.</param>
        /// <param name="user"> The user requesting access. This cannot be null. </param>
        /// <returns>
        /// A mapping the entity type ID to whether the user can create instances of that 
        /// type (true) or not (false).
        /// </returns>
        public IDictionary<long, bool> CheckTypeAccess(IList<EntityType> entityTypes, EntityRef permission, EntityRef user)
        {
            if ( entityTypes == null )
                throw new ArgumentNullException( nameof( entityTypes ) );
            if ( permission == null )
                throw new ArgumentNullException( nameof( permission ) );
            if ( user == null )
                throw new ArgumentNullException( nameof( user ) );

            ISet<long> subjectIds;
            IDictionary<long, bool> result;

            if (SkipCheck(user))
            {
                result = SetAll(entityTypes.Select(et => et.Id), true);
                return result;
            }

            using (new SecurityBypassContext())
            {                
                subjectIds = GetSubjects(user);

                result = SetAll(entityTypes.Select(et => et.Id), false);

                // Cache of ancestor and self for each type id
                var entityTypesToAncestorsAndSelf = new Dictionary<long, ISet<long>>();

                foreach (Subject subject in Entity.Get<Subject>(subjectIds))
                {                    
                    // Get the createable entity types for this subject
                    var creatableEntityTypes = AllowedEntityTypes( permission, subject);

                    foreach (var entityType in entityTypes)
                    {
                        long typeId = entityType.Id;
                        bool haveAccess;
                        
                        result.TryGetValue(typeId, out haveAccess);                        
                        if (haveAccess)
                        {
                            // Already have access continue
                            continue;
                        }

                        // Get the ancestors and self for this type.
                        ISet<long> ancestorsAndSelf;
                        if (!entityTypesToAncestorsAndSelf.TryGetValue(typeId, out ancestorsAndSelf))
                        {
                            // No ancestors for this type. Get and cache.
                            ancestorsAndSelf = new HashSet<long>(entityType.GetAncestorsAndSelf().Select(et => et.Id));                            
                            entityTypesToAncestorsAndSelf[typeId] = ancestorsAndSelf;
                        }

                        if (ancestorsAndSelf == null || ancestorsAndSelf.Count == 0)
                        {
                            continue;
                        }

                        // Check if createable type contains and of the ancestors and self
                        bool canAccess = creatableEntityTypes.Any(et => ancestorsAndSelf.Contains(et));
                        if ( canAccess )
                        {
                            result[typeId] = true;
                        }
                    }                    

                    // Short circuit if the user has access to all types
                    if (result.Values.All( canAccess => canAccess ) )
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Should a security check be performed?
        /// </summary>
        /// <param name="user">
        /// The user to check. This cannot be null.
        /// </param>
        /// <returns>
        /// True if a security check should be skipped, false otherwise.
        /// </returns>
        public static bool SkipCheck(EntityRef user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return SecurityBypassContext.IsActive || IsGlobalTenant( user ) || _securityDisabled.Value;
        }

        /// <summary>
        /// Is it the global tenant user?
        /// </summary>
        /// <param name="user">
        /// The <see cref="EntityRef"/> of the user to check.
        /// </param>
        /// <returns>
        /// True if it is the global tenant user, false otherwise.
        /// </returns>
        internal static bool IsGlobalTenant(EntityRef user)
        {
            return (!user.HasId && !user.HasEntity && user.Alias == null && user.Namespace == null);
        }

        /// <summary>
        /// Grant access to unknown IDs (to keep synergy with old type system).
        /// </summary>
        /// <param name="mapping">
        /// The mapping of entity ID to whether the user has access (true) or not (false).
        /// This cannot be null.
        /// </param>
        /// <param name="entityTypes">
        /// A mapping of a list of type IDs to entity IDs of that type. This may be null.
        /// </param>
        /// <returns>
        /// The corrected mapping, allowing access to invalid or unknown IDs.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="mapping"/> cannot be null.
        /// </exception>
        internal IDictionary<long, bool> AllowAccessToTypelessIds(IDictionary<long, bool> mapping, IDictionary<long, ISet<EntityRef>> entityTypes)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping");
            }

            var result = new Dictionary<long, bool>(mapping);

            if (entityTypes != null)
            {
                foreach (KeyValuePair<long, ISet<EntityRef>> entityType in
                    entityTypes.Where(kvp => kvp.Key == AccessControl.EntityTypeRepository.TypelessId))
                {
                    foreach (long id in entityType.Value.Select(x => x.Id))
                    {
                        result[id] = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Grant access to any temporary IDs.
        /// </summary>
        /// <param name="mapping">
        /// The mapping of entity ID to whether the user has access (true) or not (false).
        /// </param>
        /// <returns>
        /// True if granted access to all ids, false otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="mapping"/> cannot be null.
        /// </exception>
        internal bool AllowAccessToTemporaryIds(IDictionary<long, bool> mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping");
            }

            bool allHaveAccess = false;

            var keysArray = mapping.Keys.ToArray();
            if (keysArray.Length > 0)
            {
                // Have keys. Assume all have access
                allHaveAccess = true;
            }

            foreach (long id in keysArray)
            {
                bool allowAccess = mapping[id] || EntityId.IsTemporary(id);                
                mapping[id] = allowAccess;

                if (allHaveAccess && !allowAccess)
                {
                    allHaveAccess = false;
                }
            }

            return allHaveAccess;
        }

        /// <summary>
        /// Check whether the queries for the specified <paramref name="permission"/> allow the <paramref name="subjectId"/>
        /// access to <paramref name="entities"/> using the related security queries. Used for read, modify and delete
        /// permissions.
        /// </summary>
        /// <param name="subjectId">
        /// The ID of the subject (user or role).
        /// </param>
        /// <param name="permission">
        /// The permission (operation). This cannot be null.
        /// </param>
        /// <param name="entityType">
        /// The type ID of the entities to check. This cannot be null.
        /// </param>
        /// <param name="entities">
        /// The entities to check. This cannot be null or contain null.
        /// </param>
        /// <param name="allEntities">
        /// All entities.
        /// </param>
        /// <param name="queryResultsCache">
        /// The query results cache.
        /// </param>
        /// <param name="result">
        /// The map of entity IDs to whether the relationship exists.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        internal void CheckAccessControlByQuery(long subjectId, EntityRef permission,
            long entityType, IList<EntityRef> entities, ISet<long> allEntities,
            IDictionary<long, ISet<long>> queryResultsCache, IDictionary<long, bool> result)
        {
            using (Profiler.MeasureAndSuppress("CheckAccessControlByQuery"))
            {
                if (permission == null)
                {
                    throw new ArgumentNullException("permission");
                }
                if (result == null)
                {
                    throw new ArgumentNullException("result");
                }
                if (entities == null)
                {
                    throw new ArgumentNullException("entities");
                }
                if (allEntities == null)
                {
                    throw new ArgumentNullException("allEntities");
                }
                if (queryResultsCache == null)
                {
                    throw new ArgumentNullException("queryResultsCache");
                }
                if (entities.Contains(null))
                {
                    throw new ArgumentException("Cannot check access for null entities", "entities");
                }

                IEnumerable<AccessRuleQuery> queries;

                // Allow access to temporary IDs
                if (AllowAccessToTemporaryIds(result))
                {
                    return;
                }

                using (MessageContext messageContext = new MessageContext(EntityAccessControlService.MessageName))
                {
                    queries = QueryRepository.GetQueries(subjectId, permission, new[] { entityType });
                    QueryResult queryResult;

                    // Check if any queries grant access to all instances of the type
                    StructuredQuery shortCircuitQuery = CheckIfAnyQueryProvideAccessToAllInstancesOfType(entityType, queries, entities, result);
                    if (shortCircuitQuery != null)
                    {
                        messageContext.Append(
                                () => string.Format(
                                "{0} allowed '{1}' access to entities '{2}' because it allows access to all instances of the type.",
                                AccessControlDiagnosticsHelper.GetAccessRuleName(shortCircuitQuery),
                                Permissions.GetPermissionByAlias(permission),
                                string.Join(", ", entities.Select(x => x.ToString()))));
                        return;
                    }
                    
                    long securityOwnerRelId = WellKnownAliases.CurrentTenant.SecurityOwner;

                    foreach (AccessRuleQuery accessRuleQuery in queries)
                    {
                        StructuredQuery structuredQuery = accessRuleQuery.Query;
                        var allowedEntities = new HashSet<long>();
                        ISet<long> queryResultSet;

                        if (!queryResultsCache.TryGetValue(accessRuleQuery.ReportId, out queryResultSet))
                        {
                            var querySettings = new QuerySettings
                            {
                                SecureQuery = false,
                                Hint = "security - " + Name
                            };

                            bool filtered = false;

                            if (allEntities.Count <= MaximumNumberOfFilteredEntities)
                            {
                                filtered = true;
                                querySettings.SupportRootIdFilter = true;
                                querySettings.RootIdFilterList = allEntities;
                            }

                            queryResult = null;
                            try
                            {
                                using (MessageContext msg = new MessageContext("Reports", MessageContextBehavior.New))
                                {
                                    queryResult = Factory.QueryRunner.ExecuteQuery(structuredQuery, querySettings);
                                }
                            }
                            catch (Exception ex)
                            {
                                AccessControlDiagnosticsHelper.WriteInvalidSecurityReportMessage(structuredQuery, messageContext, ex);
                            }

                            queryResultSet = new HashSet<long>();

                            if (queryResult != null && QueryInspector.IsQueryUndamaged(structuredQuery))
                            {
                                foreach (DataRow dataRow in queryResult.DataTable.Rows)
                                {
                                    var id = dataRow.Field<long>(0);
                                    if (filtered || allEntities.Contains(id))
                                    {
                                        queryResultSet.Add(id);
                                    }
                                }
                            }
                            else
                            {
                                if (queryResult != null)
                                {
                                    AccessControlDiagnosticsHelper.WriteInvalidSecurityReportMessage(structuredQuery, messageContext);
                                }
                            }

                            queryResultsCache[accessRuleQuery.ReportId] = queryResultSet;
                        }

                        foreach (EntityRef entityRef in entities)
                        {
                            if (queryResultSet.Contains(entityRef.Id) &&
                                result.ContainsKey(entityRef.Id))
                            {
                                allowedEntities.Add(entityRef.Id);
                                result[entityRef.Id] = true;
                            }
                        }

                        // ReSharper disable AccessToForEachVariableInClosure
                        // ReSharper disable SpecifyACultureInStringConversionExplicitly
                        if (allowedEntities.Count > 0)
                        {
                            messageContext.Append(
                                () => string.Format(
                                "{0} allowed '{1}' access to entities '{2}' out of '{3}'",
                                AccessControlDiagnosticsHelper.GetAccessRuleName(structuredQuery),
                                Permissions.GetPermissionByAlias(permission),
                                string.Join(", ",
                                    allowedEntities.Select(x => x.ToString())),
                                string.Join(", ", entities.Select(x => x.ToString()))));
                        }
                        else
                        {
                            messageContext.Append(
                                () => string.Format(
                                    "{0} returned no results for '{1}' access to entities '{2}'",
                                    AccessControlDiagnosticsHelper.GetAccessRuleName(structuredQuery),
                                    Permissions.GetPermissionByAlias(permission),
                                    string.Join(", ", entities.Select(x => x.ToString()))));
                        }
                        // ReSharper restore AccessToForEachVariableInClosure
                        // ReSharper restore SpecifyACultureInStringConversionExplicitly

                        // Set the cache invalidation information
                        using (CacheContext cacheContext = CacheContext.GetContext())
                        {                            
                            // ******************* TEMPORARY WORKAROUND ***********************
                            // Until we properly implement filtering the invalidating relationships and fields by type
                            // we will ignore invalidating on the security owner relationship

                            cacheContext.RelationshipTypes.Add(
                                StructuredQueryHelper.GetReferencedRelationships(structuredQuery).Where(er => er.Id != securityOwnerRelId).Select(er => er.Id));
                            cacheContext.FieldTypes.Add(
                                StructuredQueryHelper.GetReferencedFields(structuredQuery, true, true).Select(er => er.Id));
                        }                        
                    }
                }                
            }
        }

        /// <summary>
        /// Check if any of the queries grant access to all instances of the provided types.
        /// If so, update all results as true.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="queries"></param>
        /// <param name="entities"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static StructuredQuery CheckIfAnyQueryProvideAccessToAllInstancesOfType(long entityType, IEnumerable<AccessRuleQuery> queries,
            IList<EntityRef> entities, IDictionary<long, bool> result)
        {
            foreach (AccessRuleQuery accessRuleQuery in queries)
            {
                StructuredQuery structuredQuery = accessRuleQuery.Query;

                if (!QueryInspector.IsQueryUndamaged(structuredQuery))
                {
                    continue; // note: this gets logged by CheckAccessControlByQuery
                }

                bool queryGrantsAll = accessRuleQuery.DoesQueryGrantAllOfTypes( entityType );

                if (queryGrantsAll)
                {
                    foreach (EntityRef entityRef in entities)
                    {
                        long id = entityRef.Id;
                        if (result.ContainsKey(id))
                        {
                            result[id] = true;
                        }
                    }
                    return structuredQuery;
                }
            }
            return null;
        }

        /// <summary>
        /// Check whether the specified <paramref name="permission"/> exists between the <paramref name="subjectId"/>
        /// and the <paramref name="entities"/> using just a relationship. Used for create permission.
        /// </summary>
        /// <param name="subjectId">
        /// The ID of the subject (user or role).
        /// </param>
        /// <param name="permission">
        /// The permission (operation). This cannot be null.
        /// </param>
        /// <param name="entityType">
        /// The type of the checked entities.
        /// </param>
        /// <param name="entities">
        /// The checked entities. This cannot be null or contain null.
        /// </param>
        /// <param name="result">
        /// The map of entity IDs to whether the relationship exists.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="entities"/> cannot contain null.
        /// </exception>
        internal void CheckAccessControlByRelationship(long subjectId, EntityRef permission,
            long entityType, IList<EntityRef> entities, IDictionary<long, bool> result)
        {
            if (permission == null)
            {
                throw new ArgumentNullException("permission");
            }
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            if (entities.Contains(null))
            {
                throw new ArgumentException(@"Entities cannot contain null", "entities");
            }
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            Subject subject;
            IEnumerable<long> allowedEntityTypes;

            bool containType;
            using (new SecurityBypassContext())
            {
                subject = Entity.Get<Subject>(new EntityRef(subjectId));
                allowedEntityTypes = AllowedEntityTypes(permission, subject);
                
                containType = allowedEntityTypes.Contains(entityType);
            }

            if ( containType )
            {
                foreach (EntityRef entity in entities)
                {
                    result[entity.Id] = true;
                }

                using (MessageContext messageContext = new MessageContext(EntityAccessControlService.MessageName))
                {
                    messageContext.Append(() => string.Format(
                        "'{0}' access to entities '{1}' allowed",
                        Permissions.GetPermissionByAlias(permission),
                        string.Join(", ", entities.Select(x => x.ToString()))));
                }
            }
        }

        /// <summary>
        /// Check whether the user has automatic or hardcoded access to the entities in <paramref name="entityType"/>.
        /// </summary>
        /// <param name="permission">
        /// The <see cref="Permission"/> being checked. This cannot be null.
        /// </param>
        /// <param name="entityType">
        /// A key value pair mapping the entity type ID (key) and the entities being checked of that type (value).
        /// </param>
        /// <param name="permissionToAccess">
        /// The map of the permission ID to true (if the user can access it) or false (if not). This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// None of the arguments can be null.
        /// </exception>
        internal void CheckAutomaticAccess(EntityRef permission, KeyValuePair<long, ISet<EntityRef>> entityType,
            Dictionary<long, IDictionary<long, bool>> permissionToAccess)
        {
            if (permission == null)
            {
                throw new ArgumentNullException("permission");
            }
            if (permissionToAccess == null)
            {
                throw new ArgumentNullException("permissionToAccess");
            }

            if (Permissions.Read.Equals(permission))
            {
                WellKnownAliases aliases = WellKnownAliases.CurrentTenant;

                long[] autoReadAccessTypes =
                {
                    aliases.Type,
                    aliases.Field,
                    aliases.Relationship
                };

                if (autoReadAccessTypes.Contains(entityType.Key))
                {
                    foreach (EntityRef instance in entityType.Value)
                    {
                        permissionToAccess[permission.Id][instance.Id] = true;
                    }
                }
            }
        }

        /// <summary>
        /// What entity types does the given <see cref="Subject"/> have the given <see cref="Permission"/> to?
        /// This is used by <see cref="CheckAccessControlByRelationship"/>.
        /// </summary>
        /// <param name="permission">
        /// The <see cref="Permission"/> to check. This cannot be null.
        /// </param>
        /// <param name="subject">
        /// The user or role to check. This cannot be null.
        /// </param>
        /// <returns>
        /// The list of <see cref="EntityType"/>s the user has access to.
        /// </returns>
        protected virtual IEnumerable<long> AllowedEntityTypes(EntityRef permission, Subject subject)
        {
            if (permission == null)
            {
                throw new ArgumentNullException("permission");
            }
            if (subject == null)
            {
                throw new ArgumentNullException("subject");
            }

            IReadOnlyCollection<AccessRule> accessRules;
            IReadOnlyCollection<long> entityTypes;

            accessRules = subject.AllowAccess
                .Where(x => (x.AccessRuleEnabled ?? false) && (x.ControlAccess != null) && x.PermissionAccess.Any(y => EntityRefComparer.Instance.Equals(y, permission)))
                .ToList();
            entityTypes = accessRules
                .Select(x => x.ControlAccess.Id)
                .ToList();

            if (CacheContext.IsSet())
            {
                using (CacheContext cacheContext = new CacheContext())
                {                    
                    cacheContext.EntityTypes.Add(accessRules.SelectMany(ar => ar.TypeIds));
                    cacheContext.Entities.Add(accessRules.Select(ar => ar.Id));
                    cacheContext.Entities.Add(entityTypes);
                    cacheContext.Entities.Add(subject.Id);
                }
            }

            return entityTypes;
        }

        /// <summary>
        /// Allow or deny access to all.
        /// </summary>
        /// <param name="entities">
        /// The entities to deny access to. This cannot be null or contain null.
        /// </param>
        /// <param name="canAccess">
        /// The flag to set access to.
        /// </param>
        /// <returns>
        /// A dictionary with each entity ref as the key and false as the value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entities"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="entities"/> cannot contain null.
        /// </exception>
        internal IDictionary<long, bool> SetAll(IEnumerable<long> entities, bool canAccess)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }

            return entities.ToDictionarySafe( x => x, x => canAccess );
        }

        /// <summary>
        /// Collate access, ensuring the subject (user or role) only has access if
        /// they can perform every operation (has each permission).
        /// </summary>
        /// <param name="entities">
        /// The entities to check. This cannot be null or contain null.
        /// </param>
        /// <param name="permissionToAccess">
        /// A mapping of each permission (operation) to a mapping of the entity to
        /// whether it has access or not. This cannot be null or contain null.
        /// </param>
        /// <returns>
        /// A dictionary with each entity ref as the key and a flag indicating whether
        /// the subject has access or not (true if they have access, false otherwise).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// No argument can contain null.
        /// </exception>
        internal IDictionary<long, bool> CollateAccess(IList<EntityRef> entities,
            IDictionary<long, IDictionary<long, bool>> permissionToAccess)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }
            if (entities.Any(x => x == null))
            {
                throw new ArgumentException(@"Entities cannot contain null", "entities");
            }
            if (permissionToAccess == null)
            {
                throw new ArgumentNullException("permissionToAccess");
            }

            IDictionary<long, bool> result = new Dictionary<long, bool>();

            foreach (EntityRef entity in entities)
            {
                result[entity.Id] = permissionToAccess.Keys.Count > 0
                    && permissionToAccess.Values.All(x => x[entity.Id]);
            }

            return result;
        }

        /// <summary>
        /// Get the subjects for a user.
        /// </summary>
        /// <param name="user">
        /// The user to check. This cannot be null.
        /// </param>
        /// <returns>
        /// The subjects (i.e. user and roles).
        /// </returns>
        private ISet<long> GetSubjects(EntityRef user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            ISet<long> subjectIds;

            subjectIds = new HashSet<long>(RoleRepository.GetUserRoles(user.Id));
            using (MessageContext messageContext = new MessageContext(EntityAccessControlService.MessageName))
            {
                messageContext.Append(() => string.Format(
                    "Checking access for user '{0}' ({1}) in roles '{2}'",
                    Entity.Get<UserAccount>(user).Name,
                    user.Id,
                    string.Join(", ", subjectIds.ToList().Select(id => string.Format("'{0}' ({1})", Entity.Get<Subject>(id).Name, id)))));
            }
            subjectIds.Add(user.Id);

            return subjectIds;

        }
    }
}
