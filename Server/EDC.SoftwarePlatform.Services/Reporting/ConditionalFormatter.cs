// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Data;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Reporting.Formatting;
using EDC.ReadiNow.Model;
using ReadiNow.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Entity = EDC.ReadiNow.Model.Entity;
using BarFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.BarFormattingRule;
using ImageFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.ImageFormattingRule;
using ColorFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.ColorFormattingRule;
using IconFormattingRule = EDC.ReadiNow.Metadata.Reporting.Formatting.IconFormattingRule;
using IconRule = EDC.ReadiNow.Metadata.Reporting.Formatting.IconRule;
using ColorRule = EDC.ReadiNow.Metadata.Reporting.Formatting.ColorRule;
using StructureViewExpression = EDC.ReadiNow.Metadata.Query.Structured.StructureViewExpression;
using ReadiNow.Reporting.Definitions;
using ReadiNow.Reporting.Result;

namespace ReadiNow.Reporting
{
    /// <summary>
    /// Class ConditionalFormatter.
    /// </summary>
    class ConditionalFormatter
    {
        private readonly ReportResult _reportResult;
        private readonly Dictionary<string, List<ConditionInfo>> _formatRules;
        // A cache of structure levels for a given structure view.
        private readonly Dictionary<long, Dictionary<long, StructureViewLevelsValue>> _structureViewLevelsCache = new Dictionary<long, Dictionary<long, StructureViewLevelsValue>>();        

        /// <summary>
        /// Gets a value indicating whether this instance has rules.
        /// </summary>
        /// <value><c>true</c> if this instance has rules; otherwise, <c>false</c>.</value>
        public bool HasRules
        {
			get
			{
				return (_formatRules != null && _formatRules.Count > 0 );
			}
        }

