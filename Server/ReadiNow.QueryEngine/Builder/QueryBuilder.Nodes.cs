// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Metadata;
using Model = EDC.ReadiNow.Model;
using ReadiNow.QueryEngine.Builder.SqlObjects;
using ReadiNow.QueryEngine.Runner;
using EDC.ReadiNow.Core.Cache;

namespace ReadiNow.QueryEngine.Builder
{
    using EDC.ReadiNow.Metadata.Query.Structured;

    /// <summary>
    ///     QueryBuilder partial class.
    ///     Contains members for processing StructuredQuery tree nodes.
    /// </summary>
    public partial class QueryBuilder
	{        
		/// <summary>
		///     Apply over a ResourceRelationship to handle any simulated relationships that should be included or excluded.
		///     Used in resource editor where relationships must be added/removed prior to the resource being saved.
		/// </summary>
        private void ApplyFauxRelationships( RelatedResource relationship, SqlTable relationshipTable, SqlQuery sqlQuery )
        {
            FauxRelationships faux = relationship.FauxRelationships;

            // Detect if the target of the faux relationship even exists on the database. E.g if creating a new resource, and adding new relations to it, the parent may not yet be saved.            
            bool isNewTarget = !faux.HasTargetResource;
            bool isTempTargetResource = faux.IsTargetResourceTemporary;

            // Explicitly include related resources
            if ( faux.HasIncludedResources )
            {
                // And union it into the relationship query
                // First, union in existing relationships (not applicable if the target is being newly created)
                string unionSql = "";
                if ( !isNewTarget )
                {
                    unionSql = "select TenantId, FromId, ToId, TypeId from dbo.Relationship union ";
                }

                // Then add in any faux relationships
                if ( relationship.RelationshipDirection == RelationshipDirection.Forward )
                {
                    unionSql = "( " + unionSql + "select @tenant TenantId, ir.Id FromId, {0} ToId, {1} TypeId from @includeResources ir )";
                }
                else
                {
                    unionSql = "( " + unionSql + "select @tenant TenantId, {0} FromId, ir.Id ToId, {1} TypeId from @includeResources ir )";
                }

				string relationshipTypeIdParamName = RegisterSharedParameter( System.Data.DbType.Int64, relationship.RelationshipTypeId.Id.ToString( CultureInfo.InvariantCulture ) );

                // A bit hacky - overwrite the relationship table name with the entire union clause
                relationshipTable.Name = string.Format(
                    unionSql,
                    isNewTarget ? "0" : "@targetResource",
					FormatEntity( relationship.RelationshipTypeId, relationshipTypeIdParamName )
                    );
                relationshipTable.NameContainsSql = true;
                // Only secure the join if the query is being secured
                if ( _querySettings.SecureQuery )
                {

                    relationshipTable.SecureResources = !isTempTargetResource;
                }
            }
            else if ( isNewTarget )
            {
                // This is a completely new resource, and does not yet have any included rows.
                relationshipTable.Conditions.Add( "1=2" );
            }

            // Explicitly exclude related resources
            if ( faux.HasExcludedResources )
            {
                if ( relationship.RelationshipDirection == RelationshipDirection.Forward )
                {
                    relationshipTable.Conditions.Add( "$.FromId not in ( select Id from @excludeResources )" );
                }
                else
                {
                    relationshipTable.Conditions.Add( "$.ToId not in ( select Id from @excludeResources )" );
                }
            }
        }

		/// <summary>
		///     Recursively builds the table join tree from the structure query tree.
		/// </summary>
		/// <param name="query">Input StructuredQuery.</param>
		/// <param name="entity">StructuredQuery node being processed.</param>
		/// <param name="parentTable">Parent table that this table may be joining to. May be null.</param>
		/// <param name="sqlQuery">Query to be receiving this structure.</param>
		/// <returns></returns>
		private EntityTables BuildEntityTableJoinTree( StructuredQuery query, Entity entity, SqlTable parentTable, SqlQuery sqlQuery )
		{
			// Note: for the most part we aren't interested in the returned result, as the tables will get built inside the sqlQuery
			// Only return the result so we can capture the topmost table.

			EntityTables entityTables = CreateEntityPrimaryTable( query, entity, parentTable, sqlQuery );
            
            // The EntityNode should generally not already be set, but in the case of a downcast, the existing parentTable may be returned
            // In that case, we don't want to override the entitynode, because the parent node security rules can be safely applied to the child, but not vice-versa.
            if ( entityTables.EntityTable.EntityNode == null )
            {
                entityTables.EntityTable.EntityNode = entity;
            }

			sqlQuery.References.RegisterEntity( entity, entityTables );

			if ( entity.RelatedEntities != null )
			{
				foreach ( Entity relatedEntity in entity.RelatedEntities )
				{
					var relatedResourceEntity = relatedEntity as RelatedResource;
					if ( relatedResourceEntity != null &&
					     relatedResourceEntity.ExcludeIfNotReferenced &&
					     !IsEntityReferenced( query, relatedResourceEntity ) )
					{
						// Check to see if this resource entity is used anywhere else in the query                            
						// If not continue ignore the join.
						continue;
					}

					BuildEntityTableJoinTree( query, relatedEntity, entityTables.EntityTable, sqlQuery );
				}
			}

			// Add any conditions that are explicitly assigned to this node
			if ( entity.Conditions != null )
			{
				foreach ( var expr in entity.Conditions )
				{
					SqlExpression sqlExpr = ConvertExpression( expr, sqlQuery );
					entityTables.EntityTable.Conditions.Add( sqlExpr.BoolSql );
                    entityTables.EntityTable.HasCustomConditions = true;
				}
			}

			return entityTables;
		}

