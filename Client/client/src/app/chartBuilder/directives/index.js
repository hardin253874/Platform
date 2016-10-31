// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use	strict';

    /**
    * Module implementing the index of directives used in chart builder.
    */
    angular.module('mod.app.chartBuilder.directives',
        [
            'mod.app.chartBuilder.directives.spChartBuilder',
            'mod.app.chartBuilder.directives.spChartBuilderToolbox',
            'mod.app.chartBuilder.directives.spChartBuilderAvailableSources',
            'mod.app.chartBuilder.directives.spSeriesPanel',
            'mod.app.chartBuilder.directives.spChartTypes',
            'mod.app.chartBuilder.directives.spAxisTypeProperties',
            'mod.app.chartBuilder.directives.spStackProperties',
            'mod.app.chartBuilder.directives.spDataLabelProperties'
        ]);
}());