// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

// WORK IN PROGRESS

(function() {
    'use strict';

    /**
    * Module implementing a date control using the native input control
    *
    * @module spDateControl
    */
    angular.module('app.controls.spDateAndTimeMobileControl', ['ngLocale', 'sp.common.fieldValidator', 'mod.common.spCachingCompile'])
        .directive('spDateAndTimeMobileControl', function (spDialogService, spControlProvider, spCachingCompile) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    model: '=?'
                },                
                link: function (scope, element) {


                    /////
                    // Setup the provider options.
                    /////
                    var options = {
                        
                        //typeParser: defaultDateParser//spParseDate//spUtils.parseDate
                    };

                    /////
                    // Invoke the provider.
                    /////
                    spControlProvider(scope, options);

                    var cachedLinkFunc = spCachingCompile.compile('controls/spDateAndTimeMobileControl/spDateAndTimeMobileControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });        
       
    
}());