// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using ProtoBuf;

namespace EDC.ReadiNow.Model
{
	[ProtoContract]
	public class SerializableEntityRelationshipCacheTypeKey : SerializableEntityRelationshipCacheKey, IEquatable<SerializableEntityRelationshipCacheTypeKey>
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="SerializableEntityRelationshipCacheTypeKey" /> class from being
		///     created.
		/// </summary>
		protected SerializableEntityRelationshipCacheTypeKey( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SerializableEntityRelationshipCacheTypeKey" /> class.
		/// </summary>
		/// <param name="entityId">The primary key.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="typeId">The type identifier.</param>
		public SerializableEntityRelationshipCacheTypeKey( SerializableEntityId entityId, Direction direction, long typeId )
		{
			EntityId = entityId;
			Direction = direction;
			TypeId = typeId;
		}

		/// <summary>
		///     Gets or sets the type identifier.
		/// </summary>
		/// <value>
		///     The type identifier.
		/// </value>
		[ProtoMember( 1 )]
		public long TypeId
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
		public bool Equals( SerializableEntityRelationshipCacheTypeKey other )
		{
			if ( other == null )
			{
				return false;
			}

			return EntityId == other.EntityId && Direction == other.Direction && TypeId == other.TypeId;
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
			var serializableEntityRelationshipCacheTypeKey = obj as SerializableEntityRelationshipCacheTypeKey;

			if ( serializableEntityRelationshipCacheTypeKey == null )
			{
				return false;
			}

			return Equals( serializableEntityRelationshipCacheTypeKey );
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

				hash = hash * 92821 + base.GetHashCode( );

				hash = hash * 92821 + TypeId.GetHashCode( );
				
				return hash;
			}
		}
	}
}