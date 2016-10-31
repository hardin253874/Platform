// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a decimal control.
    * spDecimalControl displays and modifies decimal values.
    *
    * @module spDecimalControl
    * @example

    Using the spDecimalControl:

    &lt;sp-decimal-control model="myModel"&gt;&lt;/sp-decimal-control&gt

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
    angular.module('app.controls.spDecimalControl', ['mod.common.spCachingCompile'])
        .directive('spDecimalControl', function (spControlProvider, spCachingCompile) {

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
                        typeParser: spUtils.parseFloat,
                        onAfterProviderInit: onAfterProviderInit
                    };

                    scope.onClick = function (event) {
                        if (!event) return;

                        scope.$emit('spControlOnFormClick', event.target);
                    };

                    /////
                    // Determines whether a value is undefined or null.
                    /////
                    function isUndefinedOrNull(value) {
                        return _.isUndefined(value) || _.isNull(value) || _.isNaN(value);
                    }

                    /////
                    // Method that fires after the provider has initialized.
                    /////
                    function onAfterProviderInit() {

                        scope.validationModel._decimalPlaces = !isUndefinedOrNull(scope.model.decimalPlaces) ? scope.model.decimalPlaces : 3;
                        scope.validationModel.getDecimalPlaces = function () {
                            return scope.validationModel._decimalPlaces;
                        };

                        scope.$watch('model.decimalPlaces', function (newValue, oldValue) {
                            setValidationModelDecimalPlaces();
                        });
                        
                    }

                    /////
                    // Set the validation models decimal places.
                    /////
                    function setValidationModelDecimalPlaces() {
                        if (!isUndefinedOrNull(scope.model.decimalPlaces)) {
                            scope.validationModel._decimalPlaces = parseInt(scope.model.decimalPlaces, 10);
                        }
                    }                    

                    /////
                    // Invoke the provider.
                    /////
                    spControlProvider(scope, options);

                    var cachedLinkFunc = spCachingCompile.compile('controls/spDecimalControl/spDecimalControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        })
    .directive('spDecimalControlValidation', function ($parse, spControlValidationProvider) {

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

                scope.validationModel = $parse(attrs.spDecimalControlValidation)(scope);

                /////
                // Setup the provider options.
                /////
                var options = {
                    directiveName: 'spDecimalControlValidation',
                    typeParser: spUtils.parseFloat,
                    absoluteMinimum: -1000000000,
                    absoluteMaximum: 1000000000,
                    decimalPlaces: 3,
                    onValidate: onValidate,
                    onAfterProviderInit: onAfterProviderInit
                };

                /////
                // Sets up the watch for decimal places changing.
                /////
                function onAfterProviderInit() {

                }

                /////
                // Validates that the number of decimal places has not been exceeded.
                /////
                function onValidate(value, messages) {
                    var decimalPlaces;
                    var periodLocation;
                    var stringValue;

                    if (value || value === 0) {
                        stringValue = value.toString();

                        if (scope.validationModel.getDecimalPlaces)
                            decimalPlaces = scope.validationModel.getDecimalPlaces();
                        else
                            decimalPlaces = options.decimalPlaces;

                        if (decimalPlaces || decimalPlaces === 0) {
                            periodLocation = stringValue.indexOf('.');

                            if (periodLocation >= 0 && stringValue.length - periodLocation - 1 > decimalPlaces) {

                                messages.push('The maximum number of decimal places has been exceeded.');
                                return false;
                            }
                        }
                    }

                    return true;
                }

                /////
                // Run the provider.
                /////
                spControlValidationProvider(scope, ctrl, options);
            }
        };
    })
        .filter('spDecimalControlFilter', function () {
            /////
            // Format the value.
            /////
            return function (value, decimalPlaces) {
                
                if (_.isNull(value) || _.isUndefined(value))
                    return '';
                
                value = parseFloat(value);

                if (decimalPlaces || decimalPlaces === 0) {

                    if (decimalPlaces < 0)
                        decimalPlaces = 0;
                    else if (decimalPlaces > 10)
                        decimalPlaces = 10;
                } else {
                    decimalPlaces = 3;
                }

                return Globalize.format(value, 'n' + decimalPlaces);
            };
        });
}());