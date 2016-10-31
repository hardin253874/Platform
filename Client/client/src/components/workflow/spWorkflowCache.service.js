// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, jsonLookup, jsonString, jsonBool, jsonInt, spWorkflowConfiguration, spWorkflow */

(function () {
    'use strict';

    angular.module('mod.services.workflowService')
        .factory('spWorkflowCacheService', spWorkflowCacheService);

    /* @ngInject */
    function spWorkflowCacheService($q, spEntityService) {

        // simple cache of very static data.....
        // todo - add some expiry mechanism
        // todo - add flushing if the signed in user changes
        var cache = {};

        ///////////////////////////////////////////////////////////////////////
        // The interface
        //

        var exports = {
            invalidateCache: invalidateCache,
            getCacheableEntity: getCacheableEntity,
            resetCache: resetCache
        };

        return exports;

        ///////////////////////////////////////////////////////////////////////
        // The implementation
        //

        /**
         * Return a promise for the results of an entity request for a given entity.
         * May immediately resolve to a the cached results for some key.
         * If a request is made then the results will be cached for next time.
         */
        function getCacheableEntity(key, id, request, refresh) {
            console.time('getCacheableEntity:' + key);

            if (!refresh && cache[key]) {
                console.timeEnd('getCacheableEntity:' + key);
                return $q.when(cache[key]);
            }

            return spEntityService.getEntity(id, request, { batch: true }).then(function (entity) {
                cache[key] = entity;
                console.timeEnd('getCacheableEntity:' + key);
                console.log('getCacheableEntity: cache count', _.keys(cache).length);
                return entity;

            }, function (error) {
                console.error('spWorkflowService: error %o requesting %s', error, key, id, request);
                throw error;
            });
        }

        /**
         * Invalidates the entire cache
         */
        function resetCache() {
            console.log('resetCache');
            cache = {};
        }

        function invalidateCache(key) {
            cache[key] = null;
        }

    }

})();


