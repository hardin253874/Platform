// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.builder')
        .controller('sp.workflow.activityController', function ($scope, spWorkflowEditorViewService) {

            var defaultContext = {
                workflow: $scope.workflow,
                activity: $scope.entity
            };

            var defaultActions = [
                {
                    name: 'exprEditor',
                    action: function (parameter, action) {
                        return spWorkflowEditorViewService.openExprEditor(_.defaults({parameter: parameter}, defaultContext));
                    }
                },
                {
                    name: 'typeChooser',
                    action: function (parameter, action) {
                        return spWorkflowEditorViewService.openSingleKnownEntityChooser(_.defaults({chooserType: 'typeChooser', parameter: parameter}, defaultContext));
                    }
                },
                {
                    name: 'resourceChooser',
                    action: function (parameter, action) {
                        return spWorkflowEditorViewService.openSingleKnownEntityChooser(_.defaults({chooserType: 'resourceChooser', parameter: parameter}, defaultContext));
                    }
                },
                {
                    name: 'parameterChooser',
                    action: function (parameter, action) {
                        return spWorkflowEditorViewService.openSingleParameterChooser(_.defaults({parameter: parameter}, defaultContext));
                    }
                },
                {
                    name: 'fieldChooser',
                    action: function (parameter, action) {
                        return spWorkflowEditorViewService.openSingleKnownEntityChooser(_.defaults({chooserType: 'fieldChooser', parameter: parameter}, defaultContext));
                    }
                },
                {
                    name: 'relChooser',
                    action: function (parameter, action) {
                        return spWorkflowEditorViewService.openSingleKnownEntityChooser(_.defaults({chooserType: 'relChooser', parameter: parameter}, defaultContext));
                    }
                }
            ];

            function workflowUpdated() {

                console.time('activityController.workflowUpdated');

                if ($scope.workflow.activities && $scope.workflow.activities[$scope.entity.idP]) {

                    $scope.activityParameters = $scope.workflow.activities[$scope.entity.idP].parameters;

                    // maybe move this to the service...
                    _.forEach($scope.activityParameters, function (p) {
                        p.actions = _.keyBy(defaultActions, 'name');
                    });
                }

                console.timeEnd('activityController.workflowUpdated');
            }

            $scope.$watch('workflow.processState.count', workflowUpdated);
        });
}());
