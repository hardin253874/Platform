// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, spReportEntityQueryManager, spReportEntity, jsonLookup,
  ReportEntityQueryManager, spReportPropertyDialog */

(function () {
    'use strict';

    angular.module('app.reportBuilder')
        .controller("reportBuilderToolBoxController", ReportBuilderToolBoxController);

    /* @ngInject */
    function ReportBuilderToolBoxController($scope, reportBuilderService) {

        $scope.spReportBuilderService = reportBuilderService;
        $scope.reportBuilderOptions = {"reportEntity": null, "treeNode": null};
        $scope.reportBuilderToolBoxMode = {reportEntity: null, reportTreeNode: null};
        $scope.reportModel = null;
        $scope.currentReportEntity = null;
        $scope.reportBuilderAction = null;
        $scope.$watch('spReportBuilderService.getReportModel()', function () {
            $scope.reportModel = $scope.spReportBuilderService.getReportModel();
            if ($scope.reportModel) {
                $scope.reportBuilderOptions = {"reportModel": $scope.reportModel};
            }
        });

        $scope.$watch('spReportBuilderService.getReportEntityUpdated()', function () {
            $scope.reportBuilderToolBoxMode.reportEntity = $scope.spReportBuilderService.getReportEntity();
            $scope.reportBuilderToolBoxMode.reportTreeNode = $scope.spReportBuilderService.getReportTreeNode();
            if ($scope.reportBuilderToolBoxMode.reportEntity) {
                $scope.reportBuilderOptions = {
                    "reportEntity": $scope.reportBuilderToolBoxMode.reportEntity,
                    "treeNode": $scope.reportBuilderToolBoxMode.reportTreeNode,
                    "reportEntityUpdated": sp.newGuid()
                };
            }
        });
    }
}());
