// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Database;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    [RunWithTransaction]
    [FailOnEvent]
    public class AccessRuleTests
    {
        [Test]
        [TestCaseSource("Test_Enable_Source")]
        [RunAsDefaultTenant]
        public void Test_EnableReadRule(string description, IEntityAccessControlChecker entityAccessControlChecker)
        {
            AccessRule accessRule;
            EntityType entityType;
            UserAccount userAccount;
            IEntity entity1;
            IDictionary<long, bool> result;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                entityType = Entity.Create<EntityType>();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                userAccount = Entity.Create<UserAccount>();

                accessRule = Entity.Create<AccessRule>();
                accessRule.AccessRuleEnabled = false;
                accessRule.PermissionAccess.Add(Permissions.Read.Entity.As<Permission>());
                accessRule.ControlAccess = entityType.As<SecurableEntity>();
                accessRule.AccessRuleReport = TestQueries.Entities().ToReport();
                accessRule.Save();

                userAccount.AllowAccess.Add(accessRule);
                userAccount.Save();

                entity1 = Entity.Create(entityType);
                entity1.Save();
                ctx.CommitTransaction();
            }

            // Ensure no access
            result = entityAccessControlChecker.CheckAccess(
                new[] { new EntityRef(entity1) },
                new[] { Permissions.Read },
                userAccount);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entity1.Id)
                .And.Property("Value").False, "Disabled check");

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule.AccessRuleEnabled = true;
                accessRule.Save();
                ctx.CommitTransaction();
            }

            // Ensure access
            result = entityAccessControlChecker.CheckAccess(
                new[] { new EntityRef(entity1) },
                new[] { Permissions.Read },
                userAccount);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result, Has.Exactly(1).Property("Key").EqualTo(entity1.Id)
                .And.Property("Value").True, "Enabled check");
        }

        private IEnumerable Test_Enable_Source()
        {
            yield return new TestCaseData("Noncached", new EntityAccessControlChecker());
            yield return new TestCaseData("Cached", ((EntityAccessControlService)Factory.EntityAccessControlService).EntityAccessControlChecker);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_EnableCreateRule()
        {
            UserAccount userAccount;
            EntityType entityType1;
            AccessRule accessRule;
            IEntityAccessControlService entityAccessControlService;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                userAccount = new UserAccount();
                userAccount.Name = Guid.NewGuid().ToString();
                userAccount.Save();

                entityType1 = new EntityType();
                entityType1.Inherits.Add(UserResource.UserResource_Type);
                entityType1.Save();

                entityAccessControlService = Factory.EntityAccessControlService;
                ctx.CommitTransaction();
            }

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.CanCreate(entityType1), Is.False,
                    "User can somehow initially create entities (!?)");
            }

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                accessRule = new AccessRuleFactory().AddAllowCreate(
                    userAccount.As<Subject>(),
                    entityType1.As<SecurableEntity>());
                ctx.CommitTransaction();
            }

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.CanCreate(entityType1), Is.True,
                    "User cannot initially create entities");
            }

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule.AccessRuleEnabled = false;
                accessRule.Save();
                ctx.CommitTransaction();
            }

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.CanCreate(entityType1), Is.False,
                    "User can create entities afterwards (!?)");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ChangePermissionsOnAccessRule()
        {
            UserAccount userAccount;
            EntityType entityType;
            IEntity entity;
            EntityRef entityRef;
            AccessRule accessRule;
            IEntityAccessControlService entityAccessControlService;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                userAccount = new UserAccount();
                userAccount.Name = Guid.NewGuid().ToString();
                userAccount.Save();

                entityType = new EntityType();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entity = Entity.Create(entityType);
                entity.Save();
                entityRef = new EntityRef(entity);

                entityAccessControlService = Factory.EntityAccessControlService;

                ctx.CommitTransaction();
            }

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Read }), Is.False,
                    "User can somehow initially read entities (!?)");
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Modify }), Is.False,
                    "User can somehow initially write entities (!?)");
            }

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                accessRule = new AccessRuleFactory().AddAllowByQuery(
                    userAccount.As<Subject>(),
                    entityType.As<SecurableEntity>(),
                    new[] { Permissions.Read, Permissions.Modify },
                    TestQueries.Entities(entityType).ToReport());
                ctx.CommitTransaction();
            }

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Read }), Is.True,
                    "User cannot read entity after access rule creation");
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Modify }), Is.True,
                    "User cannot modify entity after access rule creation");
            }

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule.PermissionAccess.Remove(Permissions.Modify.Entity.As<Permission>());
                accessRule.Save();
                ctx.CommitTransaction();
            }

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Read }), Is.True,
                    "User cannot read entity after removing modify access");
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Modify }), Is.False,
                    "User can modify entity after removing modify access");
            }

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule.PermissionAccess.Remove(Permissions.Read.Entity.As<Permission>());
                accessRule.Save();
                ctx.CommitTransaction();
            }

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Read }), Is.False,
                    "User can read entity after removing read access");
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Modify }), Is.False,
                    "User can modify entity after removing read access");
            }

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                accessRule.PermissionAccess.Add(Permissions.Read.Entity.As<Permission>());
                accessRule.Save();
                ctx.CommitTransaction();
            }
            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Read }), Is.True,
                    "User cannot read entity after re-adding read access");
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Modify }), Is.False,
                    "User can modify entity after re-adding read access");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_ChangeAccessRuleForPermissions()
        {
            UserAccount userAccount;
            EntityType entityType;
            IEntity entity;
            EntityRef entityRef;
            AccessRule accessRule;
            IEntityAccessControlService entityAccessControlService;
            Permission readPermission;
            Permission modifyPermission;

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                userAccount = new UserAccount();
                userAccount.Name = Guid.NewGuid().ToString();
                userAccount.Save();

                entityType = new EntityType();
                entityType.Inherits.Add(UserResource.UserResource_Type);
                entityType.Save();

                entity = Entity.Create(entityType);
                entity.Save();
                entityRef = new EntityRef(entity);

                entityAccessControlService = Factory.EntityAccessControlService;

                ctx.CommitTransaction();
            }

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Read }), Is.False,
                    "User can somehow initially read entities (!?)");
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Modify }), Is.False,
                    "User can somehow initially write entities (!?)");
            }


            using ( var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            using (new SecurityBypassContext())
            {
                accessRule = new AccessRuleFactory().AddAllowByQuery(
                userAccount.As<Subject>(),
                entityType.As<SecurableEntity>(),
                new[] { Permissions.Read, Permissions.Modify },
                TestQueries.Entities(entityType).ToReport());
                ctx.CommitTransaction();

            }

            readPermission = Entity.Get<Permission>(Permissions.Read, true);
            modifyPermission = Entity.Get<Permission>(Permissions.Modify, true);

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Read }), Is.True,
                    "User cannot read entity after access rule creation");
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Modify }), Is.True,
                    "User cannot modify entity after access rule creation");
            }

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                modifyPermission.PermissionAccessBy.Remove(accessRule);
                modifyPermission.Save();
                ctx.CommitTransaction();
            }

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Read }), Is.True,
                    "User cannot read entity after removing modify access");
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Modify }), Is.False,
                    "User can modify entity after removing modify access");
            }

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                readPermission.PermissionAccessBy.Remove(accessRule);
                readPermission.Save();
                ctx.CommitTransaction();
            }

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Read }), Is.False,
                    "User can read entity after removing read access");
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Modify }), Is.False,
                    "User can modify entity after removing read access");
            }

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                readPermission.PermissionAccessBy.Add(accessRule);
                readPermission.Save();
                ctx.CommitTransaction();
            }

            using (new SetUser(userAccount))
            {
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Read }), Is.True,
                    "User cannot read entity after re-adding read access");
                Assert.That(entityAccessControlService.Check(entityRef, new[] { Permissions.Modify }), Is.False,
                    "User can modify entity after re-adding read access");
            }
        }

        [TestCase( "core:everyoneRole" )]
        [TestCase( "core:administratorRole" )]
        [RunAsDefaultTenant]
        public void Test_SystemRoleCannotBeDeleted( string roleAlias )
        {
            UserAccount userAccount;

            using ( new SecurityBypassContext( ) )
            {
                userAccount = Entity.Get<UserAccount>( "core:administratorUserAccount" );
            }

            using ( new SetUser( userAccount ) )
            {
                Assert.That( Factory.EntityAccessControlService.Check( new EntityRef( roleAlias ), new [ ] { Permissions.Delete } ), Is.False,
                        "User cannot delete system role" );
            }

        }
    }
}