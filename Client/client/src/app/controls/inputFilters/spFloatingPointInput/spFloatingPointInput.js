// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    angular.module('app.controls.inputFilters.spFloatingPointInput', [])
        .directive('spFloatingPointInput', function ($parse) {
            return {
                restrict: 'A',
                require: 'ngModel',
                priority: 100,  // note: priority is set higher so that this directive runs last and we can inject our parser as the first element of '$parsers' array. (to make our parser run first). 
                link: function (scope, element, attrs, modelCtrl) {
                    var pointChar = Globalize.culture().numberFormat['.'];
                    var seperaterChar = Globalize.culture().numberFormat[','];
                    var notNumber = new RegExp("[^0-9\\-" + pointChar + seperaterChar + "]", "g");

                    modelCtrl.$parsers.unshift(function (inputValue) {

                        if (!inputValue) {
                            return inputValue;
                        }
                        
                        var transformedInput = inputValue.replace(notNumber, '');

                        if (transformedInput !== inputValue) {   // replace the text with the filtered one
                            modelCtrl.$setViewValue(transformedInput);
                            modelCtrl.$render();
                        }

                        return transformedInput;

                    });
                }
            };
        });
}());