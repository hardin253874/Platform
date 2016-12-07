// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Services.Console;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EDC.ReadiNow.EntityRequests;

namespace EDC.SoftwarePlatform.WebApi.Test.Console
{
    /// <summary>
    /// Test class for the Action Controller and <see cref="ActionService"/>.
    /// </summary>
    [TestFixture]
    public class ActionControllerTests
    {
        #region Tests
        /// <summary>
        /// Tests the service call when an invalid id is sent.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void PostActionsWithInvalidSelection()
        {
            using (var request = new PlatformHttpRequest(@"data/v1/actions", PlatformHttpMethod.Post))
            {
                request.PopulateBody( new ActionRequest { SelectedResourceIds = new long[ ] { 100000000 } } );
                var response = request.GetResponse();

                response.Should().NotBeNull();
                response.StatusCode.Should().Be(HttpStatusCode.OK, "We have a {0} returned, expected {1}.", response.StatusCode, HttpStatusCode.OK);

                var body = request.DeserialiseResponseBody<ActionResponse>();
                body.Should().NotBeNull();
                body.Actions.Should().NotBeNull().And.BeEmpty();
            }
        }

        /// <summary>
        /// Tests the <see cref="ActionService"/> for an empty or null request.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void GetActionsEmptyRequest()
        {
            var entityMock = new Mock<IEntityRepository>();
            entityMock
                .Setup(e => e.Get<Resource>(It.IsAny<IEnumerable<long>>()))
                .Returns(default(List<Resource>));

            var svc = new ActionService { EntityModelHelper = entityMock.Object };
            svc.FlushCaches();

            svc.Invoking(s => s.GetActions(null)).ShouldThrow<ArgumentException>("The request object may not be null.");
            
            var request = svc.GetActions(new ActionRequestExtended());
            request.Should().NotBeNull();
            request.Actions.Should().NotBeNull().And.BeEmpty();            
        }

        /// <summary>
        /// Tests the <see cref="ActionService"/> for a request referring to a single entity.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void GetActionsSingleSelectedResource( )
        {
            // arrange
            var resource = Entity.Create<Resource>();
            resource.Save( );

            var request = new ActionRequestExtended
            {
                LastSelectedResourceId = resource.Id,
                SelectedResourceIds = new[] { resource.Id }
            };

            var svc = new ActionService( );
            svc.FlushCaches();

            // act
            var result = svc.GetActions(request);

            // assert
            result.Should().NotBeNull();

            request.SelectedResources.Should().NotBeNull().And.NotBeEmpty();
            request.SelectedResources.Count().Should().Be(1);
            request.SelectedResources.Should().Contain(r => r.Id == resource.Id);
            request.LastSelectedResource.Id.Should().Be(resource.Id);
            request.SelectedResourceTypes.Should().NotBeNull().And.NotBeEmpty();
            foreach (EntityType t in resource.EntityTypes)
            {
                var type = request.SelectedResourceTypes.FirstOrDefault(a => a.Id == t.Id);
                type.Should().NotBeNull();
                if (type == null) continue;
                type.Alias.Should().Be(t.Alias);
                type.TenantId.Should().Be(t.TenantId);
            }

            result.Actions.Should().NotBeNull();
            RemovePowerTools(result);

            result.Actions.Count.Should().Be(3);
            result.Actions[0].Name.Should().Be("View 'Resource'"); // View
            result.Actions[0].Order.Should().Be(10);
            result.Actions[1].Name.Should().Be("Edit 'Resource'"); // Edit
            result.Actions[1].Order.Should().Be(20);
            result.Actions[2].Name.Should().Be("Delete 'Resource'"); // Delete
            result.Actions[2].Order.Should().Be(30);
        }

