// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spHeaderColumnContainerControl', ['mod.common.spCachingCompile'])
        .directive('spHeaderColumnContainerControl', function (spCachingCompile) {

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
                link: function (scope, element) {

                    scope.$on('gather', function (event, callback) {

                        var options = {
                            contentClass: '.contents'
                        };

                        callback(scope.formControl, scope.parentControl, element, options);
                    });


                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spHeaderColumnContainerControl/spHeaderColumnContainerControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());