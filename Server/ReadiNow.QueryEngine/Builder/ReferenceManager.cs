// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata.Query.Structured;
using ReadiNow.QueryEngine.Builder.SqlObjects;

namespace ReadiNow.QueryEngine.Builder
{
    /// <summary>
    /// Holds various references within a query.
    /// Note: given that the serialization process does not easily permit 'interesting' object graphs,
    /// most of the StructureQuery uses local guids for internal references to other parts of the query.
    /// This class provides a convenient container of various lookups when processing the query without
    /// cluttering up SqlQuery.
    /// </summary>
	internal class ReferenceManager
	{
        private readonly Dictionary<Guid, SqlExpression> _columnExpressionByColumnId = new Dictionary<Guid, SqlExpression>( );
		private readonly Dictionary<Tuple<Guid, object>, SqlExpression> _columnExpressionByEntityNodeIdAndColumnId = new Dictionary<Tuple<Guid, object>, SqlExpression>( );
		private readonly Dictionary<Guid, Entity> _entityNodeByNodeId = new Dictionary<Guid, Entity>( );
		private readonly Dictionary<ScalarExpression, SqlExpression> _mappedExpressions = new Dictionary<ScalarExpression, SqlExpression>( );
		private readonly Dictionary<Tuple<Guid, Guid>, SqlTable> _tableByEntityNodeIdAndBaseTableId = new Dictionary<Tuple<Guid, Guid>, SqlTable>( );
		private readonly Dictionary<Entity, EntityTables> _tablesByEntity = new Dictionary<Entity, EntityTables>( );
        private readonly Dictionary<long, List<AccessRuleCteTypeRegistration>> _typeIdsToAccessRuleCtes = new Dictionary<long, List<AccessRuleCteTypeRegistration>>();
        private readonly Dictionary<long, ISet<long>> _entityBatches = new Dictionary<long, ISet<long>>();

		public SqlTable FindBaseTable( Guid entityLocalId, Guid baseTableId )
		{
			var key = new Tuple<Guid, Guid>( entityLocalId, baseTableId );

			SqlTable result;
			_tableByEntityNodeIdAndBaseTableId.TryGetValue( key, out result );
			return result;
		}

		public T FindEntity<T>( Guid localId ) where T : Entity
		{
			var res = TryFindEntity<T>( localId );
			if ( res == null )
			{
				throw new Exception( "Query entity could not be found. Internal Id: " + localId.ToString( ) );
			}
			return res;
		}

		public SqlExpression FindExpression( Guid entityLocalId, object columnId )
		{
			var key = new Tuple<Guid, object>( entityLocalId, columnId );

			SqlExpression result;
			_columnExpressionByEntityNodeIdAndColumnId.TryGetValue( key, out result );
			return result;
		}

        public static SqlExpression FindMappedExpression(Guid columnId, SqlQuery query)
        {
            SqlExpression result = null;
            query.References._columnExpressionByColumnId.TryGetValue(columnId, out result);      
            return result;
        }

		public static SqlQuery FindQueryContainingEntity( Guid localId, SqlQuery query )
		{
			if ( query.References._entityNodeByNodeId.ContainsKey( localId ) )
			{
				return query;
			}
			return
				query.Subqueries.Select( q => FindQueryContainingEntity( localId, q ) ).FirstOrDefault( q => q != null );
		}

		public SqlExpression FindSelectColumn( Guid columnId )
		{
			SqlExpression result;
			_columnExpressionByColumnId.TryGetValue( columnId, out result );
			return result;
		}

		public SqlTable FindSqlTable( Guid nodeId )
		{
			var entity = FindEntity<Entity>( nodeId );
			return FindSqlTable( entity );
		}

		public SqlTable FindSqlTable( Entity entity )
		{
			return _tablesByEntity[ entity ].EntityTable;
        }

        public SqlTable FindSqlTable( Entity entity, bool searchSubqueries )
        {
            if ( !searchSubqueries )
                return FindSqlTable( entity );

            EntityTables result;
            if ( _tablesByEntity.TryGetValue( entity, out result ) )
                return result.EntityTable;

            foreach ( var pair in _tablesByEntity )
            {
                SqlUnion subQueryUnion = pair.Value.EntityTable.SubQuery;
                if ( subQueryUnion == null )
                    continue;
                foreach ( SqlQuery query in subQueryUnion.Queries )
                {
                    SqlTable res = query.References.FindSqlTable( entity, true );
                    if ( res != null )
                        return res;
                }
            }
            return null;
        }

        public EntityTables FindTables( Guid nodeId )
		{
			var entity = FindEntity<Entity>( nodeId );
			return _tablesByEntity[ entity ];
		}

