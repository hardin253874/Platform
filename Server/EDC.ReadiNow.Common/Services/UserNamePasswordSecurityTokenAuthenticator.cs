// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Selectors;
using System.IdentityModel.Policy;
using System.Collections.ObjectModel;
using EDC.ReadiNow.Security;
using System.IdentityModel.Tokens;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Services
{    
    /// <summary>
    /// 
    /// </summary>
    internal class UserNamePasswordSecurityTokenAuthenticator : CustomUserNameSecurityTokenAuthenticator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserNamePasswordSecurityTokenAuthenticator"/> class.
        /// </summary>
        /// <param name="validator">A <see cref="T:System.IdentityModel.Selectors.UserNamePasswordValidator"/>  that authenticates the user name and password using a custom authentication scheme.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="validator"/> is null.</exception>
        public UserNamePasswordSecurityTokenAuthenticator(UserNamePasswordValidator validator)
            : base(validator)
        {
        }


        /// <summary>
        /// Authenticates the specified user name and password and returns the set of authorization policies for <see cref="T:System.IdentityModel.Tokens.UserNameSecurityToken"/> security tokens.
        /// </summary>
        /// <param name="userName">The user name associated with the security token.</param>
        /// <param name="password">The password associated with the security token.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/> of type <see cref="T:System.IdentityModel.Policy.IAuthorizationPolicy"/> that contains the set of authorization policies in effect for this application.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="userName"/> is null.</exception>
        ///   
        /// <exception cref="T:System.IdentityModel.Tokens.SecurityTokenValidationException">
        ///   <paramref name="userName"/> and <paramref name="password"/> combination are not valid.</exception>
        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(string userName, string password)
        {            
            ReadOnlyCollection<IAuthorizationPolicy> currentPolicies = base.ValidateUserNamePasswordCore(userName, password);
            
            List<IAuthorizationPolicy> newPolicies = new List<IAuthorizationPolicy>(currentPolicies);
            
            string tenant = string.Empty;

            // Split the tenant and username fields
            string[] credentials = userName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (credentials.Length > 1)
            {
                tenant = credentials[0];
                userName = credentials[1];
            }

            // Validate the user name and password                        
            RequestContextData contextData = UserAccountCache.GetRequestContext(userName, password, tenant);

            if (contextData == null)
            {
                throw new SecurityTokenValidationException("Invalid username and/or password.");
            }            
                
            // Extend the original context with culture information                    
            newPolicies.Add(new IdentityTenantAuthorizationPolicy(contextData.Identity, contextData.Tenant));                         
 
            return newPolicies.AsReadOnly();                              
        }
    }
}