        /// <summary>
        /// Tests the <see cref="ActionService"/> for a request referring to a multiple selected entities.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void GetActionsMultipleSelectedResources()
        {
            // arrange
            var resource1 = Entity.Create<Resource>();
            var resource2 = Entity.Create<Resource>();
            resource1.Save( );
            resource2.Save( );

            var svc = new ActionService( );
            svc.FlushCaches();


            var request = new ActionRequestExtended
            {
                LastSelectedResourceId = resource2.Id,
                SelectedResourceIds = new[] { resource1.Id, resource2.Id }
            };

            // act
            var result = svc.GetActions(request);

            // assert
            result.Should().NotBeNull();

            request.SelectedResources.Should().NotBeNull().And.NotBeEmpty();
            request.SelectedResources.Count().Should().Be(2);
            request.SelectedResources.Should().Contain(r => r.Id == resource1.Id);
            request.SelectedResources.Should().Contain(r => r.Id == resource2.Id);
            request.LastSelectedResource.Should().NotBeNull();
            request.SelectedResourceTypes.Should().NotBeNull().And.NotBeEmpty();
            foreach (EntityType t in resource1.EntityTypes)
            {
                var type = request.SelectedResourceTypes.FirstOrDefault(a => a.Id == t.Id);
                type.Should().NotBeNull();
                if (type == null) continue;
                type.Alias.Should().Be(t.Alias);
                type.TenantId.Should().Be(t.TenantId);
            }

            result.Actions.Should().NotBeNull();
            result.Actions.Count.Should().Be(1);
            result.Actions[0].Name.Should().Be("Delete selected"); // Multi-delete
            result.Actions[0].Order.Should().Be(30);
        }

        /// <summary>
        /// Tests the <see cref="ActionService"/> for a request with a reference to a host resource.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void GetActionsWithReport()
        {
            // arrange
            var resource = Entity.Create<Resource>();
            resource.Save( );

            var report = Entity.Create<ReadiNow.Model.Report>( );
            report.ResourceViewerConsoleForm = new CustomEditForm();
            var typeToEditWithForm = new EntityType();
            report.ResourceViewerConsoleForm.TypeToEditWithForm = typeToEditWithForm;
            report.Save( );

            var svc = new ActionService( );
            svc.FlushCaches();


            var request = new ActionRequestExtended
            {
                LastSelectedResourceId = resource.Id,
                SelectedResourceIds = new[] { resource.Id },
                ReportId = report.Id
            };

            // act
            var result = svc.GetActions(request);

            // assert
            result.Should().NotBeNull();

            request.SelectedResources.Should().NotBeNull().And.NotBeEmpty();
            request.SelectedResources.Count().Should().Be(1);
            request.SelectedResources.Should().Contain(r => r.Id == resource.Id);
            request.LastSelectedResource.Should().NotBeNull();
            request.SelectedResourceTypes.Should().NotBeNull().And.NotBeEmpty();
            foreach (EntityType t in resource.EntityTypes)
            {
                var type = request.SelectedResourceTypes.FirstOrDefault(a => a.Id == t.Id);
                type.Should().NotBeNull();
                if (type == null) continue;
                type.Alias.Should().Be(t.Alias);
                type.TenantId.Should().Be(t.TenantId);
            }


            result.Actions.Should().NotBeNull();
            RemovePowerTools(result);

            result.Actions.Count.Should().Be(3);
            result.Actions[0].Name.Should().Be("View 'Resource'"); // View
            result.Actions[0].Order.Should().Be(10);
            result.Actions[0].AdditionalData.Should().NotBeNull().And.NotBeEmpty();
            result.Actions[0].AdditionalData["CustomForm"].Should().Be(report.ResourceViewerConsoleForm.Id);
            result.Actions[0].AdditionalData["CustomFormEditsTypeId"].Should().Be(typeToEditWithForm.Id);
            result.Actions[1].Name.Should().Be("Edit 'Resource'"); // Edit
            result.Actions[1].Order.Should().Be(20);
            result.Actions[1].AdditionalData.Should().NotBeNull().And.NotBeEmpty();
            result.Actions[1].AdditionalData["CustomForm"].Should().Be(report.ResourceViewerConsoleForm.Id);
            result.Actions[1].AdditionalData["CustomFormEditsTypeId"].Should().Be(typeToEditWithForm.Id);
            result.Actions[2].Name.Should().Be("Delete 'Resource'"); // Delete
            result.Actions[2].Order.Should().Be(30);
            result.Actions[2].AdditionalData.Should().NotBeNull().And.NotBeEmpty();
            result.Actions[2].AdditionalData["CustomForm"].Should().Be(report.ResourceViewerConsoleForm.Id);
            result.Actions[2].AdditionalData["CustomFormEditsTypeId"].Should().Be(typeToEditWithForm.Id);
        }

