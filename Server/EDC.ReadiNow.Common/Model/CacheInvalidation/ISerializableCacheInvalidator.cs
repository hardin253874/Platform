// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.ReadiNow.Model.CacheInvalidation
{
	/// <summary>
	///     Serialize cache invalidation information
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	public interface ISerializableCacheInvalidator<TKey>
	{
		/// <summary>
		///     A unique name for this cache invalidator that supports serialization.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		string Name
		{
			get;
		}

		/// <summary>
		///     Merge the values for the specified serializable cache invalidation key with those already present.
		/// </summary>
		/// <param name="lazySerializableKey">The lazy serializable key.</param>
		void FromSerializableKey( Lazy<SerializableCacheInvalidationKey<TKey>> lazySerializableKey );

		/// <summary>
		///     Extract the cache invalidation values for the specified key into a serializable structure.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>A serializable structure containing the values for the specified invalidation key</returns>
		Lazy<SerializableCacheInvalidationKey<TKey>> ToSerializableKey( TKey key );
	}
}