        /// <summary>
        ///     Adds additional joins to the specified table of entities to ensure that only resources of a certain definition type are selected.
        /// </summary>
        /// <param name="tableToConstrain">The table to constrain.</param>
        /// <param name="typeId">The Id of the type to allow.</param>
        /// <param name="exactTypeOnly">
        ///     If set to <c>true</c> then the definition must match exactly. If false, then descendent definitions also allowed.
        /// </param>
        /// <param name="sqlQuery">The SQL query the joined tables will be created in.</param>
        /// <returns>Returns the tableToConstrain, or the new relationship table if one is being created on demand.</returns>
        private SqlTable ConstrainDefinitionType( SqlTable tableToConstrain, IEntityRef typeId, bool exactTypeOnly, SqlQuery sqlQuery )
		{
		    SqlTable newTableToConstrain;

            if ( typeId == null )
            {
                throw new ArgumentNullException( "typeId" );
            }

			var isOfType = new EntityRef( "core", "isOfType" );

            bool exactTypeOnlyImpl = exactTypeOnly;
            ISet<long> derivedTypes = null;

            // Don't join for derived types if there aren't any.
            if ( !exactTypeOnly )
            {
                EntityType type = Model.Entity.Get<EntityType>( typeId.Id );
                if ( type.Alias == "core:resource" )
                    return tableToConstrain; // all types derive from resource .. don't bother constraining
                
                using ( CacheManager.ExpectCacheMisses( ) )
                {
	                exactTypeOnlyImpl = type.DerivedTypes.Count <= 0;
                    if (!exactTypeOnlyImpl && _querySettings.DerivedTypesTempTableThreshold > 0)
                    {
                        // Have derived types.
                        ISet<long> derivedTypesAndSelf = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(typeId.Id);
						if ( derivedTypesAndSelf.Count > 0 &&
                            derivedTypesAndSelf.Count <= _querySettings.DerivedTypesTempTableThreshold)
                        {
                            derivedTypes = new SortedSet<long>(derivedTypesAndSelf);
                            exactTypeOnlyImpl = true;
                        }                        
                    }                    
                }
            }

            // Create isOfType join.
            SqlTable relationshipTable;
            if ( tableToConstrain != null )
            {
                // Constrain an existing table
                relationshipTable = sqlQuery.CreateJoinedTable( "dbo.Relationship", "r", tableToConstrain, JoinHint.Required, "FromId", tableToConstrain.IdColumn );
                relationshipTable.JoinHint = JoinHint.ConstrainWithExists; // its faster                
                relationshipTable.Conditions.Add( "$.TenantId = @tenant" );
                newTableToConstrain = tableToConstrain;
            }
            else
            {
                // No constraint table : the type check will constitute the root entity table.
                relationshipTable = sqlQuery.CreateTable( "dbo.Relationship", "e" );
                relationshipTable.IdColumn = "FromId";
                relationshipTable.FilterByTenant = true;
                newTableToConstrain = relationshipTable;
            }

			string isOfTypeParamName = RegisterSharedParameter( System.Data.DbType.Int64, isOfType.Id.ToString( CultureInfo.InvariantCulture ) );

			relationshipTable.Conditions.Add( "$.TypeId = " + FormatEntity( isOfType, isOfTypeParamName ) );

            if ( exactTypeOnlyImpl )
			{
                if (derivedTypes == null)
                {
					string typeIdParamName = RegisterSharedParameter( System.Data.DbType.Int64, typeId.Id.ToString( CultureInfo.InvariantCulture ) );

					relationshipTable.Conditions.Add( string.Format( "$.ToId = {0}", FormatEntity( typeId, typeIdParamName ) ) );
                }
                else
                {
                    IEnumerable<string> formattedDerivedTypes = derivedTypes.Select( id =>
                    {
						string derivedTypeIdParamName = RegisterSharedParameter( System.Data.DbType.Int64, id.ToString( CultureInfo.InvariantCulture ) );

						return FormatEntity( new EntityRef( id ), true, derivedTypeIdParamName );
                    } );

                    relationshipTable.Conditions.Add(string.Format("$.ToId in ({0})", string.Join(", ", formattedDerivedTypes)));                                        
                }
            }
			else
			{
				SqlTable derivedTypeTable;

			    if (_sqlBatch != null)
			    {
					string typeIdParamName = RegisterSharedParameter( System.Data.DbType.Int64, typeId.Id.ToString( CultureInfo.InvariantCulture ) );

					string formattedEntityId = FormatEntity( typeId, true, typeIdParamName );
			        const string derivedTableName = "#derived";
                    string sharedDerivedTableName = derivedTableName + formattedEntityId;
			        string usedDerivedTableName;

			        string createTableFormatString = 
						"create table {0} ( Id bigint primary key ){2}insert into {0} select Id from dbo.fnDerivedTypes({1}, @tenant)";
			        const string dropTableFormatString = "drop table {0}";

			        string sharedCreateTableSql = string.Format(createTableFormatString, sharedDerivedTableName, formattedEntityId, Environment.NewLine);
                        
                    if (_querySettings.UseSharedSql)
			        {                        
                        string dropTableSql = string.Format(dropTableFormatString, sharedDerivedTableName);

                        // Return the sql used to create/drop the tables.
                        // Do not emit it as part of the sql for the query.
                        _sharedSqlPreamble.Add(sharedCreateTableSql);
                        _sharedSqlPostamble.Add(dropTableSql);

                        usedDerivedTableName = sharedDerivedTableName;
			        }
			        else
			        {
                        // Check if the shared sql exists.
                        // This is an optimization.
                        bool isSharedSqlAvailable = IsSharedSqlPreambleAvailable(sharedCreateTableSql);                        

                        if (isSharedSqlAvailable)
			            {
                            // The shared sql is available. So we don't emit the create or drop sql
			                usedDerivedTableName = sharedDerivedTableName;
			            }
			            else
			            {
                            // The shared sql is not available, so emit the SQL
			                usedDerivedTableName = derivedTableName;

                            string createTableSql = string.Format(createTableFormatString, derivedTableName, formattedEntityId, Environment.NewLine);
                            string dropTableSql = string.Format(dropTableFormatString, derivedTableName);

                            // Emit the sql as part of the query                            
                            _sqlBatch.SqlPreamble = string.Concat(Environment.NewLine, createTableSql, _sqlBatch.SqlPreamble);
                            _sqlBatch.SqlPostamble = string.Concat(_sqlBatch.SqlPostamble, Environment.NewLine, dropTableSql);               
			            }			                                    
			        }

                    derivedTypeTable = sqlQuery.CreateJoinedTable(usedDerivedTableName, "dt", relationshipTable, JoinHint.Required, "Id", "ToId");                    
			    }
				else
				{
					string typeIdParamName = RegisterSharedParameter( System.Data.DbType.Int64, typeId.Id.ToString( CultureInfo.InvariantCulture ) );

					//// Via inherited type
					derivedTypeTable = sqlQuery.CreateJoinedTable( string.Format( "dbo.fnDerivedTypes({0}, @tenant)", FormatEntity( typeId, typeIdParamName ) ), "dt", relationshipTable, JoinHint.Required, "Id", "ToId" );
				}

				derivedTypeTable.NameContainsSql = true;
			}

            return newTableToConstrain;
		}


