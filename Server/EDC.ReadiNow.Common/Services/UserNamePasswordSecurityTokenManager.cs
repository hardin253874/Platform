// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Security;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace EDC.ReadiNow.Services
{    
    /// <summary>
    /// 
    /// </summary>
    internal class UserNamePasswordSecurityTokenManager : ServiceCredentialsSecurityTokenManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserNamePasswordSecurityTokenManager"/> class.
        /// </summary>
        /// <param name="credentials">The credentials.</param>
        public UserNamePasswordSecurityTokenManager(UserNamePasswordServiceCredentials credentials)
            : base(credentials)
        {
        }


        /// <summary>
        /// Creates a security token authenticator based on the <see cref="T:System.IdentityModel.Selectors.SecurityTokenRequirement"/>.
        /// </summary>
        /// <param name="tokenRequirement">The security token requirement.</param>
        /// <param name="outOfBandTokenResolver">When this method returns, contains a <see cref="T:System.IdentityModel.Selectors.SecurityTokenResolver"/>. This parameter is passed uninitialized.</param>
        /// <returns>
        /// The security token authenticator.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="tokenRequirement"/> is null.</exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">A security token authenticator cannot be created for the <paramref name=" tokenRequirement"/> that was passed in.</exception>
        public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
        {
            if (tokenRequirement.TokenType == SecurityTokenTypes.UserName)
            {
                outOfBandTokenResolver = null;
                return new UserNamePasswordSecurityTokenAuthenticator(ServiceCredentials.UserNameAuthentication.CustomUserNamePasswordValidator);
            }

            return base.CreateSecurityTokenAuthenticator(tokenRequirement, out outOfBandTokenResolver);
        }
    }
}
