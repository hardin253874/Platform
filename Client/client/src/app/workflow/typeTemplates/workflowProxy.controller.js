// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, _, console, spWorkflowConfiguration, spWorkflow */

(function () {
    'use strict';

    angular.module('sp.workflow.activities')
        .controller('workflowProxyController', function ($scope, spWorkflowService, spEntityService, spWorkflowEditorViewService) {

            $scope.open = function (actionName, parameter) {
                spWorkflowEditorViewService.chooseResource('core:workflow', $scope.entity.workflowToProxy).then(function (resource) {
                    var id = resource && spWorkflow.makeIdOrAlias(resource.id);
                    if (id && id !== _.result($scope.entity.workflowToProxy, 'id')) {
                        console.assert(resource.entity);
                        $scope.entity.workflowToProxy = resource.entity;
                        spWorkflowService.activityUpdated($scope.workflow, $scope.entity, 'workflowToProxy');
                    }
                });
            };

            $scope.workflowToProxyField = {
                name: 'Workflow',
                description: 'Workflow to run as a nested workflow'
            };
            spEntityService.getEntity('core:workflowToProxy', 'name,description').then(function (entity) {
                $scope.workflowToProxyField = entity;
            });

        });
}());