		/// <summary>
		///     Creates the SqlTable for the specified entity/relationship.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="parentTable">The parent table. Maybe null for root.</param>
		/// <param name="sqlQuery">The SQL query.</param>
		/// <returns>
		///     The table to use for upward joins.
		/// </returns>
        private EntityTables CreateEntityPrimaryTable(StructuredQuery query, Entity entity, SqlTable parentTable, SqlQuery sqlQuery)
        {
            EntityTables table;

            var resource = entity as RelatedResource;
            if (resource != null)
            {
                table = RegisterRelatedResourceEntity(resource, parentTable, sqlQuery);
                return table;
            }

            var downcast = entity as DownCastResource;
            if (downcast != null)
            {
                table = RegisterDownCastResourceEntity(downcast, parentTable, sqlQuery);
                return table;
            }

            var joinToSelf = entity as JoinToSelfEntity;
            if (joinToSelf != null)
            {
                table = RegisterJoinToSelfEntity(joinToSelf, parentTable, sqlQuery);
                return table;
            }

            var customJoin = entity as CustomJoinNode;
            if ( customJoin != null )
            {
                table = RegisterCustomJoinNode( customJoin, parentTable, sqlQuery );
                return table;
            }

            var singleRow = entity as SingleRowNode;
            if ( singleRow != null )
            {
                table = RegisterSingleRowNode( singleRow, parentTable, sqlQuery );
                return table;
            }

            var resourceEntity = entity as ResourceEntity;
            if (resourceEntity != null) // must be after types that derive from ResourceEntity
            {
                table = RegisterResourceEntity(resourceEntity, sqlQuery);
                return table;
            }

            var aggregateEntity = entity as AggregateEntity;
            if (aggregateEntity != null)
            {
                table = RegisterAggregateEntity(query, aggregateEntity, parentTable, sqlQuery);
                return table;
            }

            throw new InvalidOperationException(entity.GetType().Name);
        }


