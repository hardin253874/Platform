// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Tree.Nodes
{
    [QueryEngine(CalculationOperator.Add)]
    class AddNode : BinaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            var left = Left.EvaluateInt(evalContext);
            if (left == null) return null;

            var right = Right.EvaluateInt(evalContext);
            if (right == null) return null;

            var result = left.Value + right.Value;
            return result;
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            var left = Left.EvaluateDecimal(evalContext);
            if (left == null) return null;

            var right = Right.EvaluateDecimal(evalContext);
            if (right == null) return null;

            var result = left.Value + right.Value;
            return result;
        }
    }


    [QueryEngine(CalculationOperator.Subtract)]
    class SubtractNode : BinaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            var left = Left.EvaluateInt(evalContext);
            if (left == null) return null;

            var right = Right.EvaluateInt(evalContext);
            if (right == null) return null;

            var result = left.Value - right.Value;
            return result;
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            var left = Left.EvaluateDecimal(evalContext);
            if (left == null) return null;

            var right = Right.EvaluateDecimal(evalContext);
            if (right == null) return null;

            var result = left.Value - right.Value;
            return result;
        }
    }


    [QueryEngine(CalculationOperator.Multiply)]
    class MultiplyNode : BinaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            var left = Left.EvaluateInt(evalContext);
            if (left == null) return null;

            var right = Right.EvaluateInt(evalContext);
            if (right == null) return null;

            var result = left.Value * right.Value;
            return result;
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            var left = Left.EvaluateDecimal(evalContext);
            if (left == null) return null;

            var right = Right.EvaluateDecimal(evalContext);
            if (right == null) return null;

            var result = left.Value * right.Value;
            return result;
        }
    }


    [QueryEngine(CalculationOperator.Divide)]
    class DivideNode : BinaryOperatorNode
    {
        internal override int? OnDetermineDecimalPlaces(CompileContext context)
        {
            // Return at least the default number of decimal places.

            int? suggested = base.OnDetermineDecimalPlaces(context);
            int defaultDp = context.GetDecimalPlaces(ResultType);

            int result = Math.Max(suggested ?? defaultDp, defaultDp);
            return result;
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            decimal? left = Left.EvaluateDecimal(evalContext);
            if (left == null) return null;

            decimal? right = Right.EvaluateDecimal(evalContext);
            if (right == null) return null;

            if (right.Value == 0)
                return null;

            var result = left.Value / right.Value;
            return result;
        }
    }


    [QueryEngine(CalculationOperator.Negate)]
    class NegateNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            var value = Argument.EvaluateInt(evalContext);
            if (value == null) return null;

            return -value;
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            var value = Argument.EvaluateDecimal(evalContext);
            if (value == null) return null;

            return -value;
        }
    }


    [QueryEngine(CalculationOperator.Modulo)]
    class ModuloNode : BinaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            int? left = Left.EvaluateInt(evalContext);
            if (left == null) return null;

            int? right = Right.EvaluateInt(evalContext);
            if (right == null) return null;

            var result = left.Value % right.Value;
            return result;
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            decimal? left = Left.EvaluateDecimal(evalContext);
            if (left == null) return null;

            decimal? right = Right.EvaluateDecimal(evalContext);
            if (right == null) return null;

            var result = left.Value % right.Value;
            return result;
        }
    }

}
