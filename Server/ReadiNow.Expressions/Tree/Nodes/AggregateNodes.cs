// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Metadata.Query.Structured;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.Collections.Generic;

namespace ReadiNow.Expressions.Tree.Nodes
{
    abstract class AggregateNode : EntityNode
    {
        protected override IEnumerable<EntitySetHandle> OnVisitItems(EvaluationContext evalContext)
        {
            yield return new EntitySetHandle
            {
                Expression = this
            };
        }

        protected override IEnumerable<EntitySetHandle> OnVisitAllItems(EvaluationContext evalContext)
        {
            yield return new EntitySetHandle
            {
                Expression = this
            };
        }

        public IEnumerable<EntitySetHandle> VisitChildItems(EvaluationContext evalContext)
        {
            // Backup (and set restore) for the 'EnsureDefaultRow' flag
            // Ensure that there are no default rows generated in sub-trees that feed into aggregates.
            bool prevEnsureRow = evalContext.EnsureDefaultRow;
            evalContext.EnsureDefaultRow = false;
            Action restoreEnsureRow = () => evalContext.EnsureDefaultRow = prevEnsureRow;

            // Return handle to Visit each entity
            IEnumerable<EntitySetHandle> handles = OnVisitItems(evalContext).Select(handle => { handle.Activate(evalContext); return handle; });

            // And cross-join all child nodes against each other - as inner joins for aggregate.
            handles = ChildContainer.ChildEntityNodes.Aggregate
                (
                    handles,
                    (current, joinNode) => current.SelectMany(
                        handle => joinNode.VisitItems(evalContext),
                        (parent, child) =>
                        {
                            child.AddParent(parent);
                            return child;
                        }
                    )
                );


            handles = handles.CallbackAtEnd( restoreEnsureRow );

            return handles;
        }

        public override SQ.Entity OnBuildQueryNode(QueryBuilderContext context, bool allowReuse)
        {
            // Hack to fix count(convert(Manager,context()).[Direct Reports])
            // A more general fix should ensure that we do not reuse a context here that is already used in the parent scope.. or something??
            var nodesToProcess = ChildContainer.ChildEntityNodes;
            if (nodesToProcess.Count == 1 && nodesToProcess[0] is GetRootContextEntityNode &&
                nodesToProcess[0].ChildContainer.ChildEntityNodes.Count == 1)
                nodesToProcess = nodesToProcess[0].ChildContainer.ChildEntityNodes;

            // Find the child tree node
            if ( nodesToProcess.Count > 1 )
                throw new Exception( "Cross-join in report calculations are unsupported." );

            var aggregateQueryNode = new AggregateEntity( );
            context.ParentNodeStack.Push( aggregateQueryNode );

            SQ.Entity childNode;
            if ( nodesToProcess.Count == 0 )
                childNode = new SingleRowNode( );
            else
                childNode = nodesToProcess.Single( ).BuildQueryNode( context, false );

            // Move child node to the right place (it will try to add to RelatedEntities, but we don't want it there)
            aggregateQueryNode.RelatedEntities.Clear( );
            aggregateQueryNode.GroupedEntity = childNode;
            context.ParentNodeStack.Pop( );

            // Register the aggregate 
            context.ParentNode.RelatedEntities.Add( aggregateQueryNode );
            return aggregateQueryNode;
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            var aggregateNode = context.GetNode(this);
            var result = new EDC.ReadiNow.Metadata.Query.Structured.AggregateExpression
            {
                NodeId = aggregateNode.NodeId,
                AggregateMethod = Method
            };
            if (Method != AggregateMethod.Count)
            {
                result.Expression = Argument.BuildQuery(context);
            }
            return result;
        }

        protected virtual AggregateMethod Method
        {
            get { throw new InvalidOperationException("BuildQuery method unavailable for " + GetType().Name); }
        }
    }

