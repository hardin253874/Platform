// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals spEntity */

describe('Entity Model|spEntity|spec:', function () {
    'use strict';

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });


    describe('spEntity.Field', function () {

        // Introduction Tests

        it('constructor requires a field type', function () {

            expect(function() {
                return new spEntity.Field(123);
            }).toThrow();
        });

        it('constructor accepts IDs', function () {
            var f = new spEntity.Field(123, spEntity.DataType.Decimal);
            expect(f.id.getId()).toBe(123);
        });

        it('isNumeric works', function () {
            expect(new spEntity.Field(1, spEntity.DataType.Int32).isNumeric()).toBeTruthy();
            expect(new spEntity.Field(1, spEntity.DataType.Decimal).isNumeric()).toBeTruthy();
            expect(new spEntity.Field(1, spEntity.DataType.Currency).isNumeric()).toBeTruthy();
            expect(new spEntity.Field(1, spEntity.DataType.String).isNumeric()).toBeFalsy();
            expect(new spEntity.Field(1, spEntity.DataType.Date).isNumeric()).toBeFalsy();
            expect(new spEntity.Field(1, spEntity.DataType.DateTime).isNumeric()).toBeFalsy();
            expect(new spEntity.Field(1, spEntity.DataType.Time).isNumeric()).toBeFalsy();
            expect(new spEntity.Field(1, spEntity.DataType.Bool).isNumeric()).toBeFalsy();
            expect(new spEntity.Field(1, spEntity.DataType.Guid).isNumeric()).toBeFalsy();
        });

        it('distinct fields stay distinct', function () {
            var e = spEntity.fromJSON({rel1:[{
                dataState: 0,
                id: { id: 12160, ns: 'console', alias: 'smallThumbnail' },
                typeId: 'console:thumbnailSizeEnum',
                description: 'A'
            },
                {
                    dataState: 0,
                    id: { id: 12522, ns: 'console', alias: 'largeThumbnail' },
                    typeId: 'console:thumbnailSizeEnum',
                    description: 'B'
                }]});
            expect(e.rel1[0].description).toBe('A');
            expect(e.rel1[1].description).toBe('B');
        });

        describe('markAsPristine', function () {

            it('should run', function () {
                var e = spEntity.fromJSON({ name: 'myname' });
                var f = e.getFieldContainer('name');
                expect(f._pristine).toBeFalsy();
                expect(f._wasPristine).toBeFalsy();
                f.markAsPristine();
                expect(f._pristine).toBeTruthy();
                expect(f._wasPristine).toBeTruthy();
            });

            it('should detect changes', function () {
                var e = spEntity.fromJSON({ name: 'myname' });
                var f = e.getFieldContainer('name');
                f.markAsPristine();
                e.name = 'newname';
                expect(f._pristine).toBeFalsy();
                expect(f._wasPristine).toBeTruthy();
            });

        });

        

    });

});