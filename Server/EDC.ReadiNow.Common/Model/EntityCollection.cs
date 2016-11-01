// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Collection of type-specific entities.
	/// </summary>
	public sealed class EntityCollection<TEntity> : IEntityCollection<TEntity>
		where TEntity : class, IEntity
	{
		/// <summary>
		///     Change tracker.
		/// </summary>
		private readonly IChangeTracker<IMutableIdKey> _tracker;

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityCollection&lt;TEntity&gt;" /> class.
		/// </summary>
		public EntityCollection( )
		{
			_tracker = new ChangeTracker<IMutableIdKey>( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityCollection&lt;TEntity&gt;" /> class.
		/// </summary>
		/// <param name="collection">The collection.</param>
		public EntityCollection( IEnumerable<TEntity> collection )
			: this( )
		{
			if ( collection != null )
			{
				/////
				// Add each element to the change tracker.
				/////
				foreach ( TEntity entity in collection )
				{
					Add( entity );
				}
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityCollection&lt;TEntity&gt;" /> class.
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <param name="readOnly">
		///     if set to <c>true</c> [read only].
		/// </param>
		internal EntityCollection( IChangeTracker<IMutableIdKey> collection, bool readOnly )
		{
			_tracker = collection ?? new ChangeTracker<IMutableIdKey>( );

			SourceIsReadOnly = readOnly;
		}

		/// <summary>
		///     Gets or sets the <see cref="TEntity" /> with the specified index.
		/// </summary>
		public TEntity this[ int index ]
		{
			get
			{
				/////
				// Struct type always returns an instance.
				/////
				IMutableIdKey match = _tracker.ElementAt( index );

				return Entity.Get<TEntity>( match.Key );
			}
			set
			{
				Precheck( );

				/////
				// Struct type always returns an instance.
				/////
				IMutableIdKey match = _tracker.ElementAt( index );

				_tracker.Remove( match );

				if ( value != null )
				{
					_tracker.Add( ( ( IEntityInternal ) value ).MutableId );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [source is read only].
		/// </summary>
		/// <value>
		///     <c>true</c> if [source is read only]; otherwise, <c>false</c>.
		/// </value>
		private bool SourceIsReadOnly
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the tracker.
		/// </summary>
		IChangeTracker<IMutableIdKey> IChangeTrackerAccessor<IMutableIdKey>.Tracker
		{
			[DebuggerStepThrough]
			get
			{
				return _tracker;
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
				return _tracker.Count;
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
				return _tracker.IsReadOnly | SourceIsReadOnly;
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
		public void Add( TEntity item )
		{
			/////
			// Input validation.
			/////
			if ( item == null )
			{
				return;
			}

			Precheck( );

			if ( _tracker.All( p => p.Key != item.Id ) )
			{
                _tracker.Add(((IEntityInternal)item).MutableId);
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
			Precheck( );

			_tracker.Clear( );
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
		public bool Contains( TEntity item )
		{
			if ( item == null )
			{
				return false;
			}

			return _tracker.Any( pair => pair.Key == item.Id );
		}

		/// <summary>
		///     Copies the collection to the specified array.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo( TEntity[] array, int arrayIndex )
		{
			GetCurrentInstances( ).ToArray( ).CopyTo( array, arrayIndex );
		}

		/// <summary>
		///     Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<TEntity> GetEnumerator( )
		{
			return GetCurrentInstances( ).GetEnumerator( );
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
		public bool Remove( TEntity item )
		{
			if ( item == null )
			{
				return false;
			}

			Precheck( );

			IMutableIdKey[] matches = _tracker.Where( pair => pair.Key == item.Id ).ToArray( );

			return matches.Aggregate( false, ( current, match ) => current & _tracker.Remove( match ) );
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
		///     Adds the range.
		/// </summary>
		/// <param name="collection">The collection.</param>
		public void AddRange( IEnumerable<TEntity> collection )
		{
			if ( collection == null )
			{
				throw new ArgumentNullException( "collection" );
			}

			Precheck( );

			foreach ( TEntity item in collection )
			{
				Add( item );
			}
		}

		/// <summary>
		///     Removes the range.
		/// </summary>
		/// <param name="collection">The collection.</param>
		public void RemoveRange( IEnumerable<TEntity> collection )
		{
			if ( collection == null )
			{
				return;
			}

			Precheck( );

			foreach ( TEntity item in collection )
			{
				Remove( item );
			}
		}

		/// <summary>
		///     Gets the current instances.
		/// </summary>
		/// <returns>
		///     An enumeration of the current entities.
		/// </returns>
		private IEnumerable<TEntity> GetCurrentInstances( )
		{
			using ( var ctx = new SourceEntityContext( ) )
			{
				ctx.Writable = !SourceIsReadOnly;

				return Entity.Get<TEntity>( _tracker.Select( pair => pair.Key ) );
			}
		}

		/// <summary>
		///     Performs a pre-check on the collection state.
		/// </summary>
		private void Precheck( )
		{
			if ( IsReadOnly )
			{
				throw new ReadOnlyException( "This collection belongs to a read-only entity." );
			}
		}
	}
}