// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using Entity = EDC.ReadiNow.Model.Entity;

namespace ReadiNow.Reporting.Helpers
{
    public class ReportConditionHelper
    {
        public static ActivityArgument ArgumentForConditionType(ActivityArgument reportColumnExpressionType, string value)
        {
            IEntity result;
            if (reportColumnExpressionType.Is<StringArgument>())
            {
                result = new StringArgument { StringParameterValue = value };
            }
            else if (reportColumnExpressionType.Is<IntegerArgument>())
            {
                int intValue;
                result = !int.TryParse(value, out intValue) ? new IntegerArgument() : new IntegerArgument { IntParameterValue = intValue };
            }
            else if (reportColumnExpressionType.Is<CurrencyArgument>())
            {
                decimal currencyValue;
                result = !decimal.TryParse(value, out currencyValue) ? new CurrencyArgument() : new CurrencyArgument { DecimalParameterValue = currencyValue };
            }
            else if (reportColumnExpressionType.Is<DecimalArgument>())
            {
                decimal decimalValue;
                result = !decimal.TryParse(value, out decimalValue) ? new DecimalArgument() : new DecimalArgument { DecimalParameterValue = decimalValue };
            }
            else if (reportColumnExpressionType.Is<DateArgument>())
            {
                DateTime dateValue;
                result = !DateTime.TryParse(value, out dateValue) ? new DateArgument() : new DateArgument { DateParameterValue = dateValue };
            }
            else if (reportColumnExpressionType.Is<TimeArgument>())
            {
                DateTime timeValue;
                result = !DateTime.TryParse(value, out timeValue) ? new TimeArgument() : new TimeArgument { TimeParameterValue = timeValue };
            }
            else if (reportColumnExpressionType.Is<DateTimeArgument>())
            {
                DateTime dateTimeValue;
                result = !DateTime.TryParse(value, out dateTimeValue) ? new DateTimeArgument() : new DateTimeArgument { DateTimeParameterValue = dateTimeValue };
            }
            else if (reportColumnExpressionType.Is<GuidArgument>())
            {
                Guid guidValue;
                result = !Guid.TryParse(value, out guidValue) ? new GuidArgument() : new GuidArgument { GuidParameterValue = guidValue };
            }
            else if (reportColumnExpressionType.Is<BoolArgument>())
            {
                bool boolValue;
                result = !bool.TryParse(value, out boolValue) ? new BoolArgument() : new BoolArgument { BoolParameterValue = boolValue };
            }
            else if (reportColumnExpressionType.Is<ResourceArgument>())
            {
                // Convert the value to an entityId
                TypedArgument tResult = reportColumnExpressionType.As<TypedArgument>();
                long entityId;
                result = new ResourceArgument
                {
                    ResourceParameterValue = long.TryParse(value, out entityId) ? Entity.Get(entityId).As<Resource>() : new Resource(),
                    ConformsToType = tResult.ConformsToType
                };
            }
            else if (reportColumnExpressionType.Is<ResourceListArgument>())
            {
                TypedArgument tResult = reportColumnExpressionType.As<TypedArgument>();
                long entityId;
                result = new ResourceListArgument
                {
                    ResourceListParameterValues = new EntityCollection<Resource>
                            {
                                long.TryParse(value, out entityId) ? Entity.Get(entityId).As<Resource>() : new Resource()
                            },
                    ConformsToType = tResult.ConformsToType
                };
            }
            else
            {
                throw new Exception("Unhandled expression result type");
            }

            // Caller must save
            return result.As<ActivityArgument>();
        }

        /// <summary>
        /// Returns a new activity argument describing the argument type based on the column expression type and condition.
        /// Also populates the value for the argument prior to returning.
        /// </summary>
        /// <param name="reportColumnExpressionType">Type of the report column expression.</param>
        /// <param name="conditionType">Type of the condition.</param>
        /// <param name="value">The value.</param>
        /// <returns>ActivityArgument.</returns>
        public static ActivityArgument ArgumentForConditionType(ActivityArgument reportColumnExpressionType, ConditionType conditionType, string value)
        {
            ActivityArgument targetArgumentType = ArgumentForConditionType(reportColumnExpressionType, conditionType);
            return ArgumentForConditionType(targetArgumentType, value);
        }

        /// <summary>
        /// Returns a new activity argument describing the argument type based on the column expression type and condition.
        /// </summary>
        /// <param name="reportColumnExpressionType">Type of the report column expression.</param>
        /// <param name="conditionType">Type of the condition.</param>
        /// <returns>ActivityArgument.</returns>
        public static ActivityArgument ArgumentForConditionType(ActivityArgument reportColumnExpressionType, ConditionType conditionType)
        {
            switch (conditionType)
            {
                case ConditionType.Equal:
                case ConditionType.NotEqual:
                case ConditionType.Contains:
                case ConditionType.StartsWith:
                case ConditionType.EndsWith:
                    if (reportColumnExpressionType.Is<ResourceArgument>() || reportColumnExpressionType.Is<ResourceListArgument>())
                        return new StringArgument().As<ActivityArgument>();
                    return reportColumnExpressionType;
                case ConditionType.LastNDays:
                case ConditionType.LastNDaysTillNow:
                case ConditionType.NextNDaysFromNow:
                case ConditionType.NextNDays:
                case ConditionType.LastNWeeks:
                case ConditionType.NextNWeeks:
                case ConditionType.LastNMonths:
                case ConditionType.NextNMonths:
                case ConditionType.LastNQuarters:
                case ConditionType.NextNQuarters:
                case ConditionType.LastNYears:
                case ConditionType.NextNYears:
                case ConditionType.LastNFinancialYears:
                case ConditionType.NextNFinancialYears:
                    return new IntegerArgument().As<ActivityArgument>();
                case ConditionType.DateEquals:
                    return new DateArgument().As<ActivityArgument>();
                case ConditionType.FullTextSearch:
                        return new StringArgument().As<ActivityArgument>();
                case ConditionType.AnyOf:
                case ConditionType.AnyExcept:
                case ConditionType.CurrentUser:
                    if (reportColumnExpressionType.Is<ResourceArgument>() || reportColumnExpressionType.Is<ResourceListArgument>())
                        return reportColumnExpressionType;
                    return new StringArgument().As<ActivityArgument>();
                case ConditionType.Unspecified:
                    return new StringArgument().As<ActivityArgument>();
                default:
                    return new StringArgument().As<ActivityArgument>();
            }
        }

    }
}
