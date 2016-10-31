// Copyright 2011-2016 Global Software Innovation Pty Ltd

angular.module('mod.common.spVisDataBuilderService', [
    'mod.common.spVisDataService',
    'spApps.reportServices'
])
   /**
    * Module implementing generic data services for building visualisers.
    * @module spVisDataBuilderService
    */
    .service('spVisDataBuilderService', function ($q, spVisDataService, spReportService) {
        'use strict';

        function getAggregateMethods(type) {
            var res = [
                { alias: null, name: 'Value' },
                { alias: 'aggCountUniqueItems', name: 'Count (Unique)' }
            ];

            if (spEntity.DataType.isNumeric(type)) {
                res.push({ alias: 'aggSum', name: 'Sum' });
                res.push({ alias: 'aggAverage', name: 'Average' });
            }

            if (spEntity.DataType.isNumeric(type) || spEntity.DataType.isDateType(type) || type === 'ChoiceRelationship') {
                res.push({ alias: 'aggMax', name: 'Max' });
                res.push({ alias: 'aggMin', name: 'Min' });
            }
            return res;
        }

        //
        // loadReportMetadata
        //
        function loadReportMetadata(model) {
            var reportId = model.reportId;

            if (!reportId) {
                model.reportMetadata = null;
                return $q.when();
            }

            return spReportService.getReportData(reportId, { metadata: 'basic', startIndex: 0, pageSize: 0 })
                .then(function (reportResult) {
                    model.reportMetadata = reportResult.meta;
                });
        }


        //
        // makeSpecialSourceInfo
        //
        function makeSpecialSourceInfo(specialSourceAlias) {
            var info = spVisDataService.specialSources[specialSourceAlias];
            var specialSourceInfo = {
                name: info.name,
                colId: null,
                specialChartSource: specialSourceAlias,
                sourceAggMethod: null,
                type: info.type,
                getDrag: function () { return specialSourceInfo; }
            };
            return specialSourceInfo;
        }

        //
        // getAvailableReportColumnSources
        // -> [ { name, colId, specialChartSource, type, getDrag() } , ... ]
        //
        function getAvailableColumnSources(model, options) {
            options = _.defaults(options, {
                hideCount:false,
                hideRowNumber:false
            });
            var reportMetadata = sp.result(model, 'reportMetadata');
            if (!reportMetadata) {
                return [];
            }

            var rcols = reportMetadata.rcols;
            var keys = _.keys(rcols);

            // Create available sources
            var sources =
                _.chain(keys)
                    .map(function (key) {
                        return makeColumnSourceInfo(key, rcols[key]);
                    })
                    .sortBy('ord')
                    .value();

            if (!options.hideCount)
                sources.push(makeSpecialSourceInfo('core:countSource'));
            if (!options.hideRowNumber)
                sources.push(makeSpecialSourceInfo('core:rowNumberSource'));
            return sources;
        }

        //
        // makeColumnSourceInfo
        //
        function makeColumnSourceInfo(colId, colMetadata) {
            var res = {
                name: colMetadata.title,
                colId: parseInt(colId, 10),
                ord: colMetadata.ord,
                specialChartSource: null,
                sourceAggMethod: null,
                type: colMetadata.type,
                cardinality: colMetadata.card,
                getDrag: function () {
                    return res;
                }
            };
            return res;
        }

        //
        // getSourceInfoForSource
        // Returns a 'source info' object for a source entity.
        //
        function getSourceInfoForSource(model, chartSource) {
            var columnId = sp.result(chartSource, 'chartReportColumn.idP');
            if (!columnId) {
                return null;
            }

            var colMeta = model.reportMetadata.rcols[columnId];
            if (!colMeta) {
                console.warn('Could not find report column metadata for source.');
                return null;
            }

            var res = makeColumnSourceInfo(columnId, colMeta);
            return res;
        }

        //
        // createChartSource
        // Create a chartSource entity from a 'source info' object
        //
        function createChartSource(chartSourceInfo) {
            var source = spEntity.fromJSON({
                typeId: 'chartSource',
                name: jsonString(chartSourceInfo.name),
                chartReportColumn: jsonLookup(chartSourceInfo.colId),
                specialChartSource: jsonLookup(chartSourceInfo.specialChartSource),
                sourceAggMethod: jsonLookup(chartSourceInfo.sourceAggMethod)
            });
            return source;
        }

        var exports = {
            loadReportMetadata: loadReportMetadata,
            getAggregateMethods: getAggregateMethods,
            makeSpecialSourceInfo: makeSpecialSourceInfo,
            getAvailableColumnSources: getAvailableColumnSources,
            makeColumnSourceInfo: makeColumnSourceInfo,
            getSourceInfoForSource: getSourceInfoForSource,
            createChartSource: createChartSource,
            test: {
                makeColumnSourceInfo: makeColumnSourceInfo
            }
        };

        return exports;
    });