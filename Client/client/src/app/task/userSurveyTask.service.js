// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use	strict';

    angular.module('mod.app.userSurveyTask').factory('spUserSurveyTaskService', UserSurveyTaskService);

    function UserSurveyTaskService(spEntityService) {

        return {
            getTask: getTask,
            createDisplayStructure: createDisplayStructure, // Create the structure used fro displaying the questions. 
            completeTask: completeTask,                     // Mark a task as complete
            getAnswerText: getAnswerText,                   // Get a textural representation of the answer - null of no answer
            getTaskProgress: getTaskProgress,               // Get the progress of the task. float between 0 and 1
            setTaskStatusToInProgress: setTaskStatusToInProgress, // Set the status of the task to inProgress
            getTaskQuestions: getTaskQuestions,             // Get all the questions that form the surevey 
            getTaskAnswers: getTaskAnswers,                 // Get all the answers that form the survey
            answerIsAnswered: isAnswered                    // Is the given answer answered?
        };

        function getTask(taskId) {

            var questionRequest = 'isOfType.alias, name, questionId, questionOrder, numericQuestionIsInteger, choiceQuestionIsMulti, choiceQuestionChoiceSet.{ name, choiceOptionSetChoices.{name, description, choiceOptionOrder}}, questionAllowNotes, questionAllowAttachments';
            var sectionRequest = 'name, surveySectionOrder, surveySectionWeight, surveyPage, surveyQuestions.{' + questionRequest + '}';
            var questionAnswer = 'surveyAnswerOriginalQuestionText, questionBeingAnswered.{' + questionRequest + '}, questionInSection.{' + sectionRequest + '}, surveyAnswerString, surveyAnswerNumber, {surveyAnswerSingleChoice, surveyAnswerMultiChoice}.{name, description}, surveyAnswerNotes, surveyAnswerAttachments.{name, description, fileDataHash}';
            var resultsRequest = 'surveyStartedOn, surveyCompletedOn, surveyAnswers.{' + questionAnswer + '}, campaignForResults.surveyClosesOn, surveyTarget.name, surveyStatus.{name, alias}, surveyResponseComments';
            var request = 'name, userTaskIsComplete, taskStatus.{id, alias}, userTaskDueOn, userSurveyTaskTargetDefinition.name, userSurveyTaskSurveyResponse.{' + resultsRequest + '}, userSurveyTaskForReview, userSurveyTaskAllowComments, userSurveyTaskHelp, availableTransitions.fromExitPoint.{name, description, exitPointOrdinal}, userResponse.id, userSurveyTaskAllowTargetEdit';

            return spEntityService.getEntity(taskId, request);
        }

        function createDisplayStructure(task) {
            var answers = getTaskAnswers(task);

            return _(answers).groupBy(groupAnswerBySectionFn).map(createSection).sortBy(['sectionOrder', 'name']).value();
        }

        function groupAnswerBySectionFn(answer) {
            return answer.questionInSection.idP;
        }

        function createSection(answers) {
            var section = answers[0].questionInSection;
            return {
                name: section.name,
                sectionOrder: section.surveySectionOrder,
                page: section.surveyPage || 1,
                entity: section, // all questions will have the same section
                answers: _(answers).sortBy(['questionBeingAnswered.questionOrder', 'questionBeingAnswered.questionId', 'questionBeingAnswered.name']).value()
            };
        }

        function completeTask(task) {
            task.userTaskIsComplete = true;
            task.taskStatus = spEntity.fromJSON({ id: 'core:taskStatusCompleted' });
            task.userSurveyTaskSurveyResponse.surveyStatus = spEntity.fromJSON({ id: 'core:sseCompleted' });
        }
        
        function getAnswerText(answer) {
            if (answer.surveyAnswerString)
                return answer.surveyAnswerString;
            if (answer.surveyAnswerNumber)
                return answer.surveyAnswerNumber;
            else if (answer.surveyAnswerSingleChoice)
                return answer.surveyAnswerSingleChoice.name;
            else if (answer.surveyAnswerMultiChoice)
                return _.map(answer.surveyAnswerMultiChoice, 'name').join(', ');
            else return "Unanswered";
        }
        
        function getTaskProgress(task) {
            var answers = getTaskAnswers(task);

            if (answers && answers.length) {
                var answered = _.filter(answers, isAnswered);
                return Math.floor(100 * (answered.length / answers.length));
            }
            
            return 0.0;
        }

        function setTaskStatusToInProgress(task) {
            if (task.taskStatus.alias() === 'core:taskStatusNotStarted')
                task.taskStatus = spEntity.fromJSON({ id: 'core:taskStatusInProgress' });

            var result = task.userSurveyTaskSurveyResponse;

            if (result.surveyStatus.alias() === 'core:sseNotStarted')
                result.surveyStatus = spEntity.fromJSON({ id: 'core:sseInProgress' });
        }

        function getTaskQuestions(task) {
            return _.map(getTaskAnswers(task), 'questionBeingAnswered');
        }

        function getTaskAnswers(task) {
            var results = task.userSurveyTaskSurveyResponse;

            return results.surveyAnswers;
        }
        
        function isAnswered(answer) {
            var questionType = answer.questionBeingAnswered.isOfType[0].alias();

            if (questionType === 'core:choiceQuestion') {
                return answer.surveyAnswerSingleChoice || answer.surveyAnswerMultiChoice.length;
            } else if (questionType === 'core:textQuestion') {
                return !isWhitespaceOrNull(answer.surveyAnswerString);
            } else if (questionType === 'core:numericQuestion') {
                return !isWhitespaceOrNull(answer.surveyAnswerNumber);
            }

            return false;
        }
        
        function isWhitespaceOrNull(s) {
            return !s || /^\s+$/.test(s);
        }
    }

}());