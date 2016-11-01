// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.Database;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Model;

namespace ReadiNow.Expressions.Tree.Nodes
{
    public class OrderByTerm
    {
        public ExpressionNode Expression { get; set; }

        public Direction Direction { get; set; }
    }

    class OrderByNode : EntityNode
    {
        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            // Hmm.. should the where even be in the primary expression tree?
            return Argument.EvaluateEntity(evalContext);
        }

        public List<OrderByTerm> OrderTerms { get; set; }

        protected override IEnumerable<EntitySetHandle> OnVisitAllItems(EvaluationContext evalContext)
        {
            // Stream of entities to evaluate
            IEnumerable<EntitySetHandle> entities = base.OnVisitAllItems(evalContext);

            First first = new First();
            foreach (var termIter in OrderTerms)
            {
                var term = termIter;
                bool desc = termIter.Direction == Direction.Reverse;

                switch (term.Expression.ResultType.Type)
                {
                    case DataType.String:
                    case DataType.Xml:
                        entities = Sort(entities, evalContext, term.Expression, term.Expression.EvaluateString, first, desc);
                        break;
                    case DataType.Int32:
                        entities = Sort(entities, evalContext, term.Expression, term.Expression.EvaluateInt, first, desc);
                        break;
                    case DataType.Currency:
                    case DataType.Decimal:
                        entities = Sort(entities, evalContext, term.Expression, term.Expression.EvaluateDecimal, first, desc);
                        break;
                    case DataType.Date:
                        entities = Sort(entities, evalContext, term.Expression, term.Expression.EvaluateDate, first, desc);
                        break;
                    case DataType.Time:
                        entities = Sort(entities, evalContext, term.Expression, term.Expression.EvaluateTime, first, desc);
                        break;
                    case DataType.DateTime:
                        entities = Sort(entities, evalContext, term.Expression, term.Expression.EvaluateDateTime, first, desc);
                        break;
                    case DataType.Bool:
                        entities = Sort(entities, evalContext, term.Expression, term.Expression.EvaluateBool, first, desc);
                        break;
                    case DataType.Guid:
                        entities = Sort(entities, evalContext, term.Expression, term.Expression.EvaluateGuid, first, desc);
                        break;

                    case DataType.Entity:   // static builder will perform cast to string
                    case DataType.Binary:
                    case DataType.None:
                    default:
                        throw new InvalidOperationException();
                }
            }

            return entities;
        }

        private IEnumerable<EntitySetHandle> Sort<T>(IEnumerable<EntitySetHandle> unsorted, EvaluationContext evalContext, ExpressionNode termExpr, Func<EvaluationContext, T> evaluationFunc, bool isFirst, bool isDesc)
        {
            Func<EntitySetHandle, T> evaluate = handle =>
                {
                    handle.Activate(evalContext);
                    //var ls = termExpr.OnDetermineListSource();
                    //var blah = ls.OnVisitItems(evalContext);
                    //var blah2 = blah.FirstOrDefault();
                    //if (blah2 != null)
                    //    blah2.Activate(evalContext);
                    T result = evaluationFunc(evalContext);
                    return result;
                };

            if (isFirst)
            {
                if (isDesc)
                    return unsorted.OrderByDescending(evaluate);
                else
                    return unsorted.OrderBy(evaluate);
            }
            else
            {
                if (isDesc)
                    return ((IOrderedEnumerable<EntitySetHandle>)unsorted).ThenByDescending(evaluate);
                else
                    return ((IOrderedEnumerable<EntitySetHandle>)unsorted).ThenBy(evaluate);
            }
        }

        public class DelegateComparer<T> : IComparer<T>
        {
            public Func<T,T,int> ComparisonFunction { get; set; }

            public int Compare(T x, T y)
            {
                return ComparisonFunction(x, y);
            }
        }
    }
}
