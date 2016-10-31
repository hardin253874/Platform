// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, spReportEntity */

angular.module('spApps.reportServices', ['mod.common.spReportDataCacheService', 'mod.common.spEntityService', 'mod.common.spWebService', 'sp.navService', 'sp.common.filters', 'mod.common.spReportHelpers']);

/**
 *  A set of client side services working against the entity webapi service.
 *  @module spReportService
 */

angular.module('spApps.reportServices').factory('spReportService', function ($http, $q, $timeout, $filter, spReportDataCacheService, spEntityService, spWebService, spNavService, spReportHelpers) {
    'use strict';
    var exports = {};

    function webApiRoot() {
        return spWebService.getWebApiRoot();
    }


    /**
     * Internal function to run a report and return a promise.
     * Notes
     * - this is a little old and could be consolidated with getReportData.
     * - I don't think postData is used...
     */
    function xhrRunReport(reportId, options, postData) {

        //console.log('runReport id %s, params %o, post %o', reportId, params, postData);

        if (!reportId) {
            return $q.reject('no report id');
        }

        _.defaults(options, {
            entityTypeId: options.picker,
            metadata: 'basic',
            startIndex: 0,
            pageSize: 500
        });

        return getReportData(reportId, options).then(function (data) {

            // reshape the data from the new report service to that which our somewhat legacy callers expect

            var results = { cols: [], data: [], id: reportId };
            results.cols = _.map(_.sortBy(data.meta.rcols, 'ord'), function (c) {
                return _.pick(c, 'title', 'type');
            });
            results.data = _.map(data.gdata, function (d) {
                return {
                    id: d.eid,
                    item: _.map(d.values, function (v) {
                        return { value: v.val || _.first(_.values(v.vals))};
                    })
                };
            });

            console.log('getReportData returning %o, legacy return is %o', data, results);
            return results;
        });
    }


    /**
     * Find the default picker report for the given resource type (entity type) and
     * run it in picker mode passing the resource type as a constraint.
     * @param {string|number} typeId
     * @param {object} params
     * @param {object} postData
     * @returns A promise
     *
     * @todo update this function to use the spEntityService rather than direct $http on the webapi
     */
    exports.runDefaultReportForType = function (typeId, options) {
        // given the entityRef for a entity type we first do a query to find the default
        // report for the type, and then we run the report passing the type as the 'picker'

        // return a promise for the report data

        var url = spEntityService._getEntityRequestUrl(typeId);    // fix me

        //console.log('runDefaultReportForType entity %o', typeId);

        return $http({
            method: 'GET',
            url: url,
            params: { request: 'defaultPickerReport.{alias,name},inherits*.defaultPickerReport.{alias,name}' },
            headers: spWebService.getHeaders()
        }).then(function (data) {

                var reportId = null;
                var entities = spEntity.entityDataToEntities(data.data);
                var entity = entities[0];
                var entityTypeId = entity.id();

                // search up through the types and inherited types to find the default report

                while (!reportId && entity) {
                    var defaultReportRel = entity.relationship('core:defaultPickerReport');
                    if (defaultReportRel.length > 0) {
                        reportId = entity.relationship('core:defaultPickerReport')[0].id();
                        console.log('runDefaultReportForType: using report %o from type %o for type %o', entity.getDefaultPickerReport(), spEntity.toJSON(entity), typeId);
                    } else {
                        entity = entity.getLookup('core:inherits');
                    }
                }

                return xhrRunReport(reportId, _.defaults({ picker: entityTypeId }, options));
            });
    };


    /**
     * Run the report identified by the given entityRef and return a promise of the report results
     * @param {string|number} entityRef
     * @param {object} params
     * @param {object} postData
     * @returns A promise
     */
    exports.runReport = function (entityRef, params, postData) {

        entityRef = new spEntity.EntityRef(entityRef);

        // if given an alias then we need to look up the id
        // todo - change the webapi to support getting an alias

        if (!entityRef._alias) { // yeah ok, accessing a 'private'....
            return xhrRunReport(entityRef.id(), params, postData);
        }

        return $http({
            method: postData ? 'POST' : 'GET',
            url: webApiRoot() + '/spapi/data/v1/entity/' + (entityRef._ns || 'core') + '/' + entityRef._alias,
            params: { request: 'id' },
            data: postData,
            headers: spWebService.getHeaders()
        }).then(function (data) {

                var entities = spEntity.entityDataToEntities(data.data);
                return xhrRunReport(entities[0].id(), params, postData);
            });
    };


    /**
     * Run a structured query. The format is like...
     * @param {object} query
     * @returns A promise
     */
    exports.runQuery = function (query) {
        return $http({
            method: 'POST',
            url: webApiRoot() + '/spapi/data/v1/entity/query',
            data: query,
            headers: spWebService.getHeaders()
        }).then(function (response) {
                return response.status === 200 ? response.data : null;
            });
    };

    function runResourcePickerReport(report) {

        // assume the report has been partially built, just waiting for data (and setting of cols if different from the default)

        if (!(report.parameters && report.parameters.resourceType && report.parameters.resourceType.value)) {
            // resolve immediately... no data, but caller can see it needs a parameter
            return $q.when(report);
        }

        return exports.runDefaultReportForType(report.parameters.resourceType.value).then(function (results) {

            report.results.cols = results.cols;
            report.results.data = results.data;
            return report;
        });
    }

    /**
     * @deprecated
     *
     * This function looks a little odd as it has had unused code removed, but remaining hasn't been refactored
     * as it will go too one day.
     */
    exports.runPickerReport = function (id, parameters) {

        var report = {
            id: id,
            results: {
                cols: [
                    { title: 'Name', type: 'String' },
                    { title: 'Description', type: 'String' }
                ],
                data: []
            },
            parameters: {}
        };

        parameters = parameters || {};

        if (id === 'resources') {

            report.parameters = _.extend({ resourceType: { value: null, type: 'resourceType' } }, _.pick(parameters, 'resourceType'));
            return runResourcePickerReport(report);
        }

        return $q.reject('unknown picker ' + id);
    };


    // New report API methods



    /**
     * Gets the data for the specified report.
     * @param {number} reportId - the report id.
     * @param {string} type - metadata type. Valid values are 'full' or 'basic'
     * @returns {promise} of report metadata.
     */
    function getReportData(reportId, options) {
        var metadata = null;
        var page = null;
        var doPost = false;
        var data = {};
        var entityTypeId = null;
        var relationDetailString = null;
        var relent = {};
        var maxCols;
        var onErr;
        var isRefresh = false;
        var canCache = false;
        var useCache = false;
        var loadLatest;
        var timerKey = 'getReportData: ' + reportId + ', ' + JSON.stringify(options || {});

        if (options) {

            // cache first pages only if paging is used
            canCache = !angular.isDefined(options.startIndex) || options.startIndex === 0;

            if (options.metadata) {
                metadata = options.metadata;
            }

            if (angular.isDefined(options.startIndex) &&
                angular.isDefined(options.pageSize)) {
                page = options.startIndex + ',' + options.pageSize;
            }

            if (options.entityTypeId) {
                entityTypeId = options.entityTypeId;
                canCache = false; // don't cache picker/templated reports
            }

            if (angular.isDefined(options.relationDetail) &&
                angular.isDefined(options.relationDetail.eid) &&
                angular.isDefined(options.relationDetail.relid) &&
                angular.isDefined(options.relationDetail.direction)) {
                relationDetailString = options.relationDetail.eid + ',' + options.relationDetail.relid + ',' + options.relationDetail.direction;
            }

            if (angular.isDefined(options.inclids) && _.isArray(options.inclids) && options.inclids.length > 0) {
                relent.inclids = options.inclids;
            }

            if (angular.isDefined(options.exclids) && _.isArray(options.exclids) && options.exclids.length > 0) {
                relent.exclids = options.exclids;
            }

            if (angular.isDefined(relent.inclids) || angular.isDefined(relent.exclids)) {
                data.relent = relent;
                doPost = true;
            }

            if (angular.isDefined(options.relfilters) && _.isArray(options.relfilters) && options.relfilters.length > 0) {
                doPost = true;
                data.relfilters = options.relfilters;
            }

            if (angular.isDefined(options.filtereids) && _.isArray(options.filtereids) && options.filtereids.length > 0) {
                doPost = true;
                data.filtereids = options.filtereids;
            }

            if (options.sort) {
                doPost = true;
                data.sort = options.sort;
                canCache = false; // don't cache sorted results
            }

            if (options.conds) {
                doPost = true;
                data.conds = options.conds;
            }

            if (options.rollup) {
                doPost = true;
                data.rollup = options.rollup;
            }

            if (options.quickSearch) {
                doPost = true;
                data.qsearch = options.quickSearch;
                canCache = false; // don't cache quick searched results
            }

            if (!_.isUndefined(options.isReset)) {
                doPost = true;
                data.isreset = options.isReset;
            }

            if (options.maxCols) {
                maxCols = options.maxCols;
            }

            if (options.onErr) {
                onErr = options.onErr;
            }

            if (options.isRefresh) {
                isRefresh = options.isRefresh === true;
            }
        }
        
        var params = {
            metadata: metadata,
            page: page,
            type: entityTypeId,
            relationship: relationDetailString,
            cols: maxCols
        };

        var fnReq = function () {
            console.time(timerKey);

            return $http({
                method: doPost ? 'POST' : 'GET',
                url: webApiRoot() + '/spapi/data/v1/report/' + sp.aliasOrIdUri(reportId),
                data: _.isEmpty(data) ? null : data,
                params: params,
                headers: spWebService.getHeaders()
            });
        };

        var cacheKey = spReportDataCacheService.getCacheKey(reportId, params, data);

        if (options && options.shouldCache && _.isFunction(options.shouldCache)) {
            // determine if local storage is to be used in this report data request
            useCache = (options.shouldCache() && canCache && (spReportDataCacheService.disabled !== true)) === true;

            // check if instructed to load up the latest data subsequent to returning any from cache
            if (useCache && spNavService.getCacheMarker(cacheKey) === true) {

                // receive direction on how to update the relevant controls once we receive the updated data
                if (options.loadLatest && _.isFunction(options.loadLatest)) {
                    loadLatest = options.loadLatest;
                }
            }
        }

        if (useCache && !isRefresh) {
            return spReportDataCacheService.loadCachedReportData(cacheKey).then(function (cachedData) {
                if (cachedData) {
                    // update underneath the cached data...
                    if (loadLatest) {
                        $timeout(function () {
                            sp.logTime('Report request to server ' + reportId + ' (post cache)');

                            // ...when it finally returns.
                            fnReq().then(function (response) {
                                sp.logTime('Report server response ' + reportId + ' (post cache)');

                                spReportDataCacheService.cacheReportData(cacheKey, response.data); // update the cache
                                spNavService.clearCacheMarker(cacheKey); // exclude this key
                                loadLatest(response.data);
                            }).finally(function() {
                                console.timeEnd(timerKey);
                            });
                        });
                    }
                    return $q.when(cachedData);
                } else {
                    sp.logTime('Report request to server ' + reportId + ' (caching)');

                    return fnReq().then(function(response) {
                        sp.logTime('Report server response ' + reportId + ' (caching)');

                        spReportDataCacheService.checkReportModifiedCache(reportId, response.data);
                        spReportDataCacheService.cacheReportData(cacheKey, response.data); // add the missing data to cache

                        return response.data;
                    }, onErr).finally(function () {
                        console.timeEnd(timerKey);
                    });
                }
            });
        } else {
            // sp.logTime('Report request to server ' + reportId);

            return fnReq().then(function (response) {
                // sp.logTime('Report server response ' + reportId);

                // check if any caches are stale
                spReportDataCacheService.checkReportModifiedCache(reportId, response.data);
                if (useCache && isRefresh) {
                    // store refreshed data as it may be more up-to-date (combine later on request)
                    spReportDataCacheService.cacheReportData(cacheKey, response.data);
                    spNavService.clearCacheMarker(cacheKey);
                }

                return response.data;
            }, onErr).finally(function () {
                console.timeEnd(timerKey);
            });
        }
    }

    exports.getReportData = getReportData;

    /**
     * Puts the data for the specified report.
     * @param {number} reportId - the report id.
     * @param {string} type - metadata type. Valid values are 'full' or 'basic'
     * @returns {promise} of report metadata.
     */
    exports.putReport = function (reportId, options) {
        var data = {};

        if (options) {
            if (options.sort) {
                data.sort = options.sort;
            }

            if (options.cfrules) {
                data.cfrules = options.cfrules;
            }

            if (options.conds) {
                data.conds = options.conds;
            }

            if (options.valrules) {
                data.valrules = options.valrules;
            }
        }

        return $http({
            method: 'PUT',
            url: webApiRoot() + '/spapi/data/v1/report/' + sp.aliasOrIdUri(reportId),
            data: _.isEmpty(data) ? null : data,
            headers: spWebService.getHeaders()
        }).then(function (response) {
                return response.data;
            });
    };


    /**
     * Gets the data for the specified report.
     * @param {object} reportEntity - the report model.
     * @param {string} type - metadata type. Valid values are 'full' or 'basic'
     * @returns {promise} of report metadata.
     */
    exports.getReportDataByReportEntity = function (reportEntity, options) {
        var metadata = null;
        var page = null;

        if (options) {
            if (options.metadata) {
                metadata = options.metadata;
            }

            if (angular.isDefined(options.startIndex) &&
                angular.isDefined(options.pageSize)) {
                page = options.startIndex + ',' + options.pageSize;
            }
        }
        var tempReportEntity = spReportEntity.cloneReportEntityNugget(reportEntity);
        var reportData = spEntityService.packageEntityNugget(tempReportEntity);

        return $http({
            method: 'POST',
            url: webApiRoot() + '/spapi/data/v1/report/builder/entity/0',
            data: reportData,
            params: {
                metadata: metadata,
                page: page
            },
            headers: spWebService.getHeaders()
        }).then(function(response) {
            return response.data;
        });
    };

    /**
     * Get the specified reportmodel which include report settings and report query from report.
     * @param id
     * @returns {promise} of report model.
     */
    exports.getReportModel = function (rid) {

        if (!rid) {
            return $q.reject('no report id');
        }

        return $http({
            method: 'GET',
            url: webApiRoot() + '/spapi/data/v1/report/getreportmodel/' + rid,
            headers: spWebService.getHeaders()
        }).then(function (response) {
                console.log(response.status);
                return response.status === 200 ? response.data : null;
            });
    };

    /**
     * Get the specified reportmodel which include report settings and report query from report.
     * @param id
     * @returns {promise} of report model.
     */
    exports.getNewReportName = function (rid) {

        if (!rid) {
            return $q.reject('no report id');
        }

        return $http({
            method: 'GET',
            url: webApiRoot() + '/spapi/data/v1/report/builder/newname/' + sp.aliasOrIdUri(rid),
            headers: spWebService.getHeaders()
        }).then(function (response) {
                console.log(response.status);
                return response.status === 200 ? response.data : null;
            });
    };

    /**
    * save as the existing report by different report name
    * @param id
    * @param reportName
    * @returns {promise} of new report id.
    */
    exports.saveAsReport = function(rid,reportName) {
        if (!rid) {
            return $q.reject('no report id');
        }

        return $http({
            method: 'POST',
            url: webApiRoot() + '/spapi/data/v1/report/saveas/' + rid,
            data: reportName,
            headers: spWebService.getHeaders()
        }).then(function (response) {
            return response.status === 200 ? response.data : null;
        });
    };

    /**
     * Update the specified reportmodel which include report settings and report query to report.
     * @param id
     * @returns {promise} of report model.
     */
    exports.updateReportModel = function (rid, reportModel) {

        if (!rid) {
            return $q.reject('no report id');
        }

        if (!reportModel) {
            return $q.reject('no report model');
        }

        return $http({
            method: 'POST',
            url: webApiRoot() + '/spapi/data/v1/report/update/' + rid,
            data: reportModel,
            headers: spWebService.getHeaders()
        }).then(function (response) {
                console.log(response.status);
                return response.status === 200 ? response.data : null;
            });

    };


    /**
     * Returns a function for formatting data as a string
     * @param rcol
     * @param valRule
     * @returns Formatter function.
     */
    // Return a format function for the specified column.
    exports.getColumnFormatFunc = spReportHelpers.getColumnFormatFunc;

    /**
    * Clears any data associated with the reportId from the local storage cache.
    * @param {long} reportId
    */
    exports.clearReportDataCache = function(reportId) {
        spReportDataCacheService.clearReportData(reportId);
    };

    return exports;

});
