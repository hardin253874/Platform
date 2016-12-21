// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.vectorFieldChart = function () {

        this.axisConfig = 'primary-value';

        this.render = function (context, series) {

            var chart = context.chart;
            var accessors = chart.accessors;
            var x = series.horzAxis;
            var y = series.vertAxis;

            // Fallback to primary if not specified
            var endPrimarySource = accessors.endPrimarySource || accessors.primarySource;
            var endValueSource = accessors.endValueSource || accessors.valueSource;

            var x1 = _.flowRight(x.scale, accessors.primarySource);
            var y1 = _.flowRight(y.scale, accessors.valueSource);
            var x2 = _.flowRight(x.scale, endPrimarySource);
            var y2 = _.flowRight(y.scale, endValueSource);

            // Ensure data present for both endpoints
            var predicate = function (d) {
                var px1 = accessors.primarySource(d);
                var py1 = accessors.valueSource(d);
                var px2 = endPrimarySource(d);
                var py2 = endValueSource(d);
                if (!(px1 && py1 && px2 && py2))
                    return false;
                var dx = px2 - px1;
                var dy = py2 - py1;
                var l = Math.sqrt(dx * dx + dy * dy);
                return l > 2; // threshold length of vector
            }

            var filteredData = series.data.filter(predicate);

            var arrowLen = 10;
            var getRotate = function (angle) {
                var sin = Math.sin(angle * Math.PI / 180.0);
                var cos = Math.cos(angle * Math.PI / 180.0);
                return function (d) {
                    var px1 = x1(d);
                    var py1 = y1(d);
                    var px2 = x2(d);
                    var py2 = y2(d);

                    // Normalize to vector around origin of required length
                    var dx = px2 - px1;
                    var dy = py2 - py1;
                    var l = Math.sqrt(dx * dx + dy * dy);
                    dx = dx * arrowLen / l;
                    dy = dy * arrowLen / l;
                    // Rotate and offset
                    var resx = dx * cos + dy * sin + px1;
                    var resy = dy * cos - dx * sin + py1;
                    return resx + ' ' + resy;
                };
            };
            var getArrow = function(d) {
                var rot1 = getRotate(-25);
                var rot2 = getRotate(25);
                var px1 = x1(d);
                var py1 = y1(d);
                return px1 + ' ' + py1 + ',' + rot1(d) + ',' + rot2(d);
            }

            // Arrowhead is around x1
            var g = spCharts.containers(series.elem, filteredData);
            g.append("line")
                .style("stroke", accessors.color)
                .style("stroke-width", "2")
                .attr("x1", x1)
                .attr("y1", y1)
                .attr("x2", x2)
                .attr("y2", y2);
            g.append("polygon")
                .style("fill", accessors.color)
                .style("stroke", accessors.color)
                .style("stroke-width", "2")
                .attr("points", getArrow);

            chart.decorateDataElements(g);

            // Data labels
            chart.drawLabels(series.elem, {
                data: filteredData,
                xpos: function (d) { return (x1(d) + x2(d)) / 2; },
                ypos: function (d) { return (y1(d) + y2(d)) / 2; },
                dy: '-1em'
            });
        };
    };

})(spChartTypes || (spChartTypes = {}));

