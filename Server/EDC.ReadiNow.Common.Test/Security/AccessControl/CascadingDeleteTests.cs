// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class CascadingDeleteTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_CascadingDeleteOfAccessRules()
        {
            Folder folder;
            Report report;
            AccessRule accessRule;
            SecurableEntity resourceType;
            Permission readPermission;
            Subject subject;

            using (DatabaseContext.GetContext(true))
            {
                folder = new Folder();
                folder.Save();

                report = Entity.Create<Report>();
                report.SecurityReportInFolder = folder;
                report.Save();

                resourceType = Entity.Get<SecurableEntity>(new EntityRef("core:resource"));
                Assert.That(resourceType, Is.Not.Null, "Resource type not loaded");

                readPermission = Entity.Get<Permission>(new EntityRef("core:read"));
                Assert.That(readPermission, Is.Not.Null, "Read permission not loaded");

                subject = Entity.Create<Subject>();
                subject.Name = "Test Subject";
                subject.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.Name = "Test Access Rule";
                accessRule.AllowAccessBy = subject;
                accessRule.AccessRuleReport = report;
                accessRule.ControlAccess = resourceType;
                accessRule.PermissionAccess.Add(readPermission);
                accessRule.Save();

                Assert.That(Entity.Exists(accessRule), Is.True, "Access rule existential crisis!");
                Assert.That(Entity.Exists(resourceType), Is.True, "Resource type existential crisis!");
                Assert.That(Entity.Exists(readPermission), Is.True, "Read permission existential crisis!");
                Assert.That(Entity.Exists(subject), Is.True, "Subject existential crisis!");
                Assert.That(Entity.Exists(report), Is.True, "Report existential crisis!");

                Entity.Delete(accessRule);

                Assert.That(!Entity.Exists(accessRule), Is.True, "Access rule still exists after delete");
                Assert.That(Entity.Exists(resourceType), Is.True, "Resource type does not exist after delete");
                Assert.That(Entity.Exists(readPermission), Is.True, "Read permission type does not exist after delete");
                Assert.That(Entity.Exists(subject), Is.True, "Subject does not exist after delete");
                Assert.That(!Entity.Exists(report), Is.True, "Report still exists after delete");
                Assert.That(!Entity.Exists(folder), Is.True, "Folder still exists after delete");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CascadingDeleteOfReport()
        {
            Folder folder;
            Report report;
            AccessRule accessRule;
            SecurableEntity resourceType;
            Permission readPermission;
            Subject subject;

            using (DatabaseContext.GetContext(true))
            {
                folder = new Folder();
                folder.Save();

                report = Entity.Create<Report>();
                report.SecurityReportInFolder = folder;
                report.Save();

                resourceType = Entity.Get<SecurableEntity>(new EntityRef("core:resource"));
                Assert.That(resourceType, Is.Not.Null, "Resource type not loaded");

                readPermission = Entity.Get<Permission>(new EntityRef("core:read"));
                Assert.That(readPermission, Is.Not.Null, "Read permission not loaded");

                subject = Entity.Create<Subject>();
                subject.Name = "Test Subject";
                subject.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.Name = "Test Access Rule";
                accessRule.AllowAccessBy = subject;
                accessRule.AccessRuleReport = report;
                accessRule.ControlAccess = resourceType;
                accessRule.PermissionAccess.Add(readPermission);
                accessRule.Save();

                Assert.That(Entity.Exists(report), Is.True, "Report exists");

                Entity.Delete(report);

                Assert.That(!Entity.Exists(accessRule), Is.True, "Access rule still exists after delete");
                Assert.That(Entity.Exists(resourceType), Is.True, "Resource type does not exist after delete");
                Assert.That(Entity.Exists(readPermission), Is.True, "Read permission type does not exist after delete");
                Assert.That(Entity.Exists(subject), Is.True, "Subject does not exist after delete");
                Assert.That(!Entity.Exists(report), Is.True, "Report still exists after delete");
                Assert.That(!Entity.Exists(folder), Is.True, "Folder still exists after delete");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CascadingDeleteOfSubject()
        {
            Folder folder;
            Report report;
            AccessRule accessRule;
            SecurableEntity resourceType;
            Permission readPermission;
            Subject subject;

            using (DatabaseContext.GetContext(true))
            {
                folder = new Folder();
                folder.Save();

                report = Entity.Create<Report>();
                report.SecurityReportInFolder = folder;
                report.Save();

                resourceType = Entity.Get<SecurableEntity>(new EntityRef("core:resource"));
                Assert.That(resourceType, Is.Not.Null, "Resource type not loaded");

                readPermission = Entity.Get<Permission>(new EntityRef("core:read"));
                Assert.That(readPermission, Is.Not.Null, "Read permission not loaded");

                subject = Entity.Create<Subject>();
                subject.Name = "Test Subject";
                subject.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.Name = "Test Access Rule";
                accessRule.AllowAccessBy = subject;
                accessRule.AccessRuleReport = report;
                accessRule.ControlAccess = resourceType;
                accessRule.PermissionAccess.Add(readPermission);
                accessRule.Save();

                Assert.That(Entity.Exists(subject), Is.True, "Report exists");

                Entity.Delete(subject);

                Assert.That(!Entity.Exists(accessRule), Is.True, "Access rule still exists after delete");
                Assert.That(Entity.Exists(resourceType), Is.True, "Resource type does not exist after delete");
                Assert.That(Entity.Exists(readPermission), Is.True, "Read permission type does not exist after delete");
                Assert.That(!Entity.Exists(subject), Is.True, "Subject still exists after delete");
                Assert.That(!Entity.Exists(report), Is.True, "Report still exists after delete");
                Assert.That(!Entity.Exists(folder), Is.True, "Folder still exists after delete");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_CascadingDeleteOfSecurableEntity()
        {
            Folder folder;
            Report report;
            AccessRule accessRule;
            SecurableEntity securableEntity;
            Permission readPermission;
            Subject subject;

            using (DatabaseContext.GetContext(true))
            {
                folder = new Folder();
                folder.Save();

                report = Entity.Create<Report>();
                report.SecurityReportInFolder = folder;
                report.Save();

                securableEntity = Entity.Create<SecurableEntity>();
                securableEntity.Name = "Test Securable Entity";
                securableEntity.Save();

                readPermission = Entity.Get<Permission>(new EntityRef("core:read"));
                Assert.That(readPermission, Is.Not.Null, "Read permission not loaded");

                subject = Entity.Create<Subject>();
                subject.Name = "Test Subject";
                subject.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.Name = "Test Access Rule";
                accessRule.AllowAccessBy = subject;
                accessRule.AccessRuleReport = report;
                accessRule.ControlAccess = securableEntity;
                accessRule.PermissionAccess.Add(readPermission);
                accessRule.Save();

                Assert.That(Entity.Exists(securableEntity), Is.True, "Report exists");

                Entity.Delete(securableEntity);

                Assert.That(!Entity.Exists(accessRule), Is.True, "Access rule still exists after delete");
                Assert.That(!Entity.Exists(securableEntity), Is.True, "Securable entity still exists after delete");
                Assert.That(Entity.Exists(readPermission), Is.True, "Read permission type does not exist after delete");
                Assert.That(Entity.Exists(subject), Is.True, "Subject does not exist after delete");
                Assert.That(!Entity.Exists(report), Is.True, "Report still exists after delete");
                Assert.That(!Entity.Exists(folder), Is.True, "Folder still exists after delete");
            }
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test_CascadingDeleteOfPermission()
        {
            Folder folder;
            Report report;
            AccessRule accessRule;
            SecurableEntity securableEntity;
            Permission permission;
            Subject subject;

            using (DatabaseContext.GetContext(true))
            {
                folder = new Folder();
                folder.Save();

                report = Entity.Create<Report>();
                report.SecurityReportInFolder = folder;
                report.Save();

                report = Entity.Create<Report>();
                report.Save();

                securableEntity = Entity.Create<SecurableEntity>();
                securableEntity.Name = "Test Securable Entity";
                securableEntity.Save();

                permission = Entity.Create<Permission>();
                permission.Name = "Test Permission";
                permission.Save();

                subject = Entity.Create<Subject>();
                subject.Name = "Test Subject";
                subject.Save();

                accessRule = Entity.Create<AccessRule>();
                accessRule.Name = "Test Access Rule";
                accessRule.AllowAccessBy = subject;
                accessRule.AccessRuleReport = report;
                accessRule.ControlAccess = securableEntity;
                accessRule.PermissionAccess.Add(permission);
                accessRule.Save();

                Assert.That(Entity.Exists(permission), Is.True, "Permission exists");

                Entity.Delete(permission);

                Assert.That(Entity.Exists(accessRule), Is.True, "Access rule does not exist after delete");
                Assert.That(Entity.Exists(securableEntity), Is.True, "Securable entity does not exist after delete");
                Assert.That(!Entity.Exists(permission), Is.True, "Permission still exists after delete");
                Assert.That(Entity.Exists(subject), Is.True, "Subject does not exist after delete");
                Assert.That(Entity.Exists(report), Is.True, "Report does not exist after delete");
                Assert.That(Entity.Exists(folder), Is.True, "Folder still exists after delete");
            }
        }
    }
}