    class CountNode : AggregateNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            int result = VisitChildItems(evalContext).Count();
            return result;
        }

        protected override AggregateMethod Method
        {
            get { return AggregateMethod.Count; }
        }
    }

    class AnyNode : AggregateNode
    {

        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            bool result = VisitChildItems(evalContext)
                .Select(handle =>
                {
                    handle.Activate(evalContext);
                    bool? singleResult = Argument.EvaluateBool(evalContext);
                    return singleResult == true;
                }).Any();
            return result;
        }
    }

    class EveryNode : AggregateNode
    {
        protected override bool? OnEvaluateBool(EvaluationContext evalContext)
        {
            bool result = VisitChildItems(evalContext)
                .Select(handle =>
                {
                    handle.Activate(evalContext);
                    bool? singleResult = Argument.EvaluateBool(evalContext);
                    return singleResult == true;
                }).All(x => x);
            return result;
        }
    }

    class SumNode : AggregateNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            int result = VisitChildItems(evalContext)
                .Select(handle =>
                {
                    handle.Activate(evalContext);
                    int? singleResult = Argument.EvaluateInt(evalContext);
                    return singleResult ?? 0;
                }).Sum();
            return result;
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            decimal result = VisitChildItems(evalContext)
                .Select(handle =>
                {
                    handle.Activate(evalContext);
                    decimal? singleResult = Argument.EvaluateDecimal(evalContext);
                    return singleResult ?? 0;
                }).Sum();
            return result;
        }

        protected override AggregateMethod Method
        {
            get { return AggregateMethod.Sum; }
        }
    }

    class AverageNode : AggregateNode
    {
        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            decimal result = VisitChildItems(evalContext)
                .Select(handle =>
                {
                    handle.Activate(evalContext);
                    decimal? singleResult = Argument.EvaluateDecimal(evalContext);
                    return singleResult;
                })
                .Where(nullable => nullable != null)
                .Select(nullable => nullable.Value)                
                .Average();
            return result;
        }

        protected override AggregateMethod Method
        {
            get { return AggregateMethod.Average; }
        }
    }

    class StdevNode : AggregateNode
    {
        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            // ReSharper disable PossibleMultipleEnumeration

            var handles = VisitChildItems(evalContext);

            decimal sum = 0;
            int count = 0;

            foreach (var handle in handles)
            {
                handle.Activate(evalContext);
                decimal? singleResult = Argument.EvaluateDecimal(evalContext);
                if (singleResult == null)
                    continue;       // I think

                decimal value = singleResult.Value;
                count++;
                sum += value;
            }

            if (count <= 1)
                return null;

            double mean = (double)sum / count;
            double sqDiffSum = 0;

            foreach ( var handle in handles)
            {
                handle.Activate(evalContext);
                decimal? singleResult = Argument.EvaluateDecimal(evalContext);
                if (singleResult == null)
                    continue;       // I think

                decimal value = singleResult.Value;

                double diff = (double)value - mean;
                sqDiffSum += diff * diff;
            }

            bool isSample = true;
            int adjust = isSample ? 1 : 0;

            double variance = sqDiffSum / (count - adjust);
            decimal stdev = (decimal)Math.Sqrt(variance);
            return stdev;

            // ReSharper enable PossibleMultipleEnumeration
        }

        protected override AggregateMethod Method
        {
            get { return AggregateMethod.StandardDeviation; }
        }
    }

    abstract class MaxMinNode : AggregateNode
    {
        protected override int? OnEvaluateInt(EvaluationContext evalContext)
        {
            var result = MaxMin(evalContext, Argument.EvaluateInt);
            return result;            
        }

        protected override decimal? OnEvaluateDecimal(EvaluationContext evalContext)
        {
            var result = MaxMin(evalContext, Argument.EvaluateDecimal);
            return result;
        }

        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            var result = MaxMin(evalContext, Argument.EvaluateString);
            return result;
        }

        protected override DateTime? OnEvaluateDate(EvaluationContext evalContext)
        {
            var result = MaxMin(evalContext, Argument.EvaluateDate);
            return result;
        }

        protected override DateTime? OnEvaluateTime(EvaluationContext evalContext)
        {
            var result = MaxMin(evalContext, Argument.EvaluateTime);
            return result;
        }

        protected override DateTime? OnEvaluateDateTime(EvaluationContext evalContext)
        {
            var result = MaxMin(evalContext, Argument.EvaluateDateTime);
            return result;
        }

        protected override IEntity OnEvaluateEntity( EvaluationContext evalContext )
        {
            bool isEnum;
            if (!evalContext.TypeIsEnum.TryGetValue(ResultType.EntityTypeId, out isEnum))
            {
                isEnum = EDC.ReadiNow.Model.Entity.Get<EntityType>( ResultType.EntityTypeId ).Is<EnumType>( );
                evalContext.TypeIsEnum [ ResultType.EntityTypeId ] = isEnum;
            }

            string bestName = "";
            int? bestInt = null;
            IEntity bestEntity = null;
            int match = Method == AggregateMethod.Max ? 1 : -1;

            foreach (var handle in VisitChildItems( evalContext ))
            {
                handle.Activate( evalContext );
                IEntity entity = handle.Entity;

                if ( entity != null )
                {
                    if ( isEnum )
                    {
                        int? cur = entity.GetField<int?>( "core:enumOrder" );
                        if ( bestInt == null || cur != null && Math.Sign( cur.Value - bestInt.Value ) == match )
                        {
                            bestInt = cur;
                            bestEntity = entity;
                        }
                    }
                    else
                    {
                        string cur = entity.GetField<string>( "core:name" );
                        if ( cur != null && string.Compare( cur, bestName, StringComparison.OrdinalIgnoreCase ) == match )
                        {
                            bestName = cur;
                            bestEntity = entity;
                        }
                    }
                }
            }

            return bestEntity;
        }

        protected abstract T MaxMin<T>(EvaluationContext evalContext, Func<EvaluationContext, T> lookup);
    }
    
    class MaxNode : MaxMinNode
    {
        protected override T MaxMin<T>(EvaluationContext evalContext, Func<EvaluationContext, T> lookup) 
        {
            T result = VisitChildItems(evalContext)
                .Select(handle =>
                {
                    handle.Activate(evalContext);
                    T singleResult = lookup(evalContext);
                    return singleResult;
                })
                .Where(nullable => nullable != null)
                .Max();
            return result;
        }

        protected override AggregateMethod Method
        {
            get { return AggregateMethod.Max; }
        }
    }

    class MinNode : MaxMinNode
    {
        protected override T MaxMin<T>(EvaluationContext evalContext, Func<EvaluationContext, T> lookup)
        {
            T result = VisitChildItems(evalContext)
                .Select(handle =>
                {
                    handle.Activate(evalContext);
                    T singleResult = lookup(evalContext);
                    return singleResult;
                })
                .Where(nullable => nullable != null)
                .Min();
            return result;
        }

        protected override AggregateMethod Method
        {
            get { return AggregateMethod.Min; }
        }
    }

    class StringJoinNode : AggregateNode
    {
        protected override string OnEvaluateString(EvaluationContext evalContext)
        {
            var sb = new StringBuilder();

            var handles = VisitChildItems(evalContext);

            bool isNull = true;
            bool first = true;
            foreach (var handle in handles)
            {
                handle.Activate(evalContext);
                string value = Argument.EvaluateString(evalContext);

                if (first)
                {
                    first = false;
                    isNull = value == null;
                }
                else
                {
                    sb.Append(", ");
                }
                sb.Append(value);
            }
            if (isNull)
                return null;
            string result = sb.ToString();
            return result;
        }
    }

    

}
