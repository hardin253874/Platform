// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;

namespace ReadiNow.Expressions.Tree.Nodes
{
    class AllInstancesNode : EntityNode
    {
        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            IEntity result = evalContext.GetCurrentEntity(this);
            return result;
        }

        protected override IEnumerable<EntitySetHandle> OnVisitItems(EvaluationContext evalContext)
        {
            // The type of instances to load
            IEntity type = Argument.EvaluateEntity(evalContext);

            // Get instances
            IEnumerable<IEntity> instances = Entity.GetInstancesOfType(new EntityRef(type.Id), true);

            foreach (var instance in instances)
            {
                var result = new EntitySetHandle
                {
                    Entity = instance,
                    Expression = this
                };
                yield return result;
            }
        }

    }

}
