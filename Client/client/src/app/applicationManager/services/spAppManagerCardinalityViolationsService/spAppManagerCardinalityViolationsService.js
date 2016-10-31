// Copyright 2011-2016 Global Software Innovation Pty Ltd


(function () {
    'use strict';

    /**
    * Module implementing a dialog for publishing applications.
    *
    * @module mod.app.applicationManager.services.spAppManagerPublishService
    */
    angular.module('mod.app.applicationManager.services.spAppManagerCardinalityViolationsService', [
        'mod.app.applicationManager.services.spAppManagerService'
    ])
        .controller('spAppManagerCardinalityViolationsController', ['$scope', '$uibModalInstance', '$timeout', 'options', 'spAppManagerService', function ($scope, $uibModalInstance, $timeout, options, spAppManagerService) {

            /////
            // Setup the dialog model
            /////
            $scope.model = {
                title: 'Cardinality Violations'
            };

            $scope.data = options.violations;

            $scope.ok = function () {
                $uibModalInstance.close(false);
            };

            /////
            // Load function.
            /////
            function load() {

                $scope.gridOptions = {
                    data: 'data',
                    rowHeight: 20,
                    headerRowHeight: 22,
                    enableRowSelection: false,
                    filterOptions: {
                        useExternalFilter: true
                    },
                    rowTemplate: '<div ng-style="{ \'cursor\': row.cursor }" ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div ng-cell></div></div>',
                    columnDefs: [
                        {
                            displayName: 'Type',
                            sortable: false,
                            resizable: false,
                            groupable: false,
                            cellTemplate: '<div ng-class="getRowClass(row.entity)">{{row.entity.type}}</div>'
                        },
                        {
                            displayName: 'From',
                            sortable: false,
                            resizable: false,
                            groupable: false,
                            cellTemplate: '<div ng-class="getRowClass(row.entity)">{{row.entity.from}}</div>'
                        },
                        {
                            displayName: 'To',
                            sortable: false,
                            resizable: false,
                            groupable: false,
                            cellTemplate: '<div ng-class="getRowClass(row.entity)">{{row.entity.to}}</div>'
                        },
                    ]
                };
            }

            /////
            // Initial load.
            /////
            load();
        }])
        .service('spAppManagerCardinalityViolationsService', ['spDialogService', function (spDialogService) {
            // setup the dialog
            var exports = {
                showDialog: function (options) {
                    var dialogOptions = {
                        templateUrl: 'applicationManager/services/spAppManagerCardinalityViolationsService/spAppManagerCardinalityViolationsService.tpl.html',
                        controller: 'spAppManagerCardinalityViolationsController',
                        windowClass: 'appManager-cardinality',
                        resolve: {
                            options: function () {
                                return options;
                            }
                        }
                    };

                    return spDialogService.showModalDialog(dialogOptions);
                }
            };

            return exports;
        }]);
}());