// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, sp, spEntity */

describe('Entity Model|spEntityService|Batching|intg:', function () {
    'use strict';

    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spEntityService'));


    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    describe('automatic batching', function () {

        // Introduction Tests

        it('should run batched when batching enabled', inject(function (spEntityService) {

            var options = { batch: true };
            var p1 = spEntityService.getEntity('test:af01', 'name', options);
            var p2 = spEntityService.getEntity('test:af02', 'description', options);

            var result1 = {};
            var result2 = {};
            TestSupport.waitCheckReturn(p1, result1);
            TestSupport.waitCheckReturn(p2, result2);

            runs(function () {
                expect(result1.value.getName()).toBeDefined('Test 01');
                expect(result2.value.getDescription()).toBeDefined('Description of Test 2');
            });
        }));
    });

    describe('manual batching', function () {

        // Introduction Tests

        it('should run batched when batching enabled', inject(function (spEntityService) {

            var batch = new spEntityService.BatchRequest();
            var r1 = spEntityService.makeGetEntityRequest('test:af01', 'name');
            var r2 = spEntityService.makeGetEntitiesRequest(['test:af02', 'test:af03'], 'description');
            var r3 = spEntityService.makeGetEntitiesOfTypeRequest('test:person', 'test:age');
            batch.addRequest(r1);
            batch.addRequest(r2);
            batch.addRequest(r3);
            batch.runBatch();

            var result1 = {};
            var result2 = {};
            var result3 = {};
            TestSupport.waitCheckReturn(r1.promise, result1);
            TestSupport.waitCheckReturn(r2.promise, result2);
            TestSupport.waitCheckReturn(r3.promise, result3);

            runs(function () {
                expect(result1.value.getName()).toBeDefined('Test 01');
                expect(result2.value[0].getDescription()).toBeDefined('Description of Test 2');
                expect(result3.value.length).toBeGreaterThan(10);
            });
        }));

        it('should merge fields', inject(function (spEntityService) {

            var options = { batch: true };
            var p1 = spEntityService.getEntity('test:af03', 'name,test:afNumber', options);
            var p2 = spEntityService.getEntity('test:af03', 'name,description', options);

            var result1 = {};
            var result2 = {};
            TestSupport.waitCheckReturn(p1, result1);
            TestSupport.waitCheckReturn(p2, result2);

            runs(function () {
                expect(result1.value).toBe(result2.value);
                expect(result1.value.getName()).toBe('Test 03');
                expect(result1.value.getAfNumber()).toBe(300);
                expect(result2.value.getDescription()).toBe('Description of Test 3');
            });
        }));

        it('should work with $q.all', inject(function ($q, spEntityService) {

            var batch = new spEntityService.BatchRequest();
            var r1 = spEntityService.getEntity('test:af01', 'name', { batch: batch });
            var r2 = spEntityService.getEntities(['test:af02', 'test:af03'], 'description', { batch: batch });
            var r3 = spEntityService.getEntitiesOfType('test:person', 'test:age', { batch: batch });

            batch.runBatch();

            var promise = $q.all({
                result1: r1,
                result2: r2,
                result3: r3
            });

            var result = {};
            TestSupport.waitCheckReturn(promise, result);

            runs(function () {
                var res = result.value;
                expect(res.result1.getName()).toBeDefined('Test 01');
                expect(res.result2[0].getDescription()).toBeDefined('Description of Test 2');
                expect(res.result3.length).toBeGreaterThan(10);
            });
        }));

    });

});
