// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spreadsheetInfoTestData */

describe('Import Files|spec|controllers|spMemberMappingOptions', function () {
    'use strict';

    // Load the modules
    beforeEach(module('ng'));
    beforeEach(module('mod.app.importFile.controllers.spMemberMappingOptions'));
    beforeEach(module('importFile/controllers/spMemberMappingOptions.tpl.html'));
    beforeEach(module('mockedNavService'));
    beforeEach(module('mockedEntityService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    var allFieldTypeId = 18751;
    var mockFromType = allFieldTypeId;
    var mockToType = 2;
    var mockTargetField = 3;

    function mockMappingRow() {
        return {
            mapping: spEntity.fromJSON({
                mappedRelationship: {
                    fromType: jsonLookup(mockFromType),
                    toType: jsonLookup(mockToType)
                },
                mapRelationshipInReverse: false,
                mappedRelationshipLookupField: jsonLookup()
            })
        };
    }

    it('controller should load', inject(function ($controller, $rootScope) {

        var scope = $rootScope.$new();
        var controller = $controller('spMemberMappingOptionsController', {
            $scope: scope,
            $uibModalInstance: { },
            mappingRow: { mapping: null }
        });
        expect(controller).toBeTruthy();
    }));

    it('template should load', inject(function ($rootScope, $compile, $templateCache) {
        var template = $templateCache.get('importFile/controllers/spMemberMappingOptions.tpl.html');
        expect(template).toBeTruthy();

        var scope = $rootScope.$new();
        var element = angular.element(template);
        $compile(element)(scope);
        scope.$digest();

        expect(element[0]).toBeTruthy();
        //var chartElem = element.find('sp-axis-type-properties')[0];
        //expect(chartElem).toBeTruthy();
    }));

    describe('createModel', function () {

        it('runs', inject(function (spMemberMappingOptions) {
            var mappingRow = mockMappingRow();
            var model = spMemberMappingOptions.createModel(mappingRow);
            expect(model).toBeTruthy();
            expect(model.mapping).toBe(mappingRow.mapping);
        }));
    });

    describe('loadModelData', function () {

        it('loads fields with default selected', inject(function (spMemberMappingOptions, spEntityService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity().cloneDeep();
            var relEntity = spEntity.fromJSON({ fromType: jsonLookup(), toType: jsonLookup() });
            relEntity.toType = typeEntity;
            spEntityService.mockGetEntity(relEntity);
            
            var mappingRow = mockMappingRow();
            mappingRow.mapping.mappedRelationship = spEntity.fromId(relEntity.idP);
            var model = spMemberMappingOptions.createModel(mappingRow);

            TestSupport.wait(
                spMemberMappingOptions.loadModelData(model)
                .then(function () {
                    expect(model.lookupByMembers).toBeArray(5); // expects string and number fields only
                    expect(model.defaultLookupField).toBeTruthy();
                    expect(model.selectedLookupField).toBe(model.defaultLookupField);
                })
            );
        }));

        it('loads fields for reverse relationship', inject(function (spMemberMappingOptions, spEntityService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity().cloneDeep();
            var relEntity = spEntity.fromJSON({ fromType: jsonLookup(), toType: jsonLookup() });
            relEntity.fromType = typeEntity;
            spEntityService.mockGetEntity(relEntity);

            var mappingRow = mockMappingRow();
            mappingRow.mapping.mapRelationshipInReverse = true;
            mappingRow.mapping.mappedRelationship = spEntity.fromId(relEntity.idP);
            var model = spMemberMappingOptions.createModel(mappingRow);

            TestSupport.wait(
                spMemberMappingOptions.loadModelData(model)
                .then(function () {
                    expect(model.lookupByMembers).toBeArray(5); // expects string and number fields only
                    expect(model.defaultLookupField).toBeTruthy();
                    expect(model.selectedLookupField).toBe(model.defaultLookupField);
                })
            );
        }));

        it('loads fields with non-default selected', inject(function (spMemberMappingOptions, spEntityService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity().cloneDeep();
            var relEntity = spEntity.fromJSON({ fromType: jsonLookup(), toType: jsonLookup() });
            relEntity.toType = typeEntity;
            spEntityService.mockGetEntity(relEntity);
            
            var mappingRow = mockMappingRow();
            mappingRow.mapping.mappedRelationship = spEntity.fromId(relEntity.idP);
            mappingRow.mapping.mappedRelationshipLookupField = 2852; // Int Field from test data
            var model = spMemberMappingOptions.createModel(mappingRow);

            TestSupport.wait(
                spMemberMappingOptions.loadModelData(model)
                .then(function () {
                    expect(model.lookupByMembers).toBeArray(5); // expects string and number fields only
                    expect(model.defaultLookupField).toBeTruthy();
                    expect(model.selectedLookupField).toBe(model.defaultLookupField);
                })
            );
        }));

    });
    
    describe('loadModelData', function () {

        it('loads fields with default selected', inject(function (spMemberMappingOptions, spEntityService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity().cloneDeep();
            var relEntity = spEntity.fromJSON({ fromType: jsonLookup(), toType: jsonLookup() });
            relEntity.toType = typeEntity;
            spEntityService.mockGetEntity(relEntity);
            
            var mappingRow = mockMappingRow();
            mappingRow.mapping.mappedRelationship = spEntity.fromId(relEntity.idP);
            var model = spMemberMappingOptions.createModel(mappingRow);

            TestSupport.wait(
                spMemberMappingOptions.loadModelData(model)
                .then(function () {
                    expect(model.lookupByMembers).toBeArray(5); // expects string and number fields only
                    expect(model.defaultLookupField).toBeTruthy();
                    expect(model.selectedLookupField).toBe(model.defaultLookupField);
                })
            );
        }));

        it('loads fields for reverse relationship', inject(function (spMemberMappingOptions, spEntityService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity().cloneDeep();
            var relEntity = spEntity.fromJSON({ fromType: jsonLookup(), toType: jsonLookup() });
            relEntity.fromType = typeEntity;
            spEntityService.mockGetEntity(relEntity);

            var mappingRow = mockMappingRow();
            mappingRow.mapping.mapRelationshipInReverse = true;
            mappingRow.mapping.mappedRelationship = spEntity.fromId(relEntity.idP);
            var model = spMemberMappingOptions.createModel(mappingRow);

            TestSupport.wait(
                spMemberMappingOptions.loadModelData(model)
                .then(function () {
                    expect(model.lookupByMembers).toBeArray(5); // expects string and number fields only
                    expect(model.defaultLookupField).toBeTruthy();
                    expect(model.selectedLookupField).toBe(model.defaultLookupField);
                })
            );
        }));

        it('loads fields with non-default selected', inject(function (spMemberMappingOptions, spEntityService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity().cloneDeep();
            var relEntity = spEntity.fromJSON({ fromType: jsonLookup(), toType: jsonLookup() });
            relEntity.toType = typeEntity;
            spEntityService.mockGetEntity(relEntity);
            
            var mappingRow = mockMappingRow();
            mappingRow.mapping.mappedRelationship = spEntity.fromId(relEntity.idP);
            mappingRow.mapping.mappedRelationshipLookupField = 12176; // Int Field from test data
            var model = spMemberMappingOptions.createModel(mappingRow);

            TestSupport.wait(
                spMemberMappingOptions.loadModelData(model)
                .then(function () {
                    expect(model.lookupByMembers).toBeArray(5); // expects string and number fields only
                    expect(model.defaultLookupField).toBeTruthy();
                    expect(model.selectedLookupField.memberName).toBe('Number');
                })
            );
        }));
    });

    describe('applyChanges', function () {

        it('when changed to name field', inject(function (spMemberMappingOptions, spEntityService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity().cloneDeep();
            var relEntity = spEntity.fromJSON({ fromType: jsonLookup(), toType: jsonLookup() });
            relEntity.toType = typeEntity;
            spEntityService.mockGetEntity(relEntity);

            var mappingRow = mockMappingRow();
            mappingRow.mapping.mappedRelationship = spEntity.fromId(relEntity.idP);
            mappingRow.mapping.mappedRelationshipLookupField = 12176; // Int Field from test data
            var model = spMemberMappingOptions.createModel(mappingRow);

            TestSupport.wait(
                spMemberMappingOptions.loadModelData(model)
                .then(function () {
                    model.selectedLookupField = model.lookupByMembers[1];   // some other field that has if 16153
                    spMemberMappingOptions.applyChanges(model);
                    expect(mappingRow.mapping.mappedRelationshipLookupField).toBe(null);
                })
            );
        }));

        it('when changed to another field', inject(function (spMemberMappingOptions, spEntityService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity().cloneDeep();
            var relEntity = spEntity.fromJSON({ fromType: jsonLookup(), toType: jsonLookup() });
            relEntity.toType = typeEntity;
            spEntityService.mockGetEntity(relEntity);

            var mappingRow = mockMappingRow();
            mappingRow.mapping.mappedRelationship = spEntity.fromId(relEntity.idP);
            mappingRow.mapping.mappedRelationshipLookupField = 12176; // Int Field from test data
            var model = spMemberMappingOptions.createModel(mappingRow);

            TestSupport.wait(
                spMemberMappingOptions.loadModelData(model)
                .then(function () {
                    model.selectedLookupField = model.lookupByMembers[3];   // some other field that has if 16153
                    spMemberMappingOptions.applyChanges(model);
                    expect(mappingRow.mapping.mappedRelationshipLookupField.idP).toBe(16153);
                })
            );
        }));
    });

});