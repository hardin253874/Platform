// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, d3, sp, spEntity */
(function () {
    'use strict';

    // Standard service for supporting visualizations that are bound to data sources. E.g. charts.

    angular.module('mod.common.spVisDataService', [
        'spApps.reportServices',
        'mod.common.spReportHelpers',
        'mod.common.spEntityService',
        'sp.navService'
    ])
    .service('spVisDataService', ['$q', 'spReportService', 'spEntityService', 'spNavService', 'spReportHelpers', VisDataService]);

    function VisDataService($q, spReportService, spEntityService, spNavService, spReportHelpers) {

        var sourceRequest = 'name, chartReportColumn.id, specialChartSource.alias, sourceAggMethod.alias';

        // Defines special sources
        var specialSources = {
            'core:rowNumberSource': { name: 'Row Number', type: spEntity.DataType.Int32 },
            'core:countSource': { name: 'Count', type: spEntity.DataType.Int32 }
        };

        // Load any data required for the chart
        function requestVisData(visModel, options) {
            // Accepts: 
            // visModel = {
            //    reportId: number
            //    sources: [ array of chart sources ]
            //    sourcesRequestingAllData: [ array of chart sources ]
            // }
            // returns a promise to return visData (formerly chartData), which is {
            //    rowData: [ ... ]   // rows, suitable for being passed to accessors
            //    reportData: the raw report result
            //    lookups: { sourceId: entityArray, sourceId: ... etc }  // instances - if there were any
            // }

            var promise;
            var visData = { reportData: null, lookups: {} };

            // Report data
            var reportId = visModel.reportId;
            var reportRq = createReportRequest(visModel, options.externalConds);
            reportRq.isRefresh = sp.result(options, 'isRefresh') === true;
            reportRq.shouldCache = function () { return spNavService.getIsCacheable(); };
            reportRq.loadLatest = function (reportData) {
                visData.reportData = reportData;
                visData.reportData.meta.isPivot = !!reportRq.rollup;
                if (options.loadLatest && _.isFunction(options.loadLatest)) {
                    options.loadLatest(visData);
                }
            };
            promise = spReportService.getReportData(reportId, reportRq).then(function (reportData) {
                visData.reportData = reportData;
                visData.reportData.meta.isPivot = !!reportRq.rollup;

                if (!reportData.meta.isPivot) {
                    visData.rowData = reportData.gdata;
                } else {
                    visData.rowData = _.filter(reportData.meta.rdata, { map: 0 });
                }
                return visData;
            });

            // Source that load all data
            var sources = visModel.sourcesRequestingAllData || [];
            if (sources.length) {
                promise = promise.then(function () {
                    // types we need to load data for
                    var types = _.compact(_.uniq(_.map(sources, function (source) {
                        var info = locateColumnContent(source, visData);
                        return (info.colMeta.type === 'InlineRelationship' || info.colMeta.type === 'UserInlineRelationship') ? info.colMeta.tid : null;
                    })));
                    // dict of promises
                    var lookupsRqs = _.zipObject(types, _.map(types, function (typeId) {
                        return spEntityService.getEntitiesOfType(typeId, 'name').then(function (entities) {
                            return _.sortBy(entities, 'name');
                        });
                    }));
                    return $q.all(lookupsRqs);
                }).then(function (lookups) {
                    visData.lookups = lookups;
                    return visData;
                });
            }

            return promise;
        }


        // Create a createReportRequest
        function createReportRequest(visModel, externalConds) {
            var rq = { metadata: 'full' };

            if (isPivot(visModel)) {
                rq.rollup = createRollupRequest(visModel);
                rq.startIndex = 0;
                rq.pageSize = 0; // pivot charts return their data in the metadata section
            }

            if (externalConds) {
                rq.conds = externalConds;
            }
            return rq;
        }


        // Determine if the source model represents pivoted data
        function isPivot(visModel) {
            var res = _.some(visModel.sources, function (source) {
                return source.sourceAggMethod !== null || sp.result(source, 'specialChartSource.nsAlias') === 'core:countSource';
            });
            return res;
        }


        // Create a createRollupRequest
        function createRollupRequest(visModel) {

            var groups = {};
            var aggs = {};

            // For each source, determine if its a group or aggregate
            // Then check to see if it is already included in the request
            var sources = visModel.sources;
            _.forEach(sources, function (source) {
                var aggAlias = sp.result(source, 'sourceAggMethod.eidP.getAlias');
                var colId = sp.result(source, 'chartReportColumn.idP');
                var specialChartSource = sp.result(source, 'specialChartSource.nsAlias');

                if (specialChartSource === 'core:countSource') {
                    aggs[0] = [{ style: 'aggCount' }];
                } else if (aggAlias) {
                    // agg
                    var agg = aggs[colId];
                    if (!agg) {
                        agg = aggs[colId] = [];
                    }
                    var hasStyle = _.some(agg, { style: aggAlias });
                    if (!hasStyle) {
                        agg.push({
                            style: aggAlias
                        });
                    }
                } else {
                    // group
                    if (colId !== null && !_.isUndefined(colId)) {
                        if (!groups[colId]) {
                            groups[colId] = {
                                style: 'groupList'
                            };
                        }
                    }
                }
            });

            var rollupRq = {
                groups: _.map(_.keys(groups), function (id) {
                    var res = {};
                    res[id] = groups[id];
                    return res;
                }),
                aggs: aggs,
                ignorerows: true  // don't get individual row data
            };
            return rollupRq;
        }


        // Basic locator for column data.
        // Used to abstract the different formats for flat report data vs pivot report data
        function locateColumnContent(sourceEntity, visData) {
            // Format of result = {
            //    colMeta:  the colMeta object
            //    valRule:  the value rules that apply to the column
            //    accessor: translates row -> value object
            //    cfRule:   conditional formatting info for the column
            //    isFirstCol: true if its the first column
            // }

            var reportMetadata = visData.reportData.meta;

            var res;
            var valRule = null;

            var colId = sp.result(sourceEntity, 'chartReportColumn.idP');
            if (!colId)
                return null;

            var colMeta = reportMetadata.rcols[colId];
            var cfRule = sp.result(reportMetadata, 'cfrules.' + colId);  // conditional formatting metadata

            // Get the value formatting for the specified column.
            if (reportMetadata.valrules) {
                valRule = reportMetadata.valrules[colId];
            }

            // Flat columns
            // accessor here accepts a row and returns a value object
            if (!reportMetadata.isPivot) {
                var ord = colMeta ? colMeta.ord : null; // index of column within each row
                res = {
                    colMeta: colMeta,
                    valRule: valRule,
                    accessor: function (row) {
                        return row ? row.values[ord] : null;
                    },
                    cfRule: cfRule,
                    isFirstCol: ord === 0
                };
                res.colMeta.colTitle = res.colMeta.title;
                return res;
            }

            // Pivot columns (grouped)
            if (!sourceEntity.sourceAggMethod) {
                var index = _.findIndex(reportMetadata.rmeta.groups, function (group) {
                    /*jshint -W116 */
                    return _.first(_.keys(group)) == colId; //string==int
                    /*jshint +W116 */
                });
                if (index === -1) {
                    console.warn('Could not find group column');
                    return null;
                }
                res = {
                    colMeta: colMeta,
                    valRule: valRule,
                    accessor: function (row) {
                        return row.hdrs[index][colId];
                    },
                    cfRule: cfRule
                };
                res.colMeta.colTitle = res.colMeta.title;
                return res;
            }

            // Pivot columns (aggregated)
            var aggAlias = sourceEntity.sourceAggMethod.eidP.getAlias();
            var aggMeta = reportMetadata.rmeta.aggs[colId];
            var aggIndex = _.findIndex(aggMeta, { style: aggAlias });
            if (aggIndex === -1) {
                console.warn('Could not find agg column');
                return null;
            }
            res = {
                colMeta: {
                    title: colMeta.title + (!aggAlias ? '' : (' (' + aggAlias.substring(3) + ')')),
                    colTitle: colMeta.title,
                    type: aggMeta[aggIndex].type
                },
                valRule: valRule,
                accessor: function (row) {
                    var rawObj = row.aggs[colId][aggIndex];
                    return {
                        val: rawObj.value,
                        vals: rawObj.values
                    };
                },
                cfRule: null // not applicable to aggregated columns
            };
            return res;
        }


        // Converts a pivot row to a set of conditions that matches that row
        function convertPivotRowToConds(row, sources, reportMetadata) {
            // Convert group headers format to analyzer format
            // output: [ { expid:<expid>, oper:'Equal', type:'String,etc..', value: 'Value' }, ... ]

            var conds = _.map(sources, function (source) {
                // find the column, so we can get its expression ID
                var colid = sp.result(source, 'entity.chartReportColumn.idP');
                if (!colid)
                    return null;

                // Do not filter the aggregated sources
                var aggMethod = sp.result(source, 'entity.sourceAggMethod.nsAlias');
                if (aggMethod)
                    return null;

                // Get the metadata
                var colMeta = reportMetadata.rcols[colid];
                var outputCond = {
                    expid: colid + '',
                    type: colMeta.type
                };

                // Get the value
                var inputObj = source.rawAccessor(row);

                switch (colMeta.type) {
                    case 'String':
                    case 'Currency':
                    case 'Int32':
                    case 'Decimal':
                    case 'Date':
                    case 'Time':
                    case 'DateTime':
                        // empty string for most types; undefined for date,time,datetime
                        if (!inputObj.val) {
                            outputCond.oper = 'IsNull';
                        } else {
                            outputCond.oper = 'Equal';
                            outputCond.value = inputObj.val;
                        }
                        break;

                    case 'Bool':
                        outputCond.oper = 'Equal';
                        outputCond.value = inputObj.val || 'False';
                        break;

                    case 'ChoiceRelationship':
                    case 'InlineRelationship':
                    case 'UserInlineRelationship':
                        // undefined if empty
                        if (!inputObj.vals) {
                            outputCond.oper = 'IsNull';
                        } else {
                            if (colMeta.aggcol) {
                                outputCond.oper = 'Contains';
                            } else {
                                outputCond.oper = 'AnyOf';
                            }
                            outputCond.values = inputObj.vals;
                        }

                        break;
                    default:
                        return null;
                }
                return outputCond;
            });
            var noNulls = _.compact(conds);
            noNulls.isConditions = true;
            return noNulls;
        }


        // Checks the format type to see if it is one that can transform from linear to categorical
        function isCategoricalScaleFormat(formatType) {
            return ((formatType === 'dateMonthYear') ||
            (formatType === 'dateYear') ||
            (formatType === 'dateDayMonth') ||
            (formatType === 'dateMonth') ||
            (formatType === 'dateQuarter') ||
            (formatType === 'dateQuarterYear') ||
            (formatType === 'dateWeekday') ||
            (formatType === 'dateTimeDayMonth') ||
            (formatType === 'dateTimeDate') ||
            (formatType === 'dateTimeMonth') ||
            (formatType === 'dateTimeMonthYear') ||
            (formatType === 'dateTimeQuarter') ||
            (formatType === 'dateTimeQuarterYear') ||
            (formatType === 'dateTimeYear') ||
            (formatType === 'dateTimeWeekday') ||
            (formatType === 'dateTimeHour') ||
            (formatType === 'timeHour'));
        }


        // Given a source entity (instance of chartSource), creates an 'accessor' function/object.
        // Accepts:
        //    sourceEntity .. a chartSource entity
        //    options {
        //      visData: mandatory - as returned by requestVisData
        //      nameCache: optional
        //    }
        // Returns:
        //    A function, that is also decoration with various properties (some of which are also functions)
        //    Returned function receives (row, index) and returns the data value from that row, for the sourceEntity
        //    Note: index is only required because of the 'row number' source.
        //    Additional properties:
        //    .entity     = the sourceEntity
        //    .formatter  = function(datavalue) -> text
        //    .formatType = date-time format type (this might get moved elsewhere)
        //    .sorter     = function(datavalue) -> sortabledatavalue
        //    .colType    = column type, as returned by reports metadata .. eg 'DateTime', 'Int32', 'ChoiceRelationship', 'UserInlineRelationship', 'InlineRelationship'
        //    .typeId     = type of choice or related data, if applicable  (as returned by reports metadata)
        //    .title      = text, a suitable way to refer to this source
        //    .colTitle   = needs investigation
        //    .rules      = undefined, or array of conditional formatting rules
        //    .cfColorAccessor = function(row) -> { fgcolor:hex, bgcolor:hex, colour:hex }  .. conditional formatting color for the cell
        //    .getText    = function(row, index) -> text
        //    
        function createDataAccessor(sourceEntity, options) {
            var accessor = null;

            var visData = options.visData;
            var nameCache = options.nameCache || {};
            var reportMetadata = visData.reportData.meta;

            var specialSource = sp.result(sourceEntity, 'specialChartSource.nsAlias');

            var colInfo = locateColumnContent(sourceEntity, visData);
            if (!(colInfo || specialSource))
                return null;

            var colMeta = sp.result(colInfo, 'colMeta');
            var formatType = sp.result(colInfo, 'valRule.datetimefmt');
            var formatter = function (d) { return d ? d.toString() : null; };
            var sorter = _.identity;

            if (specialSource === 'core:rowNumberSource') {
                accessor = function (row, index) { return index; };
                colMeta = { type: 'Int32', title: 'Row number' };
            } else if (specialSource === 'core:countSource') {
                accessor = function (row) { return row && row.total || 0; };
                colMeta = { type: 'Int32', title: 'Count' };
            } else if (specialSource === 'core:emptySource') {
                accessor = function (row) { return ''; };
                colMeta = { type: 'String', title: '' };
            } else if (colMeta.type === 'String' && colMeta.entityname && !reportMetadata.isPivot) {
                accessor = function (row) {
                    if (!row)
                        return null;
                    var rawObj = colInfo.accessor(row);
                    nameCache[row.eid] = rawObj.val;
                    return row.eid;
                };
                formatter = function (value) {
                    return nameCache[value] || '[Blank]';
                };
                sorter = formatter;
            } else if (spEntity.DataType.isResource(colMeta.type)) {
                accessor = function (row) {
                    if (!row)
                        return null;
                    var rawObj = colInfo.accessor(row);
                    var raw = rawObj.vals;
                    var key = _.keys(raw)[0];
                    var text = key ? raw[key] : '[Blank]';
                    nameCache[key] = text;
                    return key;
                };
                formatter = function (id) {
                    return nameCache[id];
                };
                sorter = formatter;
            } else {
                var converter;
                if (spEntity.DataType.isNumeric(colMeta.type)) {
                    converter = function (data) { return +data; };
                } else if (colMeta.type === spEntity.DataType.Date || colMeta.type === spEntity.DataType.DateTime) {
                    converter = function (data) {
                        return data ? new Date(data) : null;
                    };
                } else if (colMeta.type === spEntity.DataType.Time) {
                    converter = function (data) {
                        if (!data)
                            return null;
                        var val1 = new Date(data);
                        // The usual 1753 is represented as negative, which doesn't play nice
                        var val2 = new Date(2000, 0, 1, val1.getUTCHours(), val1.getUTCMinutes(), val1.getUTCSeconds(), 0);
                        return val2;
                    };
                } else if (colMeta.type === 'String') {
                    converter = function (data) { return data || '[Blank]'; };
                }
                converter = converter || function (data) { return data; };
                formatter = spReportHelpers.getColumnFormatFunc(colMeta, colInfo.valRule);
                accessor = function (row) {
                    var rawObj = colInfo.accessor(row);
                    var res = rawObj ? converter(rawObj.val) : null;
                    return res;
                };
            }
            accessor.entity = sourceEntity;
            accessor.formatter = formatter;
            accessor.formatType = formatType;
            accessor.sorter = sorter;
            accessor.colType = colMeta.type;
            accessor.typeId = colMeta.tid;  // type of choice or related data, if applicable
            accessor.title = colMeta.title;
            accessor.rawAccessor = colInfo ? colInfo.accessor : null;
            accessor.getText = _.flowRight(formatter, accessor);

            // source title
            if (specialSource)
                accessor.colTitle = specialSources[specialSource] ? specialSources[specialSource].name : '';
            if (colInfo)
                accessor.colTitle = colInfo.colMeta.colTitle;

            // cf rules
            var ruleArr = sp.result(colInfo, 'cfRule.rules');
            if (ruleArr) {
                accessor.rules = ruleArr;
                var cfRuleAccessor = function (row) {
                    if (!row)
                        return null;
                    var rawObj = colInfo.accessor(row);
                    var cfIndex = rawObj.cfidx;
                    if (!cfIndex && cfIndex !== 0)
                        return null;

                    var rule = ruleArr[cfIndex];
                    return rule;
                };
                accessor.cfColorAccessor = function (row) {
                    if (!row)
                        return null;
                    var rule = cfRuleAccessor(row);
                    var col = getRuleColors(rule);
                    return col; // returns { fgHex:'#hexhex', bgHex:'#hexhex', colorHex:'#hexhex' }
                };
            }
            return accessor;
        }


        // Converts a cf rule color to a d3 happy hex colour
        function convertCol(argb) {
            return argb ? d3.rgb(argb.r, argb.g, argb.b).toString() : null;
        }


        // Converts a cf rule to a colour
        // Accepts: cf rule from report metadata
        // Returns: { fgHex:'#hexhex', bgHex:'#hexhex', colorHex:'#hexhex' }
        // Where 'color' is the background colour, unless its while, in which case it's the foreground color
        function getRuleColors(rule) {
            var bgArgb = sp.result(rule, 'bgcolor');
            var bgHex = convertCol(bgArgb);
            var fgArgb = sp.result(rule, 'fgcolor');
            var fgHex = convertCol(fgArgb);
            var colorHex = (bgHex && bgHex !== '#ffffff') ? bgHex : fgHex;
            return {
                fgHex: fgHex,
                bgHex: bgHex,
                colorHex: colorHex
            };
        }


        // expose private methods for unit tests
        var exports = {
            sourceRequest: sourceRequest,
            specialSources: specialSources,
            requestVisData: requestVisData,
            isPivot: isPivot,
            createDataAccessor: createDataAccessor,
            convertPivotRowToConds: convertPivotRowToConds,
            isCategoricalScaleFormat: isCategoricalScaleFormat,
            getRuleColors: getRuleColors,
            test: {
                locateColumnContent: locateColumnContent,
                createReportRequest: createReportRequest,
                createRollupRequest: createRollupRequest
            }
        };


        return exports;
    }

})();
