// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	/// Detached proxy class that sits between the user code and the EntityRelationshipModificationCache.
	/// By default it is detached and once any modifications are made to it, it becomes attached.
	/// </summary>
	internal class EntityRelationshipModificationProxy : IDictionary<long, IChangeTracker<IMutableIdKey>>
	{
		/// <summary>
		/// Thread synchronization.
		/// </summary>
		private static readonly object _syncRoot = new object( );

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityRelationshipModificationProxy"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		internal EntityRelationshipModificationProxy( EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey key )
			: this( key, new ConcurrentDictionary<long, IChangeTracker<IMutableIdKey>>( ), false )
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityRelationshipModificationProxy"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <param name="isAttached">if set to <c>true</c> [is attached].</param>
		internal EntityRelationshipModificationProxy( EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey key, IDictionary<long, IChangeTracker<IMutableIdKey>> value, bool isAttached )
		{
			Key = key;
			Value = value;
			IsAttached = isAttached;
		}

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>
		/// The key.
		/// </value>
		public EntityRelationshipModificationCache.EntityRelationshipModificationCacheKey Key
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		public IDictionary<long, IChangeTracker<IMutableIdKey>> Value
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is attached.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is attached; otherwise, <c>false</c>.
		/// </value>
		private bool IsAttached
		{
			get;
			set;
		}

		/// <summary>
		/// Attaches to the EntityRelationshipModificationCache.
		/// </summary>
		public void Attach( )
		{
			lock ( _syncRoot )
			{
				if ( !IsAttached )
				{
					EntityRelationshipModificationCache.Instance [ Key ] = Value;
					IsAttached = true;
				}
			}
		}

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		public void Add( long key, IChangeTracker<IMutableIdKey> value )
		{
			if ( !IsAttached )
			{
				Attach( );
			}

			Value.Add( key, value );
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
		/// </returns>
		public bool ContainsKey( long key )
		{
			return Value.ContainsKey( key );
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		public ICollection<long> Keys
		{
			get
			{
				return Value.Keys;
			}
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </returns>
		public bool Remove( long key )
		{
			if ( !IsAttached )
			{
				Attach( );
			}

			return Value.Remove( key );
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
		/// <returns>
		/// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
		/// </returns>
		public bool TryGetValue( long key, out IChangeTracker<IMutableIdKey> value )
		{
			return Value.TryGetValue( key, out value );
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		public ICollection<IChangeTracker<IMutableIdKey>> Values
		{
			get
			{
				return Value.Values;
			}
		}

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public IChangeTracker<IMutableIdKey> this [ long key ]
		{
			get
			{
				return Value [ key ];
			}
			set
			{
				if ( !IsAttached )
				{
					Attach( );
				}

				Value[ key ] = value;
			}
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public void Add( KeyValuePair<long, IChangeTracker<IMutableIdKey>> item )
		{
			if ( !IsAttached )
			{
				Attach( );
			}

			Value.Add( item );
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public void Clear( )
		{
			if ( !IsAttached )
			{
				Attach( );
			}

			Value.Clear( );
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		public bool Contains( KeyValuePair<long, IChangeTracker<IMutableIdKey>> item )
		{
			if ( !IsAttached )
			{
				Attach( );
			}

			return Value.Contains( item );
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo( KeyValuePair<long, IChangeTracker<IMutableIdKey>> [ ] array, int arrayIndex )
		{
			Value.CopyTo( array, arrayIndex );
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public int Count
		{
			get
			{
				return Value.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return Value.IsReadOnly;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public bool Remove( KeyValuePair<long, IChangeTracker<IMutableIdKey>> item )
		{
			if ( !IsAttached )
			{
				Attach( );
			}

			return Value.Remove( item );
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyValuePair<long, IChangeTracker<IMutableIdKey>>> GetEnumerator( )
		{
			return Value.GetEnumerator( );
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
		{
			return ( ( System.Collections.IEnumerable ) Value ).GetEnumerator( );
		}
	}
}
