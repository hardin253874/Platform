// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, sp */
var spEntity;
var EdcEntity;  // legacy

(function (spEntity) {
    'use strict';
    
    /**
     * RelationshipInstance constructor. Do not use.
     * @private
     * @class
     * @name spEntity.RelationshipInstance
     *
     * @classdesc
     * The RelationshipInstance object holds data for one instance of a relationship from an entity.
     * It is typically not used directly. Rather use the relationship or lookup accessors on the Entity object.
     */
    var RelationshipInstance = (function () {
        function RelationshipInstance(entity, relEntity) {
            // remember CLONE
            if (entity) {
                this.entity = _.isObject(entity) ? entity : spEntity.fromId(entity);
            } else {
                this.entity = null;
            }
            if (relEntity) {
                this.relEntity = _.isObject(relEntity) ? relEntity : spEntity.fromId(relEntity);
            } else {
                this.relEntity = null;
            }
            this._dataState = spEntity.DataStateEnum.Create;
        }

        Object.defineProperties(RelationshipInstance.prototype, {
            'dataState': {
                get: function () { return this.getDataState(); },
                set: function (value) { return this.setDataState(value); },
                enumerable: true
            }
        });


        /**
         * Gets the data state of this relationship instance. Either Create, Delete, or Unchanged. Update is invalid here.
         */
        RelationshipInstance.prototype.getDataState = function getDataState() {
            return this._dataState;
        };

        /**
         * Sets the data state of this relationship instance. Either Create, Delete, or Unchanged. Update is invalid here.
         * @param {spEntity.DataStateEnum} value The new datastate.
         */
        RelationshipInstance.prototype.setDataState = function setDataState(value) {
            this._dataState = value;
        };

        /**
         * Set the relationship entity to the given entity (entity, id, alias, etc)
         * @param value
         */
        RelationshipInstance.prototype.setRelationshipEntity = function (value) {
            this.relEntity = spEntity.asEntity(value);
            this._dataState = this._dataState === spEntity.DataStateEnum.Unchanged ? spEntity.DataStateEnum.Update : this._dataState;
        };

        /**
         * @internal
         * Do not use outside of spEntity.js
         */
        RelationshipInstance.prototype._asRawRelationshipInstance = function () {
            return {
                entity: this.entity ? this.entity.eid()._getIdOrDummyId() : 0,
                relEntity: this.relEntity ? this.relEntity.eid()._getIdOrDummyId() : 0,
                dataState: this._dataState
            };
        };
        return RelationshipInstance;
    })();
    spEntity.RelationshipInstance = RelationshipInstance;




    /**
     * Relationship constructor. Do not use.
     * @private
     * @class
     * @name spEntity.Relationship
     *
     * @classdesc
     * The Relationship object (or relationship container) holds data for all instances of a relationship of a given relationship type from an entity.
     * It is typically not used directly. Rather use the relationship or lookup accessors on the Entity object.
     * 
     */
    var Relationship = (function () {
        // rid can be an alias, object containing {id, isReverse}, or just id (not recommended)
        function Relationship(rid, owner) {
            var that = this;
            if (!owner) {
                throw new Error('Expected owner');
            }
            this.owner = owner;     // the entity that owns this relationship.

            var rid2 = spEntity._Entity.normalizeRelTypeId(rid);
            this.id = spEntity.asEntityRef(rid2.id);

            // remember CLONE
            this.instances = [];    // holds array of relationship instances ( instances of RelationshipInstance )
            this.changeId = 0;      // spEntity is responsible for updating this when ever a relationship is modified.
            this.removeExisting = false;    // flags whether to remove existing relationship instances
            this._deleteExisting = false;   // flags whether to remove existing relationship instances and delete the related entities.
            this.isReverse = rid2.isReverse;
            this.isLookup = false;

            // array of entities
            var entities = [];
            // add modifier helpers
            entities.add = function (value) {
                that.add(value);
            };
            entities.clear = function () {
                that.clear();
            };
            entities.remove = function (value) {
                that.remove(value);
            };
            entities.deleteExisting = function () {
                that.deleteExisting();
            };
            entities.autoCardinality = function () {
                that.autoCardinality();
            };
            entities.deleteEntity = function (value) {
                that.deleteEntity(value);
            };
            entities.getRelationshipContainer = function () {
                return that;
            };
            entities.getInstances = function () {
                return that.getInstances();
            };
            this.entities = entities;
        }

        Relationship.prototype._runAsCommand = function (runFn) {
            // Capture for closures
            var instances = this.instances.slice(0);
            var removeExisting = this.removeExisting;
            var _deleteExisting = this._deleteExisting;
            var _autoCardinality = this._autoCardinality;
            var entities = this.entities.slice(0);
            var oldState = this.owner._dataState;

            var that = this;
            var undoFn = function() {
                that.instances = instances;
                that.removeExisting = removeExisting;
                that._deleteExisting = _deleteExisting;
                that._autoCardinality = _autoCardinality;
                that.entities.length = 0;
                spUtils.pushArray(that.entities, entities);
                that.owner._dataState = oldState;
                that.changeId++;
            };

            var command = {
                run: function() {
                    runFn.call(that);   // wrap to ensure we run with the right 'this'
                },
                undo: undoFn
            };
            this.owner._graph.history.run(command);
        };

        /**
         * @internal
         * Converts an array of entities into new relationship instances.
         */
        Relationship.prototype._entitiesArray = function (entities) {
            var that = this;
            if (!entities) {
                return [];
            } else if (!_.isArray(entities)) {
                entities = [entities];
            }
            var res = entities.map(function(value) {
                var e = spEntity.asEntity(value, { graph: that.owner._graph });
                return e;
            });
            return res;
        };

        /**
         * @internal
         * Converts an array of entities into new relationship instances.
         */
        Relationship.prototype._entitiesToRelInsts = function(entities, dataState) {
            var that = this;
            return entities.map(function (entity) {
                entity._setGraph(that.owner._graph); // while we're here, attach to the graph
                var ri = new spEntity.RelationshipInstance(entity); // dataState=create
                if (dataState){
                    ri._dataState = dataState;
                }
                return ri;
            });
        };

        /**
         * @internal
         * Converts an array of entities into new relationship instances.
         */
        Relationship.prototype._changed = function() {
            this.changeId++;
            this.owner.setDirty();
        };

        /**
         * @internal
         * Gets the currently visible set of entities.
         */
        Relationship.prototype._updateEntitiesFromInstances = function() {
            var that = this;
            var entities = this.instances ? this.instances.map(function (i) {
                // while we're here, attach to the graph
                i.entity._setGraph(that.owner._graph);
                if (i.relEntity) {
                    i.relEntity._setGraph(that.owner._graph);
                }

                return i.entity;
            }) : [];
            this.entities.length = 0;
            sp.pushArray(this.entities, entities);
        };


        /**
         * Creates a new client-side instance of Entity that has the specfied ID.
         * Use this to refer to existing resources. (The 'create on server' flag is not set.)
         *
         * @param {*} value An entity, or ID (number, alias, EntityRef) or an array of any of these.
         *
         * @function
         * @name spEntity.Relationship#add
         */
        Relationship.prototype.add = function (value) {
            this._runAsCommand(function() {
                // convert value(s) into a proper array of entities
                var entities = this._entitiesArray(value);
                // and make corresponding instances, with state=create
                var newInst = this._entitiesToRelInsts(entities);
                // add to entities
                sp.pushArray(this.entities, entities);
                // clear out any existing matching instances
                this.instances = sp.except(this.instances, entities, function(ri, e) {
                    return ri.entity.eid().matches(e.eid());
                });
                // add to instances
                sp.pushArray(this.instances, newInst);
                this._changed();
            });
        };


        /**
         * Removes the relationship to the specified entity or entities.
         *
         * @param {*} value An entity, or ID (number, alias, EntityRef) or an array of any of these
         *            OR a predicate function that identifies the entities (called from the filter function)
         *
         * @function
         * @name spEntity.Relationship#remove
         */
        Relationship.prototype.remove = function (value, _deleteEntityToo) {
            this._runAsCommand(function() {

                if (_.isFunction(value)) {
                    this.remove(_.filter(this.entities, value), _deleteEntityToo);
                    return;
                }

                // convert value(s) to delete into a proper array of entities
                var delEnt = this._entitiesArray(value);

                // clear out any existing matching entities
                var remaining = sp.except(this.entities, delEnt, function(curE, delE) {
                    return curE.eid().matches(delE.eid());
                });
                this.entities.length = 0; // need to reuse the same array instance
                sp.pushArray(this.entities, remaining);

                // clear out any existing matching relationship instances
                this.instances = sp.except(this.instances, delEnt, function(ri, e) {
                    return ri.entity.eid().matches(e.eid());
                });

                // add removal rows to instances
                var genuineDeletes = _.reject(delEnt, { dataState: spEntity.DataStateEnum.Create });
                var delInst = this._entitiesToRelInsts(genuineDeletes, spEntity.DataStateEnum.Delete);
                if (!this.removeExisting) {
                    sp.pushArray(this.instances, delInst);
                }

                // and flag for deletion
                if (_deleteEntityToo) {
                    genuineDeletes.forEach(function (e) {
                        e.dataState = spEntity.DataStateEnum.Delete;
                    });
                }

                this._changed();
            });
        };


        Relationship.prototype.deleteEntity = function (value) {
            this.remove(value, true);
        };


        /**
         * Marks all existing relationship instances for removal in the database.
         * Does not delete the related entities themselves though.
         * Empties the entities and instances arrays.
         *
         * @function
         * @name spEntity.Relationship#clear
         */
        Relationship.prototype.clear = function () {
            this._runAsCommand(function() {
                this.removeExisting = true;
                this.entities.length = 0;
                this.instances.length = 0;
                this._changed();
            });
        };


        /**
         * Mark a relationship as unchanged
         */
        Relationship.prototype.markUnchanged = function () {
            this.removeExisting = false;
            // todo: we should probably also remove relationship instances that are flagged as deletes
            this.getInstances().forEach(function (ri) {
                ri.setDataState(spEntity.DataStateEnum.Unchanged);
            });
        };


        /**
         * Deletes existing related entities (as well as the relationships to them).
         * Empties the entities and instances arrays.
         *
         * @function
         * @name spEntity.Relationship#deleteExisting
         */
        Relationship.prototype.deleteExisting = function () {
            this._runAsCommand(function() {
                this.removeExisting = true;
                this._deleteExisting = true;
                this.entities.length = 0;
                this.instances.length = 0;
                this._changed();
            });
        };


        /**
         * Indicates to the server that it should remove old relationship on either the source or target as required
         * if it would otherwise cause a cardinality conflict.
         *
         * @function
         * @name spEntity.Relationship#autoCardinality
         */
        Relationship.prototype.autoCardinality = function () {
            this._runAsCommand(function () {
                this._autoCardinality = true;
            });
        };


        /**
         * Gets the currently visible set of entities.
         * The resulting array has shortcut functions to the container functions for: add, remove, clear, getInstances.
         * It also has a getRelationshipContainer() function to get to the container.
         *
         * @returns {Array.<spEntity.Entity>} An array of entities being pointed to, with additional functions attached.
         *
         * @function
         * @name spEntity.Relationship#getEntities
         */
        Relationship.prototype.getEntities = function () {
            // Note: rel accessor factories bypasses this
            return this.entities;
        };


        // Do the two lists of entities contain the same ids
        function entityListsEqual(list1, list2) {
            if (list1.length !== list2.length) {
                return false;
            }

            var combinedIds = _.union( _.map(list1, "nsAliasOrId"), _.map(list2, "nsAliasOrId"));

            return combinedIds.length === list1.length;
        }

        /**
         * Sets the currently visible set of entities.
         * Use with caution - it is usually better to call .add, .clear or .remove for specific rows
         * otherwise the entire set of data will be resent and resaved. Relationship instances also get recreated.
         * Note: this can be an entity, alias, ID, or array of the above.
         * RelationshipInstances are manufactores for the entities.
         *
         * @param {*} value An entity, or ID (number, alias, EntityRef) or an array of any of these.
         *
         * @function
         * @name spEntity.Relationship#setEntities
         */
        Relationship.prototype.setEntities = function (value) {

            if (value === this.entities) {
                // note: this 'if' block is only here because some legacy code is directly manipulating the array returned by getEntities, and passing it back in
                // and the attempt to clear the length is causing it to empty itself before setting itself.
                value = value.slice(0); //copy
            }

            var newEntities = this._entitiesArray(value);

            if (!entityListsEqual(newEntities, this.entities)) {        // check if there are really any changes
                this._runAsCommand(function () {
                    this.removeExisting = true; // if we do a 'set' then the given instances represent all the instances

                    this.entities.length = 0;
                    sp.pushArray(this.entities, newEntities);
                    this.instances = this._entitiesToRelInsts(newEntities);
                    this._changed();

                });
            }
        };


        /**
         * Gets the relationship instances collection.
         *
         * @returns {Array.<spEntity.RelationshipInstance>} The relationship instance objects.
         *
         * @function
         * @name spEntity.Relationship#getInstances
         */
        Relationship.prototype.getInstances = function () {
            return this.instances;
        };


        /**
         * Sets the relationship instances collection. Use with caution!
         * @param {Array.<spEntity.RelationshipInstance>} value The new array of relationship instances.
         *
         * @function
         * @name spEntity.Relationship#setInstances
         */
        Relationship.prototype.setInstances = function (value) {
            this._runAsCommand(function() {
                this.removeExisting = true;
                this.instances = value;
                this._updateEntitiesFromInstances();
            });
        };


        /**
         * Sets the relationship instances collection. Use with caution!
         * @param {Array.<spEntity.RelationshipInstance>} value The new array of relationship instances.
         *
         * @function
         * @name spEntity.Relationship#_setInstancesFast
         */
        Relationship.prototype._setInstancesFast = function (value) {
            this.removeExisting = true;
            this.instances = value;
            var entities = _.map(value, 'entity');
            this.entities.length = 0;
            sp.pushArray(this.entities, entities);
        };


        /**
         * Searches for a relationship instance to the specified entity.
         * @param {*} entityId The ID (number, alias, EntityRef) of the entity to find.
         *
         * @returns {RelationshipInstance} The relationship instance object.
         *
         * @function
         * @name spEntity.Relationship#findInstance
         */
        Relationship.prototype.findInstance = function (value) {
            var res = _.find(this.instances, function (i) {
                return i.entity.eid().matches(value);
            });
            return res;
        };
        

        /**
         * @internal
         * Do not use outside of spEntity.js
         */
        Relationship.prototype._asRawRelationship = function() {
            var rr = {
                relTypeId: this.id._asRawEntityRef(),
                instances: this.instances.map(function(i) {
                    return i._asRawRelationshipInstance();
                })
            };
            if (this.removeExisting) {
                rr.removeExisting = this.removeExisting;
            }
            if (this._deleteExisting) {
                rr.deleteExisting = this._deleteExisting;
            }
            if (this._autoCardinality) {
                rr.autoCardinality = this._autoCardinality;
            }
            if (this.isReverse) {
                rr.isReverse = this.isReverse;
            }
            return rr;
        };
        return Relationship;
    })();
    spEntity.Relationship = Relationship;

    
})(spEntity || (spEntity = EdcEntity = {}));

