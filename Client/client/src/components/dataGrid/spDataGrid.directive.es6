// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global $, _, console, angular, sp, spUtils, spEntity */

(function () {
    'use strict';

    /**
     * Module implementing a data grid.
     * Displays a data grid.
     *
     * @module spDataGrid
     * @example

     Using the spDataGrid:

     &lt;sp-data-grid grid-options="gridOptions"&gt;&lt;/sp-data-grid&gt

     where gridOptions is available on the controller with the following properties:
     - rowData - {array}. The row data to display in the grid. Each array element is expected to be an object with a <b>cells</b> array.
     - rowData[].cells - {array}. The cells for the row. cells is any array of objects with a <b>value</b> or <b>formattingIndex</b> field.
     - rowData[].cells.value - {object}. The value to display in the row cell.
     - rowData[].cells.formattingIndex - {number}. The formatting for this cell. This is an index into the columnDefinitions.cellFormatting array.

     - columnDefinitions - {array}. The column definitions for the grid.
     - columnDefinitions[].columnId - {number}. The id of this column
     - columnDefinitions[].cellIndex - {number}. The data this column is displaying. This is an index into the rowData[].cells array.
     - columnDefinitions[].displayName - {string}. The display name for the column.
     - columnDefinitions[].hideValue - {boolean}. True to hide the value for cells in this column, false otherwise. Default false.
     - columnDefinitions[].visible - {boolean}. True if this column is visible, false otherwise. Default true.
     - columnDefinitions[].imageScaleId - {number}. The id of the image scaling entity id. Only used when this column is displaying images/icons.
     - columnDefinitions[].imageSizeId - {number}. The id of the image size entity id. Only used when this column is displaying images/icons.
     - columnDefinitions[].imageWidth - {number}. The width of the images shown in this column. Only used when this column is displaying images/icons.
     - columnDefinitions[].imageHeight - {number}. The height of the images shown in this column. Only used when this column is displaying images/icons.
     - columnDefinitions[].contextMenu - {object}. The context menu config. See the spContextMenu for more information.
     - columnDefinitions[].cellFilterFunc - {function}. The function to use to filter on the cell ('currency', 'date', etc..).
     - columnDefinitions[].type - {string}. The column data type. e.g. String, Int32, DateTime
     - columnDefinitions[].lines - {number}. The number of lines to display.
     - columnDefinitions[].alignment - {string}. The alignment of cell to display.
     - columnDefinitions[].cellFormatting - {array|dictionary}. The cell formatting definitions
     - cellFormatting[].backgroundColor - {object}. The cell background color. An object with r,g,b,a fields.
     - cellFormatting[].foregroundColor - {object}. The cell foreground color. An object with r,g,b,a fields.
     - cellFormatting[].bounds - Used when range formatting is specified.
     - lower - {object}. The lower value. A number when the column type is numeric, a date time string for dates and times.
     - upper - {object}. The upper value. A number when the column type is numeric, a date time string for dates and times.
     - cellFormatting[].imageId - {number}. The id of the image entity to display.
     - cellFormatting[].alignment - {string}. Left, Centre, Right.
     - columnDefinitions[].totals - {array}. Array of totals for this column
     - totals[].id - {string}. The id of this total. e.g. sum, min etc
     - totals[].displayName - {string}. The display name of the total.

     - multiSelect - {boolean}. True to select many items, false otherwise.

     - useExternalSorting - {boolean}. True to use external sorting, false to use the grid's internal sorting.

     - selectedItems - {array}. The array of selected items.

     - sortInfo - {array}. Array of sorted columns.
     - sortInfo[].columnId - {number}. Id of sorted column.
     - sortInfo[].sortDirection - {string}. Sort order 'asc' or 'desc'.

     - aggregateInfo - {object}. Aggregate info
     - aggregateInfo.groupedColumns - {array}. Array of grouped columns
     - groupedColumns[].columnId - {number}. Id of grouped column
     - groupedColumns[].collapsed - {bool}. True if the group is collapsed, false otherwise
     - aggregateInfo.contextMenu - {object}. The context menu config for aggregate columns. See the spContextMenu for more information.
     - aggregateInfo.showSubTotals - {bool}. True to show sub totals. These appear in the aggregate headers.
     - aggregateInfo.showGrandTotals - {bool}. True to show grand totals. This appears as the first row in the grid.
     - aggregateInfo.showCount
     - aggregateInfo.showAggregateLabel
     - aggregateInfo.aggregateData - {spReportAggregateDataManager}.

     - moreDataAvailable - {bool}. True if more data is available, false otherwise

     = touchEnabled - {boo}. True if touch support is to be enabled

     This control emits the following events:
     - spDataGridEventDataRequested - This event is raised when the grid is scrolled to the bottom. It is used to load data on demand.
     - spDataGridEventGridDoubleClicked - This event is raised when the grid is double clicked.
     - spDataGridEventGridSorted - This event is raised when the grid is sorted.
     - spDataGridEventSelectionChanged - This event is raised when a grid row is selected.
     *
     * @example
     Sample gridOptions that displays a grid with 3 columns and 2 rows of data.

     - The last column displays images.
     - Only row 0 has cell formatting.
     - Row[0].Cell[0] has progress formatting.
     - Row[0].Cell[1] has highlight formatting.
     - Row[0].Cell[2] has image formatting.

     var gridOptions =
     {
         rowData: [
             // The row data with indexes into the column formatting
             {
                 cells: [
                     {
                         value: 1,
                         formattingIndex: 0
                     },
                     {
                         value: 'Name 1',
                         formattingIndex: 0
                     },
                     {
                         value: 'Description 1',
                         formattingIndex: 0
                     }
                 ]
             },
             {
                 cells: [
                     {
                         value: 2
                     },
                     {
                         value: 'Name 2'
                     },
                     {
                         value: 'Description 2'
                     }
                 ]
             },
         ],
         columnDefinitions: [
             {
                 cellIndex: 0,
                 displayName: 'Id',
                 type: 'Int32',
                 cellFormatting: [
                     // Progress formatting
                     {
                         backgroundColor: {
                             r: 0,
                             g: 255,
                             b: 0,
                             a: 1
                         },
                         foregroundColor: {
                             r: 255,
                             g: 255,
                             b: 255,
                             a: 1
                         },
                         bounds: {
                             lower: 0,
                             upper: 100
                          }
                     }
                 ]
             },
             {
                 cellIndex: 1,
                 displayName: 'Name',
                 cellFormatting: [
                     // Highlight formatting
                     {
                         backgroundColor: {
                             r: 0,
                             g: 255,
                             b: 0,
                             a: 1
                         },
                         foregroundColor: {
                             r: 255,
                             g: 255,
                             b: 255,
                             a: 1
                         }
                     }
                 ]
             },
             {
                 // This cell is displaying images.
                 cellIndex: 2,
                 displayName: 'Description',
                 imageScaleId: 111,
                 imageSizeId: 222,
                 imageWidth: 30,
                 imageHeight: 30,
                 cellFormatting: [
                     // Image formatting
                     {
                         imageId: 12345
                     }
                 ]
             }]
     };
     */
    angular.module('mod.common.ui.spDataGrid')
        .directive('spDataGrid', function () {
            return {
                restrict: 'E',
                templateUrl: 'dataGrid/spDataGrid.tpl.html',
                replace: false,
                controller: DataGridController,
                scope: true

            };
        });

    /* @ngInject */
    function DataGridController($scope, $element, $attrs, $templateCache, $q,
                                spDataGridPlugins, spDataGridUtils, spXsrf, spContextMenuService, spActionsService,
                                $parse, reportBuilderService, $document, $timeout, spImageViewerDialog, spNavService,
                                spThemeService, spMobileContext, $compile, $window) {
        
        $scope.debug = true;

        // ReSharper disable InconsistentNaming
        var DEFAULT_ROWHEIGHT = isTouchDevice() ? 46 : 28;
        var DEFAULT_LINEHEIGHT = 20;
        var AGGREGATE_ROW_LINEHEIGHT = 20;
        var IMAGE_BASE_URL = '/spapi/data/v1/image/thumbnail/';
        // ReSharper restore InconsistentNaming

        var body = $document.find('body');
        var firstEmptyGroup;
        var firstEmptyGroupParentGroups = {};
        var searchText = '';
        let selectedColumnCssClass;
        
        var debouncedSearch = _.debounce(search, 500);

        // Create a debounced method to raise grid event data events
        var onGridEventData = _.debounce(function () {
            $scope.$emit('spDataGridEventData', null);
            $timeout(function () {
                var aggRow;

                if (!$scope.gridHelperPlugin) {
                    return;
                }

                // Find the first group with no data
                firstEmptyGroupParentGroups = {};
                firstEmptyGroup = $scope.gridHelperPlugin.getFirstEmptyGroup();
                if (firstEmptyGroup) {
                    aggRow = firstEmptyGroup;
                    aggRow = aggRow.orig || aggRow;
                    aggRow = aggRow.parent;

                    while (aggRow) {
                        aggRow = aggRow.orig || aggRow;

                        firstEmptyGroupParentGroups[aggRow.aggIndex] = aggRow;

                        aggRow = aggRow.parent;
                    }
                }
            });
        }, 100);

        // Create a debounced method to raise update grid messages (This one has less of a delay.)
        var raiseUpdateGridMessageDebouncedFast = _.debounce(function () {
            $scope.$broadcast($scope.updateGridMsg);
        }, 50);

        // Create a debounced method to raise update grid messages
        var raiseUpdateGridMessageDebounced = _.debounce(function () {
            raiseUpdateGridMessageDebouncedFast();                      // call the fast on to ensure that fast an slow debounces are debounced together.
        }, 300);

        // Create a debounced method to handle messages
        var updateGroupsStateDebounced = _.debounce(function () {
            if (!$scope.gridLayoutPlugin ||
                (!sp.result($scope, 'dataGridOptions.aggregateInfo.groupedColumns.length') && !sp.result($scope, 'dataGridOptions.existingAggregateRows.length'))) {
                return;
            }

            $timeout(function () {
                updateAggregateRowDefaultStates();
                updateAggregateRowExistingStates();
                // Emitted so that report can scroll to a group if required
                $scope.$emit('spDataGridGroupsStateChanged');
            });
        }, 200);

        $scope.dataGridOptions = $scope.$eval($attrs.gridOptions);
        $scope.dataGridOptions.dataGrid = {};
        $scope.updateGridMsg = 'spDataGridUpdate';

        $scope.nav = spNavService;
        $scope.themeLoaded = false;
        $scope.consoleThemeModel = {
            consoleTheme: null,
            headerStyle: {}
        };

        $scope.DEFAULT_CELLPADDING = 3;
        $scope.PROGRESS_BAR_HEIGHT = '20px';

        $scope.selectionProviderPlugin = new spDataGridPlugins.SelectionProviderPlugin($parse, $timeout);
        $scope.gridLayoutPlugin = new spDataGridPlugins.GridLayoutPlugin($timeout);
        $scope.gridSortingPlugin = new spDataGridPlugins.GridSortingPlugin();
        $scope.gridColumnsChangeTrackerPlugin = new spDataGridPlugins.GridColumnsChangeTrackerPlugin();
        $scope.gridHelperPlugin = new spDataGridPlugins.GridHelperPlugin();
        $scope.gridCleanupPlugin = new spDataGridPlugins.GridCleanupPlugin($templateCache);        

        $scope.model = {
            // The row data. This points to dataGridOptions.rowData
            rowData: [],
            // The column definitions. This is an array of columnDefs as required
            // by the ng-grid
            columnDefinitions: [],
            // Sort info
            sortInfo: [],
            aggregateInfo: {},
            hideReportHeader: $scope.dataGridOptions.hideReportHeader,
            style: $scope.dataGridOptions.reportStyle,
            // The angular ng-grid options
            // Note: the data and columnDefs are arrays not strings.
            // This is so that we can control when the updates happen to improve grid performance.
            ngGridOptions: {
                data: [],
                columnDefs: [],
                rowHeight: DEFAULT_ROWHEIGHT,
                aggregateRowHeight: DEFAULT_ROWHEIGHT,
                headerRowHeight: DEFAULT_ROWHEIGHT,
                plugins: [$scope.selectionProviderPlugin, $scope.gridLayoutPlugin, $scope.gridSortingPlugin, $scope.gridColumnsChangeTrackerPlugin, $scope.gridHelperPlugin, $scope.gridCleanupPlugin],
                selectedItems: [],
                rowTemplate: $templateCache.get(isTouchDevice() ? 'dataGrid/rowTemplateTouch.tpl.html' : 'dataGrid/rowTemplate.tpl.html'),
                aggregateTemplate: $templateCache.get('dataGrid/aggregateTemplate.tpl.html'),
                enableColumnResize: true,
                multiSelect: $scope.dataGridOptions.multiSelect || false,
                beforeSelectionChange: onBeforeSelectionChange,
                afterSelectionChange: onAfterSelectionChange,
                rowDataHeaderTemplate: $templateCache.get('dataGrid/rowDataHeaderTemplate.tpl.html'),
                showRowDataHeader: $scope.dataGridOptions.aggregateInfo ? $scope.dataGridOptions.aggregateInfo.showGrandTotals : false,
                forceSyncScrolling: true,
                groupsCollapsedByDefault: false,
                preventAutoSortOnGroup: true,
                footerTemplate: $templateCache.get('dataGrid/footerTemplate.tpl.html'),
                menuTemplate: $templateCache.get('dataGrid/menuTemplate.tpl.html'),
                suppressRebuildOnColumnUpdate: true,
                suppressAlternatingRowStyles: true,
                headerColor: 'transparent',
                //noKeyboardNavigation: true
            }
        };

        $scope.contextMenu = [];

        $scope.dragOptions = {
            onDragStart: function () {

            },
            onDragEnd: function () {

            }
        };

        $scope.dropOptions = {
            onAllowDrop: function (source, target, dragData) {
                if (!target || !$scope.dataGridOptions.isEditMode) {
                    return false;
                }

                var t = $(target);              
                return true;
            },
            onDragOver: function (event, source, target, dragData, dropData) {
                if (!target || !$scope.dataGridOptions.isEditMode) {
                    return false;
                }

                var t = $(target);

                var jTarget = t.closest('.ngHeaderText');
                
                if (jTarget && jTarget.length > 0) {
                    //drop on report header
                    jTarget.css('background-color', '#D9E4EE');
                    if (dragData.colDef) {
                        jTarget.css('border-left-style', 'solid');
                        jTarget.css('border-left-color', 'blue');
                    } else {
                        jTarget.css('border-right-style', 'solid');
                        jTarget.css('border-right-color', 'blue');
                    }
                } else {
                    //drop on report content
                    showInsertionIndicatorReportColumn(event, target, dragData, dropData);
                }
                return true;

            },
            onDragLeave: function (event, source, target, dragData) {
                if (!target || !$scope.dataGridOptions.isEditMode) {
                    return false;
                }

                var t = $(target);

                var jTarget = t.closest('.ngHeaderText');

                if (jTarget && jTarget.length > 0) {
                    jTarget.css('background-color', 'transparent');
                    if (dragData.colDef) {
                        jTarget.css('border-left-style', 'none');
                        jTarget.css('border-left-color', 'transparent');
                    } else {
                        jTarget.css('border-right-style', 'none');
                        jTarget.css('border-right-color', 'transparent');
                    }
                }
                return true;
            },
            onDrop: function (event, source, target, dragData, dropData) {               
                if (!target || !$scope.dataGridOptions.isEditMode) {
                    return;
                }
                var t = $(target);
                

                var jTarget = t.closest('.ngHeaderText');

                if (jTarget && jTarget.length > 0) {
                    jTarget.css('background-color', 'transparent');
                    jTarget.css('border-right-style', 'none');
                    jTarget.css('border-right-color', 'transparent');
                }

                hideInsertIndicator();
                $scope.onDrop(dragData, dropData);

                return;
            }
        };


        function showInsertionIndicatorReportColumn(event, target, dragData, dropData) {
            //find parent gridelement from current cell
            var gridElement = findGridElement(target);

            var clientRect = target.getBoundingClientRect();
            var gridRect = gridElement ? gridElement.getBoundingClientRect() : target.getBoundingClientRect();
            $scope.insertIndicatorDropData = dropData;
            //make a fake report column rect which uses report grid's top, current cell's left, report grid's height and current cell's width
            positionInsertIndicator(gridRect.top, clientRect.left, gridRect.height, target.clientWidth, true);

        }

        //find the parent report grid element
        function findGridElement(cell)
        {
            if (!cell.parentNode)
                return null;
            else if (cell.parentNode && cell.parentNode.tagName === "SP-DATA-GRID")
                return cell.parentNode;
            else
                return findGridElement(cell.parentNode);
        }

        var insertIndicatorElement;
        var insertIndicatorScope;
        $scope.insertIndicatorDropData = null;
        $scope.insertIndicatorDropOptions = {                                
            onDrop: function (event, source, target, dragData, dropData) {
                $scope.onDrop(dragData, $scope.insertIndicatorDropData);
                hideInsertIndicator();
            }
        };
        // Create the insert indicator.
        function createInsertIndicator() {
            if (insertIndicatorElement) {
                return;
            }

            insertIndicatorScope = $scope.$new();

            $scope.$apply(function () {
                insertIndicatorElement = $compile('<div class="navInsertionIndicator" sp-droppable="insertIndicatorDropOptions" sp-droppable-data="insertIndicatorDropData" />')(insertIndicatorScope);
                if (insertIndicatorElement) {
                    body.append(insertIndicatorElement);
                    insertIndicatorElement.hide();
                }
            });
        }


        // Destroy the insert indicator.
        function destroyInsertIndicator() {
           
            if (insertIndicatorElement) {
                insertIndicatorElement.remove();
                insertIndicatorElement = null;
            }

            if (insertIndicatorScope) {
                insertIndicatorScope.$destroy();
                insertIndicatorScope = null;
            }
        }

        // Hides the insert indicator.
        function hideInsertIndicator() {           
            if (!insertIndicatorElement) {
                return;
            }

            insertIndicatorElement.hide();
        }


        // Shows the insert indicator.
        function showInsertIndicator() {
            if (!insertIndicatorElement) {
                return;
            }

            insertIndicatorElement.show();
        }

        // Positions the insert indicator.
        function positionInsertIndicator(top, left, height, width, isBorder) {
            var scrollX, scrollY;

            if (!_.isNumber(top) || !_.isNumber(left) || !_.isNumber(height) || !_.isNumber(width)) {
                return;
            }

            if (!insertIndicatorElement) {
                createInsertIndicator();
            }

            if (isBorder) {
                insertIndicatorElement.removeClass('navInsertionIndicatorLine');
                insertIndicatorElement.addClass('navInsertionIndicatorRightBorderLine');
            } else {
                insertIndicatorElement.removeClass('navInsertionIndicatorRightBorderLine');
                insertIndicatorElement.addClass('navInsertionIndicatorLine');
            }

            scrollX = $window.scrollX || 0;
            scrollY = $window.scrollY || 0;

            insertIndicatorElement.css({
                width: width,
                height: height,
                top: top + scrollY,
                left: left + scrollX
            }).show();
        }

        $scope.getReportStyle = function () {
            try {
                if (!$scope.dataGridOptions.reportStyle || $scope.dataGridOptions.reportStyle === '' || $scope.dataGridOptions.reportStyle === 'Default' || $scope.dataGridOptions.reportStyle.replace('core:', '') === 'reportStyle0') {
                    return '';
                } else {
                    return $scope.dataGridOptions.reportStyle.replace('core:', '');
                }
            } catch (e) {
                return '';
            }
        };

        // This method is used to simply stop the event from propagating up the DOM
        $scope.stopEventPropagation = function (event) {
            if (event) {
                event.stopPropagation();
            }
        };

        // Manually select on right-click, because ngGrid does not
        $scope.rightClickSelect = selectRowCell;

        $scope.getContextMenuActions = function () {

            var hideGridContextMenu = sp.result($scope.dataGridOptions, 'hideGridContextMenu');

            var deferred = $q.defer();

            if ($scope.lastSelectedRow &&
                $scope.lastSelectedRow.isAggRow) {
                return deferred.promise;
            }

            // for (grid) selection, there should at least be something to act on
            if (hideGridContextMenu || !$scope.dataGridOptions.selectedItems || $scope.dataGridOptions.selectedItems.length === 0) {

                // return nothing, to close "loading..."
                deferred.resolve([]);
                return deferred.promise;
            }

            var ids = getSelectedIdsFromOptions($scope.dataGridOptions);
            var lastId = getSelectedIdsFromLastSelected($scope.dataGridOptions);
            var cellId = getSelectedIdsFromLastSelectedCell();
            var selected;
            if ($scope.options && $scope.options.selectedItems && _.some($scope.options.selectedItems, 'obj')) {
                selected = _.map($scope.options.selectedItems, 'obj');
            }

            var formDataEntityId = $scope.options.formDataEntity ? $scope.options.formDataEntity.idP : -1;

            var actionRequest =
            {
                ids: ids,
                lastId: lastId,
                cellId: cellId,
                hostIds: [],
                data: {},
                display: 'contextmenu',
                formDataEntityId: formDataEntityId,
                selected: selected
            };

            if ($scope.dataGridOptions.decorateActionRequest) {
                $scope.dataGridOptions.decorateActionRequest(actionRequest);
            }

            spActionsService.getConsoleActions(actionRequest)
                .then(function (response) {
                    var menuKey = '';
                    if (actionRequest.display) {
                        menuKey += actionRequest.display;
                    }
                    if (actionRequest.hostIds && actionRequest.hostIds.length > 0) {
                        menuKey += actionRequest.hostIds.join('');
                    }
                    var items = spContextMenuService.getItemsFromActions(response.actions, menuKey);
                    deferred.resolve(items);
                }, function (reason) {
                    console.error('spDataGrid.getContextMenuActions error:', reason);
                    throw reason;
                });

            return deferred.promise;
        };

        $scope.sortColumnToggle = function (col) {
            var sortInfoChanged, groupColumnIds = [];

            if (!col || !col.colDef || !col.colDef.spColumnDefinition || !col.colDef.spColumnDefinition.columnId || !$scope.gridSortingPlugin) {
                return;
            }

            if ($scope.model.aggregateInfo &&
                $scope.model.aggregateInfo.aggregateData) {
                groupColumnIds = _.map($scope.model.aggregateInfo.aggregateData.getGroupColumns(), 'id');
            }

            sortInfoChanged = spDataGridUtils.updateColumnSortInfo($scope.model.sortInfo, col.colDef.spColumnDefinition.columnId, 'toggle', groupColumnIds);

            if (sortInfoChanged) {
                // Update the sort info
                $scope.gridSortingPlugin.sortData($scope.model.sortInfo);
            }

            if (sortInfoChanged) {
                $scope.$emit('spDataGridEventGridSorted', $scope.model.sortInfo);
            }
        };

        $scope.getCellText = function (row, col) {
            var value = '',
                cellData,
                columnDefinition;

            if (!row || !col || !col.colDef || !col.colDef.spColumnDefinition) {
                return value;
            }

            columnDefinition = col.colDef.spColumnDefinition;

            if (columnDefinition.hideValue) {
                return value;
            } else if (row.entity &&
                row.entity.cells) {
                cellData = row.entity.cells[columnDefinition.cellIndex];
                if (cellData) {
                    value = cellData.value;
                }
            }

            if (columnDefinition.cellFilterFunc) {
                value = columnDefinition.cellFilterFunc(value);
            }

            if (_.isUndefined(value) ||
                _.isNull(value)) {
                value = '';
            }

            return value;
        };

        $scope.onDrop = function (column, field) {

            if (field) {
                //re-order column
                if (column.colDef) {
                    $scope.$apply(function () {
                        reportBuilderService.setActionFromReport('reOrderColumnByDragDrop', column, field);
                    });
                } else {
                    $scope.$apply(function () {
                        reportBuilderService.setActionFromReport('addColumnByDragDrop', column, field);
                    });

                    destroyInsertIndicator();
                }
            }
        };

        $scope.getFormattedCellImageStyle = function (row, col) {
            var style = {},
                imageUrl,
                formattingData;

            if (!row || !col) {
                return undefined;
            }

            formattingData = $scope.getFormattedCellImageFormattingData(row, col);
            if (!formattingData) {
                return undefined;
            }

            imageUrl = IMAGE_BASE_URL +
                (_.isString(formattingData.imageId) ? formattingData.imageId.replace(':', '-') : formattingData.imageId) + '/' +
                (_.isString(col.colDef.spColumnDefinition.imageSizeId) ? col.colDef.spColumnDefinition.imageSizeId.replace(':', '-') : col.colDef.spColumnDefinition.imageSizeId) + '/' +
                (_.isString(col.colDef.spColumnDefinition.imageScaleId) ? col.colDef.spColumnDefinition.imageScaleId.replace(':', '-') : col.colDef.spColumnDefinition.imageScaleId);

            var tokenisedImageUrl = spXsrf.addXsrfTokenAsQueryString(imageUrl);

            if (formattingData.backgroundColor) {
                style['background'] = spUtils.getCssColorFromARGBString(formattingData.backgroundColor);
                style['background-color'] = spUtils.getCssColorFromARGBString(formattingData.backgroundColor);
            }

            style['background-image'] = 'url(\'' + tokenisedImageUrl + '\')';
            style.width = col.colDef.spColumnDefinition.imageWidth + 'px';
            style.height = col.colDef.spColumnDefinition.imageHeight + 'px';

            return style;
        };

        $scope.getFormattedCellImageFormattingData = function (row, col) {
            var cellFormatting,
                formattingData,
                cellData = null,
                columnDefinition;

            if (!row || !col) {
                return null;
            }

            cellFormatting = getFormattedCellFormattingData(row, col);
            if (!cellFormatting) {
                return null;
            }

            cellData = cellFormatting.cellData;
            columnDefinition = cellFormatting.columnDefinition;
            formattingData = cellFormatting.formattingData;

            if (!cellData || !columnDefinition.imageSizeId || !columnDefinition.imageScaleId ||
                angular.isUndefined(cellData.formattingIndex)) {
                return null;
            }

            formattingData = columnDefinition.cellFormatting[cellData.formattingIndex];
            if (!formattingData || !formattingData.imageId) {
                return null;
            }
            formattingData.backgroundColor = cellData.backgroundColor;
            return formattingData;
        };

        $scope.getIsInlineEditing = function (row) {
            return $scope.dataGridOptions.getIsInlineEditing(row.entity.rowIndex);
        };
        
        $scope.getInlineEditingTemplate = function (row, col) {
            return $scope.dataGridOptions.getInlineEditingTemplate(row.entity.rowIndex, col.colDef.index);
        };

        $scope.getDefaultCellStyle = function (row, col) {
            var style = {}, cellFormatting, formattingData;

            if (!row || !col) {
                return style;
            }

            setCellTextStyle(col, style);

            // Get the formatting data
            cellFormatting = getFormattedCellFormattingData(row, col);
            if (cellFormatting) {
                formattingData = cellFormatting.formattingData;
            }

            angular.extend(style, getTextAlignmentStyle(formattingData, col));

            return style;
        };

        $scope.getAggregateCellTemplateClass = function (row, isHeader, isAggRowLeftContextMenuOpen) {
            var classes = '';

            if (!row) {
                return classes;
            }

            if (row.depth === 0) {
                classes = 'aggRowDepth0';
            } else if (row.depth === 1) {
                classes = 'aggRowDepth1';
            } else if (row.depth === 2) {
                classes = 'aggRowDepth2';
            } else {
                classes = 'aggRowDepthElse';
            }

            if (isHeader) {
                if (isAggRowLeftContextMenuOpen) {
                    classes = classes + ' dataGridAggregateHeaderMenuOpened';
                } else {
                    classes = classes + ' dataGridAggregateHeader';
                }
            }

            return classes;
        };

        $scope.getFormattedCellTemplateClass = function (row, col) {
            var classes = 'dataGridCellVerticalAlign';

            if (row && col && $scope.showPercentBar(row, col)) {
                classes = classes + ' dataGridCellProgressBarGradient';
            }

            return classes;
        };

        // Returns the background style for the specified cell from the formatting info.
        $scope.getFormattedCellBackgroundStyle = function (row, col) {
            var cellFormatting,
                formattingData,
                cssColor,
                style,
                percentage,
                cellData = null,
                columnDefinition,
                colWidth = 0,
                shadingWidth;

            if (!row || !col) {
                return null;
            }

            cellFormatting = getFormattedCellFormattingData(row, col);
            if (!cellFormatting) {
                return null;
            }

            cellData = cellFormatting.cellData;
            columnDefinition = cellFormatting.columnDefinition;
            formattingData = cellFormatting.formattingData;

            style = {};

            // Set the background color if it exists
            if (formattingData.backgroundColor) {
                cssColor = getCssColor(formattingData.backgroundColor);
                if (cssColor) {
                    style['background-color'] = cssColor;
                }
            }

            // Set the progress bar size if it exists
            if (formattingData.bounds) {
                percentage = spDataGridUtils.getValueAsPercentage(cellData.value, columnDefinition.type, formattingData.bounds);

                if (percentage < 0) {
                    style.width = '0px';
                } else if (percentage > 100) {
                    style.width = '100%';
                } else {
                    if (col) {
                        colWidth = col.width || 0;
                    }

                    shadingWidth = (colWidth * percentage) / 100.0;

                    if (shadingWidth < 1) {
                        style.width = '1px';
                    } else {
                        style.width = percentage + '%';
                    }
                }

                style.height = $scope.PROGRESS_BAR_HEIGHT;
                style['overflow'] = 'hidden';
                style['position'] = 'absolute';
            } else {
                style.height = '100%';
                style.width = '100%';
            }

            return style;
        };

        $scope.getProgressBarCellStyle = function (col) {
            var style = {};

            if (!col) {
                return style;
            }

            var colWidth = col.width - 10 || 0;
            style['position'] = 'relative';
            style['width'] = colWidth + 'px';
            return style;
        };

        $scope.getProgressBarCellBackgroundStyle = function (row, col) {
            var cellFormatting,
                formattingData,
                cssColor,
                style,
                percentage,
                cellData = null,
                columnDefinition,
                colWidth = 0,
                shadingWidth;

            if (!row || !col) {
                return null;
            }

            cellFormatting = getFormattedCellFormattingData(row, col);
            if (!cellFormatting) {
                return null;
            }

            cellData = cellFormatting.cellData;
            columnDefinition = cellFormatting.columnDefinition;
            formattingData = cellFormatting.formattingData;

            style = {};

            // Set the background color if it exists
            if (formattingData.backgroundColor) {
                cssColor = getCssColor(formattingData.backgroundColor);
                if (cssColor) {
                    style['background-color'] = cssColor;
                }
            }

            // Set the progress bar size if it exists
            if (formattingData.bounds) {
                percentage = spDataGridUtils.getValueAsPercentage(cellData.value, columnDefinition.type, formattingData.bounds);

                if (percentage < 0) {
                    style.width = '0px';
                } else if (percentage > 100) {
                    style.width = '100%';
                } else {
                    if (col) {
                        colWidth = col.width || 0;
                    }

                    shadingWidth = (colWidth * percentage) / 100.0;

                    if (shadingWidth < 1) {
                        style.width = '1px';
                    } else {
                        style.width = percentage + '%';
                    }
                }

                style.height = $scope.PROGRESS_BAR_HEIGHT;
                style['overflow'] = 'hidden';
                style['vertical-align'] = 'middle';
                //style['top'] = '4px';
                style['position'] = 'absolute';
            } else {
                style.height = '100%';
                style.width = '100%';
            }

            return style;
        };

        $scope.getProgressBarFormattedInnerCellForegroundStyle = function (row, col) {
            var cellFormatting,
                formattingData,
                colWidth,
                cssColor,
                style = {};

            if (!row || !col) {
                return style;
            }

            colWidth = col.width - 10 || 0;

            // Get the formatting data
            cellFormatting = getFormattedCellFormattingData(row, col);
            if (cellFormatting) {
                formattingData = cellFormatting.formattingData;
            }

            //set Contrast foreground Color for progress bar
            if (formattingData &&
                formattingData.bounds) {
                //
                // style['line-height'] = ($scope.model.ngGridOptions.rowHeight - ($scope.DEFAULT_CELLPADDING * 2)) + 'px';
                style.width = colWidth + 'px';
                setCellTextAlign(col, style);
                //angular.extend(style, getTextAlignmentStyle(formattingData, col));
                //style['text-align'] = 'right';
                //style['right'] = '-2px';
                style["display"] = "block";
                cssColor = spUtils.getContrastColor(formattingData.backgroundColor);
                if (cssColor) {
                    style.color = cssColor;
                }
                style['position'] = 'absolute';

            } else {
                style["display"] = "none";
            }

            return style;
        };

        // Returns the foreground style for the specified cell from the formatting info.
        $scope.getFormattedCellForegroundStyle = function (row, col) {
            var cellFormatting,
                formattingData,
                cssColor,
                style = {};

            if (!row || !col) {
                return style;
            }

            setCellTextStyle(col, style);

            // Get the formatting data
            cellFormatting = getFormattedCellFormattingData(row, col);
            if (cellFormatting) {
                formattingData = cellFormatting.formattingData;
            }

            angular.extend(style, getTextAlignmentStyle(formattingData, col));

            // Set the foreground color if it exists
            if (formattingData &&
                formattingData.foregroundColor) {
                cssColor = getCssColor(formattingData.foregroundColor);
                if (cssColor) {
                    style.color = cssColor;
                }
            }

            return style;
        };

        // Retirm the standarnd getFormattedCellForegroundStyle with column width
        $scope.getProgressBarCellLabelStyle = function (row, col) {
            var style = {},
                colWidth;

            if (!row || !col) {
                return style;
            }

            style = $scope.getFormattedCellForegroundStyle(row, col);
            colWidth = col.width - 10 || 0;

            style.width = colWidth + 'px';
            style.top = "3px";
            style.right = "1px !important";
            return style;

        };

        // Returns the alignment style for column header
        $scope.getHeaderStyle = function (col) {
            var themeStyle = spThemeService.getReportHeaderStyle();
            var style = _.defaults({}, themeStyle);

            if (!col) {
                return style;
            }

            var columnDefinition = col.colDef.spColumnDefinition;
            if (!_.isNull(columnDefinition.alignment) && !_.isUndefined(columnDefinition.alignment) &&
                (columnDefinition.alignment !== 'Default') &&
                (columnDefinition.alignment !== 'Automatic')
            ) {
                switch (columnDefinition.alignment) {
                    case 'Right':
                        style['text-align'] = 'right';
                        break;
                    case 'Centre':
                        style['text-align'] = 'center';
                        break;
                    case 'Left':
                        style['text-align'] = 'left';
                        break;
                }
            } else {
                //if column alignment is default, remove existing text align property
                if (style && style.hasOwnProperty('text-align')) {
                    delete style['text-align'];
                }

                switch (columnDefinition.type) {
                    case spEntity.DataType.Int32:
                    case spEntity.DataType.Decimal:
                    case spEntity.DataType.Currency:
                        style['text-align'] = 'right';
                        break;
                }
            }
            return style;
        };

        // Returns the alignment style for column header
        $scope.getTotalRowStyle = function (col) {
            var style = {};

            if (!col) {
                return style;
            }

            var columnDefinition = col.colDef.spColumnDefinition;
            if (!_.isNull(columnDefinition.alignment) && !_.isUndefined(columnDefinition.alignment) &&
                (columnDefinition.alignment !== 'Default') &&
                (columnDefinition.alignment !== 'Automatic')
            ) {
                switch (columnDefinition.alignment) {
                    case 'Right':
                        style['text-align'] = 'right';
                        break;
                    case 'Centre':
                        style['text-align'] = 'center';
                        break;
                    case 'Left':
                        style['text-align'] = 'left';
                        if (col.index === 0) {
                            style['padding-left'] = '40px';
                        }
                        break;
                }
            }
            return style;
        };

        // Returns the alignment style for image/icon formatting columns
        $scope.getFormattedCellImageAlignmentStyle = function (row, col) {
            var cellFormatting,
                formattingData,
                style = {};

            if (!row || !col) {
                return style;
            }

            var columnDefinition = col.colDef.spColumnDefinition;
            cellFormatting = getFormattedCellFormattingData(row, col);
            if (cellFormatting) {
                formattingData = cellFormatting.formattingData;
            }

            if (!_.isNull(columnDefinition.alignment) && !_.isUndefined(columnDefinition.alignment) &&
                (columnDefinition.alignment !== 'Default') &&
                (columnDefinition.alignment !== 'Automatic')
            ) {
                formattingData.alignment = columnDefinition.alignment;
                switch (columnDefinition.alignment) {
                    case 'Right':
                        style['text-align'] = 'right';
                        break;
                    case 'Centre':
                        style['text-align'] = 'center';
                        break;
                    case 'Left':
                        style['text-align'] = 'left';
                        break;
                }
            }


            angular.extend(style, getImageTextAlignmentStyle(formattingData, col));

            return style;
        };

        $scope.formattedCellTypeIsAlignRight = function (col) {
            var columnDefinition,
                isAlignRight = false;

            if (!col) {
                return false;
            }

            columnDefinition = col.colDef.spColumnDefinition;

            if (!columnDefinition) {
                return false;
            }

            if (!_.isNull(columnDefinition.alignment) && !_.isUndefined(columnDefinition.alignment) &&
                (columnDefinition.alignment !== 'Default') &&
                (columnDefinition.alignment !== 'Automatic')
            ) {
                switch (columnDefinition.alignment) {
                    case 'Right':
                        isAlignRight = true;
                        break;
                    case 'Centre':
                        isAlignRight = false;
                        break;
                    case 'Left':
                        isAlignRight = false;
                        break;
                }
            } else {
                switch (columnDefinition.type) {
                    case spEntity.DataType.Int32:
                    case spEntity.DataType.Decimal:
                    case spEntity.DataType.Currency:
                        isAlignRight = true;
                        break;
                }
            }

            return isAlignRight;
        };

        // Returns the style of the text cell that is part of image columns
        $scope.getFormattedCellImageForegroundStyle = function (col) {
            var style = {};

            if (!col) {
                return style;
            }

            setCellTextStyle(col, style);

            return style;
        };

        // Returns true if the current cell has percentage formatting, false otherwise.
        $scope.showPercentBar = function (row, col) {
            var cellFormatting,
                formattingData;

            cellFormatting = getFormattedCellFormattingData(row, col);
            if (!cellFormatting) {
                return false;
            }

            formattingData = cellFormatting.formattingData;

            // Set the progress bar size if it exists
            if (formattingData.bounds &&
                angular.isDefined(formattingData.bounds.lower) &&
                angular.isDefined(formattingData.bounds.upper)) {
                return true;
            }

            return false;
        };

        // Emit the grid double clicked event
        $scope.onGridDoubleClicked = function () {
            if (!$scope.model.ngGridOptions) {
                return;
            }

            $scope.$emit('spDataGridEventGridDoubleClicked', $scope.model.ngGridOptions.selectedItems);
        };

        $scope.getAggregateColumnStyle = function (columns, column, row) {
            var c, col, sumWidth = 0;

            if (!$scope.model.ngGridOptions) {
                return {};
            }

            for (c = 0; c < columns.length; c = c + 1) {
                col = columns[c];

                if (col === column) {
                    // Early out
                    break;
                }

                if (col.visible) {
                    sumWidth += col.width;
                }
            }

            if (sumWidth > 0 &&
                row.offsetLeft) {
                sumWidth = sumWidth - row.offsetLeft;
            }


            return {
                left: sumWidth + 'px',
                height: $scope.model.ngGridOptions.aggregateRowHeight + 'px',
                width: column.width + 'px'
            };

        };

        $scope.getAggregateCellStyle = function (column) {

            var style = 'height: ' + AGGREGATE_ROW_LINEHEIGHT + 'px';

            var alignment = sp.result(column, 'colDef.spColumnDefinition.alignment');

            if (alignment &&
                alignment !== 'Default' &&
                alignment !== 'Automatic') {
                switch (alignment) {
                    case 'Right':
                        style += '; text-align: right';
                        break;
                    case 'Centre':
                        style += '; text-align: center';
                        break;
                    case 'Left':
                        style += '; text-align: left';
                        break;
                }
            }

            return style;
        };

        $scope.getAggregateCellSubTotalValue = function (row, column, total) {
            var groupValues = [],
                aggRow = row,
                depth = aggRow.depth,
                columnId,
                value;

            if (!$scope.model.aggregateInfo || !$scope.model.aggregateInfo.aggregateData || !column || !column.colDef || !column.colDef.spColumnDefinition || !total || !total.id) {
                return '';
            }

            // Get the labels for this row up to the root group
            while (aggRow) {
                aggRow = aggRow.orig || aggRow;
                groupValues.unshift(aggRow.label);
                aggRow = aggRow.parent;
            }

            columnId = column.colDef.spColumnDefinition.columnId;

            value = $scope.model.aggregateInfo.aggregateData.getSubTotalValue(groupValues, depth, columnId, total.id);
            return getFormattedTotalValue(value, column.colDef.spColumnDefinition, total.id);
        };

        $scope.getAggregateOptionLabel = function (total) {
            if (!spUtils.isNullOrUndefined($scope.model.aggregateInfo.showOptionLabels)) {
                if ($scope.model.aggregateInfo.showOptionLabels) {
                    return total.displayName + ":";
                } else {
                    return '';
                }
            }
            else {
                return total.displayName + ":";
            }

        };

        $scope.getAggregateCellGrandTotalValue = function (column, total) {
            var columnId, value;

            if (!$scope.model.aggregateInfo || !$scope.model.aggregateInfo.aggregateData || !column || !column.colDef || !column.colDef.spColumnDefinition || !total || !total.id) {
                return '';
            }

            columnId = column.colDef.spColumnDefinition.columnId;

            value = $scope.model.aggregateInfo.aggregateData.getGrandTotalValue(columnId, total.id);
            return getFormattedTotalValue(value, column.colDef.spColumnDefinition, total.id);
        };

        $scope.getAggregateCellGrandTotalCount = function () {
            var countValue,
                label;

            if ($scope.model.aggregateInfo &&
                $scope.model.aggregateInfo.aggregateData &&
                $scope.model.aggregateInfo.showCount) {
                countValue = $scope.model.aggregateInfo.aggregateData.getGrandTotalValue(null, 'aggCount');
                if (countValue && !_.isNull(countValue.value) && !_.isUndefined(countValue.value)) {
                    label = ' (' + $scope.model.aggregateInfo.aggregateData.formatValue(countValue.value, spEntity.DataType.Int32) + ')';
                }
            }

            return label;
        };

        $scope.expandAggregateRows = function (depth) {
            if (!$scope.gridLayoutPlugin) {
                return;
            }

            $scope.gridLayoutPlugin.setAllAggregateRowState(false, depth, false, true);
        };

        $scope.collapseAggregateRows = function (depth) {
            if (!$scope.gridLayoutPlugin) {
                return;
            }

            $scope.gridLayoutPlugin.setAllAggregateRowState(true, depth, false, true);
        };

        $scope.aggregateRowToggleExpand = function (row) {
            if (!row || !row.toggleExpand) {
                return;
            }

            row.toggleExpand();

            if (!$scope.gridLayoutPlugin) {
                return;
            }

            $scope.$emit('spDataGridAggregateRowStateChanged', $scope.gridLayoutPlugin.getAllAggregateRowState());
        };

        $scope.getAggregateRowLabel = function (row, columns) {
            var label = '',
                aggregateColumn,
                aggRow = row,
                depth = aggRow.depth,
                groupValues = [],
                groupData,
                availableCountLabel,
                loadedCountLabel,
                countValue,
                aggregateData;

            if (!$scope.model.aggregateInfo) {
                return '';
            }

            aggregateData = $scope.model.aggregateInfo.aggregateData;

            // Find the aggregate column
            aggregateColumn = _.find(columns, function (c) {
                return c.field === row.field;
            });

            if ($scope.model.aggregateInfo.showAggregateLabel) {
                if (aggregateColumn &&
                    aggregateColumn.displayName) {
                    label = label + aggregateColumn.displayName + ': ';
                }
            }

            // Get the labels for this row up to the root group
            while (aggRow) {
                aggRow = aggRow.orig || aggRow;
                groupValues.unshift(aggRow.label);
                aggRow = aggRow.parent;
            }

            // Convert the group value to the actual data
            if (aggregateData &&
                aggregateColumn &&
                aggregateColumn.colDef &&
                aggregateColumn.colDef.spColumnDefinition) {
                groupData = aggregateData.getGroupData(groupValues);
                groupData = aggregateData.formatValue(groupData, aggregateColumn.colDef.spColumnDefinition.columnId);
            }

            label = label + (groupData ? groupData : '[None]');

            // Append the count
            if (aggregateData &&
                $scope.model.aggregateInfo.showCount) {
                // Find the first column with a count total
                countValue = _.get(aggregateData.getSubTotalValue(groupValues, depth, null, 'aggCount'), 'value', 0);

                if (countValue && !_.get(row, 'aggChildren.length', 0) &&
                    _.get(row, 'children.length', 0) !== countValue) {
                    loadedCountLabel = aggregateData.formatValue(row.children.length, spEntity.DataType.Int32);
                    availableCountLabel = aggregateData.formatValue(countValue, spEntity.DataType.Int32);

                    label = label + ' (' + loadedCountLabel + '/' + availableCountLabel + ')';
                } else {
                    label = label + ' (' + $scope.model.aggregateInfo.aggregateData.formatValue(countValue, spEntity.DataType.Int32) + ')';
                }
            }

            return label;
        };

        $scope.showColumnSortedCue = function (col) {
            var groupedColumsCount = 0;

            if (!col) {
                return false;
            }

            if ($scope.model.aggregateInfo &&
                $scope.model.aggregateInfo.groupedColumns) {
                groupedColumsCount = $scope.model.aggregateInfo.groupedColumns.length;
            }

            return ((col.sortPriority - groupedColumsCount) === 1 || col.sortPriority === null || col.sortPriority === undefined);
        };

        $scope.isMoreGroupDataAvailable = function (row) {
            var moreGroupDataAvailable,
                aggRow = row,
                groupValues = [],
                depth,
                countValue;

            if (!$scope.gridHelperPlugin || !row) {
                return false;
            }

            if (angular.isDefined($scope.dataGridOptions.moreDataAvailable) && !$scope.dataGridOptions.moreDataAvailable) {
                return false;
            }

            if (!$scope.model.aggregateInfo || !$scope.model.aggregateInfo.aggregateData) {
                return false;
            }

            if (isPagingPerGroupEnabled()) {
                // Paging per group is enabled.

                if (_.get(row, 'aggChildren.length', 0)) {
                    // Show get more data link only on leaf group nodes
                    // when paging is enabled
                    return false;
                }

                depth = aggRow.depth;

                // Get the labels for this row up to the root group
                while (aggRow) {
                    aggRow = aggRow.orig || aggRow;
                    groupValues.unshift(aggRow.label);
                    aggRow = aggRow.parent;
                }

                countValue = _.get($scope.model.aggregateInfo.aggregateData.getSubTotalValue(groupValues, depth, null, 'aggCount'), 'value', 0);

                return (countValue && _.get(row, 'children.length', 0) < countValue);
            } else {
                // Existing behaviour.
                // Paging per group is disabled.

                if (firstEmptyGroup) {
                    // Have an empty group
                    moreGroupDataAvailable = row.aggIndex >= (firstEmptyGroup.aggIndex - 1);
                } else {
                    // Don't have an empty group
                    // If it's the last group then show that more data is available
                    moreGroupDataAvailable = (row.aggIndex === $scope.gridHelperPlugin.getGroupsCount() - 1);
                }

                if (!moreGroupDataAvailable) {
                    // check if row is parent of an empty one
                    moreGroupDataAvailable = _.has(firstEmptyGroupParentGroups, row.aggIndex);
                }

                return moreGroupDataAvailable;
            }
        };

        $scope.aggregateRowLoadDataClick = function (event, aggRow) {
            var groupValues = [],
                aggRowTmp,
                groupDataFilter = null,
                childRowsCount = 0;

            if (!$scope.gridLayoutPlugin || !aggRow || !$scope.model.aggregateInfo || !$scope.model.aggregateInfo.aggregateData) {
                return;
            }

            if (isPagingPerGroupEnabled()) {
                // Work out what the filter should be for the selected group

                aggRowTmp = aggRow;

                while (aggRowTmp) {
                    aggRowTmp = aggRowTmp.orig || aggRowTmp;
                    groupValues.unshift(aggRowTmp.label);
                    aggRowTmp = aggRowTmp.parent;
                }

                groupDataFilter = [];

                for (var g = 1; g < groupValues.length + 1; g++) {
                    groupDataFilter.push($scope.model.aggregateInfo.aggregateData.getGroupData(_.take(groupValues, g)));
                }

                childRowsCount = _.get(aggRow, 'children.length', 0);
            }

            var eventArgs = {
                // The index at which to get the next page of data
                index: getActualDataRowCount(),
                // The source of the data request
                source: 'aggRowClick',
                // The selected aggregate row index
                aggRowIndex: aggRow.aggIndex,
                // The offset from the top of the selected aggregate row. Used to maintain the same scroll position
                aggRowOffsetTop: aggRow.offsetTop - $scope.gridLayoutPlugin.getScrollTop(),
                // The group filter
                groupDataFilter: groupDataFilter,
                // The number of child rows in the current group
                childRowsCount: childRowsCount
            };

            event.stopPropagation();
            // Request more data

            $scope.$emit('spDataGridEventDataRequested', eventArgs);
        };

        $scope.showImageViewerDialog = function (row, col) {
            if (isTouchDevice()) {
                return;
            }

            if (!row || !col) {
                return;
            }

            var formattingData = $scope.getFormattedCellImageFormattingData(row, col),
                options = {};

            if (!formattingData || !formattingData.imageId) {
                return;
            }

            options.imageUrl = '/spapi/data/v1/image/' +
                (_.isString(formattingData.imageId) ? formattingData.imageId.replace(':', '-') : formattingData.imageId);
            options.backgroundColor = formattingData.backgroundColor;
            spImageViewerDialog.showModalDialog(options);
        };

        $scope.onKeyPress = onKeyPress;

        $scope.dataGridOptions.dataGrid.getFirstSelectableRowDataItem = function () {
            if (!$scope.gridHelperPlugin) {
                return null;
            }

            return $scope.gridHelperPlugin.getFirstSelectableRowDataItem();
        };

        $scope.dataGridOptions.dataGrid.scrollToTop = function () {
            if (!$scope.gridLayoutPlugin) {
                return;
            }

            $scope.gridLayoutPlugin.scrollTo(0);
        };

        $scope.dataGridOptions.dataGrid.scrollTo = function (rowIndex) {
            if (!$scope.gridLayoutPlugin) {
                return;
            }

            $scope.gridLayoutPlugin.scrollTo(rowIndex);
        };

        $scope.dataGridOptions.dataGrid.scrollToGroup = function (groupIndex, prevOffsetTop) {
            if (!$scope.gridLayoutPlugin || groupIndex < 0) {
                return;
            }

            $scope.gridLayoutPlugin.scrollToGroup(groupIndex, prevOffsetTop);
        };

        $scope.dataGridOptions.dataGrid.isScrolledToTop = function () {
            if (!$scope.gridLayoutPlugin) {
                return false;
            }

            return $scope.gridLayoutPlugin.isScrolledToTop();
        };

        $scope.dataGridOptions.dataGrid.selectRow = function (rowIndex, state, continueSelection, suppressUpdates) {
            if (!$scope.selectionProviderPlugin) {
                return;
            }
            return $scope.selectionProviderPlugin.selectRow(rowIndex, state, continueSelection, suppressUpdates);
        };

        $scope.dataGridOptions.dataGrid.clearSelection = function () {
            if (!$scope.selectionProviderPlugin) {
                return;
            }

            return $scope.selectionProviderPlugin.clearSelection();
        };

        $scope.dataGridOptions.dataGrid.getMinRowsToRender = function () {
            if (!$scope.gridHelperPlugin) {
                return;
            }

            return $scope.gridHelperPlugin.getMinRowsToRender();
        };

        $scope.dataGridOptions.dataGrid.getAllAggregateRowState = function () {
            if (!$scope.gridLayoutPlugin) {
                return;
            }

            return $scope.gridLayoutPlugin.getAllAggregateRowState();
        };

        $scope.dataGridOptions.dataGrid.getRenderedRange = function () {
            if (!$scope.gridHelperPlugin) {
                return null;
            }

            return $scope.gridHelperPlugin.getRenderedRange();            
        };

        $scope.dataGridOptions.dataGrid.expandAggregateRows = function (depth) {
            $scope.expandAggregateRows(depth);
        };

        $scope.dataGridOptions.dataGrid.collapseAggregateRows = function (depth) {
            $scope.collapseAggregateRows(depth);
        };

        $scope.dataGridOptions.dataGrid.focusInlineEditCellElement = function (columnCssClass) {
            if (!$scope.selectionProviderPlugin || !sp.result($scope, 'dataGridOptions.isInlineEditing')) {
                return;
            }

            if (!columnCssClass) {
                columnCssClass = selectedColumnCssClass;
            }

            $scope.selectionProviderPlugin.focusCellElement(columnCssClass);
        };

        $scope.$watch('nav.getThemes()', function (getThemesCompleted) {
            if (getThemesCompleted && !$scope.themeLoaded) {
                updateTheme();
                if ($scope.model.ngGridOptions.columnDefs && $scope.model.ngGridOptions.columnDefs.length) {
                    $scope.gridLayoutPlugin.rebuildGrid();
                    $scope.gridHelperPlugin.buildColumns();
                }
            }
        });

        $scope.$watch('dataGridOptions.getActionExecutionContext', function () {
            $scope.getActionExecutionContext = $scope.dataGridOptions.getActionExecutionContext;
        });

        $scope.$watch('dataGridOptions.hideReportHeader', function () {
            $scope.gridLayoutPlugin.showReportHeader(!$scope.dataGridOptions.hideReportHeader);
        });

        $scope.$watch('dataGridOptions.rowData', function () {
            $scope.model.rowData = $scope.dataGridOptions.rowData;

            var haveNewData = !_.isEmpty($scope.dataGridOptions.rowData);
            var haveExistingData = !_.isEmpty($scope.model.ngGridOptions.data);

            $scope.model.ngGridOptions.data = $scope.dataGridOptions.rowData;

            if ($scope.gridHelperPlugin &&
                (haveNewData || haveExistingData)) {
                // refresh the grid data
                $scope.gridHelperPlugin.refreshRowData($scope.model.ngGridOptions.data);

                updateGroupsStateDebounced();
            }
        });

        $scope.$watch('dataGridOptions.multiSelect', function () {
            if (updateGridMultiSelect()) {
                $scope.gridLayoutPlugin.rebuildGrid();
            }
        });

        $scope.$watch('dataGridOptions.columnDefinitions', function () {
            if (!$scope.gridLayoutPlugin || !$scope.gridHelperPlugin || !$scope.model.ngGridOptions || !$scope.gridSortingPlugin) {
                return;
            }

            var areNewColumnsEmpty = _.isEmpty($scope.dataGridOptions.columnDefinitions);
            var areExistingColumnsEmpty = _.isEmpty($scope.model.ngGridOptions.columnDefs);

            if (areNewColumnsEmpty && areExistingColumnsEmpty) {
                return;
            }

            setGridRowHeight($scope.dataGridOptions);
            setGridAggregateRowHeight($scope.dataGridOptions);
            setGridColumDefs($scope.dataGridOptions);

            updateUseExternalSorting();
            updateGridMultiSelect();
            updateTheme();

            // Build new columns
            $scope.model.ngGridOptions.columnDefs = $scope.model.columnDefinitions;
            $scope.gridHelperPlugin.buildColumns($scope.model.ngGridOptions.columnDefs);

            // Update the grouped columns
            updateGroupedColumns();

            $scope.gridLayoutPlugin.updateShowRowDataHeader($scope.model.ngGridOptions.showRowDataHeader);
            $scope.gridLayoutPlugin.updateGridRowHeight($scope.model.ngGridOptions.rowHeight, $scope.model.ngGridOptions.aggregateRowHeight, $scope.model.ngGridOptions.rowDataHeaderHeight);

            // Rebuild grid
            $scope.gridLayoutPlugin.rebuildGrid(_.uniqueId());

            $scope.gridSortingPlugin.sortData($scope.model.sortInfo);
        });

        $scope.$watch('dataGridOptions.useExternalSorting', function () {
            updateUseExternalSorting();
        });

        $scope.$watch('dataGridOptions.sortInfo', function () {
            if (!$scope.gridSortingPlugin) {
                return;
            }

            $scope.model.sortInfo = $scope.dataGridOptions.sortInfo;
            $scope.gridSortingPlugin.sortData($scope.model.sortInfo);
        });

        $scope.$watch('dataGridOptions.aggregateInfo', function () {
            $scope.model.aggregateInfo = $scope.dataGridOptions.aggregateInfo;
        });

        $scope.$on('ngGridEventColumns', function () {
            raiseUpdateGridMessageDebouncedFast();
        });

        // Raise an update grid message when scrolling
        $scope.$on('ngGridEventRows', function (event, rows) {
            raiseUpdateGridMessageDebouncedFast();
            // console.log('ngGridEventRows', rows);
        });

        // Respond to some action being taken which may have altered the selected data
        $scope.$on('actionExecuted', function (evt, args) {
            clearSelection();
        });

        // Destroy the auto update plugin
        $scope.$on('$destroy', function () {
            if ($scope.selectionProviderPlugin) {
                $scope.selectionProviderPlugin.destroy();
                $scope.selectionProviderPlugin = null;
            }

            if ($scope.gridLayoutPlugin) {
                $scope.gridLayoutPlugin.destroy();
                $scope.gridLayoutPlugin = null;
            }

            if ($scope.gridSortingPlugin) {
                $scope.gridSortingPlugin.destroy();
                $scope.gridSortingPlugin = null;
            }

            if ($scope.gridColumnsChangeTrackerPlugin) {
                $scope.gridColumnsChangeTrackerPlugin.destroy();
                $scope.gridColumnsChangeTrackerPlugin = null;
            }

            if ($scope.gridHelperPlugin) {
                $scope.gridHelperPlugin.destroy();
                $scope.gridHelperPlugin = null;
            }

            if ($scope.gridCleanupPlugin) {
                $scope.gridCleanupPlugin.destroy();
                $scope.gridCleanupPlugin = null;
            }            
            
            if ($scope.model &&
                $scope.model.ngGridOptions) {
                $scope.model.ngGridOptions.plugins = [];
                $scope.model.ngGridOptions = null;
            }

            $scope.dataGridOptions.dataGrid = null;

            body.removeClass('dataGridUnselectable');
        });

        $scope.$on('measureArrangeComplete', function (event) {
            if ($scope.gridLayoutPlugin) {
                $scope.gridLayoutPlugin.rebuildGrid();
            }
        });

        // The 300 is for #26587 - timing issue when flipping between tabs in view mode
        var gridRebuildDebounceDelay = !isTouchDevice() && !$scope.dataGridOptions.isEditMode ? 300 : 10;
        $scope.$on('app.layout.done', _.debounce(function () {
            if ($scope.gridLayoutPlugin) {
                $scope.gridLayoutPlugin.rebuildGrid();
            }
        }, gridRebuildDebounceDelay));

        $scope.$on('forceRebuildGrid', function (event) {
            if ($scope.gridLayoutPlugin) {
                $scope.gridLayoutPlugin.rebuildGrid();
            }
        });

        // When the grid is scrolled to the end request more data.
        $scope.$on('ngGridEventScroll', function (event) {
            var eventArgs = {
                // The index at which to get the next page of data
                index: getActualDataRowCount(),
                // The source of the data request
                source: 'scroll'
            };
            event.stopPropagation();
            $scope.$emit('spDataGridEventDataRequested', eventArgs);
        });

        $scope.$on('ngGridEventGroups', function () {
            if (!$scope.gridLayoutPlugin ||
                (!sp.result($scope, 'dataGridOptions.aggregateInfo.groupedColumns.length') && !sp.result($scope, 'dataGridOptions.existingAggregateRows.length'))) {
                return;
            }

            updateGroupsStateDebounced();
        });

        // Raise grid event data events
        $scope.$on('ngGridEventData', function (event) {
            event.stopPropagation();
            onGridEventData();
            raiseUpdateGridMessageDebounced();
        });

        $scope.dataGridCellClick = function (col) {            
            selectedColumnCssClass = col ? col.colIndex() : null;            
        };

        initialiseForTouch();
        bindSelectionHandlers($element, body);

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Implementation

        function search() {
            var cellIndex = -1,
                cellFilterFunc,
                startIndex = 0,
                r,
                cellValue,
                firstSelectedRow,
                foundIndex = -1,
                regExp,
                columnDef;

            if (_.isEmpty(searchText) || !$scope.dataGridOptions.dataGrid || !$scope.model.ngGridOptions) {
                return;
            }

            regExp = new RegExp('^' + spUtils.escapeRegExp(searchText), 'i');
            columnDef = _.find($scope.model.columnDefinitions, function (c) {
                return c.visible;
            });

            // Clear search text
            searchText = '';

            if (columnDef &&
                columnDef.spColumnDefinition) {
                cellIndex = columnDef.spColumnDefinition.cellIndex;
                cellFilterFunc = columnDef.spColumnDefinition.cellFilterFunc;
            }

            if (cellIndex < 0) {
                return;
            }

            // Get the first selected row
            firstSelectedRow = _.min($scope.model.ngGridOptions.selectedItems, 'rowIndex');

            // Get its index
            if (firstSelectedRow &&
                firstSelectedRow.rowIndex) {
                startIndex = firstSelectedRow.rowIndex;
            }

            // Find the next matching rowindex
            for (r = startIndex + 1; r < $scope.model.rowData.length; r++) {
                if ($scope.model.rowData[r] &&
                    $scope.model.rowData[r].cells &&
                    $scope.model.rowData[r].cells[cellIndex]) {
                    cellValue = $scope.model.rowData[r].cells[cellIndex].value;

                    if (cellFilterFunc) {
                        cellValue = cellFilterFunc(cellValue);
                    }

                    if (regExp.test(cellValue)) {
                        foundIndex = r;
                        break;
                    }
                }
            }

            if (foundIndex >= 0) {
                // Select and scroll to row
                $scope.dataGridOptions.dataGrid.selectRow(foundIndex, true, false);
                $scope.dataGridOptions.dataGrid.scrollTo(foundIndex);
            } else {
                $scope.dataGridOptions.dataGrid.scrollTo(startIndex);
            }
        }

        function setCellTextStyle(col, style) {
            var columnDefinition = col.colDef.spColumnDefinition;

            if (!$scope.model.ngGridOptions) {
                return;
            }

            if (columnDefinition.lines > 1) {
                style['line-height'] = DEFAULT_LINEHEIGHT + 'px';
                style.height = '100%';
                style.width = '100%';
                style['word-wrap'] = 'break-word';
                style['white-space'] = 'pre-wrap';
            } else {
                style['line-height'] = ($scope.model.ngGridOptions.rowHeight - ($scope.DEFAULT_CELLPADDING * 2)) + 'px';
                style.width = '100%';
                style.overflow = 'hidden';
                style['text-overflow'] = 'ellipsis';
            }

            setCellTextAlign(col, style);

        }

        function setCellTextAlign(col, style) {
            var columnDefinition = col.colDef.spColumnDefinition;

            if (!$scope.model.ngGridOptions) {
                return;
            }

            if (!_.isNull(columnDefinition.alignment) && !_.isUndefined(columnDefinition.alignment) &&
                (columnDefinition.alignment !== 'Default') &&
                (columnDefinition.alignment !== 'Automatic')
            ) {
                switch (columnDefinition.alignment) {
                    case 'Right':
                        style['text-align'] = 'right';
                        break;
                    case 'Centre':
                        style['text-align'] = 'center';
                        break;
                    case 'Left':
                        style['text-align'] = 'left';
                        break;
                }
            } else {
                switch (columnDefinition.type) {
                    case spEntity.DataType.Int32:
                    case spEntity.DataType.Decimal:
                    case spEntity.DataType.Currency:
                        style['text-align'] = 'right';
                        break;
                }
            }
        }

        function isCountTotal(id) {
            switch (id) {
                case 'aggCount':
                case 'aggCountWithValues':
                case 'aggCountUniqueItems':
                case 'aggCountUniqueNotBlanks':
                    return true;
                default:
                    return false;
            }
        }

        function getFormattedTotalValue(totalValue, column, totalId) {
            if (!totalValue) {
                return '';
            } else {
                if (isCountTotal(totalId)) {
                    return $scope.model.aggregateInfo.aggregateData.formatValue(totalValue.value, spEntity.DataType.Int32);
                }

                if (column &&
                    totalValue.type &&
                    totalValue.type === column.type) {
                    // The total type is the same as the column type
                    // Format it like the column
                    return $scope.model.aggregateInfo.aggregateData.formatValue(totalValue.value, column.columnId);
                } else {
                    if (totalValue.type) {
                        return $scope.model.aggregateInfo.aggregateData.formatValue(totalValue.value, totalValue.type);
                    } else {
                        return totalValue.value;
                    }
                }
            }
        }

        function isPagingPerGroupEnabled() {
            if (!$scope.model.aggregateInfo || !$scope.model.aggregateInfo.aggregateData) {
                return false;
            }

            return !$scope.model.aggregateInfo.aggregateData.haveAggregateGroupColumns() && !$scope.dataGridOptions.isEditMode;
        }

        function getActualDataRowCount() {
            // Exclude the special hidden aggregate value rows
            // which are there to trick the grid into showing groups.
            if ($scope.model.aggregateInfo &&
                $scope.model.aggregateInfo.aggregateData &&
                $scope.model.aggregateInfo.aggregateData.hasRollupData()) {
                return _.filter($scope.model.rowData, function (r) {
                    return r && !r.isHiddenAggValueRow;
                }).length;
            } else {
                return $scope.model.rowData.length;
            }
        }

        // Get the alignment style for image/icon formatting columns
        function getImageTextAlignmentStyle(formattingData, col) {
            var style = {};

            if (formattingData &&
                formattingData.alignment &&
                (formattingData.alignment !== 'Default') &&
                (formattingData.alignment !== 'Automatic')) {
                switch (formattingData.alignment) {
                    case 'Right':
                        style['text-align'] = 'right';
                        break;
                    case 'Centre':
                        style['text-align'] = 'center';
                        break;
                    case 'Left':
                        style['text-align'] = 'left';
                        break;
                }
            } else {
                // fallback to default style
                style = getDefaultImageTextAlignmentStyle(col);
            }

            return style;
        }

        // Get the alignment style for non image columns
        function getTextAlignmentStyle(formattingData, col) {
            var style = {};

            if (formattingData &&
                formattingData.alignment &&
                (formattingData.alignment !== 'Default') &&
                (formattingData.alignment !== 'Automatic')) {
                switch (formattingData.alignment) {
                    case 'Right':
                        style['text-align'] = 'right';
                        style.right = '1px';
                        break;
                    case 'Centre':
                        style['text-align'] = 'center';
                        style.width = '100%';
                        break;
                    case 'Left':
                        style['text-align'] = 'left';
                        style.width = '100%';
                        break;
                }
            } else {
                // fallback to default style
                style = getDefaultTextAlignmentStyle(col);
            }

            return style;
        }

        function getDefaultImageTextAlignmentStyle(col) {
            var columnDefinition = col.colDef.spColumnDefinition,
                style = {};

            if (!columnDefinition || !columnDefinition.type) {
                return style;
            }

            switch (columnDefinition.type) {
                case 'Image':
                    style['text-align'] = 'center';
                    break;
                case spEntity.DataType.Int32:
                case spEntity.DataType.Decimal:
                case spEntity.DataType.Currency:
                    style['text-align'] = 'right';
                    break;
            }

            return style;
        }

        function getDefaultTextAlignmentStyle(col) {
            var columnDefinition = col.colDef.spColumnDefinition,
                style = {};

            if (!columnDefinition || !columnDefinition.type) {
                return style;
            }

            switch (columnDefinition.type) {
                case spEntity.DataType.Int32:
                case spEntity.DataType.Decimal:
                case spEntity.DataType.Currency:
                    style.right = '1px';
                    break;
            }

            return style;
        }

        function getCssColor(color) {
            var r = 0, g = 0, b = 0, a = 1;

            if (!color) {
                return null;
            }

            if (color.r) {
                r = color.r;
            }

            if (color.g) {
                g = color.g;
            }

            if (color.b) {
                b = color.b;
            }

            if (color.a >= 0) {
                a = color.a / 255;
            }

            return 'rgba(' + r + ',' + g + ',' + b + ',' + a + ')';
        }

        function initialiseForTouch() {

            // The right click and the rowPress both fire on mobile devices and can get unreliable. Better to use just one or the other
            if (isTouchDevice()) {
                $scope.rowRightClick = function () {
                };                 // Ignore the click events as they don't play nicely with the touch events.

                $scope.rowTap = function (event, row, col) {
                    event.preventDefault();
                    event.srcEvent.preventDefault();

                    selectRowCell(event, row, col);         // The grid does not automatically select on a touch tap.

                    $scope.onGridDoubleClicked(row, col);

                };

                $scope.rowPress = function (event, row, col) {
                    event.preventDefault();
                    event.srcEvent.preventDefault();
                    $scope.rightClickSelect(event, row, col);

                    // The bubbled up event will open the context menu
                };
            } else {
                $scope.rowTap = $scope.rowPress = function () {
                };      // Ignore the touch events as they don't play nicely with the click handlers

                $scope.rowRightClick = function (event, row, col) {

                    $scope.rightClickSelect(event, row, col);

                    // The bubbled up event will open the context menu
                };

            }
        }

        function isTouchDevice() {
            return spMobileContext.isMobile || spMobileContext.isTablet;
        }

        function updateTheme() {
            if (spNavService.getThemes && spNavService.getThemes()) {
                $scope.consoleThemeModel.headerStyle = spThemeService.getReportHeaderStyle();
                //get header background color and update the grid by gridLayoutPlugin
                if ($scope.consoleThemeModel.headerStyle &&
                    $scope.consoleThemeModel.headerStyle.hasOwnProperty('background')) {
                    $scope.gridLayoutPlugin.updateHeaderColor($scope.consoleThemeModel.headerStyle.background);
                }
                $scope.themeLoaded = true;
            }
        }

        function selectRowCell(event, row, col) {

            if (!$scope.dataGridOptions.dataGrid) {
                return;
            }

            if (row) {
                $scope.lastSelectedRow = row;
                var rowIndex = row.entity ? row.entity.rowIndex : row.rowIndex;
                var selected = $scope.dataGridOptions.selectedItems;
                var isSelected = _.some(selected, function (s) {
                    return s.rowIndex === rowIndex;
                });

                if (!isSelected) {

                    $scope.dataGridOptions.dataGrid.selectRow(rowIndex, true, false);
                }
            }
            if (col) {
                $scope.lastSelectedColumn = col;
            }
        }

        function clearSelection() {
            if (!$scope.dataGridOptions.dataGrid) {
                return;
            }

            $scope.lastSelectedRow = undefined;
            $scope.lastSelectedColumn = undefined;
            $scope.dataGridOptions.selectedItems.length = 0;

            $scope.dataGridOptions.dataGrid.clearSelection();
        }

        function getFocusedColumnCssClass() {
            if (!sp.result($scope, 'dataGridOptions.isInlineEditing')) {
                return null;
            }
            
            let currentElement = $(document.activeElement);            
            let foundParent = null;

            // Walk the parents until we find the ngCell element
            while (currentElement) {
                let parent = currentElement.parent();

                if (!parent || !parent.length) {
                    break;
                }

                if (parent.attr('sp-data-grid-row-col-scope') &&
                    parent.hasClass('ngCell')) {
                    foundParent = parent;
                    break;
                }

                currentElement = parent;
            }

            if (foundParent) {
                return sp.result(foundParent, 'scope.col.colIndex');
            }

            return null;
        }

        function bindSelectionHandlers(element, body) {
            // Skip if browser is not IE
            //if (navigator.userAgent.indexOf('MSIE') === -1) {
            //    return;
            //}
            // Using navigator object to detect browser seems very unreliable. Running this for all browsers as this seems to work for all (IE, Chrome, Safari).

            // HACK for IE: On key down add the dataGridUnselectable class to the body
            // This is to work around IE behaviour which is selecting
            // text outside the grid on shift click.
            element.bind('keydown', function (e) {
                if (e.keyCode === 16) { //shift key
                    body.addClass('dataGridUnselectable');
                }                                

                if (e.keyCode === 40) { //arrow down key, select the next row data or first row data in current grid
                    let colCssClass = getFocusedColumnCssClass();
                    if (selectedItemInCurrentRowData()) {
                        selectToRow(e, 'Next');                        
                    } else if ($scope.model.rowData && $scope.model.rowData.length > 0) {
                        selectRowCell(e, $scope.model.rowData[0], null);
                    }                    
                    $scope.dataGridOptions.dataGrid.focusInlineEditCellElement(colCssClass);
                }

                if (e.keyCode === 38) { //arrow up key, select the last row data or first row data in current grid
                    let colCssClass = getFocusedColumnCssClass();
                    if (selectedItemInCurrentRowData()) {
                        selectToRow(e, 'Last');
                    } else if ($scope.model.rowData && $scope.model.rowData.length > 0) {
                        selectRowCell(e, $scope.model.rowData[0], null);
                    }
                    $scope.dataGridOptions.dataGrid.focusInlineEditCellElement(colCssClass);
                }

                if (e.keyCode === 13) { //enter key
                    //if selected item in current row data, execute double click event
                    if (selectedItemInCurrentRowData()) {
                        $scope.$emit('spDataGridEventGridDoubleClicked', $scope.model.ngGridOptions.selectedItems);
                    } else if ($scope.model.rowData && $scope.model.rowData.length > 0) {
                        //press the first row of current grid
                        selectRowCell(e, $scope.model.rowData[0], null);
                        //if only one row data exists in grid
                        if ($scope.model.rowData.length === 1) {
                            //execute double click event
                            $scope.$emit('spDataGridEventGridDoubleClicked', $scope.model.ngGridOptions.selectedItems);
                        }
                    }
                }
                return true;
            });

            // On key up remove the dataGridUnselectable class from the body
            element.bind('keyup', function (e) {
                if (e.keyCode === 16) { //shift key
                    body.removeClass('dataGridUnselectable');
                }                                                
                return true;
            });
        }

        function selectedItemInCurrentRowData() {
            if (spUtils.isNullOrUndefined($scope.model.ngGridOptions) || spUtils.isNullOrUndefined($scope.model.ngGridOptions.selectedItems) || $scope.model.ngGridOptions.selectedItems.length === 0) {
                return false;
            }

            if (spUtils.isNullOrUndefined($scope.model.rowData) || $scope.model.rowData.length === 0) {
                return false;
            }

            var selectedItem = _.filter($scope.model.rowData, function (r) {
                return r.eid === $scope.model.ngGridOptions.selectedItems[0].eid;
            });

            return selectedItem.length > 0;
        }

        function selectToRow(e, dir) {
            //get the selectitem from current report grid
            var selectedItem = _.filter($scope.model.rowData, function (r) {
                return r.eid === $scope.model.ngGridOptions.selectedItems[0].eid;
            });

            //get the to row index by dir
            if (selectedItem.length > 0) {
                var rowIndex = dir === 'Next' ? selectedItem[0].rowIndex + 1 : selectedItem[0].rowIndex - 1;

                //get the next rowdata
                var row = _.filter($scope.model.rowData, function (r) {
                    return r.rowIndex === rowIndex;
                });

                if (row.length > 0) {
                    //select the next row
                    selectRowCell(e, row[0], null);
                    if (!$scope.dataGridOptions.isInlineEditing) {
                        //scroll to next row index
                        $scope.dataGridOptions.dataGrid.scrollTo(rowIndex);   
                    }                    
                }

            }
        }

        function getSelectedIdsFromOptions(gridOptions) {
            return spActionsService.getEntityIdsFromDataGridSelection(gridOptions.selectedItems, gridOptions.columnDefinitions);
        }

        function getSelectedIdsFromLastSelected(gridOptions) {
            var id = -1;

            if ($scope.lastSelectedRow) {
                var item = $scope.lastSelectedRow.entity;
                if (item) {
                    id = item.eid;
                }

                if (angular.isUndefined(id) || id < 0) {
                    var idColumn = _.find(gridOptions.columnDefinitions, function (columnDefinition) {
                        if (angular.isDefined(columnDefinition.displayName)) {
                            return (columnDefinition.displayName.match(/Id$/) && columnDefinition.type === 'Int32');
                        }
                        return false;
                    });

                    if (angular.isDefined(idColumn)) {
                        id = item.cells[idColumn.cellIndex].value;
                    }
                }
            }

            return id;
        }

        function getSelectedIdsFromLastSelectedCell() {
            var id = -1;

            if ($scope.lastSelectedColumn) {
                var col = $scope.lastSelectedColumn;
                var item = $scope.lastSelectedRow.entity;
                if (item) {
                    var related = item.cells[col.index].value;
                    if (related && _.isObject(related)) {
                        var keys = _.keys(related);
                        var k = _.first(keys);
                        id = _.parseInt(k);
                    }
                }
            }

            return id;
        }

        function onBeforeSelectionChange() {

            // on a touch device prevent the selection changing if the context menu is open.
            //if (isTouchDevice() && ($scope.model.contextMenuIsOpen)) {
            //    return false;
            //}

            return true;
        }

        function onAfterSelectionChange() {
            if (!$scope.model.ngGridOptions) {
                return;
            }

            $scope.dataGridOptions.selectedItems = _.clone($scope.model.ngGridOptions.selectedItems);
            $scope.$emit('spDataGridEventSelectionChanged', $scope.dataGridOptions.selectedItems);            
        }

        function updateGridMultiSelect() {
            var result = false;

            if (!$scope.gridLayoutPlugin || !$scope.model.ngGridOptions) {
                return false;
            }

            var multiSelectNew = $scope.dataGridOptions.multiSelect || false,
                multiSelectOld = $scope.model.ngGridOptions.multiSelect || false;

            if (multiSelectNew !== multiSelectOld) {
                result = $scope.gridLayoutPlugin.updateGridMultiSelect(multiSelectNew);
                if (result) {
                    $scope.model.ngGridOptions.multiSelect = multiSelectNew;
                }
            }

            return result;
        }

        function updateUseExternalSorting() {
            if (!$scope.gridSortingPlugin || !$scope.model.ngGridOptions) {
                return;
            }

            if ($scope.model.ngGridOptions.useExternalSorting !== $scope.dataGridOptions.useExternalSorting) {
                if ($scope.gridSortingPlugin.updateUseExternalSorting($scope.dataGridOptions.useExternalSorting)) {
                    $scope.model.ngGridOptions.useExternalSorting = $scope.dataGridOptions.useExternalSorting;
                }
            }
        }

        function updateGroupedColumns() {
            if (!$scope.gridLayoutPlugin || !$scope.gridHelperPlugin || !$scope.model.ngGridOptions) {
                return;
            }

            // Clear existing groups
            $scope.gridHelperPlugin.clearGrouping(false);

            // This method is called within the column definition watcher.
            // The dataGridOptions.aggregateInfo watcher may not have fired yet
            // so we look at the aggregate info on the options rather than the model.
            if ($scope.dataGridOptions.aggregateInfo &&
                $scope.dataGridOptions.aggregateInfo.groupedColumns) {
                _.forEach($scope.dataGridOptions.aggregateInfo.groupedColumns, function (gc) {
                    // Find the column with the specified id
                    var column = _.find($scope.model.columnDefinitions, function (cd) {
                        return cd.spColumnDefinition && (cd.spColumnDefinition.columnId === gc.columnId);
                    });
                    if (column &&
                        column.field) {
                        $scope.gridHelperPlugin.groupBy(column.field);
                    }
                });
            }

            $scope.model.ngGridOptions.showRowDataHeader = $scope.dataGridOptions.aggregateInfo && $scope.dataGridOptions.aggregateInfo.aggregateData ? $scope.dataGridOptions.aggregateInfo.showGrandTotals && $scope.dataGridOptions.aggregateInfo.aggregateData.hasAggregateColumns() : false;
        }

        function updateAggregateRowDefaultStates() {
            // Set grouping collapsed state
            if (!$scope.gridLayoutPlugin || !$scope.dataGridOptions.aggregateInfo || !$scope.dataGridOptions.aggregateInfo.groupedColumns) {
                return;
            }

            _.forEachRight($scope.dataGridOptions.aggregateInfo.groupedColumns, function (gc, index) {
                $scope.gridLayoutPlugin.setAllAggregateRowState(gc.collapsed, index, true);
            });
        }

        function updateAggregateRowExistingStates() {
            // Set grouping collapsed state
            if (!$scope.gridLayoutPlugin || !$scope.dataGridOptions.aggregateInfo || !$scope.dataGridOptions.aggregateInfo.groupedColumns || !$scope.dataGridOptions.existingAggregateRows) {
                return;
            }

            $scope.gridLayoutPlugin.setExistingAggregateRowStates($scope.dataGridOptions.existingAggregateRows);
        }

        function setGridRowHeight() {
            var maxHeightColumn,
                maxLinesColumn,
                rowHeight = DEFAULT_ROWHEIGHT,
                multiLineRowHeight = 0;

            if (!$scope.model.ngGridOptions) {
                return;
            }

            // Find the maximum imageHeight across all columns
            maxHeightColumn = _.maxBy($scope.dataGridOptions.columnDefinitions, function (c) {
                return c.cellFormatting && !_.isEmpty(c.cellFormatting) && c.imageWidth > 0 && c.imageHeight > 0 && c.imageSizeId && c.imageScaleId ? c.imageHeight : 0;
            });

            // Find the maximum number of lines across all columns
            maxLinesColumn = _.maxBy($scope.dataGridOptions.columnDefinitions, function (c) {
                return c.lines > 0 ? c.lines : 0;
            });

            if (maxLinesColumn &&
                maxLinesColumn.lines > 1) {
                multiLineRowHeight = (maxLinesColumn.lines * DEFAULT_LINEHEIGHT) + ($scope.DEFAULT_CELLPADDING * 2);
            }

            // Set the row height if the max height column is an image column
            if (maxHeightColumn &&
                maxHeightColumn.cellFormatting && !_.isEmpty(maxHeightColumn.cellFormatting) &&
                maxHeightColumn.imageHeight > 0 &&
                maxHeightColumn.imageWidth > 0 &&
                maxHeightColumn.imageSizeId &&
                maxHeightColumn.imageScaleId) {
                rowHeight = maxHeightColumn.imageHeight + ($scope.DEFAULT_CELLPADDING * 2);
                if (rowHeight < DEFAULT_ROWHEIGHT) {
                    rowHeight = DEFAULT_ROWHEIGHT;
                }
            }

            if (multiLineRowHeight > rowHeight) {
                rowHeight = multiLineRowHeight;
            }

            $scope.model.ngGridOptions.rowHeight = rowHeight;
        }

        function setGridAggregateRowHeight() {
            var maxTotalsColumn,
                rowDataHeaderHeight = DEFAULT_ROWHEIGHT,
                aggregateRowHeight = DEFAULT_ROWHEIGHT,
                showSubTotals = $scope.dataGridOptions.aggregateInfo ? $scope.dataGridOptions.aggregateInfo.showSubTotals : false,
                showGrandTotals = $scope.dataGridOptions.aggregateInfo ? $scope.dataGridOptions.aggregateInfo.showGrandTotals : false;

            if (!$scope.model.ngGridOptions) {
                return;
            }

            // Find the maximum number of totals across all columns
            maxTotalsColumn = _.maxBy($scope.dataGridOptions.columnDefinitions, function (c) {
                return c.totals ? c.totals.length : 0;
            });

            if (maxTotalsColumn &&
                maxTotalsColumn.totals) {
                if (showSubTotals) {
                    aggregateRowHeight = (maxTotalsColumn.totals.length) * AGGREGATE_ROW_LINEHEIGHT + $scope.DEFAULT_CELLPADDING;
                }

                if (showGrandTotals) {
                    rowDataHeaderHeight = (maxTotalsColumn.totals.length) * AGGREGATE_ROW_LINEHEIGHT + $scope.DEFAULT_CELLPADDING;
                }
            }

            if (aggregateRowHeight < DEFAULT_ROWHEIGHT) {
                aggregateRowHeight = DEFAULT_ROWHEIGHT;
            }

            if (rowDataHeaderHeight < DEFAULT_ROWHEIGHT) {
                rowDataHeaderHeight = DEFAULT_ROWHEIGHT;
            }

            if (angular.isDefined($scope.dataGridOptions.moreDataAvailable) &&
                $scope.dataGridOptions.moreDataAvailable) {
                //only increase the agregate row height when the total or subtotal is set
                if (maxTotalsColumn &&
                    maxTotalsColumn.totals &&
                    maxTotalsColumn.totals > 0) {
                    aggregateRowHeight += AGGREGATE_ROW_LINEHEIGHT;
                }


            }


            $scope.model.ngGridOptions.aggregateRowHeight = aggregateRowHeight;
            $scope.model.ngGridOptions.rowDataHeaderHeight = rowDataHeaderHeight;
        }

        function getFormattedCellFormattingData(row, col) {
            var formattingData,
                cellData = null,
                columnDefinition = col.colDef.spColumnDefinition;

            // Return null if the column has no cell formatting
            if (!columnDefinition || !columnDefinition.cellFormatting) {
                return null;
            }

            // Get the cell data
            if (row.entity &&
                row.entity.cells) {
                cellData = row.entity.cells[columnDefinition.cellIndex];
            }

            if (!cellData ||
                angular.isUndefined(cellData.formattingIndex)) {
                return null;
            }

            // Get the formatting data
            formattingData = columnDefinition.cellFormatting[cellData.formattingIndex];
            // Get the formatting data
            if (!formattingData) {
                return null;
            }

            return {
                columnDefinition: columnDefinition,
                cellData: cellData,
                formattingData: formattingData
            };
        }

        function setGridColumDefs() {
            if (!$scope.dataGridOptions.columnDefinitions) {
                $scope.model.columnDefinitions = [];
                return;
            }


            // Set the column definitions
            $scope.model.columnDefinitions = $scope.dataGridOptions.columnDefinitions.map(function (c) {
                var cellTemplate = null;

                // Load the appropriate cell template
                if (c.cellFormatting && !_.isEmpty(c.cellFormatting)) {
                    if (c.imageWidth > 0 && c.imageHeight > 0 && c.imageSizeId && c.imageScaleId) {
                        cellTemplate = $templateCache.get('dataGrid/formattedCellImageTemplate.tpl.html');
                    } else if (c.cellFormatting[0] && c.cellFormatting[0].bounds) {
                        cellTemplate = $templateCache.get('dataGrid/progressBarCellTemplate.tpl.html');
                    }

                    else {
                        cellTemplate = $templateCache.get('dataGrid/formattedCellTemplate.tpl.html');                        
                    }
                }

                // Custom cell template
                if (c.customCellTemplate) {
                    cellTemplate = c.customCellTemplate;
                }

                if (cellTemplate === null) {
                    cellTemplate = $templateCache.get('dataGrid/defaultCellTemplate.tpl.html');                    
                }                

                return {
                    spColumnDefinition: c,
                    field: c.cellIndex >= 0 ? 'cells[' + c.cellIndex + '].value' : '',
                    displayName: c.displayName,
                    visible: angular.isDefined(c.visible) ? c.visible : true,
                    cellTemplate: cellTemplate,
                    headerCellTemplate: $templateCache.get('dataGrid/headerCellTemplate.tpl.html'),
                    inlineEditingCellTemplate: $templateCache.get('dataGrid/defaultCellInlineEditingTemplate.tpl.html'),
                    resizable: true
                };
            });
        }

        function onKeyPress(event) {
            var newChar;

            if (!event || $scope.dataGridOptions.isInlineEditing) {
                return;
            }

            newChar = String.fromCharCode(event.which);

            if (_.isNull(searchText) ||
                _.isUndefined(searchText)) {
                searchText = '';
            }

            searchText = searchText + newChar;

            if (!_.isEmpty(searchText)) {
                debouncedSearch();
            }

            //if only one row in
        }
    }
}());