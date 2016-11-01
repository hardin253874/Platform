// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using ProtoBuf;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Entity Relationship Cache Message
	/// </summary>
	[ProtoContract]
	public class EntityRelationshipCacheMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EntityRelationshipCacheMessage" /> class.
		/// </summary>
		public EntityRelationshipCacheMessage( )
		{
			RemoveKeys = new HashSet<SerializableEntityRelationshipCacheKey>( );
			RemoveTypeKeys = new HashSet<SerializableEntityRelationshipCacheTypeKey>( );
		}

		/// <summary>
		/// Gets or sets a value indicating whether the relationship cache should be cleared in the forward direction.
		/// </summary>
		/// <value>
		///   <c>true</c> if [clear forward]; otherwise, <c>false</c>.
		/// </value>
		[ProtoMember( 1 )]
		public bool ClearForward
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the relationship cache should be cleared in the reverse direction.
		/// </summary>
		/// <value>
		///   <c>true</c> if [clear forward]; otherwise, <c>false</c>.
		/// </value>
		[ProtoMember( 2 )]
		public bool ClearReverse
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the remove keys.
		/// </summary>
		/// <value>
		///     The remove keys.
		/// </value>
		[ProtoMember( 3 )]
		public ISet<SerializableEntityRelationshipCacheKey> RemoveKeys
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the remove type keys.
		/// </summary>
		/// <value>
		///     The remove type keys.
		/// </value>
		[ProtoMember( 4 )]
		public ISet<SerializableEntityRelationshipCacheTypeKey> RemoveTypeKeys
		{
			get;
			set;
		}

        /// <summary>
        ///     Gets or sets the remove type keys.
        /// </summary>
        /// <value>
        ///     The remove type keys.
        /// </value>
        [ProtoMember( 5 )]
        public bool SuppressFullInvalidation
        {
            get;
            set;
        }
	}
}