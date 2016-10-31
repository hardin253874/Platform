// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spreadsheetInfoTestData */

describe('Import Files|spec|services|spImportFileService', function () {

    var testImportConfigId = 1;
    var allFieldTypeId = 18751;

    beforeEach(module('ng'));
    beforeEach(module('mod.app.importFile.services.spImportFileWebService'));
    beforeEach(module('mod.app.importFile.services.spImportFileService'));
    beforeEach(module('mockedEntityService'));
    //beforeEach(module('sp.app.navigation'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    //
    // Setup the mocked entity service.
    //
    beforeEach(inject(function (spEntityService) {

        spEntityService.mockGetEntityJSON({
            id: testImportConfigId,
            typeId: 'core:importConfig',
            name: 'Test Config',
            importFileType: jsonLookup('core:importFileTypeExcel'),
            importConfigMapping: {
                typeId: 'core:apiResourceMapping',
                importHeadingRow: 2,
                importDataRow: 4,
                mappingSourceReference: jsonString('Department'),
                mappedType: {
                    id: allFieldTypeId,
                    name: 'All Fields'
                },
                resourceMemberMappings: [
                    {
                        name: 'A', // column
                        typeId: 'core:apiFieldMapping',
                        mappedField: jsonLookup(21244) // 21244 = 'Date' field
                    }
                ]
            }
        });
    }));

    /// Helpers
    function getAllFieldType() {
        return spreadsheetInfoTestData.getAllFieldTypeEntity();
    }

    //
    // Tests start here
    //

    it('exists', inject(function (spImportFileService) {
        expect(spImportFileService).toBeTruthy();
    }));

    describe('walk through', function () {

        it('new configuration', inject(function (spImportFileService, $httpBackend, $q, spEntityService) {

            // Mock server calls
            var importRunId = 12345;
            var allFields = getAllFieldType();
            var mockStatus = {
                importStatus: 'success',
                importMessages: null,
                recordsSucceeded: 1,
                recordsFailed: 0,
                recordsTotal: 1
            };
            spEntityService.mockGetEntity(allFields);
            $httpBackend.whenGET('/spapi/data/v2/importSpreadsheet/sheet?fileId=myfileuploadid&fileFormat=Excel&fileName=MyFileName.xlsx&sheet=').respond(200, spreadsheetInfoTestData.spreadsheetData);
            $httpBackend.whenGET('/spapi/data/v2/importSpreadsheet/import?config=123&file=myfileuploadid&filename=MyFileName.xlsx&testrun=true').respond(200, importRunId);
            $httpBackend.whenGET('/spapi/data/v2/importSpreadsheet/import/12345').respond(200, mockStatus);
            
            spyOn(spEntityService, 'putEntity').andReturn($q.when(123));

            var model = null;
            var importConfig = null;

            TestSupport.wait(
                // 1. Create model
                spImportFileService.createModel(0)
                .then(function (modelResult) {
                    model = modelResult;
                    expect(model).toBeTruthy();
                    importConfig = model.importConfig;
                    expect(importConfig).toBeTruthy(); //object
                    expect(model.sheets).toBeArray(0);
                    expect(spImportFileService.isPageValid(model)).toBeFalsy();
                })
                .then(function () {
                    // 2. File uploaded
                    return model.upload.onFileUploadComplete('MyFileName.xlsx', 'myfileuploadid');
                })
                .then(function () {
                    expect(model.sheets).toBeArray(2);
                    expect(spImportFileService.isPageValid(model)).toBeTruthy();

                    // Move to page 2
                    model.nav.curPage += 1;
                    expect(spImportFileService.isPageValid(model)).toBeFalsy();

                    // 3. select type
                    var type = spEntity.fromJSON({ id: allFieldTypeId, name: 'All Fields' });
                    return spImportFileService.setSelectedType(model, type);
                })
                .then(function () {
                    return spImportFileService.prepareMappings(model);
                })
                .then(function () {
                    expect(model.importConfig.importConfigMapping.mappedType.idP).toBe(allFields.idP);
                    expect(spImportFileService.isPageValid(model)).toBeTruthy();
                    expect(model.selectedResourceName).toBe('All Fields');
                    expect(model.targetMembers.length > 0).toBeTruthy();

                    // Move to page 3
                    model.nav.curPage += 1;
                    expect(spImportFileService.isPageValid(model)).toBeTruthy(); // because name column gets matched
                    var mappingRow = model.mappingTable[0];

                    // Check available fields
                    var someTarget = model.targetMembers[0];
                    expect(someTarget.memberName).toBe('Date');
                    expect(someTarget.fieldGroup).toBe('Date Fields');

                    // Check mapping row
                    expect(mappingRow.colId).toBe('A');
                    expect(mappingRow.spreadsheetColumnName).toBe('(A) Name');
                    expect(mappingRow.sample1).toBe('Adam');
                    expect(mappingRow.sample2).toBe('Betty');
                    expect(mappingRow.targetDesc).toBe('String Field');   // 'Name' column automatically matched to 'Name' field
                    expect(mappingRow.targetMember.memberName).toBe('Name');
                    //expect(spImportFileService.isPageValid(model)).toBeFalsy();

                    // Select some target fields
                    mappingRow.targetMember = someTarget;
                    mappingRow.handleChange();
                    mappingRow = model.mappingTable[0]; // refetch, because it refreshes

                    // Check mapping row selection and model
                    expect(spImportFileService.isPageValid(model)).toBeTruthy();
                    expect(mappingRow.targetDesc).toBe('Date Field');
                    expect(mappingRow.targetMember.memberName).toBe('Date');
                    expect(mappingRow.targetMember.fieldGroup).toBe('Date Fields');
                    expect(model.importConfig.importConfigMapping.resourceMemberMappings.length).toBe(1);
                    expect(model.importConfig.importConfigMapping.resourceMemberMappings[0].mappedField.idP).toBe(21244);   // 21244= date field id

                    // Mark workflows for suppression
                    model.options.suppressWorkflows = true;
                    model.options.testRun = true;

                    // Start import
                    return spImportFileService.startImport(model, true);
                })
                .then(function () {
                    // importConfig contains value just prior to save
                    expect(spEntityService.putEntity).toHaveBeenCalledWith(importConfig);
                    expect(importConfig.importFileType.nsAlias).toBe('core:importFileTypeExcel');
                    var mapping = importConfig.importConfigMapping;
                    expect(mapping.importHeadingRow).toBe(1);
                    expect(mapping.importDataRow).toBe(2);
                    expect(mapping.mappingSuppressWorkflows).toBe(true);

                    expect(model.importRunId).toBe(importRunId);
                    expect(model.importStatus.importStatus).toBe('success');
                    expect(model.importStatus.recordsSucceeded).toBe(1);
                    expect(model.importStatus.recordsFailed).toBe(0);
                    expect(model.importStatus.recordsTotal).toBe(1);
                    expect(model.importPercent).toBe(100);
                    expect(model.importSummary).toBe('Spreadsheet import completed');
                    expect(model.importMessage).toBe('1 of 1 records successfully verified.');
                }),
                { customFlush: $httpBackend.flush });
        }));

        it('existing configuration', inject(function (spImportFileService, $httpBackend, $q, spEntityService) {

            // Mock server calls
            var allFields = getAllFieldType();
            spEntityService.mockGetEntity(allFields);
            $httpBackend.whenGET('/spapi/data/v2/importSpreadsheet/sheet?fileId=myfileuploadid&fileFormat=Excel&fileName=MyFileName.xlsx&sheet=Department').respond(spreadsheetInfoTestData.spreadsheetData);
            //spyOn(spEntityService, 'putEntity').andReturn($q.when(123));

            var model = null;
            var importConfig = null;

            TestSupport.wait(
                // 1. Load model
                spImportFileService.createModel(testImportConfigId)
                .then(function (modelResult) {
                    model = modelResult;
                    expect(model).toBeTruthy();
                    expect(model.headingRow).toBe(2);
                    expect(model.dataRow).toBe(4);
                    importConfig = model.importConfig;
                    expect(importConfig).toBeTruthy(); //object
                    expect(spImportFileService.isPageValid(model)).toBeFalsy(); // because file is not uploaded yet
                    expect(model.sheetName).toBe('Department');
                    expect(model.sheets).toBeArray(1);
                    expect(model.sheets[0].sheetId).toBe('Department');
                    expect(model.sheets[0].sheetName).toBe('Department');
                })
                .then(function () {
                    // 2. File uploaded
                    return model.upload.onFileUploadComplete('MyFileName.xlsx', 'myfileuploadid');
                })
                .then(function () {
                    expect(model.sheets).toBeArray(2);
                    expect(spImportFileService.isPageValid(model)).toBeTruthy();
                    
                    // Move to page 2
                    model.nav.curPage += 1;
                    expect(model.typePickerOptions.selectedEntities).toBeArray(1);
                    expect(model.typePickerOptions.selectedEntities[0].idP).toBe(allFields.idP);
                    expect(model.typePickerOptions.selectedEntities[0].name).toBe('All Fields');
                    expect(spImportFileService.isPageValid(model)).toBeTruthy(); 
                    return spImportFileService.prepareMappings(model);
                })
                .then(function () {
                    expect(model.importConfig.importConfigMapping.mappedType.idP).toBe(allFields.idP);
                    expect(model.selectedResourceName).toBe('All Fields');
                    expect(model.targetMembers.length > 0).toBeTruthy();

                    // Move to page 3
                    model.nav.curPage += 1;
                    expect(spImportFileService.isPageValid(model)).toBeTruthy();
                    var mappingRow = model.mappingTable[0];

                    // Check available fields
                    var someTarget = model.targetMembers[0];
                    expect(someTarget.memberName).toBe('Date');
                    expect(someTarget.fieldGroup).toBe('Date Fields');

                    // Check mapping row
                    expect(mappingRow.colId).toBe('A');
                    expect(mappingRow.spreadsheetColumnName).toBe('(A) Name');
                    expect(mappingRow.sample1).toBe('Adam');
                    expect(mappingRow.sample2).toBe('Betty');
                    expect(mappingRow.targetDesc).toBe('Date Field');
                    expect(mappingRow.targetMember.memberName).toBe('Date');
                    expect(mappingRow.targetMember.fieldGroup).toBe('Date Fields');

                    // Start import
                    //return spImportFileService.startImport(model);
                })
                .then(function () {
                    //expect(spEntityService.putEntity).toHaveBeenCalledWith(importConfig);
                }),
                { customFlush: $httpBackend.flush });
        }));
    });

    describe('createModel', function () {

        it('handles create', inject(function (spImportFileService) {
            TestSupport.wait(
                spImportFileService.createModel(0).then(function (model) {
                    expect(model).toBeTruthy();
                    var importConfig = model.importConfig;
                    expect(importConfig).toBeTruthy(); //object
                    expect(importConfig.importConfigMapping).toBeTruthy(); //object
                    expect(importConfig.importConfigMapping.resourceMemberMappings).toBeArray();
                    expect(model.showSheetSettings).toBe(false);    // initially hide settings for new configs
                })
            );

        }));

        it('handles load', inject(function (spImportFileService) {
            TestSupport.wait(
                spImportFileService.createModel(testImportConfigId).then(function (model) {
                    expect(model).toBeTruthy();
                    var importConfig = model.importConfig;
                    expect(importConfig).toBeTruthy(); //object
                    expect(importConfig.importConfigMapping).toBeTruthy(); //object
                    expect(importConfig.importConfigMapping.resourceMemberMappings).toBeArray();
                    expect(model.showSheetSettings).toBe(true);    // initially show settings for new configs
                })
            );
        }));

    });

    describe('getFileFormat', function () {

        it('handles csv', inject(function (spImportFileService) {
            var fileType = spImportFileService.test.getFileFormat('test1.csv');
            expect(fileType).toBe('Csv');
        }));

        it('handles Excel', inject(function (spImportFileService) {
            var fileType = spImportFileService.test.getFileFormat('test1.xlsx');
            expect(fileType).toBe('Excel');
        }));

        it('handles others', inject(function (spImportFileService) {
            var fileType = spImportFileService.test.getFileFormat('test1.xls');
            expect(fileType).toBe(null);
        }));

        it('handles no file extension', inject(function (spImportFileService) {
            var fileType = spImportFileService.test.getFileFormat('test1');
            expect(fileType).toBe(null);
        }));

        it('handles empty', inject(function (spImportFileService) {
            var fileType = spImportFileService.test.getFileFormat('');
            expect(fileType).toBe(null);
        }));
    });

    describe('can page advance', function () {

        it('page one can advance if document is loaded', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.nav.curPage = 1;
            model.isDocumentReady = true;
            expect(spImportFileService.isPageValid(model)).toBe(true);
        }));

        it('page one cannot advance if document is not loaded', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.nav.curPage = 1;
            expect(spImportFileService.isPageValid(model)).toBe(false);
        }));
    });

    describe('row numbers', function () {

        it('defaults to 1 and 2', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            expect(model.noHeadingRow).toBe(false);
            expect(model.headingRow).toBe(1);
            expect(model.dataRow).toBe(2);
        }));

        it('increasing data row leaves heading row unchanged', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.dataRow = 3;
            spImportFileService.dataRowChanged(model);
            expect(model.noHeadingRow).toBe(false);
            expect(model.headingRow).toBe(1);
            expect(model.dataRow).toBe(3);
        }));

        it('decreasing data row to equal heading row moves heading row up', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.headingRow = 3;
            spImportFileService.headingRowChanged(model);
            model.dataRow = 3;
            spImportFileService.dataRowChanged(model);
            expect(model.noHeadingRow).toBe(false);
            expect(model.headingRow).toBe(2);
            expect(model.dataRow).toBe(3);
        }));

        it('decreasing data row to above heading row moves heading row up', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.headingRow = 3;
            spImportFileService.headingRowChanged(model);
            model.dataRow = 2;
            spImportFileService.dataRowChanged(model);
            expect(model.noHeadingRow).toBe(false);
            expect(model.headingRow).toBe(1);
            expect(model.dataRow).toBe(2);
        }));

        it('decreasing data row to 1 hides heading row', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.dataRow = 1;
            spImportFileService.dataRowChanged(model);
            expect(model.noHeadingRow).toBe(true);
            expect(model.headingRow).toBe(0);
            expect(model.dataRow).toBe(1);
        }));

        it('decreasing heading row leaves data row unchanged', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.headingRow = 2;
            model.dataRow = 3;
            spImportFileService.headingRowChanged(model);
            expect(model.noHeadingRow).toBe(false);
            expect(model.headingRow).toBe(2);
            expect(model.dataRow).toBe(3);
        }));

        it('increasing heading row to equal data row moves data row down', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.headingRow = 2;
            spImportFileService.headingRowChanged(model);
            expect(model.noHeadingRow).toBe(false);
            expect(model.headingRow).toBe(2);
            expect(model.dataRow).toBe(3);
        }));

        it('increasing heading row to beyond data row moves data row down', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.headingRow = 4;
            spImportFileService.headingRowChanged(model);
            expect(model.noHeadingRow).toBe(false);
            expect(model.headingRow).toBe(4);
            expect(model.dataRow).toBe(5);
        }));

        it('hiding heading row causes data row to change from 2 to 1', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.noHeadingRow = true;
            spImportFileService.noHeadingChanged(model);
            expect(model.noHeadingRow).toBe(true);
            expect(model.headingRow).toBe(0);
            expect(model.dataRow).toBe(1);
        }));

        it('unhiding heading row causes data row to change from 1 to 2', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.noHeadingRow = true;
            spImportFileService.noHeadingChanged(model);
            model.noHeadingRow = false;
            spImportFileService.noHeadingChanged(model);
            expect(model.noHeadingRow).toBe(false);
            expect(model.headingRow).toBe(1);
            expect(model.dataRow).toBe(2);
        }));

        it('hiding heading row causes data row to be unchanged if not 2', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.dataRow = 3;
            spImportFileService.dataRowChanged(model);
            model.noHeadingRow = true;
            spImportFileService.noHeadingChanged(model);
            expect(model.noHeadingRow).toBe(true);
            expect(model.headingRow).toBe(0);
            expect(model.dataRow).toBe(3);
        }));

        it('unhiding heading row causes data row to be unchanged if not 2', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            model.dataRow = 3;
            spImportFileService.dataRowChanged(model);
            model.noHeadingRow = true;
            spImportFileService.noHeadingChanged(model);
            model.noHeadingRow = false;
            spImportFileService.noHeadingChanged(model);
            expect(model.noHeadingRow).toBe(false);
            expect(model.headingRow).toBe(1);
            expect(model.dataRow).toBe(3);
        }));
    });

    describe('selection of type', function () {

        it('type schema gets loaded', inject(function (spImportFileService, spEntityService) {
            spEntityService.mockGetEntity(getAllFieldType());

            var type = spEntity.fromJSON({ id: allFieldTypeId, name: 'All Fields' });

            var model = null;

            TestSupport.wait(
                spImportFileService.createModel(0)
                .then(function (newModel) {
                    model = newModel;
                    model.file.fileFormat = 'Excel';
                    return spImportFileService.setSelectedType(model, type);
                })
                .then(function () {
                    expect(model.selectedResourceName).toBe('All Fields');
                    expect(model.targetMembers.length > 0).toBeTruthy();
                    expect(model.configName).toBe('Import All Fields from Excel');
                    var targetField = model.targetMembers[0];
                    expect(targetField.fieldGroup).toBeTruthy();
                    expect(targetField.memberInfo).toBeTruthy();
                    expect(targetField.memberName).toBeTruthy();
                }));
        }));

        it('updates name if type is changed again', inject(function (spImportFileService, spEntityService) {
            spEntityService.mockGetEntity(getAllFieldType());

            var type = spEntity.fromJSON({ id: allFieldTypeId, name: 'All Fields' });
            var type2 = spEntity.fromJSON({ id: allFieldTypeId, name: 'All Fields2' });

            var model = null;

            TestSupport.wait(
                spImportFileService.createModel(0)
                .then(function (newModel) {
                    model = newModel;
                    model.file.fileFormat = 'Excel';
                    return spImportFileService.setSelectedType(model, type);
                })
                .then(function () {
                    return spImportFileService.setSelectedType(model, type2);
                })
                .then(function () {
                    expect(model.selectedResourceName).toBe('All Fields2');
                    expect(model.configName).toBe('Import All Fields2 from Excel');
                }));
        }));

        it('doesn\'t update name if already set', inject(function (spImportFileService, spEntityService) {
            spEntityService.mockGetEntity(getAllFieldType());

            var type = spEntity.fromJSON({ id: allFieldTypeId, name: 'All Fields' });

            var model = null;

            TestSupport.wait(
                spImportFileService.createModel(0)
                .then(function (newModel) {
                    model = newModel;
                    model.file.fileFormat = 'Excel';
                    model.configName = 'Sunshine';
                    return spImportFileService.setSelectedType(model, type);
                })
                .then(function () {
                    expect(model.configName).toBe('Sunshine');
                }));
        }));
    });

    describe('onFileUploadComplete', function () {

        it('runs', inject(function (spImportFileService, $httpBackend) {
            var model = spImportFileService.createEmptyModel();

            $httpBackend.whenGET('/spapi/data/v2/importSpreadsheet/sheet?fileId=myfileuploadid&fileFormat=Excel&fileName=MyFileName.xlsx&sheet=').respond(spreadsheetInfoTestData.spreadsheetData);

            TestSupport.wait(
                model.upload.onFileUploadComplete('MyFileName.xlsx', 'myfileuploadid')
                .then(function () {
                    expect(model.isDocumentReady).toBe(true);
                    expect(model.file.fileName).toBe('MyFileName.xlsx');
                    expect(model.file.fileFormat).toBe('Excel');
                    expect(model.file.fileId).toBe('myfileuploadid');
                    expect(model.sampleDataTable).toBeTruthy();
                    expect(model.sheets).toBeArray(2);
                }),
                { customFlush: $httpBackend.flush });
        }));
    });

    describe('getAvailableTargetMembers', function () {

        it('runs', inject(function (spImportFileService) {
            var typeEntity = getAllFieldType();
            var typeResource = new spResource.Type(typeEntity);
            var targetMembers = spImportFileService.test.getAvailableTargetMembers(typeResource);
            expect(targetMembers).toBeArray();
            expect(targetMembers.length > 0).toBeTruthy();
            var targetField = targetMembers[0];
            expect(targetField.fieldGroup).toBe('Date Fields');
            expect(targetField.memberInfo).toBeTruthy();
            expect(targetField.memberName).toBeTruthy('Date');
        }));

        it('handles missing type', inject(function (spImportFileService) {
            var targetMembers = spImportFileService.test.getAvailableTargetMembers();
            expect(targetMembers).toBeArray(0);
        }));
    });

    describe('getAvailableSourceColumns', function () {

        it('handles no sample table', inject(function (spImportFileService) {
            var model = spImportFileService.createEmptyModel();
            var res = spImportFileService.test.getAvailableSourceColumns(model);
            expect(res).toBeArray(0);
        }));

        it('handles normal data', inject(function (spImportFileService) {

            var model = spImportFileService.createEmptyModel();
            model.sampleDataTable = spreadsheetInfoTestData.spreadsheetData.initialSampleTable;

            var res = spImportFileService.test.getAvailableSourceColumns(model);
            expect(res).toBeArray(2);
            expect(res[0].colId).toBe('A');
            expect(res[0].colName).toBe('(A) Name');
            expect(res[1].colId).toBe('B');
            expect(res[1].colName).toBe('(B) Age');
            expect(res[1].sample1).toBe('35');
            expect(res[1].sample2).toBe('23');
        }));

        it('handles no sample rows', inject(function (spImportFileService) {

            var model = spImportFileService.createEmptyModel();
            model.sampleDataTable = {
                columns: spreadsheetInfoTestData.spreadsheetData.initialSampleTable.columns,
                rows: []
            };

            var res = spImportFileService.test.getAvailableSourceColumns(model);
            expect(res).toBeArray(2);
            expect(res[0].colId).toBe('A');
            expect(res[0].colName).toBe('(A) Name');
            expect(res[1].colId).toBe('B');
            expect(res[1].colName).toBe('(B) Age');
            expect(res[1].sample1).toBe('');
            expect(res[1].sample2).toBe('');
        }));
    });

    describe('saveImportConfig', function () {

        it('runs', inject(function (spImportFileService, spEntityService, $q) {
            var model = null;
            var importConfig = null;

            spyOn(spEntityService, 'putEntity').andReturn($q.when(123));

            TestSupport.wait(
                spImportFileService.createModel(0)
                .then(function (newModel) {
                    model = newModel;
                    importConfig = model.importConfig;
                    return spImportFileService.test.saveImportConfig(model);
                })
                .then(function () {
                    expect(spEntityService.putEntity).toHaveBeenCalledWith(importConfig);
                    expect(model.importConfigId).toBe(123);
                    expect(model.importConfig).toBe(null);  // for now we clear it, because it's the old one
                })
            );
        }));
    });

    describe('cancelImport', function () {

        it('runs', inject(function (spImportFileService, $httpBackend) {
            var model = null;

            $httpBackend.whenGET('/spapi/data/v2/importSpreadsheet/cancel/321').respond(spreadsheetInfoTestData.spreadsheetData);

            TestSupport.wait(
                spImportFileService.createModel(0)
                .then(function (newModel) {
                    model = newModel;
                    model.importRunId = 321;
                    return spImportFileService.cancelImport(model);
                }));
        }));
    });

    describe('saveAndReload', function () {

        it('runs', inject(function (spImportFileService, spEntityService, $q) {
            var model = null;
            var importConfig = null;

            spyOn(spEntityService, 'putEntity').andReturn($q.when(testImportConfigId));

            TestSupport.wait(
                spImportFileService.createModel(0)
                .then(function (newModel) {
                    model = newModel;
                    model.configName = 'Some name';
                    model.nav.curPage = model.nav.optionsPage;
                    model.options.testRun = true;
                    importConfig = model.importConfig;
                    return spImportFileService.saveAndReload(model);
                })
                .then(function () {
                    expect(spEntityService.putEntity).toHaveBeenCalledWith(importConfig);
                    expect(model.importConfigId).toBe(testImportConfigId);
                    expect(model.importConfig).not.toBe(null);
                    expect(model.importConfig.idP).toBe(testImportConfigId);
                    expect(model.options.testRun).toBe(true);
                })
            );
        }));

        it('rejects invalid names', inject(function (spImportFileService, spEntityService, $q) {
            var model = null;
            var importConfig = null;

            spyOn(spEntityService, 'putEntity').andReturn($q.when(testImportConfigId));

            TestSupport.wait(
                spImportFileService.createModel(0)
                .then(function (newModel) {
                    model = newModel;
                    model.configName = '<';
                    model.nav.curPage = model.nav.optionsPage;
                    importConfig = model.importConfig;
                    return spImportFileService.saveAndReload(model);
                })
                .then(function () {
                    expect(spEntityService.putEntity).not.toHaveBeenCalledWith(importConfig);
                })
            );
        }));

        it('rejects blank names', inject(function (spImportFileService, spEntityService, $q) {
            var model = null;
            var importConfig = null;

            spyOn(spEntityService, 'putEntity').andReturn($q.when(testImportConfigId));

            TestSupport.wait(
                spImportFileService.createModel(0)
                .then(function (newModel) {
                    model = newModel;
                    model.configName = '';
                    model.nav.curPage = model.nav.optionsPage;
                    importConfig = model.importConfig;
                    return spImportFileService.saveAndReload(model);
                })
                .then(function () {
                    expect(spEntityService.putEntity).not.toHaveBeenCalledWith(importConfig);
                })
            );
        }));
    });

    describe('allRequiredMembersAreMapped', function () {

        it('true if no mandatory members', inject(function (spImportFileService, spEntityService) {
            spEntityService.mockGetEntity(getAllFieldType());
            var type = spEntity.fromJSON({ id: allFieldTypeId, name: 'All Fields' });

            var model = null;
            TestSupport.wait(
                spImportFileService.createModel(0)
                .then(function (newModel) {
                    model = newModel;
                    return spImportFileService.setSelectedType(model, type);
                })
                .then(function () {
                    var res = spImportFileService.test.allRequiredMembersAreMapped(model);
                    expect(res).toBe(true);
                })
            );
        }));

        it('false if unmapped mandatory members', inject(function (spImportFileService, spEntityService) {
            var typeEntity = getAllFieldType().cloneDeep();
            var field = _.filter(typeEntity.fields, function (f) { return f.name === 'Number'; })[0];
            field.isRequired = true;

            spEntityService.mockGetEntity(typeEntity);
            var type = spEntity.fromJSON({ id: allFieldTypeId, name: 'All Fields' });

            var model = null;
            TestSupport.wait(
                spImportFileService.createModel(0)
                .then(function (newModel) {
                    model = newModel;
                    return spImportFileService.setSelectedType(model, type);
                })
                .then(function () {
                    var res = spImportFileService.test.allRequiredMembersAreMapped(model);
                    expect(res).toBe(false);
                })
            );
        }));

        it('true if unmapped mandatory member mapped', inject(function (spImportFileService, spEntityService) {
            var typeEntity = getAllFieldType().cloneDeep();
            var field = _.filter(typeEntity.fields, function (f) { return f.name === 'Number'; })[0];
            field.isRequired = true;

            spEntityService.mockGetEntity(typeEntity);
            var type = spEntity.fromJSON({ id: allFieldTypeId, name: 'All Fields' });

            var model = null;
            TestSupport.wait(
                spImportFileService.createModel(0)
                .then(function (newModel) {
                    model = newModel;
                    return spImportFileService.setSelectedType(model, type);
                })
                .then(function () {
                    var mapping = spEntity.fromJSON({
                        name: 'A',
                        typeId: 'apiFieldMapping',
                        mappedField: jsonLookup(field.idP)
                    });
                    model.importConfig.importConfigMapping.resourceMemberMappings.push(mapping);
                    var res = spImportFileService.test.allRequiredMembersAreMapped(model);
                    expect(res).toBe(true);
                })
            );
        }));
    });

});
