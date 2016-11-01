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
	public class GroupedValueListProxy<TGroup, TInterface, TItem, TOriginalValue, TNewValue> : IGroupedValueListProxy<TInterface, TNewValue>
		where TInterface : class, IGroupedObject<TGroup>, IValueObject<TOriginalValue>
		where TItem : new( )
	{
		/// <summary>
		///     Real Object.
		/// </summary>
		private readonly IGroupedValueList<TGroup, TInterface, TOriginalValue> _realObject;

		/// <summary>
		///     Group.
		/// </summary>
		private readonly TGroup _group;

		/// <summary>
		///     Container.
		/// </summary>
		private TInterface _container;

		/// <summary>
		///     Initializes a new instance of the
		///     <see
		///         cref="GroupedValueListProxy{TGroup, TInterface, TItem, TOriginalValue, TNewValue}" />
		///     class.
		/// </summary>
		/// <param name="realObject">The real object.</param>
		/// <param name="group">The group.</param>
		public GroupedValueListProxy( IGroupedValueList<TGroup, TInterface, TOriginalValue> realObject, TGroup group )
		{
			_realObject = realObject;
			_group = group;
		}

		/// <summary>
		///     Ensures the container.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="System.Exception">Could not create a container for the value - the container is not of type  + typeof ( TInterface ).Name</exception>
		private TInterface EnsureContainer( )
		{
			if ( _container == null )
			{
				// Find an item that matches our group
				_container = Items.FirstOrDefault( );

				// If no item is found, create a new object and add it to the list
				if ( Equals( _container, default( TInterface ) ) )
				{
					var container = new TItem( );
					if ( !( container is TInterface ) )
					{
						throw new Exception( "Could not create a container for the value - the container is not of type " + typeof ( TInterface ).Name );
					}
					_container = ( TInterface ) ( object ) container;
					_container.Group = _group;
					_realObject.Add( _container );
				}
			}
			return _container;
		}

		/// <summary>
		///     Iterates the values.
		/// </summary>
		/// <param name="action">The action.</param>
		private void IterateValues( Func<IValueObject<TOriginalValue>, int, int, bool> action )
		{
			int i = 0;
			foreach ( TInterface obj in _realObject )
			{
				// Get the number of items of the target value i this object
				int count = obj.Values != null ? obj.Values.OfType<TNewValue>( ).Count( ) : 0;

				// Perform some action on this item
				if ( !action( obj, i, count ) )
				{
					return;
				}

				i += count;
			}
		}

		/// <summary>
		///     Gets the enumerator internal.
		/// </summary>
		/// <returns></returns>
		private IEnumerator<TNewValue> GetEnumeratorInternal( )
		{
			return Items
				.Where( o => o.ValueCount > 0 )
				.SelectMany( o => o.Values.OfType<TNewValue>( ) )
				.GetEnumerator( );
		}

		#region IList<TNewValue> Members

		/// <summary>
		///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">
		///     The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </param>
		public virtual void Add( TNewValue item )
		{
			// Add the value to the object
			if ( item is TOriginalValue )
			{
				var value = ( TOriginalValue ) ( object ) item;
				EnsureContainer( ).AddValue( value );
			}
		}

		/// <summary>
		///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public virtual void Clear( )
		{
			IEnumerable<TInterface> items = Items.Where( o => o.Values != null );

			foreach ( TInterface original in items )
			{
				// Clear all values from each matching object
				original.SetValue( default( TOriginalValue ) );
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
		public virtual bool Contains( TNewValue item )
		{
			if ( item is TOriginalValue )
			{
				return Items.Any( o => o.ContainsValue( ( TOriginalValue ) ( object ) item ) );
			}
			return false;
		}

		/// <summary>
		///     Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public virtual void CopyTo( TNewValue[] array, int arrayIndex )
		{
			Items
				.Where( o => o.Values != null )
				.SelectMany( o => o.Values )
				.ToArray( )
				.CopyTo( array, arrayIndex );
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
				return Items.Sum( o => o.ValueCount );
			}
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
		public virtual bool Remove( TNewValue item )
		{
			if ( item is TOriginalValue )
			{
				var value = ( TOriginalValue ) ( object ) item;

				TInterface container = Items.FirstOrDefault( o => o.ContainsValue( value ) );

				if ( container != null )
				{
					container.RemoveValue( value );
					return true;
				}
			}
			return false;
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public virtual IEnumerator<TNewValue> GetEnumerator( )
		{
			return GetEnumeratorInternal( );
		}

		/// <summary>
		///     Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator( )
		{
			return GetEnumeratorInternal( );
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
		public virtual int IndexOf( TNewValue item )
		{
			int index = -1;

			if ( item is TOriginalValue )
			{
				var value = ( TOriginalValue ) ( object ) item;
				IterateValues( ( o, i, count ) =>
					{
						if ( o.Values != null && o.Values.Contains( value ) )
						{
							List<TOriginalValue> list = o.Values.ToList( );
							index = i + list.IndexOf( value );
							return false;
						}
						return true;
					} );
			}

			return index;
		}

		/// <summary>
		///     Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
		/// </summary>
		/// <param name="index">
		///     The zero-based index at which <paramref name="item" /> should be inserted.
		/// </param>
		/// <param name="item">
		///     The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.
		/// </param>
		public virtual void Insert( int index, TNewValue item )
		{
			IterateValues( ( o, i, count ) =>
				{
					var value = ( TOriginalValue ) ( object ) item;

					// Determine if this index is found within this object
					if ( index >= i && index < count )
					{
						// Convert the items to a list
						List<TOriginalValue> items = o.Values.ToList( );
						// Insert the item at the relative index within the list
						items.Insert( index - i, value );
						// Set the new list
						o.SetValue( items );
						return false;
					}
					return true;
				} );
		}

		/// <summary>
		///     Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		public virtual void RemoveAt( int index )
		{
			IterateValues( ( o, i, count ) =>
				{
					// Determine if this index is found within this object
					if ( index >= i && index < count )
					{
						// Convert the items to a list
						List<TOriginalValue> items = o.Values.ToList( );
						// Remove the item at the relative index within the list
						items.RemoveAt( index - i );
						// Set the new list
						o.SetValue( items );
						return false;
					}
					return true;
				} );
		}

		/// <summary>
		///     Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public virtual TNewValue this[ int index ]
		{
			get
			{
				if ( index >= 0 && index < Count )
				{
					return this
						.Skip( index )
						.FirstOrDefault( );
				}
				return default( TNewValue );
			}
			set
			{
				if ( index >= 0 && index < Count )
				{
					if ( !Equals( value, default( TNewValue ) ) )
					{
						Insert( index, value );
						index++;
					}
					RemoveAt( index );
				}
			}
		}

		#endregion

		#region IGroupedValueListProxy Members

		/// <summary>
		///     Gets the items.
		/// </summary>
		/// <value>
		///     The items.
		/// </value>
		public virtual IEnumerable<TInterface> Items
		{
			get
			{
				if ( ! EqualityComparer<TGroup>.Default.Equals( _group, default( TGroup ) ) )
				{
					return _realObject.AllOf( _group );
				}

				return _realObject;
			}
		}

		#endregion
	}
}