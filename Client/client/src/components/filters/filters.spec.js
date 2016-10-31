// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Internal|Filters', function () {
    'use strict';

    var currentCulture = Globalize.culture().name;

    // Load the modules
    beforeEach(module('sp.common.filters'));


    beforeEach(function () {
        // Note we are using US because dynamically loading the culture in Jasmine is a right pain, the US one is in built
        Globalize.culture('en-US');
    });

    afterEach(function () {

        // Restore the culture
        Globalize.culture(currentCulture);
    });

    describe('spCurrency|spec:', function () {
        it('should return empty for null or undefined', inject(function ($filter) {

            var spCurrency = $filter('spCurrency');

            expect(spCurrency(null, '$', 3, null, null)).toEqual('');
            expect(spCurrency(undefined, '$', 3, null, null)).toEqual('');
            
        }));

        it('should return formatted value - empty string value', inject(function ($filter) {
            var spCurrency = $filter('spCurrency');

            expect(spCurrency('', '$', 3, null, null)).toEqual('');
        }));

        it('should return formatted value - $, 3 decimal places, no prefix suffix', inject(function ($filter) {
            var spCurrency = $filter('spCurrency');

            expect(spCurrency(123.4567, '$', 3, null, null)).toEqual('$123.457');
            expect(spCurrency('123.4567', '$', 3, null, null)).toEqual('$123.457');
        }));        

        it('should return formatted value - $, 0 decimal places, no prefix suffix', inject(function ($filter) {
            var spCurrency = $filter('spCurrency');

            expect(spCurrency(123.4567, '$', 0, null, null)).toEqual('$123');
            expect(spCurrency('123.4567', '$', 0, null, null)).toEqual('$123');
        }));

        it('should return formatted value - default symbol, 0 decimal places, no prefix suffix', inject(function ($filter) {
            var spCurrency = $filter('spCurrency');

            expect(spCurrency(123.4567, null, 0, null, null)).toEqual('$123');
            expect(spCurrency('123.4567', null, 0, null, null)).toEqual('$123');
        }));

        it('should return formatted value - $, 4 decimal places, prefix suffix', inject(function ($filter) {
            var spCurrency = $filter('spCurrency');

            expect(spCurrency(123.4567, '$', 4, 'prefix', 'suffix')).toEqual('prefix$123.4567suffix');
            expect(spCurrency('123.4567', '$', 4, 'prefix', 'suffix')).toEqual('prefix$123.4567suffix');
        }));

        it('should return formatted value - $, grouped values', inject(function ($filter) {
            var spCurrency = $filter('spCurrency');

            expect(spCurrency(1234567, '$', 4, null, null)).toEqual('$1,234,567.0000');
            expect(spCurrency('1234567', '$', 4, null, null)).toEqual('$1,234,567.0000');
        }));       
    });

    describe('spDecimal|spec:', function () {
        it('should return empty for null or undefined', inject(function ($filter) {
            var spDecimal = $filter('spDecimal');

            expect(spDecimal(null, 3, null, null)).toEqual('');
            expect(spDecimal(undefined, 3, null, null)).toEqual('');
        }));

        it('should return formatted value - empty string value', inject(function ($filter) {
            var spDecimal = $filter('spDecimal');

            expect(spDecimal('', 3, null, null)).toEqual('');
        }));

        it('should return formatted value - 3 decimal places, no prefix suffix', inject(function ($filter) {
            var spDecimal = $filter('spDecimal');

            expect(spDecimal(123.4567, 3, null, null)).toEqual('123.457');
            expect(spDecimal('123.4567', 3, null, null)).toEqual('123.457');
        }));

        it('should return formatted value - 0 decimal places, no prefix suffix', inject(function ($filter) {
            var spDecimal = $filter('spDecimal');

            expect(spDecimal(123.4567, 0, null, null)).toEqual('123');
            expect(spDecimal('123.4567', 0, null, null)).toEqual('123');
        }));        

        it('should return formatted value - 4 decimal places, prefix suffix', inject(function ($filter) {
            var spDecimal = $filter('spDecimal');

            expect(spDecimal(123.4567, 4, 'prefix', 'suffix')).toEqual('prefix123.4567suffix');
            expect(spDecimal('123.4567', 4, 'prefix', 'suffix')).toEqual('prefix123.4567suffix');            
        }));

        it('should return formatted value - grouped values', inject(function ($filter) {
            var spCurrency = $filter('spDecimal');

            expect(spCurrency(1234567, 4, null, null)).toEqual('1,234,567.0000');
            expect(spCurrency('1234567', 4, null, null)).toEqual('1,234,567.0000');
        }));        
    });

    describe('spNumber|spec:', function () {
        it('should return empty for null or undefined', inject(function ($filter) {
            var spNumber = $filter('spNumber');

            expect(spNumber(null, null, null)).toEqual('');
            expect(spNumber(undefined, null, null)).toEqual('');
        }));

        it('should return formatted value - empty string value', inject(function ($filter) {
            var spNumber = $filter('spNumber');

            expect(spNumber('', null, null)).toEqual('');
        }));

        it('should return formatted value - no prefix suffix', inject(function ($filter) {
            var spNumber = $filter('spNumber');
            
            expect(spNumber(123, null, null)).toEqual('123');
            expect(spNumber('123', null, null)).toEqual('123');
        }));        

        it('should return formatted value - prefix suffix', inject(function ($filter) {
            var spNumber = $filter('spNumber');
            
            expect(spNumber(123, 'prefix', 'suffix')).toEqual('prefix123suffix');
            expect(spNumber('123', 'prefix', 'suffix')).toEqual('prefix123suffix');
        }));

        it('should return formatted value - decimal, no prefix suffix', inject(function ($filter) {
            var spNumber = $filter('spNumber');
            
            expect(spNumber(123.456, '', '')).toEqual('123');
            expect(spNumber('123.456', '', '')).toEqual('123');
        }));

        it('should return formatted value - grouped values', inject(function ($filter) {
            var spCurrency = $filter('spNumber');

            expect(spCurrency(1234567, null, null)).toEqual('1,234,567');
            expect(spCurrency('1234567', null, null)).toEqual('1,234,567');
        }));
    });

    describe('spBoolean|spec:', function () {
        it('should return empty string for for null or undefined', inject(function ($filter) {
            var spBoolean = $filter('spBoolean');
            
            expect(spBoolean(null)).toEqual('');
            expect(spBoolean(undefined)).toEqual('');
        }));

        it('should return formatted value - empty string value', inject(function ($filter) {
            var spBoolean = $filter('spBoolean');
            
            expect(spBoolean('')).toEqual('');
        }));

        it('should return No for false', inject(function ($filter) {
            var spBoolean = $filter('spBoolean');

            expect(spBoolean(false)).toEqual('No');
            expect(spBoolean('false')).toEqual('No');
        }));

        it('should return Yes for true', inject(function ($filter) {
            var spBoolean = $filter('spBoolean');

            expect(spBoolean(true)).toEqual('Yes');
            expect(spBoolean('true')).toEqual('Yes');
        }));

        it('should return True for true - formatting', inject(function ($filter) {
            var spBoolean = $filter('spBoolean');

            expect(spBoolean(true, 'TrueFalse')).toEqual('True');
            expect(spBoolean('true', 'TrueFalse')).toEqual('True');
        }));

        it('should return False for false - formatting', inject(function ($filter) {
            var spBoolean = $filter('spBoolean');

            expect(spBoolean(false, 'TrueFalse')).toEqual('False');
            expect(spBoolean('false', 'TrueFalse')).toEqual('False');
        }));

        it('should return Yes for true - formatting', inject(function ($filter) {
            var spBoolean = $filter('spBoolean');

            expect(spBoolean(true, 'YesNo')).toEqual('Yes');
            expect(spBoolean('true', 'YesNo')).toEqual('Yes');
        }));

        it('should return No for false - formatting', inject(function ($filter) {
            var spBoolean = $filter('spBoolean');

            expect(spBoolean(false, 'YesNo')).toEqual('No');
            expect(spBoolean('false', 'YesNo')).toEqual('No');
        }));
    });

    describe('spTime|spec:', function () {
        it('should return empty for for null or undefined', inject(function ($filter) {
            var spTime = $filter('spTime');

            expect(spTime(null,'')).toEqual('');
            expect(spTime(undefined,'')).toEqual('');
        }));

        it('should return formatted value - empty string value', inject(function ($filter) {
            var spTime = $filter('spTime');

            expect(spTime('')).toEqual('');
        }));

        it('should return formatted value - 12Hour', inject(function ($filter) {
            var spTime = $filter('spTime');

            expect(spTime('October 13, 2013 11:13:00', 'time12Hour')).toEqual('11:13 AM');
            expect(spTime(new Date('October 13, 2013 11:13:00'), 'time12Hour')).toEqual('11:13 AM');
        }));

        it('should return formatted value - 24Hour', inject(function ($filter) {
            var spTime = $filter('spTime');

            expect(spTime('October 13, 2013 14:13:00', 'time24Hour')).toEqual('14:13');
            expect(spTime(new Date('October 13, 2013 14:13:00'), 'time24Hour')).toEqual('14:13');
        }));

        it('should return formatted value - default format', inject(function ($filter) {
            var spTime = $filter('spTime');

            expect(spTime('October 13, 2013 11:13:00', null)).toEqual('11:13 AM');
            expect(spTime(new Date('October 13, 2013 11:13:00'), null)).toEqual('11:13 AM');
        }));

        it('should return formatted value - iso format 12Hour', inject(function ($filter) {
            var spTime = $filter('spTime');

            expect(spTime('1753-01-01T00:00:00.0000000Z', 'time12Hour')).toEqual('12:00 AM');
            expect(spTime('1753-01-01T17:00:00.0000000Z', 'time12Hour')).toEqual('5:00 PM');
        }));

        it('should return formatted value - iso format 24Hour', inject(function ($filter) {
            var spTime = $filter('spTime');

            expect(spTime('1753-01-01T00:00:00.0000000Z', 'time24Hour')).toEqual('00:00');
            expect(spTime('1753-01-01T17:00:00.0000000Z', 'time24Hour')).toEqual('17:00');
        }));
    });

    describe('spDate|spec:', function () {
        it('should return empty for for null or undefined', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate(null,'')).toEqual('');
            expect(spDate(undefined,'')).toEqual('');
        }));

        it('should return formatted value - empty string value', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('')).toEqual('');
        }));

        it('should return formatted value - ShortDate', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('October 13, 2013 11:13:00', 'dateShort')).toEqual('10/13/2013');
            expect(spDate(new Date('October 13, 2013 11:13:00'), 'ShortDate')).toEqual('10/13/2013');
        }));

        it('should return formatted value - DayMonth', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('October 13, 2013 14:13:00', 'dateDayMonth')).toEqual('October 13');
            expect(spDate(new Date('October 13, 2013 14:13:00'), 'dateDayMonth')).toEqual('October 13');
        }));

        it('should return formatted value - LongDate', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('October 13, 2013 11:13:00', 'dateLong')).toEqual('Sunday, October 13, 2013');
            expect(spDate(new Date('October 13, 2013 11:13:00'), 'dateLong')).toEqual('Sunday, October 13, 2013');
        }));

        it('should return formatted value - Month', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('October 13, 2013 11:13:00', 'dateMonth')).toEqual('Oct');
            expect(spDate(new Date('October 13, 2013 11:13:00'), 'dateMonth')).toEqual('Oct');
        }));

        it('should return formatted value - MonthYear', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('October 13, 2013 11:13:00', 'dateMonthYear')).toEqual('2013 October');
            expect(spDate(new Date('October 13, 2013 11:13:00'), 'dateMonthYear')).toEqual('2013 October');
        }));

        it('should return formatted value - Year', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('October 13, 2013 11:13:00', 'dateYear')).toEqual('2013');
            expect(spDate(new Date('October 13, 2013 11:13:00'), 'dateYear')).toEqual('2013');
        }));

        it('should return formatted value - default format', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('October 13, 2013 11:13:00', null)).toEqual('10/13/2013');
            expect(spDate(new Date('October 13, 2013 11:13:00'), null)).toEqual('10/13/2013');
        }));

        it('should return formatted value - Weekday', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('November 27, 2014 11:13:00', 'dateWeekday')).toEqual('Thursday');
            expect(spDate(new Date('November 27, 2014 11:13:00'), 'dateWeekday')).toEqual('Thursday');
        }));

        it('should return formatted value - Quarter Q1', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('February 13, 2013 11:13:00', 'dateQuarter')).toEqual('Q1');
            expect(spDate(new Date('February 13, 2013 11:13:00'), 'dateQuarter')).toEqual('Q1');
        }));

        it('should return formatted value - QuarterYear Q1', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('February 13, 2013 11:13:00', 'dateQuarterYear')).toEqual('Q1, 2013');
            expect(spDate(new Date('February 13, 2013 11:13:00'), 'dateQuarterYear')).toEqual('Q1, 2013');
        }));

        it('should return formatted value - QuarterYear Q2', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('April 13, 2013 11:13:00', 'dateQuarterYear')).toEqual('Q2, 2013');
            expect(spDate(new Date('April 13, 2013 11:13:00'), 'dateQuarterYear')).toEqual('Q2, 2013');
        }));

        it('should return formatted value - QuarterYear Q3', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('July 13, 2013 11:13:00', 'dateQuarterYear')).toEqual('Q3, 2013');
            expect(spDate(new Date('July 13, 2013 11:13:00'), 'dateQuarterYear')).toEqual('Q3, 2013');
        }));

        it('should return formatted value - QuarterYear Q4', inject(function ($filter) {
            var spDate = $filter('spDate');

            expect(spDate('October 13, 2013 11:13:00', 'dateQuarterYear')).toEqual('Q4, 2013');
            expect(spDate(new Date('October 13, 2013 11:13:00'), 'dateQuarterYear')).toEqual('Q4, 2013');
        }));
    });

    describe('spDateTime|spec:', function () {
        it('should return empty for for null or undefined', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime(null)).toEqual('');
            expect(spDateTime(undefined)).toEqual('');
        }));

        it('should return formatted value - empty string value', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('')).toEqual('');
        }));

        it('should return formatted value - ShortDateTime', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', 'dateTimeShort')).toEqual('10/13/2013 11:13 AM');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), 'dateTimeShort')).toEqual('10/13/2013 11:13 AM');
        }));

        it('should return formatted value - DateTime24Hour', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 14:13:00', 'dateTime24Hour')).toEqual('10/13/2013 14:13');
            expect(spDateTime(new Date('October 13, 2013 14:13:00'), 'dateTime24Hour')).toEqual('10/13/2013 14:13');
        }));

        it('should return formatted value - Month', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', 'dateTimeMonth')).toEqual('Oct');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), 'dateTimeMonth')).toEqual('Oct');
        }));

        it('should return formatted value - DayMonth', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', 'dateTimeDayMonth')).toEqual('October 13');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), 'dateTimeDayMonth')).toEqual('October 13');
        }));

        it('should return formatted value - DayMonthTime', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', 'dateTimeDayMonthTime')).toEqual('October 13 11:13 AM');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), 'dateTimeDayMonthTime')).toEqual('October 13 11:13 AM');
        }));

        it('should return formatted value - LongDateTime', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', 'dateTimeLong')).toEqual('Sunday, October 13, 2013 11:13 AM');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), 'dateTimeLong')).toEqual('Sunday, October 13, 2013 11:13 AM');
        }));

        it('should return formatted value - SortableDateTime', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', 'dateTimeSortable')).toEqual('2013-10-13T11:13:00');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), 'dateTimeSortable')).toEqual('2013-10-13T11:13:00');
        }));

        it('should return formatted value - MonthYear', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', 'dateTimeMonthYear')).toEqual('2013 October');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), 'dateTimeMonthYear')).toEqual('2013 October');
        }));

        it('should return formatted value - Weekday', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('November 27, 2014 11:13:00', 'dateTimeWeekday')).toEqual('Thursday');
            expect(spDateTime(new Date('November 27, 2014 11:13:00'), 'dateTimeWeekday')).toEqual('Thursday');
        }));

        it('should return formatted value - Quarter Q1', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('February 13, 2013 11:13:00', 'dateTimeQuarter')).toEqual('Q1');
            expect(spDateTime(new Date('February 13, 2013 11:13:00'), 'dateTimeQuarter')).toEqual('Q1');
        }));

        it('should return formatted value - QuarterYear Q1', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('February 13, 2013 11:13:00', 'dateTimeQuarterYear')).toEqual('Q1, 2013');
            expect(spDateTime(new Date('February 13, 2013 11:13:00'), 'dateTimeQuarterYear')).toEqual('Q1, 2013');
        }));

        it('should return formatted value - QuarterYear Q2', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('April 13, 2013 11:13:00', 'dateTimeQuarterYear')).toEqual('Q2, 2013');
            expect(spDateTime(new Date('April 13, 2013 11:13:00'), 'dateTimeQuarterYear')).toEqual('Q2, 2013');
        }));

        it('should return formatted value - QuarterYear Q3', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('July 13, 2013 11:13:00', 'dateTimeQuarterYear')).toEqual('Q3, 2013');
            expect(spDateTime(new Date('July 13, 2013 11:13:00'), 'dateTimeQuarterYear')).toEqual('Q3, 2013');
        }));

        it('should return formatted value - QuarterYear Q4', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', 'dateTimeQuarterYear')).toEqual('Q4, 2013');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), 'dateTimeQuarterYear')).toEqual('Q4, 2013');
        }));

        it('should return formatted value - Year', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', 'dateTimeYear')).toEqual('2013');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), 'dateTimeYear')).toEqual('2013');
        }));

        it('should return formatted value - date only format', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', 'dateTimeDate')).toEqual('10/13/2013');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), 'dateTimeDate')).toEqual('10/13/2013');
        }));

        it('should return formatted value - time only format', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', 'dateTimeTime')).toEqual('11:13 AM');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), 'dateTimeTime')).toEqual('11:13 AM');
        }));

        it('should return formatted value - default format', inject(function ($filter) {
            var spDateTime = $filter('spDateTime');

            expect(spDateTime('October 13, 2013 11:13:00', null)).toEqual('10/13/2013 11:13 AM');
            expect(spDateTime(new Date('October 13, 2013 11:13:00'), null)).toEqual('10/13/2013 11:13 AM');
        }));
    });

    describe('spTitleCase|spec:', function () {
        it('should return empty for null or undefined or empty', inject(function ($filter) {
            var spTitleCase = $filter('spTitleCase');

            expect(spTitleCase(null)).toEqual(undefined);
            expect(spTitleCase(undefined)).toEqual(undefined);
            expect(spTitleCase('')).toEqual(undefined);
        }));

        it('should convert to title case', inject(function ($filter) {  // other tests are handled in spUtils
            var spTitleCase = $filter('spTitleCase');

            expect(spTitleCase('there can be only one')).toEqual('There Can Be Only One');
        }));
       
    });
});