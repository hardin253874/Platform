// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity */

angular.module('spApps.enumValueService', ['mod.common.spWebService', 'mod.common.spLocalStorage', 'angular-data.DSCacheFactory']);

/**
 *  A set of client side services working against the entity webapi service.
 *  @module spReportService
 */

angular.module('spApps.enumValueService').factory('spEnumValueService', function ($http, $q, $timeout, spWebService, DSCacheFactory) {
    'use strict';
    var exports = {
        getEnumValue: getEnumValue,
        invalidateEnumValuesForEntityType: invalidateEnumValuesForEntityType
    };


    var pendingFormRequests = {};    
    var reportLocalStorageSettings = {
        capacity: 50,
        maxAge: 30 * 60 * 1000,
        recycleFreq: 5 * 60 * 1000,
        deleteOnExpire: 'aggressive',
        storageMode: 'localStorage'
    };

    var enumValueCache = new DSCacheFactory('enumValueCache', reportLocalStorageSettings);
    var entityTypeToKeysCache = {};

    return exports;

    function getEnumValue(reportId, options) {
        var metadata = null;       
        var entityTypeId = null;
        var doPost = false;
        var data = {};
        var loadLatest;
        var entityTypeKeys;

        var timerKey = 'getEnumValue: ' + reportId + ', ' + JSON.stringify(options || {});

        if (options.metadata) {
            metadata = options.metadata;
        }

        if (options.entityTypeId) {
            entityTypeId = options.entityTypeId;
        }

        var params = {
            metadata: metadata,
            type: entityTypeId
        };

        var fnReq = function () {
            console.time(timerKey);

            return $http({
                method: doPost ? 'POST' : 'GET',
                url: webApiRoot() + '/spapi/data/v1/report/' + spUtils.aliasOrIdUri(reportId),
                data: _.isEmpty(data) ? null : data,
                params: params,
                headers: spWebService.getHeaders()
            });
        };

        var cacheKey = getCacheKey(reportId, params, data);
        //get same request form pendingRequest
        var pendingRequest = pendingFormRequests[entityTypeId];

        if (pendingRequest) {            
            return pendingRequest;
        }

        // Cache the entityTypeId to the cache key
        if (entityTypeId) {
            entityTypeKeys = entityTypeToKeysCache[entityTypeId];
            if (!entityTypeKeys) {
                entityTypeKeys = {};
                entityTypeToKeysCache[entityTypeId] = entityTypeKeys;
            }

            entityTypeKeys[cacheKey.name] = true;
        }        

        //load cached enumValues
        var request = loadCachedEnumValues(cacheKey).then(function (cachedData) {
            if (cachedData) {                
                pendingFormRequests[entityTypeId] = undefined;
                return cachedData;
            } else {
                spUtils.logTime('EnumValueReport request to server ' + reportId + ' (caching)');               

                return fnReq().then(function (response) {
                    spUtils.logTime('EnumValueReport server response ' + reportId + ' (caching)');
                    
                    cacheEnumValues(cacheKey, response.data); // add the enum values to cache
                    
                    return response.data;
                }).finally(function () {
                    console.timeEnd(timerKey);
                    pendingFormRequests[entityTypeId] = undefined;
                });
            }
        });

        pendingFormRequests[entityTypeId] = request;
        
        return request;
    }

    function webApiRoot() {
        return spWebService.getWebApiRoot();
    }

    //Load the value from enumValueCache. change the cache expired value to 2 mins.
    function loadCachedEnumValues(key) {
        if (key && key.name) {
            //get cached enum value
            var json = enumValueCache.get(key.name);            
            if (json) {                
                var data = JSON.parse(json.reportData);
                if (data) {
                    var now = (new Date()).getTime();
                    var expired = new Date(now - (2 * 60 * 1000)); // 2 mins
                    //get the 2 mins expired time stamp
                    var expiredTimeStamp = expired.toISOString();
                    // expire the data if its too old
                    if (json.timestamp < expiredTimeStamp) {
                        enumValueCache.remove(key.name);
                    } else {
                        return $q.when(data);
                    }
                }
            }
        }
        return $q.when(null);
    }
    
    function getExpiredTimeStamp()
    {
        var now = (new Date()).getTime();
        var expired = new Date(now - (2 * 60 * 1000)); // 2 mins
        //convert expired datetime value to timestamp format, otherwise, can't compare with the enumValueCache time
        //TODO, should be some easy way to convert the Date object to the same format of DSCacheFactory 
        enumValueCache.put('timestamp', { timestamp: expired, reportData: null });
        var expiredTimeStamp = enumValueCache.get('timestamp').timestamp;
        enumValueCache.remove('timestamp');

        return expiredTimeStamp;
    }

    function cacheEnumValues(key, data) {
        if (!key || !key.name) { return; }
        var json = JSON.stringify(data);
        try {
            var now = (new Date()).getTime();
            enumValueCache.put(key.name, { timestamp: new Date(now - 0), reportData: json });
        } catch (e) {
            console.error('Failed to cache enum Value data in local storage', e);
        }
    }

    function getCacheKey(reportId, params, data) {
        var meta = sp.result(params, 'metadata');
        var d = JSON.stringify(data);
        var p = JSON.stringify(params);
        var p2 = JSON.stringify(_.omit(params, 'metadata'));
        var key = {
            name: '' + reportId + '-' + getHash(d + (!meta ? p2 : p))
        };
        if (meta && meta.length > 0) {
            key.noMetaName = '' + reportId + '-' + getHash(d + p2);
        }
        return key;
    }

    function getHash(str) {
        var hash = 0, strlen = str.length;
        if (strlen === 0) {
            return hash;
        }
        var i, c;
        for (i = 0; i < strlen; i++) {
            c = str.charCodeAt(i);
            // jshint ignore:start
            hash = ((hash << 5) - hash) + c; // eslint-disable-line no-bitwise
            hash = hash & hash; // eslint-disable-line no-bitwise
            // jshint ignore:end
        }
        return hash;
    }
   
    function invalidateEnumValuesForEntityType(entityTypeId) {
        var entityTypeKeys;

        if (!entityTypeId) {
            return;
        }

        entityTypeKeys = entityTypeToKeysCache[entityTypeId];
        if (entityTypeKeys) {
            _.forOwn(entityTypeKeys, function (value, keyName) {
                enumValueCache.remove(keyName);
            });
        }

        delete entityTypeToKeysCache[entityTypeId];
    }    
});
