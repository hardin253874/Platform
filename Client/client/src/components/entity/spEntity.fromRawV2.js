// Copyright 2011-2016 Global Software Innovation Pty Ltd
var spEntity;
var EdcEntity;  // legacy

(function (spEntity) {
    //spEntity.total = 0;

    /**
     * Converts the raw v2 JSON received from WebAPI calls to a usable Entity object.
     *
     * @param {object} entityData Raw JSON from v2 WebAPI call.
     * @returns {Entity} Entity object for use in client.
     *
     * @function
     * @name spEntity.entityDataVer2ToEntities
     */
    spEntity.entityDataVer2ToEntities = function entityDataToEntities(entityData) {

        // Handle legacy format
        if (!(entityData.results && entityData.results.length) && entityData.ids) {
            entityData.results = [{ code: 200, ids: entityData.ids }];
        }
        var res = spEntity.batchEntityDataVer2ToEntities(entityData);
        return res[0];
    };

    /**
     * Converts the raw v2 JSON received from WebAPI calls to a usable Entity object.
     *
     * @param {object} entityData Raw JSON from v2 WebAPI call.
     * @returns {Entity} Entity object for use in client.
     *
     * @function
     * @name spEntity.batchEntityDataVer2ToEntities
     */
    spEntity.batchEntityDataVer2ToEntities = function batchEntityDataVer2ToEntities(entityData) {
        //var start = performance.now();

        if (!entityData) {
            return null;
        }

        var context = {
            accessorCache: {},
            entityCache: {},
            entityData: entityData,
            graph: spEntity._newGraphHandle(),
            aliasFieldIds: {}, // array of ids that represent alias (multiple, because some may be temp IDs if generated client-side)
            isOfTypeRelIds: {} // array of ids that represent isOfType (as above)
        };

        // Find the ID of the alias field, and isOfType rel, if present
        for (var membFieldId in entityData.members) {
            var memb = entityData.members[membFieldId];
            if (memb.alias === 'core:alias') {
                context.aliasFieldIds[membFieldId] = true;
            }
            if (memb.frel && memb.frel.alias === 'core:isOfType') {
                context.isOfTypeRelIds[membFieldId] = true;
            }
        }

        // Return an array of arrays. An array of results, with each result containing the array of entities.
        var results = _.map(entityData.results, function (result) {
            if (result.code !== 200) {
                return null;
            }
            var entities = _.map(result.ids, function (id) {
                return entityFromEntityData(id, context);
            });
            return entities;
        });

        //spEntity.total += (performance.now() - start);
        return results;
    };


    /*
     * Internal: Converts WebAPI entityData JSON to an entity.
     */
    function entityFromEntityData(id, context) {
        var entityCache = context.entityCache;
        var entityData = context.entityData;

        // Check if we've already converted this
        var entity = entityCache[id];
        if (entity) {
            return entity;
        }

        // Look json for this entity
        var entityJson = entityData.entities[id];

        // Create (and cache) a new instance
        entity = new spEntity._Entity({ id: parseInt(id, 10), graph: context.graph });
        entity._fields = []; // get rid of name field that got registered by call to _setSummary
        entityCache[id] = entity;

        // If there was no raw json, then we're done
        if (!entityJson) {
            return entity;
        }

        // Convert the members
        for (var membId in entityJson) {
            var memberInfo = entityData.members[membId];
            var data = entityJson[membId];

            // Has a field data type.
            if (memberInfo.dt) {
                // Convert the field
                addFieldFromEntityData(membId, data, memberInfo, entity, context);

                // Special extra handling for alias field
                if (context.aliasFieldIds[membId] && data) {
                    entity.eid().setNsAlias(data);
                }
            }             
            else {
                // Convert the relationship in either/both directions
                if (data.f) {
                    addRelFromEntityData(membId, data.f, memberInfo.frel, entity, context, entity, false);
                }
                if (data.r) {
                    addRelFromEntityData(membId, data.r, memberInfo.rrel, entity, context, entity, true);
                }

                // Special extra handling for isOfType relationship
                if (context.isOfTypeRelIds[membId] && data) {
                    var types = _.invokeMap(entity.getRelationship(spEntity.aliases.isOfType), 'eid'); // pluck eid()
                    entity.setTypes(types);
                }
            }
        }            

        entity._dataState = spEntity.DataStateEnum.Unchanged;

        return entity;
    }

    
    function addFieldFromEntityData(fieldId, fieldData, fieldMember, entity, context) {
        // create the field
        var fieldEid = new spEntity.EntityRef({ id: parseInt(fieldId, 10), nsAlias: fieldMember.alias }, true);
        var field = new spEntity.Field(fieldEid, fieldMember.dt);

        // set the value
        field._value = field._convertRawToNative(fieldMember.dt, fieldData);
        field.markAsPristine();
        
        // add to the entity        
        entity._fields.push(field);
        
        // set the accessor
        var key = fieldEid.getAlias();
        if (key) {
            var accessor = context.accessorCache[key];
            if (!accessor) {
                accessor = spEntity._fieldAccessorFactory(field.id);
                context.accessorCache[key] = accessor;
            }
            spEntity._addAccessor(entity, accessor, field);
        }
    }


    function addRelFromEntityData(relId, relData, relMember, entity, context, owner, isReverseActual) {
        // create the field
        var relEid = new spEntity.EntityRef({ id: parseInt(relId, 10), nsAlias: relMember.alias }, true);
        var rel = new spEntity.Relationship(relEid, owner);
        rel.isLookup = relMember.isLookup === true;
        rel.isReverse = isReverseActual && !relMember.alias;   // if we received an alias in the reverse direction, then it would be a reverse alias, so we don't need to flag as reversed again. Hopefully.
        rel._isReverseActual = isReverseActual;                // caution: only used in re-serialization, does not get created in fromJSON
        rel._setInstancesFast(relData.map(function (r) {
            return relInstFromEntityData(r, context);
        }));
        rel.removeExisting = false;
        rel._deleteExisting = false;

        // add to the entity        
        entity._relationships.push(rel);

        // set the accessor
        var key = relEid.getAlias();
        if (key) {
            var accessor = context.accessorCache[key];
            if (!accessor) {
                var relIdObj = { id: rel.id, isReverse: rel.isReverse };
                accessor = rel.isLookup ? spEntity._lookupAccessorFactory(relIdObj) : spEntity._relAccessorFactory(relIdObj);
                context.accessorCache[key] = accessor;
            }
            spEntity._addAccessor(entity, accessor, rel);
        }
    }


    function relInstFromEntityData(rawInstance, context) {
        var instance = new spEntity.RelationshipInstance();

        // Relationships arrive as either a number 123, or as an pair {e:123,ri:456} with the entity ID and the relationship instance.
        var entityId;
        var instanceId;
        if (_.isFinite(rawInstance)) {
            entityId = rawInstance;
        } else {
            entityId = rawInstance.e;
            instanceId = rawInstance.ri;
        }

        instance.entity = entityId ? entityFromEntityData(entityId, context) : null;
        instance.relEntity = instanceId ? entityFromEntityData(instanceId, context) : null;
        instance.setDataState(spEntity.DataStateEnum.Unchanged);
        return instance;
    }

})(spEntity || (spEntity = EdcEntity = {}));

