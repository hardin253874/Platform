// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Console|Controls|spDataGrid|spec:|spDataGridUtils', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.ui.spDataGridUtils'));       

    it('ensure getValueAsPercentage returns valid values', inject(function (spDataGridUtils) {        
        // Test null value
        expect(spDataGridUtils.getValueAsPercentage(null, 'Int32', null)).toBe(-1);

        // Test null type
        expect(spDataGridUtils.getValueAsPercentage(0, null, null)).toBe(-1);

        // Test null bounds
        expect(spDataGridUtils.getValueAsPercentage(0, 'Int32', null)).toBe(-1);

        // Test invalid type
        expect(spDataGridUtils.getValueAsPercentage(0, 'String', null)).toBe(-1);

        // Test invalid bounds
        expect(spDataGridUtils.getValueAsPercentage(0, 'Int32', { lower: 10, upper: 10 })).toBe(-1);

        // Test valid number
        expect(spDataGridUtils.getValueAsPercentage(5, 'Int32', { lower: 0, upper: 10 })).toBe(50);

        // Test valid number as string
        expect(spDataGridUtils.getValueAsPercentage('6', 'Int32', { lower: 0, upper: 10 })).toBe(60);

        // Test below lower range
        expect(spDataGridUtils.getValueAsPercentage(-1, 'Int32', { lower: 0, upper: 10 })).toBe(-10);

        // Test above upper range
        expect(spDataGridUtils.getValueAsPercentage(1000, 'Int32', { lower: 0, upper: 10 })).toBe(10000);

        // Test valid date
        expect(spDataGridUtils.getValueAsPercentage('2013-10-14T20:03:52Z', 'Date', { lower: '2013-10-10T20:03:52Z', upper: '2013-10-20T20:03:52Z' })).toBe(40);

        // Test below lower range
        expect(spDataGridUtils.getValueAsPercentage('2012-10-14T20:03:52Z', 'Date', { lower: '2013-10-10T20:03:52Z', upper: '2013-10-20T20:03:52Z' })).toBe(-3610);

        // Test above upper range
        expect(spDataGridUtils.getValueAsPercentage('2016-10-14T20:03:52Z', 'Date', { lower: '2013-10-10T20:03:52Z', upper: '2013-10-20T20:03:52Z' })).toBe(11000);
    }));
});