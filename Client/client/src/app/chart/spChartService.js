// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module for working with charts.
    * 
    * @module spChartService
    *
    */

    var sourceTypes = ['primarySource', 'valueSource', 'endValueSource', 'sizeSource', 'colorSource', 'imageSource', 'textSource', 'symbolSource', 'associateSource'];
    var chartPropertiesQueryString = 'name, description, chartTitle, chartReport.name, chartTitleAlign.alias,inSolution.name,console:navigationElementIcon.{alias, name, imageBackgroundColor},hideOnDesktop,hideOnTablet,hideOnMobile,isPrivatelyOwned';

    angular.module('mod.common.ui.spChartService', ['mod.common.spEntityService', 'mod.common.spVisDataService', 'sp.app.settings'])
        .value('sourceTypes', sourceTypes)
        .value('chartProperties', chartPropertiesQueryString)
        .value('chartQuery', '' +
            chartPropertiesQueryString + ',' +
            'chartHasSeries.{ ' +
            '   name, seriesOrder, { chartType, stackMethod, markerShape, markerSize, dataLabelPos }.alias, ' +
            '   chartCustomColor, chartNegativeColor, hideColorLegend, useConditionalFormattingColor, ' +
            '   { primaryAxis, valueAxis }.{ name, showGridLines, showAllChoiceValues, axisScaleType.alias, axisMinimumValue, axisMaximumValue }, ' +
            '   {' + sourceTypes.join() + '}.{ name, chartReportColumn.id, specialChartSource.alias, sourceAggMethod.alias } ' +
            '}'
        )
        .service('spChartService', function ($q, spEntityService, chartQuery, sourceTypes, chartProperties, spVisDataService, spAppSettings) {
            var exports = this;
            var svc = this;


            /////  STATIC DATA  /////
            (function () {

                // Define some chart types and source types
                exports.chartTypes = [
                    { alias: 'lineChart', name: 'Line', reqPrimary: true, reqValue: true, labelAbove: true, disallowNegColor: true, hasAxes:true },
                    { alias: 'areaChart', name: 'Area', reqPrimary: true, reqValue: true, labelAbove: true, disallowNegColor: true, hasAxes: true },
                    { alias: 'columnChart', name: 'Column', reqValue: true, labelAbove: true, hasAxes: true },
                    { alias: 'barChart', name: 'Bar', reqValue: true, labelRight: true, hasAxes: true },
                    { alias: 'pieChart', name: 'Pie', hasImage: true, reqValue: true },
                    { alias: 'donutChart', name: 'Donut', hasImage: true, reqValue: true },
                    { alias: 'matrixChart', name: 'Matrix', hasImage: true, reqValue: true, hasAxes: true },
                    { alias: 'scatterChart', name: 'Scatter', reqValue: true, labelAbove: true, hasAxes: true },
                    { alias: 'bubbleChart', name: 'Bubble', hasImage: true, reqValue: true, hasAxes: true },
                    { alias: 'funnelChart', name: 'Funnel', hasImage: true, reqValue: true },
                    { alias: 'gaugeChart', name: 'Gauge', reqValue: true },
                    { alias: 'treeMapChart', name: 'Tree Map', isTree: true, hasImage: true, reqPrimary: true, reqValue: true },
                    { alias: 'forceGraph', name: 'Force Graph', isTree: true, reqPrimary: true, reqValue: false },
                    { alias: 'horzHierarchy', name: 'Horizontal Tree', isTree: true, reqPrimary: true, reqValue: false },
                    { alias: 'radialTreeGraph', name: 'Radial Tree', isTree: true, reqPrimary: true, reqValue: false },
                    { alias: 'sunburstChart', name: 'Sunburst', isTree: true, hasImage: true, reqPrimary: true, reqValue: false }
                ];

                //
                // getChartSourceTypes
                //
                exports.getChartSourceTypes = _.once(function () {
                    // Some helpers for implementing 'display'
                    var anyOf = function (list) { return _.partial(_.includes, list); };
                    var anyExcept = function (list) { return _.flowRight(function (res) { return !res; }, anyOf(list)); };
                    var hasProp = function (propName) {
                        return function (alias) {
                            var ct = _.find(svc.chartTypes, { alias: alias });
                            return sp.result(ct, propName);
                        };
                    };

                    var setAxisName = function (axisAlias, data) {
                        var series = data.dropData.series;
                        var axis = series.getLookup(axisAlias);
                        axis.name = data.dragData.name;
                    };

                    var chartSourceTypes = [
                        // hasProps = has property page
                        // display = callback to determine if applicable
                        { alias: 'primarySource', name: 'Primary', hasProps: true, display: anyExcept(['gaugeChart']), dropCallback: _.partial(setAxisName, 'primaryAxis') },
                        { alias: 'valueSource', name: 'Values', hasProps: true, display: anyExcept(['forceGraph', 'horzHierarchy', 'radialTreeGraph']), dropCallback: _.partial(setAxisName, 'valueAxis') },
                        { alias: 'endValueSource', name: 'End value', display: anyOf(['barChart', 'columnChart', 'areaChart']) },
                        { alias: 'associateSource', name: 'Associate', display: hasProp('isTree') },
                        { alias: 'sizeSource', name: 'Size', display: anyOf(['bubbleChart']) },
                        { alias: 'colorSource', name: 'Colour', hasProps: true, display: anyExcept(['gaugeChart']) },
                        { alias: 'textSource', name: 'Text', display: anyExcept(['gaugeChart']) },
                        { alias: 'symbolSource', name: 'Symbol', hasProps: true, display: anyOf(['scatterChart', 'lineChart']) },
                        { alias: 'imageSource', name: 'Image', display: hasProp('hasImage') }
                    ];

                    // .. and decorate them with some extra goodies
                    _.forEach(chartSourceTypes, function (cst) {
                        if (!cst.display) {
                            cst.display = function (ct) { return true; };
                        }
                        cst.displayCt = function (chartType) {
                            var alias = sp.result(chartType, 'eidP.getAlias');
                            return cst.display(alias);
                        };
                    });

                    return chartSourceTypes;
                });


                //
                // getChartSourceType
                //
                exports.getChartSourceType = function (alias) {
                    var chartSourceTypes = svc.getChartSourceTypes();
                    var chartSourceType = _.find(chartSourceTypes, { alias: alias });
                    return chartSourceType;
                };

                //
                // getChartType
                //
                exports.getChartType = function (alias) {
                    var chartType = _.find(svc.chartTypes, { alias: (alias||'').replace('core:','') });
                    return chartType;
                };

            })();

            //
            // supportedScalesForDataType
            //
            exports.supportedScalesForDataType = function(dataType) {
                var isNumeric = spEntity.DataType.isNumeric(dataType);
                var isDateType = spEntity.DataType.isDateType(dataType);
                var res = {
                    categoryScaleType: true,
                    dateTimeScaleType: isDateType,
                    linearScaleType: isDateType || isNumeric,
                    logScaleType: isNumeric
                };
                res.defaultScaleType = isDateType ? 'dateTimeScaleType' : ( res.linearScaleType ? 'linearScaleType' : 'categoryScaleType' );
                return res;
            };

            //
            // supportedScalesForChartType
            //
            exports.supportedScalesForChartType = function (chartTypeAlias) {
                var hasAxes = !!exports.getChartType(chartTypeAlias).hasAxes;
                var isMatrix = chartTypeAlias === 'core:matrixChart' || chartTypeAlias === 'matrixChart';
                var res = {
                    categoryScaleType: hasAxes,
                    dateTimeScaleType: hasAxes,
                    linearScaleType: !isMatrix,
                    logScaleType: !isMatrix
                };
                res.defaultScaleType = res.linearScaleType ? 'linearScaleType' : 'categoryScaleType';
                return res;
            };

            //
            // supportedScales
            //
            exports.supportedScales = function (dataType, chartTypeAlias) {
                var dt = exports.supportedScalesForDataType(dataType);
                var ct = exports.supportedScalesForChartType(chartTypeAlias);
                var def = ct[dt.defaultScaleType] ? dt.defaultScaleType : ct.defaultScaleType; // prefer default for datatype, so long as its permitted
                var res = {
                    categoryScaleType: dt.categoryScaleType && ct.categoryScaleType,
                    dateTimeScaleType: dt.dateTimeScaleType && ct.dateTimeScaleType,
                    linearScaleType: dt.linearScaleType && ct.linearScaleType,
                    logScaleType: dt.logScaleType && ct.logScaleType,
                    defaultScaleType: def
                };
                return res;
            };

            // Is the source a required source
            exports.isSourceRequired = function isSourceRequired(chartTypeAlias, sourceAlias) {
                var info = _.find(exports.chartTypes, { alias: chartTypeAlias });
                var res;
                if (sourceAlias === 'primarySource') {
                    res = !!info.reqPrimary;
                } else if (sourceAlias === 'valueSource') {
                    res = !!info.reqValue;
                } else {
                    res = false;
                }
                return res;
            };

            // Load a chart
            // Returns a promise for the chart entity
            exports.getChart = function getChart(chartId, options) {
                var opts = _.defaults(options || {}, { hint: 'chart' });
                return spEntityService.getEntity(chartId, chartQuery, opts);
            };

            //Load chart properties
            exports.getChartProperties = function getChartProperties(chartId) {
               return spEntityService.getEntity(chartId, chartProperties, {batch:true, hint: 'chart properties' });
            };


            // Examine a chart entity and add any missing bits
            exports.newChart = function newChart() {
                var chart = exports.repairChartEntity(null);
                return chart;
            };

            // Examine a chart entity and add any missing bits
            exports.repairChartEntity = function repairChartEntity(chart) {
                // Check root chart object
                if (!chart)
                    chart = spEntity.fromJSON({
                        typeId: 'core:chart',
                        name: 'New Chart',
                        chartHasSeries: [],
                        chartReport: jsonLookup(),
                        inSolution: jsonLookup(),
                        hideOnDesktop: false,
                        hideOnTablet: true,
                        hideOnMobile: true,
                        isPrivatelyOwned: !spAppSettings.publicByDefault,
                        'console:navigationElementIcon': jsonLookup()

                    });

                // Ensure at least one series
                if (!chart.getChartHasSeries().length) {
                    var series = exports.newSeries();
                    chart.setChartHasSeries(series);
                }

                // Check series
                _.forEach(chart.getChartHasSeries(), function (series) {
                    if (!series.getChartType())
                        series.setChartType('columnChart');
                    if (!series.getPrimaryAxis())
                        series.setPrimaryAxis(exports.newAxis(true));
                    if (!series.getValueAxis())
                        series.setValueAxis(exports.newAxis(false));
                });

                return chart;
            };



            // Create a new series entity, with relevant members registered
            exports.newSeries = function newSeries(seriesToCopy) {
                var lookups = _.union(sourceTypes, ['primaryAxis', 'valueAxis']);
                var chartTypeAlias = sp.result(seriesToCopy, 'chartType.nsAlias');
                var json = {
                    typeId: 'chartSeries',
                    name: 'Chart Series',
                    chartCustomColor: jsonString(),  // null=use default
                    chartNegativeColor: jsonString(), // null=use default
                    chartType: jsonLookup(chartTypeAlias),
                    seriesOrder: 0
                };
                _.forEach(lookups, function (alias) { json[alias] = jsonLookup(null); });
                var series = spEntity.fromJSON(json);

                if (seriesToCopy) {
                    series.setPrimaryAxis(seriesToCopy.getPrimaryAxis());
                    series.setValueAxis(seriesToCopy.getValueAxis());
                } else {
                    series.setPrimaryAxis(exports.newAxis(true));
                    series.setValueAxis(exports.newAxis(false));
                }

                return series;
            };


            // Create a new axis
            exports.newAxis = function newAxis(isPrimary) {
                return spEntity.fromJSON({
                    typeId: 'chartAxis',
                    axisScaleType: jsonLookup('automaticScaleType'),
                    axisMinimumValue: jsonString(null),
                    axisMaximumValue: jsonString(null)
                });
            };


            // Load any data required for the chart
            exports.requestChartData = function requestChartData(chart, options) {
                // Create request model
                var sourceModel = {
                    reportId: chart.chartReport.idP,
                    sources: exports.getSources(chart),
                    sourcesRequestingAllData: svc.getSourcesThatShowAllData(chart)
                };

                // returns a promise to return visData (chartData)
                return spVisDataService.requestVisData(sourceModel, options);
            };


            // Find sources that need to load all instances
            exports.getSourcesThatShowAllData = function getSourcesThatShowAllData(chart) {
                var res = [];
                _.forEach(chart.chartHasSeries, function (ser) {
                    if (ser.primarySource && sp.result(ser, 'primaryAxis.showAllChoiceValues'))
                        res.push(ser.primarySource);
                    if (ser.valueSource && sp.result(ser, 'valueAxis.showAllChoiceValues'))
                        res.push(ser.valueSource);
                });
                return res;
            };


            // Find all sources in the chart
            exports.getSources = function getSources(chart) {
                if (!chart)
                    return [];

                var sources = [];

                // Visit each series
                _.forEach(chart.chartHasSeries, function (series) {
                    var chartType = sp.result(series, 'chartType.eidP.getAlias');
                    if (!chartType)
                        return;
                    var sourceTypes = svc.getChartSourceTypes();

                    // Visit each source type
                    _.forEach(sourceTypes, function (sourceType) {
                        if (!sourceType.display(chartType))  // don't include non-applicable sources.
                            return;
                        var source = series.getLookup(sourceType.alias);
                        if (source && (source.chartReportColumn || source.specialChartSource)) {
                            sources.push(source);
                        }
                    });
                });
                return sources;
            };

            // Create a new series entity, with relevant members registered
            exports.getSeriesOrdered = function getSeriesOrdered(chart) {
                if (!chart || !chart.chartHasSeries)
                    return null;
                // It seems that the sortBy in the new lodash version now puts null values
                // after others whereas before it sorted them first. This is breaking some
                // assumptions, so retain the previous behaviour and move those with null to the front.
                return _.sortBy(chart.chartHasSeries, function (s) {return _.isNull(s.seriesOrder) ? -1 : s.seriesOrder;});
            };


            // Converts a cf rule to a legend entry
            exports.ruleToLegendEntry = function ruleToLegendEntry(rule) {
                var labels = {
                    Equal: 'VAL',
                    NotEqual: 'Not VAL',
                    AnyOf: 'VAL',
                    AnyExcept: 'Not VAL',
                    StartsWith: 'VAL...',
                    EndsWith: '...VAL',
                    Contains: '...VAL...',
                    IsTrue: 'Yes',
                    IsFalse: 'No',
                    GreaterThan: '> VAL',
                    GreaterThanOrEqual: '>= VAL',
                    LessThan: '< VAL',
                    LessThanOrEqual: '<= VAL',
                    Today: 'Today',
                    ThisMonth: 'This month',
                    ThisQuarter: 'This quarter',
                    ThisYear: 'This year',
                    Unspecified: 'Other'
                };
                var val = rule.val;
                if (rule.vals)
                    val = _.values(rule.vals).join(', ');

                var text = labels[rule.oper];
                if (text)
                    text = text.replace('VAL', val || '');
                else
                    text = 'Rule ' + val;

                var res = {
                    color: spVisDataService.getRuleColors(rule).colorHex,
                    text: text
                };
                return res;
            };
        });

}());