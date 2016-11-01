// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Globalization;
using EDC.ReadiNow.Model;
using Model = EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata;
using System.Data;
using EDC.ReadiNow.Core;
using EDC.Database;

namespace ReadiNow.Expressions.NameResolver
{

    /// <summary>
    /// A class for resolving names that appear in user code. (E.g. document generation).
    /// Note: currently just implemented by relying on 'name'.
    /// </summary>
    public class ScriptNameResolver : IScriptNameResolver
    {
        /// <summary>
        /// Queries to preload all members for a particular type.
        /// Note: a fair amount of relationship data gets loaded at server warm-up, so is excluded here.
        /// Note: Some data is included for fields that will be used by the expression engine - the main consumer of ScriptNameResolver.
        /// </summary>
        private const string
            PreloadMembersOfType = @"
                    let @FIELD = {
                        name,
                        fieldScriptName
                        // isOfType.id, decimalPlaces, defaultValue
                    }
                    let @REL = {
                        name,
                        fromName, fromScriptName,
                        toName, toScriptName
                    }
                    isOfType.id,
                    relationships.@REL,
                    reverseRelationships.@REL,
                    fields.@FIELD
                ";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="entityRepository">The repository to load schema information from. Recommend the GraphEntityRepository.</param>
		/// <param name="queryRunner">The query runner.</param>
		/// <exception cref="System.ArgumentNullException">
		/// entityRepository
		/// or
		/// queryRunner
		/// </exception>
        public ScriptNameResolver(IEntityRepository entityRepository, IQueryRunner queryRunner)
        {
            if (entityRepository == null)
                throw new ArgumentNullException("entityRepository");
            if (queryRunner == null)
                throw new ArgumentNullException("queryRunner");

            EntityRepository = entityRepository;
            QueryRunner = queryRunner;
        }


        /// <summary>
        /// The repository to load schema information from.
        /// </summary>
        public IEntityRepository EntityRepository { get; private set; }

        /// <summary>
        /// The query runner to find types that match a script name.
        /// </summary>
        public IQueryRunner QueryRunner { get; private set; }


        /// <summary>
        /// Given the script name of a type or object, returns its ID.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns>The ID of the type, or zero if there are zero or duplicate matches.</returns>
        public long GetTypeByName(string typeName)
        {
            using (Profiler.Measure("ScriptNameResolver.GetTypeByName"))
            {
                StructuredQuery query = BuildTypeByNameQuery();

                QuerySettings settings = new QuerySettings();
                settings.SecureQuery = false;
                settings.SharedParameters = new Dictionary<ParameterValue, string>
                    { { new ParameterValue(DbType.String, typeName), "@scriptName" } };

                Factory.NonCachedQuerySqlBuilder.BuildSql(query, settings);

                QueryResult result = QueryRunner.ExecuteQuery(query, settings);

                if (result == null)
                    return 0;
                DataTable table = result.DataTable;
                if (table == null || table.Rows.Count != 1)
                    return 0;
                object value = table.Rows[0][0];
                if (value == null)
                    return 0;
                long typeId = (long)value;
                return typeId;
            }
        }

        /// <summary>
        /// Create a structured query that will return types/objects matching a particular name.
        /// </summary>
        /// <remarks>
        /// Returns objects, system types, enums, and activity types. But not other things that derive from type (such as fieldType).
        /// </remarks>
        /// <returns>
        /// StructuredQuery with a @scriptName query parameter.
        /// </returns>
        private static StructuredQuery BuildTypeByNameQuery()
        {
            // List of types that we allow to be referenced by names
            long[] allowedTypeTypes = new[] {
                    Definition.Definition_Type.Id,
                    ManagedType.ManagedType_Type.Id,
                    EntityType.EntityType_Type.Id,
                    EnumType.EnumType_Type.Id,
                    ActivityType.ActivityType_Type.Id
                };

            // Create the structured query
            var rootEntity = new ResourceEntity( new EntityRef( WellKnownAliases.CurrentTenant.Type ) );
            var typeType = new RelatedResource(new EntityRef(WellKnownAliases.CurrentTenant.IsOfType));
            rootEntity.RelatedEntities.Add(typeType);
            var query = new StructuredQuery() { RootEntity = rootEntity };
            var col = new SelectColumn { Expression = new SQ.IdExpression { NodeId = rootEntity.NodeId } };
            query.SelectColumns.Add(col);

            // Allowed-type condition
            var typeCondition = new QueryCondition
            {
                Expression = new SQ.IdExpression { NodeId = typeType.NodeId },
                Operator = ConditionType.AnyOf,
                Arguments = allowedTypeTypes.Select(id => new TypedValue() { Value = id, Type = DatabaseType.IdentifierType }).ToList()
            };
            query.Conditions.Add(typeCondition);

            // Script-name condition
            var calcExpr = new CalculationExpression
            {
                Operator = CalculationOperator.IsNull,
                Expressions = new List<ScalarExpression>
                {
                    new ResourceDataColumn(rootEntity, new EntityRef("core:typeScriptName")),
                    new ResourceDataColumn(rootEntity, new EntityRef("core:name")),
                }
            };
            var nameCondition = new QueryCondition
            {
                Expression = calcExpr,
                Operator = ConditionType.Equal,
                Parameter = "@scriptName"
            };
            query.Conditions.Add(nameCondition);

            return query;
        }

