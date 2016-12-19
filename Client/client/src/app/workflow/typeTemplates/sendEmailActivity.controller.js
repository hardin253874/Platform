// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflow, spWorkflowConfiguration, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.activities')
        .controller('sendEmailActivityController', function ($scope, spWorkflowService) {

            $scope.showInboxPicker = false;
            $scope.showRecipientListSettings = false;
            $scope.showRecipientAddressSettings = false;

            var stringToBoolean = function (string) {
                if (!string)
                    return false;
                switch (string.toLowerCase().trim()) {
                    case "true":
                    case "yes":
                    case "1":
                        return true;
                    case "false":
                    case "no":
                    case "0":
                    case null:
                        return false;
                    default:
                        return Boolean(string);
                }
            };

            $scope.$watch('activityParameters', function () {

                if (!$scope.activityParameters['core:sendEmailNoReply'].expression.expressionString) 
                    $scope.activityParameters['core:sendEmailNoReply'].expression.expressionString = "true";

                $scope.activityParameters['core:sendEmailRecipientsType'].resourceType = "core:sendEmailActivityRecipientsTypeEnum";
                $scope.activityParameters['core:sendEmailDistributionType'].resourceType = "core:sendEmailActivityDistributionTypeEnum";

                var val = $scope.activityParameters['core:sendEmailRecipientsType'].expression.expressionString;
                if (val && (val.indexOf("List") !== -1)) {
                    $scope.showRecipientListSettings = true;
                    $scope.showRecipientAddressSettings = false;
                }
                else if (val && (val.indexOf("Address") !== -1)) {
                    $scope.showRecipientListSettings = false;
                    $scope.showRecipientAddressSettings = true;
                }

                $scope.showInboxPicker = stringToBoolean($scope.activityParameters['core:sendEmailNoReply'].expression.expressionString);

            });
        });
}());
