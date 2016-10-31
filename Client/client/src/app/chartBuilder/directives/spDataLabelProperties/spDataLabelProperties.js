// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * spDataLabelProperties
    */
    angular.module('mod.app.chartBuilder.directives.spDataLabelProperties', [
    ])
        .directive('spDataLabelProperties', function () {

            return {
                restrict: 'E',
                templateUrl: 'chartBuilder/directives/spDataLabelProperties/spDataLabelProperties.tpl.html',
                replace: true,
                scope: {
                    model: '='
                }
            };
        });
}());