        /// <summary>
        /// Given a type and a name, return any instances of that type that have that name.
        /// </summary>
        /// <remarks>
        /// The name must return a unique exact match. If there are duplicates, then none are returned.
        /// </remarks>
        /// <param name="instanceName">The name (not script name) of the instance.</param>
        /// <param name="typeId">The type of instances to search (including derived types).</param>
        /// <returns>The instance. Or null there are zero or duplicate matches.</returns>
        public IEntity GetInstance(string instanceName, long typeId)
        {
            ISet<long> acceptableTypes = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(typeId);

            // TODO : make a better version of Entity.GetByName that takes type as an argument
            IEnumerable<IEntity> entities = Model.Entity.GetByName(instanceName);

            // Filter by type
            IEnumerable<IEntity> results = entities.Where(i => acceptableTypes.Contains(i.TypeIds.FirstOrDefault()));

            try
            {
                return results.SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                return null; // duplicates
            }
        }


		/// <summary>
		/// Given a type (e.g. Person), resolve a name that could be a field name, or a relationship name.
		/// </summary>
		/// <param name="memberScriptName">Name of the member script.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="memberTypeFilter">Types of members to find.</param>
		/// <returns>
		/// Either a field definition, or relationship definition, or any. Or null there are zero or duplicate matches.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">memberScriptName</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">typeId</exception>
		/// <exception cref="System.ArgumentException">memberTypeFilter</exception>
        public MemberInfo GetMemberOfType(string memberScriptName, long typeId, MemberType memberTypeFilter)
        {
            if (string.IsNullOrEmpty(memberScriptName))
                throw new ArgumentNullException("memberScriptName");
            if (typeId <= 0)
                throw new ArgumentOutOfRangeException("typeId");
            if (memberTypeFilter == MemberType.Type)
                throw new ArgumentException("memberTypeFilter");

            // Fetch types from entity repository separately, so they will be cached and invalidated individually
            List<EntityType> types = new List<EntityType>();
            ISet<long> typeIds = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(typeId);
            foreach (long ancestorTypeId in typeIds)
            {
                EntityType type = EntityRepository.Get<EntityType>(ancestorTypeId, PreloadMembersOfType);
                types.Add(type);
            }

            IEnumerable<MemberInfo> results = GetMemberOfTypes(memberScriptName, types, memberTypeFilter);
            try
            {
                return results.SingleOrDefault();
            }
            catch (InvalidOperationException)   // multiple matches
            {
	            ScriptNameResolverContext.Reason = NullMemberNameReason.Duplicate;
                return null;
            }
        }


        /// <summary>
        /// Given a set of types (e.g. Person), resolve a name that could be a field name, or a relationship name.
        /// Does not consider inheritance, which is presumed to already be resolved.
        /// </summary>
		private static IEnumerable<MemberInfo> GetMemberOfTypes(string memberName, IEnumerable<EntityType> types, MemberType findMembers)
        {
            bool findFields = findMembers.HasFlag(MemberType.Field);
            bool findRels = findMembers.HasFlag(MemberType.Relationship);

            foreach (var type in types)
            {
                if (findFields)
                {
                    // Test fields
                    foreach (var field in type.Fields)
                    {
                        if (CompareNames(field.FieldScriptName, field.Name, memberName))
                        {
                            var result = new MemberInfo
                            {
                                MemberId = field.Id,
                                MemberType = MemberType.Field
                            };
                            yield return result;
                        }
                    }
                }

                if (findRels)
                {
                    // Test relationships
                    foreach (var rel in type.Relationships)
                    {
                        if (CompareNames(rel.ToScriptName, rel.ToName ?? rel.Name, memberName))
                        {
                            var result = new MemberInfo
                            {
                                MemberId = rel.Id,
                                MemberType = MemberType.Relationship,
                                Direction = Direction.Forward
                            };
                            yield return result;
                        }
                    }

                    // Test reverse relationships
                    foreach (var rel in type.ReverseRelationships)
                    {
                        if (CompareNames(rel.FromScriptName, rel.FromName ?? rel.Name, memberName))
                        {
                            var result = new MemberInfo
                            {
                                MemberId = rel.Id,
                                MemberType = MemberType.Relationship,
                                Direction = Direction.Reverse
                            };
                            yield return result;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns true if effective script name matches the identifier being matched.
        /// </summary>
        private static bool CompareNames(string scriptName, string realName, string nameToMatch)
        {
            string effectiveScriptName = scriptName ?? realName;

            if (effectiveScriptName == null || nameToMatch == null)
                return false;

            return string.Compare(effectiveScriptName, nameToMatch, true, CultureInfo.InvariantCulture) == 0;
        }

    }

}
