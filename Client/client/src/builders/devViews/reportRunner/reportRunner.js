// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('app.reportRunnerTest', ['mod.common.spEntityService',
        'mod.common.spTenantSettings',
        'mod.ui.spReportConstants',
        'spApps.reportServices',
        'mod.common.spWebService',
        'mod.common.spXsrf',
        'ui.bootstrap'])
        .config(function ($stateProvider) {
            $stateProvider.state('reportRunner', {
                url: '/{tenant}/{eid}/reportRunner?path',
                templateUrl: 'devViews/reportRunner/reportRunner.tpl.html'
            });

            window.testNavItems = window.testNavItems || {};
            window.testNavItems.reportRunner = { name: 'Report Runner' };
        })
        .controller('reportRunnerController', function ($q, $scope, $stateParams, $http, $timeout, spEntityService, spTenantSettings, reportPageSize, spReportService, spWebService, spXsrf) {
            $scope.myTitle = 'Report Runner';
            $scope.title = 'Report Runner';
            $scope.data = {
            };

            $scope.entityId = {};

            $scope.applications = {
                selectedEntityId: 0,
                selectedEntity: null,
                entityTypeId: 'core:solution'
            };

            $scope.results = [];
            $scope.isRunning = false;
            $scope.nRun = 0;
            $scope.nRunTotal = 0;
            $scope.nRunErrors = 0;

            var requestOptions = {
                startIndex: 0,
                pageSize: reportPageSize.value,
                displayPageSize: reportPageSize.value,
                onErr: function (response) {
                    // not. happy. jan.
                    if (!response.data) {
                        response.data = { Message: response.statusText + ' (' + response.status + ')' };
                    }

                    $scope.nRunErrors++;

                    return response.data; 
                }
            };

            var templateReportIds = [];

            spTenantSettings.getTemplateReportIds().then(function (ids) {
                templateReportIds = ids;
            });

            var uri = '/sp/#/' + encodeURIComponent($stateParams.tenant) + '/';
            var batch;

            $scope.openXml = function(id) {
                var xuri = spEntityService.getEntityXmlUrl(id);
                var turi = spXsrf.addXsrfTokenAsQueryString(xuri);
                var win = window.open(turi, '_blank');
                win.focus();
            };

            $scope.run = function () {

                $scope.results = [];
                $scope.isRunning = true;
                $scope.nRunErrors = 0;

                return getReports().then(function(reports) {

                    $timeout(function () {
                        $scope.nRun = reports.length;
                        $scope.nRunTotal = reports.length;
                    }, 1);

                    // hackety-hack
                    ////batch = _.map(_.filter(reports, function (report) {
                    ////    return _.some(_.map(report.reportColumns, 'name'), function(n) {
                    ////        return n.toLowerCase().indexOf(" by") >= 0;
                    ////    });
                    ////}), function (report) {
                    ////    return {
                    ////        reportId: report.idP,
                    ////        reportUri: uri + encodeURIComponent(report.idP) + '/report',
                    ////        debugUri: uri + encodeURIComponent(report.idP) + '/viewForm',
                    ////        reportName: report.name,
                    ////        rowCount: undefined,
                    ////        runTime: 0,
                    ////        error: false
                    ////    };
                    ////});
                    
                    batch = _.map(reports, function (report) {
                        return {
                            reportId: report.idP,
                            reportUri: uri + encodeURIComponent(report.idP) + '/report',
                            debugUri: uri + encodeURIComponent(report.idP) + '/viewForm',
                            reportName: report.name,
                            rowCount: 0,
                            info: '',
                            runTime: 0,
                            error: false
                        };
                    });

                    runReport();

                });
            };

            function runReport () {
                if (batch && batch.length > 0) {

                    var report = batch.pop();

                    $scope.nRun = batch.length;

                    $timeout(function () {

                        var t0 = performance.now();

                        if (!_.has(templateReportIds, report.reportId)) {
                            try {
                                spReportService.getReportData(report.reportId, requestOptions).then(function (data) {
                                    var t1 = performance.now();

                                    var info = sp.result(data, 'Message');
                                    var r = sp.result(data, 'gdata.length');
                                    if (r && r > 0) {
                                        report.rowCount = r;
                                    }
                                    report.runTime = t1 - t0;
                                    report.error = info !== undefined;
                                    if (report.error === true) {
                                        report.info = info;
                                    }

                                    $scope.results.push(report);
                                    runReport();
                                });
                            } catch (e) {
                                var te = performance.now();

                                report.runTime = te - t0;
                                report.error = true;

                                $scope.nRunErrors++;

                                $scope.results.push(report);
                                runReport();
                            }
                        } else {
                            $scope.results.push(report);
                            runReport();
                        }

                    }, 1);
                } else {

                    $scope.nRun = 0;
                    $scope.isRunning = false;
                }
            }

            function getReports() {

                //var ids = [34864, 47519, 59100, 59762, 60822, 70559];
                //return spEntityService.getEntities(ids, 'name');

                //var q = 'name, reportColumns.{ name, isOfType.name }';
                var q = 'name';

                if ($scope.applications.selectedEntityId > 0) {
                    return spEntityService.getEntitiesOfType('core:report', q, { filter: 'id([Resource in application])=' + $scope.applications.selectedEntityId });
                } else {
                    return spEntityService.getEntitiesOfType('core:report', q, { filter: 'true' });
                }
            }
        });
}());
