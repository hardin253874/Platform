// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, spWorkflowConfiguration, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.activities')
        .controller('startSurveyActivityController', startSurveyController);
    
    function startSurveyController($scope, spWorkflowService) {

        $scope.$watch('activityParameters', function () {

            $scope.activityParameters['core:inStartSurveyCampaign'].resourceType = 'core:surveyCampaign';
            $scope.activityParameters['core:inStartSurveyTarget'].resourceType = 'core:userResource';
       
            var timeoutValueString = $scope.activityParameters['core:inStartSurveyDueDays'].expression.expressionString || '0';
            $scope.formTimeoutValue = parseFloat(timeoutValueString, 10);
        });

        $scope.$watch('formTimeoutValue', function (value, oldValue) {
            if (value != oldValue)
                $scope.activityParameters['core:inStartSurveyDueDays'].expression.expressionString = value.toString();
        });
    }

}());