        /// <summary>
        /// Tests the <see cref="ActionService"/> for a request with a reference to a host resource.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void GetActionsWithReportOnTabControlInForm()
        {
            // arrange
            var resource = Entity.Create<Resource>();
            var report = Entity.Create<ReadiNow.Model.Report>();
            
            var testType1 = new EntityType { Name = "Test Type 1" };
            var testType2 = new EntityType { Name = "Test Type 2" };
            var testType1Form = new CustomEditForm { Name = "testType1Form", TypeToEditWithForm = testType1 };
            var testType2Form = new CustomEditForm { Name = "testType2Form", TypeToEditWithForm = testType2 };

            var tabControl = new TabRelationshipRenderControl { ResourceViewerConsoleForm = testType1Form };    // assign custom form to tab host control
            report.ResourceViewerConsoleForm = testType2Form;                                                   // assign custom form to the report
            tabControl.RelationshipDisplayReport = report;

            report.Save( );
            resource.Save( );
            testType1.Save( );
            testType2.Save( );
            testType1Form.Save( );
            testType2Form.Save( );
            tabControl.Save( );

            var svc = new ActionService( );
            svc.FlushCaches();

            var request = new ActionRequestExtended
            {
                LastSelectedResourceId = resource.Id,
                SelectedResourceIds = new[] { resource.Id },
                HostResourceIds = new[] { tabControl.Id },
                ReportId = report.Id,
                ActionDisplayContext = ActionContext.QuickMenu
            };

            // act
            var result = svc.GetActions(request);

            // assert
            result.Should().NotBeNull();

            request.SelectedResources.Should().NotBeNull().And.NotBeEmpty();
            request.SelectedResources.Count().Should().Be(1);
            request.SelectedResources.Should().Contain(r => r.Id == resource.Id);
            request.LastSelectedResource.Should().NotBeNull();
            request.SelectedResourceTypes.Should().NotBeNull().And.NotBeEmpty();
            foreach (EntityType t in resource.EntityTypes)
            {
                var type = request.SelectedResourceTypes.FirstOrDefault(a => a.Id == t.Id);
                type.Should().NotBeNull();
                if (type == null) continue;
                type.Alias.Should().Be(t.Alias);
                type.TenantId.Should().Be(t.TenantId);
            }


            result.Actions.Should().NotBeNull();
            RemovePowerTools(result);

            result.Actions.Count.Should().Be(8);

            result.Actions[0].Name.Should().Be("View 'Resource'"); // View
            result.Actions[0].Order.Should().Be(10);
            result.Actions[0].AdditionalData.Should().NotBeNull().And.NotBeEmpty();
            result.Actions[0].AdditionalData["CustomForm"].Should().Be(tabControl.ResourceViewerConsoleForm.Id);    // make sure the form is the one assigned to tab control, not the form assigned to report
            result.Actions[0].AdditionalData["CustomFormEditsTypeId"].Should().Be(testType1.Id);
            
            result.Actions[1].Name.Should().Be("Edit 'Resource'"); // Edit
            result.Actions[1].Order.Should().Be(20);
            result.Actions[1].AdditionalData.Should().NotBeNull().And.NotBeEmpty();
            result.Actions[1].AdditionalData["CustomForm"].Should().Be(tabControl.ResourceViewerConsoleForm.Id);
            result.Actions[1].AdditionalData["CustomFormEditsTypeId"].Should().Be(testType1.Id);
            
            result.Actions[2].Name.Should().Be("Link to Existing"); // Link
            result.Actions[2].Order.Should().Be(25);
            result.Actions[2].AdditionalData.Should().NotBeNull().And.NotBeEmpty();
            result.Actions[2].AdditionalData["CustomForm"].Should().Be(tabControl.ResourceViewerConsoleForm.Id);
            result.Actions[2].AdditionalData["CustomFormEditsTypeId"].Should().Be(testType1.Id);

            result.Actions[3].Name.Should().Be("Remove Link"); // Remove Link
            result.Actions[3].Order.Should().Be(26);
            result.Actions[3].AdditionalData.Should().NotBeNull().And.NotBeEmpty();
            result.Actions[3].AdditionalData["CustomForm"].Should().Be(tabControl.ResourceViewerConsoleForm.Id);
            result.Actions[3].AdditionalData["CustomFormEditsTypeId"].Should().Be(testType1.Id);

            result.Actions[4].Name.Should().Be("Delete 'Resource'"); // Delete
            result.Actions[4].Order.Should().Be(30);
            result.Actions[4].AdditionalData.Should().NotBeNull().And.NotBeEmpty();
            result.Actions[4].AdditionalData["CustomForm"].Should().Be(tabControl.ResourceViewerConsoleForm.Id);
            result.Actions[4].AdditionalData["CustomFormEditsTypeId"].Should().Be(testType1.Id);

            result.Actions[5].Name.Should().Be("Excel"); // Excel
            result.Actions[5].Order.Should().Be(500);
            result.Actions[5].AdditionalData.Should().NotBeNull().And.NotBeEmpty();
            result.Actions[5].AdditionalData["CustomForm"].Should().Be(tabControl.ResourceViewerConsoleForm.Id);
            result.Actions[5].AdditionalData["CustomFormEditsTypeId"].Should().Be(testType1.Id);
            
            result.Actions[6].Name.Should().Be("CSV"); // CSV
            result.Actions[6].Order.Should().Be(501);
            result.Actions[6].AdditionalData.Should().NotBeNull().And.NotBeEmpty();
            result.Actions[6].AdditionalData["CustomForm"].Should().Be(tabControl.ResourceViewerConsoleForm.Id);
            result.Actions[6].AdditionalData["CustomFormEditsTypeId"].Should().Be(testType1.Id);
            
            result.Actions[7].Name.Should().Be("Word"); // Word
            result.Actions[7].Order.Should().Be(502);
            result.Actions[7].AdditionalData.Should().NotBeNull().And.NotBeEmpty();
            result.Actions[7].AdditionalData["CustomForm"].Should().Be(tabControl.ResourceViewerConsoleForm.Id);
            result.Actions[7].AdditionalData["CustomFormEditsTypeId"].Should().Be(testType1.Id);
        }

