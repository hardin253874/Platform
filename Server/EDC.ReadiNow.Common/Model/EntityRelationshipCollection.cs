// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Relationship Instance collection.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public sealed class EntityRelationshipCollection<TEntity> : IEntityRelationshipCollection<TEntity>
		where TEntity : class, IEntity
	{
		/// <summary>
		///     Change tracker.
		/// </summary>
		private readonly IChangeTracker<IMutableIdKey> _tracker;

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityRelationshipCollection{TEntity}" /> class.
		/// </summary>
		public EntityRelationshipCollection( )
		{
			_tracker = new ChangeTracker<IMutableIdKey>( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityRelationshipCollection{TEntity}" /> class.
		/// </summary>
		/// <param name="collection">The collection.</param>
		[SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures" )]
		public EntityRelationshipCollection( IEnumerable<TEntity> collection )
			: this( )
		{
			if ( collection != null )
			{
				/////
				// Add each element to the change tracker.
				/////
				foreach ( var relationshipInstance in collection )
				{
					Add( relationshipInstance );
				}
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityRelationshipCollection&lt;TEntity&gt;" /> class.
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <param name="readOnly">
		///     If set to <c>true</c>, the collection cannot be modified.
		/// </param>
		internal EntityRelationshipCollection( IChangeTracker<IMutableIdKey> collection, bool readOnly )
		{
			_tracker = collection ?? new ChangeTracker<IMutableIdKey>( );

			SourceIsReadOnly = readOnly;
		}

		/// <summary>
		///     Gets the entities.
		/// </summary>
		public IEntityCollection<TEntity> Entities
		{
			get
			{
				var entityCollection = new EntityCollection<TEntity>( _tracker, SourceIsReadOnly );

				return entityCollection;
			}
		}

		/// <summary>
		///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <returns>
		///     true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
		/// </returns>
		private bool SourceIsReadOnly
		{
			get;
			set;
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
		///     Gets the tracker.
		/// </summary>
		IChangeTracker<IMutableIdKey> IChangeTrackerAccessor<IMutableIdKey>.Tracker
		{
			get
			{
				return _tracker;
			}
		}

		/// <summary>
		///     Adds the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <exception cref="RelationshipInstanceException"></exception>
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
        		_tracker.Add( ( ( IEntityInternal ) item ).MutableId );
			}
		}

		/// <summary>
		///     Adds the range.
		/// </summary>
		/// <param name="collection">The collection.</param>
		[SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures" )]
		public void AddRange( IEnumerable<EntityRelationship<TEntity>> collection )
		{
			if ( collection == null )
			{
				throw new ArgumentNullException( "collection" );
			}

			Precheck( );

			foreach ( var item in collection )
			{
				Add( item.Entity );
			}
		}

		/// <summary>
		///     Determines whether [contains] [the specified item].
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>
		///     <c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains( TEntity item )
		{
			if ( item == null )
			{
				return false;
			}

			return _tracker.Any( pair => pair.Key == item.Id );
		}

		public void CopyTo( TEntity[] array, int arrayIndex )
		{
			Entity.Get<TEntity>( _tracker.Select( pair => pair.Key ) ).ToArray( ).CopyTo( array, arrayIndex );
		}

		/// <summary>
		///     Removes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
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
		///     Removes the range.
		/// </summary>
		/// <param name="collection">The collection.</param>
		[SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures" )]
		public void RemoveRange( IEnumerable<EntityRelationship<TEntity>> collection )
		{
			if ( collection == null )
			{
				return;
			}

			Precheck( );

			foreach ( var item in collection )
			{
				Remove( item.Entity );
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
			return _tracker.Select( pair =>
			{
			    TEntity entity = null;

			    if (!SourceIsReadOnly)
			    {
                    // Get the entity as writeable
			        using (var ctx = new SourceEntityContext())
			        {
			            ctx.Writable = !SourceIsReadOnly;
			            entity = Entity.Get<TEntity>(pair.Key, SecurityOption.SkipDenied);
			        }
			    }

                if (SourceIsReadOnly || entity == null)
			    {
                    // The source is readonly or we don't have permission
                    // to get the entity as writeable.                    
                    entity = Entity.Get<TEntity>(pair.Key, SecurityOption.SkipDenied);
			    }

                return entity;			    
			});
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