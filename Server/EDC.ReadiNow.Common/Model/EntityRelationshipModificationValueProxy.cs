// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	/// Detached proxy class that sits between the user code and the values stored in the EntityRelationshipModificationCache.
	/// /// By default it is detached and once any modifications are made to it, it becomes attached.
	/// </summary>
	internal class EntityRelationshipModificationValueProxy : IChangeTracker<IMutableIdKey>
	{
		/// <summary>
		/// Thread synchronization.
		/// </summary>
		private static readonly object _syncRoot = new object( );

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityRelationshipModificationValueProxy"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="relationshipTypeId">The relationship type identifier.</param>
		internal EntityRelationshipModificationValueProxy( EntityRelationshipModificationProxy key, long relationshipTypeId )
			: this( key, relationshipTypeId, null )
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityRelationshipModificationValueProxy"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="relationshipTypeId">The relationship type identifier.</param>
		/// <param name="initialCollection">The initial collection.</param>
		internal EntityRelationshipModificationValueProxy( EntityRelationshipModificationProxy key, long relationshipTypeId, IEnumerable<IMutableIdKey> initialCollection )
		{
			Key = key;
			RelationshipTypeId = relationshipTypeId;
			IsAttached = false;

			if ( initialCollection == null )
			{
				Value = new ChangeTracker<IMutableIdKey>( );
			}
			else
			{
				Value = new ChangeTracker<IMutableIdKey>( initialCollection );
			}
		}

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>
		/// The key.
		/// </value>
		public EntityRelationshipModificationProxy Key
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the relationship type identifier.
		/// </summary>
		/// <value>
		/// The relationship type identifier.
		/// </value>
		public long RelationshipTypeId
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
		public ChangeTracker<IMutableIdKey> Value
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
		/// Attaches this instance.
		/// </summary>
		private void Attach( )
		{
			lock ( _syncRoot )
			{
				if ( !IsAttached )
				{
					Key.Attach( );
					Key.Value [ RelationshipTypeId ] = Value;
					IsAttached = true;
				}
			}
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public void Add( IMutableIdKey item )
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
		public bool Contains( IMutableIdKey item )
		{
			return Value.Contains( item );
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo( IMutableIdKey [ ] array, int arrayIndex )
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
		public bool Remove( IMutableIdKey item )
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
		public IEnumerator<IMutableIdKey> GetEnumerator( )
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
			return ((System.Collections.IEnumerable)this).GetEnumerator( );
		}

		/// <summary>
		/// Resets the object’s state to unchanged by accepting the modifications.
		/// </summary>
		public void AcceptChanges( )
		{
			Value.AcceptChanges( );
		}

		/// <summary>
		/// Gets the object's changed status.
		/// </summary>
		public bool IsChanged
		{
			get
			{
				return Value.IsChanged;
			}
		}

		/// <summary>
		/// Gets the added.
		/// </summary>
		/// <value>
		/// The added.
		/// </value>
		public IEnumerable<IMutableIdKey> Added
		{
			get
			{
				return Value.Added;
			}
		}

		/// <summary>
		/// Gets the collection.
		/// </summary>
		/// <value>
		/// The collection.
		/// </value>
		public IEnumerable<IMutableIdKey> Collection
		{
			get
			{
				return Value.Collection;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="EntityRelationshipModificationValueProxy"/> is flushed.
		/// </summary>
		/// <value>
		///   <c>true</c> if flushed; otherwise, <c>false</c>.
		/// </value>
		public bool Flushed
		{
			get
			{
				return Value.Flushed;
			}
			set
			{
				Value.Flushed = value;
			}
		}

		/// <summary>
		/// Gets the removed.
		/// </summary>
		/// <value>
		/// The removed.
		/// </value>
		public IEnumerable<IMutableIdKey> Removed
		{
			get
			{
				return Value.Removed;
			}
		}
	}
}
