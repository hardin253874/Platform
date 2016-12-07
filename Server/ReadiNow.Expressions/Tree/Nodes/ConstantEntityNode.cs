// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata;

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

        /// <summary>
        /// Returns a tree node that is representative of an entity or.
        /// </summary>
        /// <param name="context">Context information about this query building session, including the target structured query object.</param>
        /// <param name="allowReuse">A dedicated node should be returned because the caller intends on disturbing it.</param>
        /// <returns>A query node that can be used within the query.</returns>
        public override SQ.Entity OnBuildQueryNode( QueryBuilderContext context, bool allowReuse )
        {
            IEntity instance = Instance.Entity;
            long typeId = instance.TypeIds.First( );

            if ( context.ParentNode == null )
                throw new Exception( "No context." );

            // Look for an existing relationship node in the tree that we can reuse
            SQ.CustomJoinNode result = !allowReuse ? null :
                context.ParentNode
                .RelatedEntities
                .OfType<SQ.CustomJoinNode>( )
                .FirstOrDefault( cj => IsMatch(cj, Instance.Id) && cj.EntityTypeId.Id == typeId );

            // New node
            if ( result == null )
            {
                result = new SQ.CustomJoinNode
                {
                    EntityTypeId = typeId,
                    JoinPredicateScript = "true"
                };
                SQ.ScalarExpression predicate = new SQ.ComparisonExpression
                {
                    Operator = SQ.ComparisonOperator.Equal,
                    Expressions =
                    {
                        new SQ.IdExpression
                        {
                            NodeId = result.NodeId
                        },
                        new SQ.LiteralExpression
                        {
                            Value = TypedValueHelper.FromDataType( ResultType.Type, Instance.Id )
                        }
                    }
                };
                result.Conditions = new List<SQ.ScalarExpression> { predicate };
            }
            context.ParentNode.RelatedEntities.Add( result );
            AddChildNodes( context, result, allowReuse );

            return result;
        }

        private static bool IsMatch( SQ.CustomJoinNode joinNode, long instanceId )
        {
            if ( joinNode.Conditions == null || joinNode.Conditions.Count != 1 )
                return false;
            SQ.ComparisonExpression comp = joinNode.Conditions[ 0 ] as SQ.ComparisonExpression;
            if ( comp == null )
                return false;
            if ( comp.Operator != SQ.ComparisonOperator.Equal )
                return false;
            if ( comp.Expressions.Count != 2 )
                return false;
            if ( !( comp.Expressions[ 0 ] is SQ.IdExpression ) )
                return false;
            SQ.LiteralExpression valueExpr = comp.Expressions[ 1 ] as SQ.LiteralExpression;
            object oValue = valueExpr?.Value?.Value;
            long? value = oValue as long?;
            return value == instanceId;
        }
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
        }
    }
}
