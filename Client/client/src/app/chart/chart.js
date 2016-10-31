// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, sp */
(function () {
    'use strict';

    /*
     * Main entry point for chart view page
     */

    angular.module('mod.app.chart', [
        'ui.router',
        'sp.navService',
        'mod.common.ui.spContextMenu',
        'mod.app.navigationProviders',
        'mod.common.spEntityService',
        'mod.common.spVisDataActionService'
    ]);

    angular.module('mod.app.chart')
        .config(chartViewStateConfiguration)
        .controller('chartController', ChartController);

    /* @ngInject */
    function chartViewStateConfiguration($stateProvider) {

        var data = {
            showBreadcrumb: false
        };
        $stateProvider.state('chart', {
            url: '/{tenant}/{eid}/chart?path',
            templateUrl: 'chart/views/chart.tpl.html',
            data: data
        });
    }

    /* @ngInject */
    function ChartController($scope, $stateParams, spNavService, spNavigationBuilderProvider, spEntityService, spVisDataActionService) {
        var chartId = sp.coerseToNumberOrLeaveAlone($stateParams.eid) || 0,
            navigationBuilderProvider = spNavigationBuilderProvider($scope);

        $scope.model = {
            chartEntity: null,
            chartId: chartId,
            configContextMenu: {
            }
        };

        // Capture spNavService so we can watch it
        $scope.navService = spNavService;
        $scope.viewerOptions = {};

        loadChartEntityDetails();

        // Navigate to builder to modify the chart.
        $scope.configMenuModifyEntity = function () {
            if (!spNavService.isSelfServeEditMode) {
                return;
            }

            spNavService.navigateToChildState('chartBuilder', $stateParams.eid, $stateParams);
        };

        // Configure the chart's properties
        $scope.configMenuUpdateEntityProperties = function () {
            if (!spNavService.isSelfServeEditMode) {
                return;
            }
            navigationBuilderProvider.configureNavItem($scope.model.chartEntity);
        };

        //navigate to report page on chart segment click.
        $scope.$watch('viewerOptions.drilldownConds', function (newDrilldownConds, oldDrilldownConds) {

            //if the init of newDrilldownConds is set as empty array [] and oldDrilldownConds is null or undefined, the drilldownConds is not changed            
            if (!newDrilldownConds || (!oldDrilldownConds && !newDrilldownConds.isConditions))
                return;

            var actionData = { drilldownConds: newDrilldownConds };
            spVisDataActionService.executeClickAction(actionData, true, sp.result($scope.model, 'chartEntity.chartReport'));
        });

        //Navigate to view form on chart data point click.
        $scope.$watch('viewerOptions.selectedEntityId', function (id) {
            spVisDataActionService.executeClickAction(id, false, sp.result($scope.model, 'chartEntity.chartReport'));
        });

        // Delete the chart
        $scope.configMenuDeleteEntity = function () {
            if (!spNavService.isSelfServeEditMode) {
                return;
            }

            navigationBuilderProvider.removeNavItem($scope.model.chartEntity);
        };

        function loadChartEntityDetails() {
            if ($scope.model.chartId) {
                spEntityService.getEntity($scope.model.chartId, 'name, description, isOfType.{alias},hideOnDesktop,hideOnTablet,hideOnMobile,isPrivatelyOwned', { hint: 'chart', batch: true }).then(function (chartEntity) {
                    $scope.model.chartEntity = chartEntity;
                    $scope.model.configContextMenu = navigationBuilderProvider.buildConfigureContextMenu(chartEntity);
                });
            }
        }
    }

}());