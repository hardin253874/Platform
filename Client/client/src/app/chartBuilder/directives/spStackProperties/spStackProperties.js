// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing the series toolbox for the chart builder.
    * This allows each chart series to be configured, and various inputs (such as data & text) sources to be dropped.
    * 
    * @module seriesPanel                    
    */
    angular.module('mod.app.chartBuilder.directives.spStackProperties', [
    ])
        .directive('spStackProperties', function () {

            return {
                restrict: 'E',
                templateUrl: 'chartBuilder/directives/spStackProperties/spStackProperties.tpl.html',
                replace: true,
                scope: {
                    model: '='
                }
            };
        });
}());