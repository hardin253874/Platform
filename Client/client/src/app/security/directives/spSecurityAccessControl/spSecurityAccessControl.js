/// <reference path="../../../editForm/custom/spSubjectRecordAccessSummary/spSubjectRecordAccessSummary.js" />
// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use strict';

    angular.module('app.security.directives.spSecurityAccessControl', [
            'sp.navService',
            'ngGrid',
            'mod.app.accessControl.service',
            'mod.common.ui.spDialogService',
            'mod.common.ui.spBusyIndicator',
            'sp.common.directives',
            'mod.common.ui.spAnalyzerPopup',
            'sp.common.spDialog',
            'mod.ui.spDataGridPlugins',
            'mod.common.ui.spContextMenu',
            'sp.app.settings'
        ])
        .directive('spSecurityAccessControl', function() {
            return {
                restrict: 'E',
                replace: true,
                scope: {
                    options: '='
                },
                templateUrl: 'security/directives/spSecurityAccessControl/spSecurityAccessControl.tpl.html',
                controller: 'spSecurityAccessControlController'
            };
        })
        .controller('spSecurityAccessControlController', function ($scope, $log, $stateParams, $q, spAccessControlService, spNavService, spAlertsService, spDialogService, spDialog, spDataGridPlugins, $timeout, $templateCache, spAppSettings) {

            var loadQueryOptions = { includeHidden: spAppSettings.initialSettings.devMode };
            var deleteCellTemplate = $templateCache.get('security/directives/spSecurityAccessControl/securityAccessControlDeleteCell.tpl.html');
            var enabledCellTemplate = $templateCache.get('security/directives/spSecurityAccessControl/securityAccessControlEnabledCell.tpl.html');
            var permissionsCellTemplate = $templateCache.get('security/directives/spSecurityAccessControl/securityAccessControlPermissionsCell.tpl.html');
            var permissionsCellEditTemplate = $templateCache.get('security/directives/spSecurityAccessControl/securityAccessControlPermissionsCellEdit.tpl.html');
            var rowTemplate = $templateCache.get('security/directives/spSecurityAccessControl/securityAccessControlRow.tpl.html');
            var gridLayoutPlugin = new spDataGridPlugins.GridLayoutPlugin($timeout);
            var viewModeColumnDefs = [
                { displayName: 'Enabled', cellTemplate: enabledCellTemplate, width: 70 },
                { displayName: 'Role or User Account', field: 'roleOrUserAccount', visible: true },
                { displayName: 'Object', field: 'accessRule.controlAccess.name' },
                { displayName: 'Permissions', cellTemplate: permissionsCellTemplate, width: 240 },
                { displayName: 'Query', field: 'queryName' }
            ];
            var editModeColumnDefs = [
                { displayName: 'Enabled', cellTemplate: enabledCellTemplate, width: 70 },
                { displayName: 'Role or User Account', field: 'roleOrUserAccount', visible: true },
                { displayName: 'Object', field: 'accessRule.controlAccess.name' },
                { displayName: 'Permissions', cellTemplate: permissionsCellEditTemplate, width: 240 },
                { displayName: 'Query', field: 'queryName' },
                { displayName: '', cellTemplate: deleteCellTemplate, width: 130 }
            ];
            var contextMenuItemDefs = [
                { text: 'Edit Query', type: 'click', click: 'edit(row)', icon: 'assets/images/16x16/edit.svg', disabled: '!canEditQuery(row)' },
                { text: 'Delete', type: 'click', click: 'delete(row)', icon: 'assets/images/16x16/delete.svg', disabled: '!canDelete(row)' }
            ];

            $scope.model = {
                contextMenu: {
                    menuItems: []
                },
                columnDefs: viewModeColumnDefs,
                filteredQueries: [],
                busyIndicator: {
                    type: 'spinner',
                    placement: 'element',
                    isBusy: false
                },
                gridOptions: {
                    data: 'model.filteredQueries',
                    enableRowSelection: true,
                    multiSelect: false,
                    rowTemplate: rowTemplate,
                    columnDefs: 'model.columnDefs',
                    plugins: [gridLayoutPlugin]
                },
                quickSearch: {
                    value: "",
                    onSearchValueChanged: function () {
                        $scope.model.filteredQueries = filter(spAccessControlService.queries, $scope.model.quickSearch.value || "", sp.result($scope.model, 'columnDefs[1].visible'));
                    }
                }
            };

            var originalIsDirty;

            $scope.gridLoaded = false;

            function initialize() {
                if (!$scope.options) return;

                $scope.model.busyIndicator.isBusy = true;
                    
                spAccessControlService.loadQueries(loadQueryOptions).then(function () {
                    updateQueries(spAccessControlService.queries, $scope.model.quickSearch.value);
                }, function (error) {
                    spAlertsService.addAlert("An error occurred loading security queries: " + error.message, {
                        expires: false,
                        severity: spAlertsService.sev.Error
                    });
                }).finally(function () {
                    $scope.model.busyIndicator.isBusy = false;
                });

                // This is kept in sync if navigating away to a report and returning
                if (spAccessControlService.viewMode === spAccessControlService.modes.edit && $scope.options.mode !== 'edit') {
                    $scope.options.mode = 'edit';
                }                

                $timeout(function() {
                    $scope.gridLoaded = true;
                }, 0);
            }

            function isDirty() {
                var dirty = false;

                if (originalIsDirty) {
                    dirty = originalIsDirty();
                }

                if (!dirty) {
                    dirty = spAccessControlService.isDirty();
                }

                return dirty;
            }

            function onEdit() {
                if ($scope.options) {
                    $scope.options.mode = 'edit';
                }
            }

            function onCancel() {
                if (spAccessControlService.isDirty()) {
                    showLostChangesPrompt().then(function (result) {
                        if (result === 'ok') {
                            doRefresh(true);
                        }
                    });
                } else {
                    doRefresh(true);
                }
            }

            function onSave() {
                $scope.model.busyIndicator.isBusy = true;

                var doDeletes = true;
                var doUpdates = true;

                // if this is an edit form create, then let the edit form do the save
                if (loadQueryOptions && loadQueryOptions.subject && loadQueryOptions.subject.dataState === spEntity.DataStateEnum.Create) {
                    doUpdates = false;
                }

                // if this is a straight edit mode save of an existing, let the edit form do the deletes
                if (loadQueryOptions && loadQueryOptions.subject && loadQueryOptions.subject.dataState !== spEntity.DataStateEnum.Create && $scope.options.mode === 'edit') {
                    doDeletes = false;
                }

                var refresh = _.partial(doRefresh, true);

                return spAccessControlService.commit(doUpdates, doDeletes).then(function () {
                    spAlertsService.addAlert("Security changes saved.", { expires: 5, severity: spAlertsService.sev.Success });
                }, function (message) {
                    spAlertsService.addAlert("An error occurred saving the changes: " + message, { expires: false, severity: spAlertsService.sev.Error });
                }).then(refresh).finally(function () {
                    $scope.model.busyIndicator.isBusy = false;
                });
            }

            $scope.refresh = function () {
                if (spAccessControlService.isDirty()) {
                    showLostChangesPrompt().then(function (result) {
                        if (result === 'ok') {
                            doRefresh(false);
                        }
                    });
                } else {
                    doRefresh(false);
                }
            };

            $scope.new = function () {
                var showBusy = function () {
                    $scope.model.busyIndicator.isBusy = true;
                };
                var hideBusy = function () {
                    $scope.model.busyIndicator.isBusy = false;
                };

                spAccessControlService.createQuery(showBusy, hideBusy, loadQueryOptions).then(function (accessRule) {
                    // unfortunately the data lives in two places, so try to keep it in sync
                    if (loadQueryOptions.subject && $scope.options.insertCreated && accessRule) {
                        $scope.options.insertCreated(accessRule);
                    }

                    updateQueries(spAccessControlService.queries, $scope.model.quickSearch.value);

                    // allow for ease of editing when viewing by subject
                    if ($scope.options.mode !== 'edit' && loadQueryOptions.subject) {
                        onSave();
                    }
                }, function (message) {
                    if (message !== "cancel") {

                        if (message && message.data && message.data.Message) {
                            spAlertsService.addAlert("An error occurred creating the new access rule: " + message.data.Message, { expires: false, severity: spAlertsService.sev.Error });
                        } else {
                            spAlertsService.addAlert("An error occurred creating the new access rule", { expires: false, severity: spAlertsService.sev.Error });
                        }
                    }
                }).finally(function () {
                    hideBusy();
                });
            };

            $scope.edit = function (row) {
                spNavService.getCurrentItem().noTreeRefresh = true;
                spNavService.navigateToChildState('reportBuilder', row.entity.accessRule.accessRuleReport.id(), $stateParams);
            };

            $scope.canEditQuery = function(row) {
                if (row) {
                    var rpt = sp.result(row, 'entity.accessRule.accessRuleReport');
                    return rpt && rpt.canModify;
                }
                return false;
            };

            $scope.delete = function (row) {
                // show busy if in view mode
                if ($scope.options.mode !== 'edit' && loadQueryOptions.subject) {
                    $scope.model.busyIndicator.isBusy = true;
                }
                spDialog.confirmDialog('Confirm delete', 'Are you sure you want to delete this access rule?').then(function (result) {
                    if (result) {
                        // unfortunately the data lives in two places, so try to keep it in sync
                        if (loadQueryOptions.subject && $scope.options.removeDeleted && row.entity.accessRule) {
                            $scope.options.removeDeleted(row.entity.accessRule);
                        }

                        spAccessControlService.deleteQuery(row.entity);
                        updateQueries(spAccessControlService.queries, $scope.model.quickSearch.value);

                        // allow for ease of editing when viewing by subject
                        if ($scope.options.mode !== 'edit' && loadQueryOptions.subject) {
                            onSave();
                        }
                    }
                }).finally(function() {
                    $scope.model.busyIndicator.isBusy = false;
                });
            };

            $scope.canDelete = function(row) {
                if (row) {
                    var rule = sp.result(row, 'entity.accessRule');
                    return rule && rule.canDelete;
                }
                return false;
            };

            $scope.getPermissionLabel = function(permission) {
                switch (permission) {
                    case "": return "None";
                    case "core:read": return "View";
                    case "core:modify,core:read": return "View and Edit";
                    case "core:delete,core:modify,core:read": return "View, Edit and Delete";
                    case "core:create,core:modify,core:read": return "Create, View and Edit";
                    case "core:create": return "Create";
                    case "core:create,core:delete,core:modify,core:read": return "Full (Create, View, Edit and Delete)";
                    default: return "Unsupported";
                }
            };

            $scope.isDisabled = function() {
                return !$scope.options || $scope.options.mode !== 'edit' || $scope.options.isInDesign;
            };

            $scope.disableNew = function() {
                return !$scope.options || $scope.options.isInDesign || ($scope.options.subject && !$scope.options.subject.canModify);
            };

            $scope.doubleClick = function(row) {
                if (loadQueryOptions.subject && $scope.canEditQuery(row)) {
                    $scope.edit(row);
                }
            };

            function doRefresh(switchModes) {
                $scope.model.busyIndicator.isBusy = true;

                return spAccessControlService.loadQueries(loadQueryOptions).then(function () {
                    updateQueries(spAccessControlService.queries, $scope.model.quickSearch.value);
                    if (switchModes) {
                        if ($scope.options) {
                            $scope.options.mode = 'view';
                        }
                        spAccessControlService.viewMode = spAccessControlService.modes.view;
                    }
                }, function (message) {
                    spAlertsService.addAlert("An error occurred reloading security queries: " + message.data.Message,
                        { expires: false, severity: spAlertsService.sev.Error });
                })
                .finally(function () {
                    $scope.model.busyIndicator.isBusy = false;
                });
            }

            function showLostChangesPrompt() {
                return spDialogService.showMessageBox("Access Rules", "Your unsaved changes will be lost.", [
                    { result: 'ok', label: 'OK', cssClass: 'btn-primary' },
                    { result: 'cancel', label: 'Cancel' }
                ]);
            }

            function filter(queries, filterText, useRoleOrUserAccountInFilter) {
                var lowerCaseFilterText = filterText.toLowerCase();

                var filtered = queries ;

                // filter by subject
                if (loadQueryOptions.subject) {
                    var id = sp.result(loadQueryOptions.subject, 'idP', 0);
                    filtered = _.filter(filtered, function (query) {
                        return (query.accessRule.allowAccessBy && query.accessRule.allowAccessBy.idP === id);
                    });
                }

                // filter by text
                return _.filter(filtered, function (query) {                   
                    //to valid query object then do the match check, otherwise the filter function will return incorrect results.
                    return (useRoleOrUserAccountInFilter && isMatchedInQueryObject(query.roleOrUserAccount, lowerCaseFilterText)) ||
                        (query.accessRule && query.accessRule.controlAccess && isMatchedInQueryObject(query.accessRule.controlAccess.name, lowerCaseFilterText)) ||
                        isMatchedInQueryObject(query.queryName, lowerCaseFilterText) ||
                        isMatchedInPermissions(query.permissions, lowerCaseFilterText);
                });
            }

            //valid the query object.
            function isMatchedInQueryObject(value, lowerCaseFilterText) {               
                var isNotEmpty = function (value) {
                    return !_.isUndefined(value) &&
                       !_.isNull(value) &&
                       !_.isEmpty(value);
                };

                return isNotEmpty(value) &&
                    value.toLowerCase().indexOf(lowerCaseFilterText) !== -1;
            }

            //valid the query permission.
            function isMatchedInPermissions(value, lowerCaseFilterText) {

                var isNotEmpty = function (value) {
                    return !_.isUndefined(value) &&
                       !_.isNull(value) &&
                       !_.isEmpty(value);
                };

                if (!isNotEmpty(value))
                    return false;

                var permissions = $scope.getPermissionLabel(value);

                return permissions.toLowerCase().indexOf(lowerCaseFilterText) !== -1;                
            }

            function updateQueries(queries, filterText) {
                $scope.model.filteredQueries = filter(queries, filterText);
            }

            // Handle any actions
            $scope.$on('accessAction', function (event, args) {
                if (!args || !args.action) {
                    return;
                }

                switch (args.action) {
                    case 'save':
                        onSave();
                        break;
                    case 'cancel':
                        onCancel();
                        break;
                    case 'edit':
                        onEdit();
                        break;
                }
            });

            $scope.$watch("options.mode", function(mode) {
                if (mode === 'view') {
                    spAccessControlService.viewMode = spAccessControlService.modes.view;
                    $scope.model.columnDefs = viewModeColumnDefs;
                } else if (mode === 'edit') {
                    spAccessControlService.viewMode = spAccessControlService.modes.edit;
                    $scope.model.columnDefs = editModeColumnDefs;
                }
            });

            $scope.$watch("options.subject", function () {
                if ($scope.options) {
                    if ($scope.options.subject) {
                        loadQueryOptions.subject = $scope.options.subject;

                        // allow the context menu
                        $scope.model.contextMenu.menuItems = contextMenuItemDefs;

                        // hide the certain columns
                        viewModeColumnDefs[1].visible = false;
                        editModeColumnDefs[1].visible = false;
                        editModeColumnDefs[5].visible = false;
                    } else {
                        if (loadQueryOptions.subject) {
                            delete loadQueryOptions.subject;

                            $scope.model.contextMenu.menuItems = [];

                            viewModeColumnDefs[1].visible = true;
                            editModeColumnDefs[1].visible = true;
                            editModeColumnDefs[5].visible = true;
                        }
                    }
                }
            });

            $scope.$watch("options", initialize);

            var appLayoutDoneDebounced = _.debounce(function () {
                if (gridLayoutPlugin) {
                    gridLayoutPlugin.rebuildGrid(_.uniqueId());
                }
            }, 300);

            var forceRebuildRecordAccess = _.debounce(function () {
                if (gridLayoutPlugin) {
                    gridLayoutPlugin.rebuildGrid(_.uniqueId());
                }
            },10);

            $scope.$on('app.layout.done', appLayoutDoneDebounced);

            $scope.$on('forceRebuildRecordAccess', forceRebuildRecordAccess);

            // Destroy the auto update plugin
            $scope.$on('$destroy', function () {
                if (gridLayoutPlugin) {
                    gridLayoutPlugin.destroy();
                    gridLayoutPlugin = null;
                }
            });

            function initializeDirtyHandler() {
                var currentNavItem = spNavService.getCurrentItem();

                // Initialise the dirty handler
                if (currentNavItem && currentNavItem.isDirty !== isDirty) {

                    originalIsDirty = currentNavItem.isDirty;
                    
                    currentNavItem.isDirty = isDirty;
                }
            }

            initializeDirtyHandler();
        });
}());