// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use	strict';

    /*
    ** The task controller exists as a redirect from a task to the edit form. It will display the task if the task does not have an associated record.
    *  Otherwise it will display the record with the taskId specified.
     */
    angular.module('mod.app.task', ['mod.common.spEntityService', 'sp.navService'])

        .config(function ($stateProvider) {


            $stateProvider.state('taskView', {
                url: '/{tenant}/{eid}/taskView?path&formId',
                template: '<span ng-controller="taskController"></span>',
                data: { showBreadcrumb: false  }
            });
        })

        .controller('taskController', function ($scope, $state, $stateParams, spEntityService, spNavService) {

            var taskId = parseInt($stateParams.eid, 10) || $stateParams.eid;

            var request = "recordToPresent.id, isOfType.alias, openInEditMode, formToUse.id, promptForTaskArguments.id";

            spEntityService.getEntity(taskId, request).then(navigateToRecord, failToFindTask);

            function navigateToRecord(task) {
                var id, params, taskType, view = 'viewForm';
                var formId = parseInt($stateParams.formId, 10) || $stateParams.formId;

                if (task.recordToPresent) {
                    id = task.recordToPresent.idP;
                    params = {
                        taskId: task.idP,
                        inWizard: true
                    };

                    if (task.openInEditMode) {
                        view = 'editForm';
                    }

                    if (task.formToUse) {
                        params.formId = task.formToUse.idP;
                    }
                } else {
                    id = task.idP;

                    taskType = task.isOfType[0].nsAlias;

                    if (taskType === 'core:userSurveyTask') {
                        view = 'userSurveyTask';
                        params = {};

                    } else if (taskType === 'core:promptUserTask') {
                        view = "workflowInput";

                        params = {
                            inWizard: true
                        };

                    } else {    // just open the task
                        id = task.idP;

                        params = {
                            inWizard: true,
                            formId: task.formToUse ? task.formToUse.idP : formId // fall back
                        };
                    }
                }

                spNavService.navigateToSibling(
                    view,
                    id,
                    params);
            }

            function failToFindTask(error) {
                console.log('Unable to open task, returning to parent. TaskId: ', taskId);
                spNavService.navigateToParent();
            }
        });

}());