// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module containing data grid utility functions.       
    * 
    * @module spDataGridUtils    
    */
    angular.module('mod.common.ui.spDataGridUtils', [])
        .factory('spDataGridUtils', function () {
            var exports = {};

            /**
            * Updates the sort info array to contain sorting info for the specified column
            * and direction.
            * The column being sorted will become the primary sort column. i.e. will become
            * the first element of the sortInfoArr.
            *
            * @param {array} sortInfoArr The array of column sort info. See spDataGrid for more information.
            * @param {number} columnId The id of the column to sort by.
            * @param {string} direction The direction to sort by, can be either 'asc', 'desc', 'toggle'            
            * @param {array} groupByColumnIds The array of group by column ids.
            * @returns {bool} True if the sort info was changed, otherwise false.                                    
            */
            exports.updateColumnSortInfo = function (sortInfoArr, columnId, direction, groupByColumnIds) {
                var columnSortInfo,
                    sortInfoIndex,
                    sortInfoChanged = false,
                    isGroupColumn = false,
                    countGroupColumns = !groupByColumnIds ? 0 : groupByColumnIds.length;

                if (!columnId) {
                    return false;
                }

                // Check if the sort info array has sort entries for the grouped columns
                // If not create and add them to the start of the array
                // in the same order as the group by columns
                _.forEach(groupByColumnIds, function (gId, gIndex) {
                    var c, cIndex = _.findIndex(sortInfoArr, function (si) {
                        return si.columnId === gId;
                    });

                    if (gId === columnId) {
                        isGroupColumn = true;
                    }

                    if (cIndex !== gIndex) {
                        if (cIndex === -1) {
                            // Sort info for the specified grouped column
                            // does not exist
                            sortInfoArr.splice(gIndex, 0, {
                                columnId: gId,
                                sortDirection: 'asc'
                            });
                        } else {
                            c = sortInfoArr[cIndex];
                            sortInfoArr.splice(cIndex, 1);
                            sortInfoArr.splice(gIndex, 0, c);
                        }
                    }                    
                });                

                // Find the sort info for the specified column
                columnSortInfo = _.find(sortInfoArr, function (si) {
                    return si.columnId === columnId;
                });
                var changeDirection = true;
                if (!isGroupColumn) {
                    if (!columnSortInfo) {
                        // There is not sort info for this column
                        // so create one and add it to the front of the array
                        columnSortInfo = {
                            columnId: columnId,
                            sortDirection: ''
                        };                        
                        // Add it after the last grouped column or at the beginning
                        // if there are no grouped columns
                        sortInfoArr.splice(countGroupColumns, 0, columnSortInfo);
                        sortInfoChanged = true;
                    } else {
                        // Found an existing sort info for this column
                        // Move it to the front
                        sortInfoIndex = sortInfoArr.indexOf(columnSortInfo);
                        if (sortInfoIndex !== 0) {
                            sortInfoArr.splice(sortInfoIndex, 1);                            
                            // Add it after the last grouped column or at the beginning
                            // if there are no grouped columns
                            sortInfoArr.splice(countGroupColumns, 0, columnSortInfo);
                            sortInfoChanged = true;
                            // from bug 23984, the epected behaviour is that If we first sort by Column A and then sort by Column B, 
                            // if we click on Column A again then Column A should return to the top of the stack but it should not affect the sort order
                            changeDirection = false;
                        }
                    }
                }

                // Set the direction based on the direction parameter
                // if current column exists in report sort list but not top one, move it to first of sort list and do not change the direction
                if (changeDirection) {
                    switch (direction) {
                        case 'asc':
                        case 'desc':
                            if (columnSortInfo.sortDirection !== direction) {
                                columnSortInfo.sortDirection = direction;
                                sortInfoChanged = true;
                            }
                            break;
                        case 'toggle':
                            columnSortInfo.sortDirection = columnSortInfo.sortDirection === 'asc' ? 'desc' : 'asc';
                            sortInfoChanged = true;
                            break;
                    }
                }
                

                return sortInfoChanged;
            };


            /**
            * Removes sorting info for the specified column from the specified sorting info array.            
            *
            * @param {array} sortInfoArr The array of column sort info. See spDataGrid for more information.
            * @param {number} columnId The id of the column to remove.            
            * @returns {bool} True if the sort info was changed, otherwise false.
            *          
            */
            exports.removeColumnSortInfo = function (sortInfoArr, columnId) {
                var columnSortInfo, sortInfoIndex, sortInfoChanged = false;

                if (!columnId) {
                    return false;
                }

                // Find the sort info for the specified column
                columnSortInfo = _.find(sortInfoArr, function (si) {
                    return si.columnId === columnId;
                });

                if (columnSortInfo) {
                    // Found an existing sort info for this column
                    // Remove it
                    sortInfoIndex = sortInfoArr.indexOf(columnSortInfo);
                    sortInfoArr.splice(sortInfoIndex, 1);
                    sortInfoChanged = true;
                }

                return sortInfoChanged;
            };


            /**
            * Gets the specified value as a percentage.  
            *
            * @param {object} value The value to format.
            * @param {string} type The type of the value.            
            * @param {object} bounds The range bounds.   
            * @returns {number} The value as a percentage.            
            */
            exports.getValueAsPercentage = function (value, type, bounds) {
                var percentage = 0,
                    numberValue,
                    dateTimeValueMs,
                    dateTimeLowerMs,
                    dateTimeUpperMs;

                if (angular.isUndefined(value) ||
                    !type ||
                    !bounds ||
                    angular.isUndefined(bounds.lower) ||
                    angular.isUndefined(bounds.upper) ||
                    bounds.lower === bounds.upper) {
                    return -1;
                }

                switch (type) {
                    case spEntity.DataType.Int32:
                    case spEntity.DataType.Decimal:
                    case spEntity.DataType.Currency:
                        if (!_.isNumber(value)) {
                            numberValue = Number(value);
                        } else {
                            numberValue = value;
                        }

                        percentage = ((numberValue - bounds.lower) * 100.0) / (bounds.upper - bounds.lower);
                        break;
                    case spEntity.DataType.Date:
                    case spEntity.DataType.Time:
                    case spEntity.DataType.DateTime:
                        if (!_.isDate(value)) {
                            dateTimeValueMs = (new Date(value)).getTime();
                        } else {
                            dateTimeValueMs = value.getTime();
                        }

                        dateTimeLowerMs = (new Date(bounds.lower)).getTime();
                        dateTimeUpperMs = (new Date(bounds.upper)).getTime();

                        percentage = ((dateTimeValueMs - dateTimeLowerMs) * 100.0) / (dateTimeUpperMs - dateTimeLowerMs);
                        break;
                }                

                return percentage;
            };

            return exports;
        });
}());