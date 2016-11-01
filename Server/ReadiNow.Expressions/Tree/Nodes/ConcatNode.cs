// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Tree.Nodes
{
    /// <summary>
    /// Concatenates (adds) two strings together.
    /// Returns null if both inputs are null.
    /// Returns the non-null input if one string is not null.
    /// </summary>
    [QueryEngine(CalculationOperator.Concatenate)]
    class ConcatNode : BinaryOperatorNode
    {
        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            var left = Left.EvaluateString(evalContext);
            var right = Right.EvaluateString(evalContext);

            if (left == null)
            {
                if (right == null)
                    return null;
                else
                    return right;
            }
            else
            {
                if (right == null)
                    return left;
                else
                    return left + right;
            }
        }

    }
}
