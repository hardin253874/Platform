// Copyright 2011-2016 Global Software Innovation Pty Ltd


(function () {
    'use strict';

    /**
    * Module implementing a dialog for publishing applications.
    *
    * @module mod.app.applicationManager.services.spAppManagerPublishService
    */
    angular.module('mod.app.applicationManager.services.spAppManagerPublishService', [
        'mod.app.applicationManager.services.spAppManagerService',
        'mod.app.applicationManager.services.spAppManagerCardinalityViolationsService'
    ])
        .controller('spAppManagerPublishController', ['$scope', '$uibModalInstance', '$timeout', 'options', 'spAppManagerService', 'spAppManagerCardinalityViolationsService', function ($scope, $uibModalInstance, $timeout, options, spAppManagerService, spAppManagerCardinalityViolationsService) {

            var emptyDate = new Date('0001-01-01T00:00:00');

            /////
            // Setup the dialog model
            /////
            $scope.model = {
                title: 'Publish',
                busyIndicator: {
                    type: 'spinner',
                    text: 'Loading...',
                    placement: 'element',
                    isBusy: false
                },
                app: options.data,
                appName: options.name,
                notStaged: true,
                showDiffOnly: false,
                errorMessage: '',
                clickErrorMessage: '',
                search: {
                    onSearchValueChanged: _.debounce(onSearchValueChanged, 250),
                    value: ''
                }
            };

            /////
            // Only show the diff rows.
            /////
            $scope.showDiffOnly = function() {
                applyFilters($scope.results, ($scope.model.search.value || '').toLowerCase());
            };

            /////
            // Search the results.
            /////
            function onSearchValueChanged() {
                applyFilters($scope.results, ($scope.model.search.value || '').toLowerCase());
            }

            $scope.clickError = function() {
                var violationOptions = {
                    violations: $scope.model.violations
            };

                return spAppManagerCardinalityViolationsService.showDialog(violationOptions);
            };

            /////
            // Stage the application.
            /////
            $scope.stageApp = function() {
                $scope.model.busyIndicator.isBusy = true;
                $scope.model.busyIndicator.text = 'Staging results...';

                spAppManagerService.stageApplication(options.data.id).then(function (results) {

                    if (!results) {
                        return;
                    }

                    if (results.cardinalityViolations) {
                        $scope.model.clickErrorMessage = 'Warning: Cardinality violations detected in application. Click to examine.';
                        $scope.model.violations = results.cardinalityViolations;
                    }

                    $scope.model.busyIndicator.text = 'Formatting...';

                    $scope.addMap = _.keyBy(results.entities, 'uid');
                    $scope.results = orderDependencies(results.entities);

                    $scope.ordered = $scope.results;

                }).finally(function () {
                    $scope.model.busyIndicator.isBusy = false;
                    $scope.model.notStaged = false;
                });
            };

            /////
            // Filter the results.
            /////
            function applyFilters(items, filter) {

                if (!$scope.model.showDiffOnly && !filter) {
                    $scope.ordered = $scope.results;
                    return;
                }

                var passed = {};
                var parentId;

                _.forEach(items, function (item) {
                    if (!!(!filter || (((item.name || '').toLowerCase().indexOf(filter) >= 0) || ((item.typeName || '').toLowerCase().indexOf(filter) >= 0))) && !!(!$scope.model.showDiffOnly || (item.hasOwnProperty('state') && item.state !== 0))) {
                        passed[item.uid] = true;

                        parentId = item.parents && item.parents.length && item.parents[0].parentUid;

                        while (parentId) {

                            passed[parentId] = true;

                            item = $scope.addMap[parentId];

                            parentId = item && item.parents && item.parents.length && item.parents[0].parentUid;
                        }
                    }
                });

                var results = [];

                _.forEach(items, function (item) {
                    if (passed.hasOwnProperty(item.uid)) {
                        results.push(item);
                    }
                });

                $scope.ordered = results;
            }

            /////
            // Ok clicked.
            /////
            $scope.ok = function () {

                $scope.model.busyIndicator.isBusy = true;
                $scope.model.busyIndicator.text = 'Publishing...';

                return spAppManagerService.publishApplication(options.data.id).then(function(results) {
                    $uibModalInstance.close();
                }, function (error) {
                    $scope.model.errorMessage = error;
                })
                .finally(function() {
                    $scope.model.busyIndicator.isBusy = false;
                });
            };

            /////
            // Cancel clicked.
            /////
            $scope.cancel = function() {
                $uibModalInstance.close(false);
            };

            /////
            // Determines whether the specified date represents an empty date time.
            /////
            $scope.emptyDate = function(value) {
                try {
                    var dt = new Date(value);
                    

                    return dt.getTime() === emptyDate.getTime();
                } catch(error) {
                }

                return true;
            };

            /////
            // Gets the indent icon.
            /////
            $scope.getIndentIcon = function (item) {
                if (!item.parents || !item.parents.length) {
                    return '';
                }

                return 'assets/images/childlink.png';
            };

            /////
            // Get the indent style.
            /////
            $scope.getIndentStyle = function(item) {
                var style = {};

                var width = item.parents && item.parents.length && item.parents[0].depth * 10;

                style['margin-left'] = width + 'px';

                return style;
            };

            /////
            // Get the row classes.
            /////
            $scope.getRowClass = function(item) {
                var classes = '';

                if (item.parents && item.parents.length) {
                    classes += ' childRow';
                }

                if (item.state === 2) {
                    classes += ' rowAdded';
                }

                if (item.state === 4) {
                    classes += ' rowRemoved';
                }

                return classes;
            };

            /////
            // Get the relationship name that was followed.
            /////
            $scope.getRelationship = function (item) {
                var relationship = '';

                if (item.parents && item.parents.length) {
                    relationship = item.parents[0].relTypeName;
                }

                return relationship;
            };

            /////
            // Gets the direction of the relationship.
            /////
            $scope.getDirection = function (item) {
                var direction = '';

                if (item.parents && item.parents.length) {
                    switch (item.parents[0].reason) {
                        case 0:
                            direction = '';
                            break;
                        case 1:
                            direction = 'Forward';
                            break;
                        case 2:
                            direction = 'Reverse';
                            break;
                        case 3:
                            direction = 'Forward';
                            break;
                        case 4:
                            direction = 'Reverse';
                            break;
                        case 5:
                            direction = 'N/A';
                            break;
                    }
                }

                return direction;
            };

            /////
            // Gets the reason for this items inclusion.
            /////
            $scope.getReason = function (item) {
                var reason = '';

                if (!item.state || item.state === 0) {
                    reason = 'No change';
                } else if (item.state && item.state === 2) {
                    if (!item.parents || !item.parents.length || item.parents[0].reason === 0) {
                        reason = 'Explicitly included in the application';
                    } else {
                        var parent = item.parents[0];
                        var parentEntity = $scope.addMap[parent.parentUid];

                        switch (parent.reason) {
                        case 1:
                            reason = 'Implicitly included after following the forward relationship \'' + parent.relTypeName + '\' from the ' + (parentEntity.typeName || 'unknown type') + ' \'' + (parentEntity.name || 'unnamed') + '\'';
                            break;
                        case 2:
                            reason = 'Implicitly included after following the reverse relationship \'' + parent.relTypeName + '\' from the ' + (parentEntity.typeName || 'unknown type') + ' \'' + (parentEntity.name || 'unnamed') + '\'';
                            break;
                        case 3:
                            reason = 'Implicitly including the relationship instance after following the forward relationship \'' + parent.relTypeName + '\' from the ' + (parentEntity.typeName || 'unknown type') + ' \'' + (parentEntity.name || 'unnamed') + '\'';
                            break;
                        case 4:
                            reason = 'Implicitly including the relationship instance after following the reverse relationship \'' + parent.relTypeName + '\' from the ' + (parentEntity.typeName || 'unknown type') + ' \'' + (parentEntity.name || 'unnamed') + '\'';
                            break;
                        case 5:
                            reason = 'Implicitly including the relationship instance for the relationships type \'' + parent.relTypeName + '\'';
                            break;
                        }
                    }
                } else if (item.state && item.state === 4) {
                    reason = 'Explicitly removed';
                }

                return reason;
            };

            /////
            // Orders the dependencies.
            /////
            function orderDependencies(items) {

                if (!items || !items.length) {
                    return [];
                }

                var orderedList = [];
                var unorderedList = items.slice(0);
                var index;

                /////
                // The items are already ordered by depth and name, just need to shuffle the array.
                /////
                _.forEach(unorderedList, function(entry) {
                    if (!entry.parents || !entry.parents.length) {
                        orderedList.push(entry);
                    } else {
                        index = _.findIndex(orderedList, function(orderedEntry) {
                            return orderedEntry.uid === entry.parents[0].parentUid;
                        });

                        if (index >= 0) {
                            orderedList.splice(index + 1, 0, entry);
                        }
                    }
                });

                return orderedList;
            }

            /////
            // Load function.
            /////
            function load() {

                $scope.gridOptions = {
                    data: 'ordered',
                    rowHeight: 20,
                    headerRowHeight: 22,
                    enableRowSelection: false,
                    filterOptions: {
                        useExternalFilter: true
                    },
                    rowTemplate: '<div ng-style="{ \'cursor\': row.cursor }" uib-tooltip="{{getReason(row.entity)}}" ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}"><div ng-cell></div></div>',
                    columnDefs: [
                        {
                            displayName: 'Name',
                            sortable: false,
                            resizable: false,
                            groupable: false,
                            cellTemplate: '<div ng-class="getRowClass(row.entity)"><img ng-src="{{getIndentIcon(row.entity)}}" ng-if="getIndentIcon(row.entity)" ng-style="getIndentStyle(row.entity)" class="indentIcon" /><span class="indentText">{{row.entity.name || \'&lt;No Name&gt;\'}}</span></div>'
                        },
                        {
                            displayName: 'Type',
                            sortable: false,
                            resizable: false,
                            groupable: false,
                            cellTemplate: '<div ng-class="getRowClass(row.entity)">{{row.entity.typeName}}</div>'
                        },
                        {
                            displayName: 'Relationship Followed',
                            sortable: false,
                            resizable: false,
                            groupable: false,
                            cellTemplate: '<div ng-class="getRowClass(row.entity)">{{getRelationship(row.entity)}}</div>'
                        },
                        {
                            displayName: 'Direction',
                            sortable: false,
                            resizable: false,
                            groupable: false,
                            cellTemplate: '<div ng-class="getRowClass(row.entity)">{{getDirection(row.entity)}}</div>'
                        }
                    ]
                };
            }

            /////
            // Initial load.
            /////
            load();
        }])
        .service('spAppManagerPublishService', ['spDialogService', function (spDialogService) {
            // setup the dialog
            var exports = {
                showDialog: function (options) {
                    var dialogOptions = {
                        templateUrl: 'applicationManager/services/spAppManagerPublishService/spAppManagerPublishService.tpl.html',
                        controller: 'spAppManagerPublishController',
                        windowClass: 'appManager-publish',
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