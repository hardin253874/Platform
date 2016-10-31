// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // The spApplicableTasks directive displays any tasks that are applicable to the selected record for the current user
    // It allows rhe user to action the tasks.
    /////
    angular.module('app.controls.spApplicableTasks', ['mod.common.spUserTask', 'sp.common.filters', 'mod.common.spMobile', 'mod.common.spCachingCompile', 'mod.common.alerts'])
        .directive('spApplicableTasks', function($rootScope, spUserTask, spEntityService, spNavService, spPromiseService, spMobileContext, spCachingCompile) {

            var templateFile = spMobileContext.isMobile ? 'spApplicableTasksMobile.tpl.html' : 'spApplicableTasks.tpl.html';
            
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    taskId: '=?',
                    disabled: '=?',
                    taskList: '=',         // The list of tasks obtained fron spUserTask
                    actionButtons: '=',
                    inWizard: '=',
                    navigateAsChild: '=?',  // If hte user hits a button do a navigate to sibling rather than to child. This allows wizard like navigating.
                    returnOnCompletion: '=?',
                    beforeTransition: '&'   // a function that returns promise to run. If successful we will transition.
                },
                link: link
            };

            function link(scope, element, attrs) {
                scope.notificationCount = 0;

                function reportFetchTaskError(error) {
                    console.error('LoadTask failed: ', error);
                }

                function reportFetchTaskListError(error) {
                    console.error('LoadTaskList failed: ', error);
                }

                function saveTaskList(tasks) {

                    scope.taskList = tasks;
                    return tasks;
                }

                function selectTask() {
                    var found;

                    var tasks = _.filter(scope.taskList, function(t) {
                        return t.userTaskIsComplete !== true;
                    });

                    if (!tasks || tasks.length === 0) {
                        found = null;
                    } else {
                        if (!scope.taskId) {
                            found = tasks[0];
                            scope.taskId = found.idP;
                        } else {

                            found = _.find(tasks, {'idP': scope.taskId});
                        }
                    }

                    scope.task = found;
                    scope.notificationCount = tasks.length;

                    return found;
                }

                function handleFailedTaskFetch(error) {
                    if (error === 404) {
                        // this is expected if back is hit on a task that is marked to be deleted
                        console.log('spApplicableTasks: Unable to find task, ignoring.');
                    } else {
                        console.log('Error fetching task: ', error);
                    }
                }

                function createListFromTaskId(id) {
                    return spUserTask.getTask(id)
                        .then(function (task) {
                            if (scope.taskList)
                                scope.taskList.length = 0;  // preserve the existing object in case someone else is looking at it.
                            else
                                scope.taskList = [];

                            scope.taskList[0] = task;
                        })
                        .catch(handleFailedTaskFetch);
                }

                function refreshList() {
                    selectTask();
                }

                scope.$watchCollection('taskList', refreshList);
                scope.$watch('taskId', refreshList);


                scope.$watch('task', function (newTask, oldTask) {
                    // check if the task record has been refreshed and push it into the array
                    if (newTask && oldTask && newTask.idP === oldTask.idP) {
                        var index = _.findIndex(scope.taskList, oldTask);
                        scope.taskList[index] =  newTask;
                    }
                });

                var cachedLinkFunc = spCachingCompile.compile('controls/spApplicableTasks/' + templateFile);
                cachedLinkFunc(scope, function (clone) {
                    element.append(clone);
                });
            }

        });


}());