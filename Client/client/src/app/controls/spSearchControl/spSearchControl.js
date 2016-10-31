// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
     * Module implementing a search control.
     *
     * @module spSearchControl
     * @example

     Using the spSearchControl:

     &lt;sp-search-control model="value"&gt;&lt;/sp-search-control&gt

     where myModel is the value to search for.

     */
    angular.module('app.controls.spSearchControl', ['mod.common.spMobile', 'mod.common.spCachingCompile'])
        .directive('spSearchControl', function (spMobileContext, spCachingCompile) {

            return {
                restrict: 'E',
                replace: false,
                transclude: false,
                scope: {
                    spModel: '=?',
                    startVisible: '=?'
                },                
                link: function (scope, element) {

                    scope.changed = function () {
                        if (_.isFunction(scope.spModel.onSearchValueChanged)) {
                            scope.spModel.onSearchValueChanged();
                        }
                    };

                    scope.clear = function () {
                        if (scope.spModel.value) {
                            scope.spModel.value = null;

                            if (spMobileContext.isMobile) {
                                element.find('.sp-search-control-input').focus();
                            }

                            scope.changed();
                        }
                    };

                    scope.show = function () {
                        if (spMobileContext.isMobile) {
                            element.find('input').addClass('show-search');
                        }                        
                    };

                    scope.onBlur = function () {
                        if (spMobileContext.isMobile &&
                            !scope.spModel.value) {
                            element.find('input').removeClass('show-search');
                        }
                    };

                    if (!scope.spModel) {
                        scope.spModel = {value: null, isBusy: false};
                    }                    

                    var cachedLinkFunc = spCachingCompile.compile('controls/spSearchControl/spSearchControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });

                    if (scope.startVisible) {
                        scope.show();
                    }

                    if (spMobileContext.isMobile) {
                        scope.$on('$locationChangeSuccess', function (event, newUrl, oldUrl) {
                            if (newUrl !== oldUrl) {
                                element.find('input').removeClass('show-search');
                            }
                        });
                    }                    
                }
            };
        });
}());