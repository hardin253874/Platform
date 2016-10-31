// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spreadsheetInfoTestData */

describe('Import Files|spec|services|spImportFileWebService', function () {

    beforeEach(module('mod.app.importFile.services.spImportFileWebService'));
    //beforeEach(module('mockedEntityService'));
    //beforeEach(module('mockedReportService'));
    //beforeEach(module('sp.app.navigation'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    //
    // Setup the mocked entity service.
    //
    //beforeEach(inject(function (spEntityService, spReportService) {

    //    var json = {
    //        id: chartId,
    //        typeId: 'core:chart',
    //        name: 'Test Chart',
    //        description: 'Test chart desc'
    //    };

    //    spEntityService.mockGetEntityJSON(json);

    //    spReportService.mockGetReportData(123, chartBuilderTestData.internetAccessReport);
    //}));


    //
    // Tests start here
    //

    it('exists', inject(function (spImportFileWebService) {
        expect(spImportFileWebService).toBeTruthy();
    }));

    describe('getSpreadsheetInfo', function() {
        it('runs', inject(function (spImportFileWebService, $httpBackend) {
            $httpBackend.whenGET('/spapi/data/v2/importSpreadsheet/sheet?fileId=myfileuploadid&fileFormat=Excel&fileName=MyFileName.xlsx&sheet=Sheet1').respond(spreadsheetInfoTestData.spreadsheetData);

            TestSupport.wait(
                spImportFileWebService.getSpreadsheetInfo('myfileuploadid', 'Excel', 'MyFileName.xlsx', 'Sheet1')
                .then(function (result) {
                    expect(JSON.stringify(result)).toBe(JSON.stringify(spreadsheetInfoTestData.spreadsheetData));
                }),
                { customFlush: $httpBackend.flush });
        }));
    });

    describe('getSampleTable', function () {
        it('runs', inject(function (spImportFileWebService, $httpBackend) {
            $httpBackend.whenGET('/spapi/data/v2/importSpreadsheet/sample?hrow=1&drow=2&last=&fileId=myfileuploadid&fileFormat=Excel&sheet=Sheet1').respond( spreadsheetInfoTestData.sampleData );

            TestSupport.wait(
                spImportFileWebService.getSampleTable('myfileuploadid', 1, 2, null, 'Excel', 'Sheet1')
                .then(function (result) {
                    expect(JSON.stringify(result)).toBe(JSON.stringify(spreadsheetInfoTestData.sampleData));
                }),
                { customFlush: $httpBackend.flush });
        }));
    });

});
