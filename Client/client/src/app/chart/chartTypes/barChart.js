// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spCharts */

var spChartTypes;

(function (spChartTypes) {

    spChartTypes.barChart = function() {

        this.axisConfig = 'value-primary';

        this.getDomain = spCharts.getStackDomain;

        this.render = function(context, series) {

            var chart = context.chart;
            var accessors = chart.accessors;
            var x = series.horzAxis;

            // Get stack info
            var sbs = spCharts.getSideBySideAccessors(context, series);
            var bandWidth = sbs.bandWidth;
            var stack = spCharts.createStackedData(accessors, series, sbs.pos);

            var fill = accessors.endValueSource ? accessors.colorDiff : accessors.color;
            var getRow = stack.getRow;
            var ypos = _.flowRight(sbs.pos, getRow);

            // Add groups
            var gGroups = spCharts.containers(series.elem, stack.groups);

            // Add group bars
            var g = spCharts.containers(gGroups, stack.getGroupData);
            g.append("rect")
                .attr("y", ypos)
                .attr("height", bandWidth)
                .style("fill", _.flowRight(fill, getRow))
                .attr("x", x.baseline())
                .attr("width", 0)
                .transition().duration(spCharts.animateTime)
                .attr("x", _.flowRight(Math.floor, stack.valuePos))
                .attr("width", spCharts.sizeWithGaps(stack.valuePos, stack.valueSize));

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
                    ypos: function (d, i) {
                        return ypos(d, i) + bandWidth() / 2;
                    }
                };
                if (labelOutside) {
                    labelOpts.dx = '0.7em';
                    labelOpts.textAnchor = 'start';
                    //var isNeg = function (d) {
                    //    return accessors.valueSource(getRow(d)) < 0;
                    //};
                    //labelOpts.dx = function(d) {
                    //    return isNeg(d) ? '-0.7em' : '0.7em';
                    //};
                    //labelOpts.textAnchor = function(d) {
                    //    return isNeg(d) ? 'end' : 'start';
                    //};
                } else {
                    labelOpts.xpos = function (d) {
                        return stack.valuePos(d) + stack.valueSize(d) / 2;
                    };
                    labelOpts.box = true;
                }

                chart.drawLabels(series.elem, labelOpts);
            }
        };

    };

})(spChartTypes || (spChartTypes = {}));

