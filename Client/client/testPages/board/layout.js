/*global _, angular, console */

(function () {
    'use strict';

    angular.module('app')
        .factory('appState', function () {
            return {};
        })
        .directive('myAppLayout', myAppLayout);

    /* @ngInject */
    function myAppLayout(appState) {
        return {
            restrict: 'E',
            transclude: true,
            replace: true,
            template: '<div class="app-layout" ng-transclude></div>',
            link: link
        };

        function link(scope) {
            scope.onClick = function () {
                console.log('hello', appState);
            };
            scope.toggleNav = function () {
                appState.showNav = !appState.showNav;
            };
            scope.appState = appState;
            appState.showNav = true;
        }
    }
}());
