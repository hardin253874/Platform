// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
	/// <summary>
	///     JSON Login Credential
	/// </summary>
	public class JsonLoginCredential
	{
		/// <summary>
		///     Gets or sets the Password.
		/// </summary>
		/// <value>
		///     The Password.
		/// </value>
		[DataMember( Name = "password", EmitDefaultValue = false, IsRequired = true )]
		public string Password
		{
			get;
			set;
		}

		/// <summary>
		///		Should the password value be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializePassword( )
		{
			return Password != null;
		}

		/// <summary>
		///     Gets or sets a value indicating whether the login is persistent.
		/// </summary>
		/// <value>
		///     <c>true</c> if the login is persistent; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "persistent", EmitDefaultValue = false, IsRequired = false )]
		public bool Persistent
		{
			get;
			set;
		}

		/// <summary>
		///		Should the persistent value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializePersistent( )
	    {
			return Persistent;
	    }

		/// <summary>
		///     Gets or sets the Tenant.
		/// </summary>
		/// <value>
		///     The Tenant.
		/// </value>
		[DataMember( Name = "tenant", EmitDefaultValue = false, IsRequired = true )]
		public string Tenant
		{
			get;
			set;
		}

		/// <summary>
		///		Should the tenant value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeTenant( )
	    {
			return Tenant != null;
	    }

		/// <summary>
		///     Gets or sets the UserName.
		/// </summary>
		/// <value>
		///     The UserName.
		/// </value>
		[DataMember( Name = "username", EmitDefaultValue = false, IsRequired = true )]
		public string Username
		{
			get;
			set;
		}

		/// <summary>
		///		Should the username value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeUsername( )
	    {
			return Username != null;
	    }
	}
}