		public EntityTables FindTables( Entity entity )
		{
			return _tablesByEntity[ entity ];
		}

		public SqlExpression GetMappedExpression( ScalarExpression expression )
		{
			SqlExpression result;
			_mappedExpressions.TryGetValue( expression, out result );
			return result;
		}

		public void RegisterBaseTable( Guid entityLocalId, Guid baseTableId, SqlTable table )
		{
			var key = new Tuple<Guid, Guid>( entityLocalId, baseTableId );
			_tableByEntityNodeIdAndBaseTableId[ key ] = table;
		}

		public void RegisterEntity( Entity entity, EntityTables tables )
		{
			_entityNodeByNodeId[ entity.NodeId ] = entity;
			_tablesByEntity[ entity ] = tables;
		}

		public void RegisterExpression( Guid entityLocalId, object columnId, SqlExpression expression )
		{
			var key = new Tuple<Guid, object>( entityLocalId, columnId );
			_columnExpressionByEntityNodeIdAndColumnId[ key ] = expression;
		}

		public void RegisterMappedExpression( ScalarExpression expression, SqlExpression sqlExpressionToUse )
		{
			// The specified logical expression is actually contained within a sub query and cannot be resolved directly.
			// Instead, use the specified SQL expression to access it.
			_mappedExpressions[ expression ] = sqlExpressionToUse;
		}

		public void RegisterSelectColumn( Guid columnId, SqlExpression expression )
		{
			_columnExpressionByColumnId[ columnId ] = expression;
		}

        /// <summary>
        /// Registers the specified access rule CTE for the specified type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="cte">Name of the cte.</param>
        /// <param name="cteIsApplicableToAllInstancesOfType">True if this is applicable to all instances of the type, false if only a subset of instances of the type.</param>
        /// <param name="cteType"></param>
        public void RegisterAccessRuleCteForType(long typeId, AccessRuleCte cte, bool cteIsApplicableToAllInstancesOfType, AccessRuleCteTypeEnum cteType)
        {
            List<AccessRuleCteTypeRegistration> ctes;            

            if (!_typeIdsToAccessRuleCtes.TryGetValue(typeId, out ctes))
            {
                ctes = new List<AccessRuleCteTypeRegistration>();
                _typeIdsToAccessRuleCtes[typeId] = ctes;
            }

            AccessRuleCteTypeRegistration cteReg = new AccessRuleCteTypeRegistration
            {
                AccessRuleCte = cte,
                TypeId = typeId,
                RuleIsApplicableToAllInstances = cteIsApplicableToAllInstancesOfType,
                AccessRuleType = cteType
            };

            ctes.Add(cteReg);
        }

        /// <summary>
        /// Gets the access rule CTEs that have been registered for the specified type.
        /// </summary>
        /// <param name="typeId">The type id.</param>
        /// <returns>
        /// The registered access rule CTEs.
        /// </returns>
        public List<AccessRuleCteTypeRegistration> GetAccessRuleCtesForType(long typeId)
        {
            List<AccessRuleCteTypeRegistration> ctes;

            if (!_typeIdsToAccessRuleCtes.TryGetValue(typeId, out ctes))
            {
                ctes = new List<AccessRuleCteTypeRegistration>();
            }

            return ctes;
        }


		public T TryFindEntity<T>( Guid localId ) where T : Entity
		{
			Entity result;
			if ( !_entityNodeByNodeId.TryGetValue( localId, out result ) )
			{
				return null;
			}
			if ( !( result is T ) )
			{
				throw new Exception(
					string.Format( "Referenced entity is of type {0} when type {1} was expected. Internal Id: {2}",
					               result.GetType( ).Name, typeof ( T ).Name, localId ) );
			}

			return ( T ) result;
		}


        /// <summary>
        /// Registers the entity batch.
        /// </summary>
        /// <param name="batchId">The batch identifier.</param>
        /// <param name="entityIds">The entity ids.</param>
        internal void RegisterEntityBatch(long batchId, IEnumerable<long> entityIds)
        {
            ISet<long> ids;
            if (!_entityBatches.TryGetValue(batchId, out ids))
            {
                ids = new HashSet<long>();
                _entityBatches[batchId] = ids;
            }

            if (entityIds != null)
            {
                ids.UnionWith(entityIds);   
            }            
        }


        /// <summary>
        /// Gets the entity batches.
        /// </summary>
        /// <returns></returns>
        internal Dictionary<long, ISet<long>> GetEntityBatches()
        {
            return _entityBatches;
        }
	}
}