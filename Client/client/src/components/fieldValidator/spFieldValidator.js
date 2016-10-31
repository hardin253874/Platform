// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, sp, Globalize */

/**
 *  The spFieldValidator module...
 *  @file
 */
(function() {
    'use strict';

    angular.module('sp.common.fieldValidator', ['mod.common.spEntityService', 'mod.common.alerts']);

    /**
     * Module for validating fields containing text 
     * 
     * @module spFieldValidator
     * @example
    
      Using the validator directly:

        var myFieldQuery = "name, " + spFieldValidator.getFieldQueryFragment();
        spEntityService.getEntity('core:name', myFieldQuery).then( function (field) {
           var validator = spFieldValidator.getValidator(field);
           var validationMessages = validator("valid name");
           var isValid = validationMessages.length === 0;
        });
   
     Using the directive:

        <input ng-model='myValue' sp-validate-field='myField'/>
     
	
     * 
     */
    angular.module('sp.common.fieldValidator')
        .factory('spFieldValidator', ['spAlertsService', function(spAlertsService) {


            var exports = {};

            //
            // constants. Absolute maximum values for the fields.
            //
            var maxNumberFieldValue = 1000000000;
            var minNumberFieldValue = -1000000000;
            var maxSingleLineStringFieldSize = 1000;
            var maxMultilineStringFieldSize = 100000;

            //
            // Regexs for determining valid field text
            //
            var validIntegerRegex = /^\-?([0-9]+)$/;
            var validDecimalRegex = /^[+\-]?(\.\d+|\d+(\.\d+)?)$/;

            var sanitizeIntegerRegex = /[^\d-]/g;
            var sanitizeDecimalRegex = /[^\d-.]/g;

            exports.sanitizeDateRegex = /[^\d-/]/g;
            exports.sanitizeTimeRegex = /[^\d-:-A,M,P _]/g;

            /**
             * Get the entityQuery fragment that can be used to pull all the field validation information.
             * @returns {string} The query fragement
             */
            //TODO: THIS WILL BECOME OBSOLETE - SEE SH
            exports.getFieldQueryFragment = function() {
                return 'isOfType.{id, alias}, isRequired, allowMultiLines, pattern.{regex, regexDescription}, ' +
                    'minLength, maxLength, minInt, maxInt, minDecimal, maxDecimal, minDate, maxDate, minTime, maxTime, minDateTime, maxDateTime ';
            };


            /**
             *  Return a validation function that can be used to validate a resource field.
             * 
             * @param {object} field
             * @param {bool} isRequiredOnForm Optional 
             * @return {function} A function that takes a string and returns an array of error messages.
             */
            exports.getValidator = function(field, isRequiredOnForm) {
                var fieldType = field.getIsOfType()[0];

                switch (fieldType.alias()) {
                    case 'core:stringField':
                        return getStringFieldValidator(field);
                    case 'core:intField':
                        return getIntFieldValidator(field);
                    case 'core:dateField':
                        return getDateFieldValidator(field);
                    case 'core:timeField':
                        return getTimeFieldValidator(field);
                    case 'core:dateTimeField':
                        return getDateTimeFieldValidator(field);
                    case 'core:decimalField':
                    case 'core:currencyField':
                        return getDecimalFieldValidator(field);
                    default:
                        // return the do nothgin validator
                        return function() { return []; };
                }
            };
            
            /**
             *  Return a parser function that can be used to parse/validate a resource field.
             * 
             * @param {object} field
             * @return {function} A function that takes a string and returns an array of error messages.
             */
            exports.getParser = function (field) {
                var fieldType = field.getIsOfType()[0];

                switch (fieldType.alias()) {
                    case 'core:intField':
                        return getIntFieldParser();
                    case 'core:dateField':
                        return getDateFieldParser();
                    case 'core:timeField':
                        return getTimeFieldParser();
                    case 'core:decimalField':
                    case 'core:currencyField':
                        return getDecimalFieldParser();
                    default:
                        // return the do nothing parser
                        return function (value, returnedMessages) { return value; };
                }
            };

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

            /**
             *  Return a validation function that can be used to validate a date field.
             * 
             * @private
             * @param {object} field
             * @return {function} A function that takes a string and returns an array of error messages.
             */

            function getDateFieldValidator(field) {
                var min, max;
                var minUtc = field.getField('minDate');
                var maxUtc = field.getField('maxDate');
                
                if (_.isDate(minUtc)) {
                    min = spUtils.translateToLocal(minUtc);
                }
                
                if (_.isDate(maxUtc)) {
                    max = spUtils.translateToLocal(maxUtc);
                }
                
                var isRequired = field.getField('isRequired');
                // var hasDateRangeSet = _.isDate(min) && _.isDate(max);

                
                return function (text) {
                    return exports.validateDate(min, max, isRequired, text);
                    //var result = [];
                    //if (text) {

                    //    if (!isNaN(text) && _.isDate(text)) {
                    //        if (min && (text < min)) {
                    //            if (hasDateRangeSet) {
                    //                result.push('The value is out of range; the value must be between ' + Globalize.format(min, 'd') + ' and ' + Globalize.format(max, 'd') + ' inclusive');
                    //            } else {
                    //                result.push('The value less than the minimum value; the minimum value is ' + Globalize.format(min, 'd'));
                    //            }
                    //        }

                    //        if (max && (text > max)) {
                    //            if (hasDateRangeSet) {
                    //                result.push('The value is out of range; the value must be between ' + Globalize.format(min, 'd') + ' and ' + Globalize.format(max, 'd') + ' inclusive');
                    //            } else {
                    //                result.push('The value is out of range; the maximum value is ' + Globalize.format(max, 'd'));
                    //            }
                    //        }
                    //    } else {
                    //        result.push('Invalid format, the date must be in the format ' + Globalize.culture().calendars.standard.patterns.d);
                    //    }
                    //}
                    //return result;
                };
            }

            exports.validateDate = function (min, max, isRequired, value) {
                var hasDateRangeSet = _.isDate(min) && _.isDate(max);
                var result = [];
                if (value) {
                    var dateValue = spUtils.parseDate(value);
                    
                    if (_.isString(value) && value !== '' && !dateValue) {
                        result.push(spUtils.invalidDateMsgText());
                    }
                    else if (dateValue) {
                        if (min && (dateValue < min)) {
                            if (hasDateRangeSet) {
                                result.push('The value is out of range; the value must be between ' + Globalize.format(min, 'd') + ' and ' + Globalize.format(max, 'd') + ' inclusive');
                            } else {
                                result.push('The value less than the minimum value; the minimum value is ' + Globalize.format(min, 'd'));
                            }
                        }

                        if (max && (dateValue > max)) {
                            if (hasDateRangeSet) {
                                result.push('The value is out of range; the value must be between ' + Globalize.format(min, 'd') + ' and ' + Globalize.format(max, 'd') + ' inclusive');
                            } else {
                                result.push('The value is out of range; the maximum value is ' + Globalize.format(max, 'd'));
                            }
                        }
                    } 
                }
                return result;
            };


            /**
             *  Return a validation function that can be used to validate a time field.
             *  note: the value provided for validation is expected to be in utc.
             * 
             * @private
             * @param {object} field
             * @return {function} A function that takes a string and returns an array of error messages.
             */

            function getTimeFieldValidator(field) {
                var min = null, max = null;
                var minUtc = field.getField('minTime');
                if (minUtc && _.isDate(minUtc)) {
                    min = new Date(1973, 0, 1, minUtc.getUTCHours(), minUtc.getUTCMinutes(), 0, 0);
                }

                var maxUtc = field.getField('maxTime');
                if (maxUtc && _.isDate(maxUtc)) {
                    max = new Date(1973, 0, 1, maxUtc.getUTCHours(), maxUtc.getUTCMinutes(), 0, 0);
                }
                
                var isRequired = field.getField('isRequired');

                //var max = field.getField('maxTime');
                //var hasDateRangeSet = _.isDate(min) && _.isDate(max);

                
                return function(text) {
                    return exports.validateTime(min, max, isRequired, text);
                };
            }
            
            exports.validateTime = function (min, max, isRequired, value) {
                var hasDateRangeSet = _.isDate(min) && _.isDate(max);
                var result = [];
                if (value) {
                    var dateValue = spUtils.parseDate(value);

                    if (_.isString(value) && value !== '' && !dateValue) {
                        result.push('Invalid time value');
                    }
                    else if (dateValue) {
                        if (min && (dateValue < min)) {
                            if (hasDateRangeSet) {
                                result.push('The value is out of range; the value must be between ' + Globalize.format(min, 't') + ' and ' + Globalize.format(max, 't') + ' inclusive');
                            } else {
                                result.push('The value less than the minimum value; the minimum value is ' + Globalize.format(min, 't'));
                            }
                        }

                        if (max && (dateValue > max)) {
                            if (hasDateRangeSet) {
                                result.push('The value is out of range; the value must be between ' + Globalize.format(min, 't') + ' and ' + Globalize.format(max, 't') + ' inclusive');
                            } else {
                                result.push('The value is out of range; the maximum value is ' + Globalize.format(max, 't'));
                            }
                        }
                    }
                }               
                return result;
            };

            /**
             *  Return a validation function that can be used to validate a time field.
             *  note: the value provided for validation is expected to be in utc.
             * 
             * @private
             * @param {object} field
             * @return {function} A function that takes a string and returns an array of error messages.
             */

            function getDateTimeFieldValidator(field) {
                //var min = null, max = null;
                //var minUtc = field.getField('minDateTime');
                //if (minUtc && _.isDate(minUtc)) {
                //    min = new Date(1973, 0, 1, minUtc.getUTCHours(), minUtc.getUTCMinutes(), 0, 0);
                //}

                //var maxUtc = field.getField('maxDateTime');
                //if (maxUtc && _.isDate(maxUtc)) {
                //    max = new Date(1973, 0, 1, maxUtc.getUTCHours(), maxUtc.getUTCMinutes(), 0, 0);
                //}
                var min = field.getField('minDateTime');
                var max = field.getField('maxDateTime');
                var hasDateRangeSet = _.isDate(min) && _.isDate(max);

                return function(text) {
                    var result = [];
                    if (text) {

                        var dt = text;
                        if (_.isString(dt)) {

                            // chk if date and time components are valid
                            var caratIndex = dt.lastIndexOf("^");
                            var datePart = dt.substr(0, caratIndex);
                            var timePart = dt.substr(caratIndex + 1);
                            var tempDt = spUtils.parseDateString(datePart); //Globalize.parseDate(datePart);
                            if (!_.isDate(tempDt)) {
                                result.push(spUtils.invalidDateMsgText());
                                return result;
                            } else {
                                var datetime = new Date((tempDt.getMonth() + 1) + '/' + tempDt.getDate() + '/' + tempDt.getFullYear() + ' ' + timePart);

                                if (datetime.toString() == 'Invalid Date') {
                                    result.push('Invalid time value.');
                                    return result;
                                } else {
                                    dt = datetime;
                                }

                            }
                        }

                        if (_.isDate(dt)) {

                            if (min && (dt < min)) {
                                if (hasDateRangeSet) {
                                    result.push('The value is out of range; the value must be between ' + Globalize.format(min, 'dd/MM/yy h:mm tt') + ' and ' + Globalize.format(max, 'dd/MM/yy h:mm tt') + ' inclusive');
                                } else {
                                    result.push('The value is out of range; the minimum value is ' + Globalize.format(min, 'dd/MM/yy h:mm tt'));
                                }
                            }

                            if (max && (dt > max)) {
                                if (hasDateRangeSet) {
                                    result.push('The value is out of range; the value must be between ' + Globalize.format(min, 'dd/MM/yy h:mm tt') + ' and ' + Globalize.format(max, 'dd/MM/yy h:mm tt') + ' inclusive');
                                } else {
                                    result.push('The value is out of range; the maximum value is ' + Globalize.format(max, 'dd/MM/yy h:mm tt'));
                                }
                            }
                        } else {
                            result.push('The value is not a valid date time.');
                        }
                    }
                    return result;
                };
            }
            
            exports.validateDateTime = function (min, max, isRequired, value) {
                var hasDateRangeSet = _.isDate(min) && _.isDate(max);
                var result = [];
                if (value) {
                    var dateValue = spUtils.parseDate(value);

                    if (_.isString(value) && value !== '' && !dateValue) {
                        result.push('Invalid time value');
                    }
                    else if (dateValue) {
                        if (min && (dateValue < min)) {
                            if (hasDateRangeSet) {
                                result.push('The value is out of range; the value must be between ' + Globalize.format(min, 't') + ' and ' + Globalize.format(max, 't') + ' inclusive');
                            } else {
                                result.push('The value less than the minimum value; the minimum value is ' + Globalize.format(min, 't'));
                            }
                        }

                        if (max && (dateValue > max)) {
                            if (hasDateRangeSet) {
                                result.push('The value is out of range; the value must be between ' + Globalize.format(min, 't') + ' and ' + Globalize.format(max, 't') + ' inclusive');
                            } else {
                                result.push('The value is out of range; the maximum value is ' + Globalize.format(max, 't'));
                            }
                        }
                    }
                }               
                return result;
            };

            /**
             *  Return a validation function that can be used to validate an string field.
             * 
             * @private
             * @param {object} field
             * @return {function} A function that takes a string and returns an array of error messages.
             */

            function getStringFieldValidator(field) {
                var validatingRegex, pattern, fieldSizeValidator;

                fieldSizeValidator = getFieldSizeValidator(field);

                pattern = field.pattern;

                if (pattern) {
                    validatingRegex = new RegExp(pattern.getRegex());
                } else {
                    validatingRegex = new RegExp();
                }

                var result = function(text) {
                    var myResult = [];

                    if (text && !validatingRegex.test(text)) { // ignoring empty strings

                        myResult.push('Invalid format. ' + pattern.getRegexDescription());
                    }

                    myResult = myResult.concat(fieldSizeValidator(text));

                    return myResult;
                };

                return result;
            }


            /**
            *  Return a validation function that can be used to validate a fields length.
            * 
            * @private
            * @param {object} field
            * @return {function} A function that takes a string and returns an array of error messages.
            */

            function getFieldSizeValidator(field) {

                var max = getMaxFieldSize(field);
                var min = field.getField('minLength');

                return function(text) {
                    var result = [];

                    if (!text)  // null undefined ort an empty string should not be evaluated for length. 
                        return result;

                    if (max > 0 && text.length > max)
                        result.push('The value is too long; the maximum length is ' + Globalize.format(max, 'n0') + ' characters.'); // NOTE 'd' will not work correctly on Safari

                    if (min && text.length < min)   // note we need to test for empty string
                        result.push('The value is too short; the minimum length is ' + Globalize.format(min, 'n0') + ' characters.');

                    return result;
                };
            }

            function getMaxFieldSize(field) {
                var absMax = field.getField('allowMultiLines') ? maxMultilineStringFieldSize : maxSingleLineStringFieldSize;

                return field.getField('maxLength') || absMax;
            }


            /**
         *  Return a validation function that can be used to validate an integer field.
         * 
         * @private
         * @param {object} field
         * @return {function} A function that takes a string and returns an array of error messages.
         */

            function getIntFieldValidator(field) {
                
                var minField = field.getField('minInt');
                var maxField = field.getField('maxInt');

                var displayRange = _.isNumber(minField) && _.isNumber(maxField);

                var min = minNumberFieldValue;
                if (angular.isDefined(minField) && minField != null) {
                    min = minField;
                }
                var max = maxNumberFieldValue;
                if (angular.isDefined(maxField) && maxField != null) {
                    max = maxField;
                }

                return getNumberValidator(validIntegerRegex, min, max, displayRange,null,0);
            }


            /**
         *  Return a validation function that can be used to validate an Decimal field.
         * 
         * @private
         * @param {object} field 
         * @return {function} A function that takes a string and returns an array of error messages.
         */

            function getDecimalFieldValidator(field) {
                
                var decimalPlaceValidator = getDecimalPlacesValidator(field);

                var minField = field.getField('minDecimal');
                var maxField = field.getField('maxDecimal');

                var displayRange = _.isNumber(minField) && _.isNumber(maxField);

                var min = minNumberFieldValue;
                if (angular.isDefined(minField) && minField != null) {
                    min = minField;
                }
                var max = maxNumberFieldValue;
                if (angular.isDefined(maxField) && maxField != null) {
                    max = maxField;
                }

                return getNumberValidator(validDecimalRegex, min, max, displayRange, decimalPlaceValidator, getDecimalPlaces(field));
            }
            
            /////
            // Validates that the number of decimal places has not been exceeded.
            /////
            function getDecimalPlacesValidator(field) {
                
                var decimalPlaces = getDecimalPlaces(field);

                return function (text) {
                    var periodLocation;
                    var stringValue;

                    if (text || text === 0) {
                        stringValue = text.toString();

                        if (decimalPlaces || decimalPlaces === 0) {
                            periodLocation = stringValue.indexOf('.');

                            if (periodLocation >= 0 && stringValue.length - periodLocation - 1 > decimalPlaces) {
                                return 'The maximum number of decimal places has been exceeded.';
                            }
                        }
                    }
                };
            }
            
            function getDecimalPlaces(field) {
                return field.decimalPlaces || 3;
            }


            /**
         *  Return a validation function that can be used to validate a number based upon a regex and a min and max value.
         * 
         * @private
         * @param {regex} numberRegex A regular expression describing the valid string format.
         * @param {displayRange} if true display any out of bounds as a range error.
         * @return {function} A function that takes a string and returns an array of error messages.
         */

            function getNumberValidator(numberRegex, min, max, displayRange,decimalPlaceValidator,decimalPlaces) {


                if (min < minNumberFieldValue) min = minNumberFieldValue;
                if (max > maxNumberFieldValue) max = maxNumberFieldValue;
                
                return function(text) {
                    var value;
                    var result = [];
                    var formattedMin, formattedMax;

                    // if the number was pushed in by the model rather than from the UI it will be a number not a string.
                    if (_.isNumber(text)) {
                        value = text;
                    } else {

                        if (text != null && text.trim().length !== 0) {
                            if (!numberRegex.test(text)) {
                                result.push('The value is not a valid number.');
                            } else {
                                value = parseFloat(text); // float will work in place of an integer
                            }
                        }
                    }

                    if (angular.isDefined(value) && value != null) {
                        if (decimalPlaceValidator) {
                            formattedMin = Globalize.format(min, 'n' + decimalPlaces);
                            formattedMax = Globalize.format(max, 'n' + decimalPlaces);
                        } else {
                            formattedMin = Globalize.format(min, 'n0');
                            formattedMax = Globalize.format(max, 'n0');
                        }
                        if (value < min) {
                            if (displayRange) {
                                result.push('The value is out of range; the value must be between ' + formattedMin + ' and ' + formattedMax + ' inclusive');
                            } else {
                                result.push('The value is less than the minimum value; the minimum value is ' + formattedMin);
                            }
                        }

                        if (value > max) {
                            if (displayRange) {
                                result.push('The value is out of range; the value must be between ' + formattedMin + ' and ' + formattedMax + ' inclusive');
                            } else {
                                result.push('The value is greater than the maximum value; the maximum value is ' + formattedMax);
                            }
                        }
                    }
                    if (decimalPlaceValidator) {
                        var message = decimalPlaceValidator(text);
                        if (message)
                            result.push(message);
                    }

                    return result;
                };
            }


            /**
            *  Return a trimming function that can be used to trim a field to its maximum length.
            * 
            * @param {object} field
            * @return {function} A function that takes a string and returns a trimed string.
            */
            exports.getSanitizer = function(field) {

                var fieldType = field.getIsOfType()[0];

                switch (fieldType.alias()) {
                    case 'core:stringField':
                        return function(text) {
                            var maxSize = getMaxFieldSize(field);
                            if (!text || text.length <= maxSize) {
                                return text;
                            } else {
                                return text.slice(0, maxSize);
                            }
                        };
                    case 'core:intField':
                        return function(text) {
                            return text.replace(sanitizeIntegerRegex, '');
                        };
                    case 'core:dateField':
                        return function(text) {
                            return text.replace(exports.sanitizeDateRegex, '');
                        };
                    case 'core:decimalField':
                    case 'core:currencyField':
                        return function(text) {
                            return text.replace(sanitizeDecimalRegex, '');
                        };
                    default:
                        return function(text) { return text; };
                }
            };


            /**
            *  Validate a form control in the context of a form control and a field value
            * 
            * @param {object} field
            * @param {object} formControl
            * @param {object} fieldValue
            * @return {array} An array of error messages.
            */
            exports.validateFormFieldControl = function(field, isMandatoryOnForm, fieldValue) {
                var validator = exports.getValidator(field);

                var errorMessages = validator(fieldValue);

                if (!fieldValue && ((_.isFunction(field.getIsRequired) && field.getIsRequired()) || isMandatoryOnForm)) {
                    errorMessages.push('A value is required.');
                }

                return errorMessages;
            };

            /**
            *  Validate a form control in the context of a form control and a relationship value
            * 
            * @param {object} relationship
            * @param {object} formControl
            * @param {object} relationships
            * @return {array} An array of error messages.
            */
            exports.validateFormRelationshipControl = function(relationship, isMandatoryOnForm, relationships) {
                var errorMessages = [], length;

                if (_.isArray(relationships)) {
                    length = relationships.length;
                } else {
                    length = relationships ? 1 : 0;
                }

                if (length === 0 && isMandatoryOnForm) {
                    errorMessages.push('A value is required.');
                }

                return errorMessages;
            };


            /**
            *  Raise a validation message to the nearest validationmessagecontrol or alerter
            * 
            * @param {object} scope
            * @param {object} message or messaage array
            */
            exports.raiseValidationErrors = function(scope, validationMessages) {

                if (scope.customValidationMessages) {
                    _.map(validationMessages, function (message) {
                        if (!_.includes(scope.customValidationMessages, message)) {
                            scope.customValidationMessages.push(message);
                        }
                    });

                } else {
                    scope.$emit('customValidationMessages', validationMessages); // propagate a validation message up - angular doesn't support custom validation messages 

                }


            };

            /**
            *  clear the validation messages
            * 
            * @param {object} scope
            */
            exports.clearValidationErrors = function(scope) {
                if (scope.customValidationMessages) {
                    scope.customValidationMessages.length = 0;
                } else {
                    scope.$emit('customValidationMessages', []); // propagate a validation message up - angular doesn't support custom validation messages 
                }
            };

            /**
            *  Validates a field control. If valid value then clears the existing errors else raises the validation messages
            * 
            * @param {object} field
            * @param {object} ngModel controller
            * @param {object} scope
            * @param {bool} show validation message
            */
            exports.validateFieldControl = function(field, ctrl, scope, showValidationMessage) {

                // clear the existing errors before validating the field
                exports.clearValidationErrors(scope);
                var errorMessages = exports.validateFormFieldControl(field, scope.isMandatoryOnForm, scope.fieldValue);

                var valid = errorMessages.length === 0;
                ctrl.$setValidity('spValidateField', valid);

                if (valid) {
                    exports.clearValidationErrors(scope);
                } else {
                    exports.raiseValidationErrors(scope, errorMessages);
                }


            };


            /**
            * Return a validator that can be used in the custom validator directive
            *
            * @param {object} field the field to be validated against
            * @param {object} isRequiredOnForm Is the field required to be populated.
            * @param {object} validationMessages an array to be populated with any validation messaeges. If not provided a "customValidationMessage" is emitted with the messages.
            */
            exports.getCustomParser = function (field, validationMessages) {

                var parser = exports.getParser(field);

                return function (value, returnedMessages) {
                    var messages = [];

                    var typedVal = parser(value, messages);

                    validationMessages.length = 0;
                    if (returnedMessages) {
                        returnedMessages.length = 0;
                    }

                    _.map(messages, function (val) {
                        validationMessages.push(val);
                        if (returnedMessages)
                            returnedMessages.push(val);
                    });

                    return typedVal;
                };

            };
            

            /**
            * Return a validator that can be used in the custom validator directive
            *
            * @param {object} field the field to be validated against
            * @param {object} isRequiredOnForm Is the field required to be populated.
            * @param {object} validationMessages an array to be populated with any validation messaeges. If not provided a "customValidationMessage" is emitted with the messages.
            */
            exports.getCustomValidator = function (field, isRequiredOnForm, validationMessages) {

                var validator = exports.getValidator(field);

                return function (value, returnedMessages) {
                    var messages;

                    if ((spUtils.isNullOrUndefined(value) || value.length === 0) && isRequiredOnForm) {                                        
                        messages = ['A value is required.'];
                    } else {
                        messages = validator(value);

                    }

                    validationMessages.length = 0;

                    _.map(messages, function (value) {
                        validationMessages.push(value);
                        console.warn('Validation message: ' + value);

                        if (returnedMessages)
                          returnedMessages.push(value);
                    });

                    return messages;
                };

            };
            


            ///
            //  custom parsers
            ///
            function getDateFieldParser() {
                return function (value, messages) {
                    if (!value) {
                        return undefined;
                    }
                    
                    return exports.customDateParser(value, messages);
                };
            }
            
            function getTimeFieldParser() {
                return function (value, messages) {
                    if (!value) {
                        return '';
                    }
                    
                    return exports.customTimeParser(value, messages);
                };
            }
            
            function getDecimalFieldParser() {
                return function (value, messages) {
                    if (!value) {
                        return undefined;
                    }
                    
                    if (value === '-')
                        return undefined;
                    
                    var floatValue = Globalize.parseFloat(value);

                    if (!floatValue && floatValue !== 0) {
                        messages.push('invalid value provided');
                        return undefined;
                    }

                    return floatValue;
                };
            }
            
            function getIntFieldParser() {
                return function (value, messages) {
                    
                    if (!value) {
                        return undefined;
                    }
                    
                    if (value === '-')
                        return undefined;
                    
                    var intValue = Globalize.parseInt(value);

                  if (!intValue && intValue!== 0) {
                        messages.push('invalid value provided');
                        return undefined;
                    }

                    return intValue;
                };
            }
            
            //
            ///
            // Parse the specified value into a Date object (where possible) if not valid date then push an error msg.
            ///
            exports.customDateParser = function (value, messages) {
                return customDateParserValidator(value, messages, spUtils.invalidDateMsgText());
            };

            ///
            // Parse the specified value into a Date object (where possible) if not valid date then push an error msg.
            ///
            exports.customTimeParser = function (value, messages) {
                return customDateParserValidator(value, messages, spUtils.invalidTimeMsgText);
            };

            function customDateParserValidator(value, messages, invalidMsgText) {
                var dateValue = spUtils.parseDate(value);

                if (_.isString(value) && value !== '' && !dateValue) {
                    messages.push(invalidMsgText);
                }

                return dateValue;
            }

            
            return exports;
        }])




        /**
     * When added as an attribute to an input field on a form, this will validate the input according to the provided field. The field is pr on the scope.
     * @param {string} fieldName The name of the field that has been put on the scope.
     *
     */
        .directive('spValidateField', ['$compile', 'spFieldValidator', function($compile, spFieldValidator) {

            return {
                // restrict to an attribute type.
                restrict: 'A',
                // element must have ng-model attribute.
                require: 'ngModel',

                // scope = the parent scope
                // elem = the element the directive is on
                // attr = a dictionary of attributes on the element
                // ctrl = the controller for ngModel.
                link: function(scope, elem, attr, ctrl) {
                    var fieldOnScope, showValidationMessage, fieldEntity;

                    fieldOnScope = attr.spValidateField;
                    showValidationMessage = false;

                    fieldEntity = fieldOnScope && scope.$eval(fieldOnScope);


                    if (fieldEntity) {

                        elem.on('blur', function(evt) {
                            scope.$apply(function() {
                                spFieldValidator.validateFieldControl(fieldEntity, ctrl, scope, showValidationMessage);
                            });
                        });

                        scope.$on("validateField", function(event) {
                            //console.log("Received validateField");
                            event.stopPropagation();
                            spFieldValidator.validateFieldControl(fieldEntity, ctrl, scope, showValidationMessage);
                        });

                        scope.$on("validateForm", function(event) {
                            spFieldValidator.validateFieldControl(fieldEntity, ctrl, scope, showValidationMessage);
                        });

                    } else {
                        console.error('sp-validate-field must specify a field to validate against: ', ctrl);
                    }
                }
            };
        }])

        /**
     * When added as an attribute to an input field on a form, this will validate the input according to the provided field. The field is pr on the scope.
     * @param {string} fieldName The name of the field that has been put on the scope.
     *
     */
        .directive('spCustomValidator', ['$compile', 'spFieldValidator', function($compile, spFieldValidator) {

            return {
                // restrict to an attribute type.
                restrict: 'A',
                // element must have ng-model attribute.
                require: 'ngModel',
                

                // scope = the parent scope
                // elem = the element the directive is on
                // attr = a dictionary of attributes on the element
                // ctrl = the controller for ngModel. 
                link: function (scope, elem, attr, ctrl) {

                    var validatorAttrib = attr.spCustomValidator;
                    var validator = scope.$eval(attr.spCustomValidator);
                    var placeHolderValue = attr.placeholder;
                    var modified = true;
                    var modalValue = scope.$eval(attr.ngModel);
                    var isIe = true;

                    //set isIE to false if the browser is not IE.
                    //checking for Trident in IE11.
                    if (navigator.userAgent.indexOf('MSIE') === -1 && navigator.userAgent.indexOf('Trident') === -1) {
                        isIe = false;
                    }

                    //We had to write browser specific code as IE emits a input event when the placeholder value is changed and focus on control etc.
                    //bug in IE, other browser works fine.
                    //Skip the validation once if the placeholder has some value and modal value is empty in IE browser
                    if (isIe && placeHolderValue && !modalValue)
                         modified = false;


                    scope.$watch(validatorAttrib, function (newValidator, oldValidator) {
                        if (newValidator && (newValidator !== oldValidator)) {
                            validator = newValidator;
                        }
                    });

                    ctrl.$parsers.unshift(function (viewvalue) {
                        if ((!placeHolderValue || viewvalue !== placeHolderValue) && modified) {
                            if (viewvalue)
                                viewvalue = viewvalue.trim();
                            var messages = validator(viewvalue);
                            ctrl.$setValidity('customValidationMessage', messages.length === 0);
                        }
                        modified = true;
                        return viewvalue;
                    });
                    
                    scope.$on("validateField", function () {
                        var value = ctrl.$viewValue;
                        validator = scope.$eval(attr.spCustomValidator);

                        var messages = validator(value);
                        ctrl.$setValidity('customValidationMessage', messages.length === 0);
                     });

                }
            };
        }]);    


}());
