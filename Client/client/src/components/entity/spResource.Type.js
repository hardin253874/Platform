// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity */

var spResource;

(function (spResource) {

    var Type = (function() {

        /**
         * spResource Type constructor. Wraps a resource type entity to provide convenient access to schema information.
         * Note: this assumes that the relevant data has been loaded.
         *
         * @param {Array.<spEntity.Entity>} typeEnties A type entity, or array of type entities, to be wrapped.
         *
         * @example
         * <pre>
         * var rq = spResource.makeTypeRequest();
         * spEntityService.getEntity(typeId, rq).then(function(typeEntity) {
         *    var type = new spResource.Type(typeEntity);
         *    var allVisibleFields = type.getFields();
         *    var allVisibleGroups = type.getFieldGroups();
         *    // etc..
         * });
         * </pre>
         *
         * @private
         * @class
         * @name spResource.Type
         */
        function Type(typeEntities) {
            if (_.isArray(typeEntities))
                this._typeEntities = typeEntities;
            else if (!typeEntities)
                this._typeEntities = [];
            else
                this._typeEntities = [typeEntities];
        }

        /**
         * Get the name for this type.
         *
         * @returns {string} The description.
         *
         * @function
         * @name spResource.Type#getName
         */
        Type.prototype.getName = function getName() {
            return this.getEntity().getName();
        };

        /**
         * Get the description for this type.
         *
         * @returns {string} The description.
         *
         * @function
         * @name spResource.Type#getDescription
         */
        Type.prototype.getDescription = function getDescription() {
            return this.getEntity().getField('description');
        };

        /**
         * Get the entity that was passed into the constructor.
         *
         * @returns {spEntity.Entity} The entity. (Or the first if multiple were passed, or null/undefined).
         *
         * @function
         * @name spResource.Type#getEntity
         */
        Type.prototype.getEntity = function getEntity() {
            return this._typeEntities[0];
        };

        /**
         * Get all entities that were passed into the constructor.
         *
         * @returns {Array.<spEntity.Entity>} Array of type entities.
         *
         * @function
         * @name spResource.Type#getEntities
         */
        Type.prototype.getEntities = function getEntities() {
            return this._typeEntities;
        };

        /**
         * Get all applicable types including inherited types.
         *
         * @returns {Array.<spEntity.Entity>} Sorted array of type entities.
         *
         * @function
         * @name spResource.Type#getAllEntities
         */
        Type.prototype.getAllEntities = function getAllEntities() {
            if (!this._allEntities) {
                this._allEntities = spResource.getAncestorsAndSelf(this._typeEntities);
            }
            return this._allEntities;
        };

        /**
         * Get a sorted list of all applicable field groups.
         *
         * @returns {Array.<spResource.FieldGroup>} Sorted array of field-group wrappers.
         *
         * @function
         * @name spResource.Type#getFieldGroups
         */
        Type.prototype.getFieldGroups = function getFieldGroups(options) {
            if (!this._fieldGroups) {
                var that = this;
                // This will pick up all field groups both via the types, and the members.
                // (In principle the types should return the complete set, but in practice it may not)
                // Also, some members may return 'undefined', in which case there should be one 'default' group.
                var types = this.getAllEntities();
                var members = this.getAllMembers();  // To consider: should we pass showHidden:true?
                
                var fgOnTypes = _.flatten(_.invokeMap(types, 'getRelationship', 'fieldGroups'));
                var fgOnMembers = _.invokeMap(members, 'getFieldGroupEntity');
                var fgAll = _.uniq(fgOnTypes.concat(fgOnMembers));

                // Handle default groups. If fields are unassigned to any group, find (or mock) the default group.
                if (_.includes(fgAll, null)) {
                    fgAll = _.compact(fgAll); // remove the nulls
                    // add mock def if necessary
                    var defGroup = _.find(fgAll, function (fg) { return fg.eid().getNsAlias() === 'core:default'; });
                    if (!defGroup) {
                        fgAll.push(spEntity.fromJSON({ id: 'core:default', name: 'Default', description: 'Contains Name and Description Fields' }));
                    }
                }

                var wrapped = _.map(fgAll, function(fg) {
                    return new spResource.FieldGroup(fg, that);
                });
                this._fieldGroups = _.sortBy(wrapped, spUtils.getter('getName'));
            }

            var res = _.filter(this._fieldGroups, function (fg) { return fg._isVisible(options); });
            return res;
        };


        /**
         * Get a sorted list of all applicable field groups.
         *
         * @returns {Array.<spResource.Field>} Sorted array of field-group wrappers.
         *
         * @function
         * @name spResource.Type#getFields
         */
        Type.prototype.getFields = function getFields(options) {
            if (!this._fields) {
                var that = this;
                var types = this.getAllEntities();
                this._fields = _.chain(types)
                    .map(function (t) { return t.getRelationship('fields'); })
                    .flatten()
                    .map(function (f) { return new spResource.Field(f, that); })
                    .sortBy(spUtils.getter('getName'))
                    .value();
            }
            
            var res = _.filter(this._fields, function (f) { return f._isVisible(options); });
            return res;
        };


        /**
         * Get a sorted list of all applicable relationships.
         *
         * @returns {Array.<spResource.Relationship>} Sorted array of field-group wrappers.
         *
         * @function
         * @name spResource.Type#getAllRelationships
         */
        Type.prototype.getAllRelationships = function getAllRelationships(options) {
            if (!this._allRelationships) {
                var that = this;
                var types = this.getAllEntities();
                var fwd = _.chain(types)
                    .map(function (t) { return t.getRelationship('relationships'); })
                    .flatten()
                    .map(function (r) { return new spResource.Relationship(r, that, false); })
                    .sortBy(spUtils.getter('getName'))
                    .value();
                var rev = _.chain(types)
                    .map(function (t) { return t.getRelationship('reverseRelationships'); })
                    .flatten(true)
                    .map(function (rr) { return new spResource.Relationship(rr, that, true); })
                    .sortBy(spUtils.getter('getName'))
                    .value();
                var all = fwd.concat(rev);
                this._allRelationships = _.sortBy(all, spUtils.getter('getName'));
            }
            
            var res = _.filter(this._allRelationships, function (r) { return r._isVisible(options); });
            return res;
        };


        /**
         * Get a sorted list of all applicable lookups.
         *
         * @returns {Array.<spResource.Relationship>} Sorted array of relationship wrappers.
         *
         * @function
         * @name spResource.Type#getLookups
         */
        Type.prototype.getLookups = function getLookups(options) {
            var all = this.getAllRelationships(options);
            var res = _.filter(all, function (r) { return r.isLookup(); });
            return res;
        };


        /**
         * Get a sorted list of all applicable relationships (to-many).
         *
         * @returns {Array.<spResource.Relationship>} Sorted array of relationship wrappers.
         *
         * @function
         * @name spResource.Type#getRelationships
         */
        Type.prototype.getRelationships = function getRelationships(options) {
            var all = this.getAllRelationships(options);
            var res = _.filter(all, function (r) { return r.isRelationship(); });
            return res;
        };


        /**
         * Get a sorted list of all applicable choice fields.
         *
         * @returns {Array.<spResource.Relationship>} Sorted array of relationship wrappers.
         *
         * @function
         * @name spResource.Type#getChoiceFields
         */
        Type.prototype.getChoiceFields = function getChoiceFields(options) {
            var all = this.getAllRelationships(options);
            var res = _.filter(all, function (r) { return r.isChoiceField(); });
            return res;
        };



        /**
         * Get a sorted list of all applicable field groups.
         *
         * @returns {Array<object>} Sorted array of various members.
         *
         * @function
         * @name spResource.Type#getAllMembers
         */
        Type.prototype.getAllMembers = function getAllMembers(options) {
            if (!this._allMembers) {
                var members = this.getFields().concat(this.getAllRelationships());
                members = _.sortBy(members, spUtils.getter('getName'));
                this._allMembers = members;
            }

            var res = _.filter(this._allMembers, function (m) { return m._isVisible(options); });
            return res;
        };


        /**
         * Get a sorted list of all applicable resource keys.
         *
         * @returns {Array.<Entity>} Sorted array of resource key entities.
         *
         * @function
         * @name spResource.Type#getAllResourceKeys
         */
        Type.prototype.getAllResourceKeys = function getAllResourceKeys() {
            if (!this._resourceKeys) {
                var that = this;                
                var types = _.filter(this.getAllEntities(), function (t) { return t.getResourceKeys; });
                this._resourceKeys = _.chain(types)
                    .map(function (t) { return t.getResourceKeys(); })
                    .flatten(true)
                    .sortBy(spUtils.getter('getName'))
                    .value();
            }

            return this._resourceKeys;
        };

        return Type;
    })();

    spResource.Type = Type;

})(spResource || (spResource = {}));
