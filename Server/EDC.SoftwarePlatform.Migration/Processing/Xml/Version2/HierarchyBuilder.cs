// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;
using ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.Migration.Processing.Xml.Version2
{
    /// <summary>
    /// 
    /// </summary>
    public class HierarchyBuilder
    {
        /// <summary>
        ///     Callback that needs to retrieve relationship metadata, or return null if unavailable.
        /// </summary>
        public Func<Guid, RelationshipTypeEntry> RelationshipMetadataCallback;

        /// <summary>
        ///     Cache of relationship type information.
        /// </summary>
        private readonly Dictionary<Guid, RelationshipTypeEntry> _relMetadataCache = new Dictionary<Guid, RelationshipTypeEntry>( );

        /// <summary>
        ///     The root entity.
        /// </summary>
        private EntityHierarchyEntry _root;

        /// <summary>
        ///     Map of UpgradeId to entity.
        /// </summary>
        Dictionary<Guid, EntityHierarchyEntry> _entities;

        /// <summary>
        ///     Arranges all of the entities into a hierarchical manner.
        /// </summary>
        /// <param name="packageData">Entity and relationship data</param>
        /// <returns>
        ///     A hierarchy of entities, starting with a null root entity.
        ///     All relationships are placed somewhere into the structure, with any loose
        ///     relationships being placed into the root entity.
        /// </returns>
        [NotNull]
        public EntityHierarchyEntry BuildEntityHierarchy( [NotNull] PackageData packageData )
        {
            if ( packageData == null )
                throw new ArgumentNullException( nameof( packageData ) );

            _root = new EntityHierarchyEntry
            {
                Children = new List<EntityHierarchyEntry>( ),
                ForwardRelationships = new List<RelationshipEntry>( )
            };

            // Create dictionary of entity nodes
            if ( packageData.Entities != null )
            {
                _entities = packageData.Entities.ToDictionary(
                    e => e.EntityId, e => new EntityHierarchyEntry { Entity = e } );
            }
            else
            {
                _entities = new Dictionary<Guid, EntityHierarchyEntry>( );
            }
            

            // Walk all relationships
            // Note: relationships get canonically ordered to ensure deterministic construction of hierarchy
            foreach ( RelationshipEntry rel in packageData.Relationships.OrderBy( r => r.TypeId).ThenBy( r=> r.FromId).ThenBy( r=> r.ToId) )
            {
                EntityHierarchyEntry fromEntity;
                EntityHierarchyEntry toEntity;
                _entities.TryGetValue( rel.FromId, out fromEntity );
                _entities.TryGetValue( rel.ToId, out toEntity );

                // Handle 'type' relationship
                if ( fromEntity != null && rel.TypeId == Guids.IsOfType && fromEntity.TypeRelationship == null )
                {
                    fromEntity.TypeRelationship = rel;
                    continue;
                }

                RelationshipTypeEntry relType = GetRelationshipMetadata( rel.TypeId );

                // Detect entities that contain entities
                if ( fromEntity != null && toEntity != null )
                {
                    if ( HandleContainedEntities( rel, relType, fromEntity, toEntity ) )
                        continue;
                }
                
                // Other relationships - find somewhere to add them.
                if ( fromEntity != null )
                {
                    if ( fromEntity.ForwardRelationships == null )
                        fromEntity.ForwardRelationships = new List<RelationshipEntry>( );
                    fromEntity.ForwardRelationships.Add( rel );
                    continue;
                }

                if ( toEntity != null )
                {
                    if ( toEntity.ReverseRelationships == null )
                        toEntity.ReverseRelationships = new List<RelationshipEntry>( );
                    toEntity.ReverseRelationships.Add( rel );
                    continue;
                }

                // Nowhere else to add them..
                _root.ForwardRelationships.Add( rel );
            }

            // Gather top tier entities into root entity
            foreach ( var entity in _entities.Values )
            {
                if ( entity.ParentEntity != null )
                    continue;
                _root.Children.Add( entity );
                entity.ParentEntity = _root;
            }

            return _root;
        }

        /// <summary>
        ///     Handle case if one entity is a component of the other.
        /// </summary>
        /// <param name="relType">The relationship type.</param>
        /// <param name="fromEntity">The 'from' entity.</param>
        /// <param name="toEntity">The 'to' entity.</param>
        /// <returns></returns>
        private bool HandleContainedEntities( RelationshipEntry rel, RelationshipTypeEntry relType, EntityHierarchyEntry fromEntity, EntityHierarchyEntry toEntity )
        {
            if ( relType == null )
                return false;

            // Explicitly do not next certain relationships
            if ( rel.TypeId == Guids.InSolution || rel.TypeId == Guids.IndirectInSolution )
                return false;

            // if 'fwdComponent' then parent = FromId
            // if 'revComponent' then parent = ToId
            // "Clone action" is the best proxy to determine if something is a subcomponent, while still handling 'custom' reltypes.
            bool fwdComponent = relType.CloneAction == CloneActionEnum_Enumeration.CloneEntities;
            bool revComponent = !fwdComponent && relType.ReverseCloneAction == CloneActionEnum_Enumeration.CloneEntities;

            EntityHierarchyEntry parent = null;
            EntityHierarchyEntry child = null;

            if ( fwdComponent )
            {
                parent = fromEntity;
                child = toEntity;
            }
            else if ( revComponent )
            {
                parent = toEntity;
                child = fromEntity;
            }
            else
            {
                return false;
            }

            if ( child.ParentEntity != null )
                return false;   // child already assigned to a different parent

            if ( WouldCauseCycle( parent, child ) )
                return false;

            if ( parent.Children == null )
                parent.Children = new List<EntityHierarchyEntry>( );
            parent.Children.Add( child );
            child.ParentEntity = parent;
            child.RelationshipFromParent = rel;
            child.Direction = revComponent ? Direction.Reverse : Direction.Forward;
            return true;
        }

        /// <summary>
        ///     Returns true if adding this child to this parent would cause a cycle.
        /// </summary>
        /// <param name="parent">The potential parent</param>
        /// <param name="child">The potential child</param>
        /// <returns>True if a cycle would result.</returns>
        private bool WouldCauseCycle( EntityHierarchyEntry parent, EntityHierarchyEntry child )
        {
            EntityHierarchyEntry current = parent;

            while ( current != null )
            {
                if ( current == child )
                    return true;
                current = current.ParentEntity;
            }
            return false;
        }

        /// <summary>
        ///     Gets relationship type information, and caches it locally.
        /// </summary>
        /// <param name="relTypeId">Type of relationship.</param>
        /// <returns>Relationship type data.</returns>
        private RelationshipTypeEntry GetRelationshipMetadata( Guid relTypeId )
        {
            if ( RelationshipMetadataCallback == null )
                return null;

            RelationshipTypeEntry result;
            if ( _relMetadataCache.TryGetValue( relTypeId, out result ) )
                return result;

            result = RelationshipMetadataCallback( relTypeId );
            _relMetadataCache[ relTypeId ] = result;

            return result;
        }
    }
}
