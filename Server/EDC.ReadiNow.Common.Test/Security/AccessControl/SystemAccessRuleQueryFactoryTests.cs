// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.Database.Types;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
    public class SystemAccessRuleQueryFactoryTests
    {
        [Test]
        public void GetQueriesNoRequestContextSet()
        {
            var factory = new SystemAccessRuleQueryFactory();

            Assert.That(() =>
                factory.GetQueries(),
                Throws.TypeOf<InvalidOperationException>().And.Message.EqualTo("RequestContext not set."));
        }

        [Test]
        [RunAsDefaultTenant]
        public void GetQueriesAsDefaultTenant()
        {            
            var factory = new SystemAccessRuleQueryFactory();
            var queries = factory.GetQueries();

            Assert.IsNotNull(queries, "The system access queries should not be null.");
            Assert.AreNotEqual(0, queries.Count, "The count of system access queries should not be 0.");

            using (new SecurityBypassContext())
            {
                // Verify the system rules
                foreach (var kvp in queries)
                {
                    var subjectPermission = kvp.Key;
                    var accessRuleQueries = kvp.Value;

                    Assert.IsNotNull(Entity.Get<Subject>(subjectPermission.SubjectId), "The subject does not exist.");
                    Assert.IsNotNull(Entity.Get<Permission>(subjectPermission.PermissionId), "The permission does not exist.");

                    foreach (var accessRuleQuery in accessRuleQueries)
                    {
                        Assert.IsNotNull(Entity.Get<EntityType>(accessRuleQuery.ControlsAccessForTypeId), "The entity types does not exist.");
                        Assert.IsNotNull(accessRuleQuery.Query, "The query should not be null.");

                        QueryResult result = Factory.QueryRunner.ExecuteQuery(accessRuleQuery.Query, new QuerySettings() {SecureQuery = false});
                        Assert.AreNotEqual(0, result.Columns.Count, "The number of columns is invalid");
                        Assert.IsTrue(result.Columns[0].ColumnType is IdentifierType, "The column type is invalid.");
                    }
                }
            }
            
        }
    }
}