        /// <summary>
        /// Tests the <see cref="ActionService"/> for a request with multiple potential host resources.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void GetActionsReportInScreen_Bug_25919()
        {
            var section = Entity.Get<NavSection>("test:25919_section");
            var workflow = Entity.Get<Workflow>("test:25919_workflow");
            var report = Entity.Get<ReadiNow.Model.Report>("test:25919_report");
            var screen = Entity.Get<StructureControlOnForm>("test:25919_screen");

            section.Should().NotBeNull("Section should exist in Test Solution.");
            workflow.Should().NotBeNull("Workflow should exist in Test Solution.");
            report.Should().NotBeNull("Report should exist in Test Solution.");
            screen.Should().NotBeNull("Screen should exist in Test Solution.");
            screen.ContainedControlsOnForm.Should().NotBeNull().And.NotBeEmpty();
            screen.ContainedControlsOnForm.Count.Should().Be(1);

            var ctrl = screen.ContainedControlsOnForm.First();

            var aaronWitte = Entity.Get("test:aaronWitte");
            aaronWitte.Should().NotBeNull("AAAAAAAAAAAAAAARRROOOOOOOOOOOONNNN!!!!");

            var svc = new ActionService();
            svc.FlushCaches();

            var request1 = new ActionRequestExtended
            {
                LastSelectedResourceId = aaronWitte.Id,
                SelectedResourceIds = new[] { aaronWitte.Id },
                ReportId = report.Id
            };

            var result1 = svc.GetActions(request1);

            result1.Should().NotBeNull();
            result1.Actions.Should().NotBeNull().And.NotBeEmpty();
            result1.Actions.Count(a => a.Name == "Log Test Person").Should().Be(1);

            var request2 = new ActionRequestExtended
            {
                LastSelectedResourceId = aaronWitte.Id,
                SelectedResourceIds = new []{ aaronWitte.Id },
                ReportId = report.Id,
                HostResourceIds = new []{ ctrl.Id }
            };
            
            var result2 = svc.GetActions(request2);

            result2.Should().NotBeNull();
            result2.Actions.Should().NotBeNull().And.NotBeEmpty();
            result2.Actions.Count.Should().Be(result1.Actions.Count - 1);
            result2.Actions.Count(a => a.Name == "Log Test Person").Should().Be(0);
        }

