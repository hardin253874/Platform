// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Represents an entity modification token.
	/// </summary>
	[Immutable]
	[Serializable]
	public class EntityModificationToken : IEntityModificationToken
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EntityModificationToken" /> class.
		/// </summary>
		public EntityModificationToken( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityModificationToken" /> class.
		/// </summary>
		/// <param name="entityId">The entity id.</param>
		/// <param name="modificationId">The modification id.</param>
		public EntityModificationToken( long entityId, Guid modificationId )
			: this( )
		{
			EntityId = entityId;
			ModificationId = modificationId;
		}

		/// <summary>
		///     Gets or sets the entity id.
		/// </summary>
		/// <value>
		///     The entity id.
		/// </value>
		public long EntityId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the modification id.
		/// </summary>
		/// <value>
		///     The modification id.
		/// </value>
		public Guid ModificationId
		{
			get;
			private set;
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
			if ( obj == null )
			{
				return false;
			}

			var token = obj as EntityModificationToken;

			if ( token == null )
			{
				return false;
			}

			return EntityId == token.EntityId && ModificationId == token.ModificationId;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			unchecked
			{
				int hash = 17;

				hash = hash * 92821 + EntityId.GetHashCode( );

				hash = hash * 92821 + ModificationId.GetHashCode( );

				return hash;
			}
		}

		/// <summary>
		///     Implements the operator !=.
		/// </summary>
		/// <param name="a">First object to compare.</param>
		/// <param name="b">Second object to compare.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator !=( EntityModificationToken a, EntityModificationToken b )
		{
			return !( a == b );
		}

		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="a">First object to compare.</param>
		/// <param name="b">Second object to compare.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( EntityModificationToken a, EntityModificationToken b )
		{
			if ( ReferenceEquals( a, b ) )
			{
				return true;
			}

			if ( ( ( object ) a == null ) || ( ( object ) b == null ) )
			{
				return false;
			}

			return a.EntityId == b.EntityId && a.ModificationId == b.ModificationId;
		}
	}
}