// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Model;
using Model = EDC.ReadiNow.Model;
using Entity = EDC.ReadiNow.Metadata.Query.Structured.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using ResourceExpression = EDC.ReadiNow.Metadata.Query.Structured.ResourceExpression;
using ScriptExpression = EDC.ReadiNow.Metadata.Query.Structured.ScriptExpression;
using AggregateExpression = EDC.ReadiNow.Metadata.Query.Structured.AggregateExpression;
using StructureViewExpression = EDC.ReadiNow.Metadata.Query.Structured.StructureViewExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata;

namespace ReadiNow.QueryEngine.ReportConverter
{
    static class StructuredQueryEntityHelper
    {
        /// <summary>
        /// Converts a report into a structured query object.
        /// </summary>
        /// <param name="report">The report to convert</param>
        /// <param name="settings">Conversion settings.</param>
        /// <returns>The structured query.</returns>
        public static StructuredQuery ConvertReport( Report report, ReportToQueryConverterSettings settings )
        {
            if (report == null)
            {
                throw new ArgumentNullException();
            }
            if ( settings == null )
            {
                settings = ReportToQueryConverterSettings.Default;
            }

            Report reportToLoad = report;
            if ( report.RootNode == null )
            {
                throw new ArgumentException( string.Format( "Invalid report '{0}' to load the structured query", report.Id ) );
            }

            // Preload the entities
            if ( !settings.SuppressPreload )
            {
                ReportHelpers.PreloadQuery( reportToLoad.Id );
            }

            // Create context that contains information for conversion
            FromEntityContext fromEntityContext = new FromEntityContext { Report = reportToLoad, Settings = settings };

            // Convet the report entity
            StructuredQuery sq = BuildStructuredQuery(fromEntityContext, settings);
            
            // Attach the report to the structured query (why??)
            sq.Report = report;

            return sq;
        }

        /// <summary>
        /// Build the actual structured query object.
        /// </summary>
        private static StructuredQuery BuildStructuredQuery(FromEntityContext context, ReportToQueryConverterSettings settings)
        {
            Report report = context.Report;

            using ( Profiler.Measure( "Report {0} to StructuredQuery", report.Id ) )
            {
                StructuredQuery sq = new StructuredQuery(false);

                // Build up query starting with the root node
                using (Profiler.Measure("Query Tree"))
                {
                    sq.RootEntity = StructuredQueryEntityHelper.BuildReportNode(report.RootNode, context);
                }

                // then the list of select columns
                using (Profiler.Measure("Columns"))
                {
                    sq.SelectColumns = StructuredQueryEntityHelper.BuildSelectColumns(report.ReportColumns, context, settings);
                }

                // and then the list query conditions
                using (Profiler.Measure("Conditions"))
                {
                    sq.Conditions = StructuredQueryEntityHelper.BuildQueryConditions(report.HasConditions, context, settings);
                }

                // finally build up the list of order by items
                sq.OrderBy = StructuredQueryEntityHelper.BuildOrderBy(report.ReportOrderBys, context, settings);

                // Hook up the references
                StructuredQueryEntityHelper.ResolveReferences(sq, report, context);

                // Report back any errors
                sq.InvalidReportInformation = new Dictionary<string, Dictionary<long, string>>();
                sq.InvalidReportInformation["nodes"] = context.ReportInvalidNodes;
                sq.InvalidReportInformation["columns"] = context.ReportInvalidColumns;
                sq.InvalidReportInformation["conditions"] = context.ReportInvalidConditions;

                return sq;
            }
        }

        /// <summary>
        /// Builds the root entity.
        /// </summary>
        /// <param name="node">The root node.</param>
        /// <param name="context">The context.</param>
        /// <returns>Entity.</returns>
        /// <exception cref="System.Exception">Unknown report node type.</exception>
        internal static Entity BuildReportNode( ReportNode node, FromEntityContext context)
        {
            if ( node == null )
                throw new ArgumentNullException( nameof( node ) );
            if ( context == null )
                throw new ArgumentNullException( nameof( context ) );

            Entity structuredQueryEntity;
            if (node.Is<AggregateReportNode>())
            {
                structuredQueryEntity = BuildAggregateReportNode(node.As<AggregateReportNode>(), context);
            }
            else if (node.Is<RelationshipReportNode>())
            {
                structuredQueryEntity = BuildRelationshipReportNode(node.As<RelationshipReportNode>(), context);
            }
            else if ( node.Is<CustomJoinReportNode>( ) )
            {
                structuredQueryEntity = BuildCustomJoinReportNode( node.As<CustomJoinReportNode>( ), context );
            }
            else if (node.Is<DerivedTypeReportNode>())
            {
                structuredQueryEntity = BuildDerivedTypeReportNode(node.As<DerivedTypeReportNode>(), context);
            }
            else if (node.Is<ResourceReportNode>())
            {
                structuredQueryEntity = BuildResourceReportNode(node.As<ResourceReportNode>(), context);
            }
            else
            {                
                throw new Exception("Unknown report node type. " + node.GetType().Name);
            }
			if ( node.RelatedReportNodes != null && node.RelatedReportNodes.Count > 0 )
            {
                structuredQueryEntity.RelatedEntities = new List<Entity>(node.RelatedReportNodes.Select(relatedReportNode => BuildReportNode( relatedReportNode, context)).Where(entity => entity != null));
            }
            if (structuredQueryEntity != null)
            {
                // Only valid nodes get placed into the ReportNodeToEntityMap
                // (but ReportNodeMap may contain invalid nodes for historical reasons)
                context.ReportNodeToEntityMap[node.Id] = structuredQueryEntity;
            }           
            return structuredQueryEntity;
        }

