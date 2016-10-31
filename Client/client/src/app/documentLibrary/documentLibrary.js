// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';
    
    //
    // applicationManagerController
    //
    // Controller for the application manager form.
    //
    // @module mod.app.applicationManager
    // @example
    //

    angular.module('mod.app.documentLibrary', [
        'ui.router',
        'mod.common.alerts',
        'mod.common.spEntityService',
        'mod.common.spWebService',
        'mod.ui.spReportModelManager'
    ])
        .config(function ($stateProvider) {
            $stateProvider.state('documentFolder', {
                url: '/{tenant}/{eid}/documentFolder?path',
                templateUrl: 'documentLibrary/documentLibrary.tpl.html',
                controller: 'docLibraryController'
            });
        })
        .controller('docLibraryController', function ($q, $scope, $stateParams, spState, spEntityService, spReportModelManager, spAlertsService, spWebService) {
            var reportModelManager = spReportModelManager(null);
            var model = reportModelManager.createModel();

            $scope.docLibraryOptions = {
                reportOptions: model
            };

            $scope.docLibraryIndicator = {
                isBusy: false,
                type: 'spinner',
                text: 'Loading...',
                placement: 'window'
            };

            function initialize() {
                spEntityService.getEntity('core:inFolder', 'id').then(function(rel) {
                    var relDetail = {};
                    relDetail.eid = spState.params.eid;
                    relDetail.relid = rel.idP;
                    relDetail.direction = 'fwd';
                    return relDetail;
                }).then(function(relDetails) {
                    model.relationDetail = relDetails;
                    spEntityService.getEntity('console:documentLibraryReport', 'id').then(function (report) {
                        model.reportId = report.idP;
                    });
                });
            }

            initialize();
        });
}());