		/// <summary>
		///     Generates SqlTable and sub query for an aggregation.
		/// </summary>
		/// <param name="structuredQuery">The structured query.</param>
		/// <param name="entity">The AggregateEntity that represents the aggregation in the 'from' tree.</param>
		/// <param name="parentTable">The table that this aggregation joins up to. Can be null if the aggregation is being performed at the top level.</param>
		/// <param name="sqlQuery">The query object.</param>
		/// <returns>
		///     The table
		/// </returns>
		private EntityTables RegisterAggregateEntity( StructuredQuery structuredQuery, AggregateEntity entity, SqlTable parentTable, SqlQuery sqlQuery )
		{
			if ( entity.GroupedEntity == null )
			{
				throw new Exception( "Aggregate entity must have GroupedEntity set." );
			}
			if ( entity.RelatedEntities.Count > 0 )
			{
				throw new Exception( "Aggregate entity should not have related entities." ); // note: no logical reason not to, however it's likely indicative of a bug elsewhere
			}

			// Establish sub query
			// Note that sub query has its distinct reference manager for tracking tables and other elements.
			SqlQuery subquery = sqlQuery.CreateSubQuery( );
			subquery.ParentQuery = sqlQuery;
			sqlQuery.Subqueries.Add( subquery );

			// Get joining table for the entity being grouped
			EntityTables childTables =
				BuildEntityTableJoinTree( structuredQuery, entity.GroupedEntity, parentTable, subquery ); //hmm
			SqlTable childTable = childTables.HeadTable;
			subquery.FromClause.RootTable = childTable;

			// Note: we passed in parentTable above so that the child can learn about the joining column
			// However, it will actually try to join to the table when we don't want it to, so need to manually unlink it afterwards.
			childTable.Parent = null;
			if ( parentTable != null )
			{
				parentTable.Children.Remove( childTable );
			}

            // Create the proxy table
            // Note: parentTable may be null for root aggregate
            SqlTable proxyTable = sqlQuery.CreateJoinedTable( "", // not applicable
			                                                  "ag", parentTable, JoinHint.Unspecified, childTable.JoinColumn, parentTable?.IdColumn );

            //if the used condition under current aggregate grouped entity or related entities, set joinhint to requried
            if (structuredQuery.Conditions.Any(c => c.Operator != ConditionType.Unspecified
                                                    && c.Operator != ConditionType.IsNull
                                                    && ConditionContainArgument(c)
                                                    && c.Expression is ResourceDataColumn 
                                                    && MatchGroupedEntityNode(((ResourceDataColumn)c.Expression).NodeId, entity.GroupedEntity)                                                                                                   
                                                    ))
            {
                proxyTable.JoinHint = JoinHint.Required;
            }

            //TODO, this hacky code is resolved a special sql query issue (bug 24406), both Pete and me agree with this is not the best solution, but can fix current bug
            //TODO, we can remove this hacky code when report builder allows set the aggregated field in analyzer.
		    ConvertConditionExpressionToAggregate(structuredQuery.Conditions, entity);
		    


            proxyTable.SubQuery = new SqlUnion( subquery );
			subquery.ProxyTable = proxyTable;


			// Proxy the join column through the sub query select statement
			// (If it actually joins to something)
			if ( childTable.JoinColumn != null )
			{
				string joinColumnAlias = sqlQuery.AliasManager.CreateAlias( "aggCol" );
				string joinColumnSql = GetColumnSql( childTable, childTable.JoinColumn );
				var joinColumn = new SqlSelectItem
					{
						Expression = new SqlExpression
							{
								Sql = joinColumnSql
							},
						Alias = joinColumnAlias
					};
				subquery.SelectClause.Items.Add( joinColumn );
				subquery.GroupByClause.Expressions.Add( joinColumn.Expression );
				proxyTable.JoinColumn = joinColumnAlias;

				var idExpr = new IdExpression
					{
						NodeId = entity.GroupedEntity.NodeId
					};
				sqlQuery.References.RegisterMappedExpression( idExpr, joinColumn.Expression );
			}

			// Add additional grouping columns
			if ( entity.GroupBy != null && entity.GroupBy.Count > 0 )
		    {
		        proxyTable.GroupByMap = new Dictionary<ScalarExpression, SqlExpression>();
		        foreach (ScalarExpression expr in entity.GroupBy)
		        {
		            try
		            {
						SqlExpression sqlExpr;
			            if ( TryConvertExpression( expr, subquery, false, out sqlExpr ) )
			            {
				            if ( !_querySettings.FullAggregateClustering )
				            {
					            // If we're not clustering everywhere, then explicitly cluster in summarize group-bys still.
					            sqlExpr = ApplyClusterOperation( sqlExpr, expr.ClusterOperation, sqlQuery );
				            }

				            proxyTable.GroupByMap[ expr ] = sqlExpr;
				            var mappedSqlExpr = CreateAggregateMappingExpression( sqlQuery, proxyTable, sqlExpr );
				            sqlQuery.References.RegisterMappedExpression( expr, mappedSqlExpr );

				            if ( sqlExpr.OrderingSqlCallback != null && sqlExpr.OrderingSqlRequiresGrouping )
				            {
					            mappedSqlExpr.OrderingSqlCallback =
						            exprTmp =>
							            CreateAggregateOrderbyMappingExpression( sqlQuery, proxyTable, sqlExpr.OrderingSql ).Sql;
				            }
			            }
		            }
		            catch
		            {

		            }
		        }
		    }

		    // Return proxy table
			return new EntityTables( proxyTable );
		}

