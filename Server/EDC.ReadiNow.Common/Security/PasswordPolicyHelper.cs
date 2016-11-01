// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Resources;

namespace EDC.ReadiNow.Security
{
    /// <summary>
    /// </summary>
    public static class PasswordPolicyHelper
    {
        #region Constants

        /// <summary>
        ///     The alias of the password policy instance.
        /// </summary>
        private const string PasswordPolicyInstanceAlias = "core:passwordPolicyInstance";

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the default password policy.
        /// </summary>
        /// <returns>The default password policy</returns>
        public static PasswordPolicy GetDefaultPasswordPolicy()
        {
            return Entity.Get<PasswordPolicy>(PasswordPolicyInstanceAlias);
        }

        /// <summary>
        ///     Validates the password against the password policy.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="System.ArgumentException">policy</exception>
        /// <exception cref="ValidationException">
        /// </exception>
        public static void ValidatePassword(PasswordPolicy policy, string password)
        {
            if (policy == null)
            {
                throw new ArgumentException("policy");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ValidationException(GlobalStrings.PasswordIsRequired);
            }

            if (policy.MinimumPasswordLength != null &&
                password.Length < policy.MinimumPasswordLength)
            {
                throw new ValidationException(string.Format(GlobalStrings.PasswordIsTooShort, policy.MinimumPasswordLength));
            }

            if (policy.MustContainUpperCaseCharacters != null &&
                policy.MustContainUpperCaseCharacters.Value)
            {
                var upperCaseRegex = new Regex("[A-Z]");
                if (!upperCaseRegex.IsMatch(password))
                {
                    throw new ValidationException(GlobalStrings.PasswordMustHaveUppercase);
                }
            }

            if (policy.MustContainLowerCaseCharacters != null &&
                policy.MustContainLowerCaseCharacters.Value)
            {
                var lowerCaseRegex = new Regex("[a-z]");
                if (!lowerCaseRegex.IsMatch(password))
                {
                    throw new ValidationException(GlobalStrings.PasswordMustHaveLowercase);
                }
            }

            if (policy.MustContainDigits != null &&
                policy.MustContainDigits.Value)
            {
                var digitRegex = new Regex("[0-9]");
                if (!digitRegex.IsMatch(password))
                {
                    throw new ValidationException(GlobalStrings.PasswordMustHaveDigit);
                }
            }

            if (policy.MustContainSpecialCharacters != null &&
                policy.MustContainSpecialCharacters.Value)
            {
                var specialCharRegex = new Regex(@"[^a-zA-Z0-9\s]");
                if (!specialCharRegex.IsMatch(password))
                {
                    throw new ValidationException(GlobalStrings.PasswordMustHaveSpecial);
                }
            }
        }

        #endregion Public Methods
    }
}