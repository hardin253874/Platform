// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using EDC.Database;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Tree.Nodes
{
    public abstract class ComparisonNode : BinaryOperatorNode
    {
        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            ExpressionNode arg0 = Left;
            ExpressionNode arg1 = Right;

            DataType dataType = InputType.Type;

            int? comparison = CompareExpressions(evalContext, dataType, arg0, arg1);
            if (comparison == null)
                return null;

            bool result = InterpretComparison(comparison.Value);
            return result;
        }

        protected abstract bool InterpretComparison(int comparison);

        public static int? CompareExpressions(EvaluationContext evalContext, DataType dataType, ExpressionNode left, ExpressionNode right)
        {
            int? result;
            switch (dataType)
            {
                case DataType.String:
                case DataType.Xml:
                    var string0 = left.EvaluateString(evalContext);
                    var string1 = right.EvaluateString(evalContext);
                    result = OnCompare(string0, string1);
                    break;

                case DataType.Int32:
                    var int0 = left.EvaluateInt(evalContext);
                    var int1 = right.EvaluateInt(evalContext);
                    result = OnCompare(int0, int1);
                    break;

                case DataType.Currency:
                case DataType.Decimal:
                    var dec0 = left.EvaluateDecimal(evalContext);
                    var dec1 = right.EvaluateDecimal(evalContext);
                    result = OnCompare(dec0, dec1);
                    break;

                case DataType.Date:
                    var date0 = left.EvaluateDate(evalContext);
                    var date1 = right.EvaluateDate(evalContext);
                    result = OnCompare(date0, date1);
                    break;

                case DataType.Time:
                    var time0 = left.EvaluateTime(evalContext);
                    var time1 = right.EvaluateTime(evalContext);
                    result = OnCompare(time0, time1);
                    break;

                case DataType.DateTime:
                    var dt0 = left.EvaluateDateTime(evalContext);
                    var dt1 = right.EvaluateDateTime(evalContext);
                    result = OnCompare(dt0, dt1);
                    break;

                case DataType.Entity:
                    var e0 = left.EvaluateEntity(evalContext);
                    var e1 = right.EvaluateEntity(evalContext);
                    if (e0 == null || e1 == null)
                        result = null;
                    else
                        result = OnCompare((long?)e0.Id, (long?)e1.Id);
                    break;

                case DataType.Bool:
                    var bool0 = left.EvaluateBool(evalContext);
                    var bool1 = right.EvaluateBool(evalContext);
                    result = OnCompare(bool0, bool1);
                    break;

                case DataType.Guid:
                    var guid0 = left.EvaluateBool(evalContext);
                    var guid1 = right.EvaluateBool(evalContext);
                    result = OnCompare(guid0, guid1);
                    break;

                case DataType.Binary:
                case DataType.None:
                default:
                    throw new InvalidOperationException();
            }
            return result;
        }

        private static int? OnCompare(string left, string right)
        {
            if (left == null || right == null)
                return null;
            int comparison = string.Compare(left, right, StringComparison.InvariantCultureIgnoreCase);
            return comparison;
        }

        private static int? OnCompare<T>(T? left, T? right) where T : struct, IComparable<T>
        {
            if (left == null || right == null)
                return null;
            int comparison = left.Value.CompareTo(right.Value);
            return comparison;
        }
        
    }

    [QueryEngine(ComparisonOperator.GreaterThan)]
    public class GreaterThanNode : ComparisonNode
    {
        protected override bool InterpretComparison(int comparison)
        {
            return comparison > 0;
        }
    }

    [QueryEngine(ComparisonOperator.GreaterThanEqual)]
    public class GreaterThanEqualNode : ComparisonNode
    {
        protected override bool InterpretComparison(int comparison)
        {
            return comparison >= 0;
        }
    }

    [QueryEngine(ComparisonOperator.LessThan)]
    public class LessThanNode : ComparisonNode
    {
        protected override bool InterpretComparison(int comparison)
        {
            return comparison < 0;
        }
    }

    [QueryEngine(ComparisonOperator.LessThanEqual)]
    public class LessThanEqualNode : ComparisonNode
    {
        protected override bool InterpretComparison(int comparison)
        {
            return comparison <= 0;
        }
    }

    [QueryEngine(ComparisonOperator.Equal)]
    public class EqualNode : ComparisonNode
    {
        protected override bool InterpretComparison(int comparison)
        {
            return comparison == 0;
        }
    }

    [QueryEngine(ComparisonOperator.NotEqual)]
    public class NotEqualNode : ComparisonNode
    {
        protected override bool InterpretComparison(int comparison)
        {
            return comparison != 0;
        }
    }

}
