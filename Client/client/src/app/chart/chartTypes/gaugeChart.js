// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.gaugeChart = function() {

        this.axisConfig = 'none';

        this.render = function(context, series) {

            var π = Math.PI;
            var chart = context.chart;
            var accessors = chart.accessors;
            var width = chart.plotArea.width;
            var height = chart.plotArea.height;
            var radius = Math.min(width, height) / 2;

            var totalAngle = 270;

            var needleValue = series.data.length > 0 ? accessors.valueSource(series.data[0]) : null;

            var scale = spCharts.getValueScale(chart, series, [0, 100]);
            scale.autoRange([totalAngle / -2, totalAngle / 2]);

            var rotate = function(d) { return "rotate(" + scale.d3scale(d) + ")"; };

            // Convert coordinates to paths   
            var area = d3.svg.area()
                .interpolate('linear-closed')
                .x1(function(d) { return d.x; })
                .y1(function(d) { return d.y; });

            // Start rendering
            var svg = chart.svg
                .append("g")
                .attr("class", "gauge-chart")
                .attr("transform", "translate(" + width / 2 + "," + height / 2 + ")");

            // Border
            svg.append('g')
                .attr("class", "gauge-outer")
                .append("circle")
                .attr("cx", 0)
                .attr("cx", 0)
                .attr("r", radius * 0.9);
            svg.append('g')
                .attr("class", "gauge-inner")
                .append("circle")
                .attr("cx", 0)
                .attr("cx", 0)
                .attr("r", radius * 0.85);

            // Color dividers
            var colors = [
                { size: 70, color: 'col1' },
                { size: 15, color: 'col2' },
                { size: 15, color: 'col3' }
            ];
            var start = 0;
            _.forEach(colors, function(c) {
                c.start = start;
                start = c.end;
            });

            var pie = d3.layout.pie()
                .sort(null)
                .startAngle(-π * totalAngle / 360)
                .endAngle(π * totalAngle / 360)
                .value(function(c) { return c.size; });

            var arc = d3.svg.arc()
                .outerRadius(radius * 0.8)
                .innerRadius(radius * 0.7);

            var k = pie(colors);

            var g = svg.append('g')
                .attr('class', 'gauge-ranges')
                .selectAll(".arc")
                .data(pie(colors))
                .enter().append("g")
                .attr("class", "arc")
                .append("path")
                .attr("d", arc)
                .style("stroke", "#fff")
                .style("stroke-width", "2")
                .attr("class", function(d) {
                    return d.data.color;
                });

            // Needle
            if (needleValue) {
                svg.append("path")
                    .attr("transform", rotate(needleValue))
                    .attr("class", "needle")
                    .attr("d",
                        'M' + (-radius * 0.05) + ' 0' +
                            'L 0 ' + (-radius * 0.68) + ' ' +
                            'L' + (radius * 0.05) + ' 0' +
                            'L 0 ' + (radius * 0.2) + ' z');
            }
            svg.append("circle")
                .attr("class", "needle-cap")
                .attr("r", radius * 0.1);

            // Ticks
            svg.append("g")
                .attr("class", "ticks")
                .selectAll("line")
                .data(scale.ticks(10))
                .enter().append("line")
                .attr("transform", rotate)
                .attr("x1", 0)
                .attr("y1", -radius * 0.62)
                .attr("x2", 0)
                .attr("y2", -radius * 0.68)
                .style("stroke", "black")
                .style("stroke-width", "1px");


            // Text
            var drawLabel = function(value, align) {
                svg.append("text")
                    .attr("class", "gauge-label")
                    .attr("x", Math.sin(scale.d3scale(value) * π / 180) * radius * 0.57)
                    .attr("y", Math.cos(scale.d3scale(value) * π / 180) * -radius * 0.57)
                    .attr("dy", ".35em")
                    .style("text-anchor", align)
                    .text(accessors.valueSource.formatter(value));
            };

            drawLabel(scale.domain[0], "start");
            drawLabel(scale.domain[1], "end");

            svg.append("text")
                    .attr("class", "gauge-value")
                    .attr("x", 0)
                    .attr("y",  radius * 0.65)
                    .attr("dy", ".35em")
                    .style("text-anchor", "middle")
                    .text(accessors.valueSource.formatter(needleValue));
        };

    };

})(spChartTypes || (spChartTypes = {}));

