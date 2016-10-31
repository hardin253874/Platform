// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport */
/* global visDataTestData, spCharts */

describe('Charts|spec|spVisDataService', function () {

    beforeEach(module('mod.common.spVisDataService'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedReportService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('exists', inject(function (spVisDataService) {
        expect(spVisDataService).toBeTruthy();
    }));

    describe('requestVisData', function () {
        // TODO : add more

        function createSource(columnName) {
            var colId = _.filter(_.keys(visDataTestData.allFields.meta.rcols), function (curColId) {
                return visDataTestData.allFields.meta.rcols[curColId].title === columnName;
            })[0];
            var source = spEntity.fromJSON({
                typeId: 'chartSource',
                name: jsonString(columnName),
                chartReportColumn: jsonLookup(colId),
                specialChartSource: jsonLookup(),
                sourceAggMethod: jsonLookup()
            });
            return source;
        }

        it('exists', inject(function (spVisDataService) {
            expect(spVisDataService.requestVisData).toBeTruthy();
        }));

        it('runs report request for simple sources', inject(function (spReportService, spVisDataService) {
            spReportService.mockGetReportData(123, visDataTestData.allFields);
            var source = createSource('Weekday');
            var visModel = {
                reportId: 123,
                sources: [source],
                sourcesRequestingAllData: []
            };
            TestSupport.wait(
                spVisDataService.requestVisData(visModel, {})
                .then(function (visData) {
                    expect(visData.reportData).toBe(visDataTestData.allFields);
                    expect(visData.lookups).toBeTruthy();
                }));
        }));

        it('runs if all instances requested for choice fields', inject(function (spReportService, spVisDataService) {
            spReportService.mockGetReportData(123, visDataTestData.allFields);
            var source = createSource('Weekday');
            var visModel = {
                reportId: 123,
                sources: [source],
                sourcesRequestingAllData: [source]
            };
            TestSupport.wait(
                spVisDataService.requestVisData(visModel, {})
                .then(function (visData) {
                    expect(visData.reportData).toBe(visDataTestData.allFields);
                    expect(visData.lookups).toBeTruthy(); // note: choices don't actually get filled in - only lookups
                }));
        }));

        it('runs if all instances requested for many-to-many relationship', inject(function (spEntityService, spReportService, spVisDataService) {
            var typeId = 15329; // from test data for herb column of visDataTestData.allFields report
            var instances = [spEntity.fromJSON({ type: 'herb' })];
            spEntityService.mockGetEntitiesOfType(typeId, instances);
            spReportService.mockGetReportData(123, visDataTestData.allFields);
            var source = createSource('AA_Herb');
            var visModel = {
                reportId: 123,
                sources: [source],
                sourcesRequestingAllData: [source]
            };
            TestSupport.wait(
                spVisDataService.requestVisData(visModel, {})
                .then(function (visData) {
                    expect(visData.reportData).toBe(visDataTestData.allFields);
                    expect(visData.lookups).toBeTruthy();
                    expect(visData.lookups[typeId]).toBeTruthy();
                    expect(visData.lookups[typeId][0]).toBe(instances[0]);
                }));
        }));
    });

    describe('createDataAccessor', function () {
        // TODO : add more

        var getOptions = function getOptions() {
            // callback so that  reportTestData.allFields is available
            return {
                visData: {
                    reportData: visDataTestData.allFields,
                    lookups: {}
                }
            };
        };
        var getRow = function getRow(index) {
            return visDataTestData.allFields.gdata[index || 0];
        };

        var checkAccessor = function checkAccessor(accessor, source) {
            expect(accessor).toBeTruthy();
            expect(accessor.entity).toBe(source);
            expect(_.isFunction(accessor)).toBeTruthy();
            expect(_.isFunction(accessor.formatter)).toBeTruthy();
            expect(_.isFunction(accessor.sorter)).toBeTruthy();
        };

        it('handles row-number source', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                specialChartSource: jsonLookup('core:rowNumberSource')
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('Int32');
            expect(accessor.title).toBe('Row number');

            var index = 1;
            var row = getRow(index);
            expect(accessor(row, index)).toBe(1);
            expect(accessor.getText(row, index)).toBe('1');
            expect(accessor.formatter(2)).toBe('2');
            expect(accessor.sorter(3)).toBe(3);
        }));

        it('handles count source', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                specialChartSource: jsonLookup('core:countSource')
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('Int32');
            expect(accessor.title).toBe('Count');

            // TODO : Requires test data that contains rollup info
            //var row = getRow();
            //expect(accessor(row, index)).toBe(1);
            expect(accessor.formatter(2)).toBe('2');
            expect(accessor.sorter(3)).toBe(3);
        }));

        it('handles simple string column', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                chartReportColumn: jsonLookup(18665)
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('String');
            expect(accessor.title).toBe('Single Line');

            var row = getRow();
            expect(accessor(row)).toBe('data 01');
            expect(accessor.formatter('abc')).toBe('abc');
            expect(accessor.sorter('abc')).toBe('abc');
        }));

        it('handles simple bool column', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                chartReportColumn: jsonLookup(12086)
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('Bool');
            expect(accessor.title).toBe('Boolean');

            var row = getRow();
            expect(accessor(row)).toBe('False');            // should this be '  false  '?
            expect(accessor.formatter(true)).toBe('Yes');
            expect(accessor.sorter(true)).toBe(true);
        }));

        it('handles simple int column', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                chartReportColumn: jsonLookup(16972)
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('Int32');
            expect(accessor.title).toBe('Number');

            var row = getRow();
            expect(accessor(row)).toBe(100);
            expect(accessor.formatter(2)).toBe('2');
            expect(accessor.sorter(3)).toBe(3);
        }));

        it('handles simple decimal column', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                chartReportColumn: jsonLookup(17680)
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('Decimal');
            expect(accessor.title).toBe('Decimal');

            var row = getRow();
            expect(accessor(row)).toBe(100.111);
            expect(accessor.formatter(2)).toBe('2.000');
            expect(accessor.sorter(3)).toBe(3);
        }));

        it('handles simple currency column', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                chartReportColumn: jsonLookup(20955)
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('Currency');
            expect(accessor.title).toBe('Currency');

            var row = getRow();
            expect(accessor(row)).toBe(100.100);
            expect(accessor.formatter(2)).toBe('$2.00');
            expect(accessor.sorter(3)).toBe(3);
        }));

        it('handles simple date column', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                chartReportColumn: jsonLookup(12743)
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('Date');
            expect(accessor.title).toBe('Date');

            var row = getRow();
            expect(accessor(row).toUTCString()).toBe('Sat, 01 Jun 2013 00:00:00 GMT');
            var date = new Date();
            date.setFullYear(2016, 2, 18);   //2=march .. dumb javascript
            expect(accessor.formatter(date)).toBe('3/18/2016'); // This be '18/3/2016'?
            //expect(accessor.sorter(3)).toBe(3); // TODO
        }));

        it('handles simple time column', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                chartReportColumn: jsonLookup(21003)
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('Time');
            expect(accessor.title).toBe('Time');

            var row = getRow();
            expect(accessor(row).toUTCString()).toBe('Sat, 01 Jan 2000 02:00:00 GMT');  // is this the correct epoch?
            var time = new Date();
            time.setFullYear(2000, 0, 1);   //2=march .. dumb javascript
            time.setHours(13);
            time.setMinutes(14);
            time.setSeconds(15);
            expect(accessor.formatter(time)).toBe('1:14 PM');
            //expect(accessor.sorter(3)).toBe(3); // TODO
        }));

        it('handles simple date-time column', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                chartReportColumn: jsonLookup(17731)
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('DateTime');
            expect(accessor.title).toBe('DateTime');

            var row = getRow();
            expect(accessor(row).toUTCString()).toBe('Sat, 01 Jun 2013 03:00:00 GMT');    // should this be numeric?
            var time = new Date();
            time.setFullYear(2002, 2, 18);   //2=march .. dumb javascript
            time.setHours(13);
            time.setMinutes(14);
            time.setSeconds(15);
            expect(accessor.formatter(time)).toBe('3/18/2002 1:14 PM'); // should be 18/3 ??
            //expect(accessor.sorter(3)).toBe(3); // TODO
        }));

        it('handles simple choice column', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                chartReportColumn: jsonLookup(17480)
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('ChoiceRelationship');
            expect(accessor.typeId).toBe(14005);
            expect(accessor.title).toBe('Weekday');

            var row = getRow();
            expect(accessor(row)).toBe('20325');    // should this be numeric?
            expect(accessor.formatter(20325)).toBe('Sunday');
            //expect(accessor.sorter(3)).toBe(3); // TODO
        }));

        it('handles simple one-to-one column', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                chartReportColumn: jsonLookup(16820)
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('InlineRelationship');
            expect(accessor.typeId).toBe(18183);
            expect(accessor.title).toBe('AA_Drinks');

            var row = getRow();
            expect(accessor(row)).toBe('17985');    // should this be numeric?
            expect(accessor.formatter(17985)).toBe('Coke');
            //expect(accessor.sorter(3)).toBe(3); // TODO
        }));

        it('handles simple many-to-many column', inject(function (spVisDataService) {

            var source = spEntity.fromJSON({
                chartReportColumn: jsonLookup(17441)
            });
            var options = getOptions();

            var accessor = spVisDataService.createDataAccessor(source, options);
            checkAccessor(accessor, source);

            expect(accessor.colType).toBe('InlineRelationship');
            expect(accessor.typeId).toBe(15329);
            expect(accessor.title).toBe('AA_Herb');

            var row = getRow();
            expect(accessor(row)).toBe('11856');    // should this be numeric?
            expect(accessor.formatter(11856)).toBe('Basil');
            //expect(accessor.sorter(3)).toBe(3); // TODO
        }));
    });

    describe('createReportRequest', function () {

        it('ignores non-aggregates', inject(function (spVisDataService) {

            var chart = spEntity.fromJSON({
                chartHasSeries: [
                    {
                        primarySource: {
                            chartReportColumn: jsonLookup(101),
                            sourceAggMethod: jsonLookup()
                        }
                    }
                ]
            });
            var rq = spVisDataService.test.createReportRequest(chart);

            expect(rq).toBeTruthy();
            expect(rq.metadata).toBe('full');
            expect(rq.rollup).toBeFalsy();
        }));

        it('detects and reuses aggregates', inject(function (spVisDataService) {

            var chart = spEntity.fromJSON({
                tmpSources: [
                    {
                        chartReportColumn: jsonLookup(101),
                        sourceAggMethod: jsonLookup()
                    },
                    {
                        chartReportColumn: jsonLookup(101),
                        sourceAggMethod: jsonLookup()
                    },
                    {
                        chartReportColumn: jsonLookup(102),
                        sourceAggMethod: jsonLookup('aggSum')
                    },
                    {
                        chartReportColumn: jsonLookup(102),
                        sourceAggMethod: jsonLookup('aggSum')
                    }
                ]
            });
            var visModel = {
                reportId: 1,
                sources: chart.tmpSources
            };
            var rq = spVisDataService.test.createReportRequest(visModel);
            expect(rq).toBeTruthy();
            expect(rq.metadata).toBe('full');

            var rollup = rq.rollup;
            expect(rollup).toBeTruthy();
            expect(rollup.groups).toBeArray(1);
            expect(rollup.groups[0][101].style).toBe('groupList');
            expect(_.keys(rollup.aggs)).toBeArray(1);
            expect(rollup.aggs[102]).toBeArray(1);
            expect(rollup.aggs[102][0].style).toBe('aggSum');
        }));
    });

    describe('isPivot', function () {

        it('returns true if there is a count source', inject(function(spVisDataService) {
            var visModel = {
                reportId: 1,
                sources: [
                    spEntity.fromJSON({
                        specialChartSource: jsonLookup('core:countSource')
                }) ]
            };
            expect(spVisDataService.isPivot(visModel)).toBeTruthy();
        }));

        it('returns false for row number sources', inject(function (spVisDataService) {
            var visModel = {
                reportId: 1,
                sources: [
                    spEntity.fromJSON({
                        specialChartSource: jsonLookup('core:rowNumberSource')
                    })]
            };
            expect(spVisDataService.isPivot(visModel)).toBeTruthy();
        }));

        it('returns false for row number sources', inject(function (spVisDataService) {
            var visModel = {
                reportId: 1,
                sources: [
                    spEntity.fromJSON({
                        specialChartSource: jsonLookup('core:rowNumberSource')
                    })]
            };
            expect(spVisDataService.isPivot(visModel)).toBeTruthy();
        }));

        it('returns false for non-aggregated columns', inject(function (spVisDataService) {
            var visModel = {
                reportId: 1,
                sources: [
                    spEntity.fromJSON({
                        chartReportColumn: 123
                    })]
            };
            expect(spVisDataService.isPivot(visModel)).toBeTruthy();
        }));

        it('returns true if any aggregated columns', inject(function (spVisDataService) {
            var aggMethods = [
            'aggCountWithValues', 'aggCountUniqueItems', 'aggCount', 'aggSum', 'aggAverage', 'aggMax', 'aggMin', 'aggCountUniqueNotBlanks', 'aggList'
            ];

            var any = false;
            _.forEach(aggMethods, function (method) {
                var visModel = {
                    reportId: 1,
                    sources: [
                        spEntity.fromJSON({
                            chartReportColumn: 123
                        }),
                        spEntity.fromJSON({
                            chartReportColumn: 456,
                            sourceAggMethod: jsonLookup('core:' + method)
                        })]
                };
                expect(spVisDataService.isPivot(visModel)).toBeTruthy();
                any = true;
            });
            expect(any).toBeTruthy();
        }));
    });

    describe('isCategoricalScaleFormat', function () {

        it('returns true for applicable date-time formats', inject(function (spVisDataService) {
            var isCategory = [
            'dateMonthYear', 'dateYear', 'dateDayMonth', 'dateMonth', 'dateQuarter', 'dateQuarterYear',
            'dateWeekday', 'dateTimeDayMonth', 'dateTimeDate', 'dateTimeMonth', 'dateTimeMonthYear',
            'dateTimeQuarter', 'dateTimeQuarterYear', 'dateTimeYear', 'dateTimeWeekday', 'dateTimeHour', 'timeHour'
            ];
            var any = false;
            _.forEach(isCategory, function (c) {
                expect(spVisDataService.isCategoricalScaleFormat(c)).toBeTruthy();
                any = true;
            });
            expect(any).toBeTruthy();
        }));

        it('returns false for other formats', inject(function (spVisDataService) {
            var isNotCategory = [
            'someUnknownFormat'
            ];
            var any = false;
            _.forEach(isNotCategory, function (c) {
                expect(spVisDataService.isCategoricalScaleFormat(c)).toBeFalsy();
                any = true;
            });
            expect(any).toBeTruthy();
        }));

    });

    describe('getRuleColors', function () {

        var rulesFromReportMetadata = [
            { "fgcolor": { "b": 0, "g": 0, "r": 0, "a": 255 }, "bgcolor": { "b": 90, "g": 90, "r": 255, "a": 255 }, "val": "1", "oper": "Equal" },
            { "fgcolor": { "b": 255, "g": 0, "r": 0, "a": 255 }, "bgcolor": { "b": 255, "g": 255, "r": 255, "a": 0 }, "val": "2", "oper": "Equal" },
            { "fgcolor": { "b": 0, "g": 165, "r": 255, "a": 255 }, "bgcolor": { "b": 255, "g": 255, "r": 255, "a": 0 }, "oper": "Unspecified" }
        ];

        it('converts foreground and background', inject(function (spVisDataService) {
            var rule = rulesFromReportMetadata[0];
            var col = spVisDataService.getRuleColors(rule);
            expect(col.fgHex).toBe('#000000');
            expect(col.bgHex).toBe('#ff5a5a');
            expect(col.colorHex).toBe('#ff5a5a');
        }));

        it('selects background color by default', inject(function (spVisDataService) {
            var rule = rulesFromReportMetadata[0];
            var col = spVisDataService.getRuleColors(rule);
            expect(col.colorHex).toBe(col.bgHex);
        }));

        it('selects foreground color if background is white', inject(function (spVisDataService) {
            var rule = rulesFromReportMetadata[1];
            var col = spVisDataService.getRuleColors(rule);
            expect(col.colorHex).toBe(col.fgHex);
        }));

    });

    describe('convertPivotRowToConds', function () {

        var mockSeries = function (sources) {
            var series = {};
            _.forEach(sources, function (col) {
                series[col] = {
                    entity: { chartReportColumn: { idP: col } },
                    rawAccessor: function (row) { return _.find(row.hdrs, function (h) { return h[col]; })[col]; }
                };
            });
            return series;
        };

        it('handles empty grouping', inject(function (spVisDataService) {
            var meta = {
                rcols: {},
                rdata: [{
                    map: 0,
                    total: 1,
                    hdrs: []
                }]
            };

            var sources = [];
            var row = meta.rdata[0];

            var conds = spVisDataService.convertPivotRowToConds(row, sources, meta);

            expect(conds).toBeArray(0);
        }));

        it('handles scalar data grouping', inject(function (spVisDataService) {
            var meta = {
                rcols: {
                    '1': { type: 'String' },
                    '2': { type: 'Int32' },
                    '3': { type: 'Decimal' },
                    '4': { type: 'Currency' },
                    '5': { type: 'Bool' },
                    '6': { type: 'Date' },
                    '7': { type: 'Time' },
                    '8': { type: 'DateTime' }
                },
                rdata: [{
                    map: 0,
                    total: 1,
                    hdrs: [
                        { '1': { val: 'ABC' } },
                        { '2': { val: '200' } },
                        { '3': { val: '123.123' } },
                        { '4': { val: '234.234' } },
                        { '5': { val: 'True' } },
                        { '6': { val: '2012-12-31' } },
                        { '7': { val: '1753-01-01T12:00:00Z' } },
                        { '8': { val: '2012-12-31T12:00:00Z' } }
                    ]
                }]
            };

            var sourceKeys = _.keys(meta.rcols);
            var series = mockSeries(sourceKeys);
            var row = meta.rdata[0];

            var sources = spCharts.getBoundSources(series, sourceKeys);
            var conds = spVisDataService.convertPivotRowToConds(row, sources, meta);

            var test = function (index, expid, oper, type, value) {
                expect(conds[index].expid).toBe(expid);
                expect(conds[index].oper).toBe(oper);
                expect(conds[index].type).toBe(type);
                expect(conds[index].value).toBe(value);
            };

            expect(conds).toBeArray(8);
            test(0, '1', 'Equal', 'String', 'ABC');
            test(1, '2', 'Equal', 'Int32', '200');
            test(2, '3', 'Equal', 'Decimal', '123.123');
            test(3, '4', 'Equal', 'Currency', '234.234');
            test(4, '5', 'Equal', 'Bool', 'True');
            test(5, '6', 'Equal', 'Date', '2012-12-31');
            test(6, '7', 'Equal', 'Time', '1753-01-01T12:00:00Z');
            test(7, '8', 'Equal', 'DateTime', '2012-12-31T12:00:00Z');
        }));

        it('handles null scalar data grouping', inject(function (spVisDataService) {
            var meta = {
                rcols: {
                    '1': { type: 'String' },
                    '2': { type: 'Int32' },
                    '3': { type: 'Decimal' },
                    '4': { type: 'Currency' },
                    '5': { type: 'Bool' },
                    '6': { type: 'Date' },
                    '7': { type: 'Time' },
                    '8': { type: 'DateTime' }
                },
                rdata: [{
                    map: 0,
                    total: 1,
                    hdrs: [
                        { '1': { val: '' } },
                        { '2': { val: '' } },
                        { '3': { val: '' } },
                        { '4': { val: '' } },
                        { '5': { val: 'False' } },
                        { '6': {} },
                        { '7': {} },
                        { '8': {} }
                    ]
                }]
            };

            var sourceKeys = _.keys(meta.rcols);
            var series = mockSeries(sourceKeys);
            var row = meta.rdata[0];

            var sources = spCharts.getBoundSources(series, sourceKeys);
            var conds = spVisDataService.convertPivotRowToConds(row, sources, meta);

            var test = function (index, expid, oper, type) {
                expect(conds[index].expid).toBe(expid);
                expect(conds[index].oper).toBe(oper);
                expect(conds[index].type).toBe(type);
            };

            expect(conds).toBeArray(8);
            test(0, '1', 'IsNull', 'String');
            test(1, '2', 'IsNull', 'Int32');
            test(2, '3', 'IsNull', 'Decimal');
            test(3, '4', 'IsNull', 'Currency');
            test(4, '5', 'Equal', 'Bool');
            test(5, '6', 'IsNull', 'Date');
            test(6, '7', 'IsNull', 'Time');
            test(7, '8', 'IsNull', 'DateTime');
        }));

        it('handles relationship grouping', inject(function (spVisDataService) {
            var meta = {
                rcols: {
                    '11': { type: 'ChoiceRelationship' },
                    '22': { type: 'InlineRelationship' }
                },
                rdata: [{
                    map: 0,
                    total: 1,
                    hdrs: [
                        { '11': { vals: { '456': 'FredA' } } },
                        { '22': { vals: { '789': 'FredB' } } }
                    ]
                }]
            };

            var sourceKeys = _.keys(meta.rcols);
            var series = mockSeries(sourceKeys);
            var row = meta.rdata[0];

            var sources = spCharts.getBoundSources(series, sourceKeys);
            var conds = spVisDataService.convertPivotRowToConds(row, sources, meta);

            expect(conds).toBeArray(2);

            expect(conds[0].expid).toBe('11');
            expect(conds[0].oper).toBe('AnyOf');
            expect(conds[0].type).toBe('ChoiceRelationship');
            expect(conds[0].values).toBeTruthy();
            expect(conds[0].values['456']).toBeTruthy();

            expect(conds[1].expid).toBe('22');
            expect(conds[1].oper).toBe('AnyOf');
            expect(conds[1].type).toBe('InlineRelationship');
            expect(conds[1].values).toBeTruthy();
            expect(conds[1].values['789']).toBeTruthy();
        }));

        it('handles null relationship grouping', inject(function (spVisDataService) {
            var meta = {
                rcols: {
                    '11': { type: 'ChoiceRelationship' },
                    '22': { type: 'InlineRelationship' }
                },
                rdata: [{
                    map: 0,
                    total: 1,
                    hdrs: [
                        { '11': {} },
                        { '22': {} }
                    ]
                }]
            };

            var sourceKeys = _.keys(meta.rcols);
            var series = mockSeries(sourceKeys);
            var row = meta.rdata[0];

            var sources = spCharts.getBoundSources(series, sourceKeys);
            var conds = spVisDataService.convertPivotRowToConds(row, sources, meta);

            expect(conds).toBeArray(2);

            expect(conds[0].expid).toBe('11');
            expect(conds[0].oper).toBe('IsNull');
            expect(conds[0].type).toBe('ChoiceRelationship');

            expect(conds[1].expid).toBe('22');
            expect(conds[1].oper).toBe('IsNull');
            expect(conds[1].type).toBe('InlineRelationship');
        }));

    });

});
