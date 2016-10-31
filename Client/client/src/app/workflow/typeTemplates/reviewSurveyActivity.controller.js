// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, spWorkflowConfiguration, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.activities')
        .controller('reviewSurveyActivityController', reviewSurveyActivityController);
    
    function reviewSurveyActivityController($scope, spWorkflowService) {

        $scope.$watch('activityParameters', function () {

            $scope.activityParameters['core:inReviewSurveyResponse'].resourceType = 'core:surveyResponse';
            $scope.activityParameters['core:inReviewSurveyReviewer'].resourceType = 'core:person';
            $scope.activityParameters['core:inReviewSurveyEntryState'].resourceType = 'core:surveyStatusEnum';

       
            var timeoutValueString = $scope.activityParameters['core:inReviewSurveyDueInDays'].expression.expressionString || '0';
            $scope.formTimeoutValue = parseFloat(timeoutValueString, 10);
        });

        $scope.$watch('formTimeoutValue', function (value, oldValue) {
            if (value != oldValue)
                $scope.activityParameters['core:inReviewSurveyDueInDays'].expression.expressionString = value.toString();
        });
    }

}());
