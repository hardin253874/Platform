// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spHorizontalStackContainerControl', ['mod.common.spCachingCompile'])
        .directive('spHorizontalStackContainerControl', function (spCachingCompile) {

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
                    /////
                    // Finds the closest descendents with the specified selector.
                    /////
                    function closestDescendants(source, selector) {
                        var children;

                        if (!source || source.length === 0 || !selector) {
                            return undefined;
                        }

                        children = source.children(selector);

                        if (children.length > 0) {
                            return children;
                        }

                        children = source.children();

                        return closestDescendants(children, selector);
                    }

                    scope.$on('gather', function (event, callback) {
                        callback(scope.formControl, scope.parentControl, element);
                    });


                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spHorizontalStackContainerControl/spHorizontalStackContainerControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());