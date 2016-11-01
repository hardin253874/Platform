// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Instance activation data.
	/// </summary>
	[Immutable]
    public struct ActivationData : IActivationData
	{
		/// <summary>
		///     Obtains an empty ActivationData instance.
		/// </summary>
#pragma warning disable 649
		public static readonly ActivationData Empty;
#pragma warning restore

        /// <summary>
        ///     Initializes a new instance of the <see cref="ActivationData" /> class.
        /// </summary>
        /// <param name="entityInternalData">The internal data object to use.</param>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
        internal ActivationData(EntityInternalData entityInternalData)
            : this()
        {
            EntityInternalData = entityInternalData;
        }

		/// <summary>
		///     Initializes a new instance of the <see cref="ActivationData" /> class.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="tenantId">The tenant unique identifier.</param>
		[SuppressMessage( "Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames" )]
		public ActivationData( long id, long tenantId )
			: this( )
		{
			Id = id;
			ReadOnly = true;
			TenantId = tenantId;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ActivationData" /> structure.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="tenantId">The tenant unique identifier.</param>
		/// <param name="typeIds">The type ids.</param>
		[SuppressMessage( "Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames" )]
		public ActivationData( long id, long tenantId, ICollection<long> typeIds )
			: this( id, tenantId, true )
		{
			TypeIds = typeIds;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ActivationData" /> class.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="tenantId">The tenant unique identifier.</param>
		/// <param name="readOnly">
		///     if set to <c>true</c> [read only].
		/// </param>
		[SuppressMessage( "Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames" )]
		public ActivationData( long id, long tenantId, bool readOnly )
			: this( id, tenantId )
		{
			ReadOnly = readOnly;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ActivationData" /> structure.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="tenantId">The tenant unique identifier.</param>
		/// <param name="readOnly">
		///     if set to <c>true</c> [read only].
		/// </param>
		/// <param name="typeIds">The type ids.</param>
		[SuppressMessage( "Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames" )]
		public ActivationData( long id, long tenantId, bool readOnly, ICollection<long> typeIds )
			: this( id, tenantId, readOnly )
		{
			TypeIds = typeIds;
		}

		/// <summary>
		///     Gets the id.
		/// </summary>
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets a value indicating whether the entity is to be created read-only.
		/// </summary>
		/// <value>
		///     <c>true</c> if the entity is to be created as read-only; otherwise, <c>false</c>.
		/// </value>
		public bool ReadOnly
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
		public ICollection<long> TypeIds
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tenant unique identifier.
		/// </summary>
		/// <value>
		///     The tenant unique identifier.
		/// </value>
		public long TenantId
		{
			get;
			private set;
		}

        /// <summary>
        ///     Gets the entity internal data object to be used by activation.
        /// </summary>
        /// <value>
        ///     The tenant unique identifier.
        /// </value>
        internal EntityInternalData EntityInternalData
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
			if ( obj is ActivationData )
			{
				return Equals( ( ActivationData ) obj );
			}

			return false;
		}

		/// <summary>
		///     Determines whether the specified <see cref="ActivationData" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">
		///     The <see cref="ActivationData" /> to compare with this instance.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="ActivationData" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals( ActivationData obj )
		{
			return Id == obj.Id && ReadOnly == obj.ReadOnly && TenantId == obj.TenantId && ( ( TypeIds == null && obj.TypeIds == null ) || TypeIds.Equals( obj.TypeIds ) );
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
				int hashCode = 17;

				hashCode = hashCode * 92821 + Id.GetHashCode( );
				hashCode = hashCode * 92821 + ReadOnly.GetHashCode( );
				hashCode = hashCode * 92821 + TenantId.GetHashCode( );

				if ( TypeIds != null )
				{
					hashCode = hashCode * 92821 + TypeIds.GetHashCode( );
				}

				return hashCode;
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
		public static bool operator !=( ActivationData a, ActivationData b )
		{
			return !( a.Equals( b ) );
		}

		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="a">First object to compare.</param>
		/// <param name="b">Second object to compare.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( ActivationData a, ActivationData b )
		{
			return a.Equals( b );
		}

        /// <summary>
        /// Activate the entity.
        /// </summary>
        /// <returns>The new entity.</returns>
        IEntity IActivationData.CreateEntity( )
        {
            return new Entity( this );
        }
	}
}