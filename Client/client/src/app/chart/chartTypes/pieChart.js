// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.donutChart = function (context) {
        var pie = new spChartTypes.pieChart();
        _.assign(this, pie);
        this.innerRadiusPercent = 0.58;
    };

    spChartTypes.pieChart = function () {
        this.axisConfig = 'none';

        this.render = function(context, series) {

            var chart = context.chart;
            var accessors = chart.accessors;

            var valueSourceAbs = function (d) { return Math.abs(accessors.valueSource(d)); };
            var data = _.sortBy(series.data, valueSourceAbs);
            var labelRadius, anchor;

            var width = chart.plotArea.width;
            var height = chart.plotArea.height;
            var radius = Math.min(width, height) / 2 - 10;

            var labelOutside = spCharts.labelOutside(series, 'Inside');
            if (labelOutside && accessors.textSource) {
                radius = radius * 0.9;
            }

            var arc = d3.svg.arc()
                .outerRadius(radius)
                .innerRadius(radius * (this.innerRadiusPercent || 0));

            var filterUndef = function(v) { return v ? v : 0; };
            var getData = function(pieSlice) { return pieSlice.data; };

            var pie = d3.layout.pie()
                .sort(null)
                .value(_.flowRight(filterUndef, valueSourceAbs));

            var gRoot = series.elem
                .append("g")
                .attr("transform", "translate(" + width / 2 + "," + height / 2 + ")");

            var g = gRoot.selectAll(".arc")
                .data(pie(data))
                .enter().append("g")
                .attr("class", "arc");

            if (accessors.imageSource) {
                chart.appendImagePattern(g, _.flowRight(accessors.imageSource, getData));
            }

            g.append("path")
                .attr("d", arc)
                .style("stroke", "#fff")
                .style("stroke-width", "2")
                .style("fill", _.flowRight(accessors.fill, getData));

            chart.decorateDataElements(g, { dataCallback: getData } );

            if (series.textSource) {

                if (labelOutside) {
                    labelRadius = 1.1 * radius;
                    anchor = function (d) {
                        var c = arc.centroid(d);
                        return c[0] > 0 ? 'start' : 'end';
                    };
                } else {
                    labelRadius = 0.75 * radius;
                    anchor = 'middle';
                }

                chart.drawLabels(gRoot, {
                    data: pie(data),
                    dataCallback: getData,
                    box: !labelOutside,
                    xpos: function (d) { return spCharts.normalize(arc.centroid(d), labelRadius)[0] + width/2; },
                    ypos: function (d) { return spCharts.normalize(arc.centroid(d), labelRadius)[1] + height/2; },
                    textAnchor: anchor
                    });
            }
        };
    };

})(spChartTypes || (spChartTypes = {}));

