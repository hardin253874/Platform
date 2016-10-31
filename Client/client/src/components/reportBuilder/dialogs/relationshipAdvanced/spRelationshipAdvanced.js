// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing relationship advanced.
    * 
    * @module spRelationshipAdvancedController
    * @example
        
    Using the spRelationshipAdvanced:
    
    spRelationshipAdvancedDialog.showModalDialog(options).then(function(result) {
    });
       
    where relationshipOptions is available on the controller with the following properties:
        - selectedNode - {object} - the selected entitytype node to load all relationships for current entity type
           
    * 
    */
    angular.module('mod.common.ui.spRelationshipAdvanced', ['ui.bootstrap', 'mod.common.alerts', 'mod.common.spEntityService', 'mod.common.ui.spEntityComboPicker'])

    .controller("spRelationshipAdvancedController", function ($scope, $uibModalInstance, options, spEntityService) {

        $scope.selectedNode = null;

        $scope.model = {
            resourceName : '',
            isResourceReportNode: false,
            isCheckExistenceOnlyEnabled: true,
            isExactTypeEnabled: true,
            isInstanceMustExistEnabled: true,
            isMustExistEnabled: true,
            isRecursionModeEnabled: true,
            isResourceMustExistEnabled: true,
            isJoinTypeEnabled: true,

            selectedNodeCheckExistenceOnly: false,
            selectedNodeExactType: false,
            selectedNodeInstanceMustExist: false,
            selectedNodeMustExist: false,            
            selectedNodeRecursionMode: 'None',
            selectedNodeResourceMustExist: false,
            selectedNodeExistMode: 'automatic'
        };

        $scope.$watch('options', function() {
            $scope.selectedNode = options.selectedNode;

            var queryEntity = $scope.selectedNode.qe;

            if (queryEntity && queryEntity.getType()) {
                initial(queryEntity);
            }

            spEntityService.getEntity($scope.selectedNode.etid, 'name').then(function (entityType) {
                $scope.model.resourceName = entityType.name;
            });

        });

        $scope.$watch('model.selectedNodeCheckExistenceOnly', function() {
            $scope.model.isRecursionModeEnabled = !$scope.model.selectedNodeCheckExistenceOnly;
            if ($scope.model.selectedNodeCheckExistenceOnly === true) {
                if ($scope.model.selectedNodeRecursionMode === 'None') {
                    $scope.model.selectedNodeRecursionMode = 'Recursive';
                }
            }
        });        

        function initial(queryEntity) {
            switch (queryEntity.getTypeAlias()) {
                case "core:resourceReportNode":
                case "resourceReportNode":
                    $scope.model.selectedNodeExactType = queryEntity.getExactType();

                    $scope.model.isExactTypeEnabled = false;
                    $scope.model.isResourceReportNode = true;
                    break;
                case "core:relationshipReportNode":
                case "relationshipReportNode":
                    $scope.model.selectedNodeResourceMustExist = queryEntity.getTargetMustExist();
                    $scope.model.selectedNodeCheckExistenceOnly = queryEntity.getFollowRecursive();
                    //$scope.model.selectedNodeRecursionMode = queryEntity.getFollowRecursive();
                    if ($scope.model.selectedNodeCheckExistenceOnly) {
                        if (queryEntity.getIncludeSelfInRecursive() === true) {
                            $scope.model.selectedNodeRecursionMode = 'RecursiveWithSelf';
                        } else {
                            $scope.model.selectedNodeRecursionMode = 'Recursive';
                        }
                    } else {
                        $scope.model.selectedNodeRecursionMode = 'None';
                    }                    

                    if (queryEntity.getTargetNeedNotExist() === true) {
                        $scope.model.selectedNodeExistMode = 'recordMayNotExist';
                    }
                    else if (queryEntity.getTargetMustExist() === true) {
                        $scope.model.selectedNodeExistMode = 'recordMustExist';
                    }  
                   
                    $scope.model.isResourceMustExistEnabled = false;
                    $scope.model.isRecursionModeEnabled = false;
                    $scope.model.isCheckExistenceOnlyEnabled = false;
                    $scope.model.isJoinTypeEnabled = false;
                    $scope.model.isResourceReportNode = false;
                    break;
                case "core:relationshipInstanceReportNode":
                case "relationshipInstanceReportNode":
                    $scope.model.selectedNodeExactType = queryEntity.getExactType();
                    $scope.model.selectedNodeResourceMustExist = queryEntity.getTargetMustExist();

                    $scope.model.isExactTypeEnabled = false;
                    $scope.model.isResourceMustExistEnabled = false;
                    $scope.model.isResourceReportNode = false;
                    break;
                case "core:derivedTypeReportNode":
                case "derivedTypeReportNode":
                    $scope.model.selectedNodeExactType = queryEntity.getExactType();
                    $scope.model.selectedNodeResourceMustExist = queryEntity.getTargetMustExist();

                    $scope.model.isExactTypeEnabled = false;
                    $scope.model.isResourceMustExistEnabled = false;
                    $scope.model.isResourceReportNode = false;
                    break;
                default:
                    $scope.model.isCheckExistenceOnlyEnabled = true;
                    $scope.model.isExactTypeEnabled = true;
                    $scope.model.isRecursionModeEnabled = true;
                    $scope.model.isResourceMustExistEnabled = true;
                    $scope.model.isJoinTypeEnabled = true;
                    $scope.model.isResourceReportNode = false;
                    break;
            }
            
            $scope.isRecursionModeEnabled = !$scope.selectedNodeCheckExistenceOnly;
        }
        
        function updateQueryEntity(queryEntity) {
            switch (queryEntity.getTypeAlias()) {
                case "core:resourceReportNode":
                case "resourceReportNode":
                    $scope.selectedNode.qe.getEntity().setExactType($scope.model.selectedNodeExactType);

                    break;
                case "core:relationshipReportNode":
                case "relationshipReportNode":
                    
                    
                    if ($scope.model.selectedNodeExistMode === 'recordMayNotExist') {
                        $scope.selectedNode.qe.getEntity().targetNeedNotExist = true;
                        $scope.selectedNode.qe.getEntity().targetMustExist = false;
                        $scope.model.selectedNodeResourceMustExist = false;
                    } else if ($scope.model.selectedNodeExistMode === 'recordMustExist')
                    {
                        $scope.selectedNode.qe.getEntity().targetNeedNotExist = false;
                        $scope.selectedNode.qe.getEntity().targetMustExist = true;
                        $scope.model.selectedNodeResourceMustExist = true;
                    }
                    else if ($scope.model.selectedNodeExistMode === 'automatic') {
                        $scope.selectedNode.qe.getEntity().targetNeedNotExist = false;
                        $scope.selectedNode.qe.getEntity().targetMustExist = false;
                        $scope.model.selectedNodeResourceMustExist = false;
                    }
                    
                    $scope.selectedNode.qe.getEntity().setFollowRecursive($scope.model.selectedNodeCheckExistenceOnly);
                    if ($scope.model.selectedNodeRecursionMode === 'RecursiveWithSelf') {
                        $scope.selectedNode.qe.getEntity().includeSelfInRecursive = true;
                    } else {
                        $scope.selectedNode.qe.getEntity().includeSelfInRecursive =false;
                    }
                    break;
                case "core:relationshipInstanceReportNode":
                case "relationshipInstanceReportNode":
                    //queryEntity.match = $scope.selectedNodeExactType;
                    //queryEntity.insmustexist = $scope.selectedNodeInstanceMustExist;
                    $scope.selectedNode.qe.getEntity().setExactType($scope.model.selectedNodeExactType);
                    $scope.selectedNode.qe.getEntity().setTargetMustExist($scope.model.selectedNodeResourceMustExist);
                    break;
                case "core:derivedTypeReportNode":
                case "derivedTypeReportNode":
                    //queryEntity.match = $scope.selectedNodeExactType;
                    //queryEntity.mustexist = $scope.selectedNodeMustExist;
                    $scope.selectedNode.qe.getEntity().setExactType($scope.model.selectedNodeExactType);
                    $scope.selectedNode.qe.getEntity().setTargetMustExist($scope.model.selectedNodeResourceMustExist);
                    break;
                default:
                    
                    break;
            }

            return queryEntity;
        }

        $scope.$watch('model.selectedNodeExactType', function () {
            if ($scope.model.selectedNodeExactType) {
                
                
            }
        });

        // click ok button to return selected relationships
        $scope.ok = function () {

            $scope.selectedNode.qe = updateQueryEntity($scope.selectedNode.qe);

            var retResult = { selectedNode: $scope.selectedNode };
            $uibModalInstance.close(retResult);
        };

        // click cancel to return report builder
        $scope.cancel = function () {
            $uibModalInstance.close(null);
        };
    })
    .factory('spRelationshipAdvancedDialog', function (spDialogService) {
        // setup the dialog
        var exports = {
            
            showModalDialog: function (options, defaultOverrides) {
                var dialogDefaults = {
                    title: 'Select Relationship',
                    keyboard: true,
                    backdropClick: true,
                    windowClass: 'modal relationshipadvanceddialog-view',
                    templateUrl: 'reportBuilder/dialogs/relationshipAdvanced/spRelationshipAdvanced.tpl.html',
                    controller: 'spRelationshipAdvancedController',
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