// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

// WORK IN PROGRESS

(function () {
    'use strict';

    /**
    * Module implementing a time control.
    * spTimeControl displays and modifies time values.
    *
    * @module spTimeControl
    * @example

    Using the spTimeControl:

    &lt;sp-time-control model="myModel"&gt;&lt;/sp-time-control&gt

    where myModel is available on the scope with the following members:

    Properties:
        - value {date}              - The current value.
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
        - minimumValue {date}       - (optional) The lower-bound of the value.
            default: null
        - maximumValue {date}       - (optional) The lower-bound of the value.
            default: null
        
    Note: Time do not respect timezones, and are not intended to.
    */
    angular.module('app.controls.spTimeControl', ['ngLocale', 'mod.common.ui.spDialogService', 'sp.common.fieldValidator', 'mod.common.spCachingCompile', 'app.controls.spTimeControlPopup'])
        .directive('spTimeControl', function (spDialogService, spControlProvider, spCachingCompile) {

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
                   // var internalUpdate = false;
                    scope.internalModel = {
                        value: undefined,
                        supressErrMsgs : true // by default set to true: this indicates that this control will not validate. This control will only produce parsing errors if any. e.g. a parsing error would be : hrs value is null but mins have valid number
                    };

                    ///
                    // Setup the provider options.
                    ///
                    var options = {
                        //typeParser: spUtils.parseDate
                    };

                    if (scope.model) {

                        scope.model.supressErrMsgs = true; // by default set to true: this indicates that this control will not validate. This control will only produce parsing errors if any. e.g. a parsing error would be : hrs value is null but mins have valid number
                        scope.model.isValidValue = true;                        
                    }                                       

                    ///
                    // Invoke the provider.
                    ///
                    spControlProvider(scope, options);

                    // Setup the time picker popup options
                    scope.popupOptions = {
                        isOpen: false,
                        validationModel: scope.validationModel
                    };
                    
                    scope.$watch('model.supressErrMsgs', function () {
                        if (scope.model.supressErrMsgs) {
                            scope.internalModel.supressErrMsgs = scope.model.supressErrMsgs;
                        }
                    });

                    scope.$watch('model.value', updateInternalModelValue);
                    
                    scope.$watch('internalModel.value', function (internalValue, oldValue) {                                
                        if (oldValue === internalValue) {
                            return;    // ignore the initial value
                        }                        


                        var systemTime = spUtils.translateToServerStorageDateTime(scope.internalModel.value);

                        if (spUtils.compareTimeOnly(systemTime, scope.model.value)) {
                            return; // no change
                        }


                        // validate only if scope.model.supressErrMsgs is false
                        if (!scope.internalModel.supressErrMsgs) {
                            var msgs = [];

                            validate(systemTime, msgs);

                            if (msgs.length !== 0) {
                                return;
                            }
                        }
                        
                       
                        // if update from the view and not valid value (has parsing error) then skip updating model.value
                        if (scope.validationModel.getIsValidValue() === false) {
                            return;
                        }
                       
                        scope.model.value = systemTime;

                    });

                    if (sp.result(scope, 'model.isInlineEditing')) {
                        scope.$watch('model.isReadOnly', function(isReadOnly) {
                            if (isReadOnly) {
                                // Reset internal value to model value when switching control to view mode
                                updateInternalModelValue(scope.model.value);                                                                        
                            }
                        });
                    }

                    scope.timePickerButtonClick = function () {
                        // setup the model and display the time picker
                        scope.popupOptions.model = scope.internalModel;
                        scope.popupOptions.isOpen = !scope.popupOptions.isOpen;       
                    };

                    // runs custom validations
                    function validate(value, msgs) {
                        if (scope.model.customValidator) {
                            scope.model.customValidator(value, msgs);
                        }
                    }

                    function updateInternalModelValue(modelValue) {
                        var localTime = spUtils.translateFromServerStorageDateTime(modelValue);

                        if (spUtils.compareTimeOnly(localTime, scope.internalModel.value))
                            return;
                        
                        scope.internalModel.value = localTime;
                    }

                    var cachedLinkFunc = spCachingCompile.compile('controls/spTimeControl/spTimeControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        })
        .directive('spTimeControlFormatter', function () {
            return {
                restrict: 'A',                
                scope: false,
                require: 'ngModel',
                link: function (scope, elm, attrs, ctrl) {
                    ctrl.$formatters.unshift(function (modelValue) {
                        // Format the time for display
                        if (!modelValue) return '';
                        var date = new Date(modelValue);
                        return Globalize.format(date, 't');
                    });                    
                }
            };
        })
        .directive('spTimeControlValidation', function ($parse, spControlValidationProvider, spFieldValidator) {

            ///
            // Directive structure.
            ///
            return {
                restrict: 'A',
                replace: false,
                transclude: false,
                scope: false,
                require: 'ngModel',
                priority: 100,     // note: priority is set higher so that this directive runs last and we can inject our parser as the first element of '$parsers' array. (to make our parser run first).
                link: function (scope, elm, attrs, ctrl) {

                    scope.validationModel = $parse(attrs.spTimeControlValidation)(scope);

                    /////
                    // Setup the provider options.
                    /////
                    var options = {
                        directiveName: 'spTimeControlValidation',
                        typeParser: spFieldValidator.customTimeParser,//defaultTimeParser, //spUtils.parseDate,
                        absoluteMinimum: new Date('1900-01-01'),
                        absoluteMaximum: new Date('2100-01-01')
                    };                    


                    /////
                    // Run the provider.
                    /////
                    spControlValidationProvider(scope, ctrl, options);
                    
                    //function defaultTimeParser(value, messages) {
                    //    var dateValue = spUtils.parseDate(value);

                    //    if (_.isString(value) && value !== '' && !dateValue) {
                    //        messages.push('Invalid format, the date must be in the format ' + Globalize.culture().calendars.standard.patterns.d);
                    //    }
                        
                    //    return dateValue;
                    //}

                    
                    //};
                    /////
                    // DOM -> Model
                    /////
                    //ctrl.$parsers.unshift(parseAndValidate);
                    
                    //function parseAndValidate(viewValue) {
                    //    var messages = [];

                    //    if (scope.validationModel.getCustomValidator()) {
                    //        messages = scope.validationModel.getCustomValidator()(viewValue);
                    //    } else {
                    //        messages = spFieldValidator.validateTime(scope.validationModel.getMinimum(), scope.validationModel.getMaximum(), scope.validationModel.getIsRequired(), viewValue);
                    //        scope.validationModel.message = messages.join(' - ');
                    //    }
                    //    ctrl.$setValidity(options.directiveName, messages.length === 0);

                    //    return viewValue;//spUtils.parseDate(viewValue);    // default parser expects string to be in Us format. So parsing view value using globalize
                    //}
                }
            };
        })
        .filter('spTimeControlFilter', function () {
            /////
            // Format the value.
            /////
            return function (value) {

                return Globalize.format(value, 't');
            };
        });


}());