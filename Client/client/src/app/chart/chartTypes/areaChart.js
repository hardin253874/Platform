// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.areaChart = function () {

        this.axisConfig = 'primary-value';

        this.getDomain = spCharts.getStackDomain;

        this.render = function (context, series) {

            var chart = context.chart;
            var accessors = chart.accessors;
            var x = series.horzAxis;
            var y = series.vertAxis;

            // Get stack info
            var xpos = _.flowRight(x.bandStart, accessors.primarySource);
            var stack = spCharts.createStackedData(accessors, series, xpos);
            var getRow = stack.getRow;

            var getXPos;
            if (stack.isStacked) {
                getXPos = function(d) {
                    var fn = _.flowRight(x.scale, _.property('x'));
                    var res = fn(d);
                    return res;
                };
            } else {
                getXPos = _.flowRight(x.rowScale, getRow);
            }


            // Area path factory
            var area = d3.svg.area()
                .x(getXPos)
                .y0(stack.startPos)
                .y1(stack.endPos);

            // Add groups
            var gGroups = spCharts.containers(series.elem, stack.groups);

            // Add area path
            gGroups.append("path")
                .style("fill", function(group) {
                    var col = accessors.fill(stack.getRepresentative(group));
                    return col;
                })
                .attr("d", function(g) {
                    var fn = _.flowRight(area, function(group) {
                        return _.sortBy(stack.getGroupDataUnfiltered(group), getXPos);
                    });
                    var res = fn(g);
                    return res;
                });

            chart.decorateDataGroupElements(gGroups, { representativeCallback: stack.getRepresentative });

            // Data labels
            chart.drawLabels(series.elem, {
                groups: stack.groups,
                data: stack.getGroupData,
                dy: '-1em'
            });
        };
        return this;

    };

})(spChartTypes || (spChartTypes = {}));

