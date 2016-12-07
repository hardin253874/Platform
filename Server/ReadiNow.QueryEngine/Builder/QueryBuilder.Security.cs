// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using EDC.Common;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using Model = EDC.ReadiNow.Model;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using ReadiNow.QueryEngine.Builder.SqlObjects;

using Entity = EDC.ReadiNow.Metadata.Query.Structured.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using ReadiNow.QueryEngine.Runner;
using EDC.ReadiNow.Core.Cache;

namespace ReadiNow.QueryEngine.Builder
{

    /// <summary>
	///     QueryBuilder partial class.
	///     Contains members for evaluating security and embedding CTEs.
	/// </summary>
	public partial class QueryBuilder
	{
        /// <summary>
        /// The base name of the visible ids CTE generated
        /// for an access rule report.
        /// </summary>
        private const string SecurityCteBaseNameRpt = "visIdsRpt";

        /// <summary>
        /// The base name of the visible ids CTE generated
        /// for securing a instances of a type using the relationship secures flags.
        /// </summary>
        private const string SecurityCteBaseNameType = "visIdsType";

        /// <summary>
        /// Condition applied in on/where clause of each resource join.
        /// </summary>
        private const string SecurityPredicateStart = "exists ( ";       

        /// <summary>
        /// Condition applied in on/where clause of each resource join.
        /// </summary>
        private const string SecurityPredicate = "select 1 from [{0}] where Id = {1}";

        /// <summary>
        /// Condition applied in on/where clause of each resource join.
        /// </summary>
        private const string SecurityPredicateEnd = ")";

        /// <summary>
        /// Condition applied in on/where clause of each resource join.
        /// </summary>
        internal const string SecurityDenyPredicate = "( 1 = 0 )";

        /// <summary>
        /// Is security disabled
        /// </summary>
        private static Lazy<bool> _securityDisabled = new Lazy<bool>(
            ( ) => EDC.ReadiNow.Configuration.ConfigurationSettings.GetServerConfigurationSection( ).Security.Disabled );        
        

