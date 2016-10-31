// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _ */
(function () {
    'use strict';

    /**
      * Module implementing a control provider.        
      *
      * @module spControlProvider
      * @example            
         
      Usage
     
      spControlProvider(scope, typeParser, minimum, maximum, defaultForma);
     
      where options is an object with the following properties:
         - typeParser(value) {function}                                         - Function that takes a value and returns it in it's native format.
         - minimum {?}                                                          - The minimum value allowed by the control.
         - maximum {?}                                                          - The maximum value allowed by the control.
         - defaultFormat(value, prefix, suffix, setValueCallback) {function}    - The minimum value allowed by the control.
         - replaceRegex {regex}                                                 - Regular Expression used to replace invalid characters with empty
                                                                                  strings during control change.
         - customValidationMessages                                             - array of messages
      */
    angular.module('app.controls.providers', ['mod.common.spMobile'])
    .provider('spControlProvider', function () {
        /////
        // spControlProvider
        /////
        this.$get = ['$rootScope', 'spMobileContext', function (rootScope, spMobileContext) {
            return function (scope, options) {
                var exports = {};

                if (!options)
                    options = {};

                if (options.onBeforeProviderInit)
                    options.onBeforeProviderInit();

                if (!options.typeParser)
                    options.typeParser = parseFloat;

                /////
                // Ensure there is a model.
                /////
                if (!scope.model)
                    scope.model = {};

                /////
                // Setup the validation model.
                /////
                scope.validationModel = {
                    getMinimum: function () { return scope.model.minimumValue; },
                    getMaximum: function () { return scope.model.maximumValue; },
                    getIsRequired: function () { return scope.model.isRequired; },
                    getIsInTestMode: function () { return scope.model.isInTestMode; },
                    getCustomValidator: function () { return scope.model.customValidator; },
                    getCustomFormatter: function () { return scope.model.customFormatter; },
                    getCustomTypeParser: function () { return scope.model.customTypeParser; },
                    getSupressErrMsgs: function () { return scope.model.supressErrMsgs; },
                    getIsValidValue: function () { return scope.model.isValidValue; }   // currently used in dateAndTime control 
                };

                /////
                // Sets up the validation model.
                /////
                function setValidationModel() {
                    if (options.setValidationModel)
                        options.setValidationModel();
                }

                /////
                // These values don't change on live systems.
                /////
                if (scope.model.isInTestMode) {

                    if (options.setWatchers)
                        options.setWatchers();
                }

                setValidationModel();

                if (options.onAfterProviderInit)
                    options.onAfterProviderInit();

                scope.isMobile = spMobileContext.isMobile;
                scope.isTablet = spMobileContext.isTablet;
                
                return exports;
            };
        }];
    })
    .provider('spControlValidationProvider', function () {
        /////
        // spControlProvider
        /////
        this.$get = function () {
            return function(scope, controller, options) {
                var exports = { };

                if (!options)
                    options = { };

                /////
                // Determines whether a value is undefined or null.
                /////

                function isUndefinedOrNull(value) {
                    return _.isUndefined(value) || _.isNull(value) || _.isNaN(value);
                }

                /////
                // Appends a message to the array of messages.
                /////

                function addMessage(messages, newMessage) {

                    if (!isUndefinedOrNull(messages) && newMessage) {
                        messages.push(newMessage);
                    }
                }

                /////
                // Validate the control value.
                /////

                function defaultValidator(value) {
                    var messages = [];
                    var valid = true;
                    var minimum;
                    var maximum;

                    if (scope.validationModel.getMinimum)
                        minimum = scope.validationModel.getMinimum();
                    else if (options.absoluteMinimum)
                        minimum = options.absoluteMinimum;

                    if (scope.validationModel.getMaximum)
                        maximum = scope.validationModel.getMaximum();
                    else if (options.absoluteMaximum)
                        maximum = options.absoluteMaximum;

                    /////
                    // TODO: Need to localize these messages.
                    /////
                    if (isUndefinedOrNull(value) && scope.validationModel.getIsRequired()) {
                        addMessage(messages, options.isRequriedMessage || 'This field is required.');
                        valid = false;
                    }

                    if (!isUndefinedOrNull(minimum) && value < minimum) {
                        addMessage(messages, options.minimumMessage || 'The value is less than the minimum allowed.');
                        valid = false;
                    }

                    if (!isUndefinedOrNull(maximum) && value > maximum) {
                        addMessage(messages, options.maximumMessage || 'The value exceeds than the maximum allowed.');
                        valid = false;
                    }

                    if (options.onValidate) {
                        if (!options.onValidate(value, messages)) {
                            valid = false;
                        }
                    }

                    return messages;
                }

                //function validate(value, messages) {
                //    messages.map(.var
                //    messages = defaultValidator(value);
                //    scope.validationModel.message = messages.join(' - ');
                //    controller.$setValidity(options.directiveName, messages.length === 0);
                //}

                /////
                // DOM -> Model
                /////
                var customValidator = scope.validationModel.getCustomValidator();
                var selectedValidator = customValidator ? customValidator : defaultValidator;

                var customTypeParser = scope.validationModel.getCustomTypeParser();
                var selectedParser = customTypeParser ? customTypeParser : options.typeParser;

                var customFormatter = scope.validationModel.getCustomFormatter();
                
                //if (customValidator) {
                //    // the custom validator handles both the type parsing and validation.
                //    selectedValidator = function(value) {
                //        var messages = customValidator(value);
                //        controller.$setValidity(options.directiveName, messages.length === 0);

                //        return value;
                //    };
                //} else {
                // NOTE, this is a probematic design. The type parser runs before the other parser. This means for integer parsing if the string contains invalid characters after valid ones they are ignored
                // It really should be split into a pre validator, parser and additional parser, which makes it a bit overly complex.

                var customParserFunc = function (viewvalue) {
                    var messages = [];
                    var supressErrMsgs = scope.validationModel.getSupressErrMsgs();
                    var typedValue = selectedParser(viewvalue, messages);

                    if (!supressErrMsgs) { // skip validation here as it will be done in the parent control. **hack: to make date only and time only to work in DateAndTime, dateConfig, dateAndTimeConfig control. 

                        if (messages.length === 0) {
                            messages = selectedValidator(typedValue, messages);
                        }
                    }

                    if (messages.length === 0) {
                        scope.model.isValidValue = true;
                    } else {
                        scope.model.isValidValue = false;
                    }

                    //if (!customTypeParser && !customValidator) {
                    //    scope.validationModel.message = messages.join(' - '); // only show error msg in tooltip if default parser is used don't show err msgs in tooltip
                    //}

                    controller.$setValidity(options.directiveName, messages.length === 0);

                    return typedValue;
                };

                // in case of mobile/tablet, for date only and time only fields, make custom parser as the last one to run 
                // (default date parser for inputtype=date expects a valid ISO-8601 date format (yyyy-MM-dd) string, for example: "2009-01-06"). Our custom parser returns a date object in case of mobile which fails a regex test in angular1.3.
                if ((scope.isMobile || scope.isTablet) && (options.directiveName && (options.directiveName === 'spDateControlValidation' || options.directiveName === 'spTimeControlValidation'))) {
                    controller.$parsers.push(customParserFunc);
                } else {
                    controller.$parsers.unshift(customParserFunc);
                }

                
                
                if (customFormatter) {
                    controller.$formatters.push(customFormatter);
                }

                //controller.$parsers.push(selectedValidator);

                /////
                // These values don't change on live systems.
                /////
                if (scope.validationModel.getIsInTestMode()) {

                    /////
                    // When the minimum value changes, validate.
                    /////
                    //scope.$watch('validationModel.getMinimum()', function (newValue, oldValue) {

                    //    if (newValue === oldValue)
                    //        return;

                    //    validate(options.typeParser(controller.$viewValue));
                    //});

                    /////
                    // When the maximum value changes, validate.
                    /////
                    //scope.$watch('validationModel.getMaximum()', function (newValue, oldValue) {

                    //    if (newValue === oldValue)
                    //        return;

                    //    validate(options.typeParser(controller.$viewValue));
                    //});
                    
                    /////
                    // When the maximum value changes, validate.
                    /////
                    scope.$watch('validationModel.getCustomValidator()', function (newValue, oldValue) {

                        if (newValue === oldValue)
                            return;
                         customValidator = scope.validationModel.getCustomValidator();
                         selectedValidator = customValidator ? customValidator : defaultValidator;
                         var messages = [];
                         selectedValidator(controller.$viewValue, messages);
                        
                         controller.$setValidity(options.directiveName, messages.length === 0);
                    });
                }

                if (options.onAfterProviderInit)
                    options.onAfterProviderInit();

                return exports;
            };
        };
    });

}());