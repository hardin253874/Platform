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
    angular.module('mod.common.ui.spRelationshipAdvanced', [
        'ui.bootstrap',
        'mod.common.alerts',
        'mod.common.spEntityService',
        'mod.common.ui.spEntityComboPicker',
        'mod.common.ui.spReportBuilder.spCustomJoinExpression'])

    .controller("spRelationshipAdvancedController", function ($scope, $uibModalInstance, options, spEntityService) {

        $scope.selectedNode = null;

        var model = {
            resourceName: '',
            isResourceReportNode: false,
            isCustomJoinNode: false,
            isCheckExistenceOnlyEnabled: true,
            isExactTypeEnabled: true,
            isInstanceMustExistEnabled: true,
            isMustExistEnabled: true,
            isRecursionModeEnabled: true,
            isResourceMustExistEnabled: true,
            isJoinTypeEnabled: true,
            isParentNeedNotExistEnabled: true,

            selectedNodeCheckExistenceOnly: false,
            selectedNodeExactType: false,
            selectedNodeInstanceMustExist: false,
            selectedNodeMustExist: false,
            selectedNodeRecursionMode: 'None',
            selectedNodeResourceMustExist: false,
            selectedNodeExistMode: 'automatic',
            selectedNodeParentNeedNotExist: false,

            customJoinScript: null,
            customJoinScriptOptions: {
                childTypeId: 0,
                parentTypeId: 0
            }
        };
        $scope.model = model;

        $scope.$watch('options', function () {
            $scope.selectedNode = options.selectedNode;

            var queryEntity = $scope.selectedNode.qe;
            var parentEntity = $scope.selectedNode.pe;

            if (queryEntity && queryEntity.getType()) {
                initial(queryEntity, parentEntity);
            }

            spEntityService.getEntity($scope.selectedNode.etid, 'name').then(function (entityType) {
                model.resourceName = entityType.name;
            });

        });

        $scope.$watch('model.selectedNodeCheckExistenceOnly', function () {
            model.isRecursionModeEnabled = !model.selectedNodeCheckExistenceOnly;
            if (model.selectedNodeCheckExistenceOnly === true) {
                if (model.selectedNodeRecursionMode === 'None') {
                    model.selectedNodeRecursionMode = 'Recursive';
                }
            }
        });

        function initial(queryEntity, parentEntity) {
            var typeAlias = queryEntity.getEntity().type.nsAlias;

            // Caution : This logic is backwards!! Mark things as 'enabled' to disable them.  TODO: Fix!!

            switch (typeAlias) {
                case "core:resourceReportNode":
                    model.selectedNodeExactType = queryEntity.getExactType();

                    model.isExactTypeEnabled = false;
                    model.isResourceReportNode = true;
                    break;
                case "core:customJoinReportNode":
                    var customJoin = queryEntity.getEntity();
                    model.selectedNodeResourceMustExist = queryEntity.getTargetMustExist();
                    model.selectedNodeParentNeedNotExist = customJoin.parentNeedNotExist;
                    if (queryEntity.getTargetNeedNotExist() === true) {
                        model.selectedNodeExistMode = 'recordMayNotExist';
                    }
                    else if (queryEntity.getTargetMustExist() === true) {
                        model.selectedNodeExistMode = 'recordMustExist';
                    }

                    model.isResourceMustExistEnabled = false;
                    model.isRecursionModeEnabled = true;
                    model.isCheckExistenceOnlyEnabled = false;
                    model.isJoinTypeEnabled = false;
                    model.isParentNeedNotExistEnabled = false;
                    model.isResourceReportNode = false;
                    model.isCustomJoinNode = true;

                    var parentTypeId = 0;
                    if (parentEntity) {
                        parentTypeId = sp.result(parentEntity.getEntity(), 'resourceReportNodeType.idP');
                    }
                    model.customJoinScript = customJoin.joinPredicateCalculation;
                    model.customJoinScriptOptions = {
                        childTypeId: customJoin.resourceReportNodeType.idP,
                        parentTypeId: parentTypeId
                    };
                    break;
                case "core:relationshipReportNode":
                    model.selectedNodeResourceMustExist = queryEntity.getTargetMustExist();
                    model.selectedNodeParentNeedNotExist = queryEntity.getEntity().parentNeedNotExist;
                    model.selectedNodeCheckExistenceOnly = queryEntity.getFollowRecursive();
                    //model.selectedNodeRecursionMode = queryEntity.getFollowRecursive();
                    if (model.selectedNodeCheckExistenceOnly) {
                        if (queryEntity.getIncludeSelfInRecursive() === true) {
                            model.selectedNodeRecursionMode = 'RecursiveWithSelf';
                        } else {
                            model.selectedNodeRecursionMode = 'Recursive';
                        }
                    } else {
                        model.selectedNodeRecursionMode = 'None';
                    }

                    if (queryEntity.getTargetNeedNotExist() === true) {
                        model.selectedNodeExistMode = 'recordMayNotExist';
                    }
                    else if (queryEntity.getTargetMustExist() === true) {
                        model.selectedNodeExistMode = 'recordMustExist';
                    }

                    model.isResourceMustExistEnabled = false;
                    model.isRecursionModeEnabled = false;
                    model.isCheckExistenceOnlyEnabled = false;
                    model.isJoinTypeEnabled = false;
                    model.isResourceReportNode = false;
                    model.isParentNeedNotExistEnabled = true;
                    break;
                case "core:derivedTypeReportNode":
                    model.selectedNodeExactType = queryEntity.getExactType();
                    model.selectedNodeResourceMustExist = queryEntity.getTargetMustExist();

                    model.isExactTypeEnabled = false;
                    model.isResourceMustExistEnabled = false;
                    model.isResourceReportNode = false;
                    model.isParentNeedNotExistEnabled = true;
                    break;
                default:
                    model.isCheckExistenceOnlyEnabled = true;
                    model.isExactTypeEnabled = true;
                    model.isRecursionModeEnabled = true;
                    model.isResourceMustExistEnabled = true;
                    model.isJoinTypeEnabled = true;
                    model.isResourceReportNode = false;
                    model.isParentNeedNotExistEnabled = true;
                    break;
            }

            $scope.isRecursionModeEnabled = !$scope.selectedNodeCheckExistenceOnly;
        }

        function updateQueryEntity(queryEntity) {
            var typeAlias = queryEntity.getEntity().type.nsAlias;

            switch (typeAlias) {
                case "core:resourceReportNode":
                    $scope.selectedNode.qe.getEntity().setExactType(model.selectedNodeExactType);

                    break;
                case "core:customJoinReportNode":
                case "core:relationshipReportNode":
                    if (model.selectedNodeExistMode === 'recordMayNotExist') {
                        $scope.selectedNode.qe.getEntity().targetNeedNotExist = true;
                        $scope.selectedNode.qe.getEntity().targetMustExist = false;
                        model.selectedNodeResourceMustExist = false;
                    } else if (model.selectedNodeExistMode === 'recordMustExist') {
                        $scope.selectedNode.qe.getEntity().targetNeedNotExist = false;
                        $scope.selectedNode.qe.getEntity().targetMustExist = true;
                        model.selectedNodeResourceMustExist = true;
                    }
                    else if (model.selectedNodeExistMode === 'automatic') {
                        $scope.selectedNode.qe.getEntity().targetNeedNotExist = false;
                        $scope.selectedNode.qe.getEntity().targetMustExist = false;
                        model.selectedNodeResourceMustExist = false;
                    }
                    $scope.selectedNode.qe.getEntity().parentNeedNotExist = model.selectedNodeParentNeedNotExist;

                    if (typeAlias === "core:relationshipReportNode") {
                        $scope.selectedNode.qe.getEntity().setFollowRecursive(model.selectedNodeCheckExistenceOnly);
                        if (model.selectedNodeRecursionMode === 'RecursiveWithSelf') {
                            $scope.selectedNode.qe.getEntity().includeSelfInRecursive = true;
                        } else {
                            $scope.selectedNode.qe.getEntity().includeSelfInRecursive = false;
                        }
                    } else if (typeAlias === "core:customJoinReportNode") {
                        $scope.selectedNode.qe.getEntity().joinPredicateCalculation = model.customJoinScript;
                    }
                    break;
                case "core:derivedTypeReportNode":
                    //queryEntity.match = $scope.selectedNodeExactType;
                    //queryEntity.mustexist = $scope.selectedNodeMustExist;
                    $scope.selectedNode.qe.getEntity().setExactType(model.selectedNodeExactType);
                    $scope.selectedNode.qe.getEntity().setTargetMustExist(model.selectedNodeResourceMustExist);
                    break;
                default:

                    break;
            }

            return queryEntity;
        }

        $scope.$watch('model.selectedNodeExactType', function () {
            if (model.selectedNodeExactType) {


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