// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Metadata.Query.Structured;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;

namespace ReadiNow.Expressions.Tree.Nodes
{
    class WhereNode : EntityNode
    {
        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            // Hmm.. should the where even be in the primary expression tree?
            return Left.EvaluateEntity(evalContext);
        }

        protected override IEnumerable<EntitySetHandle> OnVisitAllItems(EvaluationContext evalContext)
        {
            IEnumerable<EntitySetHandle> source = base.OnVisitAllItems(evalContext);

            foreach (var handle in source)
            {
                handle.Activate(evalContext);

                bool? predicate = Right.EvaluateBool(evalContext);
                if (predicate == true)
                {
                    yield return handle;
                }
            }
        }

        public override SQ.Entity OnBuildQueryNode(QueryBuilderContext context, bool allowReuse)
        {
            // Find a node
            if (ChildContainer.ChildEntityNodes.Count > 1)
                throw new Exception("Cross-join in report calculations are unsupported.");

            SQ.Entity reportNode;
            if ( ChildContainer.ChildEntityNodes.Count == 0 )
            {
                reportNode = new SingleRowNode( );
            }
            else
            {
                reportNode = ChildContainer.ChildEntityNodes.Single( ).BuildQueryNode( context, false );
            }

            // Attach conditions
            ScalarExpression condition = Right.BuildQuery(context);
            if ( reportNode.Conditions == null)
                reportNode.Conditions = new List<ScalarExpression>();
            reportNode.Conditions.Add(condition);

            return reportNode;
        }
    }
}
