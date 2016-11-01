// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Per Tenant Entity Type Cache Message.
	/// </summary>
	[DataContract]
	public class PerTenantEntityTypeCacheMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="PerTenantEntityTypeCacheMessage" /> class.
		/// </summary>
		public PerTenantEntityTypeCacheMessage( )
		{
			TenantIds = new HashSet<long>( );
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="PerTenantEntityTypeCacheMessage" /> is clear.
		/// </summary>
		/// <value>
		///     <c>true</c> if clear; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Order = 2 )]
		public bool Clear
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
		[DataMember( Order = 1 )]
		public ISet<long> TenantIds
		{
			get;
			set;
		}
	}
}