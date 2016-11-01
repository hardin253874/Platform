// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Tree.Nodes
{
    /// <summary>
    /// Absolute value.
    /// </summary>
    [QueryEngine(CalculationOperator.Abs)]
    class AbsNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            var value = Argument.EvaluateInt(evalContext);

            if (value == null)
                return null;
            return Math.Abs(value.Value);
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            var value = Argument.EvaluateDecimal(evalContext);

            if (value == null)
                return null;
            return Math.Abs(value.Value);
        }
    }


    /// <summary>
    /// Ceiling. Round up.
    /// </summary>
    [QueryEngine(CalculationOperator.Ceiling)]
    class CeilingNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            var value = Argument.EvaluateDecimal(evalContext);

            if (value == null)
                return null;
            return (int)Math.Ceiling(value.Value);
        }
    }


    /// <summary>
    /// Natural Exponent.
    /// </summary>
    [QueryEngine(CalculationOperator.Exp)]
    class ExpNode : UnaryOperatorNode
    {
        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            var value = Argument.EvaluateDecimal(evalContext);

            if (value == null || value.Value == 0)
                return null;
            return (decimal)Math.Exp((double)value.Value);
        }
    }


    /// <summary>
    /// Ceiling. Round down.
    /// </summary>
    [QueryEngine(CalculationOperator.Floor)]
    class FloorNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            var value = Argument.EvaluateDecimal(evalContext);

            if (value == null)
                return null;
            return (int)Math.Floor(value.Value);
        }
    }


    /// <summary>
    /// Logarithm base 10.
    /// </summary>
    [QueryEngine(CalculationOperator.Log10)]
    class Log10Node : UnaryOperatorNode
    {
        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            var value = Argument.EvaluateDecimal(evalContext);

            if (value == null || value.Value == 0)
                return null;
            return (decimal)Math.Log10((double)value.Value);
        }
    }


    /// <summary>
    /// Natural Logarithm.
    /// </summary>
    [QueryEngine(CalculationOperator.Log)]
    class LogNode : FunctionNode
    {
        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            var value = Arguments[0].EvaluateDecimal(evalContext);
            if (value == null || value.Value == 0)
                return null;

            if (Arguments.Count == 1)
            {
                return (decimal)Math.Log((double)value.Value);
            }

            var baseValue = Arguments[1].EvaluateDecimal(evalContext);
            if (baseValue == null || baseValue <= 0)
                return null;

            return (decimal)Math.Log((double)value.Value, (double)baseValue.Value);
        }
    }


    /// <summary>
    /// Raise to power.
    /// </summary>
    [QueryEngine(CalculationOperator.Power)]
    class PowerNode : BinaryOperatorNode
    {
        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            decimal? value = Left.EvaluateDecimal(evalContext);
            decimal? exponent = Right.EvaluateDecimal(evalContext);

            if (value == null || exponent == null)
                return null;
            return (decimal)Math.Pow((double)value, (double)exponent);
        }
    }


    /// <summary>
    /// Rounds a number to a specified precision.
    /// </summary>
    [QueryEngine(CalculationOperator.Round)]
    class RoundNode : BinaryOperatorNode
    {
        internal override int? OnDetermineDecimalPlaces(CompileContext context)
        {
            try
            {
                int? precision = Right.StaticEvaluateInt(context);
                if (precision != null && precision.Value < 0)
                    return 0;
                return precision;
            }
            catch
            {
                return base.OnDetermineDecimalPlaces(context);
            }
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            decimal? value = Left.EvaluateDecimal(evalContext);
            int? precision = Right.EvaluateInt(evalContext);

            if (value == null || precision == null)
                return null;

            // Emulate T-SQL behavior
            if (precision.Value < 0)
            {
                int mult = 1;
                for (int i = 0; i < -precision.Value; i++)
                    mult *= 10;
                
                decimal result = Math.Round(value.Value / mult, 0, MidpointRounding.AwayFromZero);
                decimal result2 = result * mult;
                return result2;
            }
            else
            {
                decimal result = Math.Round(value.Value, precision.Value, MidpointRounding.AwayFromZero);
                return result;
            }
        }

        public static ScalarExpression CastPrecision(ScalarExpression decimalExpression, int precision)
        {
            var result = new CalculationExpression
            {
                Operator = CalculationOperator.Cast,
                CastType = DatabaseType.DecimalType,
                Expressions = new List<ScalarExpression>
                {
                    decimalExpression,
                    new LiteralExpression { Value = new EDC.ReadiNow.Metadata.TypedValue(precision) }
                }
            };
            return result;
        }
    }


    /// <summary>
    /// Sign. Returns 1,0,-1 for positive, zero, negative.
    /// </summary>
    [QueryEngine(CalculationOperator.Sign)]
    class SignNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            if (InputType.Type == DataType.Int32)
            {
                var value = Argument.EvaluateInt(evalContext);
                if (value == null)
                    return null;
                return Math.Sign(value.Value);
            }
            else
            {
                var value = Argument.EvaluateDecimal(evalContext);
                if (value == null)
                    return null;
                return Math.Sign(value.Value);
            }
        }
    }


    /// <summary>
    /// Square the input.
    /// </summary>
    [QueryEngine(CalculationOperator.Square)]
    class SquareNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            var value = Argument.EvaluateInt(evalContext);

            if (value == null)
                return null;
            return value * value;
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            var value = Argument.EvaluateDecimal(evalContext);

            if (value == null)
                return null;
            return value * value;
        }
    }


    /// <summary>
    /// Squareroot of the input.
    /// </summary>
    [QueryEngine(CalculationOperator.Sqrt)]
    class SqrtNode : UnaryOperatorNode
    {
        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            var value = Argument.EvaluateDecimal(evalContext);

            if (value == null)
                return null;
            if (value < 0)
                return null;
            return (decimal)Math.Sqrt((double)value);
        }
    }
}
