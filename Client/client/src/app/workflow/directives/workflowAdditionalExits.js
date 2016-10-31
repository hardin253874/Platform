// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.directives.workflowAdditionalExits', [])
        .directive('spWorkflowAdditionalExits', spWorkflowAdditionalExits);


    function spWorkflowAdditionalExits(spWorkflowService) {
            return {
                restrict: 'E',
                replace: true,
                scope: {
                    workflow: '=',      // The workflow object (not entity)
                    activity: '=',       // The activity entity
                    includeActionSummary: "="

                },
                templateUrl: 'workflow/directives/workflowAdditionalExits.tpl.html',
                link: function (scope, elem, attrs) {

                    scope.addExitPoint = addExitPoint;
                    scope.removeExitPoint = removeExitPoint;

                    return;

                    function addExitPoint() {
                        scope.activity.exitPoints.add(spEntity.fromJSON({
                            typeId: spWorkflowConfiguration.aliases.exitPoint,
                            name: 'to be defined',
                            description: '',
                            isDefaultExitPoint: false,
                            exitPointOrdinal: 1 + (Math.max.apply(null, [0].concat(_.compact(_.map(scope.activity.exitPoints, 'exitPointOrdinal')))))
                        }));
                        spWorkflowService.activityUpdated(scope.workflow, scope.activity);
                    }

                    function removeExitPoint(e) {
                        scope.activity.exitPoints.remove(e);
                        spWorkflowService.activityUpdated(scope.workflow, scope.activity);
                    }

                }
            };
    }



}());