// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Database.Types;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Model;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Tree.Nodes
{
    class PartialParameterNode : ZeroArgumentNode
    {
        public string PartialName { get; set; }
    }

    class ParameterNode : ZeroArgumentNode
    {
        public string ParameterName { get; set; }

        protected override T OnEvaluateGeneric<T>(EvaluationContext evalContext, Func<ExpressionNode, T> childEvaluator)
        {
            return GetParameter<T>(evalContext);
        }

        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            // TODO : Check entity type.

            object oParameter = GetParameter<object>(evalContext);
            if (oParameter is IEntity)
            {
                return (IEntity)oParameter;
            }
            if (oParameter is EntityRef)
            {
                var entity = Entity.Get((EntityRef)oParameter);
                return entity;
            }
            if (oParameter is long)
            {
                var entity = Entity.Get((long)oParameter);
                return entity;
            }
            throw new InvalidOperationException();
        }

        public T GetParameter<T>(EvaluationContext evalContext)
        {
            object oResult = evalContext.ResolveParameter(ParameterName);
            try
            {
                if (oResult is TimeSpan)
                    oResult = TimeType.NewTime((TimeSpan)oResult);
                
                var result = (T)oResult;

                return result;
            }
            catch (InvalidCastException ex)
            {
                var message = string.Format("Calculation requested value for parameter '{0}' and expected it to be of type '{1}' but it was provided as type '{2}'.", ParameterName, typeof (T).Name, oResult.GetType().Name);
                throw new Exception(message, ex);
            }
        }
    }

    class EntityParameterNode : EntityNode
    {
        public string ParameterName { get; set; }

        private IEnumerable<IEntity> GetParameterList(EvaluationContext evalContext)
        {
            object oResult = evalContext.ResolveParameter(ParameterName);
            if (oResult != null)
            {
                if (oResult is IEntity)
                {
                    yield return (IEntity)oResult;
                }
                else
                {
                    IEnumerable eResult = (IEnumerable)oResult;

                    foreach (object value in eResult)
                    {
                        IEntity entity = ConvertEntity(value);
                        yield return entity;
                    }
                }
            }
        }

        protected IEntity ConvertEntity(object oParameter)
        {
            // TODO : Check entity type.

            if (oParameter is IEntity)
            {
                return (IEntity)oParameter;
            }
            if (oParameter is EntityRef)
            {
                var entity = Entity.Get((EntityRef)oParameter);
                return entity;
            }
            if (oParameter is long)
            {
                var entity = Entity.Get((long)oParameter);
                return entity;
            }
            throw new InvalidOperationException();
        }

        protected override IEnumerable<EntitySetHandle> OnVisitItems(EvaluationContext evalContext)
        {
            // The instances passed in via the parameter
            IEnumerable<IEntity> instances = GetParameterList(evalContext);

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
            SQ.Entity result = context.ResolveParameterNode( ParameterName );

            AddChildNodes( context, result, allowReuse );

            return result;
        }
    }
}