        /// <summary>
        /// This is a hacky function to convert condition which under aggregated node and operator is IsNull
        /// for normal query, if the condition is under aggregated node, the queryBuilder will add this condition to child aggregate table. 
        /// however for IsNull condition is little special, it should be in main where clause and the condition node should be current aggreagate node id.        
        /// </summary>
        /// <param name="conditions">structuredQuery conditions</param>
        /// <param name="aggregateEntity">current aggregated entity</param>
        private void ConvertConditionExpressionToAggregate(List<QueryCondition> conditions,
            AggregateEntity aggregateEntity)
        {
            if (conditions.Any(c => c.Operator == ConditionType.IsNull
                                                    && c.Expression is ResourceDataColumn
                                                    && MatchGroupedEntityNode(((ResourceDataColumn)c.Expression).NodeId, aggregateEntity.GroupedEntity)                                                     
                ))
            {
                foreach (QueryCondition condition in conditions)
                {
                    ResourceDataColumn resourceExpression = condition.Expression as ResourceDataColumn;
                    if (resourceExpression != null)
                    {
                        AggregateExpression aggregateExpression = new AggregateExpression
                        {
                            AggregateMethod = AggregateMethod.Min,
                            NodeId = aggregateEntity.NodeId,
                            Expression = resourceExpression
                        };
                        condition.Expression = aggregateExpression;
                    }
                }
            }
        }

        /// <summary>
        /// To verify current grouped entity or related entity's nodeid is mathced the condition nodeId
        /// </summary>
        /// <param name="conditionNodeId">condition nodeId</param>
        /// <param name="groupedEntity">grouped entity</param>
        /// <returns></returns>
        private bool MatchGroupedEntityNode(Guid conditionNodeId, Entity groupedEntity )
        {
            bool isMatched = false;
           
            if (conditionNodeId == groupedEntity.NodeId)
                return true;

            if ( groupedEntity.RelatedEntities != null && groupedEntity.RelatedEntities.Count > 0 )
            {
                if ( groupedEntity.RelatedEntities.Any(e => MatchGroupedEntityNode(conditionNodeId, e)))
                {
                    isMatched = true;
                }
            }
            return isMatched;
        }

        private bool ConditionContainArgument(QueryCondition condition)
        {
            //if the number of arguments that a particular type of condition operator requires is zero, skip the check
            if (ConditionTypeHelper.GetArgumentCount(condition.Operator) == 0)
            {
                return true;
            }
            else
            {
                if ((condition.Argument != null && condition.Argument.Value != null)
                    ||
					( condition.Arguments != null && condition.Arguments.Count > 0 && condition.Arguments [ 0 ].Value != null ) )
                    return true;
                else
                    return false;

            }
        }


        /// <summary>
        /// Repackages and passes an expression from an aggregate subquery back to the parent query.
        /// </summary>
        private SqlExpression CreateAggregateMappingExpression(SqlQuery parentQuery, SqlTable proxyTable, SqlExpression sqlExpr)
        {
            var subQuery = proxyTable.SubQuery.Queries[0];

            string alias = parentQuery.AliasManager.CreateAlias("aggCol");
            var groupByColumn = new SqlSelectItem
            {
                Expression = sqlExpr,
                Alias = alias
            };
            subQuery.SelectClause.Items.Add(groupByColumn);
            subQuery.GroupByClause.Expressions.Add(groupByColumn.Expression);

            // Register an expression that can be used to access the group-by expression
            string mappedSql = GetColumnSql( proxyTable, alias );
            var mappedSqlExpr = new SqlExpression(mappedSql);
            SqlExpression.CopyTransforms(sqlExpr, mappedSqlExpr);
            mappedSqlExpr.DatabaseType = sqlExpr.DatabaseType;
            return mappedSqlExpr;
        }


        /// <summary>
        /// Unpacks and passes an order-by expression from an aggregate subquery back to the parent query.
        /// </summary>
        private SqlExpression CreateAggregateOrderbyMappingExpression(SqlQuery parentQuery, SqlTable proxyTable, string orderBySql)
	    {
            string[] parts = orderBySql.Split(new[] { SqlOrderClause.OrderByDelimiter }, StringSplitOptions.None);
            var resultParts = (from part in parts
                          select CreateAggregateMappingExpression(parentQuery, proxyTable, new SqlExpression(part)).Sql);
            var result = string.Join(SqlOrderClause.OrderByDelimiter, resultParts);
            return new SqlExpression(result);
	    }


	    /// <summary>
		///     Handles a relationship downcast. (For example, where a relationship returns all actors, but only types derived from People should be shown).
		/// </summary>
		/// <param name="downcast"></param>
		/// <param name="parentTable">The parent table.</param>
		/// <param name="sqlQuery">The SQL query that the table will be created in.</param>
		/// <returns>The table to use for upward joins.</returns>
		private EntityTables RegisterDownCastResourceEntity( DownCastResource downcast, SqlTable parentTable, SqlQuery sqlQuery )
		{
			// Note: generally we are able to completely reuse the parent table, as it already selects the entities we require.
			// Additional filtering is only applied if explicitly requested.

			SqlTable entityTable = parentTable;

			if ( downcast.MustExist )
			{
				// Just constrain the parent table directly
				ConstrainDefinitionType( parentTable, downcast.EntityTypeId, downcast.ExactType, sqlQuery );
			}
			else if ( downcast.ExactType )
			{
				// If we want the exact type, but don't want to constrain the parent with a must exist, then we
				// must rejoin to entity table so we can have one constrained table, and one unconstrained one.
				entityTable = sqlQuery.CreateJoinedTable( "dbo.Entity", "dc", parentTable, JoinHint.Unspecified, "Id", parentTable.IdColumn );

				entityTable.IdColumn = "Id";
				ConstrainDefinitionType( entityTable, downcast.EntityTypeId, true, sqlQuery );
			}

			// If MustExist=false and ExactType=false (the typical case) we just reuse the parent table without adding any constraints,
			// and just rely on the fact that incorrect types simply won't have any data for the requested fields or relationships.

			return new EntityTables
				{
					EntityTable = entityTable,
					HeadTable = parentTable
				};
        }


