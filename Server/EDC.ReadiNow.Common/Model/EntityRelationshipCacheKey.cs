// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using ProtoBuf;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Used to key into the EntityRelationshipCache data structure.
	/// </summary>
	[Immutable]
	[Serializable]
	[TypeConverter( typeof ( EntityRelationshipCacheKeyConverter ) )]
	[DebuggerStepThrough]
	[ProtoContract]
	[ProtoInclude(10, typeof( EntityRelationshipCacheTypeKey ) )]
	public class EntityRelationshipCacheKey
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="EntityRelationshipCacheKey" /> class from being created.
		/// </summary>
		protected EntityRelationshipCacheKey( )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityRelationshipCacheKey" /> class.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="direction">The direction.</param>
		public EntityRelationshipCacheKey( long entityId, Direction direction )
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
		public long EntityId
		{
			get;
			protected set;
		}

		/// <summary>
		///     Creates the specified entity identifier.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="direction">The direction.</param>
		/// <returns></returns>
		public static EntityRelationshipCacheKey Create( long entityId, Direction direction )
		{
			return new EntityRelationshipCacheKey( entityId, direction );
		}

		/// <summary>
		///     Determines whether the specified <see cref="EntityRelationshipCacheKey" /> is equal to this
		///     instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="EntityRelationshipCacheKey" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="EntityRelationshipCacheKey" /> is equal to this
		///     instance; otherwise,
		///     <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var key = obj as EntityRelationshipCacheKey;

			if ( key == null )
			{
				return false;
			}

			return EntityId == key.EntityId && Direction == key.Direction;
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
		public bool Equals( EntityRelationshipCacheKey obj )
		{
			if ( obj == null )
			{
				return false;
			}

			return EntityId == obj.EntityId && Direction == obj.Direction;
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

				hash = hash * 92821 + Direction.GetHashCode( );

				return hash;
			}
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return EntityId.ToString( CultureInfo.InvariantCulture ) + "_" + Direction;
		}

		/// <summary>
		///     Tries the parse.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="cacheKey">The cache key.</param>
		/// <returns></returns>
		public static bool TryParse( string key, out EntityRelationshipCacheKey cacheKey )
		{
			cacheKey = null;

			if ( !string.IsNullOrEmpty( key ) )
			{
				string[ ] args = key.Split( '_' );

				if ( args.Length == 2 && !string.IsNullOrEmpty( args[ 0 ] ) && !string.IsNullOrEmpty( args[ 1 ] ) )
				{
					long id;

					if ( long.TryParse( args[ 0 ], out id ) )
					{
						Direction direction;

						if ( Enum.TryParse( args[ 1 ], out direction ) )
						{
							cacheKey = new EntityRelationshipCacheKey( id, direction );
							return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		///     Implements the operator !=.
		/// </summary>
		/// <param name="a">First object to compare.</param>
		/// <param name="b">Second object to compare.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator !=( EntityRelationshipCacheKey a, EntityRelationshipCacheKey b )
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
		public static bool operator ==( EntityRelationshipCacheKey a, EntityRelationshipCacheKey b )
		{
			if ( ReferenceEquals( a, b ) )
			{
				return true;
			}

			if ( ( ( object ) a == null ) || ( ( object ) b == null ) )
			{
				return false;
			}

			return a.EntityId == b.EntityId && a.Direction == b.Direction;
		}
	}
}