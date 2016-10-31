// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport */

describe('Services|spPromiseService|spec:', function () {
    'use strict';

    beforeEach(module('mod.services.promiseService'));

    beforeEach(inject(function ($rootScope) {
        this.addMatchers(TestSupport.matchers);
        TestSupport.setScope($rootScope);
    }));

    it('can load', inject(function (spPromiseService) {
        expect(spPromiseService).toBeTruthy();
        expect(_.isFunction(spPromiseService.when)).toBeTruthy();
    }));

    it('should resolve immediately on a property that has a value', inject(function ($rootScope, spPromiseService) {

        var obj = {
            a: 33
        };

        var result = {};
        spPromiseService.when(obj, 'a').then(function (value) {
            result.value = value;
        });

        $rootScope.$apply();

        expect(result.value).toBe(33);
    }));

    it('should resolve eventually on a property that is initially undefined and then gets a value', inject(function ($rootScope, $timeout, spPromiseService) {

        var obj = {
        };

        var result = {};
        spPromiseService.when(obj, 'a').then(function (value) {
            result.value = value;
        });

        expect(result.value).toBeUndefined();

        // verify some internals
        expect(_.keys(spPromiseService.__watches).length).toBe(1);

        $timeout(function () {
            console.log('setting obj.a');
            obj.a = 99;
        });
        $timeout.flush();
        $rootScope.$apply();

        expect(result.value).toBe(99);

        // verify some internals - importantly that we have cleaned up
        expect(_.keys(spPromiseService.__watches).length).toBe(0);

    }));

    it('should see calls to when() resolve immediately after a prop has been set', inject(function ($rootScope, $timeout, spPromiseService) {

        var obj = {
        };

        var result = {};
        spPromiseService.when(obj, 'someFunction').then(function (value) {
            result.value = value;
        });

        expect(result.value).toBeUndefined();

        // verify some internals
        expect(_.keys(spPromiseService.__watches).length).toBe(1);

        $timeout(function () {
            console.log('setting obj.someFunction = ...');
            obj.someFunction = function () {
                return 'hey';
            };
        });
        $timeout.flush();
        $rootScope.$apply();

        expect(result.value).toBe('hey');

        // verify some internals - importantly that we have cleaned up
        expect(_.keys(spPromiseService.__watches).length).toBe(0);

        result = {};
        spPromiseService.when(obj, 'someFunction').then(function (value) {
            result.value = value;
        });

        $rootScope.$apply();
        expect(result.value).toBe('hey');
    }));

    it('should treat empty array as already initialised using default isInitialised predicate', inject(function ($rootScope, $timeout, spPromiseService) {

        var obj = {
            list: []
        };

        var result = {};
        spPromiseService.when(obj, 'list').then(function (value) {
            result.value = value;
        });

        expect(result.value).toBeUndefined();

        // promise should have resolved as an empty array is not 'undefined'
        $rootScope.$apply();
        expect(result.value).toBeArray();
        expect(result.value.length).toBe(0);

        $timeout(function () {
            console.log('setting obj.list');
            obj.list = [1, 2, 3];
        });
        $timeout.flush();

        // even though we just set a new array, the promise already happened
        $rootScope.$apply();
        expect(result.value).toBeArray();
        expect(result.value.length).toBe(0);

        result = {};
        spPromiseService.when(obj, 'list').then(function (value) {
            result.value = value;
        });

        // asking for the promise again will get an immediately resolved promise on the current value
        $rootScope.$apply();
        expect(result.value).toBeArray();
        expect(result.value.length).toBe(3);
    }));


    it('should accept isInitialised predicate', inject(function ($rootScope, $timeout, spPromiseService) {

        function isInitialised(a) {
            return a && _.isArray(a) && a.length > 0;
        }

        var obj = {
            list: []
        };

        var result = {};
        spPromiseService.when(obj, 'list', isInitialised).then(function (value) {
            result.value = value;
        });

        expect(result.value).toBeUndefined();

        // promise should NOT have resolved as an empty array is not uninitialised according to our function
        $rootScope.$apply();
        expect(result.value).toBeUndefined();

        $timeout(function () {
            console.log('setting obj.list');
            obj.list = [1, 2, 3];
        });
        $timeout.flush();
        $rootScope.$apply();

        expect(result.value).toBe(obj.list);
    }));
});