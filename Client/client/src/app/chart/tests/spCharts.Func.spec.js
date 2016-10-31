// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport */
/* global spCharts */

describe('Charts|Viewer|spec|spCharts.Func', function () {
    'use strict';

    beforeEach(module('mod.common.ui.spChartService'));
    beforeEach(module('mod.common.spVisDataService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    var mockSource = function (column, sourceAggMethod, specialChartSource) {
        return {
            entity: spEntity.fromJSON({
                chartReportColumn: jsonLookup(column || null),
                sourceAggMethod: jsonLookup(sourceAggMethod || null),
                specialChartSource: jsonLookup(specialChartSource || null)
            })
        };
    };

    it('groupData runs', function () {

        var row1 = { x: 1, g: 1 };
        var row2 = { x: 2, g: 1};
        var row3 = { x: 3, g: 2 };
        var rows = [row1, row2, row3];
        var pivot = function(row) {
            return row.g;
        };

        var groups = spCharts.groupData(rows, pivot);

        // expected output:
        // [
        //    {  //#1
        //       representative: row1,
        //       data: [ { row:row1, group: #1 }, { row:row2, group: #1 } ]
        //    },
        //    {  //#2
        //       representative: row3,
        //       data: [ { row:row3, group: #2 } ]
        //    },
        // ]

        expect(groups).toBeArray(2);
        expect(groups[0].representative).toBe(row1);
        expect(groups[0].data).toBeArray(2);
        expect(groups[0].data[0].group).toBe(groups[0]);
        expect(groups[0].data[0].row).toBe(row1);
        expect(groups[0].data[1].group).toBe(groups[0]);
        expect(groups[0].data[1].row).toBe(row2);
        expect(groups[1].representative).toBe(row3);
        expect(groups[1].data).toBeArray(1);
        expect(groups[1].data[0].group).toBe(groups[1]);
        expect(groups[1].data[0].row).toBe(row3);
    });

    it('groupAndFillData runs', function () {

        var row1 = { x: 1, g: 1 };
        var row2 = { x: 2, g: 2 };
        var row3 = { x: 2, g: 1 };
        var row4 = { x: 3, g: 3 };
        var rows = [row1, row2, row3, row4];

        var pivot = function (row) { return row.g; };
        var primary = function (row) { return row.x; };
        var sort = primary;

        var groups = spCharts.groupAndFillData(rows, pivot, primary, sort);
        
        expect(groups).toBeArray(3);

        expect(groups[0].representative).toBe(row1);
        expect(groups[0].data).toBeArray(3);
        expect(groups[0].data[0].group).toBe(groups[0]); expect(groups[0].data[0].row).toBe(row1);
        expect(groups[0].data[1].group).toBe(groups[0]); expect(groups[0].data[1].row).toBe(row3);
        expect(groups[0].data[2].group).toBe(groups[0]); expect(groups[0].data[2].row).not.toBeDefined();

        expect(groups[1].representative).toBe(row2);
        expect(groups[1].data).toBeArray(3);
        expect(groups[1].data[0].group).toBe(groups[1]); expect(groups[1].data[0].row).not.toBeDefined();
        expect(groups[1].data[1].group).toBe(groups[1]); expect(groups[1].data[1].row).toBe(row2);
        expect(groups[1].data[2].group).toBe(groups[1]); expect(groups[1].data[2].row).not.toBeDefined();

        expect(groups[2].representative).toBe(row4);
        expect(groups[2].data).toBeArray(3);
        expect(groups[2].data[0].group).toBe(groups[2]); expect(groups[2].data[0].row).not.toBeDefined();
        expect(groups[2].data[1].group).toBe(groups[2]); expect(groups[2].data[1].row).not.toBeDefined();
        expect(groups[2].data[2].group).toBe(groups[2]); expect(groups[2].data[2].row).toBe(row4);
    });

    describe('combineDomains', function () {

        it('works for category scales', function () {
            var d1 = [1, 4, 6];
            var d2 = [4, 6, 8];
            var d3 = [];
            var res = spCharts.combineDomains([d1, d2, d3], 'categoryScaleType');
            expect(res.length).toBe(4);
            expect(_.indexOf(res, 1) >= 0).toBeTruthy();
            expect(_.indexOf(res, 4) >= 0).toBeTruthy();
            expect(_.indexOf(res, 6) >= 0).toBeTruthy();
            expect(_.indexOf(res, 8) >= 0).toBeTruthy();
        });

        it('works for linear scales', function () {
            var d1 = [1, 6];
            var d2 = [4, 8];
            var d3 = [];
            var res = spCharts.combineDomains([d1, d2, d3], 'linearScaleType');
            expect(res.length).toBe(2);
            expect(res[0]).toBe(1);
            expect(res[1]).toBe(8);
        });

    });

    describe('makeTicksFit', function () {

        it('returns the ticks unchanged if they all fit happily', function () {
            var domain = ['a', 'b', 'c'];
            var range = [0, 200];
            var minPixelsPerTick = 12;
            var res = spCharts.makeTicksFit(domain, range, minPixelsPerTick);
            expect(res === domain).toBeTruthy();
        });

        it('remove ticks if they dont all fit', function () {
            var domain = ['a', 'b', 'c', 'd', 'e', 'f', 'g'];
            var range = [0, 200];
            var minPixelsPerTick = 50;
            var res = spCharts.makeTicksFit(domain, range, minPixelsPerTick);
            expect(res.length).toBe(4);
            expect(res[0]).toBe('a');
            expect(res[1]).toBe('c');
            expect(res[2]).toBe('e');
            expect(res[3]).toBe('g');
            expect(res.length * minPixelsPerTick <= range[1]).toBeTruthy();
        });
    });

    describe('sourcesMatch', function () {

        it('false if either or both are null', function () {
            var s1 = mockSource('col1');
            expect(spCharts.sourcesMatch(s1, null)).toBeFalsy();
            expect(spCharts.sourcesMatch(null, s1)).toBeFalsy();
            expect(spCharts.sourcesMatch(null, null)).toBeFalsy();
        });

        it('true for matching columns without aggregates', function () {
            var s1 = mockSource('col1');
            var s2 = mockSource('col1');
            var res = spCharts.sourcesMatch(s1, s2);
            expect(res).toBeTruthy();
        });

        it('false for different columns', function () {
            var s1 = mockSource('col1');
            var s2 = mockSource('col2');
            var res = spCharts.sourcesMatch(s1, s2);
            expect(res).toBeFalsy();
        });

        it('true for matching special sources', function () {
            var s1 = mockSource(null, null, 'rowNumberSource');
            var s2 = mockSource(null, null, 'rowNumberSource');
            var res = spCharts.sourcesMatch(s1, s2);
            expect(res).toBeTruthy();
        });

        it('false for non-matching special sources', function () {
            var s1 = mockSource(null, null, 'rowNumberSource');
            var s2 = mockSource(null, null, 'countSource');
            var res = spCharts.sourcesMatch(s1, s2);
            expect(res).toBeFalsy();
        });

        it('false for special vs non-special', function () {
            var s1 = mockSource(null, null, 'rowNumberSource');
            var s2 = mockSource('col1', null, null);
            var res = spCharts.sourcesMatch(s1, s2);
            expect(res).toBeFalsy();
        });

        it('true for matching aggregates', function () {
            var s1 = mockSource('col1', 'aggSum');
            var s2 = mockSource('col1', 'aggSum');
            var res = spCharts.sourcesMatch(s1, s2);
            expect(res).toBeTruthy();
        });

        it('false for non-matching aggregates', function () {
            var s1 = mockSource('col1', 'aggSum');
            var s2 = mockSource('col1', 'aggAverage');
            var res = spCharts.sourcesMatch(s1, s2);
            expect(res).toBeFalsy();
        });

        it('false for one aggregate only', function () {
            var s1 = mockSource('col1', 'aggSum');
            var s2 = mockSource('col1', null);
            var res = spCharts.sourcesMatch(s1, s2);
            expect(res).toBeFalsy();
        });

    });

    describe('isStackedSeries', function () {

        var stackableScenario = function (stackMethod) {
            return {
                series: {
                    entity: spEntity.fromJSON({
                        chartType: jsonLookup('core:columnChart'),
                        stackMethod: jsonLookup(stackMethod||'core:stackMethodStacked')
                    }),
                    accessors: {
                        colorSource: mockSource('someColorColumn'),
                        primarySource: mockSource('somePrimaryColumn'),
                        endValueSource: null
                    },
                    valueAxis: {
                        method: 'linearScaleType'
                    }
                }
            };
        };

        it('true for normal linear stacked', function () {
            var test = stackableScenario();
            var res = spCharts.isStackedSeries(test.series);
            expect(res).toBeTruthy();
        });

        it('true for log scales', function () {
            var test = stackableScenario();
            test.series.valueAxis.method = 'logScaleType';
            var res = spCharts.isStackedSeries(test.series);
            expect(res).toBeTruthy();
        });

        it('true for 100% stacked', function () {
            var test = stackableScenario('core:stackMethod100percent');
            var res = spCharts.isStackedSeries(test.series);
            expect(res).toBeTruthy();
        });

        it('true for center stacked', function () {
            var test = stackableScenario('core:stackMethodCentre');
            var res = spCharts.isStackedSeries(test.series);
            expect(res).toBeTruthy();
        });

        it('true if value axis is linear or log', function () {
            _.forEach(['linearScaleType', 'logScaleType'], function (scaleType) {
                var test = stackableScenario();
                test.series.valueAxis.method = scaleType;
                var res = spCharts.isStackedSeries(test.series);
                expect(res).toBeTruthy();
            });
        });

        it('false if value axis is date or category', function () {
            _.forEach(['categoryScaleType', 'dateTimeScaleType'], function (scaleType) {
                var test = stackableScenario('');
                test.series.valueAxis.method = scaleType;
                var res = spCharts.isStackedSeries(test.series);
                expect(res).toBeFalsy();
            });
        });

        it('false if stacking disabled', function () {
            var test = stackableScenario('core:stackMethodNotStacked');
            var res = spCharts.isStackedSeries(test.series);
            expect(res).toBeFalsy();
        });

        it('true if stacking not set', function () {
            var test = stackableScenario();
            test.series.entity.stackMethod = null;
            var res = spCharts.isStackedSeries(test.series);
            expect(res).toBeTruthy();
        });

        it('false if an end value is specified', function () {
            var test = stackableScenario(null);
            test.series.accessors.endValueSource = mockSource('someEndValueColumn');
            var res = spCharts.isStackedSeries(test.series);
            expect(res).toBeFalsy();
        });

        it('false if the primary source is the same as the colour source', function () {
            // because no two bars of different colors would ever land on the same position
            var test = stackableScenario(null);
            test.series.accessors.colorSource = mockSource('somePrimaryColumn');
            var res = spCharts.isStackedSeries(test.series);
            expect(res).toBeFalsy();
        });
    });

    describe('isSideBySide', function () {

        var sideBySideScenario = function (stackMethod, chartType) {
            return {
                series: {
                    entity: spEntity.fromJSON({
                        chartType: jsonLookup(chartType || 'core:columnChart'),
                        stackMethod: jsonLookup(stackMethod || 'core:stackMethodNotStacked')
                    }),
                    accessors: {
                        colorSource: mockSource('someColorColumn'),
                        primarySource: mockSource('somePrimaryColumn'),
                        endValueSource: null
                    },
                    primaryAxis: {
                        method: 'categoryScaleType'
                    }
                }
            };
        };

        it('true if color is set on a category primary scale', function () {
            var test = sideBySideScenario();
            var res = spCharts.isSideBySide(test.series, test.primaryAxis);
            expect(res).toBeTruthy();
        });

        it('true if primary axis is date or category', function () {
            var test = sideBySideScenario();
            _.forEach(['categoryScaleType', 'dateTimeScaleType'], function (scaleType) {
                test.series.primaryAxis.method = scaleType;
                var res = spCharts.isSideBySide(test.series, test.primaryAxis);
                expect(res).toBeTruthy();
            });
        });

        it('false if primary axis is linear or log', function () {
            var test = sideBySideScenario();
            _.forEach(['linearScaleType', 'logScaleType'], function(scaleType) {
                test.series.primaryAxis.method = scaleType;
                var res = spCharts.isSideBySide(test.series, test.primaryAxis);
                expect(res).toBeFalsy();
            });
        });

        it('false if a stack method is set', function () {
            _.forEach(['stackMethodStacked', 'stackMethod100percent', 'stackMethod100percent'], function (stackMethod) {
                var test = sideBySideScenario('core:' + stackMethod);
                var res = spCharts.isSideBySide(test.series, test.primaryAxis);
                expect(res).toBeFalsy();
            });
        });

        it('false if not a column or bar chart', function () {
            _.forEach(['lineChart', 'areaChart', 'scatterChart'], function (chartType) {
                var test = sideBySideScenario(null, chartType);
                var res = spCharts.isSideBySide(test.series, test.primaryAxis);
                expect(res).toBeFalsy();
            });
        });

        it('false if stacking is not explicitly set', function () {
            var test = sideBySideScenario();
            test.series.entity.stackMethod = null;
            var res = spCharts.isSideBySide(test.series, test.primaryAxis);
            expect(res).toBeFalsy();
        });

        it('true even if an end value is specified', function () {
            var test = sideBySideScenario(null);
            test.series.accessors.endValueSource = mockSource('someEndValueColumn');
            var res = spCharts.isSideBySide(test.series, test.primaryAxis);
            expect(res).toBeTruthy();
        });

        it('false if the primary source is the same as the colour source', function () {
            // because no two bars of different colors would ever land on the same position
            var test = sideBySideScenario(null);
            test.series.accessors.colorSource = mockSource('somePrimaryColumn');
            var res = spCharts.isSideBySide(test.series, test.primaryAxis);
            expect(res).toBeFalsy();
        });
    });

    describe('getScaleType', function () {

        it('uses default scale type if none specified', inject(function (spChartService, spVisDataService) {
            var services = {
                spChartService: spChartService,
                spVisDataService: spVisDataService
            };
            var axis = spEntity.fromJSON({ whatever: 1 });
            var source = { colType: 'Number' };

            var res = spCharts.getScaleType(services, axis, source);
            expect(res).toBe('linearScaleType');
        }));

        it('uses axis type if specified and supported', inject(function (spChartService, spVisDataService) {
            var services = {
                spChartService: spChartService,
                spVisDataService: spVisDataService
            };
            var axis = spEntity.fromJSON({ axisScaleType: jsonLookup('categoryScaleType') });
            var source = { colType: 'Number' };

            var res = spCharts.getScaleType(services, axis, source);
            expect(res).toBe('categoryScaleType');
        }));

        it('uses default scale type if specified is unsupported', inject(function (spChartService, spVisDataService) {
            var services = {
                spChartService: spChartService,
                spVisDataService: spVisDataService
            };
            var axis = spEntity.fromJSON({ axisScaleType: jsonLookup('logScaleType') });
            var source = { colType: 'Name' };

            var res = spCharts.getScaleType(services, axis, source);
            expect(res).toBe('categoryScaleType');
        }));

        it('defaults to linearScaleType for date types that do not have paddable formatting', inject(function (spChartService, spVisDataService) {
            var services = {
                spChartService: spChartService,
                spVisDataService: spVisDataService
            };
            var axis = spEntity.fromJSON({ whatever: 1 });
            var source = { colType: 'Date' };

            var res = spCharts.getScaleType(services, axis, source);
            expect(res).toBe('linearScaleType');
        }));

        it('falls back to linearScaleType for date types that do not have paddable formatting', inject(function (spChartService, spVisDataService) {
            var services = {
                spChartService: spChartService,
                spVisDataService: spVisDataService
            };
            var axis = spEntity.fromJSON({ axisScaleType: jsonLookup('dateTimeScaleType') });
            var source = { colType: 'Date' };

            var res = spCharts.getScaleType(services, axis, source);
            expect(res).toBe('linearScaleType');
        }));

        it('uses dateTimeScaleType for date types that do have paddable formatting', inject(function (spChartService, spVisDataService) {
            var services = {
                spChartService: spChartService,
                spVisDataService: spVisDataService
            };
            var axis = spEntity.fromJSON({ whatever: 1 });
            var source = { colType: 'Date', formatType: 'dateTimeMonthYear' };

            var res = spCharts.getScaleType(services, axis, source);
            expect(res).toBe('dateTimeScaleType');
        }));
    });

    describe('dateArray', function () {

        it('handles 1', function () {
            var res = spCharts.dateArray(new Date(2001, 5, 5), new Date(2001, 5, 5));
            expect(res).toBeArray(1);
            expect(res[0].getDate()).toBe((new Date(2001, 5, 5)).getDate());
        });

        it('handles 2', function () {
            var res = spCharts.dateArray(new Date(2001, 11, 31), new Date(2002, 0, 1));
            expect(res).toBeArray(2);
            expect(res[0].getDate()).toBe((new Date(2001, 11, 31)).getDate());
            expect(res[1].getDate()).toBe((new Date(2002, 0, 1)).getDate());
        });

        it('handles 3', function () {
            var res = spCharts.dateArray(new Date(2001, 5, 5), new Date(2001, 5, 7));
            expect(res).toBeArray(3);
            expect(res[0].getDate()).toBe((new Date(2001, 5, 5)).getDate());
            expect(res[1].getDate()).toBe((new Date(2001, 5, 6)).getDate());
            expect(res[2].getDate()).toBe((new Date(2001, 5, 7)).getDate());
        });
    });

    describe('numberArray', function () {

        it('handles 1', function () {
            var res = spCharts.numberArray(0, 0);
            expect(res).toBeArray(1);
            expect(res[0]).toBe(0);
        });

        it('straddles zero', function () {
            var res = spCharts.numberArray(-1, 1);
            expect(res).toBeArray(3);
            expect(res[0]).toBe(-1);
            expect(res[1]).toBe(0);
            expect(res[2]).toBe(1);
        });
    });

    describe('padRange', function () {

        it('normal pad', function () {
            var res = spCharts.padRange([100, 200], 5, [1, 1]);
            expect(res).toBeArray(2);
            expect(res[0]).toBe(105);
            expect(res[1]).toBe(195);
        });

        it('reverse pad', function () {
            var res = spCharts.padRange([200, 100], 5, [1, 1]);
            expect(res).toBeArray(2);
            expect(res[0]).toBe(195);
            expect(res[1]).toBe(105);
        });

        it('overflow pad', function () {
            // Range should just collapse to the centre point if padding fills entirety of either side
            var res = spCharts.padRange([100, 110], 6, [1, 1]);
            expect(res).toBeArray(2);
            expect(res[0]).toBe(105);
            expect(res[1]).toBe(105);
        });

        it('reverse overflow pad', function () {
            var res = spCharts.padRange([110, 100], 6, [1, 1]);
            expect(res).toBeArray(2);
            expect(res[0]).toBe(105);
            expect(res[1]).toBe(105);
        });

        it('disable start', function () {
            var res = spCharts.padRange([100, 200], 5, [0, 1]);
            expect(res).toBeArray(2);
            expect(res[0]).toBe(100);
            expect(res[1]).toBe(195);
        });

        it('disable end', function () {
            var res = spCharts.padRange([100, 200], 5, [1, 0]);
            expect(res).toBeArray(2);
            expect(res[0]).toBe(105);
            expect(res[1]).toBe(200);
        });
    });

    describe('padDomainWithMissingTime', function() {

        it('with an invalid format', function () {
            var domain = [new Date()];
            
            var fmt = 'notavalidformat';
            // note: spChartsService.isCategoricalScaleFormat(fmt) ==> false

            var res = spCharts.padDomainWithMissingTime(domain, fmt);
            expect(res.length).toBe(domain.length);
            expect(res[0]).toBe(domain[0]);
        });

        it('with month-year format', function () {
            var d1 = new Date(2012, 2, 1);
            var d2 = new Date(2012, 2, 14);
            var d3 = new Date(2012, 5, 1);
            var d4 = new Date(2013, 2, 2);
            var domain = [d1,d2,d3,d4];
            var res = spCharts.padDomainWithMissingTime(domain, 'dateMonthYear');
            expect(res.length).toBe(13);
            expect(res[0]).toBe(d1);
            expect(res[1]).toEqual(new Date(2012, 3, 1));
            expect(res[2]).toEqual(new Date(2012, 4, 1));
            expect(res[3]).toEqual(new Date(2012, 5, 1));
            expect(res[4]).toEqual(new Date(2012, 6, 1));
            expect(res[5]).toEqual(new Date(2012, 7, 1));
            expect(res[6]).toEqual(new Date(2012, 8, 1));
            expect(res[7]).toEqual(new Date(2012, 9, 1));
            expect(res[8]).toEqual(new Date(2012, 10, 1));
            expect(res[9]).toEqual(new Date(2012, 11, 1));
            expect(res[10]).toEqual(new Date(2013, 0, 1));
            expect(res[11]).toEqual(new Date(2013, 1, 1));
            expect(res[12]).toBe(d4);

            var res2 = spCharts.padDomainWithMissingTime(domain, 'dateTimeMonthYear');
            expect(res2).toEqual(res);
        });

        it('with month format', function () {
            var y = new Date().getFullYear();
            var d1 = new Date(2012, 2, 1);
            var d2 = new Date(2012, 2, 14);
            var d3 = new Date(2012, 5, 1);
            var d4 = new Date(2013, 2, 1);
            var domain = [d1, d2, d3, d4];
            var res = spCharts.padDomainWithMissingTime(domain, 'dateMonth');
            expect(res.length).toBe(12);
            expect(res[0]).toEqual(new Date(y, 0, 1));
            expect(res[1]).toEqual(new Date(y, 1, 1));
            expect(res[2]).toBe(d1);
            expect(res[3]).toEqual(new Date(y, 3, 1));
            expect(res[4]).toEqual(new Date(y, 4, 1));
            expect(res[5]).toBe(d3);
            expect(res[6]).toEqual(new Date(y, 6, 1));
            expect(res[7]).toEqual(new Date(y, 7, 1));
            expect(res[8]).toEqual(new Date(y, 8, 1));
            expect(res[9]).toEqual(new Date(y, 9, 1));
            expect(res[10]).toEqual(new Date(y, 10, 1));
            expect(res[11]).toEqual(new Date(y, 11, 1));

            var res2 = spCharts.padDomainWithMissingTime(domain, 'dateTimeMonth');
            expect(res2).toEqual(res);
        });

        it('with quarter format', function() {
            var y = new Date().getFullYear();
            var d1 = new Date(2012, 2, 1);
            var d2 = new Date(2012, 2, 14);
            var d3 = new Date(2012, 6, 1);
            var d4 = new Date(2013, 2, 1);
            var domain = [d1, d2, d3, d4];
            var res = spCharts.padDomainWithMissingTime(domain, 'dateQuarter');
            expect(res.length).toBe(4);
            expect(res[0]).toBe(d1);
            expect(res[1]).toEqual(new Date(y, 3, 1));
            expect(res[2]).toBe(d3);
            expect(res[3]).toEqual(new Date(y, 9, 1));

            var res2 = spCharts.padDomainWithMissingTime(domain, 'dateTimeQuarter');
            expect(res2).toEqual(res);
        });

        it('with quarter-year format', function () {
            var d1 = new Date(2012, 2, 1);
            var d2 = new Date(2012, 2, 14);
            var d3 = new Date(2012, 6, 1);
            var d4 = new Date(2013, 2, 28);
            var domain = [d1, d2, d3, d4];
            var res = spCharts.padDomainWithMissingTime(domain, 'dateQuarterYear');
            expect(res.length).toBe(5);
            expect(res[0]).toBe(d1);
            expect(res[1]).toEqual(new Date(2012, 3, 1));
            expect(res[2]).toBe(d3);
            expect(res[3]).toEqual(new Date(2012, 9, 1));
            expect(res[4]).toBe(d4);

            var res2 = spCharts.padDomainWithMissingTime(domain, 'dateTimeQuarterYear');
            expect(res2).toEqual(res);
        });

        it('with year format', function () {
            var d1 = new Date(2012, 2, 1);
            var d2 = new Date(2012, 2, 14);
            var d3 = new Date(2012, 5, 1);
            var d4 = new Date(2013, 2, 1);
            var d5 = new Date(2015, 11, 25);
            var domain = [d1, d2, d3, d4, d5];
            var res = spCharts.padDomainWithMissingTime(domain, 'dateYear');
            expect(res.length).toBe(4);
            expect(res[0]).toBe(d1);
            expect(res[1]).toBe(d4);
            expect(res[2]).toEqual(new Date(2014, 0, 1));
            expect(res[3]).toBe(d5);

            var res2 = spCharts.padDomainWithMissingTime(domain, 'dateTimeYear');
            expect(res2).toEqual(res);
        });

        it('with weekday format', function () {
            var d1 = new Date(2012, 2, 1);      // Thu
            var d2 = new Date(2012, 2, 14);     // Wed
            var d3 = new Date(2012, 5, 1);      // Fri
            var d4 = new Date(2013, 2, 1);      // Fri
            var d5 = new Date(2015, 11, 25);    // Fri
            var domain = [d1, d2, d3, d4, d5];
            var res = spCharts.padDomainWithMissingTime(domain, 'dateWeekday');
            expect(res.length).toBe(7);
            expect(res[0].getDay()).toBe(0);
            expect(res[1].getDay()).toBe(1);
            expect(res[2].getDay()).toBe(2);
            expect(res[3]).toBe(d2);
            expect(res[4]).toBe(d1);
            expect(res[5]).toBe(d3);
            expect(res[6].getDay()).toBe(6);

            var res2 = spCharts.padDomainWithMissingTime(domain, 'dateTimeWeekday');
            expect(res2).toEqual(res);
        });

        it('with day-month format', function () {
            var d1 = new Date(2012, 2, 1);
            var d2 = new Date(2012, 2, 14);
            var d3 = new Date(2012, 5, 1);
            var d4 = new Date(2013, 5, 1);
            var domain = [d1, d2, d3, d4];
            var res = spCharts.padDomainWithMissingTime(domain, 'dateDayMonth');
            expect(res.length).toBe(365); // 2012 was a leap year but we are starting in March!
            expect(res[0]).toEqual(new Date(2013, 0, 1));
            expect(res[364]).toEqual(new Date(2012, 11, 31));
            expect(res).toContain(d1);
            expect(res).toContain(d2);
            expect(res).toContain(d3);
            expect(res).not.toContain(d4);
            expect(res[59]).toEqual(new Date(2012, 2, 1));

            var res2 = spCharts.padDomainWithMissingTime(domain, 'dateTimeDayMonth');
            expect(res2).toEqual(res);

            // what about that leap day then?
            var d0 = new Date(2012, 1, 3);
            var domain2 = [d0, d2, d3, d4];
            var res0 = spCharts.padDomainWithMissingTime(domain2, 'dateDayMonth');
            expect(res0.length).toBe(366);
            expect(res0[0]).toEqual(new Date(2013, 0, 1));
            expect(res0[365]).toEqual(new Date(2012, 11, 31));
            expect(res0).toContain(d0);
            expect(res0).toContain(d1);
            expect(res0).toContain(d2);
            expect(res0).toContain(d3);
            expect(res0).not.toContain(d4);
            expect(res0[59]).toEqual(new Date(2012, 1, 29));

            // what about just a small scale?
            var domain3 = [d1, d2];
            var res3 = spCharts.padDomainWithMissingTime(domain3, 'dateDayMonth');
            expect(res3.length).toBe(14);
            expect(res3[0]).toBe(d1);
            expect(res3[1]).toEqual(new Date(2012, 2, 2));
            expect(res3[2]).toEqual(new Date(2012, 2, 3));
            expect(res3[3]).toEqual(new Date(2012, 2, 4));
            expect(res3[4]).toEqual(new Date(2012, 2, 5));
            expect(res3[5]).toEqual(new Date(2012, 2, 6));
            expect(res3[6]).toEqual(new Date(2012, 2, 7));
            expect(res3[7]).toEqual(new Date(2012, 2, 8));
            expect(res3[8]).toEqual(new Date(2012, 2, 9));
            expect(res3[9]).toEqual(new Date(2012, 2, 10));
            expect(res3[10]).toEqual(new Date(2012, 2, 11));
            expect(res3[11]).toEqual(new Date(2012, 2, 12));
            expect(res3[12]).toEqual(new Date(2012, 2, 13));
            expect(res3[13]).toBe(d2);
        });

        it('with date format', function() {
            var d1 = new Date(2012, 2, 1, 3, 24, 0);
            var d2 = new Date(2012, 2, 1, 0, 0, 0);
            var d3 = new Date(2012, 2, 5, 3, 24, 0);
            var domain = [d1, d2, d3];
            var res = spCharts.padDomainWithMissingTime(domain, 'dateTimeDate');
            expect(res.length).toBe(5);
            expect(res[0]).toBe(d1);
            expect(res[1]).toEqual(new Date(2012, 2, 2, 0, 0, 0));
            expect(res[2]).toEqual(new Date(2012, 2, 3, 0, 0, 0));
            expect(res[3]).toEqual(new Date(2012, 2, 4, 0, 0, 0));
            expect(res[4]).toBe(d3);
            expect(res).not.toContain(d2);
        });

        it('with hour format', function () {
            var d1 = new Date(2012, 2, 1, 3, 24, 0);
            var d2 = new Date(2012, 2, 1, 0, 0, 0);
            var d3 = new Date(2012, 2, 5, 3, 15, 0);
            var d4 = new Date(2013, 2, 5, 7, 0, 0);
            var d5 = new Date(2012, 7, 15, 19, 10, 30);
            var domain = [d1, d2, d3, d4, d5];
            var res = spCharts.padDomainWithMissingTime(domain, 'timeHour');
            expect(res.length).toBe(24);
            expect(res[0]).toBe(d2);
            expect(res[1].getHours()).toBe(1);
            expect(res[2].getHours()).toBe(2);
            expect(res[3]).toBe(d1);
            expect(res[4].getHours()).toBe(4);
            expect(res[5].getHours()).toBe(5);
            expect(res[6].getHours()).toBe(6);
            expect(res[7]).toBe(d4);
            expect(res[8].getHours()).toBe(8);
            expect(res[9].getHours()).toBe(9);
            expect(res[10].getHours()).toBe(10);
            expect(res[11].getHours()).toBe(11);
            expect(res[12].getHours()).toBe(12);
            expect(res[13].getHours()).toBe(13);
            expect(res[14].getHours()).toBe(14);
            expect(res[15].getHours()).toBe(15);
            expect(res[16].getHours()).toBe(16);
            expect(res[17].getHours()).toBe(17);
            expect(res[18].getHours()).toBe(18);
            expect(res[19]).toBe(d5);
            expect(res[20].getHours()).toBe(20);
            expect(res[21].getHours()).toBe(21);
            expect(res[22].getHours()).toBe(22);
            expect(res[23].getHours()).toBe(23);
            expect(res).not.toContain(d3);

            var res2 = spCharts.padDomainWithMissingTime(domain, 'dateTimeHour');
            expect(res2).toEqual(res);
        });
    });
});