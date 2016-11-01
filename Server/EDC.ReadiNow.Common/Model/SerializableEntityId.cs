// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Serializable EntityId.
	/// </summary>
	[ProtoContract]
	[Serializable]
	public class SerializableEntityId : IEquatable<SerializableEntityId>
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="SerializableEntityId" /> class from being created.
		/// </summary>
		private SerializableEntityId( )
		{
			TypeIds = new List<long>( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SerializableEntityId" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="typeIds">The type ids.</param>
		public SerializableEntityId( long id, IEnumerable<long> typeIds )
			: this( )
		{
			Id = id;

			if ( typeIds != null )
			{
				TypeIds.AddRange( typeIds );
			}
		}

		/// <summary>
		///     Gets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		[ProtoMember( 1 )]
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the type ids.
		/// </summary>
		/// <value>
		///     The type ids.
		/// </value>
		[ProtoMember( 2 )]
		public List<long> TypeIds
		{
			get;
			private set;
		}

		/// <summary>
		///     Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals( SerializableEntityId other )
		{
			if ( other == null )
			{
				return false;
			}

			return Id == other.Id && TypeIds.SequenceEqual( other.TypeIds );
		}

		/// <summary>
		///     Creates the specified identifier.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="typeIds">The type ids.</param>
		/// <returns></returns>
		public static SerializableEntityId Create( long id, IEnumerable<long> typeIds )
		{
			return new SerializableEntityId( id, typeIds );
		}

		/// <summary>
		///     Creates the specified identifier.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <returns></returns>
		public static SerializableEntityId Create( long id, long typeId )
		{
			return new SerializableEntityId( id, typeId.ToEnumerable( ) );
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
			var serializableEntityId = obj as SerializableEntityId;

			if ( serializableEntityId == null )
			{
				return false;
			}

			return Equals( serializableEntityId );
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

				hash = hash * 92821 + Id.GetHashCode( );

				if ( TypeIds != null )
				{
					hash = hash * 92821 + TypeIds.GetHashCode( );
				}

				return hash;
			}
		}

		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="a">a.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( SerializableEntityId a, SerializableEntityId b )
		{
			/////
			// If both are null, or both are same instance, return true.
			/////
			if ( ReferenceEquals( a, b ) )
			{
				return true;
			}

			/////
			// If one is null, but not both, return false.
			/////
			if ( ( ( object ) a == null ) || ( ( object ) b == null ) )
			{
				return false;
			}

			return Equals( a, b );
		}

		/// <summary>
		///     Implements the operator !=.
		/// </summary>
		/// <param name="a">a.</param>
		/// <param name="b">The b.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator !=( SerializableEntityId a, SerializableEntityId b )
		{
			return !( a == b );
		}
	}
}