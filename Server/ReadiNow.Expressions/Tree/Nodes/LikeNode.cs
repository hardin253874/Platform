// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Parser;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Tree.Nodes
{
    /// <summary>
    /// Searches a string.
    /// </summary>
    [QueryEngine(ComparisonOperator.Like)]
    class LikeNode : BinaryOperatorNode
    {
        public bool InvertResult { get; set; }

        /// <summary>
        /// Set the token name.
        /// </summary>
        public override void SetToken(string token)
        {
            InvertResult = Keywords.Equals(token, Keywords.NotLike);
        }
        
        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            var toSearch = Left.EvaluateString(evalContext);
            if (toSearch == null)
                return null;

            var toFind = Right.EvaluateString(evalContext);
            if (toFind == null)
                return null;
            if (toFind.Length == 0 || toFind == "%")
                return true;

            bool anyStart = toFind.StartsWith("%");
            bool anyEnd = toFind.EndsWith("%");
            bool any = anyStart && anyEnd;
            bool result;

            if (any)
            {
                toFind = toFind.Substring(1, toFind.Length - 2);
                result = toSearch.IndexOf(toFind, StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            else if (anyStart)
            {
                toFind = toFind.Substring(1);
                result = toSearch.EndsWith(toFind, StringComparison.InvariantCultureIgnoreCase);
            }
            else if (anyEnd)
            {
                toFind = toFind.Substring(0, toFind.Length - 1);
                result = toSearch.StartsWith(toFind, StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                result = string.Compare(toSearch, toFind, StringComparison.InvariantCultureIgnoreCase) == 0;
            }

            if (InvertResult)
                result = !result;

            return result;
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            var result = new ComparisonExpression
            {
                Operator = InvertResult ? ComparisonOperator.NotLike : ComparisonOperator.Like,
                Expressions = new List<ScalarExpression>
                {
                    Left.BuildQuery(context),
                    Right.BuildQuery(context)
                }
            };
            return result;
        }


    }
}
