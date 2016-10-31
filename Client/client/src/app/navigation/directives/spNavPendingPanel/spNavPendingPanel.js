// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console */

(function () {
    "use strict";

    /*
    ** Displays the navigationPending message if the is an internal or external navigation and a dirty page.
    */
    angular.module('mod.app.navigation.directives')
        .directive('spNavPendingPanel', function () {
            return {
                restrict: 'E',
                templateUrl: 'navigation/directives/spNavPendingPanel/spNavPendingPanel.tpl.html',
                link: function ($scope, $element) {
                }
            };
        });
}());