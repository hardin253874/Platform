// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, spWorkflowConfiguration, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.activities').controller('launchPersonCampaignActivityController', launchPersonCampaignActivityController);
    
    function launchPersonCampaignActivityController($scope) {
        $scope.$watch('activityParameters', function () {
            $scope.activityParameters['core:inLaunchPersonSurvey'].resourceType = 'core:userSurvey';
            $scope.activityParameters['core:inLaunchPersonRecipients'].resourceType = 'core:person';
            $scope.activityParameters['core:inLaunchPersonTarget'].resourceType = 'core:userResource';
       
            var timeoutValueString = $scope.activityParameters['core:inLaunchPersonDueDays'].expression.expressionString || '0';
            $scope.formTimeoutValue = parseFloat(timeoutValueString, 10);
        });

        $scope.$watch('formTimeoutValue', function (value, oldValue) {
            if (value !== oldValue)
                $scope.activityParameters['core:inLaunchPersonDueDays'].expression.expressionString = value.toString();
        });
    }
}());
