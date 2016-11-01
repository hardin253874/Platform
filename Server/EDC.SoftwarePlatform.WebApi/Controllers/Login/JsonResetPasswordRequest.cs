// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
    /// <summary>
    /// Json submit email info
    /// </summary>
    public class JsonSubmitEmailRequest
    {
        /// <summary>
		///     Gets or sets the email.
		/// </summary>
		/// <value>
		///     The email addres to receive reset password email.
		/// </value>
		[DataMember( Name = "email", EmitDefaultValue = false, IsRequired = true )]
        public string Email
        {
            get;
            set;
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
    }


    /// <summary>
    /// Json Reset Password Info
    /// </summary>
    public class JsonResetPasswordRequest
    {
        /// <summary>
        ///     Gets or sets the new password.
        /// </summary>
        /// <value>
        ///     The new password.
        /// </value>
        [DataMember( Name = "key", EmitDefaultValue = false, IsRequired = true )]
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the new password.
        /// </summary>
        /// <value>
        ///     The new password.
        /// </value>
        [DataMember( Name = "newpassword", EmitDefaultValue = false, IsRequired = true )]
        public string NewPassword
        {
            get;
            set;
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
    }


}