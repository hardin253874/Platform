// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, spWorkflowConfiguration, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.activities')
        .controller('exportToActivityController', function ($scope, spWorkflowService) {

            $scope.$watch('activityParameters', function () {

                $scope.activityParameters['core:inExportToReport'].resourceType = 'core:report';
                $scope.activityParameters['core:inExportToFormat'].resourceType = 'core:exportFileTypeEnum';
       
            });

          
        });

}());
