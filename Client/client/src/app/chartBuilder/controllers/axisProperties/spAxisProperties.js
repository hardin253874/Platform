// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, sp */

(function () {
    'use strict';

    /**
    * Module implementing the axis properties dialog.
    * 
    * @module spAxisProperties
    * @example
        
    Using the spAxisProperties:
    
    var model = { axis: axisEntity, title: 'title' }
    spAxisProperties.showDialog(model).then(function(result) {
    });
       
    */
    angular.module('mod.app.chartBuilder.controllers.spAxisProperties', [
        'ui.bootstrap',
        'mod.common.alerts',
        'mod.common.ui.spDialogService',
        'mod.app.chartBuilder.directives.spStackProperties'])

     .controller("spAxisPropertiesController", function ($scope, $uibModalInstance, model, spChartBuilderService, spChartService) {

         // model defines .title, .series and .axis
         model = model || {};
         $scope.model = model;
         model.name = model.axis.name;
         model.showGridLines = model.axis.showGridLines;
         model.showAllValues = model.axis.showAllChoiceValues;
         model.isResource = _.includes(['ChoiceRelationship', 'InlineRelationship', 'UserInlineRelationship'], model.sourceType);
         model.isRange = !spChartBuilderService.isDimension(model.sourceType);
         model.isPrimary = model.axis === model.series.primaryAxis;
         model.isValue = model.axis === model.series.valueAxis;
         model.canSeeDateTimeType = _.includes(['Date', 'Time', 'DateTime'], model.sourceType);
         model.canSeeLogType = _.includes(['Int32', 'Decimal', 'Currency'], model.sourceType);

         updateMaxMin('axisMinimumValue', 'minValue');
         updateMaxMin('axisMaximumValue', 'maxValue');

         function updateMaxMin(alias, valueBinding) {
             if ($scope.model.isRange) {
                 var value = $scope.model.axis.getField(alias);
                 $scope.model[valueBinding] = value;
             }
         }

         // Watch minValue
         $scope.$watch('model.minValue', function () {
             model.minMode = (model.minValue === null || model.minValue === '') ? 'auto' : 'manual';
         });

         // Watch maxValue
         $scope.$watch('model.maxValue', function () {
             model.maxMode = (model.maxValue === null || model.maxValue === '') ? 'auto' : 'manual';
         });

         // Handle OK
         $scope.ok = function () {
             if (model.isRange) {
                 model.axis.setAxisMinimumValue(model.minMode === 'auto' ? null : model.minValue);
                 model.axis.setAxisMaximumValue(model.maxMode === 'auto' ? null : model.maxValue);
             }
             model.axis.name = model.name;
             model.axis.showGridLines = model.showGridLines;
             if (model.isResource) {
                 model.axis.showAllChoiceValues = model.showAllValues;
             }
             $uibModalInstance.close(true);
         };

         // Handle Cancel
         $scope.cancel = function () {
             $uibModalInstance.close(false);
         };

         function chartTypeIs(arr) {
             return function() {
                 var ct = sp.result(model, 'series.chartType.nsAlias');
                 var res = _.includes(arr, ct);
                 return res;
             };
         }

         // Should we show stack options?
         $scope.showStack = function() {
             return model.isValue && chartTypeIs(['core:areaChart', 'core:columnChart', 'core:barChart'])();
         };

         // Should we show data label position options?
         $scope.showDataLabelPos = function() {
             return model.isValue && chartTypeIs(['core:columnChart', 'core:barChart', 'core:pieChart', 'core:donutChart', 'core:funnelChart'])();
         };

         // Should we allow the axis type to be picked
         $scope.showAxisType = function () {
             var ct = sp.result(model, 'series.chartType.nsAlias');
             var dt = model.sourceType;
             var supported = spChartService.supportedScales(dt, ct);
             var res = supported.categoryScaleType && supported.linearScaleType;
             return res;
         };
     })
    .service('spAxisProperties', function (spDialogService) {

        // Show dialog, return promise
        this.showDialog = function (model) {
            var defaults = {
                templateUrl: 'chartBuilder/controllers/axisProperties/spAxisProperties.tpl.html',
                controller: 'spAxisPropertiesController',
                resolve: {
                    model: function () { return model; }
                }
            };
            return spDialogService.showDialog(defaults);
        };

    });

}());