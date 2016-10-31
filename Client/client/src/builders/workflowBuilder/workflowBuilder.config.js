// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, sp, spWorkflowConfiguration */

(function () {
    'use strict';

    angular.module('sp.workflow.builder')
        .config(function ($stateProvider) {

            var stateData = {
                showBreadcrumb: false,
                hideAppTabs: true,
                hideAppToolboxButton: true,
                leftPanelTemplate: 'workflowBuilder/workflowBuilderToolbox.tpl.html',
                leftPanelTitle: 'Workflow Builder'
            };

            $stateProvider.state('workflowNew', {
                url: '/{tenant}/{eid}/workflow/new?path',
                templateUrl: 'workflowBuilder/workflowBuilder.tpl.html',
                controller: 'workflowBuilderController',
                data: stateData
            });
            $stateProvider.state('workflowEdit', {
                url: '/{tenant}/{eid}/workflow?path&name',
                templateUrl: 'workflowBuilder/workflowBuilder.tpl.html',
                controller: 'workflowBuilderController',
                data: stateData
            });
        });
     }());