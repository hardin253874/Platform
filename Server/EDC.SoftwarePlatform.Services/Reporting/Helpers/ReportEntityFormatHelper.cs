// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using ReadiNow.Reporting.Definitions;
using Entity = EDC.ReadiNow.Model.Entity;
using ReportColumn = EDC.ReadiNow.Model.ReportColumn;

namespace ReadiNow.Reporting.Helpers
{
    internal class ReportEntityFormatHelper
    {
        internal static void ApplyColumnFormat(ReportColumn reportColumn, ReportColumnConditionalFormat format)
        {
            DisplayFormat columnDisplayFormat = reportColumn.ColumnDisplayFormat != null ? reportColumn.ColumnDisplayFormat.AsWritable<DisplayFormat>() : new DisplayFormat();
			if ( format.Rules != null && format.Rules.Count > 0 )
            {
                columnDisplayFormat.ColumnShowText = format.ShowValue;
                columnDisplayFormat.DisableDefaultFormat = format.DisableDefaultFormat;
                switch (format.Style)
                {
                    case ConditionalFormatStyleEnum.ProgressBar:
                        ApplyProgressFormat(reportColumn, format.Rules);
                        break;
                    case ConditionalFormatStyleEnum.Highlight:
                        ApplyHighlightFormat(reportColumn, format.Rules);
                        break;
                    case ConditionalFormatStyleEnum.Icon:
                        ApplyIconFormat(reportColumn, format.Rules);
                        break;
                }
                columnDisplayFormat.Save();
                reportColumn.ColumnDisplayFormat = columnDisplayFormat;
            }
            else
            {
                columnDisplayFormat.Delete();
                reportColumn.ColumnDisplayFormat = null;
            }
            reportColumn.Save();
        }

        private static void ApplyProgressFormat(ReportColumn column, IEnumerable<ReportConditionalFormatRule> rules)
        {
            if (column.ColumnFormattingRule != null)
            {
                column.ColumnFormattingRule.AsWritable().Delete();
            }
            ReportConditionalFormatRule rule = rules.First();
            decimal min;
            if (!decimal.TryParse(rule.PercentageBounds.LowerBounds.ToString(), out min))
            {
                min = 0;
            }
            int max;
            if (!int.TryParse(rule.PercentageBounds.UpperBounds.ToString(), out max))
            {
                max = 0;
            }
            BarFormattingRule formattingRule = new BarFormattingRule
                {
                    BarColor = rule.ForegroundColor != null ? rule.ForegroundColor.ToColourString() : null,
                    BarMinValue = ReportConditionHelper.ArgumentForConditionType(column.ColumnExpression.ReportExpressionResultType, rule.PercentageBounds.LowerBounds.ToString()),
                    BarMaxValue = ReportConditionHelper.ArgumentForConditionType(column.ColumnExpression.ReportExpressionResultType, rule.PercentageBounds.UpperBounds.ToString())
                };
            formattingRule.Save();
            column.ColumnFormattingRule = formattingRule.As<FormattingRule>();
        }

        private static void ApplyHighlightFormat(ReportColumn column, IEnumerable<ReportConditionalFormatRule> rules)
        {
            if (column.ColumnFormattingRule != null)
            {
                ColorFormattingRule oldRule = column.ColumnFormattingRule.AsWritable<ColorFormattingRule>();
                foreach (ColorRule colorRule in oldRule.ColorRules)
                {
                    if (colorRule.RuleCondition != null)
                    {
                        colorRule.RuleCondition.AsWritable().Delete();
                    }
                }
                oldRule.Delete();
            }
            int priority = 0;
            ColorFormattingRule formattingRule = new ColorFormattingRule();
            foreach (ReportConditionalFormatRule rule in rules)
            {
                ColorRule colorRule = new ColorRule
                    {
                        RulePriority = priority++,
                        ColorRuleBackground = rule.BackgroundColor != null ? rule.BackgroundColor.ToColourString() : null,
                        ColorRuleForeground = rule.ForegroundColor != null ? rule.ForegroundColor.ToColourString() : null,
                        RuleCondition = ReportConditionFromRule(rule, column)
                    };
                colorRule.Save();
                formattingRule.ColorRules.Add(colorRule);
            }
            formattingRule.Save();
            column.ColumnFormattingRule = formattingRule.As<FormattingRule>();
        }

        private static ReportCondition ReportConditionFromRule(ReportConditionalFormatRule rule, ReportColumn column)
        {
            ReportCondition reportCondition = new ReportCondition();
            if (rule.Operator.HasValue && rule.Operator != ConditionType.Unspecified)
            {
                string alias = "oper" + rule.Operator.ToString();
                reportCondition.Operator = Entity.Get<OperatorEnum>(new EntityRef(alias));
            }

            Parameter parameter = reportCondition.ConditionParameter != null ? reportCondition.ConditionParameter.AsWritable<Parameter>() : new Parameter();
            // Clear the old parameter
            if (parameter.ParamTypeAndDefault != null)
            {
                parameter.ParamTypeAndDefault.AsWritable().Delete();
            }
            // Force entity resource list for argument if we have entities
            ActivityArgument activityArgument = null;
			if ( rule.Values != null && rule.Values.Count > 0 )
            {
                ResourceListArgument resourceList = new ResourceListArgument();
                foreach (KeyValuePair<long, string> valuePair in rule.Values)
                {
                    Resource resource = Entity.Get<Resource>(valuePair.Key);
                    if (resource != null)
                    {
                        resourceList.ResourceListParameterValues.Add(resource);
                    }
                }
                TypedArgument argumentType = column.ColumnExpression.ReportExpressionResultType.As<TypedArgument>();
                resourceList.ConformsToType = argumentType.ConformsToType;
                activityArgument = resourceList.As<ActivityArgument>();
                activityArgument.Save();
            }
            else if (rule.Operator.HasValue)
            {
                int operatorCount = ConditionTypeHelper.GetArgumentCount(rule.Operator.Value);
                if (operatorCount > 0)
                {
                    activityArgument = ReportConditionHelper.ArgumentForConditionType(column.ColumnExpression.ReportExpressionResultType, rule.Operator.Value, rule.Value);
                    activityArgument.Save();
                }
            }
            parameter.ParamTypeAndDefault = activityArgument;
            parameter.Save();
            reportCondition.ConditionParameter = parameter;
            return reportCondition;
        }



