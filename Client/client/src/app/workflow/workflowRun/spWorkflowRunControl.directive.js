// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spWorkflow, spWorkflowConfiguration, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.runController')
        .directive('spWorkflowRunControl', function () {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'workflow/workflowRun/workflowRunControl.tpl.html',
                controller: 'workflowRunControlController',
                scope: {
                    workflow: '='
                }
            };
        });
}());
