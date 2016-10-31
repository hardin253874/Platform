// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spEntity;
var EdcEntity;  // legacy

// Refer to docs for fromJSON
var jsonInt, jsonString, jsonDecimal, jsonCurrency, jsonDate, jsonTime, jsonDateTime, jsonBool, jsonGuid, jsonLookup, jsonRelationship;

(function (spEntity) {

    /**
     * DataType enum constructor. Do not use.
     * @private
     * @class
     * @name spEntity.DataType
     *
     * @classdesc
     * The DataType enumeration represents what type of field data is being stored in a field or report column.
     */
    var DataType = (function () {
        function DataType() { }

        /** @name spEntity.DataType.None */
        DataType.None = 'None';

        /** @name spEntity.DataType.Entity */
        DataType.Entity = 'Entity';

        /** @name spEntity.DataType.String */
        DataType.String = 'String';

        /** @name spEntity.DataType.Int32 */
        DataType.Int32 = 'Int32';

        /** @name spEntity.DataType.Decimal */
        DataType.Decimal = 'Decimal';

        /** @name spEntity.DataType.Currency */
        DataType.Currency = 'Currency';

        /** @name spEntity.DataType.Date */
        DataType.Date = 'Date';

        /** @name spEntity.DataType.Time */
        DataType.Time = 'Time';

        /** @name spEntity.DataType.DateTime */
        DataType.DateTime = 'DateTime';

        /** @name spEntity.DataType.Bool */
        DataType.Bool = 'Bool';

        /** @name spEntity.DataType.Guid */
        DataType.Guid = 'Guid';


        /**
         * Determines if the field is numeric 
         *
         * @param {string} dataType Data type name.
         * @returns {bool} true if the data type is numeric.
         *
         * @function
         * @name spEntity.DataType.isNumeric
         */
        DataType.isNumeric = function (dataType) {
            switch (dataType) {
                case 'Decimal':
                case 'Currency':
                case 'Number':
                case 'Int32':
                    return true;
                default:
                    return false;
            }
        };


        /**
         * Determines if the field is a date related type 
         *
         * @param {string} dataType Data type name.
         * @returns {bool} true if the data type is a date type or similar.
         *
         * @function
         * @name spEntity.DataType.isDateType
         */
        DataType.isDateType = function (dataType) {
            switch (dataType) {
                case 'Date':
                case 'Time':
                case 'DateTime':
                    return true;
                default:
                    return false;
            }
        };


        /**
         * Determines if the field type is a resource type 
         *
         * @param {string} dataType Data type name.
         * @returns {bool} true if the data type is numeric.
         *
         * @function
         * @name spEntity.DataType.isResource
         */
        DataType.isResource = function (dataType) {
            switch (dataType) {
                case 'RelatedResource':     // this is deprecated (can it be deleted?)
                case 'ChoiceRelationship':
                case 'InlineRelationship':
                case 'UserInlineRelationship':
                case 'Image':
                case 'StructureLevels':
                    return true;
                default:
                    return false;
            }
        };

        return DataType;
    })();
    spEntity.DataType = DataType;


    var makeJsonFunction = function(dataType) {
        return function (value) {
            var res = { 'json': dataType };
            if (!(value === null || _.isUndefined(value))) {
                res.value = value;
            }
            return res;
        };
    };

    // Refer to docs for fromJSON
    // Defined here so the dataType values are available.
    jsonString = makeJsonFunction(DataType.String);
    jsonInt = makeJsonFunction(DataType.Int32);
    jsonDecimal = makeJsonFunction(DataType.Decimal);
    jsonDate = makeJsonFunction(DataType.Date);
    jsonTime = makeJsonFunction(DataType.Time);
    jsonDateTime = makeJsonFunction(DataType.DateTime);
    jsonBool = makeJsonFunction(DataType.Bool);
    jsonGuid = makeJsonFunction(DataType.Guid);
    jsonLookup = makeJsonFunction('Lookup');
    jsonRelationship = makeJsonFunction('Relationship');

    /**
     * Field constructor. Do not use.
     * @private
     * @class
     * @name spEntity.Field
     *
     * @classdesc
     * The Entity object holds fields and relationships.
     * This is the standard object for manipulating entity data on the client. This object is not (directly) what gets passed to network calls.
     */
    var Field = (function () {
        function Field(id, dataType) {
            if (!dataType) {
                throw new Error('No data type supplied for field. ' + (id || '<null>'));
            }
            if (!_.isObject(id)) {
                id = spEntity.asEntityRef(id);
            }

            // remember CLONE
            this.id = id;
            this._dataType = dataType;
        }

        
        /**
         * Determines if the field is numeric 
         *
         * @returns {bool} true if the field is numeric.
         *
         * @function
         * @name spEntity.Field.isNumeric
         */
        Field.prototype.isNumeric = function () {
            var res = spEntity.DataType.isNumeric(this._dataType);
            return res;
        };

        /**
         * Set the raw value of field without altering the data state. 
         *         
         * @function
         * @name spEntity.Field.setRawFieldValue
         */
        Field.prototype.setRawFieldValue = function (value) {
            this._setValue(value);
            this.markAsPristine();
        };
        
        /**
         * @internal
         * Do not use outside of spEntity.js
         */
        Field.prototype._getValue = function (value) {
            // Note: fieldAccessorFactory directly accesses the value
            return this._value;
        };

        /**
         * @internal
         * Do not use outside of spEntity.js
         */
        Field.prototype._setValue = function (value) {
            if (this._pristine && this._wasPristine) {
                this._initRaw = this._convertNativeToRaw(this._dataType, this._value);
                this._pristine = false;
            }

            this._value = this._prepareValue(value);
        };

        /**
         * @internal
         * Do not use outside of spEntity.js
         */
        Field.prototype._prepareValue = function (value) {
            if (this._dataType === 'Bool' && value === null) {
                return false;
            } else {
                return value;
            }
        };

        /**
         * @internal
         * Do not use outside of spEntity.js
         */
        Field.prototype._convertRawToNative = function (dataType, value) {
            var result;
            if (value === null) {
                result = null;
            } else {
                switch (dataType) {
                    case 'String':
                    case 'Xml':
                    case 'Guid':
                        result = value === '' ? null : value;   // empty strings should be represented as nulls
                        break;
                    case 'Bool':
                        result = _.isUndefined(value) || _.isNull(value) ? false : value;
                        break;
                    case 'Int32':
                    case 'Decimal':
                    case 'Currency':
                    case 'Number':
                        result = value;
                        break;
                    case 'Date':
                    case 'Time':
                    case 'DateTime':
                        if (value && value.substring(value.length - 1, value.length) == 'Z')
                            result = new Date(value);
                        else 
                            result = new Date(value + 'Z'); // This will treat it as a UTC value
                        break;
                    default:
                        console.log('Unrecognized field type returned from server', dataType, value || '<null>');
                        throw new Error('Unrecognized field type returned from server: ' + dataType + ' ' + (value || '<null>'));
                }
            }
            return result;
        };

        /**
         * @internal
         * Do not use outside of spEntity.js
         */
        Field.prototype._convertNativeToRaw = function (dataType, value) {
            var result;
            if (value === null || angular.isUndefined(value)) {
                result = null;
            } else {
                switch (dataType) {
                    case 'String':
                    case 'Xml':
                    case 'Guid':
                        result = value === '' ? null : (value ? value.toString() : value);   // empty strings should be represented as nulls
                        break;
                    case 'Int32':
                    case 'Decimal':
                    case 'Currency':
                    case 'Number':
                    case 'Bool':
                        result = value;
                        break;
                    case 'Date':
                    case 'Time':
                    case 'DateTime':
                        result = value.toISOString().substring(0, 19);  // Uses UTC. Reduces '2012-01-02T03:04:05.000Z' to '2012-01-02T03:04:05'
                        break;
                    default:
                        console.log('Unrecognized field type returned from server', dataType);
                        throw new Error('Unrecognized field type returned from server: ' + dataType);
                }
            }
            return result;
        };
        
        Field.prototype._asRawField = function (options) {
            if (this._pristine && options.changesOnly)
                return null;    // do not transmit if unchanged

            var rawValue = this._convertNativeToRaw(this._dataType, this._value);
            if (rawValue === this._initRaw && options.changesOnly)
                return null;    // do not transmit if unchanged

            var res = {
                fieldId: this.id._getIdOrDummyId(),
                typeName: this._dataType
            };
            var valField = 'value';
            res[valField] = rawValue !== null && !_.isUndefined(rawValue) ? rawValue.toString() : rawValue;
            return res;
        };

        /**
         * Marks the field as pristine, meaning that if no other changes are made, then the value will not be sent to the server.
         *
         * @function
         * @name spEntity.Field.markAsPristine
         */
        Field.prototype.markAsPristine = function () {
            this._wasPristine = true;
            this._pristine = true;
        };

        return Field;
    })();
    spEntity.Field = Field;

})(spEntity || (spEntity = EdcEntity = {}));

