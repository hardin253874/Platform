// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;

using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Services
{
    /// <summary>
    /// Defines a custom username and password validator for use with services.
    /// </summary>
    public class CustomUsernameValidator : UserNamePasswordValidator
    {
        /// <summary>
        /// Validates the specified username and password.
        /// </summary>
        /// <param name="username">
        /// A string containing the username of the account to to validate.
        /// </param>
        /// <param name="password">
        /// A string containing the password of the account to validate.
        /// </param>
        public override void Validate(string username, string password)
        {
            if (String.IsNullOrEmpty(username))
            {
                throw new ArgumentException("The specified username parameter is invalid.");
            }

            string tenant = string.Empty;

            // Split the tenant and username fields
            string[] credentials = username.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (credentials.Length > 1)
            {
                tenant = credentials[0];
                username = credentials[1];
            }

            // Validate the credentials
            // Don't store the credentials in the thread context.
            // This is due to the fact that the validator may operate on a different
            // thread than the actual service code.
            UserAccountValidator.Validate(username, password, tenant, false);
        }
    }
}
