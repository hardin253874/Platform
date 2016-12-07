// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Media;
using EDC.ReadiNow.Metadata.Query.Structured.Builder;
using EDC.ReadiNow.Metadata.Reporting;
using EDC.ReadiNow.Metadata.Reporting.Formatting;
using EDC.ReadiNow.Model;

using FormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.FormattingRule;
using BarFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.BarFormattingRule;
using ColorFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.ColorFormattingRule;
using IconFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.IconFormattingRule;
using ImageFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.ImageFormattingRule;
using ColorRule = EDC.ReadiNow.Metadata.Reporting.Formatting.ColorRule;
using IconRule = EDC.ReadiNow.Metadata.Reporting.Formatting.IconRule;
using EDC.ReadiNow.Core;

// ReSharper disable RedundantNameQualifier
// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable UnusedParameter.Local

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Converts from a ReportInfo (StructuredQuery, GridDataView) to a report entity.
    /// </summary>
    public static class ReportToEntityModelHelper
    {
        public static Report ConvertToEntity( StructuredQuery structuredQuery, ToEntitySettings settings = null)
        {
            Report report = new Model.Report( );

            var reportInfo = new ReportInfo();

            // Read structure view XML
            reportInfo.StructuredQuery = structuredQuery;

            // Read analyzer XML
            reportInfo.Analyzer = new AnalyzerInfo();

            // Prep settings
            if (settings == null)
            {
                settings = new ToEntitySettings();
                settings.PurgeExisting = true;
            }
            settings.ReportEntity = report;

            // Run conversion
            ConvertToEntity(reportInfo, settings);

            // Set report definition
            report.ReportUsesDefinition = report.RootNode?.As<ResourceReportNode>( )?.ResourceReportNodeType;

            return report;
        }

        /// <summary>
        /// Accepts a ReportInfo (containg a StructuredQuery, GridDataView, and analyzer details)
        /// and uses this information to fill a reportEntity entity structure.
        /// Main entry point.
        /// </summary>
        /// <param name="reportInfo"></param>
        /// <param name="settings"></param>
        public static void ConvertToEntity(ReportInfo reportInfo, ToEntitySettings settings)
        {
            var context = new ToEntityContext
                {
                    Settings = settings,
                    ReportInfo = reportInfo,
                    ReportEntity = settings.ReportEntity.AsWritable<Model.Report>()
                };

            ApplyHacks(context);

            // Politely ask the query engine to resolve expression types for us
            var queryBuilderSettings = new QuerySettings { CaptureExpressionMetadata = true, SecureQuery = false, SupportPaging = false, Hint = "ReportToEntityModel get metadata" };
            try
            {
                context.QueryResult = Factory.QuerySqlBuilder.BuildSql( reportInfo.StructuredQuery, queryBuilderSettings );
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError("Converting report {0} ({1}) failed: {2}",
                    settings.ReportEntity.Id, settings.ReportEntity.Alias ?? settings.ReportEntity.Name ?? "(No alias or name)", ex.Message);
                return;
            }

            // Remove existing content
            if (settings.PurgeExisting)
            {
                PurgeExisting(context);
            }
            
            // Create new report entities
            ConvertReport(context);

            // Clean up old entities
            if (context.ToDelete != null && context.ToDelete.Count > 0)
            {
                Model.Entity.Delete(context.ToDelete);
            }
        }


        /// <summary>
        /// Apply various transforms to fix up structured queries prior to processing.
        /// </summary>
        /// <param name="context"></param>
        private static void ApplyHacks(ToEntityContext context)
        {
            StructuredQuery structuredQuery = context.ReportInfo.StructuredQuery;

            foreach (Structured.SelectColumn selectColumn in structuredQuery.SelectColumns)
            {
                selectColumn.Expression = ApplyExpressionHacks(selectColumn.Expression, context);
            }
            foreach (Structured.QueryCondition condition in structuredQuery.Conditions)
            {
                var oldExpr = condition.Expression;
                condition.Expression = ApplyExpressionHacks(condition.Expression, context);
                foreach (var af in context.ReportInfo.Analyzer.AnalyzerFields)
                {
                    if (af.QueryExpressionId == oldExpr.ExpressionId)
                        af.QueryExpressionId = condition.Expression.ExpressionId;
                }
            }
        }


        /// <summary>
        /// Apply various transforms to fix up structured queries prior to processing.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="context"></param>
        private static ScalarExpression ApplyExpressionHacks(ScalarExpression expr, ToEntityContext context)
        {
            var rdc = expr as ResourceDataColumn;
            if (rdc != null)
            {
                DatabaseType dt = TypeForRelatedEntities(context.ReportInfo.StructuredQuery.RootEntity.RelatedEntities, rdc.NodeId);
                // Only play with the column type _if_ this is a name field and the data type is either an inline or choice field type.
                // dt will be null in all other cases.
                if (dt != null && rdc.FieldId.Entity.Alias == "name")
                {
                    // Patch to a resource expression
                    var re = new ResourceExpression();
                    re.FieldId = rdc.FieldId;
                    re.CastType = dt;
                    re.NodeId = rdc.NodeId;
                    re.ExpressionId = expr.ExpressionId;
                    return re;
                }
            }
            return expr;
        }


        /// <summary>
        /// Deletes all contents from the current report entity that is associated with the new entity model.
        /// </summary>
        /// <param name="context"></param>
        private static void PurgeExisting(ToEntityContext context)
        {
            Model.Report report = context.ReportEntity;

            var toDelete = new List<IEntity>();

            if (report.RootNode != null)
            {
                report.RootNode.AsWritable().Delete();
            }
            if (report.ReportColumns != null)
            {
                toDelete.AddRange(report.ReportColumns);
                report.ReportColumns.Clear();
            }
            if (report.ReportOrderBys != null)
            {
                toDelete.AddRange(report.ReportOrderBys);
                report.ReportOrderBys.Clear();
            }
            if (report.HasConditions != null)
            {
                toDelete.AddRange(report.HasConditions);
                report.HasConditions.Clear();
            }

            context.ToDelete.AddRange(toDelete.Select(e => e.Id));
        }

        /// <summary>
        /// Coordinates conversion of the report object
        /// </summary>
        /// <param name="context"></param>
        private static void ConvertReport(ToEntityContext context)
        {
            var report = context.ReportInfo;

            ConvertStructuredQuery(report.StructuredQuery, context);
            ConvertAnalyzer(report, context);
            ConvertGridView(report.GridView, context);
            // Hook up any column references to this column
            // (yes, it could have been a multimap, or something)
            foreach (var pair in context.ColumnReferencesToBeMapped)
            {
                Model.ReportColumn rc;
                if (!context.Columns.TryGetValue(pair.Value, out rc))
                {
                    continue;
                }
                Model.ColumnReferenceExpression columnReference = pair.Key;
                columnReference.ExpressionReferencesColumn = rc;
                columnReference.Save();
            }
            
            context.ReportEntity.Save();
        }

        #region Report Rollups and Grouping

        private static void ConvertCalculatedResult(ClientAggregate clientAggregate, ToEntityContext context)
        {
            bool showSubTotals = false;
            bool showGrandTotals = false;
            const bool showCount = true; // Not in the existing structure using the default of true for conversion
            const bool showLabel = true; // Not in the existing structure using the default of true for conversion
            const bool showOptionLabel = true; // Not in the existing structure using the default of true for conversion
            Report report = context.ReportEntity;
            int priority = 0;
            foreach (ReportGroupField groupedColumn in clientAggregate.GroupedColumns)
            {
                if (groupedColumn.ShowSubTotals && !showSubTotals)
                {
                    showSubTotals = true;
                }
                if (groupedColumn.ShowGrandTotals && !showGrandTotals)
                {
                    showGrandTotals = true;
                }
                // convert the grouping method to an entity enumeration
                string alias = "group" + groupedColumn.GroupMethod;
                GroupingMethodEnum enumEntity = Model.Entity.Get<GroupingMethodEnum>(new EntityRef(alias));
                if (enumEntity == null)
                {
                    continue;
                }
                // Get the report column using the GUID reference in the report column mapping table
                Model.ReportColumn reportColumn;
                if (!context.Columns.TryGetValue(groupedColumn.ReportColumnId, out reportColumn))
                {
                    continue;
                }
                ReportRowGroup rowGroup = new ReportRowGroup
                {
                    GroupingPriority = priority++,
                    GroupingMethod = enumEntity,
                    GroupedColumn = reportColumn
                };
                rowGroup.Save();
                report.ReportRowGroups.Add(rowGroup);
                reportColumn.Save();
            }

            foreach (ReportAggregateField aggregatedColumn in clientAggregate.AggregatedColumns)
            {
                if (aggregatedColumn.ShowSubTotals && !showSubTotals)
                {
                    showSubTotals = true;
                }
                if (aggregatedColumn.ShowGrandTotals && !showGrandTotals)
                {
                    showGrandTotals = true;
                }
                // convert the aggregation method to an entity enumeration
                string alias = "agg" + aggregatedColumn.AggregateMethod;
                AggregateMethodEnum enumEntity = Model.Entity.Get<AggregateMethodEnum>(new EntityRef(alias));
                if (enumEntity == null)
                {
                    continue;
                }
                // Get the report column using the GUID reference in the report column mapping table
                Model.ReportColumn reportColumn;
                if (!context.Columns.TryGetValue(aggregatedColumn.ReportColumnId, out reportColumn))
                {
                    continue;
                }
                ReportRollup rowRollup = new ReportRollup
                {
                    RollupMethod = enumEntity,
                    RollupColumn = reportColumn
                };
                rowRollup.Save();
                reportColumn.Save();
            }
            report.RollupSubTotals = showSubTotals;
            report.RollupGrandTotals = showGrandTotals;
            report.RollupRowLabels = showLabel;
            report.RollupRowCounts = showCount;
            report.RollupOptionLabels = showOptionLabel;
            report.Save();
        }

        #endregion Report Rollups and Grouping

        #region Structured Query
        /// <summary>
        /// Converts the StructuredQuery object graph.
        /// All nodes, columns, and expresions will be accessible via the context dictionaries.
        /// </summary>
        private static void ConvertStructuredQuery(StructuredQuery structuredQuery, ToEntityContext context)
        {
            if (structuredQuery == null)
                return;

            Model.Report reportEntity = context.ReportEntity;

            // Convert root node
            reportEntity.RootNode = ConvertNode(structuredQuery.RootEntity, context);
            
            // Convert columns
            var columns = ConvertColumns(structuredQuery, context);
            reportEntity.ReportColumns.AddRange(columns);

            // Convert order-bys
            var orderBys = ConvertOrderBys(structuredQuery, context);
            reportEntity.ReportOrderBys.AddRange(orderBys);

            // Conditions are converted as part of analyzer
        }

        /// <summary>
        /// Types for related entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="searchNodeId">The search node unique identifier.</param>
        /// <returns>DatabaseType.</returns>
        /// <hack>
        /// Patch the column to be a resource expression if it is a resource data column, there is a related resource in the query 
        /// and it is not already a choice field and the column type alias is core:name
        /// </hack>
        private static DatabaseType TypeForRelatedEntities(List<Entity> entities, Guid searchNodeId)
        {
            // Check to see if we have this related resource in our list
            RelatedResource rr = entities.OfType<RelatedResource>().FirstOrDefault(relatedEntity => relatedEntity.NodeId == searchNodeId);
            if (rr != null && rr.EntityTypeId != null)
            {
                // Get the entity type, look for a choice field, if it's not an enum type then assume it's an inline type.
                Model.EntityType type = Model.Entity.Get<Model.EntityType>(rr.EntityTypeId);
                Model.EntityRef enumType = new Model.EntityRef("core", "enumValue");
                if (type.GetAncestorsAndSelf().FirstOrDefault(a => a.Id == enumType.Id) != null)
                {
                    return DatabaseType.ChoiceRelationshipType;
                }
                return DatabaseType.InlineRelationshipType;
            }
            // Check to see if there are child related resources
            List<Entity> childEntities = entities.SelectMany(re => re.RelatedEntities).ToList();
			return childEntities.Count > 0 ? TypeForRelatedEntities( childEntities, searchNodeId ) : null;
        }

        /// <summary>
        /// Converts report columns.
        /// Also hooks up any columnReference expressions waiting in the context.
        /// </summary>
        /// <param name="structuredQuery"></param>
        /// <param name="context"></param>
        private static IEnumerable<Model.ReportColumn> ConvertColumns(StructuredQuery structuredQuery, ToEntityContext context)
        {
            var results = new List<Model.ReportColumn>();

            int pos = 0;
            foreach (Structured.SelectColumn selectColumn in structuredQuery.SelectColumns)
            {
                // Bye bye SelectColumn.ColumnName
                // Bye bye SelectColumn.ColumnId

                var reportColumn = new Model.ReportColumn();
                reportColumn.ColumnIsHidden = selectColumn.IsHidden;
                reportColumn.Name = selectColumn.DisplayName;
                reportColumn.ColumnExpression = ConvertExpression(selectColumn.Expression, context);
                reportColumn.ColumnDisplayOrder = pos++;
                reportColumn.Save();
                results.Add(reportColumn);

                context.Columns[selectColumn.ColumnId] = reportColumn;
                context.SelectColumns[selectColumn.ColumnId] = selectColumn;

            }
            return results;
        }


        /// <summary>
        /// Converts report order-bys.
        /// </summary>
        /// <param name="structuredQuery"></param>
        /// <param name="context"></param>
        private static IEnumerable<Model.ReportOrderBy> ConvertOrderBys(StructuredQuery structuredQuery,
                                                                        ToEntityContext context)
        {
            var results = new List<Model.ReportOrderBy>();
            int pos = 0;

            foreach (Structured.OrderByItem orderBy in structuredQuery.OrderBy)
            {
                var reportOrderBy = new Model.ReportOrderBy();
                reportOrderBy.OrderByExpression = ConvertExpression(orderBy.Expression, context);
                reportOrderBy.ReverseOrder = orderBy.Direction == OrderByDirection.Descending;
                reportOrderBy.OrderPriority = pos++;
                reportOrderBy.Save();
                results.Add(reportOrderBy);
            }
            return results;
        }


        ///// <summary>
        ///// Converts report conditions.
        ///// </summary>
        ///// <param name="structuredQuery"></param>
        ///// <param name="context"></param>
        //private static IEnumerable<Model.ReportOrderBy> ConvertConditions(StructuredQuery structuredQuery, ToEntityContext context)
        //{
        //    var results = new List<Model.ReportCondition>();
        //    int pos = 0;
            
        //    foreach (Structured.OrderByItem orderBy in structuredQuery.OrderBy)
        //    {
        //        var reportOrderBy = new Model.ReportOrderBy();
        //        reportOrderBy.OrderByExpression = ConvertExpression(orderBy.Expression, context);
        //        reportOrderBy.ReverseOrder = orderBy.Direction == OrderByDirection.Descending;
        //        reportOrderBy.OrderPriority = pos++;
        //        reportOrderBy.Save();
        //        results.Add(reportOrderBy);
        //    }
        //    return results;
        //}
        #endregion

        #region Node types

        /// <summary>
        /// Converts a report node (StructuredQuery Entity) to a reportNode.
        /// Returns a pre-generated node if the same NodeId has already been encountered.
        /// Returns null for null.
        /// Returns a reportNode entity, already saved.
        /// </summary>
        private static Model.ReportNode ConvertNode(Structured.Entity node, ToEntityContext context)
        {
            if (node == null)
                return null; // assert false

            // See if we've already converted it (unlikely)
            Model.ReportNode result;
            if (context.Nodes.TryGetValue(node.NodeId, out result))
                return result;

            // Convert specific inherited types.
            if (node is AggregateEntity)
            {
                result = ConvertAggregateNode((AggregateEntity) node, context);
            }
            else if (node is DownCastResource)
            {
                result = ConvertDownCastResourceNode((DownCastResource)node, context);
            }
            else if (node is RelatedResource)
            {
                result = ConvertRelatedResourceNode((RelatedResource)node, context);
            }
            else if (node is CustomJoinNode)
            {
                result = ConvertCustomJoinNode( (CustomJoinNode)node, context);
            }
            else if (node is ResourceEntity)
            {
                result = ConvertResourceNode((ResourceEntity) node, context);
            }
            else
            {
                throw new Exception("Unknown report node type. " + node.GetType().Name);
            }

            // Add to cache
            if (node.NodeId != Guid.Empty)
            {
                context.Nodes.Add(node.NodeId, result);
            }

            // Migrate over children
			if ( node.RelatedEntities != null && node.RelatedEntities.Count > 0 )
            {
                foreach (var relatedNode in node.RelatedEntities)
                {
                    var child = ConvertNode(relatedNode, context);
                    if (child != null)
                    {
                        result.RelatedReportNodes.Add(child);
                    }

                }
            }

            // Save the node entity
            result.Save();

            return result.As<ReportNode>();
        }


        /// <summary>
        /// Convert an aggregate node to an entity.
        /// </summary>
        private static Model.ReportNode ConvertAggregateNode(Structured.AggregateEntity node, ToEntityContext context)
        {
            var result = new Model.AggregateReportNode();

            // Convert grouping node. (should be one)
            result.GroupedNode = ConvertNode(node.GroupedEntity, context);

            // Convert group-by expressions (zero or more)
            if (node.GroupBy != null)
            {
                foreach (var expr in node.GroupBy)
                {
                    Model.ReportExpression reportExpr = ConvertExpression(expr, context);
                    result.GroupedBy.Add(reportExpr);
                }
            }

            // Caller will save
            return result.As<ReportNode>();
        }


        /// <summary>
        /// Convert a down-cast node to an entity.
        /// </summary>
        private static Model.ReportNode ConvertDownCastResourceNode(Structured.DownCastResource node, ToEntityContext context)
        {
            var result = new Model.DerivedTypeReportNode();
            
            result.TargetMustExist = node.MustExist;
            result.ExactType = node.ExactType;
            result.ResourceReportNodeType = ConvertEntityRef<EntityType>(node.EntityTypeId);

            // Caller will save
            return result.As<ReportNode>();
        }

        /// <summary>
        /// Convert a custom join node to an entity.
        /// </summary>
        private static Model.ReportNode ConvertCustomJoinNode( Structured.CustomJoinNode node, ToEntityContext context )
        {
            var result = new Model.CustomJoinReportNode( );

            result.TargetMustExist = node.ResourceMustExist;
            result.TargetNeedNotExist = node.ResourceNeedNotExist;
            result.ParentNeedNotExist = node.ParentNeedNotExist;
            result.ExactType = node.ExactType;
            result.JoinPredicateCalculation = node.JoinPredicateScript;
            result.ResourceReportNodeType = ConvertEntityRef<EntityType>( node.EntityTypeId );

            // Caller will save
            return result.As<ReportNode>( );
        }

        /// <summary>
        /// Convert a related-resource node to an entity.
        /// </summary>
        private static Model.ReportNode ConvertRelatedResourceNode(Structured.RelatedResource node, ToEntityContext context)
        {
            var result = new Model.RelationshipReportNode();

            result.TargetMustExist = node.ResourceMustExist;
            result.TargetNeedNotExist = node.ResourceNeedNotExist;
            result.ParentNeedNotExist = node.ParentNeedNotExist;
            result.ExactType = node.ExactType;
            result.ResourceReportNodeType = ConvertEntityRef<EntityType>(node.EntityTypeId);
            result.FollowInReverse = node.RelationshipDirection == RelationshipDirection.Reverse;
            result.FollowRelationship = Model.Entity.Get<Relationship>(node.RelationshipTypeId);

            switch (node.Recursive)
            {
                case RecursionMode.Recursive:
                    result.FollowRecursive = true;
                    result.IncludeSelfInRecursive = false;
                    break;
                case RecursionMode.RecursiveWithSelf:
                    result.FollowRecursive = true;
                    result.IncludeSelfInRecursive = true;
                    break;
            }

            // Caller will save
            return result.As<ReportNode>();
        }

        
        /// <summary>
        /// Convert a root resource node to an entity.
        /// </summary>
        private static Model.ReportNode ConvertResourceNode(Structured.ResourceEntity node, ToEntityContext context)
        {
            var result = new Model.ResourceReportNode();

            result.TargetMustExist = true; // not really important for resource node.
            result.ExactType = node.ExactType;
            result.ResourceReportNodeType = ConvertEntityRef<EntityType>(node.EntityTypeId);

            // Caller will save
            return result.As<ReportNode>();
        }

        #endregion

        #region Expressions

        /// <summary>
        /// Converts an expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="context"></param>
        /// <param name="useResourceLists">If true, flag expression types that return resources as requiring resourceList.</param>
        /// <returns></returns>
        private static Model.ReportExpression ConvertExpression(ScalarExpression expr, ToEntityContext context, bool useResourceLists = false)
        {
            if (expr == null)
                return null;

            // See if we've already converted it (unlikely)
            Model.ReportExpression result;
            if (context.Expressions.TryGetValue(expr.ExpressionId, out result))
                return result;

            // Convert specific inherited types.
            if (expr is IdExpression)
            {
                result = ConvertIdExpression((IdExpression)expr, context);
                IEntity resultType = useResourceLists ? ((IEntity)new ResourceListArgument()) : new ResourceArgument();
                resultType.Save();
                result.ReportExpressionResultType = resultType.As<ActivityArgument>();
            }
            else if (expr is ResourceExpression)
            {
                result = ConvertResourceExpression((ResourceExpression)expr, context);
            }
            else if (expr is ResourceDataColumn)
            {
                result = ConvertFieldExpression((ResourceDataColumn)expr, context);
            }
            else if (expr is ScriptExpression)
            {
                result = ConvertScriptExpression((ScriptExpression)expr, context);
            }
            else if (expr is AggregateExpression)
            {
                result = ConvertAggregateExpression((AggregateExpression)expr, context);
            }
            else if (expr is ColumnReference)
            {
                result = ConvertColumnReferenceExpression((ColumnReference)expr, context);
            }
            else
            {
                // StructureViewExpression - some rainy day
                // ScalarExpression - abstract
                // EntityExpression - abstract
                // ComparisonExpression - should not be used in this context (at least not at the moment)
                // IfElseExpression - legacy XML-based calculations
                // LiteralExpression - legacy XML-based calculations
                // LogicalExpression - legacy XML-based calculations
                // CalculationExpression - legacy XML-based calculations
                throw new Exception("Unsupported report expression type. " + expr.GetType().Name);
            }

            // Associate to node
            if (expr is EntityExpression)
            {
                EntityExpression eExpr = (EntityExpression)expr;
                ReportNode node;
                if (!context.Nodes.TryGetValue(eExpr.NodeId, out node))
                {
                    throw new Exception("Expression refers to a node that does not exist. " + eExpr.NodeId);
                }
                result.As<NodeExpression>().SourceNode = node;
            }

            // Determine expression type
            if (result.ReportExpressionResultType == null)
            {
                ResultColumn exprMetadata;
                if (context.QueryResult.ExpressionTypes.TryGetValue(expr, out exprMetadata))
                {
                    result.ReportExpressionResultType = ConvertExpressionMetadata(exprMetadata, context, useResourceLists);
                }
            }

            // Add to cache
            if (expr.ExpressionId != Guid.Empty)
            {
                context.Expressions.Add(expr.ExpressionId, result);
            }

            // Save the expression entity
            result.Save();

            return result;
        }

        /// <summary>
        /// Converts the metadata of an expression into an ActivityArgument (the entity models' typed-value).
        /// </summary>
        /// <param name="exprMetadata"></param>
        /// <param name="context"></param>
        /// <param name="useResourceLists"></param>
        /// <returns></returns>
        private static ActivityArgument ConvertExpressionMetadata(ResultColumn exprMetadata, ToEntityContext context, bool useResourceLists)
        {
            IEntity result;
            DatabaseType dbType = exprMetadata.ColumnType;

            result = TypedValueHelper.CreateTypedValueEntity(dbType, useResourceLists);
            if (dbType is StringType && exprMetadata.FieldId > 0)
            {
                StringArgument stringArgument = result.As<StringArgument>();
                Field fieldInfo = Model.Entity.Get<Field>(exprMetadata.FieldId);
                StringField stringField = fieldInfo.As<StringField>();
                bool? allowMultiLines = stringField != null ? fieldInfo.As<StringField>().AllowMultiLines : null;
                if (allowMultiLines != null && allowMultiLines.Value)
                {
                    stringArgument.StringMultiLine = true;
                }
            }
            else if (dbType is CurrencyType)
            {
                var curArg = result.As<CurrencyArgument>();
                curArg.NumberDecimalPlaces = exprMetadata.DecimalPlaces;
                result = curArg;
            }
            else if (dbType is DecimalType)
            {
                var decArg = result.As<DecimalArgument>();
                decArg.NumberDecimalPlaces = exprMetadata.DecimalPlaces;
                result = decArg;
            }
            else if (dbType is ChoiceRelationshipType || dbType is InlineRelationshipType || dbType is IdentifierType)
            {
                var resArg = result.As<TypedArgument>();
                if (exprMetadata.ResourceTypeId > 0)
                {
                    resArg.ConformsToType = Model.Entity.Get<EntityType>(exprMetadata.ResourceTypeId);
                }
                else
                {
                    EventLog.Application.WriteError("No resource ID for XML column id - {0}", exprMetadata.RequestColumn.ColumnId);
                    throw new ArgumentOutOfRangeException();
                }
                result = resArg;
            }
            else if (dbType == null)
            {
                // HACK! - Default to a string type
                // I think this is for Aggregates only where we have this happening
                result = new StringArgument();
            }

            result.Save();
            return result.As<ActivityArgument>();
        }


        /// <summary>
        /// Convert an ID expression to an entity.
        /// </summary>
        private static Model.ReportExpression ConvertIdExpression(Structured.IdExpression expr, ToEntityContext context)
        {
            var result = new Model.IdExpression();

            return result.As<ReportExpression>();
        }


        /// <summary>
        /// Convert a resource expression to an entity.
        /// </summary>
        private static Model.ReportExpression ConvertResourceExpression(Structured.ResourceExpression expr, ToEntityContext context)
        {
            var result = new Model.ResourceExpression();

            // Remember: when mapping in reverse direction, set FieldId = to EntityRef core:name
            // Remember: when mapping in reverse direction, calculate OrderFieldId  (in particular for enums)

            return result.As<ReportExpression>();
        }


        /// <summary>
        /// Convert a ResourceDataColumn (field) expression to an entity.
        /// </summary>
        private static Model.ReportExpression ConvertFieldExpression(Structured.ResourceDataColumn expr, ToEntityContext context)
        {
            var result = new Model.FieldExpression();

            // Map the field reference
            // (I think we can ignore expr.CastType)
            result.FieldExpressionField = ConvertEntityRef<Model.Field>(expr.FieldId);

            return result.As<ReportExpression>();
        }


        /// <summary>
        /// Convert a script expression to an entity.
        /// That is, an expression containing script text. Not to be confused with the legacy XML calculated expression.
        /// </summary>
        private static Model.ReportExpression ConvertScriptExpression(Structured.ScriptExpression expr, ToEntityContext context)
        {
            var result = new Model.ScriptExpression();
            result.ReportScript = expr.Script;

            return result.As<ReportExpression>();
        }


        /// <summary>
        /// Convert an aggregate expression to an entity (count, sum, max, etc..).
        /// </summary>
        private static Model.ReportExpression ConvertAggregateExpression(Structured.AggregateExpression expr, ToEntityContext context)
        {
            var result = new Model.AggregateExpression();

            // Map aggregate method
            // Take string form of enum and map to alias name.
            string alias = "agg" + expr.AggregateMethod.ToString();
            var enumEntity = Model.Entity.Get<AggregateMethodEnum>(new EntityRef(alias));
            result.AggregateMethod = enumEntity;
            
            // Map the aggregated expression
            result.AggregatedExpression = ConvertExpression(expr.Expression, context);

            // Set the type as this is typically lost and we can get this from the context aggregated node
            ReportNode reportNode;
            if (context.Nodes.TryGetValue(expr.NodeId, out reportNode))
            {
                if (reportNode.Is<Model.AggregateReportNode>())
                {
                    Model.AggregateReportNode aggregateReportNode = reportNode.As<Model.AggregateReportNode>();
                    if (aggregateReportNode.GroupedNode.Is<Model.RelationshipReportNode>())
                    {
                        Model.RelationshipReportNode relationshipReportNode = aggregateReportNode.GroupedNode.As<Model.RelationshipReportNode>();
                        ResultColumn exprMetadata;
                        if (context.QueryResult.ExpressionTypes.TryGetValue(expr, out exprMetadata))
                        {
                            exprMetadata.ResourceTypeId = relationshipReportNode.ResourceReportNodeType.Id;
                        }
                    }
                }
            }

            return result.As<ReportExpression>();
        }


        /// <summary>
        /// Convert a column reference expression to an entity.
        /// </summary>
        private static Model.ReportExpression ConvertColumnReferenceExpression(Structured.ColumnReference expr, ToEntityContext context)
        {
            var result = new Model.ColumnReferenceExpression();
            context.ColumnReferencesToBeMapped.Add(result, expr.ColumnId);

            result.Save();
            return result.As<ReportExpression>();
        }
        #endregion

        #region Conditions
        /// <summary>
        /// Converts analyzer fields.
        /// Combined StructuredQuery XML Conditions and AnalyzerFields from Analyzer XML.
        /// </summary>
        /// <param name="report"></param>
        /// <param name="context"></param>
        private static void ConvertAnalyzer(ReportInfo report, ToEntityContext context)
        {
            // Sources
            StructuredQuery query = report.StructuredQuery;
            var analyzerFields = report.Analyzer.AnalyzerFields;
            // Target
            Model.Report reportEntity = context.ReportEntity;

            int pos = 0;
            foreach (QueryCondition condition in query.Conditions)
            {
                ReportAnalyzerField analyserField = analyzerFields.FirstOrDefault(af => af.QueryExpressionId == condition.Expression.ExpressionId);

                Model.ReportCondition reportCondition = ConvertQueryCondition(condition, analyserField, context, false);
                if (condition == null)
                    continue;

                reportCondition.ConditionDisplayOrder = pos++;

                // Add the condition to the report
                reportEntity.HasConditions.Add(reportCondition);
            }
        }

        private static Model.ReportCondition ConvertQueryCondition(QueryCondition condition, ReportAnalyzerField analyserField, ToEntityContext context, bool isConditionalFormatting)
        {
            // Create condition entity
            var reportCondition = new Model.ReportCondition();

            if (!isConditionalFormatting)
            {
                // Decorate with analsyer field object
                if (analyserField == null)
                {
                    reportCondition.ConditionIsHidden = true;   // Analyser field is probably the more authorative view of what's visible.
                    reportCondition.Name = "Unnamed condition";
                }
                else
                {
                    // Bye bye ReportAnalyzerField.ColumnName
                    // Bye bye FieldTypeName (Get it from .ConditionExpression.ReportExpressionResultType)
                    reportCondition.ConditionIsHidden = analyserField.IsHidden;
                    reportCondition.Name = analyserField.DisplayName;
                }

                // Set expression
                var scalarExpr = condition.Expression;
                var expr = ConvertExpression(scalarExpr, context, true);
                if (expr == null)
                    return null;
                reportCondition.ConditionExpression = expr;
            }

            // Operator
            if (condition.Operator != ConditionType.Unspecified)
            {
                string alias = "oper" + condition.Operator.ToString();
                var enumEntity = Model.Entity.Get<OperatorEnum>(new EntityRef(alias));
                reportCondition.Operator = enumEntity;
            }

            // Value parameter
            reportCondition.ConditionParameter = ConvertConditionParameter(condition, reportCondition.ConditionExpression, context);
            return reportCondition;
        }

        private static Model.Parameter ConvertConditionParameter(QueryCondition condition, Model.ReportExpression reportExpression,
                                                      ToEntityContext context)
        {
            // No operator selected means no parameter to fill
            if (condition.Operator == ConditionType.Unspecified)
                return null;

            // No arguments means no parameter to fill
            int count = ConditionTypeHelper.GetArgumentCount(condition.Operator);
            if (count == 0)
                return null;
            
            ResultColumn exprMetadata;
            context.QueryResult.ExpressionTypes.TryGetValue(condition.Expression, out exprMetadata);
            if (exprMetadata == null)
                return null; // for now, no parameter if the query engine couldn't figure out the expression type.

            DatabaseType exprType = exprMetadata.ColumnType;

            // Create activity argument, with value if available.
            ActivityArgument activityArgument;
            if (condition.Argument != null)
            {
                // Capture type & value
                activityArgument = TypedValueHelper.CreateTypedValueEntity(condition.Argument, true);
            }
            else
            {
                DatabaseType paramDbType = ConditionTypeHelper.GetArgumentType(condition.Operator, exprType, 0);
                if (exprMetadata.IsResource)
                    paramDbType = DatabaseType.InlineRelationshipType;

                // Capture type only
                activityArgument = TypedValueHelper.CreateTypedValueEntity(paramDbType, true);

                if (activityArgument.Is<ResourceListArgument>() && reportExpression.ReportExpressionResultType.Is<ResourceListArgument>())
                {
                    ResourceListArgument rla = activityArgument.As<ResourceListArgument>();
                    rla.ConformsToType = reportExpression.ReportExpressionResultType.As<ResourceListArgument>().ConformsToType;
                }
            }

            // Handle resource list arguments
            if (activityArgument != null && activityArgument.Is<ResourceListArgument>() && condition.Arguments.Count > 0)
            {
                var resourceList = activityArgument.As<ResourceListArgument>();

                resourceList.ResourceListParameterValues.Clear();
                
                // Add multiple list entries
                // First is already added
                foreach (TypedValue argument in condition.Arguments)
                {
                    long argumentValueId;
                    if (argument.Value == null || !long.TryParse(argument.Value.ToString(), out argumentValueId))
                        continue;
                    var value = Model.Entity.Get<Resource>(argumentValueId);
                    resourceList.ResourceListParameterValues.Add(value);
                }
            }

            if (activityArgument != null)
            {
                activityArgument.Save();
            }

            // Create parameter
            var parameter = new Model.Parameter();
            parameter.ParamTypeAndDefault = activityArgument;
            parameter.Save();
            
            return parameter;

        }

        #endregion

        #region Formatting Rules

        /// <summary>
        /// Convert all grid format data.
        /// </summary>
        private static void ConvertGridView(GridReportDataView gridView, ToEntityContext context)
        {
            if (gridView == null)
                return;

            foreach (ColumnFormatting colFormat in gridView.ColumnFormats)
            {
                if (!context.SelectColumns.ContainsKey(colFormat.QueryColumnId))
                    continue;
                SelectColumn selectColumn = context.SelectColumns[colFormat.QueryColumnId];
                ReportColumn reportColumn = context.Columns[colFormat.QueryColumnId];

                reportColumn.ColumnDisplayFormat = ConvertColumnFormat(colFormat, selectColumn, context);
                reportColumn.ColumnFormattingRule = ConvertFormattingRule(colFormat.FormattingRule, selectColumn, context);
                reportColumn.Save();
            }
        }


        /// <summary>
        /// Convert static column display/formatting rules.
        /// </summary>
        private static DisplayFormat ConvertColumnFormat(ColumnFormatting colFormat, SelectColumn selectColumn,
                                                ToEntityContext context)
        {
            var displayFormat = new DisplayFormat();
            displayFormat.ColumnShowText = colFormat.ShowText;
            displayFormat.DisableDefaultFormat = colFormat.DisableDefaultFormat;
            displayFormat.MaxLineCount = colFormat.Lines;
            displayFormat.FormatPrefix = colFormat.Prefix;
            displayFormat.FormatSuffix = colFormat.Suffix;
            displayFormat.FormatDecimalPlaces = colFormat.DecimalPlaces;
            
            string alias = "align" + colFormat.TextAlignment.ToString();
            if (alias == "alignCenter") alias = "alignCentre";
            var enumEntity = Model.Entity.Get<AlignEnum>(new EntityRef(alias));
            displayFormat.FormatAlignment = enumEntity;
            // Image handling
            ImageFormattingRule ifr = colFormat.FormattingRule as ImageFormattingRule;
            if (ifr != null)
            {
                // Add relationships for the image formatting rules
                if (ifr.ThumbnailScaleId != null)
                {
                    displayFormat.FormatImageScale = Model.Entity.Get<ImageScaleEnum>(ifr.ThumbnailScaleId);
                }
                if (ifr.ThumbnailSizeId != null)
                {
                    displayFormat.FormatImageSize = Model.Entity.Get<ThumbnailSizeEnum>(ifr.ThumbnailSizeId);
                }
            }
            // Column type formatting for date/date time/time (This is all that the format string element is used for)
            if (colFormat.ColumnType is DateType)
            {
                DateColFmtEnum dcfe = FormatForDateString(colFormat.FormatString);
                if (dcfe != null)
                {
                    displayFormat.DateColumnFormat = dcfe;
                }
            }
            else if (colFormat.ColumnType is TimeType)
            {
                TimeColFmtEnum tcfe = FormatForTimeString(colFormat.FormatString);
                if (tcfe != null)
                {
                    displayFormat.TimeColumnFormat = tcfe;
                }
            }
            else if (colFormat.ColumnType is DateTimeType)
            {
                DateTimeColFmtEnum dtcfe = FormatForDateTimeString(colFormat.FormatString);
                if (dtcfe != null)
                {
                    displayFormat.DateTimeColumnFormat = dtcfe;
                }
            }

            if (colFormat.EntityListColumnFormat != null)
            {
                displayFormat.EntityListColumnFormat = colFormat.EntityListColumnFormat;
            }

            displayFormat.Save();
            return displayFormat;
        }

        #region Date/time/datetime formatting hack
        private static DateColFmtEnum FormatForDateString(string formatString)
        {
            switch (formatString)
            {
                // Date formats
                case "{0:d}":
                    return Model.Entity.Get<DateColFmtEnum>(new EntityRef("dateShort"));
                case "{0:M}":
                    return Model.Entity.Get<DateColFmtEnum>(new EntityRef("dateDayMonth"));
                case "{0:D}":
                    return Model.Entity.Get<DateColFmtEnum>(new EntityRef("dateLong"));
                case "{0:MMM}":
                    return Model.Entity.Get<DateColFmtEnum>(new EntityRef("dateMonth"));
                case "{0:Y}":
                    return Model.Entity.Get<DateColFmtEnum>(new EntityRef("dateMonthYear"));
                case "{0:yyyy}":
                    return Model.Entity.Get<DateColFmtEnum>(new EntityRef("dateYear"));
                case "{0:QY}":
                    return Model.Entity.Get<DateColFmtEnum>(new EntityRef("dateQuarterYear"));
            }
            return null;
        }

        private static TimeColFmtEnum FormatForTimeString(string formatString)
        {
            switch (formatString)
            {
                // Time formats
                case "{0:t}":
                    return Model.Entity.Get<TimeColFmtEnum>(new EntityRef("time12Hour"));
                case "{0:H:mm}":
                    return Model.Entity.Get<TimeColFmtEnum>(new EntityRef("time24Hour"));
            }
            return null;
        }

        private static DateTimeColFmtEnum FormatForDateTimeString(string formatString)
        {
            switch (formatString)
            {
                // Date - Time formats
                case "{0:d} {0:t}":
                    return Model.Entity.Get<DateTimeColFmtEnum>(new EntityRef("dateTimeShort"));
                case "{0:d} {0:H:mm}":
                    return Model.Entity.Get<DateTimeColFmtEnum>(new EntityRef("dateTime24Hour"));
                case "{0:M}":
                    return Model.Entity.Get<DateTimeColFmtEnum>(new EntityRef("dateTimeDayMonth"));
                case "{0:M} {0:t}":
                    return Model.Entity.Get<DateTimeColFmtEnum>(new EntityRef("dateTimeDayMonthTime"));
                case "{0:f}":
                    return Model.Entity.Get<DateTimeColFmtEnum>(new EntityRef("dateTimeLong"));
                case "{0:s}":
                    return Model.Entity.Get<DateTimeColFmtEnum>(new EntityRef("dateTimeSortable"));
                case "{0:Y}":
                    return Model.Entity.Get<DateTimeColFmtEnum>(new EntityRef("dateTimeMonthYear"));
                case "{0:QY}":
                    return Model.Entity.Get<DateTimeColFmtEnum>(new EntityRef("dateTimeQuarterYear"));
                case "{0:yyyy}":
                    return Model.Entity.Get<DateTimeColFmtEnum>(new EntityRef("dateTimeYear"));
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Convert conditional formatting rules. (Select type).
        /// </summary>
        private static Model.FormattingRule ConvertFormattingRule(FormattingRule formattingRule,
                                                                  SelectColumn selectColumn, ToEntityContext context)
        {
            if (formattingRule == null)
                return null;

            IEntity result;
            if (formattingRule is BarFormattingRule)
            {
                result = ConvertBarFormattingRule((BarFormattingRule)formattingRule, context);
            }
            else if (formattingRule is ColorFormattingRule)
            {
                result = ConvertColorFormattingRule((ColorFormattingRule)formattingRule, selectColumn, context);
            }
            else if (formattingRule is IconFormattingRule)
            {
                result = ConvertIconFormattingRule((IconFormattingRule)formattingRule, selectColumn, context);
            }
            else
            {
                throw new Exception("Unknown formatting rule: " + formattingRule.GetType().Name);
            }
            return result.As<Model.FormattingRule>();
        }


        /// <summary>
        /// Convert bar (range) formatting rules.
        /// </summary>
        private static IEntity ConvertBarFormattingRule(BarFormattingRule formattingRule, ToEntityContext context)
        {
            var result = new Model.BarFormattingRule();

            // Color
            result.BarColor = formattingRule.Color != null ? formattingRule.Color.GetArgbHex() : "000000ff";

            // Max/Min
            result.BarMaxValue = TypedValueHelper.CreateTypedValueEntity(formattingRule.Maximum) ??
                                 TypedValueHelper.CreateTypedValueEntity(DatabaseType.StringType);
            result.BarMinValue = TypedValueHelper.CreateTypedValueEntity(formattingRule.Minimum) ??
                                 TypedValueHelper.CreateTypedValueEntity(DatabaseType.StringType);
            
            result.Save();
            return result;
        }

        /// <summary>
        /// Convert color formatting rules.
        /// </summary>
        private static IEntity ConvertColorFormattingRule(ColorFormattingRule formattingRule,
            SelectColumn selectColumn, ToEntityContext context)
        {
            var result = new Model.ColorFormattingRule();

            int pos = 0;
            foreach (ColorRule rule in formattingRule.Rules)
            {
                var ruleEntity = new Model.ColorRule();
                ruleEntity.RulePriority = pos++;
                ruleEntity.ColorRuleBackground = rule.BackgroundColor.GetArgbHex();
                ruleEntity.ColorRuleForeground = rule.ForegroundColor.GetArgbHex();
                ruleEntity.RuleCondition = ConvertCondition(rule.Condition, selectColumn, context);
                
                ruleEntity.Save();
                result.ColorRules.Add(ruleEntity);
            }

            result.Save();
            return result;
        }


        /// <summary>
        /// Convert icon formatting rules.
        /// </summary>
        private static IEntity ConvertIconFormattingRule(IconFormattingRule formattingRule,
            SelectColumn selectColumn, ToEntityContext context)
        {
            var result = new Model.IconFormattingRule();

            int pos = 0;
            foreach (IconRule rule in formattingRule.Rules)
            {
                var ruleEntity = new Model.IconRule();
                ruleEntity.RulePriority = pos++;

                if (rule.CfEntityId != null && rule.CfEntityId.Value > 0)
                {
                    var cfi = Model.Entity.Get<ConditionalFormatIcon>(rule.CfEntityId.Value);
                    if (cfi != null)
                    {
                        ruleEntity.IconRuleCFIcon = cfi;
                        if (cfi.CondFormatImage != null)
                           ruleEntity.IconRuleImage = cfi.CondFormatImage.As<ImageFileType>();
                    }
                }
                else if (rule.IconId != null && rule.IconId.Value > 0)
                {
                    var image = Model.Entity.Get<ImageFileType>(rule.IconId.Value);
                    if (image != null)
                        ruleEntity.IconRuleImage = image;
                }
                else if (rule.Icon != IconType.None)
                {
                    string iconName = rule.Icon.ToString();
                    if (iconName.StartsWith("Arrow"))
                        iconName = iconName.Substring("Arrow".Length) + "Arrow";    // move 'arrow' from start to end

                    string color = ClosestColor(rule.Color);
                    string iconAlias = color + iconName + "CondFormat";
                    if (iconAlias == "greenTickCondFormat") iconAlias = "tickCondFormat";
                    if (iconAlias == "redCrossCondFormat") iconAlias = "crossCondFormat";

                    var cfi = Model.Entity.Get<ConditionalFormatIcon>(new EntityRef(iconAlias));
                    if (cfi != null)
                    {
                        ruleEntity.IconRuleCFIcon = cfi;
                        if (cfi.CondFormatImage != null)
                           ruleEntity.IconRuleImage = cfi.CondFormatImage.As<ImageFileType>();
                    }
                }
                ruleEntity.RuleCondition = ConvertCondition(rule.Condition, selectColumn, context);

                ruleEntity.Save();
                result.IconRules.Add(ruleEntity);
            }

            result.Save();
            return result;
        }


        /// <summary>
        /// Pick a close color alias.
        /// </summary>
        private static string ClosestColor(ColorInfo color)
        {
            var options = new Dictionary<string, ColorInfo> {
                {"black", new ColorInfo { R=0, G=0, B=0 }},
                {"red", new ColorInfo { R=180, G=0, B=0 }},
                {"green", new ColorInfo { R=0, G=180, B=0 }},
                {"yellow", new ColorInfo { R=180, G=180, B=0 }},
            };
            Func<int, int> sq = b => b * b;
            Func<ColorInfo, ColorInfo, int> dist = (c1, c2) => sq(c1.R - c2.R) + sq(c1.G - c2.G) + sq(c1.B - c2.B);
            var closest = (from pair in options
                           orderby dist(color, pair.Value)
                           select pair.Key).First();
            return closest;
        }


        /// <summary>
        /// Converts a conditional formatting condition. (c.f. Structured Query condition)
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="column">The column.</param>
        /// <param name="context">The context.</param>
        /// <returns>ReportCondition.</returns>
        private static ReportCondition ConvertCondition(Condition condition, SelectColumn column, ToEntityContext context)
        {
            var qc = new QueryCondition();
            qc.Arguments = condition.Arguments.ToList();
            qc.Operator = condition.Operator;
            qc.Expression = column.Expression;
            var res = ConvertQueryCondition(qc, null, context, true);
            return res;
        }

        #endregion

        #region Misc Helpers

        private static T ConvertEntityRef<T>(EntityRef typeId) where T : class, IEntity
        {
            if (typeId == null || typeId.Id == 0)
                return default(T);

            var res = Model.Entity.Get<T>(typeId);
            return res;
        }
        #endregion

    }
}
// ReSharper restore UnusedParameter.Local
// ReSharper restore UseObjectOrCollectionInitializer
// ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
// ReSharper restore RedundantNameQualifier
