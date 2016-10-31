// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a mandatory marker control.
    * spMandatoryMarker displays a visual indicator specifying
    * that the corresponding control is mandatory.
    *
    * @module spMandatoryMarker
    * @example

    Using the spMandatoryMarker:

    &lt;sp-mandatory-marker"&gt;&lt;/sp-mandatory-marker&gt
        
    */
    angular.module('app.controls.spMandatoryIndicator', ['app.controls', 'mod.common.spCachingCompile'])
        .directive('spMandatoryIndicator', function (spCachingCompile) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {                   
                },
                link: function (scope, element) {
                    var cachedLinkFunc = spCachingCompile.compile('controls/spMandatoryIndicator/spMandatoryIndicator.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());