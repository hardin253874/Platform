// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('mod.ui.spReportAggregateDataManager', ['sp.common.filters', 'mod.ui.spReportFilters', 'mod.common.spTenantSettings', 'mod.ui.spReportConstants', 'spApps.reportServices', 'mod.common.alerts', 'mod.ui.spReportConstants'])
        .provider('spReportAggregateDataManager', function () {
            this.$get = function ($filter, spTenantSettings, groupValueColumnIdPrefix, spReportService, spAlertsService, reportRollupSize) {
                return function spReportAggregateDataManager(reportMetadata) {
                    var exports = {},
                        // A cache of aggregate data. Keyed off group data + grouping id/
                        aggregateDataCache = {},
                        // A cache of aggregate data. Keyed off calculated group values in 
                        // aggregateDataCache.
                        reverseAggregateDataCache = {},
                        // The grand totals map entry.
                        // Stored separately to avoid unnecessary lookups.
                        grandTotalsCacheItem = {},
                        // A cache of column format functions, keyed off column id.
                        columnFormatFuncCache = {},
                        // A cache of group columns, keyed off column id used for quick lookups when formatting values
                        columnFormatGroupColumnCache = {},
                        currencySymbol = '$',
                        // Cache of group columns
                        groupColumns = [],
                        // The grand total grouping id
                        grandTotalGroupingId,
                        // This is an array of group row values.     
                        // This is used to insert hidden rows into the grid to force
                        // groups with no children to be displayed.
                        allGroupValues = [],
                        // True if the report has rollup data, false otherwise                        
                        hasRollupData = false;


                    // Get the currency symbol for the current tenant
                    spTenantSettings.getCurrencySymbol().then(function (symbol) {
                        if (symbol) {
                            currencySymbol = symbol;
                        } else {
                            currencySymbol = '$';
                        }
                    });


                    // Add the specified group values, group id to the caches
                    // groupValues is the rdata.hdrs object from the report metadata.
                    // It is an array of objects keyed off column id.
                    function addGroupValuesToCaches(groupValues, groupId, aggDataItem, reverseAggDataItem) {
                        var aggItem,
                            reverseAggItem,
                            aggValues = [];

                        if (!aggDataItem.items) {
                            aggDataItem.items = {};
                        }

                        if (!reverseAggDataItem.items) {
                            reverseAggDataItem.items = {};
                        }

                        // Add all the group values to the cache
                        _.forEach(groupValues, function (gv) {
                            var aggValue,
                                rawValue,
                                reverseAggItem,
                                // Get the group column id
                                groupColId = _.first(_.keys(gv)),
                                // Get the value
                                valObj = gv[groupColId],
                                valueKey;

                            // Get the raw value
                            if (valObj) {
                                if (valObj.val) {
                                    rawValue = valObj.val;
                                } else if (valObj.vals) {
                                    rawValue = valObj.vals;
                                }
                            }

                            // Convert the raw value to a value that can be used as a key into the cache
                            valueKey = convertValueToKey(reportMetadata.rcols[groupColId].type, rawValue);

                            aggItem = aggDataItem.items[valueKey];
                            reverseAggItem = reverseAggDataItem.items[aggValue];

                            // Populate the forward cache, keying off the
                            // group value keys
                            if (!aggItem) {
                                // Need to convert to string because the when getting the value back from the grid
                                // It is converting it to a string
                                // 0 is converted to 'null'
                                aggValue = 'AggValue_' + _.keys(aggDataItem.items).length;

                                aggItem = {
                                    aggValue: aggValue,
                                    items: {}
                                };
                                aggDataItem.items[valueKey] = aggItem;
                            } else {
                                aggValue = aggItem.aggValue;
                            }

                            aggValues.push(aggValue);

                            // Populate the reverse cache, keying off
                            // the calculated agg values
                            // Reverse cache is needed to 
                            // convert the group values to group data
                            // This is needed because we cannot get this
                            // info from the grid.
                            reverseAggItem = reverseAggDataItem.items[aggValue];

                            if (!reverseAggItem) {
                                reverseAggItem = {
                                    data: rawValue,
                                    items: {}
                                };
                                reverseAggDataItem.items[aggValue] = reverseAggItem;
                            }

                            aggDataItem = aggItem;
                            reverseAggDataItem = reverseAggItem;
                        });

                        if (groupId === 0 &&
                            aggValues.length) {
                            allGroupValues.push(aggValues);
                        }

                        // Add the grouping id cache entry
                        aggItem = aggDataItem.items[groupId];

                        if (!aggItem) {
                            aggItem = {
                                items: {}
                            };
                            aggDataItem.items[groupId] = aggItem;
                        }

                        reverseAggItem = reverseAggDataItem.items[groupId];

                        if (!reverseAggItem) {
                            reverseAggItem = {
                                items: {}
                            };
                            reverseAggDataItem.items[groupId] = reverseAggItem;
                        }

                        aggDataItem = aggItem;
                        reverseAggDataItem = reverseAggItem;

                        return {
                            aggDataItem: aggDataItem,
                            reverseAggDataItem: reverseAggDataItem
                        };
                    }


                    // Initialise the column format functions cache
                    // These functions are used to format values.
                    function initialiseColumnFormatFunctionsCache() {
                        var columnIds = [];

                        // Get the column ids for the group columns
                        if (reportMetadata.rmeta.groups) {
                            columnIds = _.map(reportMetadata.rmeta.groups, function (g) {
                                return _.first(_.keys(g));
                            });
                        }

                        // Get the columns ids of the aggregate columns
                        if (reportMetadata.rmeta.aggs) {
                            columnIds = columnIds.concat(_.keys(reportMetadata.rmeta.aggs));
                        }

                        // Create a format function and cache it
                        _.forEach(columnIds, function (columnId) {
                            var columnFormatFunc = columnFormatFuncCache[columnId];
                            if (!columnFormatFunc) {
                                columnFormatFunc = getColumnFormatFunc(columnId);
                                columnFormatFuncCache[columnId] = columnFormatFunc;
                                columnFormatFuncCache[groupValueColumnIdPrefix + columnId] = columnFormatFunc;
                                columnFormatGroupColumnCache[columnId] = reportMetadata.rcols[columnId];
                                columnFormatGroupColumnCache[groupValueColumnIdPrefix + columnId] = reportMetadata.rcols[columnId];
                            }
                        });

                        addDefaultFormatFunctions(columnFormatFuncCache);
                    }


                    // Add the total value to the caches                    
                    function addTotalValuesToCaches(totals, aggDataItem, reverseAggDataItem, totalCount) {
                        // Add the totals to the leaf entries
                        if (!aggDataItem.totals) {
                            aggDataItem.totals = {};
                        }

                        if (!reverseAggDataItem.totals) {
                            reverseAggDataItem.totals = {};
                        }

                        aggDataItem.totals['aggCount'] = {
                            value: totalCount,
                            type: spEntity.DataType.Int32
                        };
                        reverseAggDataItem.totals['aggCount'] = {
                            value: totalCount,
                            type: spEntity.DataType.Int32
                        };

                        // First key off column id and then key off the aggregate id
                        _.forOwn(totals, function (columnTotals, columnId) {
                            var totalsDataItem = aggDataItem.totals[columnId],
                                aggMetadata = reportMetadata.rmeta.aggs[columnId];

                            if (!totalsDataItem) {
                                totalsDataItem = {};
                                aggDataItem.totals[columnId] = totalsDataItem;
                                reverseAggDataItem.totals[columnId] = totalsDataItem;
                            }

                            // Add the values for the totals
                            _.forEach(columnTotals, function (ct, index) {
                                var aggMetadataItem = aggMetadata[index];

                                if (aggMetadataItem) {
                                    totalsDataItem[aggMetadataItem.style] = {
                                        value: ct.values ? ct.values : ct.value,
                                        type: aggMetadataItem.type
                                    };
                                }
                            });
                        });
                    }


                    // The function maps the rollup metadata to a structure containing nested dictionaries.
                    // This way aggregate data can be retrieved quickly.
                    // aggregateDataCache.items['groupVal1'].items['groupVal2'].items[groupId].totals[colId][aggregateMethod]
                    function initialise() {
                        var aggDataCache = {},
                            reverseAggDataCache = {};

                        grandTotalsCacheItem = {};
                        aggregateDataCache = {};
                        reverseAggregateDataCache = {};
                        groupColumns = [];
                        grandTotalGroupingId = 0;
                        allGroupValues = [];

                        hasRollupData = exports.hasRollupData();

                        if (!hasRollupData) {
                            return;
                        }

                        groupColumns = exports.getGroupColumns();
                        grandTotalGroupingId = Math.pow(2, groupColumns.length) - 1;

                        if (reportMetadata &&
                            reportMetadata.rdata &&
                            reportMetadata.rdata.length > reportRollupSize.value)
                        {
                            spAlertsService.addAlert("The maximum number of groups has been exceeded. The maximum number of groups is 1,000", {
                                severity: spAlertsService.sev.Warning,
                                expires: true
                            });
                        }

                        _.forEach(reportMetadata.rdata, function (rollupRow) {
                            var groupId = rollupRow.map, // get the group id
                                totalCount = rollupRow.total,
                                groupValues = rollupRow.hdrs, // get the group values
                                totals = rollupRow.aggs,
                                aggDataItem = aggDataCache,
                                reverseAggDataItem = reverseAggDataCache,
                                result;

                            // Add all the group values to the caches
                            result = addGroupValuesToCaches(groupValues, groupId, aggDataItem, reverseAggDataItem);

                            aggDataItem = result.aggDataItem;
                            reverseAggDataItem = result.reverseAggDataItem;

                            addTotalValuesToCaches(totals, aggDataItem, reverseAggDataItem, totalCount);

                            // Cache the grand total entry
                            if (groupId === grandTotalGroupingId) {
                                grandTotalsCacheItem = aggDataItem;
                            }
                        });

                        aggregateDataCache = aggDataCache;
                        reverseAggregateDataCache = reverseAggDataCache;

                        initialiseColumnFormatFunctionsCache();
                    }


                    // Add default format functions keyed off type 
                    function addDefaultFormatFunctions(columnFormatFuncCache) {
                        columnFormatFuncCache[spEntity.DataType.Currency] = function (value) {
                            return $filter('spCurrency')(value, currencySymbol, 3, '', '');
                        };

                        columnFormatFuncCache[spEntity.DataType.Decimal] = function (value) {
                            return $filter('spDecimal')(value, 3, '', '');
                        };

                        columnFormatFuncCache[spEntity.DataType.Int32] = function (value) {
                            return $filter('spNumber')(value, '', '', '');
                        };

                        columnFormatFuncCache.RelatedResource = $filter('relatedResource');
                        columnFormatFuncCache.ChoiceRelationship = $filter('relatedResource');
                        columnFormatFuncCache.InlineRelationship = $filter('relatedResource');
                        columnFormatFuncCache.Image = $filter('relatedResource');

                        columnFormatFuncCache[spEntity.DataType.Time] = function (value) {
                            return $filter('spTime')(value, '');
                        };

                        columnFormatFuncCache[spEntity.DataType.Date] = function (value) {
                            return $filter('spDate')(value, '');
                        };

                        columnFormatFuncCache[spEntity.DataType.DateTime] = function (value) {
                            return $filter('spDateTime')(value, '');
                        };                                                    
                    }


                    // Return a format function for the specified column.
                    function getColumnFormatFunc(columnId) {
                        var rcol = reportMetadata.rcols[columnId],
                            valRule,
                            columnFormatFunc = null;

                        // Get the value formatting for the specified column.
                        if (reportMetadata.valrules) {
                            valRule = reportMetadata.valrules[columnId];
                        }

                        columnFormatFunc = spReportService.getColumnFormatFunc(rcol, valRule);
                        return columnFormatFunc;
                    }


                    // Return the total value
                    function getTotalValue(aggDataItem, columnId, aggType) {
                        var result, columnTotals;

                        if (aggDataItem &&
                            aggDataItem.totals) {
                            if (columnId && aggType) {
                                columnTotals = aggDataItem.totals[columnId];

                                if (columnTotals) {
                                    result = columnTotals[aggType];
                                }
                            } else if (!columnId && aggType === 'aggCount') {
                                result = aggDataItem.totals[aggType];
                            }
                        }
                        
                        return result;
                    }


                    // Convert the specified value to a value that can
                    // be used as a key in the dictionary
                    function convertValueToKey(type, value) {
                        var result, valKeys;

                        if (_.isNull(value) ||
                            _.isUndefined(value)) {
                            return null;
                        }

                        switch (type) {
                        case 'ChoiceRelationship':
                        case 'InlineRelationship':
                        case 'UserInlineRelationship':
                        case 'Image':
                            result = 'Ids:' + _.keys(value).sort().join(',');
                            break;
                        default:
                            result = !_.isString(value) ? value.toString().toLowerCase() : value.toLowerCase();
                        }

                        return result;
                    }


                    /**
                    * Returns true if any of the grouped columns is an aggregate, false otherwise.                    
                    * @returns {Bool}. True if any of the grouped columns is an aggregate false otherwise.
                    */
                    exports.haveAggregateGroupColumns = function () {
                        var groupColumns = exports.getGroupColumns();

                        return _.some(groupColumns,
                            function(g) {
                                return _.get(g, 'rcol.aggcol', false);
                            });                        
                    };


                    /**
                    * Format the specified value according to the specified column's column formatting.
                    * @param {object} value. The value to format.
                    * @param {Number|string} columnIdOrType. The column id or type whose formatting to use.
                    * @returns {string} The formatted value.
                    */
                    exports.formatValue = function (value, columnIdOrType) {
                        if (!hasRollupData) {
                            return value;
                        }

                        var rcol = columnFormatGroupColumnCache[columnIdOrType];
                        if (rcol &&
                            rcol.colerr) {                            
                            return rcol.colerr;                            
                        }

                        var columnFormatFunc = columnFormatFuncCache[columnIdOrType];

                        return columnFormatFunc ? columnFormatFunc(value) : value;
                    };


                    /**
                    * Returns the group values that the specified group data correspond to.
                    * These values will be used by the grid to group the data.
                    * @param {array} arrGroupData. The group data.
                    * @returns {array}. The group values
                    */
                    exports.getGroupValues = function (arrGroupData) {
                        var result = [],
                            aggDataItem = aggregateDataCache;

                        if (!hasRollupData ||
                            arrGroupData.length !== groupColumns.length) {
                            return [];
                        }

                        _.forEach(arrGroupData, function (gv, index) {
                            var groupColumn = groupColumns[index],
                                valueKey = convertValueToKey(groupColumn.rcol.type, gv);                            

                            if (aggDataItem &&
                                aggDataItem.items) {
                                aggDataItem = aggDataItem.items[valueKey];
                            }                            

                            result.push(aggDataItem ? aggDataItem.aggValue : null);
                        });

                        return result;
                    };


                    /**
                    * Returns the group data that the specified group values correspond to.
                    * @param {array} arrGroupValues. The group values.
                    * @returns {object}. The group data.
                    */
                    exports.getGroupData = function (arrGroupValues) {
                        var result,
                            aggDataItem = reverseAggregateDataCache;

                        if (!hasRollupData) {
                            return result;
                        }                        

                        _.forEach(arrGroupValues, function (gv) {
                            if (aggDataItem &&
                                aggDataItem.items) {
                                aggDataItem = aggDataItem.items[gv];
                            }

                            result = aggDataItem ? aggDataItem.data : null;
                        });

                        return result;
                    };


                    /**
                    * Returns the grand total value for the specified column and aggregate method.                    
                    * @param {object} columnId. The column id.
                    * @param {string} aggType. The aggregate type. sum,min etc
                    * @returns {object} The grand total value.
                    */
                    exports.getGrandTotalValue = function (columnId, aggType) {
                        if (!hasRollupData) {
                            return null;
                        }

                        return getTotalValue(grandTotalsCacheItem, columnId, aggType);
                    };


                    // Returns the totals agg data item with the 
                    // specified group id.
                    function findTotalsAggDataItem(aggDataItem, groupId) {
                        var totalsAggDataItem;

                        if (!aggDataItem || !aggDataItem.items) {
                            return null;
                        }

                        totalsAggDataItem = aggDataItem.items[groupId];
                        if (!totalsAggDataItem) {
                            _.forOwn(aggDataItem.items, function (adi) {
                                totalsAggDataItem = findTotalsAggDataItem(adi, groupId);
                                if (totalsAggDataItem) {
                                    // Found the item, break out of forOwn
                                    return false;
                                }
                            });
                        }

                        return totalsAggDataItem;
                    }

                    /**
                    * Returns the subtotal value for the specified aggregate row, column and aggregate method.
                    * @param {array} arrGroupValues. The array of group values.
                    * @param {Number} groupDepth. The group depth.
                    * @param {object} columnId. The column id.
                    * @param {string} aggType. The aggregate type. sum,min etc                    
                    * @returns {object} The sub total value.
                    */
                    exports.getSubTotalValue = function (arrGroupValues, groupDepth, columnId, aggType) {
                        if (!hasRollupData) {
                            return null;
                        }

                        var groupId = grandTotalGroupingId >>> (groupDepth + 1); //eslint-disable-line no-bitwise
                        var aggDataItem = reverseAggregateDataCache;
                        var keys = arrGroupValues ? _.clone(arrGroupValues) : [];

                        if (keys.length < groupColumns.length) {
                            // Find the leaf with the specified groupId                            
                            _.forEach(keys, function (k) {
                                if (aggDataItem &&
                                    aggDataItem.items) {
                                    aggDataItem = aggDataItem.items[k];
                                }
                            });

                            aggDataItem = findTotalsAggDataItem(aggDataItem, groupId);
                        } else {
                            keys.push(groupId);

                            // Loop through the group values + the group id
                            // to find the totals entry
                            _.forEach(keys, function (k) {
                                if (aggDataItem &&
                                    aggDataItem.items) {
                                    aggDataItem = aggDataItem.items[k];
                                }
                            });
                        }
                        return getTotalValue(aggDataItem, columnId, aggType);
                    };


                    /**
                    * Returns the group columns.                    
                    * @returns {array}. The group columns.
                    */
                    exports.getGroupColumns = function () {
                        if (!hasRollupData ||
                            !reportMetadata.rmeta.groups) {
                            return [];
                        }

                        return _.map(reportMetadata.rmeta.groups, function (g) {
                            var groupColumnId = _.first(_.keys(g));

                            return {
                                id: groupColumnId,
                                style: g[groupColumnId].style,
                                value: g[groupColumnId].value,
                                rcol: reportMetadata.rcols[groupColumnId],
                                collapsed: g[groupColumnId].collapsed
                            };
                        });                        
                    };

                    /**
                    * Return true or false depending on whether the report has aggregate column.
                    * @returns {bool}. True if the report has aggregate column data, false otherwise.
                    **/
                    exports.hasAggregateColumns = function() {
                        if (!hasRollupData || !reportMetadata.rmeta.aggs) {
                            return false;
                        }

                        return (reportMetadata.rmeta.aggs);
                    };


                    /**
                    * Returns true or false depending on whether the report has rollup data.
                    * @returns {bool}. True if the report has rollup data, false otherwise.
                    */
                    exports.hasRollupData = function () {
                        return !!reportMetadata &&
                               !!reportMetadata.rmeta &&
                               !!reportMetadata.rdata;
                    };


                    /**
                    * Returns an array of the group data values.
                    * @param {object} row. The row data.
                    * @returns {array}. An array of the group data.
                    */
                    exports.getGroupColumnsData = function (row) {
                        if (!hasRollupData) {
                            return [];
                        }

                        return _.map(groupColumns, function (g) {
                            var rowValue = null;

                            if (row &&
                                g.rcol) {
                                rowValue = row.values[g.rcol.ord];
                            }
                            
                            if (!rowValue) {
                                return null;
                            } else if (rowValue.val) {
                                return rowValue.val;
                            } else if (rowValue.vals) {
                                return rowValue.vals;
                            } else {
                                return null;
                            }
                        });
                    };


                    /**
                    * This is an array of an array of group row values.     
                    * This is used to insert hidden rows into the grid to force
                    * groups with no children to be displayed.                    
                    * @returns {array}. An array of the group values.
                    */
                    exports.getAllGroupValues = function () {
                        if (!hasRollupData) {
                            return [];
                        }

                        return allGroupValues;
                    };


                    // Initialise the data dictionary
                    initialise();


                    return exports;
                };
            };
        });
}());