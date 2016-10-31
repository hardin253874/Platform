// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.radialTreeGraph = function () {

        this.axisConfig = 'none';

        this.render = function (context, series) {

            var chart = context.chart;
            var accessors = series.accessors;
            var data = series.data;
            var width = chart.plotArea.width;
            var height = chart.plotArea.height;
            var diameter = Math.min(width, height);

            var hierarchy = chart.buildHierarchy(data, accessors.primarySource, accessors.associateSource, { singleRoot: true, strictTree: true });

            var textSource = accessors.textSource || accessors.primarySource;
            var getData = function (d) { return d.data; };
            var ifParent = function (fn, defaultVal) {
                return function (d) {
                    if (d.data === null || d.isParent)
                        return defaultVal;
                    return fn(d);
                };
            };
            // Radial tree

            var tree = d3.layout.tree()
                .size([360, diameter / 2 - 50])
                .separation(function (a, b) {
                    return (a.parent == b.parent ? 1 : 2) / a.depth;
                });

            var diagonal = d3.svg.diagonal.radial()
                .projection(function (d) {
                    return [d.y, d.x / 180 * Math.PI];
                });

            var vis = chart.svg
                .append("g")
                .attr("transform", "translate(" + diameter / 2 + "," + diameter / 2 + ")");

            var cnodes = tree.nodes(hierarchy.root);
            var clinks = tree.links(cnodes);

            var link = vis.selectAll(".link")
                .data(clinks)
                .enter().append("path")
                .attr("class", "link")
                .style("stroke", "#CCC")
                .style("stroke-width", "1.5px")
                .style("fill", "none")
                .attr("d", diagonal);

            var node = vis.selectAll(".node")
                .data(cnodes)
                .enter().append("g")
                .attr("class", "node")
                .attr("transform", function(d) { return "rotate(" + (d.x - 90) + ")translate(" + d.y + ")"; });

            node.append("circle")
                .attr("r", 4.5)
                .style("fill", ifParent(_.flowRight(accessors.color, getData), 'black'));

            node.append("text")
                .attr("dy", ".31em")
                .attr("text-anchor", function(d) { return d.x < 180 ? "start" : "end"; })
                .attr("transform", function(d) { return d.x < 180 ? "translate(8)" : "rotate(180)translate(-8)"; })
                .text(hierarchy.textSource);

            // Tool-tips, etc
            chart.decorateDataElements(node, { dataCallback: getData });

        };
    };

})(spChartTypes || (spChartTypes = {}));

