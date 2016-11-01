// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Database.Types;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.Metadata.Query.Structured;
using ReadiNow.Expressions.Evaluation;

namespace ReadiNow.Expressions.Tree.Nodes
{
    interface IDateTimePartFunction
    {
        DateTimeParts DateTimePart { get; set; }
    }

    [QueryEngine(CalculationOperator.DateFromParts)]
    class DateFromPartsNode : FunctionNode
    {
        protected override DateTime? OnEvaluateDate(EvaluationContext evalContext)
        {
            int? year = Arguments[0].EvaluateInt(evalContext);
            if (year == null) return null;

            int? month = Arguments[1].EvaluateInt(evalContext);
            if (month == null) return null;

            int? day = Arguments[2].EvaluateInt(evalContext);
            if (day == null) return null;

            var result = new DateTime(year.Value, month.Value, day.Value);
            return result;
        }
    }


    [QueryEngine(CalculationOperator.TimeFromParts)]
    class TimeFromPartsNode : FunctionNode
    {
        protected override DateTime? OnEvaluateTime(EvaluationContext evalContext)
        {
            int? hour = Arguments[0].EvaluateInt(evalContext);
            if (hour == null) return null;

            int? minute = Arguments[1].EvaluateInt(evalContext);
            if (minute == null) return null;

            int? second = Arguments[2].EvaluateInt(evalContext);
            if (second == null) return null;

            DateTime result = TimeType.NewTime(hour.Value, minute.Value, second.Value);
            return result;
        }
    }


    [QueryEngine(CalculationOperator.DateTimeFromParts)]
    class DateTimeFromPartsNode : FunctionNode
    {
        protected override DateTime? OnEvaluateDateTime(EvaluationContext evalContext)
        {
            int? year = Arguments[0].EvaluateInt(evalContext);
            if (year == null) return null;

            int? month = Arguments[1].EvaluateInt(evalContext);
            if (month == null) return null;

            int? day = Arguments[2].EvaluateInt(evalContext);
            if (day == null) return null;

            int? hour = Arguments[3].EvaluateInt(evalContext);
            if (hour == null) return null;

            int? minute = Arguments[4].EvaluateInt(evalContext);
            if (minute == null) return null;

            int? second = Arguments[5].EvaluateInt(evalContext);
            if (second == null) return null;

            var localDateTime = new DateTime(year.Value, month.Value, day.Value, hour.Value, minute.Value, second.Value);
            var utcDateTime = TimeZoneHelper.ConvertToUtcTZ(localDateTime, evalContext.TimeZoneInfo);
            return utcDateTime;
        }
    }


    [QueryEngine(CalculationOperator.Year)]
    class YearNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // Note: call is to EvaluateDate. If argument was a DateTime, then it would already be casted, which would adjust from UTC to local for us already.
            DateTime? date = Argument.EvaluateDate(evalContext);
            if (date == null) return null;

