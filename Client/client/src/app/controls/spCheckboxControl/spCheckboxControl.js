// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a checkbox control.
    * spCheckboxControl displays and modifies currency values.
    *
    * @module spCheckboxControl
    * @example

    Using the spCheckboxControl:

    &lt;sp-checkbox-control model="myModel"&gt;&lt;/sp-checkbox-control&gt

    where myModel is available on the scope with the following members:

    Properties:
        - value {boolean}           - The current value.
            default: false
        - label {string}            - The label associated with the checkbox.
            default: ''
        - prefix {string}           - (optional) A string placed before the value in both read-only and modify state.
            default: ''
        - suffix {string}           - (optional) A string placed after the value in both the read-only and modify states.
            default: ''
        - isReadOnly {boolean}      - (optional) A boolean indicating whether this control is rendered as read-only or not.
            default: false
        - isRequired {boolean}      - (optional) 
            default: false
        - isInTestMode {boolean}    - (optional) A boolean indicating whether this control is currently in test mode or not.
            default: false
        
    */
    angular.module('app.controls.spCheckboxControl', ['mod.common.spCachingCompile'])
        .directive('spCheckboxControl', function (spControlProvider, spCachingCompile) {

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
                link: function ($scope, element) {

                    /////
                    // Parses the value as a boolean.
                    /////
                    function parseBool(value) {
                        return !!value;
                    }

                    /////
                    // Setup the provider options.
                    /////
                    var options = {
                        typeParser: parseBool
                    };

                    /////
                    // Invoke the provider.
                    /////
                    spControlProvider($scope, options);

                    var cachedLinkFunc = spCachingCompile.compile('controls/spCheckboxControl/spCheckboxControl.tpl.html');
                    cachedLinkFunc($scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());