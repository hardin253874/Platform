// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database.Types;
using Structured = EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using ReadiNow.Reporting.Result;
using EDC.Database;

namespace ReadiNow.Reporting
{
    internal class AnalyserExpressionHelper
    {
        /// <summary>
        /// Analysers the column for condition.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="reportCondition">The report condition.</param>
        /// <returns>
        /// ReportAnalyserColumn.
        /// </returns>
        internal static ReportAnalyserColumn AnalyserColumnForCondition(Report report, ReportCondition reportCondition)
        {
            if (reportCondition.ConditionIsHidden ?? false)
            {
                // Do not include analyser conditions when they are hidden
                return null;
            }
            ReportAnalyserColumn reportAnalyserColumn = new ReportAnalyserColumn
            {
                Title = reportCondition.Name,
                Ordinal = reportCondition.ConditionDisplayOrder ?? 0,
                IsConditionLocked = reportCondition.ConditionIsLocked ?? false
            };
            if (reportCondition.Operator != null && reportCondition.Operator.ToString().Length > 4)
            {
                string[] conditionOperatorParts = reportCondition.Operator.Alias.Split(':');
                string operatorString = conditionOperatorParts.Length == 2 ? conditionOperatorParts[1].Substring(4) : conditionOperatorParts[0].Substring(4);
                Structured.ConditionType conditionType;
                reportAnalyserColumn.Operator = Enum.TryParse(operatorString, true, out conditionType) ? conditionType : Structured.ConditionType.Unspecified;
            }
            else
            {
                reportAnalyserColumn.Operator = Structured.ConditionType.Unspecified;
            }
            if (reportCondition.ColumnForCondition != null)
            {
                reportAnalyserColumn.ReportColumnId = reportCondition.ColumnForCondition.Id;
            }

            if (reportCondition.ConditionExpression == null)
            {
                return null;
            }

            // Get the argument
            ActivityArgument reportExpressionResultType = reportCondition.ConditionExpression.Is<ColumnReferenceExpression>() ?
                                                              reportCondition.ConditionExpression.As<ColumnReferenceExpression>().ExpressionReferencesColumn.ColumnExpression.ReportExpressionResultType :
                                                              reportCondition.ConditionExpression.ReportExpressionResultType;
            // Process the expression result associated to this condition
            DatabaseType type;  // scalar type of analyser expression
            EntityType resourceType;  // resource type of analyser expression

            if (reportExpressionResultType == null || !PopulateTypeFromArgument(reportExpressionResultType, out type, out resourceType ) )
            {
                return null;
            }
            reportAnalyserColumn.Type = type;
            if ( resourceType != null )
            {
                reportAnalyserColumn.TypeId = resourceType.Id;
            }
            reportAnalyserColumn.DefaultOperator = DefaultOperatorForType( reportExpressionResultType );

            // Get the column expression for the analysable field. If this is a referenced column then use it's referenced column's expression.
            ReportExpression reportExpression = reportCondition.ConditionExpression.Is<ColumnReferenceExpression>() ?
                                                    reportCondition.ConditionExpression.As<ColumnReferenceExpression>().ExpressionReferencesColumn.ColumnExpression : reportCondition.ConditionExpression;

            // Track current user for a 'core:UserAccount' or any derivatives and 'core:Person' or any derivatives and if the source node
            // is one of these and the report expression's field type is 'core:name' then fudge the analyser type to be 'UserString' which includes all
            // string analyser operator definitions in addition to the 'Current User' operator.
            PopulateAnalyserTypeForColumn(report, resourceType, reportExpression, reportAnalyserColumn);

            // Process any values associated to this condition.
            if (reportCondition.ConditionParameter != null && reportCondition.ConditionParameter.ParamTypeAndDefault != null)
            {
                ActivityArgument activityArgument = reportCondition.ConditionParameter.ParamTypeAndDefault;
                PopulateValueFromArgument(activityArgument, reportAnalyserColumn);
            }

            // resource picker for condition parameter
            if (reportCondition.ConditionParameterPicker != null)
            {
                reportAnalyserColumn.ConditionParameterPickerId = reportCondition.ConditionParameterPicker.Id;
            }

            return reportAnalyserColumn;
        }

