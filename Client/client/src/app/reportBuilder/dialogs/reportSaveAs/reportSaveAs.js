// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spReportEntity */

(function () {
    'use strict';

    /**
    * Module implementing save current report to different entity.
    * 
    * @module spReportSaveAsController
    * @example
        
    Using the spReportSaveAsController:
    p
    spReportSaveAsDialog.showModalDialog(options).then(function(result) {
    });
       
    where reportOptions is available on the controller with the following properties:        
        - reportEntity - {object} - the report entity
           
    * 
    */

    angular.module('mod.app.reportSaveAs', ['mod.common.ui.spDialogService', 'mod.common.alerts', 'mod.common.spEntityService', 'spApps.reportServices', 'mod.common.ui.spBusyIndicator', 'sp.app.settings', 'app.controls.spNavContainerPicker', 'mod.featureSwitch'])
     .controller("spReportSaveAsController", function ($scope, $uibModalInstance, options, spEntityService, spReportService, spAppSettings, rnFeatureSwitch) {
         $scope.options = options || {};
         $scope.model = {
             reportEntity: null,
             reportName: '',
             disableOkButton: true,
             cloningInProgress:false,
             gridBusyIndicator: {
                 type: 'spinner',
                 placement: 'element'
             },
             errors: [],
             initialContainerId: null,
             updatedContainerId: null
         };
         
         var fsSelfServeEnabled = rnFeatureSwitch.isFeatureOn('fsSelfServe');

         $scope.$watch('options', function () {
             if ($scope.options && $scope.options.reportEntity) {
                 $scope.model.reportEntity = $scope.options.reportEntity;
                 $scope.model.reportName = $scope.model.reportEntity.getName();
                 
                 if ($scope.options.containerId) {
                    $scope.model.initialContainerId = $scope.options.containerId;
                 }

                 $scope.getReportNewName($scope.model.reportEntity.getEntity().id());
             }
         });

         $scope.$watch('model.reportName', function() {
             if ($scope.model.reportName && $scope.model.reportName.length > 0) {
                 $scope.model.disableOkButton = false;
             } else {
                 $scope.model.disableOkButton = true;
             }
         });

         $scope.getReportNewName = function (rid) {
           
             spReportService.getNewReportName(rid).then(function (response) {
                 $scope.model.reportName = response;
                
             });
         };

         $scope.cloneReportEntity = function() {
             if ($scope.model.reportEntity) {                 
                 $scope.model.gridBusyIndicator.isBusy = true;
                 var cloneReportEntity = spReportEntity.cloneReportEntity($scope.model.reportEntity.getEntity());
                 cloneReportEntity.name = $scope.model.reportName;

                 cloneReportEntity.registerField('core:isPrivatelyOwned', spEntity.DataType.Bool);
                 cloneReportEntity.isPrivatelyOwned = !spAppSettings.publicByDefault;

                 if ($scope.model.updatedContainerId) {
                     cloneReportEntity.setRelationship('console:resourceInFolder', $scope.model.updatedContainerId);
                     cloneReportEntity.setField('console:consoleOrder', null, spEntity.DataType.Int32);
                 }

                 if (cloneReportEntity) {
                     //$uibModalInstance.close(cloneReportentity);

                     spEntityService.putEntity(cloneReportEntity).then(function (id) {
                         $scope.model.gridBusyIndicator.isBusy = false;
                         $uibModalInstance.close({ reportId: id, containerId: $scope.model.updatedContainerId });
                     }, function (error) {
                         $scope.model.gridBusyIndicator.isBusy = false;
                         addError('Fail to save as report, ' + (error.data.ExceptionMessage || error.data.Message));
                           //spAlertsService.addAlert('Fail to save as report, ' + error.data.ExceptionMessage);
                       })
                     .finally(function () {
                         $scope.model.cloningInProgress = false;
                     });
                 }
              
             }
         };

         $scope.canSetContainer = function() {
             return fsSelfServeEnabled;
         };

         // Add an error
         function addError(errorMsg) {
             $scope.model.errors.push({ type: 'error', msg: errorMsg });
         }

         // click ok to return report builder
         $scope.ok = function () {
             $scope.model.errors = [];
             if (!$scope.model.cloningInProgress) {
                 $scope.model.cloningInProgress = true;
                 $scope.model.disableOkButton = true;
                 $scope.cloneReportEntity();
             }
             
         };         

         // click cancel to return report builder
         $scope.cancel = function () {
             $uibModalInstance.close(null);
         };


        $scope.onContainerUpdated = function(containerId) {
            $scope.model.updatedContainerId = containerId;
        };
    })
    .factory('spReportSaveAsDialog', function (spDialogService) {
        // setup the dialog
        var exports = {

            showModalDialog: function (options, defaultOverrides) {
                var dialogDefaults = {
                    title: 'Report Save As',
                    keyboard: true,
                    backdropClick: true,
                    windowClass: 'modal reportSaveAsDialog-view',
                    templateUrl: 'reportBuilder/dialogs/reportSaveAs/reportSaveAs.tpl.html',
                    controller: 'spReportSaveAsController',
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