// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spreadsheetInfoTestData */

describe('Import XML|spec|services|spImportXml', function () {

    var testImportConfigId = 1;
    var allFieldTypeId = 18751;

    beforeEach(module('ng'));
    beforeEach(module('mod.app.importXml.services.spImportXml'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    //
    // Tests start here
    //

    it('exists', inject(function (spImportXml) {
        expect(spImportXml).toBeTruthy();
    }));

    describe('service', function () {

        it('createModel', inject(function (spImportXml) {
            var model = spImportXml.createModel();
            expect(model).toBeTruthy();
            expect(model.onFileUploadComplete).toBeTruthy();
        }));

        it('file upload', inject(function (spImportXml, $httpBackend) {
            var model = spImportXml.createModel();
            var testResult = { "entities": [{ "typeName": "Report", "name": "Another report" }] };
            $httpBackend.whenGET('/spapi/data/v2/importXml?fileId=abcd&fileName=Another+report.xml').respond(testResult);
            $httpBackend.whenGET('/spapi/data/v1/console/tree').respond({});

            TestSupport.wait(
                model.onFileUploadComplete('Another report.xml', 'abcd')
                .then(function (result) {
                    expect(result).toBeTruthy();
                    expect(result.entities).toBeArray(1);
                    expect(result.entities[0].name).toBe('Another report');
                    expect(result.entities[0].typeName).toBe('Report');
                }),
                { customFlush: $httpBackend.flush });
        }));
    });

});