            return date.Value.Year;
        }
    }


    [QueryEngine(CalculationOperator.Month)]
    class MonthNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // Note: call is to EvaluateDate. If argument was a DateTime, then it would already be casted, which would adjust from UTC to local for us already.
            DateTime? date = Argument.EvaluateDate(evalContext);
            if (date == null) return null;

            return date.Value.Month;
        }
    }


    [QueryEngine(CalculationOperator.Day)]
    class DayNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // Note: call is to EvaluateDate. If argument was a DateTime, then it would already be casted, which would adjust from UTC to local for us already.
            DateTime? date = Argument.EvaluateDate(evalContext);
            if (date == null) return null;

            return date.Value.Day;
        }
    }


    [QueryEngine(CalculationOperator.Quarter)]
    class QuarterNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // Note: call is to EvaluateDate. If argument was a DateTime, then it would already be casted, which would adjust from UTC to local for us already.
            DateTime? date = Argument.EvaluateDate(evalContext);
            if (date == null) return null;

            return CalculateQuarter(date.Value);
        }

        public static int CalculateQuarter(DateTime date)
        {
            int month = date.Month;
            int quarter = (month - 1) / 3 + 1;
            return quarter;
        }
    }


    [QueryEngine(CalculationOperator.DayOfYear)]
    class DayOfYearNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // Note: call is to EvaluateDate. If argument was a DateTime, then it would already be casted, which would adjust from UTC to local for us already.
            DateTime? date = Argument.EvaluateDate(evalContext);
            if (date == null) return null;

            return date.Value.DayOfYear;
        }
    }


    [QueryEngine(CalculationOperator.WeekDay)]
    class WeekdayNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // Note: call is to EvaluateDate. If argument was a DateTime, then it would already be casted, which would adjust from UTC to local for us already.
            DateTime? date = Argument.EvaluateDate(evalContext);
            if (date == null) return null;

            int dayOfWeek = 1 + (int)date.Value.DayOfWeek; // so that Sunday=1 and Saturday=6, same as T-SQL default
            return dayOfWeek;
        }
    }


    [QueryEngine(CalculationOperator.Week)]
    class WeekNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // Note: call is to EvaluateDate. If argument was a DateTime, then it would already be casted, which would adjust from UTC to local for us already.
            DateTime? date = Argument.EvaluateDate(evalContext);
            if (date == null) return null;

            return CalculateWeek(date.Value);
        }

        public static int CalculateWeek(DateTime date)
        {
            // Calculate week number of the year in the same manner as SQL
            // First week is week one. Second week starts on the next Sunday
            DateTime first = new DateTime(date.Year, 1, 1);
            int firstDate = (int)first.DayOfWeek;
            int adjustedOffset = date.DayOfYear + firstDate - 1;
            int week = adjustedOffset / 7 + 1;  // first week of year is 1
            return week;
        }
    }


    [QueryEngine(CalculationOperator.Hour)]
    class HourNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // Note: call is to EvaluateTime. If argument was a DateTime, then it would already be casted, which would adjust from UTC to local for us already.
            DateTime? date = Argument.EvaluateTime(evalContext);
            if (date == null) return null;

            return date.Value.Hour;
        }
    }


    [QueryEngine(CalculationOperator.Minute)]
    class MinuteNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // Note: call is to EvaluateTime. If argument was a DateTime, then it would already be casted, which would adjust from UTC to local for us already.
            DateTime? date = Argument.EvaluateTime(evalContext);
            if (date == null) return null;

            return date.Value.Minute;
        }
    }


    [QueryEngine(CalculationOperator.Second)]
    class SecondNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // Note: call is to EvaluateTime. If argument was a DateTime, then it would already be casted, which would adjust from UTC to local for us already.
            DateTime? date = Argument.EvaluateTime(evalContext);
            if (date == null) return null;

            return date.Value.Second;
        }
    }


    [QueryEngine(CalculationOperator.TodayDate)]
    class GetDateNode : ZeroArgumentNode
    {
        protected override DateTime? OnEvaluateDate(EvaluationContext evalContext)
        {
            // Date is always represented in local timezone.
            DateTime utcNow = DateTime.UtcNow;
            DateTime localNow = TimeZoneHelper.ConvertToLocalTimeTZ(utcNow, evalContext.TimeZoneInfo);
            return localNow.Date;
        }
    }


    [QueryEngine(CalculationOperator.TodayDateTime)]
    class GetDateTimeNode : ZeroArgumentNode
    {
        protected override DateTime? OnEvaluateDateTime(EvaluationContext evalContext)
        {
            // DateTime is always represented as UTC.
            DateTime utcNow = DateTime.UtcNow;
            return utcNow;
        }
    }


    [QueryEngine(CalculationOperator.Time)]
    class GetTimeNode : ZeroArgumentNode
    {
        protected override DateTime? OnEvaluateTime(EvaluationContext evalContext)
        {
            // Date is always represented in local timezone.
            DateTime utcNow = DateTime.UtcNow;
            DateTime localNow = TimeZoneHelper.ConvertToLocalTimeTZ(utcNow, evalContext.TimeZoneInfo);
            return TimeType.NewTime(localNow.TimeOfDay);
        }
    }
        
}
