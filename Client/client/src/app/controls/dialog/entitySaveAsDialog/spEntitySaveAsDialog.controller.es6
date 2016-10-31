// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

// TODO - Switch this to use a custom edit form

(function () {
    'use strict';

    angular.module('app.controls.dialog.spEntitySaveAsDialog')
        .controller('spEntitySaveAsDialogController', spEntitySaveAsDialogController);

     /* @ngInject */
    function spEntitySaveAsDialogController($scope, $uibModalInstance, options, spEntityService, spAppSettings, rnFeatureSwitch) {
        var fsSelfServeEnabled = rnFeatureSwitch.isFeatureOn('fsSelfServe');

        $scope.options = options || {};
        $scope.model = {
            disableOkButton: true,
            busyIndicator: {
                type: 'spinner',
                placement: 'element'
            },            
            errors: [],
            initialContainerId: null,
            updatedContainerId: null
        };
        
        $scope.$watch('options', onOptionsChanged);

        $scope.$watch('model.entityName', onEntityNameChanged);            

        $scope.getTitle = function() {
            let typeName = $scope.options.typeName;
            return typeName ? `${typeName} Save As` : 'Resource Save As';
        };

        $scope.getLabel = function() {
            let typeName = $scope.options.typeName;
            return typeName ? `${typeName} name :` : 'Resource name :';
        };            

        // Ok callback
        $scope.ok = function() {
            $scope.model.errors = [];
            cloneAndUpdateEntity();
        };

        // Cancel callback
        $scope.cancel = function() {
            $uibModalInstance.close(null);
        };

        $scope.onContainerUpdated = function(containerId) {
            $scope.model.updatedContainerId = containerId;
        };

        $scope.canSetContainer = function () {
            if (!fsSelfServeEnabled) {
                return false;
            }

            if ($scope.options.canSetContainer) {
                return true;
            } else {
                const typeAlias = sp.result($scope, 'model.entity.type.nsAlias');
                return typeAlias === 'console:screen' || typeAlias === 'core:report' || typeAlias === 'core:chart';
            }
        };

        function onEntityNameChanged() {
            $scope.model.disableOkButton = !$scope.model.entityName || $scope.model.entityName.length <= 0 || !$scope.model.entity;
        }

        function onOptionsChanged() {
            if ($scope.options && $scope.options.entity) {
                $scope.model.entity = $scope.options.entity;
                $scope.model.entityName = $scope.model.entity.name;
                
                if ($scope.options.containerId) {
                    $scope.model.initialContainerId = $scope.options.containerId;
                }
            }
        }

        // Clones and updates the entity
        function cloneAndUpdateEntity() {
            if (!$scope.model.entity || !$scope.model.entityName) {
                return;
            }

            $scope.model.busyIndicator.isBusy = true;

            // Save the current entity state
            let entity = $scope.model.entity;
            let history = entity.graph.history;
            let preCloneBookmark = history.addBookmark('PreCloneEntity');            

            // Update name
            entity.name = $scope.model.entityName;            

            if ($scope.canSetContainer()) {
                entity.registerField('core:isPrivatelyOwned', spEntity.DataType.Bool);
                entity.isPrivatelyOwned = !spAppSettings.publicByDefault;

                if ($scope.model.updatedContainerId) {
                    entity.setRelationship('console:resourceInFolder', $scope.model.updatedContainerId);
                    entity.setField('console:consoleOrder', null, spEntity.DataType.Int32);
                }
            }

            // Do the clone and update and then revert changes to the original
            spEntityService.cloneAndUpdateEntity(entity)
                .then(function(id) {
                        // Return id of cloned entity
                        $uibModalInstance.close({ entityId: id, containerId: $scope.model.updatedContainerId });                        
                    },
                    function(error) {
                        let saveType = $scope.options.typeName ? $scope.options.typeName : 'resource';
                        let errorMsg = sp.result(error, 'data.ExceptionMessage') || sp.result(error, 'data.Message');
                        addError(`Failed to save ${saveType}. ${errorMsg}`);
                    })
                .finally(function() {
                    $scope.model.busyIndicator.isBusy = false;

                    if (history && preCloneBookmark) {
                        history.undoBookmark(preCloneBookmark);
                    }
                });
        }

        // Add an error to the page
        function addError(errorMsg) {
            $scope.model.errors.push({ type: 'error', msg: errorMsg });
        }
    }
})();