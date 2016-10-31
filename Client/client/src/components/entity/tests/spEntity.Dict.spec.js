// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity */

describe('Entity Model|spEntity|spec:', function () {
    'use strict';

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });


    describe('spEntity.Dict', function () {

        // Introduction Tests

        it('can add & get by ID', function () {
            var dict = new spEntity.Dict();
            dict.add(123, 'abc');
            var res = dict.get(123);
            expect(res).toEqual('abc');
        });

        it('can add & get by alias1', function () {
            var dict = new spEntity.Dict();
            dict.add('hello', 'abc');
            var res = dict.get('core:hello');
            expect(res).toEqual('abc');
        });

        it('can add & get by alias2', function () {
            var dict = new spEntity.Dict();
            dict.add('hello', 'abc');
            var res = dict.get('hello');
            expect(res).toEqual('abc');
        });

        it('contains works', function () {
            var dict = new spEntity.Dict();
            dict.add('hello', 123);
            var res = dict.contains('hello');
            expect(res).toBeTruthy();
        });

        it('contains works in case not found', function () {
            var dict = new spEntity.Dict();
            dict.add('hello', 123);
            var res = dict.contains('hello2');
            expect(res).toBeFalsy();
        });

        it('can be used as a set', function () {
            var dict = new spEntity.Dict();
            dict.add('hello');
            var res = dict.contains('hello');
            expect(res).toBeTruthy();
        });

        it('returns the key when used as a set', function () {
            var dict = new spEntity.Dict();
            var e = spEntity.fromId('hello');
            dict.add(e);
            var res = dict.get('hello');
            expect(res).toBe(e);
        });

        it('remove works', function () {
            var dict = new spEntity.Dict();
            dict.add('hello', 123);
            dict.add(456, 456);
            expect(dict.contains('hello')).toBeTruthy();
            expect(dict.contains(456)).toBeTruthy();
            dict.remove('hello');
            dict.remove(456);
            expect(dict.contains('hello')).toBeFalsy();
            expect(dict.contains(456)).toBeFalsy();
        });

        it('values works', function () {
            var dict = new spEntity.Dict();
            dict.add('1', 'a');
            dict.add('2', 'b');
            var res = dict.values();
            res.sort();
            expect(res.length).toEqual(2);
            expect(res[0]).toEqual('a');
            expect(res[1]).toEqual('b');
        });

    });

});