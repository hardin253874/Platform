// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Reports|View|Filters', function() {
    'use strict';

    // Load the modules
    beforeEach(module('mod.ui.spReportFilters'));

    describe('relatedResource|spec:', function() {
        it('should return empty for null or undefined', inject(function($filter) {
            var relatedResource = $filter('relatedResource');

            expect(relatedResource(null)).toEqual('');
            expect(relatedResource(undefined)).toEqual('');
        }));

        it('should return formatted value - non empty values', inject(function($filter) {
            var relatedResource = $filter('relatedResource');
            var resource = {
                f1: 'a',
                f2: 'b',
                f3: 'c'
            };

            expect(relatedResource(resource)).toEqual('a, b, c');
        }));

        it('should return formatted value - empty values', inject(function($filter) {
            var relatedResource = $filter('relatedResource');
            var resource = {
                f1: 'a',
                f2: '',
                f3: 'c'
            };

            expect(relatedResource(resource)).toEqual('a, c');
        }));

        it('should return formatted value - null values', inject(function($filter) {
            var relatedResource = $filter('relatedResource');
            var resource = {
                f1: 'a',
                f2: null,
                f3: 'c'
            };

            expect(relatedResource(resource)).toEqual('a, c');
        }));

        it('should return formatted value - ensure sorted', inject(function($filter) {
            var relatedResource = $filter('relatedResource');
            var resource = {
                f3: 'c',
                f2: 'b',
                f1: 'a'
            };

            expect(relatedResource(resource)).toEqual('a, b, c');
        }));
    });

    describe('reportDataAsEntities|spec:', function() {
        it('should return empty array for null or undefined', inject(function($filter) {
            var reportDataAsEntities = $filter('reportDataAsEntities');

            expect(reportDataAsEntities(null)).toEqual([]);
            expect(reportDataAsEntities(undefined)).toEqual([]);
        }));

        it('should return converted report data - non empty values', inject(function($filter) {
            var reportDataAsEntities = $filter('reportDataAsEntities');
            var reportData = {
                meta: {
                    title: 'All Fields',
                    rcols: {
                        '1234': {
                            ord: 0,
                            title: 'All Fields',
                            type: 'String',
                            fid: 1697,
                            entityname: true
                        },
                        '5678': {
                            ord: 1,
                            title: 'Currency',
                            type: 'Currency',
                        }
                    }
                },
                gdata: [
                    {
                        eid: 1111,
                        values: [
                            {
                                val: 'AF 1'
                            }
                        ]
                    },
                    {

                        eid: 2222,
                        values: [
                            {
                                val: 'AF 2'
                            }
                        ]
                    }
                ]
            };

            var entities = reportDataAsEntities(reportData);

            expect(entities.length).toBe(2);
            expect(entities[0].id()).toBe(reportData.gdata[0].eid);
            expect(entities[0].getName()).toBe(reportData.gdata[0].values[0].val);
            expect(entities[1].id()).toBe(reportData.gdata[1].eid);
            expect(entities[1].getName()).toBe(reportData.gdata[1].values[0].val);
        }));
    });
});