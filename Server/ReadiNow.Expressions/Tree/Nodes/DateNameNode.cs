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
using ReadiNow.Expressions.Evaluation;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Parser;

namespace ReadiNow.Expressions.Tree.Nodes
{
    /// <summary>
    /// Implementation consistent with T-SQL DateName
    /// http://msdn.microsoft.com/en-au/library/ms174395.aspx
    /// </summary>
    class DateNameNode : UnaryOperatorNode, IDateTimePartFunction
    {
        public DateTimeParts DateTimePart { get; set; }

        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            DateTime? argument;

            switch (InputType.Type)
            {
                case DataType.Date:
                    argument = Argument.EvaluateDate(evalContext);
                    if (argument == null)
                        return null;
                    break;

                case DataType.Time:
                    argument = Argument.EvaluateTime(evalContext);
                    if (argument == null)
                        return null;
                    break;

                case DataType.DateTime:
                    DateTime? utcArgument = Argument.EvaluateDateTime(evalContext);
                    if (utcArgument == null)
                        return null;

                    argument = TimeZoneHelper.ConvertToLocalTimeTZ(utcArgument.Value, evalContext.TimeZoneInfo);
                    break;

                default:
                    throw new InvalidOperationException(ResultType.Type.ToString());
            }

            string result = CalcDateName(DateTimePart, argument.Value);
            return result;
        }

        static string CalcDateName(DateTimeParts part, DateTime value)
        {
            // Attempting to match T-SQL behavior
            // For any given datepart, if the values fall in the same part, then the result is zero. Then add 1 for each additional part that elapses.
            // E.g. jan-12 to dec-12 is zero years difference, but dec-11 to jan-13 is two years difference, even thought the gap is only 4 months longer.

            string result;
            switch (part)
            {
                case DateTimeParts.Year:
                    result = value.Year.ToString();
                    break;
                case DateTimeParts.Quarter:
                    result = QuarterNode.CalculateQuarter(value).ToString();
                    break;
                case DateTimeParts.Month:
                    result = value.ToString("MMMM");
                    break;
                case DateTimeParts.Day:
                    result = value.Day.ToString();
                    break;
                case DateTimeParts.Week:
                    result = WeekNode.CalculateWeek(value).ToString();
                    break;
                case DateTimeParts.Hour:
                    result = value.Hour.ToString();
                    break;
                case DateTimeParts.Minute:
                    result = value.Minute.ToString();
                    break;
                case DateTimeParts.Second:
                    result = value.Second.ToString();
                    break;
                case DateTimeParts.DayOfYear:
                    result = value.DayOfYear.ToString();
                    break;
                case DateTimeParts.Weekday:
                    result = value.ToString("dddd");
                    break;
                default:
                    throw new InvalidOperationException(part.ToString());
            }
            return result;
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            var result = new CalculationExpression
            {
                Operator = CalculationOperator.DateName,
                DateTimePart = DateTimePart,
                Expressions = new List<ScalarExpression>
                {
                    Argument.BuildQuery(context)
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
