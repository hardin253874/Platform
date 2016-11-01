// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Core;

namespace ReadiNow.Expressions.Tree.Nodes
{
    class ConstantEntityNode : ZeroArgumentNode //EntityNode
    {
        public EntityRef Instance { get; set; }

        public override void RegisterDependencies(ISet<long> identifiedEntities, ISet<long> fields, ISet<long> relationships)
        {
            identifiedEntities.Add(Instance.Id);
        }

        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            IEntity entity = Entity.Get(Instance);
            return entity;
        }
    }

    class ResourceInstanceNode : EntityNode
    {
        public EntityRef Instance { get; set; }

        public override void RegisterDependencies(ISet<long> identifiedEntities, ISet<long> fields, ISet<long> relationships)
        {
            if (Instance != null)
            {
                identifiedEntities.Add(Instance.Id);
            }

            identifiedEntities.Add(ResultType.EntityTypeId);
        }

        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            IEntity result = evalContext.GetCurrentEntity(this);
            return result;
        }

        protected override IEnumerable<EntitySetHandle> OnVisitItems(EvaluationContext evalContext)
        {
            IEntity instance = Instance.Entity;
            if (instance == null)
                yield break;

            var result = new EntitySetHandle
            {
                Entity = instance,
                Expression = this
            };
            yield return result;
        }

        //// TODO: Note: this becomes non-trivial because we essentially need to cross-join in an Entity table for subsequent relationships to join from
        // E.g, consider:  resource(Type,[Person]).Instances.Name + resource(Type,[Building]).Instances.Name
        //protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        //{
        //}
    }

    class ResourceInstanceDynamicNode : EntityNode
    {
        public EntityType EntityType { get; set; }
        public ExpressionNode NameExpression { get; set; }

        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            string name = NameExpression.EvaluateString(evalContext);

            if (name != null)       // whitespace could be a legitimate name ... I guess
            {
                IEntity instance = Factory.ScriptNameResolver.GetInstance(name, EntityType.Id);
                return instance;
            }
            else
                return null;
        }

        protected override IEnumerable<EntitySetHandle> OnVisitItems(EvaluationContext evalContext)
        {
            IEntity instance = OnEvaluateEntity(evalContext);

            if (instance == null)
                yield break;

            var result = new EntitySetHandle
            {
                Entity = instance,
                Expression = this
            };
            yield return result;
        }     }
}
