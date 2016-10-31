// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.columnChart = function() {

        this.axisConfig = 'primary-value';

        this.getDomain = spCharts.getStackDomain;

        this.render = function (context, series) {

            var chart = context.chart;
            var accessors = chart.accessors;
            var y = series.vertAxis;
            
            // Get stack info
            var sbs = spCharts.getSideBySideAccessors(context, series);
            
            var bandWidth = sbs.bandWidth;
            var stack = spCharts.createStackedData(accessors, series, sbs.pos);

            var fill = accessors.endValueSource ? accessors.colorDiff : accessors.color;
            var getRow = stack.getRow;
            var xpos = _.flowRight(sbs.pos, getRow);

            // Add groups
            var gGroups = spCharts.containers(series.elem, stack.groups);

            // Add group bars
            var g = spCharts.containers(gGroups, stack.getGroupData);
            g.append("rect")
                .attr("x", xpos)
                .attr("width", bandWidth)
                .style("fill", _.flowRight(fill, getRow))
                .attr("y", y.baseline())
                .attr("height", 0)
                .transition().duration(spCharts.animateTime)
                .attr("y", _.flowRight(Math.floor, stack.valuePos))
                .attr("height", spCharts.sizeWithGaps(stack.valuePos, stack.valueSize));

            var decorOpts = { dataCallback: getRow, extraText: stack.extraText };
            chart.decorateDataElements(g, decorOpts);
            
            // Add text labels
            if (series.textSource) {
                var labelOutside = !stack.isStacked && spCharts.labelOutside(series, 'Outside');
                var labelOpts = {
                    groups: stack.groups,
                    data: stack.getGroupData,
                    dataCallback: getRow,
                    delay: 1300,
                    decorOpts: decorOpts,
                    xpos: function (d, i) {
                        return xpos(d, i) + bandWidth() / 2;
                    }
                };
                if (labelOutside) {
                    labelOpts.dy = '-1em';
                } else {
                    labelOpts.ypos = function (d) {
                        return stack.valuePos(d) + stack.valueSize(d) / 2;
                    };
                    labelOpts.box = true;
                }

                chart.drawLabels(series.elem, labelOpts);
            }
        };

    };

})(spChartTypes || (spChartTypes = {}));

