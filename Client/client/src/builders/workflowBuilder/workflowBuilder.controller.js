// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, sp, spWorkflowConfiguration */

(function () {
    'use strict';

    angular.module('sp.workflow.builder')
        .controller('workflowBuilderController', function ($scope, $q, $location, titleService, spState, spReportService, spNavService, spAlertsService, spWorkflowService, spEditFormDialog) {

            var pausedRunsMessage = 'This workflow has runs in progress. When saved, a linked copy of this workflow will be created to allow those runs to continue.';
            var supersededMessage = 'This is an old version of the workflow, which cannot be modified.';

            // ensure that workflow cache is cleaned when we reopen workflow builder, until we have a more sophisticated invalidation mechanism
            spWorkflowService.resetCache();

            // ensure the id is either a numeric id or a string alias
            var workflowId = spState.name === 'workflowNew' ? 0 : sp.coerseToNumberOrLeaveAlone(spState.params.eid);

            function notifyLayoutChange() {
                $scope.$emit('app.layout');
            }

            function setBusy() {
                $scope.busyIndicator.isBusy = true;
            }

            function clearBusy() {
                $scope.busyIndicator.isBusy = false;
            }

            function addAlert(text, expires) {
                spAlertsService.addAlert(text, {expires: expires});
            }

            function updateZoom(zoom) {
                $scope.zoom = zoom;
                spWorkflow.mergeExtendedProperties($scope.workflow.entity, { zoom: zoom });
            }

            function zoomIn() {
                updateZoom($scope.zoom + 0.1);
            }

            function zoomOut() {
                updateZoom(Math.max(0.1, $scope.zoom - 0.1));
            }

            function zoomReset() {
                $scope.zoom = 1;
            }

            function workflowOpened(workflow) {
                $scope.navItem.data.workflow = workflow;
                $scope.workflow = workflow;
                $scope.workflow.selectedEntity = workflow.entity;
                $scope.zoom = spWorkflow.getExtendedProperties($scope.workflow.entity).zoom || 1.0;
                $scope.hasNewerVersion = !!workflow.entity.wfNewerVersion;

                if ($scope.hasNewerVersion)
                    addAlert(supersededMessage, 5);
                else 
                    checkPausedRuns(workflow.entity);
            }

            function save() {
                addAlert('Saving workflow', 3);
                setBusy();
                spWorkflowService.saveWorkflow($scope.workflow).then(function (workflow) {
                    clearBusy();
                    addAlert('Saved workflow - id ' + workflow.entity.idP, 3);
                    if (spState.name !== 'workflowEdit') {
                        spNavService.navigateToSibling('workflowEdit', workflow.entity.idP);
                    } else {
                        spWorkflowService.openWorkflow(workflow.entity.idP).then(workflowOpened);
                    }
                }).catch(function (err) {
                    clearBusy();
                    var additionalInfo = sp.result(err, 'data.Message');
                    spAlertsService.addAlert('Saving workflow failed: ' + additionalInfo, 'error');
                });
            }

            function saveAs() {
                spWorkflowService.saveAsWorkflow($scope.workflow).then(function (result) {
                    if (!result) {
                        return;
                    }

                    var newWorkFlowId = result.entityId;                    
                    addAlert('Saved new workflow - id ' + newWorkFlowId, 3);
                        
                    spNavService.navigateToSibling('workflowEdit', newWorkFlowId);                    
                });
            }

            function deleteSelected() {
                spWorkflowService.deleteSelected($scope.workflow);
            }

            function gotoListView() {
                $location.path('/' + $scope.appData.tenant + '/9999999/workflow/list');
            }

            function canRun() {
                return workflowId && !$scope.navItem.isDirty();
            }

            function gotoRunView() {
                if (canRun()) {
                    spNavService.navigateToState('workflowRun', spState.params);
                }
            }

            function canSave() {
                return sp.result($scope, 'workflow.entity.canModify') || sp.result($scope, 'workflow.entity.dataState') === spEntity.DataStateEnum.Create;
            }

            function showDebugDiagram() {
                $scope.showDebugDiagram = !$scope.showDebugDiagram;
                if ($scope.showDebugDiagram) {
                    $scope.showDebugEntity = false;
                }
            }

            function showDebugEntity() {
                $scope.showDebugEntity = !$scope.showDebugEntity;
                if ($scope.showDebugEntity) {
                    $scope.showDebugDiagram = false;
                }
            }

            $scope.selectedEntityUpdated = function () {
                console.warn('workflowController.selectedEntityUpdated: ', arguments);
                console.trace();
                spWorkflowService.activityUpdated($scope.workflow, $scope.workflow.selectedEntity);
            };

            $scope.setSelectedEntity = function () {
                console.warn('setSelectedEntity: not implemented');
            };

            $scope.setSelectedItems = function () {
                console.warn('setSelectedItems: not implemented');
            };

            $scope.doProcess = function () {
                spWorkflowService.processWorkflow($scope.workflow);
            };

            var propsHeights = [250, 500, 0];
            var propsHeightIndex = 0;
            $scope.toggleShowProperties = function () {
                propsHeightIndex += 1;
                if (propsHeightIndex >= propsHeights.length) propsHeightIndex = 0;
                $scope.propsViewHeight = propsHeights[propsHeightIndex];
                $scope.showPropertiesPane = $scope.propsViewHeight > 0;
                notifyLayoutChange();
            };

            $scope.showPropertiesDialog = function () {
                var options = {
                    title: 'Workflow Properties',
                    entity: $scope.workflow.entity,
                    form: 'core:resourceWithApplicationForm',
                    optionsEnabled: false,
                    saveEntity: false
                };

                spEditFormDialog.showDialog(options);
            };

            $scope.$on('toolbar.clicked', function (event, id) {
                ($scope.toolActions[id] ||
                    _.bind(console.log, console, 'workflowController.on toolbarclicked "%s" not yet implemented', id))();
            });

            $scope.$watch('workflow.selectedEntity', function (entity) {
                //console.log('workflowController: watch selectedEntity', entity);

                // clicking on new elements in the diagram fails to cancel edit mode of the workflow title... so force it
                $scope.nameEditMode = false;

                // showing this in the props view....
                $scope.selectedEntityTypeName = entity && _.result(_.find($scope.workflow.activityTypes, _.partial(spWorkflow.hasEid, entity.getType())), 'getName') || '';

                $scope.debugEntity = entity;
            });

            $scope.$watch('workflowName', function (name) {
                var workflow = $scope.workflow;
                if (name && workflow && workflow.entity && workflow.entity.name !== name) {
                    workflow.entity.name = name;
                    spWorkflowService.workflowUpdated(workflow);
                }
            });

            $scope.$watch('workflow.entity.name', function (name) {
                if (name) {
                    $scope.workflowName = name;
                }
            });

            $scope.$watch('workflow.updateCount', function () {
                // This is the main place to watch for updates to the workflow model and to
                // kick off the workflow process operation.
                //TODO - consider doing this in the service, but we'd have to have the controller somehow tell it to start or
                // stop anyway... we don't want it happening when nothing is currently viewing the workflow.
                if ($scope.workflow) {
                    spWorkflowService.processWorkflow($scope.workflow);
                }
            });

            $scope.$watch('workflow.validationMessages', function (messages) {
                if ($scope.workflow) {
                    $scope.workflow.validationMessageStrings = _.map(messages, 'message');
                }
            });

            function undo() {
                return spWorkflow.undo($scope.workflow);
            }

            function redo() {
                return spWorkflow.redo($scope.workflow);
            }

            function close() {
                spNavService.navigateToParent();
            }

            function checkPausedRuns(workflow) {
                return spWorkflowService.getPausedRuns(workflow.idP).then(function (pausedRuns) {
                    if (pausedRuns.length > 0) {
                        addAlert(pausedRunsMessage, 10);
                    }
                });
            }

            $scope.toolActions = {
                'close': close,
                'undo': undo,
                'redo': redo,
                'save': save,
                'saveas': saveAs,
                'delete': deleteSelected,
                'list': gotoListView,
                'run': gotoRunView,
                'zoomin': zoomIn,
                'zoomout': zoomOut,
                'zoomreset': zoomReset,
                'info': $scope.showPropertiesDialog,
                'debug': showDebugEntity
            };

            $scope.busyIndicator = { isBusy: true };
            $scope.zoom = 1.0;
            $scope.showPropertiesPane = true;
            $scope.nameEditMode = false;

            titleService.setTitle('Workflow');

            $scope.workflow = null;
            $scope.addMenu = null;
            $scope.canRun = canRun;
            $scope.canSave = canSave;

            $scope.navItem = spState.navItem || {};
            $scope.navItem.data = $scope.navItem.data || {};
            $scope.navItem.isDirty = function () {
                return sp.result($scope.navItem.data, 'workflow.updateCount') > 0;
            };

            var alreadyOpenWorkflow = sp.result($scope.navItem, 'data.workflow');

            if (alreadyOpenWorkflow) {
                console.log('workflowController: attaching to already open or new workflow data: ' + alreadyOpenWorkflow.entity.debugString);
            } else {
                console.log('workflowController:  ' + (workflowId ? 'opening workflow ' + workflowId : 'open new workflow'));
            }

            // this logic is a bit irky... to come back to
            $q.when(alreadyOpenWorkflow)
                .then(function (workflow) {
                    if (workflow) return workflow; // already open
                    if (workflowId) return spWorkflowService.openWorkflow(workflowId);
                    
                    return spWorkflowService.newWorkflow();
                })
                .then(function (workflow) {
                    if (workflow) workflowOpened(workflow);

                })
                .catch(function (result) {
                    console.log('workflowController: error opening workflow: %o', result);
                    addAlert((workflowId ? 'Error opening workflow ' + workflowId : 'Error creating new workflow') + ', error: ' + result.toString());
                })
                .finally(function () {
                    $scope.$emit('app.layout');
                    clearBusy();
                });
        });
}());