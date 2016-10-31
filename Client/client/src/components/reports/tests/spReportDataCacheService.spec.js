// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Reports|spReportDataCacheService|spec:', function () {
    'use strict';
    var $q = null;
    var $timeout = null;
    var $scope;

    beforeEach(module('mod.common.spReportDataCacheService'));

    beforeEach(inject(function (_$q_, _$timeout_, _$rootScope_) {
        $q = _$q_;
        $timeout = _$timeout_;
        $scope = _$rootScope_.$new();
    }));

    it('can load service', inject(function (spReportDataCacheService) {
        expect(spReportDataCacheService).toBeTruthy();
    }));

    it('should be able to set and get data', inject(function (spReportDataCacheService) {
        var key = { name: 'test' };
        spReportDataCacheService.cacheReportData(key, 'foo');
        $timeout.flush();
        
        spReportDataCacheService.loadCachedReportData(key).then(function(data) {
            expect(data).toBeTruthy();
            expect(data).toBe('foo');
        });
        $scope.$apply();
    }));

    it('should create a meaningful cache key', inject(function(spReportDataCacheService) {
        var key = spReportDataCacheService.getCacheKey(555555, {}, {});
        expect(key).toBeTruthy();
        expect(key.name).toBeTruthy();
        expect(key.name.length).toBeGreaterThan(7);
        expect(key.name.slice(0, 7)).toBe('555555-');
    }));

    it('should clear all data for a report', inject(function (spReportDataCacheService) {
        var key1 = spReportDataCacheService.getCacheKey(555555, { something: '' }, {});
        var key2 = spReportDataCacheService.getCacheKey(555555, {}, {});
        var key3 = spReportDataCacheService.getCacheKey(123456, {}, {});

        expect(key1.name).toNotEqual(key2.name);
        expect(key2.name).toNotEqual(key3.name);
        expect(key3.name).toNotEqual(key1.name);

        spReportDataCacheService.cacheReportData(key1, 'foo1');
        spReportDataCacheService.cacheReportData(key2, 'foo2');
        spReportDataCacheService.cacheReportData(key3, 'foo3');
        $timeout.flush();

        spReportDataCacheService.clearReportData(555555);

        spReportDataCacheService.loadCachedReportData(key1).then(function (data) {
            expect(data).toBeNull();
        });
        spReportDataCacheService.loadCachedReportData(key2).then(function (data) {
            expect(data).toBeNull();
        });
        spReportDataCacheService.loadCachedReportData(key3).then(function (data) {
            expect(data).toBeTruthy();
            expect(data).toBe('foo3');
        });
        $scope.$apply();
    }));
});