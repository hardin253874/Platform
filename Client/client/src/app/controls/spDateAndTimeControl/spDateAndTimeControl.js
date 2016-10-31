// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

// WORK IN PROGRESS

(function () {
    'use strict';

    /**
    * Module implementing a date control.
    * spDateAndTimeControl displays and modifies date values.
    *
    * @module spDateAndTimeControl
    * @example

    Using the spDateAndTimeControl:

    &lt;sp-date-control model="myModel"&gt;&lt;/sp-date-control&gt

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
        
    Note: Dates do not respect timezones, and are not intended to.
    */
    angular.module('app.controls.spDateAndTimeControl', [
        'ngLocale', 'mod.common.ui.spDialogService',
        'sp.common.fieldValidator', 'mod.common.spCachingCompile', 'mod.common.alerts'
    ])
        .directive('spDateAndTimeControl', function (spDialogService, spControlProvider, spCachingCompile,
                                                     spAlertsService) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                //require: 'ngModel',
                scope: {
                    model: '=?'
                },
                link: function (scope, element) {
                    
                    scope.internalDateTimeValue = undefined;
                    scope.internalDateUpdate = false;
                    scope.internalTimeUpdate = false;

                    /////
                    // Setup the provider options.
                    /////
                    var options = {
                        //typeParser: spUtils.parseDate
                    };

                    /////
                    // Invoke the provider.
                    /////
                    spControlProvider(scope, options);
                    
                    // todo: add a default validator for dateAndTime control if used outside of edit form
                    
                    // model for date control
                    scope.dateCtrlModel = { value: undefined, disableControl: false, supressErrMsgs: true, isValidValue: true, isInlineEditing: sp.result(scope, 'model.isInlineEditing') };

                    // model for time control
                    scope.timeCtrlModel = { value: undefined, disableControl: true, supressErrMsgs: true, isValidValue: true, isInlineEditing: sp.result(scope, 'model.isInlineEditing') };
                    
                    ///
                    // watches
                    ///
                    scope.$watch('model.minimumValue', function () {
                        scope.dateCtrlModel.minimumValue = scope.model.minimumValue;
                    });
                    
                    scope.$watch('model.maximumValue', function () {
                        scope.dateCtrlModel.maximumValue = scope.model.maximumValue;
                    });
                    
                    scope.$watch('model.value', function () {
                        
                        if (_.isDate(scope.model.value) && _.isDate(scope.internalDateTimeValue) && scope.model.value.getTime() === scope.internalDateTimeValue.getTime())
                            return;
                        
                        if (scope.model.value === scope.internalDateTimeValue)
                            return;

                        scope.internalDateTimeValue = scope.model.value;
                    });
                    
                    scope.$watch('internalDateTimeValue', function () {
                        // sync
                        updateModelValue();
                        
                        updateDateCtrlModelValue();
                        
                        updateTimeCtrlModelValue();
                    });
                    
                    scope.$watch('dateCtrlModel.value', function (newVal, oldVal) {
                        if (scope.internalDateUpdate) {
                            scope.internalDateUpdate = false;
                            return;
                        }
                        
                        if (newVal === oldVal)
                            return;

                        var tempDate;
                        var msgs = [];

                        if (newVal) {
                            
                            // valid date value, invalid time value ( set time value to 12 am)
                            if (!scope.timeCtrlModel.isValidValue) {
                                tempDate = new Date(scope.dateCtrlModel.value.getFullYear(), scope.dateCtrlModel.value.getMonth(), scope.dateCtrlModel.value.getDate(), 0, 0, 0, 0); // combine date and time parts
                            }
                            else {
                                tempDate = getDateTimeValueOnDateValueChange();
                            }

                            scope.timeCtrlModel.disableControl = false;
                        }
                        else {
                            // if new value is null then make the time value null too
                            tempDate = null;
                            scope.timeCtrlModel.disableControl = true;
                        }
                        
                        // validate
                        validate(tempDate, msgs);
                        if(msgs.length === 0 || !tempDate) {
                            scope.internalDateTimeValue = tempDate;
                        }
                        console.log('date value valid: ' + scope.dateCtrlModel.isValidValue);
                        console.log('update from DATE view');
                        
                    });
                    
                    scope.$watch('timeCtrlModel.value', function (newVal, oldVal) {
                        
                        if (scope.internalTimeUpdate) {
                            scope.internalTimeUpdate = false;
                            return;
                        }
                        
                        if (newVal === oldVal)
                            return;

                        var newTimeModelVal = newVal ? spUtils.translateFromServerStorageDateTime(newVal) : newVal;

                        
                        var tempDate;

                        if (newTimeModelVal) {

                            // valid time value, invalid date value
                            if (!scope.dateCtrlModel.isValidValue) {
                                return; // ** skip ** validation and updating internal dateTime value (hold valid time value internally)
                            }
                            
                            if(_.isDate(scope.dateCtrlModel.value)) {
                                tempDate = new Date(scope.dateCtrlModel.value.getFullYear(), scope.dateCtrlModel.value.getMonth(), scope.dateCtrlModel.value.getDate(), newTimeModelVal.getHours(), newTimeModelVal.getMinutes(), 0, 0); // combine date and time parts
                            }
                            else {
                                // this should not happen. if dateValue is not a valid date then time control should be disabled
                                raiseErrorAlert('unexpected error in time and date control');
                            }
                        }
                        else if (scope.dateCtrlModel.isValidValue && _.isDate(scope.dateCtrlModel.value)) {
                            // if time value is null but date is not null then set time value to 12 am
                            tempDate = new Date(scope.dateCtrlModel.value.getFullYear(), scope.dateCtrlModel.value.getMonth(), scope.dateCtrlModel.value.getDate(), 0, 0, 0, 0); // combine date and time parts
                        }
                        
                        // validate
                        var msgs = [];
                        validate(tempDate, msgs);
                        if (msgs.length === 0) {
                            scope.internalDateTimeValue = tempDate;
                        }
                        
                        console.log('update from TIME view');
                        
                    });
                    
                    scope.$watch('dateCtrlModel.isValidValue', function (newValue, oldValue) {
                        if (newValue === oldValue)
                            return;
                        console.log('isValidValue changed');
                        
                        var msgs = [];
                        
                        if(newValue) {
                            // todo : revalidate using the custom validator to hook into the custom error msgs
                            
                            if (scope.dateCtrlModel.value) {
                                scope.timeCtrlModel.disableControl = false; // enable the time control
                            }
                            
                            // valid date value, invalid time value
                            if (!scope.timeCtrlModel.isValidValue) {
                                //return; // ** skip ** validation and updating internal dateTime value (hold valid date value internally)
                                scope.timeCtrlModel.disableControl = false;
                                var val = Globalize.format(scope.dateCtrlModel.value, 'd') + '^invalid';    // hack:
                                validate(val, msgs);
                                return; // ** skip ** validation and updating internal dateTime value (hold valid date value internally)
                            }

                            var tempDateTime = getDateTimeValueOnDateValueChange();
                            validate(tempDateTime, msgs);  
                        }
                        else {
                            scope.timeCtrlModel.disableControl = true;
                            validate('invalid^invalid', msgs);  // hack:
                        }
                    });
                    
                    scope.$watch('timeCtrlModel.isValidValue', function (newValue, oldValue) {
                        if (newValue === oldValue)
                            return;
                        console.log('Time model : isValidValue changed');
                        
                        if (!scope.dateCtrlModel.isValidValue) {
                            return; // ** skip ** validation and updating internal dateTime value (hold valid time value internally)
                        }

                        var msgs = [];

                        if (newValue) {
                            // enable the time control
                            //scope.timeCtrlModel.disableControl = false;
                            // valid date value, invalid time value
                            if (_.isDate(scope.dateCtrlModel.value) && _.isDate(scope.timeCtrlModel.value)) {
                                var tempDateTime = getDateTimeValueOnDateValueChange();
                                validate(tempDateTime, msgs);
                            }
                        }
                        else {
                            if (_.isDate(scope.dateCtrlModel.value)) {
                                var val = Globalize.format(scope.dateCtrlModel.value, 'd') + '^invalid';    // hack:
                                validate(val, msgs);  
                            }
                        }
                    });
                    
                    scope.$watch('model.disableControl', function (newValue, oldValue) {
                        if (newValue === oldValue)
                            return;
                        
                        scope.dateCtrlModel.disableControl = newValue;
                        scope.timeCtrlModel.disableControl = newValue;
                    });
                    
                    if (sp.result(scope, 'model.isInlineEditing')) {
                        scope.$watch('model.isReadOnly', function(isReadOnly) {
                            scope.dateCtrlModel.isReadOnly = isReadOnly;
                            scope.timeCtrlModel.isReadOnly = isReadOnly;
                        });
                    }

                    ///
                    // helpers
                    ///
                    // runs custom validations
                    function validate(value, msgs) {
                        var customValidator = scope.validationModel.getCustomValidator();
                        if(customValidator) {
                            customValidator(value, msgs);
                        }
                    }
                    
                    function getDateTimeValueOnDateValueChange() {
                        var tempDate;
                        var datePart = scope.dateCtrlModel.value;
                        var timePart = _.isDate(scope.timeCtrlModel.value)? spUtils.translateFromServerStorageDateTime(scope.timeCtrlModel.value): null;

                        // if date is null then return null
                        if (!_.isDate(datePart)) {
                            return null;
                        }

                        // valid date value, valid time value. And time value not null
                        if (_.isDate(timePart)) {
                            tempDate = new Date(datePart.getFullYear(), datePart.getMonth(), datePart.getDate(), timePart.getHours(), timePart.getMinutes(), 0, 0); // combine date and time parts
                        }
                        else {  // valid date value, valid time value. And time value is null
                            tempDate = new Date(datePart.getFullYear(), datePart.getMonth(), datePart.getDate(), 0, 0, 0, 0); // set time part to 12am
                        }
                        return tempDate;
                    }
                    
                    function updateModelValue() {
                        if (_.isDate(scope.model.value) && _.isDate(scope.internalDateTimeValue) && scope.model.value.getTime() === scope.internalDateTimeValue.getTime())
                            return;
                        
                        if (scope.model.value === scope.internalDateTimeValue)
                            return;
                        
                        scope.model.value = scope.internalDateTimeValue;
                    }
                    
                    function updateDateCtrlModelValue() {
                        if (dateComponentsAreEqual(scope.internalDateTimeValue, scope.dateCtrlModel.value)) {
                            return;
                        }
                        
                        if (_.isDate(scope.internalDateTimeValue)) {
                            scope.internalDateUpdate = true;
                            scope.dateCtrlModel.value = new Date(scope.internalDateTimeValue.getFullYear(), scope.internalDateTimeValue.getMonth(), scope.internalDateTimeValue.getDate(), 0, 0, 0, 0);
                            return;
                        }

                        if (scope.dateCtrlModel.value === scope.internalDateTimeValue)
                            return;
                        
                        scope.internalDateUpdate = true;
                        scope.dateCtrlModel.value = scope.internalDateTimeValue;
                    }
                    
                    function updateTimeCtrlModelValue() {
                        if (timeComponentsAreEqual()) {
                            return;
                        }

                        if (_.isDate(scope.internalDateTimeValue)) {
                            scope.internalTimeUpdate = true;
                            scope.timeCtrlModel.value = spUtils.translateToServerStorageDateTime(scope.internalDateTimeValue);//new Date(1973, 0, 1, scope.internalDateTimeValue.getHours(), scope.internalDateTimeValue.getMinutes(), 0, 0);
                            
                            scope.timeCtrlModel.disableControl = false; // enable time control if value is a date
                            return;
                        }

                        scope.internalTimeUpdate = true;
                        scope.timeCtrlModel.value = scope.internalDateTimeValue;
                        scope.timeCtrlModel.disableControl = true; // disable time control if value is not a date
                    }
                    
                    function dateComponentsAreEqual(datetime1, datetime2) {
                        if (_.isDate(datetime1) && _.isDate(datetime2)) {
                            var tempDt1 = new Date(datetime1.getFullYear(), datetime1.getMonth(), datetime1.getDate(), 0, 0, 0, 0);
                            var tempDt2 = new Date(datetime2.getFullYear(), datetime2.getMonth(), datetime2.getDate(), 0, 0, 0, 0);
                            return tempDt1.getTime() === tempDt2.getTime();
                        }

                        return false;
                    }
                    
                    function timeComponentsAreEqual() {
                        if (_.isDate(scope.internalDateTimeValue) && _.isDate(scope.timeCtrlModel.value)) {
                            return spUtils.translateToServerStorageDateTime(scope.internalDateTimeValue).getTime() === scope.timeCtrlModel.value.getTime();
                        }

                        return false;
                    }

                    var cachedLinkFunc = spCachingCompile.compile('controls/spDateAndTimeControl/spDateAndTimeControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };

            function raiseErrorAlert(message) {
                spAlertsService.addAlert(message, { severity: spAlertsService.sev.Error });
            }
        })
        .filter('spDateAndTimeControlFilter', function () {
            /////
            // Format the value.
            /////
            return function (value) {
                return spUtils.isNullOrUndefined(value) ? '' : Globalize.format(value, 'd') + ' ' + Globalize.format(value, 'h:mm tt');
            };
        });


}());