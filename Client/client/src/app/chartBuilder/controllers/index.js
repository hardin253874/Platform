// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use	strict';

    /**
    * Module implementing the index of directives used in chart builder.
    */
    angular.module('mod.app.chartBuilder.controllers',
        [
            'mod.app.chartBuilder.controllers.spAxisProperties',
            'mod.app.chartBuilder.controllers.spSeriesColorProperties',
            'mod.app.chartBuilder.controllers.spNewChartDialog',
            'mod.app.chartBuilder.controllers.spSymbolProperties'
        ]);
}());