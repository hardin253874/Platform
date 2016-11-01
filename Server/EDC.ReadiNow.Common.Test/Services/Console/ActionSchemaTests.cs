// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Services.Console
{
    [TestFixture]
	[RunWithTransaction]
    public class ActionSchemaTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_ActionRequiresPermission_Exists()
        {
            Assert.That(Entity.Get<EntityType>("console:actionMenuItem").Relationships,
                Has.Exactly(1).Property("Alias").EqualTo("console:actionRequiresPermission")
                          .And.Property("ToType").Property("Alias").EqualTo("core:permission")
                          .And.Property("RelType").Property("Alias").EqualTo("core:relManyToMany"));
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("console:viewResourceAction", new string[0])]
        [TestCase("console:editResourceAction", new[] { "core:modify" })]
        [TestCase("console:deleteResourceAction", new[] { "core:delete" })]
        [TestCase("console:viewInstancesOfTypeAction", new string[0])]
        [TestCase("console:openFolderAction", new string[0])]
        [TestCase("console:openScreenAction", new string[0])]
        [TestCase("console:openReportAction", new string[0])]
        [TestCase("console:exportExcelAction", new string[0])]
        [TestCase("console:exportCsvAction", new string[0])]
        [TestCase("console:exportWordAction", new string[0])]
        [TestCase("console:addRelationshipAction", new string[0])]
        [TestCase("console:removeRelationshipAction", new string[0])]
        [TestCase("console:createWorkflowInDesignerAction", new[] { "core:create" })]
        [TestCase("console:editWorkflowInDesignerAction", new[] { "core:modify" })]
        [TestCase("console:runWorkflowAction", new string[0])]
        [TestCase("console:viewUserTaskAction", new string[0])]
        [TestCase("console:exportSolutionAction", new string[0])]
        [TestCase("console:publishSolutionAction", new string[0])]
        [TestCase("console:installSolutionAction", new string[0])]
        [TestCase("console:upgradeSolutionAction", new string[0])]
        [TestCase("console:repairSolutionAction", new string[0])]
        [TestCase("console:uninstallSolutionAction", new string[0])]
        public void Test_ActionRequiresPermission_Exists(string actionItemAlias, string[] expectedRequiredPermissionAliases)
        {
            Assert.That(Entity.Get<ActionMenuItem>(actionItemAlias), Is.Not.Null);
            Assert.That(Entity.Get<ActionMenuItem>(actionItemAlias).ActionRequiresPermission.Select(a => a.Alias),
                Is.EquivalentTo(expectedRequiredPermissionAliases));
        }
    }
}
