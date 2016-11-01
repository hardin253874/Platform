// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.Collections.Generic
{
	/// <summary>
	///     A thread-safe bi-directional dictionary.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class ConcurrentBidirectionalDictionary<TKey, TValue> : BidirectionalDictionary<TKey, TValue>
	{
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
		public override TValue this[ TKey key ]
		{
			set
			{
				lock ( _syncRoot )
				{
					base[ key ] = value;
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
		public override ICollection<TValue> Values
		{
			get
			{
				lock ( _syncRoot )
				{
					return base.Values;
				}
			}
		}

		/// <summary>
		///     Tries the get by value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="key">The key.</param>
		/// <returns>
		///     True if the key was found; False otherwise.
		/// </returns>
		public override bool TryGetByValue( TValue value, out TKey key )
		{
			lock ( _syncRoot )
			{
				return base.TryGetByValue( value, out key );
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
		public override bool TryGetValue( TKey key, out TValue value )
		{
			lock ( _syncRoot )
			{
				return base.TryGetValue( key, out value );
			}
		}
	}
}