// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, sp */

(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spChartRenderControl', [
        'app.editForm.chartRenderControl',
        'mod.common.spMobile',
        'mod.common.spCachingCompile',
        'sp.themeService'
    ]);

    angular.module('mod.app.editForm.designerDirectives.spChartRenderControl')
        .directive('spChartRenderControl', spChartRenderControlDirective);

    /* @ngInject */
    function spChartRenderControlDirective(spMobileContext, spCachingCompile, spThemeService) {

        /////
        // Directive structure.
        /////
        return {
            restrict: 'AE',
            replace: false,
            transclude: false,
            scope: {
                formControl: '=?',
                parentControl: '=?',
                formData: '=?',
                formMode: '=?',
                isInTestMode: '=?',
                isReadOnly: '=?',
                isInDesign: '=?'
            },
            controller: 'chartRenderControl',
            link: link
        };

        function link(scope, element) {

            scope.getControlStyle = function (formControl) {
                var style = {};

                if (formControl) {

                    if (formControl.renderingBackgroundColor) {
                        style['background-color'] = formControl.renderingBackgroundColor;
                    } else {
                        style['background-color'] = 'white';
                    }
                }

                return style;
            };

            scope.getTitleStyle = function (formControl) {
                if (formControl) {
                    return spThemeService.getHeadingStyle();
                }
            };

            scope.$on('gather', function (event, callback) {
                callback(scope.formControl, scope.parentControl, element);
            });

            scope.$on('measureArrangeComplete', function (event) {

                if (scope && scope.formControl) {

                    if (element) {

                        var height = element.outerHeight();
                        if (height && height > 0) {

                            // 150px min height
                            if (height < 150) {
                                height = 150;
                            }

                            // The following is to fix a bug where the container control sizes incorrectly
                            // on mobile ... i believe its height is reverting to auto as it cannot find a
                            // containing block with an explicit height
                            // Anyway this seems to make it work.
                            var chartContainerElement = element.find('> .chart-render-control-container');
                            if (chartContainerElement && spMobileContext.isMobile) {
                                chartContainerElement.css('height', height + 'px');
                            }

                            var chartElement = element.find('> .chart-render-control-container > .chart-render-control');
                            if (chartElement) {

                                var titleElement = element.find('> .chart-render-control-container > .form-title');
                                if (titleElement) {

                                    // subtract the title height
                                    var titleHeight = titleElement.outerHeight(true);
                                    if (titleHeight > 0 && height > titleHeight) {

                                        chartElement.css('height', 'calc(100% - ' + titleHeight + 'px)');
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spChartRenderControl/spChartRenderControl.tpl.html');
            cachedLinkFunc(scope, function (clone) {
                element.append(clone);
            });
        }
    }
}());