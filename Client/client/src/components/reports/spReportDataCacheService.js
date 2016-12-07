// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use strict';

    angular.module('mod.common.spReportDataCacheService', ['sp.common.loginService',
        'mod.common.spLocalStorage',
        'angular-data.DSCacheFactory']);

    /**
     * Service for the purpose of caching report data in local storage.
     *
     * @module spReportDataCacheService
     */

    angular.module('mod.common.spReportDataCacheService').factory('spReportDataCacheService', function ($q,
        $rootScope, $timeout, spLoginService, spLocalStorage, DSCacheFactory) {
        var exports = { disabled: false };
        var reportLocalStorageSettings = {
            capacity: 50,
            maxAge: 30 * 60 * 1000,
            recycleFreq: 5 * 60 * 1000,
            deleteOnExpire: 'aggressive',
            storageMode: 'localStorage'
        };

        var reportDataCache = new DSCacheFactory('reportDataCache', reportLocalStorageSettings);
        var reportChangedCache = new DSCacheFactory('reportChangedCache', reportLocalStorageSettings);

        function cacheReportData(key, data) {
            if (!key || !key.name || exports.disabled === true) { return; }
            $timeout(function () {
                var json = JSON.stringify(data);
                try {
                    reportDataCache.put(key.name, { timestamp: new Date(), reportData: json });
                } catch (e) {
                    console.error('Failed to cache report data in local storage', e);
                }
            });
        }

        /**
        * Adds report data to the local storage for later retrieval.
        * @param {object} key
        * @param {object} data
        */
        exports.cacheReportData = cacheReportData;

        function clearReportData(reportId) {
            var keys = reportDataCache.$$storage.getItem(reportDataCache.$$prefix + '.keys'); // hmmm...pfft.
            if (!keys) {
                return;
            }

            var p = '' + reportId + '-';
            var keysArr = JSON.parse(keys);
            var toRemove = _.filter(keysArr, function (key) {
                // add save code to check key startWith function
                return _.isString(key) && (key.indexOf(p) === 0);
            });

            if (toRemove) {
                var toRemoveLen = toRemove.length;
                if (toRemoveLen === 0) {
                    return;
                }
                for (var i = 0; i < toRemoveLen; i++) {
                    reportDataCache.remove(toRemove[i]);
                }
            }
        }

        /**
        * Clears any data associated with the reportId from the local storage cache.
        * @param {long} reportId
        */
        exports.clearReportData = clearReportData;

        function clearData() {
            reportDataCache.removeAll();
            reportChangedCache.removeAll();
        }

        /**
        * Clears all report data from the local storage cache.
        */
        exports.clearData = clearData;

        function loadCachedReportData(key) {
            if (key && key.name && exports.disabled !== true) {
                var now = (new Date()).getTime();
                var expired = new Date(now - (5 * 60 * 1000)); // 5 mins
                var expiredTimeStamp = expired.toISOString();
                var json = reportDataCache.get(key.name);
                if (json) {
                    var data = JSON.parse(json.reportData);
                    if (data) {
                        // check if there is more recent data available (on a refresh)
                        if (key.noMetaName && (key.name !== key.noMetaName)) {
                            var json2 = reportDataCache.get(key.noMetaName);
                            if (json2) {
                                if (json2.timestamp < expiredTimeStamp)
                                    reportDataCache.remove(key.noMetaName);
                                else {
                                    var data2 = JSON.parse(json2.reportData);
                                    if (data2) {
                                        if (json.timestamp < json2.timestamp) {
                                            data = _.assign(data, data2);
                                        }
                                    }
                                }
                            }
                        }
                        // expire the data if its too old
                        if (json.timestamp < expiredTimeStamp) {
                            reportDataCache.remove(key.name);
                        } else {
                            return $q.when(data);
                        }
                    }
                }
            }
            return $q.when(null);
        }

        /**
        * Loads the report data that has been stored against a given key.
        * @param {string} key
        */
        exports.loadCachedReportData = loadCachedReportData;

        function checkReportModifiedCache(reportId, data) {
            if (exports.disabled === true) {
                return;
            }

            var expire = false;
            var incomingDate = null;
            var date = sp.result(data, 'meta.modified');
            
            if (date) {
                incomingDate = new Date(date);
            } else {
                date = '';
            }

            if (incomingDate && incomingDate.length > 0) {
                var currentDate = reportChangedCache.get(reportId);
                if (currentDate === '') {
                    expire = true;
                } else {
                    if (currentDate && currentDate.length > 0) {
                        var t1 = new Date(currentDate);
                        var t2 = new Date(incomingDate);
                        expire = t1 < t2;
                    }
                }
            }

            if (expire) {
                clearReportData(reportId);
            }
            
            try {
                reportChangedCache.put(reportId, date);
            } catch (e) {
                console.error('Failed to cache the report last modified date in local storage', e);
            }
        }

        /**
        * Compares if the report for the incoming data has been modified since last cached.
        * @param {long} reportId
        * @param {object} data
        */
        exports.checkReportModifiedCache = checkReportModifiedCache;

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

        /**
        * Constructs a unique cache key for the report and the particular request.
        * @param {long} reportId
        * @param {object} params
        * @param {object} data
        */
        exports.getCacheKey = getCacheKey;

        function getHash(str) {
            var hash = 0, strlen = str.length;
            if (strlen === 0) {
                return hash;
            }
            var i, c;
            for (i = 0; i < strlen; i++) {
                c = str.charCodeAt(i);
                // jshint ignore:start
                hash = ((hash << 5) - hash) + c; //eslint-disable-line no-bitwise
                hash = hash & hash; //eslint-disable-line no-bitwise
                // jshint ignore:end
            }
            return hash;
        }

        function handleCacheReset() {
            var previousAccountId = spLocalStorage.getItem('reportCacheUserId');
            var currentAccountId = '' + spLoginService.accountId;

            if (previousAccountId !== currentAccountId) {
                spLocalStorage.setItem('reportCacheUserId', currentAccountId); // add key to local storage
                clearData();
            }
        }

        $rootScope.$on('signedin', function () {
            handleCacheReset();
        });

        handleCacheReset();

        return exports;
    });

}());