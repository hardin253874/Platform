// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using Model = EDC.ReadiNow.Model;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.Parser;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;

namespace ReadiNow.Expressions.Tree.Nodes
{
    [DebuggerDisplay("{RelationshipName} - {Direction}")]
    class AccessRelationshipNode : EntityNode
    {
        public Direction Direction { get; set; }

        private EntityRef RelationshipId
        {
            get
            {
                // Currently this must be a literal
                var relationship = Right as ConstantEntityNode;
                if (relationship == null)
                    throw new ParseException("Relationship required.");
                var relationshipTypeId = relationship.Instance;
                return relationshipTypeId;
            }
        }

        // For debugging
        public string RelationshipName
        {
            get { return Model.Entity.Get<Resource>(RelationshipId).Name; }
        }

        public override void RegisterDependencies(ISet<long> identifiedEntities, ISet<long> fields, ISet<long> relationships)
        {
            RegisterChildAs(Right, relationships);
        }

        protected override IEnumerable<EntitySetHandle> OnVisitItems(EvaluationContext evalContext)
        {
            // The relationship (definition) being accessed
            // (don't evaluate in loop for now ... OK while they're just literals)
            IEntity relationship = Right.EvaluateEntity(evalContext);

            IEntity current = Left.EvaluateEntity(evalContext);
            if (current == null)
                yield break;

            // Relationship data
            IEntityRelationshipCollection<IEntity> values = current.GetRelationships(relationship,
                                                                                              Direction);
            foreach (var pair in values)
            {
                var result = new EntitySetHandle
                    {
                        Entity = pair != null ? pair.Entity : null,
                        Expression = this
                    };
                yield return result;
            }
        }

        public override EDC.ReadiNow.Metadata.Query.Structured.Entity OnBuildQueryNode(QueryBuilderContext context, bool allowReuse)
        {
            if (context.ParentNode == null)
                throw new Exception("No context.");

            var relationshipTypeId = RelationshipId;
            var direction = Direction == Direction.Forward ? RelationshipDirection.Forward : RelationshipDirection.Reverse;

            // Look for an existing relationship node in the tree that we can reuse
            // TODO : We shouldn't really match existing nodes if they were layed down as part of this build, as the expression tree should contain the intended target structure
            RelatedResource result = !allowReuse ? null :
                context.ParentNode
                .RelatedEntities
                .OfType<RelatedResource>()
                .FirstOrDefault(
                    rr => rr.RelationshipTypeId.Id == relationshipTypeId.Id
                       && rr.RelationshipDirection == direction
                       && (rr.Conditions == null || rr.Conditions.Count == 0) );

            // Or create one
            if (result == null)
            {
                // New node
                result = new RelatedResource
                {
                    RelationshipTypeId = relationshipTypeId,
                    RelationshipDirection = direction
                };
                context.ParentNode.RelatedEntities.Add(result);
            }
            AddChildNodes(context, result, allowReuse);

            return result;
        }

        /// <summary>
        /// Determine if a relationship can be reused.
        /// </summary>
        public bool AreEquivalent(AccessRelationshipNode other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (other.Left != Left)
                return false;

            if (other.Direction != Direction)
                return false;

            if (other.RelationshipId.Id != RelationshipId.Id)
                return false;

            return true;
        }


        /// <summary>
        /// Override for class-specific XML generation.
        /// </summary>
        /// <param name="element">The element that represents this node, to which attributes and children should be added.</param>
        /// <param name="context">XML generation context.</param>
        protected override void OnGetXElement( XElement element, ExpressionXmlContext context )
        {
            element.Add( new XAttribute( "direction", Direction ) );
            element.Add( new XAttribute( "relationshipId", RelationshipId ) );
        }
    }

}
