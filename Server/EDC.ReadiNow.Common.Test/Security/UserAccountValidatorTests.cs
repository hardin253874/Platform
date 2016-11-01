// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace EDC.ReadiNow.Test.Security
{
    [TestFixture]
    public class UserAccountValidatorTests
    {
        [Test]
        public void Authenticate_NullUsername()
        {
            Assert.That(() => UserAccountValidator.Authenticate(null, "password", "tenant", false),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("username"));
        }

        [Test]
        public void Authenticate_EmptyUsername()
        {
            Assert.That(() => UserAccountValidator.Authenticate(string.Empty, "password", "tenant", false),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("username"));
        }

        [Test]
        public void Authenticate_NullPassword()
        {
            Assert.That(() => UserAccountValidator.Authenticate("username", null, "tenant", false),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("password"));
        }

        [Test]
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(+1, true)]
        public void Authenticate_PasswordLength(int passwordLengthAdjustment, bool expectPasswordTooLongException)
        {
            Assert.That(
                () => UserAccountValidator.Authenticate("Administrator", "a".PadLeft(UserAccountValidator.MaxPasswordLength + passwordLengthAdjustment), "edc", false),
                expectPasswordTooLongException 
                    ? (Constraint) Throws.TypeOf<InvalidCredentialException>().And.Property("Message").StartsWith("Password cannot be longer than")
                    : Throws.TypeOf<InvalidCredentialException>().And.Property("Message").EqualTo(UserAccountValidator.InvalidUserNameOrPasswordMessage));
        }

        [Test]
        public void Authenticate_NullTenant()
        {
            Assert.That(() => UserAccountValidator.Authenticate("username", "password", null, false),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("tenant"));
        }

        [Test]
        public void Authenticate_EmptyTenant()
        {
            Assert.That(() => UserAccountValidator.Authenticate("username", "password", "", false),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("tenant"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [FailOnEvent]
        public void Authenticate_OK()
        {
            const string password = "Foobar123!@#";
            UserAccount userAccount = null;
            userAccount = new UserAccount();

            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.Password = password;
            userAccount.Save();

            Assert.That(
                () => UserAccountValidator.Authenticate(userAccount.Name, password, RequestContext.GetContext().Tenant.Name, false),
                Throws.Nothing);

            userAccount = Entity.Get<UserAccount>(userAccount.Id);

            // Not updating LastLogin for now
            //Assert.That(
            //    userAccount.LastLogon,
            //    Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
            Assert.That(
                userAccount.AccountStatus_Enum,
                Is.EqualTo(UserAccountStatusEnum_Enumeration.Active));
            Assert.That(
                userAccount.BadLogonCount,
                Is.Null.Or.EqualTo(0));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [FailOnEvent]
        public void Authenticate_BadUsername()
        {
            const string username = "___";
            const string password = "Foobar123!@#";

            Assert.That(Entity.GetByField<UserAccount>(username, new EntityRef("core:name")),
                Is.Empty, "User name exists");

            Assert.That(
                () => UserAccountValidator.Authenticate(username, password, RequestContext.GetContext().Tenant.Name, false),
                Throws.TypeOf<InvalidCredentialException>());
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [FailOnEvent]
        public void Authenticate_BadPassword()
        {
            const string password = "Foobar123!@#";
            UserAccount userAccount;

            userAccount = new UserAccount();
            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.Password = password;
            userAccount.Save();

            Assert.That(
                () => UserAccountValidator.Authenticate(userAccount.Name, password + "_", RequestContext.GetContext().Tenant.Name, false),
                Throws.TypeOf<InvalidCredentialException>());
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [FailOnEvent]
        public void Authenticate_BadTenant()
        {
            const string password = "Foobar123!@#";
            UserAccount userAccount = null;
            userAccount = new UserAccount();

            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.Password = password;
            userAccount.Save();

            Assert.That(
                () => UserAccountValidator.Authenticate(userAccount.Name, password, "foo", false),
                Throws.TypeOf<InvalidCredentialException>());
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [FailOnEvent]
        public void Authenticate_DisabledTenant()
        {
            const string password = "Foobar123!@#";
            UserAccount userAccount;
            Tenant tenant;
            long tenantId;

            userAccount = new UserAccount();
            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.Password = password;
            userAccount.Save();

            tenantId = RequestContext.GetContext().Tenant.Id;
            try
            {
                RequestContext.SetSystemAdministratorContext();

                tenant = Entity.Get<Tenant>(tenantId, true);
                tenant.IsTenantDisabled = true;
                tenant.Save();

                RequestContext.SetTenantAdministratorContext("EDC");

                Assert.That(
                    () => UserAccountValidator.Authenticate(userAccount.Name, password, RequestContext.GetContext().Tenant.Name, false),
                    Throws.TypeOf<TenantDisabledException>());
            }
            finally 
            {
                RequestContext.SetSystemAdministratorContext();

                tenant = Entity.Get<Tenant>(tenantId, true);
                tenant.IsTenantDisabled = false;
                tenant.Save();

                RequestContext.SetTenantAdministratorContext("EDC");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(-1, false)]
        [TestCase(0, true)]
        [TestCase(+1, true)]
        public void Authenticate_Lockout(int logonAttemptOffset, bool expectLockedOut)
        {
            const string password = "Foobar123!@#";
            UserAccount userAccount;
            PasswordPolicy passwordPolicy;

            passwordPolicy = Entity.Get<PasswordPolicy>("core:passwordPolicyInstance");

            userAccount = new UserAccount();
            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.Password = password;
            userAccount.Save();

            for (int i = 0; i < passwordPolicy.AccountLockoutThreshold + logonAttemptOffset; i++)
            {
                try
                {
                    UserAccountValidator.Authenticate(
                        userAccount.Name,
                        userAccount.Password + "_foo", // Invalid password
                        RequestContext.GetContext().Tenant.Name,
                        false);
                }
                catch (Exception)
                {
                    // This will throw exceptions if the user is locked, disabled or expired. Ignore these exceptions in this test.
                }
            }

            userAccount = Entity.Get<UserAccount>(userAccount.Id);
            Assert.That(userAccount.AccountStatus_Enum,
                expectLockedOut
                    ? Is.EqualTo(UserAccountStatusEnum_Enumeration.Locked)
                    : Is.EqualTo(UserAccountStatusEnum_Enumeration.Active));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(UserAccountStatusEnum_Enumeration.Active, true)]
        [TestCase(UserAccountStatusEnum_Enumeration.Locked, false)]
        [TestCase(UserAccountStatusEnum_Enumeration.Expired, false)]
        [TestCase(UserAccountStatusEnum_Enumeration.Disabled, false)]
        public void Authenticate_LockoutActiveUserOnly(UserAccountStatusEnum_Enumeration status, bool expectLockedOut)
        {
            const string password = "Foobar123!@#";
            UserAccount userAccount;
            PasswordPolicy passwordPolicy;

            passwordPolicy = Entity.Get<PasswordPolicy>("core:passwordPolicyInstance");

            userAccount = new UserAccount();
            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.AccountStatus_Enum = status;
            userAccount.Password = password;
            userAccount.Save();

            for (int i = 0; i < passwordPolicy.AccountLockoutThreshold; i++)
            {
                try
                {
                    UserAccountValidator.Authenticate(
                        userAccount.Name, 
                        userAccount.Password + "_foo", // Invalid password
                        RequestContext.GetContext().Tenant.Name, 
                        false);    
                }
                catch (Exception)
                {
                    // This will throw exceptions if the user is locked, disabled or expired. Ignore these exceptions in this test.
                }
            }

            userAccount = Entity.Get<UserAccount>(userAccount.Id);
            Assert.That(userAccount.AccountStatus_Enum,
                expectLockedOut ? Is.EqualTo(UserAccountStatusEnum_Enumeration.Locked) : Is.EqualTo(status));
        }

        [Test]
        public void Validate_NullUsername()
        {
            Assert.That(() => UserAccountValidator.Validate(null, "password", "tenant", false),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("username"));
        }

        [Test]
        public void Validate_EmptyUsername()
        {
            Assert.That(() => UserAccountValidator.Validate(string.Empty, "password", "tenant", false),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("username"));
        }

        [Test]
        public void Validate_NullPassword()
        {
            Assert.That(() => UserAccountValidator.Validate("username", null, "tenant", false),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("password"));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(+1, false)]
        public void Validate_NoLockout(int logonAttemptOffset, bool expectLockedOut)
        {
            const string password = "Foobar123!@#";
            UserAccount userAccount;
            PasswordPolicy passwordPolicy;

            passwordPolicy = Entity.Get<PasswordPolicy>("core:passwordPolicyInstance");

            userAccount = new UserAccount();
            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.Password = password;
            userAccount.Save();

            for (int i = 0; i < passwordPolicy.AccountLockoutThreshold + logonAttemptOffset; i++)
            {
                try
                {
                    UserAccountValidator.Validate(
                        userAccount.Name,
                        userAccount.Password + "_foo", // Invalid password
                        RequestContext.GetContext().Tenant.Name,
                        false);
                }
                catch (Exception)
                {
                    // This will throw exceptions if the user is locked, disabled or expired. Ignore these exceptions in this test.
                }
            }

            userAccount = Entity.Get<UserAccount>(userAccount.Id);
            Assert.That(userAccount.AccountStatus_Enum,
                expectLockedOut
                    ? Is.EqualTo(UserAccountStatusEnum_Enumeration.Locked)
                    : Is.EqualTo(UserAccountStatusEnum_Enumeration.Active));
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase( "Active", null )]
        [TestCase( "Expired", typeof(AccountExpiredException) )]
        [TestCase( "Locked", typeof( AccountLockedException ) )]
        [TestCase( "Disabled", typeof( AccountDisabledException ) )]
        public void ValidateAccountStatus( string accountStatus, Type exceptionType )
        {
            UserAccount userAccount;
            var status = ( UserAccountStatusEnum_Enumeration ) Enum.Parse( typeof( UserAccountStatusEnum_Enumeration ), accountStatus );
        
            userAccount = new UserAccount( );
            userAccount.Name = "Test user " + Guid.NewGuid( );
            userAccount.AccountStatus_Enum = status;
            userAccount.Password = "HelloWorld123!@#";
            userAccount.Save( );

            if ( exceptionType == null )
            {
                UserAccountValidator.ValidateAccountStatus( userAccount, false );
            }
            else
            {
                Assert.Throws( exceptionType, ( ) => UserAccountValidator.ValidateAccountStatus( userAccount, false ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public void Authenticate_ExpiredPassword(bool skipExpiryCheck, bool neverExpires)
        {
            const string password = "Foobar123!@#";
            UserAccount userAccount;

            PasswordPolicy passwordPolicy = Entity.Get<PasswordPolicy>("core:passwordPolicyInstance");            
            int maximumPasswordAge = passwordPolicy.MaximumPasswordAge ?? 0;

            Assert.AreNotEqual(0, maximumPasswordAge);

            userAccount = new UserAccount();
            userAccount.Name = "Test user " + Guid.NewGuid();
            userAccount.AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active;
            userAccount.PasswordNeverExpires = neverExpires;            
            userAccount.Password = password;
            userAccount.Save();

            // Set the password as being expired
            userAccount.PasswordLastChanged = DateTime.UtcNow.AddDays(-1 * (maximumPasswordAge + 1));
            userAccount.Save();

            if (skipExpiryCheck || neverExpires)
            {
                UserAccountValidator.Authenticate(userAccount.Name, password, RequestContext.GetContext().Tenant.Name, false, skipExpiryCheck);
            } else
            {
                Assert.That(
                () => UserAccountValidator.Authenticate(userAccount.Name, password, RequestContext.GetContext().Tenant.Name, false, skipExpiryCheck),                
                Throws.TypeOf<PasswordExpiredException>());
            }            
        }
    }
}
