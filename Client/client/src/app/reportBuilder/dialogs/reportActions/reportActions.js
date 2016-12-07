// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spEntity, jsonRelationship, jsonBool, jsonString, jsonLookup */

(function () {
    'use strict';

    //
    // spReportActionsDialogController
    //
    // Controller for the dialog used to edit action config in Report Builder.
    //
    // @module mod.app.reportActionsDialog
    // @example
    //

    angular.module('mod.app.reportActionsDialog', ['mod.common.ui.spDialogService', 'mod.common.alerts', 'mod.common.ui.spTabs', 'mod.common.ui.spActionsService', 'mod.common.spEntityService', 'mod.common.spMobile'])
        .controller('spReportActionsDialogController', function ($q, $scope, $rootScope, $uibModalInstance, options, spActionsService, spEntityService, spAlertsService, spMobileContext) {

            // options gives us:
            // { hostEntity (eg the reportControl or chartControl if coming from screen builder, reportEntity if just a sole report) }

            // templates
            var isEnabledCheckboxCellTemplate = '<div style="padding-left:10px;"><input type="checkbox" ng-model="row.entity.isenabled" ng-hide="row.entity.eid < 0" ng-disabled="row.entity.canEditIsEnabled === false" ng-change="tab.changeIsEnabled(row)" /></div>';
            var isButtonCheckboxCellTemplate = '<div style="padding-left:10px;"><input type="checkbox" ng-model="row.entity.isbutton" ng-disabled="!row.entity.isenabled || row.entity.canEditIsButton === false" /></div>';
            var imageCellTemplate = '<div><div class="action-icon" style="background-image:url(\'{{row.entity.icon}}\');"></div></div>';

            var columns = [
                { field: 'icon', displayName: 'Icon', cellTemplate: imageCellTemplate, width: 'auto' },
                { field: 'displayname', displayName: 'Label' },
                { field: 'isenabled', displayName: 'Enable', cellTemplate: isEnabledCheckboxCellTemplate },
                { field: 'isbutton', displayName: 'Show Button', cellTemplate: isButtonCheckboxCellTemplate }
            ];

            var isMobile = spMobileContext.isMobile;

            // tabs
            var tabInstanceActions = createTab('Record Actions', 'reportBuilder/dialogs/reportActions/reportInstanceActions.tpl.html');
            var tabActions = createTab('Report Actions', 'reportBuilder/dialogs/reportActions/reportActions.tpl.html');
            $scope.tabs = [tabInstanceActions, tabActions];
            $scope.tab = tabInstanceActions;

            // load up report actions
            $scope.$watch('options', function () {
                $scope.model = $scope.model || {};
                $scope.model.options = options;

                var hostIds = [];
                var hostTypeIds = [];
                var formControl = $scope.model.options.formControlEntity;
                var report = $scope.model.options.reportEntity;
                var entityTypeId = $scope.model.options.entityTypeId;

                // retrieve ALL applicable actions from service, to then overlay our settings on to
                if (formControl) {
                    if (formControl.dataState !== spEntity.DataStateEnum.Create) {
                        hostIds = [formControl.idP];
                    }

                    hostTypeIds = _.map(formControl.typesP, 'idP');
                }

                var reportId = report ? report.idP : $scope.model.options.reportId;
                var request = {
                    reportId: reportId,
                    hostIds: hostIds,
                    hostTypeIds: hostTypeIds,
                    entityTypeId: entityTypeId,
                    data: {},
                    display: 'all'
                };

                var promise = spActionsService.getActionsMenu(request).then(function (response) {
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

                    // Report Actions
                    var reportActions = _.filter(actions, function (action) {
                        return !action.iscontext && action.state !== 'createForm';
                    });

                    _.forEach(reportActions, function(reportAction) {
                        if (!reportAction.displayname) {
                            reportAction.displayname = reportAction.emptyname || reportAction.multiname || reportAction.name;
                        }
                    });

                    tabActions.items = _.sortBy(reportActions, ['order', 'displayname']);
                }, function (error) {
                    console.error('spReportActionsDialogController error:', error.data.message);
                    spAlertsService.addAlert('Failed to get action menu.', 'error');
                    $uibModalInstance.close(null);
                });

                if (formControl || report) {
                    var host = formControl ? formControl : report;
                    loadBehaviorData(promise, host);
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
                if (!actionHost.resourceConsoleBehavior || !actionHost.selectionBehavior) {
                    var query = '{ k:resourceConsoleBehavior, k:selectionBehavior }' +
                                '.k:behaviorActionMenu.{ ' +
                                '   k:showNewActionsButton, k:showExportActionsButton, k:showEditInlineActionsButton, ' +
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
                                '}';

                    promises.push(spEntityService.getEntity(actionHost.id(), query).then(function (actionData) {
                        if (actionData.resourceConsoleBehavior && actionData.selectionBehavior) {
                            spEntity.augment(actionHost, actionData);
                        }
                    }));
                }


                $q.all(promises).then(function () {
                    if (actionHost.resourceConsoleBehavior && actionHost.selectionBehavior) {
                        // update the row checks
                        displayBehaviorOnTab(actionHost.resourceConsoleBehavior, tabInstanceActions);
                        displayBehaviorOnTab(actionHost.selectionBehavior, tabActions);

                        // update the new menu "button" visibility
                        var rowNewAll = _.find(tabInstanceActions.items, function (item) {
                            return item.id === -1;
                        });
                        if (rowNewAll && actionHost.resourceConsoleBehavior && actionHost.selectionBehavior) {
                            var showNewMenu = actionHost.resourceConsoleBehavior.behaviorActionMenu.showNewActionsButton;
                            rowNewAll.isbutton = (showNewMenu === true);
                        }

                        // update the export menu "button" visibility
                        var rowExportAll = _.find(tabActions.items, function(item) {
                            return item.id === -2;
                        });
                        if (rowExportAll && actionHost.resourceConsoleBehavior && actionHost.selectionBehavior) {
                            var showExportMenu = actionHost.selectionBehavior.behaviorActionMenu.showExportActionsButton;
                            rowExportAll.isbutton = (showExportMenu === true);
                        }

                        // update the edit inline button visibility
                        var editInlineItem = _.find(tabActions.items, function(item) {
                            return item.id === -3;
                        });
                        if (editInlineItem && actionHost.resourceConsoleBehavior && actionHost.selectionBehavior) {
                            var showEditInlineButton = actionHost.selectionBehavior.behaviorActionMenu.showEditInlineActionsButton !== false;
                            editInlineItem.isbutton = showEditInlineButton === true;
                        }
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

                                if (item.state === 'createForm') {
                                    // look to see if any types have been suppressed for the "new" menu
                                    check = _.some(behavior.behaviorActionMenu.suppressedTypesForNewMenu, function (suppressed) {
                                        return suppressed.id() === item.eid;
                                    });

                                    item.isenabled = !check;

                                    // include types as buttons for the "new" menu
                                    check = _.some(behavior.behaviorActionMenu.includeTypesForNewButtons, function (included) {
                                        return included.id() === item.eid;
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
                                if (item.state === 'createForm') {
                                    // suppressed types for "new" actions
                                    var suppressedType = _.find(behavior.behaviorActionMenu.suppressedTypesForNewMenu, function (t) {
                                        return t.idP === item.eid;
                                    });
                                    if (!item.isenabled && !suppressedType) {
                                        behavior.behaviorActionMenu.suppressedTypesForNewMenu.add(item.eid);
                                    }
                                    if (item.isenabled && suppressedType) {
                                        behavior.behaviorActionMenu.suppressedTypesForNewMenu.remove(item.eid);
                                    }

                                    // include types as buttons for "new" actions
                                    var includedType = _.find(behavior.behaviorActionMenu.includeTypesForNewButtons, function (n) {
                                        return n.idP === item.eid;
                                    });
                                    if (!item.isbutton && includedType) {
                                        behavior.behaviorActionMenu.includeTypesForNewButtons.remove(item.eid);
                                    }
                                    if (item.isbutton && item.isenabled && !includedType) {
                                        behavior.behaviorActionMenu.includeTypesForNewButtons.add(item.eid);
                                    }
                                } else {
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
                var report = $scope.model.options.reportEntity;
                var actionHost = formControl ? formControl : report;
                var showNewMenu = false;
                var showExportMenu = false;
                var showEditInlineButton = false;
                var newMenuItems = [];
                var exportMenuItems = [];
                var actionButtons = [];

                if (actionHost) {
                    if (!actionHost.resourceConsoleBehavior || !actionHost.selectionBehavior) {
                        augmentHostEntity(actionHost);
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

                    updateBehaviorFromTab(actionHost.selectionBehavior, tabActions);

                    // send back if the "export" menu should be visible
                    var rowExportAll = _.find(tabActions.items, function(item) {
                        return item.id === -2 && item.eid === -1;
                    });
                    if (rowExportAll) {
                        showExportMenu = rowExportAll.isbutton === true;
                        actionHost.selectionBehavior.behaviorActionMenu.showExportActionsButton = showExportMenu;
                    }

                    // send back the new contents of the "new" menu
                    newMenuItems = _.filter(tabInstanceActions.items, function (item) {
                        return item.eid > 0 && item.state === 'createForm' && item.isenabled && showNewMenu;
                    });

                    // send back if the "edit inline" button should be visible
                    var editInlineItem = _.find(tabActions.items, function(item) {
                        return item.id === -3 && item.eid === -1;
                    });
                    if (editInlineItem) {
                        showEditInlineButton = editInlineItem.isbutton === true;
                        actionHost.selectionBehavior.behaviorActionMenu.showEditInlineActionsButton = showEditInlineButton;
                    }

                    // hack the names, so what we pass back is consistent (for "new" only thankfully)
                    if (newMenuItems) {
                        _.forEach(newMenuItems, function(item) {
                            //item.name = item.buttonname;
                        });
                    }

                    exportMenuItems = _.filter(tabActions.items, function(item) {
                        return item.method === 'export' && item.isenabled && showExportMenu;
                    });

                    // send back the visible action buttons
                    var m = tabInstanceActions.items.concat(tabActions.items);
                    actionButtons = _.filter(m, function (item) {
                        return item.isenabled && item.isbutton && item.method;
                    });
                }

                $uibModalInstance.close({
                    showNewMenu: showNewMenu && _.some(newMenuItems),
                    showExportMenu: showExportMenu && _.some(exportMenuItems),
                    showEditInlineButton: showEditInlineButton,
                    newMenu: newMenuItems,
                    exportMenu: exportMenuItems,
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

                if (_.isUndefined(actionHost.resourceConsoleBehavior) || _.isUndefined(actionHost.selectionBehavior)) {
                    var template = spEntity.fromJSON({
                        'console:resourceConsoleBehavior': jsonLookup(createEmptyBehavior()),
                        'console:selectionBehavior': jsonLookup(createEmptyBehavior())
                    });
                    spEntity.augmentSingle(actionHost, template);
                    template.resourceConsoleBehavior.name = actionHost.name + '_rcb';
                    template.resourceConsoleBehavior.behaviorActionMenu.name = actionHost.name + '_rcb_menu';
                    template.selectionBehavior.name = actionHost.name + '_sb';
                    template.selectionBehavior.behaviorActionMenu.name = actionHost.name + '_sb_menu';
                } else {
                    actionHost.resourceConsoleBehavior = createEmptyBehavior();
                    actionHost.resourceConsoleBehavior.name = actionHost.name + '_rcb';
                    actionHost.resourceConsoleBehavior.behaviorActionMenu.name = actionHost.name + '_rcb_menu';
                    actionHost.selectionBehavior = createEmptyBehavior();
                    actionHost.selectionBehavior.name = actionHost.name + '_sb';
                    actionHost.selectionBehavior.behaviorActionMenu.name = actionHost.name + '_sb_menu';
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
                if (!item.isenabled) {
                    item.isbutton = false;
                }

                if (r) {
                    item.id = r.idP;

                    // as a button?
                    check = _.some(behavior.behaviorActionMenu.includeActionsAsButtons, function (included) {
                        return included.id() === item.id;
                    });

                    item.isbutton = item.isenabled && check;
                }
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

                if (!item.isbutton && includeRelated) {
                    behavior.behaviorActionMenu.includeActionsAsButtons.remove(item.id);
                }

                if (item.isbutton && item.isenabled && !includeRelated) {
                    behavior.behaviorActionMenu.includeActionsAsButtons.add(item.id);
                }
            }

            function createEmptyBehavior() {
                return spEntity.fromJSON({
                    typeId: 'console:consoleBehavior',
                    'console:behaviorActionMenu': jsonLookup(createEmptyActionMenu())
                });
            }

            function createEmptyActionMenu() {
                return spEntity.fromJSON({
                    typeId: 'console:actionMenu',
                    'console:showNewActionsButton': jsonBool(),
                    'console:showExportActionsButton': jsonBool(),
                    'console:showEditInlineActionsButton': jsonBool(),
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
                    htmlActionMethod: jsonString(info.method),
                    htmlActionState: jsonString(info.state),
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
                    htmlActionMethod: jsonString(info.method),
                    htmlActionState: jsonString(info.state),
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
                    up: function() {
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
                    down: function() {
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
                    selectionChanged: function(tab) {
                        $scope.tab = tab;
                    },

                    // prevent showing a button, if the option is not in the menu
                    changeIsEnabled: function (row) {
                        if (row && row.entity && row.entity.isenabled === false) {
                            row.entity.isbutton = false;
                        }
                    },

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
        .factory('spReportActionsDialog', function (spDialogService) {
            // setup
            var exports = {

                showModalDialog: function (options, defaultOverrides) {
                    var dialogDefaults = {
                        title: 'Actions',
                        keyboard: true,
                        backdropClick: true,
                        windowClass: 'modal reportactionsdialog-view',
                        templateUrl: 'reportBuilder/dialogs/reportActions/reportActionsDialog.tpl.html',
                        controller: 'spReportActionsDialogController',
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