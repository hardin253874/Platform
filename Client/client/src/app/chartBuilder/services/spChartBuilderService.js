// Copyright 2011-2016 Global Software Innovation Pty Ltd

angular.module('mod.app.chartBuilder.services.spChartBuilderService', [
    'mod.common.spEntityService',
    'spApps.reportServices',
    'mod.common.spVisDataService',
    'mod.common.ui.spChartService',
    'sp.app.navigation',
    'mod.common.spVisDataBuilderService'
])
   /**
    * Module implementing the chart builder service.
    * spChartBuilderService provides methods to interact with the chart builder between directives.
    *
    * @module spChartBuilderService
    */
    .service('spChartBuilderService', function ($q, navDirtyMessage, spVisDataService, spChartService, spEntityService, spReportService, spVisDataBuilderService) {
        'use strict';

        var exports = {};
        var svc = exports;
        var checkModel, assertModel, clearHistory, isDimension, isMeasure;


        /////  MODEL MANAGEMENT  /////
        (function () {

            //
            // tempModel
            //
            exports.tempModel = function () {
                var model = {};
                model.windowBusy = {
                    type: 'spinner',
                    text: 'Loading...',
                    placement: 'element',
                    isBusy: true
                };
                return model;
            };

            //
            // initializeModel
            //
            function initializeModel(model) {
                model.chartBookmark = model.chart.graph.history.addBookmark('Unmodified chart');
                model.refresh = 0;
                model.windowBusy = {
                    type: 'spinner',
                    text: 'Saving...',
                    placement: 'element',
                    isBusy: false
                };
                return model;
            }


            //
            // checkModel
            //
            checkModel = function(model) {
                return model && model.chart && model.chartBookmark;
            };


            //
            // assertModel
            //
            assertModel = function(model) {
                if (!checkModel(model)) {
                    console.error('Chart model is invalid');
                    throw new Error('Chart model is invalid');
                }
            };


            //
            // createChartModel
            //
            exports.createChartModel = function (options) {
                var model = {
                    chart: spChartService.newChart()
                };
                if (options && options.reportId) {
                    model.chart.chartReport = options.reportId;
                }
                initializeModel(model);
                return model;
            };


            //
            // loadChartModel
            //
            exports.loadChartModel = function (id) {
                return $q.all({
                    chart: spChartService.getChart(id, { batch: false })
                }).then(initializeModel);
            };


            //
            // reloadChart - reloads the chart within the model
            //
            exports.reloadChart = function (model) {
                // Check
                if (model.chart.dataState === spEntity.DataStateEnum.Create) {
                    console.error('Chart cannot be reloaded - it has not been saved yet.');
                    return $q.when();
                }

                return spChartService.getChart(model.chart.idP, { batch: false })
                    .then(function (chart) {
                        model.chart = chart;
                        return model;
                    }).then(initializeModel);
            };


            //
            // saveChart
            //
            exports.saveChart = function (model) {
                assertModel(model);

                // Ensure we're not accidentally changing the report, otherwise we will get permissions errors
                // There is currently no good reason to be doing this when saving a chart ... if this ever changes, ensure self-serve users can create charts without modify on the underlying report.
                spEntity.ensureUnchanged(model.chart, 'chartReport');
                
                return spEntityService.putEntity(model.chart).then(function (chartId) {
                    var reportId = sp.result(model, 'chart.chartReport.idP');
                    if (reportId && reportId > 0) {
                        spReportService.clearReportDataCache(reportId);
                    }
                    return chartId;
                });
            };
        })();


        /////  HISTORY MANAGEMENT  /////
        (function () {

            // Wraps a task in a bookmark
            // The task may return false (explicitly) to cancel the changes
            // The task may return a promise
            exports.applyChange = function (model, callback) {
                var bm = model.chart.graph.history.addBookmark();
                var res = callback();
                var handleResult = function(result) {
                    if (result === false) {
                        bm.undo();
                    } else {
                        bm.endBookmark();
                        svc.refreshChart(model);
                    }
                };
                if (res && res.then) {
                    // handle promises                    
                    return $q.when(res).then(handleResult);
                } else {
                    handleResult(res);
                    return null;
                }
            };


            //
            // refreshChart
            // Communicate that a refresh is required
            //
            exports.refreshChart = function (model) {
                if (model) {
                    model.refresh = (model.refresh || 0) + 1;
                }
            };


            //
            // clearHistory
            //
            clearHistory = function(model) {
                model.chart.graph.history.clear();
                model.chartBookmark = model.chart.graph.history.addBookmark('Unmodified chart');
            };


            //
            // undo
            //
            exports.undo = function (model) {
                if (!checkModel(model))
                    return;

                model.chart.graph.history.undoBookmark();
                svc.refreshChart(model);
            };


            //
            // redo
            //
            exports.redo = function (model) {
                if (!checkModel(model))
                    return;

                model.chart.graph.history.redoBookmark();
                svc.refreshChart(model);
            };


            //
            // isDirty
            //
            exports.isDirty = function (model) {
                if (!checkModel(model)) return false;

                var res = model.chart.graph.history.changedSinceBookmark(model.chartBookmark);
                return res;
            };


            //
            // dirtyMessage
            //
            exports.dirtyMessage = function (model) {
                if (svc.isDirty(model)) {
                    return navDirtyMessage.defaultMsg;
                }
                return undefined;
            };

        })();


        /////  REPORT METADATA  /////
        (function () {

            //
            // loadReportMetadata
            //
            exports.loadReportMetadata = function (model) {
                model.reportId = sp.result(model, 'chart.chartReport.idP');

                return spVisDataBuilderService.loadReportMetadata(model)
                    .then(function () {
                        if (!model.reportMetadata) return; // no reportId
                        // Use report name as authorative
                        model.chart.chartReport.name = model.reportMetadata.title;
                        if (model.chart.dataState === spEntity.DataStateEnum.Create) {
                            if (!model.chart.name || model.chart.name === 'New Chart') {
                                model.chart.name = model.reportMetadata.title + ' Chart';
                            }
                        }
                        clearHistory(model);
                    },
                    function (e) {
                        console.error('Failed to load report', e);
                    });
            };

            //
            // applySuggestions
            // chartType - optional alias
            //
            exports.applySuggestions = function (model, chartType) {
                if (!checkModel(model))
                    return;

                var primary = null;
                var value = null;
                var color = null;
                var series;

                // Select sources
                try {
                    series = _.first(model.chart.chartHasSeries);
                    var sources = svc.getAvailableColumnSources(model);

                    _.forEach(sources, function(source) {
                        if (source.type === 'Image' || source.specialChartSource)
                            return;
                        if (!primary || (primary.type === 'String' || !isDimension(primary.type)) && isDimension(source.type))
                            primary = source;
                        if (!value || value === primary || !isMeasure(value.type) && isMeasure(source.type))
                            value = source;
                        //if (isDimension(source.type) && (!color || color === primary || color === value) || (sp.result(color, 'type') !== 'ChoiceRelationship' && source.type === 'ChoiceRelationship'))
                        //    color = source;
                    });
                    color = primary;

                    var apply = function (source, relAlias) {
                        try {
                            var dragData = source.getDrag();
                            var chartSource = svc.dragDataToSource(dragData);
                            series.setLookup(relAlias, chartSource);
                        } catch (e) {
                            console.error(e);
                        }
                    };

                    // Do not apply color to these chart types, as there's a high probability it will fragment the data and show no results
                    if (chartType === 'areaChart' || chartType === 'lineChart') {
                        color = null;
                    }
                    if (chartType === 'gaugeChart') {
                        primary = null;
                        color = null;
                    }

                    if (primary) apply(primary, 'primarySource');
                    if (color) apply(color, 'colorSource');
                    if (value && isMeasure(value.type)) {
                        value.sourceAggMethod = 'aggSum';
                    } else {
                        value = spVisDataBuilderService.makeSpecialSourceInfo('core:countSource');
                    }
                    apply(value, 'valueSource');

                } catch (e) {
                    console.error(e);
                }


                // Select chart type
                try {

                    if (!chartType) {
                        chartType = 'columnChart';
                        if (primary && value) {
                            //if (isDimension(primary.type) && isDimension(value.type))
                            //    chartType = 'scatterChart';
                            if (isMeasure(primary.type) && isMeasure(value.type))
                                chartType = 'scatterChart';
                        }
                    }
                    series.chartType = chartType;
                } catch (e) {
                    console.error(e);
                }
            };

        })();


        /////  UNSORTED CRUFT  /////
        (function() {

            exports.getAggregateMethods = spVisDataBuilderService.getAggregateMethods;


            exports.getSeriesName = function (model, series, getDefaultName) {
                // Keep in sync with spCharts.getSeriesName
                if (!model || !series)
                    return '';
                if (!getDefaultName && series.name && series.name !== '' && series.name !== 'Chart Series')
                    return series.name;

                var res = '';
                try {
                    var value = svc.getSourceInfoForSource(model, series.valueSource).name;
                    if (value)
                        res += ' ' + value;
                }
                catch (e) { }
                try {
                    var ct = _.find(spChartService.chartTypes, function (ct1) {
                        return ct1.alias === series.chartType.eidP.getAlias();
                    });
                    if (ct)
                        res += ' ' + ct.name;
                } catch (e) { }
                if (res) {
                    res = res.slice(1);
                }
                return res || 'Chart Series';
            };

            exports.setSeriesName = function (model, series, name) {
                if (!model || !series)
                    return;
                if (name === svc.getSeriesName(model, series, true))
                    series.name = null; // revert to using default
                else
                    series.name = name;
            };

            isDimension = exports.isDimension = function isDimension(type) {
                var dimensions = ['String', 'ChoiceRelationship', 'InlineRelationship', 'UserInlineRelationship', 'Bool', 'Entity'];
                return _.includes(dimensions, type);
            };

            isMeasure = exports.isMeasure = function isMeasure(type) {
                var measures = ['Int32', 'Decimal', 'Currency'];
                return _.includes(measures, type);
            };


exports.getAvailableColumnSources = spVisDataBuilderService.getAvailableColumnSources;


            //
            // Find the current source that applies to a given chart source type on a given series
            //
            exports.getSource = function (model, series, cst) {
                if (!model || !cst || !series)
                    return null;
                var chartSource = series.getLookup(cst.alias);
                return chartSource;
            };

            //
            // Return info for the current source data that applies to a given chart source type on a given series
            // -> { name, colId, specialChartSource, type, getDrag() }
            //
            exports.getSourceInfo = function (model, series, cst) {
                if (!model || !model.reportMetadata)
                    return null;

                var chartSource = svc.getSource(model, series, cst);
                if (!chartSource)
                    return null;

                // Handle 'row number' source
                if (chartSource.specialChartSource)
                    return spVisDataBuilderService.makeSpecialSourceInfo(chartSource.specialChartSource.nsAlias);

                // Find metadata
                var res = svc.getSourceInfoForSource(model, chartSource);
                return res;
            };

            //
            // getSourceInfoForSource
            //
            exports.getSourceInfoForSource = spVisDataBuilderService.getSourceInfoForSource;

        })();


        /////  DRAG DROP  /////
        (function () {

            //
            // Executes a drag-drop
            //
            exports.doDragDrop = function (dragData, dropData) {
                var model = dropData.model || dropData.modelCallback();
                svc.applyChange(model, function () {
                    var dragData1 = dragData();
                    dropData.onDrop(model, dragData1, dropData);
                });
            };

            //
            // A general drop area to allow removals
            //
            exports.getDropOptions = function (applyCallback) {
                var options =
                {
                    onAllowDrop: function (source, target, dragData, dropData) {
                        var realDragData = dragData();
                        if (dropData && dropData.onAllowDrop) {
                            return dropData.onAllowDrop(realDragData, dropData);
                        }
                        return true;
                    },
                    onDrop: function (event, source, target, dragData, dropData) {
                        svc.doDragDrop(dragData, dropData);
                        if (applyCallback)
                            applyCallback();
                    }
                };
                return options;
            };


            // Create a drag-data factory for a chart source type
            exports.getCstDragData = function (model, series, cst) {
                return function() {
                    var source = svc.getSource(model, series, cst);
                    if (!source)
                        return null;

                    var dragData = svc.sourceToDragData(source);
                    if (!dragData)
                        return null;

                    dragData.canDelete = true;
                    dragData.delete = function () {
                        var existing = svc.getSource(model, series, cst);
                        if (existing) {
                            series.setLookup(cst.alias, null);
                        }
                    };

                    return dragData;
                };
            };


            // Create a drop-data factory for a chart source type
            exports.getCstDropData = function (model, series, cst) {
                return {
                    cst: cst,
                    series: series,
                    model: model,
                    onDrop: function (model, dragData, dropData) {

                        var chartSource = svc.dragDataToSource(dragData);
                        var chartSourceType = dropData.cst;
                        var series = dropData.series;
                        series.setLookup(chartSourceType.alias, chartSource);

                        if (chartSourceType.dropCallback) {
                            chartSourceType.dropCallback({ dragData: dragData, dropData: dropData });
                        }
                    }
                };
            };


            //
            // Create new drag information from a chartSource entity
            //
            exports.sourceToDragData = function (chartSource) {
                var res = {
                    colId: sp.result(chartSource, 'chartReportColumn.idP'),
                    specialChartSource: sp.result(chartSource, 'specialChartSource.nsAlias'),
                    sourceAggMethod: sp.result(chartSource, 'sourceAggMethod.nsAlias')
                };
                return res;
            };


            //
            // Create a new chartSource entity from drag information
            //
            exports.dragDataToSource = function (dragData) {
                return spVisDataBuilderService.createChartSource(dragData);
            };

            exports.getBackgroundDropData = function (modelCallback) {
                return {
                    modelCallback: modelCallback,
                    onAllowDrop: function (dragData, dropData) {
                        return dragData.canDelete || false;
                    },
                    onDrop: function (model, dragData, dropData) {
                        if (dragData && dragData.delete)
                            dragData.delete();
                    }
                };
            };


            // Create a drag-data factory for a series
            exports.getSeriesDragData = function (model, series) {
                return function () {
                    return { series: series, isSeries: true, canDelete: false };
                };
            };


            // Create a drop-data factory for a series
            exports.getSeriesDropData = function (model, series) {
                return {
                    series: series,
                    model: model,
                    onAllowDrop: function (dragData, dropData) {
                        return dragData.isSeries && dragData.series != dropData.series;
                    },
                    onDrop: function (model, dragData, dropData) {
                        if (!dragData.isSeries)
                            return;
                        var ordered = spChartService.getSeriesOrdered(model.chart);
                        // find
                        var iDrag = _.indexOf(ordered, dragData.series);
                        var iDrop = _.indexOf(ordered, dropData.series);
                        // relocate
                        if (iDrag > -1 && iDrop > -1) {
                            ordered.splice(iDrag, 1);   // remove
                            ordered.splice(iDrop, 0, dragData.series);   // insert
                        }
                        // renumber
                        _.forEach(ordered, function (s, idx) { s.seriesOrder = idx; });
                    }
                };
            };


        })();

        return exports;
    });