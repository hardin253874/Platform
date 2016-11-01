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
        /// The new password.
        /// </summary>
        [DataMember(Name = "password", IsRequired = true)]
        public string Password { get; set; }
    }
}