        /// <summary>
        /// Builds the select columns.
        /// </summary>
        /// <param name="reportColumns">The report columns.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">Conversion settings.</param>
        /// <returns>List{SelectColumn}.</returns>
        internal static List<SelectColumn> BuildSelectColumns(IEntityCollection<ReportColumn> reportColumns, FromEntityContext context, ReportToQueryConverterSettings settings)
        {
            List<SelectColumn> columns = new List<SelectColumn>();
            // Order the columns based on position
            IEnumerable<ReportColumn> orderedReportColumns = reportColumns.OrderBy(rc => rc.ColumnDisplayOrder);
            foreach (ReportColumn orderedReportColumn in orderedReportColumns)
            {
                ScalarExpression scalarExpression = null;
                if (orderedReportColumn.ColumnExpression == null)
                {
                    context.ReportInvalidColumns[orderedReportColumn.Id] = orderedReportColumn.Name;
                    if (!settings.SchemaOnly)
                        continue;
                }
                else
                {
                    scalarExpression = BuildExpression(orderedReportColumn.ColumnExpression, context);
                    if (scalarExpression == null)
                    {
                        EventLog.Application.WriteWarning("the column expression id: {0} is invalid.", orderedReportColumn.ColumnExpression.Id.ToString());
                        context.ReportInvalidColumns[orderedReportColumn.Id] = orderedReportColumn.Name;
                        if (!settings.SchemaOnly)
                            continue;
                    }
                }

                Guid columnId = Guid.NewGuid();
                SelectColumn selectColumn = new SelectColumn
                {
                    IsHidden = orderedReportColumn.ColumnIsHidden ?? false,
                    DisplayName = orderedReportColumn.Name,
                    Expression = scalarExpression,
                    ColumnId = columnId,
                    EntityId = orderedReportColumn.Id
                };
                context.ReportColumnMap[orderedReportColumn.Id] = columnId;
                columns.Add(selectColumn);
            }
            return columns;
        }

