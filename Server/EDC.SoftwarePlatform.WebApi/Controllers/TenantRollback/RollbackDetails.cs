// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.TenantRollback
{
	/// <summary>
	/// </summary>
	[DataContract]
	public class RollbackDetails
	{
		/// <summary>
		///     Gets or sets the date.
		/// </summary>
		/// <value>
		///     The date.
		/// </value>
		[DataMember( Name = "date" )]
		public DateTime Date
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[DataMember( Name = "name" )]
		public String Name
		{
			get;
			set;
		}
	}
}