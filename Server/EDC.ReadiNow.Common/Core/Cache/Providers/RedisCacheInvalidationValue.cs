// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.Serialization.Surrogates;
using ProtoBuf;
using ProtoBuf.Meta;

namespace EDC.ReadiNow.Core.Cache.Providers
{
	/// <summary>
	///     Pair containing the cache value and its serializable invalidation information.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	[ProtoContract]
	public class RedisCacheInvalidationValue<TKey, TValue>
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="RedisCacheInvalidationValue{TKey, TValue}" /> class from being
		///     created.
		/// </summary>
		private RedisCacheInvalidationValue( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisCacheInvalidationValue{TKey, TValue}" /> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="invalidationKey">The invalidation key.</param>
		public RedisCacheInvalidationValue( TValue value, Lazy<SerializableCacheInvalidationKey<TKey>> invalidationKey )
			: this( )
		{
			Value = value;
			InvalidationKey = invalidationKey;
			CreationDate = DateTime.UtcNow;
		}

		/// <summary>
		///     Gets the creation date.
		/// </summary>
		/// <value>
		///     The creation date.
		/// </value>
		[ProtoMember( 3 )]
		public DateTime CreationDate
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the invalidation key.
		/// </summary>
		/// <value>
		///     The invalidation key.
		/// </value>
		[ProtoMember( 2 )]
		public Lazy<SerializableCacheInvalidationKey<TKey>> InvalidationKey
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		[ProtoMember( 1 )]
		public TValue Value
		{
			get;
			private set;
		}
	}
}