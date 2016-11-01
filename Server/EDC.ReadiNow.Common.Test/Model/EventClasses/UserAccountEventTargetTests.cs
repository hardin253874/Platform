// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;
using EDC.Security;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model.EventClasses
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    public class UserAccountEventTargetTests
    {
        /// <summary>
        ///     Tests saving a user account with an illegal password fails validation.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestSaveUserAccountPasswordFailsValidation()
        {
            var userAccount = new UserAccount { Name = "userAccountHashTest" + Guid.NewGuid(), Password = "p" };
            Assert.Throws<ValidationException>(() => userAccount.Save());
        }

        /// <summary>
        ///     Tests the saving a user account hashes the password.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestSaveUserAccountPasswordIsHashed()
        {
            string password = "P@ssword1";
            var userAccount = new UserAccount {Name = "userAccountHashTest" + Guid.NewGuid(), Password = password};
            userAccount.Save();

            Assert.AreNotEqual(password, userAccount.Password);
            Assert.IsTrue(CryptoHelper.ValidatePassword(password, userAccount.Password));
        }


        /// <summary>
        /// Tests the password only hashed when changed.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestPasswordOnlyHashedWhenChanged()
        {
            string password = "P@ssword1";
            var userAccount = new UserAccount { Name = "userAccountHashTest" + Guid.NewGuid(), Password = password };
            userAccount.Save();

            Assert.AreNotEqual(password, userAccount.Password);
            Assert.IsTrue(CryptoHelper.ValidatePassword(password, userAccount.Password));

            userAccount.ChangePasswordAtNextLogon = true;
            userAccount.Save();

            // Verify password is still correct and is not rehashed
            Assert.AreNotEqual(password, userAccount.Password);
            Assert.IsTrue(CryptoHelper.ValidatePassword(password, userAccount.Password));
        }


        /// <summary>
        /// Tests that the bad logon count is reset when the account status is set to active.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestResetBadLogonCountWhenAccountStatusSetToActive()
        {
            string password = "P@ssword1";
            var userAccount = new UserAccount { Name = "resetBadLogonTest" + Guid.NewGuid(), Password = password, BadLogonCount = 10, AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Locked };
            userAccount.Save();

            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.Save();

            // Verify bad logon count is reset
            Assert.AreEqual(0, userAccount.BadLogonCount);            
        }


        /// <summary>
        /// Tests that the bad logon count is not reset when the account status is not changed.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void TestNotResetBadLogonCountWhenAccountStatusIsUnchanged()
        {
            string password = "P@ssword1";
            var userAccount = new UserAccount { Name = "resetBadLogonTest" + Guid.NewGuid(), Password = password, BadLogonCount = 10, AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Locked };
            userAccount.Save();

            userAccount.ChangePasswordAtNextLogon = true;
            userAccount.Save();

            // Verify bad logon count is not reset
            Assert.AreEqual(10, userAccount.BadLogonCount);
        }
    }
}