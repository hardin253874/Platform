// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Metadata.Query.Structured.Helpers
{
    /// <summary>
    /// Various helper methods for reports.
    /// </summary>
    public static class ReportHelpers
    {
        // Note: pre-load queries touch fields and relationships that the server likely needs to access
        // while running reports. It does not necessarily need to include every piece of info that the client may require.
        // (That is, this may exclude some info that the client requests get; and include some info that the client does not need.
        // It's for pre-filling the server side entity cache)

        // Query string for pre-loading report
        private const string ReportPreloaderQueryTemplate = @"
            let @TYPE = {
                isOfType.id
            }

            let @ALIASTYPE = {
                alias, isOfType.id
            }

            let @REPORTEXPVALUE = {
                isOfType.id,
                name,
                conformsToType.{ name, isOfType.id, defaultPickerReport.id }, 
                { resourceParameterValue, resourceArgumentValue }.@TYPE,
                resourceListParameterValues.{ isOfType.id, name},
                stringParameterValue,
                intParameterValue,
                decimalParameterValue,
                boolParameterValue,
                dateParameterValue,
                timeParameterValue,
                dateTimeParameterValue,
                numberDecimalPlaces
            }

            let @FIELD = {
                alias, name, isOfType.id,
                fieldCalculation
            }

            let @EXPR = {
                isOfType.id,
                reportExpressionResultType.@REPORTEXPVALUE,
                sourceNode.followRelationship.@ALIASTYPE,
                fieldExpressionField.@FIELD,
                reportScript,
                expressionReferencesColumn.@COLUMN,
                aggregateMethod.@ALIASTYPE,
                aggregatedExpression.@EXPR,                
                structureViewExpressionSourceNode.id,
                structureViewExpressionStructureView.id,
                expressionForColumn.columnDisplayFormat.{ dateColumnFormat, timeColumnFormat, dateTimeColumnFormat }.@ALIASTYPE
            }

            let @NODE = {
                isOfType.id,
                relatedReportNodes.@NODE,
                groupedNode.@NODE,
                parentAggregatedNode.@NODE,
                parentReportNode.@NODE,
                exactType,
                targetMustExist,
                targetNeedNotExist,
                parentNeedNotExist,
                joinPredicateCalculation,
                resourceReportNodeType.@ALIASTYPE,
                followInReverse,
                followRecursive,
                includeSelfInRecursive,
                constrainParent,
                checkExistenceOnly,
                followRelationship.{ cardinality.@ALIASTYPE, securesFrom, securesTo },
                groupedBy.@EXPR
                <visual>,
                    name
                </visual>
            }

            let @CONDITION = {   
                name,
                isOfType.id,
                conditionExpression.@EXPR,
                operator.@ALIASTYPE,
                conditionParameterPicker.id,
                conditionParameter.{
                    isOfType.{name,alias},
                    paramTypeAndDefault.@REPORTEXPVALUE
                }
                <visual>,
                    conditionIsHidden,
                    conditionIsLocked,
                    conditionDisplayOrder,
                    columnForCondition.id
                </visual>
            }

            <visual>
            let @COLFORMAT = {
                isOfType.id,
                barColor,
                barMinValue.@REPORTEXPVALUE,
                barMaxValue.@REPORTEXPVALUE,
                iconRules.{
                    isOfType.id,
                    rulePriority,
                    ruleCondition.@CONDITION,
                    iconRuleImage.id,
                    iconRuleCFIcon.condFormatImage.id
                },
                colorRules.{
                    isOfType.id,
                rulePriority,
                    ruleCondition.@CONDITION,
                    colorRuleForeground,
                    colorRuleBackground
                }
            }

            let @COLUMNDISPLAYFORMAT = {
                isOfType.id,
                formatImageScale.@ALIASTYPE,
                formatImageSize.@ALIASTYPE,
                formatAlignment.@ALIASTYPE,
                columnShowText,
                disableDefaultFormat,
                formatDecimalPlaces,
                formatPrefix,
                formatSuffix,
                maxLineCount,
                entityListColumnFormat.@ALIASTYPE
            }

            let @COLUMNGROUP = {
                isOfType.id,
                groupingPriority,
                groupingCollapsed,
                groupingMethod.@ALIASTYPE
            }
            </visual>

            let @COLUMN = {
                isOfType.id,
                name,
                columnDisplayOrder,
                columnIsHidden,
                columnExpression.@EXPR
                <visual>,
                    columnFormattingRule.@COLFORMAT,
                    columnDisplayFormat.@COLUMNDISPLAYFORMAT,
                    columnGrouping.@COLUMNGROUP, 
                    columnRollup.{ isOfType.id, rollupMethod.@ALIASTYPE }
                </visual>
            }

            let @ORDERBY = {
                isOfType.id,
                reverseOrder,
                orderPriority,
                orderByExpression.@EXPR
            }

            let @REPORT = {
                name,
                modifiedDate,
                isOfType.id,
                rootNode.@NODE,
                reportColumns.@COLUMN,
                hasConditions.@CONDITION,
                reportOrderBys.@ORDERBY,
                reportForAccessRule.accessRuleHidden
                <visual>,
                    rollupGrandTotals, rollupSubTotals, rollupRowCounts, rollupRowLabels, rollupOptionLabels,
                    reportStyle.@ALIASTYPE,
                    resourceViewerConsoleForm.@TYPE,
                    hideAddButton, hideNewButton, hideRemoveButton, hideActionBar, hideReportHeader
                </visual>
            }

            @REPORT";


        /// <summary>
        /// Preloader string for full reports, including formatting.
        /// (Just remove the visual tags themselves)
        /// </summary>
        public static readonly string ReportPreloaderQuery = Regex.Replace( ReportPreloaderQueryTemplate, "<visual>|</visual>", "", RegexOptions.Multiline );

        /// <summary>
        /// Preloader string for structured queries only, excluding formatting.
        /// (Remove the visual tags and their contents)
        /// </summary>
        public static readonly string QueryPreloaderQuery = Regex.Replace( ReportPreloaderQueryTemplate, "<visual>[^<]*</visual>", "", RegexOptions.Multiline );

        /// <summary>
        /// Pre-load report entities.
        /// </summary>
        /// <param name="reportId">The ID of the report to preload.</param>
        public static void PreloadReport(EntityRef reportId)
        {
            using ( MessageContext messageContext = new MessageContext( "Reports" ) )
            {
                messageContext.Append(() => string.Format("Preload report {0}", reportId));

                var rq = new EntityRequest( reportId, ReportPreloaderQuery, "Preload report " + reportId.ToString( ) );
                BulkPreloader.Preload( rq );
            }
        }

        /// <summary>
        /// Pre-load just the query portion of a report.
        /// </summary>
        /// <param name="reportId">The ID of the report to preload.</param>
        public static void PreloadQuery( EntityRef reportId )
        {
            var rq = new EntityRequest( reportId, QueryPreloaderQuery, "Preload query " + reportId.ToString( ) );
            BulkPreloader.Preload( rq );
        }

        /// <summary>
        /// Create a query that converts a calculation into a report that returns a list of IDs.
        /// </summary>
        /// <param name="filterCalculation">The calculation script.</param>
        /// <param name="entityType">The type of resource to load (and filter).</param>
        /// <param name="exactType">True if the report should return exact types only, false to include derived types.</param>
        public static StructuredQuery BuildFilterQuery(string filterCalculation, EntityRef entityType, bool exactType)
        {
            // Validate
            if (string.IsNullOrEmpty("filterCalculation"))
                throw new ArgumentNullException("filterCalculation");
            if (entityType == null)
                throw new ArgumentNullException("entityType");

            // Compile the calculation into an expression
            BuilderSettings builderSettings = new BuilderSettings {
                ExpectedResultType = ExprType.Bool,
                RootContextType = ExprTypeHelper.EntityOfType(entityType),
                ScriptHost = ScriptHostType.Report,
                ScriptHostIsApi = true
            };
            IExpression filterCalcExpr = Factory.ExpressionCompiler.Compile(filterCalculation, builderSettings);

            // Create the structured query
            var rootEntity = new ResourceEntity {
                EntityTypeId = entityType,
                ExactType = exactType
            };            
            var query = new StructuredQuery() {
                RootEntity = rootEntity
            };

            // Generate a report condition for the filter
            QueryBuilderSettings queryBuilderSettings = new QueryBuilderSettings {
                ContextEntity = rootEntity,
                ConvertBoolsToString = false,
                StructuredQuery = query
            };
            ScalarExpression filterScalarExpr = Factory.ExpressionCompiler.CreateQueryEngineExpression( filterCalcExpr, queryBuilderSettings );
            rootEntity.Conditions = new List<ScalarExpression> { filterScalarExpr };

            return query;
        }
    }
}
