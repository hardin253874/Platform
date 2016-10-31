// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, spWorkflowConfiguration, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.activities')
        .controller('generateDocActivityController', function ($scope, spWorkflowService) {

            $scope.$watch('activityParameters', function () {

                $scope.activityParameters['core:inGenerateDocTemplate'].resourceType = 'core:reportTemplate';
                $scope.activityParameters['core:inGenerateDocSource'].resourceType = 'core:userResource';
       
            });

          
        });

}());
