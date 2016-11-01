// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';
    angular.module('mod.app.applicationManager.directives.spAppManager', [
        'mod.common.alerts',
        'mod.common.spWebService',
        'mod.common.spEntityService',
        'mod.ui.spReportModelManager',
        'mod.app.applicationManager.services.spAppManagerService',
        'mod.app.applicationManager.services.spAppManagerPublishService',
        'mod.common.spXsrf',
        'sp.navService',
        'mod.common.spFileDownloadService',
        'mod.common.ui.spDialogService'
    ])
    .directive('spAppManager', function ($rootScope, $q, spAlertsService, spWebService, spEntityService, spReportModelManager, spAppManagerService, spAppManagerPublishService, spXsrf, spNavService, spFileDownloadService, spDialogService) {
        return {
            restrict: 'E',
            templateUrl: 'applicationManager/directives/spAppManager/spAppManager.tpl.html',
            replace: true,
            transclude: false,
            link: function (scope, element, attrs) {
                var reportModel;

                scope.reportOptions = {};

                scope.appManagerIndicator = {
                    isBusy: false,
                    type: 'spinner',
                    text: 'Loading...',
                    placement: 'window'
                };

                // Events

                scope.$on('spReportEventModelReady', function (event, model) {
                    reportModel = model;
                    reportModel.customDataProvider = getAppManagerData;
                });

                // Public Functions (wired to actions)

                scope.refresh = function () {
                    if (reportModel && reportModel.refreshButtonOptions && reportModel.refreshButtonOptions.refreshCallback) {
                        reportModel.refreshButtonOptions.refreshCallback();
                    }
                };

                scope.publish = function () {
                    return getCallback(function (arg) {

                        if (arg) {
                            var name;

                            name = arg.cells && arg.cells[0] && arg.cells[0].value;

                            var options = {
                                id: arg.eid,
                                name: name || arg.obj.name,
                                data: arg.obj
                            };

                            return spAppManagerPublishService.showDialog(options);
                        }
                    }, getArg);
                };

                scope.deploy = function () {
                    var btns = [
                                { result: true, label: 'OK' },
                                { result: false, label: 'Cancel' }
                    ];
                    var message = 'Are you sure that you want to install the selected application?';

                    var id = -1;

                    if (scope &&
                        scope.reportOptions &&
                        scope.reportOptions.selectedItems &&
                        scope.reportOptions.selectedItems.length > 0) {
                        var item = scope.reportOptions.selectedItems[0];

                        if (item) {
                            if (item.obj) {
                                id = item.obj.pid;
                            }

                            if (item.cells && item.cells.length > 0 && item.cells[0].value) {
                                message = 'Are you sure that you want to install "' + item.cells[0].value + '"?';
                            }
                        }
                    }

                    if (id >= 0) {
                        var options = {
                            title: 'Confirm install',
                            failedDependencyTitle: 'Unable to install',
                            message: message,
                            dependentMessage: 'The following applications will also be installed:',
                            upgradeMessage: 'The following applications will be upgraded:',
                            webServiceApi: '/spapi/data/v1/appManager/getPackageDependencies/',
                            mode: 'install',
                            id: id,
                            btns: btns
                        };

                        var dialogOptions = {
                            templateUrl:
                                'applicationManager/directives/spAppManager/spAppManagerDetails.tpl.html',
                            controller: 'spAppManagerDetailsController',
                            resolve: {
                                options: function () {
                                    return options;
                                }
                            }
                        };

                        return spDialogService.showModalDialog(dialogOptions).then(function (result) {
                            if (result === true) {
                                return getCallback(spAppManagerService.deployApplication, getAppIdArg, 'Deploying...').then(function () {
                                    spNavService.refreshTree(false);
                                });
                            }

                            return $q.when();
                        });
                    } else {
                        return $q.when();
                    }
                };

                scope.upgrade = function () {
                    var btns = [
                                { result: true, label: 'OK' },
                                { result: false, label: 'Cancel' }
                    ];
                    var message = 'Are you sure that you want to upgrade the selected application?';

                    var id = -1;

                    if (scope &&
                        scope.reportOptions &&
                        scope.reportOptions.selectedItems &&
                        scope.reportOptions.selectedItems.length > 0) {
                        var item = scope.reportOptions.selectedItems[0];

                        if (item) {
                            if (item.obj) {
                                id = item.obj.pid;
                            }

                            if (item.cells && item.cells.length > 0 && item.cells[0].value) {
                                message = 'Are you sure that you want to upgrade "' + item.cells[0].value + '"?';
                            }
                        }
                    }

                    if (id >= 0) {
                        var options = {
                            title: 'Confirm install',
                            failedDependencyTitle: 'Unable to install',
                            message: message,
                            dependentMessage: 'The following applications will also be upgraded:',
                            webServiceApi: '/spapi/data/v1/appManager/getPackageDependencies/',
                            mode: 'upgrade',
                            id: id,
                            btns: btns
                        };

                        var dialogOptions = {
                            templateUrl:
                                'applicationManager/directives/spAppManager/spAppManagerDetails.tpl.html',
                            controller: 'spAppManagerDetailsController',
                            resolve: {
                                options: function () {
                                    return options;
                                }
                            }
                        };

                        return spDialogService.showModalDialog(dialogOptions).then(function (result) {
                            if (result === true) {
                                return getCallback(spAppManagerService.upgradeApplication, getVersionIdArg, 'Upgrading...').then(function () {
                                    spNavService.refreshTree(false);
                                });
                            }

                            return $q.when();
                        });
                    } else {
                        return $q.when();
                    }
                };

                scope.repair = function () {
                    return getCallback(spAppManagerService.repairApplication, getVersionIdArg, 'Repairing...').then(function () {
                        spNavService.refreshTree(false);
                    });
                };

                scope.uninstall = function (scope) {
                    var btns = [
                                { result: true, label: 'OK' },
                                { result: false, label: 'Cancel' }
                    ];
                    var message = 'Are you sure that you want to uninstall the selected application?';

                    var id = -1;

                    if (scope &&
                        scope.reportOptions &&
                        scope.reportOptions.selectedItems &&
                        scope.reportOptions.selectedItems.length > 0) {
                        var item = scope.reportOptions.selectedItems[0];

                        if (item) {
                            id = item.eid;

                            if (item.cells && item.cells.length > 0 && item.cells[0].value) {
                                message = 'Are you sure that you want to uninstall "' + item.cells[0].value + '"?';
                            }
                        }
                    }

                    if (id >= 0) {
                        var options = {
                            title: 'Confirm uninstall',
                            message: message,
                            dependentMessage: 'The following dependent applications will also be uninstalled:',
                            webServiceApi: '/spapi/data/v1/appManager/getDependents/',
                            mode: 'uninstall',
                            id: id,
                            btns: btns
                        };

                        var dialogOptions = {
                            templateUrl:
                                'applicationManager/directives/spAppManager/spAppManagerDetails.tpl.html',
                            controller: 'spAppManagerDetailsController',
                            resolve: {
                                options: function () {
                                    return options;
                                }
                            }
                        };

                        return spDialogService.showModalDialog(dialogOptions).then(function (result) {
                            if (result === true) {
                                return getCallback(spAppManagerService.uninstallApplication, getAppIdArg, 'Uninstalling...').then(function () {
                                    spNavService.refreshTree(false);
                                });
                            }

                            return $q.when();
                        });
                    } else {
                        return $q.when();
                    }
                };

                scope.export = function () {
                    var p = $q.when();

                    scope.appManagerIndicator.isBusy = true;

                    var selectedItems = scope.reportOptions.selectedItems;
                    if (selectedItems && _.isArray(selectedItems)) {
                        var selected = _.first(selectedItems);
                        if (selected && selected.obj && selected.obj.vid) {
                            var name = selected.obj.name.replace(/\s/g, '') + '.xml';
                            var vid = selected.obj.vid;

                            p = spAppManagerService.exportApplication(vid).then(function (token) {
                                scope.appManagerIndicator.isBusy = false;

                                if (!token) {
                                    return;
                                }

                                token = token.trim();
                                
                                if (token.charAt(0) === '"' && token.charAt(token.length - 1) === '"')
                                {
                                    token = token.substr(1, token.length - 2);
                                }

                                // download or prompt to save
                                spFileDownloadService.downloadFile(spXsrf.addXsrfTokenAsQueryString(spWebService.getWebApiRoot() + '/spapi/data/v1/appManager/' + token + '/' + encodeURIComponent(name) + '/export'));
                            });
                        }
                    }

                    return p;
                };

                // Private Methods 

                function appManagerGetActionExecutionContext(action, ids) {
                    return {
                        scope: scope,
                        state: action.state,
                        selectionEntityIds: ids,
                        isEditMode: false,
                        refreshDataCallback: function () {
                            scope.refresh();

                            if (action && ( action.method === 'delete' || action.name === 'Uninstall' ) ) {
                                spNavService.refreshTree(true);
                            }
                        }
                    };
                }

                function getAppManagerData(data) {

                    if (!scope.model) {
                        scope.model = {
                            reportId: scope.reportOptions.reportId
                        };
                    }

                    if (data && data.meta) {
                        scope.model.reportMetadata = data.meta;
                    }

                    if (!scope.model.quickSearch) {
                        scope.model.quickSearch = {};
                    }

                    return spAppManagerService.getApplications().then(function (apps) {
                        var appManagerData = [];

                        // process rows
                        _.forEach(data.gdata, function (row) {
                            var app = _.find(apps, function (a) {
                                return a.sid === row.eid;
                            });

                            var name = '';
                            var description = '';
                            var installedVersion = '';
                            var availableVersion = '';                            
                            var canModifyAppRowValue = {
                                cfidx: 1,
                                val: false
                            };

                            if (row && row.values && row.values.length > 0) {
                                name = row.values[0].val;
                            }

                            if (row && row.values && row.values.length > 1) {
                                description = row.values[1].val;
                            }

                            if (row && row.values && row.values.length > 4) {
                                canModifyAppRowValue = row.values[4];

                                if (!canModifyAppRowValue) {
                                    canModifyAppRowValue = {
                                        cfidx: 1,
                                        val: false
                                    };
                                }
                            }

                            if (app) {
                                // row data and an accompanying entity
                                installedVersion = app.solutionVersion;
                                availableVersion = app.packageVersion;                                

                                appManagerData.push({
                                    eid: app.sid,
                                    obj: app,
                                    values: [
                                        { val: name || app.name },
                                        { val: description },
                                        { val: formatVersion(installedVersion) },
                                        { val: formatVersion(availableVersion) },
                                        canModifyAppRowValue
                                    ]
                                });
                            } else {
                                // row data, but no app object
                                if (row && row.values && row.values.length > 2) {
                                    installedVersion = row.values[2].val;
                                }

                                if (row && row.values && row.values.length > 3) {
                                    availableVersion = row.values[3].val;
                                }

                                appManagerData.push({
                                    values: [
                                        { val: name },
                                        { val: description },
                                        { val: formatVersion(installedVersion) },
                                        { val: formatVersion(availableVersion) },
                                        canModifyAppRowValue
                                    ]
                                });
                            }
                        });

                        // process apps
                        _.forEach(apps, function (app) {
                            var row = _.find(data.gdata, function (r) {
                                return app.sid === r.eid;
                            });
                            if (!row) {
                                // apps with no rows
                                appManagerData.push({
                                    eid: app.sid || app.eid,
                                    obj: app,
                                    values: [
                                        { val: app.name },
                                        { val: '' },
                                        { val: app.solutionVersion },
                                        { val: app.packageVersion },
                                        {
                                            cfidx: 1,
                                            val: false
                                        }
                                    ]
                                });
                            }
                        });

                        // Apply quick search against visible columns only here since the report quick-search is
                        // run across all columns including hidden columns which may be confusing.
                        if (data.quickSearch && data.quickSearch.value) {
                            var quickSearch = data.quickSearch.value.toLowerCase();

                            var visibleColumns = [];

                            if (data.columnDefinitions) {
                                _.forEach(data.columnDefinitions, function(col, index) {
                                    if (col.visible) {
                                        visibleColumns.push(index);
                                    }
                                });
                            } else {
                                // Default to Name, Installed Version, Available Version
                                visibleColumns = [0, 2, 3];
                            }

                            var filtered = _.filter(appManagerData, function (dataRow) {

                                if (dataRow && dataRow.values) {
                                    var found = false;

                                    _.forEach(visibleColumns, function(index) {
                                        if (dataRow.values[index] && dataRow.values[index].val && dataRow.values[index].val.toLowerCase().indexOf(quickSearch) >= 0) {
                                            found = true;
                                            // Exit from loop
                                            return false;
                                        }
                                    });

                                    return found;
                                }

                                return false;
                            });

                            appManagerData = filtered;
                        }

                        return appManagerData;
                    });
                }

                /////
                // Ensure the version number is in a consistent format.
                /////
                function formatVersion(version, componentCount)
                {
                    if (!version || !_.isString(version)) {
                        return version;
                    }

                    if (!componentCount) {
                        componentCount = 4;
                    }

                    var components = version.split('.');

                    if (!components || !_.isArray(components)) {
                        return version;
                    }

                    while (components.length < componentCount) {
                        components.push('0');
                    }

                    for (var componentIndex = 0; componentIndex < components.length; componentIndex++) {
                        var component = components[componentIndex];

                        if (! component) {
                            component = '0';
                        } else {
                            var componentInt = parseInt(component);

                            if (!componentInt) {
                                component = '0';
                            } else {
                                component = componentInt.toString();
                            }
                        }

                        components[componentIndex] = component;
                    }

                    return components.join('.');
                }

                function getAppIdArg(selected) {
                    return selected && selected.obj && selected.obj.id;
                }

                function getVersionIdArg(selected) {
                    return selected && selected.obj && selected.obj.vid;
                }

                function getArg(selected) {
                    return selected;
                }

                function getCallback(callback, argCallback, busyText) {
                    var p = $q.when();
                    var selectedItems = scope.reportOptions.selectedItems;

                    if (selectedItems && _.isArray(selectedItems)) {
                        var selected = _.first(selectedItems);
                        argCallback = argCallback || getArg;
                        var arg = argCallback(selected);
                        if (arg) {

                            if (busyText) {
                                scope.appManagerIndicator.isBusy = true;
                                scope.appManagerIndicator.text = busyText;
                            }

                            p = callback(arg)
                                .finally(function() {

                                if (busyText) {
                                    scope.appManagerIndicator.text = 'Loading...';
                                    scope.appManagerIndicator.isBusy = false;
                                }

                            });
                        }
                    }
                    return p;
                }

                function initialize() {
                    // retrieve the id of the backing report if not done so
                    spEntityService.getEntity('console:applicationManagementReport', 'id', { hint: 'appMgmt', batch: true }).then(function (report) {
                        scope.reportOptions.reportId = report.idP;
                        scope.reportOptions.getActionExecutionContext = appManagerGetActionExecutionContext;
                    }, function (error) {
                        console.error('spAppManager error:', error);
                        spAlertsService.addAlert('Failed to get the application manager report.', 'error');
                    });
                }

                initialize();
            }
        };
    })
    .controller('spAppManagerDetailsController',
    [
        '$scope', '$uibModalInstance', '$timeout', '$http', 'options', 'spDeleteService', 'spWebService',
        function ($scope, $uibModalInstance, $timeout, $http, options, spDeleteService, spWebService) {

            /////
            // Setup the dialog model
            /////
            $scope.model = {
                title: options.title,
                failedDependencyTitle: options.failedDependencyTitle,
                message: options.message,
                dependentMessage: options.dependentMessage,
                upgradeMessage: options.upgradeMessage,
                mode: options.mode,
                okText: options.btns[0].label,
                cancelText: options.btns[1].label,
                busyIndicator: {
                    type: 'spinner',
                    text: 'Loading...',
                    placement: 'element',
                    isBusy: false
                },
                dependencies: null
            };

            $scope.$on('signedout',
                function () {
                    $scope.model.cancel();
                });

            $scope.model.close = function (result) {
                $uibModalInstance.close(result);
            };

            $scope.model.loadDependencies = function () {
                var url = spWebService.getWebApiRoot() + options.webServiceApi + options.id;

                return $http({
                    method: 'GET',
                    url: url,
                    headers: spWebService.getHeaders()
                })
                    .then(function (response) {

                        if (response && response.status === 200) {
                            if (response.data && response.data.length > 0) {

                                if ($scope.model.mode === 'install') {
                                    $scope.model.dependencies = _.orderBy(_.reject(response.data, 'upgrade'), 'name');
                                    $scope.model.upgrades = _.orderBy(_.filter(response.data, 'upgrade'), 'name');
                                } else {
                                    $scope.model.dependencies = _.orderBy(response.data, 'name');
                                }
                            } else {
                                $scope.model.dependencies = null;
                            }
                        }
                    }, function (error) {
                        if (error && error.data && error.data.ExceptionType === 'EDC.SoftwarePlatform.Migration.Processing.ApplicationDependencyException') {
                            if ($scope.model.failedDependencyTitle) {
                                $scope.model.title = $scope.model.failedDependencyTitle;
                            }
                            $scope.model.message = error.data.ExceptionMessage;
                            $scope.model.okDisabled = true;
                        }
                        console.log(error);
                    });
            };

            $scope.model.loadDependencies();
        }
    ]);
}());