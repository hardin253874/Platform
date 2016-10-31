// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* jshint bitwise:false */

(function () {
    'use strict';

    /**
    * Module containing data grid plugins.    
    * It contains the following plugins:
    * <ul>
    *   <li>GridLayoutPlugin - forces a rebuild of the grid</li>    
    *   <li>GridSortingPlugin - used to perform grid sorting</li>
    *   <li>GridColumnsChangeTrackerPlugin - tracks column changes</li>    
    * </ul>   
    * 
    * @module spDataGridPlugins    
    */
    angular.module('mod.ui.spDataGridPlugins', ['mod.common.spNgUtils'])
        .factory('spDataGridPlugins', function (spNgUtils) {
            var exports = {};


            /**
            * Ensures that the current function is executed in an apply/digest cycle.     
            *
            * @param {object} scope The current scope.
            * @param {function} fn The function to execute.     
            *     
            */
            function safeApply(scope, fn) {
                if (!scope.$root.$$phase) {
                    // digest or apply not in progress
                    scope.$apply(fn);
                } else {
                    // digest or apply already in progress
                    fn();
                }
            }


            // This is an ng-grid plugin used to contain general
            // helper methods.
            exports.GridHelperPlugin = function () {
                var self = this;
                this.grid = null;                
                this.scope = null;
                this.pluginScope = null;


                // This is called by the ng-grid when it initializes.        
                this.init = function (scope, grid, services) {
                    // Store a copy of the parameters so they can be used
                    self.domUtilityService = services.DomUtilityService;
                    self.grid = grid;
                    self.scope = scope.$parent;
                    self.pluginScope = scope;
                };


                // Cleanup plugin
                this.destroy = function () {
                    if (!self) {
                        return;
                    }

                    self = null;
                };


                // Gets the first selectable row data item
                this.getFirstSelectableRowDataItem = function() {
                    if (!self || !self.scope || !self.grid || !self.grid.rowCache) {
                        return null;
                    }                   
                    
                    // Find the first non-hidden row
                    var rowData = _.find(self.grid.rowCache, function (r) {
                        if (!r) {
                            return false;
                        }

                        var row = r.clone || r;                        

                        return row.entity && !row.entity.isHiddenAggValueRow;
                    });
                                        
                    if (rowData && rowData.clone) {                        
                        rowData = rowData.clone;                        
                    }

                    // Return the row data item
                    if (rowData) {
                        return rowData.entity;                       
                    } else {
                        return null;
                    }
                };

                this.getRenderedRange = function () {
                    if (!self || !self.grid || !self.grid.rowFactory) {
                        return null;
                    }

                    return self.grid.rowFactory.renderedRange;
                };

                this.getFirstEmptyGroup = function () {
                    if (!self || !self.scope || !self.grid) {
                        return null;
                    }

                    var firstEmptyGroup = _.find(self.grid.rowFactory.aggCache, function (ar) {
                        return (!ar.aggChildren || ar.aggChildren.length === 0) &&
                            (!ar.children || ar.children.length === 0);
                    });

                    return firstEmptyGroup;
                };


                this.getGroupsCount = function () {
                    if (!self || !self.scope || !self.grid) {
                        return 0;
                    }

                    return self.grid.rowFactory.aggCache.length;
                };            


                this.groupBy = function (field) {
                    if (!self || !self.scope || !self.grid) {
                        return;
                    }

                    var col = self.scope.columns.filter(function (c) {
                        return c.field === field;
                    })[0];

                    if (!col.groupable || !col.field) {
                        return;
                    }
                    if (!self.scope.preventAutoSortOnGroup) {
                        //first sort the column
                        if (!col.sortDirection) {
                            col.sort({ shiftKey: self.scope.configGroups.length > 0 ? true : false });
                        }
                    }

                    var indx = self.scope.configGroups.indexOf(col);
                    if (indx === -1) {
                        col.isGroupedBy = true;
                        self.scope.configGroups.push(col);
                        col.groupIndex = self.scope.configGroups.length;
                    } else {
                        self.scope.removeGroup(indx);
                    }
                    self.grid.$viewport.scrollTop(0);
                };


                // Clears the grouping
                this.clearGrouping = function (updateGrid) {
                    if (!self || !self.scope || !self.grid || !self.domUtilityService) {
                        return;
                    }

                    if (self.scope && self.scope.columns && self.scope.columns.length > 0) {
                        var columns = _.clone(self.scope.columns);
                        _.forEach(columns, function (col) {
                            if (col.groupIndex) {
                                col.isGroupedBy = false;
                                col.groupIndex = 0;
                            }
                            if (col.isAggCol) {
                                _.pull(self.scope.columns, col);
                            }
                        });                        
                    }

                    if (self.scope) {
                        self.scope.configGroups = [];
                    }

                    if (self.grid) {
                        self.grid.config.groups = [];
                        self.grid.fixColumnIndexes();
                    }


                    if (updateGrid) {
                        self.domUtilityService.digest(self.scope);
                        self.scope.adjustScrollLeft(0);
                    }
                };				


                // Build the grid columns
                // This code is pretty much a cut and paste of the ng-grid's column watcher method.                
                // Note: this function does not call Rebuild grid. The caller is responsible for refreshing the grid.
                this.buildColumns = function (columnDefs) {
                    if (!self || !self.scope || !self.grid || !self.domUtilityService) {
                        return;
                    }

                    

                    self.scope.hasUserChangedGridColumnWidths = false;
                    if (!columnDefs) {
                        self.grid.refreshDomSizes();
                        self.grid.buildColumns();
                        return;
                    }

                    self.grid.config.columnDefs = columnDefs;
                    // we have to set this to false in case we want to autogenerate columns with no initial data.
                    self.grid.lateBoundColumns = false;
                    self.scope.columns = [];
                    self.grid.buildColumns();
                    self.grid.eventProvider.assignEvents();                    
                };


                // Refresh the row data
                // This code is pretty much a cut and paste of the ng-grid's data watcher method.
                this.refreshRowData = function (data) {
                    if (!self || !self.scope || !self.grid || !self.domUtilityService) {
                        return;
                    }

                    // make a temporary copy of the data
                    self.grid.data = $.extend([], data);
                    self.grid.rowFactory.fixRowCache();
                    angular.forEach(self.grid.data, function (item, j) {
                        var indx = self.grid.rowMap[j] || j;
                        if (self.grid.rowCache[indx]) {
                            self.grid.rowCache[indx].ensureEntity(item);
                        }
                        self.grid.rowMap[indx] = j;
                    });
                    self.grid.searchProvider.evalFilter();
                    self.grid.configureColumnWidths();
                    self.grid.refreshDomSizes();
                    if (self.grid.config.sortInfo.fields.length > 0) {
                        self.grid.sortColumnsInit();
                        self.scope.$emit('ngGridEventSorted', self.grid.config.sortInfo);
                    }
                    self.scope.$emit("ngGridEventData", self.grid.gridId);
                    self.scope.adjustScrollTop(self.grid.$viewport.scrollTop(), true);
                };


                // Return the minimum number of rows to render
                this.getMinRowsToRender = function () {
                    if (!self || !self.scope || !self.grid) {
                        return 0;
                    }

                    // return the minimum rows + 12 excess rows
                    // 6 before and 6 after. Taken from ng-grid EXCESS_ROWS constant
                    return self.grid.minRowsToRender() + 12;
                };
            };


            // This is an ng-grid plugin that can be used to force
            // a grid layout update using a new rowHeight.
            // This plugin is needed because it appears that the ng-grid
            // caches the rowHeight and builds styles based on it.    
            exports.GridLayoutPlugin = function ($timeout) {
                var self = this;
                this.grid = null;                
                this.scope = null;
                this.timeout = $timeout;                                                          
                this.isAnimationScheduled = false;


                // This is called by the ng-grid when it initializes.        
                this.init = function (scope, grid, services) {
                    // Store a copy of the parameters so they can be used
                    self.domUtilityService = services.DomUtilityService;
                    self.grid = grid;
                    self.scope = scope;                    

                    scope.$on('window.resize', rebuildGridEventHandler);
                    scope.$on('app.layout.postAnimate', rebuildGridEventHandler);
                };


                // Cleanup plugin
                this.destroy = function () {
                    if (!self) {
                        return;
                    }

                    self = null;
                };
               

                // Scope event callback.
                function rebuildGridEventHandler() {
                    if (!self) {
                        return;
                    }

                    // Ensure we call rebuild grid with no parameter
                    self.rebuildGrid();
                }


                // Request animation callback
                function rebuildGridAnimationCallback() {
                    if (!self || !self.scope) {
                        return;
                    }

                    self.isAnimationScheduled = false;

                    safeApply(self.scope, function() {
                        if (!self || !self.scope || !self.grid || !self.domUtilityService) {
                            return;
                        }                                               

                        // Refresh the grid
                        self.domUtilityService.RebuildGrid(self.scope, self.grid);
                    });

                    if (self.scope.model &&
                        self.scope.model.requestedRebuildChangeId &&
                        self.scope.model.requestedRebuildChangeId !== self.scope.model.completedRebuildChangeId) {
                        $timeout(function () {
                            // Set the completed id in a timeout to give other events a
                            // chance to be processed so that we don't unecessarily refresh the grid.
                            if (self &&
                                self.scope &&
                                self.scope.model.requestedRebuildChangeId) {
                                self.scope.model.completedRebuildChangeId = self.scope.model.requestedRebuildChangeId;
                            }
                        });
                    }                    
                }                

                
                // Debounced rebuild grid method
                var rebuildGridDebounced = _.debounce(function () {
                    if (!self) {
                        return;
                    }                                       

                    self.isAnimationScheduled = true;

                    spNgUtils.requestAnimationFrame(rebuildGridAnimationCallback);
                }, 50);                


                // Rebuild the grid
                this.rebuildGrid = function (changeId) {
                    if (!self || !self.scope || !self.scope.model || self.isAnimationScheduled) {
                        return;
                    }

                    if (changeId) {
                        self.scope.model.requestedRebuildChangeId = changeId;
                        self.scope.model.completedRebuildChangeId = null;
                    } else {
                        if (!self.scope.model.requestedRebuildChangeId ||
                            !self.scope.model.completedRebuildChangeId ||
                            self.scope.model.requestedRebuildChangeId !== self.scope.model.completedRebuildChangeId) {
                            // Grid is not ready yet
                            return;
                        }
                    }

                    rebuildGridDebounced();
                };

                function setAllAggregateRowStateImpl(collapsed, depth) {
                    if (!self || !self.scope || !self.grid) {
                        return;
                    }

                    

                    var haveDepth = !_.isUndefined(depth) && !_.isNull(depth);                

                    _.forEach(self.grid.rowFactory.aggCache, function (ar) {
                        if (haveDepth) {
                            if (ar.depth === depth) {
                                ar.setExpand(collapsed);
                            }
                        } else {
                            ar.setExpand(collapsed);
                        }
                    });

                   
                }


                this.getAllAggregateRowState = function() {
                    if (!self || !self.scope || !self.grid) {
                        return;
                    }

                    return _.map(self.grid.rowFactory.aggCache, function (ar) {
                        return {
                            aggIndex: ar.aggIndex,
                            collapsed: ar.collapsed,
                            label: ar.label,
                            rowIndex: ar.rowIndex
                        };
                    });
                };

                // Set the aggregate row state. Used to collapse
                // or expand all aggregate rows.
                this.setAllAggregateRowState = function (collapsed, depth, runImmediate, raiseChangeMessage) {
                    if (!self || !self.scope || !self.grid) {
                        return;
                    }

                    if (runImmediate) {
                        setAllAggregateRowStateImpl(collapsed, depth);
                        if (raiseChangeMessage) {
                            self.scope.$emit('spDataGridAggregateRowStateChanged', self.getAllAggregateRowState());
                        }
                    } else {
                        self.timeout(function () {
                            setAllAggregateRowStateImpl(collapsed, depth);
                            if (raiseChangeMessage) {
                               self.scope.$emit('spDataGridAggregateRowStateChanged', self.getAllAggregateRowState());
                            }
                        }, 100);
                    }
                };

                //Set the existing aggregate row state from Nav item
                this.setExistingAggregateRowStates = function (existingAggregateRow) {
                    if (!self || !self.scope || !self.grid) {
                        return;
                    }

                    //reset current nav item's aggregaterow state                  
                    if (existingAggregateRow) {
                        for (var i = 0; i < existingAggregateRow.length; i++) {
                            self.grid.rowFactory.aggCache[i].setExpand(existingAggregateRow[i].collapsed);
                        }
                    }
                    
                };


                // This method is used to force the grid to update it's rowHeight
                this.updateGridMultiSelect = function (multiSelect) {
                    if (!self || !self.scope || !self.grid || !self.domUtilityService) {
                        return false;
                    }

                    // Update the grid's multi select options
                    self.grid.config.multiSelect = multiSelect;
                    self.scope.$parent.multiSelect = multiSelect;
                    self.scope.selectionProvider.multi = multiSelect;                               

                    return true;
                };

                // This method is used to show/hide the report header
                this.showReportHeader = function (show) {
                    if (!self || !self.scope || !self.grid || !self.domUtilityService) {
                        return;
                    }

                    self.grid.config.showReportHeader = show;
                };

                // This method is used to force the grid to update it's rowHeight
                this.updateGridRowHeight = function (rowHeight, aggregateRowHeight, rowDataHeaderHeight) {
                    if (!self || !self.scope || !self.grid || !self.domUtilityService) {
                        return;
                    }

                        // Update the grid's row height
                    self.scope.$parent.rowHeight = rowHeight;
                    self.grid.config.rowHeight = rowHeight;
                    self.grid.rowFactory.rowHeight = rowHeight;
                    self.grid.rowFactory.rowConfig.rowHeight = rowHeight;
                        
                    if (aggregateRowHeight) {
                        self.scope.$parent.aggregateRowHeight = aggregateRowHeight;
                        self.grid.config.aggregateRowHeight = aggregateRowHeight;
                        self.grid.rowFactory.aggregateRowHeight = aggregateRowHeight;
                        self.grid.rowFactory.rowConfig.aggregateRowHeight = aggregateRowHeight;
                    }

                    if (rowDataHeaderHeight) {
                        self.scope.$parent.rowDataHeaderHeight = rowDataHeaderHeight;
                        self.grid.config.rowDataHeaderHeight = rowDataHeaderHeight;
                        self.grid.rowFactory.rowDataHeaderHeight = rowDataHeaderHeight;
                        self.grid.rowFactory.rowConfig.rowDataHeaderHeight = rowDataHeaderHeight;
                    }                              
                };

                this.updateHeaderColor = function (color) {
                    if (!self || !self.scope) {
                        return;
                    }
                    if (color && color.length > 0)
                        self.grid.config.headerColor = color;
                };

                // This method updates the state of the showRowDataHeader value.
                this.updateShowRowDataHeader = function (showRowDataHeader) {
                    if (!self || !self.scope || !self.grid || !self.domUtilityService) {
                        return;
                    }                                        

                    self.scope.$parent.showRowDataHeader = showRowDataHeader;
                    self.grid.config.showRowDataHeader = showRowDataHeader;
                    self.grid.rowFactory.showRowDataHeader = showRowDataHeader;
                    self.grid.rowFactory.rowConfig.showRowDataHeader = showRowDataHeader;                              
                };


                // This method is used to force the grid to scroll to the specified row index
                this.scrollTo = function (rowIndex) {
                    if (!self || !self.scope || !self.grid) {
                        return;
                    }

                    var scrollPosition = rowIndex * self.grid.config.rowHeight;

                    safeApply(self.scope, function () {
                        if (!self || !self.scope || !self.grid) {
                            return;
                        }

                        // Update the grid's scroll position
                        self.grid.$viewport.scrollTop(scrollPosition);
                        self.scope.adjustScrollTop(scrollPosition, true);                        
                    });
                };                

                // Returns true if the row is hidden, false otherwise
                function isRowHidden(row) {
                    return _.get(row, '_ng_hidden_', false);
                }

                // Gets the offset from the top of the specified aggregate row
                function getAggRowOffset(aggRowIndex, aggRows, rowFactory) {
                    var result = {
                        offset: 0
                    };

                    getAggRowOffsetImpl(aggRowIndex, aggRows, rowFactory, result);

                    return result.offset;
                }

                // Gets the offset from the top of the specified aggregate row
                function getAggRowOffsetImpl(aggRowIndex, aggRows, rowFactory, result) {
                    if (!result) {
                        return true;
                    }

                    if (!result.offset) {
                        result.offset = 0;
                    }

                    var foundAggGroup;

                    _.forEach(aggRows, function(aggRow) {
                        if (aggRow.aggIndex === aggRowIndex) {
                            // We have found the aggRow, so stop
                            foundAggGroup = true;
                            return false;
                        }

                        if (isRowHidden(aggRow)) {
                            // Skip hidden rows
                            return true;
                        }

                        // Add the height of the aggrow
                        result.offset += rowFactory.aggregateRowHeight;

                        // Enumerate children
                        if (!aggRow.collapsed) {
                            if (aggRow.children && aggRow.children.length) {
                                result.offset += _.reject(aggRow.children, isRowHidden).length * rowFactory.rowHeight;
                            }

                            if (aggRow.aggChildren && aggRow.aggChildren.length) {
                                if (getAggRowOffsetImpl(aggRowIndex, aggRow.aggChildren, rowFactory, result)) {
                                    foundAggGroup = true;
                                    return false;
                                }
                            }
                        }

                        return true;
                    });

                    return foundAggGroup;
                }                                        

                // Scroll to the specified group index.
                this.scrollToGroup = function (aggIndex, prevOffsetTop) {
                    var offset, rowFactory, scrollPosition, aggRows;

                    if (!self || !self.scope || !self.grid || !self.grid.rowFactory || !self.grid.rowFactory.aggCache) {
                        return;
                    }

                    rowFactory = self.grid.rowFactory;

                    // Get root aggRows only
                    aggRows = _.reject(rowFactory.aggCache, 'parent');

                    offset = rowFactory.showRowDataHeader ? rowFactory.rowDataHeaderHeight : 0;                    
                    offset += getAggRowOffset(aggIndex, aggRows, rowFactory);
                    
                    scrollPosition = offset - prevOffsetTop;
                    
                    if (!scrollPosition) {
                        return;
                    }

                    safeApply(self.scope, function () {
                        if (!self || !self.scope || !self.grid || !self.grid.$viewport) {
                            return;
                        }
                        
                        // Update the grid's scroll position
                        self.grid.$viewport.scrollTop(scrollPosition);
                        self.scope.adjustScrollTop(scrollPosition, true);                        
                    });
                };

                // This method returns true if the grid is scrolled to the top
                this.isScrolledToTop = function () {
                    if (!self || !self.grid) {
                        return false;
                    }

                    return self.grid.prevScrollTop === 0;
                };                


                // This method returns the current viewport scrolltop
                this.getScrollTop = function () {
                    if (!self || !self.grid || !self.grid.$viewport) {
                        return 0;
                    }

                    return self.grid.$viewport.scrollTop();
                }; 
            };            


            // This is an ng-grid plugin that can be used to sort the grid data.    
            exports.GridSortingPlugin = function () {
                var self = this;
                this.grid = null;
                this.scope = null;                


                // This is called by the ng-grid when it initializes.        
                this.init = function (scope, grid, services) {
                    self.grid = grid;
                    self.scope = scope;                    
                };


                // Destroy this plugin
                this.destroy = function () {
                    if (!self) {
                        return;
                    }

                    self = null;
                };


                // This method is used to force the grid to update it's useExternalSorting setting
                this.updateUseExternalSorting = function (useExternalSorting) {
                    if (!self || !self.scope || !self.grid) {
                        return false;
                    }
                    
                    // Update the grid's useExternalSorting setting
                    self.grid.config.useExternalSorting = useExternalSorting;                    

                    return true;
                };


                // Sort the grid data        
                this.sortData = function (sortInfo) {
                    if (!self || !self.scope || !self.grid || !self.scope.columns || self.scope.columns.length === 0) {
                        return;
                    }

                    safeApply(self.scope, function () {
                        if (!self || !self.scope || !self.grid || !self.scope.columns || self.scope.columns.length === 0) {
                            return;
                        }

                        var sortedColumns = [],
                            existingSortedColumns,
                            existingSortedColumnsInfo,
                            newSortedColumnsInfo;

                        // Find all existing columns with a sort direction, sorted by priority
                        existingSortedColumns = _.sortBy(_.filter(self.scope.columns, function (c) {
                            return c.sortDirection;
                        }), 'sortPriority');

                        existingSortedColumnsInfo = _.map(existingSortedColumns, function (sc) {
                            return {
                                field: sc.field,
                                sortDirection: sc.sortDirection
                            };
                        });

                        if (sortInfo) {
                            // Update the column sort info
                            _.forEach(sortInfo, function (si) {
                                var gridColumn = _.find(self.scope.columns, function (col) {
                                    return (col && col.colDef && col.colDef.spColumnDefinition) && (col.colDef.spColumnDefinition.columnId === si.columnId);
                                });
                                if (gridColumn) {
                                    gridColumn.sortDirection = angular.lowercase(si.sortDirection);
                                    sortedColumns.push(gridColumn);
                                }
                            });
                        }

                        // Find new sorted columns
                        newSortedColumnsInfo = _.map(sortedColumns, function (sc) {
                            return {
                                field: sc.field,
                                sortDirection: sc.sortDirection
                            };
                        });

                        if (angular.equals(existingSortedColumnsInfo, newSortedColumnsInfo)) {
                            // sort is unchanged. return
                            return;
                        }

                        // Clear the sorting for unsorted columns
                        _.forEach(self.scope.columns, function (col) {
                            if (sortedColumns.indexOf(col) === -1) {
                                col.sortDirection = '';
                                col.sortPriority = null;
                            }
                        });

                        self.grid.lastSortedColumns = [];

                        // Either pass an array for multiple columns or a single object
                        self.grid.sortData(sortedColumns.length === 1 ? sortedColumns[0] : sortedColumns, {});
                    });
                };
            };


            // This is an ng-grid plugin that will raise an
            // spNgGridColumnsChanged event when the grid columns are
            // changed.
            exports.GridColumnsChangeTrackerPlugin = function () {
                var self = this;
                this.scope = null;                


                // This is called by the ng-grid when it initializes.        
                this.init = function (scope, grid, services) {
                    self.scope = scope;

                    self.scope.$watch('columns', function (columns) {
                        self.scope.$emit('spDataGridEventColumnsChanged', self.scope.columns);
                    });                    
                };


                // Destroy this plugin
                this.destroy = function () {
                    if (!self) {
                        return;
                    }

                    self = null;
                };
            };


            // This is an ng-grid plugin that replaces the built in selection provider with the one
            // in this plugin. 
            // This plugin must be the first plugin in the list of grid config plugins.
            // This plugin changes the selection provider so that normal shift/ctrl selections            
            // are respected.
            // The default ng-grid selection provider will select/unselect rows when clicked.
            // With this provider the grid will only allow multiple rows to be selected/unselected
            // if the ctrl key is pressed. Also clicking a row will always select it, it will not
            // toggle it's selection.
            exports.SelectionProviderPlugin = function ($parse, $timeout) {
                var self = this;
                this.grid = null;
                this.scope = null;
                this.selectionProvider = null;
                this.timeout = $timeout;


                // This is called by the ng-grid when it initializes.        
                this.init = function (scope, grid, services) {
                    self.grid = grid;
                    self.scope = scope;

                    var gridScope = scope.$parent;
                    // Replace the ng-grid selection provider with the modified one
                    gridScope.selectionProvider = new DataGridSelectionProvider(grid, gridScope, $parse);
                    grid.rowFactory.selectionProvider = gridScope.selectionProvider;
                    self.selectionProvider = gridScope.selectionProvider;
                };


                // Destroy this plugin
                this.destroy = function () {
                    if (!self) {
                        return;
                    }

                    self = null;
                };


                // Select the row at the specified index
                // state {bool} - True to select, false to unselect
                // continueSelection {bool} - True to append to current selection, false to clear                
                this.selectRow = function (rowIndex, state, continueSelection, suppressUpdates) {
                    if (!self) {
                        return;
                    }

                    var isSelected = state ? true : false,
                        rowItem = self.grid.rowCache[rowIndex];

                    if (rowItem) {
                        if (rowItem.clone) {
                            self.selectionProvider.setSelection(rowItem.clone, isSelected, continueSelection, suppressUpdates);
                        }
                        self.selectionProvider.setSelection(rowItem, isSelected, continueSelection, suppressUpdates);
                        self.selectionProvider.lastClickedRow = rowItem;
                        return true;
                    }

                    return false;
                };

                this.clearSelection = function () {
                    if (!self) {
                        return;
                    }

                    var rows = $.grep(self.grid.rowCache, function (r) {
                        return r.selected === true;
                    });

                    self.selectionProvider.clearSelection(rows);
                    self.selectionProvider.lastClickedRow = undefined;
                };

                var focusCellElementDebounced = _.debounce(function (columnCssClass) {
                    if (!self || !self.selectionProvider.lastClickedRow) {
                        return;
                    }

                    self.timeout(function () {                        
                        var elm = self.selectionProvider.lastClickedRow.clone ? self.selectionProvider.lastClickedRow.clone.elm : self.selectionProvider.lastClickedRow.elm;
                        if (!elm) {
                            return;
                        }

                        // Find the first visible input or focusable anchor element
                        var inputToFocusSelector = 'div.rn-inline-edit div.ngCell';
                        if (columnCssClass) {
                            inputToFocusSelector += '.' + columnCssClass;
                        }
                        inputToFocusSelector += ' :input:visible:enabled:first';

                        var aToFocusSelector = 'div.rn-inline-edit div.ngCell';
                        if (columnCssClass) {
                            aToFocusSelector += '.' + columnCssClass;
                        }
                        aToFocusSelector += ' a[href]:visible:enabled:first';

                        var inputToFocus = elm.find(inputToFocusSelector);
                        if (!inputToFocus || !inputToFocus.length) {
                            // Look for any visible anchor tags to focus
                            inputToFocus = elm.find(aToFocusSelector);
                        }
                        
                        if ((!inputToFocus || !inputToFocus.length) && columnCssClass) {
                            // Fallback to first visible input if a column has been specified and no input found
                            inputToFocus = elm.find('div.rn-inline-edit div.ngCell :input:visible:enabled:first');
                            if (!inputToFocus || !inputToFocus.length) {
                                // Look for any visible anchor tags to focus
                                inputToFocus = elm.find('div.rn-inline-edit div.ngCell a[href]:visible:enabled:first');
                            }                                                        
                        }

                        if (!inputToFocus || !inputToFocus.length || inputToFocus.attr('readonly')) {
                            return;
                        }

                        inputToFocus.focus();                       
                        inputToFocus.select();
                    }, 100); 
                }, 20);                

                this.focusCellElement = function (columnCssClass) {
                    focusCellElementDebounced(columnCssClass);
                };

                // This is a copy of the ngSelectionProvider constructor with modifications
                function DataGridSelectionProvider(grid, $scope, $parse) {
                    var self = this;
                    self.multi = grid.config.multiSelect;
                    self.selectedItems = grid.config.selectedItems;
                    self.selectedIndex = grid.config.selectedIndex;
                    self.lastClickedRow = undefined;
                    self.ignoreSelectedItemChanges = false; // flag to prevent circular event loops keeping single-select var in sync
                    self.pKeyParser = $parse(grid.config.primaryKey);
                    self.isMac = navigator.platform.toLowerCase().indexOf('mac') >= 0;

                    $scope.$on('$destroy', function () {
                        if (!self) {
                            return;
                        }

                        self.lastClickedRow = null;
                        self.selectedItems = null;
                        self = null;
                    });


                    // function to manage the selection action of a data item (entity) 
                    self.ChangeSelection = function (rowItem, evt) {
                        if (!self) {
                            return;
                        }

                        // ctrl-click + shift-click multi-selections
                        // up/down key navigation in multi-selections
                        var charCode = evt.which || evt.keyCode;
                        var isUpDownKeyPress = (charCode === 40 || charCode === 38);
                        var contSelect = self.isMac ? evt.metaKey : evt.ctrlKey;

                        if (evt && evt.shiftKey && !evt.keyCode && self.multi && grid.config.enableRowSelection) {
                            if (self.lastClickedRow) {
                                var rowsArr;
                                if ($scope.configGroups.length > 0) {
                                    rowsArr = grid.rowFactory.parsedData.filter(function (row) {
                                        return !row.isAggRow;
                                    });
                                }
                                else {
                                    rowsArr = grid.filteredRows;
                                }

                                var thisIndx = rowItem.rowIndex;
                                var prevIndx = self.lastClickedRowIndex;

                                if (thisIndx === prevIndx) {
                                    return false;
                                }

                                if (thisIndx < prevIndx) {
                                    thisIndx = thisIndx ^ prevIndx;
                                    prevIndx = thisIndx ^ prevIndx;
                                    thisIndx = thisIndx ^ prevIndx;
                                    thisIndx--;
                                }
                                else {
                                    prevIndx++;
                                }

                                var rows = [];
                                for (; prevIndx <= thisIndx; prevIndx++) {
                                    rows.push(rowsArr[prevIndx]);
                                }

                                if (rows[rows.length - 1] && rows[rows.length - 1].beforeSelectionChange(rows, evt)) {
                                    for (var i = 0; i < rows.length; i++) {
                                        var ri = rows[i];
                                        var selectionState = ri.selected;
                                        ri.selected = !selectionState;
                                        if (ri.clone) {
                                            ri.clone.selected = ri.selected;
                                        }
                                        var index = self.selectedItems.indexOf(ri.entity);
                                        if (index === -1) {
                                            self.selectedItems.push(ri.entity);
                                        }
                                        else {
                                            self.selectedItems.splice(index, 1);
                                        }
                                    }
                                    rows[rows.length - 1].afterSelectionChange(rows, evt);
                                }
                                self.lastClickedRow = rowItem;
                                self.lastClickedRowIndex = rowItem.rowIndex;

                                return true;
                            }
                        }
                        else if (!self.multi) {
                            if (self.lastClickedRow === rowItem) {
                                self.setSelection(self.lastClickedRow, contSelect ? !rowItem.selected : true);
                            } else {
                                if (self.lastClickedRow) {
                                    self.setSelection(self.lastClickedRow, false);
                                }
                                self.setSelection(rowItem, contSelect ? !rowItem.selected : true);
                            }
                        }
                        else if (!evt.keyCode || isUpDownKeyPress && !grid.config.selectWithCheckboxOnly) {
                            self.setSelection(rowItem, contSelect ? !rowItem.selected : true, contSelect);
                        }
                        self.lastClickedRow = rowItem;
                        self.lastClickedRowIndex = rowItem.rowIndex;
                        return true;
                    };

                    self.getSelection = function (entity) {
                        if (!self) {
                            return;
                        }

                        var isSelected = false;
                        if (grid.config.primaryKey) {
                            var val = self.pKeyParser(entity);
                            angular.forEach(self.selectedItems, function (c) {
                                if (val === self.pKeyParser(c)) {
                                    isSelected = true;
                                }
                            });
                        }
                        else {
                            isSelected = self.selectedItems.indexOf(entity) !== -1;
                        }
                        return isSelected;
                    };

                    // just call this func and hand it the rowItem you want to select (or de-select)    
                    self.setSelection = function (rowItem, isSelected, ctrlKey, suppressUpdates) {
                        if (!self) {
                            return;
                        }

                        if (grid.config.enableRowSelection) {
                            if (!isSelected) {
                                var indx = self.selectedItems.indexOf(rowItem.entity);
                                if (indx !== -1) {
                                    self.selectedItems.splice(indx, 1);
                                }
                            }
                            else {
                                if ((!self.multi || (self.multi && !ctrlKey)) && self.selectedItems.length > 0) {
                                    self.toggleSelectAll(false, true);
                                }
                                if (self.selectedItems.indexOf(rowItem.entity) === -1) {
                                    self.selectedItems.push(rowItem.entity);
                                }
                            }
                            rowItem.selected = isSelected;
                            if (rowItem.orig) {
                                rowItem.orig.selected = isSelected;
                            }
                            if (rowItem.clone) {
                                rowItem.clone.selected = isSelected;
                            }
                            if (!suppressUpdates) {
                                rowItem.afterSelectionChange(rowItem);
                            }
                        }
                    };

                    // @return - boolean indicating if all items are selected or not
                    // @val - boolean indicating whether to select all/de-select all
                    self.toggleSelectAll = function (checkAll, bypass, selectFiltered) {
                        if (!self) {
                            return;
                        }

                        var rows = selectFiltered ? grid.filteredRows : grid.rowCache;
                        if (bypass || grid.config.beforeSelectionChange(rows, checkAll)) {
                            var selectedlength = self.selectedItems.length;
                            if (selectedlength > 0) {
                                self.selectedItems.length = 0;
                            }
                            for (var i = 0; i < rows.length; i++) {
                                rows[i].selected = checkAll;
                                if (rows[i].clone) {
                                    rows[i].clone.selected = checkAll;
                                }
                                if (checkAll) {
                                    self.selectedItems.push(rows[i].entity);
                                }
                            }
                            if (!bypass) {
                                grid.config.afterSelectionChange(rows, checkAll);
                            }
                        }
                    };

                    self.clearSelection = function (rows) {

                        if (!rows) {
                            return;
                        }

                        for (var i = 0; i < rows.length; i++) {
                            var row = rows[i];
                            if (row) {
                                row.selected = false;
                                if (row.orig) { row.orig.selected = false; }
                                if (row.clone) { row.clone.selected = false; }
                            }
                        }

                        self.selectedItems.length = 0;
                    };
                }
            };


            // This is an ng-grid plugin that will cleanup the 
            // grid when it is destroyed.
            exports.GridCleanupPlugin = function (templateCache) {
                var self = this;
                this.scope = null;
                this.grid = null;                
                this.templateCache = templateCache;

                // This is called by the ng-grid when it initializes.        
                this.init = function (scope, grid, services) {
                    self.scope = scope;
                    self.grid = grid;                    
                };

                // Cleanup the grid
                this.destroy = function () {                    
                    if (!self ||                        
                        !self.grid ||
                        !self.templateCache) {
                        return;
                    }

                    var templates = ['rowTemplate', 'aggregateTemplate', 'headerRowTemplate', 'checkboxCellTemplate', 'checkboxHeaderTemplate', 'menuTemplate', 'footerTemplate', 'rowDataHeaderTemplate'];                    

                    if (self.grid.gridId) {
                        // Remove templates from cache
                        _.forEach(templates, function (t) {
                            var tKey = self.grid.gridId + t + '.html';
                            self.templateCache.remove(tKey);
                        });
                    }

                    self.grid.config.plugins = [];                    
                    self = null;
                };
            };            

            return exports;
        });
}());