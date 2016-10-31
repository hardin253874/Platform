// Copyright 2011-2016 Global Software Innovation Pty Ltd
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

    /**
     * Converts a client-side JSON format to an entity object.
     *
     * @param {object} json Raw JSON from WebAPI call.
     * @param {object} options Allows the graph object to be passed.
     * @returns {Entity} Entity object for use in client.
     *
     * Note: null values are not accepted. Use a fallback such as jsonString(myValue) to indicate type if there is a risk of null values.
     * @function
     * @name spEntity.fromJSON
     */
    spEntity.asEntity = spEntity.fromJSON = function fromJSON(json, options) {
        options = options || {};
        if (!json)
            return json;
        var tracker = new JsonCycleTaskTracker();
        tracker.newGraph = !options.graph;
        tracker.graph = options.graph || spEntity._newGraphHandle();

        var resContainer = fromJSONImpl(json, tracker);
        tracker.runTasks();
        var res = tracker.resolve(resContainer);
        return res;
    };

    function fromJSONImpl(json, _tracker) {
        if (!json)
            return {doValue: json};
        if (spEntity.isEntity(json)) {
            _tracker.addExternalEntity(json);
            return {doValue: json};
        }

        // Handle arrays
        if (_.isArray(json)) {
            var res = _.map(json, function (j) {
                return fromJSONImpl(j, _tracker);
            });
            return res;
        }

        // Handle entities by ID
        // (Supported so we can use these inside relationship arrays, or inside jsonLookup, etc.
        if (_.isFinite(json) || _.isString(json)) {
            return {doFind: json};
        }

        // create entity
        var useId = json.id;
        if (json.alias) {
            useId = new spEntity.EntityRef({id: json.id, nsAlias: json.alias});
        }
        var entity = new spEntity._Entity({
            id: useId,
            typeId: json.typeId,
            dataState: json.dataState,
            graph: _tracker.graph
        });
        _tracker.addEntity(entity);

        // Fill members
        for (var key in json) {
            var value = json[key];

            if (key === 'id' || key === 'typeId' || key === 'dataState' || key === 'alias') {
                continue;
            } else if (key === 'idP' || key === 'eid' || key === 'eidP' || key === 'nsAlias') {
                throw new Error('fromJSON does not support: ' + key);
            } else if (value === null || _.isUndefined(value)) {
                throw new Error('fromJSON does not allow null values. Use a fallback. E.g. jsonString(myValue), or jsonLookup(myLookup)');
            } else if (value.json) {
                var valueValue = _.isUndefined(value.value) ? null : value.value;
                if (value.json === 'Relationship') {
                    // fallback of jsonRelationship(value) (but don't refer it it by name, as it may be passed in from a string, etc.
                    entity.registerRelationship(key);
                    var convertedArray1 = fromJSONImpl(valueValue || [], _tracker);
                    _tracker.addTask(convertedArray1, entity, key);
                } else if (value.json === 'Lookup') {
                    // same for lookup
                    entity.registerLookup(key);
                    var converted1 = fromJSONImpl(valueValue || null, _tracker);
                    _tracker.addTask(converted1, entity, key);
                } else {
                    // same for fields, e.g. jsonString(value)
                    entity.setField(key, valueValue, value.json);
                }
            } else if (_.isString(value)) {
                entity.setField(key, value, spEntity.DataType.String);
            } else if (_.isFinite(value)) {
                entity.setField(key, value, spEntity.DataType.Int32);
            } else if (value === true || value === false) {
                entity.setField(key, value, spEntity.DataType.Bool);
            } else if (_.isArray(value)) {
                entity.registerRelationship(key);
                var convertedArray = fromJSONImpl(value, _tracker);
                _tracker.addTask(convertedArray, entity, key);
            } else {
                entity.registerLookup(key);
                var converted = fromJSONImpl(value, _tracker);
                _tracker.addTask(converted, entity, key);
            }
        }
        return {doValue: entity};
    }

    function JsonCycleTaskTracker() {
        this.entities = {};
        this.dict = new spEntity.Dict();
        this.tasks = [];
        // Register an entity for later retrieval
        this.addEntity = function (e) {
            this.dict.add(e);
        };
        // Register an entity for later retrieval (but use its graph)
        this.addExternalEntity = function (newEntity) {
            if (newEntity.graph !== this.graph) {
                if (!this.newGraph) {
                    //console.warn('Entities of different graphs have been combined in fromJSON.');
                } else {
                    this.newGraph = false;
                    _.forEach(this.dict.values(), function (e) {
                        e._setGraph(newEntity.graph);
                    });
                }
            }
            this.addEntity(newEntity);
        };
        // Register that we will later want to rewire a particular relationship value
        this.addTask = function (value, entity, relKey) {
            this.tasks.push({value: value, entity: entity, key: relKey});
        };
        // Run all binding tasks
        this.runTasks = function () {
            var that = this;
            for (var i = 0; i < this.tasks.length; i++) {
                var task = this.tasks[i];
                if (_.isArray(task.value)) {
                    var entities = this.resolve(task.value);
                    task.entity.setRelationship(task.key, entities);
                } else {
                    var entity = this.resolve(task.value);
                    task.entity.setLookup(task.key, entity);
                }
            }
        };
        // Take a tracker object and find its entity
        this.resolve = function (value) {
            if (!value)
                return value;
            if (_.isArray(value)) {
                var that = this;
                var arr = _.map(value, function (v) {
                    return that.resolve(v);
                });
                return arr;
            }
            if (value.doValue !== undefined)
                return value.doValue;
            if (_.isUndefined(value.doFind))
                throw new Error('Internal error in fromJSONTracker');
            var res = this.dict.get(value.doFind);
            if (!res) {
                res = spEntity.fromId(value.doFind);
                res._graph = this.graph;
                this.addEntity(res);
            }
            return res;
        };
        return this;
    }

    /**
     * Converts an entity object to client-side JSON format. Intended for creating mock tests only.
     *
     * @param {Entity} entity Entity object for use in client.
     * @param {object} context Internal object passed during recursion. Do not use when calling.
     * @returns {object} Plain ol' js obj
     *
     * @function
     * @name spEntity.toJSON
     */
    spEntity.toJSON = function toJSON(entity, options) {

        var skipNullOrUndefined = options && options.skipNullOrUndefined;
        var skipEmpty = options && options.skipEmpty;
        var skipDataState = options && options.skipDataState;

        function addKeyValue(obj, key, value) {
            if (!(skipNullOrUndefined && (_.isUndefined(value) || _.isNull(value))) &&
                !(skipEmpty && _.isEmpty(value))) {

                obj[key] = value;
            }
        }

        function toJSONInner(entity, context) {

            var json;

            var getKey = function (id, membType) {
                var ns = id.getNamespace();
                var alias = id.getAlias();
                if (alias === null) {
                    json._warnings = (json._warnings || '') + membType + ' ' + id.getId() + ' has no alias; ';
                    return null;
                } else if (ns === 'core') {
                    return alias;
                } else {
                    return ns + ':' + alias;
                }
            };

            var mapRelInst = function (ri) {
                return toJSONInner(ri.entity, context);
            };

            if (!entity)
                return entity;

            // create object
            json = {
                id: entity.eid().getNsAliasOrId()
            };

            if (!skipDataState) {
                json._dataState = entity._dataState;
            }

            if (context[entity.eid().getId()]) {
                json._warnings = 'already visited ' + entity.debugString;
                return json;
            }

            context[entity.eid().getId()] = true;

            // Fill fields
            _.forEach(entity._fields, function (field) {
                var key = getKey(field.id, 'Field');
                if (key)                    {
                    addKeyValue(json, key, field._value);
                }
            });

            // Fill rels
            _.forEach(entity._relationships, function (rel) {
                var key = getKey(rel.id, 'Relationship');
                if (key) {
                    var value = _.map(rel.instances, mapRelInst);
                    addKeyValue(json, key, rel.isLookup ? _.first(value) : value);
                }
            });

            return json;
        }

        return toJSONInner(entity, {});
    };

})(spEntity || (spEntity = EdcEntity = {}));

