// Copyright 2011-2016 Global Software Innovation Pty Ltd

var spEntity;
var EdcEntity;  // legacy

(function (spEntity) {

    /**
     * Converts the raw JSON received from WebAPI calls to a usable Entity object.
     *
     * @param {object} entityData Raw JSON from WebAPI call.
     * @returns {Entity} Entity object for use in client.
     *
     * @function
     * @name spEntity.entityDataToEntities
     */
    spEntity.entityDataToEntities = function entityDataToEntities(entityData) {
        var context = {
            accessorCache: {}
        };

        var entities = entityData.ids.map(function (id) {
            return entityFromEntityDataV1(id, entityData, context);
        });
        return entities;
    };


    /*
     * Internal: Converts WebAPI entityData JSON to an entity.
     */
    function entityFromEntityDataV1(id, entityData, context) {
        var accessorCache = context.accessorCache;

        var rawEntity = findByRawId(entityData.entities, id);
        if (rawEntity._entity) {
            //console.log('loaded entity from entityData (cached)', rawEntity._entity.debugString);
            return rawEntity._entity;
        }
        var entity = new spEntity._Entity({
            id: entityRefFromEntityDataV1(id, entityData, context)
        });
        rawEntity._entity = entity;
        entity._typeIds = rawEntity.typeIds.map(function (t) {
            return entityRefFromEntityDataV1(t, entityData, context);
        });

        // Convert fields, and bind accessors
        entity._fields = rawEntity.fields.map(function (f) {
            // Convert the field
            var field = fieldFromEntityDataV1(f, entityData, context);
            // Set the accessor
            var key = field.id.getAlias();
            if (key) {
                var accessor = accessorCache[key];
                if (!accessor) {
                    accessor = spEntity._fieldAccessorFactory(field.id);
                    accessorCache[key] = accessor;
                }
                spEntity._addAccessor(entity, accessor, field);
            }
            return field;
        });

        // Convert relationships, and bind accessors
        entity._relationships = rawEntity.relationships.map(function (r) {
            // Convert the relationship
            var rel = relationshipFromEntityDataV1(r, entityData, context, entity);
            // Set the accessor
            var key = rel.id.getAlias();
            if (key) {
                var accessor = accessorCache[key];
                if (!accessor) {
                    var relIdObj = { id: rel.id, isReverse: rel.isReverse };
                    accessor = rel.isLookup ? spEntity._lookupAccessorFactory(relIdObj) : spEntity._relAccessorFactory(relIdObj);
                    accessorCache[key] = accessor;
                }
                spEntity._addAccessor(entity, accessor, rel);
            }
            return rel;
        });

        entity._dataState = rawEntity.dataState;
        return entity;
    }


    function fieldFromEntityDataV1(f, entityData, context) {
        var fieldId = entityRefFromEntityDataV1(f.fieldId, entityData, context);
        var field = new spEntity.Field(fieldId, f.typeName);
        field._value = field._convertRawToNative(f.typeName, f.value);
        field.markAsPristine();
        return field;
    }


    function relationshipInstanceFromEntityDataV1(rawInstance, entityData, context) {
        var instance = new spEntity.RelationshipInstance();
        instance.entity = rawInstance.entity ? entityFromEntityDataV1(rawInstance.entity, entityData, context) : null;
        instance.relEntity = rawInstance.relEntity ? entityFromEntityDataV1(rawInstance.relEntity, entityData, context) : null;
        instance._dataState = rawInstance.dataState;
        return instance;
    }


    function relationshipFromEntityDataV1(rawRel, entityData, context, owner) {
        var relId = new spEntity.EntityRef(rawRel.relTypeId);
        var rel = new spEntity.Relationship(relId, owner);
        rel.isReverse = rawRel.isReverse === true;
        rel.isLookup = rawRel.isLookup === true;
        rel.setInstances(rawRel.instances.map(function (r) {
            return relationshipInstanceFromEntityDataV1(r, entityData, context);
        }));
        rel.removeExisting = false;
        rel._deleteExisting = false;
        return rel;
    }


    /** Return an EntityRef based on the raw entity ref in the entity data object */
    function entityRefFromEntityDataV1(id, entityData, context) {
        var rawEntityRef = findByRawId(entityData.entityRefs, id);
        var entityRef = spEntity.asEntityRef(id);
        entityRef._alias = rawEntityRef.alias;
        entityRef._ns = rawEntityRef.ns;
        return entityRef;
    }

    function findByRawId(arr, id) {
        if (arr && id) {
            var i = 0, len = arr.length;
            for (; i < len; ++i) {
                var o = arr[i];
                if (o.id && +o.id === +id) {
                    return o;
                }
            }
        }
        return null;
    }

})(spEntity || (spEntity = EdcEntity = {}));

