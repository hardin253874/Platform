// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Security
{
	/// <summary>
	///     User Account Cache Message
	/// </summary>
	[DataContract]
	public class UserAccountCacheMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="UserAccountCacheMessage" /> class.
		/// </summary>
		public UserAccountCacheMessage( )
		{
			Usernames = new HashSet<string>( );
		}

		/// <summary>
		///     Gets or sets the name of the tenant.
		/// </summary>
		/// <value>
		///     The name of the tenant.
		/// </value>
		[DataMember( Order = 1 )]
		public string TenantName
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the user names.
		/// </summary>
		/// <value>
		///     The user names.
		/// </value>
		[DataMember( Order = 2 )]
		public ISet<string> Usernames
		{
			get;
			set;
		}
	}
}