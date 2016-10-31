// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, TestSupport */

describe('Console|Workflow|intg:', function () {
    'use strict';

    var waitCheckReturn = TestSupport.waitCheckReturn;

    beforeEach(module('mod.app.workflow'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    describe('workflowDataChooserService', function () {

        it('800: service created', inject(function (spWorkflowChooserDataService) {
            expect(spWorkflowChooserDataService).toBeTruthy();
        }));


        it('810: getTypes', inject(function (spWorkflowChooserDataService) {
        
            var promise = spWorkflowChooserDataService.getTypes();
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;
                resultsOk(results);
                expect(results.data.length).toBe(100);     // only 100 results should be shown
                expect(results.cols.length).toBe(2);

            });

        }));

        it('811: getTypes filtered', inject(function (spWorkflowChooserDataService) {

            var promise = spWorkflowChooserDataService.getTypes("AA_All Fields");
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;
                resultsOk(results);
                expect(results.data.length).toBe(1);     // only 1 result
                expect(results.data[0].item[0].value).toBe('AA_All Fields');     // only 1 result
            });

        }));

        it('820: getFields', inject(function (spWorkflowChooserDataService) {
        
            var promise = spWorkflowChooserDataService.getFields('test:allFields');
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;
                resultsOk(results);
                expect(results.data.length).toBeGreaterThan(5);    
                expect(results.cols.length).toBe(2);

            });

        }));

        it('821: getFields filtered', inject(function (spWorkflowChooserDataService) {

            var promise = spWorkflowChooserDataService.getFields('test:allFields', 'Name');
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;

                resultsOk(results);
                expect(results.data.length).toBe(1);     // only 1 result
                expect(results.data[0].item[0].value).toBe('Name');     // only 1 result

            });

        }));

        it('830: getResources', inject(function (spWorkflowChooserDataService) {

            var promise = spWorkflowChooserDataService.getResources('test:allFields');
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;
                resultsOk(results);
                expect(results.data.length).toBeGreaterThan(1);
                expect(results.cols.length).toBe(3);

            });

        }));

        it('831: getResources filtered', inject(function (spWorkflowChooserDataService) {

            var promise = spWorkflowChooserDataService.getResources('test:allFields', 'Test 01');
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;

                resultsOk(results);
                expect(results.data.length).toBe(1);     // only 1 result
                expect(results.data[0].item[0].value).toBe('Test 01');     // only 1 result

            });

        }));

        it('840: getRelationships', inject(function (spWorkflowChooserDataService) {

            var promise = spWorkflowChooserDataService.getRelationships('test:allFields');
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;
                resultsOk(results);
                expect(results.data.length).toBeGreaterThan(5);
                expect(results.cols.length).toBe(3);

            });

        }));

        it('841: getRelationships filtered', inject(function (spWorkflowChooserDataService) {

            var promise = spWorkflowChooserDataService.getRelationships('test:allFields', 'AA_Drinks');
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;

                resultsOk(results);
                expect(results.data.length).toBe(1);     // only 1 result
                expect(results.data[0].item[0].value).toBe('AA_Drinks');     // only 1 result

            });

        }));

        it('850: getFunctions', inject(function (spWorkflowChooserDataService) {

            var promise = spWorkflowChooserDataService.getFunctions();
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;
                resultsOk(results);
                expect(results.data.length).toBeGreaterThan(5);
                expect(results.cols.length).toBe(3);

            });

        }));

        it('851: getFunctions filtered', inject(function (spWorkflowChooserDataService) {

            var promise = spWorkflowChooserDataService.getFunctions('sum');
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;

                resultsOk(results);
                expect(results.data.length).toBe(1);     // only 1 result
                expect(results.data[0].item[0].value).toBe('sum');     // only 1 result

            });

        }));

        it('850: getReports', inject(function (spWorkflowChooserDataService) {

            var promise = spWorkflowChooserDataService.getReports('test:allFields');
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;
                resultsOk(results);
                expect(results.data.length).toBeGreaterThan(1);
                expect(results.cols.length).toBe(2);

            });

        }));

        it('851: getReports limited', inject(function (spWorkflowChooserDataService) {

            var promise = spWorkflowChooserDataService.getReports('core:resource');
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;
                resultsOk(results);
                expect(results.data.length).toBe(100);
            });

        }));

        it('852: getReports filtered', inject(function (spWorkflowChooserDataService) {

            var promise = spWorkflowChooserDataService.getReports('test:allFields','Images');
            var result = {};

            waitCheckReturn(promise, result);

            runs(function () {
                var results = result.value.results;

                resultsOk(results);
                expect(results.data.length).toBe(1);     // only 1 result
                expect(results.data[0].item[0].value).toBe('Images');     // only 1 result

            });

        }));
        function resultsOk(results) {
            expect(results).toBeTruthy();
            expect(results).toBeTruthy();
            expect(results.data).toBeTruthy();
            expect(results.data.length).toBeTruthy();

            expect(results.cols).toBeTruthy();
            expect(results.cols.length).toBeGreaterThan(0);

        }
    });
});