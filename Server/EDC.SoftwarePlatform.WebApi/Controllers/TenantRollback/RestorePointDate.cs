// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.TenantRollback
{
	/// <summary>
	///     Restore Point Date class.
	/// </summary>
	[DataContract]
	public class RestorePointDate
	{
		/// <summary>
		///     Gets or sets the date string.
		/// </summary>
		/// <value>
		///     The date string.
		/// </value>
		[DataMember( Name = "dateString" )]
		public string DateString
		{
			get;
			set;
		}
	}
}