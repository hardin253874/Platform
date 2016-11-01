// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Metadata.Query.Structured.Entity;
using IdExpression = EDC.ReadiNow.Metadata.Query.Structured.IdExpression;
using ScriptExpression = EDC.ReadiNow.Metadata.Query.Structured.ScriptExpression;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    /// <summary>
    /// General class for entity access control bugs.
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class EntityAccessControlTests
    {
        /// <summary>
        /// A test for Bug #22309 (http://sptfs01.sp.local:8080/tfs/web/wi.aspx?pcguid=01c396f1-8bb4-4766-9c37-a25c59b63ee4&id=22309).
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void Test_LoginFailureWithAccessRuleContainingCalculatedColumn()
        {
            StructuredQuery securityStructuredQuery;
            UserAccount userAccount;
            Guid entitiesNodeGuid;
            Guid selectColumnGuid;

            // Create the query containing a calculated column
            entitiesNodeGuid = Guid.NewGuid();
            selectColumnGuid = Guid.NewGuid();
            securityStructuredQuery = new StructuredQuery
            {
                RootEntity = new ResourceEntity
                {
                    NodeId = entitiesNodeGuid,
                    ExactType = false,
                    EntityTypeId = new EntityRef("core:resource")
                },
                SelectColumns = new List<SelectColumn>()
            };
            securityStructuredQuery.Conditions.Add(new QueryCondition()
            {
                Expression = new ColumnReference()
                {
                    ColumnId = selectColumnGuid
                },
                Operator = ConditionType.Equal,
                Argument = new TypedValue("allow")
            });
            securityStructuredQuery.SelectColumns.Add(new SelectColumn()
            {
                Expression = new IdExpression()
                {
                    NodeId = entitiesNodeGuid
                },
                ColumnName = "Id",
                DisplayName = "Id"
            });
            securityStructuredQuery.SelectColumns.Add(new SelectColumn()
            {
                ColumnId = selectColumnGuid,
                Expression = new ScriptExpression()
                {
                    Script = "iif(description = 'foobar', 'deny', 'allow')",
                    NodeId = entitiesNodeGuid
                },
                ColumnName = "Access",
                DisplayName = "Access"
            });

            // Create test user and grant the user access to everything 
            // returned by the above structured query.
            userAccount = new UserAccount();
            userAccount.Name = "Test User " + Guid.NewGuid();
            userAccount.Save();
            new AccessRuleFactory().AddAllowByQuery(
                userAccount.As<Subject>(),
                ReadiNow.Model.Entity.Get<SecurableEntity>("core:resource"),
                new[] { Permissions.Read, Permissions.Modify, Permissions.Delete, Permissions.Create },
                securityStructuredQuery.ToReport());

            Factory.EntityAccessControlService.ClearCaches();

            using (new SetUser(userAccount))
            {                
                Assert.That(
                    () => ReadiNow.Model.Entity.GetByField<ReadiNowIdentityProvider>("ReadiNow", ReadiNowIdentityProvider.Name_Field).FirstOrDefault(),
                    Is.Not.Null);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void GetMatchesWithSecurityBypassContext()
        {
            UserAccount userAccount;

            userAccount = new UserAccount
            {
                Name = "Test user " + Guid.NewGuid()
            };
            userAccount.Save();

            using (new SecurityBypassContext())
            using (new SetUser(userAccount))
            {                
                Assert.That(
                    () => ReadiNow.Model.Entity.GetByField<ReadiNowIdentityProvider>("ReadiNow", ReadiNowIdentityProvider.Name_Field).FirstOrDefault(),
                    Is.Not.Null);
            }
        }

        // Basic security checks (useful for profiling)
        //[Test]
        //[RunAsDefaultTenant]
        //[RunWithTransaction]
        //public void Test_AccessCheck_AsNewUser()
        //{
        //    UserAccount userAccount;

        //    userAccount = new UserAccount();
        //    userAccount.Name = "Test User " + Guid.NewGuid();
        //    userAccount.Save();

        //    new AccessRuleFactory().AddAllowByQuery(
        //        userAccount.As<Subject>(),
        //        ReadiNow.Model.Entity.Get<SecurableEntity>("core:resource"),
        //        new[] { Permissions.Read, Permissions.Modify, Permissions.DeletePermission, Permissions.CreatePermission },
        //        TestQueries.Entities().ToReport());

        //    new EntityAccessControlFactory().ClearCaches();

        //    new EntityAccessControlChecker().CheckAccess(
        //        new [] { new EntityRef(userAccount) }, 
        //        new [] { Permissions.Read },
        //        userAccount);
        //}

        //[Test]
        //[RunAsDefaultTenant]
        //[RunWithTransaction]
        //public void Test_AccessCheck_AsAdmin()
        //{
        //    EntityRef runAsUser;
        //    EntityRef testEntity;

        //    runAsUser = new EntityRef("core:administratorUserAccount");
        //    testEntity = new EntityRef("core:folder");

        //    BulkPreloader.TenantWarmup();
        //    new EntityAccessControlFactory().ClearCaches();

        //    new EntityAccessControlChecker().CheckAccess(
        //        new[] { testEntity },
        //        new[] { Permissions.Read },
        //        runAsUser);
        //}
    }
}
