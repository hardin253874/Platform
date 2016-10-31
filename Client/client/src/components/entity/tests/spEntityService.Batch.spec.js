// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Entity Model|spEntityService|Batching|spec:', function () {
    'use strict';

    var mockResult = null;

    beforeEach(module('ng'));
    beforeEach(module('mod.common.spEntityService'));

    // mock data returned by entity services. (done separately, because we need $q, and cant get it while mocking the services)
    beforeEach(inject(function (spEntityService, $q) {
        var getBatchMocked = function () {
            var res = $q.defer();
            res.resolve(mockResult);
            return res.promise;
        };

        spEntityService._issueBatchRequest = jasmine.createSpy('_issueBatchRequest').andCallFake(getBatchMocked);
    }));

    describe('manual batching', function () {
        it('should reject invalid requests', inject(function ($rootScope, spEntityService) {
            var batch = new spEntityService.BatchRequest();
            expect(function () { batch.addRequest(null); }).toThrow();
            expect(function() { batch.addRequest(spEntityService.makeGetEntityRequest('test:af01', '')); }).toThrow();
        }));
    });

    describe('disable batching', function () {
        it('should prevent batching', inject(function ($rootScope, spEntityService) {
            var options = { batch: true };

            spEntityService.disableBatching = true;

            var mockResultA = {
                data: {
                    results: [
                        { code: 200, ids: [123] },
                    ],
                    entities: {
                        '123': { '77': 'Test 01' },
                    },
                    members: {
                        '77': { alias: 'core:name', dt: 'String' },
                    }
                }
            };

            var mockResultB = {
                data: {
                    results: [
                        { code: 200, ids: [234] },
                    ],
                    entities: {
                        '234': { '77': 'Test 01' },
                    },
                    members: {
                        '77': { alias: 'core:name', dt: 'String' },
                    }
                }
            };

            mockResult = mockResultA;
            var p1 = spEntityService.getEntity('test:af01', 'name', options);
            mockResult = mockResultB;
            var p2 = spEntityService.getEntity('test:af02', 'name', options);

            // we are not actually batching so no need to wait for the timeout
            p1.then(function (result) {
                expect(result.value.idP).toBe(123);
            });

            p2.then(function (result) {
                expect(result.value.idP).toBe(234);
            });
        }));
    });

    describe('automatic batching', function () {

        // Introduction Tests

        it('should run', inject(function ($rootScope, spEntityService) {

            mockResult = {
                data: {
                    results: [
                        { code: 200, ids: [123] },
                        { code: 200, ids: [456] }
                    ],
                    entities: {
                        '123': { '77': 'Test 01' },
                        '456': { '88': 'Description of Test 2' }
                    },
                    members: {
                        '77': { alias: 'core:name', dt: 'String' },
                        '88': { alias: 'core:description', dt: 'String' }
                    }
                }
            };

            var options = { batch: true };
            var p1 = spEntityService.getEntity('test:af01', 'name', options);
            var p2 = spEntityService.getEntity('test:af02', 'description', options);

            var result1 = {};
            var result2 = {};
            TestSupport.waitCheckReturn($rootScope, p1, result1);
            TestSupport.waitCheckReturn($rootScope, p2, result2);

            runs(function () {
                expect(result1.value.getName()).toBe('Test 01');
                expect(result2.value.getDescription()).toBe('Description of Test 2');
            });
        }));

        it('should merge fields', inject(function ($rootScope, spEntityService) {

            mockResult = {
                data: {
                    results: [
                        { code: 200, ids: [789] },
                        { code: 200, ids: [789] }
                    ],
                    entities: {
                        '789': {
                            '77': 'Test 03',
                            '88': 'Description of Test 3',
                            '99': 300
                        },
                    },
                    members: {
                        '77': { alias: 'core:name', dt: 'String' },
                        '88': { alias: 'core:description', dt: 'String' },
                        '99': { alias: 'test:afNumber', dt: 'Int32' }
                    }
                }
            };

            var options = { batch: true };
            var p1 = spEntityService.getEntity('test:af03', 'name,test:afNumber', options);
            var p2 = spEntityService.getEntity('test:af03', 'name,description', options);

            var result1 = {};
            var result2 = {};
            TestSupport.waitCheckReturn($rootScope, p1, result1);
            TestSupport.waitCheckReturn($rootScope, p2, result2);

            runs(function () {
                expect(result1.value).toBe(result2.value);
                expect(result1.value.getName()).toBe('Test 03');
                expect(result1.value.getAfNumber()).toBe(300);
                expect(result2.value.getDescription()).toBe('Description of Test 3');
            });
        }));
        
        it('should extract named queries', inject(function ($rootScope, spEntityService) {

            var results = {
                data: {
                    results: [
                        { code: 200, ids: [123], name:'single', single:true },
                        { code: 200, ids: [456], name:'arrayImplicit' },
                        { code: 200, ids: [789], name:'arrayExplicit', single:false }
                    ],
                    entities: {
                        '123': { '77': 'aa' },
                        '456': { '77': 'bb' },
                        '789': { '77': 'cc' }
                    },
                    members: {
                        '77': { alias: 'core:name', dt: 'String' },
                        '88': { alias: 'core:description', dt: 'String' }
                    }
                }
            };
            
            var namedQueries = spEntityService.extractNamedQueriesFromBatch(results);
            
            expect(namedQueries.single).toBeTruthy();
            expect(namedQueries.single.getName()).toEqual('aa');
            expect(namedQueries.arrayImplicit).toBeTruthy();
            expect(namedQueries.arrayImplicit.length).toEqual(1);
            expect(namedQueries.arrayImplicit[0].getName()).toEqual('bb');
            expect(namedQueries.arrayExplicit).toBeTruthy();
            expect(namedQueries.arrayExplicit.length).toEqual(1);
            expect(namedQueries.arrayExplicit[0].getName()).toEqual('cc');

            
            }));
    });

});
