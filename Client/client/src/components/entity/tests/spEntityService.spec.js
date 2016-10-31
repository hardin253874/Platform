// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals entityTestData, spEntity */

describe('Entity Model|spEntityService|spec:', function () {

    beforeEach(module('ng'));
    beforeEach(module('mod.common.spEntityService'));

    describe('spEntityService', function() {

        it('should expect true to be truthy', function () {
            expect(true).toBeTruthy();
        });

    });

    describe('batch requests', function () {

        it('getEntity should call service and handle response', inject(function ($httpBackend, $rootScope, spEntityService) {

            // { queries: ['name'], requests: [{ rq:0, get:'basic', ids:['test:af01']}]}
            var response = {
                "results": [ {
                    "code": 200,
                    "ids": [ 16736 ]
                } ],
                "ids": [],
                "entities": { "16736": { "7765": "Test 01" } },
                "members": {
                    "7765": { "alias": "core:name", "dt": "String" }
                }
            };
            $httpBackend.whenPOST('/spapi/data/v2/entity?af01').respond(response);

            var p1 = spEntityService.getEntity('test:af01', 'name');

            var result1 = {};
            TestSupport.waitCheckReturn($rootScope, p1, result1, $httpBackend.flush);

            runs(function () {
                expect(result1.value.getName()).toBeDefined('Test 02');
            });
        }));

        it('getEntity should run with a complex request', inject(function ($httpBackend, $rootScope, spEntityService) {
            
            $httpBackend.whenPOST('/spapi/data/v2/entity?af02').respond(entityTestData.af02);

            var p1 = spEntityService.getEntity('test:af02', 'name');
            
            var result1 = {};
            TestSupport.waitCheckReturn($rootScope, p1, result1, $httpBackend.flush);

            runs(function () {
                expect(result1.value.getName()).toBeDefined('Test 02');
            });
        }));

        it('getEntities should call service and handle response', inject(function ($httpBackend, $rootScope, spEntityService) {

            // { queries: ['name'], requests: [{ rq:0, get:'basic', ids:['test:af01','test:af02']}]}
            var response = {
                "results": [{
                    "code": 200,
                    "ids": [16736, 16737]
                }],
                "ids": [],
                "entities": {
                    "16736": { "7765": "Test 01" },
                    "16737": { "7765": "Test 02" }
                },
                "members": {
                    "7765": { "alias": "core:name", "dt": "String" }
                }
            };
            $httpBackend.whenPOST('/spapi/data/v2/entity?multiple').respond(response);

            var p1 = spEntityService.getEntities(['test:af01','test:af02'], 'name');

            var result1 = {};
            TestSupport.waitCheckReturn($rootScope, p1, result1, $httpBackend.flush);

            runs(function () {
                expect(result1.value[0].getName()).toBeDefined('Test 01');
                expect(result1.value[1].getName()).toBeDefined('Test 02');
            });
        }));

        it('getEntitiesOfType should call service and handle response', inject(function ($httpBackend, $rootScope, spEntityService) {

            // { queries: ['name'], requests: [{ rq:0, get:'instances', ids:['test:nationalityEnumType']}]}
            var response = {
                "results": [{
                    "code": 200,
                    "ids": [15874, 16155, 16336, 16368]
                }],
                "ids": [],
                "entities": {
                    "15874": { "7765": "SWE" },
                    "16155": { "7765": "AUS" },
                    "16336": { "7765": "GBR" },
                    "16368": { "7765": "RSA" }
                },
                "members": {
                    "7765": { "alias": "core:name", "dt": "String" }
                }
            };
            $httpBackend.whenPOST('/spapi/data/v2/entity?instOf-nationalityEnumType').respond(response);

            var p1 = spEntityService.getEntitiesOfType('test:nationalityEnumType', 'name');

            var result1 = {};
            TestSupport.waitCheckReturn($rootScope, p1, result1, $httpBackend.flush);

            runs(function () {
                expect(result1.value.length).toBe(4);
                expect(result1.value[0].getName()).toBeDefined('SWE');
            });
        }));

        it('getInstancesOfType should call service and handle response', inject(function ($httpBackend, $rootScope, spEntityService) {

            // { queries: ['name'], requests: [{ rq:0, get:'instances', ids:['test:nationalityEnumType']}]}
            var response = {
                "results": [{
                    "code": 200,
                    "ids": [15874, 16155, 16336, 16368]
                }],
                "ids": [],
                "entities": {
                    "15874": { "7765": "SWE" },
                    "16155": { "7765": "AUS" },
                    "16336": { "7765": "GBR" },
                    "16368": { "7765": "RSA" }
                },
                "members": {
                    "7765": { "alias": "core:name", "dt": "String" }
                }
            };
            $httpBackend.whenPOST('/spapi/data/v2/entity?instOf-nationalityEnumType').respond(response);

            var p1 = spEntityService.getInstancesOfType('test:nationalityEnumType', 'name');

            var result1 = {};
            TestSupport.waitCheckReturn($rootScope, p1, result1, $httpBackend.flush);

            runs(function () {
                expect(result1.value.length).toBe(4);
                expect(result1.value[0].name).toBeDefined('SWE');
            });
        }));

        it('getEntityByNameAndTypeName call service and handle response', inject(function ($httpBackend, $rootScope, spEntityService) {

            // { queries: ['name'], requests: [{ rq:0, get:'instances', ids:['test:nationalityEnumType']}]}
            var response = {
                "results": [ { "code": 200, "ids": [17033] } ],
                "ids": [ 17033 ],
                "entities": {
                    "17033": { "7765": "Pizza" }
                },
                "members": {
                    "7765": { "alias": "core:name", "dt": "String" }
                }
            };
            $httpBackend.whenGET('/spapi/data/v2/entity?name=Pizza&typename=Definition&request=name').respond(response);

            var p1 = spEntityService.getEntityByNameAndTypeName('Pizza', 'Definition', 'name');

            var result1 = {};
            TestSupport.waitCheckReturn($rootScope, p1, result1, $httpBackend.flush);

            runs(function () {
                expect(result1.value.getName()).toBeDefined('Pizza');
            });
        }));

        it('putEntity should run', inject(function ($httpBackend, $rootScope, spEntityService) {

            $httpBackend.expectPOST('/spapi/data/v1/entity/').respond(200, 123);

            var e = spEntity.fromJSON({ typeId: 'test:herb', name: 'Chocolate' });
            var p1 = spEntityService.putEntity(e);

            var result1 = {};
            TestSupport.waitCheckReturn($rootScope, p1, result1, $httpBackend.flush);

            runs(function () {
                expect(result1.value).toBe(123);
            });
        }));

        it('deleteEntity should run with ID', inject(function ($httpBackend, $rootScope, spEntityService) {

            $httpBackend.expectDELETE('/spapi/data/v1/entity/123').respond(200, '');

            var p1 = spEntityService.deleteEntity(123);

            TestSupport.waitCheckReturn($rootScope, p1, null, $httpBackend.flush);
        }));

        it('deleteEntities should run with IDs', inject(function ($httpBackend, $rootScope, spEntityService) {

            $httpBackend.expectDELETE('/spapi/data/v1/entity/123,456').respond(200, '');

            var p1 = spEntityService.deleteEntity([123, 456]);

            TestSupport.waitCheckReturn($rootScope, p1, null, $httpBackend.flush);
        }));



    });

});
