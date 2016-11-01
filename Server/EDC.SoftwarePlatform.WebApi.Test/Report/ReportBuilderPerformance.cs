// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using EDC.ReadiNow.Test;
using EDC.Security;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using EDC.ReadiNow.Expressions;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.WebApi.Test.Report
{
    /// <summary>
    /// </summary>
    [TestFixture]
    public class ReportBuilderPerformance
    {
        /// <summary>
        ///     Test creating a type and form, and verifying cache invalidation occurred on definition.
        /// </summary>
        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void RunReportBuilderEntityRequest()
        {
            if (_reportId == 0)
                _reportId = EDC.ReadiNow.Expressions.CodeNameResolver.GetInstance(_reportName, "Report").Id;

            // Request all definitions and enum types
            string id = _reportId.ToString();
            string rq = "{name,description, alias, isOfType.{name,alias}, hideActionBar, hideReportHeader , rollupSubTotals,rollupGrandTotals, rollupRowCounts, rollupRowLabels, rollupOptionLabels, {rootNode,rootNode.relatedReportNodes*,rootNode.groupedNode*}.{name,isOfType.{name,alias},exactType,targetMustExist,targetNeedNotExist,resourceReportNodeType.{name,isOfType.{name,alias}},followInReverse,followRecursive,includeSelfInRecursive,constrainParent,checkExistenceOnly,followRelationship.{name, cardinality.alias, fromName, toName,{fromType, toType}.{name,alias,inherits.alias}},groupedBy.{isOfType.{name,alias},reportExpressionResultType.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    },sourceNode.id,fieldExpressionField.{name,alias},reportScript,expressionReferencesColumn.id,aggregateMethod.alias,aggregatedExpression*.{    isOfType.alias,    reportExpressionResultType.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    },    sourceNode.id,    fieldExpressionField.id,    reportScript,    expressionReferencesColumn.id,    aggregateMethod.{id,name,alias,isOfType.{name,alias}}}}}, reportColumns.{name,isOfType.{name,alias},columnDisplayOrder,columnIsHidden,columnExpression.{isOfType.{name,alias},reportExpressionResultType.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    },sourceNode.id,fieldExpressionField.{name,alias},reportScript,expressionReferencesColumn.id,aggregateMethod.alias,aggregatedExpression*.{    isOfType.alias,    reportExpressionResultType.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    },    sourceNode.id,    fieldExpressionField.id,    reportScript,    expressionReferencesColumn.id,    aggregateMethod.{id,name,alias,isOfType.{name,alias}}}} , columnFormattingRule.{isOfType.{name,alias},barColor,{barMinValue, barMaxValue}.{ intParameterValue, decimalParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue },iconRules.iconRuleImage.{isOfType.{name,alias},name},colorRules.{isOfType.{name,alias},colorRuleForeground,colorRuleBackground},{iconRules, colorRules}.{isOfType.{name,alias},rulePriority,ruleCondition. {name,isOfType.{name,alias},conditionDisplayOrder,conditionIsHidden,conditionIsLocked,conditionExpression.{isOfType.{name,alias},reportExpressionResultType.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    },sourceNode.id,fieldExpressionField.{name,alias},reportScript,expressionReferencesColumn.id,aggregateMethod.alias,aggregatedExpression*.{    isOfType.alias,    reportExpressionResultType.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    },    sourceNode.id,    fieldExpressionField.id,    reportScript,    expressionReferencesColumn.id,    aggregateMethod.{id,name,alias,isOfType.{name,alias}}}}, operator.{isOfType.alias,alias},columnForCondition.{name,alias},conditionParameter.{isOfType.{name,alias},paramTypeAndDefault.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    }}}}}, columnDisplayFormat.{isOfType.{name,alias},columnShowText, formatImageScale.alias,dateColumnFormat.alias,timeColumnFormat.alias,dateTimeColumnFormat.alias,formatAlignment.{name,alias, isOfType.{name,alias}},formatDecimalPlaces,formatPrefix,formatSuffix,maxLineCount,formatAlignment.alias,formatImageSize.alias}, columnGrouping.{isOfType.{name,alias},groupingPriority,groupingMethod.{name,alias}}, columnRollup.{isOfType.{name,alias},rollupMethod.{name,alias}}} , reportOrderBys.{isOfType.{name,alias},reverseOrder,orderPriority,orderByExpression.{isOfType.{name,alias},reportExpressionResultType.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    },sourceNode.id,fieldExpressionField.{name,alias},reportScript,expressionReferencesColumn.id,aggregateMethod.alias,aggregatedExpression*.{    isOfType.alias,    reportExpressionResultType.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    },    sourceNode.id,    fieldExpressionField.id,    reportScript,    expressionReferencesColumn.id,    aggregateMethod.{id,name,alias,isOfType.{name,alias}}}}}, hasConditions.{name,isOfType.{name,alias},conditionDisplayOrder,conditionIsHidden,conditionIsLocked,conditionExpression.{isOfType.{name,alias},reportExpressionResultType.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    },sourceNode.id,fieldExpressionField.{name,alias},reportScript,expressionReferencesColumn.id,aggregateMethod.alias,aggregatedExpression*.{    isOfType.alias,    reportExpressionResultType.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    },    sourceNode.id,    fieldExpressionField.id,    reportScript,    expressionReferencesColumn.id,    aggregateMethod.{id,name,alias,isOfType.{name,alias}}}}, operator.{isOfType.alias,alias},columnForCondition.{name,alias},conditionParameter.{isOfType.{name,alias},paramTypeAndDefault.{       name,alias,       isOfType.{name,alias},       numberDecimalPlaces,       conformsToType.{name,isOfType.{name,alias}},       resourceArgumentValue.{name,alias},       resourceListParameterValues.{name,alias},       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue    }}}, k:resourceInFolder.{name, isOfType.{name,alias}} , reportUsesDefinition.{name, isOfType.{name,alias}}, isDefaultDisplayReportForTypes.{name, isOfType.{name,alias}}, isDefaultPickerReportForTypes.{name, isOfType.{name,alias}}, k:navigationElementIcon.{name, isOfType.{name,alias}},hideOnDesktop,hideOnTablet,hideOnMobile, inSolution.{name, isOfType.{name,alias}} , reportStyle.{name, isOfType.{name,alias}} , { k:resourceConsoleBehavior, k:selectionBehavior }.k:behaviorActionMenu.{    k:showNewActionsButton, k:showExportActionsButton, k:showEditInlineActionsButton,   { k:menuItems, k:suppressedActions, k:includeActionsAsButtons }.{       { name,         k:menuIconUrl,         k:isActionButton,         k:isMenuSeparator,         { k:actionMenuItemToWorkflow }.{ name },         { k:actionMenuItemToReportTemplate }.{ name }       }    },    { k:includeTypesForNewButtons, k:suppressedTypesForNewMenu }.id } , resourceViewerConsoleForm.{name, alias, description, isOfType.{name,alias}}}";
            string query = @"{'queries':['" + rq + @"'],'requests':[{'get':'basic','ids':[" + id + "],'hint':'unit test','rq':0}]}";            
            query = query.Replace("'", "\"");
            JsonQueryResult res = TestEntity.RunBatchTest(query, HttpStatusCode.OK, 1);
            Assert.That(res, Is.Not.Null);
        }

        /// <summary>
        ///     Test creating a type and form, and verifying cache invalidation occurred on definition.
        /// </summary>
        [Test]
        [Explicit]
        [RunAsDefaultTenant]
        public void RunReportBuilderEntityRequest_WithVariables()
        {
            if (_reportId == 0)
                _reportId = EDC.ReadiNow.Expressions.CodeNameResolver.GetInstance(_reportName, "Report").Id;

            // Request all definitions and enum types
            string id = _reportId.ToString();
            string rq = @"let @REPORTEXPRESULTTYPE = {
       name,alias,
       isOfType.{name,alias},
       numberDecimalPlaces,
       conformsToType.{name,isOfType.{name,alias}},
       resourceArgumentValue.{name,alias},
       resourceListParameterValues.{name,alias},
       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue
    }
let @EXPR = {isOfType.{name,alias},reportExpressionResultType.@REPORTEXPRESULTTYPE,sourceNode.id,fieldExpressionField.{name,alias},reportScript,expressionReferencesColumn.id,aggregateMethod.alias,aggregatedExpression*.{
    isOfType.alias,
    reportExpressionResultType.@REPORTEXPRESULTTYPE,
    sourceNode.id,
    fieldExpressionField.id,
    reportScript,
    expressionReferencesColumn.id,
    aggregateMethod.{id,name,alias,isOfType.{name,alias}}}}
let @CONDITION = {name,isOfType.{name,alias},conditionDisplayOrder,conditionIsHidden,conditionIsLocked,conditionExpression.@EXPR,
    operator.{isOfType.alias,alias},
    columnForCondition.{name,alias},
    conditionParameter.{isOfType.{name,alias},
    paramTypeAndDefault.@REPORTEXPRESULTTYPE}}

{name,description, alias, isOfType.{name,alias}, hideActionBar, hideReportHeader , rollupSubTotals,rollupGrandTotals, rollupRowCounts, rollupRowLabels, rollupOptionLabels, {rootNode,rootNode.relatedReportNodes*,rootNode.groupedNode*}.{name,isOfType.{name,alias},exactType,targetMustExist,targetNeedNotExist,resourceReportNodeType.{name,isOfType.{name,alias}},followInReverse,followRecursive,includeSelfInRecursive,constrainParent,checkExistenceOnly,followRelationship.{name, cardinality.alias, fromName, toName,{fromType, toType}.{name,alias,inherits.alias}},groupedBy.@EXPR}, reportColumns.{name,isOfType.{name,alias},columnDisplayOrder,columnIsHidden,columnExpression.@EXPR , columnFormattingRule.{isOfType.{name,alias},barColor,{barMinValue, barMaxValue}.{ intParameterValue, decimalParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue },iconRules.iconRuleImage.{isOfType.{name,alias},name},colorRules.{isOfType.{name,alias},colorRuleForeground,colorRuleBackground},{iconRules, colorRules}.{isOfType.{name,alias},rulePriority,ruleCondition.@CONDITION}}, columnDisplayFormat.{isOfType.{name,alias},columnShowText, formatImageScale.alias,dateColumnFormat.alias,timeColumnFormat.alias,dateTimeColumnFormat.alias,formatAlignment.{name,alias, isOfType.{name,alias}},formatDecimalPlaces,formatPrefix,formatSuffix,maxLineCount,formatAlignment.alias,formatImageSize.alias}, columnGrouping.{isOfType.{name,alias},groupingPriority,groupingMethod.{name,alias}}, columnRollup.{isOfType.{name,alias},rollupMethod.{name,alias}}} , reportOrderBys.{isOfType.{name,alias},reverseOrder,orderPriority,orderByExpression.@EXPR }, hasConditions.@CONDITION, k:resourceInFolder.{name, isOfType.{name,alias}} , reportUsesDefinition.{name, isOfType.{name,alias}}, isDefaultDisplayReportForTypes.{name, isOfType.{name,alias}}, isDefaultPickerReportForTypes.{name, isOfType.{name,alias}}, k:navigationElementIcon.{name, isOfType.{name,alias}},hideOnDesktop,hideOnTablet,hideOnMobile, inSolution.{name, isOfType.{name,alias}} , reportStyle.{name, isOfType.{name,alias}} , let @ACTIONHOST = { k:resourceConsoleBehavior, k:selectionBehavior }.k:behaviorActionMenu.{
    k:showNewActionsButton, k:showExportActionsButton, k:showEditInlineActionsButton,
   { k:menuItems, k:suppressedActions, k:includeActionsAsButtons }.{
       { name,
         k:menuIconUrl,
         k:isActionButton,
         k:isMenuSeparator,
         { k:actionMenuItemToWorkflow }.{ name },
         { k:actionMenuItemToReportTemplate }.{ name }
       }
    },
    { k:includeTypesForNewButtons, k:suppressedTypesForNewMenu }.id } , resourceViewerConsoleForm.{name, alias, description, isOfType.{name,alias}}}";
            string query = @"{'queries':['" + rq + @"'],'requests':[{'get':'basic','ids':[" + id + "],'hint':'unit test','rq':0}]}";
            query = query.Replace("'", "\"");
            JsonQueryResult res = TestEntity.RunBatchTest(query, HttpStatusCode.OK, 1);
            Assert.That(res, Is.Not.Null);
        }

        static long _reportId;
        static string _reportName = "AF_All Fields";

    }
}