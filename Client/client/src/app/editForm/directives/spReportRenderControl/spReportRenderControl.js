// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spReportRenderControl', ['app.editForm.reportRenderControl', 'mod.common.spCachingCompile', 'sp.themeService'])
        .directive('spReportRenderControl', function (spCachingCompile, spThemeService) {

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
                controller: 'reportRenderControl',
                link: function (scope, element) {
                    scope.$on('gather', function (event, callback) {
                        callback(scope.formControl, scope.parentControl, element);
                    });

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

                    scope.$on('measureArrangeComplete', function (event) {

                        if (scope && scope.formControl) {

                            if (element) {

                                var height = element.outerHeight();
                                if (height && height > 0) {

                                    // 150px min height 
                                    if (height < 150) {
                                        height = 150;
                                    }

                                    var reportElement = element.find('> .report-render-control > .spreport-view');
                                    if (!reportElement || reportElement.length === 0) {

                                        // check for the mobile container
                                        reportElement = element.find('> .report-render-control > .spmobile-report-view-container');
                                    }

                                    if (reportElement) {

                                        var titleElement = element.find('> .report-render-control > .form-title');
                                        var viewButtonElement = element.find('> .report-render-control > .rrc-full-report-button');
                                        if (titleElement || viewButtonElement) {

                                            var minusHeight = 0;

                                            if (titleElement) {
                                                minusHeight += titleElement.outerHeight(true);
                                            }
                                            
                                            if (viewButtonElement) {
                                                minusHeight += viewButtonElement.outerHeight(true);
                                            }

                                            // subtract the title and/or button heights
                                            if (minusHeight > 0 && height > minusHeight) {

                                                reportElement.css('height', 'calc(100% - ' + minusHeight + 'px)');
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });


                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spReportRenderControl/spReportRenderControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());