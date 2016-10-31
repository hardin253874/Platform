// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module field properties helper functions.       
    * 
    * @module fieldPropertiesHelper    
    */
    angular.module('mod.app.configureDialog.fieldPropertiesHelper', [])
        .factory('fieldPropertiesHelper', function () {
            var exports = {};
            exports.fieldDefaultObject = {
                name: jsonString(''),
                description: jsonString(''),
                typeId: 'core:stringField',
                defaultValue: jsonString(''),
                isRequired: false,
                fieldWatermark: jsonString(''),
                fieldScriptName: jsonString(''),
                fieldCalculation: jsonString(''),
                isCalculatedField: false
            };
            exports.formControlDefaultObject = {
                name: jsonString(''),
                description: jsonString(''),
                typeId: 'console:singleLineTextControl',
                'console:renderingBackgroundColor': 'white',
                'console:mandatoryControl': false,
                'console:readOnlyControl': false,
                'console:showControlHelpText': false,
                'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic'),
                'console:fieldToRender': exports.fieldDefaultObject
            };
            exports.formControlOnlyObject = {
                name: jsonString(''),
                description: jsonString(''),
                typeId: 'console:singleLineTextControl',
                'console:renderingBackgroundColor': 'Transparent',
                'console:mandatoryControl': false,
                'console:showControlHelpText': false,
                'console:readOnlyControl': false,
                'console:renderingHorizontalResizeMode': jsonLookup('console:resizeAutomatic'),
                'console:renderingVerticalResizeMode': jsonLookup('console:resizeAutomatic')
            };

            /**   
            *Set the maximum value of the field according the type
            *
            * @param {object} type The field.
            * @param {object} maxValue of the field to set.            
            * @param {object} field object.            
            */
            exports.setMaxValue = function (type, maxValue, field) {
                switch (type) {
                    case 'Decimal':
                    case 'Currency':
                        field.setField('core:maxDecimal', maxValue, spEntity.DataType.Decimal);
                        break;
                    case 'Int32':
                        field.setField('core:maxInt', maxValue, spEntity.DataType.Int32);
                        break;
                    case 'String':
                        field.setField('core:maxLength', maxValue, spEntity.DataType.Int32);
                        break;
                    case 'Date':
                        field.setField('core:maxDate', maxValue, spEntity.DataType.Date);
                        break;
                    case 'Time':
                        field.setField('core:maxTime', maxValue, spEntity.DataType.Time);
                        break;
                    case 'DateTime':
                        field.setField('core:maxDateTime', maxValue, spEntity.DataType.DateTime);
                        break;
                }
            };


            /**   
           *Set the minimum value of the field according the type
           *
           * @param {object} type The field.
           * @param {object} minValue of the field to set.            
           * @param {object} field object.            
           */
            exports.setMinValue = function (type, minValue, field) {
                switch (type) {
                    case 'Decimal':
                    case 'Currency':
                        field.setField('core:minDecimal', minValue, spEntity.DataType.Decimal);
                        break;
                    case 'Int32':
                        field.setField('core:minInt', minValue, spEntity.DataType.Int32);
                        break;
                    case 'String':
                        field.setField('core:minLength', minValue, spEntity.DataType.Int32);
                        break;
                    case 'Date':
                        field.setField('core:minDate', minValue, spEntity.DataType.Date);
                        break;
                    case 'Time':
                        field.setField('core:minTime', minValue, spEntity.DataType.Time);
                        break;
                    case 'DateTime':
                        field.setField('core:minDateTime', minValue, spEntity.DataType.DateTime);
                        break;
                }
            };
            /**
            *Returns true for decimal and currency type field
            * @param {object} field.            
            * @return {bool} true/false. 
            **/
            exports.isDecimalPlacesVisible = function(field) {
                if (field) {
                    var type = field.getIsOfType()[0];
                    if (type.alias() === 'core:decimalField' || type.alias() === 'core:currencyField')
                        return true;
                }
                return false;
            };
            /**
          *Returns true for string type field
          * @param {object} field.            
          * @return {bool} true/false. 
          **/
            exports.isStringPatternVisible = function(field) {
                if (field) {
                    var type = field.getIsOfType()[0];
                    if (type.alias() === 'core:stringField')
                        return true;
                }
                return false;
            };
            /**
            * Returns a template function for the field object.
            **/
            exports.getTemplateFnForField = function (currentType) {
                var templateFn = function (type) {
                    if (type === currentType)
                        return spEntity.fromJSON(exports.fieldDefaultObject);
                    return null;
                };
                return templateFn;
            };
            /**
           * Returns a template function for the field control object.
           **/
            exports.getTemplateFnForFormControl = function (controlType) {
                var templateFn = function (type) {
                    if (type === controlType)
                        return spEntity.fromJSON(exports.formControlDefaultObject);
                    return null;
                };
                return templateFn;
            };
            /**
          * Returns a template function for the field control object.
          **/
            exports.getTemplateFnForFormControlOnly = function (controlType) {
                var templateFn = function (type) {
                    if (type === controlType)
                        return spEntity.fromJSON(exports.formControlOnlyObject);
                    return null;
                };
                return templateFn;
            };
            /**
           * returns a datatype string to detemine the min and max vales of teh field.
           **/
            exports.getDataTypeForMinMax = function (field) {
                var dataType = spEntityUtils.dataTypeForField(field);
                switch (dataType) {
                case 'String':
                    return 'Length';
                case 'Int32':
                    return 'Int';
                case 'Currency':
                    return 'Decimal';
                default:
                    return dataType;
                }
            };
            /**
           * returns the minimun value of the field.
           **/
           exports.getMinimumFieldValue = function(field) {
               return field.getField('min' + exports.getDataTypeForMinMax(field));
           };
            /**
           *returns the maximum value of the field.
           **/
            exports.getMaximumFieldValue = function (field) {
                return field.getField('max' + exports.getDataTypeForMinMax(field));
            };
            /**
           * returns the max field from the array of fields
           *@param {Array(object)} fields
           *@param {object} fieldToRender
           **/
            exports.getMaxField = function (fields, fieldToRender) {
                return _.find(fields, function (field) {
                    return field.getAlias() === 'core:max' + exports.getDataTypeForMinMax(fieldToRender);
                });
            };
            /**
             * returns the min field from the array of fields
           *@param {Array(object)} fields
           *@param {object} fieldToRender
           **/
            exports.getMinField = function (fields,fieldToRender) {
                return _.find(fields, function(field) {
                    return field.getAlias() === 'core:min' + exports.getDataTypeForMinMax(fieldToRender);
                });
            };

            /**
             * Get the lookups toType.
             */

          function getToType(lookup) {
                var toType;
                var entity;

                if (lookup && lookup.getEntity) {
                    entity = lookup.getEntity();

                    if (entity) {
                        toType = entity.getToType();

                        return toType;
                    }
                }
                return null;
            }

            /**
             * Gets the selected type.
             */

          exports.getSelectedType = function (spFormBuilderService) {
                var type;
                var lookup;
                var toType;

                lookup = spFormBuilderService.selectedLookup;

                if (lookup) {
                    toType =getToType(lookup);
                } else {
                    toType = spFormBuilderService.definition;
                }

                type = new spResource.Type(toType);

                return type;
            };
            return exports;


        });
}());