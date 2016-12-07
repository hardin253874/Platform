// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Services.Console;
using EDC.ReadiNow.Test.Security.AccessControl;
using FluentAssertions;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;

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

        /// <summary>
        /// Basic test for all actions with default settings for reports at various levels of a
        /// type/definition hierarchy.
        /// </summary>
        /// <param name="reportKey">The test report key.</param>
        /// <param name="selectName">The expected selection text in the actions.</param>
        /// <param name="expectedActionKeys">The expected action keys.</param>
        [Test]
        [RunAsDefaultTenant]
        [TestCase("unitReport", "Loose Unit", new[]
        {
            "New (All)", "Export (All)", "Edit Inline", "Excel", "CSV", "Word",
            "Loose Unit", "Parent", "Grandparent", "View 'Loose Unit'", "Edit 'Loose Unit'",
            "Delete 'Loose Unit'", "Assign Data to App", "Launch Person Campaign", "Stop", "Some Unit"
        })]
        [TestCase("parentReport", "Parent", new[]
        {
            "New (All)", "Export (All)", "Edit Inline", "Excel", "CSV", "Word",
            "Parent", "Grandparent", "View 'Parent'", "Edit 'Parent'",
            "Delete 'Parent'", "Assign Data to App", "Launch Person Campaign", "Stop", "Look", "Some Unit", "Muppet"
        })]
        [TestCase("grandParentReport", "Grandparent", new[]
        {
            "New (All)", "Export (All)", "Edit Inline", "Excel", "CSV", "Word",
            "Grandparent", "View 'Grandparent'", "Edit 'Grandparent'", "Delete 'Grandparent'",
            "Assign Data to App", "Launch Person Campaign", "Stop", "Look", "Listen", "Some Unit", "Muppet", "Some Elder"
        })]
        public void Test_GetActions(string reportKey, string selectName, string[] expectedActionKeys)
        {
            ActionResponse response;

            var testObject = SetupGrandDesign();
            var request = new ActionRequestExtended(new ActionRequest
            {
                ReportId = testObject.Reports[reportKey].Id,
                ActionDisplayContext = ActionContext.All
            });

            var svc = new ActionService();
            svc.FlushCaches();

            using (new SetUser(testObject.Accounts["administrator"]))
            {
                response = svc.GetActions(request);
            }

            response.Should().NotBeNull();

            var expectedActions = testObject.Actions
                .Where(a => expectedActionKeys.Contains(a.Key))
                .Select(a => a.Value)
                .Union(testObject.SelectActions(selectName)
                .Where(s => expectedActionKeys.Contains(s.Name))).ToList();

            var actions = response.Actions.Where(a => !a.Name.StartsWith("Debug '")).ToList();

            actions.ShouldAllBeEquivalentTo(expectedActions, options => options
            .Excluding(o => o.Id)
            .Excluding(o => o.EntityId)
            .Excluding(o => o.Alias)
            .Excluding(o => o.AdditionalData)
            .Excluding(o => o.RequiresPermissions)
            .Excluding(o => o.RequiresParentPermissions));
        }

        private static GrandDesign SetupGrandDesign()
	    {
            var testObject = new GrandDesign();
            CreateTestTypesAndInstances(ref testObject);
	        CreateUsersAndSecurity(ref testObject);
	        CreateReportsAndFormsEtc(ref testObject);
            return testObject;
	    }

	    private static void CreateTestTypesAndInstances(ref GrandDesign testObject)
	    {
            //
            // Fields
            //
	        var unitFieldGroup = Entity.Create<FieldGroup>();
	        unitFieldGroup.Name = "Unit Test";

            var isHappyField = Entity.Create<BoolField>();
	        isHappyField.Alias = "test:isHappy";
            isHappyField.Name = "Happy?";
	        isHappyField.FieldInGroup = unitFieldGroup;

	        var birthdayField = Entity.Create<DateField>();
	        birthdayField.Alias = "test:birthday";
	        birthdayField.Name = "Birthday";
	        birthdayField.FieldInGroup = unitFieldGroup;

            //
            // Types
            //
            var unit = Entity.Create<Definition>().As<EntityType>();
	        unit.Alias = "test:unit";
            unit.Name = "Loose Unit";
            unit.Inherits.Add(UserResource.UserResource_Type);
            unit.FieldGroups.Add(unitFieldGroup);
            unit.Fields.Add(isHappyField.As<Field>());
            unit.Fields.Add(birthdayField.As<Field>());
            unit.Save();

            var parent = Entity.Create<Definition>().As<EntityType>();
            parent.Alias = "test:parent";
            parent.Name = "Parent";
            parent.Inherits.Add(unit);
            parent.Save();

            var grandParent = Entity.Create<Definition>().As<EntityType>();
            grandParent.Alias = "test:grandParent";
            grandParent.Name = "Grandparent";
            grandParent.Inherits.Add(parent);
            grandParent.Save();

            //
            // Relationships
            //
            var kidsRel = Entity.Create<Relationship>();
            kidsRel.Name = "Kids";
            kidsRel.FromType = parent;
            kidsRel.FromName = "Parental";
            kidsRel.ToType = unit;
            kidsRel.ToName = "Tin Lid";
            kidsRel.RelType_Enum = RelTypeEnum_Enumeration.RelManyToMany;
            kidsRel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
	        kidsRel.RelationshipInToTypeGroup = unitFieldGroup;
	        kidsRel.RelationshipInFromTypeGroup = unitFieldGroup;
            kidsRel.Save();

            var grandKidsRel = Entity.Create<Relationship>();
            grandKidsRel.Name = "Grandchildren";
            grandKidsRel.FromType = grandParent;
            grandKidsRel.FromName = "Mouldy Oldy";
            grandKidsRel.ToType = unit;
            grandKidsRel.ToName = "Whipper Snapper";
            grandKidsRel.RelType_Enum = RelTypeEnum_Enumeration.RelManyToMany;
            grandKidsRel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
	        grandKidsRel.RelationshipInToTypeGroup = unitFieldGroup;
	        grandKidsRel.RelationshipInFromTypeGroup = unitFieldGroup;
            grandKidsRel.Save();

            //
            // Instances
            //
            var morty = Entity.Create(unit);
            morty.SetField("core:name", "Morty");
            morty.SetField(isHappyField, false);
            morty.SetField(birthdayField, new DateTime(2012, 06, 30));
            morty.Save();

            var jerry = Entity.Create(parent);
            jerry.SetField("core:name", "Jerry");
            jerry.SetField(isHappyField, false);
            jerry.SetField(birthdayField, new DateTime(1976, 03, 13));
            jerry.SetRelationships(kidsRel, new EntityRelationship<IEntity>(morty).ToEntityRelationshipCollection());
            jerry.Save();

            var rick = Entity.Create(grandParent);
            rick.SetField("core:name", "Rick");
            rick.SetField(isHappyField, true);
            rick.SetField(birthdayField, new DateTime(1955, 10, 24));
            rick.SetRelationships(kidsRel, new EntityRelationship<IEntity>(jerry).ToEntityRelationshipCollection());
            rick.SetRelationships(grandKidsRel, new EntityRelationship<IEntity>(morty).ToEntityRelationshipCollection());
            rick.Save();

            testObject.Types.Add("unit", unit);
            testObject.Types.Add("parent", parent);
            testObject.Types.Add("grandParent", grandParent);
	    }

	    private static void CreateUsersAndSecurity(ref GrandDesign testObject)
	    {
            //
            // Users
            //
            var administratorUser = Entity.Get<UserAccount>(new EntityRef("core:administratorUserAccount"), true);
            testObject.Accounts.Add("administrator", administratorUser);

            //
            // Access Rules
            //
            new AccessRuleFactory().AddAllowByQuery(administratorUser.As<Subject>(),
                testObject.Types["unit"].As<SecurableEntity>(),
                new[] { "core:read", "core:modify", "core:delete", "core:create" }.Select(p => new EntityRef(p)),
                TestQueries.Entities(testObject.Types["unit"]).ToReport());

            //
            // Read
            //
            var reader = Entity.Create<Person>();
            reader.FirstName = "Robbie";
            reader.LastName = "Reader";

            var readerAccount = Entity.Create<UserAccount>();
            readerAccount.Name = "reader";
            readerAccount.AccountHolder = reader;
            readerAccount.Save();
            testObject.Accounts.Add("reader", readerAccount);

            new AccessRuleFactory().AddAllowByQuery(readerAccount.As<Subject>(),
                testObject.Types["unit"].As<SecurableEntity>(),
                new[] { "core:read" }.Select(p => new EntityRef(p)),
                TestQueries.Entities(testObject.Types["unit"]).ToReport());

            //
            // Read + Modify
            //
            var modifier = Entity.Create<Person>();
	        modifier.FirstName = "Margie";
	        modifier.LastName = "Modifier";

            var modifierAccount = Entity.Create<UserAccount>();
            modifierAccount.Name = "modifier";
            modifierAccount.AccountHolder = modifier;
            modifierAccount.Save();
            testObject.Accounts.Add("modifier", modifierAccount);

            new AccessRuleFactory().AddAllowByQuery(modifierAccount.As<Subject>(),
                testObject.Types["unit"].As<SecurableEntity>(),
                new[] { "core:read", "core:modify" }.Select(p => new EntityRef(p)),
                TestQueries.Entities(testObject.Types["unit"]).ToReport());

            //
            // Read, Modify + Create   
            //
            var creator = Entity.Create<Person>();
            creator.FirstName = "Connie";
            creator.LastName = "Creator";

            var creatorAccount = Entity.Create<UserAccount>();
            creatorAccount.Name = "creator";
            creatorAccount.AccountHolder = creator;
            creatorAccount.Save();
            testObject.Accounts.Add("creator", creatorAccount);

            new AccessRuleFactory().AddAllowByQuery(creatorAccount.As<Subject>(),
                testObject.Types["unit"].As<SecurableEntity>(),
                new[] { "core:read", "core:modify", "core:create" }.Select(p => new EntityRef(p)),
                TestQueries.Entities(testObject.Types["unit"]).ToReport());

            //
            // Read, Modify + Delete
            //
            var deleter = Entity.Create<Person>();
            deleter.FirstName = "Donnie";
            deleter.LastName = "Deleter";

            var deleterAccount = Entity.Create<UserAccount>();
            deleterAccount.Name = "deleter";
            deleterAccount.AccountHolder = deleter;
            deleterAccount.Save();
            testObject.Accounts.Add("deleter", deleterAccount);

            new AccessRuleFactory().AddAllowByQuery(deleterAccount.As<Subject>(),
                testObject.Types["unit"].As<SecurableEntity>(),
                new[] { "core:read", "core:modify", "core:delete" }.Select(p => new EntityRef(p)),
                TestQueries.Entities(testObject.Types["unit"]).ToReport());

            //
            // Folder
            //
            var folder = Entity.Create<Folder>();
            folder.Name = "Get Actions";
            folder.ResourceInFolder.Add(Entity.GetByName<NavSection>("Test Solution").First().As<NavContainer>());
            folder.Save();
	        testObject.WorkingFolder = folder;

            //
            // All these accounts should see...
            //
	        foreach (var account in testObject.Accounts.Values)
	        {
                //
                // ...workflows
                //
                new AccessRuleFactory().AddAllowByQuery(account.As<Subject>(),
                    Workflow.Workflow_Type.As<SecurableEntity>(),
                    new EntityRef("core:read").ToEnumerable(),
                    TestQueries.Entities(new EntityRef("core:workflow")).ToReport());

                //
                // ... report templates
                //
                new AccessRuleFactory().AddAllowByQuery(account.As<Subject>(),
                    ReportTemplate.ReportTemplate_Type.As<SecurableEntity>(),
                    new EntityRef("core:read").ToEnumerable(),
                    TestQueries.Entities(new EntityRef("core:reportTemplate")).ToReport());
            }
        }

	    private static void CreateReportsAndFormsEtc(ref GrandDesign testObject)
	    {
            //
            // Reports
            //
            var unitReport = ActionTestHelper.CreateReport("test:unitReport", "Loose Units", null, testObject.Types["unit"], true);
            unitReport.ResourceInFolder.Add(testObject.WorkingFolder.As<NavContainer>());
            unitReport.Save();
            testObject.Reports.Add("unitReport", unitReport);

            var parentReport = ActionTestHelper.CreateReport("test:parentReport", "Parents", null, testObject.Types["parent"], true);
            parentReport.ResourceInFolder.Add(testObject.WorkingFolder.As<NavContainer>());
            parentReport.Save();
            testObject.Reports.Add("parentReport", parentReport);

            var grandParentReport = ActionTestHelper.CreateReport("test:grandParentReport", "Grandparents", null, testObject.Types["grandParent"], true);
            grandParentReport.ResourceInFolder.Add(testObject.WorkingFolder.As<NavContainer>());
            grandParentReport.Save();
            testObject.Reports.Add("grandParentReport", grandParentReport);

            //
            // Forms
            //
            var unitForm = ActionTestHelper.CreateForm("test:unitForm", "Loose Unit", null, testObject.Types["unit"], true);
            unitForm.Save();
            testObject.Forms.Add("unitForm", unitForm);

            var parentForm = ActionTestHelper.CreateForm("test:parentForm", "Parent", null, testObject.Types["parent"], true);
            parentForm.Save();
            testObject.Forms.Add("parentForm", parentForm);

            var grandParentForm = ActionTestHelper.CreateForm("test:grandParentForm", "Grandparent", null, testObject.Types["grandParent"], true);
            grandParentForm.Save();
            testObject.Forms.Add("grandParentForm", grandParentForm);

            //
            // Screen
            //
            var actionsScreen = ActionTestHelper.CreateScreen("test:getActionsScreen",
                "Action Screen", null, new Chart[0], new[] { parentReport }, new[] { unitForm },
                out testObject.ScreenComponents);
            actionsScreen.ResourceInFolder.Add(testObject.WorkingFolder.As<NavContainer>());
            actionsScreen.Save();

            //
            // Workflows
            //
            var unitWorkflow = ActionTestHelper.CreateWorkflow("test:unitWorkflow", "Stop", null, testObject.Types["unit"], "Unit");
            unitWorkflow.Save();

            var parentWorkflow = ActionTestHelper.CreateWorkflow("test:parentWorkflow", "Look", null, testObject.Types["parent"], "Parent");
            parentWorkflow.Save();

            var grandParentWorkflow = ActionTestHelper.CreateWorkflow("test:grandParentWorkflow", "Listen", null, testObject.Types["grandParent"], "Grandparent");
            grandParentWorkflow.Save();

            //
            // Document Generation
            //
            var unitGenerator = ActionTestHelper.CreateReportTemplate("text:unitGenerator", "Some Unit", null, testObject.Types["unit"]);
            unitGenerator.Save();

            var parentGenerator = ActionTestHelper.CreateReportTemplate("text:parentGenerator", "Muppet", null, testObject.Types["parent"]);
            parentGenerator.Save();

            var grandParentGenerator = ActionTestHelper.CreateReportTemplate("text:grandParentGenerator", "Some Elder", null, testObject.Types["grandParent"]);
            grandParentGenerator.Save();
        }
       
        private IEnumerable<ActionMenuItemInfo> Flatten(ActionMenuItemInfo actionMenuItem)
        {
            if (actionMenuItem == null)
            {
                throw new ArgumentNullException("actionMenuItem");
            }

            return new[] { actionMenuItem }.Concat(actionMenuItem.Children != null ? actionMenuItem.Children.SelectMany(Flatten) : Enumerable.Empty<ActionMenuItemInfo>());
        }

	    private class GrandDesign
	    {
	        internal Folder WorkingFolder { get; set; }
	        internal readonly Dictionary<string, EntityType> Types = new Dictionary<string, EntityType>();
            internal readonly Dictionary<string, UserAccount> Accounts = new Dictionary<string, UserAccount>();
            internal readonly Dictionary<string, Report> Reports = new Dictionary<string, Report>();
            internal readonly Dictionary<string, CustomEditForm> Forms = new Dictionary<string, CustomEditForm>();
            internal Dictionary<string, ControlOnForm> ScreenComponents = new Dictionary<string, ControlOnForm>();
	        internal readonly Dictionary<string, ActionMenuItemInfo> Actions;

	        internal IEnumerable<ActionMenuItemInfo> SelectActions(string select)
	        {
	            return new List<ActionMenuItemInfo>
	            {
                    new ActionMenuItemInfo
                    {
                        Order = 10,
                        Name = $"View '{@select}'",
                        EmptySelectName = "View",
                        Icon = "assets/images/16x16/view.svg",
                        IsEnabled = true,
                        IsMenu = true,
                        IsContextMenu = true,
                        IsSystem = true,
                        IsButton = false,
                        AppliesToSelection = true,
                        HtmlActionMethod = "navigate",
                        HtmlActionState = "viewForm"
                    },
                    new ActionMenuItemInfo
                    {
                        Order = 20,
                        Name = $"Edit '{@select}'",
                        EmptySelectName = "Edit",
                        Icon = "assets/images/16x16/edit.svg",
                        IsEnabled = true,
                        IsMenu = true,
                        IsContextMenu = true,
                        IsSystem = true,
                        IsButton = false,
                        AppliesToSelection = true,
                        HtmlActionMethod = "navigate",
                        HtmlActionState = "editForm"
                    },
                    new ActionMenuItemInfo
                    {
                        Order = 30,
                        Name = $"Delete '{@select}'",
                        EmptySelectName = "Delete",
                        MultiSelectName = "Delete selected",
                        Icon = "assets/images/16x16/delete.svg",
                        IsEnabled = true,
                        IsMenu = true,
                        IsContextMenu = true,
                        IsSystem = true,
                        IsButton = false,
                        AppliesToSelection = true,
                        AppliesToMultipleSelection = true,
                        HtmlActionMethod = "delete"
                    }
                };
	        }

	        internal GrandDesign()
	        {
	            Actions = new Dictionary<string, ActionMenuItemInfo>
	            {
                    // yuk... hard coded ones
	                {
                        "New (All)", new ActionMenuItemInfo
                        {
                            Order = -1,
                            Name = "New (All)",
                            DisplayName = "New (All)",
                            Icon = "assets/images/icon_new.png",
                            IsEnabled = true,
                            IsContextMenu = true,
                            IsSystem = true,
                            IsButton = true
                        }
                    },
	                {
                        "Export (All)", new ActionMenuItemInfo
                        {
                            Order = -1,
                            Name = "Export (All)",
                            DisplayName = "Export (All)",
                            Icon = "assets/images/16x16/export.png",
                            IsEnabled = true,
                            IsContextMenu = false,
                            IsSystem = true,
                            IsButton = false
                        }
                    },
	                {
	                    "Edit Inline", new ActionMenuItemInfo
	                    {
                            Order = -2,
                            Name = "Edit Inline",
                            DisplayName = "Edit Inline",
                            Icon = "assets/images/16x16/edit.svg",
                            IsEnabled = true,
                            IsContextMenu = false,
                            IsSystem = true,
                            IsButton = true
                        }
	                },
	                {
	                    "Excel", new ActionMenuItemInfo
	                    {
	                        Order = 500,
                            Name = "Excel",
                            DisplayName = "Export to Excel",
                            Icon = "assets/images/16x16/excel.svg",
                            IsEnabled = true,
                            IsMenu = true,
                            IsContextMenu = false,
                            IsSystem = true,
                            AppliesToSelection = false,
                            HtmlActionMethod = "export",
                            HtmlActionState = "excel"
	                    }
	                },
	                {
	                    "CSV", new ActionMenuItemInfo
	                    {
                            Order = 501,
                            Name = "CSV",
                            DisplayName = "Export to CSV",
                            Icon = "assets/images/16x16/csv.svg",
                            IsEnabled = true,
                            IsMenu = true,
                            IsContextMenu = false,
                            IsSystem = true,
                            AppliesToSelection = false,
                            HtmlActionMethod = "export",
                            HtmlActionState = "csv"
                        }
	                },
	                {
                        "Word", new ActionMenuItemInfo
                        {
                            Order = 502,
                            Name = "Word",
                            DisplayName = "Export to Word",
                            Icon = "assets/images/16x16/word.svg",
                            IsEnabled = true,
                            IsMenu = true,
                            IsContextMenu = false,
                            IsSystem = true,
                            AppliesToSelection = false,
                            HtmlActionMethod = "export",
                            HtmlActionState = "word"
                        }
	                },
	                {
                        "Assign Data to App", new ActionMenuItemInfo
                        {
                            Order = 1000,
                            Name = "Assign Data to App",
                            DisplayName = "Assign Data to App",
                            Icon = "assets/images/run.svg",
                            IsEnabled = false,
                            IsMenu = true,
                            IsContextMenu = true,
                            IsSystem = false,
                            AppliesToSelection = true,
                            HtmlActionMethod = "run",
                            HtmlActionState = "Input"
                        }
                    },
	                {
                        "Launch Person Campaign", new ActionMenuItemInfo
                        {
                            Order = 1000,
                            Name = "Launch Person Campaign",
                            DisplayName = "Launch Person Campaign",
                            Icon = "assets/images/run.svg",
                            IsEnabled = false,
                            IsMenu = true,
                            IsContextMenu = true,
                            IsSystem = false,
                            AppliesToSelection = true,
                            HtmlActionMethod = "run",
                            HtmlActionState = "Input"
                        }
                    },
                    // New - Loose Unit
	                {
                        "Loose Unit", new ActionMenuItemInfo
                        {
                            Name = "Loose Unit",
                            DisplayName = "New 'Loose Unit'",
                            Order = 0,
                            Icon = "assets/images/icon_new.png",
                            IsEnabled = true,
                            IsMenu = true,
                            IsSystem = true,
                            IsNew = true,
                            HtmlActionMethod = "navigate",
                            HtmlActionState = "createForm"
                        }
                    },
                    // New - Parent
	                {
                        "Parent", new ActionMenuItemInfo
                        {
                            Name = "Parent",
                            DisplayName = "New 'Parent'",
                            Order = 0,
                            Icon = "assets/images/icon_new.png",
                            IsEnabled = true,
                            IsMenu = true,
                            IsSystem = true,
                            IsNew = true,
                            HtmlActionMethod = "navigate",
                            HtmlActionState = "createForm"
                        }
                    },
                    // New - Grandparent
	                {
                        "Grandparent", new ActionMenuItemInfo
                        {
                            Name = "Grandparent",
                            DisplayName = "New 'Grandparent'",
                            Order = 0,
                            Icon = "assets/images/icon_new.png",
                            IsEnabled = true,
                            IsMenu = true,
                            IsSystem = true,
                            IsNew = true,
                            HtmlActionMethod = "navigate",
                            HtmlActionState = "createForm"
                        }
                    },
                    // Workflow - Stop
	                {
	                    "Stop", new ActionMenuItemInfo
                        {
                            Order = 1000,
                            Name = "Stop",
                            DisplayName = "Stop",
                            Icon = "assets/images/run.svg",
                            IsEnabled = false,
                            IsMenu = true,
                            IsContextMenu = true,
                            IsSystem = false,
                            AppliesToSelection = true,
                            HtmlActionMethod = "run",
                            HtmlActionState = "Unit"
                        }
                    },
                    // Workflow - Look
	                {
	                    "Look", new ActionMenuItemInfo
                        {
                            Order = 1000,
                            Name = "Look",
                            DisplayName = "Look",
                            Icon = "assets/images/run.svg",
                            IsEnabled = false,
                            IsMenu = true,
                            IsContextMenu = true,
                            IsSystem = false,
                            AppliesToSelection = true,
                            HtmlActionMethod = "run",
                            HtmlActionState = "Parent"
                        }
                    },
                    // Workflow - Listen
	                {
	                    "Listen", new ActionMenuItemInfo
                        {
                            Order = 1000,
                            Name = "Listen",
                            DisplayName = "Listen",
                            Icon = "assets/images/run.svg",
                            IsEnabled = false,
                            IsMenu = true,
                            IsContextMenu = true,
                            IsSystem = false,
                            AppliesToSelection = true,
                            HtmlActionMethod = "run",
                            HtmlActionState = "Grandparent"
                        }
                    },
                    // Doc Gen - Some Unit
	                {
                        "Some Unit", new ActionMenuItemInfo
                        {
                            Name = "Some Unit",
                            DisplayName = "Some Unit",
                            Order = 900,
                            Icon = "assets/images/generate.svg",
                            IsEnabled = true,
                            IsButton = false,
                            IsMenu = true,
                            IsContextMenu = true,
                            IsSystem = false,
                            AppliesToSelection = true,
                            AppliesToMultipleSelection = false,
                            HtmlActionMethod = "generate"
                        }
                    },
                    // Doc Gen - Muppet
	                {
                        "Muppet", new ActionMenuItemInfo
                        {
                            Name = "Muppet",
                            DisplayName = "Muppet",
                            Order = 900,
                            Icon = "assets/images/generate.svg",
                            IsEnabled = true,
                            IsButton = false,
                            IsMenu = true,
                            IsContextMenu = true,
                            IsSystem = false,
                            AppliesToSelection = true,
                            AppliesToMultipleSelection = false,
                            HtmlActionMethod = "generate"
                        }
                    },
                    // Doc Gen - Some Unit
	                {
                        "Some Elder", new ActionMenuItemInfo
                        {
                            Name = "Some Elder",
                            DisplayName = "Some Elder",
                            Order = 900,
                            Icon = "assets/images/generate.svg",
                            IsEnabled = true,
                            IsButton = false,
                            IsMenu = true,
                            IsContextMenu = true,
                            IsSystem = false,
                            AppliesToSelection = true,
                            AppliesToMultipleSelection = false,
                            HtmlActionMethod = "generate"
                        }
                    }
                };
	        }
	    }
    }
}
