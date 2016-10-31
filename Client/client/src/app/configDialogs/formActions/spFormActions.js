// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spEntity, jsonRelationship, jsonBool, jsonString, jsonLookup */

(function () {
    'use strict';

    //
    // spFormActionsDialogController
    //
    // Controller for the dialog used to edit action config in Form Builder.
    //
    // @module mod.app.configureDialogs.spFormActionsDialog.
    // @example
    //

    angular.module('mod.app.configureDialogs.spFormActionsDialog', ['mod.common.ui.spDialogService', 'mod.common.alerts', 'mod.common.ui.spTabs', 'mod.common.ui.spActionsService', 'mod.common.spEntityService', 'mod.common.spMobile'])
        .controller('spFormActionsDialogController', function ($q, $scope, $rootScope, $uibModalInstance, options, spActionsService, spEntityService, spAlertsService, spMobileContext) {

            // templates
            var isEnabledCheckboxCellTemplate = '<div style="padding-left:10px;"><input type="checkbox" ng-model="row.entity.isenabled" ng-hide="row.entity.eid < 0" ng-disabled="row.entity.canEditIsEnabled === false" ng-change="tab.changeIsEnabled(row)" /></div>';
            //var isButtonCheckboxCellTemplate = '<div style="padding-left:10px;"><input type="checkbox" ng-model="row.entity.isbutton" ng-disabled="!row.entity.isenabled || row.entity.canEditIsButton === false" /></div>';
            var imageCellTemplate = '<div><div class="action-icon" style="background-image:url(\'{{row.entity.icon}}\');"></div></div>';

            var columns = [
                { field: 'icon', displayName: 'Icon', cellTemplate: imageCellTemplate, width: 'auto' },
                { field: 'displayname', displayName: 'Label' },
                { field: 'isenabled', displayName: 'Enable', cellTemplate: isEnabledCheckboxCellTemplate }
                //{ field: 'isbutton', displayName: 'Show Button', cellTemplate: isButtonCheckboxCellTemplate }
            ];

            var isMobile = spMobileContext.isMobile;

            // tabs
            var tabInstanceActions = createTab('Record Actions', 'configDialogs/formActions/spFormInstanceActions.tpl.html');
            $scope.tabs = [tabInstanceActions];
            $scope.tab = tabInstanceActions;

            // load up form actions
            $scope.$watch('options', function () {
                $scope.model = $scope.model || {};
                $scope.model.options = options;

                var formControl = $scope.model.options.formControlEntity;
                var entityTypeId = $scope.model.options.entityTypeId;

                var formId = $scope.model.options.formId;
                var request = {
                    formId: formId,
                    entityTypeId: entityTypeId,
                    data: {},
                    display: 'all',
                    hostIds: [formId]

                };

                var promise = spActionsService.getFormActionsMenu(request).then(function (response) {
                    var actions = response.actions;

                    // Record Actions
                    var recordActions = _.filter(actions, function (action) {
                        return action.iscontext || action.state === 'createForm';
                    });

                    _.forEach(recordActions, function (recordAction) {
                        if (recordAction.state !== 'createForm' && !recordAction.displayname) {
                            recordAction.displayname = recordAction.emptyname || recordAction.multiname || recordAction.name;
                        }
                    });

                    tabInstanceActions.items = _.sortBy(recordActions, ['order', 'displayname']);
                }, function (error) {
                    console.error('spFormActionsDialogController error:', error.data.message);
                    spAlertsService.addAlert('Failed to get action menu.', 'error');
                    $uibModalInstance.close(null);
                });

                if (formControl) {
                    loadBehaviorData(promise, formControl);
                }
            });

            $scope.$on('signedout', function () {
                $scope.cancel();
            });

            function loadBehaviorData(promise, actionHost) {
                var promises = [];
                if (promise) {
                    promises.push(promise);
                }

                // no behavior info yet in memory
                var query = '{ k:resourceConsoleBehavior }' +
                                '.{name, alias, k:suppressActionsForType, k:behaviorActionMenu.{ ' +
                                '   k:showNewActionsButton, k:showExportActionsButton, ' +
                                '   { k:menuItems, k:suppressedActions, k:includeActionsAsButtons }.{ ' +
                                '      { name, ' +
                                '        k:menuIconUrl, ' +
                                '        k:isActionButton, ' +
                                '        k:isMenuSeparator, ' +
                                '        { k:actionMenuItemToWorkflow }.{ name }, ' +
                                '        { k:actionMenuItemToReportTemplate }.{ name } ' +
								'      } ' +
                                '   }, ' +
                                '   { k:includeTypesForNewButtons, k:suppressedTypesForNewMenu }.id ' +
                                '}}';

                promises.push(spEntityService.getEntity(actionHost.id(), query).then(function (actionData) {
                    if (actionData.resourceConsoleBehavior) {
                        spEntity.augment(actionHost, actionData);
                    }
                }));

                $q.all(promises).then(function () {
                    if (actionHost.resourceConsoleBehavior) {
                        // update the row checks
                        displayBehaviorOnTab(actionHost.resourceConsoleBehavior, tabInstanceActions);
                    }
                });
            }

            function displayBehaviorOnTab(behavior, tab) {
                if (behavior) {
                    _.forEach(tab.items, function (item) {
                        if (item && (item.id > 0 || item.eid > 0)) {

                            // Process 'most' actions here...
                            if (item.method !== 'run' && item.method !== 'generate') {
                                var check = false;
                                if (item.state !== 'createForm') {
                                    // look to see if we have suppressed this item...
                                    check = _.some(behavior.behaviorActionMenu.suppressedActions, function (suppressed) {
                                        return suppressed.id() === item.id;
                                    });

                                    item.isenabled = !check;

                                    // include actions as buttons
                                    check = _.some(behavior.behaviorActionMenu.includeActionsAsButtons, function (included) {
                                        return included.id() === item.id;
                                    });

                                    item.isbutton = item.isenabled && check;
                                }
                            }

                            // Process actions related to a workflow
                            if (item.method === 'run') {
                                updateActionItemWithRelatedEntity(item, behavior,
                                    function (wf) { return wf.actionMenuItemToWorkflow; },
                                    function (related) { return related.idP === item.eid; });
                            }
                        }

                        // Process actions related to a report template
                        if (item.method === 'generate') {
                            updateActionItemWithRelatedEntity(item, behavior,
                                function (rt) { return rt.actionMenuItemToReportTemplate; },
                                function (related) { return item.data && item.data.ReportTemplateId && related.idP === item.data.ReportTemplateId; });
                        }
                    });
                }
            }

            function updateBehaviorFromTab(behavior, tab) {
                if (behavior) {
                    _.forEach(tab.items, function (item) {
                        if (item && (item.id > 0 || item.eid > 0)) {
                            if (item.method !== 'run') {
                                // suppressed action items
                                var suppressed = _.find(behavior.behaviorActionMenu.suppressedActions, function (s) {
                                    return s.idP === item.id;
                                });
                                if (!item.isenabled && !suppressed) {
                                    behavior.behaviorActionMenu.suppressedActions.add(item.id);
                                }
                                if (item.isenabled && suppressed) {
                                    behavior.behaviorActionMenu.suppressedActions.remove(item.id);
                                }

                                // include actions as buttons
                                var included = _.find(behavior.behaviorActionMenu.includeActionsAsButtons, function (i) {
                                    return i.idP === item.id;
                                });
                                if (!item.isbutton && included) {
                                    behavior.behaviorActionMenu.includeActionsAsButtons.remove(item.id);
                                }
                                if (item.isbutton && item.isenabled && !included) {
                                    behavior.behaviorActionMenu.includeActionsAsButtons.add(item.id);
                                }
                            }

                            // Process actions related to a workflow
                            if (item.method === 'run') {
                                updateBehaviorForRelatedEntityActionItem(item, behavior,
                                    function (wf) { return wf.actionMenuItemToWorkflow; },
                                    function (r) { return r.idP === item.eid; },
                                    createWorkflowActionMenuItem);
                            }
                        }

                        // Process actions related to a report template
                        if (item.method === 'generate') {
                            updateBehaviorForRelatedEntityActionItem(item, behavior,
                                function (rt) { return rt.actionMenuItemToReportTemplate; },
                                function (r) { return item.data && item.data.ReportTemplateId && r.idP === item.data.ReportTemplateId; },
                                createDocumentGenerationActionMenuItem);
                        }
                    });
                }
            }

            // ok
            $scope.ok = function () {
                var formControl = $scope.model.options.formControlEntity;
                //TODO: in FormAction control, don't need report object. this line should be removed.
                var report = $scope.model.options.reportEntity;
                var actionHost = formControl ? formControl : report;
                var showNewMenu = false;
                var actionButtons = [];

                if (actionHost) {
                    if (!actionHost.resourceConsoleBehavior) {
                        augmentHostEntity(actionHost);
                    }

                    //if actionhost is formControl, the resourceConsoleBehavior.suppressActionsForType should be false. if it is true will block load standard resource behavior items e.g. View, Edit, Delete
                    if (formControl && actionHost.resourceConsoleBehavior && actionHost.resourceConsoleBehavior.suppressActionsForType)
                    {
                        actionHost.resourceConsoleBehavior.suppressActionsForType = false;
                    }

                    updateBehaviorFromTab(actionHost.resourceConsoleBehavior, tabInstanceActions);

                    // send back if the "new" menu should be visible
                    var rowNewAll = _.find(tabInstanceActions.items, function (item) {
                        return item.id === -1 && item.eid === -1;
                    });
                    if (rowNewAll) {
                        showNewMenu = isMobile || (rowNewAll.isbutton === true);
                        actionHost.resourceConsoleBehavior.behaviorActionMenu.showNewActionsButton = showNewMenu;
                    }

                    // send back the visible action buttons
                    var m = tabInstanceActions.items;
                    actionButtons = _.filter(m, function (item) {
                        return item.isenabled && item.isbutton && item.method;
                    });
                }

                $uibModalInstance.close({
                    actionButtons: actionButtons
                });
            };

            // cancel
            $scope.cancel = function () {
                $uibModalInstance.close(null);
            };

            ////
            // Helpers
            ////

            function augmentHostEntity(actionHost) {

                if (_.isUndefined(actionHost.resourceConsoleBehavior)) {
                    var template = spEntity.fromJSON({
                        'console:resourceConsoleBehavior': jsonLookup(createEmptyBehavior())
                    });
                    spEntity.augmentSingle(actionHost, template);
                    template.resourceConsoleBehavior.name = actionHost.name + '_rcb';
                    template.resourceConsoleBehavior.behaviorActionMenu.name = actionHost.name + '_rcb_menu';
                } else {
                    actionHost.resourceConsoleBehavior = createEmptyBehavior();
                    actionHost.resourceConsoleBehavior.name = actionHost.name + '_rcb';
                    actionHost.resourceConsoleBehavior.behaviorActionMenu.name = actionHost.name + '_rcb_menu';
                }

            }

            function updateActionItemWithRelatedEntity(item, behavior, fnGetRel, fnGetRelItem) {
                var check = false;
                var r;

                check = _.some(behavior.behaviorActionMenu.menuItems, function (relatedItem) {
                    var f = _.some(fnGetRel(relatedItem), fnGetRelItem);
                    if (f === true) {
                        r = relatedItem;
                    }
                    return f;
                });

                item.isenabled = check;
                item.isbutton = true;   // *** all form actions are set to show as button
            }

            function updateBehaviorForRelatedEntityActionItem(item, behavior, fnGetRel, fnGetRelItem, fnCreate) {
                var relatedItem = _.find(behavior.behaviorActionMenu.menuItems, function (menuItem) {
                    return _.some(fnGetRel(menuItem), fnGetRelItem);
                });

                if (!item.isenabled && relatedItem) {
                    behavior.behaviorActionMenu.menuItems.deleteEntity(relatedItem);
                }

                if (item.isenabled && !relatedItem) {
                    relatedItem = fnCreate(item);
                    behavior.behaviorActionMenu.menuItems.add(relatedItem);
                }

                // align ids
                if (!item.id && relatedItem) {
                    item.id = relatedItem.idP;
                }

                // deal with button
                var includeRelated = _.find(behavior.behaviorActionMenu.includeActionsAsButtons, function (i) {
                    return i.idP === item.id;
                });


                // ** all form actions are marked as buttons 
                if (item.isenabled && !includeRelated) {
                    item.isbutton = true;
                    behavior.behaviorActionMenu.includeActionsAsButtons.add(item.id);
                }
            }

            function createEmptyBehavior() {
                return spEntity.fromJSON({
                    typeId: 'console:consoleBehavior',
                    'console:suppressActionsForType': true,     // ** supressing actions for form type. 
                    'console:behaviorActionMenu': jsonLookup(createEmptyActionMenu())
                });
            }

            function createEmptyActionMenu() {
                return spEntity.fromJSON({
                    typeId: 'console:actionMenu',
                    'console:showNewActionsButton': jsonBool(),
                    'console:showExportActionsButton': jsonBool(),
                    'console:menuItems': jsonRelationship(),
                    'console:suppressedActions': jsonRelationship(),
                    'console:suppressedTypesForNewMenu': jsonRelationship(),
                    'console:includeActionsAsButtons': jsonRelationship(),
                    'console:includeTypesForNewButtons': jsonRelationship()
                });
            }

            function createWorkflowActionMenuItem(info) {
                return spEntity.fromJSON({
                    typeId: 'console:workflowActionMenuItem',
                    name: info.name,
                    'console:menuIconUrl': info.icon,
                    'console:menuOrder': info.order,
                    'console:isActionButton': info.isbutton,
                    'console:isMenuSeparator': info.isseparator,
                    'console:isContextMenu': info.iscontext,
                    'console:isActionItem': info.ismenu,
                    'console:isSystem': info.issystem,
                    'console:appliesToSelection': info.isselect,
                    'console:appliesToMultiSelection': info.ismultiselect,
                    'htmlActionMethod': jsonString(info.method),
                    'htmlActionState': jsonString(info.state),
                    'console:actionMenuItemToWorkflow': [info.eid]
                });
            }

            function createDocumentGenerationActionMenuItem(info) {
                var rtid = 0;
                if (info && info.data && info.data.ReportTemplateId) {
                    rtid = info.data.ReportTemplateId;
                }
                return spEntity.fromJSON({
                    typeId: 'console:generateDocumentActionMenuItem',
                    name: info.name,
                    'console:menuIconUrl': info.icon,
                    'console:menuOrder': info.order,
                    'console:isActionButton': info.isbutton,
                    'console:isMenuSeparator': info.isseparator,
                    'console:isContextMenu': info.iscontext,
                    'console:isActionItem': info.ismenu,
                    'console:isSystem': info.issystem,
                    'console:appliesToSelection': info.isselect,
                    'console:appliesToMultiSelection': info.ismultiselect,
                    'htmlActionMethod': jsonString(info.method),
                    'htmlActionState': jsonString(info.state),
                    'console:actionMenuItemToReportTemplate': [rtid]
                });
            }

            // Create a tab object for inclusion in the dialog
            function createTab(label, url) {
                return {
                    name: label,
                    url: url,
                    items: [],
                    canMoveUp: false,
                    canMoveDown: false,

                    // Move an action up in the menu by it's ordinal
                    up: function () {
                        var tab = this;

                        if (!tab.canMoveUp) {
                            return;
                        }

                        var selectedItem = tab.gridOptions.selectedItems[0];
                        var n = _.indexOf(tab.items, selectedItem);
                        if (n > 0 && tab.items.length > n) {
                            var moveAboveItem = tab.items[n - 1];
                            if (moveAboveItem) {
                                var ordinal = moveAboveItem.order;
                                if (!_.isNumber(ordinal) || ordinal <= 0) {
                                    selectedItem.order = 0;
                                } else {
                                    selectedItem.order = ordinal - 1;
                                }

                                tab.items = _.sortBy(tab.items, ['order', 'name']);

                                tab.updateMoveButtons();
                            }
                        }
                    },

                    // Move an action down in the menu by it's ordinal
                    down: function () {
                        var tab = this;

                        if (!tab.canMoveDown) {
                            return;
                        }

                        var selectedItem = tab.gridOptions.selectedItems[0];
                        var n = _.indexOf(tab.items, selectedItem);
                        if (n >= 0 && tab.items.length > (n + 1)) {
                            var moveBelowItem = tab.items[n + 1];
                            if (moveBelowItem) {
                                var ordinal = moveBelowItem.order;
                                if (_.isNumber(ordinal) && (selectedItem.order <= ordinal)) {
                                    selectedItem.order = ordinal + 1;

                                    tab.items = _.sortBy(tab.items, ['order', 'name']);

                                    tab.updateMoveButtons();
                                }
                            }
                        }
                    },

                    // Track the current tab
                    selectionChanged: function (tab) {
                        $scope.tab = tab;
                    },

                    //// prevent showing a button, if the option is not in the menu
                    //changeIsEnabled: function (row) {
                    //    if (row && row.entity && row.entity.isenabled === false) {
                    //        //row.entity.isbutton = false;
                    //    }
                    //},

                    // Show/hide the move buttons
                    updateMoveButtons: function () {
                        var tab = this;

                        tab.canMoveUp = false;
                        tab.canMoveDown = false;

                        var selectedItem = tab.gridOptions.selectedItems[0];

                        if (!selectedItem || selectedItem.issystem) {
                            return;
                        }

                        var idx = _.indexOf(tab.items, selectedItem);
                        var len = tab.items.length;

                        if (idx > 0 && len > idx) {
                            var itemAbove = tab.items[idx - 1];
                            if (itemAbove && !itemAbove.issystem) {
                                var orderAbove = itemAbove.order;
                                if (_.isNumber(orderAbove) && (selectedItem.order >= 0)) {
                                    tab.canMoveUp = true;
                                }
                            }
                        }
                        if (idx >= 0 && len > (idx + 1)) {
                            var itemBelow = tab.items[idx + 1];
                            if (itemBelow && !itemBelow.issystem) {
                                var orderBelow = itemBelow.order;
                                if (_.isNumber(orderBelow) && (selectedItem.order <= orderBelow)) {
                                    tab.canMoveDown = true;
                                }
                            }
                        }
                    },

                    gridOptions: {
                        data: 'tabItem.items',
                        columnDefs: columns,
                        enableSorting: false,
                        multiSelect: false,
                        selectedItems: [],
                        afterSelectionChange: function (data) {
                            if (data && data.selected) {
                                $scope.tab.updateMoveButtons();
                            }
                        }
                    }
                };
            }

        })
        .factory('spFormActionsDialog', function (spDialogService) {
            // setup
            var exports = {

                showModalDialog: function (options, defaultOverrides) {
                    var dialogDefaults = {
                        title: 'Actions',
                        keyboard: true,
                        backdropClick: true,
                        windowClass: 'modal formactionsdialog-view',
                        templateUrl: 'configDialogs/formActions/spFormActionsDialog.tpl.html',
                        controller: 'spFormActionsDialogController',
                        resolve: {
                            options: function () {
                                return options;
                            }
                        }
                    };

                    if (defaultOverrides) {
                        angular.extend(dialogDefaults, defaultOverrides);
                    }

                    return spDialogService.showModalDialog(dialogDefaults);
                }
            };

            return exports;
        });
}());