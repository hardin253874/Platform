// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, sp */

describe('Console|CalcWebService|intg:', function () {
    "use strict";

    var waitCheckReturn = TestSupport.waitCheckReturn,
        getTestEntityName = TestSupport.getTestEntityName,
        getUpdatedTestEntityName = TestSupport.getUpdatedTestEntityName;

    function getTestId(test) {
        return test.description.split(':')[0];
    }

    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spEntityService'));
    beforeEach(module('sp.common.spCalcEngineService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    it('100: test we can compile a simple expression', inject(function (spCalcEngineService) {
        var testId = getTestId(this);

        var promise = spCalcEngineService.compileExpression("'hello world ' + [p1]", null, null, [
            { name: 'p1', type: 'String' }
        ]);

        var result = {};
        waitCheckReturn(promise, result);

        runs(function () {
            console.log('test %s: result', testId, result);
            expect(result.value).toBeTruthy();

            var compileResult = result.value.data;
            expect(compileResult).toBeTruthy();
            expect(compileResult.resultType).toBe('String');
            expect(compileResult.error).toBeFalsy();
        });

    }));

    it('110: test we can compile list based expression', inject(function (spCalcEngineService) {
        var testId = getTestId(this);

        var promise = spCalcEngineService.compileExpression("[p1]", null, null, [
            { name: 'p1', type: 'String', isList: true }
        ]);

        var result = {};
        waitCheckReturn(promise, result);

        runs(function () {
            console.log('test %s: result', testId, result);
            expect(result.value).toBeTruthy();

            var compileResult = result.value.data;
            expect(compileResult).toBeTruthy();
            expect(compileResult.resultType).toBe('String');
            expect(compileResult.isList).toBeTruthy();
            expect(compileResult.error).toBeFalsy();
        });

    }));

    it('120: test we can compile a resource expression', inject(function (spEntityService, spCalcEngineService) {
        var testId = getTestId(this);

        var testEntity;

        var promise = spEntityService.getEntity('test:steveGibbon', 'name,oldshared:age,isOfType.name')
            .then(function (entity) {
                testEntity = entity;
                return spCalcEngineService.compileExpression("[p1].Name + ' has age ' + [p1].Age", null, null, [
                    { name: 'p1', type: 'Entity', entityTypeId: _.first(testEntity.isOfType).id().toString() }
                ]);
            });

        var result = {};
        waitCheckReturn(promise, result);

        runs(function () {
            console.log('test %s: result', testId, result);
            expect(result.value).toBeTruthy();

            var evalResult = result.value.data;
            expect(evalResult).toBeTruthy();
            expect(evalResult.resultType).toBe('String');
            expect(evalResult.error).toBeFalsy();
        });
    }));


    it('120: test we can compile another resource expression', inject(function (spEntityService, spCalcEngineService) {
        var testId = getTestId(this);

        var testEntity;

        var promise = spEntityService.getEntity('test:steveGibbon', 'name,oldshared:age,isOfType.{alias,name}')
            .then(function (entity) {
                testEntity = entity;
                return spCalcEngineService.compileExpression("[p1].Age", null, null, [
                    { name: 'p1', type: 'Entity', entityTypeId: _.first(testEntity.isOfType).id().toString() }
                ]);
            });

        var result = {};
        waitCheckReturn(promise, result);

        runs(function () {
            console.log('test %s: result', testId, result);
            expect(result.value).toBeTruthy();

            var evalResult = result.value.data;
            expect(evalResult).toBeTruthy();
            expect(evalResult.resultType).toBe('Int32');
            expect(evalResult.error).toBeFalsy();
        });
    }));

    it('130: test we can compile another resource expression', inject(function ($q, spEntityService, spCalcEngineService) {
        var testId = getTestId(this);

        var testEntity, managerTypeEntity;

        var promise = $q.all([
            spEntityService.getEntity('test:steveGibbon', 'name,oldshared:age,isOfType.{alias,name}'),
            spEntityService.getEntity('test:peterAylett', 'isOfType.{alias,name}')
        ])
            .then(function (entities) {
                testEntity = entities[0];
                managerTypeEntity = entities[1].isOfType[0];

                return spCalcEngineService.compileExpression("[p1].Manager", null, null, [
                    { name: 'p1', type: 'Entity', entityTypeId: _.first(testEntity.isOfType).id().toString() }
                ]);
            });

        var result = {};
        waitCheckReturn(promise, result);

        runs(function () {
            console.log('test %s: result', testId, result);
            expect(result.value).toBeTruthy();

            var evalResult = result.value.data;
            expect(evalResult).toBeTruthy();
            expect(evalResult.resultType).toBe('Entity');
            expect(evalResult.entityTypeId).toBe(managerTypeEntity.idP);
            expect(evalResult.error).toBeFalsy();
        });
    }));


    it('140: test we can compile another resource expression using alias for type id', inject(function (spEntityService, spCalcEngineService) {
        var testId = getTestId(this);

        var testEntity;

        var promise = spEntityService.getEntity('test:steveGibbon', 'name,oldshared:age,isOfType.{alias,name}')
            .then(function (entity) {
                testEntity = entity;
                return spCalcEngineService.compileExpression("[p1].Age", null, null, [
                    { name: 'p1', type: 'Entity', entityTypeId: testEntity.type.nsAlias }
                ]);
            });

        var result = {};
        waitCheckReturn(promise, result);

        runs(function () {
            console.log('test %s: result', testId, result);
            expect(result.value).toBeTruthy();

            var evalResult = result.value.data;
            expect(evalResult).toBeTruthy();
            expect(evalResult.resultType).toBe('Int32');
            expect(evalResult.error).toBeFalsy();
        });
    }));

    it('200: test we can eval a simple expression', inject(function (spCalcEngineService) {
        var testId = getTestId(this);

        var promise = spCalcEngineService.evalExpression("'hello world ' + [p1]", null, null, [
            { name: 'p1', type: 'String', value: 'let\'s get cooking' }
        ]);

        var result = {};
        waitCheckReturn(promise, result);

        runs(function () {
            console.log('test %s: result', testId, result);
            expect(result.value).toBeTruthy();

            var evalResult = result.value.data;
            expect(evalResult).toBeTruthy();
            expect(evalResult.resultType).toBe('String');
            expect(evalResult.error).toBeFalsy();
            expect(evalResult.value).toBe('hello world let\'s get cooking');

        });

    }));

    it('210: test we can eval another simple expression', inject(function (spCalcEngineService) {
        var testId = getTestId(this);

        var promise = spCalcEngineService.evalExpression("10 * [p1]", null, null, [
            { name: 'p1', type: 'Decimal', value: '99' }
        ]);

        var result = {};
        waitCheckReturn(promise, result);

        runs(function () {
            console.log('test %s: result', testId, result);
            expect(result.value).toBeTruthy();

            var evalResult = result.value.data;
            expect(evalResult).toBeTruthy();
            expect(evalResult.resultType).toBe('Decimal');
            expect(evalResult.error).toBeFalsy();
            expect(evalResult.value).toBe('990');
        });
    }));

    it('220: test we can eval a resource expression', inject(function (spEntityService, spCalcEngineService) {
        var testId = getTestId(this);

        var testEntity;

        var promise = spEntityService.getEntity('test:steveGibbon', 'name,oldshared:age,isOfType.name')
            .then(function (entity) {
                testEntity = entity;
                return spCalcEngineService.evalExpression("[p1].Name + ' has age ' + [p1].Age", null, null, [
                    { name: 'p1', type: 'Entity', entityTypeId: _.first(testEntity.isOfType).id().toString(), value: testEntity.id().toString() }
                ]);
            });

        var result = {};
        waitCheckReturn(promise, result);

        runs(function () {
            console.log('test %s: result', testId, result);
            expect(result.value).toBeTruthy();

            var evalResult = result.value.data;
            expect(evalResult).toBeTruthy();
            expect(evalResult.resultType).toBe('String');
            expect(evalResult.error).toBeFalsy();
            expect(evalResult.value).toBe(testEntity.name + ' has age ' + testEntity.age);
        });
    }));

    it('230: test we can eval an expression using parameters that need evals', inject(function (spEntityService, spCalcEngineService) {
        var testId = getTestId(this);

        var testEntity;

        var promise = spEntityService.getEntity('test:steveGibbon', 'name,oldshared:age,isOfType.name')
            .then(function (entity) {
                testEntity = entity;
                return spCalcEngineService.evalExpression("[p1].Name + ' has age ' + [p1].Age + ' to inc to ' + [new age]", null, null, [
                    // the odd ordering here and the seemingly unused param is here to test that the server deals with params out of order
                    { name: 'another new age', type: 'Int32', expr: '[new age]' },
                    { name: 'p1', type: 'Entity', entityTypeId: _.first(testEntity.isOfType).id().toString(), value: testEntity.id().toString() },
                    { name: 'new age', type: 'Int32', expr: 'p1.Age + 1' }
                ]);
            });

        var result = {};
        waitCheckReturn(promise, result);

        runs(function () {
            console.log('test %s: result', testId, result);
            expect(result.value).toBeTruthy();

            var evalResult = result.value.data;
            expect(evalResult).toBeTruthy();
            expect(evalResult.resultType).toBe('String');
            expect(evalResult.error).toBeFalsy();
            expect(evalResult.value).toBe(testEntity.name + ' has age ' + testEntity.age + ' to inc to ' + (testEntity.age + 1));
        });
    }));

});
