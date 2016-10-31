// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spreadsheetInfoTestData */

describe('Connector|spec|spResourceEndpointService', function () {

    var endpointId = 1;
    var apiId = 2;

    beforeEach(module('mod.app.connector.spResourceEndpointService'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('sp.app.navigation'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    //
    // Setup the mocked entity service.
    //
    beforeEach(inject(function (spEntityService) {

        spEntityService.mockGetEntityJSON({
            id: endpointId,
            typeId: 'core:apiResourceEndpoint',
            name: 'Test Endpoint',
            endpointResourceMapping: jsonLookup(null),
            endpointForApi: {
                id: apiId,
                apiAddress: 'testApiAddress'
            }
        });

        spEntityService.mockGetEntityJSON({
            id: apiId,
            apiAddress: 'testApiAddress'
        });
    }));


    //
    // Tests start here
    //

    it('exists', inject(function (spResourceEndpointService) {
        expect(spResourceEndpointService).toBeTruthy();
    }));

    describe('model', function () {

        it('can load endpoint', inject(function (spResourceEndpointService, spEntityService) {

            spyOn(spEntityService, 'getEntity').andCallThrough();

            TestSupport.wait(
                spResourceEndpointService.createModel(endpointId, 0)
                .then(function (model) {
                    expect(spEntityService.getEntity).toHaveBeenCalled();
                    expect(model).toBeTruthy();
                }));
        }));

    });

    describe('makeJsonName', function () {

        it('handles null', inject(function (spResourceEndpointService) {
            expect(spResourceEndpointService.makeJsonName(null)).toBeNull();
        }));

        it('removes non alpha numeric', inject(function (spResourceEndpointService) {
            expect(spResourceEndpointService.makeJsonName('hello 1 world!')).toBe('hello1world');
        }));

        it('makes lowercase', inject(function (spResourceEndpointService) {
            expect(spResourceEndpointService.makeJsonName('HelloWorld')).toBe('helloworld');
        }));

    });

    describe('createModel', function () {

        var endsWith = function (str, suffix) {
            return str.indexOf(suffix, str.length - suffix.length) !== -1;
        };

        it('handles create', inject(function (spResourceEndpointService) {
            TestSupport.wait(
                spResourceEndpointService.createModel(0, apiId).then(function (model) {
                    expect(model).toBeTruthy();
                    expect(model.addressPrefix).toBeTruthy();
                    expect(endsWith(model.addressPrefix,'/testApiAddress')).toBeTruthy();
                    var endpoint = model.endpoint;
                    expect(endpoint).toBeTruthy(); //object
                    expect(endpoint.endpointResourceMapping).toBeTruthy(); //object
                    expect(endpoint.endpointResourceMapping.resourceMemberMappings).toBeArray();
                    expect(endpoint.endpointForApi.idP).toBe(apiId);
                })
            );
            
        }));

        it('handles load', inject(function (spResourceEndpointService) {
            TestSupport.wait(
                spResourceEndpointService.createModel(endpointId, apiId).then(function (model) {
                    expect(model).toBeTruthy();
                    expect(model.addressPrefix).toBeTruthy();
                    expect(endsWith(model.addressPrefix, '/testApiAddress')).toBeTruthy();
                    var endpoint = model.endpoint;
                    expect(endpoint).toBeTruthy(); //object
                    expect(endpoint.endpointResourceMapping).toBeTruthy(); //object
                    expect(endpoint.endpointResourceMapping.resourceMemberMappings).toBeArray();
                })
            );
        }));

    });

    describe('createMemberMappingEntity', function () {

        it('runs for fields', inject(function (spResourceEndpointService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity();
            var typeResource = new spResource.Type(typeEntity);
            var memberInfo = typeResource.getFields()[0];
            var reference = 'myJsonField';
            var mapping = spResourceEndpointService.createMemberMappingEntity(memberInfo, reference);
            expect(mapping).toBeTruthy();
            expect(mapping.name).toBe(reference);
            expect(mapping.mappedField.idP).toBe(memberInfo.getEntity().idP);
        }));

        it('runs for relationships', inject(function (spResourceEndpointService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity();
            var typeResource = new spResource.Type(typeEntity);
            var memberInfo = typeResource.getAllRelationships()[0];
            expect(memberInfo.isReverse()).toBe(true);    // it's a reverse, as it happens
            var reference = 'myJsonField';
            var mapping = spResourceEndpointService.createMemberMappingEntity(memberInfo, reference);
            expect(mapping).toBeTruthy();
            expect(mapping.name).toBe(reference);
            expect(mapping.mappedRelationship.idP).toBe(memberInfo.getEntity().idP);
            expect(mapping.mapRelationshipInReverse).toBe(true);
        }));

    });

    describe('mappingRefersToMember', function () {

        it('works for fields', inject(function (spResourceEndpointService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity();
            var typeResource = new spResource.Type(typeEntity);
            var memberInfo = typeResource.getFields()[0];
            var reference = 'myJsonField';
            var mapping = spResourceEndpointService.createMemberMappingEntity(memberInfo, reference);
            var res = spResourceEndpointService.mappingRefersToMember(mapping, memberInfo);
            expect(res).toBeTruthy();
        }));

        it('works for relationships', inject(function (spResourceEndpointService) {
            var typeEntity = spreadsheetInfoTestData.getAllFieldTypeEntity();
            var typeResource = new spResource.Type(typeEntity);
            var memberInfo = typeResource.getAllRelationships()[0];
            expect(memberInfo.isReverse()).toBe(true);    // it's a reverse, as it happens
            var reference = 'myJsonField';
            var mapping = spResourceEndpointService.createMemberMappingEntity(memberInfo, reference);
            var res = spResourceEndpointService.mappingRefersToMember(mapping, memberInfo);
            expect(res).toBeTruthy();
        }));

    });

});
