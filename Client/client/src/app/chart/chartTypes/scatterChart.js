// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.scatterChart = function () {

        this.axisConfig = 'primary-value';

        this.render = function (context, series) {

            var chart = context.chart;
            var accessors = chart.accessors;
            var x = series.horzAxis;
            var y = series.vertAxis;

            // Add scatter dots
            

            var g = chart.drawSymbols(series, series.elem, { x: x.rowScale, y: y.rowScale, useDefault: true });
            chart.decorateDataElements(g);

            // Data labels
            chart.drawLabels(series.elem, {
                data: series.data,
                dy: '-1em'
            });
        };
    };

})(spChartTypes || (spChartTypes = {}));

