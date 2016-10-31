// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global	angular,	console	*/
(function () {
    'use	strict';

    angular.module('mod.app.chartBuilder', [
        'mod.common.ui.spChartService',
        'mod.app.chartBuilder.services',
        'mod.app.chartBuilder.directives',
        'mod.app.chartBuilder.controllers'
    ])
        .config(function($stateProvider) {

            $stateProvider.state('chartBuilder', {
                url: '/{tenant}/{eid}/chartBuilder?reportId&path',
                templateUrl: 'chartBuilder/views/chartBuilder.tpl.html',
                data: {
                    leftPanelTemplate: 'chartBuilder/views/chartToolbox.tpl.html',
                    leftPanelTitle: 'Chart Builder',
                    hideAppTabs: true,
                    hideAppToolboxButton: true
                }
            });
        })
        .controller('chartBuilderToolboxController', function($scope, spNavService) {
            // Capture spNavService so we can watch it
            $scope.spNavService = spNavService;

            // Watch the navItem for the model
            $scope.$watch('spNavService.getCurrentItem().data.chartBuilderModel', function (model) {
                $scope.model = model;
            });

        })
        .controller('chartBuilderPageController', function ($q, $scope, $stateParams, spNavService, spChartBuilderService) {

            var chartId = sp.coerseToNumberOrLeaveAlone($stateParams.eid) || 0;

            // Attempt to restore chart from nav item
            var navItem = spNavService.getCurrentItem();
            $scope.model = sp.result(navItem, 'data.chartBuilderModel');

            var promise = $q.when();

            if (!$scope.model) {
                // Create/load chart model
                if (!chartId) {
                    promise = $q.when(spChartBuilderService.createChartModel($stateParams));
                } else {
                    promise = spChartBuilderService.loadChartModel(chartId);
                }

                // Store model
                promise = promise.then(function (model) {
                    $scope.model = model;
                    navItem.data = navItem.data || {};
                    navItem.data.chartBuilderModel = model;
                });
            }
            
            // Initialize model
            promise
                .then(function () {
                    // load report metadata
                    spChartBuilderService.loadReportMetadata($scope.model);
                })
                .then(function () {
                    // create default series
                    if ($scope.model.chart.dataState === spEntity.DataStateEnum.Create) {
                        spChartBuilderService.applySuggestions($scope.model);
                    }
                });

            // Page dirty handler
            navItem.isDirty = function () {
                return spChartBuilderService.isDirty($scope.model);
            };

            // Page dirty message
            navItem.dirtyMessage = function () {
                return spChartBuilderService.dirtyMessage($scope.model);
            };
        });

}());