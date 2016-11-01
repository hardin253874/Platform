// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
	/// <summary>
	///     Active account info sent back to the client after a successful login.
	/// </summary>
	[DataContract]
	public class ActiveAccountInfo
	{
		/// <summary>
		///     Gets or sets the logged-in account id.
		/// </summary>
		/// <value>
		///     The account id.
		/// </value>
		[DataMember( Name = "accountId", EmitDefaultValue = true, IsRequired = false )]
		public long AccountId { get; set; }

		/// <summary>
		/// Gets or sets the tenant.
		/// </summary>
		/// <value>
		/// The tenant.
		/// </value>
        [DataMember(Name = "tenant", EmitDefaultValue = false, IsRequired = false)]
        public string Tenant { get; set; }

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
		/// Gets or sets the username.
		/// </summary>
		/// <value>
		/// The username.
		/// </value>
        [DataMember(Name = "username", EmitDefaultValue = false, IsRequired = false)]
        public string Username { get; set; }

		/// <summary>
		///		Should the username value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeUsername( )
	    {
			return Username != null;
	    }

		/// <summary>
		/// Gets or sets the provider.
		/// </summary>
		/// <value>
		/// The provider.
		/// </value>
        [DataMember(Name = "provider", EmitDefaultValue = false, IsRequired = false)]
        public string Provider { get; set; }

		/// <summary>
		///		Should the provider value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeProvider( )
	    {
			return Provider != null;
	    }		
    }
}