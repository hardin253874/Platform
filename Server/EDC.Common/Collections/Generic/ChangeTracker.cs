// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EDC.Collections.Generic
{
	/// <summary>
	///     Generic set change tracker.
	/// </summary>
	/// <typeparam name="T">
	///     The type of items to track.
	/// </typeparam>
	/// <remarks>
	///     This class is not thread safe.
	/// </remarks>
	public class ChangeTracker<T> : IChangeTracker<T>
	{
		/////
		// ReSharper disable CompareNonConstrainedGenericWithNull
		/////

		/// <summary>
		///     Initial collection.
		/// </summary>
		private readonly ISet<T> _collection;

		/// <summary>
		///     Thread synchronization.
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		///     Added collection.
		/// </summary>
		private ISet<T> _added;

		/// <summary>
		///     Removed collection.
		/// </summary>
		private ISet<T> _removed;

		/// <summary>
		///     Creates a new instance of the ChangeTracker class.
		/// </summary>
		public ChangeTracker( )
		{
			_collection = new HashSet<T>( );
		}

		/// <summary>
		///     Creates a new instance of the ChangeTracker class.
		/// </summary>
		/// <param name="initialCollection">Initial collection to be contained within the change tracker.</param>
		public ChangeTracker( IEnumerable<T> initialCollection )
		{
			_collection = new HashSet<T>( initialCollection );
		}

		/// <summary>
		///     Gets a value indicating whether this <see cref="ChangeTracker&lt;T&gt;" /> is flushed.
		/// </summary>
		/// <value>
		///     <c>True</c> to indicate that the tracker has been flushed prior to tracking new changes; otherwise, <c>False</c>.
		/// </value>
		public bool Flushed
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the list of a added items.
		/// </summary>
		public IEnumerable<T> Added
		{
			get
			{
				lock ( _syncRoot )
				{
					if ( _added == null )
						return new T[0];

					return _added.ToArray( );
				}
			}
		}

		/// <summary>
		///     Gets the list of removed items.
		/// </summary>
		public IEnumerable<T> Removed
		{
			get
			{
				lock ( _syncRoot )
				{
					if ( _removed == null )
						return new T[0];

					return _removed.ToArray( );
				}
			}
		}

		/// <summary>
		///     Gets the list of base items.
		/// </summary>
		public IEnumerable<T> Collection
		{
			get
			{
				lock ( _syncRoot )
				{
					return _collection.ToArray( );
				}
			}
		}

		/// <summary>
		///     Accepts the changes made to the tracker.
		/// </summary>
		public void AcceptChanges( )
		{
			lock ( _syncRoot )
			{
				/////
				// Remove removed elements.
				/////
				if ( _removed != null )
				{
					_collection.ExceptWith( _removed );
				}
				_removed = null;

				/////
				// Add added elements.
				/////
				if ( _added != null )
				{
					_collection.UnionWith( _added );
				}
				_added = null;

				/////
				// Don't flush now.
				/////
				Flushed = false;
			}
		}

		/// <summary>
		///     Gets whether the tracker is currently dirty or not.
		/// </summary>
		/// <returns>
		///     true if the object’s content has changed since the last call to
		///     <see
		///         cref="M:System.ComponentModel.IChangeTracking.AcceptChanges" />
		///     ; otherwise, false.
		/// </returns>
		public bool IsChanged
		{
			get
			{
				lock ( _syncRoot )
				{
					// Count != 0 is faster than Any()
					return ( _added != null && _added.Count != 0 )
					       || ( _removed != null && _removed.Count != 0 );
				}
			}
		}

		/// <summary>
		///     Adds a new item into the collection.
		/// </summary>
		/// <param name="item">Item to be added to the collection.</param>
		/// <exception cref="T:System.NotSupportedException">
		///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </exception>
		public void Add( T item )
		{
			if ( item == null )
			{
				return;
			}

			lock ( _syncRoot )
			{
				if ( _removed == null || !_removed.Remove( item ) )
				{
					if ( _added == null )
					{
						_added = new HashSet<T>( );
					}

					_added.Add( item );
				}
			}
		}

		/// <summary>
		///     Clears the collection.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </exception>
		public void Clear( )
		{
			lock ( _syncRoot )
			{
				_added = null;
				_removed = new HashSet<T>( _collection );
			}
		}

		/// <summary>
		///     Determines whether the collection contains the specified item.
		/// </summary>
		/// <param name="item">The item to search for.</param>
		/// <returns>
		///     True if the item is contained within the collection, False otherwise.
		/// </returns>
		public bool Contains( T item )
		{
			if ( item == null )
			{
				return false;
			}

			lock ( _syncRoot )
			{
				if ( _added != null && _added.Contains( item ) )
				{
					return true;
				}

				bool removedContains = _removed != null && _removed.Contains( item );

				return _collection.Contains( item ) && !removedContains;
			}
		}

		/// <summary>
		///     Copies the contents of the collection to the specified array at the specified offset.
		/// </summary>
		/// <param name="array">Array used to receive the items in the collection.</param>
		/// <param name="arrayIndex">Index at which to start copying.</param>
		public void CopyTo( T[ ] array, int arrayIndex )
		{
			if ( array == null )
			{
				throw new ArgumentNullException( "array" );
			}

			lock ( _syncRoot )
			{
				ISet<T> output = new HashSet<T>( _collection );
				if ( _removed != null )
				{
					output.ExceptWith( _removed );
				}
				if ( _added != null )
				{
					output.UnionWith( _added );
				}

				output.CopyTo( array, arrayIndex );
			}
		}

		/// <summary>
		///     Gets the number of items contained within the collection.
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
					int count = _collection.Count;

					if ( _removed != null )
					{
						count -= _removed.Count;
					}

					if ( _added != null )
					{
						count += _added.Count;
					}

					return count;
				}
			}
		}

		/// <summary>
		///     Gets whether the collection is read-only.
		/// </summary>
		/// <returns>
		///     true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
		/// </returns>
		public bool IsReadOnly
		{
			get
			{
				lock ( _syncRoot )
				{
					return _collection.IsReadOnly;
				}
			}
		}

		/// <summary>
		///     Removed the specified element from the collection.
		/// </summary>
		/// <param name="item">Item to be removed from the collection.</param>
		/// <returns>
		///     True if the item was removed, False otherwise.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		///     The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </exception>
		public bool Remove( T item )
		{
			if ( item == null )
			{
				return false;
			}

			lock ( _syncRoot )
			{
				if ( _added != null && _added.Remove( item ) )
				{
					return true;
				}

				if ( _collection.Contains( item ) )
				{
					if ( _removed == null )
					{
						_removed = new HashSet<T>( );
					}

					return _removed.Add( item );
				}
			}

			return false;
		}

		/// <summary>
		///     Gets an enumerator for traversing over this collection.
		/// </summary>
		/// <returns>
		///     An IEnumerator that traverses this collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator( )
		{
			lock ( _syncRoot )
			{
				IEnumerable<T> result = _collection;

				if ( _removed != null )
				{
					result = result.Except( _removed );
				}

				if ( _added != null )
				{
					result = result.Union( _added );
				}

				return ( ( IEnumerable<T> ) result.ToArray( ) ).GetEnumerator( );
			}
		}

		/// <summary>
		///     Gets an enumerator for traversing over this collection.
		/// </summary>
		/// <returns>
		///     An IEnumerator that traverses this collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator( )
		{
			lock ( _syncRoot )
			{
				return this.ToArray( ).GetEnumerator( );
			}
		}

		/// <summary>
		///     Adds a range of items into the collection.
		/// </summary>
		/// <param name="collection">Collection of items being added to the underlying collection.</param>
		public void AddRange( IEnumerable<T> collection )
		{
			if ( collection == null )
			{
				throw new ArgumentNullException( "collection" );
			}

			lock ( _syncRoot )
			{
				foreach ( T item in collection )
				{
					Add( item );
				}
			}
		}

		/// <summary>
		///     Removes the collection of items from the collection.
		/// </summary>
		/// <param name="collection">Collection of items to be removed from the underlying collection.</param>
		public void RemoveRange( IEnumerable<T> collection )
		{
			if ( collection == null )
			{
				throw new ArgumentNullException( "collection" );
			}

			lock ( _syncRoot )
			{
				foreach ( T item in collection )
				{
					Remove( item );
				}
			}
		}

		/////
		// ReSharper restore CompareNonConstrainedGenericWithNull
		/////
	}
}