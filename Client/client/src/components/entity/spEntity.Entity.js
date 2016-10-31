// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, sp, spUtils, spEntityUtils */

/// <reference path="../../Scripts/underscore.js" />
/// <reference path="../js/spUtils.js" />
/// <reference path="../js/spEntity.js" />

var fieldAccess = 0;
var spEntity;
var EdcEntity;  // legacy

(function (spEntity) {
    'use strict';

    /**
     * Entity constructor. Do not use.
     * Use spEntity.fromId or spEntity.createEntityOfType instead.
     * @private
     * @class
     * @name spEntity.Entity
     *
     * @classdesc
     * The Entity object holds fields and relationships.
     * This is the standard object for manipulating entity data on the client. This object is not (directly) what gets passed to network calls.
     */
    var Entity = (function () {

        function Entity(options) {

            // remember CLONE

            options = options || {
            };

            this._graph = options.graph || spEntity._newGraphHandle();

            // EntityRef
            var idIsEntityRef = _.isObject(options.id) && _.isFunction(options.id.getId);
            if (idIsEntityRef) {
                this._id = options.id;
            } else if (options.id) {
                this._id = spEntity.asEntityRef(options.id);
            } else {
                this._id = new spEntity.EntityRef(spEntity._getNextId());
            }

            // EntityRef[]
            this._typeIds = options.typeId ? [
                spEntity.asEntityRef(options.typeId)
            ] : [];

            // Field[]
            this._fields = [];

            // Relationship[]
            this._relationships = [];

            // DataStateEnum
            this._dataState =
                options.dataState !== undefined ? options.dataState :
                options.id || options.alias ? spEntity.DataStateEnum.Unchanged :
                spEntity.DataStateEnum.Create;

            if (this._dataState === spEntity.DataStateEnum.Create && this._typeIds.length === 0) {
                //console.warn('Creating an entity without a type');
                //console.trace();
            }

            this.extendedProperties = {
            };
        }

        Entity.prototype._setGraph = function (graph) {
            if (this._graph === graph)
                return;
            var entities = spEntityUtils.walkEntities(this);
            _.forEach(entities, function(e) {
                e._graph = graph;
            });
        };

        Object.defineProperty(Entity.prototype, "graph", {
            get: function () { return this._graph; },
            enumerable: true
        });

        /*
         * Internal: Factory for generating dynamic field accessors.
         */
        var fieldAccessorFactory = function (memberId) {
            var newAccessor = {
                alias:
                    memberId.getAlias(),
                capAlias:
                    spUtils.capitaliseFirstLetter(memberId.getAlias()),
                getterFactory:
                    function (fieldContainer) {
                        return function () {
                            return fieldContainer._value;
                        };
                    },
                setter:
                    function (value) {
                        return this.setField(memberId, value);
                    }
            };
            return newAccessor;
        };
        spEntity._fieldAccessorFactory = fieldAccessorFactory;


        /*
         * Internal: Factory for generating dynamic lookup accessors.
         * memberId is an object containing {id, isReverse}
         */
        var lookupAccessorFactory = function (memberId) {
            var newAccessor = {
                alias:
                    memberId.id.getAlias(),
                capAlias:
                    spUtils.capitaliseFirstLetter(memberId.id.getAlias()),
                getterFactory:
                    function (relContainer) {
                        return function () {
                            return relContainer.entities[0] || null;
                        };
                    },
                setter:
                    function (value) {
                        return this.setLookup(memberId, value);
                    }
            };
            return newAccessor;
        };
        spEntity._lookupAccessorFactory = lookupAccessorFactory;


        /*
         * Internal: Factory for generating dynamic relationship accessors.
         * memberId is an object containing {id, isReverse}
         */
        var relAccessorFactory = function (memberId) {
            var newAccessor = {
                alias:
                    memberId.id.getAlias(),
                capAlias:
                    spUtils.capitaliseFirstLetter(memberId.id.getAlias()),
                getterFactory:
                    function (relContainer) {
                        return function () {
                            return relContainer.entities;
                        };
                    },
                setter:
                    function (value) {
                        return this.setRelationship(memberId, value);
                    }
            };
            return newAccessor;
        };
        spEntity._relAccessorFactory = relAccessorFactory;


        /*
         * Internal: Registers accessors onto an entity.
         */
        var addAccessor = function (entity, accessor, container) {
            var accCapName = accessor.capAlias;
            var accName = accessor.alias;
            var thisGetter = accessor.getterFactory(container);
            var getterName = 'get' + accCapName;
            var setterName = 'set' + accCapName;

            // If someone is trying to register name, then override the prototype, because the accessor will be faster
            var justDoIt = (accName === 'name' && !entity.hasOwnProperty('name'));

            if (justDoIt || !(getterName in entity)) {
                entity[getterName] = thisGetter;
            }
            if (justDoIt || !(setterName in entity)) {
                entity[setterName] = accessor.setter;
            }
            if (justDoIt || !(accName in entity)) {
                Object.defineProperty(entity, accName, {
                    get: thisGetter,
                    set: accessor.setter,
                    enumerable: true,
                    configurable: true
                });
            }
        };
        spEntity._addAccessor = addAccessor;


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Basic properties
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        Object.defineProperties(Entity.prototype, {
            'idP': {
                get: function() { return this._id.id(); },
                enumerable: true
            },
            'eidP': {
                get: function() { return this._id; },
                enumerable: true
            },
            'nsAlias': {
                get: function () { return this._id.getNsAlias(); },
                enumerable: true
            },
            'nsAliasOrId': {
                get: function () { return this._id.nsAliasOrId; },
                enumerable: true
            },
            'aliasOnly': {
                get: function () { return this._id.getAlias(); },
                enumerable: true
            },
            'debugString': {
                get: function () {
                    var s = '';
                    if (this._id) {
                        s += 'id:' + this.idP;
                        s += ' "' + this.nsAlias + '"';
                        s += ' name: "' + this.name + '"';
                        var types = this.typesP;
                        if (types && types.length) {
                            s += ' type:' + types[0].idP + ' "' + types[0].nsAlias + '"';
                        }
                    }
                    return s;
                },
                enumerable: true
            },
            'isNew': {
                get: function () { return this.dataState === spEntity.DataStateEnum.Create; },
                enumerable: true
            }
        });

        Entity.prototype.eid = function() {
            return this._id;
        };
        Entity.prototype.id = function() {
            return this._id.id();
        };
        Entity.prototype.alias = function() {
            return this._id.alias();
        };
        Entity.prototype.aliasOrId = function() {
            return this._id.alias() || this._id.id();
        };

        Entity.prototype.setEid = function (eid) {
            this._id = eid;
        };
        
        Entity.prototype.setId = function (id) {
            this._id = new spEntity.EntityRef({
                id: id,
                ns: this._id.getNamespace(),
                alias: this._id.getAlias()
            });
        };

        /**
         * Gets the alias. Deprecated. Use nsAlias instead.
         */
        Entity.prototype.getAlias = function () {
            return this.nsAlias;
        };
        
        /**
         * Gets the name of the resource.
         * Note: this is just a convenient accessor so that name doesn't need to be explicitly registered.
         *
         * @returns {string} The resource name.
         *
         * @function
         * @name spEntity.Entity#getName
         */
        Entity.prototype.getName = function () {
            return this.getField('core:name');
        };

        /**
         * Sets the name of the resource.
         * Note: this is just a convenient accessor so that name doesn't need to be explicitly registered.
         *
         * @param {string} The resource name.
         * @returns {spEntity.Entity} This entity for chaining.
         *
         * @function
         * @name spEntity.Entity#setName
         */
        Entity.prototype.setName = function (value) {
            return this.setField('core:name', value, spEntity.DataType.String);
        };

        /**
         * Gets the data state of this entity. Either Create, Delete, Update or Unchanged.
         */
        Entity.prototype.getDataState = function getDataState() {
            return this._dataState;
        };

        /**
         * Sets the data state of this entity. Either Create, Delete, Update or Unchanged.
         * @param {spEntity.DataStateEnum} value The new datastate.
         */
        Entity.prototype.setDataState = function setDataState(value) {
            this._dataState = value;
        };


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Types
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        Object.defineProperties(Entity.prototype,  {
            'typesP': {
                get: function () { return this.getTypes(); },
                set: function (value) { return this.setTypes(value); },
                enumerable: true
            },
            'type': {
                get: function () { return this.getType(); },
                set: function (value) { return this.setTypes([value]); },
                enumerable: true
            },
            'dataState': {
                get: function () { return this.getDataState(); },
                set: function (value) { return this.setDataState(value); },
                enumerable: true
            },
            'name': {
                get: function () { return this.getName(); },
                set: function (value) { return this.setName(value); },
                enumerable: true
            }
        });

        /**
         * Gets the type(s) of this entity.
         *
         * @returns {Array<EntityRef>} The type(s) of this entity.
         *
         * @function
         * @name spEntity.Entity#getTypes
         */
        Entity.prototype.getTypes = function () {
            return this._typeIds;
        };


        /**
         * Sets the type(s) of this entity.
         *
         * @function
         * @param {Array<EntityRef>} value The first type of this entity.
         * @returns {spEntity.Entity} This entity for chaining.
         *
         * @function
         * @name spEntity.Entity#setTypes
         */
        Entity.prototype.setTypes = function (value) {
            if (!_.isArray(value)) {
                value = [
                    value
                ];
            }
            var oldState = this._dataState;
            var oldTypes = this._typeIds;
            var newTypes = value.map(spEntity.asEntityRef);
            var that = this;

            this._graph.history.run({
                run: function () {
                    that._typeIds = newTypes;
                    that.setDirty();
                },
                undo: function () {
                    that._typeIds = oldTypes;
                    that._dataState = oldState;
                }
            });
            
            return this;
        };


        /**
         * The type of this entity (or one type if there are multiple).
         *
         * @returns {EntityRef} The first type of this entity.
         *
         * @function
         * @name spEntity.Entity#getType
         */
        Entity.prototype.getType = function () {
            if (!this._typeIds)
                return new spEntity.EntityRef(0);
            if (this._typeIds.length < 1)
                return new spEntity.EntityRef(0);
            return this._typeIds[0];
        };


        /*
         * DEPRECATED - Use getTypes/setTypes instead.
         */
        Entity.prototype.types = function (value) {
            if (_.isUndefined(value)) {
                return this.getTypes();
            }
            return this.setTypes(value);
        };


        /*
         * DEPRECATED - Use getType()
         */
        Entity.prototype.firstTypeId = function () {
            if (!this.getType()) {
                console.warn('returning falsy from firstTypeId on ', this);
                console.trace();
            }
            return this.getType();
        };


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Fields
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /**
         * Gets the field container that contains the field value.
         *
         * @param {string|number} fieldId The ID or alias of the field to access.
         * @returns {spEntity.Field} The Field object that contains the field data.
         *
         * @function
         * @name spEntity.Entity#getFieldContainer
         */
        Entity.prototype.getFieldContainer = function (fieldId) {
            var field = _.find(this._fields, function (f) {
                return f.id.matches(fieldId);
            });
            return field;
        };


        /**
         * Accepts a field alias, and returns the current value of that field.
         *
         * @param {string|number} fieldId The ID or alias of the field to access.
         * @returns {*} The current value.
         *
         * @function
         * @name spEntity.Entity#getField
         */
        Entity.prototype.getField = function (fieldId) {
            var field = this.getFieldContainer(fieldId);
            var fieldValue = field ? field._getValue() : null;
            return fieldValue;
        };


        /**
         * Sets the value of a field.
         *
         * @param {string|number} fieldId The ID or alias of the field to modify.
         * @param {*} value New value.
         * @param {string} typeName The field data type.
         * @returns {spEntity.Entity} The current entity for chaining.
         *
         * @function
         * @name spEntity.Entity#setField
         */
        Entity.prototype.setField = function (fieldId, value, typeName) {
            var that = this;
            var oldDataState = this._dataState;

            var field = this.getFieldContainer(fieldId);
            if (!field) {
                field = this.registerField(fieldId, typeName);
                field._setValue(value);

                // TODO: consider whether field registration can be undone
                this._graph.history.run({
                    run: function () {
                        that.setDirty();
                    },
                    undo: function () {
                        that._dataState = oldDataState;
                    }
                });

            } else {
                var oldValue = field._getValue();
                var newValue = field._prepareValue(value);
                var oldSer = field._convertNativeToRaw(field._dataType, oldValue);
                var newSer = field._convertNativeToRaw(field._dataType, newValue);

                if (oldSer !== newSer) {
                    this._graph.history.run({
                        run: function () {
                            field._setValue(newValue);
                            that.setDirty();
                        },
                        undo: function () {
                            field._setValue(oldValue);
                            that._dataState = oldDataState;
                        }
                    });                    
                }
            }
            return this;
        };


        /**
         * Registers a field on an entity, so that its accessor functions can be used.
         *
         * @param {string|number} fieldId The ID or alias of the field to register.
         * @param {spEntity.DataType} dataType The field data type.
         * @returns {spEntity.Field} The field container.
         *
         * @function
         * @name spEntity.Entity#registerField
         */
        Entity.prototype.registerField = function (fieldId, dataType) {
            var fieldId2 = spEntity.asEntityRef(fieldId);
            var field = this.getFieldContainer(fieldId2);
            if (!field) {
                field = new spEntity.Field(fieldId2, dataType);
                field._setValue(null);
                this._fields.push(field);

                // register accessor functions
                if (_.isFunction(fieldId2.getAlias) &&
                    fieldId2.getAlias()) {
                    addAccessor(this, fieldAccessorFactory(fieldId2), field);
                }
            }
            return field;
        };


        /**
         * Detects whether a field is registered and known for this entity.
         *
         * @param {string|number} relTypeId Number or alias.
         * @returns {bool} True if the relationship is known.
         *
         * @function
         * @name spEntity.Entity#hasField
         */
        Entity.prototype.hasField = function (fieldId) {
            var res = this.getFieldContainer(fieldId) != null;
            return res;
        };


        /*
         * DEPRECATED - Use getField / setField
         */
        Entity.prototype.field = function (idOrAlias, value, typeName) {
            if (_.isUndefined(value)) {
                return this.getField(idOrAlias);
            }
            return this.setField(idOrAlias, value, typeName);
        };


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Relationships
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /**
         * Converts a relationship type ID into a fixed form: an object that has an id property containing an EntityRef
         * and an isReverse bool property.
         *
         * E.g. pass: 'myalias', 'test:myalias', {'myalias',isReverse:false}, {123,isReverse:true}, etc..
         *
         * @param {string|number} relTypeId Number or alias. Or entity ref. Or object with id set to the above with an isReverse property.
         * @returns {spEntity.Relationship} The relationship container object.
         *
         * @function
         * @name spEntity.Entity.normalizeRelTypeId
         */
        var normalizeRelTypeId = Entity.normalizeRelTypeId = function normalizeRelTypeId(relTypeId) {
            var res;
            if (relTypeId.hasOwnProperty('isReverse')) {
                if (relTypeId.id instanceof spEntity.EntityRef) {
                    res = relTypeId; // already normalized
                } else {
                    res = {
                        id: spEntity.asEntityRef(relTypeId.id),
                        isReverse: relTypeId.isReverse
                    };
                }
            } else {
                var entRef = spEntity.asEntityRef(relTypeId);
                //if (entRef.id() > 0)
                //    alert('You must specify the relationship direction when passing by ID.');
                res = {
                    id: entRef,
                    isReverse: false
                };
            }
            return res;
        };

        /**
         * Gets the relationship container that contains the relationship instances, along with change state information.
         * There is one of these per-entity per-relationship-type per-direction (if both directions were loaded).
         * The 'changeId' field on the relationship container can be used in angular watches.
         *
         * @param {string|number} relTypeId Number or alias.
         * @returns {spEntity.Relationship} The relationship container object.
         *
         * @function
         * @name spEntity.Entity#getRelationshipContainer
         */
        Entity.prototype.getRelationshipContainer = function (relTypeId) {
            // Note: rel accessor factories bypasses this
            var rid = normalizeRelTypeId(relTypeId);
            var rel = _.find(this._relationships, function (r) {
                var res = (r.isReverse === rid.isReverse) && (r.id.matches(rid.id));
                return res;
            });
            return rel;
        };


        /**
         * Registers a relationship (to-many) on an entity, so that its accessor functions can be used.
         *
         * @param {string|number} fieldId The ID or alias of the relationship to register.
         * @returns {spEntity.Relationship} The relationship container.
         *
         * @function
         * @name spEntity.Entity#registerRelationship
         */
        Entity.prototype.registerRelationship = function (relTypeId) {
            var relId2 = normalizeRelTypeId(relTypeId);
            var rel = this.getRelationshipContainer(relId2);
            if (!rel) {
                rel = new spEntity.Relationship(relId2, this);
                this._relationships.push(rel);

                // register accessor functions
                if (relId2.id.getAlias()) {
                    addAccessor(this, relAccessorFactory(relId2), rel);
                }
            }
            return rel;
        };


        /**
         * Detects whether a relationship is registered and known for this entity.
         *
         * @param {string|number} relTypeId Number or alias.
         * @returns {bool} True if the relationship is known.
         *
         * @function
         * @name spEntity.Entity#hasRelationship
         */
        Entity.prototype.hasRelationship = function (relTypeId) {
            var res = this.getRelationshipContainer(relTypeId) != null;
            return res;
        };


        /**
         * Accepts a relationship alias, and returns an array containing the related entities.
         * (Note that this API does not provide access to the relationship instance entities.)
         *
         * @param {string|number} relTypeId Number or alias.
         * @returns {Array<spEntity.Entity>} The current values.
         *
         * @function
         * @name spEntity.Entity#getRelationship
         */
        Entity.prototype.getRelationship = function (relTypeId) {
            // Note: rel accessor factories bypasses this
            var rel = this.getRelationshipContainer(relTypeId);
            if (!rel) {
                return [];  // for convenience of caller. Use hasRelationship if you really care about whether it exists
            }
            var res = rel.getEntities();
            return res;
        };


        /**
         * Accepts a relationship alias, and array of entities, and sets those entities to be the only entities that are related to.
         * @param {string|number} relTypeId Number or alias.
         * @param {Array<Entity>} entities Array of new entities. Or array of IDs/aliases, and entities will be created.
         * @returns {spEntity.Entity} The current entity, for chaining.
         *
         * @name spEntity.Entity#setRelationship
         * @function
         */
        Entity.prototype.setRelationship = function (relTypeId, entities) {
            var rel = this.registerRelationship(relTypeId); // get existing or register new
            rel.setEntities(entities);
            return this;
        };


        /*
         * DEPRECATED - Use getRelationship / setRelationship instead
         */
        Entity.prototype.relationship = function(relTypeId, entities) {
            if (_.isUndefined(entities)) {
                return this.getRelationship(relTypeId);
            }
            return this.setRelationship(relTypeId, entities);
        };


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Lookups
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /**
         * Returns a single related entity for the specified relationship.
         *
         * @param {string|number} relTypeId Number or alias.
         * @returns {spEntity.Entity} The current related entity or null.
         *
         * @function
         * @name spEntity.Entity#getLookup
         */
        Entity.prototype.getLookup = function (relTypeId) {
            var entities = this.getRelationship(relTypeId);
            return entities && entities.length > 0 ? entities[0] : null;
        };


        /**
         * Registers a lookup (to-one) on an entity, so that its accessor functions can be used.
         *
         * @param {string|number} fieldId The ID or alias of the relationship to register.
         * @param {object} options A list of options. Pass isReverse=true to indicate that the relationship is in the reverse direction.
         * @returns {spEntity.Relationship} The relationship container.
         *
         * @function
         * @name spEntity.Entity#registerLookup
         */
        Entity.prototype.registerLookup = function (relTypeId, options) {
            var relId2 = normalizeRelTypeId(relTypeId);
            var rel = this.getRelationshipContainer(relId2);
            if (!rel) {
                rel = new spEntity.Relationship(relId2, this);
                rel.isLookup = true;
                this._relationships.push(rel);

                // register accessor functions
                if (relId2.id.getAlias()) {
                    addAccessor(this, lookupAccessorFactory(relId2), rel);
                }
            }
            return rel;
        };


        /**
         * Accepts a relationship alias, and array of entities, and sets those entities to be the only entities that are related to.
         *
         * @param {string|number} relTypeId Number or alias.
         * @param {Array<Entity>} entity New value.
         * @returns {spEntity.Entity} The current entity, for chaining.
         *
         * @function
         * @name spEntity.Entity#setLookup
         */
        Entity.prototype.setLookup = function (relTypeId, entity) {
            var relTypeId2 = normalizeRelTypeId(relTypeId);
            var existing = this.getLookup(relTypeId2);

            // we don't want to set the change flag unless there is actually a change.
            var changed;
            if (existing == null) {
                // changed if nulls don't match, or if this relationship is being registered for the 1st time
                var hasRel = this.hasRelationship(relTypeId2);
                if (!hasRel) {
                    this.registerLookup(relTypeId2);
                }
                changed = (existing != entity) || !hasRel;
            }
            else if (entity == null) {
                // since there is an existing value
                changed = true;
            } else if (spEntity.isEntity(entity)) {
                // changed if instance doesn't match (as they may have different data)
                changed = existing !== entity;
            } else {
                // if just setting by ID, then only mark as changed if the ID has changed
                var er = spEntity.asEntityRef(entity);
                changed = !existing.eid().matches(er);
            }

            if (changed) {
                this.setRelationship(relTypeId2, entity);
            }
            return this;
        };


        /**
         * Wraps a lookup with a getter/setter that can be used as a property to bind to the alias.
         * If namespace is core, it must not be specified. Otherwise it must be specified.
         * E.g. <select ng-model="model.entity.lookupAsProp('lookupAlias').value">
         *        <option value="stackMethodStacked">Stacked</option> ..
         *
         * @param {string|number} relTypeId Number or alias.
         * @returns {spEntity.Entity} An object with a property 'value' that can be used to get/set the property by alias.
         *
         * @function
         * @name spEntity.Entity#lookupAsProp
         */
        Entity.prototype.lookupAsProp = function (relTypeId, defaultAlias) {
            var that = this;
            var getter = function () {
                var e = that.getLookup(relTypeId);
                var alias = e ? e.nsAlias : (defaultAlias || '');
                if (alias.slice(0, 5) === 'core:')
                    alias = alias.slice(5);
                return alias;
            };
            var setter = function(value) {
                return that.setLookup(relTypeId, value === '' ? null : value);
            };
            var res = sp.asProp(getter, setter);
            return res;
        };


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Relationship Instances
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////


        Entity.prototype.relationshipInstances = /** DEPRECATED
        * Get or set the relationship instances for a relationship type. An instance is an entity plus
        * an optional 'relationship entity' that may exist to describe the relationship itself.
        * This is similar to the relationship() method except that that only deals with the related entity
        * whereas this method also provides access to the 'relationship entity'.
        */
        function (relTypeId, relInstances) {
            var rel = this.registerRelationship(relTypeId);
            if (!relInstances) {
                return rel.getInstances();
            }
            rel.setInstances(relInstances);
            return this;
        };
        
        Entity.prototype.mergeRelationship = function(relTypeId, entities) {
            var existing = this.relationship(relTypeId) || [];
            if (!_.isArray(entities)) {
                entities = [
                    entities
                ];
            }
            this.relationship(relTypeId, _.union(existing, entities));
        };
        Entity.prototype.findRelated = function(relTypeId, idOrAlias) {
            return _.find(this.relationship(relTypeId), function(t) {
                return t.eid().matches(idOrAlias);
            });
        };

        /**
         * @internal
         * Do not use outside of spEntity.js
         */
        Entity.prototype._updateEntityData = function (entityData, options, addedEntities) {
            // options={changesOnly:bool}

            var entityCount, dontSkip;

            // handle cyclic relationships
            addedEntities = addedEntities || [];
            if (addedEntities.indexOf(this) >= 0) {
                return;// already done, leave now

            }
            addedEntities.push(this);
            // add my id to entityData.entityRefs, merging with any existing
            spEntity.EntityRef.mergeEntityRef(entityData.entityRefs, this._id);
            // Recursively check my related entities. Take a note if this found any changed entities and added them to entityData.
            // If we do find any then we don't skip this one, even if it is itself unchanged. We need this so the 
            // service can follow however many relationships to get to the changed entities.
            entityCount = entityData.entities.length;
            this._relationships.forEach(function(r) {
                r.instances.forEach(function(i) {
                    if (i.entity) {
                        i.entity._updateEntityData(entityData, options, addedEntities);
                    }
                    if (i.relEntity) {
                        i.relEntity._updateEntityData(entityData, options, addedEntities);
                    }
                });
            });
            dontSkip = entityCount !== entityData.entities.length;
            //console.log('spEntity .... dontSkip', dontSkip, entityCount, entityData.entities.length);
            
            
            // skip out if this entity is unchanged. Note we've already handled above any related entities that may have themselves changed
            // however must include any entity that is in our ids list (mostly the root entity)
            var thisId = this._id._getIdOrDummyId();
            if (!dontSkip && this._dataState == spEntity.DataStateEnum.Unchanged && !_.includes(entityData.ids, thisId)) {
                return;
            }
            // add me to entityData.entities, merging with any existing
            var rawEntity = _.find(entityData.entities, _.bind(function(e) {
                return this._id.matches(e.id);
            }, this));
            if (!rawEntity) {
                rawEntity = {
                    id: this._id._getIdOrDummyId(),
                    typeIds: [],
                    fields: [],
                    relationships: [],
                    dataState: this._dataState
                };
                entityData.entities.push(rawEntity);
            } else {
                if (rawEntity.dataState != this._dataState) {
                    if (this._dataState == spEntity.DataStateEnum.Create || rawEntity.dataState == spEntity.DataStateEnum.Unchanged) {
                        rawEntity.dataState = this._dataState;
                    }
                }
            }
            // merge my typeids into both entityRefs and entity typeIds

            spEntity.EntityRef.mergeEntityRefs(entityData.entityRefs, this._typeIds);
            rawEntity.typeIds = _.union(rawEntity.typeIds, this._typeIds.map(function(t) {
                return t._getIdOrDummyId();
            }));
            // merge my fields
            _.forEach(this._fields, function (field) {
                var fieldRaw = field._asRawField(options);
                if (fieldRaw) {
                    spEntity.EntityRef.mergeEntityRef(entityData.entityRefs, field.id);
                    rawEntity.fields.push(fieldRaw);
                }
            });
            // merge my relationships
            _.forEach(this._relationships, _.bind(function (rel) {
                var relRaw = rel._asRawRelationship();
                if (this._dataState === spEntity.DataStateEnum.Create && rel.id.nsAlias === 'core:isOfType')
                    return; // don't record isOfType rel in creates because it conflicts with the type info and causes the server to not write the type rel
                if (relRaw) {
                    spEntity.EntityRef.mergeEntityRef(entityData.entityRefs, rel.id);
                    rawEntity.relationships.push(relRaw);
                }
            }, this));
        };


        /**
         * Marks an entity as being changed, if it was previously marked as unchanged.
         * (Entities marked as 'create' or 'delete' remain at those states.)
         */
        Entity.prototype.setDirty = function() {
            if (this._dataState == spEntity.DataStateEnum.Unchanged) {
                this._dataState = spEntity.DataStateEnum.Update;
            }
        };


        /**
         * Clears the dataState of all entities in a graph back to unchanged.
         *
         * @param {spEntity.Entity} entity The entity to start walking the graph from.
         * @returns {spEntity.Entity} The entity that was passed in, for chaining.
         *
         * @function
         * @name spEntity.Entity#markAllUnchanged
         */
        Entity.prototype.markAllUnchanged = function markAllUnchanged() {
            var entities = spEntityUtils.walkEntities(this);
            entities.forEach(function (e) {
                e._relationships.forEach(function (r) {
                    r.markUnchanged();
                });
                e.setDataState(spEntity.DataStateEnum.Unchanged);
            });
            return this;
        };

        
        /**
         * Determines if there are any changes (create, delete, update) anywhere within the model.
         *
         * @param {spEntity.Entity} entity The entity to start walking the graph from.
         * @returns {spEntity.Entity} True if there are any changes, otherwise false.
         *
         * @function
         * @name spEntity.Entity#hasChangesRecursive
         */
        Entity.prototype.hasChangesRecursive = function hasChangesRecursive() {
            var entities = spEntityUtils.walkEntities(this);
            var anyChanges = _.some(entities, function (e) {
                var hasChange = e.getDataState() != spEntity.DataStateEnum.Unchanged;
                return hasChange;
            });
            return anyChanges;
        };


        /**
         * Determines if there are any changes (create, delete, update) anywhere within the model.         
         *
         * @param {spEntity.Entity} entity The entity to start walking the graph from.
         * @returns {spEntity.Entity} True if there are any changes, otherwise false.
         *
         * @function
         * @name spEntity.Entity#cloneDeep
         */
        Entity.prototype.cloneDeep = function cloneDeep(options) {
            options = options || {};
            _.defaults(options, {
                preserveFieldTracking: true,
                includeDeleted: true,
                ignoreRelationships: null
            });

            // Call walk entities and ensure that deleted instances
            // are retrieved. This is due to the fact that the clone
            // walks the deleted instances.
            var entities = spEntityUtils.walkEntities(this, { includeDeleted: true, ignoreRelationships: options.ignoreRelationships });
            
            var dict = new spEntity.Dict();

            // pre-create all entities so we can wire references
            _.forEach(entities, function (e) {
                var er = new spEntity.EntityRef(e.eid()); // clone entityref
                var eNew = new spEntity._Entity({ id: er });                
                dict.add(e, eNew);
            });


            // copy entities
            _.forEach(entities, function (e) {
                var eOld = e;
                // get the new entity from the dictionary
                // to ensure that all the fields are cloned
                // when there are multiple instances of the
                // same entity in the source graph.
                var eNew = dict.get(e);
                // copy fields
                _.forEach(eOld._fields, function (f) {
                    eNew.setField(f.id, _.cloneDeep(f._getValue()), f._dataType);    // to ensure registation is done

                    if (options.preserveFieldTracking && f._wasPristine) {
                        var newField = eNew.getFieldContainer(f.id);
                        newField._wasPristine = true;
                        newField._pristine = f._pristine;
                        if (!f._pristine) {
                            newField._initRaw = f._initRaw;
                        }
                    }                  
                });
                // copy relationships
                _.forEach(eOld._relationships, function (r) {

                    // 'clone', 'cloneref' or 'ignore'
                    var cloneMode = (options.ignoreRelationships && options.ignoreRelationships(r)) || 'clone';

                    if (cloneMode === 'ignore') {
                        //do not copy the ignore relationships to new entity
                    } else {
                        var newR;
                        if (r.isLookup)
                            newR = eNew.registerLookup(r.id);
                        else
                            newR = eNew.registerRelationship(r.id);
                        newR.isReverse = r.isReverse;
                        newR.removeExisting = r.removeExisting;
                        var instances = options.includeDeleted ? r.instances : _.filter(r.instances, function (instance) { return instance._dataState !== spEntity.DataStateEnum.Delete; });

                        // copy rel instances
                        newR.instances = _.map(instances, function (ri) {
                            var newEntity;
                            if (!dict.contains(ri.entity)) {
                                // cloneref .. just refer to existing entity (but only capture id/alias)
                                newEntity = spEntity.fromId(ri.entity);
                            } else {
                                // clone by value
                                newEntity = dict.get(ri.entity);
                            }
                            var newRi = new spEntity.RelationshipInstance(newEntity, null);
                            newRi._dataState = ri._dataState;
                            return newRi;
                        });
                        newR._updateEntitiesFromInstances();
                    }                    
                });
                eNew._id = new spEntity.EntityRef(eOld._id);
                eNew._typeIds = _.map(eOld._typeIds, function(er) { return new spEntity.EntityRef(er); });
                eNew._dataState = eOld._dataState;
            });

            var res = dict.get(this);
            return res;
        };
        

        return Entity;
    })();
    spEntity._Entity = Entity;

})(spEntity || (spEntity = EdcEntity = {}));

