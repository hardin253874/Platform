// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Services.Common
{
	/// <summary>
	///     Defines the result associated with a resource operation.
	/// </summary>
	[DataContract]
	public class ResourceExistsResult
	{
		/// <summary>
		///     Gets or sets whether the resource object exists.
		/// </summary>
		[DataMember]
		public bool Exists
		{
			get
			{
				return ExistsById | ExistsByName;
			}
			set
			{
			}
		}

		/// <summary>
		///     Gets or sets whether the resource object exists by id.
		/// </summary>
		[DataMember]
		public bool ExistsById
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets whether the resource object exists by name.
		/// </summary>
		[DataMember]
		public bool ExistsByName
		{
			get;
			set;
		}
	}
}