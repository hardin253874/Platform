// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntityUtils */

var spEntity;
var EdcEntity;  // legacy

(function (spEntity) {
    //spEntity.total = 0;

    /**
     * Converts a client side entity or entities to the raw v2 JSON format.
     * IMPORTANT: This implementation is currently only suitable for caching unmodified data.
     *            -Local data that is not stored includes: removeExisting, deleteExisting
     *            -Entities with temp IDs also get serialized with those temp IDs, which may collide with other IDs when deserialiezd.
     *            -Does not keep track of deleted relationships
     *            These issues need to be addressed if we want to use it for caching locally modified data.
     *            Further, data must come from the entity service (not fromJSON) as it relies on ID numbers and actual relationship alias direction
     *
     * @param {object} entities Entity or entities to convert to raw JSON v2.
     * @returns {object} JSON entity data v2.
     *
     * @function
     * @name spEntity.entitiesToEntityDataVer2
     */
    spEntity.entitiesToEntityDataVer2 = function entitiesToEntityDataVer2(entities) {

        if (!entities)
            return null;
        if (!_.isArray(entities))
            entities = [entities];

        // Functions to build identifiers
        var entityRefToId = function (entRef) { return entRef._getIdOrDummyId(); };
        var entityToId = function (e) { return entityRefToId(e.eidP); };

        // Result object
        var result = { entities:{}, members:{} };
        result.ids = _.map(entities, entityToId);

        // Make some IDs
        var aliasId = spEntity._getNextId();
        result.members[aliasId] = { alias: 'core:alias', dt: 'String' };
        var isOfTypeId = spEntity._getNextId();
        result.members[isOfTypeId] = { frel: { alias: 'core:isOfType' } };

        var visited = {};
        var typesToAdd = [];

        // Visit all entities
        var allEntities = spEntityUtils.walkEntities(entities);
        _.forEach(allEntities, function (e) {
            // Get raw entity container - note: it's possible (but undesirable) that there may be duplicates. Last in wins.
            var id = entityToId(e);
            visited[id] = true;
            var eraw = result.entities[id];
            if (!eraw) result.entities[id] = eraw = {};

            // Capture alias as a field
            var addAlias = e.eidP.nsAlias && !_.some(e._fields, function (f) { return f.id.nsAlias === 'core:alias'; });
            if (addAlias) {
                eraw[aliasId] = e.eidP.nsAlias;
            }
            // Capture types as a relationship, and manufacture entities if they have aliases
            var addIsOfType = e.typesP && e.typesP.length && !_.some(e._relationships, function (r) { return r.id.nsAlias === 'core:isOfType' && !r.isReverse; });
            if (addIsOfType) {
                eraw[isOfTypeId] = { f: _.map(e.typesP, entityRefToId) };
                _.forEach(e.typesP, function (type) {
                    if (type.nsAlias) {
                        var tid = entityRefToId(type);
                        var traw = result.entities[tid];
                        if (!traw) result.entities[tid] = traw = {};
                        traw[aliasId] = type.nsAlias;
                    }
                });
            }

            // Fields
            _.forEach(e._fields, function (field) {
                var fid = entityRefToId(field.id)+'';
                var data = field._convertNativeToRaw(field._dataType, field._value);
                eraw[fid] = data;

                // Field defn
                if (!(fid in result.members)) {
                    var fieldDefn = { dt: field._dataType };
                    if (field.id.nsAlias !== null)
                        fieldDefn.alias = field.id.nsAlias;
                    result.members[fid] = fieldDefn;
                }
            });

            // Relationships
            _.forEach(e._relationships, function (rel) {
                var rid = entityRefToId(rel.id) + '';
                // Combined fwd/rev container
                var rComb;
                if (!(rid in eraw))
                    eraw[rid] = rComb = {};
                else
                    rComb = eraw[rid];

                // Store data
                var ids = _.map(rel.entities, entityToId);
                rComb[rel._isReverseActual ? 'r' : 'f'] = ids;

                // Rel defn
                var relCombDefn = result.members[rid];
                if (!relCombDefn) {                    
                    result.members[rid] = relCombDefn = {};
                }
                var defnKey = rel._isReverseActual ? 'rrel' : 'frel';
                if (!relCombDefn.hasOwnProperty(defnKey)) {
                    var relDefn = relCombDefn[defnKey] = {
                        isLookup: rel.isLookup
                    };
                    if (rel.id.nsAlias !== null)
                        relDefn.alias = rel.id.nsAlias;
                }
            });
        });
        return result;
    };

})(spEntity || (spEntity = EdcEntity = {}));

