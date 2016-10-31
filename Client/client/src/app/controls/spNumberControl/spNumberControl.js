// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /**
    * Module implementing a number control.
    * spNumberControl displays and modifies integer values.
    *
    * @module spNumberControl
    * @example

    Using the spNumberControl:

    &lt;sp-number-control model="myModel"&gt;&lt;/sp-number-control&gt

    where myModel is available on the scope with the following members:

    Properties:
        - value {number}            - The current value.
            default: 0
        - prefix {string}           - (optional) A string placed before the value in both read-only and modify state.
            default: ''
        - suffix {string}           - (optional) A string placed after the value in both the read-only and modify states.
            default: ''
        - isReadOnly {boolean}      - (optional) A boolean indicating whether this control is rendered as read-only or not.
            default: false
        - isRequired {boolean}      - (optional) True if this control requires a value; false otherwise.
            default: false
        
        - isInTestMode {boolean}    - (optional) A boolean indicating whether this control is currently in test mode or not.
            default: false
        - minimumValue {number}     - (optional) The lower-bound of the value.
            default: -1000000000
        - maximumValue {number}     - (optional) The lower-bound of the value.
            default: 1000000000

        
    */
    angular.module('app.controls.spNumberControl', ['app.controls', 'mod.common.spCachingCompile'])
        .directive('spNumberControl', function (spControlProvider, spCachingCompile) {

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
                    // Setup the control provider options.
                    /////
                    var options = {
                        typeParser: spUtils.parseFloat
                    };

                    scope.onClick = function (event) {
                        if (!event) return;

                        scope.$emit('spControlOnFormClick', event.target);
                    };

                    /////
                    // Run the control provider.
                    /////
                    spControlProvider(scope, options);

                    var cachedLinkFunc = spCachingCompile.compile('controls/spNumberControl/spNumberControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        })
        .directive('spNumberControlValidation', function ($parse, spControlValidationProvider) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'A',
                replace: false,
                transclude: false,
                scope: false,
                require: 'ngModel',
                link: function (scope, elm, attrs, ctrl) {

                    scope.validationModel = $parse(attrs.spNumberControlValidation)(scope);

                    /////
                    // Setup the provider options.
                    /////
                    var options = {
                        directiveName: 'spNumberControlValidation',
                        typeParser: spUtils.parseFloat,
                        absoluteMinimum: -1000000000,
                        absoluteMaximum: 1000000000
                    };

                    /////
                    // Run the provider.
                    /////
                    spControlValidationProvider(scope, ctrl, options);
                }
            };
        })
        .filter('spNumberControlFilter', function() {
            /////
            // Format the value.
            /////
            return function (value) {

                if (_.isNull(value) || _.isUndefined(value))
                    return '';
                
                value = parseFloat(value);

                if (value || value === 0) {
                    return Globalize.format(value, 'n0');
                } else {
                    return value;
                }
            };
        });
}());