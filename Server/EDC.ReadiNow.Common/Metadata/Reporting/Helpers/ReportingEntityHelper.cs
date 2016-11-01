// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using EDC.Database;
using EDC.ReadiNow.Metadata.Media;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Metadata.Reporting.Formatting;
using EDC.ReadiNow.Model;
using ColorRule = EDC.ReadiNow.Metadata.Reporting.Formatting.ColorRule;
using IconRule = EDC.ReadiNow.Metadata.Reporting.Formatting.IconRule;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Metadata.Reporting.Helpers
{
    public class ReportingEntityHelper
    {

		/// <summary>
		/// Builds the column format rules for the report columns.
		/// </summary>
		/// <param name="reportColumns">The report columns.</param>
		/// <param name="context">The context.</param>
		/// <returns>
		/// List{ColumnFormatting}.
		/// </returns>
        internal static List<ColumnFormatting> BuildColumnFormats(IEntityCollection<ReportColumn> reportColumns, IReportToQueryContext context)
        {
            List<ColumnFormatting> reportColumnFormats = new List<ColumnFormatting>();

            foreach (ReportColumn reportColumn in reportColumns)
            {
                ColumnFormatting columnFormatting = null;
                if (reportColumn.ColumnDisplayFormat != null && reportColumn.ColumnExpression != null)
                {
                    Formatting.ImageFormattingRule imageFormattingRule = null;
                    if (reportColumn.ColumnDisplayFormat.FormatImageScale != null &&
                        reportColumn.ColumnDisplayFormat.FormatImageSize != null)
                    {

                        imageFormattingRule = new Formatting.ImageFormattingRule
                            {
                                ThumbnailSizeId = new Model.EntityRef(reportColumn.ColumnDisplayFormat.FormatImageSize.Id),
                                ThumbnailScaleId = new Model.EntityRef(reportColumn.ColumnDisplayFormat.FormatImageScale.Id),
                            };
                    }


                    columnFormatting = new ColumnFormatting
                        {
                            EntityId = reportColumn.Id, 
                            ColumnName = reportColumn.Name, 
                            DecimalPlaces = reportColumn.ColumnDisplayFormat.FormatDecimalPlaces ?? 0, 
                            ShowText = reportColumn.ColumnDisplayFormat.ColumnShowText ?? false, 
                            Prefix = reportColumn.ColumnDisplayFormat.FormatPrefix, 
                            Suffix = reportColumn.ColumnDisplayFormat.FormatSuffix, 
                            Lines = reportColumn.ColumnDisplayFormat.MaxLineCount ?? 0,
                            FormattingRule = imageFormattingRule ?? null
                        };
                    // Column Type
                    columnFormatting.ColumnType = TypedValueHelper.GetDatabaseType(reportColumn.ColumnExpression.ReportExpressionResultType);
                    
                    DisplayFormat displayFormat = reportColumn.ColumnDisplayFormat;
                    // Format String
                    if (displayFormat.DateColumnFormat != null)
                    {
                        columnFormatting.DateFormat = displayFormat.DateColumnFormat;
                    }
                    if (displayFormat.DateTimeColumnFormat != null)
                    {
                        columnFormatting.DateTimeFormat = displayFormat.DateTimeColumnFormat;
                    }
                    if (displayFormat.TimeColumnFormat != null)
                    {
                        columnFormatting.TimeFormat = displayFormat.TimeColumnFormat;
                    }

                    // Text Alignment
                    if (displayFormat.FormatAlignment != null)
                    {
                        columnFormatting.Alignment = displayFormat.FormatAlignment;
                    }

                }
                // Formatting Rule
                if (reportColumn.ColumnFormattingRule != null && reportColumn.ColumnExpression != null)
                {
                    if (columnFormatting == null)
                        columnFormatting = new ColumnFormatting
                        {
                            EntityId = reportColumn.Id,
                            ColumnName = reportColumn.Name,
                        };
                    columnFormatting.FormattingRule = BuildColumnFormatRule(reportColumn.ColumnFormattingRule, columnFormatting.ColumnType, context);
                }

                if (columnFormatting != null && reportColumn.ColumnExpression != null)
                {
                    reportColumnFormats.Add(columnFormatting);
                }
            }

            return reportColumnFormats;
        }

        private static Formatting.FormattingRule BuildColumnFormatRule(Model.FormattingRule formattingRule, DatabaseType columnType, IReportToQueryContext context)
        {
            var converter = Factory.Current.Resolve<IReportToQueryPartsConverter>( );

            if (formattingRule.Is<Model.BarFormattingRule>())
            {
                Model.BarFormattingRule barRule = formattingRule.As<Model.BarFormattingRule>();
                Formatting.BarFormattingRule barFormattingRule = new Formatting.BarFormattingRule()
                    {
                        Color = ColourFromString(barRule.BarColor)
                    };
                List<TypedValue> value = TypedValueHelper.TypedValueFromEntity(barRule.BarMinValue);
				if ( value != null && value.Count > 0 )
                {
                    barFormattingRule.Minimum = value[0];
                }
                value = TypedValueHelper.TypedValueFromEntity(barRule.BarMaxValue);
				if ( value != null && value.Count > 0 )
                {
                    barFormattingRule.Maximum = value[0];
                }
                return barFormattingRule;
            }
            if (formattingRule.Is<Model.ColorFormattingRule>())
            {
                Model.ColorFormattingRule modelColourRule = formattingRule.As<Model.ColorFormattingRule>();
				List<ColorRule> colourRules = modelColourRule.ColorRules.Count > 0 ?
                                                             modelColourRule.ColorRules.OrderBy(rule => rule.RulePriority ?? 0).Select(colorRule => new ColorRule
                                                                 {
                                                                     BackgroundColor = ColourFromString(colorRule.ColorRuleBackground),
                                                                     ForegroundColor = ColourFromString(colorRule.ColorRuleForeground),
                                                                     Condition = converter.ConvertCondition( colorRule.RuleCondition, columnType, context )
                                                                 }).ToList()
                                                             : null;
                Formatting.ColorFormattingRule colourFormattingRule = new Formatting.ColorFormattingRule
                    {
                        Rules = colourRules
                    };
                return colourFormattingRule;
            }
            if (formattingRule.Is<Model.IconFormattingRule>())
            {
                Model.IconFormattingRule modelIconFormattingRule = formattingRule.As<Model.IconFormattingRule>();
                return new Formatting.IconFormattingRule
                    {
                        Rules = modelIconFormattingRule.IconRules.OrderBy(rule => rule.RulePriority ?? 0).Select(rule => new Formatting.IconRule
                            {
                                CfEntityId = rule.IconRuleCFIcon != null ? rule.IconRuleCFIcon.Id: -1,
                                IconId = GetIconId(rule),
                                Condition = converter.ConvertCondition( rule.RuleCondition, columnType, context )
                            }).ToList()
                    };
            }
            return null;
        }

        private static long GetIconId(Model.IconRule rule)
        {
            long iconId = -1;
            if (rule.IconRuleCFIcon != null && rule.IconRuleCFIcon.CondFormatImage != null)
            {
                iconId = rule.IconRuleCFIcon.CondFormatImage.Id;
            }
            else if (rule.IconRuleImage != null)
            {
                iconId = rule.IconRuleImage.Id;
            }
            return iconId;
        }

        private static ColorInfo ColourFromString(string colourHexString)
        {
            try
            {
                UInt32 barColour = Convert.ToUInt32(colourHexString, 16);
                ColorInfo colourInfo = new ColorInfo
                    {
                        A = (byte)((barColour & 0xff000000) >> 24),
                        R = (byte)((barColour & 0x00ff0000) >> 16),
                        G = (byte)((barColour & 0x0000ff00) >> 8),
                        B = (byte)(barColour & 0x000000ff)
                    };
                return colourInfo;
            }
            catch (Exception)
            {
                // Failed to format, just leave it as null
                throw;
            }
        }
    }
}
