// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
	/// <summary>
	///     Response result.
	/// </summary>
	[DataContract]
	public class ResponseResult
	{
		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="ResponseResult" /> is success.
		/// </summary>
		/// <value>
		///     <c>true</c> if success; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "success", EmitDefaultValue = true, IsRequired = false )]
		public bool Success
		{
			get;
			set;
		}
	}
}