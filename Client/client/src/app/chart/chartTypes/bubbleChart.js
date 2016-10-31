// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.bubbleChart = function() {

        this.axisConfig = 'primary-value';

        this.render = function(context, series) {

            var chart = context.chart;
            var accessors = chart.accessors;
            var x = series.horzAxis;
            var y = series.vertAxis;

            var maxMaxRadius = 100;
            var suggestRadius = 30;
            var minMaxRadius = 10;

            var overfill = 1.2;
            var xSize = spCharts.useBands(x.method) ? x.bandWidthFull() / 2 * overfill : suggestRadius;
            var ySize = spCharts.useBands(y.method) ? y.bandWidthFull() / 2 * overfill : suggestRadius;
            var maxRadius = Math.max(minMaxRadius, Math.min(xSize, ySize, maxMaxRadius));

            var radius;
            if (accessors.sizeSource) {
                var sqrtSource = _.flowRight(Math.sqrt, Math.abs, accessors.sizeSource);
                var sizeAxis = spEntity.fromJSON({ axisMinimumValue: 0, axisMaximumValue: jsonDecimal(), axisScaleType: jsonLookup('linearScaleType') });
                var domain = spCharts.getSourceDomain(context, series, sqrtSource, 'linearScaleType');
                var size = chart.getScale(sizeAxis, series.sizeSource, null, { domain: domain, scaleType: 'linearScaleType' });
                size.autoRange([0, maxRadius]);
                radius = _.flowRight(size.scale, sqrtSource);
            } else {
                radius = maxRadius + 'px';
            }

            // Add grouped bars
            var g = spCharts.containers(series.elem, series.data);

            if (accessors.imageSource) {
                chart.appendImagePattern(g, accessors.imageSource);
            }

            g.append("circle")
                .attr("r", 0)
                .style("fill", accessors.fill)
                .attr("cx", x.rowScale)
                .attr("cy", y.rowScale)
                .transition().duration(spCharts.animateTime)
                .attr("r", radius);

            chart.decorateDataElements(g);

            // Data labels
            chart.drawLabels(series.elem, {
                data: series.data,
                box: true
            });

        };
    };

})(spChartTypes || (spChartTypes = {}));

