// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Scheduling.iCalendar.Collections
{
	/// <summary>
	///     A list of objects that are keyed.
	/// </summary>
	[Serializable]
	public class GroupedList<TGroup, TItem> : IGroupedList<TGroup, TItem>
		where TItem : class, IGroupedObject<TGroup>
	{
		/// <summary>
		///     Lists
		/// </summary>
		private readonly List<IMultiLinkedList<TItem>> _lists = new List<IMultiLinkedList<TItem>>( );

		/// <summary>
		///     List map.
		/// </summary>
		private readonly Dictionary<TGroup, IMultiLinkedList<TItem>> _dictionary = new Dictionary<TGroup, IMultiLinkedList<TItem>>( );

		/// <summary>
		///     Subscribes to key changes.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		private TItem SubscribeToKeyChanges( TItem item )
		{
			if ( item != null )
			{
				item.GroupChanged += item_GroupChanged;
			}
			return item;
		}

		/// <summary>
		///     Unsubscribes from key changes.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		private TItem UnsubscribeFromKeyChanges( TItem item )
		{
			if ( item != null )
			{
				item.GroupChanged -= item_GroupChanged;
			}
			return item;
		}

		/// <summary>
		///     Groups the modifier.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">The item's group cannot be null.</exception>
		protected virtual TGroup GroupModifier( TGroup group )
		{
			if ( EqualityComparer<TGroup>.Default.Equals( group, default( TGroup ) ) )
			{
				throw new ArgumentNullException( "group", @"The item's group cannot be null." );
			}

			return group;
		}

		/// <summary>
		///     Ensures the list.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <param name="createIfNecessary">
		///     if set to <c>true</c> [create if necessary].
		/// </param>
		/// <returns></returns>
		private IMultiLinkedList<TItem> EnsureList( TGroup group, bool createIfNecessary )
		{
			if ( !_dictionary.ContainsKey( group ) )
			{
				if ( createIfNecessary )
				{
					var list = new MultiLinkedList<TItem>( );
					_dictionary[ group ] = list;

					if ( _lists.Count > 0 )
					{
						// Attach the list to our list chain
						IMultiLinkedList<TItem> previous = _lists[ _lists.Count - 1 ];
						previous.SetNext( list );
						list.SetPrevious( previous );
					}

					_lists.Add( list );
					return list;
				}
			}
			else
			{
				return _dictionary[ group ];
			}
			return null;
		}

		/// <summary>
		///     Lists for index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="relativeIndex">Index of the relative.</param>
		/// <returns></returns>
		private IMultiLinkedList<TItem> ListForIndex( int index, out int relativeIndex )
		{
			foreach ( var list in _lists )
			{
				if ( list.StartIndex <= index &&
				     list.ExclusiveEnd > index )
				{
					relativeIndex = index - list.StartIndex;
					return list;
				}
			}
			relativeIndex = -1;
			return null;
		}

		/// <summary>
		///     Item_s the group changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void item_GroupChanged( object sender, ObjectEventArgs<TGroup, TGroup> e )
		{
			TGroup oldValue = e.First;
			TGroup newValue = e.Second;
			var obj = sender as TItem;

			if ( obj != null )
			{
				// Remove the object from the hash table
				// based on the old group.
				if ( !Equals( oldValue, default( TGroup ) ) )
				{
					// Find the specific item and remove it
					TGroup group = GroupModifier( oldValue );
					if ( _dictionary.ContainsKey( group ) )
					{
						IMultiLinkedList<TItem> items = _dictionary[ group ];

						// Find the item's index within the list
						int index = items.IndexOf( obj );
						if ( index >= 0 )
						{
							// Get a reference to the object
							TItem item = items[ index ];

							// Remove the object
							items.RemoveAt( index );

							// Notify that this item was removed, with the overall
							// index of the item in the keyed list.
							OnItemRemoved( UnsubscribeFromKeyChanges( item ), items.StartIndex + index );
						}
					}
				}

				// If a new group exists, then re-add this item into the hash
				if ( !Equals( newValue, default( TGroup ) ) )
				{
					Add( obj );
				}
			}
		}

		#region IGroupedList<TGroup, TObject> Members

		/// <summary>
		///     Occurs when items are added.
		/// </summary>
		[field: NonSerialized]
		public event EventHandler<ObjectEventArgs<TItem, int>> ItemAdded;

		/// <summary>
		///     Occurs when items are removed.
		/// </summary>
		[field: NonSerialized]
		public event EventHandler<ObjectEventArgs<TItem, int>> ItemRemoved;

		/// <summary>
		///     Called when items are added.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="index">The index.</param>
		protected void OnItemAdded( TItem obj, int index )
		{
			if ( ItemAdded != null )
			{
				ItemAdded( this, new ObjectEventArgs<TItem, int>( obj, index ) );
			}
		}

		/// <summary>
		///     Called when items are removed.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="index">The index.</param>
		protected void OnItemRemoved( TItem obj, int index )
		{
			if ( ItemRemoved != null )
			{
				ItemRemoved( this, new ObjectEventArgs<TItem, int>( obj, index ) );
			}
		}

		/// <summary>
		///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">
		///     The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </param>
		public virtual void Add( TItem item )
		{
			if ( item != null )
			{
				// Get the "real" group for this item
				TGroup group = GroupModifier( item.Group );

				// Add a new list if necessary
				IMultiLinkedList<TItem> list = EnsureList( group, true );
				int index = list.Count;
				list.Add( SubscribeToKeyChanges( item ) );
				OnItemAdded( item, list.StartIndex + index );
			}
		}

		/// <summary>
		///     Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
		/// </summary>
		/// <param name="item">
		///     The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.
		/// </param>
		/// <returns>
		///     The index of <paramref name="item" /> if found in the list; otherwise, -1.
		/// </returns>
		public virtual int IndexOf( TItem item )
		{
			// Get the "real" group
			TGroup group = GroupModifier( item.Group );
			if ( _dictionary.ContainsKey( group ) )
			{
				// Get the list associated with this object's group
				IMultiLinkedList<TItem> list = _dictionary[ group ];

				// Find the object within the list.
				int index = list.IndexOf( item );

				// Return the index within the overall KeyedList
				if ( index >= 0 )
				{
					return list.StartIndex + index;
				}
			}
			return -1;
		}

		/// <summary>
		///     Clears the specified group.
		/// </summary>
		/// <param name="group">The group.</param>
		public virtual void Clear( TGroup group )
		{
			group = GroupModifier( group );

			if ( _dictionary.ContainsKey( group ) )
			{
				// Get the list associated with the group
				TItem[] list = _dictionary[ group ].ToArray( );

				// Save the starting index of the list
				int startIndex = _dictionary[ group ].StartIndex;

				// Clear the list (note that this also clears the list
				// in the _Lists object).
				_dictionary[ group ].Clear( );

				// Notify that each of these items were removed
				for ( int i = list.Length - 1; i >= 0; i-- )
				{
					OnItemRemoved( UnsubscribeFromKeyChanges( list[ i ] ), startIndex + i );
				}
			}
		}

		/// <summary>
		///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public virtual void Clear( )
		{
			// Get a list of items that are being cleared
			TItem[] items = _lists.SelectMany( i => i ).ToArray( );

			// Clear our lists out
			_dictionary.Clear( );
			_lists.Clear( );

			// Notify that each item was removed
			for ( int i = items.Length - 1; i >= 0; i-- )
			{
				OnItemRemoved( UnsubscribeFromKeyChanges( items[ i ] ), i );
			}
		}

		/// <summary>
		///     Determines whether the specified group contains key.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns>
		///     <c>true</c> if the specified group contains key; otherwise, <c>false</c>.
		/// </returns>
		public virtual bool ContainsKey( TGroup group )
		{
			group = GroupModifier( group );
			return _dictionary.ContainsKey( group );
		}

		/// <summary>
		///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <returns>
		///     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public virtual int Count
		{
			get
			{
				return _lists.Sum( list => list.Count );
			}
		}

		/// <summary>
		///     Counts the of.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		public virtual int CountOf( TGroup group )
		{
			group = GroupModifier( group );
			if ( _dictionary.ContainsKey( group ) )
			{
				return _dictionary[ group ].Count;
			}
			return 0;
		}

		/// <summary>
		///     Gets the Values of this instance.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<TItem> Values( )
		{
			return _dictionary.Values.SelectMany( i => i );
		}

		/// <summary>
		///     Alls the of.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		public virtual IEnumerable<TItem> AllOf( TGroup group )
		{
			group = GroupModifier( group );
			if ( _dictionary.ContainsKey( group ) )
			{
				return _dictionary[ group ];
			}
			return new TItem[0];
		}

		/// <summary>
		///     Removes the specified obj.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public virtual bool Remove( TItem obj )
		{
			TGroup group = GroupModifier( obj.Group );
			if ( _dictionary.ContainsKey( group ) )
			{
				IMultiLinkedList<TItem> items = _dictionary[ group ];
				int index = items.IndexOf( obj );

				if ( index >= 0 )
				{
					items.RemoveAt( index );
					OnItemRemoved( UnsubscribeFromKeyChanges( obj ), index );
					return true;
				}
			}
			return false;
		}

		/// <summary>
		///     Removes the specified group.
		/// </summary>
		/// <param name="group">The group.</param>
		/// <returns></returns>
		public virtual bool Remove( TGroup group )
		{
			group = GroupModifier( group );
			if ( _dictionary.ContainsKey( group ) )
			{
				IMultiLinkedList<TItem> list = _dictionary[ group ];

				for ( int i = list.Count - 1; i >= 0; i-- )
				{
					TItem obj = list[ i ];
					list.RemoveAt( i );
					OnItemRemoved( UnsubscribeFromKeyChanges( obj ), list.StartIndex + i );
				}
				return true;
			}
			return false;
		}

		/// <summary>
		///     Sorts the keys.
		/// </summary>
		/// <param name="comparer">The comparer.</param>
		public virtual void SortKeys( IComparer<TGroup> comparer = null )
		{
			_lists.Clear( );

			IMultiLinkedList<TItem> previous = null;
			foreach ( TGroup group in _dictionary.Keys.OrderBy( k => k, comparer ) )
			{
				IMultiLinkedList<TItem> list = _dictionary[ group ];
				if ( previous == null )
				{
					previous = list;
					previous.SetPrevious( null );
				}
				else
				{
					previous.SetNext( list );
					list.SetPrevious( previous );
					previous = list;
				}

				_lists.Add( list );
			}
		}

		#endregion

		#region ICollection<TObject> Members

		/// <summary>
		///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">
		///     The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </param>
		/// <returns>
		///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		public virtual bool Contains( TItem item )
		{
			TGroup group = GroupModifier( item.Group );
			if ( _dictionary.ContainsKey( group ) )
			{
				return _dictionary[ group ].Contains( item );
			}
			return false;
		}

		/// <summary>
		///     Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public virtual void CopyTo( TItem[] array, int arrayIndex )
		{
			_dictionary.SelectMany( kvp => kvp.Value ).ToArray( ).CopyTo( array, arrayIndex );
		}

		/// <summary>
		///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <returns>
		///     true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
		/// </returns>
		public virtual bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region IList<TObject> Members

		/// <summary>
		///     Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
		/// </summary>
		/// <param name="index">
		///     The zero-based index at which <paramref name="item" /> should be inserted.
		/// </param>
		/// <param name="item">
		///     The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.
		/// </param>
		public virtual void Insert( int index, TItem item )
		{
			int relativeIndex;
			IMultiLinkedList<TItem> list = ListForIndex( index, out relativeIndex );
			if ( list != null )
			{
				list.Insert( relativeIndex, item );
				OnItemAdded( item, index );
			}
		}

		/// <summary>
		///     Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		public virtual void RemoveAt( int index )
		{
			int relativeIndex;
			IMultiLinkedList<TItem> list = ListForIndex( index, out relativeIndex );
			if ( list != null )
			{
				TItem item = list[ relativeIndex ];
				list.RemoveAt( relativeIndex );
				OnItemRemoved( item, index );
			}
		}

		/// <summary>
		///     Gets or sets the <see cref="TItem" /> at the specified index.
		/// </summary>
		/// <value>
		///     The <see cref="TItem" />.
		/// </value>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public virtual TItem this[ int index ]
		{
			get
			{
				int relativeIndex;
				IMultiLinkedList<TItem> list = ListForIndex( index, out relativeIndex );
				if ( list != null )
				{
					return list[ relativeIndex ];
				}
				return default( TItem );
			}
			set
			{
				int relativeIndex;
				IMultiLinkedList<TItem> list = ListForIndex( index, out relativeIndex );
				if ( list != null )
				{
					// Remove the item at that index and replace it
					TItem item = list[ relativeIndex ];
					list.RemoveAt( relativeIndex );
					list.Insert( relativeIndex, value );
					OnItemRemoved( item, index );
					OnItemAdded( item, index );
				}
			}
		}

		#endregion

		#region IEnumerable<U> Members

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<TItem> GetEnumerator( )
		{
			return new GroupedListEnumerator<TItem>( _lists );
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator( )
		{
			return new GroupedListEnumerator<TItem>( _lists );
		}

		#endregion
	}
}