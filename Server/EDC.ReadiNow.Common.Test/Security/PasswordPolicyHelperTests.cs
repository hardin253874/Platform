// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security
{
    [TestFixture]
	[RunWithTransaction]
    public class PasswordPolicyHelperTests
    {
        /// <summary>
        ///     Tests the get default password policy.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestGetDefaultPasswordPolicy()
        {
            PasswordPolicy passwordPolicy = PasswordPolicyHelper.GetDefaultPasswordPolicy();
            Assert.IsNotNull(passwordPolicy);
        }

        /// <summary>
        ///     Tests validating against all conditions fails.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateAllFailsDigits()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 5,
                             MustContainDigits = true,
                             MustContainLowerCaseCharacters = true,
                             MustContainSpecialCharacters = true,
                             MustContainUpperCaseCharacters = true
                         };

            Assert.Throws<ValidationException>(() => PasswordPolicyHelper.ValidatePassword(pp, "aaaaaA$"));
        }

        /// <summary>
        ///     Tests validating against all conditions fails.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateAllFailsLength()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 5,
                             MustContainDigits = true,
                             MustContainLowerCaseCharacters = true,
                             MustContainSpecialCharacters = true,
                             MustContainUpperCaseCharacters = true
                         };

            Assert.Throws<ValidationException>(() => PasswordPolicyHelper.ValidatePassword(pp, "1"));
        }

        /// <summary>
        ///     Tests validating against all conditions fails.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateAllFailsLowerCase()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 5,
                             MustContainDigits = true,
                             MustContainLowerCaseCharacters = true,
                             MustContainSpecialCharacters = true,
                             MustContainUpperCaseCharacters = true
                         };

            Assert.Throws<ValidationException>(() => PasswordPolicyHelper.ValidatePassword(pp, "12345A$"));
        }

        /// <summary>
        ///     Tests validating against all conditions fails.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateAllFailsSpecial()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 5,
                             MustContainDigits = true,
                             MustContainLowerCaseCharacters = true,
                             MustContainSpecialCharacters = true,
                             MustContainUpperCaseCharacters = true
                         };

            Assert.Throws<ValidationException>(() => PasswordPolicyHelper.ValidatePassword(pp, "12345aA"));
        }

        /// <summary>
        ///     Tests validating against all conditions fails.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateAllFailsUpperCase()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 5,
                             MustContainDigits = true,
                             MustContainLowerCaseCharacters = true,
                             MustContainSpecialCharacters = true,
                             MustContainUpperCaseCharacters = true
                         };

            Assert.Throws<ValidationException>(() => PasswordPolicyHelper.ValidatePassword(pp, "12345a$"));
        }

        /// <summary>
        ///     Tests validating against all conditions passes.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateAllPasses()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 5,
                             MustContainDigits = true,
                             MustContainLowerCaseCharacters = true,
                             MustContainSpecialCharacters = true,
                             MustContainUpperCaseCharacters = true
                         };

            Assert.DoesNotThrow(() => PasswordPolicyHelper.ValidatePassword(pp, "12345aA$"));
        }


        /// <summary>
        ///     Tests validating against password min length fails.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateMinimumPasswordLengthFails()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 5,
                             MustContainDigits = false,
                             MustContainLowerCaseCharacters = false,
                             MustContainSpecialCharacters = false,
                             MustContainUpperCaseCharacters = false
                         };

            Assert.Throws<ValidationException>(() => PasswordPolicyHelper.ValidatePassword(pp, "123"));
        }


        /// <summary>
        ///     Tests validating against password min length passes.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateMinimumPasswordLengthPasses()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 5,
                             MustContainDigits = false,
                             MustContainLowerCaseCharacters = false,
                             MustContainSpecialCharacters = false,
                             MustContainUpperCaseCharacters = false
                         };

            Assert.DoesNotThrow(() => PasswordPolicyHelper.ValidatePassword(pp, "12345"));
        }


        /// <summary>
        ///     Tests validating against must contain digits fails.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateMustContainDigitsFails()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 1,
                             MustContainDigits = true,
                             MustContainLowerCaseCharacters = false,
                             MustContainSpecialCharacters = false,
                             MustContainUpperCaseCharacters = false
                         };

            Assert.Throws<ValidationException>(() => PasswordPolicyHelper.ValidatePassword(pp, "aaa"));
        }


        /// <summary>
        ///     Tests validating against must contain digits passes.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateMustContainDigitsPasses()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 1,
                             MustContainDigits = true,
                             MustContainLowerCaseCharacters = false,
                             MustContainSpecialCharacters = false,
                             MustContainUpperCaseCharacters = false
                         };

            Assert.DoesNotThrow(() => PasswordPolicyHelper.ValidatePassword(pp, "aaa12"));
        }


        /// <summary>
        ///     Tests validating against must contain lower case chars fails.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateMustContainLowerCaseCharsFails()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 1,
                             MustContainDigits = false,
                             MustContainLowerCaseCharacters = true,
                             MustContainSpecialCharacters = false,
                             MustContainUpperCaseCharacters = false
                         };

            Assert.Throws<ValidationException>(() => PasswordPolicyHelper.ValidatePassword(pp, "AAAAA"));
        }


        /// <summary>
        ///     Tests validating against must contain lower case chars passes.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateMustContainLowerCaseCharsPasses()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 1,
                             MustContainDigits = false,
                             MustContainLowerCaseCharacters = true,
                             MustContainSpecialCharacters = false,
                             MustContainUpperCaseCharacters = false
                         };

            Assert.DoesNotThrow(() => PasswordPolicyHelper.ValidatePassword(pp, "aaaaAA"));
        }


        /// <summary>
        ///     Tests validating against must contain special chars fails.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateMustContainSpecialCharsFails()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 1,
                             MustContainDigits = false,
                             MustContainLowerCaseCharacters = false,
                             MustContainSpecialCharacters = true,
                             MustContainUpperCaseCharacters = false
                         };

            Assert.Throws<ValidationException>(() => PasswordPolicyHelper.ValidatePassword(pp, "bbbbb"));
        }


        /// <summary>
        ///     Tests validating against must contain special chars passes.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateMustContainSpecialCharsPasses()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 1,
                             MustContainDigits = false,
                             MustContainLowerCaseCharacters = false,
                             MustContainSpecialCharacters = true,
                             MustContainUpperCaseCharacters = false
                         };

            Assert.DoesNotThrow(() => PasswordPolicyHelper.ValidatePassword(pp, "aaa$%aAA"));
        }


        /// <summary>
        ///     Tests validating against must contain upper case chars fails.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateMustContainUpperCaseCharsFails()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 1,
                             MustContainDigits = false,
                             MustContainLowerCaseCharacters = false,
                             MustContainSpecialCharacters = false,
                             MustContainUpperCaseCharacters = true
                         };

            Assert.Throws<ValidationException>(() => PasswordPolicyHelper.ValidatePassword(pp, "bbbbb"));
        }


        /// <summary>
        ///     Tests validating against must contain upper case chars passes.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateMustContainUpperCaseCharsPasses()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 1,
                             MustContainDigits = false,
                             MustContainLowerCaseCharacters = false,
                             MustContainSpecialCharacters = false,
                             MustContainUpperCaseCharacters = true
                         };

            Assert.DoesNotThrow(() => PasswordPolicyHelper.ValidatePassword(pp, "aaaaAA"));
        }


        /// <summary>
        ///     Tests that validating a null password fails.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestValidateNullPasswordFails()
        {
            var pp = new PasswordPolicy
                         {
                             MinimumPasswordLength = 5,
                             MustContainDigits = false,
                             MustContainLowerCaseCharacters = false,
                             MustContainSpecialCharacters = false,
                             MustContainUpperCaseCharacters = false
                         };

            Assert.Throws<ValidationException>(() => PasswordPolicyHelper.ValidatePassword(pp, null));
        }
    }
}