// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Services.Console
{
    /// <summary>
    /// Helpers for the ActionService.
    /// </summary>
    static class ActionServiceHelpers
    {
        /// <summary>
        /// Defines preload content for loading console behaviors.
        /// </summary>
        private const string BehaviorRequest = @"
            let @EXPRESSION = {
                isOfType.id,
                k:actionExpressionString,
                k:actionExpressionEntities.{
                    isOfType.name,
                    name,
                    referencedEntity.{isOfType.name, name}
                }
            }
            let @WORKFLOW = {
                isOfType.name,
                name,
                inputArgumentForRelatedResource.name
            }
            let @REPORTTEMPLATE = {
                isOfType.name,
                name
            }
            let @MENUITEM = {
	            isOfType.id,
	            alias, name, description,
	            htmlActionTarget, htmlActionMethod, htmlActionState,
	            k:menuOrder,
	            multiSelectName, emptySelectName,
	            k:menuIconUrl,
	            k:isMenuSeparator, k:isActionButton, k:isActionItem, k:isContextMenu, k:isSystem,
	            k:appliesToSelection, k:appliesToMultiSelection,
	            k:actionRequiresPermission.isOfType.id,
                k:actionRequiresExpression.@EXPRESSION,
                k:actionMenuItemToWorkflow.@WORKFLOW,
                k:actionMenuItemToReportTemplate.@REPORTTEMPLATE
            }
            let @MENU = {
	            isOfType.id,
	            k:showNewActionsButton,
                k:suppressNewActions,
	            k:showExportActionsButton,
                k:showEditInlineActionsButton,
	            k:menuItems.@MENUITEM,
	            k:suppressedActions.id, k:suppressedTypesForNewMenu.id, k:includeActionsAsButtons.id, k:includeTypesForNewButtons.id
            }
            let @BEHAVIOR = {
	            isOfType.id,
                k:html5CreateId,
	            k:suppressActionsForType,
	            k:behaviorActionMenu.@MENU
            }
            ";

        /// <summary>
        /// Defines preload content for loading the report actions menu.
        /// </summary>
        internal const string TypeInfoRequest = @"
            let @TYPEINFO = {
	            isOfType.id,
                alias,
                name, isAbstract,
	            k:defaultEditForm.isOfType.id,
	            reportTemplatesApplyToType.{ name, isOfType.id },
	            { k:typeConsoleBehavior, k:selectionBehavior, k:resourceConsoleBehavior }.@BEHAVIOR
            }
            let @ANCESTORS = {
	            isOfType.id,
	            inherits.@TYPEINFO,
	            inherits.@ANCESTORS		
            }
            let @DERIVED = {
	            isOfType.id,
	            derivedTypes.@TYPEINFO,
	            derivedTypes.@DERIVED		
            }
            ";

        /// <summary>
        /// Defines preload content for loading the report actions menu.
        /// </summary>
        internal const string ReportRequest = BehaviorRequest + TypeInfoRequest + @"
            let @NODE = {
	            isOfType.id,
	            resourceReportNodeType.@TYPEINFO,
	            resourceReportNodeType.@ANCESTORS,
	            resourceReportNodeType.@DERIVED,
	            groupedNode.@NODE
            }
            let @FORM = {
	            isOfType.id,
	            k:typeToEditWithForm.isOfType.id
            }
            let @REPORT = {
	            isOfType.name,
	            name,
	            { k:selectionBehavior, k:resourceConsoleBehavior }.@BEHAVIOR,
	            resourceViewerConsoleForm.@FORM,	
	            rootNode.@NODE
            }
            @REPORT";

        /// <summary>
        /// Defines preload content for loading the form actions menu.
        /// </summary>
        internal const string FormRequest = BehaviorRequest + TypeInfoRequest + @"            
            let @FORM = {
	            name,
                isOfType.name,
                k:typeToEditWithForm.name,
	            { k:selectionBehavior, k:resourceConsoleBehavior }.@BEHAVIOR
            }
            @FORM";

        /// <summary>
        /// Defines preload content for loading the actions menu for a report base type
        /// </summary>
        internal const string ReportBaseTypeRequest = BehaviorRequest + TypeInfoRequest + @"
            name,
            alias,
            isAbstract,
            k:defaultEditForm.id,
            reportTemplatesApplyToType.{ name, isOfType.id },
            { k:typeConsoleBehavior, k:selectionBehavior, k:resourceConsoleBehavior }.@BEHAVIOR,
            isOfType.name,
            inherits.@ANCESTORS,
            inherits.@TYPEINFO,
            derivedTypes.@DERIVED,
            derivedTypes.@TYPEINFO";

        /// <summary>
        /// Defines preload content for loading the actions menu for an individual resource.
        /// </summary>
        internal const string ResourceRequest = BehaviorRequest + @"
            name,
            k:resourceConsoleBehavior.@BEHAVIOR,
            isOfType.{ name, k:typeConsoleBehavior.@BEHAVIOR }";

        /// <summary>
        /// Defines preload content for loading the actions menu for a viewer of resources, such as a tab relationship control, etc.
        /// Note: Ideally the ReportRequest would directly reference this, but it can't do so conveniently.
        /// </summary>
        internal const string ResourceViewerRequest = BehaviorRequest + @"
            isOfType.id,
            name,
            { k:selectionBehavior, k:resourceConsoleBehavior }.@BEHAVIOR,
            resourceViewerConsoleForm.{ isOfType.id, k:typeToEditWithForm.id }";

        /// <summary>
        /// Defines preload content for determining which workflows apply to an action menu.
        /// </summary>
        internal const string WorkflowRequest = @"
            let @WORKFLOWRQ = {
                isOfType.id,
                name,
                inputArgumentForAction.{ name, isOfType.id, conformsToType.id },
                wfNewerVersion.{ name, isOfType.id }
            }
            isOfType.id,
            name,
            instancesOfType.@WORKFLOWRQ";

    }
}
