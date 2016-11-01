// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Cache that stores the modifications made to an entities relationships.
	/// </summary>
	[SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
	internal sealed class EntityRelationshipModificationCache : CacheBase<EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey, IDictionary<long, IChangeTracker<IMutableIdKey>>>
	{
		/// <summary>
		///     Represents the singleton instance of the cache.
		/// </summary>
		private static readonly EntityRelationshipModificationCache InstanceMember = new EntityRelationshipModificationCache( );

		/// <summary>
		///     Primary Key lookup.
		/// </summary>
		private readonly Dictionary<long, ISet<EntityRelationshipModificationCacheKey>> _primaryKeyLookup = new Dictionary<long, ISet<EntityRelationshipModificationCacheKey>>( );

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		///     Prevents a default instance of the <see cref="EntityRelationshipModificationCache" /> class from being created.
		/// </summary>
		private EntityRelationshipModificationCache( )
            : base("Entity Relationship Modification", false, false)
		{
		}

		/// <summary>
		///     Gets the instance.
		/// </summary>
		public static EntityRelationshipModificationCache Instance
		{
			get
			{
				return InstanceMember;
			}
		}

		/// <summary>
		///     Gets or sets the <see cref="IDictionary{Int64,IChangeTracker}" /> with the specified key.
		/// </summary>
		/// <value>
		///     The <see cref="IDictionary{Int64,IChangeTracker}" />.
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public override IDictionary<long, IChangeTracker<IMutableIdKey>> this[ EntityRelationshipModificationCacheKey key ]
		{
			get
			{
				return base[ key ];
			}
			set
			{
				lock ( _syncRoot )
				{
					base[ key ] = value;

					RemovePrimaryKey( key );
					AddPrimaryKey( key );
				}
			}
		}

		/// <summary>
		///     Adds the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>
        ///     True if the specified key/value pair was added; 
        ///     False if the specified key/value pair was updated.
        /// </returns>
		public override bool Add( EntityRelationshipModificationCacheKey key, IDictionary<long, IChangeTracker<IMutableIdKey>> value )
		{
            bool added;

			lock ( _syncRoot )
			{
                added = base.Add( key, value );

				AddPrimaryKey( key );
			}

            return added;
        }

		/// <summary>
		///     Clears this instance.
		/// </summary>
		public override void Clear( )
		{
			lock ( _syncRoot )
			{
				base.Clear( );

				_primaryKeyLookup.Clear( );
			}
		}

		/// <summary>
		///     Gets the keys by entity id.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <returns>
		///     A new set containing the keys in the cache that have the specified entity identifier.
		/// </returns>
		public ISet<EntityRelationshipModificationCacheKey> GetKeysByEntityId( long entityId )
		{
			ISet<EntityRelationshipModificationCacheKey> values;

			_primaryKeyLookup.TryGetValue( entityId, out values );

			if ( values == null )
			{
				return new HashSet<EntityRelationshipModificationCacheKey>( );
			}

			/////
			// Return a copy of the keys, not the set actually stored in the cache.
			/////
			return new HashSet<EntityRelationshipModificationCacheKey>( values );
		}

		/// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     True if the key was successfully removed from the cache, false otherwise.
		/// </returns>
		public override bool Remove( EntityRelationshipModificationCacheKey key )
		{
			lock ( _syncRoot )
			{
				RemovePrimaryKey( key );

				return base.Remove( key );
			}
		}

		/// <summary>
		///     Adds the primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		private void AddPrimaryKey( EntityRelationshipModificationCacheKey key )
		{
			ISet<EntityRelationshipModificationCacheKey> values;

			if ( !_primaryKeyLookup.TryGetValue( key.PrimaryKey.EntityId, out values ) )
			{
				values = new HashSet<EntityRelationshipModificationCacheKey>( );
				_primaryKeyLookup[ key.PrimaryKey.EntityId ] = values;
			}

			values.Add( key );
		}

		/// <summary>
		///     Removes the primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		private void RemovePrimaryKey( EntityRelationshipModificationCacheKey key )
		{
			ISet<EntityRelationshipModificationCacheKey> values;

			if ( _primaryKeyLookup.TryGetValue( key.PrimaryKey.EntityId, out values ) )
			{
				values.Remove( key );

				if ( values.Count == 0 )
				{
					_primaryKeyLookup.Remove( key.PrimaryKey.EntityId );
				}
			}
		}

		/// <summary>
		///     Used to key into the EntityRelationshipModificationCache data structure.
		/// </summary>
		[Immutable]
		internal class EntityRelationshipModificationCacheKey
		{
			/// <summary>
			///     Primary key.
			/// </summary>
			private readonly IEntityModificationToken _primaryKey;

			/// <summary>
			///     Secondary key.
			/// </summary>
			private readonly Direction _secondaryKey = Direction.Forward;

			/// <summary>
			///     Initializes a new instance of the <see cref="EntityFieldModificationCache.EntityFieldModificationCacheKey" /> class.
			/// </summary>
			/// <param name="primaryKey">The primary key.</param>
			/// <param name="secondaryKey">The secondary key.</param>
			public EntityRelationshipModificationCacheKey( IEntityModificationToken primaryKey, Direction secondaryKey )
			{
				_primaryKey = primaryKey;
				_secondaryKey = secondaryKey;
			}

			/// <summary>
			///     Gets the primary key.
			/// </summary>
			public IEntityModificationToken PrimaryKey
			{
				get
				{
					return _primaryKey;
				}
			}

			/// <summary>
			///     Gets the secondary key.
			/// </summary>
			public Direction SecondaryKey
			{
				get
				{
					return _secondaryKey;
				}
			}

			/// <summary>
			///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
			/// </summary>
			/// <param name="obj">
			///     The <see cref="System.Object" /> to compare with this instance.
			/// </param>
			/// <returns>
			///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
			/// </returns>
			public override bool Equals( object obj )
			{
				if ( obj == null )
				{
					return false;
				}

				var key = obj as EntityRelationshipModificationCacheKey;

				if ( key == null )
				{
					return false;
				}

				return PrimaryKey == key.PrimaryKey && SecondaryKey == key.SecondaryKey;
			}

			/// <summary>
			///     Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
			/// </returns>
			public override int GetHashCode( )
			{
				unchecked
				{
					int hash = 17;

					if ( PrimaryKey != null )
					{
						hash = hash * 92821 + PrimaryKey.GetHashCode( );
					}

					hash = hash * 92821 + SecondaryKey.GetHashCode( );

					return hash;
				}
			}

			/// <summary>
			///     Implements the operator !=.
			/// </summary>
			/// <param name="a">First object to compare.</param>
			/// <param name="b">Second object to compare.</param>
			/// <returns>
			///     The result of the operator.
			/// </returns>
			public static bool operator !=( EntityRelationshipModificationCacheKey a, EntityRelationshipModificationCacheKey b )
			{
				return !( a == b );
			}

			/// <summary>
			///     Implements the operator ==.
			/// </summary>
			/// <param name="a">First object to compare.</param>
			/// <param name="b">Second object to compare.</param>
			/// <returns>
			///     The result of the operator.
			/// </returns>
			public static bool operator ==( EntityRelationshipModificationCacheKey a, EntityRelationshipModificationCacheKey b )
			{
				if ( ReferenceEquals( a, b ) )
				{
					return true;
				}

				if ( ( ( object ) a == null ) || ( ( object ) b == null ) )
				{
					return false;
				}

				return Equals( a.PrimaryKey, b.PrimaryKey ) && a.SecondaryKey == b.SecondaryKey;
			}
		}
	}
}