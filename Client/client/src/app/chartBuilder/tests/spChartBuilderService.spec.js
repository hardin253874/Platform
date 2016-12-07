// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals chartBuilderTestData */

describe('Charts|Builder|spec|spChartBuilderService', function () {

    var chartId = 1;

    beforeEach(module('mod.common.ui.spChartService'));
    beforeEach(module('mod.app.chartBuilder.services.spChartBuilderService'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedReportService'));
    beforeEach(module('sp.app.navigation'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    //
    // Setup the mocked entity service.
    //
    beforeEach(inject(function (spEntityService, spReportService) {

        var json = {
            id: chartId,
            typeId: 'core:chart',
            name: 'Test Chart',
            description: 'Test chart desc'
        };

        spEntityService.mockGetEntityJSON(json);

        spReportService.mockGetReportData(123, chartBuilderTestData.internetAccessReport);
    }));


    //
    // Tests start here
    //

    it('exists', inject(function (spChartBuilderService) {
        expect(spChartBuilderService).toBeTruthy();
    }));

    describe('model', function () {

        it('can create chart model', inject(function (spChartBuilderService) {

            var model = spChartBuilderService.createChartModel();
            expect(model).toBeTruthy();

            var chart = model.chart;
            expect(chart).toBeTruthy();
            expect(chart.name).toBe('New Chart');
        }));

        it('can create chart model with report ID', inject(function (spChartBuilderService) {

            var options = { reportId: 123 };
            var model = spChartBuilderService.createChartModel(options);
            expect(model.chart.chartReport.idP).toBe(123);
        }));

        it('can load chart model', inject(function (spChartBuilderService, spEntityService) {

            spyOn(spEntityService, 'getEntity').andCallThrough();

            var options = { reportId: 123 };
            TestSupport.wait(
                spChartBuilderService.loadChartModel(chartId)
                .then(function (model) {
                    expect(spEntityService.getEntity).toHaveBeenCalled();
                    expect(model).toBeTruthy();

                    var chart = model.chart;
                    expect(chart).toBeTruthy();
                    expect(chart.name).toBe('Test Chart');
                }));
        }));

        it('can reload chart model', inject(function (spChartBuilderService, spEntityService) {

            var options = { reportId: 123 };
            var model;

            spyOn(spEntityService, 'getEntity').andCallThrough();

            TestSupport.wait(
                spChartBuilderService.loadChartModel(chartId)
                .then(function(model1) {
                    model = model1;
                    expect(spEntityService.getEntity).toHaveBeenCalled();
                    spChartBuilderService.reloadChart(model);
                })
                .then(function () {
                    expect(spEntityService.getEntity).toHaveBeenCalled();
                    expect(model).toBeTruthy();

                    var chart = model.chart;
                    expect(chart).toBeTruthy();
                    expect(chart.name).toBe('Test Chart');
                }));
        }));

        it('can save chart model', inject(function ($q, spChartBuilderService, spEntityService) {

            var model = spChartBuilderService.createChartModel();

            spyOn(spEntityService, 'putEntity').andReturn($q.when(123));

            TestSupport.wait(
                spChartBuilderService.saveChart(model)
                .then(function (id) {
                    expect(id).toBe(123);
                }));
        }));

        it('can create temporary model', inject(function (spChartBuilderService) {
            var model = spChartBuilderService.tempModel();
            expect(model).toBeTruthy();
            expect(model.windowBusy).toBeTruthy();
            expect(model.windowBusy.isBusy).toBeTruthy();
        }));

        it('can create temporary model', inject(function (spChartBuilderService) {
            var model = spChartBuilderService.tempModel();
            expect(model).toBeTruthy();
            expect(model.windowBusy).toBeTruthy();
            expect(model.windowBusy.isBusy).toBeTruthy();
        }));

    });

    describe('change detection', function () {
        it('undo works', inject(function (spChartBuilderService) {

            var model = spChartBuilderService.createChartModel();
            spChartBuilderService.applyChange(model, function () {
                model.chart.name = 'hello';
            });

            spChartBuilderService.undo(model);
            expect(model.chart.name).toBe('New Chart');
        }));

        it('redo works', inject(function (spChartBuilderService) {

            var model = spChartBuilderService.createChartModel();
            spChartBuilderService.applyChange(model, function () {
                model.chart.name = 'hello';
            });

            spChartBuilderService.undo(model);
            expect(model.chart.name).toBe('New Chart');

            spChartBuilderService.redo(model);
            expect(model.chart.name).toBe('hello');
        }));

        it('isDirty returns false if no changes', inject(function (spChartBuilderService) {

            var model = spChartBuilderService.createChartModel();
            expect(spChartBuilderService.isDirty(model)).toBeFalsy();
        }));

        it('isDirty detects changes', inject(function (spChartBuilderService) {

            var model = spChartBuilderService.createChartModel();
            model.chart.name = 'A new name';
            expect(spChartBuilderService.isDirty(model)).toBeTruthy();
        }));

        it('dirtyMessage returns nothing if no changes', inject(function (spChartBuilderService) {

            var model = spChartBuilderService.createChartModel();
            expect(spChartBuilderService.dirtyMessage(model)).toBeFalsy();
        }));

        it('dirtyMessage reports changes', inject(function (spChartBuilderService, navDirtyMessage) {

            var model = spChartBuilderService.createChartModel();
            model.chart.name = 'A new name';
            expect(spChartBuilderService.dirtyMessage(model)).toBe(navDirtyMessage.defaultMsg);
        }));
    });

    describe('report metadata', function () {

        it('loadReportMetadata handles no reportId', inject(function (spChartBuilderService) {

            var model = spChartBuilderService.createChartModel();
            model.chart.chartReport = 0;

            TestSupport.wait(
                spChartBuilderService.loadReportMetadata(model)
                .then(function () {
                    expect(model.reportMetadata).toBe(null);
                }));
        }));

        it('loadReportMetadata runs', inject(function(spChartBuilderService, spReportService) {

            spyOn(spReportService, 'getReportData').andCallThrough();

            var model = spChartBuilderService.createChartModel();
            model.chart.chartReport = 123;
            model.chart.chartReport.name = 'not set';

            TestSupport.wait(
                spChartBuilderService.loadReportMetadata(model)
                .then(function () {
                    expect(spReportService.getReportData).toHaveBeenCalled();
                    expect(model.reportMetadata).toBeTruthy();
                    expect(model.chart.chartReport.name).toBe('Internet Access');
                }));
        }));

        it('getAvailableColumnSources returns columns', inject(function (spChartBuilderService) {
            var model = {
                reportMetadata: chartBuilderTestData.internetAccessReport.meta
            };

            var res = spChartBuilderService.getAvailableColumnSources(model);
            expect(res).toBeTruthy();
            expect(res).toBeArray(5);

            var first = res[0];
            expect(first.name).toBe('State');
            expect(first.colId).toBe(9857);
            expect(first.specialChartSource).toBeNull();
            expect(first.type).toBe('String');
            expect(first.getDrag).toBeTruthy();
            expect(first.getDrag()).toBe(first);

            var count = res[3];
            expect(count.name).toBe('Count');
            expect(count.colId).toBe(null);
            expect(count.specialChartSource).toBe('core:countSource');
            expect(count.type).toBe('Int32');
            expect(count.getDrag).toBeTruthy();
            expect(count.getDrag()).toBe(count);

            var last = res[4];
            expect(last.name).toBe('Row Number');
            expect(last.colId).toBe(null);
            expect(last.specialChartSource).toBe('core:rowNumberSource');
            expect(last.type).toBe('Int32');
            expect(last.getDrag).toBeTruthy();
            expect(last.getDrag()).toBe(last);
        }));

        it('getAvailableColumnSources returns empty if no metadata', inject(function (spChartBuilderService) {
            expect(spChartBuilderService.getAvailableColumnSources(null)).toBeArray(0);
            expect(spChartBuilderService.getAvailableColumnSources({})).toBeArray(0);
        }));

        it('applySuggestions runs', inject(function (spChartBuilderService) {
            var model = spChartBuilderService.createChartModel();
            model.reportMetadata = chartBuilderTestData.internetAccessReport.meta;
            spChartBuilderService.applySuggestions(model);
        }));
    });

    describe('chart sources', function () {

        it('getSourceInfo works', inject(function (spChartService, spChartBuilderService) {

            // TODO: Use a predefined model instead of using drag-drop
            var model = spChartBuilderService.createChartModel();
            model.reportMetadata = chartBuilderTestData.internetAccessReport.meta;

            // Drag a source
            var sources = spChartBuilderService.getAvailableColumnSources(model);
            var source = _.find(sources, { name: 'State' });
            var colId = source.colId;
            var dragData = source.getDrag;

            // Drop on primary
            var primarySource = spChartService.getChartSourceType('primarySource');
            var series = _.first(model.chart.chartHasSeries);
            var dropData = spChartBuilderService.getCstDropData(model, series, primarySource);

            // Do drag-drop
            spChartBuilderService.doDragDrop(dragData, dropData);

            // Check result
            expect(series.primarySource.chartReportColumn.idP).toBe(colId);
            // END TODO

            var info = spChartBuilderService.getSourceInfo(model, series, primarySource);
            expect(info.name).toBe('State');
            expect(info.colId).toBe(colId);
            expect(info.specialChartSource).toBeNull();
            expect(info.type).toBe('String');
            expect(_.isFunction(info.getDrag)).toBeTruthy();
            var drag1 = info.getDrag();
            expect(drag1).toBeTruthy();
        }));
    });

    describe('series', function () {

        it('getSeriesName works if series name is not set', inject(function (spChartBuilderService) {
            var model = spChartBuilderService.createChartModel();
            var series = _.first(model.chart.chartHasSeries);
            series.name = null;
            series.chartType = 'columnChart';

            var name = spChartBuilderService.getSeriesName(model, series);
            expect(name).toBe('Column');
        }));

        it('getSeriesName works if series name is set', inject(function (spChartBuilderService) {
            var model = spChartBuilderService.createChartModel();
            var series = _.first(model.chart.chartHasSeries);
            series.name = 'Test Series';
            var name = spChartBuilderService.getSeriesName(model, series);
            expect(name).toBe('Test Series');
        }));

        it('getSeriesName bails for null series', inject(function (spChartBuilderService) {
            var model = spChartBuilderService.createChartModel();
            var name = spChartBuilderService.getSeriesName(model, null);
            expect(name).toBe('');
        }));

        it('setSeriesName sets a value', inject(function (spChartBuilderService) {
            var model = spChartBuilderService.createChartModel();
            var series = _.first(model.chart.chartHasSeries);
            spChartBuilderService.setSeriesName(model, series, 'Whatever');
            expect(series.name).toBe('Whatever');
        }));

        it('setSeriesName reverts to default', inject(function (spChartBuilderService) {
            var model = spChartBuilderService.createChartModel();
            var series = _.first(model.chart.chartHasSeries);
            series.name = null;
            series.chartType = 'columnChart';

            spChartBuilderService.setSeriesName(model, series, 'Whatever');
            expect(series.name).toBe('Whatever');
            spChartBuilderService.setSeriesName(model, series, 'Column');
            expect(series.name).toBe(null);
        }));
    });

    describe('drag-drop', function () {

        var model;
        var colId;

        it('drag from report to primary source', inject(function (spChartService, spChartBuilderService) {
            model = spChartBuilderService.createChartModel();
            model.reportMetadata = chartBuilderTestData.internetAccessReport.meta;

            // Drag a source
            var sources = spChartBuilderService.getAvailableColumnSources(model);
            var source = _.find(sources, { name: 'State' });
            colId = source.colId;
            var dragData = source.getDrag;

            // Drop on primary
            var primarySource = spChartService.getChartSourceType('primarySource');
            var series = _.first(model.chart.chartHasSeries);
            var dropData = spChartBuilderService.getCstDropData(model, series, primarySource);

            // Do drag-drop
            spChartBuilderService.doDragDrop(dragData, dropData);

            // Check result
            expect(series.primarySource.chartReportColumn.idP).toBe(colId);
        }));

        it('then drag from primary source to value source', inject(function (spChartService, spChartBuilderService) {

            var series = _.first(model.chart.chartHasSeries);

            // Drag primary source
            var primarySource = spChartService.getChartSourceType('primarySource');
            var dragData = spChartBuilderService.getCstDragData(model, series, primarySource);

            // Drop on value source
            var valueSource = spChartService.getChartSourceType('valueSource');
            var dropData = spChartBuilderService.getCstDropData(model, series, valueSource);

            // Do drag-drop
            spChartBuilderService.doDragDrop(dragData, dropData);

            // Check result
            expect(series.valueSource.chartReportColumn.idP).toBe(colId);
        }));

        it('then drag from primary source to nowhere', inject(function (spChartService, spChartBuilderService) {

            var series = _.first(model.chart.chartHasSeries);

            // Drag primary source
            var primarySource = spChartService.getChartSourceType('primarySource');
            var dragData = spChartBuilderService.getCstDragData(model, series, primarySource);

            // Drop on value source
            var modelCallback = _.constant(model);
            var dropData = spChartBuilderService.getBackgroundDropData(modelCallback);

            // Do drag-drop
            spChartBuilderService.doDragDrop(dragData, dropData);

            // Check result
            expect(series.primarySource).toBeNull();
        }));
    });

    describe('getDropOptions', function () {

        var ran = false;
        var testDragData = function() {
            return {
                canDelete: true,
                delete: function () { ran = true; }
            };
        };

        it('runs', inject(function (spChartBuilderService) {
            var options = spChartBuilderService.getDropOptions();
            expect(options).toBeTruthy();
            expect(options.onDrop).toBeTruthy();
            expect(options.onAllowDrop).toBeTruthy();
        }));

        //it('onAllowDrop works', inject(function () {
        //    var options = spChartBuilderService.getDropOptions();
        //    expect(options.onAllowDrop(null, null, testDragData, null)).toBeTruthy();
        //}));

        //it('onDrop works', inject(function () {
        //    var options = spChartBuilderService.getDropOptions();
        //    options.onDrop(null, null, null, testDragData, null);
        //    expect(ran).toBeTruthy();
        //}));
    });

    describe('refreshChart', function () {

        it('triggers change in refresh property', inject(function (spChartBuilderService) {

            var model = spChartBuilderService.createChartModel();
            var refresh1 = model.refresh;
            spChartBuilderService.refreshChart(model);
            expect(model.refresh !== refresh1).toBeTruthy();
            expect(model.refresh).toBeTruthy();
        }));
    });

    describe('aggregates', function () {

        it('getAggregateMethods works for numerics', inject(function (spChartBuilderService) {
            var size = 6;
            expect(spChartBuilderService.getAggregateMethods(spEntity.DataType.Int32)).toBeArray(size);
            expect(spChartBuilderService.getAggregateMethods(spEntity.DataType.Decimal)).toBeArray(size);
            expect(spChartBuilderService.getAggregateMethods(spEntity.DataType.Currency)).toBeArray(size);
        }));

        it('getAggregateMethods works for range types', inject(function (spChartBuilderService) {
            var size = 4;
            expect(spChartBuilderService.getAggregateMethods(spEntity.DataType.Date)).toBeArray(size);
            expect(spChartBuilderService.getAggregateMethods(spEntity.DataType.Time)).toBeArray(size);
            expect(spChartBuilderService.getAggregateMethods(spEntity.DataType.DateTime)).toBeArray(size);
            expect(spChartBuilderService.getAggregateMethods('ChoiceRelationship')).toBeArray(size);
        }));

        it('getAggregateMethods works for other types', inject(function (spChartBuilderService) {
            var size = 2;
            expect(spChartBuilderService.getAggregateMethods(spEntity.DataType.String)).toBeArray(size);
            expect(spChartBuilderService.getAggregateMethods('InlineRelationship')).toBeArray(size);
        }));
    });

});
