// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.matrixChart = function () {

        this.axisConfig = 'primary-value';

        this.render = function (context, series) {
            var chart = context.chart;
            var accessors = chart.accessors;
            var x = series.horzAxis;
            var y = series.vertAxis;

            // Add cells
            var g = spCharts.containers(series.elem, series.data);

            if (accessors.imageSource) {
                chart.appendImagePattern(g, accessors.imageSource);
            }

            var rect = {
                x: _.flowRight(x.bandStartFull, accessors.primarySource),
                y: _.flowRight(y.bandStartFull, accessors.valueSource),
                width: x.bandWidthFull,
                height: y.bandWidthFull
            };

            g.append("rect")
                .attr("class", "matrix-cell")
                .call(chart.applyRect, rect)
                .style("fill", accessors.fill);

            chart.decorateDataElements(g);

            // Data labels
            chart.drawLabels(series.elem, {
                data: series.data,
                clipRect: rect
            });

            // Draw lines
            series.elem.append("g").selectAll("line")
                .data(x.domain)
                .enter()
                .append("line")
                .attr("x1", function (d) { return x.d3scaleFull(d) + x.bandWidthFull(); })
                .attr("x2", function (d) { return x.d3scaleFull(d) + x.bandWidthFull(); })
                .attr("y1", 0)
                .attr("y2", chart.plotArea.height)
                .attr("class", "gridline");
            series.elem.append("g").selectAll("line")
                .data(y.domain)
                .enter()
                .append("line")
                .attr("y1", y.d3scaleFull)
                .attr("y2", y.d3scaleFull)
                .attr("x1", 0)
                .attr("x2", chart.plotArea.width)
                .attr("class", "gridline");
        };

    };

})(spChartTypes || (spChartTypes = {}));

