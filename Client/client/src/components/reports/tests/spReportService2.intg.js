// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, sp */

describe('Reports|spReportService2|intg:', function () {
    'use strict';

    beforeEach(module('sp.common.loginService'));
    beforeEach(module('spApps.reportServices'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    it('100: runDefaultReportForType() works', inject(function (spReportService) {

        TestSupport.wait(
            spReportService.runDefaultReportForType({ ns: 'oldshared', alias: 'employee' })
            .then(function (data) {
                expect(data).toBeReportResultsWithData();
            }));
    }));

    it('300: runPickerReport(\'resources\', \'test:employee\') works', inject(function (spReportService) {

        TestSupport.wait(
            spReportService.runPickerReport('resources', { resourceType: { value: 'test:employee' }})
            .then(function (data) {
                expect(data).toBePickerReportResultsWithData();
            }));
    }));

    it('300: runPickerReport(\'resources\', \'core:resource\') works', inject(function (spReportService) {

        //TestSupport.wait(
        //    spReportService.runPickerReport('resources', { resourceType: { value: 'core:resource' }})
        //    .then(function (data) {
        //        expect(data).toBePickerReportResultsWithData();
        //        expect(data.results.data).toBeArray();
        //        expect(data.results.data.length).toBeGreaterThan(0);
        //    }));


        //TestSupport.waitCheckReturn(spReportService.runPickerReport('resources', { resourceType: { value: 'core:resource' } })
        //    .then(function (data) {
        //        expect(data).toBePickerReportResultsWithData();
        //        expect(data.results.data).toBeArray();
        //        expect(data.results.data.length).toBeGreaterThan(0);
        //    }), {}, null, null, 100000);
    }));

    it('400: runPickerReport(\'resources\', \'core:enumType\') works', inject(function (spReportService) {

        TestSupport.wait(
            spReportService.runPickerReport('resources', { resourceType: { value: 'core:enumType' }})
            .then(function (data) {
                expect(data).toBePickerReportResultsWithData();
                expect(data.results.data).toBeArray();
                expect(data.results.data.length).toBeGreaterThan(0);
            }));
    }));

    it('410: runPickerReport(\'resources\', \'test:condiments\') works', inject(function (spReportService) {

        TestSupport.wait(
            spReportService.runPickerReport('resources', { resourceType: { value: 'test:condimentsEnum' }})
            .then(function (data) {
                expect(data).toBePickerReportResultsWithData();
                expect(data.results.data).toBeArray();
                expect(data.results.data.length).toBeGreaterThan(0);
            }));
    }));

});

