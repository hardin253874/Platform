// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, sp */

(function () {
    'use strict';

    /**
    * Module implementing a form builder chart control.
    * spFormBuilderChartControl renders a child form that has been placed on the form during design time.
    *
    * @module spFormBuilderChartControl
    */
    angular.module('mod.app.formBuilder.directives.spFormBuilderChartControl', [
        'mod.common.spUuidService'
    ])
        .directive('spFormBuilderChartControl', function (spUuidService) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: true,
                transclude: false,
                scope: {
                    control: '=?' // the chartRenderControl
                },
                templateUrl: 'formBuilder/directives/spFormBuilder/directives/spFormBuilderChartControl/spFormBuilderChartControl.tpl.html',
                link: function (scope, element, attrs, ctrl) {

                    scope.hoverOptions = {
                        className: 'sp-form-builder-chart-control',
                        hoverClassName: 'sp-form-builder-hover',
                        childSelector: '> .ui-resizable-handle, > .sp-form-builder-toolbar'
                    };

                    /////
                    // Define the 'name' property.
                    /////
                    Object.defineProperty(scope, 'name', {
                        get: function () {
                            return scope.control.name || scope.control.chartToRender.name;
                        },
                        set: function (newName) {
                            scope.control.name = newName;
                        },
                        enumerable: true,
                        configurable: true
                    });

                    scope.model = {
                        chartId: sp.result(scope.control, 'chartToRender.idP')
                    };

                    scope.chartOptions = {
                        containerClass: '.sp-chart',
                        mode: 'screenBuilder'
                    };

                    scope.onConfigureClick = function () {
                        console.error('configure chart');
                    };
                }
            };
        });
}());
