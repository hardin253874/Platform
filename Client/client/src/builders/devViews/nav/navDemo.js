// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity */

(function () {
    'use strict';

    angular.module('app.navDemo', ['ui.router', 'titleService', 'mod.common.alerts', 'sp.navService', 'mod.common.spEntityService', 'spApps.reportServices'])
        .config(function ($stateProvider) {
            $stateProvider.state('navDemo', {
                url: '/{tenant}/{eid}/navDemo?path',
                templateUrl: 'devViews/nav/navDemo.tpl.html',
                controller: 'navDemoController'
            });

            $stateProvider.state('navDemoChild', {
                url: '/{tenant}/{eid}/navDemoChild?path',
                templateUrl: 'devViews/nav/navDemo.tpl.html',
                controller: 'navDemoController'
            });

            window.testNavItems = window.testNavItems || {};
            window.testNavItems.navDemo = { name: 'Nav Demo' };
        })
        .value('navDemoLogs', { open: false, logs: []})
        .controller('navDemoController', function ($scope, spNavService, spState, navDemoLogs) {

            var logsMaxLength = 10;

            var trimLogs = _.debounce(function () {
                if (navDemoLogs.logs.length > logsMaxLength) {
                    //log('trimmed from ' + navDemoLogs.logs.length);
                    navDemoLogs.logs = _.drop(navDemoLogs.logs, navDemoLogs.logs.length - logsMaxLength);
                }
            }, 1000);

            function log(str) {
                navDemoLogs.logs.push([new Date(), str]);
                trimLogs();
            }

            console.log('navDemoController ctor, state: ' + spState.name);

            $scope.$on("$destroy", function () {
                console.log('navDemoController destroyed. state: ' + spState.name);
            });

            $scope.$on('$locationChangeStart', function (event, to, from) {
                log('$locationChangeStart - ' + decodeURI(from) + '->' + decodeURI(to));
            });

            $scope.$on('$locationChangeSuccess', function (event, to, from) {
                log('$locationChangeSuccess - ' + decodeURI(from) + '->' + decodeURI(to));
            });

            $scope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {
                log('on $stateChangeStart - ' + fromState.name + '->' + toState.name);
            });

            $scope.$on('$stateChangeSuccess', function (event, toState, toParams, fromState, fromParams) {
                log('on $stateChangeSuccess - ' + fromState.name + '->' + toState.name);
            });

            $scope.navToChild = function () {
                spNavService.navigateToChildState('navDemoChild', 99999, null, { id: 33, name: 'something' });
            };

            $scope.navToSibling = function () {
                spNavService.navigateToSibling('navDemoChild', 99998, null);
            };

            $scope.navToParent = function () {
                spNavService.navigateToParent();
            };

            $scope.navBack = function () {
                spNavService.navigateBack();
            };

            $scope.keys = _.keys;
            $scope.values = _.values;
            $scope.pairs = _.toPairs;

            function addInheritedScopeProps(scope) {
                if (!scope) return;
                $scope.navItemScopeProps = $scope.navItemScopeProps.concat(_.map(_.filter(_.keys(scope), function (k) {
                    return k && k.length && k[0] !== '$';
                }), function (k) {
                    return $scope.navItem.scope === scope ? k : k + ' (' + scope.$id + ')';
                }));
                addInheritedScopeProps(scope.$parent);
            }

            $scope.$watch('spNavService.getBreadcrumb()', function (pathItems) {
                $scope.pathItems = pathItems;
                $scope.navItem = spState.navItem;
                $scope.navItemProps = _.keys($scope.navItem);
                $scope.navItemDataProps = _.keys($scope.navItem.data);

                $scope.navItemScopeProps = [];
                addInheritedScopeProps($scope.navItem.scope);
            });

            $scope.spNavService = spNavService;
            $scope.spState = spState;
            $scope.navDemoLogs = navDemoLogs;

            // initialise the scope key with a known inherited property
            $scope.scopeKey = 'bcIndex0';

        });
}());
