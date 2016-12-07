// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, spWorkflowConfiguration, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.activities').controller('startSurveyActivityController', startSurveyController);
    
    function startSurveyController($scope) {

        $scope.$watch('activityParameters', function () {
            $scope.activityParameters['core:inStartSurveyCampaign'].resourceType = 'core:surveyCampaign';
        });
    }
}());
