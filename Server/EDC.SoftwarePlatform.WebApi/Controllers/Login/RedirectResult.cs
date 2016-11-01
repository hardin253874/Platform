// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
	/// <summary>
	///     Redirect result.
	/// </summary>
	[DataContract]
	[KnownType( typeof ( LoginResult ) )]
	public abstract class RedirectResult
	{
		/// <summary>
		///     Gets or sets the error message.
		/// </summary>
		/// <value>
		///     The error message.
		/// </value>
		[DataMember( Name = "error", EmitDefaultValue = true, IsRequired = false )]
		public string ErrorMessage
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the redirect URL.
		/// </summary>
		/// <value>
		///     The redirect URL.
		/// </value>
		[DataMember( Name = "redirectUrl", EmitDefaultValue = true, IsRequired = true )]
		public string RedirectUrl
		{
			get;
			set;
		}
	}
}