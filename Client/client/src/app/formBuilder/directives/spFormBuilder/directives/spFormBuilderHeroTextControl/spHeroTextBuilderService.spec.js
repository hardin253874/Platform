// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals chartBuilderTestData */

describe('Form Builder|spec|Hero Text|spHeroTextBuilderService', function () {

    beforeEach(module('mod.common.spHeroTextBuilderService'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedReportService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    //
    // Setup the mocked entity service.
    //
    //beforeEach(inject(function (spEntityService, spReportService) {

    //    spReportService.mockGetReportData(123, chartBuilderTestData.internetAccessReport);
    //}));

    function mockControl() {
        var json = {
            id: 123,
            typeId: 'heroTextControl',
            name: 'My Label',
            heroTextReport: jsonLookup(456),
            heroTextSource: {
                typeId: 'chartSource',
                name: 'Source name',
                chartReportColumn: jsonLookup(),
                specialChartSource: jsonLookup(),
                sourceAggMethod: jsonLookup()
            },
            heroTextStyle: 'style2'
        };
        return spEntity.fromJSON(json);
    }

    //
    // Tests start here
    //

    it('exists', inject(function (spHeroTextBuilderService) {
        expect(spHeroTextBuilderService).toBeTruthy();
    }));

    describe('model', function () {

        it('can create hero text model', inject(function (spHeroTextBuilderService) {

            var heroTextControl = spEntity.fromJSON({});
            var model = spHeroTextBuilderService.createModel(heroTextControl);
            expect(model).toBeTruthy();
            expect(model.control).toBe(heroTextControl);
            expect(model.reportPickerOptions).toBeTruthy();
            expect(model.methods).toBeArray(2);  // 'value' and 'count unique'
        }));

        it('can initialize an unloaded new control', inject(function (spHeroTextBuilderService) {

            var heroTextControl = spEntity.fromJSON({});
            var model = spHeroTextBuilderService.createModel(heroTextControl);

            TestSupport.wait(spHeroTextBuilderService.initModel(model).then(function () {
                expect(model.control.name).toBe('Title');
                expect(model.control.heroTextReport).toBeNull();
                expect(model.control.heroTextSource).toBeNull();
                expect(model.control.heroTextStyle).toBe('style1');
                expect(spHeroTextBuilderService.okEnabled(model)).toBe(false);
            }));
        }));

        it('can initialize a loaded existing control', inject(function (spHeroTextBuilderService, spReportService) {
            var heroTextControl = mockControl();
            spReportService.mockGetReportData(456, chartBuilderTestData.internetAccessReport);

            var model = spHeroTextBuilderService.createModel(heroTextControl);

            TestSupport.wait(spHeroTextBuilderService.initModel(model).then(function () {
                // Control data
                expect(model.control.name).toBe('My Label');
                expect(model.control.heroTextReport.idP).toBe(456);
                expect(model.control.heroTextSource).not.toBeNull();
                expect(model.control.heroTextStyle).toBe('style2');
                expect(spHeroTextBuilderService.okEnabled(model)).toBe(true);
                // Model data
                expect(model.name).toBe('My Label');
                expect(model.style).toBe('style2');
            }));
        }));

        it('can initialize an unloaded existing control', inject(function (spHeroTextBuilderService, spEntityService, spReportService) {
            var heroTextControl = mockControl();
            spEntityService.mockGetEntity(heroTextControl);
            spReportService.mockGetReportData(456, chartBuilderTestData.internetAccessReport);

            var model = spHeroTextBuilderService.createModel(heroTextControl);

            TestSupport.wait(spHeroTextBuilderService.initModel(model).then(function () {
                // Control data
                expect(model.control.name).toBe('My Label');
                expect(model.control.heroTextReport.idP).toBe(456);
                expect(model.control.heroTextSource).not.toBeNull();
                expect(model.control.heroTextStyle).toBe('style2');
                expect(spHeroTextBuilderService.okEnabled(model)).toBe(true);
                // Model data
                expect(model.name).toBe('My Label');
                expect(model.style).toBe('style2');
                expect(model.columns).toBeArray(4);
                expect(model.columns[3].name).toBe('Count');
                expect(model.columns[3].specialChartSource).toBe('core:countSource');
            }));
        }));

        it('handles a selected column', inject(function (spHeroTextBuilderService, spReportService) {
            var heroTextControl = mockControl();
            heroTextControl.heroTextSource.chartReportColumn = 5813; // the 'With Internet' Int32 column in the sample report
            heroTextControl.heroTextSource.sourceAggMethod = 'core:aggMax';
            spReportService.mockGetReportData(456, chartBuilderTestData.internetAccessReport);

            var model = spHeroTextBuilderService.createModel(heroTextControl);

            TestSupport.wait(spHeroTextBuilderService.initModel(model).then(function () {
                // Control data
                expect(model.control.name).toBe('My Label');
                expect(model.control.heroTextReport.idP).toBe(456);
                expect(model.control.heroTextSource).not.toBeNull();
                expect(model.control.heroTextStyle).toBe('style2');
                expect(spHeroTextBuilderService.okEnabled(model)).toBe(true);
                // Model data
                expect(model.name).toBe('My Label');
                expect(model.style).toBe('style2');
                expect(model.columns).toBeArray(4);
                expect(model.columns[1].colId).toBe(5813);
                expect(model.column).toBe(model.columns[1]);
                expect(model.methods).toBeArray(6);
                expect(model.methods[4].alias).toBe('aggMax');
                expect(model.methods[4].name).toBe('Max');
                expect(model.method).toBe(model.methods[4]);
            }));
        }));

        it('handles a selected count', inject(function (spHeroTextBuilderService, spReportService) {
            var heroTextControl = mockControl();
            heroTextControl.heroTextSource.specialChartSource = 'core:countSource';
            spReportService.mockGetReportData(456, chartBuilderTestData.internetAccessReport);

            var model = spHeroTextBuilderService.createModel(heroTextControl);

            TestSupport.wait(spHeroTextBuilderService.initModel(model).then(function () {
                // Control data
                expect(model.control.name).toBe('My Label');
                expect(model.control.heroTextReport.idP).toBe(456);
                expect(model.control.heroTextSource).not.toBeNull();
                expect(model.control.heroTextStyle).toBe('style2');
                // Model data
                expect(model.name).toBe('My Label');
                expect(model.style).toBe('style2');
                expect(model.columns).toBeArray(4);
                expect(model.column).toBe(model.columns[3]);
                expect(model.methods).toBeArray(1);
                expect(model.methods[0].name).toBe('Count');
                expect(model.method).toBe(model.methods[0]);
            }));
        }));
    });

    describe('apply changes', function () {

        it('can apply changes to new control', inject(function (spHeroTextBuilderService, spReportService) {
            var heroTextControl = mockControl();
            heroTextControl.heroTextReport = null;
            spReportService.mockGetReportData(456, chartBuilderTestData.internetAccessReport);

            var model = spHeroTextBuilderService.createModel(heroTextControl);

            TestSupport.wait(spHeroTextBuilderService.initModel(model)
                .then(function() {
                    // Control data
                    expect(model.columns).toBeArray(0);
                    var reportEntity = spEntity.fromJSON({ id: 456, name: 'Internet Report' });
                    model.reportPickerOptions.selectedEntities = [reportEntity];
                    return spHeroTextBuilderService.reportChanged(model);
                })
                .then(function () {
                    expect(model.columns).toBeArray(4);
                    expect(model.column).toBe(model.columns[3]); // select Count by default
                    model.column = model.columns[0];
                    spHeroTextBuilderService.columnChanged(model);
                    expect(model.methods).toBeArray(2);
                    expect(model.methods[1].alias).toBe('aggCountUniqueItems');
                    model.method = model.methods[1];
                    spHeroTextBuilderService.updateSample(model);
                    expect(spHeroTextBuilderService.okEnabled(model)).toBe(true);
                    spHeroTextBuilderService.applyChanges(model);                

                    var control = model.control;
                    expect(control).toBeTruthy();
                    expect(control.heroTextReport.idP).toBe(456);
                    expect(control.heroTextSource.chartReportColumn.idP).toBe(9857);
                    expect(control.heroTextSource.sourceAggMethod.nsAlias).toBe('core:aggCountUniqueItems');
                }));
        }));

        it('can apply changes to existing control - select a column', inject(function (spHeroTextBuilderService, spReportService) {
            var heroTextControl = mockControl();
            spReportService.mockGetReportData(456, chartBuilderTestData.internetAccessReport);

            var model = spHeroTextBuilderService.createModel(heroTextControl);

            TestSupport.wait(spHeroTextBuilderService.initModel(model).then(function () {
                // Control data

                model.name = 'New name';
                model.style = 'style3';
                model.column = model.columns[0];
                spHeroTextBuilderService.columnChanged(model);
                expect(model.methods).toBeArray(2);
                expect(model.methods[1].alias).toBe('aggCountUniqueItems');
                model.method = model.methods[1];
                spHeroTextBuilderService.updateSample(model);
                expect(spHeroTextBuilderService.okEnabled(model)).toBe(true);
                spHeroTextBuilderService.applyChanges(model);                

                var control = model.control;
                expect(control).toBeTruthy();
                expect(model.control.name).toBe('New name');
                expect(model.control.heroTextStyle).toBe('style3');
                expect(control.heroTextReport.idP).toBe(456);
                expect(control.heroTextSource.chartReportColumn.idP).toBe(9857);
                expect(control.heroTextSource.sourceAggMethod.nsAlias).toBe('core:aggCountUniqueItems');
            }));
        }));

        it('can apply changes to existing control - select count', inject(function (spHeroTextBuilderService, spReportService) {
            var heroTextControl = mockControl();
            spReportService.mockGetReportData(456, chartBuilderTestData.internetAccessReport);

            var model = spHeroTextBuilderService.createModel(heroTextControl);

            TestSupport.wait(spHeroTextBuilderService.initModel(model).then(function () {
                // Control data

                model.column = model.columns[3]; // count
                spHeroTextBuilderService.columnChanged(model);
                expect(model.methods).toBeArray(1);
                expect(model.methods[0].name).toBe('Count');
                expect(spHeroTextBuilderService.okEnabled(model)).toBe(true);
                spHeroTextBuilderService.applyChanges(model);

                var control = model.control;
                expect(control).toBeTruthy();
                expect(control.heroTextReport.idP).toBe(456);
                expect(control.heroTextSource.chartReportColumn).toBeNull();
                expect(control.heroTextSource.sourceAggMethod).toBeNull();
                expect(control.heroTextSource.specialChartSource.nsAlias).toBe('core:countSource');
            }));
        }));
    });

});
