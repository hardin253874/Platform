// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, spWorkflowConfiguration, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.activities')
        .controller('displayFormActivityController', function ($scope, spWorkflowService) {

            $scope.$watch('activityParameters', function () {

                // extend the parameters including some overrides of the default action handlers

                $scope.activityParameters['core:inDisplayFormForUser'].resourceType = 'core:person';
                $scope.activityParameters['core:inDisplayFormForm'].resourceType = 'console:customEditForm';
                $scope.activityParameters['core:inDisplayFormPriority'].resourceType = 'core:eventEmailPriorityEnum';

                var timeoutValueString = $scope.activityParameters['core:inDisplayFormTimeOut'].expression.expressionString || '0';
                $scope.formTimeoutValue = parseFloat(timeoutValueString, 10);
            });

            $scope.$watch('formTimeoutValue', function (value, oldValue) {
                if (value !== oldValue)
                    $scope.activityParameters['core:inDisplayFormTimeOut'].expression.expressionString = value.toString();
            });

        });

}());
