// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

(function () {
    'use strict';

    angular.module('sp.workflow.listController', ['ui.router', 'ngGrid', 'mod.common.spEntityService', 'spApps.reportServices', 'mod.services.workflow', 'mod.services.workflowConfiguration'])
        .config(function ($stateProvider) {
            $stateProvider.state('workflowList', {
                url: '/{tenant}/{eid}/workflow/list?path',
                templateUrl: 'workflow/workflowList/workflowList.tpl.html'
            });

            // register a nav item for this page (most fields are defaulted, but you can provide your own viewType, iconUrl, href and order)
            //window.testNavItems = window.testNavItems || {};
            //window.testNavItems.workflowList = { name: 'Workflow List' };
        })
        .controller('WorkflowListController', function ($scope, $location, titleService, spEntityService) {
            // this controller handles the data model being viewed with data exposed
            // suitable for binding and functions for commands

            function loadWorkflowType() {
                spEntityService.getEntity('workflow', 'id', { batch: true }).then(function (entity) {
                    $scope.workflowTypeId = entity.id();
                }, function (err) {
                    console.error('Failed to load Workflow type: ', err);
                    $scope.xhrError = 'An error occurred requesting workflow data.';
                });
            }

            function loadWorkflowList() {
                spEntityService.getInstancesOfType({ ns: 'core', alias: 'workflow' }, null, true).then(function (list) {
                    $scope.workflows = _.sortBy(list, 'name');
                }, function (err) {
                    console.error('Failed to load Workflows: ', err);
                    $scope.xhrError = 'An error occurred requesting workflow data.';
                });
            }

            function loadReportsList() {
                spEntityService.getInstancesOfType({ ns: 'core', alias: 'report' }).then(function (list) {
                    $scope.reportsList = list;
                }, function (err) {
                    console.error('Failed to load reports: ', err);
                    $scope.xhrError = 'An error occurred requesting the reports list.';
                });
            }

            function runWorkflow(id) {
                console.log('running', id, $scope.appData.tenant);
                $location.path('/' + $scope.appData.tenant + '/' + id + '/workflow/run');
            }

            function deleteWorkflow(id) {
                console.log('deleting', id);
                spEntityService.deleteEntity(id).then(function () {
                    loadWorkflowList();
                });
            }

            $scope.workflows = [
                { id: 0, name: 'loading...' }
            ];

            $scope.run = runWorkflow;
            $scope["delete"] = deleteWorkflow;

            loadWorkflowType();
            loadWorkflowList();
            loadReportsList();

            titleService.setTitle('Workflow List');
        });
}());
