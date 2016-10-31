// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.funnelChart = function () {

        this.axisConfig = 'none';

        this.render = function (context, series) {

            var chart = context.chart;
            var accessors = chart.accessors;
            var width = chart.plotArea.width;
            var height = chart.plotArea.height;

            var labelOutside = spCharts.labelOutside(series, 'Inside');

            var getValue = function (d) { return Math.abs(accessors.valueSource(d)); };
            var getData = function (p) { return p.data; };

            // Get sum of all values
            var sum = 0;
            var data = _.forEach(series.data, function (d) {
                sum += getValue(d);
            });
            if (sum <= 0)
                return;

            // Get positions. Positions will be normalized between 0.0 and 1.0
            var partialSum = 0;
            var positions = _.compact(_.map(series.data, function (d) {
                var value = getValue(d);
                if (!value)
                    return null;
                var p0 = 1 - Math.sqrt((sum - partialSum) / sum);
                partialSum += value;
                var p1 = 1 - Math.sqrt((sum - partialSum) / sum);
                var res = {
                    p0: p0,
                    p1: p1,
                    data: d
                };
                return res;
            }));
            if (positions.length > 1) {
                positions[positions.length - 1].isLast = true;
            }

            // Define scales to go from 0-1 positional co-ordinates to pixel coordinates
            // Note: x-scales must meet at a single apex (0.5), or area calculations will be incorrect
            // (Round down for crisp horizontal edges, while being able to maintain antialiased diagonal edges)
            var py = _.flowRight(Math.floor, d3.scale.linear().domain([0, 1]).range([0.1 * height, 0.9 * height]));
            var px0 = d3.scale.linear().domain([0, 1]).range([0.15 * width, 0.43 * width]);
            var px1 = d3.scale.linear().domain([0, 1]).range([0.85 * width, 0.57 * width]);

            // Convert positions to layer coordinates
            var layer = function(pos) {
                var y0 = py(pos.p0);
                var y1 = py(pos.p1);
                var res = {
                    "values": [
                        { x: px0(pos.p0), y: y0 },
                        { x: px1(pos.p0), y: y0 },
                        { x: px1(pos.isLast ? pos.p0 : pos.p1), y: y1 },
                        { x: px0(pos.isLast ? pos.p0 : pos.p1), y: y1 }
                    ]
                };
                return res;
            };

            // Convert coordinates to closed path
            var area = function (points) {
                if (!points) return '';
                var res = '';
                for (var i=0; i<points.length; i++)
                    res += (i===0?'M':'L') + points[i].x+','+points[i].y;
                res += 'Z';
                return res;
            };

            // Start rendering
            var svg = series.elem
                .append("g")
                .attr("class", "data");

            // Data
            var g = spCharts.containers(svg, positions);

            if (accessors.imageSource) {
                chart.appendImagePattern(g, _.flowRight(accessors.imageSource, getData));
            }

            g.append('path')
                .attr('d', function(p) { return area(layer(p).values); })
                .style("stroke", "#fff")
                .style("stroke-width", "2")
                .style("fill", _.flowRight(accessors.fill, getData));

            chart.decorateDataElements(g, { dataCallback: getData });

            // Text
            if (series.textSource) {

                var labelXpos;
                if (labelOutside) {
                    labelXpos = function(p) { return px1((p.isLast? p.p0 : (p.p0 + p.p1) / 2)) + 10; };
                } else {
                    labelXpos = width / 2;
                }

                chart.drawLabels(series.elem, {
                    data: positions,
                    dataCallback: getData,
                    box: !labelOutside,
                    textAnchor: labelOutside ? 'start' : 'middle',
                    xpos: labelXpos,
                    ypos: function (p) { return py((p.p0 + p.p1) / 2); }
                });
            }

        };

    };

})(spChartTypes || (spChartTypes = {}));

