// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module for selecting a chart type
    * 
    * @module spChartTypes                    
    */
    angular.module('mod.app.chartBuilder.directives.spChartTypes', [
        'mod.common.ui.spChartService',
        'mod.app.chartBuilder.services.spChartBuilderService'
    ])
        .directive('spChartTypes', function () {

            return {
                restrict: 'E',
                templateUrl: 'chartBuilder/directives/spChartTypes/spChartTypes.tpl.html',
                replace: true,
                transclude: false,
                controller: 'spChartTypesController',
                scope: {
                    chartType: '='
                }
            };
        })
        .controller('spChartTypesController', function ($scope, spChartService) {

            $scope.chartTypes = spChartService.chartTypes;

            $scope.onSelect = function(chartType) {
                $scope.chartType = chartType;
            };
        });
}());