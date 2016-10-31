// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing the toolbox of available report columns for the chart builder.
    * 
    * @module availableSources                    
    */
    angular.module('mod.app.chartBuilder.directives.spChartBuilderAvailableSources', [
        'mod.app.chartBuilder.services.spChartBuilderService',
        'sp.navService'
    ])
        .directive('spAvailableSources', function () {

            return {
                restrict: 'E',
                templateUrl: 'chartBuilder/directives/spChartBuilderAvailableSources/spChartBuilderAvailableSources.tpl.html',
                replace: true,
                transclude: false,
                controller: 'spChartBuilderAvailableSourcesController',
                scope: {
                    model: '='
                }
            };
        })
        .controller('spChartBuilderAvailableSourcesController', function ($scope, $rootScope, spChartBuilderService, spNavService) {

            // Dragging of sources
            $scope.dragOptions = {};
            $scope.appData = $rootScope.appData;

            // Available sources, which are bound to the UI
            $scope.sources = [];

            // Watch reportMetadata for available columns
            $scope.$watch('model.reportMetadata', function () {
                $scope.sources = spChartBuilderService.getAvailableColumnSources($scope.model);
            });

            $scope.reportId = function () {
                return sp.result($scope, 'model.chart.chartReport.idP');
            };

            $scope.reportName = function() {
                var res = sp.result($scope, 'model.reportMetadata.title') || '<none>';
                return res;
            };

            $scope.convertTypeToImageUrl = spUtils.convertTypeToImageUrl;

            $scope.refreshSources = function () {
                spChartBuilderService.loadReportMetadata($scope.model).then(function () {
                    $scope.sources = spChartBuilderService.getAvailableColumnSources($scope.model);
                });
            };

        });
}());