// Copyright 2011-2016 Global Software Innovation Pty Ltd

var spCharts;

(function (spCharts) {

    // Calculates and draws the chart axes
    spCharts.drawCanvas = function (context) {

        var chart = context.chart;

        // Add chart
        var svg = chart.createPlotAreaSvg(context.elem);
        chart.svg = svg;
        chart.createLegend();
        chart.calcPlotArea();

        var horzAxes = buildAxes('x', chart);
        var vertAxes = buildAxes('y', chart);

        // Measure axes
        chart.calcPlotArea();
        var width = chart.plotArea.width;
        var height = chart.plotArea.height;
        positionAxes(horzAxes, [0, width]);
        positionAxes(vertAxes, [height, 0]);

        measureAndScrunch(chart, 'width', 'left', vertAxes);
        measureAndScrunch(chart, 'height', 'bottom', horzAxes);

        svg.attr("transform", "translate(" + chart.margin.left + "," + chart.margin.top + ")");

        width = chart.plotArea.width;
        height = chart.plotArea.height;

        positionAxes(horzAxes, [0, width]);
        positionAxes(vertAxes, [height, 0]);

        // Draw axes
        _.forEach(horzAxes, function (axis) {
            drawAxis(svg, chart, axis);
        });
        _.forEach(vertAxes, function (axis) {
            drawAxis(svg, chart, axis);
        });
    };

    function measureAndScrunch(chart, sizeProp, paddingProp, axes) {
        var maxAxisGutter = 200;
        var minPlotArea = 80;

        if (chart.isMobile && sizeProp === 'width') {
            chart.convertLegendToPopup();
            chart.calcPlotArea();
        }

        if (!axes[0])
            return;

        var gap = measureAxis(chart.svg, chart, axes[0]);
        chart.margin[paddingProp] = Math.min(maxAxisGutter, Math.max(chart.margin[paddingProp], gap));
        chart.calcPlotArea();
        
        if (chart.plotArea[sizeProp] < minPlotArea && sizeProp === 'width') {
            chart.convertLegendToPopup();
            chart.calcPlotArea();
        }

        if (chart.plotArea[sizeProp] < minPlotArea) {
            axes[0].hideLabels = true;
            chart.margin[paddingProp] = 20;
            chart.calcPlotArea();
        }

        if (chart.plotArea[sizeProp] < minPlotArea) {
            axes[0].hideAxisName = true;
            chart.margin[paddingProp] = 5;
            chart.calcPlotArea();
        }
    }

    function buildAxes(orientation, chart) {
        var axisEntities = [];
        var axes = [];

        // Find all axis entities
        _.forEach(chart.series, function (s) {
            var axisEntity = getAxisEntity(s, orientation);
            if (!axisEntity || _.includes(axisEntities, axisEntity))
                return; //continue
            axisEntities.push(axisEntity);
        });

        // Process all axis entities
        _.forEach(axisEntities, function (axisEntity) {
            // find all series that apply to axis
            var applicableSeries = _.filter(chart.series, function (s2) {
                var axisEntity2 = getAxisEntity(s2, orientation);
                return axisEntity2 === axisEntity;
            });
            // find all sources that apply to axis
            var seriesToSources = function (s2) {
                return getAxisSources(chart, s2, orientation);
            };
            var representativeSeries = applicableSeries[0];
            var representativeSource = seriesToSources(representativeSeries)[0];
            if (!representativeSource)
                return; //continue

            var orient = orientation === 'x' ? 'bottom' : 'left';
            var axis = spCharts.prepareAxes(chart, axisEntity, applicableSeries, seriesToSources, representativeSource, orient);
            axis.orientation = orientation;

            _.forEach(applicableSeries, function (s2) {
                s2[orientation === 'x' ? 'horzAxis':'vertAxis'] = axis;
                var primaryOrValue = whichAxis(s2, orientation);
                if (primaryOrValue) {
                    s2[primaryOrValue] = axis;
                }
            });
            axes.push(axis);
        });

        return axes;
    }

    // Based on chart type, and physical axis, which logical axis is it?
    function whichAxis(series, orientation) {
        var axisConfig = series.chartType.axisConfig;
        if (axisConfig === 'primary-value' && orientation === 'x' || axisConfig === 'value-primary' && orientation === 'y')
            return 'primaryAxis';
        if (axisConfig === 'primary-value' && orientation === 'y' || axisConfig === 'value-primary' && orientation === 'x')
            return 'valueAxis';
        return null;
    }

    // Get the axis entity
    function getAxisEntity(series, orientation) {
        var res = null;
        var which = whichAxis(series, orientation);
        if (which === 'primaryAxis')
            res = series.entity.getPrimaryAxis();
        else if (which === 'valueAxis')
            res = series.entity.getValueAxis();
        return res;
    }

    // Get the source(s) for the axis entity
    function getAxisSources(chart, series, orientation) {
        var res = null;
        var which = whichAxis(series, orientation);
        if (which === 'primaryAxis') {
            res = [series.primarySource];
        }
        else if (which === 'valueAxis') {
            res = [series.valueSource];
            if (series.endValueSource)
                res.push(series.endValueSource);
        }
        return res;
    }

    function positionAxes(axes, range) {
        var totalSize = range[1] - range[0];
        var sign = totalSize > 0 ? 1 : -1;
        var axisGutter = 10 * sign; // gap between axes

        var axisCount = axes.length;
        var totalGutter = axisGutter * (axisCount - 1);
        var available = totalSize - totalGutter;

        _.forEach(axes, function (axis, i) {
            var p = range[0];
            var myRange = [
                p + available * i / axisCount + i * axisGutter,
                p + available * (i+1) / axisCount + i * axisGutter];
            axis.autoRange(myRange, false);
        });
    }

    function measureAxis(svg, chart, axis) {
        var g = drawAxis(svg, chart, axis, true);
        var box = g[0][0].getBBox();
        g.remove();

        var res = axis.orientation === 'x' ? box.height : box.width;
        if (axis.axisEntity.name) {
            res += axis.orientation === 'x' ? 16 : 24;
        }
        return res;
    }

    function drawAxis(svg, chart, axis, bMeasure) {
        var gClass = axis.orientation + " axis";
        var range = axis.range;
        if (axis.hideLabels)
            gClass += " hide-labels";
   
        // Limit number of ticks
        if (spCharts.useBands(axis.method) && !axis.d3axis.tickValues()) {
            axis.d3axis.tickSize(0, 0);
            var minimumPixelsPerTick = 12;
            var newTicks = spCharts.makeTicksFit(axis.domain, range, minimumPixelsPerTick);
            if (newTicks !== axis.domain)
                axis.d3axis.tickValues(newTicks);
        }

        var g = svg.append("g")
            .attr("class", gClass)
            .call(axis.d3axis);

        if (axis.orientation === 'x') {
            g.attr("transform", "translate(0," + chart.plotArea.height + ")")
                .selectAll("text")
                .attr("x", "0")
                .attr("y", "0")
                .style("text-anchor", "end")
                .attr("dx", "-.8em")
                .attr("transform", function(d) {
                    return "rotate(-45)";
                });
        } else {
            g.selectAll("text")
                .attr("x", "0")
                .attr("y", "0")
                .attr("dx", "-3px");
        }

        // bail out here if we're just measuring
        if (bMeasure)
            return g;

        // axis baseline
        // (manually redrawn as it may extend beyond the d3 range due to padding)
        var isX = axis.orientation === 'x';
        g.append("line")
            .attr({
                "x1": isX ? range[0] : 0,
                "y1": isX ? 0 : range[0],
                "x2": isX ? range[1] : 0,
                "y2": isX ? 0 : range[1]
            });

        // grid lines
        if (axis.axisEntity.showGridLines && axis.ticks) {
            var fnPos = function (d) {
                return axis.scale(d);
            };
            if (axis.orientation === 'x') {
                svg.selectAll("line.vgrid").data(axis.ticks(4)).enter()
                    .append("line")
                    .attr(
                        {
                            "class": "vgrid grid",
                            "y1": 0,
                            "y2": chart.plotArea.height,
                            "x1": fnPos,
                            "x2": fnPos
                        });
            }
            else if (axis.orientation === 'y') {
                svg.selectAll("line.hgrid").data(axis.ticks(4)).enter()
                    .append("line")
                    .attr(
                        {
                            "class": "hgrid grid",
                            "x1": 0,
                            "x2": chart.plotArea.width,
                            "y1": fnPos,
                            "y2": fnPos
                        });
            }
        }

        // Draw axis label
        if (axis.axisEntity.name && !axis.hideAxisName) {
            var gLabel = svg.append("g")
            .attr("class", axis.orientation + " axis-label")
                .append("text")
                .text(axis.axisEntity.name);
            if (axis.orientation === 'x') {
                gLabel.attr("x", chart.plotArea.width / 2)
                    .attr("y", chart.plotArea.height + chart.margin.bottom - 3);
            } else {
                gLabel.attr("transform", function (d) {
                        return "translate(" + (12-chart.margin.left) + " " + (chart.plotArea.height / 2) + ") rotate(-90)";
                    });
            }

        }

        return g;
    }

    spCharts.getValueScale = function getValueScale(chart, series, suggestedAutoRange) {
        var scale = spCharts.prepareAxes(chart, series.entity.getValueAxis(), [series], _.constant([series.valueSource]), series.valueSource, 'x', suggestedAutoRange);
        return scale;
    };

    spCharts.prepareAxes = function prepareAxes(chart, axisEntity, seriess, seriesToSources, representativeSource, orient, suggestedAutoRange) {
        // Determine type of scale
        var scaleType = spCharts.getScaleType(chart.services, axisEntity, representativeSource);
        var domain = spCharts.getAxisDomain(chart, seriess, seriesToSources, scaleType);
        var scale = chart.getScale(axisEntity, representativeSource, suggestedAutoRange, { scaleType: scaleType, domain: domain });
        var axis = d3.svg.axis().scale(scale.d3scale).orient(orient);

        if (scale.format)
            axis.tickFormat(scale.format);

        // Force integer-only ticks for integer charts
        if (representativeSource.colType === 'Int32') {
            var tmpDomain = scale.domain;
            var integers = 1 + Math.abs(tmpDomain[1] - tmpDomain[0]);
            var defaultTicks = axis.ticks()[0]; //d3 default sugggested ticks
            if (integers <= defaultTicks) {
                axis.ticks([integers]);
            }
        }

        scale.axisEntity = axisEntity;
        scale.d3axis = axis;

        return scale;
    };

    // Returns 'categoryScaleType', 'linearScaleType' or 'logScaleType'
    spCharts.getScaleType = function getScaleType(services, axisEntity, representativeSource) {
        var source = representativeSource;
        // Determine type of scale
        var supported = services.spChartService.supportedScalesForDataType(source.colType);
        var scaleType = supported.defaultScaleType;

        // Check for user preferences
        if (axisEntity && axisEntity.axisScaleType) {
            var axisScaleType = axisEntity.axisScaleType.eid().getAlias();
            if (axisScaleType && axisScaleType !== 'automaticScaleType' && supported[axisScaleType]) {
                scaleType = axisScaleType;
            }
        }

        // Hack around date-time issues
        // The date-time scale type currently doesn't work for format types that weren't previously identified as being safe for category.
        // So make it revert back to the linearScaleType in this scenario to at least preserve the legacy behavior.
        if (scaleType === 'dateTimeScaleType') {
            if (!services.spVisDataService.isCategoricalScaleFormat(source.formatType)) {
                scaleType = 'linearScaleType';
            }
        }
        
        return scaleType;
    };

    // Combines the domains of all applicable sources across multiple series
    spCharts.getAxisDomain = function combineMultipleSeriesDomains(chart, seriess, seriesToSources, method) {
        var domains = _.map(seriess, _.bind(function (series) {
            // Determine how to get the domain for a given chart type (e.g. to allow for stacked domains)
            var getDomainFn = spCharts.getSourceDomain;
            if (series.chartType && series.chartType.getDomain) {
                getDomainFn = series.chartType.getDomain; // can't use sp.result because it'll eval the function
            }
            var seriesSource = seriesToSources(series)[0];
            var d = getDomainFn({ chart: chart }, series, seriesSource, method);
            return d;
        }, this));

        var domain = spCharts.combineDomains(domains, method);

        // Note: domain is mutated in getScale. It should probably be moved here.

        return domain;
    };

})(spCharts || (spCharts = {}));

