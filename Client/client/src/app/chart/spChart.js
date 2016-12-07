// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, d3, angular, sp, spCharts */

(function () {
    'use strict';

    /**
     * Module implementing a report control.
     *
     * @module spChart
     * @example

     Using the spChart:

     &lt;sp-chart options="options"&gt;&lt;/sp-chart&gt

     where options is available on the controller with the following properties:
     - chartId - {number}. The entity of the chart id to display.

     - reportModel - {object}. An existing report model. This is used to initialise the report directive from a prior
     state. e.g. when navigating back from a child edit form.
     If both the reportModel and reportId are specified, reportModel is favoured.
     *
     */

    angular.module('mod.common.ui.spChart', [
        'mod.common.spEntityService',
        'mod.common.ui.spChartService',
        'mod.common.ui.spContextMenu',
        'mod.common.ui.spActionsService',
        'mod.common.ui.spBusyIndicator',
        'mod.common.spXsrf',
        'mod.common.spMobile',
        'mod.common.alerts',
        'mod.common.spWebService',
        'mod.common.spCachingCompile',
        'mod.common.spVisDataService'
    ]);

    angular.module('mod.common.ui.spChart')
        .directive('spChart', spChartDirective);

    /* @ngInject */
    function spChartDirective($q, spChartService, spContextMenuService, spXsrf, $timeout, spMobileContext, spAlertsService, spCachingCompile, spVisDataService, spWebService) {

        var magicALittleLessHeight = 4;         // A magic number that keeps a chart from growing by 4 pixels on each redraw.
                                                // I'm guessing it's padding or a border that is being included by mistake.
        var magicALittleLessHeightMobile = 5;   // ^^^ What he said.

        return {
            restrict: 'E',
            replace: false,
            transclude: false,
            scope: {
                chartEntity: '=?',
                chartId: '=?',
                options: '=?',
                formControl: '=?',
                parentControl: '=?'
            },
            link: link
        };

        function link(scope, el, attrs) {
            var chartTooltipClassId;
            var redrawChart = false;                        // draw the chart on the next cycle
            var haveDrawn = false;                          // have we drawn for the first time
            var redrawCounter = 0;                          // This increments each time we have a user instigate size change.
            var oldSize = null;

            scope.rootElem = el[0];
            scope.contextMenuIsOpen = false;
            scope.contextMenu = {menuItems: []};
            scope.options = scope.options || {};
            scope.busyIndicator = {
                type: 'spinner',
                text: 'Loading...',
                placement: 'element',
                isBusy: false
            };
            scope.chartRefreshOptions = {
                refreshCallback: function () { refreshChart({ isRefresh: true }); },
                isInDesign: false
            };
            scope.isMobile = spMobileContext.isMobile;
            scope.isTablet = spMobileContext.isTablet;

            // Called by context menu to load the menu. Returns a promise.
            scope.getContextMenuActions = getContextMenuActions;
            scope.getActionExecutionContext = function(action, ids) {
                return {
                    scope: scope,
                    state: action.state,
                    selectionEntityIds: ids,
                    refreshDataCallback: function() {
                        refreshChart({ isRefresh: true });
                    }
                };
            };

            // Watch chartId
            scope.$watch('chartId', chartIdChanged);

            // Watch chartEntity
            scope.$watch('chartEntity', function () {
                if (_.isUndefined(scope.chartEntity)) {
                    // if undefined, then chartId is not bound properly .. so ignore it
                    return;
                }
                if (scope.chartEntityInner === scope.chartEntity) {
                    return;
                }
                scope.chartEntityInner = scope.chartEntity;
                refreshChart();
            });

            // Watch options
            scope.$watch('options', function () {
                scope.options.refreshChart = refreshChart;
            });

            // Watch options
            scope.$watch('options.externalConds', function () {
                refreshChart();
            });

            scope.$watch(resizeTest, function (value, oldValue) {
                if (value !== oldValue && haveDrawn) {
                    triggerDrawChart();
                }
            });

            // Cleanup
            scope.$on('$destroy', function () {
                cleanupTooltips();
            });

            scope.$on('gather', function (event, callback) {
                callback(scope.formControl, scope.parentControl, el);
            });

            function chartIdChanged() {
                if (_.isUndefined(scope.chartId)) {
                    // if undefined, then chartId is not bound properly .. so ignore it
                    return; 
                }
                if (!scope.chartId) {
                    // if null or zero, it's probably acting as a source of truth
                    scope.chartEntity = null;
                    scope.chartData = null;
                    scope.chartEntityInner = null;
                    return;
                }
                scope.busyIndicator.isBusy = true;
                spChartService.getChart(scope.chartId, { batch: true }).then(function(chartEntity) {
                    scope.chartEntity = chartEntity;
                    scope.chartEntityInner = chartEntity;
                    scope.chartData = null;
                    if (scope.options.refreshChart) {
                        scope.options.refreshChart();
                    } else {
                        scope.busyIndicator.isBusy = false;
                    }
                });
            }

            function refreshChart(overrideParams) {
                var chartEntity = scope.chartEntityInner;
                cleanupTooltips();
                d3.select(scope.rootElem).select("svg").remove();
                if (!chartEntity) return;
                if (!chartEntity.chartReport) return;

                scope.chartData = null;
                scope.busyIndicator.isBusy = true;

                var chartDataOptions = {
                    externalConds: scope.options.externalConds,
                    isInScreen: scope.options.isInScreen,
                    loadLatest: function(chartData) {
                        scope.chartData = chartData;
                        triggerDrawChart();
                    }
                };

                if (overrideParams) {
                    chartDataOptions = _.extend(chartDataOptions, overrideParams);
                }

                spChartService.requestChartData(chartEntity, chartDataOptions).then(function (chartData) {
                    scope.chartData = chartData;
                    triggerDrawChart();
                    scope.busyIndicator.isBusy = false;
                }, function(error) {
                    scope.busyIndicator.isBusy = false;
                    spAlertsService.addAlert('An error occurred getting the chart: ' + sp.result(error, 'data.Message'), { expires: false, severity: spAlertsService.sev.Error });
                });

                // Update any listeners.. as the selection probably longer applies
                filterSelectedCallback([], true);
            }

            function containerSize() {
                var size;

                if (scope.options && scope.options.containerClass) {

                    var container = el.closest(scope.options.containerClass);

                    if (container && container.length) {
                        return {
                            width: container.width(),
                            height: container.height()
                        };
                    }
                }

                return {
                    width: scope.rootElem.clientWidth || scope.rootElem.parentElement.clientWidth,
                    height: scope.rootElem.clientHeight || scope.rootElem.parentElement.clientHeight
                };
            }

            //
            // Resize if there has been a user instigated change.
            //
            function resizeTest() {
                var newSize = containerSize();

                if (oldSize && newSize && newSize.height && newSize.width && (oldSize.width !== newSize.width ||
                    oldSize.height !== newSize.height)) {  // this test is not great. WE can get an extra false trigger after a draw.

                    redrawCounter++;
                }

                oldSize = newSize;
                return redrawCounter;
            }

            //
            // redraw the chart on the next digest cycle.
            //
            function triggerDrawChart() {
                if (redrawChart || !scope.chartData) {
                    return;
                }

                redrawChart = true;

                $timeout(function () {
                    redrawChart = false;

                    var tablet = spMobileContext.isTablet;
                    var mobile = spMobileContext.isMobile;
                    var magic = magicALittleLessHeight;
                    if (mobile) {
                        magic = magicALittleLessHeightMobile;
                    }

                    var size = containerSize();
                    size.width = size.width || 640;
                    size.height = (size.height && size.height - magic) || 480;

                    drawChart(size);
                    haveDrawn = true;
                });
            }

            function menusEnabled() {
                return scope.options.mode !== 'screenBuilder' && scope.options.mode !== 'chartBuilder';
            }

            //
            // Draw chart
            //
            function drawChart(size) {
                if (!scope.chartData)
                    return;

                // console.log("spChart: drawing");

                cleanupTooltips();
                d3.select(scope.rootElem).select("svg").remove();
                scope.options.selectedEntityId = 0;
                scope.options.drilldownConds = spVisDataService.getEmptyConds();

                var chart = new spCharts.Chart();
                chart.isMobile = spMobileContext.isMobile;
                chart.setServices({
                    spVisDataService: spVisDataService,
                    spChartService: spChartService,
                    spWebService: spWebService,
                    spXsrf: spXsrf,
                    appData: sp.result(scope, '$root.appData')
                });
                chart.setSize(size);
                if (menusEnabled()) {
                    chart.entitySelectedCallback = entitySelectedCallback;
                    chart.filterSelectedCallback = filterSelectedCallback;
                    chart.entityContextMenuCallback = entityContextMenuCallback;
                }
                chart.setChartEntity(scope.chartEntityInner);
                chart.setChartData(scope.chartData);
                chartTooltipClassId = chart.getTooltipClassId();

                chart.drawChart(scope.rootElem);
            }

            // Called by d3 chart event when an entity is left-clicked
            function entitySelectedCallback(id, elem) {
                scope.options.selectedEntityId = id;
                scope.$apply();
            }

            // Called by d3 chart event when a pivot data point is selected
            function filterSelectedCallback(drilldownConds, noApply) {
                var conds = drilldownConds || [];
                if (scope.options.externalConds) {
                    conds = conds.concat(scope.options.externalConds);
                }

                $timeout(function () {
                    scope.options.drilldownConds = conds;
                }, 0);
            }

            // Called by d3 chart event when an entity is right-clicked
            function entityContextMenuCallback(id, elem) {
                scope.menuToShow = id;
                scope.contextMenuIsOpen = true;
                scope.$apply();
            }

            function getContextMenuActions (menuScope) {
                if (!menusEnabled() || !scope.menuToShow)
                    return $q.when(null);
                var consoleActionRequest =
                {
                    ids: [scope.menuToShow],
                    additional: {}
                };

                return spContextMenuService.getConsoleActionsAsMenu(consoleActionRequest);
            }

            // Cleanup any tooltips created by this chart
            function cleanupTooltips() {
                if (chartTooltipClassId) {
                    d3.selectAll('.' + chartTooltipClassId).remove();
                    chartTooltipClassId = null;
                }
            }

            var cachedLinkFunc = spCachingCompile.compile('chart/spChart.tpl.html');
            cachedLinkFunc(scope, function (clone) {
                el.append(clone);
                scope.rootElem = clone[0];
            });
        }
    }

}());