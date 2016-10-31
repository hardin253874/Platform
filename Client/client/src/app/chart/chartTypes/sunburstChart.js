// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.sunburstChart = function () {
        this.axisConfig = 'none';

        this.render = function (context, series) {

            var chart = context.chart;
            var accessors = chart.accessors;
            var data = _.sortBy(series.data, accessors.primarySource);

            var width = chart.plotArea.width;
            var height = chart.plotArea.height;
            var radius = Math.min(width, height) / 2;

            var getData = function (node) { return node.data; };
            var segmentSize = accessors.valueSource ? _.flowRight(accessors.valueSource, getData) : d3.functor(1);

            var ifNull = function (fn, defaultVal) {
                return function (d) {
                    if (d.data === null)
                        return defaultVal;
                    return fn(d);
                };

            };
            var partition = d3.layout.partition()
                .sort(null)
                .size([2 * Math.PI, radius * radius])
                .value(segmentSize);

            var arc = d3.svg.arc()
                .startAngle(function (d) { return d.x; })
                .endAngle(function (d) { return d.x + d.dx; })
                .innerRadius(function (d) { return Math.sqrt(d.y); })
                .outerRadius(function (d) { return Math.sqrt(d.y + d.dy); });

            var hierarchy = chart.buildHierarchy(data, accessors.primarySource, accessors.associateSource, { singleRoot: true, strictTree: true });

            var gRoot = series.elem
                .append("g")
                .attr("transform", "translate(" + width / 2 + "," + height / 2 + ")")
                .datum(hierarchy.root);

            var g = spCharts.containers(gRoot, partition.nodes);

            if (accessors.imageSource) {
                chart.appendImagePattern(g, _.flowRight(accessors.imageSource, getData));
            }
            var path = g.append("path")
                .attr("display", function(d) { return d.depth ? null : "none"; }) // hide inner ring
                .attr("d", arc)
                .style("stroke", "#fff")
                .style("fill", ifNull(_.flowRight(accessors.fill, getData), 'black'));

            chart.decorateDataElements(path, { dataCallback: getData });

            if (series.textSource) {
                
                chart.drawLabels(gRoot, {
                    rootDatum: hierarchy.root,
                    data: partition.nodes,
                    dataCallback: getData,
                    box: true,
                    xpos: function (d) { return arc.centroid(d)[0] + width/2; },
                    ypos: function (d) { return arc.centroid(d)[1] + height/2; },
                    text: hierarchy.textSource
                });
            }
        };
    };

})(spChartTypes || (spChartTypes = {}));

