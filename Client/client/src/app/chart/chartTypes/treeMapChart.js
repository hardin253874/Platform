// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.treeMapChart = function () {
        this.axisConfig = 'none';

        this.render = function (context, series) {

            var chart = context.chart;
            var accessors = chart.accessors;
            var data = series.data;

            var width = chart.plotArea.width;
            var height = chart.plotArea.height;

            var hierarchy = chart.buildHierarchy(data, accessors.primarySource, accessors.associateSource, { singleRoot: true, strictTree: true });

            var getData = function (node) {
                return node.data;
            };

            var treemap = d3.layout.treemap()
                .round(true)
                .size([width, height])
                .children(function (n) { return n.children; })
                //.sticky(true)
                .value(_.flowRight(accessors.valueSource, getData));

            var svg = series.elem
                .append("g")
                .attr("transform", "translate(.5,.5)"); // so we get nice pixel alignment

            var nodes = treemap.nodes(hierarchy.root)
                .filter(function (d) {
                    return d.data;
                });

            var g = svg.selectAll(".tm")
                  .data(nodes)
                .enter().append("g")
                  .attr("class", "tm")
                  .attr("transform", function (d) { return "translate(" + d.x + "," + d.y + ")"; });
                  //.on("click", function (d) { return zoom(node == d.parent ? root : d.parent); });

            if (accessors.imageSource) {
                chart.appendImagePattern(g, _.flowRight(accessors.imageSource, getData));
            }

            g.append("rect")
                .attr("width", function (d) { return d.dx - 1; })
                .attr("height", function (d) {
                    if (d.dy < 1)
                        d.dy = 1;
                    return d.dy - 1;
                })
                .style("stroke", "#fff")
                .style("stroke-width", "1")
                .style("fill", _.flowRight(accessors.fill, getData));

            chart.decorateDataElements(g, { dataCallback: getData });

            if (series.textSource) {

                chart.drawLabels(series.elem, {
                    data: nodes,
                    box: true,
                    xpos: function (d) { return d.x + d.dx / 2; },
                    ypos: function (d) { return d.y + d.dy / 2; },
                    text: hierarchy.textSource,
                    visible: function (d) { return d.children.length; } // hide non-leaf nodes
                });
            }

        };
    };

})(spChartTypes || (spChartTypes = {}));

