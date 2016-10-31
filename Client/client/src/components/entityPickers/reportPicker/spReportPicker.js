// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a report picker.
    * That is, a picker for picking a report. (as opposed to entityReportPicker, which is a picker for picking any arbirary entity using a report)
    * 
    * @module spEntityComboBoxPicker
    * @example
        
    Using the spReportPicker:
    
    &lt;sp-report-picker report="reportEntity" options="options"&gt;&lt;/sp-report-picker&gt
       
    where options is available on the controller with the following properties:
        - disabled - {bool} - true to disable the control, false otherwise
    
    Note: you only need to specify entityTypeId or entities.
    * 
    */
    angular.module('mod.common.ui.spReportPicker', ['mod.common.ui.spEntityComboPicker'])
        .directive('spReportPicker', function () {
            return {
                restrict: 'E',
                templateUrl: 'entityPickers/reportPicker/spReportPicker.tpl.html',
                controller: 'spReportPickerController',
                scope: {
                    options: '=',
                    report: '='
                }
            };
        })
        .controller('spReportPickerController', function ($scope) {

            $scope.model = {
                pickerOptions: {
                    entityTypeId: 'core:report',
                    selectedEntityId: 0,
                    isInPicker: true
                }
            };

            $scope.$watch('report', function () {
                var reportId = sp.result($scope, 'report.idP') || 0;
                $scope.model.pickerOptions.selectedEntityId = reportId;
            });
            $scope.$watch('model.pickerOptions.selectedEntityId', function () {
                var id = sp.result($scope, 'model.pickerOptions.selectedEntityId') || 0;
                var reportId = sp.result($scope, 'report.idP') || 0;
                if (id !== reportId) {
                    $scope.report = id ? spEntity.fromId(id) : null;
                }
            });
        });
}());