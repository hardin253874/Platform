// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /////
    // A field title plus the mandatory and error markers.
    /////
    angular.module('mod.app.editForm.designerDirectives.spTitlePlusMarkers', ['mod.app.editForm.designerDirectives.spCustomValidationMessage', 'mod.common.spCachingCompile'])
        .directive('spTitlePlusMarkers', function (spCachingCompile) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    isReadOnly: '=?',
                    isRequired: '=?',
                    messages: '=?',
                    titleModel: '=?'
                },
                link: function(scope, element) {
                    var cachedLinkFunc = spCachingCompile.compile('editForm/directives/spTitlePlusMarkers/spTitlePlusMarkers.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());