// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, $, angular, sp, spEntity */

(function () {
    'use strict';

    /**
     * Module providing user task UI functionality
     *
     * @module spUserTaskUi
     * @example

     Usage


     */
    angular.module('mod.common.spUserTask', ['mod.common.spEntityService', 'spApps.reportServices', 'sp.navService',
        'mod.services.workflow', 'mod.services.promiseService', 'sp.common.loginService']);

    angular.module('mod.common.spUserTask')
        .factory('spUserTask', spUserTask);

    function spUserTask(spEntityService, spReportService, spNavService, spWorkflowRunService, spLoginService) {

        var taskQuery = "name, taskComment, hideComment, openInEditMode, userTaskIsComplete, taskStatus.id," +
            "userTaskCompletedOn, availableTransitions.fromExitPoint.{name, description, exitPointOrdinal}, " +
            "userResponse.id, waitForNextTask, workflowRunForTask.id, recordToPresent.name";

        var pendingTasksQuery = "taskWithinWorkflowRun.{isOfType.alias, assignedToUser.id, recordToPresent.id, formToUse.id, userTaskIsComplete, openInEditMode}";

        var pendingTasksForRecordQuery = "tasksForRecord.{assignedToUser.id, " + taskQuery + "}";

        var exports = {
            getPendingTaskInfo: getPendingTaskInfo,
            getPendingTasksForRecord: getPendingTasksForRecord,
            navigateToFollowOnTasks: navigateToFollowOnTasks,
            waitToNavigateToFollowOnTasks: waitToNavigateToFollowOnTasks,
            completeTask: completeTask,
            getTask: getTask
        };

        return exports;

        

        /**
         * Look for a pending {task | record | form id} for the given workflow for the current user
         *
         * @param workflow or workflowRunId The workflow run to check
         * @return an object containing {taskId | recordId | formId}
         */
        function getPendingTaskInfo(workflowRun) {

            var workflowRunId = spEntity.isEntity(workflowRun) ? workflowRun.id() : workflowRun;

            return spEntityService.getEntity(workflowRunId, pendingTasksQuery, { hint: 'spUserTask.getPendingTaskInfo', batch: true })
            .then(function (wfRun) {
                return _.find(wfRun.taskWithinWorkflowRun, function (t) {
                    return !t.userTaskIsComplete;
                });
            });
        }

        /**
         * Look for a pending tasks for the given record for the current user
         *
         * @param record, record or recordId to check
         * @param username, option, the user to check. If unset the logged on user is used.
         * @return a promise for an array of tasks
         */
        function getPendingTasksForRecord(record) {

            var recordId = spEntity.isEntity(record) ? record.id() : record;
            
            var filterCondition = "[Task status] <> 'Completed'";

            return spEntityService.getEntity(recordId, pendingTasksForRecordQuery, { hint: 'spUserTask.getPendingTasksForRecord', batch: true, filter: filterCondition })
           .then(function (record) {
               return _.sortBy(record.tasksForRecord, 'name');
           });
        }

        /**
         * If the provided WorkflowRun has follow-up tasks for the current user, navigate to them as a child.
         * @param workflowRunId The workflow run to check
         * @param navAsChild if true navigate to a child
         * @return a promise to enact the navigation if required.
         */
        function navigateToFollowOnTasks(workflowRun, navAsChild, returnOnCompletion) {
            var workflowRunId = spEntity.isEntity(workflowRun) ? workflowRun.id() : workflowRun;

            // check for further forms
            return getPendingTaskInfo(workflowRunId).then(function (taskInfo) {
                var params, resourceId, state, taskType, navigateChild;

                if (taskInfo) {
                    params = { inWizard: 'true' };
                    resourceId = taskInfo.idP;
                    taskType = sp.result(taskInfo, 'type.nsAlias');
                    navigateChild = navAsChild;

                    params.taskId = taskInfo.idP;
                    params.returnOnCompletion = returnOnCompletion;

                    // todo: this is beginning to look like it should be more configurable
                    if (taskType !== 'core:userSurveyTask') {
                        if (taskType === 'core:promptUserTask') {
                            state = 'workflowInput';
                        } else {
                            if (taskInfo.recordToPresent)
                                resourceId = taskInfo.recordToPresent.idP;

                            state = taskInfo.openInEditMode ? 'editForm' : 'viewForm'; // the value is a string

                            if (taskInfo.formToUse)
                                params.formId = taskInfo.formToUse.idP;
                        }

                        if (navigateChild) {
                            spNavService.navigateToChildState(state, resourceId, params);
                        } else {
                            spNavService.navigateToSibling(state, resourceId, params);
                        }
                    }
                } else {
                    if (returnOnCompletion) {
                        spNavService.navigateToParent();
                    }
                }

                return taskInfo;        // not really necessary
            });
        }

        /**
         * Wait for the provided workflowRun to stop and if it has follow-up tasks for the current user, navigate to them as a child.
         * @param workflowRunId The workflow run to check
         * @param inWizard Being in a wizard affects the navigation behavior.
         * @param options spWorkflowRunService.waitForRunToStop. Also requestParentRefresh
         * @return a promise to enact the navigation if required which returns true if the navigate happened.
         */
        function waitToNavigateToFollowOnTasks(workflowRun, inWizard, returnOnCompletion, options) {

            var workflowRunId = spEntity.isEntity(workflowRun) ? workflowRun.idP : workflowRun;

            return spWorkflowRunService.waitForRunToStopWithThrow(workflowRunId, options)
                .then(spWorkflowRunService.getWorkflowRunResults)
                .then(function (result) {

                var navAsChild = !inWizard;     // if we a are not in a wizard yet, we need to do a child navigate. If we are in the wizard we don;t have to
                if (result.workflowRunStatus.name === 'Paused') {
                    return navigateToFollowOnTasks(workflowRunId, navAsChild, returnOnCompletion);
                } else if (result.workflowRunStatus.name === 'Completed' && returnOnCompletion && inWizard) {
                    return spNavService.navigateToParent(options.requestParentRefresh);
                } else if (result.workflowRunStatus.name === 'Failed') {
                    return spWorkflowRunService.getWorkflowRunResults(workflowRunId).then(function (results) {
                        var message = results.errorLogEntry.name;
                        console.log('workflow failed: ', message, results.errorLogEntry.description);
                        throw new Error(message);
                    });
                }

                return false;   // we didn't navigate
            });
        }

        /**
         * Complete a task using the given exit, optionally providing a comment
         * @param task The task to be completed.
         * @param transition The exit being used for completion. Can be either the name of the transition or the transition object.
         * @return a promise for the task
         */
        function completeTask(task, transition) {
            return getTask(task.idP)                    // NOTE: We are fetching a fresh copy of task to deal with a workflow cloning after the initial fetch
                .then(function (latestTask) {           // This would be better handled by not relying on the transition object in the task and using a string.
                    if (latestTask.availableTransitions) {
                        var trans;

                        if (_.isString(transition)) {
                            trans = _.find(latestTask.availableTransitions, function (t) {
                                return t.fromExitPoint && t.fromExitPoint.getName() === transition;
                            });
                        } else {
                            trans = transition;
                        }

                        if (!trans) {
                            throw new Error('Unable to find matching transition');
                        }

                        task.userResponse = trans;
                    }

                    task.userTaskIsComplete = true;
                    task.taskStatus = spEntity.fromJSON({ id: 'core:taskStatusCompleted' });
                    return task;
                })
                .then(spEntityService.putEntity);
        }

        /**
         * Get the task along with possible exits, selected exit and completion information task
         * @param taskId The id of the task
         * @return a promise for the task
         */
        function getTask(taskId) {
            return spEntityService.getEntity(taskId, taskQuery, { hint: 'getTask', batch: true });
        }
    }
}());