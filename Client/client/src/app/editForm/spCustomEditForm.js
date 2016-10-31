// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('mod.common.editForm.editFormDirectives', [
        'ng',
        'sp.common.fieldValidator',
        'mod.app.editFormServices',
        'mod.common.alerts',
        'mod.common.spMobile',
        'mod.common.spCachingCompile'
    ])

    /**
     *  @document This directive allows a custom edit form to be embedded within a page. The 'form-control' and 'form-data' attributes are the names of scope variable containing the
     *  resources. (The caller must fetch or create the resources.). 'form-mode' is the name of a scope variable containing either 'edit', 'view'
     *  If 'action-panel-file' is set to a valid template file, that file is included to the right of the title.
     *  All the arguments are dynamically watched.
     */
        .directive('spCustomEditForm', function ($rootScope, $timeout, spMobileContext, spCachingCompile) {
            var isMobile = spMobileContext.isMobile;
            var templateUrl = isMobile ? 'editForm/partials/customEditFormMobile.tpl.html' : 'editForm/partials/customEditForm.tpl.html';

            return {
                restrict: 'E',
                scope: {
                    formControl: '=',
                    parentControl: '=?',
                    formData: '=',
                    formTheme: '=?',
                    formMode: '=?',
                    actionPanelFile: '=?',
                    actionPanelOptions: '=?',
                    isInTestMode: '=?',
                    isInDesign: '=?',
                    isEmbedded: '=?',
                    pagerOptions: '=?'                // only used in mobile
                },
                link: function (scope, element) {

                    scope.$on('gather', function (event, callback) {
                        var formControl = scope.formControl;
                        if (isMobile) {
                            formControl = sp.result(scope.pagerOptions, ['pages', 0, 'scope', 'pageFormControl']) || formControl;
                        }

                        callback(formControl, scope.parentControl, element);
                    });

                    var cachedLinkFunc = spCachingCompile.compile(templateUrl);
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });

                    // 
                    // Workaround to deal with the android keyboard showing and covering the field being edited.
                    //
                    element.on('focusin', 'input, textarea', function (event) {
                        if (spMobileContext.isMobile && navigator.userAgent.indexOf('Android') > -1) {
                            var target = event.currentTarget;

                            $timeout(function () {
                                target.scrollIntoViewIfNeeded();
                            }, 1000);
                        }
                    });
                }
            };
        });
}());