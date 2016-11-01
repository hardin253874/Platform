// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;

namespace ReadiNow.Expressions.Tree.Nodes
{
    /// <summary>
    /// Represents a reference to the root level context entity that was passed into the expression engine from the host.
    /// </summary>
    class GetRootContextEntityNode : EntityNode
    {
        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            IEntity result = evalContext.Settings.ContextEntity;
            return result;
        }

        protected override IEnumerable<EntitySetHandle> OnVisitItems(EvaluationContext evalContext)
        {
            var result = new EntitySetHandle
            {
                Entity = evalContext.Settings.ContextEntity,
                Expression = this
            };
            yield return result;
        }

        public override void OnDetermineResultType(BuilderSettings settings)
        {
            ResultType = settings.RootContextType.Clone();
            ResultType.Constant = false;
        }

        public override EDC.ReadiNow.Metadata.Query.Structured.Entity OnBuildQueryNode(QueryBuilderContext context, bool allowReuse)
        {
            if (context.Settings.ContextEntity == null)
                throw new Exception("No context.");

            var contextEntity = context.Settings.ContextEntity;
            EDC.ReadiNow.Metadata.Query.Structured.Entity result;

            if (allowReuse)
            {
                result = contextEntity;
            }
            else
            {
                var proxy = new EDC.ReadiNow.Metadata.Query.Structured.JoinToSelfEntity();
                proxy.EntityTypeId = contextEntity.EntityTypeId;   // just in case
                result = proxy;
            }

            AddChildNodes(context, result, allowReuse);
            return result;
        }
    }
}

