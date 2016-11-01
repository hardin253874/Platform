// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;

namespace EDC.Collections.Generic
{
	/// <summary>
	///     ConcurrentDictionary extension methods
	/// </summary>
	public static class ConcurrentDictionaryExtensions
	{
		/// <summary>
		///     Adds the or update.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="addValueFactory">The add value factory.</param>
		/// <param name="updateValueFactory">The update value factory.</param>
		/// <param name="added">if set to <c>true</c> [added].</param>
		/// <param name="updated">if set to <c>true</c> [updated].</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		///     dictionary
		///     or
		///     key
		///     or
		///     addValueFactory
		///     or
		///     updateValueFactory
		/// </exception>
		public static TValue AddOrUpdate<TKey, TValue>( this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory, out bool added, out bool updated )
		{
			if ( dictionary == null )
			{
				throw new ArgumentNullException( "dictionary" );
			}

			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			if ( addValueFactory == null )
			{
				throw new ArgumentNullException( "addValueFactory" );
			}

			if ( updateValueFactory == null )
			{
				throw new ArgumentNullException( "updateValueFactory" );
			}

			added = false;
			updated = false;

			while ( true )
			{
				TValue oldValue;

				TValue newValue;

				if ( dictionary.TryGetValue( key, out oldValue ) )
				{
					/////
					// Key exists, try to update
					/////
					newValue = updateValueFactory( key, oldValue );

					if ( dictionary.TryUpdate( key, newValue, oldValue ) )
					{
						updated = true;
						return newValue;
					}
				}
				else
				{
					/////
					// Try to add
					/////
					newValue = addValueFactory( key );

					if ( dictionary.TryAdd( key, newValue ) )
					{
						added = true;
						return newValue;
					}
				}
			}
		}

		/// <summary>
		///     Gets the or add.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="valueFactory">The value factory.</param>
		/// <param name="added">if set to <c>true</c> [added].</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		///     dictionary
		///     or
		///     key
		///     or
		///     valueFactory
		/// </exception>
		public static TValue GetOrAdd<TKey, TValue>( this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory, out bool added )
		{
			if ( dictionary == null )
			{
				throw new ArgumentNullException( "dictionary" );
			}

			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			if ( valueFactory == null )
			{
				throw new ArgumentNullException( "valueFactory" );
			}

			while ( true )
			{
				TValue value;

				if ( dictionary.TryGetValue( key, out value ) )
				{
					added = false;
					return value;
				}

				value = valueFactory( key );
				if ( dictionary.TryAdd( key, value ) )
				{
					added = true;
					return value;
				}
			}
		}
	}
}