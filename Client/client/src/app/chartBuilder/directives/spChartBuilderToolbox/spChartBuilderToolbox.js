// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a form builder toolbox control.
    * spFormBuilderToolbox provides the toolbox for interacting with the canvas.
    *
    * @module spFormBuilderToolbox
    * @example

    Using the spFormBuilderToolbox:

    &lt;sp-form-builder-toolbox&gt;&lt;/sp-form-builder-toolbox&gt
        
    */
    angular.module('mod.app.chartBuilder.directives.spChartBuilderToolbox', [
        'mod.app.chartBuilder.services.spChartBuilderService'
    ])
        .directive('spChartBuilderToolbox', function (spChartBuilderService) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    model: '='
                },
                templateUrl: 'chartBuilder/directives/spChartBuilderToolbox/spChartBuilderToolbox.tpl.html',
                link: function (scope) {

                    // A general drop area to allow removals
                    scope.dropOptions = spChartBuilderService.rootDropOptions;

                }

            };
        });
}());