// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /**
    * Module implementing form saveAs functionality.
    * 
    * @module spFormSaveAsController
    * @example
        
    Using the spFormSaveAsController:
    
    spFormSaveAsDialog.showModalDialog(options).then(function(result) {
    });
       
    where options is available on the controller with the following properties:        
        - formEntity - {object} - the form entity
           
    * 
    */

    angular.module('mod.app.formBuilder.spFormSaveAsDialog', [
            'mod.common.ui.spDialogService',
            'mod.common.alerts',
            'mod.common.spEntityService',
            'mod.common.ui.spBusyIndicator',
            'mod.app.formBuilder.services.spFormBuilderService',
            'sp.app.settings',
            'app.controls.spNavContainerPicker',
            'mod.featureSwitch'
        ])
        .controller("spFormSaveAsController", function ($scope, $uibModalInstance, options, spEntityService, spAlertsService, spFormBuilderService, spAppSettings, rnFeatureSwitch) {
            var fsSelfServeEnabled = rnFeatureSwitch.isFeatureOn('fsSelfServe');

            $scope.options = options || {};
            $scope.model = {
                disableOkButton: true,
                gridBusyIndicator: {
                    type: 'spinner',
                    placement: 'element'
                },
                errors: [],
                initialContainerId: null,
                updatedContainerId: null
            };

            $scope.canSetContainer = function () {
                return fsSelfServeEnabled && $scope.options.mode === spFormBuilderService.builders.screen;
            };

            $scope.getTitle = function () {
                return $scope.options.mode  === spFormBuilderService.builders.screen ? 'Screen Save As' : 'Form Save As';
            };

            $scope.getLabel = function () {
                return $scope.options.mode === spFormBuilderService.builders.screen ? 'Screen name' : 'Form name';
            };

            $scope.$watch('options', function() {
                if ($scope.options && $scope.options.formEntity) {
                    $scope.model.formEntity = $scope.options.formEntity;
                    $scope.model.formName = $scope.model.formEntity.getName();

                    if ($scope.options.containerId) {
                        $scope.model.initialContainerId = $scope.options.containerId;
                    }
                }
            });

            $scope.$watch('model.formName', function() {
                if ($scope.model.formName && $scope.model.formName.length > 0) {
                    $scope.model.disableOkButton = false;
                } else {
                    $scope.model.disableOkButton = true;
                }
            });

            $scope.cloneFormEntity = function () {                
                var formEntity, preCloneBookmark, history;

                if ($scope.model.formEntity) {
                    $scope.model.gridBusyIndicator.isBusy = true;

                    formEntity = $scope.model.formEntity;
                    history = formEntity.graph.history;
                    preCloneBookmark = history.addBookmark('PreCloneForm');
                    
                    formEntity.name = $scope.model.formName;

                    // note: edit forms do not inherit the isPrivatelyOwned field (at the moment)
                    if (formEntity.type.nsAlias === 'console:screen') {
                        formEntity.registerField('core:isPrivatelyOwned', spEntity.DataType.Bool);
                        formEntity.isPrivatelyOwned = !spAppSettings.publicByDefault;

                        if ($scope.model.updatedContainerId) {
                            formEntity.setRelationship('console:resourceInFolder', $scope.model.updatedContainerId);
                            formEntity.setField('console:consoleOrder', null, spEntity.DataType.Int32);
                        }
                    }

                    spEntityService.cloneAndUpdateEntity(formEntity).then(function (id) {
                        $uibModalInstance.close({ formId: id, containerId: $scope.model.updatedContainerId });
                    }, function (error) {
                        var saveType = $scope.options.mode === spFormBuilderService.builders.screen ? 'screen' : 'form';
                        addError('Failed to save ' + saveType + '. ' + (error.data.ExceptionMessage || error.data.Message));
                    }).finally(function () {
                        $scope.model.gridBusyIndicator.isBusy = false;

                        if (history && preCloneBookmark) {
                            history.undoBookmark(preCloneBookmark);   
                        }                        
                    });
                }
            };

            $scope.onContainerUpdated = function(containerId) {
                $scope.model.updatedContainerId = containerId;
            };

            /////
            // Add an error
            /////

            function addError(errorMsg) {
                $scope.model.errors.push({ type: 'error', msg: errorMsg });
            }

            /////
            // click ok to return report builder
            /////
            $scope.ok = function() {
                $scope.model.errors = [];
                $scope.cloneFormEntity();
            };

            /////
            // click cancel to return report builder
            /////
            $scope.cancel = function() {
                $uibModalInstance.close(null);
            };

        })
        .factory('spFormSaveAsDialog', function(spDialogService) {
            // setup the dialog
            var exports = {
                showModalDialog: function(options, defaultOverrides) {
                    var dialogDefaults = {
                        title: 'Form Save As',
                        keyboard: true,
                        backdropClick: true,
                        windowClass: 'modal formSaveAsDialog',
                        templateUrl: 'formBuilder/controllers/spFormSaveAsDialog/spFormSaveAsDialog.tpl.html',
                        controller: 'spFormSaveAsController',
                        resolve: {
                            options: function() {
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