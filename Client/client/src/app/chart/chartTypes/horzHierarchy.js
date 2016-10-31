// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.horzHierarchy = function () {

        this.axisConfig = 'none';

        this.render = function (context, series) {

            var chart = context.chart;
            var accessors = series.accessors;
            var data = series.data;
            var width = chart.plotArea.width;
            var height = chart.plotArea.height;

            var hierarchy = chart.buildHierarchy(data, accessors.primarySource, accessors.associateSource, { singleRoot: true, strictTree: true });

            var getData = function (d) { return d.data; };
            var ifParent = function (fn, defaultVal) {
                return function(d) {
                    if (d.data === null || d.isParent)
                        return defaultVal;
                    return fn(d);
                };
            };

            // Dendrogram

            var cluster = d3.layout.cluster()
                .size([height, width - 160]);

            var diagonal = d3.svg.diagonal()
                .projection(function (d) { return [d.y, d.x]; });

            var vis = chart.svg
                .append("g")
                .attr("transform", "translate(40,0)");

            var cnodes = cluster.nodes(hierarchy.root);
            var clinks = cluster.links(cnodes);

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
                .attr("transform", function (d) {
                    return "translate(" + d.y + "," + d.x + ")";
                });

            node.append("circle")
                .attr("r", 4.5)
                .style("fill", ifParent(_.flowRight(accessors.color, getData),'black'));

            node.append("text")
                .attr("dx", function (d) { return d.children.length ? -8 : 8; })
                .attr("dy", 3)
                .style("text-anchor", function (d) { return d.children.length ? "end" : "start"; })
                .text(hierarchy.textSource);

            // Tool-tips, etc
            chart.decorateDataElements(node, { dataCallback: getData });

        };
    };

})(spChartTypes || (spChartTypes = {}));

