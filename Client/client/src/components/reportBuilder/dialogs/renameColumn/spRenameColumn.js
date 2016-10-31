// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing save current report to different entity.
    * 
    * @module spRenameColumnController
    * @example
        
    Using the spRenameColumnController:
    p
    spRenameColumnDialog.showModalDialog(options).then(function(result) {
    });
       
    where reportOptions is available on the controller with the following properties:        
        - reportColumn - {object} - the report column
           
    * 
    */

    angular.module('mod.common.ui.spRenameColumn', ['mod.common.ui.spDialogService', 'mod.common.alerts', 'mod.common.ui.spBusyIndicator'])
     .controller("spRenameColumnController", function ($scope, $uibModalInstance, options,  spAlertsService) {
         $scope.options = options || {};
         $scope.model = {
             reportColumn: null, columnName: '', disableOkButton: true, gridBusyIndicator: {
                 type: 'spinner',
                 placement: 'element'
             }
         };


         $scope.$watch('options', function () {
             if ($scope.options && $scope.options.reportColumn) {
                 $scope.model.reportColumn = $scope.options.reportColumn;
                 $scope.model.columnName = $scope.model.reportColumn.getName();                
             }
         });

         $scope.$watch('model.columnName', function () {
             if ($scope.model.columnName && $scope.model.columnName.length > 0 && $scope.model.columnName !== $scope.options.reportColumn.getName()) {
                 $scope.model.disableOkButton = false;
             } else {
                 $scope.model.disableOkButton = true;
             }
         });        

         // click ok to return report builder
         $scope.ok = function () {
             $scope.model.gridBusyIndicator.isBusy = true;

             var result = { columnId: $scope.model.reportColumn.id(), columnName: $scope.model.columnName };
             $scope.model.gridBusyIndicator.isBusy = false;

             $uibModalInstance.close(result);


         };

         // click cancel to return report builder
         $scope.cancel = function () {
             $uibModalInstance.close(null);
         };

     })
    .factory('spRenameColumnDialog', function (spDialogService) {
        // setup the dialog
        var exports = {

            showModalDialog: function (options, defaultOverrides) {
                var dialogDefaults = {
                    title: 'Rename Column',
                    keyboard: true,
                    backdropClick: true,
                    windowClass: 'modal spRenameColumnDialog-view',
                    templateUrl: 'reportBuilder/dialogs/renameColumn/spRenameColumn.tpl.html',
                    controller: 'spRenameColumnController',
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