// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport */

describe('Charts|spec|spChartService', function () {

    beforeEach(module('mod.common.ui.spChartService'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedReportService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('exists', inject(function (spChartService) {
        expect(spChartService).toBeTruthy();
    }));

    describe('chart sources', function () {

        it('getChartSourceTypes returns array', inject(function (spChartService) {
            var csts = spChartService.getChartSourceTypes();
            expect(csts).toBeArray();
            expect(csts.length).toBeGreaterThan(0);
        }));

        it('getChartSourceTypes contents are valid', inject(function (spChartService) {
            var csts = spChartService.getChartSourceTypes();
            _.forEach(csts, function (cst) {
                expect(cst.alias).toBeTruthy();
                expect(cst.name).toBeTruthy();
                expect(_.isFunction(cst.display)).toBeTruthy();
                expect(_.isFunction(cst.displayCt)).toBeTruthy();
            });
        }));

        it('getChartSourceTypes display functions run', inject(function (spChartService) {
            var csts = spChartService.getChartSourceTypes();
            _.forEach(csts, function (cst) {
                cst.displayCt('lineChart');
            });
        }));
    });

    describe('chart types', function () {

        it('getChartTypes can get charts', inject(function (spChartService) {
            expect(spChartService.getChartType('lineChart')).toBeTruthy();
            expect(spChartService.getChartType('core:lineChart')).toBeTruthy();
        }));

        it('color disallowed for line and area', inject(function (spChartService) {
            expect(spChartService.getChartType('lineChart').disallowNegColor).toBeTruthy();
            expect(spChartService.getChartType('areaChart').disallowNegColor).toBeTruthy();
        }));
    });


    describe('ruleToLegendEntry', function () {

        it('is false', inject(function (spChartService) {
            var res = spChartService.ruleToLegendEntry({ oper: 'IsFalse' });
            expect(res.text).toBe('No');
        }));

        it('contains', inject(function (spChartService) {
            var res = spChartService.ruleToLegendEntry({ oper: 'Contains', val: 'Test' });
            expect(res.text).toBe('...Test...');
        }));

        it('any of', inject(function (spChartService) {
            var res = spChartService.ruleToLegendEntry({ oper: 'AnyOf', vals: ['A', 'B'] });
            expect(res.text).toBe('A, B');
        }));
    });


    describe('supportedScales', function () {

        it('gives linear for Int32 barChart', inject(function (spChartService) {
            var res = spChartService.supportedScales('Int32', 'barChart');
            expect(res.linearScaleType).toBeTruthy();
            expect(res.dateTimeScaleType).toBeFalsy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('linearScaleType');
        }));

        it('gives linear for Decimal barChart', inject(function (spChartService) {
            var res = spChartService.supportedScales('Decimal', 'barChart');
            expect(res.linearScaleType).toBeTruthy();
            expect(res.dateTimeScaleType).toBeFalsy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('linearScaleType');
        }));

        it('gives linear for Currency lineChart', inject(function (spChartService) {
            var res = spChartService.supportedScales('Decimal', 'lineChart');
            expect(res.linearScaleType).toBeTruthy();
            expect(res.dateTimeScaleType).toBeFalsy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('linearScaleType');
        }));

        it('gives linear for Date columnChart', inject(function (spChartService) {
            // note: can be overrridden if a format is specified
            var res = spChartService.supportedScales('Date', 'columnChart');
            expect(res.linearScaleType).toBeTruthy();
            expect(res.dateTimeScaleType).toBeTruthy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('dateTimeScaleType');
        }));

        it('gives linear for DateTime columnChart', inject(function (spChartService) {
            // note: can be overrridden if a format is specified
            var res = spChartService.supportedScales('DateTime', 'columnChart');
            expect(res.linearScaleType).toBeTruthy();
            expect(res.dateTimeScaleType).toBeTruthy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('dateTimeScaleType');
        }));

        it('gives linear for Time columnChart', inject(function (spChartService) {
            // note: can be overrridden if a format is specified
            var res = spChartService.supportedScales('Time', 'columnChart');
            expect(res.linearScaleType).toBeTruthy();
            expect(res.dateTimeScaleType).toBeTruthy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('dateTimeScaleType');
        }));

        it('gives category for Int32 matrixChart', inject(function (spChartService) {
            var res = spChartService.supportedScales('Int32', 'matrixChart');
            expect(res.linearScaleType).toBeFalsy();
            expect(res.dateTimeScaleType).toBeFalsy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('categoryScaleType');
        }));

        it('gives category for string bubbleChart', inject(function (spChartService) {
            var res = spChartService.supportedScales('String', 'bubbleChart');
            expect(res.linearScaleType).toBeFalsy();
            expect(res.dateTimeScaleType).toBeFalsy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('categoryScaleType');
        }));

        it('gives category for bool columnChart', inject(function (spChartService) {
            var res = spChartService.supportedScales('Bool', 'columnChart');
            expect(res.linearScaleType).toBeFalsy();
            expect(res.dateTimeScaleType).toBeFalsy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('categoryScaleType');
        }));

        it('gives category for choice columnChart', inject(function (spChartService) {
            var res = spChartService.supportedScales('ChoiceRelationship', 'columnChart');
            expect(res.linearScaleType).toBeFalsy();
            expect(res.dateTimeScaleType).toBeFalsy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('categoryScaleType');
        }));

        it('gives category for relationship columnChart', inject(function (spChartService) {
            var res = spChartService.supportedScales('InlineRelationship', 'columnChart');
            expect(res.linearScaleType).toBeFalsy();
            expect(res.dateTimeScaleType).toBeFalsy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('categoryScaleType');
        }));

        it('gives category for user-relationship columnChart', inject(function (spChartService) {
            var res = spChartService.supportedScales('UserInlineRelationship', 'columnChart');
            expect(res.linearScaleType).toBeFalsy();
            expect(res.dateTimeScaleType).toBeFalsy();
            expect(res.categoryScaleType).toBeTruthy();
            expect(res.defaultScaleType).toBe('categoryScaleType');
        }));
    });

});
