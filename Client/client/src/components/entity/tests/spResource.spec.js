// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity, spResource */

describe('Entity Model|spResource|spec:', function () {

    beforeEach(module('ng'));
    beforeEach(module('mod.common.spResource'));

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });

    describe('getAncestorsAndSelf', function () {

        it('should return empty array for empty input', inject(function (spResource) {
            expect(spResource.getAncestorsAndSelf()).toBeEmptyArray();
            expect(spResource.getAncestorsAndSelf(null)).toBeEmptyArray();
        }));

        it('should work for type without inherits data', inject(function (spResource) {
            var t1 = spEntity.fromId(123);
            var res = spResource.getAncestorsAndSelf(t1);
            expect(res).toBeArray(1);
            expect(res[0].id()).toBe(123);
        }));

        it('should work for simple inheritance chain', inject(function (spResource) {
            var json = {
                id: 1,
                inherits: [{
                    id: 2,
                    inherits: [{ id: 3 }]
                }]
            };
            var type = spEntity.fromJSON(json);
            var res = spResource.getAncestorsAndSelf(type);
            expect(res).toBeArray(3);
            expect(res[0].id()).toBe(1, 't1');
            expect(res[1].id()).toBe(2, 't2');
            expect(res[2].id()).toBe(3, 't3');
        }));

        it('should work for complex inheritance chain', inject(function (spResource) {
            var t4 = spEntity.fromId(4);
            var json = {
                id: 1,
                inherits: [
                    { id: 2, inherits: [t4] },
                    { id: 3, inherits: [t4] }
                ]
            };
            var t1 = spEntity.fromJSON(json);

            var res = spResource.getAncestorsAndSelf(t1);
            expect(res).toBeArray(4);
            expect(res[0].id()).toBe(1, 't1');
            expect(res[1].id()).toBe(2, 't2');
            expect(res[2].id()).toBe(3, 't3');
            expect(res[3].id()).toBe(4, 't4');
        }));
    });

    describe('getDerivedTypesAndSelf', function () {

        it('should return empty array for empty imput', inject(function (spResource) {
            expect(spResource.getDerivedTypesAndSelf()).toBeEmptyArray();
            expect(spResource.getDerivedTypesAndSelf(null)).toBeEmptyArray();
        }));

        it('should work for type without inherits data', inject(function (spResource) {
            var t1 = spEntity.fromId(123);
            var res = spResource.getDerivedTypesAndSelf(t1);
            expect(res).toBeArray(1);
            expect(res[0].id()).toBe(123);
        }));

        it('should work for simple inheritance chain', inject(function (spResource) {
            var json = {
                id: 1,
                derivedTypes: [{
                    id: 2,
                    derivedTypes: [{ id: 3 }]
                }]
            };
            var t1 = spEntity.fromJSON(json);

            var res = spResource.getDerivedTypesAndSelf(t1);
            expect(res).toBeArray(3);
            expect(res[0].id()).toBe(1, 't1');
            expect(res[1].id()).toBe(2, 't2');
            expect(res[2].id()).toBe(3, 't3');
        }));

        it('should work for complex inheritance chain', inject(function (spResource) {
            var t4 = spEntity.fromId(4);
            var json = {
                id: 1,
                derivedTypes: [
                    { id: 2, derivedTypes: [t4] },
                    { id: 3, derivedTypes: [t4] }
                ]
            };
            var t1 = spEntity.fromJSON(json);

            var res = spResource.getDerivedTypesAndSelf(t1);
            expect(res).toBeArray(4);
            expect(res[0].id()).toBe(1, 't1');
            expect(res[1].id()).toBe(2, 't2');
            expect(res[2].id()).toBe(3, 't3');
            expect(res[3].id()).toBe(4, 't4');
        }));
    });

    describe('makeTypeRequest', function () {

        it('should run with defaults', inject(function (spResource) {
            var str = spResource.makeTypeRequest();
            expect(str).toBeTruthy();
        }));

        it('should run with specified options', inject(function (spResource) {
            var str = spResource.makeTypeRequest({
                fields: false,
                relationships: true,
                fieldGroups: false
            });
            expect(str).toBeTruthy();
        }));

    });

});
