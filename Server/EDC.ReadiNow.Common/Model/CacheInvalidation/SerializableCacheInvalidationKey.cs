// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using ProtoBuf;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
	/// <summary>
	///     Serializable CacheInvalidation key.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	[ProtoContract]
	public class SerializableCacheInvalidationKey<TKey>
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="SerializableCacheInvalidationKey{TKey}" /> class from being created.
		/// </summary>
		private SerializableCacheInvalidationKey( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SerializableCacheInvalidationKey{TKey}" /> class.
		/// </summary>
		/// <param name="name">The name of the invalidation cache.</param>
		/// <param name="key">The key.</param>
		public SerializableCacheInvalidationKey( string name, TKey key )
			: this( )
		{
			Name = name;
			Key = key;
		}

		/// <summary>
		///     Gets or sets the entity invalidating relationship types to cache key.
		/// </summary>
		/// <value>
		///     The entity invalidating relationship types to cache key.
		/// </value>
		[ProtoMember( 6 )]
		public IEnumerable<long> EntityInvalidatingRelationshipTypesToCacheKey
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the entity to cache key.
		/// </summary>
		/// <value>
		///     The entity to cache key.
		/// </value>
		[ProtoMember( 3 )]
		public IEnumerable<long> EntityToCacheKey
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the entity type to cache key.
		/// </summary>
		/// <value>
		///     The entity type to cache key.
		/// </value>
		[ProtoMember( 7 )]
		public IEnumerable<long> EntityTypeToCacheKey
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the field type to cache key.
		/// </summary>
		/// <value>
		///     The field type to cache key.
		/// </value>
		[ProtoMember( 4 )]
		public IEnumerable<long> FieldTypeToCacheKey
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the key.
		/// </summary>
		/// <value>
		///     The key.
		/// </value>
		[ProtoMember( 2 )]
		public TKey Key
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[ProtoMember( 1 )]
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the relationship type to cache key.
		/// </summary>
		/// <value>
		///     The relationship type to cache key.
		/// </value>
		[ProtoMember( 5 )]
		public IEnumerable<long> RelationshipTypeToCacheKey
		{
			get;
			set;
		}
	}
}