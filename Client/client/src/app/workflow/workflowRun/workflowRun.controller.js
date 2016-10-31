// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spWorkflow, spWorkflowConfiguration, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.runController')
        .controller('workflowRunController', function ($scope, $q, $location, $state, $stateParams, spAlertsService,
                                                       spPromiseService, titleService, spEntityService, spReportService,
                                                       spWorkflowService, workflowEntityRequest, activityTypesRequest,
                                                       activityTypeConfig, spNavService, spUserTask) {

            // convenience aliases
            var aliases = spWorkflowConfiguration.aliases;

            // ensure the id is either a numeric id or a string alias
            var workflowId = sp.coerseToNumberOrLeaveAlone($stateParams.eid);

            function setBusy() {
                $scope.busyIndicator.isBusy = true;
            }

            function clearBusy() {
                $scope.busyIndicator.isBusy = false;
            }

            function addAlert(text, expires) {
                spAlertsService.addAlert(text, {expires: expires});
            }

            function workflowOpened(workflow) {
                $scope.navItem.data.workflow = workflow;
                $scope.workflow = workflow;
                $scope.workflow.selectedEntity = workflow.entity;

                return spWorkflowService.validateWorkflow($scope.workflow.entity.idP).then(function (messages) {
                    $scope.workflow.serverValidationMessages = messages;
                });
            }

            $scope.$on('toolbar.clicked', function (event, id) {
                //console.log('toolbar button clicked "%s"', id);

                switch (id) {
                    case 'edit':
                        spNavService.navigateToSibling('workflowEdit', $stateParams.eid);
                        break;
                    case 'close':
                        spNavService.navigateToParent();
                        break;
                }
            });

            titleService.setTitle('Run Workflow');

            $scope.busyIndicator = { isBusy: true };
            $scope.workflow = null;

            $scope.navItem = spNavService.getCurrentItem() || {};
            $scope.navItem.data = $scope.navItem.data || {};

            $scope.navItem.isDirty = function () {
                return sp.result($scope.navItem.data, 'workflow.updateCount') > 0;
            };

            if ($scope.navItem.data.workflow) {
                console.log('workflowRunController: attaching to existing workflow data: ' + $scope.navItem.data.workflow.entity.debugString);
            } else {
                console.log('workflowRunController:  ' + (workflowId ? 'opening workflow ' + workflowId : 'open new workflow'));
            }

            $q.when($scope.navItem.data.workflow || (workflowId && spWorkflowService.openWorkflow(workflowId)))
                .then(function (workflow) {

                    if (!workflow) {
                        throw 'Failed to load workflow';
                    }

                    workflowOpened(workflow);
                })
                .catch(function (result) {
                    console.log('workflowRunController: error opening workflow: %o', result);
                    addAlert('Error opening workflow ' + workflowId + ', error: ' + result.toString());
                })
                .finally(function () {
                    $scope.$emit('app.layout');
                    clearBusy();
                });
        });
}());