        /// <summary>
        /// Populates the analyser type for column.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="entityType">The resource type returned if the analyser refers to a resource expression.</param>
        /// <param name="reportExpression">The report expression.</param>
        /// <param name="reportAnalyserColumn">The report analyser column.</param>
        private static void PopulateAnalyserTypeForColumn(Report report, EntityType entityType, ReportExpression reportExpression, ReportAnalyserColumn reportAnalyserColumn)
        {
            reportAnalyserColumn.AnalyserType = reportAnalyserColumn.Type.GetDisplayName();

            if (reportExpression.Is<StructureViewExpression>())
            {
                reportAnalyserColumn.AnalyserType = StructureLevelsType.DisplayName;
                reportAnalyserColumn.DefaultOperator = Structured.ConditionType.AnyBelowStructureLevel;
                var expression = reportExpression.As<StructureViewExpression>();
                var resReportNode = expression.StructureViewExpressionSourceNode.As<ResourceReportNode>();
                var eType = resReportNode.ResourceReportNodeType;
                reportAnalyserColumn.TypeId = eType.Id;
                return;
            }

            if (!reportExpression.Is<NodeExpression>())
            {
                return;
            }

            NodeExpression nodeExpression = reportExpression.As<NodeExpression>();
            if (!nodeExpression.SourceNode.Is<ResourceReportNode>())
            {
                return;
            }

            bool isNameColumnForType = false;
            if (reportExpression.Is<FieldExpression>())
            {
                var fieldExpression = reportExpression.As<FieldExpression>();
                if (fieldExpression.FieldExpressionField.Alias != "core:name")
                {
                    return;
                }
                long sourceId = fieldExpression.SourceNode != null ? fieldExpression.SourceNode.Id : 0;
                long rootId = report.RootNode != null ? report.RootNode.Id : 0;
                isNameColumnForType = (sourceId == rootId) && (sourceId != 0);
            }
            reportAnalyserColumn.IsNameColumnForType = isNameColumnForType;

            ResourceReportNode resourceReportNode = nodeExpression.SourceNode.As<ResourceReportNode>( );
            RelationshipReportNode relationshipReportNode = nodeExpression.SourceNode.As<RelationshipReportNode>( );

            if ( entityType == null)
            {
                // Need to be able accept entityType as an argument, e.g. if it is from a script column
                // But also need to be able to read it from the node, e.g. if it is the root name column. Messed up.
                entityType = resourceReportNode.ResourceReportNodeType;
                if ( entityType == null )
                {
                    return;
                }
            }

            ResourceExpression resourceExpression = reportExpression.As<ResourceExpression>();

            // Handle "Type" types
            //if the resource type is "Type", add current parent node type and descendant types' id as filtered entity ids (bug 24859)
            //Update: only the forward relationship is "isOfType", the "type" list will be restricted. (bug 27862)
            if ( entityType.Alias == "core:type" && 
                relationshipReportNode?.FollowRelationship?.Alias == "core:isOfType"
                )
            {                
                AggregateReportNode parentAggregatedNode = resourceReportNode.ParentAggregatedNode;
                ReportNode parentReportNode = parentAggregatedNode != null ? parentAggregatedNode.GroupedNode : resourceReportNode.ParentReportNode;
                ResourceReportNode parentResourceReportNode = parentReportNode != null ? parentReportNode.As<ResourceReportNode>() : null;

                if (parentResourceReportNode != null && parentResourceReportNode.ResourceReportNodeType != null)
                {
                    reportAnalyserColumn.FilteredEntityIds = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(
                        parentResourceReportNode.ResourceReportNodeType.Id).ToArray();
                }
            }

            // Handle "User" and "Person" types
            if ( PerTenantEntityTypeCache.Instance.IsDerivedFrom( entityType.Id, "core:person" )
                || PerTenantEntityTypeCache.Instance.IsDerivedFrom( entityType.Id, "core:userAccount" ) )
            {
                // If this is a relationship or calc then make it as a user inline relationship otherwise a simple user string.
                reportAnalyserColumn.AnalyserType = nodeExpression.SourceNode.Is<FieldExpression>( ) ? "UserString" : "UserInlineRelationship";
                return;
            }

            // Treat the root 'Name' column like a lookup, so we get the 'Any Of', 'Any Except' options.
            if ( isNameColumnForType )
            {
                reportAnalyserColumn.AnalyserType = "InlineRelationship";
                reportAnalyserColumn.TypeId = entityType.Id;
            }
        }

