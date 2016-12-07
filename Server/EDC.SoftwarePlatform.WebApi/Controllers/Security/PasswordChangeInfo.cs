// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Security
{
    [DataContract]
    public class PasswordChangeInfo
    {
		/// <summary>
		/// Gets or sets the current password.
		/// </summary>
		/// <value>
		/// The current password.
		/// </value>
		[DataMember( Name = "currentPassword", IsRequired = true )]
		public string CurrentPassword { get; set; }

		/// <summary>
		/// The new password.
		/// </summary>
		[DataMember(Name = "password", IsRequired = true)]
        public string Password { get; set; }
    }
}