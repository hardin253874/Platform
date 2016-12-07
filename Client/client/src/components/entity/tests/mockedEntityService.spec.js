// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity, jsonString, jsonInt, jsonLookup, jsonRelationship, jsonDecimal, jsonBool,
  jsonCurrency, jsonDate, jsonDateTime, jsonTime */

angular.module('mockedEntityService', ['ng']);

angular.module('mockedEntityService').factory('spEntityService', function($q, $timeout, $rootScope) {
    /**
     *  A set of client side services working against the entity webapi service.
     *  @module spEntityService
     */
    var exports = {};

    exports.isMockService = true;

    exports.putEntity = function () { throw new Error('putEntity has not been mocked.'); };
    exports.deleteEntity = function () { throw new Error('deleteEntity has not been mocked.'); };
    exports.setWebApiRoot = function () { throw new Error('setWebApiRoot has not been mocked.'); };
    exports.deleteEntities = function () { throw new Error('deleteEntities has not been mocked.'); };

    var getInstancesOfTypeData = {};
    var getEntitiesOfTypeMock = {};
    var getEntityMocks = {};

    /* Create a mock and register it with the injector. */
    exports.register = function () {
        module(function ($provide) {
            $provide.value('spEntityService', this);
        });
    };

    /* This method sets the spEntity.Entity to be returned by the getEntity method
    whever an entity by that ID or alias is requested. */
    exports.mockGetEntityJSON = function (json) {
        var entity = spEntity.fromJSON(json);
        this.mockGetEntity(entity);
        return this;
    };

    /* This method sets the spEntity.Entity to be returned by the getEntity method
    whever an entity by that ID or alias is requested. */
    exports.mockGetEntity = function (entity) {
        var arr = _.isArray(entity) ? entity : [entity];

        for (var i = 0; i < arr.length; i++) {
            var e = arr[i];
            var data = { entity: e };
            getEntityMocks[e.eid().getId()] = data;
            getEntityMocks[e.eid().getNsAlias()] = data;
            if (e.eid().getNamespace() === 'core') {
                getEntityMocks[e.eid().getAlias()] = data;
            }
        }
        return this;
    };

    /* This method sets the spEntity.Entity to be returned by the getEntity method
    whever an entity by that ID or alias is requested. */
    exports.mockGetEntityNotFound = function (entity) {
        var arr = _.isArray(entity) ? entity : [entity];

        for (var i = 0; i < arr.length; i++) {
            var e = arr[i];
            var data = { notFound:true };
            getEntityMocks[e.eid().getId()] = data;
            getEntityMocks[e.eid().getNsAlias()] = data;
            if (e.eid().getNamespace() === 'core') {
                getEntityMocks[e.eid().getAlias()] = data;
            }
        }
        return this;
    };


    /* Mocked implementation of the getEntity method */
    exports.getEntity = function (eid, request) {
        var data = getEntityMocks[eid];
        if (!data)
            throw new Error('No mock data was provided for ' + eid);

        if (data.notFound)
            return $q.when(null);

        return $q.when(data.entity);
    };

    /**
     * Mocked implementation of the getEntities method... simply a concat of individual getEntity.
     * This does NOT return a merged object graph that the actual call does do, unless the caller
     * has mocked the data so.
     */
    exports.getEntities = function (eids, request) {
        console.log('mock getEntities ', eids);
        var result = [];

        eids = _.isArray(eids) ? eids: [eids];

        _.each(eids, function(eid) {

            var data = getEntityMocks[eid];
            if (!data)
                throw new Error('No mock data was provided for ' + eid);

            result.push(data.entity);
        });

        return $q.when(result);
    };

    /* This method sets the data to be returned by the getInstancesOfType method
    for the given type id. */
    exports.mockGetInstancesOfTypeRawData = function (typeEid, data) {
        getInstancesOfTypeData[typeEid] = data;
    };
    

    /* Mocked implementation of the getInstancesOfType method */
    exports.getInstancesOfType = function (typeEid, request, refresh) {
        var data = getInstancesOfTypeData[typeEid];
        if (!data)
            throw new Error('No mock data was provided for ' + typeEid);

        var entities = spEntity.entityDataToEntities(data),
            rel = entities[0].relationship('core:instancesOfType'),
            instances = rel.map(function (e) {
                return {
                    id: e.id(),
                    alias: e.field('alias') || '',
                    name: e.field('name') || '#NoName',
                    description: e.field('description') || '',
                    entity: e
                };
            });

        var deferred = $q.defer();
            deferred.resolve(instances);
            return deferred.promise;
    };

    /* This method sets the data to be returned by the getEntitiesOfType method
    for the given type id. */
    exports.mockGetEntitiesOfType = function (typeEid, entities) {
        getEntitiesOfTypeMock[typeEid] = _.isArray(entities) ? entities : [entities];

        return this;
    };

    /* Mocked implementation of the getEntitiesOfType method */
    exports.getEntitiesOfType = function (typeEid, request) {
        var data = getEntitiesOfTypeMock[typeEid];

        if (!data)
            throw new Error('No mock data was provided for ' + typeEid);        
        
        return $q.when(data);
    };

    /**
 * Generic mechanism for creating requests
 *
 * @param {number | string} eid - the id or alias of the entity
 * @param {string} request - the entity request string
 * @returns {Object} An object representing the request
 */
    function makeRequest(requestType, request) {
        var deferred = $q.defer();
        return {
            // 'actual' represents the actual object that will be transmitted.
            actual: {
                get: requestType
            },
            request: request,
            deferred: deferred,
            promise: deferred.promise
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
        rq.actual.ids = [eid2];
        rq.promise = rq.promise.then(function (result) {
            if (result && result.length > 0)
                return result[0];
            else
                return null;
        });
        return rq;
    };

    /**
     * Construct a BatchRequest for holding multiple requests.
     * @class
     * @name spEntityService.BatchRequest
     */
    exports.BatchRequest = function() {
        var that = this;
        this.requests = []; // array of request objects that were added.

        /**
         * Adds an individual request object to a batch.
         *
         * @param {number | string} eid - the id or alias of the entity
         * @param {string} request - the request
         * @returns {Object} An object representing the request
         */
        this.addRequest = function(requestObject) {
            that.requests.push(requestObject);
        };

        /**
         * Runs the batch of requests. Only call once.
         *
         * @returns {Object} A promise called on completion. After all the individual promises are called.
         */
        this.runBatch = function () {
            var deferred = $q.defer();
            var outstandingRequests = that.requests.slice();

            _.forEach(that.requests, function(request) {
                exports.getEntity(request.actual.ids[0], request.request).then(function(results) {

                    request.deferred.resolve([results]);

                    outstandingRequests.splice(outstandingRequests.indexOf(request), 1);

                    if (outstandingRequests.length === 0) {
                        deferred.resolve();
                    }
                }, function(error) {
                    request.deferred.reject(error);
                });
            });

            return deferred.promise;
        };
    };

    // because lots of things need to mock the template report
    exports.mockTemplateReport = function mockTemplateReport() {
        var templateReport = spEntity.fromJSON({
            id: { id: 22222, ns: 'core', alias: 'templateReport' }
        });
        exports.mockGetEntity(templateReport);
    };

    return exports;
});

