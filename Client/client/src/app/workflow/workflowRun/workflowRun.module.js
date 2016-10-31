// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spWorkflow, spWorkflowConfiguration, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.runController', ['ui.router', 'mod.common.spEntityService', 'spApps.reportServices', 'mod.services.workflow', 'mod.services.workflowConfiguration', 'mod.common.spUserTask', 'mod.services.promiseService'])
        .config(function ($stateProvider) {
            $stateProvider.state('workflowRun', {
                url: '/{tenant}/{eid}/workflow/run?path',
                templateUrl: 'workflow/workflowRun/workflowRun.tpl.html',
                controller: 'workflowRunController'
            });
        });
}());
