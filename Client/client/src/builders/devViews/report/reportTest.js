// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

(function () {
    'use strict';

    angular.module('app.reportTest', ['mod.common.ui.spReport', 'mod.common.ui.spEntityComboPicker', 'mod.ui.spReportConstants'])
        .controller('reportTestController', ReportTestController);

    /* @ngInject */
    function ReportTestController($scope, reportPageSize) {
        // Selected definition

        var model = {
            reportPageSize: reportPageSize.value,
            mode: 'View',
            modes: ['View', 'Edit'],
            reportOptions: {
                selectedItems: null,
                multiSelect: true,
                reportId: 0,
                isEditMode: false
            }
        };

        $scope.model = model;
        $scope.rowNumber = 0;

        $scope.reportPickerOptions = {
            selectedEntityId: 0,
            selectedEntity: null,
            entityTypeId: 'core:report'
        };

        $scope.$watch('reportPickerOptions.selectedEntity', function () {
            if ($scope.reportPickerOptions.selectedEntity) {
                model.reportOptions.reportId = $scope.reportPickerOptions.selectedEntity.id();
            }
        });

        $scope.setPageSize = function () {
            reportPageSize.value = model.reportPageSize;
        };

        $scope.$watch('model.mode', function () {
            model.reportOptions.isEditMode = model.mode === 'Edit';
        });

        $scope.$watch('rowNumber', function (a, b) {
            if (a === b) return;

            var id = sp.result(model.reportOptions, 'selectedItems.0.eid');
            var index = 0;
            setSelectedItem(id, index);
        });

        function setSelectedItem(id, index) {
            model.reportOptions.selectedItems = [{eid: id, rowIndex: index}];
        }
    }
}());