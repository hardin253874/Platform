// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Tree.Nodes
{
    class AllInstancesNode : EntityNode
    {
        public long EntityTypeId { get; set;  }

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

        /// <summary>
        /// Returns a tree node that is representative of an entity or.
        /// </summary>
        /// <param name="context">Context information about this query building session, including the target structured query object.</param>
        /// <param name="allowReuse">A dedicated node should be returned because the caller intends on disturbing it.</param>
        /// <returns>A query node that can be used within the query.</returns>
        public override SQ.Entity OnBuildQueryNode( QueryBuilderContext context, bool allowReuse )
        {
            if ( context.ParentNode == null )
                throw new Exception( "No context." );

            // New node
            SQ.CustomJoinNode result = new SQ.CustomJoinNode
            {
                EntityTypeId = EntityTypeId,
                JoinPredicateScript = "true"
            };

            context.ParentNode.RelatedEntities.Add( result );
            AddChildNodes( context, result, allowReuse );

            return result;
        }

    }

}
