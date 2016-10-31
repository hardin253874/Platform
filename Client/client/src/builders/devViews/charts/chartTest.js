// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('app.chartTest', [
        'sp.navService',
        'sp.spNavHelper',
        'mod.common.ui.spReport',
        'mod.common.spEntityService',
        'mod.common.ui.spEntityComboPicker'])
        .config(function ($stateProvider) {
            $stateProvider.state('chartTestPage', {
                url: '/{tenant}/{eid}/chartTestPage?path&formId',
                templateUrl: 'devViews/charts/chartTest.tpl.html'
            });

            // register a nav item for this page (most fields are defaulted, but you can provide your own viewType, iconUrl, href and order)
            window.testNavItems = window.testNavItems || {};
            window.testNavItems.chartTestPage = { name: 'Chart Test Page' };

        })
        .controller('chartTestController', ['$scope', 'spEntityService', 'spNavService', 'spNavHelper', function ($scope, spEntityService, spNavService, spNavHelper) {

            $scope.containerWidth = 1024;
            $scope.containerHeight = 480;

            // Selected definition
            $scope.chartPickerOptions = {
                selectedEntityId: 0,
                selectedEntity: null,
                entityTypeId: 'core:chart'
            };

            $scope.$watch('chartPickerOptions.selectedEntity', function () {
                $scope.chartOptions.chartId = $scope.chartPickerOptions.selectedEntityId;
            });

            $scope.builder = function () {
                var id = $scope.chartPickerOptions.selectedEntityId;
                var params = {};
                spNavService.navigateToChildState('chartBuilder', id, params);
            };

            $scope.builderNew = function () {
                spNavHelper.createChart();
            };

            $scope.new = function () {
                $scope.chartOptions.newChart();
            };

            $scope.delete = function () {
                if ($scope.chartPickerOptions.selectedEntityId && window.confirm('Are you sure?'))
                    spEntityService.deleteEntity($scope.chartPickerOptions.selectedEntityId);
            };

            $scope.refresh = function () {
                if ($scope.chartPickerOptions.selectedEntity) {
                    $scope.chartOptions.refreshChart();
                }
            };

            $scope.chartOptions = {
                chartId: 0
            };

        }]);
}());