        /// <summary>
        /// Builds a list of conditional format rules for the report.
        /// </summary>
        /// <returns>Dictionary{GuidList{ReportConditionalFormatRule}}.</returns>
        public Dictionary<string, ReportColumnConditionalFormat> FormatsForReport(out Dictionary<string, ReportImageScale> imageScaleRules)
        {
            imageScaleRules = null;
			if ( _formatRules != null && _formatRules.Count > 0 )
            {
                Dictionary<string, ReportColumnConditionalFormat> formats = new Dictionary<string, ReportColumnConditionalFormat>(_formatRules.Count);
                EntityRef imageScale = null;
                EntityRef imageSize = null;
                ThumbnailSizeEnum thumbnailSizeEnum = null;
                foreach (KeyValuePair<string, List<ConditionInfo>> formatRule in _formatRules)
                {
                    ReportColumnConditionalFormat columnFormat = new ReportColumnConditionalFormat();

                    List<ReportConditionalFormatRule> rules = new List<ReportConditionalFormatRule>();
                    foreach (ConditionInfo conditionInfo in formatRule.Value)
                    {
						if ( rules.Count <= 0 )
                        {
                            columnFormat.Style = conditionInfo.Style;
                            columnFormat.ShowValue = conditionInfo.ShowText;
                        }
                        ReportConditionalFormatRule rule = new ReportConditionalFormatRule
                            {
                                Operator = conditionInfo.Operator,
                            };
                        if (conditionInfo.ColorRule != null)
                        {
                            if (conditionInfo.ColorRule.ForegroundColor != null)
                            {
                                rule.ForegroundColor = new ReportConditionColor
                                    {
                                        Alpha = conditionInfo.ColorRule.ForegroundColor.A,
                                        Blue = conditionInfo.ColorRule.ForegroundColor.B,
                                        Green = conditionInfo.ColorRule.ForegroundColor.G,
                                        Red = conditionInfo.ColorRule.ForegroundColor.R
                                    };
                            }
                            if (conditionInfo.ColorRule.BackgroundColor != null)
                            {
                                rule.BackgroundColor = new ReportConditionColor
                                    {
                                        Alpha = conditionInfo.ColorRule.BackgroundColor.A,
                                        Blue = conditionInfo.ColorRule.BackgroundColor.B,
                                        Green = conditionInfo.ColorRule.BackgroundColor.G,
                                        Red = conditionInfo.ColorRule.BackgroundColor.R
                                    };
                            }
                        }
                        if (conditionInfo.LowerBounds != null || conditionInfo.Upperbounds != null)
                        {
                            rule.PercentageBounds = new ReportPercentageBounds
                                {
                                    LowerBounds = conditionInfo.LowerBounds,
                                    UpperBounds = conditionInfo.Upperbounds
                                };
                        }
                        if (conditionInfo.IconRule != null && (conditionInfo.IconRule.IconId ?? 0) > 0)
                        {
                            if ((conditionInfo.IconRule.CfEntityId ?? 0) > 0)
                            {
                                IEntity cfEntity = Entity.Get(conditionInfo.IconRule.CfEntityId.Value);
                                var cfi = cfEntity.As<ConditionalFormatIcon>();
                                if (cfi != null)
                                {
                                    rule.CfEntityId = cfi.Id;
                                    rule.ImageEntityId = cfi.CondFormatImage != null ? cfi.CondFormatImage.Id : -1;
                                }

                            }
                            else 
                            {
                                IEntity entity = Entity.Get(conditionInfo.IconRule.IconId.Value);
                                if (entity.Is<IconFileType>())
                                {
                                    rule.ImageEntityId = conditionInfo.IconRule.IconId.Value;
                                }
                            }

                            if (imageScale == null)
                            {
                                imageScale = new EntityRef("core", "scaleImageProportionally");
                            }
                            if (imageSize == null)
                            {
                                imageSize = new EntityRef("console", "iconThumbnailSize");
                            }
                            if (thumbnailSizeEnum == null)
                            {
                                thumbnailSizeEnum = Entity.Get<ThumbnailSizeEnum>(imageSize);
                            }
                            if (imageScaleRules == null)
                            {
                                imageScaleRules = new Dictionary<string, ReportImageScale>();
                            }
                            if (!imageScaleRules.ContainsKey(formatRule.Key))
                            {
                                ReportImageScale reportImageScale = new ReportImageScale
                                    {
                                        ImageScaleId = imageScale.Id,
                                        ImageSizeId = imageSize.Id,
                                        ImageWidth = thumbnailSizeEnum.ThumbnailWidth ?? 16,
                                        ImageHeight = thumbnailSizeEnum.ThumbnailHeight ?? 16
                                    };
                                imageScaleRules.Add(formatRule.Key, reportImageScale);
                            }
                        }
                        // Handle the values for the typed value only if this is not an icon as it has relationships
						if ( conditionInfo.Values != null && conditionInfo.Values.Count > 0 )
                        {
                            if (conditionInfo.Values[0].Type is ChoiceRelationshipType || conditionInfo.Values[0].Type is InlineRelationshipType || conditionInfo.Values[0].Type is StructureLevelsType)
                            {
                                rule.Values = ProcessConditionValues(conditionInfo.Values);
                                // If there are no values returned then this must be a text string rather than a list of entity identifiers.
                                if (rule.Values == null)
                                {
                                    rule.Value = conditionInfo.Values[0].ValueString;
                                }
                            }
                            else
                            {
                                DateTime dateTimeValue;
                                if ((conditionInfo.Values[0].Type is DateTimeType || conditionInfo.Values[0].Type is TimeType || conditionInfo.Values[0].Type is DateType) &&
                                    (DateTime.TryParse(conditionInfo.Values[0].Value.ToString(), out dateTimeValue)))
                                {
                                    rule.Value = dateTimeValue.ToString("yyyy-MM-ddTHH:mm:ssZ");
                                }
                                else
                                {
                                    rule.Value = conditionInfo.Values[0].ValueString;
                                }
                            }
                        }
                        rules.Add(rule);
                    }
                    columnFormat.Rules = rules;
                    formats.Add(formatRule.Key, columnFormat);
                }
                return formats;
            }
            return null;
        }

