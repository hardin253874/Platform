// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;

namespace EDC.ReadiNow.Messaging
{
	/// <summary>
	///     Interprocess communications.
	/// </summary>
	public static partial class InterprocessCommunications
	{
		/// <summary>
		///     Bidirectional dictionary.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		[Serializable]
		private class BidirectionalDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
		{
			/// <summary>
			///     Forward dictionary.
			/// </summary>
			private readonly Dictionary<TKey, TValue> _forward = new Dictionary<TKey, TValue>( );

			/// <summary>
			///     Reverse dictionary.
			/// </summary>
			private readonly Dictionary<TValue, TKey> _reverse = new Dictionary<TValue, TKey>( );

			/// <summary>
			///     Thread safety.
			/// </summary>
			private readonly object _syncRoot = new object( );

			/// <summary>
			///     Gets or sets the <see cref="TValue" /> with the specified key.
			/// </summary>
			/// <value>
			///     The <see cref="TValue" />.
			/// </value>
			/// <param name="key">The key.</param>
			/// <returns></returns>
			public TValue this[ TKey key ]
			{
				set
				{
					lock ( _syncRoot )
					{
						TValue val;

						if ( _forward.TryGetValue( key, out val ) )
						{
							_reverse.Remove( val );
						}

						_forward[ key ] = value;
						_reverse[ value ] = key;
					}
				}
			}

			/// <summary>
			///     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the
			///     <see
			///         cref="T:System.Collections.Generic.IDictionary`2" />
			///     .
			/// </summary>
			/// <value>
			///     The values.
			/// </value>
			/// <returns>
			///     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements
			///     <see
			///         cref="T:System.Collections.Generic.IDictionary`2" />
			///     .
			/// </returns>
			public ICollection<TValue> Values
			{
				get
				{
					lock ( _syncRoot )
					{
						return _forward.Values;
					}
				}
			}

			/// <summary>
			///     Gets the enumerator.
			/// </summary>
			/// <returns>
			///     An enumerator.
			/// </returns>
			public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator( )
			{
				lock ( _syncRoot )
				{
					return _forward.GetEnumerator( );
				}
			}

			/// <summary>
			///     Returns an enumerator that iterates through a collection.
			/// </summary>
			/// <returns>
			///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
			/// </returns>
			IEnumerator IEnumerable.GetEnumerator( )
			{
				return GetEnumerator( );
			}

			/// <summary>
			///     Tries the get by value.
			/// </summary>
			/// <param name="value">The value.</param>
			/// <param name="key">The key.</param>
			/// <returns>
			///     True if the key was found; False otherwise.
			/// </returns>
			public bool TryGetByValue( TValue value, out TKey key )
			{
				lock ( _syncRoot )
				{
					return _reverse.TryGetValue( value, out key );
				}
			}

			/// <summary>
			///     Tries the get value.
			/// </summary>
			/// <param name="key">The key.</param>
			/// <param name="value">The value.</param>
			/// <returns>
			///     True if the specified value was found; False otherwise.
			/// </returns>
			public bool TryGetValue( TKey key, out TValue value )
			{
				lock ( _syncRoot )
				{
					return _forward.TryGetValue( key, out value );
				}
			}

			/// <summary>
			/// Gets the count.
			/// </summary>
			/// <value>
			/// The count.
			/// </value>
			public int Count
			{
				get
				{
					return _forward.Count;
				}
			}
		}
	}
}