        /// <summary>
        /// Tests the <see cref="ActionService"/> for all form actions avaiable in config mode.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void GetFormActionsForConfigMode()
        {
            // Should fetch actions associated with person type (only workflow and document generation type actions are fetched in form actions)
            var personType = Entity.Get<Definition>("core:person");

            var request = new ActionRequestExtended
            {
                EntityTypeId = personType.Id,
                FormId = personType.DefaultEditForm.Id,
                ActionDisplayContext = ActionContext.All
            };

            var svc = new ActionService();
            svc.FlushCaches();

            var result = svc.GetFormActionsMenuState(request);
            
            result.Should().NotBeNull();
            result.Actions.Should().NotBeNull().And.NotBeEmpty();
            result.Actions.Count.Should().Be(3);
            result.Actions.Count(a => a.Name == "Assign Data to App").Should().Be(1);
            result.Actions.Count(a => a.Name == "Person Name Update").Should().Be(1);
            result.Actions.Count(a => a.Name == "Launch Person Campaign").Should().Be(1);
        }

        /// <summary>
        /// Tests the <see cref="ActionService"/> for all form actions set on a form.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void GetFormActionsForForm()
        {
            var personType = Entity.Get<Definition>("core:person");
            var personForm = Entity.Get<CustomEditForm>(personType.DefaultEditForm.Id, true);

            // fetch action for the person type
            var request = new ActionRequestExtended
            {
                EntityTypeId = personType.Id,
                FormId = personType.DefaultEditForm.Id,
                ActionDisplayContext = ActionContext.All
            };

            var actionSvc = new ActionService();
            actionSvc.FlushCaches();

            var result = actionSvc.GetFormActionsMenuState(request);

            result.Should().NotBeNull();
            result.Actions.Should().NotBeNull().And.NotBeEmpty();
            result.Actions.Count.Should().Be(3);
            result.Actions.Count(a => a.Name == "Assign Data to App").Should().Be(1);
            result.Actions.Count(a => a.Name == "Person Name Update").Should().Be(1);
            result.Actions.Count(a => a.Name == "Launch Person Campaign").Should().Be(1);

            // create new action item and assign it to the form
            // form action
            var actionInfo = result.Actions.Find(a => a.Name == "Person Name Update");
            var wf = Entity.Get<Workflow>(actionInfo.EntityId);

            // create workflow action item 
            var wfActionMenuItem = new WorkflowActionMenuItem
            {
                Name = actionInfo.Name,
                MenuIconUrl = actionInfo.Icon,
                MenuOrder = actionInfo.Order,
                IsActionButton = actionInfo.IsButton,
                IsMenuSeparator = actionInfo.IsSeparator,
                IsContextMenu = actionInfo.IsContextMenu,
                IsActionItem = actionInfo.IsMenu,
                IsSystem = actionInfo.IsSystem,
                AppliesToSelection = true,
                AppliesToMultiSelection = false,
                HtmlActionMethod = actionInfo.HtmlActionMethod,
                HtmlActionState = actionInfo.HtmlActionState,
                ActionMenuItemToWorkflow = wf
            };

            var cb = new ConsoleBehavior
            {
                Name = $"DeleteMe_PersonForm_rcb {DateTime.Now}",
                BehaviorActionMenu = new ActionMenu
                {
                    Name = $"DeleteMe_PersonForm_rcb_menu {DateTime.Now}",
                    IncludeActionsAsButtons = new EntityCollection<ActionMenuItem>() {(ActionMenuItem) wfActionMenuItem },
                    MenuItems = new EntityCollection<ActionMenuItem>() { (ActionMenuItem) wfActionMenuItem }
                }
            };

            personForm.ResourceConsoleBehavior = cb;
            personForm.Save();

            var formActionQuery = @"{ k:resourceConsoleBehavior }
                                        .k:behaviorActionMenu.{
                                            { k:menuItems, k:suppressedActions, k:includeActionsAsButtons }.{
                                                {   name, 
                                                    description,
                                                    k:menuIconUrl,
                                                    htmlActionState,
                                                    htmlActionMethod,
                                                    k:isActionButton,
                                                    k:appliesToSelection,
                                                    k:isMenuSeparator, 
                                                    k:menuOrder,
                                                    { isOfType }.{ alias,name },
                                                    { k:actionMenuItemToWorkflow }.{ name },
                                                    { k:actionMenuItemToReportTemplate }.{ name }
                                                }
                                            },
                                            { k:includeTypesForNewButtons, k:suppressedTypesForNewMenu }.id
                                    }";

            // fetch form actions and check there is only one action button assigned to the form
            var entitySvc = new EntityInfoService();

            EntityMemberRequest req = EntityRequestHelper.BuildRequest(formActionQuery);
            var result2 = entitySvc.GetEntityData(personForm.Id, req);
            result2.Should().NotBeNull();

            var rcb = result2.Relationships.Find(a => a.RelationshipTypeId.Alias == "resourceConsoleBehavior");
            rcb.Should().NotBeNull();

            var actionMenu = rcb.Entities.First().Relationships.Find(a => a.RelationshipTypeId.Alias == "behaviorActionMenu");
            actionMenu.Should().NotBeNull();

            var actionButtons = actionMenu.Entities.First().Relationships.Find(a => a.RelationshipTypeId.Alias == "includeActionsAsButtons");
            actionButtons.Should().NotBeNull();

            var nameValue = actionButtons.Entities.First().Fields.Find(f => f.FieldId.Alias == "name").Value.Value;
            nameValue.Should().Be("Person Name Update");
            
            
            // delete console behavior
            Entity.Delete(cb.Id);

        }