        /// <summary>
        /// Processes the condition values.
        /// </summary>
        /// <param name="typedValues">The typed values.</param>
        /// <returns>Dictionary{System.Int64System.String}.</returns>
        private Dictionary<long, string> ProcessConditionValues(IEnumerable<TypedValue> typedValues)
        {
            Dictionary<long, string> values = new Dictionary<long, string>();
            foreach (TypedValue typedValue in typedValues)
            {
                long entityId;
                if (!long.TryParse(typedValue.ValueString, out entityId))
                {
                    continue;
                }
                Entity entity = Entity.Get<Entity>(entityId);            
                values[entityId] =  entity.GetField("core:name") as string;
            }
			return values.Count > 0 ? values : null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalFormatter" /> class.
        /// </summary>
        /// <param name="columnFormats">The column formats.</param>
        /// <param name="additionalColumnInfo">The additional column info.</param>
        /// <param name="reportResult">The report result.</param>
        public ConditionalFormatter(IList<ColumnFormatting> columnFormats, IEnumerable<ResultColumn> additionalColumnInfo, ReportResult reportResult)
        {
            _reportResult = reportResult;

            if (columnFormats != null)
            {
                _formatRules = new Dictionary<string, List<ConditionInfo>>(columnFormats.Count);

                // Cache the formatting rules into a dictionary for fast lookup.
                Dictionary<string, ColumnFormatting> formatters = columnFormats.ToDictionary(cf => cf.EntityId.ToString(CultureInfo.InvariantCulture));

                foreach (KeyValuePair<string, ColumnFormatting> keyValuePair in formatters.Where(f => f.Value != null))
                {
                    BarFormattingRule barFormattingRule = keyValuePair.Value.FormattingRule as BarFormattingRule;
                    if (barFormattingRule != null)
                    {
                        ConditionInfo condition = new ConditionInfo
                        {
                            ColorRule = new ColorRule { BackgroundColor = barFormattingRule.Color },
                            Style = ConditionalFormatStyleEnum.ProgressBar,
                            ShowText = keyValuePair.Value.ShowText
                        };
                        DatabaseType columnForFormatType = additionalColumnInfo.FirstOrDefault(ac => ac.RequestColumn.EntityId.ToString(CultureInfo.InvariantCulture) == keyValuePair.Key).ColumnType;
                        if (columnForFormatType != null)
                        {
                            barFormattingRule.Minimum.Type = columnForFormatType;
                            barFormattingRule.Maximum.Type = columnForFormatType;
                        }

                        DateTime minumumDateTime;
                        if (barFormattingRule.Minimum.Value != null &&
                            (columnForFormatType is DateType || columnForFormatType is TimeType || columnForFormatType is DateTimeType)
                            && DateTime.TryParse(barFormattingRule.Minimum.Value.ToString(), out minumumDateTime))
                        {
                            condition.LowerBounds = minumumDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        }
                        else
                        {
                            float minimum;
                            condition.LowerBounds = float.TryParse(barFormattingRule.Minimum.ValueString, out minimum) ? minimum : 0.0f;
                        }
                        DateTime maximumDateTime;
                        if (barFormattingRule.Maximum.Value != null &&
                            (columnForFormatType is DateType || columnForFormatType is TimeType || columnForFormatType is DateTimeType)
                            && DateTime.TryParse(barFormattingRule.Maximum.Value.ToString(), out maximumDateTime))
                        {
                            condition.Upperbounds = maximumDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        }
                        else
                        {
                            float maximum;
                            if (float.TryParse(barFormattingRule.Maximum.ValueString, out maximum))
                            {
                                condition.Upperbounds = maximum;
                            }
                        }
                        _formatRules.Add(keyValuePair.Key, new List<ConditionInfo>(new[] { condition }));
                    }
                    ColorFormattingRule colorFormattingRule = keyValuePair.Value.FormattingRule as ColorFormattingRule;
                    if (colorFormattingRule != null)
                    {
                        IList<ColorRule> colorRules = colorFormattingRule.Rules;
                        List<ConditionInfo> predicates = new List<ConditionInfo>(colorRules.Count);
                        foreach (ColorRule colorRule in colorRules)
                        {
                            ResultColumn gridResultColumn = additionalColumnInfo.FirstOrDefault(gu => gu.RequestColumn.EntityId.ToString(CultureInfo.InvariantCulture) == keyValuePair.Key);
                            if (gridResultColumn == null)
                            {
                                continue;
                            }
                            colorRule.Condition.Arguments = TypedValuesForOperatorColumnType(colorRule.Condition.Arguments, colorRule.Condition.Operator, gridResultColumn.ColumnType);
                        }
                        predicates.AddRange(from colorRule in colorRules
                                            let gridResultColumn = additionalColumnInfo.FirstOrDefault(gu => gu.RequestColumn.EntityId.ToString(CultureInfo.InvariantCulture) == keyValuePair.Key)
                                            let predicate = PredicateForRule(colorRule.Condition, IsResourceCondition(gridResultColumn), gridResultColumn, this)
                                            where (predicate != null)
                                            select new ConditionInfo
                                            {
                                                Predicate = predicate,
                                                ColorRule = colorRule,
                                                Operator = colorRule.Condition.Operator,
                                                Values = colorRule.Condition.Arguments,
                                                Style = ConditionalFormatStyleEnum.Highlight,
                                                ShowText = keyValuePair.Value.ShowText
                                            });
                        _formatRules.Add(keyValuePair.Key, predicates);
                    }
                    IconFormattingRule iconFormattingRule = keyValuePair.Value.FormattingRule as IconFormattingRule;
                    if (iconFormattingRule != null)
                    {
                        IList<IconRule> iconRulesRules = iconFormattingRule.Rules;
                        foreach (IconRule iconRule in iconRulesRules)
                        {
                            ResultColumn gridResultColumn = additionalColumnInfo.FirstOrDefault(gu => gu.RequestColumn.EntityId.ToString(CultureInfo.InvariantCulture) == keyValuePair.Key);
                            if (gridResultColumn == null)
                            {
                                continue;
                            }
                            iconRule.Condition.Arguments = TypedValuesForOperatorColumnType(iconRule.Condition.Arguments, iconRule.Condition.Operator, gridResultColumn.ColumnType);
                        }
                        List<ConditionInfo> predicates = new List<ConditionInfo>(iconRulesRules.Count);
                        predicates.AddRange(from iconRule in iconRulesRules
                                            let gridResultColumn = additionalColumnInfo.FirstOrDefault(gu => gu.RequestColumn.EntityId.ToString(CultureInfo.InvariantCulture) == keyValuePair.Key)
                                            let predicate = PredicateForRule(iconRule.Condition, IsResourceCondition(gridResultColumn), gridResultColumn, this)
                                            where (predicate != null)
                                            select new ConditionInfo
                                            {
                                                Predicate = predicate,
                                                IconRule = iconRule,
                                                Operator = iconRule.Condition.Operator,
                                                Values = iconRule.Condition.Arguments,
                                                Style = ConditionalFormatStyleEnum.Icon,
                                                ShowText = keyValuePair.Value.ShowText
                                            });
                        _formatRules.Add(keyValuePair.Key, predicates);
                    }
                }
            }
        }
        

        /// <summary>
        /// Determines whether the result column should use a resource condition.
        /// </summary>
        /// <param name="resultColumn">The result column.</param>
        /// <returns></returns>
        private bool IsResourceCondition(ResultColumn resultColumn)
        {
            if (resultColumn == null) return false;

            return resultColumn.IsRelatedResource || _reportResult.IsResultColumnEntityNameColumn(resultColumn);
        }


        /// <summary>
        /// Typeds the type of the values for operator column.
        /// </summary>
        /// <param name="typedValues">The typed values.</param>
        /// <param name="conditionType">Type of the condition.</param>
        /// <param name="columnType">Type of the column.</param>
        /// <returns>IList{TypedValue}.</returns>
        private static IList<TypedValue> TypedValuesForOperatorColumnType(IEnumerable<TypedValue> typedValues, ConditionType conditionType, DatabaseType columnType)
        {
            IList<TypedValue> values = new List<TypedValue>();
            if (typedValues == null)
                return values;

            foreach (TypedValue typedValue in typedValues)
            {
                int operatorCount = ConditionTypeHelper.GetArgumentCount(conditionType);
                if (operatorCount <= 0)
                {
                    continue;
                }
                DatabaseType type = ConditionTypeHelper.GetArgumentType(conditionType, columnType, operatorCount - 1);
                if (type != null)
                {
                    typedValue.Type = type;
                    values.Add(typedValue);
                }
            }
            return values;
        }

        /// <summary>
        /// Tries to get the rule based on the available rules for the column (if any).
        /// </summary>
        /// <param name="columnId">The column unique identifier.</param>
        /// <param name="data">The data.</param>
        /// <param name="ruleIndex">Index of the rule.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="System.InvalidOperationException">Predicate has not been set of condition.</exception>
        public bool TryGetRule(string columnId, object data, out long ruleIndex)
        {
            if ( data == null || !_formatRules.ContainsKey( columnId ) )
            {
                ruleIndex = -1;
                return false;
            }

            List<ConditionInfo> rules = _formatRules [ columnId ];
            int count = rules.Count;
            int last = count - 1;

            // Check each rule
            for (int i = 0; i < count; i++)
            {
                ConditionInfo rule = rules[i];

                // Get the condition for this rule
                if ( rule.Predicate == null )
                {
                    continue;
                }

                if (rule.ColorRule != null)
                {
                    // Accept the last rule regardless
                    if ( i == last && rule.ColorRule.Condition.Operator == ConditionType.Unspecified )
                    {
                        ruleIndex = i;
                        return true;
                    }
                }
                else if (rule.IconRule != null)
                {
                    // Accept the last rule regardless
                    if ( i == last && rule.IconRule.Condition.Operator == ConditionType.Unspecified )
                    {
                        ruleIndex = i;
                        return true;
                    }
                }

                // Test the value against the rule                
                if (rule.Predicate(data))
                {
                    ruleIndex = i;
                    return true;
                }
            }

            ruleIndex = -1;
            return false;
        }

        /// <summary>
        /// Tries to get the rule based on the default condition rules for the choice field column.
        /// </summary>
        /// <param name="conditionalFormat">The conditional column format.</param>
        /// <param name="data">The data.</param>
        /// <param name="ruleIndex">Index of the rule.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="System.InvalidOperationException">Predicate has not been set of condition.</exception>
        public bool TryGetDefaultRule(ReportColumnConditionalFormat conditionalFormat,  object data, out long ruleIndex)
        {
            if (conditionalFormat != null && conditionalFormat.Rules != null && conditionalFormat.Rules.Count > 0)
            {
                for (int i = 0; i < conditionalFormat.Rules.Count; i++)
                {
                    ReportConditionalFormatRule rule = conditionalFormat.Rules[i];

                    if (rule.Values == null ||  rule.Predicate == null)
                    {
                        continue;
                    }

                    if (rule.Predicate(data))
                    {
                        ruleIndex = i;
                        return true;
                    }
                }
            }

            ruleIndex = -1;
            return false;
        }

        /// <summary>
        /// CReate the colour values for column data element.
        /// </summary>
        /// <param name="queryColumnId">The query column unique identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns>ColorRule.</returns>
        public ColorRule ColourForColumnData(string queryColumnId, object data)
        {
            if (!_formatRules.ContainsKey(queryColumnId) || data == null)
            {
                return null;
            }
            // Run the list of column formatting rules using this value
            ConditionInfo rule = SelectMatchingRule(_formatRules[queryColumnId].ToArray(), data);
            return rule != null ? rule.ColorRule : null;
        }

        /// <summary>
        /// Selects the matching rule.
        /// </summary>
        /// <typeparam name="TRule">The type of the attribute rule.</typeparam>
        /// <param name="rules">The rules.</param>
        /// <param name="value">The value.</param>
        /// <returns>``0.</returns>
        /// <exception cref="System.InvalidOperationException">Predicate has not been set of condition.</exception>
        public static TRule SelectMatchingRule<TRule>(TRule[] rules, object value)
            where TRule : ConditionInfo
        {
            int count = rules.Length;

            // Check each rule
            for (int i = 0; i < count; i++)
            {
                TRule rule = rules[i];

                // Get the condition for this rule
                if (rule.Predicate == null)
                    throw new InvalidOperationException("Predicate has not been set of condition."); // assert false

                // Accept the last rule regardless
                if (i == count - 1 && rule.ColorRule.Condition.Operator == ConditionType.Unspecified)
                    return rule;

                // Test the value against the rule                
                if (rule.Predicate(value))
                    return rule;
            }

            // Return null if no rules were found, or no rules matched and the last rule did not have a condition of Unspecified.
            return null;
        }

        
        /// <summary>
        /// Predicates for rule.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="isResourceCondition">if set to <c>true</c> [is resource condition].</param>
        /// <param name="gridResultColumn">The result column.</param>
        /// <param name="conditionalFormatter">The conditional formatter column.</param>
        /// <returns>Predicate{System.Object}.</returns>
        private static Predicate<object> PredicateForRule(Condition condition, bool isResourceCondition, ResultColumn gridResultColumn, ConditionalFormatter conditionalFormatter)
		{
            try
            {
                Predicate<object> valueFilterPredicate = BuildValueFilterPredicateNoNulls(condition, gridResultColumn, conditionalFormatter);
                
                return value =>
                {
                    try
                    {
                        // Handle nulls
                        // for now ANY type of filter on a column will reject null values
                        if (value == null || value is DBNull)
                        {
                            switch (condition.Operator)
                            {
                                case ConditionType.NotEqual:
                                    value = "";
                                    break;
                                case ConditionType.IsNull:
                                    return true;
                                case ConditionType.IsNotNull:
                                    return false;
                                case ConditionType.AnyExcept:
                                    return true;
                                default:
                                    return false;
                            }
                        }

                        // Handle transforms
                        if (isResourceCondition)
                        {
                            var xml = (string) value;
                            if (condition.Operator == ConditionType.AnyExcept ||
                                condition.Operator == ConditionType.AnyOf ||
                                condition.Operator == ConditionType.CurrentUser)
                                value = DatabaseTypeHelper.GetEntityXmlId(xml);
                            else
                                value = DatabaseTypeHelper.GetEntityXmlName(xml);
                        }

                        // Evaluate the specific predicate
                        return valueFilterPredicate(value);
                    }
                    catch
                    {
                        return false;
                        //throw new Exception( string.Format( "Predicate failed. Data={0} {1}. Expected={2}", value == null ? "null" : value.GetType( ).Name, value, condition.ColumnType ), ex );
                    }
                };
            }
            catch (ArgumentException)
            {
                return null; //  argument does not exist .. return null
            }
		}

      
        /// <summary>
        ///     Builds a predicate that examines the raw data (as its native .Net type) to determine if
        ///     it passes the specified filter.
        /// </summary>
        /// <remarks>
        ///     This overload assumes that the values will not be null.
        /// </remarks>
        /// <param name="condition">The condition to be applied.</param>
        /// <param name="gridResultColumn">The grid result column.</param>
        /// <param name="conditionalFormatter">The conditional formatter.</param>
        /// <returns>A predicate that accepts a data value and returns true if the value is acceptable on the basis of the filter.</returns>
        private static Predicate<object> BuildValueFilterPredicateNoNulls(Condition condition, ResultColumn gridResultColumn, ConditionalFormatter conditionalFormatter)
        {
            // Get and check argument
            TypedValue argument = condition.Argument;
            string sArgument = null;
            IComparable cArgument = null;

            // Handle null strings. TODO: Improve this hack
            if (argument != null && argument.Value == null && argument.Type is StringType)
            {
                argument.Value = "";
            }

            if (argument == null || argument.Value == null)
            {
                if (ConditionTypeHelper.GetArgumentCount(condition.Operator) != 0)
                {
                    throw new ArgumentException(@"Condition has no (or null) argument.", "condition");
                }
            }
            else
            {
                sArgument = argument.Value as string;
                cArgument = argument.Value as IComparable;
            }

            bool isDate = condition.ColumnType is DateType;
            bool isCurrency = condition.ColumnType is CurrencyType;            
            // Return lambda to evaluate test
            // 'cell' is statically type object
            switch (condition.Operator)
            {
                case ConditionType.Unspecified:
                    return cell => true; // assert false - predicates should not be built from filters with unspecified operators. Silently ignore.

                case ConditionType.Equal:
                    if (argument.Type is StringType)
                    {
                        return cell => ((string)cell).Equals(argument.ValueString, StringComparison.CurrentCultureIgnoreCase);
                    }
                    return cell => argument != null && argument.Value.Equals(isCurrency ? ConvertCurrencyCell(cell, condition.ColumnType) : cell);

                case ConditionType.NotEqual:
                    if (argument.Type is StringType)
                    {
                        return cell => !((string)cell).Equals(argument.ValueString, StringComparison.CurrentCultureIgnoreCase);
                    }
                    return cell => argument != null && !argument.Value.Equals(isCurrency ? ConvertCurrencyCell(cell, condition.ColumnType) : cell);

                case ConditionType.GreaterThan:
                    return cell => cArgument != null && cArgument.CompareTo(isCurrency ? ConvertCurrencyCell(cell, condition.ColumnType) : cell) < 0;

                case ConditionType.GreaterThanOrEqual:
                    return cell => cArgument != null && cArgument.CompareTo(isCurrency ? ConvertCurrencyCell(cell, condition.ColumnType) : cell) <= 0;

                case ConditionType.LessThan:
                    return cell => cArgument != null && cArgument.CompareTo(isCurrency ? ConvertCurrencyCell(cell, condition.ColumnType) : cell) > 0;

                case ConditionType.LessThanOrEqual:
                    return cell => cArgument != null && cArgument.CompareTo(isCurrency ? ConvertCurrencyCell(cell, condition.ColumnType) : cell) >= 0;

                case ConditionType.Contains:
                    return cell => ((string)cell).IndexOf(sArgument, StringComparison.CurrentCultureIgnoreCase) >= 0;

                case ConditionType.StartsWith:
                    return cell => ((string)cell).StartsWith(sArgument, StringComparison.CurrentCultureIgnoreCase);

                case ConditionType.EndsWith:
                    return cell => ((string)cell).EndsWith(sArgument, StringComparison.CurrentCultureIgnoreCase);

                case ConditionType.AnyOf:
                    return cell => AnyOf((long)cell, condition.Arguments);
                case ConditionType.AnyExcept:
                    return cell => AnyExcept((long)cell, condition.Arguments);

                case ConditionType.Today:
                case ConditionType.ThisMonth:
                case ConditionType.ThisQuarter:
                case ConditionType.ThisYear:
                case ConditionType.CurrentFinancialYear:
                case ConditionType.LastNDays:
                case ConditionType.NextNDays:
                case ConditionType.LastNMonths:
                case ConditionType.NextNMonths:
                case ConditionType.LastNQuarters:
                case ConditionType.NextNQuarters:
                case ConditionType.LastNYears:
                case ConditionType.NextNYears:
                case ConditionType.LastNFinancialYears:
                case ConditionType.NextNFinancialYears:
                case ConditionType.LastNDaysTillNow:
                case ConditionType.NextNDaysFromNow:
                case ConditionType.ThisWeek:
                case ConditionType.LastNWeeks:
                case ConditionType.NextNWeeks:
                    return cell =>
                    {
                        DateTime minDate, maxDate;
                        var cellAsDateTime = isDate ? (DateTime)cell : UtcToLocal((DateTime)cell);

                        const int startMonthOfFinancialYear = 7; //TODO: this comes from old code. Reported as a bug to Diana to improve to respect tenant FY configuration.
                        PeriodConditionHelper.GetPeriodFromConditionType(condition.Operator, DateTime.Today, cArgument != null ? cArgument as int?: null, startMonthOfFinancialYear, isDate, out minDate, out maxDate);

                        return cellAsDateTime >= minDate && cellAsDateTime < maxDate;
                    };

                case ConditionType.DateEquals:
                    {
                        if (argument != null && argument.Value != null)
                        {
                            DateTime today = ((DateTime)argument.Value).Date;
                            DateTime threshold = today.AddDays(1);
                            return cell =>
                            {
                                DateTime dCell = isDate ? (DateTime)cell : UtcToLocal((DateTime)cell);
                                return threshold.CompareTo(dCell) > 0 && today.CompareTo(dCell) <= 0;
                            };
                        }
                    }
                    break;

                case ConditionType.IsTrue:
                    return cell => ((bool)cell);

                case ConditionType.IsFalse:
                    return cell => !((bool)cell);

                case ConditionType.IsNull:
                    return cell => string.Empty.Equals(cell); // this method does not get called if it is truly null

                case ConditionType.IsNotNull:
                    return cell => !string.Empty.Equals(cell); // this method does not get called if it is truly null

                case ConditionType.CurrentUser:
                    return IsCurrentUser;

                case ConditionType.AnyBelowStructureLevel:
                case ConditionType.AnyAboveStructureLevel:
                case ConditionType.AnyAtOrAboveStructureLevel:
                case ConditionType.AnyAtOrBelowStructureLevel:
                    return cell => ApplyStructureViewCondition((string)cell, condition, gridResultColumn, conditionalFormatter);

                default:
                    throw new Exception("Unknown filter operator.");
            }

            return null;
        }
                
        /// <summary>
        /// Apply the specified structure level condition to the given cell.
        /// </summary>
        /// <param name="cellValue">The cell value.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="gridResultColumn">The grid result column.</param>
        /// <param name="conditionalFormatter">The current conditional formatter.</param>
        /// <returns>True if the operator and arguments apply, false otherwuse</returns>
        private static bool ApplyStructureViewCondition(string cellValue, Condition condition, ResultColumn gridResultColumn, ConditionalFormatter conditionalFormatter)
        {            
            IList<TypedValue> arguments = condition.Arguments;
            ConditionType conditionOperator = condition.Operator;

            // Sanity check
            if (arguments == null || arguments.Count <= 0 || string.IsNullOrEmpty(cellValue) || gridResultColumn == null)
            {
                return false;
            }            

            var structureViewExpression = gridResultColumn.RequestColumn.Expression as StructureViewExpression;
            if (structureViewExpression == null)
            {
                return false;
            }            

            // Get the structure levels for this structure view.
            Dictionary<long, StructureViewLevelsValue> structureViewLevels;
            if (!conditionalFormatter._structureViewLevelsCache.TryGetValue(structureViewExpression.StructureViewId.Id, out structureViewLevels))
            {
                structureViewLevels = GetStructureViewLevels(structureViewExpression);
                conditionalFormatter._structureViewLevelsCache[structureViewExpression.StructureViewId.Id] = structureViewLevels;
            }

            int colonIndex = cellValue.IndexOf(':');
            if (colonIndex <= 0)
            {
                return false;
            }

            // Parse the entity id of the current structure level from the cell value
            string idString = cellValue.Substring(0, colonIndex);
            long cellEntityId;
            if (!long.TryParse(idString, out cellEntityId))
            {
                return false;                    
            }

            // Get all the levels to/from this structure level
            StructureViewLevelsValue levelsValue;
            if (!structureViewLevels.TryGetValue(cellEntityId, out levelsValue))
            {
                return false;
            }

            // Check the arguments
            foreach (var argValueAsString in arguments.Select(argument => argument.Value.ToString()))
            {
                // Check for equality
                if (argValueAsString == idString &&                        
                    (conditionOperator == ConditionType.AnyAtOrAboveStructureLevel ||
                     conditionOperator == ConditionType.AnyAtOrBelowStructureLevel))
                {
                    return true;
                }

                long argValueAsLong;
                if (!long.TryParse(argValueAsString, out argValueAsLong))
                {
                    continue;
                }

                // Check hierarchy
                switch (conditionOperator)
                {
                    case ConditionType.AnyAboveStructureLevel:                            
                    case ConditionType.AnyAtOrAboveStructureLevel:
                        if (levelsValue.ToLevelIds.Contains(argValueAsLong))
                        {
                            return true;
                        }
                        break;
                    case ConditionType.AnyAtOrBelowStructureLevel:
                    case ConditionType.AnyBelowStructureLevel:
                        if (levelsValue.FromLevelIds.Contains(argValueAsLong))
                        {
                            return true;
                        }
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the structure view levels for the given structure view.
        /// </summary>
        /// <param name="structureViewExpression">The structure view expression</param>
        /// <returns></returns>
        private static Dictionary<long, StructureViewLevelsValue> GetStructureViewLevels(StructureViewExpression structureViewExpression)
        {
            var structureViewLevels = new Dictionary<long, StructureViewLevelsValue>();
            var structureView = Entity.Get<StructureView>(structureViewExpression.StructureViewId);

            if (structureView == null)
            {
                return structureViewLevels;
            }

            bool isReverse = structureView.FollowRelationshipInReverse ?? false;
            Relationship structureViewRelationship = structureView.StructureHierarchyRelationship;            

            if (structureViewRelationship == null)
            {
                return structureViewLevels;
            }

            using (IDatabaseContext context = Factory.DatabaseProvider.GetContext())
            {
                const string sql = @"select FromId, ToId from dbo.fnGetRelationshipRecAndSelf( @relationTypeId, @tenantId, 0, @fromTypeId, @toTypeId )";

                IDbCommand command = context.CreateCommand(sql);
                command.AddParameter("@relationTypeId", DbType.Int64, structureViewRelationship.Id);
                command.AddParameter("@tenantId", DbType.Int64, RequestContext.TenantId);
                command.AddParameter("@fromTypeId", DbType.Int64, structureViewRelationship.FromType.Id);
                command.AddParameter("@toTypeId", DbType.Int64, structureViewRelationship.ToType.Id);                

                using (IDataReader reader = command.ExecuteReader())
                {
                    // Read values and build up lookup structure
                    while (reader.Read())
                    {
                        long fromId = reader.GetInt64(isReverse ? 1 : 0);
                        long toId = reader.GetInt64(isReverse ? 0 : 1);

                        StructureViewLevelsValue toLevels;

                        if (!structureViewLevels.TryGetValue(fromId, out toLevels))
                        {
                            toLevels = new StructureViewLevelsValue();
                            structureViewLevels[fromId] = toLevels;
                        }

                        toLevels.ToLevelIds.Add(toId);

                        StructureViewLevelsValue fromLevels;

                        if (!structureViewLevels.TryGetValue(toId, out fromLevels))
                        {
                            fromLevels = new StructureViewLevelsValue();
                            structureViewLevels[toId] = fromLevels;
                        }

                        fromLevels.FromLevelIds.Add(fromId);
                    }
                }

                return structureViewLevels;
            }
        }

        private static bool IsCurrentUser(object cell)
        {
            long? comparitorEntityId = cell as long?;
            if (comparitorEntityId == null)
            {
                return false;
            }
            UserAccount userAccount = Entity.Get<UserAccount>(RequestContext.GetContext().Identity.Id);
            if (comparitorEntityId == userAccount.Id)
            {
                return true;
            }
            return userAccount.AccountHolder != null && userAccount.AccountHolder.Id == comparitorEntityId;
        }

        /// <summary>
        /// check the cell value is any Except argument value array
        /// </summary>
        /// <param name="cellValue">cell value</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        private static bool AnyExcept(long cellValue, IEnumerable<TypedValue> arguments)
		{
            if (arguments == null || cellValue == 0)
                return true;
            return !arguments.Any(tv => tv.Value.ToString() == cellValue.ToString());
		}

        /// <summary>
        /// check the cell value is any of argument value array
        /// </summary>
        /// <param name="cellValue">cell value</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        private static bool AnyOf(long cellValue, IEnumerable<TypedValue> arguments)
		{
            if (arguments == null || cellValue == 0)
                return false;
            return arguments.Any(tv => tv.Value.ToString() == cellValue.ToString());
		}

        /// <summary>
        /// Builds a predicate that examines the raw data (as its native .Net type) to determine if
        /// it passes the specified filter.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="currencyType">Type of the currency.</param>
        /// <returns>A predicate that accepts a data value and returns true if the value is acceptable on the basis of the filter.</returns>
        /// <remarks>This overload assumes that the values will not be null.</remarks>
        private static object ConvertCurrencyCell(object value, DatabaseType currencyType)
        {
            if (value.ToString().StartsWith("$"))
            {
                try
                {
                    return DatabaseTypeHelper.ConvertFromString(currencyType, value.ToString().Replace("$", ""));
                }
                catch
                {
                    return value;
                }
            }

            return value;
        }

        /// <summary>
        /// Converts a UTC date/time value to the local time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>DateTime.</returns>
        private static DateTime UtcToLocal(DateTime dateTime)
        {
            DateTime utc = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            DateTime local = utc.ToLocalTime();
            return local;
        }

        /// <summary>
        /// Represents the structure levels found to/from a current level.
        /// </summary>
        private class StructureViewLevelsValue
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public StructureViewLevelsValue()
            {
                ToLevelIds = new HashSet<long>();
                FromLevelIds = new HashSet<long>();
            }

            /// <summary>
            /// The levels found at any depth below the structure level.
            /// </summary>
            public ISet<long> ToLevelIds { get; private set; }

            /// <summary>
            /// The levels found at any depth above the structure level.
            /// </summary>
            public ISet<long> FromLevelIds { get; private set; }
        }
    }
}
