// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Migration.Contract;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Cardinality aware dictionary
	/// </summary>
	public class CardinalityAwareDictionary : IDictionary<RelationshipEntryKey, RelationshipEntry>
	{
		/// <summary>
		///     The encapsulated dictionary
		/// </summary>
		private readonly Dictionary<RelationshipEntryKey, RelationshipEntry> _dictionary = new Dictionary<RelationshipEntryKey, RelationshipEntry>( );

		/// <summary>
		///     Tracks 'From One' cardinality
		/// </summary>
		private readonly Dictionary<RelationshipEntryCardinalityKey, RelationshipEntry> _fromOne = new Dictionary<RelationshipEntryCardinalityKey, RelationshipEntry>( );

		/// <summary>
		///     Tracks 'To One' cardinality
		/// </summary>
		private readonly Dictionary<RelationshipEntryCardinalityKey, RelationshipEntry> _toOne = new Dictionary<RelationshipEntryCardinalityKey, RelationshipEntry>( );

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyValuePair<RelationshipEntryKey, RelationshipEntry>> GetEnumerator( )
		{
			return _dictionary.GetEnumerator( );
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
		///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public void Add( KeyValuePair<RelationshipEntryKey, RelationshipEntry> item )
		{
			Add( item.Key, item.Value );
		}

		/// <summary>
		///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		public void Clear( )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
		///     otherwise, false.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool Contains( KeyValuePair<RelationshipEntryKey, RelationshipEntry> item )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		/// <exception cref="NotImplementedException"></exception>
		public void CopyTo( KeyValuePair<RelationshipEntryKey, RelationshipEntry>[ ] array, int arrayIndex )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Removes the first occurrence of a specific object from the
		///     <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		///     true if <paramref name="item" /> was successfully removed from the
		///     <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if
		///     <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool Remove( KeyValuePair<RelationshipEntryKey, RelationshipEntry> item )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		public int Count
		{
			get
			{
				return _dictionary.Count;
			}
		}

		/// <summary>
		///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		///     Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the
		///     specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
		/// <returns>
		///     true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise,
		///     false.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool ContainsKey( RelationshipEntryKey key )
		{
			if ( key.Cardinality != null && ( key.Cardinality == CardinalityEnum_Enumeration.ManyToOne || key.Cardinality == CardinalityEnum_Enumeration.OneToOne ) )
			{
				var cardinalityKey = new RelationshipEntryCardinalityKey( key.TypeId, key.FromId );

				if ( _toOne.ContainsKey( cardinalityKey ) )
				{
					return true;
				}
			}

			if ( key.Cardinality != null && ( key.Cardinality == CardinalityEnum_Enumeration.OneToMany || key.Cardinality == CardinalityEnum_Enumeration.OneToOne ) )
			{
				var cardinalityKey = new RelationshipEntryCardinalityKey( key.TypeId, key.ToId );

				if ( _fromOne.ContainsKey( cardinalityKey ) )
				{
					return true;
				}
			}

			return _dictionary.ContainsKey( key );
		}

		/// <summary>
		///     Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		public void Add( RelationshipEntryKey key, RelationshipEntry value )
		{
			if ( key.Cardinality != null && ( key.Cardinality == CardinalityEnum_Enumeration.ManyToOne || key.Cardinality == CardinalityEnum_Enumeration.OneToOne ) )
			{
				var cardinalityKey = new RelationshipEntryCardinalityKey( key.TypeId, key.FromId );

				_toOne.Add( cardinalityKey, value );
			}

			if ( key.Cardinality != null && ( key.Cardinality == CardinalityEnum_Enumeration.OneToMany || key.Cardinality == CardinalityEnum_Enumeration.OneToOne ) )
			{
				var cardinalityKey = new RelationshipEntryCardinalityKey( key.TypeId, key.ToId );

				_fromOne.Add( cardinalityKey, value );
			}

			_dictionary.Add( key, value );
		}

		/// <summary>
		///     Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		///     true if the element is successfully removed; otherwise, false.  This method also returns false if
		///     <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool Remove( RelationshipEntryKey key )
		{
			throw new NotImplementedException( );
		}

		/// <summary>
		///     Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">
		///     When this method returns, the value associated with the specified key, if the key is found;
		///     otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed
		///     uninitialized.
		/// </param>
		/// <returns>
		///     true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element
		///     with the specified key; otherwise, false.
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool TryGetValue( RelationshipEntryKey key, out RelationshipEntry value )
		{
			if ( key.Cardinality != null && ( key.Cardinality == CardinalityEnum_Enumeration.ManyToOne || key.Cardinality == CardinalityEnum_Enumeration.OneToOne ) )
			{
				var cardinalityKey = new RelationshipEntryCardinalityKey( key.TypeId, key.FromId );

				if ( _toOne.TryGetValue( cardinalityKey, out value ) )
				{
					return true;
				}
			}

			if ( key.Cardinality != null && ( key.Cardinality == CardinalityEnum_Enumeration.OneToMany || key.Cardinality == CardinalityEnum_Enumeration.OneToOne ) )
			{
				var cardinalityKey = new RelationshipEntryCardinalityKey( key.TypeId, key.ToId );

				if ( _fromOne.TryGetValue( cardinalityKey, out value ) )
				{
					return true;
				}
			}

			return _dictionary.TryGetValue( key, out value );
		}

		/// <summary>
		///     Gets or sets the element with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException">
		/// </exception>
		public RelationshipEntry this[ RelationshipEntryKey key ]
		{
			get
			{
				throw new NotImplementedException( );
			}
			set
			{
				throw new NotImplementedException( );
			}
		}

		/// <summary>
		///     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the
		///     <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		public ICollection<RelationshipEntryKey> Keys
		{
			get
			{
				return _dictionary.Keys;
			}
		}

		/// <summary>
		///     Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the
		///     <see cref="T:System.Collections.Generic.IDictionary`2" />.
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		public ICollection<RelationshipEntry> Values
		{
			get
			{
				return _dictionary.Values;
			}
		}
	}
}