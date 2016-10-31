// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.dataGridTest', ['mod.common.ui.spDataGrid', 'mod.common.spEntityService', 'spApps.reportServices', 'mod.common.ui.spEntityComboPicker'])
        .controller('dataGridTestController', ['$scope', 'spReportService', 'spEntityService', function ($scope, spReportService, spEntityService) {
            $scope.alignmentOptions = ['Left', 'Right', 'Centre'];

            $scope.contextMenu = {
                menuItems: [
                    {
                        text: 'Sort A-Z',
                        icon: 'assets/images/16x16/SortAscending.png',
                        type: 'click'
                    },
                    {
                        text: 'Sort Z-A',
                        icon: 'assets/images/16x16/SortDescending.png',
                        type: 'click'
                    },
                    {
                        text: 'Cancel Sort',
                        icon: 'assets/images/16x16/SortCanceling.png',
                        type: 'click'
                    },
                    {
                        text: 'Sort Options',
                        icon: 'assets/images/16x16/SortOption.png',
                        type: 'click'
                    },
                    {
                        type: 'divider'
                    },
                    {
                        text: 'Group By',
                        icon: 'assets/images/16x16/GroupBy.png',
                        type: 'click'
                    },
                    {
                        text: 'Summarize',
                        icon: 'assets/images/16x16/SummarizeDataGrid.png',
                        type: 'click'
                    },
                    {
                        type: 'divider'
                    },
                    {
                        text: 'Rename Column',
                        icon: 'assets/images/16x16/RenameColumn.png',
                        type: 'click'
                    },
                    {
                        text: 'Format Column',
                        icon: 'assets/images/16x16/Format.png',
                        type: 'click'
                    },
                    {
                        text: 'Remove Column',
                        icon: 'assets/images/16x16/Delete.png',
                        type: 'click'
                    },
                    {
                        text: 'Show Totals',
                        icon: 'assets/images/16x16/ShowTotal.png',
                        type: 'click'
                    },
                    {
                        text: 'Edit Calculation',
                        type: 'click'
                    }
                ]
            };

            // Fields
            var gridData = null, thumbNailIdsToSizes = [];

            $scope.pageSizes = [100 , 500, 10000];
            $scope.pageSize = 100;
            $scope.loadingData = false;

            // Selected definition
            $scope.definitionOptions = {                
                selectedEntityId: 'test:allFieldsTestType',
                selectedEntity: null,
                entityTypeId: 'core:definition'                
            };

            // Configured conditions
            $scope.conditions = [];            

            // Configured columns
            $scope.columnDefinitions = [
                {
                    columnId: 10,
                    displayName: 'Id',
                    type: 'Int32',
                    hideValue: false,
                    imageWidth: 0,
                    imageHeight: 0,
                    imageSizeOptions: {
                        selectedEntityId: 0,
                        entityTypeId: 'console:thumbnailSizeEnum'
                    },
                    imageScaleOptions: {
                        selectedEntityId: 0,
                        entityTypeId: 'core:imageScaleEnum'
                    },
                    visible: true,
                    totals: [
                       {
                           displayName: 'Sum'
                       },
                       {
                           displayName: 'Average'
                       }
                    ]
                },
                {
                    columnId: 20,
                    displayName: 'Name',                    
                    hideValue: false,
                    imageWidth: 0,
                    imageHeight: 0,
                    imageSizeOptions: {
                        selectedEntityId: 0,
                        entityTypeId: 'console:thumbnailSizeEnum'
                    },
                    imageScaleOptions: {
                        selectedEntityId: 0,
                        entityTypeId: 'core:imageScaleEnum'
                    },
                    visible: false
                },
                {
                    columnId: 30,
                    displayName: 'Description',                    
                    hideValue: false,
                    imageWidth: 0,
                    imageHeight: 0,
                    imageSizeOptions: {
                        selectedEntityId: 0,
                        entityTypeId: 'console:thumbnailSizeEnum'
                    },
                    imageScaleOptions: {
                        selectedEntityId: 0,
                        entityTypeId: 'core:imageScaleEnum'
                    },
                    visible: true,
                    totals: [
                        {
                            displayName: 'Sum'
                        },
                        {
                            displayName: 'Average'
                    }
                    ]
                }];            

            // Handle grid events
            $scope.$on('spDataGridEventDataRequested', function (event, eventArgs) {
                var index = eventArgs.index;
                event.stopPropagation();                

                if (index >= gridData.length) {
                    console.log('no more data');
                    return;
                }

                var selects, query;

                selects = [
                    { field: 'name', displayAs: 'Name' },
                    { field: 'description', displayAs: 'Description' }
                ];

                query = {
                    root: { id: $scope.definitionOptions.selectedEntity.id() },
                    selects: selects,
                    conds: []
                };

                $scope.loadingData = true;

                spReportService.runQuery(query).then(function (results) {
                    $scope.loadingData = false;
                    gridData = results.data;

                    $scope.gridOptions.rowData = $scope.gridOptions.rowData.concat(getGridData(gridData.slice(index, index + $scope.pageSize)));
                });
            });

            $scope.$on('spDataGridEventGridDoubleClicked', function () {
                console.log('Grid doubleclicked');
            });

            // The actual grid options
            $scope.gridOptions = {
                cellFormatting: [],
                rowData: [],
                selectedItems: [],
                sortInfo: [{ columnId: 10, sortDirection: 'asc' }, { columnId: 20, sortDirection: 'asc' }],
                columnDefinitions: [],
                multiSelect: true,
                getActionExecutionContext: function (action, ids) {
                    return {
                        state: action.state,
                        isEditMode: false,
                        selectionEntityIds: ids,
                        refreshDataCallback: function () {
                            $scope.initialize();
                        }
                    };
                }
            };            

            // Methods
            $scope.initialize = function () {
                var selects, query;

                if (thumbNailIdsToSizes.length === 0 ||
                    !$scope.definitionOptions.selectedEntity) {
                    return;
                }

                selects = [
                    { field: 'name', displayAs: 'Name' },
                    { field: 'description', displayAs: 'Description' }
                ];

                query = {
                    root: { id: $scope.definitionOptions.selectedEntity.id() },
                    selects: selects,
                    conds: []
                };

                $scope.loadingData = true;

                spReportService.runQuery(query).then(function (results) {
                    gridData = results.data;
                    $scope.loadingData = false;
                    $scope.gridOptions.rowData = getGridData(gridData.slice(0, $scope.pageSize));
                });
            };
            
            $scope.createNewCondition = function () {
                var newCondition = {
                    bounds: {
                        lower: '',
                        upper: '',
                    },
                    backgroundColor: '#FFFFFF',
                    foregroundColor: '#000000',
                    imagePickerOptions: {
                        selectedEntityId: 0,
                        entityTypeId: 'core:photoFileType'
                    }
                };
                
                $scope.conditions.push(newCondition);
                $scope.$watch('conditions[' + ($scope.conditions.length - 1) + '].imagePickerOptions.selectedEntityId', $scope.refreshGrid);
            };
            
            function colorStringToObj(color) {
                if (color) {
                    var r, g, b;

                    r = parseInt(color.substring(1, 3),16);
                    g = parseInt(color.substring(3, 5),16);
                    b = parseInt(color.substring(5), 16);

                    return {
                        r: r,
                        g: g,
                        b: b,
                        a: 255
                    };
                } else {
                    return null;
                }
            }

            function findCellFormatting(cellFormattings, cellFormat) {
                var existingFormat = _.find(cellFormattings, function (cf) {
                    return _.isEqual(cf, cellFormat);
                });
                return existingFormat;
            }

            function getThumbnailWidth(id) {
                var thumbNailEntity = thumbNailIdsToSizes[id];
                if (thumbNailEntity) {
                    return thumbNailEntity.getThumbnailWidth();
                } else {
                    return 0;
                }
            }

            function getThumbnailHeight(id) {
                var thumbNailEntity = thumbNailIdsToSizes[id];
                if (thumbNailEntity) {
                    return thumbNailEntity.getThumbnailHeight();
                } else {
                    return 0;
                }
            }            

            $scope.refreshGrid = _.debounce(refreshGridInternal, 1000);

            $scope.group = function () {
                //$scope.gridOptions.columnDefinitions[1].visible = false;
                $scope.gridOptions.aggregateInfo = {
                    contextMenu: $scope.contextMenu,
                    groupedColumns: [{ columnId: 10 }],
                    showGrandTotals: true,
                    showSubTotals: true,
                    rollup: true
                };
            };

            // Refresh the grid
            function refreshGridInternal() {
                var cellFormattings = [],
                    columnDefinitions,
                    row,
                    cell,
                    condition,
                    cellFormat,                    
                    existingFormat,
                    formatIndex,
                    r,
                    c,
                    ci;
                
                columnDefinitions = $scope.columnDefinitions.map(function (cd) {
                    return {
                        columnId: cd.columnId,
                        cellIndex: _.indexOf($scope.columnDefinitions, cd),
                        resizable: true,
                        type: cd.type,
                        displayName: cd.displayName,
                        hideValue: cd.hideValue,
                        hasFormatting: cd.hasFormatting,
                        imageScaleId: cd.imageScaleOptions.selectedEntityId,
                        imageSizeId: cd.imageSizeOptions.selectedEntityId,
                        imageWidth: getThumbnailWidth(cd.imageSizeOptions.selectedEntityId),
                        imageHeight: getThumbnailHeight(cd.imageSizeOptions.selectedEntityId),
                        contextMenu: $scope.contextMenu,
                        visible: cd.visible,
                        cellFormatting: [],
                        totals: cd.totals
                    };
                });

                // Conditions
                if ($scope.conditions.length) {
                    for (r = 0; r < $scope.gridOptions.rowData.length; r++) {
                        row = $scope.gridOptions.rowData[r];
                        for (c = 0; c < columnDefinitions.length; c++) {
                            cell = row.cells[columnDefinitions[c].cellIndex];
                            cell.formattingIndex = -1;
                            cellFormattings = columnDefinitions[c].cellFormatting;

                            for (ci = 0; ci < $scope.conditions.length; ci++) {
                                condition = $scope.conditions[ci];

                                if (condition.value === cell.value) {
                                    
                                    cellFormat = {
                                        backgroundColor: colorStringToObj(condition.backgroundColor),
                                        foregroundColor: colorStringToObj(condition.foregroundColor),                                        
                                        imageId: condition.imagePickerOptions.selectedEntityId
                                    };

                                    if (condition.bounds &&
                                        condition.bounds.lower >= 0 &&
                                        condition.bounds.upper >= 0 &&
                                        condition.bounds.upper > condition.bounds.lower) {
                                        cellFormat.bounds = condition.bounds;
                                    }

                                    cellFormat.alignment = condition.alignment;

                                    existingFormat = findCellFormatting(cellFormattings, cellFormat);

                                    if (existingFormat) {
                                        formatIndex = _.indexOf(cellFormattings, existingFormat);
                                    } else {
                                        cellFormattings.push(cellFormat);
                                        formatIndex = cellFormattings.length - 1;
                                    }

                                    cell.formattingIndex = formatIndex;
                                }
                            }
                        }
                    }
                }

                $scope.$apply(function () {                                        
                    $scope.gridOptions.columnDefinitions = columnDefinitions;
                });
            }
                        
            // Map the report data to the grid data
            function getGridData(data) {                
                return data.map(function (d) {                    
                    return {
                        hideRow: true,
                        cells: [
                            {
                                value: d.item[0].value
                            },
                            {
                                value: d.item[1].value                                
                            },
                            {
                                value: d.item[2].value                                
                            }                            
                        ]
                    };
                });
            }           

            // Get the thumbnail sizes
            spEntityService.getInstancesOfType('console:thumbnailSizeEnum', 'console:thumbnailWidth,console:thumbnailHeight', true)
                .then(function (items) {
                    var i, item;

                    thumbNailIdsToSizes = {};

                    for (i = 0; i < items.length; i++) {
                        item = items[i];

                        thumbNailIdsToSizes[item.id] = item.entity;
                    }

                    $scope.initialize();
                });

            $scope.$watch('columnDefinitions[0].imageSizeOptions.selectedEntityId', $scope.refreshGrid);
            $scope.$watch('columnDefinitions[0].imageScaleOptions.selectedEntityId', $scope.refreshGrid);
            $scope.$watch('columnDefinitions[1].imageSizeOptions.selectedEntityId', $scope.refreshGrid);
            $scope.$watch('columnDefinitions[1].imageScaleOptions.selectedEntityId', $scope.refreshGrid);
            $scope.$watch('definitionOptions.selectedEntity', $scope.initialize);
            $scope.$watch('conditions.length', $scope.refreshGrid);
        }]);
}());