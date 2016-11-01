// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Expressions;
using System.Collections.Generic;

namespace ReadiNow.Expressions.Tree.Nodes
{
    class AccessFieldNode : BinaryOperatorNode
    {
        public override void RegisterDependencies(ISet<long> identifiedEntities, ISet<long> fields, ISet<long> relationships)
        {
            RegisterChildAs(Right, fields);            
        }

        protected override T OnEvaluateGeneric<T>(EvaluationContext evalContext, Func<ExpressionNode, T> childEvaluator)
        {
            IEntity entity = Left.EvaluateEntity(evalContext);
            IEntity field = Right.EvaluateEntity(evalContext);

            if (entity == null)
                return default(T);

            using (new SecurityBypassContext())
            {
                if (((bool?)field.GetField(Field.IsFieldWriteOnly_Field)) ?? false)
                {
                    return default(T);
                }               
            }

            object value = entity.GetField(field);
            T result = (T)value;
            return result;
        }

        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            throw new InvalidOperationException();
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            // Currently this must be a literal
            var field = Right as ConstantEntityNode;
            if (field == null)
                throw new ParseException("Field required.");

            var queryNode = context.GetNode(Left);

            // Start workaround for #27148
            // If an aggregate operation is performed on a choice field, then we try to treat it like an entity
            // then we have a scenario that can't currently be represented in structured queries.
            // (Because sq aggregate operations get declared as expressions, but if it's an expression we can no longer use it as a sq node for other expressions).
            // This can happen if we use the choice-field in a comparison (e.g. max(choice)>whatever) because the compiler
            // will inject a field lookup to redirect to the ordering field.
            // For now, detect this specific scenario, and pass the aggregate expression directly through because the query engine
            // will defer to the ordering version of the SQL, which will in turn look up the enumOrder field.
            // However, ideally we should also be able to do things like max(choice).someOtherRelationshipOrField
            bool nodeIsAggregate = Left is AggregateNode || Left is EntityTypeCast && Left.Arguments[0] is AggregateNode;
            bool fieldIsEnumOrdering = field.Instance.Id == WellKnownAliases.CurrentTenant.EnumOrder;
            if (nodeIsAggregate && fieldIsEnumOrdering)
            {
                return Left.BuildQuery(context);
            }
            // End workaround

            var result = new ResourceDataColumn
            {
                FieldId = field.Instance,
                NodeId = queryNode.NodeId
            };
            return result;
        }

    }

}
