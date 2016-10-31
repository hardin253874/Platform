// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.directives.propertiesForm', [])
        .directive('spWorkflowPropertiesForm', function (spViewRegionService) {

            return {
                restrict: 'E',
                replace: true,
                template: '<div ng-include="templateUrl"></div>',
                scope: {
                    // workflow is the model object managed by the spWorkflowService
                    workflow: '='
                },
                controller: function ($scope, $timeout) {
                    //console.log('spWorkflowPropertiesForm: controller ctor', $scope);

                    $scope.selectedEntityUpdated = function () {
                        console.warn('not implemented: selectedEntityUpdated');
                    };
                    $scope.restoreArgumentTextForValues = function () {
                        console.warn('not implemented: restoreArgumentTextForValues');
                    };
                    $scope.cacheArgumentTextForValues = function () {
                        console.warn('not implemented: cacheArgumentTextForValues');
                    };
                    $scope.restoreArgumentConformsToTypeForValues = function () {
                        console.warn('not implemented: restoreArgumentConformsToTypeForValues');
                    };
                    $scope.cacheArgumentConformsToTypeForValues = function () {
                        console.warn('not implemented: cacheArgumentConformsToTypeForValues');
                    };

                    $scope.notifyUiRefresh = function () {
                        $scope.$broadcast('sp.app.ui-refresh');
                    };

                    $scope.stringToBoolean = sp.stringToBoolean;

//                    $scope.$watch('workflow', function (workflow) {
//                        console.log('spWorkflowPropertiesForm: watch workflow', workflow);
//                    });
//
//                    $scope.$watch('workflow.updateCount', function (value) {
//                        console.log('spWorkflowPropertiesForm: watch updateCount', value);
//                    });
//
//                    $scope.$watch('workflow.processState.count', function (value) {
//                        console.log('spWorkflowPropertiesForm: watch process count', value);
//                    });

                    $scope.$watch('workflow.selectedEntity', function (entity) {
                        //console.log('spWorkflowPropertiesForm: watch selectedEntity', entity);

                        // clear the following first then set after a digest cycle so we ensure
                        // fresh template and controller for a change in the selected entity,
                        // even if its the same template - same entity type
                        $scope.entity = null;
                        $scope.templateUrl = null;

                        if (entity) {
                            $timeout(function () {
                                $scope.templateUrl = entity ?
                                    'workflow/typeTemplates/' + (entity.getType().getNsAlias().replace(/core:/, '').replace(':', '_')) + '.tpl.html' :
                                    '';
                                //console.log('spWorkflowPropertiesForm: watch selectedEntity => template', $scope.templateUrl, ', entity=', entity);
                                $scope.entity = entity;
                            }, 0);
                        }

                        spViewRegionService.clearViews('workflow-properties-sidepanel');
                    });

                    // expose the workflow model and the subject entity for use by child directives
                    this.getWorkflow = function () {
                        return $scope.workflow;
                    };
                    this.getEntity = function () {
                        return $scope.entity;
                    };
                }
            };
        });
}());