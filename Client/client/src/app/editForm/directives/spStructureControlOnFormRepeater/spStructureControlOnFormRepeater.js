// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /*
    * This should be removed however due to an issue with angular, replacing nodes within
    * a transcluded node is problematic. Wrapping the replacement in a <div> solves the problem
    * but defeats the purpose of replacing the element in the first place.
    */
    angular.module('mod.app.editForm.designerDirectives.spStructureControlOnFormRepeater', ['mod.common.spCachingCompile'])
        .directive('spStructureControlOnFormRepeater', function (spCachingCompile) {

            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: false,
                link: function(scope, element) {
                    var cachedLinkFunc = spCachingCompile.compile('editForm/partials/structureControlOnForm_repeater.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());