        /// <summary>
        /// Tests the option to suppress all Create ('New') items from a menu.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void GetActionsSuppressAllNewItems()
        {
            // arrange
            var testType1 = new EntityType { Name = "Test Type 1" };
            testType1.Inherits.Add(Entity.Get<EntityType>("core:definition"));
            testType1.Save();
            var testType2 = new EntityType { Name = "Test Type 2" };
            testType2.Inherits.Add(testType1);
            testType2.Save();

            var report = Entity.Create<ReadiNow.Model.Report>();
            report.ReportUsesDefinition = testType1;
            report.Save();

            var svc = new ActionService();
            svc.FlushCaches();
            
            var request = new ActionRequestExtended
            {
                ReportId = report.Id,
                ReportBaseType = report.ReportUsesDefinition,
                HostResourceIds = new long[0],
                ActionDisplayContext = ActionContext.ActionsMenu
            };

            // act
            var result = svc.GetActions(request);
            
            // assert
            result.Should().NotBeNull();
            result.Actions.Should().NotBeNull();
            result.Actions.Count.Should().BeGreaterThan(0);

            var newMenu = result.Actions.FirstOrDefault(a => a.Name == "New");
            newMenu.Should().NotBeNull();
            newMenu.Children.Count.Should().Be(2);

            var t1 = newMenu.Children.FirstOrDefault(a => a.Name == "Test Type 1");
            t1.Should().NotBeNull();
            t1.IsNew.Should().BeTrue();
            t1.HtmlActionMethod.Should().Be("navigate");
            t1.HtmlActionState.Should().Be("createForm");
            t1.EntityId.Should().Be(testType1.Id);

            var t2 = newMenu.Children.FirstOrDefault(a => a.Name == "Test Type 2");
            t2.Should().NotBeNull();
            t2.IsNew.Should().BeTrue();
            t2.HtmlActionMethod.Should().Be("navigate");
            t2.HtmlActionState.Should().Be("createForm");
            t2.EntityId.Should().Be(testType2.Id);

            // suppress the news
            var rcb = new ConsoleBehavior();
            var am = new ActionMenu {SuppressNewActions = true};
            rcb.BehaviorActionMenu = am;
            report.ResourceConsoleBehavior = rcb;
            report.Save();

            svc.FlushCaches();

            var result2 = svc.GetActions(request);
            
            result2.Should().NotBeNull();
            result2.Actions.Should().NotBeNull();
            result2.Actions.Count.Should().BeGreaterThan(0);
            result2.Actions.Count(a => a.Name == "New").Should().Be(0);
        }

