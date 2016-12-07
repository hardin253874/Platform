// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, spWorkflowConfiguration, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.activities').controller('launchTargetCampaignActivityController', launchTargetCampaignActivityController);
    
    function launchTargetCampaignActivityController($scope) {
        $scope.$watch('activityParameters', function () {
            $scope.activityParameters['core:inLaunchTargetSurvey'].resourceType = 'core:userSurvey';
            $scope.activityParameters['core:inLaunchTargetTargets'].resourceType = 'core:userResource';
            $scope.activityParameters['core:inLaunchTargetSurveyTaker'].resourceType = 'core:relationship';
       
            var timeoutValueString = $scope.activityParameters['core:inLaunchTargetDueDays'].expression.expressionString || '0';
            $scope.formTimeoutValue = parseFloat(timeoutValueString, 10);
        });

        $scope.$watch('formTimeoutValue', function (value, oldValue) {
            if (value !== oldValue)
                $scope.activityParameters['core:inLaunchTargetDueDays'].expression.expressionString = value.toString();
        });
    }
}());
