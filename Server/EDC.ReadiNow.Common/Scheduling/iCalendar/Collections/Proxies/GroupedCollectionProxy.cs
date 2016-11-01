// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     A proxy for a keyed list.
	/// </summary>
	[Serializable]
	public class GroupedCollectionProxy<TGroup, TOriginal, TNew> : IGroupedCollectionProxy<TGroup, TOriginal, TNew>
		where TOriginal : class, IGroupedObject<TGroup>
		where TNew : class, TOriginal
	{
		/// <summary>
		///     Real Object.
		/// </summary>
		private IGroupedCollection<TGroup, TOriginal> _realObject;

		/// <summary>
		///     Predicate.
		/// </summary>
		private readonly Func<TNew, bool> _predicate;

		/// <summary>
		///     Initializes a new instance of the <see cref="GroupedCollectionProxy{TGroup, TOriginal, TNew}" /> class.
		/// </summary>
		/// <param name="realObject">The real object.</param>
		/// <param name="predicate">The predicate.</param>
		public GroupedCollectionProxy( IGroupedCollection<TGroup, TOriginal> realObject, Func<TNew, bool> predicate = null )
		{
			_predicate = predicate ?? ( o => true );
			SetProxiedObject( realObject );

			_realObject.ItemAdded += _RealObject_ItemAdded;
			_realObject.ItemRemoved += _RealObject_ItemRemoved;
		}

		/// <summary>
		///     _s the real object_ item removed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void _RealObject_ItemRemoved( object sender, ObjectEventArgs<TOriginal, int> e )
		{
			if ( e.First is TNew )
			{
				OnItemRemoved( ( TNew ) e.First, e.Second );
			}
		}

		/// <summary>
		///     _s the real object_ item added.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void _RealObject_ItemAdded( object sender, ObjectEventArgs<TOriginal, int> e )
		{
			if ( e.First is TNew )
			{
				OnItemAdded( ( TNew ) e.First, e.Second );
			}
		}

		#region IGroupedCollection Members

		/// <summary>
		///     Occurs when an item is added.
		/// </summary>
		public event EventHandler<ObjectEventArgs<TNew, int>> ItemAdded;

		/// <summary>
		///     Occurs when an item is removed.
		/// </summary>
		public event EventHandler<ObjectEventArgs<TNew, int>> ItemRemoved;

		/// <summary>
		///     Called when items are added.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="index">The index.</param>
		private void OnItemAdded( TNew item, int index )
		{
			EventHandler<ObjectEventArgs<TNew, int>> onItemAdded = ItemAdded;

			if ( onItemAdded != null )
			{
				onItemAdded( this, new ObjectEventArgs<TNew, int>( item, index ) );
			}
		}

		/// <summary>
		///     Called when items are removed.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="index">The index.</param>
		private void OnItemRemoved( TNew item, int index )
		{
			EventHandler<ObjectEventArgs<TNew, int>> onItemRemoved = ItemRemoved;

			if ( onItemRemoved != null )
			{
				onItemRemoved( this, new ObjectEventArgs<TNew, int>( item, index ) );
			}
		}

		/// <summary>
		///     Removes the specified group.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		public bool Remove( TGroup group )
		{
			return _realObject.Remove( group );
		}

		/// <summary>
		///     Clears the specified group.
		/// </summary>
		/// <param name="group">The group.</param>
		public void Clear( TGroup group )
		{
			_realObject.Clear( group );
		}

		/// <summary>
		///     Determines whether the specified group contains key.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns>
		///     <c>true</c> if the specified group contains key; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsKey( TGroup group )
		{
			return _realObject.ContainsKey( group );
		}

		/// <summary>
		///     Counts the of.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		public int CountOf( TGroup group )
		{
			return _realObject
				.AllOf( group )
				.OfType<TNew>( )
				.Where( _predicate )
				.Count( );
		}

		/// <summary>
		///     Alls the of.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		public IEnumerable<TNew> AllOf( TGroup group )
		{
			return _realObject
				.AllOf( group )
				.OfType<TNew>( )
				.Where( _predicate );
		}

		/// <summary>
		///     Sorts the keys.
		/// </summary>
		/// <param name="comparer">The comparer.</param>
		public void SortKeys( IComparer<TGroup> comparer = null )
		{
			_realObject.SortKeys( comparer );
		}

		/// <summary>
		///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">
		///     The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </param>
		public void Add( TNew item )
		{
			_realObject.Add( item );
		}

		/// <summary>
		///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public void Clear( )
		{
			// Only clear items of this type
			// that match the predicate.

			TNew[] items = _realObject
				.OfType<TNew>( )
				.Where( _predicate )
				.ToArray( );

			foreach ( TNew item in items )
			{
				_realObject.Remove( item );
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
		public bool Contains( TNew item )
		{
			return _realObject.Contains( item );
		}

		/// <summary>
		///     Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo( TNew[] array, int arrayIndex )
		{
			int i = 0;
			foreach ( TNew item in this )
			{
				array[ arrayIndex + ( i++ ) ] = item;
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
				return _realObject
					.OfType<TNew>( )
					.Where( _predicate )
					.Count( );
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
				return false;
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
		public bool Remove( TNew item )
		{
			return _realObject.Remove( item );
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<TNew> GetEnumerator( )
		{
			return _realObject
				.OfType<TNew>( )
				.Where( _predicate )
				.GetEnumerator( );
		}

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator( )
		{
			return _realObject
				.OfType<TNew>( )
				.Where( _predicate )
				.GetEnumerator( );
		}

		#endregion

		#region IGroupedCollectionProxy Members

		/// <summary>
		///     Gets the real object.
		/// </summary>
		/// <value>
		///     The real object.
		/// </value>
		public IGroupedCollection<TGroup, TOriginal> RealObject
		{
			get
			{
				return _realObject;
			}
		}

		/// <summary>
		///     Sets the proxied object.
		/// </summary>
		/// <param name="realObject">The real object.</param>
		public void SetProxiedObject( IGroupedCollection<TGroup, TOriginal> realObject )
		{
			_realObject = realObject;
		}

		#endregion
	}
}