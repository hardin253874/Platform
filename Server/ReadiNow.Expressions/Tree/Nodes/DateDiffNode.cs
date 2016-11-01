// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Utc;
using ReadiNow.Expressions.Parser;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.Compiler;

namespace ReadiNow.Expressions.Tree.Nodes
{

    class DateDiffNode : BinaryOperatorNode, IDateTimePartFunction
    {
        public DateTimeParts DateTimePart { get; set; }

        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // We are calculating right-left.
            DateTime? left;
            DateTime? right;

            switch (InputType.Type)
            {
                case DataType.Date:
                    left = Left.EvaluateDate(evalContext);
                    if (left == null)
                        return null;

                    right = Right.EvaluateDate(evalContext);
                    if (right == null)
                        return null;
                    break;

                case DataType.Time:
                    left = Left.EvaluateTime(evalContext);
                    if (left == null)
                        return null;

                    right = Right.EvaluateTime(evalContext);
                    if (right == null)
                        return null;
                    break;

                case DataType.DateTime:
                    left = Left.EvaluateDateTime(evalContext);
                    if (left == null)
                        return null;

                    right = Right.EvaluateDateTime(evalContext);
                    if (right == null)
                        return null;

                    if (CalculateInLocalTimeZone(DateTimePart))
                    {
                        left = TimeZoneHelper.ConvertToLocalTimeTZ(left.Value, evalContext.TimeZoneInfo);
                        right = TimeZoneHelper.ConvertToLocalTimeTZ(right.Value, evalContext.TimeZoneInfo);
                    }
                    break;

                default:
                    throw new InvalidOperationException(ResultType.Type.ToString());
            }

            int result = CalcDifference(DateTimePart, left.Value, right.Value);
            return result;
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

        static int CalcDifference(DateTimeParts part, DateTime left, DateTime right)
        {
            // Attempting to match T-SQL behavior
            // For any given datepart, if the values fall in the same part, then the result is zero. Then add 1 for each additional part that elapses.
            // E.g. jan-12 to dec-12 is zero years difference, but dec-11 to jan-13 is two years difference, even thought the gap is only 4 months longer.

            int result;
            switch (part)
            {
                case DateTimeParts.Year:
                    result = right.Year - left.Year;
                    break;
                case DateTimeParts.Quarter:
                    result = (right.Year * 12 + right.Month - 1) / 3 - (left.Year * 12 + left.Month - 1) / 3;
                    break;
                case DateTimeParts.Month:
                    result = right.Year * 12 + right.Month - left.Year * 12 - left.Month;
                    break;
                case DateTimeParts.Day:
                    result = (int)((right.Ticks / TimeSpan.TicksPerDay) - (left.Ticks / TimeSpan.TicksPerDay));
                    break;
                case DateTimeParts.Week:
                    result = (int)((right.Ticks / TimeSpan.TicksPerDay + 1) / 7 - (left.Ticks / TimeSpan.TicksPerDay + 1) / 7);
                    break;
                case DateTimeParts.Hour:
                    result = (int)((right.Ticks / TimeSpan.TicksPerHour) - (left.Ticks / TimeSpan.TicksPerHour));
                    break;
                case DateTimeParts.Minute:
                    result = (int)((right.Ticks / TimeSpan.TicksPerMinute) - (left.Ticks / TimeSpan.TicksPerMinute));
                    break;
                case DateTimeParts.Second:
                    result = (int)((right.Ticks / TimeSpan.TicksPerSecond) - (left.Ticks / TimeSpan.TicksPerSecond));
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
                Operator = CalculationOperator.DateDiff,
                DateTimePart = DateTimePart,
                Expressions = new List<ScalarExpression>
                {
                    Left.BuildQuery(context),
                    Right.BuildQuery(context)
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