        /// <summary>
        /// To check column is valid, 
        /// if field or relationship is removed from object. 
        /// </summary>
        /// <param name="expression">report column expression</param>
        /// <remarks>
        /// the report column expression must be exists, when field is removed from node object, 
        /// the column expression will be deleted, on this case, it is not valid column
        /// the the column expression is resourceexpression, check the relationship exists or not
        /// if the relationship is removed, the followRelationship property will be null, current
        /// column is invalid column
        /// </remarks>
        /// <returns></returns>
        internal static bool IsValidExpression(ReportExpression expression)
        {
            if (expression == null)
                return false;


            bool isValid = true;
                          
            //resourceExpression reportnode's followRelationship must not be null
            Model.ResourceExpression resourceExpression = expression.As<Model.ResourceExpression>();
            if (resourceExpression != null)
            {
                RelationshipReportNode relationshipReportNode =
                    resourceExpression.SourceNode.As<RelationshipReportNode>();
                if (relationshipReportNode != null)
                {
                    if (relationshipReportNode.FollowRelationship == null)
                    {
                        isValid = false;
                    }
                }               
            }

            //if aggregated grouped expression is resourceExpression reportnode's followRelationship must not be null
            Model.AggregateExpression aggregateExpression = expression.As<Model.AggregateExpression>();
            if (aggregateExpression != null)
            {
                if (!IsValidExpression(aggregateExpression.AggregatedExpression))
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Builds the query conditions.
        /// </summary>
        /// <param name="reportConditions">The report conditions.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">Conversion settings.</param>
        /// <returns>List{QueryCondition}.</returns>
        internal static List<QueryCondition> BuildQueryConditions(IEntityCollection<ReportCondition> reportConditions, FromEntityContext context, ReportToQueryConverterSettings settings)
        {
            List<QueryCondition> queryConditions = new List<QueryCondition>();
            foreach (ReportCondition reportCondition in reportConditions)
            {
                // TODO Sort out analyser field crap (i.e. hidden/display name etc)?? Do we need this??
                ConditionType conditionType = ConditionType.Unspecified;
                if (reportCondition.Operator != null)
                {
                    string[] conditionOperatorParts = reportCondition.Operator.Alias.Split(':');
                    string conditionOperator = conditionOperatorParts.Length == 2 ? conditionOperatorParts[1].Substring(4) : conditionOperatorParts[0].Substring(4);
                    conditionType = (ConditionType)Enum.Parse(typeof(ConditionType), conditionOperator, true);
                }

                ScalarExpression scalarExpression = null;

                if (reportCondition.ConditionExpression == null)
                {
                    context.ReportInvalidConditions[reportCondition.Id] = reportCondition.Name;
                    if (!settings.SchemaOnly)
                        continue;
                }
                else
                {
                    scalarExpression = BuildExpression(reportCondition.ConditionExpression, context);
                    if (scalarExpression == null)
                    {

                        EventLog.Application.WriteWarning("the condition expression id: {0} is invalid.", reportCondition.ConditionExpression.Id.ToString());
                        if (!settings.SchemaOnly)
                            continue; // TODO: log warning .. should we just abort the query if it refers to an invalid expression in a condition .. because it means we'll get more rows.
                    }
                }

                QueryCondition queryCondition = new QueryCondition
                    {
                        EntityId = reportCondition.Id,
                        Operator = conditionType,
                        Expression = scalarExpression
                    };
                BuildConditionParameter(reportCondition, queryCondition, context);
                queryConditions.Add(queryCondition);
            }
            return queryConditions;
        }

        /// <summary>
        /// Builds the order by collection.
        /// </summary>
        /// <param name="reportOrderBys">The report order bys.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">Conversion settings.</param>
        /// <returns>List{OrderByItem}.</returns>
        internal static List<OrderByItem> BuildOrderBy(IEntityCollection<ReportOrderBy> reportOrderBys, FromEntityContext context, ReportToQueryConverterSettings settings)
        {
            List<OrderByItem> orderByItems = new List<OrderByItem>();

            if ( context.Settings.ConditionsOnly )
            {
                return orderByItems; // don't bother converting them (e.g. for use in security)
            }

            foreach (ReportOrderBy reportOrderBy in reportOrderBys.OrderBy(rob => rob.OrderPriority))
            {
                ScalarExpression expression = BuildExpression(reportOrderBy.OrderByExpression, context);
                if (expression == null)
                {
                    EventLog.Application.WriteWarning("the order expression id: {0} is invalid.", reportOrderBy.OrderByExpression != null ? reportOrderBy.OrderByExpression.Id.ToString() : "null");
                    continue;
                }

                //the orderby expression is ColumnReferenceExpression, the referenced report column must exists
                if (context.ColumnReferenceMap.ContainsKey(expression.ExpressionId) &&
                    context.ReportColumnMap.ContainsKey(context.ColumnReferenceMap[expression.ExpressionId]))
                {
                    OrderByItem orderByItem = new OrderByItem
                        {
                            Direction =
                                reportOrderBy.ReverseOrder ?? false
                                    ? OrderByDirection.Descending
                                    : OrderByDirection.Ascending,
                            Expression = expression
                        };

                    orderByItems.Add(orderByItem);
                }
            }

            return orderByItems;
        }

        /// <summary>
        /// Resolves the references to hook up various node and column identifiers from the entity model context.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="report">The report.</param>
        /// <param name="context">The context.</param>
        internal static void ResolveReferences(StructuredQuery query, Report report, FromEntityContext context)
        {
            if (query.RootEntity is AggregateEntity)
            {
                AggregateEntity ae = query.RootEntity as AggregateEntity;
                foreach (ResourceDataColumn rdc in ae.GroupBy.OfType<ResourceDataColumn>().Select(expression => expression as ResourceDataColumn))
                {
                    rdc.NodeId = context.ReportNodeMap[rdc.SourceNodeEntityId];
                }
            }
            // Resolve the Node Expressions for report columns
            foreach (ReportColumn reportColumn in report.ReportColumns)
            {
                AssignColumnNodeGuidForExpression(query.SelectColumns, reportColumn, context);
            }

            // Resolve the Node expressions for the analyser
            foreach (QueryCondition queryCondition in query.Conditions)
            {
                ResolveExpressionToNode(StructuredQueryHelper.WalkExpressions(queryCondition.Expression), context);

                ColumnReference columnReference = queryCondition.Expression as ColumnReference;
                //if the expression is ColumnReferenceExpression, the referenced report column must exists
                if (columnReference != null && context.ColumnReferenceMap.ContainsKey(columnReference.ExpressionId) && context.ReportColumnMap.ContainsKey(context.ColumnReferenceMap[columnReference.ExpressionId]))
                {
                    columnReference.ColumnId = context.ReportColumnMap[context.ColumnReferenceMap[columnReference.ExpressionId]];
                }
            }

            // Resolve the order by expressions (NOTE: Am assuming that order by can only contain column references here)
            foreach (ColumnReference columnReference in query.OrderBy.Select(orderByItem => orderByItem.Expression).OfType<ColumnReference>())
            {
                //the orderby expression is ColumnReferenceExpression, the referenced report column must exists
                if (context.ColumnReferenceMap.ContainsKey(columnReference.ExpressionId) && 
                    context.ReportColumnMap.ContainsKey(context.ColumnReferenceMap[columnReference.ExpressionId]))
                {
                    columnReference.ColumnId =
                        context.ReportColumnMap[context.ColumnReferenceMap[columnReference.ExpressionId]];
                }
            }
        }

        /// <summary>
        /// Assigns the column node unique identifier for expression.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <param name="reportColumn">The report column.</param>
        /// <param name="context">The context.</param>
        static private void AssignColumnNodeGuidForExpression(IEnumerable<SelectColumn> columns, ReportColumn reportColumn, FromEntityContext context)
        {
            // Get the GUID of the report column
            Guid reportColumnGuid;
            if (context.ReportColumnMap.ContainsKey(reportColumn.Id))
            {
                reportColumnGuid = context.ReportColumnMap[reportColumn.Id];
            }
            else
            {
                return;
            }

            // Look up the select column using the GUID
            SelectColumn selectColumn = columns.FirstOrDefault(c => c.ColumnId == reportColumnGuid);
            if (selectColumn == null)
            {
                return;
            }
            // Handle column reference expressions
            if (selectColumn.Expression != null)
            {
                ColumnReferenceExpression columnReferenceExpression =
                    reportColumn.ColumnExpression.As<ColumnReferenceExpression>();
                if (columnReferenceExpression != null)
                {
                    long rootNodeId = columnReferenceExpression.ExpressionReferencesColumn.Id;
                    // Get the GUID for the referenced column rather than root node and populate
                    ColumnReference columnReference = selectColumn.Expression as ColumnReference;
                    if (columnReference != null)
                    {
                        columnReference.ColumnId = context.ReportColumnMap[rootNodeId];
                    }
                }
                else
                {
                    ResolveExpressionToNode(StructuredQueryHelper.WalkExpressions(selectColumn.Expression), context);
                }
            }
        }

        static private void ResolveExpressionToNode(IEnumerable<ScalarExpression> scalarExpressions, FromEntityContext context)
        {
            foreach (ScalarExpression expression in scalarExpressions)
            {
                EntityExpression entityExpression = expression as EntityExpression;
                if (entityExpression != null)
                {
                    if (!context.ReportExpressionMap.ContainsKey(expression.ExpressionId))
                    {
                        throw new ArgumentException(string.Format("Expression '{0}' for entity ID '{1}' not found in report expression map", 
                                expression.ExpressionId, expression.EntityId), 
                            "scalarExpressions");
                    }
                    if (!context.ReportNodeMap.ContainsKey(context.ReportExpressionMap[expression.ExpressionId]))
                    {
                        throw new ArgumentException(string.Format("Node '{0}' not found in report node map for expression '{1}' for entity ID '{2}'. This may be caused by duplicate GUIDs in legacy report XML.",
                                context.ReportExpressionMap[expression.ExpressionId], expression.ExpressionId, expression.EntityId),
                            "scalarExpressions");
                    }

                    entityExpression.NodeId = context.ReportNodeMap[context.ReportExpressionMap[expression.ExpressionId]];
                }
            }
        }

        // From ConvertAggregateNode
        private static AggregateEntity BuildAggregateReportNode(AggregateReportNode reportNode, FromEntityContext context)
        {
            if ( reportNode.GroupedNode == null )
                throw new Exception( "Aggregate note has no grouped node." );

            // Populate Grouped Node
            AggregateEntity aggregateEntity = new AggregateEntity {GroupedEntity = BuildReportNode( reportNode.GroupedNode, context)};

            // Populate Grouped by if present
			if ( reportNode.GroupedBy != null && reportNode.GroupedBy.Count > 0 )
            {
                aggregateEntity.GroupBy = new List<ScalarExpression>(reportNode.GroupedBy.Count);
                foreach (ReportExpression reportExpression in reportNode.GroupedBy)
                {
                    var groupByExpr = BuildExpression(reportExpression, context);
                    if (groupByExpr == null)
                        continue;   // TODO : log warning
                    aggregateEntity.GroupBy.Add(groupByExpr);
                }
            }
            Guid nodeId;
            if (!context.ReportNodeMap.TryGetValue(reportNode.Id, out nodeId))
            {
                nodeId = Guid.NewGuid();
                context.ReportNodeMap[reportNode.Id] = nodeId;
            }
            aggregateEntity.NodeId = nodeId;
            aggregateEntity.EntityId = reportNode.Id;
            return aggregateEntity;
        }

        /// <summary>
        /// Builds the relationship report node.
        /// </summary>
        /// <param name="reportNode">The report node.</param>
        /// <param name="context">The context.</param>
        /// <returns>RelatedResource.</returns>
        private static RelatedResource BuildRelationshipReportNode(RelationshipReportNode reportNode, FromEntityContext context)
        {
            RelatedResource relatedResource = new RelatedResource
                {
                    ResourceMustExist = reportNode.TargetMustExist ?? false,
                    ResourceNeedNotExist = reportNode.TargetNeedNotExist ?? false,
                    ParentNeedNotExist = reportNode.ParentNeedNotExist ?? false,
                    ExactType = reportNode.ExactType ?? false
                };
            if (reportNode.ResourceReportNodeType != null)
            {
                relatedResource.EntityTypeId = reportNode.ResourceReportNodeType.Id;
            }

            if (reportNode.FollowInReverse ?? false)
            {
                relatedResource.RelationshipDirection = RelationshipDirection.Reverse;
            }
            else
            {
                relatedResource.RelationshipDirection = RelationshipDirection.Forward;
            }

            if (reportNode.FollowRecursive == true)
            {
                if (reportNode.IncludeSelfInRecursive == true)
                {
                    relatedResource.Recursive = RecursionMode.RecursiveWithSelf;
                }
                else
                {
                    relatedResource.Recursive = RecursionMode.Recursive;                    
                }
            }
            else
            {
                relatedResource.Recursive = RecursionMode.None;
            }


            Guid nodeId;
            if (!context.ReportNodeMap.TryGetValue(reportNode.Id, out nodeId))
            {
                nodeId = Guid.NewGuid();
                context.ReportNodeMap[reportNode.Id] = nodeId;
            }

            relatedResource.NodeId = nodeId;
            relatedResource.EntityId = reportNode.Id;
            if (reportNode.FollowRelationship != null)
            {
                relatedResource.RelationshipTypeId = reportNode.FollowRelationship.Id;
            }
            else
            {
                context.ReportInvalidNodes [ reportNode.Id ] = reportNode.ResourceReportNodeType != null ? Model.Entity.GetName(reportNode.ResourceReportNodeType.Id) : "";
                EventLog.Application.WriteWarning(context.DebugInfo + "reportNode.FollowRelationship was null");
                return null;
            }
            return relatedResource;
        }

        /// <summary>
        /// Builds the derived type report node.
        /// </summary>
        /// <param name="reportNode">The report node.</param>
        /// <param name="context">The context.</param>
        /// <returns>DownCastResource.</returns>
        private static DownCastResource BuildDerivedTypeReportNode(DerivedTypeReportNode reportNode, FromEntityContext context)
        {
            DownCastResource downCastResource = new DownCastResource
                {
                    MustExist = reportNode.TargetMustExist ?? false, 
                    ExactType = reportNode.ExactType ?? false
                };
            if (reportNode.ResourceReportNodeType != null)
            {
                downCastResource.EntityTypeId = reportNode.ResourceReportNodeType.Id;
            }
            Guid nodeId;
            if (!context.ReportNodeMap.TryGetValue(reportNode.Id, out nodeId))
            {
                nodeId = Guid.NewGuid();
                context.ReportNodeMap[reportNode.Id] = nodeId;
            }
            downCastResource.NodeId = nodeId;
            downCastResource.EntityId = reportNode.Id;
            return downCastResource;
        }

        /// <summary>
        /// Builds the custom join report node.
        /// </summary>
        /// <param name="reportNode">The report node.</param>
        /// <param name="context">The context.</param>
        /// <returns>DownCastResource.</returns>
        private static CustomJoinNode BuildCustomJoinReportNode( CustomJoinReportNode reportNode, FromEntityContext context )
        {
            CustomJoinNode customJoinNode = new CustomJoinNode
            {
                JoinPredicateScript = reportNode.JoinPredicateCalculation,
                EntityTypeId = reportNode.ResourceReportNodeType?.Id,
                ResourceMustExist = reportNode.TargetMustExist ?? false,
                ResourceNeedNotExist = reportNode.TargetNeedNotExist ?? false,
                ParentNeedNotExist = reportNode.ParentNeedNotExist ?? false,
                ExactType = reportNode.ExactType ?? false
            };
            Guid nodeId;
            if ( !context.ReportNodeMap.TryGetValue( reportNode.Id, out nodeId ) )
            {
                nodeId = Guid.NewGuid( );
                context.ReportNodeMap[ reportNode.Id ] = nodeId;
            }
            customJoinNode.NodeId = nodeId;
            customJoinNode.EntityId = reportNode.Id;
            return customJoinNode;
        }

        /// <summary>
        /// Builds the resource report node.
        /// </summary>
        /// <param name="reportNode">The report node.</param>
        /// <param name="context">The context.</param>
        /// <returns>ResourceEntity.</returns>
        private static ResourceEntity BuildResourceReportNode(ResourceReportNode reportNode, FromEntityContext context)
        {
            ResourceEntity resourceEntity = new ResourceEntity
            {
                ExactType = reportNode.ExactType ?? false
            };
            if (reportNode.ResourceReportNodeType != null)
            {
                resourceEntity.EntityTypeId = reportNode.ResourceReportNodeType.Id;
            }
            Guid nodeId;
            if (!context.ReportNodeMap.TryGetValue(reportNode.Id, out nodeId))
            {
                nodeId = Guid.NewGuid();
                context.ReportNodeMap[reportNode.Id] = nodeId;
            }
            resourceEntity.NodeId = nodeId;
            resourceEntity.EntityId = reportNode.Id;
            return resourceEntity;
        }

        /// <summary>
        /// Builds the condition parameter.
        /// </summary>
        /// <param name="reportCondition">The report condition.</param>
        /// <param name="queryCondition">The query condition.</param>
        internal static void BuildConditionParameter(ReportCondition reportCondition, QueryCondition queryCondition, FromEntityContext context)
        {
            ActivityArgument activityArgument;

            // Get the column expression for the analysable field. If this is a referenced column then use it's referenced column's expression.
            ReportExpression reportExpression = reportCondition != null ? reportCondition.ConditionExpression : null;

            if (reportExpression != null && reportExpression.Is<ColumnReferenceExpression>())
            {
                reportExpression =
                    reportCondition.ConditionExpression.As<ColumnReferenceExpression>()
                        .ExpressionReferencesColumn.ColumnExpression;
            }
            

            if (reportCondition.ConditionParameter != null && reportCondition.ConditionParameter.ParamTypeAndDefault != null)
            {
                activityArgument = reportCondition.ConditionParameter.ParamTypeAndDefault;
            }
            else if (reportExpression != null && reportExpression.ReportExpressionResultType != null)
            {
                activityArgument = reportCondition.ConditionExpression.ReportExpressionResultType;
            }
            else
            {
                EventLog.Application.WriteWarning(context.DebugInfo + "condition parameter could not be processed");
                return;
            }
            List<TypedValue> typedValues = TypedValueHelper.TypedValueFromEntity(activityArgument);
            foreach (TypedValue typedValue in typedValues)
            {
                queryCondition.Arguments.Add(typedValue);
            }
        }
        
        /// <summary>
        /// Builds the expression.
        /// </summary>
        /// <param name="reportExpression">The report expression.</param>
        /// <param name="context">The context.</param>
        /// <returns>ScalarExpression.</returns>
        internal static ScalarExpression BuildExpression(ReportExpression reportExpression, FromEntityContext context)
        {

            if (reportExpression == null)
            {
                EventLog.Application.WriteWarning(context.DebugInfo + "reportExpression was null");
                return null;
            }

            ScalarExpression scalarExpression = null;
            
            if (context.InstanceExpressionMap.TryGetValue(reportExpression.Id, out scalarExpression))
            {
                return scalarExpression;
            }
            if (reportExpression.Is<Model.IdExpression>())
            {
                scalarExpression = BuildIdExpression(reportExpression.As<Model.IdExpression>());
            }
            else if (reportExpression.Is<Model.ResourceExpression>())
            {
                scalarExpression = BuildResourceExpression(reportExpression.As<Model.ResourceExpression>(), context);
            }
            else if (reportExpression.Is<Model.FieldExpression>())
            {
                scalarExpression = BuildFieldExpression(reportExpression.As<Model.FieldExpression>(), context);
            }
            else if (reportExpression.Is<Model.ScriptExpression>())
            {
                scalarExpression = BuildScriptExpression(reportExpression.As<Model.ScriptExpression>());
            }
            else if (reportExpression.Is<Model.AggregateExpression>())
            {
                return BuildAggregateExpression(reportExpression.As<Model.AggregateExpression>(), context);
            }
            else if (reportExpression.Is<Model.ColumnReferenceExpression>())
            {
                return BuildColumnReferenceExpression(reportExpression.As<ColumnReferenceExpression>(), context);
            }
            else if (reportExpression.Is<Model.StructureViewExpression>())
            {
                return BuildStructureViewExpression(reportExpression.As<Model.StructureViewExpression>(), context);
            }
            if (scalarExpression != null)
            {
                // Update the reference table for expressions of interest ignoring anything that is a column expression
                if (reportExpression.Is<NodeExpression>())
                {
                    NodeExpression nodeExpression = reportExpression.As<NodeExpression>();
                    long nodeId = nodeExpression.SourceNode.Id;

                    // Check that we have a valid node for it
                    if (context.ReportNodeToEntityMap.ContainsKey(nodeId))
                    {
                        Guid key = Guid.NewGuid();
                        context.ReportExpressionMap[key] = nodeId;
                        scalarExpression.ExpressionId = key;
                        scalarExpression.EntityId = nodeExpression.Id;
                    }
                    else
                    {
                        EventLog.Application.WriteWarning(context.DebugInfo + "expression node could not be found"); 
                        scalarExpression = null;
                    }
                }
            }
            if (scalarExpression != null)
            {
                context.InstanceExpressionMap[reportExpression.Id] = scalarExpression;
            }
            ApplyExpressionClustering(scalarExpression, reportExpression, context);
            return scalarExpression;
        }

        private static IdExpression BuildIdExpression(Model.IdExpression idExpression)
        {
            // MPK - This does not seem to be populated _or_ for that matter needed, should this be removed????
            return new IdExpression();
        }

        /// <summary>
        /// Builds the resource expression.
        /// </summary>
        /// <param name="resourceExpression">The resource expression.</param>
        /// <returns>ResourceExpression.</returns>
        private static ResourceExpression BuildResourceExpression(Model.ResourceExpression resourceExpression, FromEntityContext context)
        {
            //resourceExpression entity node's followRelationship must not be null           
            RelationshipReportNode relationshipReportNode =
                resourceExpression.SourceNode.As<RelationshipReportNode>();
            if (relationshipReportNode != null)
            {
                if (relationshipReportNode.FollowRelationship == null)
                {
                    EventLog.Application.WriteWarning(context.DebugInfo + "relationshipReportNode.FollowRelationship was null");
                    return null;
                }
            }

            //resourceExpression sourceNode is null means that the source entity is removed
            if (resourceExpression.SourceNode == null)
            {
                EventLog.Application.WriteWarning(context.DebugInfo + "resourceExpression.SourceNode was null");
                return null;
            }

            long targetTypeId = 0;
            TypedArgument ta = resourceExpression.ReportExpressionResultType.As<TypedArgument>();
            if (ta != null && ta.ConformsToType != null)
                targetTypeId = ta.ConformsToType.Id;

            DatabaseType castType = null;
            if (ta != null && ta.Name != null)
            {
                if (ta.Name == "ChoiceRelationship")
                {
                    castType = new ChoiceRelationshipType();
                }
                else
                {
                    castType = new InlineRelationshipType();
                }
            }


            return new ResourceExpression { FieldId = new EntityRef("core:name"), SourceNodeEntityId = resourceExpression.SourceNode.Id, TargetTypeId = targetTypeId, CastType = castType };
        }

        /// <summary>
        /// Builds the field expression.
        /// </summary>
        /// <param name="fieldExpression">The field expression.</param>
        /// <param name="context"></param>
        /// <returns>ResourceDataColumn.</returns>
        private static ResourceDataColumn BuildFieldExpression(Model.FieldExpression fieldExpression, FromEntityContext context)
        {
            DatabaseType castType = TypedValueHelper.GetDatabaseType(fieldExpression.ReportExpressionResultType);

            if (fieldExpression.FieldExpressionField == null)
            {
                EventLog.Application.WriteWarning(context.DebugInfo + "field was null");
                return null;
            }

            ResourceDataColumn rdc = new ResourceDataColumn
                {
                    FieldId = fieldExpression.FieldExpressionField, 
                    CastType = castType, ExpressionId = Guid.NewGuid(), 
                    SourceNodeEntityId = fieldExpression.As<NodeExpression>().SourceNode.Id
                };
            return rdc;
        }

        /// <summary>
        /// Builds the script expression.
        /// </summary>
        /// <param name="scriptExpression">The script expression.</param>
        /// <returns>ScriptExpression.</returns>
        private static ScriptExpression BuildScriptExpression(Model.ScriptExpression scriptExpression)
        {
            DatabaseType castType = TypedValueHelper.GetDatabaseType(scriptExpression.ReportExpressionResultType);

            return new ScriptExpression { Script = scriptExpression.ReportScript, ResultType = castType };
        }

        /// <summary>
        /// Builds the script expression.
        /// </summary>
        /// <param name="columnReferenceExpression">The column reference expression.</param>
        /// <param name="context">The context.</param>
        /// <returns>ScriptExpression.</returns>
        private static ColumnReference BuildColumnReferenceExpression(Model.ColumnReferenceExpression columnReferenceExpression, FromEntityContext context)
        {
            Guid columnReferenceExpressionId = Guid.NewGuid();
            if (columnReferenceExpression.ExpressionReferencesColumn == null)
                return null;
            context.ColumnReferenceMap[columnReferenceExpressionId] = columnReferenceExpression.ExpressionReferencesColumn.Id;
            return new ColumnReference {ExpressionId = columnReferenceExpressionId, EntityId = columnReferenceExpression.Id};
        }

        /// <summary>
        /// Builds the structure view expression.
        /// </summary>
        /// <param name="structureViewExpression">The column reference expression.</param>
        /// <param name="context">The context.</param>
        /// <returns>structureViewExpression.</returns>
        private static StructureViewExpression BuildStructureViewExpression(Model.StructureViewExpression structureViewExpression, FromEntityContext context)
        {
            if (structureViewExpression.StructureViewExpressionStructureView == null)
            {
                EventLog.Application.WriteWarning(context.DebugInfo + "structure view expression structure view was null");
                return null;
            }

            if (structureViewExpression.StructureViewExpressionSourceNode == null)
            {
                EventLog.Application.WriteWarning(context.DebugInfo + "structure view expression source node was null");
                return null;
            }

            Guid nodeId;
            if (!context.ReportNodeMap.TryGetValue(structureViewExpression.StructureViewExpressionSourceNode.Id, out nodeId))
            {
                EventLog.Application.WriteWarning(context.DebugInfo + "structure view expression source node id not found");
                return null;
            }

            return new StructureViewExpression { NodeId = nodeId, StructureViewId = structureViewExpression.StructureViewExpressionStructureView.Id, ExpressionId = Guid.NewGuid()};
        }

        /// <summary>
        /// Builds the aggregate expression.
        /// </summary>
        /// <param name="aggregateExpression">The aggregate expression.</param>
        /// <param name="context">The context.</param>
        /// <returns>AggregateExpression.</returns>
        private static AggregateExpression BuildAggregateExpression(Model.AggregateExpression aggregateExpression, FromEntityContext context)
        {
            string[] aggregateExpressionParts = aggregateExpression.AggregateMethod.Alias.Split(':');
            string method = aggregateExpressionParts.Length == 2 ? aggregateExpressionParts[1].Substring(3) : aggregateExpressionParts[0].Substring(3);
//            string method = aggregateExpression.AggregateMethod.Alias.Substring(3);
            AggregateMethod aggregateMethod = (AggregateMethod)Enum.Parse(typeof (AggregateMethod), method, true);
            AggregateExpression expression = new AggregateExpression 
            {
                AggregateMethod = aggregateMethod,
                Expression = aggregateExpression.AggregatedExpression != null ? BuildExpression(aggregateExpression.AggregatedExpression, context) : null
            };

            // Validate expression
            if (expression.AggregateMethod != AggregateMethod.Count && expression.Expression == null)
            {
                EventLog.Application.WriteWarning(context.DebugInfo + "the aggregate id: {0} has invalid grouped expression.", aggregateExpression.Id.ToString());
                return null;    // TODO: Log a warning
            }

            Guid key = Guid.NewGuid();
            context.ReportExpressionMap[key] = aggregateExpression.SourceNode.Id;
            expression.ExpressionId = key;
            expression.EntityId = aggregateExpression.Id;
            return expression;
        }

        /// <summary>
        /// Detect expressions that are being clustered (grouped by month, etc) and convert them to clustering expressions.
        /// </summary>
        /// <param name="scalarExpression"></param>
        /// <param name="reportExpression"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static void ApplyExpressionClustering(ScalarExpression scalarExpression, ReportExpression reportExpression, FromEntityContext context)
        {
            if (reportExpression == null || scalarExpression == null)
                return;

            var reportColumn = reportExpression.ExpressionForColumn;
            if (reportColumn == null)
                return;

            var displayFormat = reportColumn.ColumnDisplayFormat;
            if (displayFormat == null)
                return;

            ClusterOperation oper = ClusterOperation.None;

            if (displayFormat.DateColumnFormat_Enum != null)
            {
                switch (displayFormat.DateColumnFormat_Enum)
                {
                    case DateColFmtEnum_Enumeration.DateDayMonth: oper = ClusterOperation.Day | ClusterOperation.Month; break;
                    case DateColFmtEnum_Enumeration.DateMonth: oper = ClusterOperation.Month; break;
                    case DateColFmtEnum_Enumeration.DateMonthYear: oper = ClusterOperation.Month | ClusterOperation.Year; break;
                    case DateColFmtEnum_Enumeration.DateQuarter: oper = ClusterOperation.Quarter; break;
                    case DateColFmtEnum_Enumeration.DateQuarterYear: oper = ClusterOperation.Quarter | ClusterOperation.Year; break;
                    case DateColFmtEnum_Enumeration.DateYear: oper = ClusterOperation.Year; break;
                    case DateColFmtEnum_Enumeration.DateWeekday: oper = ClusterOperation.Weekday; break;
                    case DateColFmtEnum_Enumeration.DateLong:
                    case DateColFmtEnum_Enumeration.DateShort:
                    default:
                        oper = ClusterOperation.None; break;
                }
            }
            if (displayFormat.TimeColumnFormat_Enum != null)
            {
                switch (displayFormat.TimeColumnFormat_Enum)
                {
                    case TimeColFmtEnum_Enumeration.TimeHour: oper = ClusterOperation.Hour; break;
                    default:
                        oper = ClusterOperation.None; break;
                }
            }
            if (displayFormat.DateTimeColumnFormat_Enum != null)
            {
                switch (displayFormat.DateTimeColumnFormat_Enum)
                {
                    case DateTimeColFmtEnum_Enumeration.DateTimeDayMonth: oper = ClusterOperation.Day | ClusterOperation.Month; break;
                    case DateTimeColFmtEnum_Enumeration.DateTimeMonth: oper = ClusterOperation.Month; break;
                    case DateTimeColFmtEnum_Enumeration.DateTimeMonthYear: oper = ClusterOperation.Month | ClusterOperation.Year; break;
                    case DateTimeColFmtEnum_Enumeration.DateTimeQuarter: oper = ClusterOperation.Quarter; break;
                    case DateTimeColFmtEnum_Enumeration.DateTimeQuarterYear: oper = ClusterOperation.Quarter | ClusterOperation.Year; break;
                    case DateTimeColFmtEnum_Enumeration.DateTimeYear: oper = ClusterOperation.Year; break;
                    case DateTimeColFmtEnum_Enumeration.DateTimeWeekday: oper = ClusterOperation.Weekday; break;
                    case DateTimeColFmtEnum_Enumeration.DateTimeDate: oper = ClusterOperation.Year | ClusterOperation.Month | ClusterOperation.Day; break;
                    case DateTimeColFmtEnum_Enumeration.DateTimeTime: oper = ClusterOperation.Hour | ClusterOperation.Minute | ClusterOperation.Second; break;
                    case DateTimeColFmtEnum_Enumeration.DateTimeHour: oper = ClusterOperation.Hour; break;
                    case DateTimeColFmtEnum_Enumeration.DateTime24Hour:
                    case DateTimeColFmtEnum_Enumeration.DateTimeDayMonthTime:
                    case DateTimeColFmtEnum_Enumeration.DateTimeLong:
                    case DateTimeColFmtEnum_Enumeration.DateTimeShort:
                    case DateTimeColFmtEnum_Enumeration.DateTimeSortable:
                    default:
                        oper = ClusterOperation.None; break;
                }
            }

            scalarExpression.ClusterOperation = oper;
        }
    }
}