        /// <summary>
        /// Tests actions that disable themselves based on an expression.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void GetActionsWithExpressionItems()
        {
            // arrange
            var scientistType = Entity.GetByName<EntityType>("Scientist").First();
            var bornField = scientistType.GetAllFields().FirstOrDefault(f => f.Name == "Born");

            var scientist = Entity.Create(new EntityRef(scientistType));
            scientist.SetField("core:name", "Alex Engelhardt");
            scientist.SetField(new EntityRef(bornField), new DateTime(1977, 10, 24));
            scientist.Save();

            var alanTuring = Entity.GetByName<Resource>("Alan Turing").First();
            alanTuring.Should().NotBeNull();
            alanTuring.Id.Should().BeGreaterThan(0);

            var report = Entity.Create<ReadiNow.Model.Report>();
            var sb = new ConsoleBehavior();
            var am = new ActionMenu();

            var a = new ActionMenuItem
            {
                Name = "Test '%Resource%' Action",
                EmptySelectName = "Test Expression Action",
                IsActionItem = true,
                IsActionButton = true,
                IsContextMenu = true,
                AppliesToSelection = true,
                ActionRequiresExpression = new ActionExpression
                {
                    ActionExpressionString = "datediff(year, Born, [Alan].[Born]) = 0",
                    ActionExpressionEntities = new EntityCollection<NamedReference>
                    {
                        new NamedReference
                        {
                            Name = "Alan",
                            ReferencedEntity = alanTuring
                        }
                    }
                }
            };

            am.MenuItems.Add(a);
            am.IncludeActionsAsButtons.Add(a);
            sb.BehaviorActionMenu = am;
            report.SelectionBehavior = sb;
            report.Save();

            var svc = new ActionService();
            svc.FlushCaches();

            var request = new ActionRequestExtended
            {
                LastSelectedResourceId = scientist.Id,
                SelectedResourceIds = new[] { scientist.Id },
                EntityTypeId = scientistType.Id,
                ReportId = report.Id,
                ReportBaseType = scientistType,
                HostResourceIds = new long[0]
            };

            // act
            var result = svc.GetActions(request);

            // assert
            result.Should().NotBeNull();
            result.Actions.Should().NotBeNull();
            result.Actions.Count.Should().BeGreaterThan(0);

            var action = result.Actions.FirstOrDefault(i => i.Name == "Test 'Alex Engelhardt' Action");
            action.Should().NotBeNull();
            action.IsEnabled.Should().BeFalse();
            action.EntityId.Should().Be(scientist.Id);

            request.LastSelectedResourceId = alanTuring.Id;
            request.SelectedResourceIds = new[] { alanTuring.Id };

            var result2 = svc.GetActions(request);

            result2.Should().NotBeNull();
            result2.Actions.Should().NotBeNull();
            result2.Actions.Count.Should().BeGreaterThan(0);

            var action2 = result2.Actions.FirstOrDefault(i => i.Name == "Test 'Alan Turing' Action");
            action2.Should().NotBeNull();
            action2.IsEnabled.Should().BeTrue();
            action2.EntityId.Should().Be(alanTuring.Id);
        }

        private static void RemovePowerTools(ActionResponse result)
        {
            result.Actions = result.Actions.Where(a => a.Name != "Show XML" && !a.Name.StartsWith("Debug")).ToList();
        }

        #endregion
    }
}
