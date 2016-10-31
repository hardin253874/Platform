// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _ */
(function() {
    'use strict';

    angular.module('app.demoTextField', ['ui.router',  'sp.common.spDialog'])
        .constant('TextFieldState', {
            name: 'demoTextField',
            url: '/controls/summaryValidation',
            views: {
                'content@': {
                    controller: 'textFieldController',
                    templateUrl: 'controls/summaryValidation/demoTextField.tpl.html'
                }
            }
        })
    
    .controller('textFieldController', ["$scope", function ($scope) {

       
    }]);

    angular.module("app.demoTextField").directive("validationMessage", function () {
        return {
            restrict: "A",
            require: "^form",
            link: function (scope, element, attributes, controller) {
              
                // the ng-show expression used to determine message visibility
                var visibilityExpression = attributes.ngShow;

                // the validation message
                var message = element.text();
                                
                scope.watchValidationMessage(visibilityExpression, message);

            }
        };
    });
}());