        private static void ApplyIconFormat(ReportColumn column, IEnumerable<ReportConditionalFormatRule> rules)
        {
            if (column.ColumnFormattingRule != null)
            {
                IconFormattingRule oldRule = column.ColumnFormattingRule.AsWritable<IconFormattingRule>();
                foreach (IconRule iconRule in oldRule.IconRules)
                {
                    if (iconRule.RuleCondition != null)
                    {
                        iconRule.RuleCondition.AsWritable().Delete();
                    }
                }
                oldRule.Delete();
            }
            int priority = 0;
            //ImageEntityId
            IconFormattingRule formattingRule = new IconFormattingRule();
            foreach (ReportConditionalFormatRule rule in rules)
            {
                if (!rule.ImageEntityId.HasValue)
                {
                    continue;
                }
                IconRule iconRule = new IconRule
                    {
                        RulePriority = priority++,
                        IconRuleImage = Entity.Get<ImageFileType>(rule.ImageEntityId.Value),
                        IconRuleCFIcon = rule.CfEntityId.HasValue? Entity.Get<ConditionalFormatIcon>(rule.CfEntityId) : null,
                        RuleCondition = ReportConditionFromRule(rule, column)
                    };
                iconRule.Save();
                formattingRule.IconRules.Add(iconRule);
            }
            formattingRule.Save();
            column.ColumnFormattingRule = formattingRule.As<FormattingRule>();
        }

        internal static void ApplyColumnFormat(ReportColumn reportColumn, ReportColumnValueFormat format)
        {
            DisplayFormat columnDisplayFormat = reportColumn.ColumnDisplayFormat != null ? reportColumn.ColumnDisplayFormat.AsWritable<DisplayFormat>() : new DisplayFormat();
            columnDisplayFormat.ColumnShowText = !format.HideDisplayValue;
            columnDisplayFormat.DisableDefaultFormat = format.DisableDefaultFormat;
            columnDisplayFormat.FormatPrefix = format.Prefix;
            columnDisplayFormat.FormatSuffix = format.Suffix;
            columnDisplayFormat.FormatDecimalPlaces = format.DecimalPlaces > 0 ? Convert.ToInt32(format.DecimalPlaces) : new int?();
            if (reportColumn.ColumnExpression != null && reportColumn.ColumnExpression.ReportExpressionResultType != null)
            {
                if (reportColumn.ColumnExpression.ReportExpressionResultType.Is<DateArgument>())
                {
                    DateColFmtEnum dateColFmtEnum = Entity.Get<DateColFmtEnum>(new EntityRef(format.DateTimeFormat));
                    columnDisplayFormat.DateColumnFormat = dateColFmtEnum;
                }
                else if (reportColumn.ColumnExpression.ReportExpressionResultType.Is<TimeArgument>())
                {
                    TimeColFmtEnum timeColFmtEnum = Entity.Get<TimeColFmtEnum>(new EntityRef(format.DateTimeFormat));
                    columnDisplayFormat.TimeColumnFormat = timeColFmtEnum;
                }
                else if (reportColumn.ColumnExpression.ReportExpressionResultType.Is<DateTimeArgument>())
                {
                    DateTimeColFmtEnum dateTimeColFmtEnum = Entity.Get<DateTimeColFmtEnum>(new EntityRef(format.DateTimeFormat));
                    columnDisplayFormat.DateTimeColumnFormat = dateTimeColFmtEnum;
                }
            }
            columnDisplayFormat.MaxLineCount = format.NumberOfLines > 0 ? Convert.ToInt32(format.NumberOfLines) : new int?();
            if (format.ImageScaleId != null)
            {
                columnDisplayFormat.FormatImageScale = Entity.Get<ImageScaleEnum>(format.ImageScaleId);
            }
            if (format.ImageSizeId != null)
            {
                columnDisplayFormat.FormatImageSize = Entity.Get<ThumbnailSizeEnum>(format.ImageSizeId);
            }

            if (format.Alignment != null)
            {
                columnDisplayFormat.FormatAlignment = Entity.Get<AlignEnum>(new EntityRef(format.Alignment));
            }

            if (format.EntityListColumnFormat != null)
            {
                columnDisplayFormat.EntityListColumnFormat = Entity.Get<EntityListColFmtEnum>(new EntityRef(format.EntityListColumnFormat));
            }

            columnDisplayFormat.Save();
            reportColumn.ColumnDisplayFormat = columnDisplayFormat;
        }
    }
}
