// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spResource;

(function (spResource) {

    /**
     * FieldGroup constructor. Do not use.
     * @private
     * @class
     * @name spResource.FieldGroup
     *
     * @classdesc
     * A wrapper around a field group to provide visibility to all the fields and relationships 
     * in this group, as they should appear after rules such as inheritance and 'isHidden' have been applied.
     */
    var FieldGroup = (function () {

        function FieldGroup(fieldGroupEntity, typeEntity) {
            if (!typeEntity)
                throw new Error('typeEntity is required');
            if (!fieldGroupEntity)
                throw new Error('fieldGroupEntity is required');
            this._fieldGroupEntity = fieldGroupEntity; // may be 'unassigned' in the case of ungrouped fields.
            this._typeEntity = typeEntity;
        }


        /**
         * Returns the name of the field group.
         *
         * @returns {string} The field group name.
         *
         * @function
         * @name spResource.FieldGroup#getName
         */
        FieldGroup.prototype.getName = function() {
            var res = this._fieldGroupEntity.getName();
            return res;
        };


        /**
         * Returns the effective description of the field group.
         *
         * @returns {string} The effective description of this field group.
         *
         * @function
         * @name spResource.FieldGroup#getDescription
         */
        FieldGroup.prototype.getDescription = function () {
            var res = this._fieldGroupEntity.getField('description');
            return res;
        };


        /**
         * Returns the entity for this field group.
         *
         * @returns {spEntity.Entity} The field group entity.
         *
         * @function
         * @name spResource.FieldGroup#getEntity
         */
        FieldGroup.prototype.getEntity = function () {
            return this._fieldGroupEntity;
        };


        /**
         * Returns the fields in this field group.
         *
         * @returns {Array.<spResource.Field>} The wrapper objects for fields in this field group.
         *
         * @function
         * @name spResource.FieldGroup#getFields
         */
        FieldGroup.prototype.getFields = function (options) {
            var that = this;
            var res = _.filter(this._typeEntity.getFields(options), function (m) { return that._memberInThisGroup(m); });
            return res;
        };


        /**
         * Returns all relationships, lookups, and choice fields in this field group.
         *
         * @returns {Array.<spResource.Relationship>} The wrapper objects for relationships in this field group.
         *
         * @function
         * @name spResource.FieldGroup#getAllRelationships
         */
        FieldGroup.prototype.getAllRelationships = function (options) {
            var that = this;
            var res = _.filter(this._typeEntity.getAllRelationships(options), function (m) { return that._memberInThisGroup(m); });
            return res;
        };


        /**
         * Returns all lookups in this field group. Excludes choice-fields.
         *
         * @returns {Array.<spResource.Relationship>} The wrapper objects for lookups in this field group.
         *
         * @function
         * @name spResource.FieldGroup#getLookups
         */
        FieldGroup.prototype.getLookups = function (options) {
            var that = this;
            var res = _.filter(this._typeEntity.getLookups(options), function (m) { return that._memberInThisGroup(m); });
            return res;
        };


        /**
         * Returns all relationships (to-many) in this field group. Excludes lookups and choice-fields.
         *
         * @returns {Array.<spResource.Relationship>} The wrapper objects for relationships in this field group.
         *
         * @function
         * @name spResource.FieldGroup#getRelationships
         */
        FieldGroup.prototype.getRelationships = function (options) {
            var that = this;
            var res = _.filter(this._typeEntity.getRelationships(options), function (m) { return that._memberInThisGroup(m); });
            return res;
        };


        /**
         * Returns all choice fields in this field group.
         *
         * @returns {Array.<spResource.Relationship>} The wrapper objects for choice fields in this field group.
         *
         * @function
         * @name spResource.FieldGroup#getChoiceFields
         */
        FieldGroup.prototype.getChoiceFields = function (options) {
            var that = this;
            var res = _.filter(this._typeEntity.getChoiceFields(options), function (m) { return that._memberInThisGroup(m); });
            return res;
        };


        /**
         * Returns all fields, lookups, relationships and choice fields in this field group.
         *
         * @returns {Array.<object>} The wrapper objects for fields or relationships field group.
         *
         * @function
         * @name spResource.FieldGroup#getAllMembers
         */
        FieldGroup.prototype.getAllMembers = function (options) {
            var that = this;
            var res = _.filter(this._typeEntity.getAllMembers(options), function (m) { return that._memberInThisGroup(m); });
            return res;
        };

        FieldGroup.prototype._memberInThisGroup = function (member) {
            var fg = member.getFieldGroupEntity();
            if (!fg && this._fieldGroupEntity && this._fieldGroupEntity.eid().getNsAlias() === 'core:default')
                return true;  // note: If the fg is null, present it in the 'Default' group.
            var res = fg === this._fieldGroupEntity; 
            return res;
        };

        FieldGroup.prototype._isVisible = function (options) {
            return true;
        };

        return FieldGroup;
    })();

    spResource.FieldGroup = FieldGroup;

})(spResource || (spResource = {}));
