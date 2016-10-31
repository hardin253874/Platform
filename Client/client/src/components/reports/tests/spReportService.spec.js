// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Reports|spReportService|spec:', function () {
    'use strict';

    // Load the modules        
    beforeEach(module('spApps.reportServices'));

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });

    it('should be loadable', inject(function (spReportService) {
        expect(spReportService).toBeTruthy();

        expect(spReportService.runDefaultReportForType).toBeTruthy();
        expect(spReportService.runReport).toBeTruthy();
        expect(spReportService.runQuery).toBeTruthy();
        expect(spReportService.runPickerReport).toBeTruthy();
    }));


    describe('value formatter', function () {

        it('can handle number', inject(function (spReportService) {
            var rcol = { ord: 2, title: 'Number', type: 'Int32', fid: 5107 };
            var valRule = { prefix: 'A', suffix: 'Z', places: 0 };

            var fn1 = spReportService.getColumnFormatFunc(rcol);
            expect(fn1).toBeTruthy();
            expect(fn1(-1234)).toBe('-1,234');

            var fn2 = spReportService.getColumnFormatFunc(rcol, valRule);
            expect(fn2).toBeTruthy();
            expect(fn2(-1234)).toBe('A-1,234Z');
        }));

        it('can handle decimal', inject(function (spReportService) {
            var rcol = { ord: 3, title: 'Decimal', type: 'Decimal', fid: 10497, places: 3 };
            var valRule = { prefix: 'A', suffix: 'Z', places: 2 };

            var fn1 = spReportService.getColumnFormatFunc(rcol);
            expect(fn1).toBeTruthy();
            expect(fn1(-1234.567)).toBe('-1,234.567');

            var fn2 = spReportService.getColumnFormatFunc(rcol, valRule);
            expect(fn2).toBeTruthy();
            expect(fn2(-1234.567)).toBe('A-1,234.57Z');
        }));

        it('can handle currency', inject(function (spReportService) {
            var rcol = { ord: 1, title: 'Currency', type: 'Currency', fid: 10508, places: 3 };
            var valRule = { prefix: 'A', suffix: 'Z', places: 1 };

            var fn1 = spReportService.getColumnFormatFunc(rcol);
            expect(fn1).toBeTruthy();
            expect(fn1(-1234.567)).toBe('$-1,234.567');

            var fn2 = spReportService.getColumnFormatFunc(rcol, valRule);
            expect(fn2).toBeTruthy();
            expect(fn2(-1234.567)).toBe('A$-1,234.6Z');
        }));

        it('can handle boolean', inject(function (spReportService) {
            var rcol = {ord:9, title:'Yes No', type:'Bool', fid:9696, ro:true};

            var fn1 = spReportService.getColumnFormatFunc(rcol);
            expect(fn1).toBeTruthy();
            expect(fn1(true)).toBe('Yes');
            expect(fn1(false)).toBe('No');
            expect(fn1(null)).toBe('');
        }));

        it('can handle date (dateDayMonth)', inject(function (spReportService) {
            var rcol = { ord: 4, title: 'Date', type: 'Date', fid: 7964 };
            var valRule = { places: 0, datetimefmt: 'dateDayMonth' };

            var value = new Date('2014-12-31T00:00:00Z');

            var fn1 = spReportService.getColumnFormatFunc(rcol);
            expect(fn1).toBeTruthy();
            expect(fn1(value)).toBe('12/31/2014');  //hmm..

            var fn2 = spReportService.getColumnFormatFunc(rcol, valRule);
            expect(fn2).toBeTruthy();
            expect(fn2(value)).toBe('December 31');
        }));

        it('can handle time (time24Hour)', inject(function (spReportService) {
            var rcol = { ord: 4, title: 'Time', type: 'Time', fid: 7964 };
            var valRule = { places: 0, datetimefmt: 'time24Hour' };

            var value = new Date(1753, 1-1, 1, 13, 50, 59); //hmm..

            var fn1 = spReportService.getColumnFormatFunc(rcol);
            expect(fn1).toBeTruthy();
            expect(fn1(value)).toBe('1:50 PM');

            var fn2 = spReportService.getColumnFormatFunc(rcol, valRule);
            expect(fn2).toBeTruthy();
            expect(fn2(value)).toBe('13:50');
        }));

        it('can handle datetime (dateTimeMonthYear)', inject(function (spReportService) {
            var rcol = { ord: 4, title: 'DateTime', type: 'DateTime', fid: 7964 };
            var valRule = { places: 0, datetimefmt: 'dateTimeDayMonthTime' };

            var value = new Date(2014, 12-1, 4, 13, 0, 0);

            var fn1 = spReportService.getColumnFormatFunc(rcol);
            expect(fn1).toBeTruthy();
            expect(fn1(value)).toBe('12/4/2014 1:00 PM');

            var fn2 = spReportService.getColumnFormatFunc(rcol, valRule);
            expect(fn2).toBeTruthy();
            expect(fn2(value)).toBe('December 04 1:00 PM');
        }));

    });
    
});