        /// <summary>
        ///     Handles a custom join.
        /// </summary>
        /// <param name="customJoinNode"></param>
        /// <param name="parentTable">The parent table.</param>
        /// <param name="sqlQuery">The SQL query that the table will be created in.</param>
        /// <returns>The table to use for upward joins.</returns>
        private EntityTables RegisterCustomJoinNode( CustomJoinNode customJoinNode, SqlTable parentTable, SqlQuery sqlQuery )
        {
            string predicateScript = customJoinNode.JoinPredicateScript;
            if ( predicateScript == null )
                throw new Exception( "No script was specified." );
            if ( customJoinNode.EntityTypeId == null )
                throw new Exception( "No type was specified." );

            SqlTable childTable = ConstrainDefinitionType( null, customJoinNode.EntityTypeId, customJoinNode.ExactType, sqlQuery );

            if ( childTable == null )
            {
                // Ensure primary table
                // primary may still be null in scenario where joined is 'all resource types', or if root type check is suppressed
                childTable = sqlQuery.CreateTable( "dbo.Entity", "e" );
                childTable.FilterByTenant = true;
                childTable.IdColumn = "Id";
            }

            childTable.Parent = parentTable;
            parentTable.Children.Add( childTable );
            childTable.DependsOnOtherJoins = true;

            // Join condition gets stitched up later in ApplyScriptCustomJoinPredicates

            // Join hints
            JoinHint joinHint;
            if ( customJoinNode.ResourceMustExist )
            {
                joinHint = JoinHint.Required;
            }
            else if ( customJoinNode.ResourceNeedNotExist )
            {
                joinHint = JoinHint.DontConstrainParent;
            }
            else
            {
                joinHint = JoinHint.Unspecified;
            }
            childTable.JoinHint = joinHint;
            childTable.JoinNotConstrainedByParent = customJoinNode.ParentNeedNotExist;

            return new EntityTables( childTable );
        }


        /// <summary>
        ///     Handles a custom join.
        /// </summary>
        /// <param name="singleRowNode"></param>
        /// <param name="parentTable">The parent table.</param>
        /// <param name="sqlQuery">The SQL query that the table will be created in.</param>
        /// <returns>The table to use for upward joins.</returns>
        private EntityTables RegisterSingleRowNode( SingleRowNode singleRowNode, SqlTable parentTable, SqlQuery sqlQuery )
        {
            SqlTable table = sqlQuery.CreateJoinedTable( "(values(1))", "t", parentTable, JoinHint.Unspecified, null, null );
            table.NameContainsSql = true;
            table.FullTableAlias = table.TableAlias + "(val)";
            table.FilterByTenant = false;

            return new EntityTables( table );
        }


        /// <summary>
        /// Determines whether the relationship implicitly secures resources in the specified direction.
        /// </summary>
        /// <param name="relationship">The relationship.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        private bool DoesRelationshipImplicitlySecureResources(Relationship relationship, RelationshipDirection direction)
        {
            // Special case : resources implicitly secure their types in the reporting engine only.
            if ( direction == RelationshipDirection.Forward && relationship.Id == WellKnownAliases.CurrentTenant.IsOfType )
                return true;

            bool? secures = direction == RelationshipDirection.Forward ? relationship.SecuresTo : relationship.SecuresFrom;            

            return (secures ?? false);            
        }


