/*global _, angular, console, sp, spEntity */

(function () {
    'use strict';

    angular.module('app',
        [
            'testPagesAppHelper',
            'mod.common.spEntityService',
            'spApps.reportServices',
            'mod.common.ui.spDragDrop',
            'app.controls.spSearchControl'
        ]);

    angular.module('app')
        .run(configureViewTemplates)
        .run(performDefaultLogin)
        .controller('MyController', MyController);

    /* @ngInject */
    function MyController($scope, spEntityService, spReportService, $timeout) {
        var vm = this;

        // properties
        vm.requestOptions = {
            startIndex: 0,
            pageSize: 200,
            metadata: 'full'
        };
        vm.meta = {};
        vm.cols = [];
        vm.data = [];
        vm.reportFilter = {value: '', isBusy: false};
        vm.allReports = [];
        vm.reports = [];

        // functions
        vm.loadReport = loadReport;

        $scope.$on('signedin', function () {
            $timeout(loadReportList, 0);
        });

        // NOTE - need to use a function style watch as typical string expressions can
        // only be used expressions against the $scope.
        $scope.$watch(function () {
            return vm.reportFilter.value;
        }, updateReportList);

        function loadReportList() {
            var query = {
                root: {
                    id: 'core:report',
                    related: []
                },
                selects: [
                    //{ field: '_id', displayAs: 'runId' },
                    {field: 'name', displayAs: 'Name'}
                ],
                conds: []
            };

            return spReportService.runQuery(query).then(function (results) {
                vm.allReports = _(results.data)
                    .map(function (row, index) {
                        return {
                            id: sp.result(row, 'item.0.value'),
                            name: sp.result(row, 'item.1.value')
                        };
                    })
                    .sortBy('name')
                    .value();
                updateReportList();
                return vm.allReports;
            });
        }

        function updateReportList() {
            vm.reports = _.filter(vm.allReports, function (r) {
                return r.name && r.name.match(new RegExp(vm.reportFilter.value, 'gi'));
            });
        }

        function loadReport(report) {
            var reportEntityQuery = 'name,description,reportColumns.{name,columnExpression.sourceNode.{alias,name}}';

            vm.reportId = report && report.id;
            if (vm.reportId) {
                spReportService.getReportData(report.id, vm.requestOptions)
                    .then(function (data) {
                        loadReportData(data);
                    }, function (error) {
                        console.error('error running report', error);
                    });

                spEntityService.getEntity(report.id, reportEntityQuery)
                    .then(function (entity) {
                        vm.reportEntity = entity;
                        vm.reportJson = spEntity.toJSON(entity);
                    }, function (error) {
                        console.error('error getting report entity', error);
                    });
            }
        }

        function loadReportData(data) {
            vm.meta = data.meta;
            vm.cols = _(data.meta.rcols)
                .map(function (col, eid) {
                    return _.extend(col, {eid: eid});
                })
                .sortBy('ord')
                .value();
            vm.data = data.gdata;
        }
    }

    /* @ngInject */
    function configureViewTemplates(testPagesAppHelper) {
        // add your app and component templates here...
        // this is needed to get the paths correct since we aren't using the build system
        testPagesAppHelper.appTemplate('src/app/controls/spSearchControl/spSearchControl.tpl.html');
    }

    /* @ngInject */
    function performDefaultLogin(spWebService, spLoginService, testPagesAppHelper) {
        spWebService.setWebApiRoot(testPagesAppHelper.getWebApiRoot());
        spLoginService.readiNowLogin('EDC', 'Administrator', 'Password', true, true)
            .then(function (result) {
//                    console.log('login result', result);
            }).catch(function (error) {
                console.log('login fail', error);
            });
    }

}());
