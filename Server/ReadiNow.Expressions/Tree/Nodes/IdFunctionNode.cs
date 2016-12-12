// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;

namespace ReadiNow.Expressions.Tree.Nodes
{
    class IdFunctionNode : UnaryOperatorNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            IEntity entity = Argument.EvaluateEntity(evalContext);

            if (entity == null)
                return null;
            
            return (int)entity.Id;  // TODO : Something better than this!!
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            var queryNode = context.GetNode(Argument);
            var result = new EDC.ReadiNow.Metadata.Query.Structured.IdExpression
            {
                NodeId = queryNode.NodeId
            };
            return result;
        }
    }

}
