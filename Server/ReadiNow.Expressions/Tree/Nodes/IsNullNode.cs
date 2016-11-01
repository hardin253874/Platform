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
    [QueryEngine(ComparisonOperator.IsNull)]
    class EqualsNullNode : BinaryOperatorNode
    {
        public bool InvertResult { get; set; }

        /// <summary>
        /// Set the token name.
        /// </summary>
        public override void SetToken(string token)
        {
            InvertResult = Keywords.Equals(token, Keywords.IsNot);
        }

        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            object argument = Left.EvaluateObject(evalContext);
            bool result = (argument == null) != InvertResult;
            return result;
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            var result = new ComparisonExpression
            {
                Operator = InvertResult ? ComparisonOperator.IsNotNull : ComparisonOperator.IsNull,
                Expressions = new List<ScalarExpression>
                {
                    Left.BuildQuery(context)
                }
            };
            return result;
        }


    }
}
