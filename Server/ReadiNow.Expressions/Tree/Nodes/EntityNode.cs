// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Metadata.Query.Structured;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;

namespace ReadiNow.Expressions.Tree.Nodes
{
    public class ChildContainer
    {
        public ChildContainer()
        {
            ChildEntityNodes = new List<EntityNode>();
        }

        public List<EntityNode> ChildEntityNodes { get; set; }

        internal void RegisterChild(EntityNode child)
        {
            ChildEntityNodes.Add(child);
        }

        internal T RegisterOrReuseChild<T>(T child) where T : EntityNode
        {
            // Reuse exact matches without re-registering
            if (ChildEntityNodes.Contains(child))
                return child;

            EntityNode existing = null;

            // Reuse relationships
            var asRelationship = child as AccessRelationshipNode;
            if (asRelationship != null)
            {
                // Check to see if there's already a matching relationship node registered
                // If so, reuse it
                existing = ChildEntityNodes
                    .OfType<AccessRelationshipNode>()
                    .FirstOrDefault(rr => rr.AreEquivalent(asRelationship));
            }

            // Reuse parameters
            var asParameter = child as EntityParameterNode;
            if (asParameter != null)
            {
                // Check to see if there's already a matching parameter node registered here
                // If so, reuse it
                existing = ChildEntityNodes
                    .OfType<EntityParameterNode>()
                    .FirstOrDefault(rr => rr.ParameterName == asParameter.ParameterName);
            }

            if ( existing != null )
            { 
                // Only allow an existing node to be reused if neither are coming from a variable, or if both are coming from the same variable
                // (Otherwise reuse means sub-clauses in the variable won't be able to find their parent container)
                if ( child.RootForVariableContainer == existing.RootForVariableContainer )
                    return existing as T;
            }

            ChildEntityNodes.Add(child);
            return child;
        }
    }

    /// <summary>
    /// Root type for any expression that returns entities.
    /// </summary>
    public abstract class EntityNode : FunctionNode
    {
        public EntityNode()
        {
            ChildContainer = new ChildContainer();
        }

        protected EntityNode EntityArgument
        {
            get { return (EntityNode)Argument; }
        }

        /// <summary>
        /// These will get cross-joined against each other.
        /// </summary>
        public ChildContainer ChildContainer { get; set; }

        // Derive from FunctionNode just so we get arguments manged for us

        /// <summary>
        /// If this expression is the root term of a variable assignment, then the container of that variable is marked here.
        /// This is used so that we only re-use root terms if they belong to the same variable. (Otherwise key lookups fail as per #27975)
        /// </summary>
        public ChildContainer RootForVariableContainer { get; set; }

        protected override IEntity OnEvaluateEntity(EvaluationContext evalContext)
        {
            IEntity result = evalContext.GetCurrentEntity(this);
            return result;
        }

        public IEnumerable<EntitySetHandle> VisitItems(EvaluationContext evalContext)
        {
            return OnVisitAllItems(evalContext);
        }

        public virtual EntityNode GetQueryNode()
        {
            return this;
        }

        private class HandleWrap
        {
            public EntitySetHandle Handle { get; set; }
            public bool IsNotEmpty { get; set; }
        }

        protected virtual IEnumerable<EntitySetHandle> OnVisitAllItems(EvaluationContext evalContext)
        {
            // Yes .. this is a hack
            // The problem is its not necessarily the case that children should or should not constrain the parent
            // The problem is that the tree itself is built wrong.. the root node should be the 'where' node if the overall result set is to be filtered.
            bool childrenWillContrainParent = ChildContainer.ChildEntityNodes.Any(node => node is WhereNode);

            // Return handle to Visit each entity
            IEnumerable<EntitySetHandle> handles = OnVisitItems(evalContext).Select(handle => { handle.Activate(evalContext); return handle; });

            // (Wraps are used to track if any of the joins were non-default-nulls)
            var handleWraps = handles.Select(h => new HandleWrap { Handle = h, IsNotEmpty = !childrenWillContrainParent });

            // And cross-join all child nodes against each other.
            // Note: these are effectively left-joined to the parent. (We use the DefaultIfEmpty to ensure that each set returns at least one value, to ensure they act as a left join).
            var handlesEx = ChildContainer.ChildEntityNodes.Aggregate
                (
                    seed: handleWraps,
                    func: (accumHandles, joinNode) => accumHandles.SelectMany(
                        collectionSelector: handle => AddDefaultRowIfRequired(joinNode.VisitItems(evalContext), evalContext),
                        resultSelector: (parent, child) =>
                        {
                            bool wasEmpty = child == null;
                            if (wasEmpty)
                                child = new EntitySetHandle { Expression = joinNode };
                            child.AddParent(parent.Handle);
                            return new HandleWrap { Handle = child, IsNotEmpty = !wasEmpty };
                        }
                    )
                );

            if (childrenWillContrainParent)
                handlesEx = handlesEx.Where(hw => hw.IsNotEmpty);

            return handlesEx.Select(hw => hw.Handle);

        }

