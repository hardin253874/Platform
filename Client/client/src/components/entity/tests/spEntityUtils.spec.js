// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global spEntityUtils, sp, _, describe, it, beforeEach, TestSupport, expect, spEntity */

describe('Internal|spEntityUtils library|spec:', function () {
    'use strict';

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });

    describe('walkEntity', function () {

        it('should handle single entity', function () {
            var e1 = spEntity.fromJSON(123);
            var res = spEntityUtils.walkEntities(e1);
            expect(res).toBeArray(1);
        });

        it('should handle arrays', function () {
            var e1 = spEntity.fromJSON(123);
            var e2 = spEntity.fromJSON(234);
            var res = spEntityUtils.walkEntities([e1, e2]);
            expect(res).toBeArray(2);
        });

    });

    describe('convertDbStringToNative', function () {
        it('int32 string turns into a number', function () {

            expect(sp.convertDbStringToNative(spEntity.DataType.Int32, '10')).toEqual(10);
            expect(sp.convertDbStringToNative(spEntity.DataType.Int32, null)).toEqual(null);
            expect(sp.convertDbStringToNative(spEntity.DataType.Int32, '')).toEqual(null);
        });

        it('decimal string turns into a number', function () {

            expect(sp.convertDbStringToNative(spEntity.DataType.Decimal, '10.1')).toEqual(10.1);
            expect(sp.convertDbStringToNative(spEntity.DataType.Decimal, null)).toEqual(null);
            expect(sp.convertDbStringToNative(spEntity.DataType.Decimal, '')).toEqual(null);
        });

        it('currency string turns into a number', function () {

            expect(sp.convertDbStringToNative(spEntity.DataType.Currency, '10.1')).toEqual(10.1);
            expect(sp.convertDbStringToNative(spEntity.DataType.Currency, null)).toEqual(null);
            expect(sp.convertDbStringToNative(spEntity.DataType.Currency, '')).toEqual(null);
        });


        it('string string turns into a string', function () {

            expect(sp.convertDbStringToNative(spEntity.DataType.String, 'string')).toEqual('string');
            expect(sp.convertDbStringToNative(spEntity.DataType.String, null)).toEqual(null);
            expect(sp.convertDbStringToNative(spEntity.DataType.String, '')).toEqual(null);
        });

        it('utc date string turns into a Date', function () {
            //var utcXmas2011Str = '2011-12-24 20:30:00'; // utc representation of 25 Dec 2011 07:30AM (Sydney)

            //var xmas2011 = new Date(2011, 11, 25, 7, 30, 0, 0);
            
            //expect(sp.convertDbStringToNative(spEntity.DataType.DateTime, utcXmas2011Str)).toEqual(xmas2011);
            expect(sp.convertDbStringToNative(spEntity.DataType.DateTime, null)).toEqual(null);
            expect(sp.convertDbStringToNative(spEntity.DataType.DateTime, '')).toEqual(null);
        });

        
        it('time string turns into a Date with 1973-01-01 as the epoc', function () {
            // utc representation of 07:30AM (Sydney) is '20:30:00';
            var time = '20:30:00';

            var converted = sp.convertDbStringToNative(spEntity.DataType.Time, time);
            expect(converted.getHours()).toEqual(7);
            expect(converted.getMinutes()).toEqual(30);
            expect(converted.getSeconds()).toEqual(0);
            
            expect(sp.convertDbStringToNative(spEntity.DataType.DateTime, null)).toEqual(null);
            expect(sp.convertDbStringToNative(spEntity.DataType.DateTime, '')).toEqual(null);
        });
        
        
        it('Bool string turns into a bool', function () {

            expect(sp.convertDbStringToNative(spEntity.DataType.Bool, 'True')).toEqual(true);
            expect(sp.convertDbStringToNative(spEntity.DataType.Bool, 'False')).toEqual(false);
            expect(sp.convertDbStringToNative(spEntity.DataType.Bool, null)).toEqual(null);
            expect(sp.convertDbStringToNative(spEntity.DataType.Bool, '')).toEqual(false);
        });


    });

    describe('dataTypeForFieldTypeAlias', function () {
        it('that intField turns into Int32', function () {

            expect(spEntityUtils.dataTypeForFieldTypeAlias('intField')).toEqual('Int32');
        });
        
        it('that autoNumberField turns into String', function () {

            expect(spEntityUtils.dataTypeForFieldTypeAlias('autoNumberField')).toEqual('String');
        });

        it('that "core:" is ignored', function () {

            expect(spEntityUtils.dataTypeForFieldTypeAlias('core:decimalField')).toEqual('Decimal');
        });

        it('that an unknown field throws an exception', function () {

            expect(function() { spEntityUtils.dataTypeForFieldTypeAlias('unknownField'); }).toThrow(new Error("fieldTypeAlias does not correspond to a valid dataType: unknownField"));
        });
        
    });
    
    describe('dataTypeForFieldTypeAlias to fieldTypeAliasForDataType should be reflexive', function () {
        it('that intField turns into Int32', function () {
            expect(spEntityUtils.dataTypeForFieldTypeAlias(spEntityUtils.fieldTypeAliasForDataType(spEntity.DataType.String))).toEqual(spEntity.DataType.String);
            expect(spEntityUtils.dataTypeForFieldTypeAlias(spEntityUtils.fieldTypeAliasForDataType(spEntity.DataType.Int32))).toEqual(spEntity.DataType.Int32);
            expect(spEntityUtils.dataTypeForFieldTypeAlias(spEntityUtils.fieldTypeAliasForDataType(spEntity.DataType.Decimal))).toEqual(spEntity.DataType.Decimal);
            expect(spEntityUtils.dataTypeForFieldTypeAlias(spEntityUtils.fieldTypeAliasForDataType(spEntity.DataType.Currency))).toEqual(spEntity.DataType.Currency);
            expect(spEntityUtils.dataTypeForFieldTypeAlias(spEntityUtils.fieldTypeAliasForDataType(spEntity.DataType.Date))).toEqual(spEntity.DataType.Date);
            expect(spEntityUtils.dataTypeForFieldTypeAlias(spEntityUtils.fieldTypeAliasForDataType(spEntity.DataType.Time))).toEqual(spEntity.DataType.Time);
            expect(spEntityUtils.dataTypeForFieldTypeAlias(spEntityUtils.fieldTypeAliasForDataType(spEntity.DataType.DateTime))).toEqual(spEntity.DataType.DateTime);
            expect(spEntityUtils.dataTypeForFieldTypeAlias(spEntityUtils.fieldTypeAliasForDataType(spEntity.DataType.Bool))).toEqual(spEntity.DataType.Bool);
            expect(spEntityUtils.dataTypeForFieldTypeAlias(spEntityUtils.fieldTypeAliasForDataType(spEntity.DataType.Guid))).toEqual(spEntity.DataType.Guid);
        });

    });
});
