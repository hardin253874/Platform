// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.lineChart = function() {

        this.axisConfig = 'primary-value';

        this.render = function (context, series) {

            var chart = context.chart;
            var accessors = chart.accessors;
            var x = series.horzAxis;
            var y = series.vertAxis;

            // Line path factory
            var line = d3.svg.line().x(x.rowScale).y(y.rowScale);

            // Containers
            var gLines = series.elem.append("g").attr("class", "lines");
            var gSeries = spCharts.containers(gLines, series.groups);

            // Lines
            gSeries.append("path")
                .style("fill", "none")
                .style("stroke", function(group) {
                    var col = accessors.color(group.representative);
                    return col;
                })
                .style("stroke-width", "1.5px")
                .attr("d", _.flowRight(line, function(group) {
                    return _.sortBy(group.data, x.rowScale);
                }));

            chart.decorateDataGroupElements(gSeries, { representativeCallback: function (g) { return g.representative; } });

            // Symbols
            var gSymb = chart.drawSymbols(series, series.elem, { x: x.rowScale, y: y.rowScale, makeContainer: true });
            if (gSymb) {
                chart.decorateDataElements(gSymb);
            }

            // Data labels
            chart.drawLabels(series.elem, {
                data: series.data,
                dy: '-1em'
            });

        };
    };

})(spChartTypes || (spChartTypes = {}));