        /// <summary>
        /// Generate necessary queries to apply security.
        /// A CTE for each access rule report is created.
        /// Resource joins that need to be secured join against the appropriate CTE(s).
        /// </summary>
        /// <param name="fullQuery">The full query.</param>
        private void SecureQuery(SqlSelectStatement fullQuery)
        {
            using ( MessageContext msg = new MessageContext( "Reports" ) )
            using ( new SecurityBypassContext( ) )
            {
                msg.Append( ( ) => new string( '-', 50 ) );
                msg.Append( ( ) => "Security CTE" );

                // Don't apply security to a query that itself is being rendered to supply security.
                if (!_querySettings.SecureQuery)
                {
                    return;
                }

                bool allowAll = false;
                bool denyAll = false;

                ISet<long> securableTypes = new HashSet<long>();

                // Get all the resource types referenced by the structured query.
                HashSet<EntityRef> referencedTypes = StructuredQueryHelper.GetReferencedResourceTypes(_structuredQuery);

                // Get all the base types of all the referenced types. Walk down then up to handle multiple inheritance.
                PerTenantEntityTypeCache cache = PerTenantEntityTypeCache.Instance;
                foreach (EntityRef refType in referencedTypes)
                {
                    securableTypes.UnionWith(cache.GetDescendantsAndSelf(refType.Id)
                                                  .SelectMany(cache.GetAncestorsAndSelf));
                }

                IEnumerable<long> implicitlySecuredEntityTypes;
                List<AccessRuleQuery> accessRuleQueries;
                List<AccessRuleQuery> systemAccessRuleQueries;

                using (CacheManager.ExpectCacheMisses())
                {
                    // Get any implicitly secured entity types and relationships
                    using (Profiler.Measure("GetImplicitlySecuredEntityTypes"))
                    {
                        implicitlySecuredEntityTypes = GetImplicitlySecuredEntityTypes(_structuredQuery);
                    }

                    // Find all the access control structured queries for the securable types that the current user has read access to.
                    accessRuleQueries = Factory.QueryRepository.GetQueries( _querySettings.RunAsUser, Permissions.Read, securableTypes.ToList( ) )
                        .Where( arq => arq != null && !arq.IgnoreOnReports ).ToList( );

                    // Find all the system access control structured queries for the securable types that the current user has read access to.
                    systemAccessRuleQueries = Factory.SystemQueryRepository.GetQueries(_querySettings.RunAsUser, Permissions.Read, securableTypes.ToList())
                        .Where(arq => arq != null && !arq.IgnoreOnReports).ToList();
                }

                // If no auth queries were found then deny all.
	            var securedEntityTypes = implicitlySecuredEntityTypes as IList<long> ?? implicitlySecuredEntityTypes.ToList( );

	            if (accessRuleQueries.Count > 0 || securedEntityTypes.Count > 0)
                {
                    var sharedSqlPre = new SortedSet<string>();
                    var sharedSqlPost = new SortedSet<string>();
	                var sharedParameters = new Dictionary<ParameterValue, string>( );

                    // Generate the sql for all the access rule queries
                    using (Profiler.Measure("RenderAccessRuleQueriesCtes"))
                    {
                        RenderAccessRuleQueriesCtes(fullQuery, accessRuleQueries, referencedTypes.Select(e => e.Id).ToList(), sharedSqlPre, sharedSqlPost, sharedParameters, AccessRuleCteTypeEnum.Default);
                    }

                    // Generate the sql for all the system access rule queries
                    using (Profiler.Measure("RenderSystemAccessRuleQueriesCtes"))
                    {
                        RenderAccessRuleQueriesCtes(fullQuery, systemAccessRuleQueries, referencedTypes.Select(e => e.Id).ToList(), sharedSqlPre, sharedSqlPost, sharedParameters, AccessRuleCteTypeEnum.System);
                    }

                    // Render any secured entities CTEs
                    RenderImplicitlySecuredEntitiesCtes( fullQuery, securedEntityTypes, _querySettings.RunAsUser );

                    // Determine what CTEs actually need to be rendered
                    DetermineIfCtesAreUsed( referencedTypes, fullQuery.References );

                    // Register any available shared sql so that it is not emitted multiple times
                    RegisterAvailableSharedSqlPreamble(sharedSqlPre);

                    // Update the query with the shared sql
                    UpdatePrePostSql(sharedSqlPre, sharedSqlPost);

	                UpdateSharedParameters( sharedParameters );
                }
                else
                {
                    denyAll = true;
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                fullQuery.AllowAccessToAllResources = allowAll;
                fullQuery.DenyAccessToAllResources = denyAll;

                msg.Append( ( ) => "End Security CTE" );
                msg.Append( ( ) => new string( '-', 50 ) );
            }
        }


        /// <summary>
        ///     Create and register a new CTE.
        ///     Caller must ensure that PopCte() also gets called after the CTE contents are generated.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="statement">The statement.</param>
        /// <returns></returns>
        private SqlCte CreateCte( string alias, SqlSelectStatement statement )
        {
            // Create CTE
            var cte = new SqlCte
            {
                Union = new SqlUnion( ),
                Name = statement.AliasManager.CreateAlias( alias )
            };

            // Register CTE
            _currentStatement.WithClause.Items.Add( cte );

            // If currently in CTE, ensure this CTE depends on the new one
            if ( _currentStatement.CurrentCte.Count > 0 )
            {
                _currentStatement.CurrentCte.Peek( ).DependsOn.Add( cte );
            }
            _currentStatement.CurrentCte.Push( cte );

            return cte;
        }

        /// <summary>
        ///     Call after a CreateCte
        /// </summary>
        private void PopCte( )
        {
            _currentStatement.CurrentCte.Pop( );
        }


        /// <summary>
        /// Process the access rule structured queries and generate CTEs which will be emitted in the query.
        /// </summary>
        /// <param name="fullQuery">The full query.</param>
        /// <param name="authQueries">The authentication queries.</param>
        /// <param name="referencedTypes">The referenced types.</param>
        /// <param name="sharedSqlPre">The shared pre SQL.</param>
        /// <param name="sharedSqlPost">The shared post SQL.</param>
        /// <param name="sharedParameters">The shared parameters.</param>
        /// <param name="cteType"></param>
        private void RenderAccessRuleQueriesCtes(SqlSelectStatement fullQuery, IEnumerable<AccessRuleQuery> authQueries, IEnumerable<long> referencedTypes, ISet<string> sharedSqlPre, ISet<string> sharedSqlPost, IDictionary<ParameterValue, string> sharedParameters, AccessRuleCteTypeEnum cteType)
	    {
            if (fullQuery == null || authQueries == null || referencedTypes == null)
            {
                return;
            }        

            var processedReports = new HashSet<long>();

            foreach (AccessRuleQuery accessRuleQuery in authQueries)
            {
                long accessRuleReportId = accessRuleQuery.ReportId;

                StructuredQuery accessRuleStructuredQuery = accessRuleQuery.Query;                
              
                // Sanity check. Ensure we don't generate duplicate CTEs
                if (processedReports.Contains(accessRuleReportId))
                {
                    continue;
                }
                processedReports.Add(accessRuleReportId);

                // Get the type that this rule applies to
                long ruleForTypeId = accessRuleQuery.ControlsAccessForTypeId;

                string authCteName = SecurityCteBaseNameRpt + accessRuleReportId;

                if (accessRuleStructuredQuery.SelectColumns.Count() > 1)
                {
                    // Create a copy of the structured query so that we can optimise it.
                    accessRuleStructuredQuery = accessRuleStructuredQuery.ShallowCopy();
                    StructuredQueryHelper.OptimiseAuthorisationQuery(accessRuleStructuredQuery);                                        
                }

                // Get ancestor and descendant types for this control access type.
                // For descendants, walk down then up to handle multiple inheritance.
                PerTenantEntityTypeCache cache = PerTenantEntityTypeCache.Instance;
                ISet<long> ancestors = cache.GetAncestorsAndSelf(ruleForTypeId);
                ISet<long> descendants = new HashSet<long>(cache.GetDescendantsAndSelf(ruleForTypeId)
                                                                .SelectMany(cache.GetAncestorsAndSelf));
                descendants.ExceptWith(ancestors);
                descendants.Add(ruleForTypeId);
                IEnumerable<long> ancestorsOnly = ancestors.Where( id => id != ruleForTypeId );

                // Note: When registering access rules against ancestor types, we want to ensure we have strict ancestors, as
                // those rules will only cover some instances of the types. 

                // Create object to represent the CTE
                AccessRuleCte accessRuleCte = new AccessRuleCte
                {
                    CteName = authCteName,
                    AllowAll = accessRuleQuery.DoesQueryGrantAllOfTypes( ruleForTypeId ),
                    RuleForTypeId = ruleForTypeId
                };


                accessRuleCte.PrepareCte = ( bool appliesToAllInstances ) =>
                    {
                        QueryBuild qr = null;

                        var settings = new QuerySettings
                        {
                            Hint = "Auth Rpt-" + accessRuleReportId,
                            SecureQuery = false,
                            UseSharedSql = true,
                            SuppressRootTypeCheck = appliesToAllInstances,
							SharedParameters = sharedParameters
                        };

                        if ( accessRuleCte.SqlCte != null && !appliesToAllInstances )
                        {
                            // hack unique name if we're re-processing (todo: make stuff immutable)
                            authCteName = authCteName + "-tc";
                            accessRuleCte.CteName = authCteName;
                        }

                        IQuerySqlBuilder builder = settings.SecurityQuerySqlBuilder ?? Factory.QuerySqlBuilder;
                        try
                        {
                            using ( MessageContext msg = new MessageContext( "Reports" ) )
                            {
                                msg.Append( ( ) => "Security CTE callback" );
                                if (cteType == AccessRuleCteTypeEnum.Default)
                                {
                                    msg.Append(() => string.Format("Rule {0} {1}", accessRuleReportId, Model.Entity.GetName(accessRuleReportId)));
                                }
                                else
                                {
                                    msg.Append(() => string.Format("System Rule {0}", accessRuleReportId));
                                }
                                
                                qr = builder.BuildSql( accessRuleStructuredQuery, settings );

                                msg.Append( ( ) => "End Security CTE callback" );
                            }

                            if ( qr == null )
                                throw new Exception( "Query Result was null" );
                        }
                        catch ( Exception ex )
                        {
                            AccessControlDiagnosticsHelper.WriteInvalidSecurityReportMessage(accessRuleStructuredQuery, null, ex);
                        }


                        if ( qr.SqlIsUncacheable )
                        {
                            MarkSqlAsUncacheable( "Security query was uncacheable" );
                        }
                        if ( qr.DataIsUncacheable )
                        {
                            MarkDataAsUncacheable( "Security query was uncacheable" );
                        }
                        if ( qr.DataReliesOnCurrentUser )
                        {
                            MarkDataReliesOnCurrentUser( "Security query relies on current user" );
                        }

                        string cteSql;

	                    sharedParameters.UnionWith( qr.SharedParameters );

                        if (QueryInspector.IsQueryUndamaged(accessRuleStructuredQuery) && sharedSqlPre != null && sharedSqlPost != null)
                        {
                            // Add any shared SQL to the set of shared sql statements
                            sharedSqlPre.UnionWith( qr.SharedSqlPreamble );
                            sharedSqlPost.UnionWith( qr.SharedSqlPostamble );

                            cteSql = qr.Sql;
                        }
                        else
                        {                                                        
                            AccessControlDiagnosticsHelper.WriteInvalidSecurityReportMessage(accessRuleStructuredQuery);

                            // Return null in the case of a corrupt report
                            cteSql = "select null";
                        }

                        // Create the CTE sql for this access rule report                       
                        var secCteSql = new SqlBuilder();
                        secCteSql.Append("[{0}] (Id) as (", authCteName);
                        secCteSql.AppendOnNewLine(cteSql);
                        secCteSql.AppendOnNewLine("--End Auth Rpt-" + accessRuleReportId);
                        secCteSql.AppendOnNewLine(")");

                        // Add the CTE
                        accessRuleCte.SqlCte = fullQuery.WithClause.AddRawCte(secCteSql.ToString());
                        accessRuleCte.SqlCteSuppressedRootType = settings.SuppressRootTypeCheck;
                    };


                // Register this access rule report against each descendant that is in the report referenced types
                descendants.Intersect(referencedTypes).ToList().ForEach(id => fullQuery.References.RegisterAccessRuleCteForType(id, accessRuleCte, true, cteType));

                // Register this access rule report against each referenced type which is an ancestor type
                ancestorsOnly.Intersect(referencedTypes).ToList().ForEach(id => fullQuery.References.RegisterAccessRuleCteForType(id, accessRuleCte, false, cteType));
            }            
	    }

        /// <summary>
        /// Determine the IDs of all types in the query that may require additional security checks to determine visibility via SecuresTo.
        /// That is: all types used in the query that have at least one 'SecuresTo' relationship inbound, or 'SecuresFrom' relationship outbound.
        /// Except, ignore cases where that type was reached via a relationship node in a direction that means the child can only be viewed if the parent is also visible.
        /// </summary>
        /// <param name="structuredQuery">The structured query.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">referencedTypes</exception>
        private IEnumerable<long> GetImplicitlySecuredEntityTypes( StructuredQuery structuredQuery )
        {
            var result = new HashSet<long>();

            // Visit each node
            StructuredQueryHelper.VisitNodes(structuredQuery.RootEntity, (node, ancestors) =>
            {
                long typeId = 0;

                var relatedResource = node as RelatedResource;

                // Get type from relationship node
                if ( relatedResource != null )
                {
                    var relationship = Model.Entity.Get<Relationship>( relatedResource.RelationshipTypeId );
                    if ( relationship == null )
                        return; // assert false, for happy reports

                    bool isForward = relatedResource.RelationshipDirection == RelationshipDirection.Forward;

                    if ( !relatedResource.ParentNeedNotExist )
                    {
                        // Even though the current node is an implicitly secured node, we arrived along the line of the relationship so we don't need to calculate security for the child.
                        // (However, short circuiting is possible if all target values are being shown, because the securing relationship may not be present)
                        bool implicitlySecuredByParent =
                            ( isForward && ( relationship.SecuresTo == true ) )
                            || ( ( !isForward ) && ( relationship.SecuresFrom == true ) );
                        if ( implicitlySecuredByParent )
                            return;
                    }

                    // Get target type
                    if ( relatedResource.EntityTypeId != null )
                    {
                        typeId = relatedResource.EntityTypeId.Id;
                    }
                    else
                    {
                        EntityType entityType = isForward ? relationship.ToType : relationship.FromType;
                        if (entityType != null)
                        {
                            typeId = entityType.Id;
                        }
                    }
                }
                else
                {
                    // Get type from root resource node
                    var resourceEntity = node as ResourceEntity;

                    if ( resourceEntity != null && resourceEntity.EntityTypeId != null )
                    {
                        typeId = resourceEntity.EntityTypeId.Id;
                    }
                }

                // Check and store result
                if ( typeId > 0 && !result.Contains( typeId ) )
                {
                    if ( IsTypeSecuredImplicitly( typeId ) )
                    {
                        result.Add( typeId );
                    }
                }

            });

            return result;
        }

        /// <summary>
        /// Gets the implicitly secured entity types.
        /// </summary>
        /// <param name="typeId">The type ID.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">referencedTypes</exception>
        private bool IsTypeSecuredImplicitly( long typeId )
        {
            var entityType = Model.Entity.Get<EntityType>( typeId );

            if ( entityType == null )
                throw new ArgumentException( $"Type {typeId} could not be loaded.", nameof( typeId ) );

            // Skip generating auth queries via secures to/from flags for types marked as allow read.
            // Note: Access rules should still be in place to grant access to these types.
            if ( EntityTypeHelper.IsAllowEveryoneRead( entityType ) )
            {
                return false;
            }

            using ( Profiler.Measure( "IsTypeSecuredImplicitly" ) )
            {
                if ( IsTypeSecuredImplicitly( entityType ) )
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Determines whether the type is secured implicitly.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        private bool IsTypeSecuredImplicitly(EntityType entityType)
        {
            if ( entityType == null )
                throw new ArgumentNullException( nameof( entityType ) );

            // This if check is A HACK done to make self serve reports work for CBA
            // Specifically, the 'new report' dialog, and similar, should not show objects that self serve users cannot see.
            // The 'type' object has at least one implicit relationship, which would otherwise cause all this code to run,
            // and the current implementation of 'secure implicit' causes all access rules to be evaluated, including the one
            // that allows read for all objects to everyone - even though that rule is marked to not be included in reports.
            if ( entityType.Alias == "core:type" || entityType.Alias == "core:definition" || entityType.Alias == "core:managedType" )
                return false;

            IEnumerable<Relationship> forwardRelationships;
            IEnumerable<Relationship> reverseRelationships;            

            // Get all relationships from this type
            EntityTypeHelper.GetAllRelationships(entityType.Id, true, out forwardRelationships, out reverseRelationships);

            // Process forward relationships
            IEnumerable<Relationship> securingForwardRelationships = forwardRelationships.Where(r => r?.SecuresFrom ?? false);

            if (securingForwardRelationships.Any())
            {                
                return true;
            }

            // Process reverse relationships
            IEnumerable<Relationship> securingReverseRelationships = reverseRelationships.Where(r => r?.SecuresTo ?? false);

            if (securingReverseRelationships.Any())
            {                
                return true;
            }

            return false;
        }


        /// <summary>
        /// Determines whether the type is secured implicitly.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="implicitlySecuredEntityTypes">The implicitly secured entity types.</param>
        /// <returns></returns>
        private bool IsTypeSecuredImplicitly(long typeId, IEnumerable<long> implicitlySecuredEntityTypes)
        {
	        var securedEntityTypes = implicitlySecuredEntityTypes as IList<long> ?? implicitlySecuredEntityTypes.ToList( );

	        return securedEntityTypes.Contains(typeId) || 
                securedEntityTypes.Select(t => PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(t)).Any(descendants => descendants.Contains(typeId));
        }


	    /// <summary>
        /// Gets the implicitly secured nodes.
        /// </summary>
        /// <param name="structuredQuery">The structured query.</param>
        /// <param name="implicitlySecuredEntityTypes">The implicitly secured entity types.</param>
        /// <returns></returns>
	    private ISet<Guid> GetImplicitlySecuredNodes(StructuredQuery structuredQuery, IEnumerable<long> implicitlySecuredEntityTypes)
        {            
            var result = new HashSet<Guid>();           

            StructuredQueryHelper.VisitNodes(structuredQuery.RootEntity, (node, ancestors) =>
            {
                var relatedResource = node as RelatedResource;

                if (relatedResource != null)
                {
                    if (relatedResource.EntityTypeId != null)
                    {
                        if (IsTypeSecuredImplicitly(relatedResource.EntityTypeId.Id, implicitlySecuredEntityTypes))
                        {                            
                            result.Add(relatedResource.NodeId);
                        }                                            
                    }
                    else
                    {
                        var relationship = Model.Entity.Get<Relationship>(relatedResource.RelationshipTypeId);
                        if (relationship != null)
                        {
                            switch (relatedResource.RelationshipDirection)
                            {
                                case RelationshipDirection.Forward:
                                    if (relationship.ToType != null &&
                                        IsTypeSecuredImplicitly(relationship.ToType.Id, implicitlySecuredEntityTypes))
                                    {                                        
                                        result.Add(relatedResource.NodeId);                                        
                                    }
                                    break;
                                case RelationshipDirection.Reverse:
                                    if (relationship.FromType != null &&
                                        IsTypeSecuredImplicitly(relationship.FromType.Id, implicitlySecuredEntityTypes))
                                    {                                        
                                        result.Add(relatedResource.NodeId);
                                    }
                                    break;
                            }
                        }
                    }

                    return;
                }

                var resourceEntity = node as ResourceEntity;                

                if (resourceEntity != null &&
                    resourceEntity.EntityTypeId != null &&
                    IsTypeSecuredImplicitly(resourceEntity.EntityTypeId.Id, implicitlySecuredEntityTypes))
                {                    
                    result.Add(resourceEntity.NodeId);
                }
            });

            return result;
        }


        /// <summary>
        /// Finds the non aggregate entity parent node.
        /// </summary>
        /// <param name="entityNode">The entity node.</param>
        /// <param name="ancestorNodes">The ancestor nodes.</param>
        /// <returns></returns>
	    private Entity FindNonAggregateEntityParentNode(Entity entityNode, IEnumerable<Entity> ancestorNodes)
	    {
	        while (true)
	        {
	            Entity parentNode = ancestorNodes.FirstOrDefault(node =>
	            {
	                var aggregateEntity = node as AggregateEntity;
	                if (aggregateEntity == null)
	                {
	                    return node.RelatedEntities.Contains(entityNode);
	                }

	                return aggregateEntity.GroupedEntity == entityNode;
	            });

	            if (!(parentNode is AggregateEntity))
	            {
	                return parentNode;
	            }
	            entityNode = parentNode;	            	            
	        }
	    }


        /// <summary>
        /// Removes the aggregate entity nodes.
        /// </summary>
        /// <param name="structuredQuery">The structured query.</param>
	    private void RemoveAggregateEntityNodes(StructuredQuery structuredQuery)
        {            
            IEnumerable<Entity> nodes = StructuredQueryHelper.WalkNodes(structuredQuery.RootEntity).ToList();

            foreach (Entity node in nodes)
            {
                var aggregateEntity = node as AggregateEntity;

                if (aggregateEntity == null ||
                    aggregateEntity.GroupedEntity is AggregateEntity)
                {
                    continue;
                }
                
                // Find the parent node
                Entity parentNode = FindNonAggregateEntityParentNode(aggregateEntity, nodes);

                if (parentNode == null)
                {
                    structuredQuery.RootEntity = aggregateEntity.GroupedEntity;
                }
                else
                {
                    parentNode.RelatedEntities.Add(aggregateEntity.GroupedEntity);
                    parentNode.RelatedEntities.Remove(aggregateEntity);
                }
            }
        }

        /// <summary>
        /// Transforms the structured query to a query returning implicitly secured resources.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="implicitlySecuredEntityTypes">The implicitly secured entity types.</param>
        /// <returns></returns>
        private StructuredQuery TransformStructuredQueryToImplicitlySecured(StructuredQuery query, IEnumerable<long> implicitlySecuredEntityTypes)
	    {
            // Get all the query nodes returning implicitly secured entity types
            ISet<Guid> implicitlySecuredNodeIds = GetImplicitlySecuredNodes(query, implicitlySecuredEntityTypes);

			if ( implicitlySecuredNodeIds.Count <= 0 )
            {
                return null;
            }

            // Create a deep copy of the structured query
            StructuredQuery transformedQuery = query.DeepCopy();

            // Copy any conditions from columns into the condition expression as the columns will be removed
            IEnumerable<QueryCondition> qcWithColumnRefs = transformedQuery.Conditions.Where(qc => qc.Expression is ColumnReference);

            foreach (QueryCondition qc in qcWithColumnRefs)
            {
                var columnRef = qc.Expression as ColumnReference;
                if (columnRef == null)
                {
                    continue;
                }

                // Get the referenced column
                SelectColumn referencedColumn = transformedQuery.SelectColumns.FirstOrDefault(c => c.ColumnId == columnRef.ColumnId);
                if (referencedColumn == null)
                {
                    continue;
                }

                // Copy the expression from the column to the condition
                qc.Expression = referencedColumn.Expression;
            }

            // Remove aggregate nodes
            RemoveAggregateEntityNodes(transformedQuery);

            // Clear orderby and select columns
            transformedQuery.OrderBy.Clear();
            transformedQuery.SelectColumns.Clear();            

            // Add back select columns for implicitly secured resource ids only
            foreach (Guid nodeId in implicitlySecuredNodeIds)
            {
                transformedQuery.SelectColumns.Add(new SelectColumn()
                {
                    ColumnId = Guid.NewGuid(),
                    Expression = new IdExpression {NodeId = nodeId}
                });
            }
            //set select distinct in security query
            transformedQuery.Distinct = true;
            return transformedQuery;
	    }


        /// <summary>
        /// Gets the entities of the specified types from the result.
        /// </summary>
        /// <param name="entityTypeIds">The entity type ids.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private ICollection<long> GetEntitiesOfType(ISet<long> entityTypeIds, QueryResult result)
	    {
            // Entities of the specified types
            var entities = new HashSet<long>();

            // Get all the columns returning resources of the specified types
            List<ResultColumn> columns = result.Columns.Where(c => entityTypeIds.Contains(c.ResourceTypeId)).ToList( );

            if (columns.Count <= 0)
            {
                return entities;
            }

            // Get all the column indexes of the result columns
            IEnumerable<int> columnIndexes = columns.Select(c => result.Columns.IndexOf(c));

            // Get the entity ids
            foreach (int columnIndex in columnIndexes)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {                    
                    if (row.IsNull(columnIndex))
                    {
                        continue;
                    }

                    entities.Add((long)row[columnIndex]);
                }
            }            

            return entities;
	    }


        /// <summary>
        /// Renders the implicitly secured entities ctes.
        /// </summary>
        /// <param name="fullQuery">The full query.</param>
        /// <param name="implicitlySecuredEntityTypes">The implicitly secured entity types.</param>
        /// <param name="userId">The user identifier.</param>
        private void RenderImplicitlySecuredEntitiesCtes(SqlSelectStatement fullQuery, IList<long> implicitlySecuredEntityTypes, long userId)
	    {
            if (implicitlySecuredEntityTypes == null || implicitlySecuredEntityTypes.Count <= 0) return;

            // Transform the structured query to a query returning the ids of implicitly secured entity types
            StructuredQuery implicitlySecuredStructuredQuery = TransformStructuredQueryToImplicitlySecured(_structuredQuery, implicitlySecuredEntityTypes);

            if ( implicitlySecuredStructuredQuery == null )
            {
                return;
            }

            // Get runtime settings for implicitly-secured calculation
            // Note: for implicit security we need runtime settings even to just build the query.
            // This disqualifies the query for caching if we actually end up using the data, but we decide that if RenderSecuredEntitiesSql gets called.
            QuerySettings runtimeSettings = _querySettings as QuerySettings;
            if ( runtimeSettings == null )
            {
                // If we were only provided build-settings, then assume defaults for all runtime settings.
                runtimeSettings = new QuerySettings( );
            }

            // Build settings for implicit-security calculation run
            QuerySettings settingsForImplicitSecurity = new QuerySettings( )
            {
                SecureQuery = false,
                Hint = "Implicit security for report - " + _querySettings.Hint,
                TargetResource = runtimeSettings.TargetResource,
                IncludeResources = runtimeSettings.IncludeResources,
                ExcludeResources = runtimeSettings.ExcludeResources,
            };
            if ( _querySettings.EmergencyDecorationCallback != null )
            {
                _querySettings.EmergencyDecorationCallback( settingsForImplicitSecurity );
            }

            // This query gives us all the implicitly secured resources the original query may return
            IQueryRunner queryRunner = _querySettings.SecurityQueryRunner ?? Factory.QueryRunner;
            QueryResult result = queryRunner.ExecuteQuery( implicitlySecuredStructuredQuery, settingsForImplicitSecurity );

            var processedEntityTypes = new HashSet<long>( );

            foreach (long entityTypeId in implicitlySecuredEntityTypes)
            {                
                // Sanity check. Ensure we don't generate duplicate CTEs
                if (processedEntityTypes.Contains(entityTypeId))
                {
                    continue;
                }

                processedEntityTypes.Add(entityTypeId);

                // Skip generating auth queries via secures to/from flags for types marked as allow read.
                // Note: Access rules should still be in place to grant access to these types.
                if (EntityTypeHelper.IsAllowEveryoneRead(Model.Entity.Get<EntityType>(entityTypeId)))
                {
                    continue;
                }

                // Skip generating auth queries via secures to/from flags for nodes that could be of any resource type.
                // This is done for performance (at the cost of correctness). A node for all resource will bring in so much content that the report never renders.
                // E.g. for application library 'Resources in solution' See bug 25633
                if ( entityTypeId == ( new EntityRef( "core:resource" ) ).Id )
                {
                    EventLog.Application.WriteWarning( "Ignoring secures from/to rules for report node of type resource. See #25633" );
                    continue;
                }

                ISet<long> descendants = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(entityTypeId);

                string accessRuleCteName = SecurityCteBaseNameType + entityTypeId;

                AccessRuleCte accessRuleCte = new AccessRuleCte
                {
                    CteName = accessRuleCteName,
                    RuleForTypeId = entityTypeId,
                    AllowAll = false
                };

                accessRuleCte.PrepareCte = ( bool appliesToAllInstances ) =>
                {
                    // Get all entities of the specified type
                    ICollection<long> entityIds = GetEntitiesOfType( descendants, result );

                    // Get the entity ids that the current user has access to
                    ICollection<long> securedEntityIds = GetSecuredEntities( entityIds, userId );

                    // Render the sql that returns all the entities of the current
                    // type that the current user has access to           
                    string sql = RenderSecuredEntitiesSql( entityTypeId, securedEntityIds, fullQuery.References );                   

                    // Create the CTE sql for this access rule report
                    var secCteSql = new SqlBuilder( );
                    secCteSql.Append( "[{0}] (Id) as (", accessRuleCteName );
                    secCteSql.AppendOnNewLine( "--QueryEng (hint: EntityType-{0})", entityTypeId );
                    secCteSql.AppendOnNewLine( sql );
                    secCteSql.AppendOnNewLine( "--End EntityType-" + entityTypeId );
                    secCteSql.AppendOnNewLine( ")" );

                    accessRuleCte.SqlCte = fullQuery.WithClause.AddRawCte(secCteSql.ToString());
                    accessRuleCte.SqlCteSuppressedRootType = false;
                };

                // Register the CTE for each of the descendents of this type.                
                descendants.ToList().ForEach(id => fullQuery.References.RegisterAccessRuleCteForType(id, accessRuleCte, false, AccessRuleCteTypeEnum.Implicit));    // Pass false if unsure 
            }
	    }


        /// <summary>
        /// Renders the secured entities SQL.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="securedEntityIds">The secured entity ids.</param>
        /// <param name="references">The references.</param>
        /// <returns></returns>
        private string RenderSecuredEntitiesSql(long entityTypeId, ICollection<long> securedEntityIds, ReferenceManager references)
        {
            if (securedEntityIds.Count <= 0)
            {
                return "select null";
            }
            
            references.RegisterEntityBatch(entityTypeId, securedEntityIds);            

            var sqlBuilder = new SqlBuilder();

            sqlBuilder.AppendOnNewLine("select");
            sqlBuilder.AppendOnNewLine("    b.EntityId");
            sqlBuilder.AppendOnNewLine("from");
            sqlBuilder.AppendOnNewLine( "    {0} b", QueryRunner.EntityBatchParameterName );
            sqlBuilder.AppendOnNewLine("where");
            sqlBuilder.AppendOnNewLine("    b.BatchId = {0}", entityTypeId);            

            MarkSqlAsUncacheable( "Precalculated implicitly secured entities" );

            return sqlBuilder.ToString();
	    }

		/// <summary>
		/// Updates the shared parameters.
		/// </summary>
		/// <param name="sharedParameters">The shared parameters.</param>
	    private void UpdateSharedParameters( IDictionary<ParameterValue, string> sharedParameters  )
	    {
		    _sqlBatch.SharedParameters.UnionWith( sharedParameters );
	    }

	    /// <summary>
        /// Updates the pre post SQL.
        /// </summary>
        /// <param name="sharedSqlPre">The shared SQL pre statements.</param>
        /// <param name="sharedSqlPost">The shared SQL post statements.</param>
        private void UpdatePrePostSql(ISet<string> sharedSqlPre, ISet<string> sharedSqlPost)
	    {
			if ( sharedSqlPre.Count > 0 )
            {                
                // Update the report sql preamble                
                _sqlBatch.SqlPreamble = string.Concat(_sqlBatch.SqlPreamble, Environment.NewLine, string.Join(Environment.NewLine, sharedSqlPre));
            }

			if ( sharedSqlPost.Count > 0 )
            {
                // Update the report sql postamble                
                _sqlBatch.SqlPostamble = string.Concat(_sqlBatch.SqlPostamble, Environment.NewLine, string.Join(Environment.NewLine, sharedSqlPost));               
            }                        
	    }
               

		/// <summary>
		///     Fast single-pass substitution of aliases into SQL text.
		///     E.g. select FromId from Relationship where TypeId = $shared:worksFor$
		///     Dollar sign can be escaped (in sql content, not alias) by passing it twice.
		/// </summary>
		/// <param name="sql">The SQL.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Unbalanced escape sequence</exception>
		private string SubstituteAliases( string sql )
		{
			var sb = new StringBuilder( );

			int pos = 0;

			while ( true )
			{
				if ( pos == sql.Length )
				{
					break;
				}
				int startAlias = sql.IndexOf( '$', pos );
				if ( startAlias == -1 )
				{
					break;
				}

				string rawSql = sql.Substring( pos, startAlias - pos );
				sb.Append( rawSql );

				int endAlias = sql.IndexOf( '$', startAlias + 1 );
				if ( endAlias == -1 )
				{
					throw new Exception( "Unbalanced escape sequence" );
				}

				int aliasLength = endAlias - startAlias - 1;
				if ( aliasLength == 0 )
				{
					sb.Append( "$" );
				}
				else
				{
					string alias = sql.Substring( startAlias + 1, aliasLength );
					var er = new EntityRef( alias );

					string erParamName = RegisterSharedParameter( DbType.Int64, er.Id.ToString( CultureInfo.InvariantCulture ) );

					string id = FormatEntity( er, erParamName );
					sb.Append( id );
				}
				pos = endAlias + 1;
			}
			if ( pos != sql.Length )
			{
				sb.Append( sql.Substring( pos ) );
			}

			string result = sb.ToString( );
			return result;
		}

        /// <summary>
        /// Return true if the specified list of access rules allow access to all.
        /// </summary>
        /// <param name="accessRuleCtes">The access rules.</param>
        /// <param name="allowAllDefault">True if the default rules allow all.</param>
        /// <param name="allowAllSystem">True if the system rules allow all.</param>
        /// <returns>True if the default and system rules allow all.</returns>
        private static bool DoAccessRulesAllowAll(IEnumerable<AccessRuleCteTypeRegistration> accessRuleCtes, out bool allowAllDefault, out bool allowAllSystem)
        {
            allowAllSystem = false;
            allowAllDefault = false;

            bool allowAll = false;
            
            foreach (var accessRuleCteReg in accessRuleCtes)
            {                
                if (accessRuleCteReg.RuleIsApplicableToAllInstances && accessRuleCteReg.AccessRuleCte.AllowAll)
                {
                    if (accessRuleCteReg.AccessRuleType == AccessRuleCteTypeEnum.Default)
                    {
                        allowAllDefault = true;
                    }
                    else if (accessRuleCteReg.AccessRuleType == AccessRuleCteTypeEnum.System)
                    {
                        allowAllSystem = true;
                    }
                }

                if (allowAllDefault && allowAllSystem)
                {
                    // If any of the applicable security rules for this type allow access to all instances, then automatically grant access to the whole set.
                    // Importantly: so long as the rule an exact type match, or a parent type, of the type we're currently trying to secure.
                    allowAll = true;                    
                    break;
                }                
            }

            return allowAll;
        }

        /// <summary>
        /// Determine what access rule CTEs actually get referenced, and mark them for rendering.
        /// </summary>
        /// <param name="typesOfResourcesToSecure">The type of resources that this table/join is rendering, and that need to be secured.</param>
        /// <param name="references">The references.</param>
        /// <returns></returns>
        static void DetermineIfCtesAreUsed( IEnumerable<EntityRef> typesOfResourcesToSecure, ReferenceManager references )
        {
            // Ideally this would happen as part of GetAccessRuleReportConditionSql
            // but we have some controlflow knots to untangle.
            // Namely GetAccessRuleReportConditionSql doesn't get called until sometime after CTEs are rendered.

            if ( references == null )
            {
                throw new ArgumentNullException( "references" );
            }

            foreach ( EntityRef type in typesOfResourcesToSecure )
            {
                List<AccessRuleCteTypeRegistration> accessRuleCtes = references.GetAccessRuleCtesForType( type.Id );

                // Check if any short-circuit before flagging all of them as being used                

                bool allowAllSystem;
                bool allowAllDefault;

                // This returns true if both allowAllDefault and allowAllSystem is true
                if (DoAccessRulesAllowAll(accessRuleCtes, out allowAllDefault, out allowAllSystem)) continue;

                foreach ( var accessRuleCteReg in accessRuleCtes )
                {
                    // If the rule is a system rule and the system rules allow all then skip the CTE generation
                    if (allowAllSystem &&
                        accessRuleCteReg.AccessRuleType == AccessRuleCteTypeEnum.System)
                    {
                        continue;                            
                    }

                    // If the rule is a default rule and the default rules allow all then skip the CTE generation
                    if (allowAllDefault &&
                        accessRuleCteReg.AccessRuleType == AccessRuleCteTypeEnum.Default)
                    {
                        continue;
                    }

                    // Generate CTE, either if we haven't done it already
                    // Or if we did it already, but suppressed the root type on the first go, and now we don't want it suppressed.
                    // (suppressing the root type check is a performance gain .. but in some cases we must do the check)                                                
                    if ( accessRuleCteReg.AccessRuleCte.SqlCte == null )
                    {
                        accessRuleCteReg.AccessRuleCte.PrepareCte( accessRuleCteReg.RuleIsApplicableToAllInstances );
                    }
                    else if ( accessRuleCteReg.AccessRuleCte.SqlCteSuppressedRootType && !accessRuleCteReg.RuleIsApplicableToAllInstances )
                    {
                        accessRuleCteReg.AccessRuleCte.PrepareCte( accessRuleCteReg.RuleIsApplicableToAllInstances );
                        EventLog.Application.WriteWarning( "Security CTE was rebuilt .. possible slight performance impact" );
                    }
                }
            }
        }


        /// <summary>
        /// Gets the access rule report condition SQL for the given access rule CTEs.
        /// </summary>
        /// <param name="typeOfResourcesToSecure">The type of resources that this table/join is rendering, and that need to be secured.</param>
        /// <param name="idColumn">The identifier column.</param>
        /// <param name="references">The references.</param>
        /// <returns></returns>
        internal static string GetAccessRuleReportConditionSql(long typeOfResourcesToSecure, string idColumn, ReferenceManager references)
        {
            if (string.IsNullOrEmpty(idColumn))
            {
                throw new ArgumentNullException("idColumn");
            }

            if (references == null)
            {
                throw new ArgumentNullException("references");
            }

            List<AccessRuleCteTypeRegistration> accessRuleCtes = references.GetAccessRuleCtesForType(typeOfResourcesToSecure);

			if ( accessRuleCtes.Count <= 0 )
            {
                return SecurityDenyPredicate;
            }

            bool allowAllSystem;
            bool allowAllDefault;

            // Check if any short-circuit before flagging all of them as being used
            if (DoAccessRulesAllowAll(accessRuleCtes, out allowAllDefault, out allowAllSystem))
            {
                return null;
            }            

            var implicitCtes = accessRuleCtes.Where(ar => ar.AccessRuleType == AccessRuleCteTypeEnum.Implicit).ToList();            
            var defaultCtes = accessRuleCtes.Where(ar => ar.AccessRuleType == AccessRuleCteTypeEnum.Default).ToList();            
            var systemCtes = accessRuleCtes.Where(ar => ar.AccessRuleType == AccessRuleCteTypeEnum.System).ToList();
            
            var conditionSql = new StringBuilder();                                   

            if (implicitCtes.Count > 0 && defaultCtes.Count == 0 && systemCtes.Count == 0)
            {
                // Have implicit rules only.
                // Implicit rules already check system rules so just render and return.
                conditionSql.Append(SecurityPredicateStart);                
                RenderAccessRuleReportConditions(conditionSql, implicitCtes, idColumn);
                conditionSql.Append(SecurityPredicateEnd);
                return conditionSql.ToString();                
            }            

            if (allowAllDefault || allowAllSystem)
            {
                // Either system OR default rules allow all. 
                // So we render the ones that do not allow all.                    
                var ctesToRender = new List<AccessRuleCteTypeRegistration>();                

                // Add any implicit rules if we have any                
                ctesToRender.AddRange(implicitCtes);
                // Add the access rules that do not allow all
                ctesToRender.AddRange(allowAllDefault ? systemCtes : defaultCtes);

                if (ctesToRender.Count == 0)
                {
                    return SecurityDenyPredicate;
                }

                // Render access rules
                conditionSql.Append(SecurityPredicateStart);                                                    
                RenderAccessRuleReportConditions(conditionSql, ctesToRender, idColumn);
                conditionSql.Append(SecurityPredicateEnd);
            }
            else
            {
                // System AND default rules do not allow all
                // Need to do an intersect
                if (defaultCtes.Count == 0 || systemCtes.Count == 0)
                {
                    // No intersection possible. Just render implicit rules.                    
                    if (implicitCtes.Count == 0)
                    {
                        return SecurityDenyPredicate;
                    }

                    conditionSql.Append(SecurityPredicateStart);
                    RenderAccessRuleReportConditions(conditionSql, implicitCtes, idColumn);
                    conditionSql.Append(SecurityPredicateEnd);                    
                }
                else
                {
                    // Render all implict rules first as these already check system rules   
                    if (implicitCtes.Count > 0)
                    {
                        conditionSql.Append("( ");

                        conditionSql.Append(SecurityPredicateStart);
                        RenderAccessRuleReportConditions(conditionSql, implicitCtes, idColumn);
                        conditionSql.Append(SecurityPredicateEnd);
                        
                        conditionSql.Append(" or ( ");                        
                    }
                    
                    conditionSql.Append(SecurityPredicateStart);                                        
                    RenderAccessRuleReportConditions(conditionSql, defaultCtes, idColumn);
                    conditionSql.Append(SecurityPredicateEnd);
                    
                    conditionSql.Append(" and ");

                    conditionSql.Append(SecurityPredicateStart);
                    RenderAccessRuleReportConditions(conditionSql, systemCtes, idColumn);
                    conditionSql.Append(SecurityPredicateEnd);

                    if (implicitCtes.Count > 0)
                    {
                        conditionSql.Append(SecurityPredicateEnd);
                        conditionSql.Append(SecurityPredicateEnd);
                    }                    
                }
            }            

            return conditionSql.ToString();
        }

        /// <summary>
        /// Redner access rule report conditions
        /// </summary>
        /// <param name="conditionSql"></param>
        /// <param name="ctes"></param>
        /// <param name="idColumn"></param>
        private static void RenderAccessRuleReportConditions(StringBuilder conditionSql, ICollection<AccessRuleCteTypeRegistration> ctes, string idColumn)
        {
            if (ctes.Count == 0)
            {
                return;
            }

            var first = new First();            
            
            foreach (var accessRuleCteReg in ctes)
            {
                AccessRuleCte accessRuleCte = accessRuleCteReg.AccessRuleCte;
                if (!first)
                {
                    conditionSql.Append(" union all ");
                }

                conditionSql.AppendFormat(SecurityPredicate, accessRuleCte.CteName, idColumn);
            }            
        }

        /// <summary>
        /// Gets the secured entities.
        /// </summary>
        /// <param name="entityIds">The entity ids.</param>
        /// <param name="userAccountId">The user account identifier.</param>
        /// <returns></returns>
        private ICollection<long> GetSecuredEntities(ICollection<long> entityIds, long userAccountId)
        {
            SetUser setUser = null;

            if (entityIds.Count <= 0)
            {
                return new List<long>();
            }

            if (userAccountId != 0)
            {
                var userAccount = Model.Entity.Get<UserAccount>(userAccountId);
                setUser = new SetUser(userAccount);
            }

            using (setUser)
            {
                // Ensure any security by pass is disabled
                using (new SecurityBypassContext(false))
                {
                    var refs = entityIds.Select(id => new EntityRef(id)).ToList();

                    // Check security
                    IDictionary<long, bool> accessControlCheckResult = Factory.EntityAccessControlService.Check(refs, new[] {Permissions.Read});

                    return new List<long>(entityIds.Where(id => accessControlCheckResult[id]));
                }
            }
        }
	}
}