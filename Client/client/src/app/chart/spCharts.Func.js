// Copyright 2011-2016 Global Software Innovation Pty Ltd

var spCharts;

// Stand-alone functional stuff .. everything gets passed to arguments

(function (spCharts) {

    // Partition/pivot the data based on groups, series, and other factors
    // Similar to _.groupBy, except it captures representative rows, and provides links from rows back to group
    spCharts.groupData = function groupData(rows, pivotAccessor) {

        var groupMap = {};
        var groups = [];

        _.forEach(rows, function (row) {

            var data = {
                row: row,
                group: null
            };

            var groupKey = pivotAccessor(row);
            var group = groupMap[groupKey];
            if (!group) {
                group = {
                    data: [],
                    representative: row    // a representative data row
                };
                groupMap[groupKey] = group;
                groups.push(group);
            }
            data.group = group;
            group.data.push(data);
        });
        return groups;
    };


    // Function that access the row of a row box. (Provided for indirection)
    spCharts.rowBoxRow = _.property('row');


    // Partition/pivot the data based on groups, series, and other factors
    // Similar to _.groupBy, except it captures representative rows, and provides links from rows back to group
    spCharts.groupAndFillData = function groupData(rows, pivotAccessor, primaryAccessor, sortAccessor) {

        // Rows must be sorted for algorithm to work
        // Sort accessor must effectively sort the primary accessor
        var sorted = _.sortBy(rows, sortAccessor);

        var groupMap = {};
        var groups = [];
        var prevPrimary = {}; // just some object
        var prevPrev = prevPrimary;
        var length = 0;

        var bringToLength = function (group, len, primary) {
            while (group.data.length < len) {
                var empty = {
                    group: group,
                    x: primary
                    // row: undefined
                };
                group.data.push(empty);
            }
        };

        _.forEach(sorted, function (row) {

            // Get primary
            var primary = primaryAccessor(row);

            // New primary value
            if ('' + primary != '' + prevPrimary) {
                _.forEach(groups, function (g) { bringToLength(g, length, prevPrimary); });
                prevPrev = prevPrimary;
                prevPrimary = primary;
                length++;
            }

            // Get pivot group
            var groupKey = pivotAccessor(row);
            var group = groupMap[groupKey];
            if (!group) {
                group = {
                    data: [],
                    representative: row    // a representative data row
                };
                bringToLength(group, length - 1, prevPrev);
                groupMap[groupKey] = group;
                groups.push(group);
            }

            // Ensure value is unique before storing
            if (group.data.length == length - 1) {
                var datum = {
                    row: row,
                    group: group,
                    x: prevPrimary
                };
                group.data.push(datum);
            }


        });

        // bring any stragglers to the same length
        _.forEach(groups, function (g) { bringToLength(g, length, prevPrimary); });

        return groups;
    };

    // Determine if a series is stacked
    spCharts.isStackedSeries = function isStackedSeries(series) {
        var stackMethod = sp.result(series.entity, 'stackMethod.nsAlias');
        var isStacked = series.accessors.colorSource &&
            !spCharts.sourcesMatch(series.accessors.colorSource, series.accessors.primarySource) &&
            !series.accessors.endValueSource &&
            (!stackMethod || stackMethod !== 'core:stackMethodNotStacked') &&
            (!series.valueAxis || _.includes(['linearScaleType','logScaleType'], series.valueAxis.method));
        return isStacked;
    };

    // Determine if a series will show bars side-by-side
    spCharts.isSideBySide = function isSideBySide(series) {
        var stackMethod = sp.result(series.entity, 'stackMethod.nsAlias');
        var chartType = sp.result(series.entity, 'chartType.nsAlias');
        var res = series.accessors.colorSource &&
            (chartType === 'core:barChart' || chartType === 'core:columnChart') &&
            !spCharts.sourcesMatch(series.accessors.colorSource, series.accessors.primarySource) &&
            (stackMethod === 'core:stackMethodNotStacked') &&
            spCharts.useBands(series.primaryAxis.method);
        return res;
    };

    // Determine if a scale type uses bands for each location
    spCharts.useBands = function isBands(scaleType) {
        return scaleType === 'categoryScaleType' || scaleType === 'dateTimeScaleType';
    };

    // Create stacked charts groups
    spCharts.createStackedData = function createStackedData(accessors, series, sortAccessor) {

        var isStacked = spCharts.isStackedSeries(series);
        var valueAxis = series.valueAxis;

        var groups, startPos, endPos, getRow, getGroupData, getGroupDataUnfiltered, method;
        var additionalSize = 0;

        if (isStacked) {
            // Stacked
            var primarySource = accessors.primarySource;
            var valueSource = accessors.valueSource;
            var colorSource = accessors.colorSource;

            groups = spCharts.groupAndFillData(series.data, colorSource, primarySource, sortAccessor);
            var groupPrimary = _.flowRight(primarySource, spCharts.rowBoxRow);
            var groupValue = _.flowRight(valueSource, spCharts.rowBoxRow);

            // Configure the d3 stack
            var stack = d3.layout.stack()
                .values(_.property('data')) // gets the data array out of a group
                .x(groupPrimary)
                .y(groupValue);

            // Baseline algo
            var methods = {
                'core:stackMethodStacked': 'zero',
                'core:stackMethod100percent': 'expand',
                'core:stackMethodCentre': 'silhouette'
            };
            method = sp.result(series.entity, 'stackMethod.nsAlias') || 'core:stackMethodStacked';

            // Create stack data
            stack.offset(methods[method]);

            if (method === 'core:stackMethod100percent') {
                // scale results 0..100 instead of 0..1
                stack.out(function (d, y0, y) { d.y0 = y0 * 100; d.y = y * 100; });
            }

            // Do the d3 stack
            if (groups.length > 0) stack(groups);

            // Getters
            if (valueAxis) {
                startPos = function(rowBox) { return valueAxis.scale(rowBox.y0); };
                endPos = function(rowBox) { return valueAxis.scale(rowBox.y0 + rowBox.y); };
            }
            getRow = spCharts.rowBoxRow;
            getGroupData = function (group) { return _.filter(group.data, getRow); };
            getGroupDataUnfiltered = _.property('data');

        } else {
            // Non-stacked

            if (valueAxis) {
                startPos = valueAxis.rowScale;
                
                if (series.accessors.endValueSource) {
                    if (spCharts.useBands(valueAxis.method)) {
                        // Ranges on category axis - extend from the start of the first category to the end of the second.
                        startPos = _.flowRight(valueAxis.bandStart, series.accessors.valueSource);
                        endPos = _.flowRight(valueAxis.bandStart, series.accessors.endValueSource);
                        additionalSize = valueAxis.bandWidth();
                    } else {
                        // Ranges on linear axis - extend from one exact location to the other.
                        endPos = _.flowRight(valueAxis.scale, series.accessors.endValueSource);
                    }
                } else {
                    endPos = valueAxis.baseline;
                }
            }

            groups = [ series.data ];
            getRow = _.identity;
            getGroupDataUnfiltered = getGroupData = _.identity;
        }

        return {
            isStacked: isStacked,
            groups: groups,  // array of groups
            getRow: getRow,  // accessor for collecting rows from containers,
            startPos: startPos,
            endPos: endPos,
            valuePos: function(d) { return Math.min(startPos(d), endPos(d)); },
            valueSize: function(d) { return Math.max(1, Math.abs(startPos(d) - endPos(d))) + additionalSize; }, // max used to ensure at least a single pixel for range bars that start/end at same value
            getGroupData: getGroupData,    // fn to get the non-empty data rowBoxes from a group
            getRepresentative: function (g) { return getRow(getGroupData(g)[0]); },
            getGroupDataUnfiltered: getGroupDataUnfiltered,
            method: method || null,
            extraText: method === 'core:stackMethod100percent' ? { 'Percent': function (d) { return d3.round(d.y,1) + '%'; } } : null
        };
    };

    // Get the domain for a given source
    spCharts.getSourceDomain = function getSourceDomain(context, series, source, method) {
        var domain = _.map(series.data, source);
        if (method === 'linearScaleType' || method === 'logScaleType') {
            domain = [d3.min(domain), d3.max(domain)];
        }
        return domain;
    };

    // Create accessors for bars that render side-by-side
    spCharts.getSideBySideAccessors = function getSideBySideAccessors(context, series) {
        var accessors = context.chart.accessors;
        var primaryAxis = series.primaryAxis;

        if (!spCharts.isSideBySide(series)) {
            return {
                pos: _.flowRight(primaryAxis.bandStart, accessors.primarySource),
                bandWidth: primaryAxis.bandWidth
            };
        }
        var domain = spCharts.getSourceDomain(context, series, accessors.colorSource, 'categoryScaleType');
        var innerAxis = spEntity.fromJSON({ axisScaleType: jsonLookup('categoryScaleType') });
        var innerScale = context.chart.getScale(innerAxis, series.colorSource, null, { domain: domain, scaleType: 'categoryScaleType' });
        innerScale.autoRange([0, primaryAxis.bandWidth()]);

        return {
            pos: function xpos(row, index) {
                var ppos = primaryAxis.bandStart(accessors.primarySource(row, index));
                var cpos = innerScale.bandStart(accessors.colorSource(row, index));
                return ppos + cpos;
            },
            bandWidth: innerScale.bandWidth
        };
    };

    // Combines an array of domains into a single domain
    spCharts.combineDomains = function combineDomains(domains, method) {
        var res = _.union.apply(null, domains);
        if (method === 'linearScaleType' || method === 'logScaleType') {
            res = [d3.min(res), d3.max(res)];
        }
        return res;
    };

    // Sort a choice field domain by the order the items are listed in the metadata
    spCharts.getResourceDomain = function getResourceDomain(domain, source, chartData, bShowAllChoices, nameCache) {
        if (!source.typeId || !domain || !chartData)
            return domain;

        // Get all instances of type
        var instances;

        if (source.colType === 'ChoiceRelationship') {
            var choiceMetadata = chartData.reportData.meta.choice;
            if (choiceMetadata) {
                instances = choiceMetadata[source.typeId];
            }
        } else if (source.colType === 'InlineRelationship' || source.colType === 'UserInlineRelationship') {
            var resources = chartData.lookups[source.typeId];
            if (resources) {
                instances = _.map(resources, function (e) {
                    return { id: e.idP, name: e.name };
                });
            }
        }

        // instances is ordered array [ {id:123, name:'abc'}, ... ]
        if (!instances)
            return domain;

        // clip for sanity
        var maxInstances = 100;
        instances = _.take(instances, maxInstances);

        if (bShowAllChoices) {
            var allValues = _.map(instances, function (inst) { return '' + inst.id; }); // pluck and string
            domain = spCharts.combineDomains([domain, allValues], 'categoryScaleType');
        }

        var lookup = {};
        _.forEach(instances, function (val, index) {
            lookup[val.id] = index;
            nameCache[val.id] = val.name;
        });

        var res = _.sortBy(domain, function (id) {
            if (!id)
                return -2;  // sort null at top
            var pos = lookup[id];
            return pos;
        });
        return res;
    };

    // Returns true if two sources are the same
    spCharts.sourcesMatch = function sourcesMatch(s1, s2) {
        if (!s1 || !s2)
            return false;
        var paths = ['chartReportColumn', 'sourceAggMethod', 'specialChartSource'];
        return _.every(paths, function(path) {
            return spEntity.equivalent(sp.result(s1.entity, path), sp.result(s2.entity, path) ) ;
        });
    };

    // Calculates the domain of a stacked series.
    spCharts.getStackDomain = function getStackDomain(context, series, source, method) {

        // note: category doesn't really make sense for stack values
        if (source !== series.accessors.valueSource || spCharts.useBands(method)) {
            // fall back to default
            return spCharts.getSourceDomain(context, series, source, method);
        }

        var accessors = context.chart.accessors;

        // Get stack info
        var sort =  accessors.primarySource;
        var stack = spCharts.createStackedData(accessors, series, sort);

        var domain;
        if (stack.isStacked) {
            // just examine the min/max values of the bottom/top groups respectively
            // we need to examine all data points because intermediate groups may be higher
            if (stack.groups.length === 0) {
                domain = [];
            } else {
                var last = stack.groups.length - 1;
                var min = d3.min(stack.groups[0].data, function (rowBox) { return rowBox.y0; });
                var max = d3.max(stack.groups[last].data, function (rowBox) { return rowBox.y0 + rowBox.y; });
                domain = [min, max];
            }
        } else {
            domain = spCharts.getSourceDomain(context, series, source, method);

            if (series.accessors.endValueSource) {
                var domain2 = spCharts.getSourceDomain(context, series, series.accessors.endValueSource, method);
                domain = spCharts.combineDomains([domain, domain2]);
            }
        }
        return domain;
    };

    // Draws the chart title
    spCharts.drawTitle = function drawTitle(svg, chartEntity, box) {

        if (!chartEntity.chartTitle)
            return;

        var x;
        var anchor;

        switch (sp.result(chartEntity, 'chartTitleAlign.nsAlias')) {
            case 'core:alignLeft':
                x = box.left;
                anchor = 'start';
                break;
            case 'core:alignRight':
                x = box.right;
                anchor = 'end';
                break;
            default:
                x = (box.left + box.right) / 2;
                anchor = 'middle';
                break;
        }

        var title = svg.append("g")
            .attr("class", "chart-title")
            .append("text")
            .attr("text-anchor", anchor)
            .attr("x", x)
            .attr("y", 0)
            .text(chartEntity.chartTitle);

        spCharts.alignToTop(title);
    };

    // Aligns a text box so that the top of the bounding box is at the current 'y' coordinate.
    spCharts.alignToTop = function alignToTop(text) {
        var box = text.node().getBBox();
        var curY = parseFloat(text.attr("y"));
        text.attr("y", curY + (curY - box.y));
    };

    // Normalizes a vector to unit vector, or some other length
    spCharts.normalize = function normalize(vector, newLength) {
        var len = Math.sqrt(vector[0] * vector[0] + vector[1] * vector[1]);
        if (!len) return [0, 0];
        var scale = (newLength || 1) / len;
        var res = [vector[0] * scale, vector[1] * scale];
        return res;
    };

    spCharts.labelOutside = function labelOutside(series, defaultPos) {
        var val = sp.result(series, 'entity.dataLabelPos.nsAlias') || 'core:dataLabelPos' + defaultPos;
        var res = val === 'core:dataLabelPosOutside';
        return res;
    };

    spCharts.containers = function containers(parentElem, data, elemName) {
        return parentElem.selectAll(elemName || 'g')
                .data(data)
                .enter().append(elemName || 'g');
    };

    spCharts.markerInfo = function markerInfo(series) {
        var alias = sp.result(series, 'entity.markerShape.nsAlias');
        if (!alias || alias === 'core:markerShapeNone')
            return null;
        var filled = alias.indexOf('Filled') != -1;
        var shape = alias.slice(filled ? 22 : 16).toLowerCase();
        if (shape === 'triangleup') shape = 'triangle-up';
        return { filled: filled, shape: shape };
    };

    spCharts.markerSize = function markerSize(series) {
        var alias = sp.result(series, 'entity.markerSize.nsAlias');
        if (alias == 'core:sizeSmall') return 25;
        if (alias == 'core:sizeLarge') return 81;
        return 49; // square pixels
    };

    // Returns function that adjusts height/width to have nice pixel boundaries and a single pixel gap
    spCharts.sizeWithGaps = function heightWithGaps(fnPos, fnSize) {
        return function(d) {
            // Stuff around to get the anti-alias boundaries 'just nice'
            var yRaw = fnPos(d);
            var hRaw = fnSize(d);
            var y2 = Math.floor(yRaw + hRaw);
            var height = y2 - Math.floor(yRaw);
            return Math.max(1, height - 1); // visible gap
        };
    };

    // Returns the unique dates determined by the accompanying comparison function
    function uniqueDates(dates, compareFn) {
        var d = dates.concat();
        // n-squared. can probably be better here. (_.union? _.merge?)
        for (var i = 0; i < d.length; ++i) {
            for (var j = i + 1; j < d.length; ++j) {
                if (compareFn(d[i], d[j]) === 0) {
                    // remove the jth element from the array as it can be considered duplicate
                    d.splice(j--, 1);
                }
            }
        }
        return d;
    }

    // Compares two dates with the help of the provided comparator function
    function compareDatesWithFunction(d1, d2, fnC) {
        var c1 = fnC(d1);
        var c2 = fnC(d2);
        if (c1 > c2) return 1;
        if (c1 < c2) return -1;
        return 0;
    }

    function compareMonths(d1, d2) { return compareDatesWithFunction(d1, d2, function (d) { return !d ? null : d.getMonth(); }); }
    function compareYears(d1, d2) { return compareDatesWithFunction(d1, d2, function (d) { return !d ? null : d.getYear(); }); }
    function compareDays(d1, d2) { return compareDatesWithFunction(d1, d2, function (d) { return !d ? null : d.getDate(); }); }
    function compareWeekdays(d1, d2) { return compareDatesWithFunction(d1, d2, function (d) { return !d ? null : d.getDay(); }); }
    function compareHours(d1, d2) { return compareDatesWithFunction(d1, d2, function (d) { return !d ? null : d.getHours(); }); }

    // Compares if dates appear in the same logical quarter
    function compareQuarters(d1, d2) {
        var m1 = d1.getMonth();
        var m2 = d2.getMonth();
        var q1 = (Math.floor(m1 / 3) * 3);
        var q2 = (Math.floor(m2 / 3) * 3);
        if (q1 > q2) return 1;
        if (q1 < q2) return -1;
        return 0;
    }

    // Compares one date with another disregarding the year component
    function compareDayMonths(d1, d2) {
        var r = compareMonths(d1, d2);
        if (r === 0) {
            r = compareDays(d1, d2);
        }
        return r;
    }

    // Compares one date with another disregarding the date component
    function compareMonthYears(d1, d2) {
        var r = compareYears(d1, d2);
        if (r === 0) {
            r = compareMonths(d1, d2);
        }
        return r;
    }

    // Compares one date with another if they are the same quarter of the same year
    function compareQuarterYears(d1, d2) {
        var r = compareYears(d1, d2);
        if (r === 0) {
            r = compareQuarters(d1, d2);
        }
        return r;
    }

    // Compares one date with another disregarding all components less granular than the day
    function compareDates(d1, d2) {
        var r = compareYears(d1, d2);
        if (r === 0) {
            r = compareMonths(d1, d2);
            if (r === 0) {
                r = compareDays(d1, d2);
            }
        }
        return r;
    }

    // For time based format types, will pad the domain with missing dates to best resemble an appropriate time scale
    spCharts.padDomainWithMissingTime = function (domain, formatType) {
        // TODO!!! What about when a manual min or max is set on the axis in the chart?????
        // TODO!!!!! Right now it seems ignored by time scale anyway, so whatever (#26132)
        // TODO!!!!!!! There is a problem with grouping, only with the DateTime col type right now which is impacting this (#26136)
        var ticks = [];
        var compareFn = _.identity;
        var today = new Date();
        var year = today.getFullYear(); // this year

        switch (formatType) {

            case 'dateMonth': // Monthly
            case 'dateTimeMonth':
                // avoid generating more filler dates then necessary.
                // just get the full set of months
                ticks = d3.time.scale()
                    .domain([new Date(year, 0, 1), new Date(year, 11, 1)])
                    .nice(d3.time.month.utc)
                    .ticks(d3.time.months, 1);

                compareFn = compareMonths; // sort ignoring year
                break;

            case 'dateMonthYear':
            case 'dateTimeMonthYear':
                ticks = d3.time.scale()
                    .domain([_.min(domain), _.max(domain)])
                    .nice(d3.time.month.utc)
                    .ticks(d3.time.months, 1);

                // we generally don't need the last one that d3 provides here
                if (ticks.length > 0) { ticks.splice(ticks.length--, 1); }

                compareFn = compareMonthYears; // disregard the date
                break;

            case 'dateQuarter': // Quarterly
            case 'dateTimeQuarter':
                // avoid generating more filler dates then necessary.
                // just get the full set of quarters (hmmm. what about financial year?)
                ticks = d3.time.scale()
                    .domain([new Date(year, 0, 1), new Date(year, 11, 1)])
                    .nice(d3.time.month.utc)
                    .ticks(d3.time.months, 3);

                compareFn = compareQuarters;
                break;

            case 'dateQuarterYear':
            case 'dateTimeQuarterYear':
                ticks = d3.time.scale()
                    .domain([_.min(domain), _.max(domain)])
                    .nice(d3.time.month.utc)
                    .ticks(d3.time.months, 3); // three-monthly

                // we generally don't need the last one that d3 provides here
                if (ticks.length > 0) { ticks.splice(ticks.length--, 1); }

                compareFn = compareQuarterYears;
                break;

            case 'dateYear': // Yearly
            case 'dateTimeYear':
                ticks = d3.time.scale()
                    .domain([_.min(domain), _.max(domain)])
                    .nice(d3.time.year.utc)
                    .ticks(d3.time.years, 1);

                // we generally don't need the last one that d3 provides here
                if (ticks.length > 0) { ticks.splice(ticks.length--, 1); }

                compareFn = compareYears;
                break;

            case 'dateWeekday': // Days
            case 'dateTimeWeekday':
                // get a set of dates that will pad out the weekdays for us
                ticks = d3.time.scale()
                    .domain([today, d3.time.day.offset(today, -7)])
                    .ticks(d3.time.days, 1);

                compareFn = compareWeekdays; // sort against weekday
                break;

            case 'dateDayMonth':
            case 'dateTimeDayMonth':
                ticks = d3.time.scale()
                    .domain([_.min(domain), _.max(domain)])
                    .ticks(d3.time.days, 1);

                compareFn = compareDayMonths; // sort ignoring year
                break;

            case 'dateTimeDate':
                ticks = d3.time.scale()
                    .domain([_.min(domain), _.max(domain)])
                    .ticks(d3.time.days, 1);

                compareFn = compareDates;
                break;

            case 'timeHour': // Hourly
            case 'dateTimeHour':
                // get a set of times that will pad out the available hours in a day for us
                ticks = d3.time.scale()
                    .domain([today, d3.time.day.offset(today, -1)])
                    .ticks(d3.time.hours, 1);

                compareFn = compareHours; // sort against hour
                break;
        }

        // important! do this before the sort so that we implicitly keep the existing (rather than incoming) dates
        // they are typically the ones associated with the data points!
        domain = uniqueDates(domain.concat(ticks), compareFn);

        // now sort
        domain.sort(compareFn);

        return domain;
    };

    // Get series name
    spCharts.getSeriesName = function getSeriesName(series) {
        // Keep in sync with spChartBuilderService.getSeriesName
        if (series.entity.name && series.entity.name !== 'Chart Series')
            return series.entity.name;

        var res = '';
        try {
            // Use colTitle if available to avoid the aggregate suffix
            if (series.valueSource.colTitle)
                res += ' ' + series.valueSource.colTitle;
        }
        catch (e) { }
        try {
            res += ' ' + series.chartType.name;
        } catch (e) { }
        if (res) {
            res = res.slice(1);
        }
        return res || 'Chart Series';
    };

    // Return every int from min to max, inclusive
    spCharts.numberArray = function numberArray(min, max) {
        return _.range(min, max + 1);
    };

    // Return every date from min to max, inclusive
    spCharts.dateArray = function numberArray(min, max) {
        var res = [];
        var cur = min;
        res.push(min);
        var bail = 20;
        while (cur.getDate() !== max.getDate()) {
            var next = new Date(cur);
            next.setDate(cur.getDate() + 1);
            cur = next;
            res.push(cur);
            if (!(bail--))
                throw new Error("dataArray failed");
        }
        return res;
    };

    // Adds padding to a range - but only so long as that side of the range is not a domain point of zero (so we snugly fit the zero line)
    spCharts.padRange = function padRange(range, padding, domain) {
        var sign = range[1] > range[0] ? 1 : -1;
        var min = range[0] + (domain[0] !== 0 ? padding * sign : 0);
        var max = range[1] - (domain[1] !== 0 ? padding * sign : 0);
        if (sign * (max - min) < 0) { // if the sign has changed
            min = max = (min + max) / 2;
        }
        return [min,max];
    };

    spCharts.nativeType = function nativeType(value, type) {
        if (value === null || value === undefined )
            return null;
        switch (type) {
            case 'Int32':
                return parseInt(value, 10);
            case 'Decimal':
            case 'Currency':
                return parseFloat(value, 10);
        }
        return value;
    };

    spCharts.getBoundSources = function getBoundSources(series, sourceTypes) {

        var sources = _.compact(_.map(sourceTypes, function(sourceType) {
            return series[sourceType];
        }));
        return sources;
    };

    // Only return every nth entry if there are too many to fit
    spCharts.makeTicksFit = function makeTicksFit(domain, range, minimumPixelsPerTick) {
        var size = Math.abs(range[1] - range[0]);
        if (minimumPixelsPerTick * domain.length > size) {
            var mod = Math.ceil(domain.length * minimumPixelsPerTick / size);
            var newDomain = _.filter(domain, function(value, index) {
                return index % mod === 0;
            });
            return newDomain;
        } else {
            return domain;
        }
    };

})(spCharts || (spCharts = {}));

