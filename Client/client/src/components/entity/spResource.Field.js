// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spResource;

(function (spResource) {

    /**
     * Field constructor. Do not use.
     * @private
     * @class
     * @name spResource.Field
     *
     * @classdesc
     * Represents a wrapped field as it should appear when viewing schema details for a type.
     * Eventually this will also include name/description overrides.
     */
    var Field = (function () {

        function Field(fieldEntity, typeEntity) {
            if (!fieldEntity)
                throw new Error('fieldEntity is required');
            this._fieldEntity = fieldEntity;
            this._typeEntity = typeEntity;
        }


        /**
         * Returns the effective name of the field.
         *
         * @returns {string} The effective name of this field.
         *
         * @function
         * @name spResource.Field#getName
         */
        Field.prototype.getName = function () {
            return this.getOverridableValue('name');
        };


        /**
         * Returns the effective description of the field.
         *
         * @returns {string} The effective description of this field.
         *
         * @function
         * @name spResource.Field#getDescription
         */
        Field.prototype.getDescription = function () {
            return this.getOverridableValue('description');
        };


        /**
         * Returns the indentifier name to refer to this field in scripts.
         *
         * @returns {string} The effective script name of this field.
         *
         * @function
         * @name spResource.Field#getScriptName
         */
        Field.prototype.getScriptName = function () {
            // requires scriptInfo:true in the options when loading.
            return this._fieldEntity.fieldScriptName || this.getName();
        };


        /**
         * Returns the effective, possibly overridden, value of the specified field.
         *
         * @returns {object} The effective value of this field.
         *
         * @function
         * @name spResource.Field#getOverrideValue
         */
        Field.prototype.getOverridableValue = function (fieldName) {
            if (!this['_has_' + fieldName]) {
                var res = null;
                var found = false;
                var overrides = this.getFieldOverrides();
                for (var i = 0; i < overrides.length; i++) {
                    var oField = overrides[i];
                    var value = oField.getField(fieldName);
                    if (value) {
                        res = value;
                        found = true;
                        break;
                    }
                }
                // fall back to field
                if (!found) {
                    res = this._fieldEntity.getField(fieldName);
                }
                this['_has_' + fieldName] = true;
                this['_val_' + fieldName] = res;
                return res;
            }
            return this['_val_' + fieldName];
        };


        /**
         * Returns the effective, possibly overridden, entity being pointed to by a lookup.
         * If the lookup is not set, then the value is not returned.
         *
         * @returns {object} The effective entity being pointed to.
         *
         * @function
         * @name spResource.Field#getOverridableLookup
         */
        Field.prototype.getOverridableLookup = function (lookupName) {
            if (!this['_has_' + lookupName]) {
                var res = null;
                var found = false;
                var overrides = this.getFieldOverrides();
                for (var i = 0; i < overrides.length; i++) {
                    var oField = overrides[i];
                    var value = oField.getLookup(lookupName);
                    if (value) {
                        res = value;
                        found = true;
                        break;
                    }
                }
                // fall back to field
                if (!found) {
                    res = this._fieldEntity.getLookup(lookupName);
                }
                this['_has_' + lookupName] = true;
                this['_val_' + lookupName] = res;
                return res;
            }
            return this['_val_' + lookupName];
        };


        /**
         * Returns the complete set of field override entities that apply to this field.
         *
         * @returns {Array.<spEntity.Entity>} The field override entities.
         *
         * @function
         * @name spResource.Field#getFieldOverrides
         */
        Field.prototype.getFieldOverrides = function () {
            if (!this._fieldOverrides) {
                // All overrides returned for this field (but may apply to other types)
                var candidateOverrides = this._fieldEntity.getRelationship('fieldOverriddenBy');

                this._fieldOverrides = [];
                if (candidateOverrides.length > 0 && this._typeEntity) {
                    // Check the types in order, to only get the overrides that apply to this type (roughly in order of inheritance)
                    var types = this._typeEntity.getAllEntities();
                    var fnGetFieldOverrideForType = function(co) {
                        return co.getLookup('fieldOverrideForType') === type;
                    };
                    for (var i = 0; i < types.length; i++) {
                        var type = types[i];
                        var vals = _.filter(candidateOverrides, fnGetFieldOverrideForType);
                        this._fieldOverrides = this._fieldOverrides.concat(vals);
                    }
                }
            }
            return this._fieldOverrides;
        };


        /**
         * Returns the entity for this field.
         *
         * @returns {spEntity.Entity} The field entity.
         *
         * @function
         * @name spResource.Field#getEntity
         */
        Field.prototype.getEntity = function () {
            return this._fieldEntity;
        };


        /**
         * Returns the entity that represents the field group for this field.
         *
         * @returns {spEntity.Entity} The field group entity, or undefined.
         *
         * @function
         * @name spResource.Field#getFieldGroupEntity
         */
        Field.prototype.getFieldGroupEntity = function () {
            var fg = this.getOverridableLookup('fieldInGroup');
            return fg || null;
        };


        /**
         * Returns the type array of the field.
         *
         * @returns [{object}] The type array of this field.
         *
         * @function
         * @name spResource.Field#getTypes
         */
        Field.prototype.getTypes = function () {
            return this._fieldEntity.getIsOfType();
        };


        /**
         * Returns true if this member is a field.
         *
         * @returns {bool} True if this is a field.
         *
         * @function
         * @name spResource.Field#isField
         */
        Field.prototype.isField = function () {
            return true;
        };


        /**
        * Returns true if the field is required.
        *
        * @returns {bool} True if the field is required. Defaults to false if not required.
        *
        * @function
        * @name spResource.Field#isRequired
        */
        Field.prototype.isRequired = function() {
            var required = this._fieldEntity.isRequired;
            return required || false;
        };


        /**
        * Returns true if the field is readonly.
        *
        * @returns {bool} True if the field is readonly. Defaults to false if not readonly.
        *
        * @function
        * @name spResource.Field#isReadOnly
        */
        Field.prototype.isReadOnly = function () {
            var readonly = this._fieldEntity.isFieldReadOnly;
            return readonly || false;
        };


        /**
         * Returns true if the field is hidden.
         *
         * @returns {bool} True if the field is hidden. Defaults to false if not loaded.
         *
         * @function
         * @name spResource.Field#isHidden
         */
        Field.prototype.isHidden = function () {
            var hidden = this._fieldEntity.hideField;
            return hidden || false;
        };

        /**
         * Returns true if the field is hidden.
         *
         * @returns {bool} True if the field is hidden. Defaults to false if not loaded.
         *
         * @function
         * @name spResource.Field#isHidden
         */
        Field.prototype.memberType = function () {
            return 'field';
        };

        /**
         * Returns a description of the member type
         */
        Field.prototype.memberTypeDesc = function() {
            return sp.result(this.getEntity(), 'isOfType.0.name') || 'Field'; // e.g. String Field
        };


        Field.prototype._isVisible = function (options) {
            if (options && options.showHidden) {
                return true;
            }
            if (options && options.hideNonWritable) {
                var alias = this._fieldEntity.eid().getNsAlias();
                if (alias === 'core:createdDate' || alias === 'core:modifiedDate') {
                    return false;
                }
                var fieldTypeAlias = this._fieldEntity.type.nsAlias;
                if (fieldTypeAlias === 'core:autoNumberField') {
                    return false;
                }
                if (this._fieldEntity.isCalculatedField) {
                    return false;
                }
            }
            if (options && options.hideCalculatedFields) {
                if (this._fieldEntity.isCalculatedField) {
                    return false;
                }
            }
            return !this.isHidden();
        };

        return Field;
    })();

    spResource.Field = Field;

})(spResource || (spResource = {}));
