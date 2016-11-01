// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.IdentityModel.Selectors;
using System.ServiceModel.Security;

namespace EDC.ReadiNow.Services
{
    /// <summary>
    // This class is used to hook into WCF security as the CustomUserNamePasswordValidator executes on a different thread
    // than the actual operation, which means that the RequestContext is not set on the correct thread.
    /// </summary>
    public class UserNamePasswordServiceCredentials : ServiceCredentials
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="UserNamePasswordServiceCredentials"/> class.
        /// </summary>
        public UserNamePasswordServiceCredentials()
        {
        }


        /// <summary>
        /// Prevents a default instance of the <see cref="UserNamePasswordServiceCredentials"/> class from being created.
        /// </summary>
        /// <param name="clone">The clone.</param>
        private UserNamePasswordServiceCredentials(UserNamePasswordServiceCredentials clone)
            : base(clone)
        {
        }
        #endregion


        /// <summary>
        /// Copies the essential members of the current instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.ServiceModel.Description.ServiceCredentials"/> instance.
        /// </returns>
        protected override ServiceCredentials CloneCore()
        {
            return new UserNamePasswordServiceCredentials(this);
        }


        /// <summary>
        /// Creates a token manager for this service.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.ServiceModel.Security.ServiceCredentialsSecurityTokenManager"/> instance.
        /// </returns>
        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            // Check if the current validation mode is for custom username password validation
            if (UserNameAuthentication.UserNamePasswordValidationMode == UserNamePasswordValidationMode.Custom)
            {
                return new UserNamePasswordSecurityTokenManager(this);
            }            

            return base.CreateSecurityTokenManager();
        }
    }
}
