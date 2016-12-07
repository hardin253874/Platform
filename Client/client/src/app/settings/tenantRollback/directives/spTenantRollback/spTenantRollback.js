// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';
    angular.module('mod.app.settings.tenantRollback.directives.spTenantRollback',
        [
            'mod.common.alerts',
            'mod.common.spWebService',
            'mod.common.spEntityService',
            'mod.ui.spReportModelManager',
            'mod.common.spXsrf',
            'sp.navService',
            'mod.common.ui.spDialogService'
        ])
        .directive('spTenantRollback',
            function($rootScope,
                $http,
                $q,
                spAlertsService,
                spWebService,
                spEntityService,
                spReportModelManager,
                spXsrf,
                spNavService,
                spDialogService) {
                return {
                    restrict: 'E',
                    templateUrl: 'settings/tenantRollback/directives/spTenantRollback/spTenantRollback.tpl.html',
                    replace: true,
                    transclude: false,
                    link: function(scope) {
                        scope.model = {
                            allRestorePoints: [],
                            allCustomRestorePoints: [],
                            rollbackDates: [],
                            selectedRollbackDate: null,
                            restorePoints: [],
                            selectedRestorePoint: null,
                            changesBy: [],
                            rollbackLogs: []
                        };

                        scope.$watch('model.selectedRollbackDate',
                            function(newVal, oldVal) {
                                if (oldVal === newVal || !newVal) {
                                    return;
                                }

                                var values = scope.model.dateMap[newVal.dateString];

                                values = _.orderBy(values, 'date', 'desc');

                                scope.model.restorePoints = [];
                                scope.model.selectedRestorePoint = null;

                                var restorePointSet = {};

                                var restorePoints = [];

                                _.forEach(values,
                                    function (restorePoint) {
                                        var date = new Date(restorePoint.date);

                                        var newDate = new Date(date.getYear(),
                                            date.getMonth(),
                                            date.getDate(),
                                            date.getHours(),
                                            date.getMinutes(),
                                            date.getSeconds(),
                                            0);

                                        var timeFormat = Globalize.format(newDate, 't');

                                        if (restorePoint.userDefined) {
                                            var medTimeFormat = Globalize.format(newDate, 'T');

                                            if (restorePoint.name) {
                                                var rollbackRegEx = /Rollback to '(.*)'/i;

                                                var matches = restorePoint.name.match(rollbackRegEx);

                                                if (matches && matches.length > 1) {
                                                    var rollbackDate = new Date(matches[1]);

                                                    if (spUtils.isValidDate(rollbackDate)) {
                                                        restorePoint.name = 'Rollback to \'' + Globalize.format(rollbackDate, 'f') + '\'';
                                                    }
                                                }
                                            }

                                            restorePoints.push({
                                                name: restorePoint.name + ' (' + medTimeFormat + ')',
                                                date: timeFormat,
                                                restorePoint: restorePoint
                                            });
                                        } else if (!_.has(restorePointSet, timeFormat)) {
                                            restorePointSet[timeFormat] = 1;

                                            restorePoints.push({
                                                name: timeFormat,
                                                date: timeFormat,
                                                restorePoint: restorePoint
                                            });
                                        }
                                    });

                                if (restorePoints.length) {
                                    scope.model.restorePoints = restorePoints;
                                }

                                if (scope.model.previouslySelectedRestorePoint) {
                                    var previousSelectedRestorePoint = _.filter(scope.model.restorePoints, function (r) {
                                        return r.name === scope.model.previouslySelectedRestorePoint;
                                    });

                                    if (previousSelectedRestorePoint && previousSelectedRestorePoint.length > 0) {
                                        scope.model.selectedRestorePoint = previousSelectedRestorePoint[0];
                                    }

                                    scope.model.previouslySelectedRestorePoint = null;
                                } else if (!scope.model.selectedRestorePoint) {
                                    scope.model.selectedRestorePoint = scope.model.restorePoints[0];
                                }

                            });

                        scope.$watch('model.selectedRestorePoint',
                            function(newVal, oldVal) {
                                if (oldVal === newVal || !newVal) {
                                    return;
                                }

                                if (newVal.restorePoint.userNames) {
                                    scope.model.changesBy = newVal.restorePoint.userNames;
                                } else {
                                    getUserActivity(newVal.restorePoint.date)
                                        .then(function (data) {
                                            var changes = data;

                                            changes = _.sortBy(_.uniq(changes));

                                            newVal.restorePoint.userNames = changes;

                                            scope.model.changesBy = changes;
                                        });
                                }
                            });

                        scope.createRestorePoint = function() {
                            var btns = [
                                { result: true, label: 'Create' },
                                { result: false, label: 'Cancel' }
                            ];
                            var message = 'Creates a new system restore point with the specified name.';

                            var options = {
                                title: 'Create Restore Point',
                                message: message,
                                btns: btns
                            };

                            var dialogOptions = {
                                windowClass: 'tenantRollback-createRestorePoint',
                                templateUrl:
                                    'settings/tenantRollback/directives/spTenantRollback/spTenantRollbackCreateRestorePoint.tpl.html',
                                controller: 'spTenantRollbackCreateController',
                                resolve: {
                                    options: function() {
                                        return options;
                                    }
                                }
                            };

                            return spDialogService.showModalDialog(dialogOptions)
                                .then(function(result) {
                                    if (result === true) {
                                        reInitialize();
                                    }

                                    return $q.when();
                                });
                        };

                        scope.startRollback = function() {
                            var btns = [
                                { result: true, label: 'Start Rollback' },
                                { result: false, label: 'Cancel' }
                            ];
                            var message = 'Rollback to ' + scope.model.selectedRestorePoint.name + ' on ' + scope.model.selectedRollbackDate.dateString + '.';

                            var options = {
                                title: 'Confirm Rollback',
                                message: message,
                                btns: btns,
                                selectedRestorePoint: scope.model.selectedRestorePoint
                            };

                            var dialogOptions = {
                                windowClass: 'tenantRollback-confirmRestorePoint',
                                templateUrl:
                                    'settings/tenantRollback/directives/spTenantRollback/spTenantRollbackConfirm.tpl.html',
                                controller: 'spTenantRollbackConfirmController',
                                resolve: {
                                    options: function () {
                                        return options;
                                    }
                                }
                            };

                            return spDialogService.showModalDialog(dialogOptions)
                                .then(function (result) {
                                    if (result === true) {
                                        reInitialize();
                                    }

                                    return $q.when();
                                });
                        };

                        function getTenantRollbackData() {
                            return $http({
                                    method: 'GET',
                                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/tenantRollback/',
                                    headers: spWebService.getHeaders()
                                })
                                .then(function(response) {
                                        return response && response.data;
                                    },
                                    function(error) {
                                        var errorText = 'Failed to retrieve tenant rollback data.';

                                        spAlertsService.addAlert(errorText, 'error');

                                        throw error && error.data;
                                    });
                        }

                        function getUserActivity(dateString) {
                            return $http({
                                method: 'POST',
                                url: spWebService.getWebApiRoot() + '/spapi/data/v1/tenantRollback/getUserActivity/',
                                headers: spWebService.getHeaders(),
                                data: { dateString: dateString }
                            })
                                .then(function (response) {
                                    return response && response.data;
                                },
                                    function (error) {
                                        var errorText = 'Failed to retrieve tenant rollback user activity.';

                                        spAlertsService.addAlert(errorText, 'error');

                                        throw error && error.data;
                                    });
                        }

                        function reInitialize() {
                            var selectedRollbackDate = scope.model.selectedRollbackDate.dateString;
                            var selectedRestorePoint = scope.model.selectedRestorePoint.name;

                            initialize()
                                .then(function() {

                                    if (selectedRollbackDate) {
                                        var previousSelectedRollbackDate = _.filter(scope.model.rollbackDates,
                                            function(r) {
                                                return r.dateString === selectedRollbackDate;
                                            });

                                        if (previousSelectedRollbackDate && previousSelectedRollbackDate.length > 0) {
                                            scope.model.selectedRollbackDate = previousSelectedRollbackDate[0];
                                        }
                                    }

                                    if (selectedRestorePoint) {
                                        scope.model.previouslySelectedRestorePoint = selectedRestorePoint;
                                    }
                                });
                        }

                        function initialize() {
                            return getTenantRollbackData()
                                .then(function(data) {

                                    scope.model.allRestorePoints = [];
                                    scope.model.allCustomRestorePoints = [];
                                    scope.model.restorePoints = [];
                                    scope.model.dateMap = [];
                                    scope.model.rollbackDates = [];
                                    scope.model.selectedRollbackDate = null;
                                    scope.model.selectedRestorePoint = null;

                                    var rollbackDates = [];

                                    if (data && data.restorePoints && data.restorePoints.length) {

                                        scope.model.allRestorePoints = data.restorePoints;

                                        _.forEach(data.restorePoints,
                                            function(restorePoint) {

                                                var date = new Date(restorePoint.date);

                                                var dateString = date.toDateString();
                                                restorePoint.dateString = dateString;

                                                var values = scope.model.dateMap[dateString];

                                                if (!values) {
                                                    values = [
                                                    ];
                                                    scope.model.dateMap[dateString] = values;
                                                    rollbackDates.push(restorePoint);
                                                }

                                                restorePoint.userDefined = false;
                                                values.push(restorePoint);
                                            });
                                    }

                                    if (data && data.customRestorePoints && data.customRestorePoints.length) {

                                        scope.model.allCustomRestorePoints = data.customRestorePoints;

                                        _.forEach(data.customRestorePoints,
                                            function(customRestorePoint) {

                                                var date = new Date(customRestorePoint.date);

                                                var dateString = date.toDateString();
                                                customRestorePoint.dateString = dateString;

                                                var values = scope.model.dateMap[dateString];

                                                if (!values) {
                                                    values = [
                                                    ];
                                                    scope.model.dateMap[dateString] = values;
                                                    rollbackDates.push(customRestorePoint);
                                                }

                                                customRestorePoint.userDefined = true;
                                                values.push(customRestorePoint);
                                            });
                                    }

                                    if (rollbackDates.length) {
                                        scope.model.rollbackDates = _.orderBy(rollbackDates, 'date', 'desc');
                                        scope.model.selectedRollbackDate = scope.model.rollbackDates[0];
                                    }

                                    scope.model.rollbackLogs = [];
                                    var rollbackLogs = [];

                                    if (data && data.rollbackLogs && data.rollbackLogs.length) {
                                        _.forEach(data.rollbackLogs,
                                            function(log) {

                                                var date = new Date(log.date);

                                                var logEntry = {
                                                    userName: log.userName,
                                                    dateObj: date,
                                                    date: Globalize.format(date, 'f'),
                                                    rollbackDate: Globalize.format(new Date(log.rollbackDate), 'f')
                                                };

                                                rollbackLogs.push(logEntry);

                                            });
                                    }

                                    if (rollbackLogs.length) {
                                        scope.model.rollbackLogs = _.orderBy(rollbackLogs, 'dateObj', 'desc');
                                    }

                                });
                        }

                        initialize();
                    }
                };
            })
        .controller('spTenantRollbackCreateController',
        [
            '$scope', '$uibModalInstance', '$http', '$q', 'options', 'spDeleteService', 'spWebService', 'spAlertsService',
            function($scope, $uibModalInstance, $http, $q, options, spDeleteService, spWebService, spAlertsService) {

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
                    function() {
                        $scope.model.cancel();
                    });

                $scope.model.close = function(result) {

                    if (result) {
                        return $http({
                                method: 'POST',
                                url: spWebService.getWebApiRoot() + '/spapi/data/v1/tenantRollback/create',
                                headers: spWebService.getHeaders(),
                                data: { name: $scope.model.name }
                            })
                            .then(function() {
                                    $uibModalInstance.close(result);
                                },
                                function(error) {
                                    var errorText = 'Failed to create restore point.';

                                    spAlertsService.addAlert(errorText, 'error');

                                    throw error && error.data;
                                });
                    } else {
                        $uibModalInstance.close(result);

                        return $q.when();
                    }
                };
            }
        ])
        .controller('spTenantRollbackConfirmController',
        [
            '$scope', '$uibModalInstance', '$http', '$q', 'options', 'spDeleteService', 'spWebService', 'spAlertsService',
            function($scope, $uibModalInstance, $http, $q, options, spDeleteService, spWebService, spAlertsService) {

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
                    selectedRestorePoint: options.selectedRestorePoint,
                    running: false,
                    busyIndicator: {
                        type: 'spinner',
                        text: 'Loading...',
                        placement: 'element',
                        isBusy: false
                    },
                    dependencies: null
                };

                $scope.$on('signedout',
                    function() {
                        $scope.model.cancel();
                    });

                $scope.model.close = function(result) {

                    if (result) {
                        $scope.model.running = true;

                        return $http({
                                method: 'POST',
                                url: spWebService.getWebApiRoot() + '/spapi/data/v1/tenantRollback/rollback',
                                headers: spWebService.getHeaders(),
                                data: { date: $scope.model.selectedRestorePoint.restorePoint.date, name: $scope.model.selectedRestorePoint.name }
                            })
                            .then(function () {
                                    spAlertsService.addAlert('Tenant rollback complete', { severity: spAlertsService.sev.Success, expires: true });

                                    $uibModalInstance.close(result);
                                },
                                function(error) {
                                    var errorText = 'Unable to rollback at the current time. Please try again later.';

                                    spAlertsService.addAlert(errorText, 'error');

                                    $scope.model.running = false;

                                    throw error && error.data;
                                });
                    } else {
                        $uibModalInstance.close(result);

                        return $q.when();
                    }
                };
            }
        ]);
}());