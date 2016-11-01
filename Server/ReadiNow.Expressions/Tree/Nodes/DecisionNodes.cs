// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;

namespace ReadiNow.Expressions.Tree.Nodes
{
    /// <summary>
    /// IIF(condition, result-if-true, result-if-false)
    /// </summary>
    class IifNode : FunctionNode
    {
        protected override T OnEvaluateGeneric<T>(EvaluationContext evalContext, Func<ExpressionNode, T> childEvaluator)
        {
            // Get arguments
            bool? condition = Arguments[0].EvaluateBool(evalContext);
            if (condition == null) return default(T);   //null

            ExpressionNode chosenNode = condition == true ? Arguments[1] : Arguments[2];
            T result = childEvaluator(chosenNode);
            return result;
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            ScalarExpression condition = Arguments[0].BuildQuery(context);
            ScalarExpression ifTrue = Arguments[1].BuildQuery(context);
            ScalarExpression ifFalse = Arguments[2].BuildQuery(context);

            var result = new IfElseExpression
            {
                BooleanExpression = condition,
                IfBlockExpression = ifTrue,
                ElseBlockExpression = ifFalse
            };
            return result;
        }
    }

    /// <summary>
    /// An IIF condition that has an entity result type
    /// </summary>
    /// <remarks>
    /// Needs to be a separate class to IifNode, because this must derive from EntityNode, which is problematic if the result is not an entity.
    /// </remarks>
    class IifEntityNode : EntityNode
    {
        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            // Get arguments
            bool? condition = Arguments[0].EvaluateBool(evalContext);

            ExpressionNode chosenNode = condition == true ? Arguments[1] : Arguments[2];
            IEntity result = chosenNode.EvaluateEntity(evalContext);
            return result;
        }
    }

    /// <summary>
    /// IsNull(value, value-if-null)
    /// </summary>
    [QueryEngine(CalculationOperator.IsNull)]
    class IsNullNode : FunctionNode
    {
        protected override T OnEvaluateGeneric<T>(EvaluationContext evalContext, Func<ExpressionNode, T> childEvaluator)
        {
            // Evaluate the main value
            T value = childEvaluator(Arguments[0]);
            
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (value != null)
                return value;
            // ReSharper restore CompareNonConstrainedGenericWithNull

            T defaultValue = childEvaluator(Arguments[1]);
            return defaultValue;
        }
    }
}
