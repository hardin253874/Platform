// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, spReportEntityQueryManager, spReportEntity, jsonLookup,
  ReportEntityQueryManager, spReportPropertyDialog */

(function () {
    'use strict';

    angular.module('app.reportBuilder')
        .config(function ($stateProvider) {
            $stateProvider.state('reportBuilder', {
                url: '/{tenant}/{eid}/reportBuilder?path',
                templateUrl: 'reportBuilder/reportBuilder.tpl.html',
                data: {
                    leftPanelTemplate: 'reportBuilder/toolbox.tpl.html',
                    leftPanelTitle: 'Report Builder',
                    hideAppTabs: true,
                    hideAppToolboxButton: true
                }
            });

            $stateProvider.state('reportSchemaBuilder', {
                url: '/{tenant}/{eid}/reportSchemaBuilder?schemaOnly&path',
                templateUrl: 'reportBuilder/reportBuilder.tpl.html',
                data: {
                    leftPanelTemplate: 'reportBuilder/toolbox.tpl.html',
                    leftPanelTitle: 'Report Schema Builder',
                    hideAppTabs: true,
                    hideAppToolboxButton: true
                }
            });

            // register a nav item for this page (most fields are defaulted, but you can provide your own viewType, iconUrl, href and order)
            window.testNavItems = window.testNavItems || {};
            window.testNavItems.reportBuilder = {name: 'Report Builder'};
        })
        .constant('reportBuilderState', {
            name: 'reportbuilder',
            url: '/reportBuilder',
            views: {
                'content@': {
                    controller: 'reportBuilderPageController',
                    templateUrl: 'reportBuilder/reportBuilder.tpl.html'
                }

            }
        });

}());
