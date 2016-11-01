// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using ProtoBuf;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Entity Relationship Cache Type Key.
	/// </summary>
	[ProtoContract]
	public class EntityRelationshipCacheTypeKey : EntityRelationshipCacheKey
	{
		/// <summary>
		///     Prevents a default instance of the <see cref="EntityRelationshipCacheKey" /> class from being created.
		/// </summary>
		protected EntityRelationshipCacheTypeKey( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityRelationshipCacheKey" /> class.
		/// </summary>
		/// <param name="entityId">The primary key.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="typeId">The type identifier.</param>
		public EntityRelationshipCacheTypeKey( long entityId, Direction direction, long typeId )
			: this( )
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
		///     Creates the specified entity identifier.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="direction">The direction.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <returns></returns>
		public static EntityRelationshipCacheTypeKey Create( long entityId, Direction direction, long typeId )
		{
			return new EntityRelationshipCacheTypeKey( entityId, direction, typeId );
		}

		/// <summary>
		///     Determines whether the specified <see cref="EntityRelationshipCacheTypeKey" /> is equal to this
		///     instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="EntityRelationshipCacheTypeKey" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="EntityRelationshipCacheTypeKey" /> is equal to this
		///     instance; otherwise,
		///     <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var key = obj as EntityRelationshipCacheTypeKey;

			if ( key == null )
			{
				return false;
			}

			return base.Equals( key ) && TypeId == key.TypeId;
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
		public bool Equals( EntityRelationshipCacheTypeKey obj )
		{
			if ( obj == null )
			{
				return false;
			}

			return base.Equals( obj ) && TypeId == obj.TypeId;
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

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return base.ToString( ) + "_" + TypeId;
		}

		/// <summary>
		///     Tries the parse.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="cacheKey">The cache key.</param>
		/// <returns></returns>
		public static bool TryParse( string key, out EntityRelationshipCacheTypeKey cacheKey )
		{
			cacheKey = null;

			if ( !string.IsNullOrEmpty( key ) )
			{
				string[ ] args = key.Split( '_' );

				if ( args.Length == 3 && !string.IsNullOrEmpty( args[ 0 ] ) && !string.IsNullOrEmpty( args[ 1 ] ) && !string.IsNullOrEmpty( args[ 2 ] ) )
				{
					long id;

					if ( long.TryParse( args[ 0 ], out id ) )
					{
						Direction direction;

						if ( Enum.TryParse( args[ 1 ], out direction ) )
						{
							long typeId;

							if ( long.TryParse( args[ 2 ], out typeId ) )
							{
								cacheKey = new EntityRelationshipCacheTypeKey( id, direction, typeId );
								return true;
							}
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
		public static bool operator !=( EntityRelationshipCacheTypeKey a, EntityRelationshipCacheTypeKey b )
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
		public static bool operator ==( EntityRelationshipCacheTypeKey a, EntityRelationshipCacheTypeKey b )
		{
			if ( ReferenceEquals( a, b ) )
			{
				return true;
			}

			if ( ( ( object ) a == null ) || ( ( object ) b == null ) )
			{
				return false;
			}

			return a.EntityId == b.EntityId && a.Direction == b.Direction && a.TypeId == b.TypeId;
		}
	}
}