// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spImageFileNameUploadControl', ['mod.common.spCachingCompile'])
        .directive('spImageFileNameUploadControl', function (spCachingCompile) {

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
                link: function (scope, element) {
                    scope.$on('gather', function (event, callback) {
                        callback(scope.formControl, scope.parentControl, element);
                    });

                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spImageFileNameUploadControl/spImageFileNameUploadControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });

                }
            };

        });
}());