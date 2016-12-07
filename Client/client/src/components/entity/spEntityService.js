// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, $, angular, console, spUtils, sp, spEntity, spEntityUtils */

//todo - added better error handling for getEntity and putEntity... should just expose the xhr status

// current spEntityService follows:

angular.module('mod.common.spEntityService', ['ng', 'mod.common.spWebService', 'sp.entityAliases']);

angular.module('mod.common.spEntityService').factory('spEntityService', function ($http, $q, $timeout, spWebService, $rootScope) {
    'use strict';

    /**
     *  A set of client side services working against the entity webapi service.
     *  @module spEntityService
     */
    var exports = { disableBatching: false };

    var cache = {};

    
    function getEidUrl(eid) {
        var entityRef = new spEntity.EntityRef(eid);
        var alias = entityRef.getAlias();
        var res;
        if (alias) {
            res = (entityRef.getNamespace() || 'core') + '/' + alias;
        } else {
            res = entityRef.getId();
        }
        return res;
    }

    function getEntityRequestUrl(eid, options) {
        var version = 'v1';
        if (options && options.version) {
            version = 'v' + options.version;
        }

        var url = spWebService.getWebApiRoot() + '/spapi/data/' + version + '/entity/',
            entityRef = new spEntity.EntityRef(eid);

        var alias = entityRef.getAlias();
        if (alias) {
            url += (entityRef.getNamespace() || 'core') + '/' + alias;
        } else {
            if (version === 'v1')
                url += entityRef.getId();
            else
                url += 'a/' + entityRef.getId();
        }

        return url;
    }
    exports._getEntityRequestUrl = getEntityRequestUrl;

    function getEntityPostUrl() {

        return spWebService.getWebApiRoot() + '/spapi/data/v1/entity/';
    }

    function getEntityDeleteUrl(id) {

        return spWebService.getWebApiRoot() + '/spapi/data/v1/entity/' + (id || '');
    }

    function aliasOnly(idOrAlias) {
        if (!idOrAlias)
            return '';
        var i = idOrAlias.indexOf && idOrAlias.indexOf(':');
        if (i === -1 || !i)
            return idOrAlias;
        return idOrAlias.slice(i + 1);
    }

    function buildPostData(entityData, options) {

        //note - some of the ways entityData may be passed, as tested for below, is out of date
        //and only remains while transitioning to always passing in certain forms. Need to
        //double check what we are still using and remove the out of data code.

        // options={changesOnly:bool}

        // console.group('buildPostData');
        console.time('buildPostData');

        var data = null;
        if (_.isArray(entityData) || entityData._id && entityData.field) {
            data = spEntity.entitiesToEntityData(entityData, options);
        } else if (entityData._rawEntityData) { // is EntityData
            data = entityData._rawEntityData;
        } else if (entityData._entityData) { // is Entity
            data = entityData._entityData._rawEntityData;
        } else if (entityData.ids && entityData.entities) { // is raw entity data
            data = entityData;
        }
        if (!data) {
            throw new Error('code out of date - need to be passing an Entity with contained _entityData');
        }

        // console.groupCollapsed('entityRefs', data.entityRefs.length);
        // console.groupEnd();

        // console.groupCollapsed('entities', data.entities.length);
        // console.groupEnd();

        console.log('buildPostData: entities=' + data.entities.length + ', entityRefs=' + data.entityRefs.length);
        console.timeEnd('buildPostData');
        // console.groupEnd();

        return data;
    }

    /** Todo: remove me
     * @private
     */
    exports._getWebApiRoot = function () {
        return spWebService.getWebApiRoot();
    };

    /**
     * Set the root of the webapi service. This is required if the client is being not being hosted from the same
     * location as the spapi web service.
     *
     * @param {string} path the path to the root of the webapi web. May or may not include a trailing /
     *
     * @example
     *
     * If the spapi web is https://somemachine.local/spapi then call this using
     *
<pre>
    spEntityService.setWebApiRoot('https://somemachine.local/')
</pre>
     */
    exports.setWebApiRoot = function (path) {
        spWebService.setWebApiRoot(path);
    };

    /**
     * Make a webapi request to the entityInfo service and return the resulting entityData.
     * @param {int|string} eid eid
     * @param {string} request entity info service request
     * @returns {promise} A promise for an spEntity.EntityData
     */
    exports._getEntityData = function (eid, request, options) {

        return $http({
            method: 'GET',
            url: getEntityRequestUrl(eid, options),
            params: { request: request },
            headers: spWebService.getHeaders()
        }).then(function (response) {
                return response.data;
            });
    };

    /**
     * Make an async webapi request to the entityInfo service and return the resulting entity
     * @param {number | string} eid - the id or alias of the entity
     * @param {string} request - the request
     * @param {Object} options - Optional.
     *          Pass {batch:true} to enable batching of requests.
     *          Or {mayUseCache:true} to allow it to use a cached entity object.
     * @returns {promise<spEntity.Entity>} A promise for an spEntity.Entity
     */
    exports.getEntity = function (eid, request, options) {
        var rq = exports.makeGetEntityRequest(eid, request);
        return exports.runRequest(rq, options);
    };

    /**
     * Performs an async request for multiple entities via the entity info webapi and returns a
     * promise for the resulting list of entities.
     *
     * @param {number|string} eids - one or multiple entity ids, each may be a number (id) or string (alias).
     * @param {string} request - one or multiple entity info requests.
     * @returns {promise<Array<spEntity.Entity>>} A promise for a array of spEntity.Entity. Note - a failed promise has the
     * same data as a failed $http request.
     */
    exports.getEntities = function (eids, request, options) {
        if (!eids || eids.length === 0) {
            return $q.when([]);
        }
        var rq = exports.makeGetEntitiesRequest(eids, request);
        return exports.runRequest(rq, options);
    };

    /**
     * Make an async webapi request to the entityInfo service and return the resulting entity
     * @param {number | string} typeId - the id or alias of the entity type
     * @param {string} request - the request. Defaults to alias/name/description.
     * @param {Object} options - Optional. Pass {batch:true} to enable batching of requests.
     * @returns {promise<spEntity.Entity>} A promise for an spEntity.Entity
     */
    exports.getEntitiesOfType = function (typeId, request, options) {
        var rq = exports.makeGetEntitiesOfTypeRequest(typeId, request, options);
        return exports.runRequest(rq, options);
    };

    /**
     * Get instances of type, with legacy wrapper.
     *
     * @param {number | string} typeId - the id or alias of the entity type
     * @param {string} request - the request. Defaults to alias/name/description.
     * @param {Object} options - Optional. Pass {batch:true} to enable batching of requests.
     * @returns {promise<spEntity.Entity>} A promise for an spEntity.Entity
     */
    exports.getInstancesOfType = function (typeId, request, options) {
        return exports.getEntitiesOfType(typeId, request, options).then(function (entities) {
            var instances = entities.map(function (e) {
                return {
                    id: e.id(),
                    alias: e.getField('alias') || '',
                    name: e.getName() || '#NoName',
                    description: e.getField('description') || '',
                    entity: e
                };
            });
            return instances;
        });
    };

    /**
     * Request an entity by its name, and the name of its type. This method is intended to assist with automated testing only.
     * @param {string} name - the name of the entity
     * @param {string} typeName - the name of the entity type
     * @param {string} request - the request
     * @returns {promise<spEntity.Entity>} A promise for an spEntity.Entity
     */
    exports.getEntityByNameAndTypeName = function (name, typeName, request) {

        var url = spWebService.getWebApiRoot() + '/spapi/data/v2/entity';
        var params = {
            name: name,
            typename: typeName,
            request: request
        };
        url += '?' + $.param(params);

        return $http({ method: 'GET', url: url, headers: spWebService.getHeaders() })
            .then(function (response) {
                var entities = spEntity.entityDataVer2ToEntities(response.data);
                return entities[0];
            });
    };


    /**
     * Clones the entity at the root of the request then updates it or any cloned or original related entities as needed.
     *
     * @param {spEntity.Entity} entity - the entity object (and all related entities)     
     * @returns {promise.<number>} a promise for the id of the updated or created entity
     */
    exports.cloneAndUpdateEntity = function (entity) {

        if (!entity) {
            console.error('cloneAndUpdateEntity was called with null entity');
            return;
        }

        return $q.when()
            .then(function () {
                var postData = buildPostData(entity, { changesOnly: true });
                console.log('cloneAndUpdateEntity: posting');

                var url = getEntityPostUrl() + 'cloneAndUpdate';                

                return $http({
                    method: 'POST',
                    url:  url,
                    data: postData,
                    headers: spWebService.getHeaders()
                });
            })
            .then(function (response) {
                var data = response.data;

                return data;
            });
    };


    /**
     * Save the entity, either creating new or update existing entity as needed.
     *
     * @param {spEntity.Entity} entity - the entity object (and all related entities)
     * @param {bool} updateIds - Whether the original entity should have temporary ids replaced with real ids.
     * @returns {promise.<number>} a promise for the id of the updated or created entity
     */
    exports.putEntity = function (entity, updateIds) {

        if (!entity) {
            console.error('putEntity was called with null entity');
            return;
        }

        return $q.when()
            .then(function () {
                var postData = buildPostData(entity, { changesOnly: true });
                console.log('putEntity: posting');

                var url = getEntityPostUrl();

                if (updateIds) {
                    url += 'getNewIds/';
                }

                return $http({
                    method: 'POST',
                    url:  url,
                    data: postData,
                    headers: spWebService.getHeaders()
                });
            })
            .then(function (response) {
                var data = response.data;
                console.log('putEntity: post type %s', typeof data);

                if (!updateIds && !_.isFinite(data)) {
                    throw new Error('Expected result ID');
                } else if (updateIds && !_.isPlainObject(data)) {
                    throw new Error('Expected result map');
                }

                var newId;

                if (updateIds) {
                    var entities = spEntityUtils.walkEntities(entity);

                    if (entities && Object.keys(data).length) {
                        _.forEach(entities, function (e) {

                            e.setDataState(spEntity.DataStateEnum.Unchanged);

                            if (e._id._id) {
                                newId = data[e._id._id];

                                if (newId && e._id._id !== newId) {
                                    e._id._id = newId;
                                }
                            }

                            e.graph.history.clear();
                        });
                    }



                    newId = entity._id._id;
                } else {
                    newId = data;
                }

                return newId;
            });
    };

    /**
     * Clones the entity, optionally applying the new name.
     *
     * @param {number|string} eid - the entity identifier
     * @param {string} name - the entities new name
     * @returns {promise.<number>} a promise for the id of the new entity
     */
    exports.cloneEntity = function(eid, name) {

        return $q.when()
            .then(function() {
                return $http({
                    method: 'POST',
                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/entity/clone',
                    data: {
                        id: eid,
                        name: name
                    },
                    headers: spWebService.getHeaders()
                });
            })
            .then(function(response) {
                var data = response.data;

                //Expected result to be number.
                if (!_.isFinite(data)) {
                    throw new Error('Expected result ID');
                }
                var newId = parseInt(data, 10);
                return newId;
            });
    };


    /**
     * Delete the entity identified by the given id.
     *
     * @param {number} id the id of an entity to delete
     * @returns {promise} the $http promise... only useful to track completion, no data is returned
     */
    exports.deleteEntity = function (id) {

        var url = getEntityDeleteUrl(id);

        return $http({
            method: 'DELETE',
            url: url,
            parameters: { id: id },
            headers: spWebService.getHeaders()
        });
    };

    /**
     * Delete the entities identified by the given ids.
     *
     * todo - Implement a batch limit and make multiple delete webapi calls.
     *
     * @param {number} ids an array of ids of entities to delete
     * @returns {promise} the $http promise... only useful to track completion, no data is returned
     */
    exports.deleteEntities = function (ids) {

        //TODO: add batching of some max batch size

        var url = ids.reduce(function (p, c) {
            return p + 'id=' + c + '&';
        }, getEntityDeleteUrl() + '?');

        return $http({
            method: 'DELETE',
            url: url,
            headers: spWebService.getHeaders()
        });
    };




    /**
     * @doc object
     * @name EntityType
     * @methodOf sp_common.service:spEntityService
     * @description
     *
     * EntityType is a class that provides easy access to the common properties of an entity type,
     * such as alias, name and description, as well as all field and relationship that instances of
     * the type may have.
     *
     * Objects of EntityType are created within the spEntityService and may be returned from its exported functions
     *
     */
    var EntityType = function (te, parent) {
        this.id = te.id();
        this.alias = te.alias();
        this.type = te.types()[0].alias();
        this.name = te.name;
        this.description = te.field('core:description');

        this.isAbstract = te.field('core:isAbstract');
        if (_.isString(this.isAbstract)) {
            this.isAbstract = this.isAbstract.toLowerCase() === 'true';
        }

        this.inheritedTypes = parent ? [parent] : [];
        this.fields = te.relationship('core:fields').map(function (f) {
            return {
                id: f.id(),
                alias: f.alias(),
                name: f.name,
                description: f.field('description'),
                type: f.firstTypeId().alias()
            };
        });
        var relationships = te.relationship('core:relationships').map(function (r) {
            var toType = r.getLookup('toType');
            /** A field on an entity type
             *  @typedef EntityType.Field
             */
            return {
                /** id
                 * @memberof EntityType.Field */
                id: r.id(),
                alias: r.alias(),
                name: r.field('core:toName') || r.name,
                description: r.field('description'),
                toType: toType ? toType.alias() : null,
                toTypeType: toType ? toType.firstTypeId().alias() : null,
                cardinality: r.cardinality && r.cardinality.alias() || 'manyToMany'
            };
        });
        var reverseRelationships = te.relationship('core:reverseRelationships').map(function (r) {
            var toType = r.getLookup('toType');
            return {
                id: r.id(),
                alias: r.alias(),
                name: r.field('core:fromName') || r.name,
                description: r.field('description'),
                toType: toType ? toType.alias() : null,
                toTypeType: toType ? toType.firstTypeId().alias() : null,
                cardinality: r.cardinality && r.cardinality.alias() || 'manyToMany',
                isReverse: true
            };
        });
        this.relationships = relationships.concat(reverseRelationships);
    };
    EntityType.prototype.fieldCount = function () {
        return this.inheritedTypes.reduce(function (cv, t) {
            return cv + t.fieldCount();
        }, this.fields.length);
    };
    /** get all fields including inherited
     * @memberof EntityType
     */
    EntityType.prototype.allFields = function () {
        var list = this.fields.slice(0);
        this.inheritedTypes.forEach(function (t) {
            list = list.concat(t.allFields().map(function (f) {
                return _.extend({ isInherited: true }, f);
            }));
        });
        return _.uniqBy(list, function (item) {
            return item.id;
        });
    };
    /** get all relationships including inherited
     * @memberof EntityType
     */
    EntityType.prototype.allRelationships = function () {
        var list = this.relationships.slice(0);
        this.inheritedTypes.forEach(function (t) {
            list = list.concat(t.allRelationships().map(function (r) {
                return _.extend({ isInherited: true }, r);
            }));
        });
        return _.uniqBy(list, function (item) {
            return item.id;
        });
    };

    var EntityTypes = function () {
        this.types = [];
        this.error = null;
    };

    EntityTypes.prototype.add = function (te, parent, child) {
        var t = spUtils.findByKey(this.types, 'id', te.id());

        if (!t) {
            t = new EntityType(te);
            this.types.push(t);

            te.relationship('core:derivedTypes').forEach(_.bind(function (e) {

                var typeAlias = e.types()[0].alias();

                if (!typeAlias) {
                    console.log('WARNING: unexpected null alias for: ' + e.debugString);
                }

                if (!typeAlias || (typeAlias !== 'core:fieldType' &&
                                    typeAlias !== 'core:relationship' &&
                                    typeAlias.slice(0, 7) !== 'console')) {

                    this.add(e, t);

                    //console.log('>added', typeAlias, e.name, e.types().length);

                } //else console.log('>skipped', typeAlias, e.name);
            }, this));

            te.relationship('core:inherits').forEach(_.bind(function (e) {
                var typeAlias = e.types()[0].alias();

                if (!typeAlias) {
                    console.log('WARNING: unexpected null alias for: ' + e.debugString);
                }

                if (!typeAlias || (typeAlias !== 'core:fieldType' &&
                                    typeAlias !== 'core:relationship' &&
                                    typeAlias.slice(0, 7) !== 'console')) {

                    this.add(e, null, t);

                } //else console.log('>skipped', typeAlias, e.name);
            }, this));
        }

        if (parent) {
            t.inheritedTypes.push(parent);
        }
        if (child) {
            child.inheritedTypes.push(t);
        }

        return t;
    };

    function getFromCache(cacheKey) {
        // expire stuff
        var timeNow = new Date().getTime();
        _.each(cache, function (value, key) {
            if (value && value.expires < timeNow) {
                //console.log('spEntityService - expiring cache entry: ', key);
                delete cache[key];
            }
            //else console.log('spEntityService - leaving cache entry: ', key);
        });

        // look
        var cacheEntry = cache[cacheKey];
        if (cacheEntry) {
            //console.log('spEntityService - using cache entry: ', cacheKey);
            return cacheEntry.value;
        }
        return null;
    }

    function addToCache(key, value, timeoutMsec) {
        //console.log('spEntityService - caching entry: "%s" %o, cache count %s', key, value, _.keys(cache).length);
        cache[key] = { value: value, expires: new Date().getTime() + timeoutMsec };
    }

    var currentBatch = null;

    /*
     * Performs the actual batch request once the timer has elapsed.
     * @param {Object} rqBatch - The batch object, containing the array of requests.
     */
    function issueBatch(rqBatch) {
        // Remove from pool.. accepts no additional requests
        currentBatch = null;

        // Run the batch. This will fire any individual promises.
        rqBatch.runBatch();

        // Flush any async events.
        $rootScope.$apply();
    }

    /*
     * Queues a request up to be automatically processed as part of a batch.
     * @param {Object} request - the request object, as created by makeGetEntityRequest, or similar.
     */
    function runRequestAutoBatched(requestObject) {
        var delay = 0; //zero is sufficient to wait until the end of the digest cycle
        var rqBatch = currentBatch;

        if (!rqBatch) {
            rqBatch = new exports.BatchRequest();
            currentBatch = rqBatch;
            setTimeout(issueBatch, delay, rqBatch);
        }
        rqBatch.addRequest(requestObject);
    }


    /**
     * Runs a single request object.
     *
     * @param {Object} requestObject - A request object, as created by makeGetEntityRequest, or similar.
     * @param {Object} options - Additional options. Pass {batch:true} to automatically batch.
     * @returns {Object} A promise.
     */
    exports.runRequest = function (requestObject, options) {
        if (options) {
            if (options.hint) {
                requestObject.hint = options.hint + '-' + requestObject.hint;
            }
            if (options.isolated) {
                requestObject.isolated = true;
            }
        }

        if (options && options.batch && !exports.disableBatching) {
            if (options.batch instanceof exports.BatchRequest) {
                options.batch.addRequest(requestObject);
            } else {
                runRequestAutoBatched(requestObject);
            }
        } else {
            var batch = new exports.BatchRequest();
            batch.addRequest(requestObject);
            batch.runBatch();
        }

        return requestObject.promise;
    };

    /**
     * Construct a BatchRequest for holding multiple requests.
     * @class
     * @name spEntityService.BatchRequest
     */
    exports.BatchRequest = function BatchRequest() {
        var that = this;

        // represents the enture batch query sent over the wire
        this.actual = {
            queries: [],    // array of reusable request query strings
            requests: []    // array of individual requests
        };
        this.requests = []; // array of request objects that were added.
        this.hint = '';     // hint string

        /**
         * Adds an individual request object to a batch.
         *
         * @param {number | string} eid - the id or alias of the entity
         * @param {string} request - the request
         * @returns {Object} An object representing the request
         */
        this.addRequest = function (requestObject) {
            if (!requestObject || !requestObject.actual)
                throw new Error('Invalid requestObject');
            if (!requestObject.request)
                throw new Error('No request string.');
            // register whole request object, and actual network object
            this.requests.push(requestObject);
            var rqActual = requestObject.actual;
            if (requestObject.hint) {
                rqActual.hint = requestObject.hint;
            }
            if (requestObject.isolated) {
                rqActual.isolated = requestObject.isolated;
            }
            this.actual.requests.push(rqActual);
            // determine if the request string is already used.
            rqActual.rq = _.indexOf(this.actual.queries, requestObject.request);
            if (rqActual.rq === -1) {
                rqActual.rq = this.actual.queries.length;
                this.actual.queries.push(requestObject.request);
            }
            this.hint += ' ' + requestObject.hint;
        };

        /**
         * Runs the batch of requests. Only call once.
         *
         * @returns {Object} A promise called on completion. After all the individual promises are called.
         */
        this.runBatch = function () {
            var hint = this.hint ? this.hint.slice(1) : null;
            return exports._issueBatchRequest(this.actual, hint)
                .then(function(response) {
                    // Decode response
                    var json = response.data;
                    var results = spEntity.batchEntityDataVer2ToEntities(json);
                    if (results.length !== that.requests.length)
                        throw new Error('Wrong number of results');

                    // Process individual requests
                    for (var i = 0; i < results.length; i++) {
                        // Call back to user code
                        try {
                            var deferred = that.requests[i].deferred;
                            if (json.results[i].code !== 200) {
                                // handle failure of individual request
                                deferred.reject(json.results[i].code);
                            } else {
                                deferred.resolve(results[i]);
                            }
                        } catch(e) { // catch errors in user processing
                            console.error(e);
                        }
                    }
                },
                function (err) {
                    // Handle overall failure by notifying each requester
                    console.error('spEntityService.runBatch err: ' + (sp.result(err, 'status') || err));
                    _.each(that.requests, function (request) {
                        try {
                            request.deferred.reject(err);
                        } catch (e) {
                            console.error(e);
                        }
                    });
                });

        };
    };

    /**
     * Issues the batch request. Broken out so it can be mocked.
     *
     * @param {BatchRequest} batch - the batch object.
     * @returns {Object} The promise, to be passed the raw network results.
     */
    exports._issueBatchRequest = function(batch, hint) {
        var url = spWebService.getWebApiRoot() + '/spapi/data/v2/entity';
        if (hint) {
            url += '?' + encodeURIComponent(hint); // STRICTLY for debugging/logging only
        }
        var args = { headers: spWebService.getHeaders() };
        return $http.post(url, batch, args);
    };

    /**
     * Generic mechanism for creating requests
     *
     * @param {number | string} eid - the id or alias of the entity
     * @param {string} request - the entity request string
     * @returns {Object} An object representing the request
     */
    function makeRequest (requestType, request) {
        var deferred = $q.defer();
        return {
            // 'actual' represents the actual object that will be transmitted.
            actual: {
                get: requestType
            },
            request: request,
            deferred: deferred,
            promise: deferred.promise,
            hint: ''  // STRICTLY for logging/diagnostics only 
        };
    }


    /**
     * Generates a request object that represents a request for a single entity.
     *
     * @param {number | string} eid - the id or alias of the entity
     * @param {string} request - the request
     * @returns {Object} An object representing the request
     */
    exports.makeGetEntityRequest = function (eid, request) {
        var eid2 = spEntity.asAliasOrId(eid);

        var rq = makeRequest('basic', request);
        rq.hint = aliasOnly(eid);

        if (!_.isString(rq.hint)) {
            rq.hint = rq.hint.toString();
        }

        if (_.isString(eid2)) {
            rq.actual.aliases = [eid2];
        } else if (_.isFinite(eid2)) {
            rq.actual.ids = [eid2];
        }

        rq.promise = rq.promise.then(function (result) {
            if (result && result.length > 0)
                return result[0];
            else
                return null;
        });
        return rq;
    };


    /**
     * Generates a request object that represents a request for multiple entities.
     *
     * @param {number|string} eids - array of entity ids, each may be a number (id) or string (alias).
     * @param {string} request - The entity request to get for all of these instances.
     * @returns {Object} An object representing the request
     */
    exports.makeGetEntitiesRequest = function (eids, request) {
        if (!_.isArray(eids))
            throw new Error('Expected array of IDs or aliases.');

        var rq = makeRequest('basic', request);
        rq.hint = 'multiple';

        _.forEach(eids, function (eid) {
            if (_.isString(eid)) {
                if (!_.has(rq.actual, 'aliases')) {
                    rq.actual.aliases = [];
                }

                rq.actual.aliases.push(eid);
            } else if (_.isFinite(eid)) {
                if (!_.has(rq.actual, 'ids')) {
                    rq.actual.ids = [];
                }

                rq.actual.ids.push(eid);
            }
        });
        return rq;
    };


    /**
     * Generates a request object that represents a request for multiple entities.
     *
     * @param {number|string} typeEid - ID or alias of a resource type.
     * @param {string} request - The entity request to get for all of these instances. Defaults to alias, name, description.
     * @param {Object} options - an exactType option that can be true/false (false by default), and a filter string that can be a ReadiNow calculation that evaluates to a true/false.
     * @returns {Object} An object representing the request
     */
    exports.makeGetEntitiesOfTypeRequest = function (typeEid, request, options) {
        var opts = _.defaults(options || {}, { filter: null, exactType: false });

        var typeEid2 = spEntity.asAliasOrId(typeEid);

        var rqType;
        if (opts.filter)
            rqType = opts.exactType ? 'filterexactinstances' : 'filterinstances';
        else
            rqType = opts.exactType ? 'exactinstances' : 'instances';
        
        var rq = makeRequest(rqType, request || 'alias,name,description');

        if (_.isString(typeEid2)) {
            rq.actual.aliases = [typeEid2];
        } else if (_.isFinite(typeEid2)) {
            rq.actual.ids = [typeEid2];
        }
        if (opts.filter) {
            rq.actual.filter = opts.filter;
        }

        rq.hint = 'instOf-' + aliasOnly(typeEid);
        return rq;
    };


    /**
    * Takes an EntityPackage returned from a custom service and makes it accessible based on the named queries
    * Part of a series of APIs to pass arbitrary entity data from the server back to the client over custom services. See EntityPackage on the server.
    *
    * @param {object} entityPackage - Take an entity package and unwrap the named queries.
    */
    exports.extractNamedQueriesFromBatch = function (batch) {
        var result;
        var data = batch.data;

        var names = _.map(data.results, 'name');
        var singleNames = _.map(_.filter(data.results, 'single'), 'name');

        var batchResult = spEntity.batchEntityDataVer2ToEntities(data);

        result = _.zipObject(names, batchResult);

        _.each(singleNames, function (singleName) { result[singleName] = _.first(result[singleName]); });

        return result;
    };


    /**
     * Takes an entity object and packages it so that it can be sent to the server as part of an adhoc request.
     * Decode at the other end with EntityNugget.DecodeEntity. Do not rely on the internal format. It will almost certainly change.
     *
     * @param {object} entity - Take the entity to be package.
     */
    exports.packageEntityNugget = function (entity) {
        if (!entity)
            return { v1: null };
        if (!spEntity.isEntity)
            throw new Error('Expected an entity');
        // For v1
        var encoded = {
            v1: buildPostData(entity, { changesOnly: false })
        };
        return encoded;
    };

    /**
     * Cache an entity in the cache with key 'entity:{id}'. Possible options include
     * to replace or merge with any existing entity (merge is the default)
     * and the expiry options - see code for defaults, not repeating here.
     * @param entity
     * @param options
     */
    function addEntityToCache(entity, options) {

        console.assert(entity && entity.id());

        // todo - complete the implementation as per description above.
        // in the meantime we are using the existing caching mechanism
        addToCache('entity:' + entity.eid().id(), entity, 10 * 60 * 1000);
        addToCache('entity:' + entity.eid().getNsAlias(), entity, 10 * 60 * 1000);

    }
    exports.addEntityToCache = addEntityToCache;

    /**
     * Get the entity from the cache if it exists otherwise return null.
     * @param id
     * @returns {*}
     */
    function getEntityFromCache(id) {

        // todo - complete the implementation as per description above.
        // in the meantime we are using the existing caching mechanism
        return getFromCache('entity:' + id);
    }
    exports.getEntityFromCache = getEntityFromCache;

    return exports;
});

