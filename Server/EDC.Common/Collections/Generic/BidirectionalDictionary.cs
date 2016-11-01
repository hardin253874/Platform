// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace EDC.Collections.Generic
{
	/// <summary>
	///     Bidirectional dictionary.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <remarks>
	///     This has minimal implementation. Expand as you see fit.
	/// </remarks>
	[Serializable]
	public class BidirectionalDictionary<TKey, TValue>
	{
		/// <summary>
		///     Forward dictionary.
		/// </summary>
		protected readonly Dictionary<TKey, TValue> ForwardDictionary = new Dictionary<TKey, TValue>( );

		/// <summary>
		///     Reverse dictionary.
		/// </summary>
		protected readonly Dictionary<TValue, TKey> ReverseDictionary = new Dictionary<TValue, TKey>( );

		/// <summary>
		///     Gets or sets the <see cref="TValue" /> with the specified key.
		/// </summary>
		/// <value>
		///     The <see cref="TValue" />.
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public virtual TValue this[ TKey key ]
		{
			set
			{
				TValue val;

				if ( ForwardDictionary.TryGetValue( key, out val ) )
				{
					ReverseDictionary.Remove( val );
				}

				ForwardDictionary[ key ] = value;
				ReverseDictionary[ value ] = key;
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
		public virtual ICollection<TValue> Values
		{
			get
			{
				return ForwardDictionary.Values;
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
		public virtual bool TryGetByValue( TValue value, out TKey key )
		{
			return ReverseDictionary.TryGetValue( value, out key );
		}

		/// <summary>
		///     Tries the get value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		///     True if the specified value was found; False otherwise.
		/// </returns>
		public virtual bool TryGetValue( TKey key, out TValue value )
		{
			return ForwardDictionary.TryGetValue( key, out value );
		}
	}
}