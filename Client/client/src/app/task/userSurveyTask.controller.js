// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use strict';

    /*
    ** The task controller exists as a redirect from a task to the edit form. It will display the task if the task does not have an associated record.
    *  Otherwise it will display the record with the taskId specified.
     */
    angular.module('mod.app.userSurveyTask').controller('UserSurveyTaskController', UserSurveyTaskController);

    function UserSurveyTaskController($scope, $state, $stateParams, $q, spUserSurveyTaskService, spEntityService, spNavService, spAlertsService) {
        var that = this;
        
        var initialBookmark = null;
        var reloadSurvey = true;
        var page = 0;
        var pages;

        that.taskId = null;
        that.questions = null;
        that.task = null;
        that.answers = null;
        
        that.save = save;
        that.complete = complete;
        that.beforeSave = beforeSave;
        that.afterSave = afterSave;
        that.getAnswerText = spUserSurveyTaskService.getAnswerText;
        that.sectionsByPage = filterSectionsByPage;
        that.hasPages = hasPages;
        that.canPageBack = canPageBack;
        that.canPageNext = canPageNext;
        that.pageBack = pageBack;
        that.pageNext = pageNext;

        var navItem = spNavService.getCurrentItem();

        if (navItem)
            navItem.isDirty = isDirty;

        function initialise() {
            that.taskId = parseInt($stateParams.eid, 10);

            if (!that.taskId) {
                var error = "Id missing from parameters";
                displayError(error);
                return $q.reject(error);
            }
            
            function updateSurvey(surveyTask) {
                that.task = surveyTask;
                that.readOnly = surveyTask.userTaskIsComplete || surveyTask.userSurveyTaskForReview;
                that.title = surveyTask.name;
                that.dueDate = surveyTask.userTaskDueOn;
                that.helpText = surveyTask.userSurveyTaskHelp;
                that.surveyId = 'ABC123';

                that.results = surveyTask.userSurveyTaskSurveyResponse;

                that.answers = that.results.surveyAnswers;
                that.displayStructure = spUserSurveyTaskService.createDisplayStructure(surveyTask);

                pages = _(that.displayStructure).map('page').uniq().value().sort();

                that.targetDefinition = surveyTask.userSurveyTaskTargetDefinition;
                
                if (that.targetDefinition && that.task.userSurveyTaskAllowTargetEdit) {          // edit target
                    that.targetMode = 'edit';
                    that.targetTitle = that.targetDefinition.name;

                    var selected = [];
                    if (that.results && that.results.surveyTarget && that.results.surveyTarget.idP) {
                        selected = [that.results.surveyTarget];
                    }
                    that.targetPickerOptions = {
                        selectedEntityId: null,
                        selectedEntity: null,
                        selectedEntityIds: [],
                        selectedEntities: selected,
                        entityTypeId: that.targetDefinition.idP,
                        multiSelect: false,
                        allowCreateRecords: false
                    };
                } else if (that.results.surveyTarget) {                                         // view target
                    that.targetMode = 'view';
                    that.targetTitle = 'Target';
                } else {                                                                        // no target
                    that.targetMode = null;
                    that.targetTitle = '';
                }

                var questions = spUserSurveyTaskService.getTaskQuestions(surveyTask);

                that.allowNotes = _.some(questions, { questionAllowNotes: true });
                that.allowAttachments = _.some(questions, { questionAllowAttachments: true });
                
                if (surveyTask.userSurveyTaskAllowComments) {
                    that.commentMode = 'edit';
                } else {
                    that.commentMode = that.results.surveyResponseComments ? "view" : "none";
                }

                var stateAlias = that.results.surveyStatus.alias();

                that.showProgress = stateAlias === 'core:sseNotStarted' || stateAlias === 'core:sseInProgress';
            }

            return spUserSurveyTaskService.getTask(that.taskId).then(updateSurvey, displayRequestError);
        }

        //
        // Buttons
        //
        function save() {
            if (!that.results.surveyStartedOn) {
                that.results.surveyStartedOn = new Date();
            }

            if (that.targetDefinition &&
                that.targetPickerOptions &&
                that.targetPickerOptions.selectedEntities &&
                that.targetPickerOptions.selectedEntities.length) {
                that.results.surveyTarget = that.targetPickerOptions.selectedEntity || _.first(that.targetPickerOptions.selectedEntities);
            }

            beforeSave();

            return spEntityService.putEntity(that.task, true)
                .then(afterSave);
        }

        function complete() {
            try {
                reloadSurvey = false;
                that.readOnly = true;
                spUserSurveyTaskService.completeTask(that.task);
                save().then(spNavService.navigateToParent);
            } catch (e) {
                displayError(e.message || e);
            }
        }

        function beforeSave() {           
            spUserSurveyTaskService.setTaskStatusToInProgress(that.task);
            return $q.when();
        }

        function afterSave() {
            displayMessage("Saved");

            // workaround: reload the survey. #26982: Survey:Attachments on survey answers are not being removed or deleted.
            if (reloadSurvey) {
                return initialise().then(setInitialBookmark);
            } else {
                setInitialBookmark();
            }

            return $q.when();
        }
        
        //
        // Bookmarks
        //
        function setInitialBookmark() {
            initialBookmark = getEditHistory().addBookmark('editStart');
        }

        function isDirty() {
            if (!that.results)
                return false;

            return getEditHistory().changedSinceBookmark(initialBookmark);
        }

        function getEditHistory() {
            return that.results.graph.history;
        }

        //
        // Progress
        //
        function updateProgress() {
            if (that.answers != null) {
                that.progress = spUserSurveyTaskService.getTaskProgress(that.task);
            }
        }

        var updateProgressThrottle = _.throttle(updateProgress, 200);
        var clearProgressWatch;

        function setUpProgressWatch() {
            clearProgressWatch = $scope.$watch(updateProgressThrottle);
        }

        //
        // Paging
        //
        function filterSectionsByPage(section) {
            return section.page === pages[page];
        }

        function hasPages() {
            return pages && pages.length > 1;
        }

        function canPageBack() {
            return pages && pages.length && page > 0;
        }

        function canPageNext() {
            return pages && pages.length && (page < (pages.length - 1));
        }

        function pageBack() {
            if (canPageBack()) {
                page--;
            }
        }

        function pageNext() {
            if (canPageNext()) {
                page++;
            }
        }
        
        //
        // Error messages
        //
        function displayRequestError(error) {
            displayError('Request failed with error: ' + error);
        }

        function displaySaveError(error) {
            displayError(error.data.Message);
        }

        function displayError(error) {
            spAlertsService.addAlert(error, { severity: 'error', page: $state.current });
        }

        function displayMessage(message) {
            spAlertsService.addAlert(message, { severity: spAlertsService.sev.Info, expires: true });
        }

        initialise()
            .then(setInitialBookmark)
            .then(setUpProgressWatch);
    }

}());