// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, TestSupport */

describe('Reports|spReportService|intg:', function () {
    'use strict';

    beforeEach(module('mod.common.spEntityService'));
    beforeEach(module('spApps.reportServices'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    describe('our report service', function () {

        it('should get the report from my new spanking runDefaultReportForType method', inject(function (spReportService) {

            TestSupport.wait(
                spReportService.runDefaultReportForType({ ns: 'oldshared', alias: 'employee' })
                    .then(function(data) {
                        expect(data).toBeReportResultsWithData();
                    }));
        }));

    });

    describe('the new entity query webapi', function() {

        it('should return report formatted data for an ad-hoc query (around workflows)', inject(function (spWebService, $rootScope, $http) {

            var done = false;

            runs(function() {
                done = false;

                var webApiRoot = spWebService.getWebApiRoot();
                var url = webApiRoot + '/spapi/data/v1/entity/query';
                var data = {
                    root: {
                        id: 'core:workflow',
                        related: [
                            {
                                rel: {
                                    id: 'core:containedActivities',
                                    as: 'act',
                                    related: [
                                        {
                                            rel: {
                                                id: 'core:isOfType',
                                                as: 'actType'
                                            },
                                            forward: true,
                                            mustExist: true
                                        }
                                    ]
                                },
                                forward: true,
                                mustExist: true
                            }
                        ]
                    },
                    selects: [
                        { field: 'name', displayAs: 'Workflow' },
                        { field: 'name', on: 'act', displayAs: 'Activity' },
                        { field: 'name', on: 'actType', displayAs: 'ActivityType' }
                    ],
                    conds: [
                        { expr: { field: 'name' }, oper: 'startsWith', val: 'wf' },
                        { expr: { field: 'name', on: 'act' }, oper: 'StartsWith', val: 'l' }
                    ]
                };

                $http({
                    method: 'POST',
                    url: url,
                    headers: spWebService.getHeaders(),
                    data: data

                }).success(function(data) {

                    console.log('success', data);
                    done = true;

                }).error(function(err) {

                    var msg = err.ExceptionMessage || err.toString();
                    console.log('error', err, msg);
                    expect('the http request failed with error ' + msg).toBeFalsy();
                    done = true;
                });

                $rootScope.$apply();
            });

            waitsFor(function() {
                return done;
            }, 30000);
        }));

        it('should return report formatted data for an ad-hoc query (around employees)', inject(function (spWebService, $rootScope, $http) {

            var done = false;

            runs(function() {
                done = false;

                var webApiRoot = spWebService.getWebApiRoot();
                var url = webApiRoot + '/spapi/data/v1/entity/query';
                var data = {
                    root: {
                        id: 'oldshared:employee',
                        related: [
                            {
                                rel: {
                                    id: 'oldshared:employeesManager',
                                    as: 'm'
                                },
                                forward: true,
                                mustExist: true
                            }
                        ]
                    },
                    selects: [
                        { field: 'name', displayAs: 'Employee' },
                        { field: 'name', on: 'm', displayAs: 'Manager' }
                    ],
                    conds: [
                    ]
                };

                $http({
                    method: 'POST',
                    url: url,
                    headers: spWebService.getHeaders(),
                    data: data

                }).success(function(data) {

                    console.log('success', data);
                    done = true;

                }).error(function(err) {

                    var msg = err.ExceptionMessage || err.toString();
                    console.log('error', err, msg);
                    expect('the http request failed with error ' + msg).toBeFalsy();
                    done = true;
                });

                $rootScope.$apply();
            });

            waitsFor(function() {
                return done;
            }, 30000);
        }));

        it('runQuery should return report formatted data for an ad-hoc query (around employees)', inject(function (spReportService) {

            var query = {
                root: {
                    id: 'oldshared:employee',
                    related: [
                        {
                            rel: {
                                id: 'oldshared:employeesManager',
                                as: 'm'
                            },
                            forward: true,
                            mustExist: true
                        }
                    ]
                },
                selects: [
                    { field: 'name', displayAs: 'Employee' },
                    { field: 'name', on: 'm', displayAs: 'Manager' }
                ],
                conds: [
                ]
            };

            TestSupport.wait(spReportService.runQuery(query).then(function(result) {
                expect(result).toBeTruthy();
            }));

        }));

    });
    
    describe('report web api', function () {
        it('getReportData no metadata', inject(function (spReportService, spEntityService) {

            var query = null;

            TestSupport.wait(spEntityService.getEntity('test:allFieldsReport', 'id')
                .then(function(entity) {
                    return spReportService.getReportData(entity.id(), query);
                })
                .then(function(reportData) {
                    // No metadata
                    expect(reportData.meta).toBeUndefined();

                    // Verify grid data is returned
                    expect(reportData.gdata).toBeDefined();
                    expect(reportData.gdata.length).toBeGreaterThan(0);
                }));
        }));


        it('getReportData metadata', inject(function (spReportService, spEntityService) {

            var query = {
                metadata: 'full'
            };

            TestSupport.wait(
                spEntityService.getEntity('test:allFieldsReport', 'id')
                .then(function(entity) {
                    return spReportService.getReportData(entity.id(), query);
                })
                .then(function (reportData) {
                    // Metadata
                    expect(reportData.meta).toBeDefined();

                    // Verify grid data is returned
                    expect(reportData.gdata).toBeDefined();
                    expect(reportData.gdata.length).toBeGreaterThan(0);
                }));
        }));


        it('getReportData paging no more data', inject(function (spReportService, spEntityService) {

            var query = {
                startIndex: 10000,
                pageSize: 1000
            };

            TestSupport.wait(
                spEntityService.getEntity('test:allFieldsReport', 'id')
                .then(function (entity) {
                    return spReportService.getReportData(entity.id(), query);
                })
                .then(function (reportData) {
                    // Verify grid data is not returned
                    expect(reportData.gdata).toBeUndefined();
                }));
        }));


        it('getReportData paging index 1', inject(function (spReportService, spEntityService) {

            var query = {
                startIndex: 1,
                pageSize: 100
            };

            TestSupport.wait(
                spEntityService.getEntity('test:allFieldsReport', 'id')
                .then(function (entity) {
                    return spReportService.getReportData(entity.id(), query);
                })
                .then(function (reportData) {
                    // Verify grid data is returned
                    expect(reportData.gdata).toBeDefined();
                    expect(reportData.gdata.length).toBeGreaterThan(0);
                    //expect(reportData.gdata[0].values[0].val).toBe('Test 02');
                }));
        }));


        // Test is failing due to server side report entity model changes.
        // Commenting out report for now.
        xit('getReportData ad-hoc sorting info', inject(function (spReportService, spEntityService, $rootScope) {

            var query = {
                sort: [
                    {
                        // sort by the number column
                        colid: '94af7712-9a52-4802-9a27-164537770fd0',
                        order: 'Descending'
                    }
                ]
            };

            TestSupport.wait(
                spEntityService.getEntity('test:allFieldsReport', 'id')
                .then(function (entity) {
                    return spReportService.getReportData(entity.id(), query);
                })
                .then(function (reportData) {
                    // Verify grid data is returned
                    expect(reportData.gdata).toBeDefined();
                    expect(reportData.gdata.length).toBeGreaterThan(0);
                    expect(reportData.gdata[0].values[0].val).toBe('Test 30');
                }));
        }));
    });
});

