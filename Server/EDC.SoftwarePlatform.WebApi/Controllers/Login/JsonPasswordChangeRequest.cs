// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
    /// <summary>
    /// Json Password change Info
    /// </summary>
    public class JsonPasswordChangeRequest
    {
        /// <summary>
		///     Gets or sets the old password.
		/// </summary>
		/// <value>
		///     The old password.
		/// </value>
		[DataMember(Name = "oldpassword", EmitDefaultValue = false, IsRequired = true)]
        public string OldPassword
        {
            get;
            set;
        }

		/// <summary>
		///		Should the old password value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeOldPassword( )
	    {
			return OldPassword != null;
	    }

        /// <summary>
        ///     Gets or sets the new password.
        /// </summary>
        /// <value>
        ///     The new password.
        /// </value>
        [DataMember(Name = "newpassword", EmitDefaultValue = false, IsRequired = true)]
        public string NewPassword
        {
            get;
            set;
        }

		/// <summary>
		///		Should the new password value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeNewPassword( )
	    {
			return NewPassword != null;
	    }

        /// <summary>
        ///     Gets or sets the Tenant.
        /// </summary>
        /// <value>
        ///     The Tenant.
        /// </value>
        [DataMember(Name = "tenant", EmitDefaultValue = false, IsRequired = true)]
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
        [DataMember(Name = "username", EmitDefaultValue = false, IsRequired = true)]
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