// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.forceGraph = function() {

        this.axisConfig = 'none';

        this.render = function (context, series) {

            var chart = context.chart;
            var accessors = series.accessors;
            var data = series.data;
            var width = chart.plotArea.width;
            var height = chart.plotArea.height;

            var vis = chart.svg;

            var getData = function (d) { return d.data; };
            var getNode = function (d) { return d.node; };

            var hierarchy = chart.buildHierarchy(data, accessors.primarySource, accessors.associateSource, { singleRoot: false, strictTree: false });
            var nodes = hierarchy.nodes;
            var links = hierarchy.links;

            // Build label anchors
            var labelAnchors = [];
            var labelAnchorLinks = [];
            _.forEach(nodes, function(node) {
                var la1 = { node: node };
                var la2 = { node: node };
                labelAnchors.push(la1);
                labelAnchors.push(la2);
                labelAnchorLinks.push({
                    source: la1,
                    target: la2,
                    weight: 1
                });
            });

            // Build force graph
            var force = d3.layout.force()
                .size([width, height])
                .nodes(nodes)
                .links(links)
                .gravity(1)
                .linkDistance(50)
                .charge(-3000)
                .linkStrength(function(x) {
                    return x.weight * 10;
                });

            if (!spChartTypes.forceGraph.testMode)
                force.start();

            // Build second force graph for anchors
            var force2 = d3.layout.force()
                .nodes(labelAnchors)
                .links(labelAnchorLinks)
                .gravity(0)
                .linkDistance(0)
                .linkStrength(8)
                .charge(-100)
                .size([width, height]);

            if (!spChartTypes.forceGraph.testMode)
                force2.start();

            // Draw link lines
            var link = vis.selectAll("line.link")
                .data(links)
                .enter().append("svg:line")
                .attr("class", "link")
                .style("stroke", "#CCC");

            // Draw nodes
            var node = vis.selectAll("g.node")
                .data(force.nodes())
                .enter().append("g")
                .attr("class", "node");
            node.append("circle")
                .attr("r", 5)
                .style("fill", _.flowRight(accessors.color, getData))
                .style("stroke", "#FFF")
                .style("stroke-width", 3);
            node.call(force.drag);

            // Tool-tips, etc
            chart.decorateDataElements(node, { dataCallback: getData });

            // Text labels
            var anchorLink = vis
                .selectAll("line.anchorLink")
                .data(labelAnchorLinks); //.enter().append("svg:line").attr("class", "anchorLink").style("stroke", "#999");

            var anchorNode = vis.selectAll("g.anchorNode")
                .data(force2.nodes())
                .enter().append("g")
                .attr("class", "anchorNode");
            anchorNode.append("circle")
                .attr("r", 0)
                .style("fill", "#FFF");
            anchorNode.append("text")
                .text(function (d, i) {
                    return i % 2 === 0 ? '' : hierarchy.textSource(getNode(d));
                })
                .style("fill", "#555")
                .style("font-family", "Arial")
                .style("font-size", 12);

            // Callback to update link coordinates
            var updateLink = function() {
                this.attr("x1", function(d) {
                    return d.source.x;
                }).attr("y1", function(d) {
                    return d.source.y;
                }).attr("x2", function(d) {
                    return d.target.x;
                }).attr("y2", function(d) {
                    return d.target.y;
                });
            };

            // Callback to update nodes
            var updateNode = function() {
                this.attr("transform", function(d) {
                    return "translate(" + d.x + "," + d.y + ")";
                });
            };

            // Animation timer
            force.on("tick", function() {

                force2.start();
                node.call(updateNode);

                anchorNode.each(function(d, i) {
                    if (i % 2 === 0) {
                        d.x = d.node.x;
                        d.y = d.node.y;
                    } else {
                        if (this && this.childNodes && this.childNodes.length > 1) {
                            
                            var textNode = this.childNodes[1];
                            if (textNode) {

                                var labelWidth = 90;
                                var diffX = d.x - d.node.x;
                                var diffY = d.y - d.node.y;

                                var dist = Math.sqrt(diffX * diffX + diffY * diffY);

                                var shiftX = labelWidth * (diffX - dist) / (dist * 2);
                                shiftX = Math.max(-labelWidth, Math.min(0, shiftX));
                                var shiftY = 5;

                                textNode.setAttribute("transform", "translate(" + shiftX + "," + shiftY + ")");
                            }
                        }
                    }
                });

                anchorNode.call(updateNode);
                link.call(updateLink);
                anchorLink.call(updateLink);

            });

        };
    };

    spChartTypes.forceGraph.testMode = false;

})(spChartTypes || (spChartTypes = {}));

