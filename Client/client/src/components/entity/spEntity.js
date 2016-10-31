// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spEntityUtils */

/// <reference path="../../Scripts/underscore.js" />
/// <reference path="../js/spUtils.js" />
/// <reference path="../js/spEntity.Entity.js" />

//todo: get consistent on whether we have the named function style or just directly assign to the exported variable

/**
 * Module for manipulating entities, relationships and field data.
 * @namespace spEntity
 */
var spEntity;
var EdcEntity;  // legacy

(function (spEntity) {

    // The following is used for ids for new entities (and fields, types and relationships)
    // as well as for entities with known aliases - the server-side service logic will resolve.
    // Note: 2 ^ 53 is the largest javascript number (I think) - that's the reason for using it.
    spEntity._baseTempId = Math.pow(2, 53);
    var nextId = spEntity._baseTempId;
    function getNextId() {
        return --nextId;
    }
    spEntity._getNextId = getNextId;


    /**
     * DataStateEnum constructor. Do not use.
     * @private
     * @class
     * @name spEntity.DataStateEnum
     *
     * @classdesc
     * The DataStateEnum represents what type of changes have been made to an entity or relationship instance.
     * These flags are used to update data on the server when putEntity is called.
     */
    var DataStateEnum = (function() {
        function DataStateEnum() { }

        /** Indicates that an entity has not changed since being loaded.
         * @name spEntity.DataStateEnum.Unchanged
         */
        DataStateEnum.Unchanged = 'unchanged';

        /** Indicates that a new entity, or relationship instance, has been created locally and will be created on the server.
         * @name spEntity.DataStateEnum.Create
         */
        DataStateEnum.Create = 'create';

        /** Indicates that the fields or relationship instances of an entity have been modified.
         * @name spEntity.DataStateEnum.Update
         */
        DataStateEnum.Update = 'update';

        /** Indicates that an entity, or relationship instance, has been flagged for deletion.
         * @name spEntity.DataStateEnum.Delete
         */
        DataStateEnum.Delete = 'delete';
        return DataStateEnum;
    })();
    spEntity.DataStateEnum = DataStateEnum;

    
    /**
     * Creates a new client-side instance of Entity that has the specfied ID.
     * Use this to refer to existing resources. (The 'create on server' flag is not set.)
     *
     * @param {*} entityId The ID (number, alias, EntityRef) of the resource.
     * @returns {Entity} Entity object for use in client.
     *
     * @function
     * @name spEntity.fromId
     */
    spEntity.fromId = function fromId(entityId) {
        var opts = {
            id: spEntity.asEntityRef(entityId),
            dataState: spEntity.DataStateEnum.Unchanged
        };
        var e = new spEntity._Entity(opts);
        return e;
    };


    /**
     * Check a related entity on an entity to ensure that it hasn't accidentally included changes.
     */
    spEntity.ensureUnchanged = function ensureUnchanged(entity, relPropName) {
        if (!entity)
            return;
        var curValue = entity[relPropName];
        if (!curValue)
            return;
        if (curValue.dataState === spEntity.DataStateEnum.Unchanged)
            return;
        entity[relPropName] = null; // set to null first, or the diff detection may not accept the change
        entity[relPropName] = curValue.idP; // set by ID, causes a new unchanged entity with that ID.
    };


    /**
     * Serializes an entity or array of entities into a plain json object structure.
     * IMPORTANT: This is currently only suitable for caching unmodified data. See notes in entitiesToEntityDataVer2.
     *
     * @param {object} entities Entity or array of entities.
     * @returns {object} JSON object structure. DO NOT rely on its format.
     *
     * @function
     * @name spEntity.serialize
     */
    spEntity.serialize = function serialize(entities) {
        var data = spEntity.entitiesToEntityDataVer2(entities); 
        var res = {
            ver: 2,
            isArr: _.isArray(entities),
            data: data
        };
        return res;
    };

    /**
     * Deserializes data that was serialized with spEntity.serialize to an entity or array of entities.
     * Returns null if the data was not in the correct format.
     *
     * @param {object} json JSON object structure. DO NOT rely on its format.
     * @returns {object} Entity or array of entities.
     *
     * @function
     * @name spEntity.deserialize
     */
    spEntity.deserialize = function deserialize(json) {
        if (!json) return json;
        if (json.ver !== 2 || !json.data) {
            console.log('Could not deserialize version' + json.ver);
            return null;
        }
        var entities = spEntity.entityDataVer2ToEntities(json.data);
        if (!entities)
            return null;
        var res = json.isArr ? entities : entities[0];
        return res;
    };


    /**
     * Creates a new client-side instance of Entity that has the specfied ID.
     * Use this to refer to new entities that are to be created. (The 'create on server' flag is set)
     *
     * @param {*} typeId The ID (number, alias, EntityRef) of the definition to create.
     * @param {string} name Optional. The name of the entity.
     * @param {string} desc Optional. The description of the entity.
     * @returns {Entity} Entity object for use in client.
     *
     * @function
     * @name spEntity.createEntityOfType
     */
    spEntity.createEntityOfType = function createEntityOfType(typeId, name, desc) {
        var opts = {
            id: spEntity._getNextId(),
            typeId: spEntity.asEntityRef(typeId),
            dataState: spEntity.DataStateEnum.Create
        };
        var e = new spEntity._Entity(opts);
        if (name) {
            e.setName(name);
        }
        if (desc) {
            e.registerField('core:description', spEntity.DataType.String);
            e.setDescription(desc);
        }
        return e;
    };


    /**
     * Returns true if an object is an entity.
     *
     * @param {object} value value to test.
     * @returns {bool} True if an entity, otherwise false.
     *
     * @function
     * @name spEntity.isEntity
     */
    spEntity.isEntity = function isEntity(value) {
        var result = (value instanceof spEntity._Entity);
        return result;
    };


    /**
     * Converts a set of Entities to JSON entityData suitable for sending to the server via WebAPI calls.
     *
     * @param {Array.<Entity>} entities Array of entities.
     * @returns {object} Raw entity JSON.
     *
     * @function
     * @name spEntity.entitiesToEntityData
     */
    function entitiesToEntityData(entities, options) {
        options = options || { changesOnly: true };

        var entityData = {
            ids: [],
            entities: [],
            entityRefs: []
        };
        if (!_.isArray(entities)) {
            entities = [
                entities
            ];
        }
        entities.forEach(function(e) {
            entityData.ids.push(e._id._getIdOrDummyId());
            e._updateEntityData(entityData, options);
        });
        return entityData;
    }
    spEntity.entitiesToEntityData = entitiesToEntityData;


    /**
     * Finds an entity in a list of entities, by its ID.
     *
     * @param {Array.<Entity>} entities Array of entities.
     * @returns {Entity} The entity.
     *
     * @function
     * @name spEntity.findByEid
     */
    spEntity.findByEid = function findByEid(entities, eid) {
        return _.find(entities, function (e) {
            return e.eid().matches(eid);
        });
    };


    /**
     * Finds the specified entity in a graph of entities.
     *
     * @param {spEntity.Entity} entity The entity to start searching from.
     * @param {string|number} entityRef The alias or ID of the entity.
     * @returns {spEntity.Entity} The first entity with a matching ID/alias.
     *
     * @function
     * @name spEntity.findInGraph
     */
    spEntity.findInGraph = function (entity, entityRef) {
        var er = spEntity.asEntityRef(entityRef);
        var all = spEntityUtils.walkEntities(entity);
        var res = spEntity.findByEid(all, er);
        return res;
    };


    /**
     * Create a new graph instance.
     *
     * @function
     * @name spEntity._newGraphHandle
     */
    spEntity._newGraphHandle = function () {
        return {
            history: new spUtils.History()
        };
    };


    /**
     * Walks through the primaryEntity graph, comparing to the additionalEntity.
     * If any fields/relationships are missing (unregistered), then they are cloned
     * over from the additional entity.
     * The primaryEntity itself gets updated.
     *
     * @param {spEntity.Entity} primaryEntity The entity to be updated (which takes precedence).
     * @param {string|number} additionalEntity The entity or array to provide missing data.
     * @param {Function} templateCallback A template that gets passed a type alias and returns an entity structure to use for instances of that type.
     *
     * @function
     * @name spEntity.augment
     */
    spEntity.augment = function (primaryEntity, additionalEntity, templateCallback) {
        if (!primaryEntity)
            return;
        // additionalEntity may be an array!!

        // Prep primary
        var primary = spEntityUtils.walkEntities(primaryEntity);

        // Prep additional
        var dict = new spEntity.Dict();
        if (additionalEntity) {
            var additional = spEntityUtils.walkEntities(additionalEntity);

            _.forEach(additional, function(e) {
                dict.add(e);
            });
        }

        // Augment all primaries
        var deepAugment = false;
        _.forEach(primary, function (ePrimary) {
            var eAdditional = dict.get(ePrimary.eid());
            if (ePrimary === primaryEntity) { // explicit for root
                eAdditional = additionalEntity;
            }
            if ((!eAdditional) && templateCallback) {
                // Nothing found to augment, look for a template instead.
                var typeId = ePrimary.getType();
                if (typeId) {
                    var typeAlias = typeId.getNsAlias();
                    if (typeAlias) {
                        eAdditional = templateCallback(typeAlias);
                        deepAugment = true;
                    }
                }
            }
            if (eAdditional) {
                spEntity.augmentSingle(ePrimary, eAdditional, true, deepAugment);
            }

        });
    };


    /**
     * Compares the primaryEntity to the additionalEntity (non recursive).
     * If any fields/relationships are missing (unregistered), then they are cloned
     * over from the additional entity.
     * The primaryEntity itself gets updated.
     *
     * @param {spEntity.Entity} primaryEntity The entity to be updated (which takes precedence).
     * @param {string|number} additionalEntity The entity to provide missing data.
     * @param {Boolean} preserveDataState Preserves the datastate of the primary entity when both the primary and secondary are the same.
     *
     * @function
     * @name spEntity.augment
     */
    spEntity.augmentSingle = function (primaryEntity, additionalEntity, preserveDataState, _deepAugment) {
        if (!primaryEntity || !additionalEntity)
            return;

        var dataState;

        if (preserveDataState && primaryEntity._dataState === additionalEntity._dataState) {
            dataState = primaryEntity._dataState;
        }

        // fields
        _.forEach(additionalEntity._fields, function (f) {
            if (!primaryEntity.hasField(f.id)) {
                primaryEntity.setField(f.id, _.cloneDeep(f._getValue()), f._dataType);
            }
        });

        // relationships
        _.forEach(additionalEntity._relationships, function (r) {
            if (!primaryEntity.hasRelationship(r.id)) {
                var newR;
                if (r.isLookup)
                    newR = primaryEntity.registerLookup(r.id);
                else
                    newR = primaryEntity.registerRelationship(r.id);
                newR.isReverse = r.isReverse;
                newR.removeExisting = r.removeExisting;

                // copy rel instances
                // Caution: this currently augments in related entities by reference. We may want to do some sort of cool clone that also matches up to existing entities.
                newR.instances = _.map(r.instances, function(ri) {
                    //var newEntity = dict.get(ri.entity);
                    //var newRelEntity = dict.get(ri.relEntity) || null;
                    var newRi = new spEntity.RelationshipInstance(ri.entity, ri.relEntity);
                    newRi._dataState = ri._dataState;
                    return newRi;
                });
                newR._updateEntitiesFromInstances();
            } else if (_deepAugment && r.instances.length === 1) {
                // if the relationship is already present, and the source was a template
                var child = r.instances[0].entity;
                var related = primaryEntity.getRelationship(r.id);
                _.forEach(related, function (e) {
                    spEntity.augmentSingle(e, child, preserveDataState);
                });
            }
        });

        if (preserveDataState && !_.isUndefined(dataState)) {
            primaryEntity._dataState = dataState;
        }
    };

    spEntity.equivalent = function equivalent(e1, e2) {
        if (!e1 && !e2)
            return true;
        if (!e1 || !e2)
            return false;
        var eid1 = e1.eidP;
        if (!eid1)
            return false;
        var res = eid1.matches(e2.eidP);
        return res;
    };

})(spEntity || (spEntity = EdcEntity = {}));

