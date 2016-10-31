// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, spWorkflowConfiguration, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.activities')
        .controller('notifyActivityController', function ($scope, spWorkflowService, spWorkflowEditorViewService) {

            $scope.selectedEntry = null;

            $scope.open = function (actionName, parameter) {

                spWorkflowEditorViewService.chooseResource('core:workflow', $scope.entity.workflowToProxy).then(function (resource) {
                    $scope.entity.nReplyMap[parameter].rmeWorkflow = resource.entity;
                    spWorkflowService.activityUpdated($scope.workflow, $scope.entity, 'rmeWorkflow');
                });

            };

            $scope.$watch('activityParameters', function () {

                $scope.activityParameters['core:inPeople'].resourceType = 'core:person';
                $scope.activityParameters['core:inLinkToRecord'].resourceType = 'core:userResource';

                var timeoutValueString = $scope.activityParameters['core:inNotifyTimeOut'].expression.expressionString || '1';
                $scope.notifyTimeoutValue = parseFloat(timeoutValueString, 10);

                var acceptRepliesForString = $scope.activityParameters['core:inAcceptRepliesFor'].expression.expressionString || '1';
                $scope.acceptRepliesForValue = parseFloat(acceptRepliesForString, 10);

                var waitForReplies = $scope.activityParameters['core:inWaitForReplies'].expression.expressionString === "true";
                $scope.waitForReplies = waitForReplies;
            });

            $scope.$watch('waitForReplies', function (value, oldValue) {
                if (value !== oldValue) {
                    $scope.activityParameters['core:inWaitForReplies'].expression.expressionString = value ? 'true' : 'false';
                    updateTimeout(value, $scope.notifyTimeoutValue);
                }
            });

            $scope.$watch('notifyTimeoutValue', function (value, oldValue) {
                if (value !== oldValue) {
                    if (value <= 0) {
                        $scope.notifyTimeoutValue = oldValue;
                    } else {
                        updateTimeout($scope.waitForReplies, value);
                    }
                }

            });

            $scope.$watch('acceptRepliesForValue', function (value, oldValue) {
                if (value !== oldValue) {
                    if (value <= 0) {
                        $scope.acceptRepliesForValue = oldValue;
                    } else {
                        updateAcceptFor($scope.acceptRepliesFor, value);
                    }
                }

            });

            $scope.addReplyWorkflow = addNewReplyWorkflow;
            $scope.deleteReplyWorkflow = deleteReplyWorkflow;
            


            function updateTimeout(waitForReplies, notifyTimeoutValue) {
                var value = null;

                    $scope.activityParameters['core:inNotifyTimeOut'].expression.expressionString = notifyTimeoutValue.toString();
            }

            function updateAcceptFor(acceptRepliesFor, acceptRepliesForValue) {
                var value = null;

                $scope.activityParameters['core:inAcceptRepliesFor'].expression.expressionString = acceptRepliesForValue.toString();
            }

            function addNewReplyWorkflow() {
                var replyMap = $scope.entity.nReplyMap;
                
                $scope.entity.nReplyMap.add(spEntity.fromJSON({
                    typeId: spWorkflowConfiguration.aliases.replyMapEntry,
                    name: '',
                    rmeOrder: replyMap.length,
                    rmeWorkflow: jsonLookup()
                }));

                spWorkflowService.activityUpdated($scope.workflow, $scope.entity);

            }

            function deleteReplyWorkflow(entry) {
                var replyMap = $scope.entity.nReplyMap;

                replyMap.remove(entry);

                // update ordinals
                _.each(replyMap, function (e, index) { e.rmeOrder = index; });

                spWorkflowService.activityUpdated($scope.workflow, $scope.entity);
            }



          
        });

}());
