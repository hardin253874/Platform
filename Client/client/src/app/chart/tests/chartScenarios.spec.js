// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport */
/*global visDataTestData, spCharts, spChartTypes */

describe('Charts|spec|spChartService|scenarios', function () {

    beforeEach(module('mod.common.spVisDataService'));
    beforeEach(module('mod.common.ui.spChartService'));
    beforeEach(module('mod.common.spWebService'));
    beforeEach(module('mod.app.chartBuilder.services.spChartBuilderService'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedReportService'));
    beforeEach(module('sp.app.navigation'));   

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    function makeScenario() {
        var scenario = null;
        inject(function(spChartBuilderService) {
            var testReportId = 123;
            var builderModel = spChartBuilderService.createChartModel({ reportId: testReportId });
            scenario = {
                defaultChecks: true,
                reportTestData: visDataTestData.allFields,
                builderModel: builderModel,
                chart: builderModel.chart,
                series: builderModel.chart.chartHasSeries[0],
                isMobile: false,
                size: { width: 640, height: 480 },
                menusEnabled: false,
                chartDataOptions: {
                    externalConds: null,
                    isInScreen: false,
                    loadLatest: null
                }
            };
            scenario.makeColumnSource = function(colName) {
                var colId = _.filter(_.keys(scenario.reportTestData.meta.rcols), function(curColId) {
                    return scenario.reportTestData.meta.rcols[curColId].title === colName;
                })[0];
                var source = spEntity.fromJSON({
                    typeId: 'chartSource',
                    name: jsonString(colName),
                    chartReportColumn: jsonLookup(colId),
                    specialChartSource: jsonLookup(),
                    sourceAggMethod: jsonLookup()
                });
                return source;
            };
            scenario.makeSpecialSource = function (specialType) {
                var source = spEntity.fromJSON({
                    typeId: 'chartSource',
                    name: jsonString('Count'),
                    chartReportColumn: jsonLookup(),
                    specialChartSource: jsonLookup(specialType),
                    sourceAggMethod: jsonLookup()
                });
                return source;
            };
        });
        return scenario;
    }

    function testScenario(scenario, svgCallback) {
        inject(function (spVisDataService, spChartService, spReportService, spXsrf, spWebService) {

            // Mock target
            var containerDiv = document.createElement('div');

            // Set up mocked report data
            var reportId = scenario.chart.chartReport.idP;
            spReportService.mockGetReportData(reportId, scenario.reportTestData);

            // Set up chart renderer
            var chart = new spCharts.Chart();
            chart.isMobile = scenario.isMobile;
            chart.setServices({
                spVisDataService: spVisDataService,
                spChartService: spChartService,
                spWebService: spWebService,
                spXsrf: spXsrf
            });

            chart.setSize(scenario.size);

            if (scenario.menusEnabled) {
                //chart.entitySelectedCallback = entitySelectedCallback;
                //chart.filterSelectedCallback = filterSelectedCallback;
                //chart.entityContextMenuCallback = entityContextMenuCallback;
            }

            TestSupport.wait(
                // Get mocked data
                spChartService.requestChartData(scenario.chart, scenario.chartDataOptions)
                .then(function (visData) {
                    // Draw chart
                    chart.setChartEntity(scenario.chart);
                    chart.setChartData(visData);
                    chart.drawChart(containerDiv);

                    // Check chart
                    var svg = containerDiv.children[0];
                    if (scenario.defaultChecks) {
                        expect(svg).toBeTruthy();
                        expect(svg.tagName).toBe("svg");
                        expect(svg.children.length > 0).toBeTruthy();
                    }
                    if (svgCallback) {
                        svgCallback(svg);
                    }
                }));
        });
    }

    describe('chart rendering runs', function () {

        spChartTypes.forceGraph.testMode = true;
                    
        var simpleChartTypes = ['barChart', 'columnChart', 'areaChart', 'lineChart', 'pieChart', 'donutChart', 'matrixChart', 'scatterChart', 'funnelChart', 'bubbleChart', 'gaugeChart', 'vectorFieldChart'];
        var hierarchyCharts = ['horzHierarchy', 'radialTreeGraph', 'sunburstChart', 'treeMapChart', 'forceGraph'];
        var allChartTypes = ['barChart', 'columnChart', 'areaChart', 'lineChart', 'pieChart', 'donutChart', 'matrixChart', 'scatterChart', 'funnelChart', 'bubbleChart', 'gaugeChart', 'horzHierarchy', 'radialTreeGraph', 'sunburstChart', 'treeMapChart', 'forceGraph', 'vectorFieldChart'];

        describe('for simple', function () {
            _.forEach(simpleChartTypes, function (chartType) {
                it(chartType, function () {
                    var scenario = makeScenario();
                    scenario.chart.chartTitle = "Test chart";
                    var series = scenario.series;
                    series.chartType = spEntity.fromId(chartType);
                    if (chartType !== 'gaugeChart')
                        series.primarySource = scenario.makeColumnSource('Name');
                    series.valueSource = scenario.makeColumnSource('Number');
                    if (chartType === 'bubbleChart')
                        series.sizeSource = scenario.makeColumnSource('Number');
                    if (chartType === 'vectorFieldChart') {
                        series.endPrimarySource = scenario.makeColumnSource('Name');
                        series.endValueSource = scenario.makeColumnSource('Autonumber');
                    }
                    testScenario(scenario);
                });
            });
        });

        describe('for flat hierarchy ', function () {
            _.forEach(hierarchyCharts, function (chartType) {
                it(chartType, function () {
                    var scenario = makeScenario();
                    var series = scenario.series;
                    series.chartType = spEntity.fromId(chartType);
                    series.primarySource = scenario.makeColumnSource('Name');
                    series.valueSource = scenario.makeColumnSource('Number');
                    series.associateSource = scenario.makeColumnSource('Weekday');
                    testScenario(scenario);
                });
            });
        });

        var sources = ['primarySource', 'valueSource', 'colorSource', 'endValueSource', 'textSource'];
        var columns = ['Single Line', 'Boolean', 'Number', 'Decimal', 'Currency', 'Autonumber', 'Calculation', 'Weekday', 'List : Condiments', 'AA_Employee', 'AA_Herb', 'Date', 'Time', 'DateTime'];

        _.forEach(sources, function (source) {
            describe('when column chart ' + source, function () {
                _.forEach(columns, function (column) {
                    it('uses ' + column + ' column', function () {
                        var scenario = makeScenario();
                        var series = scenario.series;
                        series.chartType = spEntity.fromId('columnChart');
                        series.primarySource = scenario.makeColumnSource('Name');
                        series.valueSource = scenario.makeColumnSource('Number');
                        series[source] = scenario.makeColumnSource(column);
                        testScenario(scenario);
                    });
                });
                
            });
        });

        describe('when symbol source is used on', function () {
            _.forEach(['scatterChart', 'lineChart'], function (chartType) {
                it('has symbol source', function () {
                    var scenario = makeScenario();
                    var series = scenario.series;
                    series.chartType = spEntity.fromId(chartType);
                    series.primarySource = scenario.makeColumnSource('Name');
                    series.valueSource = scenario.makeColumnSource('Number');
                    series.symbolSource = scenario.makeColumnSource('Weekday');
                    testScenario(scenario);
                });
            });
        });

        var imageCharts = ['pieChart', 'donutChart', 'matrixChart', 'scatterChart', 'funnelChart', 'bubbleChart', 'sunburstChart', 'treeMapChart'];
        describe('when image is shown on ', function () {
            _.forEach(imageCharts, function (chartType) {
                it(chartType, function () {
                    var scenario = makeScenario();
                    var series = scenario.series;
                    series.chartType = spEntity.fromId(chartType);
                    series.primarySource = scenario.makeColumnSource('Name');
                    series.valueSource = scenario.makeColumnSource('Number');
                    series.imageSource = scenario.makeColumnSource('New Image Field');
                    series.associateSource = scenario.makeColumnSource('Weekday');
                    if (chartType === 'bubbleChart')
                        series.sizeSource = scenario.makeColumnSource('Number');
                    testScenario(scenario);
                });
            });
        });

        var labelPositions = ['Outside', 'Inside'];
        _.forEach(labelPositions, function(labelPosition) {
            describe('when text source is set with label ' + labelPosition, function () {
                _.forEach(allChartTypes, function (chartType) {
                    it(chartType, function () {
                        var scenario = makeScenario();
                        var series = scenario.series;
                        series.dataLabelPos = spEntity.fromId('dataLabelPos' + labelPosition);
                        series.chartType = spEntity.fromId(chartType);
                        if (chartType !== 'gaugeChart')
                            series.primarySource = scenario.makeColumnSource('Name');
                        series.valueSource = scenario.makeColumnSource('Number');
                        series.textSource = scenario.makeColumnSource('Weekday');
                        series.associateSource = scenario.makeColumnSource('Weekday');
                        if (chartType === 'bubbleChart')
                            series.sizeSource = scenario.makeColumnSource('Number');
                        if (chartType === 'vectorFieldChart') {
                            series.endPrimarySource = scenario.makeColumnSource('Name');
                            series.endValueSource = scenario.makeColumnSource('Autonumber');
                        }
                        testScenario(scenario);
                    });
                });
            });
        });



        var scaleTypes = ['logScaleType', 'linearScaleType', 'categoryScaleType'];
        describe('showing numeric data on', function () {
            _.forEach(scaleTypes, function (scaleType) {
                it(scaleType, function () {
                    var scenario = makeScenario();
                    var series = scenario.series;
                    series.chartType = spEntity.fromId('columnChart');
                    series.valueAxis.axisScaleType = spEntity.fromId(scaleType);
                    series.primarySource = scenario.makeColumnSource('Name');
                    series.valueSource = scenario.makeColumnSource('Number');
                    testScenario(scenario);
                });
            });
        });

        it('on mobile', function () {
            var scenario = makeScenario();
            scenario.isMobile = true;
            scenario.size = { width: 320, height: 240 };
            var series = scenario.series;
            series.chartType = spEntity.fromId('columnChart');
            series.primarySource = scenario.makeColumnSource('Name');
            series.valueSource = scenario.makeColumnSource('Number');
            series.colorSource = scenario.makeColumnSource('Weekday');
            testScenario(scenario);
        });

        describe('stack method', function () {
            _.forEach(['stackMethodNotStacked', 'stackMethodStacked', 'stackMethodCentre', 'stackMethod100percent'], function (stackMethod) {
                it(stackMethod, function () {
                    var scenario = makeScenario();
                    var series = scenario.series;
                    series.name = "Grouped";
                    series.chartType = spEntity.fromId('columnChart');
                    series.stackMethod = spEntity.fromId(stackMethod);
                    series.primarySource = scenario.makeColumnSource('Weekday');
                    series.valueSource = scenario.makeColumnSource('Number');
                    series.colorSource = scenario.makeColumnSource('Name');
                    testScenario(scenario);
                });
            });
        });

        describe('conditional formatting', function () {
            it('runs', function () {
                var scenario = makeScenario();
                scenario.reportTestData = visDataTestData.conditionalFormatting;
                var series = scenario.series;
                series.chartType = spEntity.fromId('columnChart');
                series.primarySource = scenario.makeColumnSource('Text Field');
                series.valueSource = scenario.makeColumnSource('Number Field');
                series.colorSource = scenario.makeColumnSource('Number Field');
                series.useConditionalFormattingColor = true;
                testScenario(scenario);
            });
        });

        //describe('element interraction', function () {
        //    it('hover', function () {
        //        var scenario = makeScenario();
        //        var series = scenario.series;
        //        series.chartType = spEntity.fromId('columnChart');
        //        series.primarySource = scenario.makeColumnSource('Name');
        //        series.valueSource = scenario.makeColumnSource('Number');
        //        testScenario(scenario, function (svg) {
        //            var dataPoints = document.evaluate("//*[local-name()='g'][@primary]", svg);
        //            var dataPoint = dataPoints.iterateNext();
        //            //dataPoint.onmouseover();
        //        });
        //    });
        //});

        describe('pivoted', function () {
            it('stacked', function () {
                var scenario = makeScenario();
                scenario.reportTestData = visDataTestData.allFieldsPivoted;
                var series = scenario.series;
                series.chartType = spEntity.fromId('columnChart');
                series.primarySource = scenario.makeColumnSource('Weekday');
                series.valueSource = scenario.makeColumnSource('Number');
                series.valueSource.sourceAggMethod = spEntity.fromId('aggSum');
                series.colorSource = scenario.makeColumnSource('Boolean');
                series.textSource = scenario.makeSpecialSource('countSource');
                series.useConditionalFormattingColor = true;
                testScenario(scenario);
            });
        });
        
    });


});
