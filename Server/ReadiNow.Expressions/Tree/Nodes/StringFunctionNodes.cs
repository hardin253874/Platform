// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using System.Linq;
using EDC.Database;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Tree.Nodes
{
    /// <summary>
    /// Charindex. 1-based offset for SQL compatibility.
    /// </summary>
    [QueryEngine(CalculationOperator.Charindex)]
    class CharIndexNode : FunctionNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            // Required output:
            //  charindex('def','abcdefghi') -> 4
            //  charindex('z','abcdefghi')   -> 0
            //  charindex('def','abcdefdefghi',4) -> 4
            //  charindex('def','abcdefdefghi',5) -> 7

            // Get arguments
            string toFind = Arguments[0].EvaluateString(evalContext);
            if (toFind == null) return null;

            string toSearch = Arguments[1].EvaluateString(evalContext);
            if (toSearch == null) return null;

            int? startAt = null;
            if (Arguments.Count > 2)
            {
                startAt = Arguments[2].EvaluateInt(evalContext);
            }

            // Evaluate
            if (startAt == null)
            {
                var result = toSearch.IndexOf(toFind, StringComparison.InvariantCulture) + 1;
                return result;
            }
            else
            {
                var result = toSearch.IndexOf(toFind, startAt.Value - 1, StringComparison.InvariantCulture) + 1;
                return result;
            }
        }
    }


    /// <summary>
    /// Len
    /// </summary>
    [QueryEngine(CalculationOperator.StringLength)]
    class LenNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            string value = Argument.EvaluateString(evalContext);
            if (value == null) return null;

            int result = value.Length;
            return result;
        }
    }


    /// <summary>
    /// Left
    /// </summary>
    [QueryEngine(CalculationOperator.Left)]
    class LeftNode : BinaryOperatorNode
    {
        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            // Get arguments
            string value = Left.EvaluateString(evalContext);
            if (value == null) return null;
            
            int? count = Right.EvaluateInt(evalContext);
            if (count == null) return null;

            // Evaluate
            if (count < 0)
                return string.Empty;
            if (count >= value.Length)
                return value;

            string result = value.Substring(0, count.Value);
            return result;
        }
    }


    /// <summary>
    /// Replace
    /// </summary>
    [QueryEngine(CalculationOperator.Replace)]
    class ReplaceNode : FunctionNode
    {
        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            // Get arguments
            string value = Arguments[0].EvaluateString(evalContext);
            if (value == null) return null;

            string find = Arguments[1].EvaluateString(evalContext);
            if (find == null) return null;

            string replace = Arguments[2].EvaluateString(evalContext);
            if (replace == null) return null;

            // Evaluate
            string result = value.Replace(find, replace);
            return result;
        }
    }


    /// <summary>
    /// Right
    /// </summary>
    [QueryEngine(CalculationOperator.Right)]
    class RightNode : BinaryOperatorNode
    {
        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            // Get arguments
            string value = Left.EvaluateString(evalContext);
            if (value == null) return null;

            int? count = Right.EvaluateInt(evalContext);
            if (count == null) return null;

            // Evaluate
            if (count < 0 || value.Length == 0)
                return string.Empty;
            if (count >= value.Length)
                return value;

            string result = value.Substring(value.Length - count.Value, count.Value);
            return result;
        }
    }


    /// <summary>
    /// Substring. 1-based offset for SQL compatibility.
    /// </summary>
    [QueryEngine(CalculationOperator.Substring)]
    class SubstringNode : FunctionNode
    {
        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            // Get arguments
            string value = Arguments[0].EvaluateString(evalContext);
            if (value == null) return null;

            int? start = Arguments[1].EvaluateInt(evalContext);
            if (start == null) return null;

            int? take = Arguments[2].EvaluateInt(evalContext);
            if (take == null) return null;

            // Evaluate
            if (start < 1 || take < 1)
                return null;

            if (start + take > value.Length + 1)
            {
                return value.Substring(start.Value - 1);
            }
            else
            {
                return value.Substring(start.Value - 1, take.Value);
            }
        }
    }


    /// <summary>
    /// ToLower
    /// </summary>
    [QueryEngine(CalculationOperator.ToLower)]
    class ToLowerNode : UnaryOperatorNode
    {
        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            string value = Argument.EvaluateString(evalContext);
            if (value == null) return null;

            string result = value.ToLowerInvariant();
            return result;
        }
    }


    /// <summary>
    /// ToUpper
    /// </summary>
    [QueryEngine(CalculationOperator.ToUpper)]
    class ToUpperNode : UnaryOperatorNode
    {
        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            string value = Argument.EvaluateString(evalContext);
            if (value == null) return null;

            string result = value.ToUpperInvariant();
            return result;
        }
    }

}
