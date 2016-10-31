// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spFormRenderControl', ['app.editForm.formRenderControl', 'mod.common.spMobile', 'mod.common.spCachingCompile', 'sp.themeService'])
        .directive('spFormRenderControl', function ($rootScope, spMobileContext, spCachingCompile, spThemeService) {

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
                controller: 'formRenderControl',
                link: function (scope, element) {

                    scope.isMobile = spMobileContext.isMobile;

                    // Forms are not supported on the mobile 
                    if (scope.isMobile)
                        return;

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

                                    var customEditFormElement = element.find('> form > sp-custom-edit-form');
                                    if (customEditFormElement) {

                                        var topElement = element.find('> form > .form-title');
                                        if (!topElement) {

                                            topElement = element.find('> form > div > sp-tool-box');
                                        }

                                        // subtract the toolbox/form-title height
                                        if (topElement) {

                                            var topHeight = topElement.outerHeight(true);
                                            if (topHeight > 0 && height > topHeight) {

                                                customEditFormElement.css('height', 'calc(100% - ' + topHeight + 'px)');
                                                customEditFormElement.css('min-height', '');
                                                customEditFormElement.css('width', '100%');
                                            }
                                        }
                                    }

                                    var containedStackElement = element.find('> form > sp-custom-edit-form > .custom-edit-form > sp-vertical-stack-container-control');
                                    if (containedStackElement) {

                                        // kill any double-scrollbars
                                        containedStackElement.css('min-height', '');
                                        containedStackElement.css('width', '100%');
                                    }
                                }
                            }                            
                        }
                    });


                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spFormRenderControl/spFormRenderControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());