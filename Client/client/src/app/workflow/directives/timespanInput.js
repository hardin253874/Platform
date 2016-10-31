// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spWorkflowConfiguration, spWorkflow, sp, spEntity */

(function () {
    'use strict';

    angular.module('sp.workflow.directives.timespanInput', [])
        .directive('spWorkflowTimespanInput', function ($parse) {
            var template = '<div>' +
                '<input type="number" ng-model="value" min="0"/>' +
                '<select ng-model="units">' +
                '<option value="1440">minutes</option>' +
                '<option value="24">hours</option>' +
                '<option value="1">days</option>' +
                '</select>' +
                '</div>';

            var minutesInDay = 3600;

            var link = function (scope, element, attrs) {

                scope.units = "1"; // days - this needs to be a string to match up to the options

                scope.$watch('model', function (newModel, oldModel) {
                    if (newModel != oldModel || scope.value === undefined)
                        scope.value = newModel * scope.units; // go from days to whatever the current units are
                });
                scope.$watch('value', function (newValue, oldValue) {
                    if (newValue != oldValue)
                        scope.model = newValue / scope.units;
                });
                scope.$watch('units', function (newUnits, oldUnits) {
                    if (newUnits != oldUnits)
                        scope.value = scope.value / oldUnits * newUnits;
                });
            };

            return {
                restrict: 'E',
                template: template,
                replace: true,
                scope: {
                    model: '='
                },
                link: link
            };
        });
}());
