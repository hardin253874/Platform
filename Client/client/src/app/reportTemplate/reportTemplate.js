// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';
    
    //
    // reportTemplateController
    //
    // Controller for the application manager form.
    //
    // @module mod.app.reportTemplate
    // @example
    //

    angular.module('mod.app.reportTemplate', [
        'ui.router',
        'mod.common.alerts',
        'mod.common.spEntityService',
        'mod.common.spWebService',
        'mod.ui.spReportModelManager'
    ])
        .config(function ($stateProvider) {
            $stateProvider.state('rptTemplate', {
                url: '/{tenant}/{eid}/rptTemplate?path',
                templateUrl: 'reportTemplate/reportTemplate.tpl.html',
                controller: 'rptTemplateController'
            });
        })
        .controller('rptTemplateController', function ($q, $scope, spEntityService, spReportModelManager, spAlertsService, spAppManagerService, spWebService) {
            var reportModelManager = spReportModelManager(null);

            var model = reportModelManager.createModel();

            $scope.rptTemplateOptions = {
                reportOptions: model
            };

            $scope.rptTemplateIndicator = {
                isBusy: false,
                type: 'spinner',
                text: 'Loading...',
                placement: 'window'
            };
            

            // Private Functions
            
            function initialize() {
                if (!model.reportId) {
                    // retrieve the id of the backing report if not done so
                    spEntityService.getEntity('console:applicationManagementReport', 'id').then(function (report) {
                        model.reportId = report.idP;
                    }, function(error) {
                        console.error('appManagerController error:', error);
                        spAlertsService.addAlert('Failed to get the application manager report.', 'error');
                    });
                }
            }
            

            
            initialize();
        });
}());