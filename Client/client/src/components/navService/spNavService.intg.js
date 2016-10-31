// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, sp */

describe('Console|Navigation|navService|intg:', function () {
    "use strict";

    beforeEach(module('mod.common.spWebService'));
    beforeEach(module('mod.services.promiseService'));
    beforeEach(module('sp.navService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    // DISABLED - has become unreliable after Jasmine 1.31 upgrade
    xit('spNavDataService can load the top of the navigation tree', inject(function (spNavDataService) {
        var results = {}, tree;

        expect(spNavDataService).toBeTruthy();

        TestSupport.waitCheckReturn(spNavDataService.getNavItems(), results);

        runs(function () {
            expect(results).toBeTruthy();
            expect(results.value).toBeTruthy();

            tree = results.value;

            // root node has null item
            expect(tree.item.id).toBeUndefined();
            // root node should have some children
            expect(tree.children).toBeTruthy();
            expect(tree.children.length).toBeGreaterThan(0);

            // do a couple of data checks based on the known actual data
            expect(_(tree.children).map('item').any({name: 'Test Solution'})).toBeTruthy();
        });
    }));

    // DISABLED - has become unreliable after Jasmine 1.31 upgrade
    xit('spNavDataService can load a portion of the tree to effect an "expand" operation', inject(function (spNavDataService) {
        var results = {}, tree;

        expect(spNavDataService).toBeTruthy();

        TestSupport.waitCheckReturn(spNavDataService.getNavTreeExpanded([
            {id: 'test:employeeReport1'}
        ]), results);

        runs(function () {
            expect(results).toBeTruthy();
            expect(results.value).toBeTruthy();

            tree = results.value;
            console.log('tree returned', tree);

            // the tree returned by this is an actual item
            expect(tree.item).not.toBeNull();

            // root node should have some children
            expect(tree.children).toBeTruthy();
            expect(tree.children.length).toBeGreaterThan(0);

            // do a couple of data checks based on the known actual data
            expect(_(tree.children).map('item').any({name: 'Test Solution'})).toBeTruthy();
            expect(tree.children[0].children.length).toBeGreaterThan(0);
        });
    }));

    //having issues loading the $location service so this is not included at the moment
    // DISABLED - has become unreliable after Jasmine 1.31 upgrade
    xit('spNavService can be loaded', inject(function ($rootScope, spNavService, spPromiseService) {
        var tree;

        expect(spNavService).toBeTruthy();

        tree = spNavService.getNavTree();

        expect(tree).toBeTruthy();
        expect(tree.children).toBeTruthy();
        expect(tree.children.length).toBe(0);

        $rootScope.$broadcast('signedin');

        var result = {};
        TestSupport.waitCheckReturn(
            spPromiseService.when(spNavService, 'getNavTree.children', sp.isNonEmptyArray),
            result);

        var done = false;

        runs(function () {
            tree = spNavService.getNavTree();

            expect(tree.children.length).toBeGreaterThan(0);

            expect(_(tree.children).map('item').any({name: 'Test Solution'})).toBeTruthy();
            expect(tree.children[0].children.length).toBeGreaterThan(0);
        });
    }));

});
