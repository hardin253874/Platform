// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AuditLog;
using EDC.ReadiNow.Security.AuditLog.EventTargets;
using Moq;
using NUnit.Framework;

// ReSharper disable CheckNamespace
namespace EDC.ReadiNow.Test.Security.AuditLogTest.EventTargets
// ReSharper restore CheckNamespace
{
    [TestFixture]
    [RunWithTransaction]
    public class AuditLogPasswordPolicyEventTargetTests
    {
        /// <summary>
        ///     Tests changing the password policy writes the correct audit log message.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestOnChangePasswordPolicy()
        {
            bool success = true;

            const int oldMinPasswordLength = 15;
            const int newMinPasswordLength = 16;
            const int oldMaxPasswordAge = 10;
            const int newMaxPasswordAge = 11;
            const bool oldMustContainUpperCaseChars = true;
            const bool newMustContainUpperCaseChars = false;
            const bool oldMustContainLowerCaseChars = true;
            const bool newMustContainLowerCaseChars = false;
            const bool oldMustContainDigits = true;
            const bool newMustContainDigits = false;
            const bool oldMustContainSpecialChars = true;
            const bool newMustContainSpecialChars = false;
            const int oldAccountLockoutDuration = 22;
            const int newAccountLockoutDuration = 23;
            const int oldAccountLockoutThreshold = 32;
            const int newAccountLockoutThreshold = 33;

            var mockAuditLog = new Mock<IAuditLog>(MockBehavior.Strict);
            mockAuditLog.Setup(al => al.OnChangePasswordPolicy(success, 
                oldMinPasswordLength, 
                newMinPasswordLength,
                oldMaxPasswordAge,
                newMaxPasswordAge,
                oldMustContainUpperCaseChars,
                newMustContainUpperCaseChars,
                oldMustContainLowerCaseChars,
                newMustContainLowerCaseChars,
                oldMustContainDigits,
                newMustContainDigits,
                oldMustContainSpecialChars,
                newMustContainSpecialChars,
                oldAccountLockoutDuration,
                newAccountLockoutDuration,
                oldAccountLockoutThreshold,
                newAccountLockoutThreshold));

            var eventTarget = new AuditLogPasswordPolicyEventTarget(mockAuditLog.Object);

            var passwordPolicy = new PasswordPolicy
            {
                MaximumPasswordAge = oldMaxPasswordAge, 
                MinimumPasswordLength = oldMinPasswordLength, 
                MustContainDigits = oldMustContainDigits, 
                MustContainLowerCaseCharacters = oldMustContainLowerCaseChars, 
                MustContainSpecialCharacters = oldMustContainSpecialChars, 
                MustContainUpperCaseCharacters = oldMustContainUpperCaseChars, 
                AccountLockoutDuration = oldAccountLockoutDuration, 
                AccountLockoutThreshold = oldAccountLockoutThreshold
            };
            passwordPolicy.Save();

            passwordPolicy.MaximumPasswordAge = newMaxPasswordAge;
            passwordPolicy.MinimumPasswordLength = newMinPasswordLength;
            passwordPolicy.MustContainDigits = newMustContainDigits;
            passwordPolicy.MustContainLowerCaseCharacters = newMustContainLowerCaseChars;
            passwordPolicy.MustContainSpecialCharacters = newMustContainSpecialChars;
            passwordPolicy.MustContainUpperCaseCharacters = newMustContainUpperCaseChars;
            passwordPolicy.AccountLockoutDuration = newAccountLockoutDuration;
            passwordPolicy.AccountLockoutThreshold = newAccountLockoutThreshold;

            IDictionary<string, object> state = new Dictionary<string, object>();

            eventTarget.GatherAuditLogEntityDetailsForSave(passwordPolicy, state);
            eventTarget.WriteSaveAuditLogEntries(success, passwordPolicy.Id, state);

            mockAuditLog.VerifyAll();
        }
    }
}