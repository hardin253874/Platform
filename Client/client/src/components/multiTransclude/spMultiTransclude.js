// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a multiple transclusion directive.
    * spMultiTransclude provides the ability to transclude multiple elements within the one template.
    *
    * @module spMultiTransclude
    * @example

    Using the spMultiTransclude:

    &lt;div sp-multi-transclude='first'&gt;&lt;/div&gt;&lt;div sp-multi-transclude='first'&gt;&lt;/div&gt;

    where 'first', 'second' etc are classes that match the elements to transclude.
        
    */
    angular.module('mod.common.ui.spMultiTransclude', [])
        .directive('spMultiTransclude', function () {
            return {
                controller: ['$scope', '$transclude', '$element', '$attrs', function ($scope, $transclude, $element, $attrs) {
                    $transclude($scope.$parent, function (clone, scope) {

                        for (var index = 0; index < clone.length; index++) {

                            var child = angular.element(clone[index]);

                            if (child.hasClass($attrs.spMultiTransclude)) {

                                $element.replaceWith(child);
                            }
                        }
                    });
                }]
            };
        });
}());