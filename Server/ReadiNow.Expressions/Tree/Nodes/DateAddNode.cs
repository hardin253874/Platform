// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.Parser;
using ReadiNow.Expressions.Evaluation;
using ReadiNow.Expressions.Compiler;

namespace ReadiNow.Expressions.Tree.Nodes
{


    class DateAddNode : BinaryOperatorNode, IDateTimePartFunction
    {
        public DateTimeParts DateTimePart { get; set; }

        protected override DateTime? OnEvaluateDate(EvaluationContext evalContext)
        {
            int? number = Left.EvaluateInt(evalContext);
            if (number == null)
                return null;

            DateTime? initial = Right.EvaluateDate(evalContext);
            if (initial == null)
                return null;

            DateTime result = AdjustDate(DateTimePart, number.Value, initial.Value);
            return result;
        }

        protected override DateTime? OnEvaluateDateTime(EvaluationContext evalContext)
        {
            int? number = Left.EvaluateInt(evalContext);
            if (number == null)
                return null;

            DateTime? utcInitial = Right.EvaluateDateTime(evalContext);
            if (utcInitial == null)
                return null;

            DateTime utcResult;
            if (CalculateInLocalTimeZone(DateTimePart))
            {
                DateTime local = TimeZoneHelper.ConvertToLocalTimeTZ(utcInitial.Value, evalContext.TimeZoneInfo);
                DateTime localResult = AdjustDate(DateTimePart, number.Value, local);
                utcResult = TimeZoneHelper.ConvertToUtcTZ(localResult, evalContext.TimeZoneInfo);
            }
            else
            {
                utcResult = AdjustDate(DateTimePart, number.Value, utcInitial.Value);
            }

            return utcResult;
        }

        protected override DateTime? OnEvaluateTime(EvaluationContext evalContext)
        {
            int? number = Left.EvaluateInt(evalContext);
            if (number == null)
                return null;

            DateTime? initial = Right.EvaluateTime(evalContext);
            if (initial == null)
                return null;

            DateTime rawResult = AdjustDate(DateTimePart, number.Value, initial.Value);

            // Ensure result is cropped to the 1753-01-01 date.
            DateTime croppedResult = TimeType.NewTime(rawResult.TimeOfDay);
            return croppedResult;
        }

        public static bool CalculateInLocalTimeZone(DateTimeParts part)
        {
            switch (part)
            {
                case DateTimeParts.Year:
                case DateTimeParts.Quarter:
                case DateTimeParts.Month:
                    return true;
                default:
                    return false;
            }
        }

        static DateTime AdjustDate(DateTimeParts part, int number, DateTime value)
        {
            DateTime result;
            switch (part)
            {
                case DateTimeParts.Year:
                    result = value.AddYears(number);
                    break;
                case DateTimeParts.Quarter:
                    result = value.AddMonths(number * 3);
                    break;
                case DateTimeParts.Month:
                    result = value.AddMonths(number);
                    break;
                case DateTimeParts.Day:
                    result = value.AddDays(number);
                    break;
                case DateTimeParts.Week:
                    result = value.AddDays(number * 7);
                    break;
                case DateTimeParts.Hour:
                    result = value.AddHours(number);
                    break;
                case DateTimeParts.Minute:
                    result = value.AddMinutes(number);
                    break;
                case DateTimeParts.Second:
                    result = value.AddSeconds(number);
                    break;
                // Because SQL just treats them the same as 'day' anyway.
                case DateTimeParts.DayOfYear:
                case DateTimeParts.Weekday:
                default:
                    throw new InvalidOperationException(part.ToString());
            }
            return result;
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            var result = new CalculationExpression
            {
                Operator = CalculationOperator.DateAdd,
                DateTimePart = DateTimePart,
                InputType = InputType.Type,
                Expressions = new List<ScalarExpression>
                {
                    // Caution.. CalculationOperator.DateAdd was implemented backwards
                    Right.BuildQuery(context),
                    Left.BuildQuery(context)
                }
            };
            return result;
        }

        public override void OnStaticValidation(BuilderSettings settings, Irony.Parsing.ParseTreeNode exceptionNode)
        {
            switch (DateTimePart)
            {
                case DateTimeParts.Hour:
                case DateTimeParts.Minute:
                case DateTimeParts.Second:
                    if (InputType.Type == DataType.Date)
                    {
                        throw ParseExceptionHelper.New(string.Format("Cannot use '{0}' with date data.", DateTimePart), exceptionNode.ChildNodes[1]);
                    }
                    break;
                default:
                    if (InputType.Type == DataType.Time)
                    {
                        throw ParseExceptionHelper.New(string.Format("Cannot use '{0}' with time data.", DateTimePart), exceptionNode.ChildNodes[1]);
                    }
                    break;
            }
        }
    }

}
