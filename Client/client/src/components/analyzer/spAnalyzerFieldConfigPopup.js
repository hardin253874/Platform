// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing the analyzer field configuration dialog.    
    * 
    * @module spAnalyzerFieldConfigPopup   
    * @example
        
    Using the spAnalyzerFieldConfigPopup:
    
    spAnalyzerFieldConfigPopup.showModalDialog(options).then(function(result) {
    });
    
    where options is an object with the following property:
        - field. The analyzer field to be configured.

    where result is:
        - false, if cancel is clicked
        - entity/null - resource picker entity (Report entity/hierarchy entity) or null if 'Default' option is selected when ok is clicked
    */
    angular.module('mod.common.ui.spAnalyzerFieldConfigPopup', ['ui.bootstrap', 'mod.common.ui.spDialogService'])
        .controller('spAnalyzerFieldConfigPopupController', function ($scope, $uibModalInstance, options, spEntityService) {

            // Setup the dialog model
            $scope.model = {
                field: options.field,
                errors: [],
            };

            var entityTypeId = sp.result($scope.model, 'field.entityTypeId');
            if (entityTypeId) {
                loadReportsForType(entityTypeId);
            }

            // Methods
            
            function loadReportsForType(typeId) {
                var buildString = 'alias, definitionUsedByReport.{ alias, name, isOfType.alias, reportForAccessRule.name }';
                spEntityService.getEntity(typeId, buildString).then(
                    function (result) {
                        var typeResourcePickers = [];
                        var inherits = spResource.getAncestorsAndSelf(result);
                        _.forEach(inherits, function (inherit) {
                            var typeReports = inherit.getRelationship('definitionUsedByReport');
                            typeResourcePickers = typeResourcePickers.concat(typeReports);
                        });

                        // filter out the access rule reports
                        var filteredResourcePickers = _.filter(typeResourcePickers, function (r) {
                            return r.getRelationship('reportForAccessRule').length === 0;
                        });
                        $scope.model.typeResourcePickers = filteredResourcePickers;

                        // set the selected picker report
                        var pickerRptId = sp.result(options, 'field.pickerReportId');
                        if (pickerRptId) {
                            $scope.model.selectedPickerReport = _.find($scope.model.typeResourcePickers, { 'idP': pickerRptId });
                        }
                    },
                    function (error) {
                        $scope.model.addError(error);
                    });
            }

            // Clear any errors
            $scope.model.clearErrors = function () {
                $scope.model.errors = [];
            };
            
            // Add an error
            $scope.model.addError = function (errorMsg) {
                $scope.model.errors.push({ type: 'error', msg: errorMsg });
            };
            
            // Ok click handler
            $scope.ok = function () {
               $scope.model.clearErrors();

               if ($scope.model.errors.length === 0) {
                   var result = sp.result($scope.model, 'selectedPickerReport.idP');    // undefined or id

                   $uibModalInstance.close(result);
                }
            };

            // Cancel click handler
            $scope.cancel = function () {
                $uibModalInstance.close(false);
            };
        })
        .factory('spAnalyzerFieldConfigPopup', function (spDialogService) {
            // setup the dialog
            var exports = {
                showModalDialog: function (options, defaultOverrides) {
                    var dialogDefaults = {
                        backdrop: true,
                        keyboard: true,
                        backdropClick: false,
                        templateUrl: 'analyzer/spAnalyzerFieldConfigPopup.tpl.html',
                        controller: 'spAnalyzerFieldConfigPopupController',
                        windowClass: 'spAnalyzerFieldConfigPopup',
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