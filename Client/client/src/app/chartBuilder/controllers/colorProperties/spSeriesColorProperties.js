// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */


(function () {
    'use strict';

    /**
    * Module implementing the axis properties dialog.
    * 
    * @module spSeriesColorProperties
    * @example
        
    Using the spAxisProperties:
    
    var model = { series: seriesEntity }
    spSeriesColorProperties.showDialog(model).then(function(result) {
    });
       
    */
    angular.module('mod.app.chartBuilder.controllers.spSeriesColorProperties', ['ui.bootstrap', 'mod.common.alerts', 'mod.common.ui.spColorPicker', 'mod.common.ui.spDialogService', 'mod.common.ui.spColorPickerDropdown'])

     .controller("spSeriesColorPropertiesController", function ($scope, $uibModalInstance, model, spChartService) {

         $scope.model = model || {};
         $scope.model.color = { a: 255, r: 0, g: 0, b: 0 };
         $scope.model.negColor = { a: 255, r: 0, g: 0, b: 0 };

         var chartType = sp.result(model, 'series.chartType.eidP.getAlias');
         $scope.model.allowNegatives = !spChartService.getChartType(chartType).disallowNegColor;

         // Watch options
         $scope.$watch('model', function () {
             var color = $scope.model.series.chartCustomColor;
             var negColor = $scope.model.series.chartNegativeColor;
             var chartTypeAlias = sp.result($scope.model.series, 'chartType.nsAlias');
             $scope.model.useDefault = !color;
             $scope.model.color = hexToRgb(color || spCharts.defaultColor(chartTypeAlias));
             $scope.model.negColor = hexToRgb(negColor || spCharts.defaultNegativeColor(chartTypeAlias));
         });

         // Handle OK
         $scope.ok = function ok() {
             $scope.model.series.chartCustomColor = checkDefault(rgbToHex($scope.model.color), spCharts.defaultColor);
             $scope.model.series.chartNegativeColor = checkDefault(rgbToHex($scope.model.negColor), spCharts.defaultNegativeColor);
             $uibModalInstance.close(true);
         };

         // Handle cancel
         $scope.cancel = function cancel() {
             $uibModalInstance.close(false);
         };

         // Handle defaults
         $scope.defaults = function defaults() {
             var chartTypeAlias = sp.result($scope.model.series, 'chartType.nsAlias');
             $scope.model.color = hexToRgb(spCharts.defaultColor(chartTypeAlias));
             $scope.model.negColor = hexToRgb(spCharts.defaultNegativeColor(chartTypeAlias));
             $scope.model.series.useConditionalFormattingColor = false;
         };

         // RGB to hex
         function rgbToHex(rgb) {
             var hex = d3.rgb(rgb.r, rgb.g, rgb.b).toString();
             return hex;
         }

         // Hex to RGB
         function hexToRgb(hex) {
             hex = hex || '#000000';
             if (hex[0] !== '#')
                 hex = '#' + hex;
             var rgb = d3.rgb(hex);
             rgb.a = 255;
             return rgb;
         }

         function checkDefault(value, defaultValueFn) {
             var chartTypeAlias = sp.result($scope.model.series, 'chartType.nsAlias');
             var defValue = defaultValueFn(chartTypeAlias);
             if (value === defValue)
                 return null;   // indicate with null if default colouring is being used
             return value;
         }

     })
    .service('spSeriesColorProperties', function (spDialogService) {

        // Show dialog, return promise
        this.showDialog = function (model) {
            var defaults = {
                templateUrl: 'chartBuilder/controllers/colorProperties/spSeriesColorProperties.tpl.html',
                controller: 'spSeriesColorPropertiesController',
                resolve: {
                    model: function () { return model; }
                }
            };
            return spDialogService.showDialog(defaults);
        };

    });
}());