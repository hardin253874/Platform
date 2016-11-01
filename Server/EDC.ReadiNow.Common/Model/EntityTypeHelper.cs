// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EDC.Common;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Entity Type partial class implementation.
	/// </summary>
	public static class EntityTypeHelper
	{
		/// <summary>
		///     Gets all defined fields.
		/// </summary>
        /// <param name="entityType">The entity type resource.</param>
		[SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
		public static IEnumerable<Field> GetAllFields( this EntityType entityType )
		{
            if (entityType == null)
                throw new ArgumentNullException("entityType");

			return GetAllFields( entityType.ToEnumerable( ) );
		}

		/// <summary>
		///     Gets all fields that are applicable to this instance (which may be of multiple types).
		/// </summary>
		public static IEnumerable<Field> GetAllFields( this IEntity instance )
		{
			return GetAllFields( instance.EntityTypes.Cast<EntityType>( ) );
		}

        /// <summary>
        ///     Gets all fields that are applicable to this instance (which may be of multiple types).
        /// </summary>
        public static IEnumerable<IEntity> GetAllFieldsAsNative(IEntity instance)
        {
            return GetAllFieldsAsNative(instance.EntityTypes.Cast<EntityType>());
        }

		/// <summary>
		///     Gets all relationships that are applicable to this instance (which may be of multiple types).
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		public static IEnumerable<Relationship> GetAllRelationships( IEntity instance, Direction direction )
		{
			return GetAllRelationships( instance.EntityTypes.Cast<EntityType>( ), direction );
		}

		/// <summary>
		///     Implementation: gets all relationships that are applicable to a set of types.
		/// </summary>
		/// <param name="types">The types.</param>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		public static IEnumerable<Relationship> GetAllRelationships( IEnumerable<EntityType> types, Direction direction )
		{
			var relationships = new HashSet<Relationship>( );

			foreach ( EntityType type in GetAllTypes( types ) )
			{
				IEntityCollection<Relationship> typeRelationships = direction == Direction.Forward ? type.Relationships : type.ReverseRelationships;

				if ( typeRelationships != null )
				{
					relationships.AddRange( typeRelationships );
				}
			}
			return relationships;
		}

        /// <summary>
        /// Implementation: gets all relationships that are applicable to a given type.
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="forwardRelationships">The forward relationships.</param>
        /// <param name="reverseRelationships">The reverse relationships.</param>
        public static void GetAllRelationships(long typeId, out IEnumerable<Relationship> forwardRelationships, out IEnumerable<Relationship> reverseRelationships)
        {
            GetAllRelationships(typeId, false, out forwardRelationships, out reverseRelationships);
        }

        /// <summary>
        /// Implementation: gets all relationships that are applicable to a given type with an option to get all the descendant types.        
        /// </summary>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="includeDescendants">if set to <c>true</c> include descendants.</param>
        /// <param name="forwardRelationships">The forward relationships.</param>
        /// <param name="reverseRelationships">The reverse relationships.</param>
        public static void GetAllRelationships(long typeId, bool includeDescendants, out IEnumerable<Relationship> forwardRelationships, out IEnumerable<Relationship> reverseRelationships)
        {
            var forwardRelationshipsSet = new HashSet<Relationship>();
            var reverseRelationshipsSet = new HashSet<Relationship>();

            forwardRelationships = forwardRelationshipsSet;
            reverseRelationships = reverseRelationshipsSet;

            ISet<long> typeIds = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf(typeId);

            if (includeDescendants)
            {
                typeIds = new HashSet<long>(typeIds);
                typeIds.UnionWith(PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(typeId));   
            }

            IEnumerable<IEntity> types = Model.Entity.Get<IEntity>(typeIds);

            foreach (IEntity type in types)
            {
                var entityType = type.As<EntityType>();
                if (entityType == null)
                {
                    continue;
                }

                IEntityCollection<Relationship> typeForwardRelationships = entityType.Relationships;

                if (typeForwardRelationships != null)
                {
                    forwardRelationshipsSet.AddRange(typeForwardRelationships);
                }

                IEntityCollection<Relationship> typeReverseRelationships = entityType.ReverseRelationships;

                if (typeReverseRelationships != null)
                {
                    reverseRelationshipsSet.AddRange(typeReverseRelationships);
                }
            }
        }

		/// <summary>
		///     Get all types (assigned or inherited) that apply to the instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<EntityType> GetAllTypes( this IEntity instance )
		{
			return GetAllTypes( instance.EntityTypes.Cast<EntityType>( ) );
		}

		/// <summary>
		/// Gets the type and all inherited types.
		/// </summary>
		/// <param name="entityType">The entity type resource.</param>
		/// <param name="sorted">if set to <c>true</c> [sorted].</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">entityType</exception>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public static IEnumerable<EntityType> GetAncestorsAndSelf( this EntityType entityType, bool sorted = false )
		{
            if (entityType == null)
                throw new ArgumentNullException(nameof( entityType ));

            return GetAllTypes(entityType.ToEnumerable(), true, sorted);
		}

		/// <summary>
		/// Gets the type and all derived types.
		/// </summary>
		/// <param name="entityType">The entity type resource.</param>
		/// <param name="sorted">if set to <c>true</c> [sorted].</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">entityType</exception>
		public static IEnumerable<EntityType> GetDescendantsAndSelf(this EntityType entityType, bool sorted = false)
		{
            if (entityType == null)
                throw new ArgumentNullException(nameof( entityType ));
            
            return GetAllTypes(entityType.ToEnumerable(), false, sorted);
		}

        /// <summary>
        /// Gets all types that could contribute fields/relationships to instances of this type.
        /// This includes all ancestors, all derived, and all ancestors of derived (because they can also contribute members to the derived).
        /// </summary>
        /// <param name="entityType">The entity type resource.</param>
	    public static IEnumerable<EntityType> GetAllMemberContributors( this EntityType entityType )
	    {
            return entityType.GetDescendantsAndSelf( )
                .SelectMany( et => et.GetAncestorsAndSelf( ) )
                .Distinct( new EntityIdEqualityComparer<EntityType>( ) );
        }

		/// <summary>
		/// Is derived from
		/// </summary>
		/// <param name="entityType">The entity type resource.</param>
		/// <param name="typeRef">The type reference.</param>
		/// <returns>
		/// True if the type is a specialized version of the given type
		/// </returns>
		/// <exception cref="System.ArgumentNullException">entityType</exception>
        public static bool IsDerivedFrom(this EntityType entityType, EntityRef typeRef)
		{
            if (entityType == null)
                throw new ArgumentNullException("entityType");

            return entityType.GetAncestorsAndSelf().FirstOrDefault(t => t.Id == typeRef.Id) != null;
		}

		/// <summary>
		///     Implementation: gets all fields that are applicable to a set of types.
		/// </summary>
		private static IEnumerable<Field> GetAllFields( IEnumerable<EntityType> types )
		{
			var fields = new HashSet<Field>( );

			foreach ( EntityType type in GetAllTypes( types ) )
			{
				IEntityCollection<Field> typeFields = type.Fields;
				if ( typeFields != null )
				{
					fields.AddRange( typeFields );
				}
			}
			return fields;
		}

        /// <summary>
        ///     Implementation: gets all fields that are applicable to a set of types.
        /// </summary>
        private static IEnumerable<IEntity> GetAllFieldsAsNative(IEnumerable<EntityType> types)
        {
            var fields = new HashSet<IEntity>();

            foreach (EntityType type in GetAllTypes(types))
            {
                // Call the GetRelationships instead of the Fields property to avoid casts
                IEntityCollection<IEntity> typeFields = type.GetRelationships<IEntity>("core:fields", Direction.Reverse).Entities;

                if (typeFields != null)
                {
	                foreach ( IEntity typeField in typeFields )
	                {
						fields.Add( Entity.AsNative( typeField ) );
	                }
                }
            }
            return fields;
        }

		/// <summary>
		/// Get all types (passed and inherited ... or passed and derived) that apply to the type(s) specified.
		/// </summary>
		/// <param name="types">The types.</param>
		/// <param name="getAncestors">If true, then ancestors are calculated. If false then descendants are calculated.</param>
		/// <param name="sorted">if set to <c>true</c> [sorted].</param>
		/// <returns></returns>
		/// <remarks>
		/// This algorithm must return them in order, starting with the types passed, and then moving to more distant types.
		/// Other functions rely on this behavior. It is currently done using a breadth-first algorithm.
		/// </remarks>
		public static IEnumerable<EntityType> GetAllTypes( IEnumerable<EntityType> types, bool getAncestors = true, bool sorted = false )
		{
			ICollection<long> results;

			if ( sorted )
			{
				List<long> list = new List<long>( );

				foreach ( var entityType in types )
				{
					list.AddRange( getAncestors ? PerTenantEntityTypeCache.Instance.GetAncestorsAndSelfSorted( entityType.Id ) : PerTenantEntityTypeCache.Instance.GetDescendantsAndSelfSorted( entityType.Id ) );
				}

				results = list.Distinct( ).ToList( );
			}
			else
			{
				HashSet<long> hash = new HashSet<long>( );

				foreach ( var entityType in types )
				{
					hash.UnionWith( getAncestors ? PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf( entityType.Id ) : PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf( entityType.Id ) );
				}

				results = hash;
			}

			

			return Entity.Get<EntityType>( results );
		}

		/// <summary>
		/// Determine if it is valid for an instance to be of the specified type,
		/// or all of the specified types at the same time.
		/// </summary>
		/// <param name="types">List of types.</param>
		public static void CheckCanInstanceBeOfTypes( IEnumerable<EntityType> types )
		{
			// The rules:
			// - there must be at least one type
			// - none of the types can be abstract
			// - no two types can have a common ancestor that has SupportMultipleTypes=false (or null)
			// - unless, they have a closer ancestor that has SupportMultipleTypes=true

			// For example, assume Employee and Visitor both inherit Person, which inherits Actor, and Company Inherits actor.
			// Assume Person supports multiple types, but Actor does not.
			// It should be valid to be an Employee and a Visitor, but not an employee and a company.
			// Assume FullTimeEmployee and PartTimeEmployee both inherit employee, and that Employee does not support multiple.
			// It should be valid to be a FullTimeEmployee or PartTimeEmployee, but not both.
			// It should be possible to be a FullTimeEmployee and a Visitor. But not a FullTimeEmployee and a Company.

			if ( types == null )
				throw new ArgumentNullException( "types" );

			var typesTaken = new HashSet<long>( );
			bool anyFound = false;

			foreach ( EntityType type in types )
			{
				anyFound = true;

				// Cannot be of an abstract type
				if ( type.IsAbstract == true )
					throw new ValidationException( string.Format( Resources.GlobalStrings.TypeIsAbstractError, type.Name ) );

				// Note: GetAncestorsAndSelf starts with the current type, and them moves to more distant ancestors.
				// Don't follow a path if we've already been that way, and it supports multi-type.
				var ancestors = Delegates.WalkGraph( type, t => t.Inherits,
				                                     ( child, parent ) => !( typesTaken.Contains( parent.Id ) && ( parent.SupportMultiTypes ?? false ) ) ).ToList( );

				// Check to see if any collide with previously visited types.
				var duplicate = ancestors.FirstOrDefault( t => typesTaken.Contains( t.Id ) );
				if ( duplicate != null )
				{
					// Uh oh, another type has already visited, but this node doesn't support multiple types
					throw new ValidationException( string.Format( Resources.GlobalStrings.TypeDoesnotSupportMultiTypesError, duplicate.Name ) );
				}
				ancestors.ForEach( t => typesTaken.Add( t.Id ) );
			}

			if ( !anyFound )
				throw new ArgumentNullException( "types" );
		}


        /// <summary>
        /// Determines whether the specified type (or base types) has the allowEveryRead field set.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static bool IsAllowEveryoneRead(EntityType entityType)
        {
            if (entityType == null)
            {
                return false;
            }

            if (entityType.AllowEveryoneRead ?? false)
            {
                return true;
            }

            // Have hit the base type. Prevent infinite recursion.
            if (entityType.Alias == "core:type")
            {
                return false;
            }

            // Check base types
            IEntityCollection<EntityType> isOfType = entityType.IsOfType;

			if ( isOfType == null || isOfType.Count <= 0 )
            {
                return false;
            }

            bool allowEveryoneRead = false;

            foreach (EntityType type in isOfType)
            {
                allowEveryoneRead = IsAllowEveryoneRead(type);

                if (allowEveryoneRead)
                {
                    break;
                }
            }

            return allowEveryoneRead;
        }
	}
}