// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Represents a relationship instance whose source is inferred from the caller.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	[Immutable]
	public class EntityRelationship<TEntity>
		where TEntity : class, IEntity
	{
		/// <summary>
		///     Entity member.
		/// </summary>
		private readonly TEntity _entity;

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityRelationship&lt;TEntity&gt;" /> class.
		/// </summary>
		/// <param name="entity">The entity.</param>
		public EntityRelationship( TEntity entity )
		{
			_entity = entity;
		}

		/// <summary>
		///     Gets the entity.
		/// </summary>
		public TEntity Entity
		{
			get
			{
				return _entity;
			}
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="System.Object" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var val = obj as EntityRelationship<TEntity>;

			if ( ( Object ) val == null )
			{
				return false;
			}

			return this == val;
		}

		/// <summary>
		///     Determines whether the specified <see cref="EntityRelationship&lt;TEntity&gt;" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="EntityRelationship&lt;TEntity&gt;" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="EntityRelationship&lt;TEntity&gt;" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals( EntityRelationship<TEntity> obj )
		{
			if ( ( object ) obj == null )
			{
				return false;
			}

			return this == obj;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			if ( Entity == null )
			{
				return 0;
			}

			return Entity.GetHashCode( );
		}

		/// <summary>
		///     Toes the entity relationship collection.
		/// </summary>
		/// <returns></returns>
		public IEntityRelationshipCollection<TEntity> ToEntityRelationshipCollection( )
		{
			return new EntityRelationshipCollection<TEntity>( new[ ]
			{
				Entity
			} );
		}

		/// <summary>
		///     Checks the entity equality.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     True if the entities are the same; false otherwise.
		/// </returns>
		private static bool CheckEntity( EntityRelationship<TEntity> a, EntityRelationship<TEntity> b )
		{
			if ( a.Entity == null )
			{
				if ( b.Entity != null )
				{
					return false;
				}
			}
			else
			{
				if ( b.Entity == null )
				{
					return false;
				}

				if ( !a.Entity.Equals( b.Entity ) )
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		///     Implements the operator !=.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator !=( EntityRelationship<TEntity> a, EntityRelationship<TEntity> b )
		{
			return !( a == b );
		}

		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="a">A.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( EntityRelationship<TEntity> a, EntityRelationship<TEntity> b )
		{
			if ( ReferenceEquals( a, b ) )
			{
				return true;
			}

			if ( ( ( object ) a == null ) || ( ( object ) b == null ) )
			{
				return false;
			}

			if ( !CheckEntity( a, b ) )
			{
				return false;
			}

			return true;
		}
	}
}