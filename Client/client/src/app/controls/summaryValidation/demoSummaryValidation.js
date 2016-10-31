// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _ */
(function () {
    'use strict';

    angular.module('app.demoSummaryValidation', ['ui.router', 'sp.common.spDialog'])


       .constant('SummaryValidationState', {
           name: 'demosummaryValidation',
           url: '/controls/summaryValidation',
           views: {
               'content@': {
                   controller: 'summaryValidationController',
                   templateUrl: 'controls/summaryValidation/demoSummaryValidation.tpl.html'
               }

           }
       })

    .controller('summaryValidationController', ["$scope", function ($scope) {

        $scope.showValidation = false;
        $scope.toggleValidationSummary = function () {
                $scope.showValidation = !$scope.showValidation;            
        };
        $scope.showAlert = false;
        $scope.toggleAlertSummary = function() {
            $scope.showAlert = !$scope.showAlert;

        };
        

        $scope.fieldControlUrl = "controls/summaryValidation/demoTextField.tpl.html";
        $scope.validationMessages = {};
        $scope.watchValidationMessage = function(expression, message) {
            // watch the return value from the scope.$eval(expression)
            $scope.$watch(function () { return $scope.$eval(expression); }, function (isVisible) {

                // check if the validation message exists
                var containsMessage = $scope.validationMessages.hasOwnProperty(expression);              

                // if the validation message doesn't exist and it should be visible, add it to the list
                if (!containsMessage && isVisible)
                    $scope.validationMessages[expression] = message;

                // if the validation message does exist and it shouldn't be visible, delete it
                if (containsMessage && !isVisible)
                    delete $scope.validationMessages[expression];
            });

        };
    }]);
    
    angular.module("app.demoSummaryValidation").directive("validationSummary", function () {
        return {
            restrict: "A",
            require: "^form",
            template: "<ul><li ng-repeat='(expression,message) in validationMessages'>{{message}}</li></ul>",
            link: function (scope, element, attributes, controller) {

                scope.validationMessages = {};

                // Hooks up a watch using [ng-show] expression
                controller.watchValidation = function (expression, message) {

                    // watch the return value from the scope.$eval(expression)
                    scope.$watch(function () { return scope.$eval(expression); }, function (isVisible) {

                        // check if the validation message exists
                        var containsMessage = scope.validationMessages.hasOwnProperty(expression);

                        // if the validation message doesn't exist and it should be visible, add it to the list
                        if (!containsMessage && isVisible)
                            scope.validationMessages[expression] = message;

                        // if the validation message does exist and it shouldn't be visible, delete it
                        if (containsMessage && !isVisible)
                            delete scope.validationMessages[expression];
                    });

                };

            }
        };


    });
    angular.module("app.demoSummaryValidation").directive("validationMessage", function () {
        return {
            restrict: "A",
            require: "^form",
            link: function (scope, element, attributes, controller) {

                // the ng-show expression used to determine message visibility
                var visibilityExpression = attributes.ngShow;

                // the validation message
                var message = element.text();

                // adds a watch to the validation message using the expression from the ng-show attribute
                controller.watchValidation(visibilityExpression, message);

                controller.watchAlert(visibilityExpression, message);
            }
        };
    });
    angular.module("app.demoSummaryValidation").directive("validateOnlyHelloWorld", function () {
        return {
            restrict: "A",
            require: "ngModel",
            link: function (scope, element, attributes, controller) {
                // dom -> model
                controller.$parsers.unshift(function (value) {
                    var isValid = angular.isString(value) && value.toString().toLowerCase() === "hello world";
                    controller.$setValidity("validateOnlyHelloWorld", isValid);

                    return isValid ? value : undefined;
                });

                // model -> dom
                controller.$formatters.unshift(function (value) {
                    var isValid = angular.isString(value) && value.toString().toLowerCase() === "hello world";
                    controller.$setValidity("validateOnlyHelloWorld", isValid);

                    return value;
                });
            }
        };
    });

    angular.module("app.demoSummaryValidation").directive("validationSetAlert", function () {
        return {
            restrict: "A",
            require: "^form",          
            template: "<label class='errorwatch' ng-click='toggleAlertSummary()'><img src='assets/images/icon_error.png' />{{validationNumbers}} errors</label>",
            link: function (scope) {
                
                
                scope.$watch('validationMessages', function() {
                    if (scope.validationMessages) {
                        scope.validationNumbers = _.size(scope.validationMessages);
                    }
                },true
                );
                


            }
        };


    });

    angular.module("app.demoSummaryValidation").directive("validationAlert", function () {
        return {
            restrict: "A",
            require: "^form",
            template: "<ul><li ng-repeat='(expression,message) in validationMessages'>{{message}}</li></ul>",
            link: function (scope, element, attributes, controller) {

                

                // Hooks up a watch using [ng-show] expression
                controller.watchAlert = function (expression, message) {

                    // watch the return value from the scope.$eval(expression)
                    scope.$watch(function () { return scope.$eval(expression); }, function (isVisible) {

                        // check if the validation message exists
                        var containsMessage = scope.validationMessages.hasOwnProperty(expression);

                        // if the validation message doesn't exist and it should be visible, add it to the list
                        if (!containsMessage && isVisible)
                            scope.validationMessages[expression] = message;

                        // if the validation message does exist and it shouldn't be visible, delete it
                        if (containsMessage && !isVisible)
                            delete scope.validationMessages[expression];
                    });

                };

            }
        };
    });
}());