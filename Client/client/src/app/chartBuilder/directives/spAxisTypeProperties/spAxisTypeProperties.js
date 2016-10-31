// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * spAxisTypeProperties
    * 
    * @module seriesPanel                    
    */
    angular.module('mod.app.chartBuilder.directives.spAxisTypeProperties', [
    ])
        .directive('spAxisTypeProperties', function () {

            return {
                restrict: 'E',
                templateUrl: 'chartBuilder/directives/spAxisTypeProperties/spAxisTypeProperties.tpl.html',
                replace: true,
                scope: {
                    model: '='
                }
            };
        });
}());