// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console */

(function () {
    "use strict";

    /*
    ** Displays the navigationPending message
    */
    angular.module('mod.app.navigation.directives')
        .directive('spNavPending', function (spEntityHelper) {
            return {
                restrict: 'E',
                scope: {
                    condition: '=',
                    message: '=',
                    onContinue: '=',
                    onCancel: '='
                },
                templateUrl: 'navigation/directives/spNavPending/spNavPending.tpl.html',
                link: function ($scope, $element) {
                }
            };
        });
}());