		/// <summary>
		///     Generates SqlTables for a related resource.
		/// </summary>
		/// <param name="relationship">The relationship.</param>
		/// <param name="parentTable">The parent table.</param>
		/// <param name="sqlQuery">The SQL query that the table will be created in.</param>
		/// <returns>The table to use for upward joins.</returns>
		private EntityTables RegisterRelatedResourceEntity( RelatedResource relationship, SqlTable parentTable, SqlQuery sqlQuery )
		{
			const string fromColumnName = "FromId";
			const string toColumnName = "ToId";
			bool forward = relationship.RelationshipDirection == RelationshipDirection.Forward;

			string relationshipTableName;

			// Determine if its a virtual relationship
			IEntity relationshipDefinition = Model.Entity.Get( relationship.RelationshipTypeId );

			bool isRecursive = relationship.Recursive != RecursionMode.None;
			bool isNormal = !isRecursive;

            // Ensure resource type is set
		    var relEnt = relationshipDefinition.As<Model.Relationship>();
            if (relEnt == null)
            {
                EventLog.Application.WriteError(@"Invalid relationship specified or invalid, Relationship entityId = {0}", relationship.RelationshipTypeId);
                throw new InvalidOperationException("Relationship not specified or invalid.");
            }

            // Get endpoint type.
		    EntityType entityType = relationship.RelationshipDirection == RelationshipDirection.Forward ? relEnt.ToType : relEnt.FromType;
		    if (entityType == null)
		    {
                throw new Exception("Could not load relationship endpoint type for relationship " + relationship.RelationshipTypeId);
		    }
		    relationship.EntityTypeId = entityType.Id;

		    // Determine table name
			if ( isRecursive )
			{
			    long? fromTypeId = relEnt.FromType?.Id;
                long? toTypeId = relEnt.ToType?.Id;
			    if ( fromTypeId == null || toTypeId == null )
			        throw new Exception( $"Relationship {relEnt.Id} is missing endpoint type details." );

                string relationshipTypeIdParamName = RegisterSharedParameter( System.Data.DbType.Int64, relationship.RelationshipTypeId.Id.ToString( CultureInfo.InvariantCulture ) );
				string fromTypeIdParamName = RegisterSharedParameter( System.Data.DbType.Int64, fromTypeId.Value.ToString( CultureInfo.InvariantCulture ) );
				string toTypeIdParamName = RegisterSharedParameter( System.Data.DbType.Int64, toTypeId.Value.ToString( CultureInfo.InvariantCulture ) );

				relationshipTableName = sqlQuery.AliasManager.CreateAlias( "#rec" );

				string formattedRelEntityId = FormatEntity( relationship.RelationshipTypeId, true, relationshipTypeIdParamName );
				string formattedFromEntityId = FormatEntity( relEnt.FromType, true, fromTypeIdParamName );
				string formattedToEntityId = FormatEntity( relEnt.ToType, true, toTypeIdParamName );

				relationshipTableName += $"_{formattedRelEntityId}_{formattedFromEntityId}_{formattedToEntityId}";

				string create = $"\nCREATE TABLE {relationshipTableName} ( FromId BIGINT, ToId BIGINT, Depth INT, TypeId BIGINT, TenantId BIGINT )\nINSERT INTO {relationshipTableName} SELECT * FROM dbo.fnGetRelationshipRecAndSelf({FormatEntity( relationship.RelationshipTypeId, relationshipTypeIdParamName )}, @tenant, {( relationship.Recursive == RecursionMode.Recursive ? 0 : 1 )}, {FormatEntity( relEnt.FromType, fromTypeIdParamName )}, {FormatEntity( relEnt.ToType, toTypeIdParamName )})";
				string drop = $"\nDROP TABLE {relationshipTableName}";

				if ( _querySettings.UseSharedSql )
				{
					// Return the sql used to create/drop the tables.
					// Do not emit it as part of the sql for the query.
					_sharedSqlPreamble.Add( create );
					_sharedSqlPostamble.Add( drop );
				}
				else
				{
					// Check if the shared sql exists.
					// This is an optimization.
					bool isSharedSqlAvailable = IsSharedSqlPreambleAvailable( create );

					if ( !isSharedSqlAvailable )
					{
						// Emit the sql as part of the query                            
						_sqlBatch.SqlPreamble = string.Concat( Environment.NewLine, create, _sqlBatch.SqlPreamble );
						_sqlBatch.SqlPostamble = string.Concat( _sqlBatch.SqlPostamble, Environment.NewLine, drop );
					}
				}
			}
			else
			{
                relationshipTableName = "dbo.Relationship";
			}


			// Determine relationship
			// TODO: this is suboptimal. Ideally, if ResourceMustExist=false, then JoinHint should be 'Unspecified'
			// then if any ResourceDataColumn expressions actually point to it and its still Unspecified then it should be flipped to 'Optional' at that time.
			// This will result in more efficient joins.
			JoinHint joinHint;
			if ( relationship.CheckExistenceOnly )
			{
				joinHint = JoinHint.ConstrainWithExists;
			}
			else if ( relationship.ResourceMustExist )
			{
                joinHint = JoinHint.Required;
            }
            else if (relationship.ResourceNeedNotExist)
            {
                joinHint = JoinHint.DontConstrainParent;
            }
            else if ( relationship.ConstrainParent )
			{
				joinHint = JoinHint.Constrain;
            }
            else
			{
				joinHint = JoinHint.Unspecified;
			}

			// Create the relationship table
			SqlTable relationshipTable =
				sqlQuery.CreateJoinedTable( relationshipTableName, "rel", parentTable, joinHint, forward ? fromColumnName : toColumnName, parentTable.IdColumn );

		    relationshipTable.JoinNotConstrainedByParent = relationship.ParentNeedNotExist;

            // Column that child tables should join to
            //if (isRecursive)
            //{
            //    relationshipTable.IdColumn = "Id";
            //}
            //else
            //{
            relationshipTable.IdColumn = forward ? toColumnName : fromColumnName;
			//}


			if ( isRecursive )
			{
				//relationshipTable.NameContainsSql = true;

				// Specify depth constraint
				if ( relationship.Recursive == RecursionMode.Recursive )
				{
					relationshipTable.Conditions.Add( "$.Depth > 0" );
				}
            }
            if ( isNormal )
            {
                // Filter tenant
                relationshipTable.FilterByTenant = true;
            }

            string relationshipTypeIdParamName1 = RegisterSharedParameter( System.Data.DbType.Int64, relationship.RelationshipTypeId.Id.ToString( CultureInfo.InvariantCulture ) );

            // Specify relationship type
            relationshipTable.Conditions.Add( "$.TypeId = " + FormatEntity( relationship.RelationshipTypeId, relationshipTypeIdParamName1 ) );

            // Secure the join
            relationshipTable.SecureResources = _querySettings.SecureQuery;

            if (relationshipTable.SecureResources)
            {
                // Note: A relationship cannot implicitly secure a full join because
                // target entities will be viewed even in the absence of a securing relationship
                bool implicitlySecured = !relationshipTable.JoinNotConstrainedByParent
                    && DoesRelationshipImplicitlySecureResources( relEnt, relationship.RelationshipDirection );
                relationshipTable.IsImplicitlySecured = implicitlySecured;
            }

			// Explicitly manufacture relationships (edge case)
			if ( relationship.FauxRelationships != null )
			{
				ApplyFauxRelationships( relationship, relationshipTable, sqlQuery );

				// Ordinarily we could inner (or whatever) join the entity table direct to the relationship table, but if we are manufacturing a faux relationship
				// and the related entity doesn't exist yet (e.g. because it's new and not yet saved) then we must left join, to get the null row, otherwise we don't get the relationship row.
				// It's a bit hacky, but at least means we're not disturbing the table tree.
                if ( !relationship.FauxRelationships.HasTargetResource )
				{
					relationshipTable.JoinHint = JoinHint.Optional;
				}
			}

			return new EntityTables
				{
					EntityTable = relationshipTable,
					HeadTable = relationshipTable
				};
		}

