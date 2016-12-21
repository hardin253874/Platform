// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, d3, sp, spEntity, jsonLookup, spChartTypes */

var spChartService = {};
var spCharts;

(function (spCharts) {
    'use strict';

    spChartService.sourceTypes = ['primarySource', 'valueSource', 'endPrimarySource', 'endValueSource', 'sizeSource', 'colorSource', 'imageSource', 'textSource', 'symbolSource', 'associateSource'];

    spCharts.defaultColor = _.constant('#1f77b4');
    spCharts.defaultNegativeColor = _.constant('#ff0000');
    spCharts.defaultLabelColor = _.constant('#666666');

    spCharts.nextTagRoot = 0;
    spCharts.animateTime = 1000;

    var Chart = (function () {

        /**
         * spCharts.Chart constructor.
         * @class
         * @name spCharts.Chart
         */
        function Chart() {
            this.accessors = {};

            this.accessors = {};
            this.tagId = 0; // unique ids within the chart
            this.tagRoot = spCharts.nextTagRoot++; // unique id between charts on screen            
        }        

        // Draw entire chart
        Chart.prototype.drawChart = function drawChart(rootElem) {

            // Prepare data
            this.prepareSeries();
            this.prepareDataGroups();
            this.createChartAccessors();

            // Margins
            this.prepareMargins();

            // Prep color data
            this.prepareColorData();

            // Draw axes
            spCharts.drawCanvas({
                elem: rootElem,
                chart: this
            });

            // Draw series
            var context = {
                elem: rootElem,
                chart: this
            };

            // Containers
            var gSeriesRoot = this.svg.append("g").attr("class", "series-root");
            this.labelRoot = this.svg.append("g").attr("class", "label-root");

            // Basic clipping
            var clipPathId = this.createId("plotAreaClip");
            this.svg.append("clipPath")
                .attr("id", clipPathId)
                .append("rect")
                .attr("x", 1)
                .attr("y", 0)
                .attr("width", this.plotArea.width)
                .attr("height", Math.max(0, this.plotArea.height-1));

            // Render all series
            _.forEach(this.series, _.bind(function (s) {
                s.elem = gSeriesRoot.append("g")
                .attr("name", spCharts.getSeriesName(s))
                .attr("class", "series")
                .attr("clip-path", "url(#" + clipPathId + ")");
                s.chartType.render(context, s);
            }, this));

            // Draw title
            spCharts.drawTitle(this.svgRoot, this.chartEntity, { left: this.margin.left, right: this.width - this.margin.right });
        };

        // Sets services
        Chart.prototype.setServices = function setServices(services) {
            _.assign(this, services);
            this.services = services;
        };

        // Set the chart size
        Chart.prototype.setSize = function setSize(size) {
            this.width = size.width;
            this.height = size.height;
        };

        // Set the chart entity
        Chart.prototype.setChartEntity = function setChartEntity(chartEntity) {
            this.chartEntity = chartEntity;
        };


        // Set the data being charted
        Chart.prototype.setChartData = function setChartData(chartData) {
            var reportData = chartData.reportData;
            this.chartData = chartData;
            this.reportMetadata = reportData.meta;
            this.rowData = chartData.rowData;
            if (!this.rowData || this.rowData.length === 0) {
                console.warn('No data to chart');
            }
        };


        // Prepare margins
        Chart.prototype.prepareMargins = function prepareMargins() {
            this.margin = { top: 5, right: 10, bottom: 5, left: 5 };

            // Axis gutter
            if (_.some(this.series, function (s) { return s.chartType.axisConfig !== 'none'; })) {
                this.margin.left = 50;
                this.margin.bottom = 50;
            }

            // Title gutter
            if (this.chartEntity.chartTitle) {
                this.margin.top += 17;
            }

            // Data label overflow buffer
            if (_.some(this.series, function (s) { return s.chartType.labelAbove && s.textSource; })) {
                this.margin.top += 17;
                this.margin.right += 25;
            }
            if (_.some(this.series, function (s) { return s.chartType.labelRight && s.textSource; })) {
                this.margin.right += 50;
            }

            // Note: legend may also adjust margin

            this.calcPlotArea();
        };


        // Recalculate the plot area from the margins
        Chart.prototype.calcPlotArea = function calcPlotArea() {
            // The area bounded by the axes.
            this.plotArea = {
                width: Math.max(0, this.width - this.margin.left - this.margin.right),
                height: Math.max(0, this.height - this.margin.top - this.margin.bottom)
            };
        };


        // Process series entities
        Chart.prototype.prepareSeries = function prepareSeries() {
            var that = this;
            var seriesEntities = this.spChartService.getSeriesOrdered(this.chartEntity);

            this.series = _.map(seriesEntities, function(seriesEntity) {
                var series = that.createSeries(seriesEntity);
                if (!series)
                    return null; //continue

                // Create general accessors for each series-specific accessors
                _.forEach(_.keys(series.sources), function(key) {
                    if (!that.accessors[key]) {
                        var newAccessor = function (datum) {
                            if (!datum || !datum.group || !datum.group.series)
                                return null;
                            var s = datum.group.series;
                            var fn = s[key];
                            if (!fn) return null;
                            var res = fn(datum); 
                            return res;
                        };
                        _.assign(newAccessor, series[key]);
                        that.accessors[key] = newAccessor;
                    }
                });
                return series;
            });

            // remove nulls
            this.series = _.filter(this.series, d3.identity);
        };


        // Create an individual series object from a series entity
        Chart.prototype.createSeries = function createSeries(seriesEntity) {
            var series = {
                entity: seriesEntity,
                sources: {}
            };

            var ct = seriesEntity.getChartType();
            if (!ct)
                return null;
            var alias = ct.eid().getAlias();
            series.chartType = new spChartTypes[alias]();
            var chartInfo = _.find(this.spChartService.chartTypes, { alias: alias });
            if (!chartInfo)
                return null;
            _.assign(series.chartType, chartInfo);

            var invalid = false;

            // Create accessor for each source
            _.forEach(spChartService.sourceTypes, _.bind(function (alias) {
                var chartSource = seriesEntity.getLookup(alias);
                var isEmpty = false;

                // Handle absense of input
                if (!chartSource) {
                    if (this.spChartService.isSourceRequired(series.chartType.alias, alias)) {
                        invalid = true;
                        console.warn('Missing required source: ' + alias);
                    } else if (alias === 'primarySource' || alias === 'valueSource') {
                        chartSource = spEntity.fromJSON({ specialChartSource: jsonLookup('emptySource') });
                        isEmpty = true;
                    }
                }

                // Create accessor
                if (chartSource) {
                    var source = this.createChartDataAccessor(chartSource);
                    if (source) {
                        source.alias = alias;
                        series[alias] = series.sources[alias] = source;
                        source.isEmpty = isEmpty;
                    }
                }
            }, this));

            if (invalid) {
                return null;
            }

            series.accessors = _.clone(series.sources);
            this.createColorAccessors(series);

            return series;
        };


        /** prepareColorData
         * @name spChart.prepareColorData
         */
        Chart.prototype.prepareColorData = function prepareColorData(series) {
            _.forEach(this.series, function (series) {
                if (series.accessors.color && series.accessors.color.prepColorData) {
                    series.accessors.color.prepColorData();
                }
            });
        };


        /** getColorAccessors
         * @name spChart.getColorAccessors
         */
        Chart.prototype.createColorAccessors = function createColorAccessors(series) {
            var that = this;
            this.variableColors = d3.scale.category20();

            var accessors = series.accessors;

            // Create color accessor
            if (series.sources.colorSource) {
                if (series.entity.useConditionalFormattingColor && series.colorSource.cfColorAccessor) {
                    // Conditional formatting
                    accessors.color = function (d) {
                        var color = series.colorSource.cfColorAccessor(d);
                        return color.colorHex;
                    };
                } else {
                    // Color series
                    accessors.color = function (d) {
                        var value = series.colorSource(d);
                        var color = that.variableColors(value);
                        return color;
                    };
                    accessors.color.prepColorData = function () {
                        var domain = spCharts.getSourceDomain({ chart: this }, series, series.colorSource, 'categoryScaleType');
                        domain = _.sortBy(domain, series.colorSource.sorter);
                        if (spEntity.DataType.isResource(series.colorSource.colType)) {
                            var showAllValues = true;
                            domain = spCharts.getResourceDomain(domain, series.colorSource, that.chartData, showAllValues, that.nameCache);
                        }
                        _.forEach(domain, function (value) { that.variableColors(value); });
                    };
                }
                accessors.colorDiff = accessors.color;
            } else {
                // Flat colors
                var chartTypeAlias = sp.result(series.entity, 'chartType.nsAlias');
                var color = ( series.entity.getChartCustomColor && series.entity.getChartCustomColor() ) || spCharts.defaultColor(chartTypeAlias);
                var disallowNeg = this.spChartService.getChartType(chartTypeAlias).disallowNegColor;
                var negColor = disallowNeg ? color : ( ( series.entity.getChartNegativeColor && series.entity.getChartNegativeColor() ) || spCharts.defaultNegativeColor(chartTypeAlias));
                series.color = color;
                series.negColor = negColor;
                accessors.color = function (d) {
                    if (!accessors.valueSource)
                        return color;
                    var val = accessors.valueSource(d);
                    var res = val < 0 ? negColor : color;
                    return res;
                };
                accessors.colorDiff = function (d) {
                    if (!(accessors.valueSource && accessors.endValueSource))
                        return color;
                    var val = accessors.valueSource(d);
                    var endVal = accessors.endValueSource(d);
                    var res = val < endVal ? negColor : color;
                    return res;
                };
            }

            // Create fill accessor
            if (series.sources.imageSource) {
                accessors.fill = function (d) {
                    var id = series.imageSource(d);
                    var res = "url(#img" + id + ") " + accessors.color(d);
                    return res;
                };
            } else {
                accessors.fill = accessors.color;
            }
        };

        
        // Ensure there is a global accessor for any/every accessor that appears on a series
        // that resolves to the current accessor on the series before running.
        Chart.prototype.createChartAccessors = function createChartAccessors() {
            var accessors = this.accessors;
            _.forEach(this.series, function (s) {
                _.forEach(_.keys(s.accessors), function (key) {
                    var accessor = accessors[key];
                    if (!accessor) {
                        accessor = function (d) {
                            if (!d)
                                return null;

                            var series = d.group.series;
                            var res = series.accessors[key](d);
                            return res;
                        };
                        accessors[key] = accessor;
                    }
                    s.accessors[key].generalAccessor = accessor;
                });
            });
            accessors.series = function(d) {
                return d.group.series;
            };
            accessors.x = function (d) {
                return d.group.series.horzAxis;
            };
            accessors.y = function (d) {
                return d.group.series.vertAxis;
            };
        };


        // Partition/pivot the data based on groups, series, and other factors
        Chart.prototype.prepareDataGroups = function prepareDataGroups() {
            var that = this;
            var combined = this.combined = [];
            var getPivot = this.createPivotKeyAccessor();

            // One group for each pivot (possibly multiple groups belonging to the series)

            var index = 1;
            _.forEach(this.series, function (series) {
                series.data = [];
                var groupMap = {};
                var groups = series.groups = [];

                _.forEach(that.rowData, function (row) {

                    var data = {
                        row: row,
                        group: null,
                        index: row.index = row.index || (index++)
                    };

                    var groupKey = getPivot(data, series);
                    var group = groupMap[groupKey];
                    if (!group) {
                        group = {
                            data: [],
                            series: series,
                            representative: data    // a representative data row
                        };
                        groupMap[groupKey] = group;
                        groups.push(group);
                    }
                    data.group = group;

                    combined.push(data);
                    group.data.push(data);
                    series.data.push(data);
                });
            });

        };


        // Create a function that will determine the group key for a data object
        Chart.prototype.createPivotKeyAccessor = function createPivotKeyAccessor() {
            var accessor = function (datum, series) {
                var ct = series.chartType.alias;

                // For these chart types, pivot on color
                if (ct !== 'lineChart')
                    return '';
                if (!series.colorSource)
                    return '';
                var res = series.colorSource(datum);
                return res;
            };
            return accessor;
        };


        // Create a source object. That is a function that extracts the value of interest from a data object, for the given source
        // along with other properties relevant for the source.
        Chart.prototype.createChartDataAccessor = function createChartDataAccessor(chartSourceEntity) {

            this.nameCache = this.nameCache || {};
            var options = {
                visData: this.chartData,
                nameCache: this.nameCache
            };

            // Create an accessor over the report data
            var rowAccessor = this.spVisDataService.createDataAccessor(chartSourceEntity, options);

            // Wrap so it can extract the report row from chart group data
            var accessor = wrapAccessorForCharts(rowAccessor);

            // And bring all the other goodies along with it
            _.assign(accessor, rowAccessor);
            accessor.rowAccessor = rowAccessor;
            accessor.getText = wrapAccessorForCharts(rowAccessor.getText);
            accessor.cfColorAccessor = wrapAccessorForCharts(rowAccessor.cfColorAccessor);

            return accessor;
        };

        // Accapts a function that takes (reportRow, index) and
        // and returns a function that accepts a charts/d3 data value.
        function wrapAccessorForCharts(accessorFn) {
            if (!accessorFn)
                return accessorFn;
            return function (d) {
                if (!d)
                    return null;

                var value = accessorFn(d.row, d.index);
                return value;
            };
        }

        // Create the root SVG element
        Chart.prototype.createSvg = function createSvg(hostElem) {
            var svg = d3.select(hostElem)
                .append("svg")
                .attr("class", "sp-chart-svg")
                .attr("width", this.width + 'px')
                .attr("height", this.height + 'px');
            this.svgRoot = svg;
            this.svgRoot.on("click", _.bind(this.onBackgroundClick, this));
            this.svg = svg; //hmm
            return svg;
        };


        // Create the root SVG element for an x-y axis plot
        Chart.prototype.createPlotAreaSvg = function createPlotAreaSvg(hostElem) {
            var svgPlot = this.createSvg(hostElem)
                .append("g")
                    .attr("transform", "translate(" + this.margin.left + "," + this.margin.top + ")");
            return svgPlot;
        };


        // Create the legend
        Chart.prototype.createLegend = function createLegend(svg) {
            if (!this.accessors.colorSource)
                if (!_.some(this.series, function (s) { return s.color; }))
                    return;

            var legendData = [];
            var found = false;
            var chart = this;

            // Copy series colours into legend
            if (this.series.length > 1) {
                _.forEach(this.series, _.bind(function (s) {
                    if (s.color) {
                        legendData.push({
                            color: s.color,
                            text: spCharts.getSeriesName(s)
                        });
                        found = true;
                    }
                }, this));
            }

            // Populate the colors
            _.forEach(this.series, function (s) {
                if (s.entity.hideColorLegend)
                    return;
                if (s.entity.useConditionalFormattingColor) {
                    if (!s.colorSource || !s.colorSource.rules || s.colorSource.rules.legendDone)
                        return;
                    _.forEach(s.colorSource.rules, function (rule) {
                        var entry = chart.spChartService.ruleToLegendEntry(rule);
                        legendData.push(entry);
                    });
                    s.colorSource.rules.legendDone = true;
                } else {
                    _.forEach(s.data, s.accessors.color);
                }
                found = true;
            });
            if (!found)
                return;

            // Copy variable colours into legend
            _.forEach(this.variableColors.domain(), _.bind(function (val) {
                legendData.push({
                    color: this.variableColors(val),
                    text: this.accessors.colorSource.formatter(val)
                });
            }, this));

            // Legend
            var legend = this.svgRoot.append("g")
                .attr("class", "legend");

            var item = legend.selectAll(".item")
                .data(legendData)
              .enter().append("g")
                .attr("class", "item")
                .attr("transform", function (ser, i) { return "translate(0," + i * 20 + ")"; });

            item.append("rect")
                .attr("class", "swatch")
                .attr("x", this.width - 18)
                .attr("width", 18)
                .attr("height", 18)
                .style("fill", function(ld) { return ld.color; });

            item.append("text")
                .attr("x", this.width - 24)
                .attr("y", 9)
                .attr("dy", ".35em")
                .text(function (ld) { return ld.text; });

            this.legendBox = legend[0][0].getBBox();
            this.legend = { g: legend };
            this.margin.noLegend = this.margin.right;
            this.margin.right += this.legendBox.width;
        };

        // Converts a legend to a popup
        Chart.prototype.convertLegendToPopup = function convertLegendToPopup(svg) {
            var chart = this;
            var pad = 4;
            if (this.legend && !this.legend.isPopup && this.legendBox && this.legendBox.width && this.legendBox.height) {
                this.legend.isPopup = true;
                this.margin.right = this.margin.noLegend;

                var legendVisible = false;
                this.legend.g
                    .style("visibility", "hidden")
                    .attr("transform", "translate(" + (0-pad) + "," + (46+pad) + ")");

                // Background
                this.legend.g.insert("rect", ":first-child")
                    .attr("x", this.legendBox.x - pad)
                    .attr("y", this.legendBox.y - pad)
                    .attr("width", this.legendBox.width + pad * 2)
                    .attr("height", this.legendBox.height + pad * 2)
                    .attr("class", "popupBorder");

                // Toggle function
                var toggleLegend = function () {
                    legendVisible = !legendVisible;
                    chart.legend.g.style("visibility", legendVisible ? "visible" : "hidden");
                    var e = d3.event;

                    if (e) {
                        e.cancelBubble = true;

                        if (e.stopPropagation) {
                            e.stopPropagation();
                        }
                    }
                };

                // Toggle link
                this.svgRoot.append("rect")
                    .attr("class", "legendToggle")
                    .attr("x", this.width - 80)
                    .attr("y", 4)
                    .attr("rx", 4)
                    .attr("ry", 4)
                    .attr("width", 80)
                    .attr("height", 35)
                    .attr("fill", "rgba(65, 75, 84, 0.6)")
                    .on("click", toggleLegend);

                // Toggle link Text
                this.svgRoot.append("text")
                    .text("Legend")
                    .attr("x", this.width - 63)
                    .attr("y", 26)
                    .attr("fill", "#fff")
                    .on("click", toggleLegend);
            }
        };

        // Generates a scale object for a given axis that will be fed by one or more sources
        Chart.prototype.getScale = function getScale(axisEntity, representativeSource, suggestedAutoRange, options) {
            var chart = this;
            var source = representativeSource;

            if (!options || !options.domain || !options.scaleType) {
                console.error('Missing options');
                }
            var method = options.scaleType;
            var domain = options.domain;

            var scale = {
                d3scale: null,      // underlying d3 scale object
                scale: null,        // from value to midpoint
                bandStart: null,    // from value to leftpoint
                bandWidth: null,    // from value to width
                autoRange: null,    // accept pixel range
                rowScale: null,     // from row to midpoint
                format: null,       // formatter function
                accessor: null,     // representative accessor
                method: method
            };

            if (method === 'categoryScaleType' || method === 'dateTimeScaleType') {
                domain = _.sortBy(domain, source.sorter);
                if (spEntity.DataType.isResource(source.colType)) {
                    domain = spCharts.getResourceDomain(domain, source, this.chartData, axisEntity.showAllChoiceValues, chart.nameCache);
                }
                if (spEntity.DataType.isDateType(source.colType) && method === 'dateTimeScaleType') {
                    // TODO : Make padding work for all formatTypes, then remove this 'if' block
                    if (this.spVisDataService.isCategoricalScaleFormat(source.formatType)) {
                        domain = spCharts.padDomainWithMissingTime(domain, source.formatType);
                    }
                }
                scale.d3scale = d3.scale.ordinal().domain(domain);
                scale.d3scaleFull = d3.scale.ordinal().domain(domain); // full width bands
                scale.scale = function (d) {
                    var res = scale.d3scale(d) + scale.d3scale.rangeBand() / 2;
                    return res;
                };
                scale.bandStart = scale.d3scale;
                scale.bandWidth = scale.d3scale.rangeBand;
                scale.bandStartFull = scale.d3scaleFull;
                scale.bandWidthFull = scale.d3scaleFull.rangeBand;
                scale.autoRange = function (range) {
                    var bandFn = domain.length < chart.plotArea.width / 8 ? 'rangeRoundBands' : 'rangeBands';  // avoid antialiasing, unless there's a large number of points (because it breaks stuff)
                    scale.range = range;
                    scale.d3scale[bandFn](range, 0.2);
                    scale.d3scaleFull[bandFn](range, 0);
                };
                scale.ticks = function (n) {
                    var mod = Math.max(1, Math.round(domain.length / n));
                    var res = _.filter(domain, function (d, i) { return i % mod === mod - 1; });
                    return res;
                };
                scale.baseline = function () { return scale.range[0]; };
            }
            else if (method === 'linearScaleType') {
                if (source.colType === 'DateTime' || source.colType === 'Date' || source.colType === 'Time') {
                    scale.d3scale = d3.time.scale()
                        .domain(domain);
                } else {

                    // Extend domain to zero, if numeric
                    if (spEntity.DataType.isNumeric(representativeSource.colType)) {
                        domain[0] = Math.min(domain[0], 0);
                    }

                    this.applyManualDomain(domain, axisEntity, suggestedAutoRange, source.colType);

                    scale.d3scale = d3.scale.linear()
                        .domain(domain);
                }
                scale.scale = function (d) { return scale.d3scale(d); };
                scale.bandStartFull = scale.bandStart = function (d) { return scale.d3scale(d); };
                scale.bandWidthFull = scale.bandWidth = _.constant(2);
                scale.autoRange = function (range) {
                    scale.range = range;
                    var rangePadding = 6; // padding that appears at start/end of linear axis
                    var d3Range = spCharts.padRange(range, rangePadding, domain);
                    scale.d3scale.range(d3Range);
                };

                // Ensure we don't get lots of dummy ticks when we shouldn't.
                var domainWidth = -1;
                var tickCount = 10;
                if (source.colType === 'Int32') {
                    domainWidth = domain[1] - domain[0] + 1;
                    if (domainWidth < tickCount) {
                        scale.d3scale.ticks = _.constant(spCharts.numberArray(domain[0], domain[1]));
                    }
                }
                if (source.colType === 'Date') {
                    domainWidth = (domain[1] - domain[0]) / (24 * 60 * 60 * 1000) + 1;
                    if (domainWidth < tickCount) {
                        scale.d3scale.ticks = _.constant(spCharts.dateArray(domain[0], domain[1]));
                    }
                }

                // Use a baseline value of zero, unless it is outside of the domain being rendered.
                var baseVal = 0;
                if (domain[1] < 0) {
                    baseVal = domain[1];
                }
                scale.baseline = function () { return scale.d3scale(baseVal); };
            }
            else if (method === 'logScaleType') {
                var zeroGuard = function(val) {
                    return val <= 0 ? null : val;
                };
                var safeSource = _.flowRight(zeroGuard, source);
                domain = [d3.min(this.combined, safeSource), d3.max(this.combined, safeSource)];

                this.applyManualDomain(domain, axisEntity, suggestedAutoRange, source.colType);

                scale.d3scale = d3.scale.log()
                    .domain(domain);
                scale.autoRange = function (range) {
                    scale.range = range;
                    scale.d3scale.range(range);
                };
                scale.baseline = function () { return scale.range[0]; };
            }
            else {
                throw new Error('Cannot determine scaling method.');
            }

            // Default implementations
            scale.d3scaleFull = scale.d3scaleFull || scale.d3scale;
            scale.ticks = scale.ticks || scale.d3scale.ticks;
            scale.scale = scale.scale || function (d) { return scale.d3scale(d); };
            scale.domain = scale.d3scale.domain();

            if (representativeSource)
                scale.rowScale = _.flowRight(scale.scale, representativeSource.generalAccessor);
            scale.format = source.formatter;

            return scale;
        };

        Chart.prototype.applyManualDomain = function applyManualDomain(domain, axisEntity, suggestedAutoRange, colType) {
            if (!axisEntity)
                return;

            // Use explicit domain values from axisEntity, if provided
            // Use suggestedAutoRange if provided and if it exceeds the calculated domain

            var min = spCharts.nativeType(axisEntity.axisMinimumValue, colType);
            if (min !== null)
                domain[0] = min;
            else if (suggestedAutoRange && suggestedAutoRange[0])
                domain[0] = Math.min(domain[0] || suggestedAutoRange[0], suggestedAutoRange[0]);

            var max = spCharts.nativeType(axisEntity.axisMaximumValue, colType);
            if (max !== null)
                domain[1] = max;
            else if (suggestedAutoRange && suggestedAutoRange[1])
                domain[1] = Math.max(domain[1] || suggestedAutoRange[1], suggestedAutoRange[1]);
        };


        // Generates an SVG pattern that can render an image
        Chart.prototype.appendImagePattern = function appendImagePattern(g, accessor) {

            var chart = this;
            var webRoot = chart.spWebService.getWebApiRoot();

            // pretty much everything here is to preserve aspect ratio.
            g.filter(function (d) {
                return d && accessor(d);
            }) // truthy
             .append("pattern")
                .attr("id", function (d) {
                    var img = "img" + accessor(d);
                    return img;
                })
                .attr("patternContentUnits", "objectBoundingBox")
                .attr("preserveAspectRatio", "xMidYMid slice")
                .attr("viewBox", "0 0 1 1")
                .attr("width", "1")
                .attr("height", "1")
             .append("image")
                .attr("preserveAspectRatio", "xMidYMid slice")
                .attr("xlink:href", _.flowRight(this.spXsrf.addXsrfTokenAsQueryString, function(id) {
                    return chart.getImageUri(id, webRoot);
                }, accessor))
                .attr("x", "0")
                .attr("y", "0")
                .attr("width", "1")
                .attr("height", "1");
        };


        // Returns the URI for the image of the given resource ID.
        Chart.prototype.getImageUri = function getImageUri(id, webRoot) {
            return webRoot + '/spapi/data/v1/image/download/' + id;
        };


        // Get a static property for a particular source using the current datum
        Chart.prototype.getSourceProp = function getSourceProp(datum, sourceType, propName) {
            var chart = this;
            var series = chart.accessors.series(datum);
            if (!series) return null;
            var accessor = series[sourceType];
            if (!accessor) return null;
            return accessor[propName];
        };

        // Get the formatted text for a particular source
        Chart.prototype.getSourceText = function getSourceText(datum, sourceType) {
            var chart = this;
            var getTextFn = chart.getSourceProp(datum, sourceType, 'getText');
            if (!getTextFn) return null;
            return getTextFn(datum);
        };

        Chart.prototype.toolTipHtml = function toolTipHtml(dRaw, options) {
            var htmlRow = function(label, value, first) {
                var res = "<br><span>" + (first ? "<strong>" : "") + label + ': ' + value + (first ? "</strong>" : "") + "</span>";
                return res;
            };

            var html = '';
            var included = {};
            var chart = this;
            var first = true;
            var getData = options.dataCallback || _.identity;
            var d = getData(dRaw);

            _.forEach(options.sourceTypes, function(sourceType) {
                var title = chart.getSourceProp(d, sourceType, 'title');
                var isEmpty = chart.getSourceProp(d, sourceType, 'isEmpty');
                if (sourceType !== 'imageSource' && title && !included[title] && !isEmpty) {
                    included[title] = true;
                    html += htmlRow(title, chart.getSourceText(d, sourceType), first);
                    first = false;
                }
            });
            if (options.extraText) {
                _.forEach(_.keys(options.extraText), function(key) {
                    var fn = options.extraText[key];
                    html += htmlRow(key, fn(dRaw));
                });
            }
            if (html.length)
                html = html.substring(4);
            return html;
        };

        // Draws text labels on the chart surface.
        Chart.prototype.decorateDataElements = function decorateDataElements(g, options) {
            // Note: custom implementation for pieChart
            options = options || {};
            var chart = this;
            var isPivot = chart.reportMetadata.isPivot || options.isGroups;

            options.sourceTypes = options.sourceTypes || spChartService.sourceTypes;

            var getData = options.dataCallback || _.identity;

            if (chart.accessors.primarySource) {
                g.attr("primary", _.flowRight(chart.accessors.primarySource.getText, getData));
            }
            if (chart.accessors.valueSource) {
                g.attr("value", _.flowRight(chart.accessors.valueSource.getText, getData));
            }

            if (!this.isMobile) {
                var htmlFn = function (dRaw) {
                    return chart.toolTipHtml(dRaw, options);
                };

                var tip = d3.tip()
                    .attr('class', 'sp-chart-tip ' + this.getTooltipClassId())
                    .offset([-10, 0])
                    .html(htmlFn);

                this.svg.call(tip);

                g.on("mouseover", tip.show)
                 .on("mouseout", tip.hide);
            }

            var resourceClick = function (d) {
                var elem = d;
                var chartType;
                if (options.dataCallback)
                    d = options.dataCallback(d);
                var id = d.row.eid;
                if (id) {                    
                    chartType = sp.result(d, 'group.series.chartType.alias');
                    //rule of chart click, If the chart is not pivot, clicking on a data point will cause an edit form to view the resource associated with that row. 
                    //bug 27437 Charts : Column , Pie and Scatter non-pivot chart are not navigating to its corresponding form when clicked on a data point
                    if (chartType &&
                        chartType !== 'lineChart' &&
                        chartType !== 'areaChart' &&
                        chartType !== 'pieChart' &&
                        chartType !== 'columnChart' &&
                        chartType !== 'scatterChart') {
                        console.log('chart np clicked filter id:', id);
                        var sources = spCharts.getBoundSources(d.group.series, options.sourceTypes);
                        var conds = chart.spVisDataService.convertPivotRowToConds(d.row, sources, chart.reportMetadata);                      
                        if (chart.filterSelectedCallback)
                            chart.filterSelectedCallback(conds, elem);
                    } else {
                        console.log('chart clicked id:', id);
                        if (chart.entitySelectedCallback)
                            chart.entitySelectedCallback(id, elem);
                    }                    
                }
            };

            var pivotClick = function (d) {
                var elem = d;
                if (options.dataCallback)
                    d = options.dataCallback(d);
                var row = d.row;
                if (row) {
					console.log('chart clicked filter');
					var sources = spCharts.getBoundSources(d.group.series, options.sourceTypes);
					var conds = chart.spVisDataService.convertPivotRowToConds(row, sources, chart.reportMetadata);
                    if (chart.filterSelectedCallback)
                        chart.filterSelectedCallback(conds, elem);
                }
            };
            var click = function (e) {
                d3.event.stopPropagation();
                if (isPivot)
                    pivotClick(e);
                else
                    resourceClick(e);
            };

            var oncontextmenu = function (d) {
                var elem = d;
                if (options.dataCallback)
                    d = options.dataCallback(d);
                var id = d.row.eid;
                if (id) {
					console.log('chart right-clicked');
                    if (chart.entityContextMenuCallback)
                        chart.entityContextMenuCallback(id, elem);
                }
                return false;
            };

            g.on("click", click)
             .on("contextmenu", oncontextmenu);
        };

        // Draws text labels on the chart surface.
        Chart.prototype.decorateDataGroupElements = function decorateDataGroupElements(g, options) {
            options = options || {};
            options.dataCallback = options.representativeCallback;
            options.sourceTypes = ['colorSource'];
            options.isGroups = true;

            this.decorateDataElements(g, options);
        };

        // Measures a text label and positions the background around it
        Chart.prototype.positionLabelBackground = function () {
            var text = this;
            var box = text.getBBox();
            var rect = text.parentNode.firstChild;
            rect.setAttribute('x', box.x - 2);
            rect.setAttribute('y', box.y);
            rect.setAttribute('width', box.width + 4);
            rect.setAttribute('height', box.height);
            rect.setAttribute('rx', 2);
            rect.setAttribute('ry', 2);
        };

        // Background click
        Chart.prototype.onBackgroundClick = function () {
            if (sp.result(this, 'reportMetadata.isPivot')) {
                var conds = this.spVisDataService.getEmptyConds();
                if (this.filterSelectedCallback)
                    this.filterSelectedCallback(conds, null);
            }
        };

        // Create an element ID
        Chart.prototype.createId = function createId(prefix) {
            var id = (prefix||'c') + (this.tagId++) + '-' + this.tagRoot;
            return id;
        };

        // Draws text labels on the chart surface.
        // Usage: g2 = chart.clipRect(g, { x:fx, y:fy, w:fw, h:fh })
        Chart.prototype.clipRect = function clip(g, rect) {
            var prefix = this.createId("clip") + "-";

            g.append("clipPath")
                .attr("id", function (d, i) { return prefix + i; })
                .append("rect")
                .call(this.applyRect, rect);
            var clippedG = g.append("g")
                .attr("clip-path", function (d, i) { return 'url(#' + prefix + i + ')'; });
            return clippedG;
        };

        // Draws text labels on the chart surface.
        // Usage: g.call(chart.applyRect, rectOptions)
        Chart.prototype.applyRect = function applyRect(selection, options) {
            return this
                .attr("x", options.x)
                .attr("y", options.y)
                .attr("width", options.width)
                .attr("height", options.height);
        };

        // Draws text labels on the chart surface.
        Chart.prototype.drawLabels = function drawLabels(parentElem, options) {
            var chart = this;
            var accessors = chart.accessors;
            var getData;
            if (!accessors.textSource)
                return;

            parentElem = chart.labelRoot;

            // Default configuration
            options = _.defaults(options, {
                dx: "0",
                dy: ".35em",
                textAnchor: "middle",
                xpos: function (d) {
                    var d2 = getData(d);
                    return accessors.x(d2).rowScale(d2);
                },
                ypos: function (d) {
                    var d2 = getData(d);
                    return accessors.y(d2).rowScale(d2);
                },
                dataCallback: _.identity
            });
            getData = options.dataCallback;

            // Create top container for labels
            var gContainer = parentElem
                .append("g")
                .attr("class", "data-labels");

            if (options.rootDatum) {
                gContainer.datum(options.rootDatum);
            }

            // Handle grouped data
            if (options.groups) {
                gContainer = spCharts.containers(gContainer, options.groups);
            }

            // Container for each label
            var gLabel = spCharts.containers(gContainer, options.data);

            // Visiblity
            if (options.visible) {
                gLabel.style("visibility", function (d) {
                    return options.visible(d) ? "hidden" : "visible"; // hide non-leaf nodes
                });
            }

            // Clip
            if (options.clipRect) {
                gLabel = this.clipRect(gLabel, options.clipRect);
            }

            // Wrap in translucent boxes
            if (options.box) {
                gLabel.append("rect");
            }

            // Text
            var text = gLabel.append("text")
                .attr("class", "data-label")
                .attr("x", options.xpos)
                .attr("y", options.ypos)
                .attr("dx", options.dx)
                .attr("dy", options.dy)
                .style("text-anchor", options.textAnchor)
                .text(options.text || function(datum) {
                    var d = getData(datum);
                    var series = accessors.series(d);
                    var fn = series.textSource;
                    if (!fn) return null;
                    var value = series.textSource(d);
                    var formatted = series.textSource.formatter(value);
                    return formatted;
                });
            if (options.translate)
                text.attr("transform", "translate(" + options.translate + ")");
            if (options.box)
                text.each(chart.positionLabelBackground);

            // Apply tooltip & context menu to labels
            var decorOpts = options.decorOpts || { dataCallback: getData };
            this.decorateDataElements(text, decorOpts);

            //if (options.delay)
            //    text.style("visibility", "hidden").transition().duration(options.delay).style("visibility", "visible");
        };

        // Builds data into a hierarchy
        Chart.prototype.buildHierarchy = function buildHierarchy(data, childSource, parentSource, options) {
            var result = {
                nodes: [],
                links: [],
                roots: [],
                root: null
            };
            var nodeLookup = {};
            var chart = this;

            // TODO: we are visiting children and parents
            // We may get nodes that only appear in the parents column, but the text accessor will probably show us text for the child. 
            // Probably need a nice way to show either

            // Create nodes
            // (Visit both getChild and getParent to ensure we get a node for anything that only appears on one side)
            _.forEach([childSource, parentSource], function (accessor) {
                if (!accessor)
                    return;
                _.forEach(data, function(datum) {
                    if (!datum)
                        return;
                    var value = accessor(datum); // hopefully an ID, but whatever
                    if (nodeLookup[value])
                        return; // continue .. already visited
                    if (accessor === parentSource && !value)
                        return; // continue .. don't create nodes for blank parents (but do for children)

                    var node = {
                        id: value,
                        name: childSource.getText(datum),
                        data: datum,
                        children: [],
                        parents:[],
                        parent:null,
                        isParent:accessor === parentSource
                    };
                    nodeLookup[value] = node;
                    result.nodes.push(node);
                });
            });

            // Create links
            if (childSource && parentSource) {
                _.forEach(data, function(row) {
                    // ID of child and parent
                    var child = childSource(row);
                    var parent = parentSource(row);
                    var childRow = nodeLookup[child];
                    var parentRow = nodeLookup[parent];
                    if (child && parent && !(options.singleRoot && childRow.parent)) {
                        childRow.parent = parentRow;
                        childRow.parents.push(parentRow);
                        parentRow.children.push(childRow);
                        result.links.push({
                            // Datum of child/parent
                            source: childRow,
                            target: parentRow,
                            weight: 1
                        });
                    }
                });
            }

            result.roots = _.filter(result.nodes, function(n) {
                return n.parent === null;
            });
            result.root = result.roots[0];

            // If there are multiple roots, create a single faux root node.
            if (options && options.singleRoot) {
                if (result.roots.length > 1) {
                    var root = {
                        parent: null,
                        parents: [],
                        children: result.roots,
                        data: null,
                        name: 'root',
                        id: 0
                    };
                    _.forEach(result.roots, function (node) {
                        node.parent = root;
                        node.parents = [root];
                    });
                    result.root = root;
                    result.roots = [root];
                }
            }

            result.textSource = function (node) {
                if (!node.data)
                    return '';
                if (node.isParent && parentSource)
                    return parentSource.getText(node.data);
                return (chart.accessors.textSource || childSource).getText(node.data);
            };

            return result;
        };
        
        Chart.prototype.drawSymbols = function drawSymbols(series, symbolContainers, options) {
            var chart = this;
            var accessors = this.accessors;

            var markerInfo = spCharts.markerInfo(series);
            var markerSize = spCharts.markerSize(series);
            if (!markerInfo && !accessors.symbolSource && !options.useDefault)
                return null;

            // Container
            if (options.makeContainer) {
                symbolContainers = symbolContainers.append("g").attr("class", "symbols");
            }
            var g = spCharts.containers(symbolContainers, series.data);

            // Symbol factory
            var symbol;
            var filled;
            var symbolIndex = function(i) {
                return d3.svg.symbol()
                    .type(d3.svg.symbolTypes[i % d3.svg.symbolTypes.length]);
            };
            var format = function (symbol) {
                symbol.size(markerSize);
                return symbol;
            };


            if (!accessors.symbolSource) {
                var shape = markerInfo ? markerInfo.shape : 'circle';
                symbol = format(d3.svg.symbol().type(shape));
                filled = !markerInfo || markerInfo.filled;
            } else {
                var symbolKeys = _.uniq(_.map(chart.combined, _.flowRight(function (d) { return '' + d; }, accessors.symbolSource)));
                var lookup = {};
                _.forEach(symbolKeys, function (key, i) {
                    lookup[key] = format(symbolIndex(i));
                });
                symbol = function (d) {
                    var value = '' + accessors.symbolSource(d);
                    return lookup[value]();
                };
                filled = true;
            }

            // Render path
            var p = g.append("svg:path")
                .attr("d", symbol)
                .attr("transform", function (d) { return "translate(" + Math.round(options.x(d)) + "," + Math.round(options.y(d)) + ")"; });

            if (filled) {
                p.style("fill", options.color || accessors.color);
            } else {
                p.style("stroke", options.color || accessors.color)
                 .style("fill", "#fff");
            }

            return g;
        };


        // Gets the tooltip class id for this chart so that
        // we can identify tooltips for this chart
        Chart.prototype.getTooltipClassId = function getTooltipClassId() {
            return 'sp-chart-tip-id-' + this.tagRoot;
        };

        return Chart;
    })();

    spCharts.Chart = Chart;

})(spCharts || (spCharts = {}));

