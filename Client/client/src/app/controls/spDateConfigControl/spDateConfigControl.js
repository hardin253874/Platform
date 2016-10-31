// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

   ///// This control is only expected to work with a 'stringField' field type.
    // The spDateConfigControl directive provides a bridge between
    // the edit form scope and the isolated render control, mapping the
    // relevant properties from one to the other.
    /////
    angular.module('app.controls.spDateConfigControl', ['sp.common.fieldValidator', 'mod.common.spCachingCompile'])
        .directive('spDateConfigControl', function (spEditForm, spFieldControlProvider, spFieldValidator, spCachingCompile) {

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
                    scope.internalDateUpdate = false;
                    
                    /////
                    // Convert the current edit form scope values into a generic model with
                    // no ties to edit form.
                    /////
                    scope.model = {
                        isReadOnly: scope.isReadOnly,
                        isInTestMode: scope.isInTestMode,
                        supressErrMsgs: true,   // this indicates that this control will do its custom validation. Default custom parser for date control should not produce and display error msg
                        isValidValue: true
                    };
                    
                    /////
                    // Watch for changes to the form control.
                    /////
                    scope.$watch("formControl", function () {
                        var fieldToRender;

                        if (scope.formControl) {

                            fieldToRender = scope.formControl.getFieldToRender();

                            if (fieldToRender.getMinDate) {
                                scope.model.minimumValue = fieldToRender.getMinDate();
                            }

                            if (fieldToRender.getMaxDate) {
                                scope.model.maximumValue = fieldToRender.getMaxDate();
                            }

                            spFieldControlProvider(scope, scope.model, scope.formControl);

                            // set custom parser and validator
                            // hack: hardcoded to use dateField custom parser and validator (even though the provided field type is 'stringField')
                            var tempDateField = spEntity.fromJSON(
                                {
                                    id: 'myDatefield',
                                    typeId: 'dateField',
                                    minDate: jsonDate(scope.model.minimumValue),
                                    maxDate: jsonDate(scope.model.maximumValue),
                                    isOfType: [{
                                        name: 'Date Field',
                                        id: 'dateField',
                                        alias: 'core:dateField'
                                    }]
                                });
                            
                            scope.model.isRequired = false; // * hardcoded to false
                            scope.model.customTypeParser = spFieldValidator.getCustomParser(tempDateField, scope.customValidationMessages);
                            scope.model.customValidator = spFieldValidator.getCustomValidator(tempDateField, scope.model.isRequired, scope.customValidationMessages);
                            
                            

                            scope.$watch('model.isValidValue', function (newValue, oldValue) {
                                if (newValue === oldValue)
                                    return;
                                
                                var msgs = [];
                                validate(msgs);
                            });
                            
                                
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
                                    
                                // check for equality
                                if (parsedDateVal) {
                                    if (_.isDate(scope.model.fieldValue) && parsedDateVal.getTime() === spUtils.translateToUtc(spUtils.parseDate(scope.model.fieldValue)).getTime())
                                        return;
                                }
                                    
                                if (value === scope.model.fieldValue)
                                    return;
                                    
                                // set value
                                if (value === spUtils.strToday) {
                                    scope.model.fieldValue = value; 
                                }
                                else if (parsedDateVal) {
                                    scope.model.fieldValue = spUtils.translateToLocal(parsedDateVal);
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
                                //console.log('model value changed: ' + scope.model.value);
                                if (scope.internalDateUpdate) {
                                    scope.internalDateUpdate = false;
                                    return;
                                }
                                
                                if (newValue === oldValue)
                                    return;
                                
                                // validate
                                var msgs = [];
                                validate(msgs);
                                if (msgs.length === 0) {
                                    chkAndUpdateModelFieldValue(newValue);  // update model.fieldValue
                                }
                            });
                        }
                    });
                    
                    ///
                    // check if values are same. if not then update formData with new value
                    ///
                    function chkAndUpdateFormData(newValue, fieldToRender) {
                        var formDataValue = scope.formData.getField(fieldToRender.id());
                        
                        // if both values are dates and are equal
                        if (formDataValue && _.isDate(newValue) && spUtils.translateToUtc(spUtils.parseDate(newValue)).toISOString() === formDataValue) {
                            return;
                        }
                        
                        if (newValue === formDataValue)
                            return;
                        
                        // set formData
                        if (_.isDate(newValue)) {
                            var utcValue = spUtils.translateToUtc(spUtils.parseDate(newValue));
                            scope.formData.setField(fieldToRender.id(), utcValue.toISOString(), spEntity.DataType.String);
                        }
                        else if (newValue || newValue === spUtils.strToday) {
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
                        if (!newValue && !scope.model.value)    // this is to cover if one is 'null' and the other one in 'undefined'.
                            return;

                        if (_.isDate(newValue) && _.isDate(scope.model.value) && newValue.getTime() === scope.model.value.getTime()) {
                            if (scope.model.useToday) {
                                scope.model.useToday = false;
                            }
                            return;
                        }

                        if (newValue === spUtils.strToday) {
                            scope.model.useToday = true;
                        }
                        else {
                            scope.model.value = newValue;
                        }
                        
                        scope.internalDateUpdate = true;
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
                    // When the model.useToday changes, update model.disableControl
                    ///
                    scope.$watch('model.useToday', function (newValue, oldValue) {
                        if (newValue === oldValue)
                            return;
                        
                        if(newValue) {
                            scope.model.disableControl = true;
                        }
                        else {
                            scope.model.disableControl = false;
                        }
                    });
                    
                    ///
                    // Set value to 'TODAY'
                    ///
                    scope.setToday = function () {
                        if (scope.model.useToday) {
                            scope.model.fieldValue = spUtils.strToday;
                        } else {
                            // validate
                            var msgs = [];
                            validate(msgs);
                            if(msgs.length === 0) {
                                scope.model.fieldValue = scope.model.value;
                            }
                        }
                    };
                    
                    // runs custom validations
                    function validate(msgs) {
                        var value;
                        if (scope.model.isValidValue) {
                            value = scope.model.value;
                        }
                        else {
                            value = 'invalid';  // hack: to show invalid format error msg
                        }
                        
                        if (scope.model.customValidator) {
                            scope.model.customValidator(value, msgs);
                        }
                    }

                    var cachedLinkFunc = spCachingCompile.compile('controls/spDateConfigControl/spDateConfigControl.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());