// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using ProtoBuf;

namespace EDC.ReadiNow.Model
{
	[ProtoContract]
	[ProtoInclude( 10, typeof ( SerializableEntityRelationshipCacheTypeKey ) )]
	public class SerializableEntityRelationshipCacheKey : IEquatable<SerializableEntityRelationshipCacheKey>
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="SerializableEntityRelationshipCacheKey" /> class from being created.
		/// </summary>
		protected SerializableEntityRelationshipCacheKey( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SerializableEntityRelationshipCacheKey" /> class.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="direction">The direction.</param>
		public SerializableEntityRelationshipCacheKey( SerializableEntityId entityId, Direction direction )
			: this( )
		{
			EntityId = entityId;
			Direction = direction;
		}

		/// <summary>
		///     Gets the secondary key.
		/// </summary>
		[ProtoMember( 2 )]
		public Direction Direction
		{
			get;
			protected set;
		}

		/// <summary>
		///     Gets the primary key.
		/// </summary>
		[ProtoMember( 1 )]
		public SerializableEntityId EntityId
		{
			get;
			protected set;
		}

		/// <summary>
		///     Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals( SerializableEntityRelationshipCacheKey other )
		{
			if ( other == null )
			{
				return false;
			}

			return EntityId == other.EntityId && Direction == other.Direction;
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var serializableEntityRelationshipCacheKey = obj as SerializableEntityRelationshipCacheKey;

			if ( obj == null )
			{
				return false;
			}

			return Equals( serializableEntityRelationshipCacheKey );
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

				if ( EntityId != null )
				{
					hash = hash * 92821 + EntityId.GetHashCode( );
				}

				hash = hash * 92821 + Direction.GetHashCode( );

				return hash;
			}
		}
	}
}