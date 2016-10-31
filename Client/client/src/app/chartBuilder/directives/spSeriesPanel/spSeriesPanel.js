// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing the series toolbox for the chart builder.
    * This allows each chart series to be configured, and various inputs (such as data & text) sources to be dropped.
    *
    * @module seriesPanel
    */
    angular.module('mod.app.chartBuilder.directives.spSeriesPanel', [
        'mod.app.chartBuilder.services.spChartBuilderService'
    ])
        .directive('spSeriesPanel', function () {

            return {
                restrict: 'E',
                templateUrl: 'chartBuilder/directives/spSeriesPanel/spSeriesPanel.tpl.html',
                replace: true,
                transclude: false,
                controller: 'spChartBuilderSeriesPanelController',
                scope: {
                    model: '='
                }
            };
        })
        .controller('spChartBuilderSeriesPanelController', function ($q, $scope, $attrs, spAxisProperties, spSeriesColorProperties, spChartService, spChartBuilderService, spSymbolProperties) {

            $scope.spChartService = spChartService;
            $scope.svc = spChartBuilderService;
            $scope.sp = sp; //spUtils

            $scope.isOpen = [];

            $scope.onChartTypeClick = function (event) {
                // Prevent clicking on the properties and delete icons from expanding the accordion
                event.stopPropagation();
                event.preventDefault();
            };

            $scope.isChartTypeOpen = false;

            function expandSeries(series) {
                $scope.isOpen = $scope.isOpen || [];
                $scope.isOpen[series.idP] = true;
            }

            // Watch backgroundClick
            $scope.$on('backgroundClick', function () {
                // close chart-type dialogs
                $scope.isOpen = [];
            });

            // Watch chart
            $scope.$watch('model.chart', function () {
                if (!chart()) return;
                expandSeries($scope.firstSeries());
            });



            // Create a new series
            $scope.addSeries = function () {
                applyChange(function() {
                    var copy = $scope.firstSeries();
                    var newSeries = spChartService.newSeries(copy);
                    newSeries.seriesOrder = chart().chartHasSeries.length;
                    chart().chartHasSeries.add(newSeries);
                    expandSeries(newSeries);
                });
            };

            // Delete a series
            $scope.removeSeries = function (series) {
                applyChange(function() {
                    chart().chartHasSeries.remove(series);
                });
            };


            function chart() {
                var chartEntity = sp.result($scope, 'model.chart');
                if (!chartEntity)
                    console.warn('chart is not set');
                return chartEntity;
            }

            $scope.firstSeries = function firstSeries() {
                return spChartService.getSeriesOrdered(chart())[0];
            };


            // Wraps a task in a bookmark
            // The task may return false (explicitly) to cancel the changes
            // The task may return a promise
            function applyChange(callback) {
                spChartBuilderService.applyChange($scope.model, callback);
            }


            /////////// Drag Drop ///////////

            $scope.dropOptions = spChartBuilderService.getDropOptions(function() { $scope.$apply(); });
            $scope.dragOptions = {};

            /////////// Axis sharing ///////////

            $scope.getSharedAxis = function (series, axisAlias) {
                if (!series || !chart())
                    return false;
                var first = $scope.firstSeries();
                var myaxis = series.getLookup(axisAlias);
                var count = _.filter(chart().chartHasSeries, function (s) { return s.getLookup(axisAlias) === myaxis; }).length;
                var res = count>1;
                return res;
            };

            $scope.setSharedAxis = function (value, series, axisAlias) {
                var oldValue = $scope.getSharedAxis(series, axisAlias);
                if (oldValue === value)
                    return;
                applyChange(function () {
                    var axis;
                    if (value) {
                        var first = $scope.firstSeries();
                        axis = first.getLookup(axisAlias);
                    } else {
                        axis = spChartService.newAxis(axisAlias === 'primaryAxis');
                    }
                    series.setLookup(axisAlias, axis);
                });
            };

            /////////// Series names ///////////

            $scope.getSeriesName = function (series) {
                return spChartBuilderService.getSeriesName($scope.model, series);
            };

            $scope.setSeriesName = function (value, series) {
                spChartBuilderService.setSeriesName($scope.model, series, value);
            };


            /////////// Manage Chart Type ///////////

            // Get the chartSource object for a source type
            $scope.getChartType = function(series) {
                return sp.result(series, 'chartType.eidP.getAlias');
            };

            $scope.setChartType = function (value, series) {
                applyChange(function () {
                    series.chartType = value;
                });
            };


            /////////// Source options ///////////

            // getSourceContextMenu
            $scope.getSourceContextMenu = function (series, cst) {
                var source = spChartBuilderService.getSource($scope.model, series, cst);
                if (!source)
                    return null;

                var menu = $scope.menuCache[source.idP];
                if (!menu) {
                    menu = {
                        menuItemsCallback: function () {
                            var info = spChartBuilderService.getSourceInfoForSource($scope.model, source);
                            if (!info)
                                return [];
                            var methods = spChartBuilderService.getAggregateMethods(info.type);
                            var curMethod = sp.result(source, 'sourceAggMethod.alias') || null;
                            var aggItems = _.map(methods, function (method) {
                                var aliasStr = method.alias ? '\'' + method.alias + '\'' : null;
                                return {
                                    text: method.name,
                                    type: 'click',
                                    click: 'setAggMethod(' + source.idP + ', ' + aliasStr + ')',
                                    icon: (curMethod === method.alias || curMethod === 'core:' + method.alias) ? 'assets/images/menutick.png' : null
                                };
                            });
                            return aggItems;
                        },
                        chartSource: source
                    };
                    $scope.menuCache[source.idP] = menu;
                }
                return menu;
            };
            $scope.menuCache = [];

            $scope.isAggregate = function (series, cst) {
                var source = spChartBuilderService.getSource($scope.model, series, cst);
                if (!source)
                    return null;

                var res = sp.result(source, 'sourceAggMethod.alias') ||
                    (sp.result(source, 'specialChartSource.nsAlias') === 'core:countSource');
                return !!res;
            };

            $scope.setAggMethod = function (sourceId, methodAlias) {
                applyChange(function() {
                    var source = $scope.menuCache[sourceId].chartSource;
                    source.sourceAggMethod = methodAlias;
                });
            };

            $scope.showSourceOptions = function (series, cst) {
                if (!series || !cst)
                    return false;
                var source = spChartBuilderService.getSource($scope.model, series, cst);
                if (!source)
                    return false;
                // Don't show options for special chart sources
                return source.specialChartSource === null;
            };


            /////////// Properties Dialogs ///////////

            // showProps
            $scope.showProps = function (series, cst) {
                if (!cst.hasProps)
                    return;
                var propFn = $scope[cst.alias + 'Properties'];
                propFn(series, cst);
            };

            // primary axis dialog
            $scope.primarySourceProperties = function (series, cst) {
                var options = {
                    series: series,
                    sourceType: sp.result($scope.svc.getSourceInfo($scope.model, series, cst), 'type'),
                    axis: series.primaryAxis,
                    title: 'Primary Axis Properties'
                };
                applyChange(_.partial(spAxisProperties.showDialog, options));
            };

            // value axis dialog
            $scope.valueSourceProperties = function (series, cst) {
                var options = {
                    series: series,
                    sourceType: sp.result($scope.svc.getSourceInfo($scope.model, series, cst), 'type'),
                    axis: series.valueAxis,
                    title: 'Value Axis Properties'
                };
                applyChange(_.partial(spAxisProperties.showDialog, options));
            };

            // color dialog
            $scope.colorSourceProperties = function (series) {
                var options = { series: series };
                applyChange(_.partial(spSeriesColorProperties.showDialog, options));
            };

            // symbol dialog
            $scope.symbolSourceProperties = function (series) {
                var options = { series: series };
                applyChange(_.partial(spSymbolProperties.showDialog, options));
            };
        });
}());