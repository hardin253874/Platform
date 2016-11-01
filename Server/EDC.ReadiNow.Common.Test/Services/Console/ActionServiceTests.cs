// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Services.Console;
using EDC.ReadiNow.Test.Security.AccessControl;
using Moq;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Services.Console
{
	[TestFixture]
	[RunWithTransaction]
    public class ActionServiceTests
    {
        [Test]
        [RunAsDefaultTenant]
        [TestCase("")]
        [TestCase("read")]
        [TestCase("modify")]
        [TestCase("delete")]
        [TestCase("read,modify")]
        [TestCase("read,delete")]
        [TestCase("modify,delete")]
        [TestCase("read,modify,delete")]
        public void Test_GetActions_Security_ReadModifyDelete(string permissionAliases)
        {
            ActionRequestExtended actionRequest;
            ActionResponse response;
            Report peopleReport;
            UserAccount userAccount;
            Person person;
            ActionMenuItem viewActionMenuItem;
            ActionMenuItem editActionMenuItem;
            ActionMenuItem deleteActionMenuItem;
            IList<ActionMenuItemInfo> flattenedResults;
            string[] splitPermissionAliases;

            splitPermissionAliases = permissionAliases.Split(',');

            userAccount = Entity.Create<UserAccount>();
            userAccount.Save();

            person = Entity.Create<Person>();
            person.Save();

            viewActionMenuItem = Entity.Get<ActionMenuItem>("console:viewResourceAction");
            Assert.That(viewActionMenuItem, Is.Not.Null, "No view menu item");
            editActionMenuItem = Entity.Get<ActionMenuItem>("console:editResourceAction");
            Assert.That(editActionMenuItem, Is.Not.Null, "No edit menu item");
            deleteActionMenuItem = Entity.Get<ActionMenuItem>("console:deleteResourceAction");
            Assert.That(deleteActionMenuItem, Is.Not.Null, "No delete menu item");

            if (!string.IsNullOrWhiteSpace(permissionAliases))
            {
                new AccessRuleFactory().AddAllowByQuery(
                    userAccount.As<Subject>(),
                    Entity.Get<SecurableEntity>(Entity.Get<EntityType>("core:person")),
                    splitPermissionAliases.Select(pa => new EntityRef(pa)),
                    TestQueries.Entities().ToReport());
            }

            peopleReport = Entity.GetByName<Report>("People").FirstOrDefault();
            Assert.That(peopleReport, Is.Not.Null, "No People report");

			actionRequest = new ActionRequestExtended
            {
                SelectedResourceIds = new[] { person.Id },
                LastSelectedResourceId = person.Id,
                CellSelectedResourceId = -1,
                ReportId = peopleReport.Id,
                HostResourceIds = new long[0],
                HostTypeIds = new List<long>(),
                AdditionalData = new Dictionary<string, object>(),
                ActionDisplayContext = ActionContext.ActionsMenu
            };

            using (new SetUser(userAccount))
            {
                response = new ActionService().GetActions(actionRequest);
            }

            flattenedResults = response.Actions.SelectMany(Flatten).ToList();

            Assert.That(flattenedResults,
                Has.Exactly(splitPermissionAliases.Contains("read") ? 1 : 0).Property("Id").EqualTo(viewActionMenuItem.Id),
                "View menu incorrect");
            Assert.That(flattenedResults,
                Has.Exactly(splitPermissionAliases.Contains("read") && splitPermissionAliases.Contains("modify") ? 1 : 0).Property("Id").EqualTo(editActionMenuItem.Id),
                "Edit menu incorrect");
            Assert.That(flattenedResults,
                Has.Exactly(splitPermissionAliases.Contains("read") && splitPermissionAliases.Contains("delete") ? 1 : 0).Property("Id").EqualTo(deleteActionMenuItem.Id),
                "Delete menu incorrect");
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("AA_Employee", "")]
        [TestCase("AA_Employee", "AA_Employee")]
        [TestCase("AA_Employee", "AA_Manager")]
        [TestCase("AA_Employee", "AA_Employee,AA_Manager")]
        public void Test_GetActions_Security_Create(string reportName, string typeNames)
        {
			ActionRequestExtended actionRequest;
            ActionResponse response;
            Report report;
            UserAccount userAccount;
            IList<ActionMenuItemInfo> flattenedResults;
            string[] splitTypeNames;
            SecurableEntity securableEntity;

            splitTypeNames = typeNames.Split(',');

            userAccount = Entity.Create<UserAccount>();
            userAccount.Name = "Action service test " + Guid.NewGuid( ).ToString( );
            userAccount.Save();

            new AccessRuleFactory( ).AddAllowReadQuery(
                userAccount.As<Subject>( ),
                Entity.Get<SecurableEntity>( Entity.Get<EntityType>( "core:report" ) ),
                TestQueries.Entities( new EntityRef( "core:report" ) ).ToReport( ) );

            if (!string.IsNullOrWhiteSpace(typeNames))
            {
                foreach (string typeName in splitTypeNames)
                {
                    securableEntity = Entity.GetByName<SecurableEntity>(typeName).FirstOrDefault();
                    Assert.That(securableEntity, Is.Not.Null,
                        string.Format("{0} is not a type", typeName));

                    new AccessRuleFactory().AddAllowCreate(
                        userAccount.As<Subject>(),
                        securableEntity);
                }
            }

            report = Entity.GetByName<Report>(reportName).FirstOrDefault();
            Assert.That(report, Is.Not.Null, string.Format("{0} is not a report", reportName));

			actionRequest = new ActionRequestExtended
            {
                SelectedResourceIds = new long[0],
                LastSelectedResourceId = 0,
                CellSelectedResourceId = -1,
                ReportId = report.Id,
                HostResourceIds = new long[0],
                HostTypeIds = new List<long>(),
                AdditionalData = new Dictionary<string, object>(),
                ActionDisplayContext = ActionContext.ActionsMenu
            };

            using (new SetUser(userAccount))
            {
                response = new ActionService().GetActions(actionRequest);
            }

            flattenedResults = response.Actions.SelectMany(Flatten).ToList();

            if (!string.IsNullOrWhiteSpace(typeNames))
            {
                foreach (string typeName in splitTypeNames)
                {
                    Assert.That(flattenedResults,
                        Has.Exactly(1).Property("HtmlActionState").EqualTo(ActionService.CreateMenuItemActionState)
                            .And.Property("Name").EqualTo(typeName));
                }
            }
            else
            {
                Assert.That(flattenedResults,
                    Has.Exactly(0).Property("HtmlActionState").EqualTo(ActionService.CreateMenuItemActionState));
            }
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("A", "A", true)]
        [TestCase("B", "A", false)]
        [TestCase("A", "B", false)]
        public void Test_GetActions_Security_MultipleSelection(string entity1Name, string entity2Name, bool expectedResult)
        {
			ActionRequestExtended actionRequest;
            ActionResponse response;
            Report report;
            UserAccount userAccount;
            ActionMenuItem deleteActionMenuItem;
            IList<ActionMenuItemInfo> flattenedResults;
            EntityType entityType;
            IEntity entity1;
            IEntity entity2;

            entityType = Entity.Create<EntityType>();
            entityType.Inherits.Add(UserResource.UserResource_Type);
            entityType.Save();

            entity1 = Entity.Create(entityType);
            entity1.SetField("core:name", entity1Name);
            entity1.Save();

            entity2 = Entity.Create(entityType);
            entity2.SetField("core:name", entity2Name);
            entity2.Save();

            userAccount = Entity.Create<UserAccount>();
            userAccount.Name = "Action service test " + Guid.NewGuid( ).ToString( );
            userAccount.Save();

            new AccessRuleFactory( ).AddAllowReadQuery(
                userAccount.As<Subject>( ),
                Entity.Get<SecurableEntity>( Entity.Get<EntityType>( "core:report" ) ),
                TestQueries.Entities( new EntityRef( "core:report" ) ).ToReport( ) );

            deleteActionMenuItem = Entity.Get<ActionMenuItem>("console:deleteResourceAction");
            Assert.That(deleteActionMenuItem, Is.Not.Null, "No delete menu item");

            new AccessRuleFactory().AddAllowByQuery(
                userAccount.As<Subject>(),
                entityType.As<SecurableEntity>(),
                new [] { Permissions.Read, Permissions.Delete },
                TestQueries.EntitiesWithNameA(entityType).ToReport());

            report = TestQueries.EntitiesWithNameA(entityType).ToReport();
            report.Save();

			actionRequest = new ActionRequestExtended
            {
                SelectedResourceIds = new[] { entity1.Id, entity2.Id },
                LastSelectedResourceId = entity1.Id,
                CellSelectedResourceId = -1,
                ReportId = report.Id,
                HostResourceIds = new long[0],
                HostTypeIds = new List<long>(),
                AdditionalData = new Dictionary<string, object>(),
                ActionDisplayContext = ActionContext.ContextMenu
            };

            using (new SetUser(userAccount))
            {
                response = new ActionService().GetActions(actionRequest);
            }

            flattenedResults = response.Actions.SelectMany(Flatten).ToList();

            Assert.That(flattenedResults,
                Has.Exactly(expectedResult ? 1 : 0).Property("Id").EqualTo(deleteActionMenuItem.Id),
                "Delete menu incorrect");
        }



        private IEnumerable<ActionMenuItemInfo> Flatten(ActionMenuItemInfo actionMenuItem)
        {
            if (actionMenuItem == null)
            {
                throw new ArgumentNullException("actionMenuItem");
            }

            return new[] { actionMenuItem }.Concat(actionMenuItem.Children != null ? actionMenuItem.Children.SelectMany(Flatten) : Enumerable.Empty<ActionMenuItemInfo>());
        }
    }
}
