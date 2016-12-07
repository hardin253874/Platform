// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.EditForm;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test.Security.AccessControl;
using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.ReadiNow.Test.Services.Console
{
    public static class ActionTestHelper
    {
        /// <summary>
        /// A query string that retrieves the relevant information about a custom edit form (and only the custom edit form type).
        /// </summary>
        public const string FormQuery = @"
name, description, alias,
isOfType.{id, name, alias, k:typeConsoleBehavior.{k:treeIcon.{name, imageBackgroundColor}}},
k:typeToEditWithForm.{name, canCreateType, k:typeConsoleBehavior.{k:treeIcon.{name, imageBackgroundColor}}}, 
k:resourceConsoleBehavior.{id, name, alias, k:suppressActionsForType},
k:resourceInFolder.{ id, name, alias},
k:renderingHorizontalResizeMode.{alias},
k:renderingVerticalResizeMode.{alias},
k:readOnlyControl,
k:containedControlsOnForm*.{
    name, description, alias, isOfType.{id, name, alias},
    k:renderingOrdinal, k:renderingHeight, k:renderingWidth, k:renderingBackgroundColor, k:hideLabel, k:renderingHorizontalResizeMode.{alias},k:renderingVerticalResizeMode.{alias},
    k:mandatoryControl, k:readOnlyControl, k:showControlHelpText, k:visibilityCalculation,
    k:thumbnailSizeSetting.{ alias, k:thumbnailWidth, k:thumbnailHeight }, k:thumbnailScalingSetting.alias,
    k:relationshipControlFilters.{
        isOfType.{id, name, alias},
        k:relationshipControlFilterOrdinal,
        k:relationshipControlFilter.id,
        k:relationshipFilter.id,
        k:relationshipDirectionFilter.{alias}
    },
	k:controlRelatedEntityDataPathNodes.{
		k:dataPathNodeOrdinal, k:dataPathNodeRelationshipDirection.alias, k:dataPathNodeRelationship.{ 
			reverseAlias, relType.alias,
			name, description, toName, fromName, 
			fromType.{name, alias, isAbstract, k:defaultEditForm.name, defaultPickerReport.name, defaultDisplayReport.name, k:formsToEditType.name}, 
			fromType.inherits*.{name, alias, k:defaultEditForm.name},
			toType.{name, alias, isAbstract, k:defaultEditForm.name, defaultPickerReport.name, defaultDisplayReport.name, k:formsToEditType.name}, 
			toType.inherits*.{name, alias, k:defaultEditForm.name},
			toTypeDefaultValue.{name, description}, fromTypeDefaultValue.{name, description},
			defaultFromUseCurrent, defaultToUseCurrent, 
			cardinality.{name, description}, 
			relationshipIsMandatory, revRelationshipIsMandatory
		}
	},
    k:wbcWorkflowToRun.{name}, k:wbcResourceInputParameter.{name}
},
k:fieldToRender.{
    name, description, alias, isOfType.{id, name, alias},
    decimalPlaces, fieldRepresents.{id, alias}, defaultValue, autoNumberDisplayPattern, fieldWatermark, isFieldReadOnly
},
k:relationshipToRender.{ 
    reverseAlias, relType.alias, name, description, toName, fromName, 
    fromType.{name, alias, isAbstract, canCreateType, k:defaultEditForm.name, defaultPickerReport.name, defaultDisplayReport.name, k:formsToEditType.name}, 
    fromType.inherits*.{name, alias, canCreateType, k:defaultEditForm.name},
    toType.{name, alias, isAbstract, canCreateType, k:defaultEditForm.name, defaultPickerReport.name, defaultDisplayReport.name, k:formsToEditType.name}, 
    toType.inherits*.{name, alias, canCreateType, k:defaultEditForm.name},
    toTypeDefaultValue.{name, description}, fromTypeDefaultValue.{name, description},
    defaultFromUseCurrent, defaultToUseCurrent, 
    cardinality.{name, description}, showRelationshipHelpText,
    relationshipIsMandatory, revRelationshipIsMandatory
}
";

        /// <summary>
        /// Creates a form entity for a given type.
        /// </summary>
        /// <param name="alias">The alias given to the form.</param>
        /// <param name="name">The name given to the form.</param>
        /// <param name="description">The description given to the form.</param>
        /// <param name="type">The type to create the form for.</param>
        /// <param name="isDefault">Whether the form should be the default for the type.</param>
        /// <returns>The created form entity.</returns>
        public static CustomEditForm CreateForm(string alias, string name, string description, EntityType type, bool isDefault)
        {
            var generator = new DefaultLayoutGenerator(CurrentUiContext.Html, new EntityRef(type), false);
            var formEntityData = generator.GetLayoutAsEntityData(FormQuery);

            var form = new EntityInfoService().CreateEntity(formEntityData).Entity.AsWritable<CustomEditForm>();
            form.Alias = alias;
            form.Name = name;
            form.Description = description;
            if (isDefault)
            {
                form.IsDefaultForEntityType = type;
            }

            return form;
        }

        /// <summary>
        /// Creates a report entity for a given type.
        /// </summary>
        /// <param name="alias">The alias given to the report.</param>
        /// <param name="name">The name given to the report.</param>
        /// <param name="description">The description given to the report.</param>
        /// <param name="type">The type to create the report for.</param>
        /// <param name="isDefault">Whether the report should be the default display report for the type.</param>
        /// <returns>The created report entity.</returns>
        public static Report CreateReport(string alias, string name, string description, EntityType type, bool isDefault)
        {
            var query = TestQueries.Entities(type);
            query.SelectColumns[0].IsHidden = true;
            query.SelectColumns.Add(new SelectColumn
            {
                ColumnName = "Name",
                DisplayName = "Unit",
                Expression = new ResourceDataColumn(query.RootEntity, new EntityRef("core:name"))
            });

            var report = query.ToReport();
            report.Alias = alias;
            report.Name = name;
            report.Description = description;
            if (isDefault)
            {
                report.IsDefaultDisplayReportForTypes.Add(type);
            }

            return report;
        }

        /// <summary>
        /// Creates an empty console behavior entity.
        /// </summary>
        /// <returns>The created console behavior entity.</returns>
        public static ConsoleBehavior CreateBehavior()
        {
            var cb = Entity.Create<ConsoleBehavior>();
            cb.BehaviorActionMenu = Entity.Create<ActionMenu>();
            return cb;
        }

        /// <summary>
        /// Creates a screen from the given charts, reports and forms.
        /// </summary>
        /// <param name="alias">The alias given to the screen.</param>
        /// <param name="name">The name given to the screen.</param>
        /// <param name="description">The description given to the screen.</param>
        /// <param name="charts">The charts to add.</param>
        /// <param name="reports">The reports to add.</param>
        /// <param name="forms">The form to add.</param>
        /// <param name="containers">A lookup to all the container controls that were created.</param>
        /// <returns>The created and saved screen entity.</returns>
        public static StructureControlOnForm CreateScreen(string alias, string name, string description,
            IList<Chart> charts, IList<Report> reports, IList<CustomEditForm> forms,
            out Dictionary<string, ControlOnForm> containers)
        {
            containers = new Dictionary<string, ControlOnForm>();

            var screen = Entity.Create("console:screen").As<StructureControlOnForm>();
            screen.Alias = alias;
            screen.Name = name;
            screen.Description = description;

            var containedControls = new List<ControlOnForm>();

            foreach (var chart in charts)
            {
                if (chart == null)
                    continue;

                var idx = charts.IndexOf(chart);
                var controlName = "Chart" + idx;
                var chartComponent = Entity.Create("console:chartRenderControl").AsWritable<ControlOnForm>();
                chartComponent.Alias = alias + controlName;
                chartComponent.SetRelationships("console:renderingVerticalResizeMode", new EntityRelationshipCollection<IEntity> { Entity.Get<IEntity>("console:resizeFifty") }, Direction.Forward);
                chartComponent.SetRelationships("console:renderingHorizontalResizeMode", new EntityRelationshipCollection<IEntity> { Entity.Get<IEntity>("console:resizeSpring") }, Direction.Forward);
                chartComponent.SetRelationships("console:chartToRender", new EntityRelationship<IEntity>(chart).ToEntityRelationshipCollection(), Direction.Forward);
                containedControls.Add(chartComponent);
                containers.Add(controlName, chartComponent);
            }

            foreach (var report in reports)
            {
                if (report == null)
                    continue;

                var idx = reports.IndexOf(report);
                var controlName = "Report" + idx;
                var reportComponent = Entity.Create("console:reportRenderControl").AsWritable<ControlOnForm>();
                reportComponent.Alias = alias + controlName;
                reportComponent.SetRelationships("console:renderingVerticalResizeMode", new EntityRelationshipCollection<IEntity> { Entity.Get<IEntity>("console:resizeFifty") }, Direction.Forward);
                reportComponent.SetRelationships("console:renderingHorizontalResizeMode", new EntityRelationshipCollection<IEntity> { Entity.Get<IEntity>("console:resizeSpring") }, Direction.Forward);
                reportComponent.SetRelationships("console:reportToRender", new EntityRelationship<IEntity>(report).ToEntityRelationshipCollection(), Direction.Forward);

                if (reports.Count > idx)
                {
                    var contextProvider = containedControls.FirstOrDefault(c => c.Alias == (alias + "Chart" + idx));
                    if (contextProvider != null)
                    {
                        reportComponent.SetRelationships("console:receiveContextFrom", new EntityRelationship<IEntity>(contextProvider).ToEntityRelationshipCollection(), Direction.Forward);
                    }
                }
                
                containedControls.Add(reportComponent);
                containers.Add(controlName, reportComponent);
            }

            foreach (var form in forms)
            {
                if (form == null)
                    continue;

                var idx = forms.IndexOf(form);
                var controlName = "Form" + idx;
                var formComponent = Entity.Create("console:formRenderControl").AsWritable<ControlOnForm>();
                formComponent.Alias = alias + controlName;
                formComponent.SetRelationships("console:renderingVerticalResizeMode", new EntityRelationshipCollection<IEntity> { Entity.Get<IEntity>("console:resizeFifty") }, Direction.Forward);
                formComponent.SetRelationships("console:renderingHorizontalResizeMode", new EntityRelationshipCollection<IEntity> { Entity.Get<IEntity>("console:resizeSpring") }, Direction.Forward);
                formComponent.SetRelationships("console:formToRender", new EntityRelationship<IEntity>(form).ToEntityRelationshipCollection(), Direction.Forward);

                if (reports.Count > idx)
                {
                    var contextProvider = containedControls.FirstOrDefault(c => c.Alias == (alias + "Report" + idx));
                    if (contextProvider != null)
                    {
                        formComponent.SetRelationships("console:receiveContextFrom", new EntityRelationship<IEntity>(contextProvider).ToEntityRelationshipCollection(), Direction.Forward);
                    }
                }

                containedControls.Add(formComponent);
                containers.Add(controlName, formComponent);
            }

            containedControls.Reverse();

            screen.ContainedControlsOnForm.AddRange(new EntityRelationshipCollection<ControlOnForm>(containedControls));
            screen.Save();

            Entity.Save(containedControls);

            return screen;
        }

        /// <summary>
        /// Creates a workflow with a resource input argument of the specified type.
        /// </summary>
        /// <param name="alias">The alias given to the workflow.</param>
        /// <param name="name">The name given to the workflow.</param>
        /// <param name="description">The description given to the workflow.</param>
        /// <param name="type">The type to assign to the input argument.</param>
        /// <param name="argumentName">The name given to the input argument.</param>
        /// <param name="isListArgument">Whether the input argument is a resource list argument.</param>
        /// <returns>The created workflow entity.</returns>
        public static Workflow CreateWorkflow(string alias, string name, string description, EntityType type, string argumentName, bool isListArgument = false)
        {
            var workflow = Entity.Create<Workflow>();
            workflow.Alias = alias;
            workflow.Name = name;
            workflow.Description = description;

            //
            // Exit
            //
            var exitPoint = Entity.Create<ExitPoint>();
            exitPoint.Name = "Exit Point";
            exitPoint.IsDefaultExitPoint = true;
            workflow.ExitPoints.Add(exitPoint);

            //
            // Input
            //
            var wfActivity = workflow.AsWritable<WfActivity>();
            ActivityArgument activityArgument;

            if (isListArgument)
            {
                var recordListArgument = Entity.Create<ResourceListArgument>();
                recordListArgument.Name = argumentName;
                recordListArgument.ConformsToType = type;

                activityArgument = recordListArgument.As<ActivityArgument>();
            }
            else
            {
                var recordArgument = Entity.Create<ResourceArgument>();
                recordArgument.Name = argumentName;
                recordArgument.ConformsToType = type;

                activityArgument = recordArgument.As<ActivityArgument>();
            }

            workflow.InputArguments.Add(activityArgument);

            var instance = new WfArgumentInstance { Name = argumentName, ArgumentInstanceActivity = wfActivity, ArgumentInstanceArgument = activityArgument };
            wfActivity.ArgumentInstanceFromActivity.Add(instance);
            workflow.ExpressionParameters.Add(instance);

            workflow.InputArgumentForAction = activityArgument;

            //
            // Log
            //
            var message = "'Input: ' + [" + argumentName + "].Name";

            var logActivity = Entity.Create<LogActivity>();
            logActivity.Name = "Log";

            var wfLogActivity = logActivity.As<WfActivity>();

            var logActivityType = wfLogActivity.IsOfType.Select(t => t.As<ActivityType>()).First(t => t != null);

            var logInput = logActivityType.InputArguments.FirstOrDefault(a => a.Name == "Message");

            var expression = Entity.Create<WfExpression>();
            expression.ExpressionString = message;
            expression.ArgumentToPopulate = logInput;
            expression.ExpressionInActivity = wfLogActivity;
            expression.IsTemplateString = false;

            logActivity.ExpressionMap.Add(expression);

            workflow.FirstActivity = wfLogActivity;
            workflow.ContainedActivities.Add(wfLogActivity);

            var logExit = logActivityType.ExitPoints.First(e => e.IsDefaultExitPoint ?? false);

            var termination = Entity.Create<Termination>();
            termination.Name = logExit.Name;
            termination.FromActivity = wfLogActivity;
            termination.FromExitPoint = logExit;
            termination.WorkflowExitPoint = exitPoint;

            wfLogActivity.ForwardTransitions.Add(termination.As<TransitionStart>());

            workflow.Terminations.Add(termination);

            return workflow;
        }

        /// <summary>
        /// Creates a report template against the specified type.
        /// </summary>
        /// <param name="alias">The alias given to the report template.</param>
        /// <param name="name">The name given to the report template.</param>
        /// <param name="description">The description given to the report template.</param>
        /// <param name="type">The type to apply the report template to.</param>
        /// <returns>The created report template entity.</returns>
        public static ReportTemplate CreateReportTemplate(string alias, string name, string description, EntityType type)
        {
            var template = Entity.Create<ReportTemplate>();
            template.Alias = alias;
            template.Name = name;
            template.Description = description;
            template.ReportTemplateAppliesToType = type;
            return template;
        }

        /// <summary>
        /// Creates an action menu item.
        /// </summary>
        /// <typeparam name="T">The action menu item implementation.</typeparam>
        /// <param name="name">The name of the menu item.</param>
        /// <param name="emptySelectName">The name to show when selection is empty.</param>
        /// <param name="multiSelectName">The name to show when there are multiple selections.</param>
        /// <param name="order">The ordinal of the menu item.</param>
        /// <param name="isMenu">True if the menu item has been or is to be included in the menu.</param>
        /// <param name="isContextMenu">True if the menu item shows on right click.</param>
        /// <param name="isButton">True if the menu item renders as a button.</param>
        /// <param name="isSeperator">True if this is just a separator for other items.</param>
        /// <param name="isSystem">True if this is a system reserved action menu item.</param>
        /// <param name="singleSelect">True if this menu item applies to single selection.</param>
        /// <param name="multiSelect">True if this menu item applies to multi selection.</param>
        /// <param name="requiredPermissions">Any permission required by this item to appear.</param>
        /// <param name="method">The method string indicating item behavior.</param>
        /// <param name="state">The state information modifying behaviour of the method above.</param>
        /// <param name="target">Optional specific information about the target this item should apply to.</param>
        /// <returns>The created action menu item entity.</returns>
        public static T CreateActionItem<T>(string name, string emptySelectName, string multiSelectName, int? order,
            bool isMenu, bool isContextMenu, bool isButton, bool isSeperator, bool isSystem,
            bool singleSelect, bool multiSelect, string[] requiredPermissions,
            string method, string state, string target) where T : ActionMenuItem
        {
            var action = Entity.Create<T>();
            action.Name = name;
            action.EmptySelectName = emptySelectName;
            action.MultiSelectName = multiSelectName;
            action.MenuOrder = order;
            action.IsActionItem = isMenu;
            action.IsContextMenu = isContextMenu;
            action.IsActionButton = isButton;
            action.IsMenuSeparator = isSeperator;
            action.IsSystem = isSystem;
            action.AppliesToSelection = singleSelect;
            action.AppliesToMultiSelection = multiSelect;
            action.HtmlActionMethod = method;
            action.HtmlActionState = state;
            action.HtmlActionTarget = target;
            foreach (var p in requiredPermissions)
            {
                action.ActionRequiresPermission.Add(Entity.Get<Permission>(p));
            }
            return action;
        }
    }
}
