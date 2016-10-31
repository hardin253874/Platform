// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Test "report" for security. This is intended as a stop gap measure until report security is implemented.
    *
    * @module testReport
    */
    angular.module('app.testReport', ['ngGrid', 'sp.navService', 'mod.common.alerts', 'mod.common.ui.spBusyIndicator', 'mod.common.spEntityService', 'mod.common.spWebService' ])
        .config(function ($stateProvider) {
            $stateProvider.state('testReport', {
                url: '/{tenant}/{eid}/testReport?path',
                templateUrl: 'test/testReport.tpl.html'
            });
        })
        .controller('TestReportController', function($scope, $http, spWebService, spAlertsService, spEntityService) {
            $scope.entities = [];
            $scope.filteredEntities = [];

            $scope.filterText = "";

            $scope.gridOptions =
            {
                data: 'filteredEntities',
                enableRowSelection: false,
                multiSelect: false,
                columnDefs: [
                    {
                        displayName: 'Name',
                        field: 'name',
                        minWidth: 100
                    },
                    {
                        displayName: 'Description',
                        field: 'description',
                        minWidth: 100
                    }
                ]
            };

            function filterEntities(filterText, entities) {
                if (!angular.isString(filterText)) {
                    throw new Error("filterText must be a string");
                }
                if (!angular.isArray(entities)) {
                    throw new Error("entities must be an array");
                }

                var lowerCaseFilterText = filterText.toLowerCase();
                return _.filter(entities, function(entity) {
                    return !lowerCaseFilterText || (entity.name ? entity.name.toLowerCase().indexOf(lowerCaseFilterText) != -1 : false) ||
                        (entity.description ? entity.description.toLowerCase().indexOf(lowerCaseFilterText) != -1 : false);
                });
            }

            $scope.$watch("filterText", function () {
                $scope.filteredEntities = filterEntities($scope.filterText, $scope.entities);
            });

            $scope.entityPickerOptions = {
                selectedEntityId: 0,
                selectedEntity: null,
                entityTypeId: 'core:definition'
            };
            
            $scope.$watch("entityPickerOptions.selectedEntityId", function (value) {
                if (value) {
                    $scope.refresh();
                }
            });

            $scope.busyIndicator = {
                type: 'spinner',
                placement: 'element',
                isBusy: false
            };
            
            $scope.refresh = function() {
                $scope.busyIndicator.isBusy = true;
                //$http({
                //        method: 'GET',
                //        url: spWebService.getWebApiRoot() + "/spapi/data/v1/securitycache",
                //        headers: spWebService.getHeaders()
                //    })
                return spEntityService.getEntitiesOfType($scope.entityPickerOptions.selectedEntityId, "core:name, core:description")
                    .then(function (result) {
                        $scope.entities = result;
                        $scope.filteredEntities = filterEntities($scope.filterText, $scope.entities);
                    }, function (message) {
                        spAlertsService.addAlert("An error occurred loading entities: " + message,
                            { expires: false, severity: spAlertsService.sev.Error });
                    })
                    .finally(function() {
                        $scope.busyIndicator.isBusy = false;
                    });
            };
            
            $scope.invalidateCache = function() {
                $scope.busyIndicator.isBusy = true;
                $http({
                    method: 'GET',
                    url: spWebService.getWebApiRoot() + "/spapi/data/v1/securitycache",
                    headers: spWebService.getHeaders()
                }).then(function () {
                    spAlertsService.addAlert("Invalidated", { severity: spAlertsService.sev.Success });
                }, function (message) {
                    spAlertsService.addAlert("An error occurred invalidating the cache: " + message,
                        { expires: false, severity: spAlertsService.sev.Error });
                }).finally(function () {
                    $scope.busyIndicator.isBusy = false;
                });
            };
        });
})();