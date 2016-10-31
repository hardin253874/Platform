// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // The spPageContainer work with the spPageSelector to flip between templates and scopes provided in an array.
    // Each page is not rendered until it is hit for the first time.
    /////

    angular.module('mod.app.editForm.designerDirectives.spPageContainer', ['sp.navService', 'mod.common.spCachingCompile']);

    angular.module('mod.app.editForm.designerDirectives.spPageContainer')
        .directive('spPageContainer', pageContainerDirective );

    /* @ngInject */
    function pageContainerDirective(spNavService, spCachingCompile) {

        return {
            restrict: 'AE',
            replace: false,
            transclude: false,
            scope: {
                pagerOptions: '='
            },
            link: link
        };

        function link($scope, element) {

            $scope.selectedPage = 0;
            $scope.isRendered = [];

            // Move to the next tab, wrapping around
            $scope.swipeLeft = onSwipeLeft;
            // Move to the previous tab
            $scope.swipeRight = onSwipeRight ;

            $scope.$watch('pagerOptions.selectedPage', selectedPageChanged);

            var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spPageSelector/spPageContainer.tpl.html');
            cachedLinkFunc($scope, function (clone) {
                element.append(clone);
            });

            function selectedPageChanged(pageIndex) {
                //console.log('DEBUG: spPageContainer selectedPage', pageIndex);

                if ($scope.pagerOptions && $scope.pagerOptions.pages) {
                    $scope.selectedPage = pageIndex;

                    // only pages that have been selected will be rendered. Once they are rendered they stay there.
                    $scope.isRendered[pageIndex] = true;

                    // do layout (ideally only first time the page is selected. But do it every time selection is changed. See #23779.)
                    $scope.$emit('doLayout');
                }
            }

            function onSwipeLeft(event) {
                event.srcEvent.stopPropagation();
                event.preventDefault();

                $scope.pagerOptions.selectedPage = ($scope.pagerOptions.selectedPage + 1) % $scope.pagerOptions.pages.length;
            }

            function onSwipeRight(event) {
                event.srcEvent.stopPropagation();
                event.preventDefault();

                if ($scope.pagerOptions.selectedPage > 0) {
                    $scope.pagerOptions.selectedPage--;
                } else {
                    spNavService.navigateToParent();
                }
            }
        }
    }
}());