        /// <summary>
        /// Interrogates the expression instance populating the report column that is exposed to the Report API service.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="reportColumn">The report column.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool PopulateTypeFromArgument(ActivityArgument argument, out DatabaseType type, out EntityType resourceType)
        {
            resourceType = null;

            if (argument.Is<StringArgument>())
            {
                type = new StringType();
            }
            else if (argument.Is<IntegerArgument>())
            {
                type = new Int32Type();
            }
            else if (argument.Is<CurrencyArgument>())
            {
                type = new CurrencyType();
            }
            else if (argument.Is<DecimalArgument>())
            {
                type = new DecimalType();
            }
            else if (argument.Is<DateArgument>())
            {
                type = new DateType();
            }
            else if (argument.Is<TimeArgument>())
            {
                type = new TimeType();
            }
            else if (argument.Is<DateTimeArgument>())
            {
                type = new DateTimeType();
            }
            else if (argument.Is<GuidArgument>())
            {
                type = new GuidType();
            }
            else if (argument.Is<BoolArgument>())
            {
                type = new BoolType();
            }
            else if (argument.Is<TypedArgument>())
            {
                TypedArgument rla = argument.As<TypedArgument>();
                resourceType = Entity.Get<EntityType>(rla.ConformsToType);
                if ( resourceType == null)
                {
                    type = null;
                    return false;
                }
				if ( resourceType.IsOfType.FirstOrDefault(t => t.Alias == "core:enumType") != null)
				{
				    // A choice field
                    type = new ChoiceRelationshipType();
                }
				else
				{
				    // Is a related resource
                    type = new InlineRelationshipType();
                }
            }
            else
            {
                type = null;
                return false;
            }
            return true;
        }

