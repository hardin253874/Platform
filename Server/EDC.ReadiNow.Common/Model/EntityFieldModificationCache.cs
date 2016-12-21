// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Cache that stores the modifications made to an entity.
	/// </summary>
	[SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
	internal sealed class EntityFieldModificationCache : CacheBase<EntityFieldModificationCache.EntityFieldModificationCacheKey, IEntityFieldValues>
	{
        /// <summary>
        ///     Represents the singleton instance of the cache.
        /// </summary>
        private static readonly Lazy<EntityFieldModificationCache> CacheInstance = new Lazy<EntityFieldModificationCache>( ( ) => new EntityFieldModificationCache( ), true );

        /// <summary>
        ///     Primary Key lookup.
        /// </summary>
        private readonly Dictionary<long, ISet<EntityFieldModificationCacheKey>> _primaryKeyLookup = new Dictionary<long, ISet<EntityFieldModificationCacheKey>>( );

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		///     Prevents a default instance of the <see cref="EntityFieldModificationCache" /> class from being created.
		/// </summary>
		private EntityFieldModificationCache( )
            : base("Entity Field Modification", false, false)
        {
		}

	    /// <summary>
	    ///     Gets the instance.
	    /// </summary>
	    public static EntityFieldModificationCache Instance => CacheInstance.Value;

		/// <summary>
		///     Gets or sets the <see cref="IDictionary{Int64,Object}" /> with the specified key.
		/// </summary>
		/// <value>
		///     The <see cref="IDictionary{Int64,Object}" />.
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
        public override IEntityFieldValues this [ EntityFieldModificationCacheKey key ]
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
        public override bool Add( EntityFieldModificationCacheKey key, IEntityFieldValues value )
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
		public ISet<EntityFieldModificationCacheKey> GetKeysByEntityId( long entityId )
		{
			ISet<EntityFieldModificationCacheKey> values;

			_primaryKeyLookup.TryGetValue( entityId, out values );

			if ( values == null )
			{
				return new HashSet<EntityFieldModificationCacheKey>( );
			}

			/////
			// Return a copy of the keys, not the set actually stored in the cache.
			/////
			return new HashSet<EntityFieldModificationCacheKey>( values );
		}

		/// <summary>
		///     Removes the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     True if the key was successfully removed from the cache, false otherwise.
		/// </returns>
		public override bool Remove( EntityFieldModificationCacheKey key )
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
		private void AddPrimaryKey( EntityFieldModificationCacheKey key )
		{
			ISet<EntityFieldModificationCacheKey> values;

			if ( !_primaryKeyLookup.TryGetValue( key.PrimaryKey.EntityId, out values ) )
			{
				values = new HashSet<EntityFieldModificationCacheKey>( );
				_primaryKeyLookup[ key.PrimaryKey.EntityId ] = values;
			}

			values.Add( key );
		}

		/// <summary>
		///     Removes the primary key.
		/// </summary>
		/// <param name="key">The key.</param>
		private void RemovePrimaryKey( EntityFieldModificationCacheKey key )
		{
			ISet<EntityFieldModificationCacheKey> values;

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
		///     Used to key into the EntityFieldModificationCache data structure.
		/// </summary>
		[Immutable]
		internal class EntityFieldModificationCacheKey
		{
			/// <summary>
			///     Primary key.
			/// </summary>
			private readonly IEntityModificationToken _primaryKey;

			/// <summary>
			///     Initializes a new instance of the <see cref="EntityFieldModificationCacheKey" /> class.
			/// </summary>
			/// <param name="primaryKey">The primary key.</param>
			public EntityFieldModificationCacheKey( IEntityModificationToken primaryKey )
			{
				_primaryKey = primaryKey;
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

				var key = obj as EntityFieldModificationCacheKey;

				if ( key == null )
				{
					return false;
				}

				return PrimaryKey == key.PrimaryKey;
			}

			/// <summary>
			///     Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
			/// </returns>
			public override int GetHashCode( )
			{
				return PrimaryKey.GetHashCode( );
			}

			/// <summary>
			///     Implements the operator !=.
			/// </summary>
			/// <param name="a">First object to compare.</param>
			/// <param name="b">Second object to compare.</param>
			/// <returns>
			///     The result of the operator.
			/// </returns>
			public static bool operator !=( EntityFieldModificationCacheKey a, EntityFieldModificationCacheKey b )
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
			public static bool operator ==( EntityFieldModificationCacheKey a, EntityFieldModificationCacheKey b )
			{
				if ( ReferenceEquals( a, b ) )
				{
					return true;
				}

				if ( ( ( object ) a == null ) || ( ( object ) b == null ) )
				{
					return false;
				}

				return Equals( a.PrimaryKey, b.PrimaryKey );
			}
		}
	}
}