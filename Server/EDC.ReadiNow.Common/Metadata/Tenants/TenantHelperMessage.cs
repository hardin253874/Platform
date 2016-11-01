// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using ProtoBuf;

namespace EDC.ReadiNow.Metadata.Tenants
{
	/// <summary>
	///     Tenant Helper Message
	/// </summary>
	[ProtoContract]
	public class TenantHelperMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TenantHelperMessage" /> class.
		/// </summary>
		public TenantHelperMessage( )
		{
			EntityIds = new HashSet<long>( );
		}

        /// <summary>
        /// The type of message that is being sent
        /// </summary>
        [ProtoMember(3)]
        public TenantHelperMessageType MessageType
        {
            get;
            set;
        }

		/// <summary>
		///     Gets or sets the entity ids.
		/// </summary>
		/// <value>
		///     The entity ids.
		/// </value>
		[ProtoMember( 2 )]
		public ISet<long> EntityIds
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		[ProtoMember( 1 )]
		public long TenantId
		{
			get;
			set;
		}
	}

    /// <summary>
    /// The type of the tenant helper message
    /// </summary>
    public enum TenantHelperMessageType {InvalidateTenant, NewTenant};
}