        private static void PopulateValueFromArgument(ActivityArgument argument, ReportAnalyserColumn reportColumn)
        {
            if (argument.Is<StringArgument>())
            {
                StringArgument stringArgument = argument.As<StringArgument>();
                reportColumn.Value = stringArgument.StringParameterValue;
            }
            else if (argument.Is<IntegerArgument>())
            {
                IntegerArgument integerArgument  = argument.As<IntegerArgument>();
                if (integerArgument.IntParameterValue != null)
                {
                    reportColumn.Value = integerArgument.IntParameterValue.ToString();
                }
            }
            else if (argument.Is<CurrencyArgument>())
            {
                CurrencyArgument currencyArgument  = argument.As<CurrencyArgument>();
                if (currencyArgument.DecimalParameterValue != null)
                {
                    reportColumn.Value = currencyArgument.DecimalParameterValue.ToString();
                }
            }
            else if (argument.Is<DecimalArgument>())
            {
                DecimalArgument decimalArgument  = argument.As<DecimalArgument>();
                if (decimalArgument.DecimalParameterValue != null)
                {
                    reportColumn.Value = decimalArgument.DecimalParameterValue.ToString();
                }
            }
            else if (argument.Is<DateArgument>())
            {
                DateArgument  dateArgument = argument.As<DateArgument>();
                
                if (dateArgument.DateParameterValue != null)
                {
                    //convert the date value to YYYY-MM-DD format
                    DateTime dateValue = (DateTime)dateArgument.DateParameterValue;
                    reportColumn.Value = dateValue.ToString("yyyy-MM-dd");
                }
            }
            else if (argument.Is<TimeArgument>())
            {
                TimeArgument timeArgument = argument.As<TimeArgument>();                
                if (timeArgument.TimeParameterValue != null)
                {
                    //convert the time value to YYYY-MM-DDTHH:mm:ssZ format
                    DateTime timeValue = (DateTime)timeArgument.TimeParameterValue;
                    reportColumn.Value = timeValue.ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
            }
            else if (argument.Is<DateTimeArgument>())
            {
                DateTimeArgument dateTimeArgument  = argument.As<DateTimeArgument>();
                if (dateTimeArgument.DateTimeParameterValue != null)
                {
                    //convert the datetime value to YYYY-MM-DDTHH:mm:ssZ format
                    DateTime dateTimeValue = (DateTime)dateTimeArgument.DateTimeParameterValue;
                    reportColumn.Value = dateTimeValue.ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
            }
            else if (argument.Is<GuidArgument>())
            {
                GuidArgument guidArgument  = argument.As<GuidArgument>();
                if (guidArgument.GuidParameterValue != null)
                {
                    reportColumn.Value = guidArgument.GuidParameterValue.ToString();
                }
            }
            else if (argument.Is<BoolArgument>())
            {
                BoolArgument boolArgument  = argument.As<BoolArgument>();
                if (boolArgument.BoolParameterValue != null)
                {
                    reportColumn.Value = boolArgument.BoolParameterValue.ToString();
                }
            }
            else if (argument.Is<TypedArgument>())
            {
                TypedArgument typedArgument = argument.As<TypedArgument>();
                EntityType type = Entity.Get<EntityType>(typedArgument.ConformsToType);
                if (type.IsOfType.FirstOrDefault(t => t.Alias == "core:enumType") != null)
                {
                    // A choice field
                    reportColumn.Type = new ChoiceRelationshipType();
                }
                else
                {
                    // Is a related resource
                    reportColumn.Type = new InlineRelationshipType();
                }
                ResourceListArgument rla = argument.As<ResourceListArgument>();
				if ( rla.ResourceListParameterValues != null && rla.ResourceListParameterValues.Count > 0 )
                {
                    Dictionary<long,string> values = new Dictionary<long, string>();
                    foreach (Resource resourceListParameterValue in rla.ResourceListParameterValues)
                    {
                        values[resourceListParameterValue.Id] = resourceListParameterValue.Name;
                    }
                    reportColumn.Values = values;
                }
            }
        }

        /// <summary>
        /// Defaults the type of the operator for.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <returns>ConditionType.</returns>
        private static Structured.ConditionType DefaultOperatorForType(ActivityArgument dataType)
        {
            Structured.ConditionType conditionType = Structured.ConditionType.Contains;
            if (dataType.Is<BoolArgument>())
            {
                conditionType = Structured.ConditionType.Unspecified;
            }
            else if (dataType.Is<DateArgument>())
            {
                conditionType = Structured.ConditionType.GreaterThan;
            }
            else if (dataType.Is<DateTimeArgument>())
            {
                conditionType = Structured.ConditionType.GreaterThan;
            }
            else if (dataType.Is<DecimalArgument>())
            {
                conditionType = Structured.ConditionType.GreaterThan;
            }
            else if (dataType.Is<IntegerArgument>())
            {
                conditionType = Structured.ConditionType.GreaterThan;
            }
            else if (dataType.Is<CurrencyArgument>())
            {
                conditionType = Structured.ConditionType.GreaterThan;
            }
            else if (dataType.Is<StringArgument>())
            {
                conditionType = Structured.ConditionType.Contains;
            }
            else if (dataType.Is<TimeArgument>())
            {
                conditionType = Structured.ConditionType.GreaterThan;
            }
            else if (dataType.Is<TypedArgument>())
            {
                conditionType = Structured.ConditionType.AnyOf;
            }
            return conditionType;
        }

    }
}
