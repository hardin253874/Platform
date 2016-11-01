// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;

namespace EDC.Collections.Generic
{
	/// <summary>
	///     Delegate dictionary that runs a custom action when adding/removing items from the cache as well as clearing.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	public class DelegateDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		/// <summary>
		///     Underlying dictionary.
		/// </summary>
		private readonly Dictionary<TKey, TValue> _dictionary;

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		///     Initializes a new instance of the <see cref="DelegateDictionary&lt;TKey, TValue&gt;" /> class.
		/// </summary>
		public DelegateDictionary( )
		{
			_dictionary = new Dictionary<TKey, TValue>( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DelegateDictionary&lt;TKey, TValue&gt;" /> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		public DelegateDictionary( int capacity )
		{
			_dictionary = new Dictionary<TKey, TValue>( capacity );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DelegateDictionary&lt;TKey, TValue&gt;" /> class.
		/// </summary>
		/// <param name="comparer">The comparer.</param>
		public DelegateDictionary( IEqualityComparer<TKey> comparer )
		{
			_dictionary = new Dictionary<TKey, TValue>( comparer );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DelegateDictionary&lt;TKey, TValue&gt;" /> class.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		public DelegateDictionary( IDictionary<TKey, TValue> dictionary )
		{
			_dictionary = new Dictionary<TKey, TValue>( dictionary );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DelegateDictionary&lt;TKey, TValue&gt;" /> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		/// <param name="comparer">The comparer.</param>
		public DelegateDictionary( int capacity, IEqualityComparer<TKey> comparer )
		{
			_dictionary = new Dictionary<TKey, TValue>( capacity, comparer );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DelegateDictionary&lt;TKey, TValue&gt;" /> class.
		/// </summary>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="comparer">The comparer.</param>
		public DelegateDictionary( IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer )
		{
			_dictionary = new Dictionary<TKey, TValue>( dictionary, comparer );
		}

		/// <summary>
		///     Gets or sets the add action.
		/// </summary>
		/// <value>
		///     The add action.
		/// </value>
		public Action<TKey, TValue> AddAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the clear action.
		/// </summary>
		/// <value>
		///     The clear action.
		/// </value>
		public Action ClearAction
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the remove action.
		/// </summary>
		/// <value>
		///     The remove action.
		/// </value>
		public Action<TKey> RemoveAction
		{
			get;
			set;
		}

		/// <summary>
		///     Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///     <paramref name="key" /> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		///     An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		///     The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
		/// </exception>
		public void Add( TKey key, TValue value )
		{
			lock ( _syncRoot )
			{
				_dictionary.Add( key, value );

				if ( AddAction != null )
				{
					AddAction( key, value );
				}
			}
		}

		/// <summary>
		///     Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
		/// </summary>
		/// <param name="key">
		///     The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </param>
		/// <returns>
		///     true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		///     <paramref name="key" /> is null.
		/// </exception>
		public bool ContainsKey( TKey key )
		{
			lock ( _syncRoot )
			{
				return _dictionary.ContainsKey( key );
			}
		}

		/// <summary>
		///     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the
		///     <see
		///         cref="T:System.Collections.Generic.IDictionary`2" />
		///     .
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements
		///     <see
		///         cref="T:System.Collections.Generic.IDictionary`2" />
		///     .
		/// </returns>
		public ICollection<TKey> Keys
		{
			get
			{
				lock ( _syncRoot )
				{
					return _dictionary.Keys;
				}
			}
		}

		/// <summary>
		///     Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		///     true if the element is successfully removed; otherwise, false.  This method also returns false if
		///     <paramref
		///         name="key" />
		///     was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		///     <paramref name="key" /> is null.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		///     The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
		/// </exception>
		public bool Remove( TKey key )
		{
			lock ( _syncRoot )
			{
				bool returnValue = _dictionary.Remove( key );

				if ( RemoveAction != null )
				{
					RemoveAction( key );
				}

				return returnValue;
			}
		}

		/// <summary>
		///     Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">
		///     When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the
		///     <paramref
		///         name="value" />
		///     parameter. This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		///     true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		///     <paramref name="key" /> is null.
		/// </exception>
		public bool TryGetValue( TKey key, out TValue value )
		{
			lock ( _syncRoot )
			{
				return _dictionary.TryGetValue( key, out value );
			}
		}

		/// <summary>
		///     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the
		///     <see
		///         cref="T:System.Collections.Generic.IDictionary`2" />
		///     .
		/// </summary>
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
				return _dictionary.Values;
			}
		}

		/// <summary>
		///     Gets or sets the element with the specified key.
		/// </summary>
		/// <returns>
		///     The element with the specified key.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		///     <paramref name="key" /> is null.
		/// </exception>
		/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
		///     The property is retrieved and <paramref name="key" /> is not found.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		///     The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.
		/// </exception>
		public TValue this[ TKey key ]
		{
			get
			{
				lock ( _syncRoot )
				{
					return _dictionary[ key ];
				}
			}
			set
			{
				lock ( _syncRoot )
				{
					Remove( key );
					Add( key, value );
				}
			}
		}

		/// <summary>
		///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">
		///     The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </param>
		/// <exception cref="T:System.NotSupportedException">
		///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </exception>
		void ICollection<KeyValuePair<TKey, TValue>>.Add( KeyValuePair<TKey, TValue> item )
		{
			lock ( _syncRoot )
			{
				Add( item.Key, item.Value );
			}
		}

		/// <summary>
		///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </exception>
		public void Clear( )
		{
			lock ( _syncRoot )
			{
				_dictionary.Clear( );

				if ( ClearAction != null )
				{
					ClearAction( );
				}
			}
		}

		/// <summary>
		///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">
		///     The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </param>
		/// <returns>
		///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		bool ICollection<KeyValuePair<TKey, TValue>>.Contains( KeyValuePair<TKey, TValue> item )
		{
			lock ( _syncRoot )
			{
				return ( ( ICollection<KeyValuePair<TKey, TValue>> ) _dictionary ).Contains( item );
			}
		}

		/// <summary>
		///     Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo( KeyValuePair<TKey, TValue>[] array, int arrayIndex )
		{
			lock ( _syncRoot )
			{
				( ( ICollection<KeyValuePair<TKey, TValue>> ) _dictionary ).CopyTo( array, arrayIndex );
			}
		}

		/// <summary>
		///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <returns>
		///     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public int Count
		{
			get
			{
				lock ( _syncRoot )
				{
					return _dictionary.Count;
				}
			}
		}

		/// <summary>
		///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <returns>
		///     true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
		/// </returns>
		public bool IsReadOnly
		{
			get
			{
				return ( ( IDictionary ) _dictionary ).IsReadOnly;
			}
		}

		/// <summary>
		///     Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">
		///     The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </param>
		/// <returns>
		///     true if <paramref name="item" /> was successfully removed from the
		///     <see
		///         cref="T:System.Collections.Generic.ICollection`1" />
		///     ; otherwise, false. This method also returns false if
		///     <paramref
		///         name="item" />
		///     is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </exception>
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove( KeyValuePair<TKey, TValue> item )
		{
			lock ( _syncRoot )
			{
				return ( ( ICollection<KeyValuePair<TKey, TValue>> ) _dictionary ).Remove( item );
			}
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator( )
		{
			lock ( _syncRoot )
			{
				return _dictionary.GetEnumerator( );
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
			lock ( _syncRoot )
			{
				return _dictionary.GetEnumerator( );
			}
		}
	}
}