		/// <summary>
		///     Generates a SqlTable for a resource. Does not join it to anything.
		/// </summary>
		/// <param name="entity">The request for the resource entity.</param>
		/// <param name="sqlQuery">The SQL query that the table will be created in.</param>
		/// <returns>The table to use for upward joins.</returns>
		private EntityTables RegisterResourceEntity( ResourceEntity entity, SqlQuery sqlQuery )
		{
            SqlTable primary = null;

            // Apply ID filter. If there was no type check then this may become the root table.
            if ( _querySettings.SupportRootIdFilter )
            {
                primary = AddRootIdFilter( sqlQuery, null );
            }

            // Type constraint not required when applying security as the root entity gets constrained already.
            if ( !_querySettings.SuppressRootTypeCheck )
            {
                // primary may be null
                primary = ConstrainDefinitionType( primary, entity.EntityTypeId, entity.ExactType, sqlQuery );
            }

            if ( primary == null )
            {
                // Ensure primary table
                // primary may still be null in scenario where root table is 'all resource types', or if root type check is suppressed
                primary = sqlQuery.CreateTable( "dbo.Entity", "e" );
                primary.FilterByTenant = true;
                primary.IdColumn = "Id";
            }

            primary.SecureResources = _querySettings.SecureQuery;
            return new EntityTables( primary );
        }

        /// <summary>
        /// Adds a filter that can be used to filter resources at a top-level via a parameter list.
        /// </summary>
        /// <param name="sqlQuery">The SQL query.</param>
        /// <param name="rootTable">Optionally, the root table to join to. May be null.</param>
        /// <remarks>The root table, or if there was one then the newly created table.</remarks>
        private SqlTable AddRootIdFilter( SqlQuery sqlQuery, SqlTable rootTable )
        {
            SqlTable childTable;
            SqlTable result = rootTable;

            if ( rootTable == null )
            {
                childTable = sqlQuery.CreateTable( QueryRunner.EntityListParameterName, "el" );
                childTable.IdColumn = "Id";
                childTable.FilterByTenant = false;
                result = childTable;
            }
            else
            {
                childTable = sqlQuery.CreateJoinedTable( QueryRunner.EntityListParameterName, "el", rootTable, JoinHint.Required, "Id", rootTable.IdColumn );
            }
            
            childTable.NameContainsSql = true;
            return result;
        }

        /// <summary>
        ///     A direct join back to some parent table.
        /// </summary>
        /// <param name="joinToSelf">The joinToSelf instance.</param>
        /// <param name="parentTable">The parent table.</param>
        /// <param name="sqlQuery">The SQL query that the table will be created in.</param>
        /// <returns>The table to use for upward joins.</returns>
        private EntityTables RegisterJoinToSelfEntity(JoinToSelfEntity joinToSelf, SqlTable parentTable, SqlQuery sqlQuery)
        {
            // Join entity table back to the parent table
            SqlTable table = sqlQuery.CreateJoinedTable("dbo.Entity", "e", parentTable, JoinHint.Unspecified, "Id", parentTable.IdColumn );

            table.IdColumn = "Id";
            table.FilterByTenant = true;
            table.SecureResources = false;    // identity join to parent, which is already secured

            return new EntityTables(table);
        }
	}
}