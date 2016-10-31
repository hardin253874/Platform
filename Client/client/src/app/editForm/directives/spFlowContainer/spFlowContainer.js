// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spFlowContainer', [])
        .directive('spFlowContainer', function () {

            return {
                restrict: 'A',
                transclude: true,
                replace: true,
                scope: false,
                templateUrl: 'editForm/directives/spFlowContainer/spFlowContainer.tpl.html',
                link: function ($scope, $element, $attrs, $ctrl) {
                    //$element.css('background-color', '#' + Math.floor(Math.random() * 16777215).toString(16));

                    $scope.click = function (section) {
                        window.alert(section);
                    };
                    
                }
            };
        });
}());