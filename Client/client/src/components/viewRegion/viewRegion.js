// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    "use strict";

    angular.module('mod.common.viewRegion', [])
        .factory('spViewRegionService', function () {

            var exports = {};
            var regions = {};

            function peekView(regionKey) {
                return _.last(regions[regionKey]);
            }

            exports.peekView = peekView;

            function viewStack(regionKey) {
                return regions[regionKey];
            }

            exports.viewStack = viewStack;

            function pushView(regionKey, view) {
                regions[regionKey] = (regions[regionKey] || []).concat([view]);
            }

            exports.pushView = pushView;

            function popView(regionKey) {
                var view = peekView(regionKey);
                regions[regionKey] = _.initial(regions[regionKey]);
                if (view && view.onPopped) {
                    view.onPopped();
                }
                return view;
            }

            exports.popView = popView;

            function clearViews(regionKey) {
                while (popView(regionKey)) {
                }
            }

            exports.clearViews = clearViews;

            return exports;
        })
        .directive('spViewRegionContainer', function ($http, $q, $templateCache, $compile, spViewRegionService) {

            return {
                restrict: 'E',
                replace: true,
                template: '<div class="layout-container" ng-class="{zoomed: isZoomed}">' +
                    '  <div ng-repeat="view in viewService.viewStack(region)" class="layout-container" ng-show="shouldShowView(view, $last)">' +
                    '    <div ng-include="view.templateUrl" class="layout-container"></div>' +
                    '  </div> ' +
                    '</div> ',
                scope: {
                    region: '@'
                },
                link: function (scope, element, attrs) {

                    function notifyLayoutChange() {
                        scope.$emit('app.layout');
                        scope.$root.$broadcast('sp.app.ui-refresh');
                    }

                    scope.close = function () {
                        var view = spViewRegionService.popView(scope.region);
                        //console.log('viewRegion closed', view);
                    };

                    scope.toggleZoom = function () {
                        scope.isZoomed = !scope.isZoomed;
                        notifyLayoutChange();
                    };

                    scope.viewService = spViewRegionService;

                    scope.$watch('viewService.viewStack(region)', function (viewStack, prev) {

                        //console.log('viewRegion stack watch fired', viewStack);

                        var topView = _.last(viewStack);
                        if (topView) {
                            element.addClass('open');
                        } else {
                            element.removeClass('open');
                            scope.isZoomed = false;
                        }
                        notifyLayoutChange();
                    });

                    scope.shouldShowView = function (view, last) {
                        view.isTopView = last;
                        return last;
                    };

                    element.addClass(scope.region.replace(/[^a-zA-Z0-9-_]/g, '') + '-view-region');
                }
            };
        });

})();
