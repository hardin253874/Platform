// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, sp, spWorkflowConfiguration */

(function () {
    'use strict';

    angular.module('sp.workflow.builder')
        .controller('workflowBuilderToolboxController', function ($scope, $q, spWorkflowService, spState) {

            $scope.dragOptions = {
                dragImage: {
                    url: 'assets/images/activities/menu/createActivity.png',
                    width: 100,
                    height: 100
                },
                onDragEnd: function (event, data) {
                    //                    console.log('WF DRAG END');
                },
                onDragStart: function (event, data) {
                    //                    console.log('WF DRAG START');
                }
            };

            function addActivity(type, opts) {

                var workflow = $scope.workflow;

                return spWorkflowService.createActivity(workflow, type).then(function (activity) {

                    if (!opts) {
                        spWorkflowService.addActivitiesInSequence(workflow, [activity]);

                    } else {
                        var targetId = opts.elementId,
                            droppedOn = spWorkflow.findWorkflowComponentEntity(workflow, targetId);

                        if (droppedOn && droppedOn.type.nsAlias !== 'core:exitPoint') {
                            // add the new activity after the target
                            workflow.selectedEntity = droppedOn;
                            workflow.selectedExit = spWorkflow.findWorkflowComponentEntity(workflow, opts.portId);
                            spWorkflowService.addActivitiesInSequence($scope.workflow, [activity]);
                        } else {
                            // add new activity without connecting it
                            if (opts.x) spWorkflow.mergeExtendedProperties(activity, { x: opts.x, y: opts.y });
                            spWorkflowService.addActivities(workflow, [activity]);
                        }
                    }

                    workflow.selectedEntity = activity;
                    workflow.selectedExit = null;

                    return activity;
                });
            }

            function addToolboxItem(type, opts) {
                if (type === 'sequence') {
                    spWorkflowService.addSequence($scope.workflow, $scope.workflow.selectedEntity, null, null);
                } else if (type === 'endEvent') {
                    $q.when(spWorkflowService.addEndEvent($scope.workflow)).then(function (e) {
                        if (e && opts && opts.x) {
                            var extProps = { endEvents: {} };
                            extProps.endEvents[e.idP] = { x: opts.x, y: opts.y };
                            spWorkflow.mergeExtendedProperties($scope.workflow.entity, extProps);
                        }
                    });
                } else if (type === 'swimlane') {
                    spWorkflowService.addSwimlane($scope.workflow);
                } else if (type) {
                    return addActivity(type, opts);
                }
            }

            $scope.getDragOptions = function (menu) {
                return _.extend({}, $scope.dragOptions, { dragImage: { url: menu.menuImage, width: 100, height: 100 } });
            };

            $scope.menuItemClicked = function (item) {
                addToolboxItem(sp.result(item, 'id'));
            };

            $scope.$on('wb.droppedOnCanvas', function (e, message) {
                addToolboxItem(sp.result(message, 'dragData.id'), message);
            });

            $scope.$watch('spState.navItem.data.workflow', function (workflow) {
                $scope.workflow = workflow;

                if (workflow) {
                    //                    console.log('workflowBuilderToolboxController: attaching to already open or new workflow data: ' + workflow.entity.debugString);
                    $scope.addMenu = spWorkflowService.getWorkflowMenu(workflow);
                }
            });

            spState.registerAction('addToolboxItem', function (opts) {
                return $q.when(addToolboxItem(opts.typeId, opts));
            });

            // We expect the current navItem to include the workflow we are operating on so
            // capture the service so we can watch for changes.
            $scope.spState = spState;

        });
}());