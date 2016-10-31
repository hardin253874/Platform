// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, sp */

(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spPageSelector',
        ['mod.common.spCachingCompile', 'mod.app.editFormServices']);

    angular.module('mod.app.editForm.designerDirectives.spPageSelector')
        .directive('spPageSelector', spPageSelector);

    /* @ngInject */
    function spPageSelector(spCachingCompile, spEditForm, $timeout) {

        /////
        // The spPageSelector directive is a UI for selecting between pages.
        /////

        return {
            restrict: 'AE',
            replace: false,
            transclude: false,
            scope: {
                pages: '=',
                selectedPage: '=',
                haveTaskPage: '='       // Is the last page a task page?
            },
            link: link
        };


        function link(scope, element) {
            scope.pageName = function (pageIndex) {
                //return pageName(sp.result(scope.pages, [pageIndex, 'scope', 'pageFormControl']));
                return sp.result(scope.pages, [pageIndex, 'name']);
            };

            scope.isSelected = function (index) {
                return index === scope.selectedPage ? 'selected' : 'unselected';
            };

            scope.selectPage = function (pageIndex) {
                scope.selectedPage = pageIndex;
            };

            scope.openDetailsPage = function () {
                scope.selectedPage = 0;
            };

            scope.openTaskPage = function () {
                scope.selectedPage = scope.pages.length - 1;
            };

            scope.$watchCollection('pages', function () {
                updateVisiblePages();
            });

            scope.$watch('haveTaskPage', function () {
                updateVisiblePages();
            });

            scope.$watch('selectedPage', updateSelectedPage);

            scope.$watch('selectedPageOption', function (page) {
                if (page && scope.visiblePages) {
                    scope.selectedPage = _.indexOf(scope.visiblePages, page);
                }
            });

            var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spPageSelector/spPageSelector.tpl.html');
            cachedLinkFunc(scope, function (clone) {
                element.append(clone);
            });

            function updateVisiblePages() {
                if (scope.pages) {

                    // add a suitable name onto the page based on its formControl
                    _.forEach(scope.pages, function (p) {
                        p.name = pageName(sp.result(p, 'scope.pageFormControl'));
                    });

                    // set the visible pages based on whether we are showing the tasks page
                    scope.visiblePages = scope.pages; //!scope.haveTaskPage ? scope.pages : _.initial(scope.pages);

                    // ensure some page is selected
                    updateSelectedPage();
                }
            }

            function updateSelectedPage() {
                if (scope.pages) {
                    scope.selectedPage = scope.selectedPage || 0;
                    scope.selectedPage = Math.min(scope.pages.length - 1, scope.selectedPage);
                    scope.selectedPageOption = scope.pages[scope.selectedPage];
                }
            }

            function pageName(formControl) {
                if (formControl) {
                    if (formControl.name) {
                        return 'Details';
                    }
                    if (sp.result(formControl, 'containedControlsOnForm.length') > 0) {
                        return spEditForm.getControlTitle(formControl.containedControlsOnForm[0]);
                    }
                }
                // mostly others mean our actions page
                return 'Actions';
            }
        }
    }
}());