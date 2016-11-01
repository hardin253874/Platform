// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Globalization;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Utc;

namespace ReadiNow.Expressions.Tree.Nodes
{
    class ConstantNode : ZeroArgumentNode
    {
        public object Value { get; set; }

        protected override T OnEvaluateGeneric<T>(EvaluationContext evalContext, Func<ExpressionNode, T> childEvaluator)
        {
            T result = (T)Value;
            return result;
        }

        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            throw new InvalidOperationException();
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            if (ResultType.IsList)
                throw new InvalidOperationException();

            var result = new LiteralExpression
            {
                Value = TypedValueHelper.FromDataType(ResultType.Type, Value)
            };
            return result;            
        }

        protected override int? OnStaticEvaluateInt(CompileContext context)
        {
            var result = (int)Value;
            return result;
        }

        protected override DateTime? OnEvaluateDateTime(EvaluationContext evalContext)
        {
            // Note: The 'value' stored is the exact value entered by the user.
            // We need to treat that value as local time, but return UTC, so convert here.
            // Note: we can't convert at parse time, because we don't have the timezone at that point.

            var localDate = (DateTime)Value;
            var utcDate = TimeZoneHelper.ConvertToUtcTZ(localDate, evalContext.TimeZoneInfo);
            return utcDate;
        }
        

    }
}