        /// <summary>
        /// We effectively do cross joins. So if any of the child paths return zero rows, then it causes the overall sub-tree 
        /// to return zero rows. This is generally undesirable, except during aggregate operations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowSource">An enumeration that will optionally get wrapped to ensure it returns a default value.</param>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        private IEnumerable<T> AddDefaultRowIfRequired<T>(IEnumerable<T> rowSource, EvaluationContext evalContext)
        {
            if (evalContext.EnsureDefaultRow)
                return rowSource.DefaultIfEmpty();
            return rowSource;
        }

        protected virtual IEnumerable<EntitySetHandle> OnVisitItems(EvaluationContext evalContext)
        {
            yield return new EntitySetHandle { Expression = this };
        }


		/// <summary>
		/// Convert the expression tree into a structured query expression.
		/// </summary>
		/// <param name="context">Context information about this query building session, including the target structured query object.</param>
		/// <param name="allowReuse">if set to <c>true</c> [allow reuse].</param>
		/// <returns>
		/// A query node that can be used within the query.
		/// </returns>
        public EDC.ReadiNow.Metadata.Query.Structured.Entity BuildQueryNode(QueryBuilderContext context, bool allowReuse)
        {
            EDC.ReadiNow.Metadata.Query.Structured.Entity result;

            if (!context.NodeCache.TryGetValue(this, out result))
            {
                result = OnBuildQueryNode(context, allowReuse);
                context.NodeCache[this] = result;
            }
            return result;
        }

        /// <summary>
        /// Returns a tree node that is representative of an entity or.
        /// </summary>
        /// <param name="context">Context information about this query building session, including the target structured query object.</param>
        /// <param name="allowReuse">A dedicated node should be returned because the caller intends on disturbing it.</param>
        /// <returns>A query node that can be used within the query.</returns>
        public virtual EDC.ReadiNow.Metadata.Query.Structured.Entity OnBuildQueryNode(QueryBuilderContext context, bool allowReuse)
        {
            string name = GetType().Name;
            throw new InvalidOperationException(name);
        }

        protected void AddChildNodes(QueryBuilderContext context, SQ.Entity parent, bool allowReuse)
        {
            context.ParentNodeStack.Push(parent);
            foreach (var node in ChildContainer.ChildEntityNodes)
            {
                node.BuildQueryNode(context, allowReuse);
            }
            context.ParentNodeStack.Pop();
        }

        protected override ScalarExpression OnBuildQuery(QueryBuilderContext context)
        {
            var result = new SQ.ResourceExpression
            {
                NodeId = context.GetNode(this).NodeId,
                FieldId = new EntityRef("core:name")
            };
            return result;
        }

        /// <summary>
        /// Override for class-specific XML generation.
        /// </summary>
        /// <param name="element">The element that represents this node, to which attributes and children should be added.</param>
        /// <param name="context">XML generation context.</param>
        protected override void OnGetXElement( XElement element, ExpressionXmlContext context )
        {
            element.Add( context.CreateList( "ChildContainer", ChildContainer.ChildEntityNodes, n => n.GetXElement( context ) ) );
        }
    }

    public class SingleRowSourceNode : EntityNode
    {
        protected override IEnumerable<EntitySetHandle> OnVisitItems(EvaluationContext evalContext)
        {
            EntitySetHandle handle = new EntitySetHandle
            {
                Expression = this
            };
            yield return handle;
        }

        public override SQ.Entity OnBuildQueryNode(QueryBuilderContext context, bool allowReuse)
        {
            return null;
        }
    }

    public class EntitySetHandle
    {
        /// <summary>
        /// The current entity value for this expression.
        /// </summary>
        public IEntity Entity { get; set; }

        /// <summary>
        /// The expression that this entity represents.
        /// </summary>
        public ExpressionNode Expression { get; set; }

        /// <summary>
        /// Singly-linked list of values.
        /// </summary>
        public List<EntitySetHandle> Parents { get; private set; }

        /// <summary>
        /// True if this handle represents an empty row being left joined in.
        /// </summary>
        public bool IsEmptyJoin { get; set; }

        public void AddParent(EntitySetHandle parent)
        {
            if (Parents == null)
                Parents = new List<EntitySetHandle>();
            Parents.Add(parent);
        }

        /// <summary>
        /// Activate must be called before evaluating any other expressions within the context of this handle.
        /// </summary>
        /// <param name="evalContext"></param>
        public void Activate(EvaluationContext evalContext)
        {
            evalContext.SetCurrentEntity(Expression, IsEmptyJoin ? null : Entity);

            if (Parents != null)
                foreach (var parent in Parents)
                    parent.Activate(evalContext);
        }
    }
}
