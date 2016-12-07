// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a dateAndTime config control.
    * spDateAndTimeConfigControl displays and modifies datetime values. It can be bound to a js date object or a string
    *
    * @module spDateAndTimeConfigControl
    * @example

    Using the spDateAndTimeConfigControl:

    &lt;sp-date-config-control model="myModel"&gt;&lt;/sp-date-config-control&gt

    where myModel is available on the scope with the following members:

    Properties:
        - value {date/ string}      - The current value. The value is either a valid date object or a string 'NOW'.
            default: 0
        - prefix {string}           - (optional) A string placed before the value in both read-only and modify state.
            default: ''
        - suffix {string}           - (optional) A string placed after the value in both the read-only and modify states.
            default: ''
        - isInTestMode {boolean}    - (optional) A boolean indicating whether this control is currently in test mode or not.
            default: false
        - minimumValue {date}       - (optional) The lower-bound of the value.
            default: null
        - maximumValue {date}       - (optional) The lower-bound of the value.
            default: null
        
    */
    angular.module('app.controls.spDateAndTimeConfigControl', ['sp.common.fieldValidator','mod.common.spCachingCompile'])
        .directive('spDateAndTimeConfigControl', function (spEditForm, spFieldControlProvider, spFieldValidator, spCachingCompile) {

            ///
            // Directive structure.
            ///
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    formControl: '=?',
                    formData: '=?',
                    formMode: '=?',
                    isInTestMode: '=?',
                    isReadOnly: '=?'
                },
                link: function (scope, element) {
                    /////
                    // Convert the current edit form scope values into a generic model with
                    // no ties to edit form.
                    /////
                    scope.model = {
                        isReadOnly: scope.isReadOnly,
                        isInTestMode: scope.isInTestMode,
                        isValidValue: true
                    };

                    /////
                    // Watch for changes to the form control.
                    /////
                    scope.$watch("formControl", function () {
                        var fieldToRender;

                        if (scope.formControl) {

                            fieldToRender = scope.formControl.getFieldToRender();

                            if (fieldToRender.getMinDateTime) {
                                scope.model.minimumValue = fieldToRender.getMinDateTime();
                            }

                            if (fieldToRender.getMaxDateTime) {
                                scope.model.maximumValue = fieldToRender.getMaxDateTime();
                            }

                            if (fieldToRender.getIsRequired) {
                                scope.model.isRequired = fieldToRender.getIsRequired();
                            }

                            // $scope is passed twice here, once as a context and once as the scope to $watch etc
                            // It is done like this in preparation for moving to "components"
                            spFieldControlProvider(scope, scope);
                            
                            // set custom parser and validator
                            // hack: hardcoded to use dateField custom parser and validator (even though the provided field type is 'stringField')
                            var tempDateTimeField = spEntity.fromJSON(
                                {
                                    id: 'myDateTimefield',
                                    typeId: 'dateTimeField',
                                    minDateTime: jsonDate(scope.model.minimumValue),
                                    maxDateTime: jsonDate(scope.model.maximumValue),
                                    isOfType: [{
                                        name: 'Date Time Field',
                                        id: 'dateTimeField',
                                        alias: 'core:dateTimeField'
                                    }]
                                });

                            scope.model.isRequired = false; // * hardcoded to false
                            scope.model.customTypeParser = spFieldValidator.getCustomParser(tempDateTimeField, scope.customValidationMessages);
                            scope.model.customValidator = spFieldValidator.getCustomValidator(tempDateTimeField, scope.model.isRequired, scope.customValidationMessages);

                                
                            /////
                            // When the form data changes, update the model.
                            /////
                            var dataWatch = 'formData && formData.getField(' + fieldToRender.id() + ')';

                            scope.$watch(dataWatch, function (value) {
                                    
                                var parsedDateVal = null;
                                    
                                // expecting an Iso date string for a valid date. 
                                // parse using native date function as 'Globalize' doesn't parse a string as a valid date if it is not in the format specified for the browser (not even the date ISO string)
                                var tempDt = Date.parse(value); // this produces a number 
                                tempDt = new Date(tempDt);      // this produces a date

                                if (spUtils.isValidDate(tempDt)) {
                                    parsedDateVal = spUtils.parseDate(tempDt);
                                }

                                if (_.isDate(parsedDateVal) && _.isDate(scope.model.fieldValue) && parsedDateVal.getTime() === spUtils.parseDate(scope.model.fieldValue).getTime())
                                    return;

                                if (value === scope.model.fieldValue)
                                    return;
                                    

                                // set value
                                if (value === spUtils.strNow) {
                                    scope.model.fieldValue = value;
                                }
                                else if (parsedDateVal) {
                                    scope.model.fieldValue = parsedDateVal;
                                }
                                
                            });

                            ///
                            // When the fieldValue changes, update form data and model.value
                            ///
                            scope.$watch('model.fieldValue', function (newValue) {

                                chkAndUpdateFormData(newValue, fieldToRender);  // update formData

                                chkAndUpdateModelValue(newValue);   // update model.value
                            });

                            ///
                            // When the model.value changes, update fieldValue
                            ///
                            scope.$watch('model.value', function (newValue, oldValue) {
                                if (newValue === oldValue)
                                    return;

                                chkAndUpdateModelFieldValue(newValue);  // update model.fieldValue
                            });
                            
                            scope.$watch('model.customValidator', function (newValue, oldValue) {

                                if (newValue === oldValue)
                                    return;
                                scope.model.customValidator(scope.model.value);
                            });
                        }
                    });


                    ///
                    // check if values are same. if not then update formData with new value
                    ///
                    function chkAndUpdateFormData(newValue, fieldToRender) {
                        var formDataValue = scope.formData.getField(fieldToRender.id());
                        
                        // if both values are dates and are equal
                        if (formDataValue && _.isDate(newValue) && newValue.toISOString() === formDataValue) {
                            return;
                        }

                        if (newValue === formDataValue)
                            return;
                        
                        // set formData
                        if (_.isDate(newValue)) {
                            scope.formData.setField(fieldToRender.id(), newValue.toISOString(), spEntity.DataType.String);
                        }
                        else if (newValue || newValue === spUtils.strNow) {
                            scope.formData.setField(fieldToRender.id(), newValue, spEntity.DataType.String);
                        }
                        else {
                            scope.formData.setField(fieldToRender.id(), null, spEntity.DataType.String);
                        }
                    }

                    ///
                    // check if values are same. if not then update model.value with new value
                    ///
                    function chkAndUpdateModelValue(newValue) {
                        if (_.isDate(newValue) && _.isDate(scope.model.value) && newValue.getTime() === scope.model.value.getTime()) {
                            if (scope.model.useNow) {
                                scope.model.useNow = false;
                            }
                            return;
                        }

                        if (newValue === spUtils.strNow) {
                            scope.model.useNow = true;
                            scope.model.disableControl = true;
                        }
                        else {
                            scope.model.value = newValue;
                            scope.model.disableControl = false;
                        }
                    }

                    ///
                    // check if values are same. if not then update model.fieldValue with new value
                    ///
                    function chkAndUpdateModelFieldValue(newValue) {
                        if (_.isDate(newValue) && _.isDate(scope.model.fieldValue) && newValue.getTime() === scope.model.fieldValue.getTime())
                            return;

                        if (scope.model.fieldValue === scope.model.value)
                            return;

                        scope.model.fieldValue = scope.model.value;
                    }

                    ///
                    // Set value to 'NOW'
                    ///
                    scope.setNow = function () {
                        if (scope.model.useNow) {
                            scope.model.fieldValue = spUtils.strNow;
                            scope.model.disableControl = true;
                        } else {
                            // validate
                            var msgs = [];
                            validate(msgs);
                            if (msgs.length === 0) {
                                scope.model.fieldValue = scope.model.value;
                            }
                            scope.model.disableControl = false;
                        }
                    };
                    
                    // runs custom validations
                    function validate(msgs) {
                        var value;
                        if (scope.model.isValidValue) {
                            value = scope.model.value;
                        }
                        else {
                            value = 'invalid^invalid';  // hack: to show invalid format error msg
                        }

                        if (scope.model.customValidator) {
                            scope.model.customValidator(value, msgs);
                        }
                    }


                    var cachedLinkFunc = spCachingCompile.compile('controls/spDateAndTimeConfigControl/spDateAndTimeConfigControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());