// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing the sort options dialog.    
    * 
    * @module spSortOptionsDialog   
    * @example
        
    Using the spSortOptionsDialog:
    
    spSortOptionsDialog.showModalDialog(options).then(function(result) {
    });
    
    where options is an object with the following properties:
        - columns - {array}. The list of available columns.
            - columns[].id - {string}. The column id   
            - columns[].name - {string}. The name of the column
            - columns[].isGroupingColumn - {bool}. True if the column is a grouping column false otherwise
        - sortInfo - {array}. The list of sorted columns.
            - sortInfo[].columnId - {string}. The column id   
            - sortInfo[].sortDirection - {string}. The sort direction, either 'asc' or 'desc'

    where result is:
        - false, if cancel is clicked
        - sortInfo - {array} if ok is clicked
    */
    angular.module('mod.common.ui.spSortOptionsDialog', ['ui.bootstrap', 'mod.common.ui.spDialogService'])
        .controller('spSortOptionsDialogController', function ($scope, $uibModalInstance, options) {

            // Setup the dialog model
            $scope.model = {
                errors: [],
                sortDirections: ['Ascending', 'Descending'],
                columns: options.columns,
                sortInfo: _.map(options.sortInfo, function (si) {
                    return {
                        column: _.find(options.columns, function (c) {
                            return c.id === si.columnId;
                        }),
                        sortDirection: si.sortDirection === 'asc' ? 'Ascending' : 'Descending'
                    };
                })
            };

            // Methods


            // Clear any errors
            $scope.model.clearErrors = function () {
                $scope.model.errors = [];
            };


            // Add an error
            $scope.model.addError = function (errorMsg) {
                $scope.model.errors.push({ type: 'error', msg: errorMsg });
            };


            // Returns true if a new sort info element can be added
            $scope.canAddSortInfo = function () {
                return $scope.model.sortInfo.length < $scope.model.columns.length;
            };


            // Adds a new sort info element
            $scope.addSortInfo = function () {
                // Sanity check
                if (!$scope.canAddSortInfo()) {
                    return;
                }

                $scope.model.clearErrors();

                $scope.model.sortInfo.push({
                    column: null,
                    sortDirection: 'Ascending'
                });
            };


            // Remove the specified sort info element
            $scope.removeSortInfo = function (index) {
                $scope.model.clearErrors();

                $scope.model.sortInfo.splice(index, 1);
            };


            // Ok click handler
            $scope.ok = function () {
                var sortedColumnIds = {},
                    groupingColumns = [],
                    sortedColumnsResult = [];

                $scope.model.clearErrors();

                // Validate the sortInfo                
                _.forEach($scope.model.sortInfo, function (si) {
                    if (!si.column) {
                        // A column has not been specified
                        $scope.model.addError('A column to sort by must be selected.');
                        return false;
                    }

                    if (_.has(sortedColumnIds, si.column.id)) {
                        // The column has already been specified
                        $scope.model.addError('The column \'' + si.column.name + '\' has been sorted multiple times.');
                        return false;
                    }

                    sortedColumnIds[si.column.id] = true;

                    sortedColumnsResult.push({
                        columnId: si.column.id,
                        sortDirection: si.sortDirection === 'Ascending' ? 'asc' : 'desc'
                    });
                    return true;
                });

                // Ensure that if any of the available columns
                // are grouping columns then these columns are be sorted by 
                // first and in the same order as the grouping columns
                groupingColumns = _.sortBy(_.filter($scope.model.columns, 'isGroupingColumn'), 'groupingColumnIndex');
                if (groupingColumns.length) {                    
                    _.forEach(groupingColumns, function (gc, index) {
                        var scr = sortedColumnsResult[index];
                        if (!scr ||
                            scr.columnId !== gc.id) {
                            // The result column at the specified index is not
                            // the specified group column
                            $scope.model.addError('This report has grouped columns. Group by columns need to be specified first and in the same order as the group order.');
                            return false;
                        }
                    });
                }

                if ($scope.model.errors.length === 0) {
                    $uibModalInstance.close(sortedColumnsResult);
                }
            };


            // Cancel click handler
            $scope.cancel = function () {
                $uibModalInstance.close(false);
            };
        })
        .factory('spSortOptionsDialog', function (spDialogService) {
            // setup the dialog
            var exports = {
                showModalDialog: function (options, defaultOverrides) {
                    var dialogDefaults = {
                        keyboard: true,
                        backdropClick: false,
                        templateUrl: 'reports/dialogs/sortOptionsDialog/spSortOptionsDialog.tpl.html',
                        controller: 'spSortOptionsDialogController',
                        windowClass: 'spSortOptionsDialog',
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