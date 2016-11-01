// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Tree.Nodes
{
    [QueryEngine(LogicalOperator.And)]
    class AndNode : BinaryOperatorNode
    {
        protected override bool? OnEvaluateBool(Evaluation.EvaluationContext evalContext)
        {
            // Truth table:
            // F  F  -> F
            // F  T  -> F
            // F  n  -> F
            // T  F  -> F
            // T  T  -> T
            // T  n  -> n
            // n  F  -> F
            // n  T  -> n
            // n  n  -> n
            // Rational: null is interpreted as 'unknown'. If one side is false, then the result is false even if the other is unknown.

            // Short-circuiting rule:
            // Right-hand does not get evaluated if left hand is false, as the result is unaffected.

            bool? left = Left.EvaluateBool(evalContext);
            if (left == false)
                return false;

            bool? right = Right.EvaluateBool(evalContext);
            if (right == false)
                return false;

            if (left == true && right == true)
                return true;

            return null;
        }
    }

    [QueryEngine(LogicalOperator.Or)]
    class OrNode : BinaryOperatorNode
    {
        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            // Truth table:
            // T  T  -> T
            // T  F  -> T
            // T  n  -> T
            // F  T  -> T
            // F  F  -> F
            // F  n  -> n
            // n  T  -> T
            // n  F  -> n
            // n  n  -> n
            // Rational: null is interpreted as 'unknown'. If one side is true, then the result is true even if the other is unknown.

            // Short-circuiting rule:
            // Right-hand does not get evaluated if left hand is true, as the result is unaffected.

            bool? left = Left.EvaluateBool(evalContext);
            if (left == true)
                return true;

            bool? right = Right.EvaluateBool(evalContext);
            if (right == true)
                return true;

            if (left == false && right == false)
                return false;

            return null;
        }
    }

    [QueryEngine(LogicalOperator.Not)]
    class NotNode : UnaryOperatorNode
    {
        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            // Truth table:
            // false -> true
            // true  -> false
            // null  -> true

            bool? argument = Argument.EvaluateBool(evalContext);

            var result = argument != true;
            return result;
        }
    }
}
