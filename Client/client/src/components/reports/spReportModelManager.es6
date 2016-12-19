// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp, spEntity */

/**
 * Module implementing the report model manager service.
 * @module spReportModelManager
 */

(function () {
    'use strict';

    angular.module('mod.ui.spReportModelManager',
        ['spApps.reportServices', 'mod.ui.spReportFilters', 'sp.common.filters', 'mod.ui.spReportConstants',
            'mod.common.spTenantSettings', 'sp.navService', 'mod.ui.spReportAggregateDataManager',
            'mod.common.spEntityService', 'mod.common.alerts', 'mod.common.ui.spTypeOperatorService']);

    angular.module('mod.ui.spReportModelManager')
        .factory('spReportModelManager', spReportModelManagerService);

    /* @ngInject */
    function spReportModelManagerService(reportPageSize, reportMobilePageSize, reportMobileMaxCols, screenReportMobilePageSize,
                                  contextMenu, spReportService, spTenantSettings, spNavService, $q, $timeout,
                                  aggregateRowContextMenu, spReportAggregateDataManager, groupValueColumnIdPrefix,
                                  spEntityService, $filter, spAlertsService, spTypeOperatorService) {

        return spReportModelManager;

        function spReportModelManager(model) {

            var currencySymbol = '$';
            var templateReportIds = {};
            var invalidReportInfos = [];
            var nameFieldEntity;
            var aggregateMethodEnumOptions = {
                aggCountWithValues: {
                    name: 'Count',
                    order: 0
                },
                aggCountUniqueItems: {
                    name: 'Count unique',
                    order: 1
                },
                aggCount: {
                    name: 'Count all',
                    order: 2
                },
                aggSum: {
                    name: 'Sum',
                    order: 3
                },
                aggAverage: {
                    name: 'Average',
                    order: 4
                },
                aggMax: {
                    name: 'Max',
                    order: 5
                },
                aggMin: {
                    name: 'Min',
                    order: 6
                },
                aggCountUniqueNotBlanks: {
                    name: 'Count unique no blanks',
                    order: 7
                },
                aggList: {
                    name: 'List',
                    order: 8
                },
            };
            var quickSearchChanged;
            var aggregateRows;
            var debouncedQuickSearch = _.debounce(quickSearch, 500);            
            var exports = {
                loadAnalyzerFieldDefinitions: loadAnalyzerFieldDefinitions,
                refreshReportData: refreshReportData,
                getReportData: getReportData,
                getReportDataByReportEntity: getReportDataByReportEntity,
                loadReportData: loadReportData,
                getRowData: getRowData,
                findRowIndex: findRowIndex,
                getModel: getModel,
                getNavServiceData: getNavServiceData,
                resetModel: resetModel,
                resetBasicModel: resetBasicModel,
                resetPagingInfo: resetPagingInfo,
                resetSelectedItems: resetSelectedItems,
                initializeFromModel: initializeFromModel,
                createModel: createModel,
                saveReport: saveReport,
                convertToNativeType: convertToNativeType,
                getAvailableChoiceEntities: getAvailableChoiceEntities,
                getInlineTypePickerReportId: getInlineTypePickerReportId,
                updateSelectedItemIndexes: updateSelectedItemIndexes,
                setSelectedItems: setSelectedItems,
                setAggregateRows: setAggregateRows,
                getAggregateRows: getAggregateRows,
                mergeFilterConditions: mergeFilterConditions,
                updateInlineEditingMetadata: updateInlineEditingMetadata
            };

            // Get the currency symbol for the current tenant
            spTenantSettings.getCurrencySymbol().then(function (symbol) {
                if (symbol) {
                    currencySymbol = symbol;
                } else {
                    currencySymbol = '$';
                }
            });

            return exports;

            ///////////////////////////////////////////////////////////////
            // Implementation functions

            // Initialize analyzer fields from metadata
            function loadAnalyzerFieldDefinitions(metadata) {
                if (!metadata) {
                    return;
                }

                // Get the analyzer columns in the correct order
                var orderedAnalyzerColumns = getOrderedKeys(metadata.anlcols, 'ord');

                // Create the analyzer fields
                model.analyzerOptions.analyzerFields = _.map(orderedAnalyzerColumns, function (value) {
                    var anlcol = metadata.anlcols[value.id],
                        operators,
                        convertedValue = null,
                        effectiveType,
                        operator,
                        resultField = {},
                        operatorsType = anlcol.anltype || anlcol.type;

                    operators = spTypeOperatorService.getApplicableOperators(operatorsType);

                    if (anlcol.oper &&
                        anlcol.oper !== 'Unspecified') {
                        // Find the operator
                        operator = _.find(operators, function (o) {
                            return o.id === anlcol.oper;
                        });
                    }

                    if (!operator &&
                        anlcol.doper) {
                        // Find the default operator
                        operator = _.find(operators, function (o) {
                            return o.id === anlcol.doper;
                        });
                    }

                    if (!operator) {
                        // Fallback to unspecified as the operator
                        operator = _.find(operators, function (o) {
                            return o.id === 'Unspecified';
                        });
                    }

                    if (operator &&
                        operator.argCount > 0) {
                        effectiveType = operator.type || anlcol.type;

                        if (anlcol.value) {
                            convertedValue = exports.convertToNativeType(effectiveType, anlcol.value);
                        } else if (anlcol.values) {
                            convertedValue = exports.convertToNativeType(effectiveType, anlcol.values);
                        }
                    }

                    //if current report condition is linked with report column, use report column title to replace report condition title
                    var title = anlcol.rcolid && sp.result(metadata.rcols, [anlcol.rcolid, 'title']);
                    title = title || anlcol.title;

                    // Return the analyzer field
                    resultField = {
                        name: title,
                        operators: operators,
                        operator: operator ? operator.id : 'Unspecified',
                        defaultOperator: anlcol.doper,
                        type: anlcol.type,
                        value: convertedValue,

                        // Tag extra members
                        tag: {
                            id: value.id,
                            anlcol: anlcol
                        }
                    };

                    if (anlcol.anltype) {
                        resultField.operatorsType = anlcol.anltype;
                    }

                    switch (anlcol.anltype ? anlcol.anltype : anlcol.type) {
                        case 'ChoiceRelationship':
                            resultField.availableEntities = exports.getAvailableChoiceEntities(anlcol.tid);
                            break;
                        case 'InlineRelationship':
                        case 'UserInlineRelationship':
                        case 'StructureLevels':
                            resultField.entityTypeId = anlcol.tid;
                            resultField.pickerReportId = anlcol.respickerid || exports.getInlineTypePickerReportId(anlcol.tid);
                            resultField.filteredEntityIds = anlcol.filtereids;
                            break;
                    }

                    return resultField;
                });
            }

            // Given an object, return an ordered list of the
            // key names.
            function getOrderedKeys(obj, orderBy) {
                var orderedKeys = [];

                // Enumerate all the own properties
                _.forOwn(obj, function (value, key) {
                    if (_.has(value, orderBy)) {
                        orderedKeys.push({id: key, ordinal: value[orderBy]});
                    }
                });

                return _.sortBy(orderedKeys, 'ordinal');
            }

            // Get the grid filter for the specified column.
            function getColumnFilterFunction(rcol, valRule) {
                var places, type, anpat;

                if (!rcol) {
                    return null;
                }

                type = rcol.type;

                if (!type) {
                    return null;
                }

                if (!valRule) {
                    valRule = {};
                }

                if (rcol.colerr) {
                    return function () {
                        return rcol.colerr;
                    };
                }

                switch (type) {
                    case spEntity.DataType.Currency:
                        places = angular.isDefined(valRule.places) ? valRule.places : 3;
                        return function (value) {
                            return $filter('spCurrency')(value, currencySymbol, places, valRule.prefix || '', valRule.suffix || '');
                        };
                    case spEntity.DataType.Decimal:
                        places = angular.isDefined(valRule.places) ? valRule.places : 3;
                        return function (value) {
                            return $filter('spDecimal')(value, places, valRule.prefix || '', valRule.suffix || '');
                        };
                    case spEntity.DataType.Int32:
                        return function (value) {
                            return $filter('spNumber')(value, valRule.prefix || '', valRule.suffix || '', rcol.anpat || '');
                        };
                    case spEntity.DataType.Bool:
                        return $filter('spBoolean');
                    case 'RelatedResource':
                    case 'ChoiceRelationship':
                    case 'InlineRelationship':
                    case 'UserInlineRelationship':
                        if (valRule.entitylistcolfmt === "stackedList" && rcol.aggcol) {
                            return value => {
                                return $filter('relatedResource')(value, { isStacked: true, lines: valRule.lines });
                            };
                        } else {
                            return $filter('relatedResource');
                        }
                    case 'Image':
                        return $filter('relatedResource');
                    case spEntity.DataType.Time:
                        return function (value) {
                            return $filter('spTime')(value, valRule.datetimefmt || '');
                        };
                    case spEntity.DataType.Date:
                        return function (value) {
                            return $filter('spDate')(value, valRule.datetimefmt || '');
                        };
                    case spEntity.DataType.DateTime:
                        return function (value) {
                            return $filter('spDateTime')(value, valRule.datetimefmt || '');
                        };
                    case 'StructureLevels':
                        if (valRule.entitylistcolfmt === "stackedList" && rcol.aggcol) {
                            return value => {
                                return $filter('structureLevels')(value, { isStacked: true, lines: valRule.lines });
                            };
                        } else {
                            return $filter('structureLevels');
                        }                        
                }

                return null;
            }

            function convertToInt(val) {
                var convertedValue = _.parseInt(val, 10);
                return _.isNaN(convertedValue) ? 0 : convertedValue;
            }

            //load report meta data and display the invalid report information
            function displayInvalidReportInformation(metadata, isEditMode) {
                if (metadata.invalid) {
                    var invalidNodesInfo = '';
                    var invalidColumnsInfo = '';
                    var invalidConditionsInfo = '';

                    _.forOwn(metadata.invalid.nodes, function (value, key) {
                        invalidReportInfos.push({type: 'node', id: convertToInt(key)});
                        invalidNodesInfo += ' ' + value;
                    });
                    if (invalidNodesInfo.length > 0)
                        invalidNodesInfo = "Invalid relationship(s):" + invalidNodesInfo + "\n";

                    _.forOwn(metadata.invalid.columns, function (value, key) {
                        invalidReportInfos.push({type: 'column', id: convertToInt(key)});
                        invalidColumnsInfo += ' ' + value;
                    });
                    if (invalidColumnsInfo.length > 0)
                        invalidColumnsInfo = "Invalid column(s):" + invalidColumnsInfo + "\n";

                    _.forOwn(metadata.invalid.conditions, function (value, key) {
                        model.withInvalidReportCondition = true;
                        invalidReportInfos.push({type: 'condition', id: convertToInt(key)});
                        invalidConditionsInfo += ' ' + value;
                    });
                    if (invalidConditionsInfo.length > 0)
                        invalidConditionsInfo = "Invalid condition(s):" + invalidConditionsInfo + "\n";

                    if (isEditMode === true) {
                        if (invalidNodesInfo.length > 0 || invalidColumnsInfo.length > 0 || invalidConditionsInfo.length > 0) {
                            spAlertsService.addAlert("The current report contains the following invalid objects", {
                                severity: spAlertsService.sev.Warning,
                                page: 'reportBuilder'
                            });
                            if (invalidNodesInfo.length > 0) {
                                spAlertsService.addAlert(invalidNodesInfo, {
                                    severity: spAlertsService.sev.Warning,
                                    page: 'reportBuilder'
                                });
                            }
                            if (invalidColumnsInfo.length > 0) {
                                spAlertsService.addAlert(invalidColumnsInfo, {
                                    severity: spAlertsService.sev.Warning,
                                    page: 'reportBuilder'
                                });
                            }
                            if (invalidConditionsInfo.length > 0) {
                                spAlertsService.addAlert(invalidConditionsInfo, {
                                    severity: spAlertsService.sev.Warning,
                                    page: 'reportBuilder'
                                });
                            }
                            spAlertsService.addAlert("These invalid objects will be automatically fixed when saved.", {
                                severity: spAlertsService.sev.Warning,
                                page: 'reportBuilder'
                            });

                            model.invalidReportInfos = invalidReportInfos;
                        }
                    }

                }
            }

            // Load the report meta data
            function loadReportMetadata(metadata, params) {
                params = params || {};

                model.aggregateDataManager = spReportAggregateDataManager(metadata);
                model.reportMetadata = metadata;

                if (!params.updateMetadataOnly) {
                    loadColumnDefinitions(metadata, params);
                    loadAggregateInfo(metadata);
                    loadSortInfo(metadata);
                    model.reportMetadata = metadata;
                }
            }

            // Set column image settings
            function setColumnImageSettings(columnDefinition, metadata, columnId) {
                var valRule;
                var cfRule = null;
                if (!columnId || !metadata) {
                    return;
                }

                if (columnDefinition &&
                    columnDefinition.type === 'Image') {
                    if (metadata.valrules) {
                        valRule = metadata.valrules[columnId];
                    }

                    columnDefinition.imageScaleId = valRule ? (valRule.scaleid || 'core:scaleImageProportionally') : 'core:scaleImageProportionally';
                    columnDefinition.imageSizeId = valRule ? (valRule.sizeid || 'console:iconThumbnailSize') : 'console:iconThumbnailSize';
                    columnDefinition.imageWidth = valRule ? (valRule.imgw || 16) : 16;
                    columnDefinition.imageHeight = valRule ? (valRule.imgh || 16) : 16; 
                    columnDefinition.hideValue = true;

                    // Create a dummy format to trick the grid into using the icon template
                    columnDefinition.cellFormatting.Image = {};
                } else {
                    if (metadata.valrules) {
                        valRule = metadata.valrules[columnId];
                    }

                    if (metadata.cfrules) {
                        cfRule = metadata.cfrules[columnId];
                    }

                    if (!valRule) {
                        return;
                    }

                    columnDefinition.imageScaleId = valRule.scaleid;
                    columnDefinition.imageSizeId = valRule.sizeid;
                    columnDefinition.imageWidth = valRule.imgw;
                    columnDefinition.imageHeight = valRule.imgh;
                    columnDefinition.imageBackgroundColor = valRule.imgbgc;                    

                    //Note: if condition format type is icon and hide the text. the cell alignment will be centre.
                    //this setting will overwrite the valueformat alignment setting.
                    if (cfRule && cfRule.style === 'Icon' && valRule.hideval === true) {
                        _.forOwn(columnDefinition.cellFormatting, function (cf) {
                            cf.alignment = 'Centre';
                        });
                    }


                }
            }

            function buildDefaultFormattingRule(rcol, metadata) {

                //if default formatting rule exists
                if (rcol &&
                    metadata.choice &&
                    metadata.choice[rcol.tid] &&
                    metadata.choice[rcol.tid].length > 0 &&
                    metadata.choice[rcol.tid][0].fmt)
                {
                    var style = metadata.choice[rcol.tid][0].fmt.fgcolor ? 'Highlight' : 'Icon';
                    var rules = _.map(metadata.choice[rcol.tid], "fmt");
                    return {
                        showval: true,
                        rules: rules,
                        style: style
                    };

                } else
                    return null;
            }

            function canEnableHyperlinksForReport(metadata) {
                if (!metadata || !metadata.alias) {
                    return true;
                }

                const alias = metadata.alias;

                return alias !== "console:boardsReport" &&
                    alias !== "console:reportsReport" &&
                    alias !== "console:chartsReport" &&
                    alias !== "console:customFormsReport" &&
                    alias !== "console:screensReport" &&
                    alias !== "core:importConfigReport" &&
                    alias !== "console:workflowReport" &&
                    alias !== "console:workflowRunsReport";
            }


            // Initialize column definitions from metadata
            function loadColumnDefinitions(metadata, params) {
                var columnDefinitions, groupColumns, orderedColumns;

                if (!metadata) {
                    return;
                }

                // Get the columns in the correct order
                orderedColumns = getOrderedKeys(metadata.rcols, 'ord');

                // Create the grid column definitions
                columnDefinitions = _.map(orderedColumns, function (value) {
                    var rcol = metadata.rcols[value.id],
                        valRule,
                        totals,
                        columAggs,
                        columnDefinition;

                    if (metadata.valrules) {
                        valRule = metadata.valrules[value.id];
                    }
                    
                    //if column is choice field and user default formatting rule is set to true,  use choice field formatting.
                    if (rcol && rcol.type === "ChoiceRelationship" &&
                        (   //if column formatting rule is null and column type is choice field, load default formatting rule as init.
                            !angular.isDefined(valRule) || 
                            (angular.isDefined(valRule) &&
                             valRule &&
                             !valRule.disabledefft)
                        )) {
                        var defaultRule = buildDefaultFormattingRule(rcol, metadata);
                        //reset defaultRue to condition format rule
                        if (defaultRule) {

                            if (!metadata.defcfrules)
                                metadata.defcfrules = {};

                            metadata.defcfrules[value.id] = defaultRule;
                        }
                    }


                    if (metadata.rmeta &&
                        metadata.rmeta.aggs) {
                        columAggs = metadata.rmeta.aggs[value.id];

                        totals = _.map(columAggs, function (ca) {
                            var totalDisplayName,
                                aggMethodOption,
                                order = 0;
                            
                            aggMethodOption = aggregateMethodEnumOptions[ca.style];
                            if (aggMethodOption) {
                                totalDisplayName = aggMethodOption.name;
                                order = aggMethodOption.order;
                            }                            

                            return {
                                id: ca.style,
                                displayName: totalDisplayName,
                                order: order
                            };
                        });
                    }

                    const showCellTextAsHyperlinks = !model.isInPicker &&
                        !params.isMobile &&
                        !rcol.aggcol &&
                        rcol.type !== "ChoiceRelationship" &&
                        (nameFieldEntity && rcol.fid === nameFieldEntity.id() || rcol.entityname) &&
                        canEnableHyperlinksForReport(metadata);

                    columnDefinition = {
                        // Column definition members
                        columnId: value.id,
                        displayName: rcol.title,
                        visible: angular.isDefined(rcol.hide) ? !rcol.hide : true,
                        contextMenu: contextMenu,
                        cellIndex: rcol.ord,
                        cellFilterFunc: getColumnFilterFunction(rcol, valRule),
                        cellFormatting: {},
                        type: rcol.type,
                        oprtype: rcol.oprtype,
                        hideValue: (valRule && angular.isDefined(valRule.hideval)) ? valRule.hideval : false,
                        totals: totals,
                        showCellTextAsHyperlinks: showCellTextAsHyperlinks,
                        // Tag extra members
                        tag: {
                            id: value.id,
                            rcol: rcol
                        }
                    };

                    if (valRule) {
                        if (valRule.lines) {
                            columnDefinition.lines = valRule.lines;
                        }

                        if (valRule.align) {
                            columnDefinition.alignment = valRule.align;
                        }
                    }

                    updateColumnCellFormattingSettings(columnDefinition, metadata, value.id);
                    setColumnImageSettings(columnDefinition, metadata, value.id);

                    return columnDefinition;
                });

                // Append hidden group columns
                groupColumns = model.aggregateDataManager ? model.aggregateDataManager.getGroupColumns() : [];

                columnDefinitions = columnDefinitions.concat(_.map(groupColumns, function (gc, index) {
                    var rcol = gc.rcol,
                        columnDefinition;

                    columnDefinition = {
                        // Column definition members
                        columnId: groupValueColumnIdPrefix + gc.id,
                        displayName: rcol.title,
                        visible: false,
                        cellIndex: columnDefinitions.length + index,
                        cellFormatting: {},
                        type: rcol.type,

                        // Tag extra members
                        tag: {
                            isGroupValueColumn: true,
                            collapsed: gc.collapsed,
                            id: gc.id,
                            rcol: rcol
                        }
                    };

                    return columnDefinition;
                }));

                model.gridOptions.columnDefinitions = columnDefinitions;
            }

            // Load the aggregate info from the metadata
            function loadAggregateInfo(metadata) {
                var groupedColumns = [];

                if (!metadata || !metadata.rmeta) {
                    model.gridOptions.aggregateInfo = {rollup: true};
                    return;
                }

                _.forEach(model.gridOptions.columnDefinitions, function (cd) {
                    if (cd &&
                        cd.tag &&
                        cd.tag.isGroupValueColumn) {
                        groupedColumns.push({
                            columnId: cd.columnId,
                            collapsed: cd.tag.collapsed
                        });
                    }
                });

                model.gridOptions.aggregateInfo = {
                    rollup: true,
                    contextMenu: aggregateRowContextMenu,
                    showSubTotals: metadata.rmeta.showst,
                    showGrandTotals: metadata.rmeta.showgt,
                    showCount: metadata.rmeta.showcnt,
                    showAggregateLabel: metadata.rmeta.showlbl,
                    showOptionLabels: !sp.isNullOrUndefined(metadata.rmeta.showoplbl) ? metadata.rmeta.showoplbl : true,
                    groupedColumns: groupedColumns,
                    aggregateData: model.aggregateDataManager
                };
            }

            // Load the sort info from the metadata
            function loadSortInfo(metadata) {
                if (!metadata) {
                    return;
                }

                model.gridOptions.sortInfo = _.map(metadata.sort, function (si) {
                    return {
                        columnId: si.colid,
                        sortDirection: angular.lowercase(si.order) === 'ascending' ? 'asc' : 'desc'
                    };
                });
            }

            // Add hidden group rows.
            // This is to force the grid to show groups even
            // if the groups have no data.
            function addHiddenAggValueRows(rowData, startIndex) {
                _.forEach(model.aggregateDataManager.getAllGroupValues(), function (groupValues, index) {
                    var result = {
                        eid: -1,
                        cells: [],
                        isHiddenAggValueRow: true,
                        rowIndex: startIndex + index
                    };

                    // Add a null for each column
                    _.forEach(model.reportMetadata.rcols, function () {
                        var cellResult = {
                            value: null
                        };
                        result.cells.push(cellResult);
                    });

                    // Add the group values
                    _.forEach(groupValues, function (gv) {
                        var cellResult = {
                            value: gv
                        };
                        result.cells.push(cellResult);
                    });

                    rowData.push(result);
                });
            }

            // Load the report grid data
            function loadReportGridData(gridData, clearData) {
                var selectedItemsWithoutIndex,
                    startIndex = clearData ? 0 : model.gridOptions.rowData.length,
                    rowData = [],
                    isRollupReport = model.aggregateDataManager ? model.aggregateDataManager.hasRollupData() : false;                

                if (clearData) {
                    // Reset the selected item row indexes
                    _.forEach(model.selectedItems, function (si) {
                        si.rowIndex = -1;
                    });

                    // Add hidden rows to report that only have the group values
                    // This is to force the grid to perform a grouping
                    // and show the groups even if they contain no data.
                    if (isRollupReport) {
                        addHiddenAggValueRows(rowData, startIndex);
                        startIndex = rowData.length;
                    }
                }

                selectedItemsWithoutIndex = _.filter(model.selectedItems, function (si) {
                    return _.isUndefined(si.rowIndex) || si.rowIndex < 0;
                });                

                _.forEach(gridData, function (row, index) {
                    var imageFormattingIndex,
                        imageId,
                        selectedItem,
                        groupData,
                        groupValues,
                        result = {
                            eid: row.eid,
                            obj: row.obj,
                            cells: [],
                            rowIndex: startIndex + index
                        };                    

                    if (selectedItemsWithoutIndex.length) {
                        // Check if any of the selected items match
                        selectedItem = _.find(selectedItemsWithoutIndex, function (si) {
                            return si.eid === row.eid || si.idP === row.eid;
                        });
                        if (selectedItem) {
                            selectedItem.rowIndex = result.rowIndex;
                        }
                    }

                    _.forEach(row.values, function (rowValue, columnIndex) {
                        var cellResult = {}, columnDef;

                        if (rowValue.val) {
                            cellResult.value = rowValue.val;
                        } else if (rowValue.vals) {
                            cellResult.value = rowValue.vals;
                        }

                        // Have a column formatting index
                        if (angular.isDefined(rowValue.cfidx)) {
                            cellResult.formattingIndex = rowValue.cfidx;
                        } else {
                            columnDef = model.gridOptions.columnDefinitions[columnIndex];
                            if (columnDef) {
                                if (columnDef.type === 'Image') {
                                    // If the type is an image id then
                                    // setup a custom column image formatting
                                    imageId = _.first(_.keys(cellResult.value));
                                    if (imageId) {
                                        imageFormattingIndex = 'Image' + imageId;
                                        columnDef.cellFormatting[imageFormattingIndex] = {
                                            imageId: imageId
                                        };
                                        cellResult.formattingIndex = imageFormattingIndex;

                                        //get image background color
                                        //the format of cell value is imageName#backgroundColor e.g. screen_default.svg#ff00b4a5
                                        var cellResultValue = _.first(_.values(cellResult.value));
                                        if (cellResultValue && 
                                            _.split(cellResultValue,'#') &&
                                            _.split(cellResultValue, '#')[1])
                                        {
                                            cellResult.backgroundColor = _.split(cellResultValue, '#')[1];
                                        }
                                    }
                                } else {
                                    if (columnDef.cellFormatting && !_.isEmpty(columnDef.cellFormatting) &&
                                        columnDef.cellFormatting[0] &&
                                        columnDef.cellFormatting[0].bounds) {
                                        cellResult.formattingIndex = 0;
                                    }
                                }
                            }
                        }

                        result.cells.push(cellResult);
                    });

                    if (isRollupReport) {
                        groupData = model.aggregateDataManager.getGroupColumnsData(row);
                        groupValues = model.aggregateDataManager.getGroupValues(groupData);

                        _.forEach(groupValues, function (v) {
                            var cellResult = {
                                value: v
                            };
                            result.cells.push(cellResult);
                        });
                    }

                    rowData.push(result);
                });

                if (clearData) {
                    // Reset the scroll position
                    if (model.gridOptions.dataGrid) {
                        model.gridOptions.dataGrid.scrollToTop();
                    }
                    model.gridOptions.rowData = rowData;                    
                } else {
                    // Append the new rows to the existing rows
                    model.gridOptions.rowData = model.gridOptions.rowData.concat(rowData);
                }
            }

            function getInlineEditingMetadataForEntities(entityIds) {
                if (_.isEmpty(entityIds) || !model.isInlineEditing) {
                    return;
                }

                if (!model.inlineEditingMetadata) {
                    model.inlineEditingMetadata = {};
                }

                entityIds = _.reject(entityIds,
                    id => _.has(model.inlineEditingMetadata, id));
                entityIds = _.take(_.uniq(entityIds), 100);

                if (_.isEmpty(entityIds)) {
                    return;
                }                

                spEntityService.getEntities(entityIds,
                        'canModify, isOfType.console:defaultEditForm.id',
                        { hint: 'getInlineEditingMetadata', batch: true })
                    .then(function(entities) {
                        _.forEach(entities,
                            e => {
                                const formId = sp.result(e, 'isOfType.0.defaultEditForm.id');                                
                                model.inlineEditingMetadata[e.id()] = {
                                    formId: formId,
                                    canModify: e.canModify
                                };                                                                
                            });
                    });
            }

            function updateColumnCellFormattingSettings(columnDefinition, metadata, columnId) {
                var cfRule,
                    defcfRule,
                    valRule,
                    rcol,
                    colHideVal = false,
                    condHideVal = false,
                    cellFormattings = {};

                if (!columnDefinition || !metadata || (!metadata.cfrules && !metadata.defcfrules) || !columnId) {
                    return;
                }

                rcol = metadata.rcols[columnId];
                if (metadata.valrules) {
                    valRule = metadata.valrules[columnId];
                }

                if (metadata.cfrules) {
                    cfRule = metadata.cfrules[columnId];
                }

                if (metadata.defcfrules) {
                    defcfRule = metadata.defcfrules[columnId];
                }


                if (!cfRule && !defcfRule) {
                    columnDefinition.cellFormatting = {};
                    return;
                }

                const updateCellFormattings = function(rules) {
                    _.forEach(rules, function (rule, index) {
                        var result = getCellFormatting(rule, columnId);
                        cellFormattings[index] = result;
                    });
                };
                

                if (defcfRule && defcfRule.rules &&
                    (
                        !valRule || 
                        !valRule.disablecffmt
                    )
                 ) {
                    updateCellFormattings(defcfRule.rules);

                } else if (cfRule && cfRule.rules) {
                    updateCellFormattings(cfRule.rules);
                }

                columnDefinition.cellFormatting = cellFormattings;

                // Is the value hidden via column settings
                if (rcol &&
                    angular.isDefined(rcol.hideval) &&
                    rcol.hideval) {
                    colHideVal = true;
                }

                // Is the value hidden via conditional format settings
                if (!angular.isDefined(cfRule))
                    condHideVal = false;
                else {
                    if (angular.isDefined(cfRule.showval)) {
                        condHideVal = !cfRule.showval;
                    } else {
                        condHideVal = true;
                    }
                }

                columnDefinition.hideValue = colHideVal || condHideVal;
            }

            function getCellFormatting(rule, columnId) {
                var result = {
                    tag: { columnId: columnId }
                };

                if (rule.fgcolor) {
                    result.foregroundColor = rule.fgcolor;
                }

                if (rule.bgcolor) {
                    result.backgroundColor = rule.bgcolor;
                }

                if (rule.bounds) {
                    result.bounds = rule.bounds;
                }

                if (rule.imgid) {
                    result.imageId = rule.imgid;
                }

                if (rule.cfid) {
                    result.cfId = rule.cfid;
                }

                return result;
            }

            function setReportRequestOptions(requestOptions) {
                if (model.hasAdHocSorting) {
                    if (model.reportMetadata.sort) {
                        requestOptions.sort = model.reportMetadata.sort;
                    } else {
                        requestOptions.sort = [];
                    }
                }

                var conds = [];
                if (model.analyzerOptions.conds)
                    conds.push.apply(conds, model.analyzerOptions.conds);
                if (model.externalConds)
                    conds.push.apply(conds, model.externalConds);
                if (requestOptions.groupFilterConds)
                    conds.push.apply(conds, requestOptions.groupFilterConds);
                requestOptions.conds = conds;
            }           

            // Refresh the report data
            function refreshReportData(params) {
                exports.resetPagingInfo();
                return exports.getReportData(params);
            }

            // Returns true if the report requires it's metadata to be refreshed.
            // This can occur if the report's schema has changed on the server
            // and only the report's data is requested (i.e. without metadata).
            function requireMetadataRefresh(params, newData) {
                var columnsCount, rowValuesCount;

                // Metadata is already requested
                if (params && params.includeMetadata) {
                    return false;
                }

                // We have no data
                if (!newData || !newData.gdata) {
                    return false;
                }                

                // Report is requested without metadata                

                // Get the number of current columns
                columnsCount = sp.result(model, 'gridOptions.columnDefinitions.length') - (sp.result(model, 'gridOptions.aggregateInfo.groupedColumns.length') || 0);
                // Get the number of columns in the new data                
                rowValuesCount = sp.result(_.first(newData.gdata), 'values.length');

                // Return true if column counts do not match
                return (rowValuesCount > 0) && (columnsCount > 0) && (rowValuesCount !== columnsCount);                
            }

            /**
             * Get the report data and metadata from the reporting web API.
             * @param {object} params Report loading options.
             */
            function getReportData(params) {
                //sp.logTime('Report data requested');
                return getTemplateReportIds().then(function () {
                    var isTemplateReport = _.has(templateReportIds, model.reportId);
                    var isMobile = sp.result(params, 'isMobile');
                    var isInScreen = sp.result(params, 'isInScreen');                    
                    var screenReportMobilePageSizeValue = screenReportMobilePageSize.value;
                    var reportMobilePageSizeValue = reportMobilePageSize.value;
                    var reportPageSizeValue = reportPageSize.value;

                    if (params && params.pageSizeMultipler) {
                        screenReportMobilePageSizeValue = screenReportMobilePageSizeValue * params.pageSizeMultipler;
                        reportMobilePageSizeValue = reportMobilePageSizeValue * params.pageSizeMultipler;
                        reportPageSizeValue = reportPageSizeValue * params.pageSizeMultipler;
                    }

                    var requestOptions = {
                        startIndex: model.startIndex,
                        entityTypeId: isTemplateReport ? model.entityTypeId : 0,
                        relationDetail: model.relationDetail,
                        inclids: model.inclids,
                        exclids: model.exclids,
                        relfilters: model.relationshipFilters,
                        filtereids: model.filteredEntityIds,
                        pageSize: isMobile && isInScreen ? screenReportMobilePageSizeValue : isMobile ? reportMobilePageSizeValue : reportPageSizeValue,
                        displayPageSize: isMobile && isInScreen ? screenReportMobilePageSizeValue - 1 : isMobile ? reportMobilePageSizeValue : reportPageSizeValue,
                        maxCols: isMobile ? reportMobileMaxCols.value : undefined,
                        isReset: params ? params.isReset : false,
                        isRefresh: params ? params.isRefresh === true : false, // a refresh request by the user (via button, auto or action),
                        isGroupPage: params ? params.isGroupPage : undefined,
                        aggRowIndex: params ? params.aggRowIndex : undefined,
                        groupFilterConds: params ? params.groupFilterConds : undefined
                    };
                    
                    if (model.quickSearch &&
                        (model.quickSearch.value || model.quickSearch.doQuickSearch)) {
                        requestOptions.quickSearch = model.quickSearch.value;
                        model.quickSearch.doQuickSearch = false;
                        params.includeMetadata = true;
                    }

                    setReportRequestOptions(requestOptions);

                    if (params) {
                        if (params.includeMetadata) {
                            requestOptions.metadata = 'full';
                        }
                    }

                    if (!model.reportId) {
                        return $q.when();
                    }

                    model.gridBusyIndicator.isBusy = true;

                    // Get the report meta data and initialize the report
                    requestOptions.shouldCache = function () { return spNavService.getIsCacheable(); };
                    requestOptions.loadLatest = function (data) {
                        exports.loadReportData(params, requestOptions, data);
                    };

                    return spReportService.getReportData(model.reportId, requestOptions).then(function (data) {
                        if (requireMetadataRefresh(params, data)) {                                
                            if (!params) {
                                params = {};
                            }
                            // Include metadata and refresh.                                
                            params.includeMetadata = true;
                            resetPagingInfo();
                            return getReportData(params);
                        }

                        if (model.customDataProvider) {
                            // Pass additional data to custom provider without mutating the original object.
                            var customData = {};
                            customData.quickSearch = model.quickSearch;

                            if (model.gridOptions && model.gridOptions.columnDefinitions) {
                                customData.columnDefinitions = model.gridOptions.columnDefinitions;
                            }

                            _.assign(customData, data);

                            if (!customData.meta) {
                                _.set(customData, 'meta.sort', requestOptions.sort || []);
                            }

                            return $q.when(model.customDataProvider(customData)).then(function (newData) {
                                data.gdata = newData;
                                exports.loadReportData(params, requestOptions, data);
                            });
                        } else {
                            return $q.when(exports.loadReportData(params, requestOptions, data));
                        }
                    }, function (error) {
                        model.gridBusyIndicator.isBusy = false;
                        reportRunError(error, model);
                    });
                });
            }

            function reportRunError(error, model) {
                // The title is fetched async so may not be available when the error is returned. 
                var messageStart = model.title ? 'An error occurred running the report \'' + model.title + '\'. ' : 'An error occurred running a report. ';    // Due to a race condition fetching the title we don't always have it.
                spAlertsService.addAlert(messageStart + sp.result(error, 'data.Message'), { expires: false, severity: spAlertsService.sev.Error });
            }

            /**
             * Get the report data and metadata from the reporting web API.
             * @param {object} params Report loading options.
             */
            function getReportDataByReportEntity(params, schemaOnly) {
                var requestOptions = {
                    startIndex: model.startIndex,
                    pageSize: params && params.isMobile ? reportMobilePageSize.value : reportPageSize.value
                };

                if (params) {
                    if (params.includeMetadata) {
                        requestOptions.metadata = 'full';
                    }

                    if (params.pageSizeMultipler) {
                        requestOptions.pageSize = requestOptions.pageSize * params.pageSizeMultipler;
                    }
                }                

                if (!model.reportEntity) {
                    return;
                }

                if (schemaOnly && schemaOnly === true) {
                    requestOptions.metadata = 'schema';
                } 

                model.gridBusyIndicator.isBusy = true;

                // Get the report meta data and initialize the report
                return spReportService.getReportDataByReportEntity(model.reportEntity, requestOptions).then(function (data) {
                    exports.loadReportData(params, requestOptions, data);
                }, function (error) {
                    model.gridBusyIndicator.isBusy = false;
                });
            }

            /**
             * The common method to load reportdata from reportService getReportData call
             * @param {object} params Report loading options.
             * @param {object} requestOptions request Options.
             * @param {object} data report data.
             */
            function loadReportData(params, requestOptions, data) {
                spTenantSettings.getNameFieldEntity().then(name => {
                    nameFieldEntity = name;
                    loadReportDataInternal(params, requestOptions, data);
                });
            }

            /**
             * The common method to load reportdata from reportService getReportData call
             * @param {object} params Report loading options.
             * @param {object} requestOptions request Options.
             * @param {object} data report data.
             */
            function loadReportDataInternal(params, requestOptions, data) {
                // Hack to fix performance issue when updating columns
                var rowDataBackup;

                try {
                    if (requestOptions.metadata) {
                        // Clear the rows before updating columns
                        rowDataBackup = model.gridOptions.rowData;
                        model.gridOptions.rowData = [];
                        // Load metadata
                        if (!model.isEditMode) {
                            loadReportMetadata(data.meta, params);
                        } else {
                            loadReportMetadata(data.meta, {isMobile: params.isMobile });
                        }

                        displayInvalidReportInformation(data.meta, model.isEditMode);

                        exports.loadAnalyzerFieldDefinitions(data.meta);

                        //set hideActionBar, hideReportHeader and ReportStyle property
                        model.hideActionBar = data.meta.hideact === undefined || data.meta.hideact === null ? false : data.meta.hideact;
                        model.gridOptions.hideReportHeader = data.meta.hideheader === undefined || data.meta.hideheader === null ? false : data.meta.hideheader;
                        model.gridOptions.reportStyle = data.meta.style ? data.meta.style : 'Default';
                    }

                    model.moreDataAvailable = true;
                    if (sp.result(data, 'moreDataAvailable')) {
                        // do nothing
                    }
                    else if (!data.gdata || data.gdata.length < requestOptions.pageSize) {
                        if (!requestOptions.isGroupPage) {                            
                            model.moreDataAvailable = false;
                        }                        
                    }

                    if (data.gdata && data.gdata.length > 0 && requestOptions.displayPageSize < data.gdata.length) {
                        data.gdata.pop();   // remove the extra last data row that we fetched to see if there is more data available
                        if (!requestOptions.isGroupPage) {                            
                            data.moreDataAvailable = true;  // put a flag on the data (to be used if it is fetched from cache) to mark there is more data as we are removing the extra row
                        }                        
                    }
                } finally {
                    try {
                        if (requestOptions.metadata) {
                            model.gridOptions.rowData = rowDataBackup;
                        }
                        if (!model.isEditMode && model.withInvalidReportCondition) {
                            //in view mode,
                            //we should abort load report grid data if it refers to with invalid report condition .. because it means we'll get more rows.
                        } else {
                            loadReportGridData(data.gdata, requestOptions.startIndex === 0 && !requestOptions.isGroupPage);
                        }
                    }
                    finally {
                        model.gridBusyIndicator.isBusy = false;
                        $timeout(function () {
                            //sp.logTime('Report done');
                            model.reportRanAtleastOnce = true;
                        }, 0, false);
                    }
                }
            }

            /**
             * Get the requested row data.
             *
             * @param {number} index The index of the row to return
             * @returns {object} The row data.
             */
            function getRowData(index) {
                // TODO - support for loading in next page of data

                if (index < model.gridOptions.rowData.length && index >= 0) {
                    return model.gridOptions.rowData[index];
                }

                return null;
            }

            /**
             * Finds the index of the requested entity id.
             *
             * @param {number} id The entity id to find.
             * @returns {number} The index of the found entity id or -1.
             */
            function findRowIndex(id) {
                return _.findIndex(model.gridOptions.rowData, function (row) {
                    return row.eid === id;
                });
            }

            /**
             * Gets the model data.
             *
             * @returns {object} The model data.
             */
            function getModel() {
                return model;
            }

            /**
             * Gets the navigation service data.
             *
             * @returns {object} An object that can be registered with the navigation service for the purposes of
             * initialising from a saved state and for enumerating the report data.
             * The object has the following methods selected(), next(), prev(), setSelected()
             */
            function getNavServiceData() {
                // TODO : this will need to change when the protocol between reports and edit form is defined
                return {
                    convertRowData: function (row) {
                        return (row) ? {
                            id: row.eid,
                            name: row.eid,
                            href: spNavService.getChildHref('explorer', row.eid)
                        } : null;
                    },

                    reportModelManager: spReportModelManager(model),

                    selectedIndex: 0,

                    selected: function () {
                        return this.convertRowData(this.reportModelManager.getRowData(this.selectedIndex));
                    },

                    next: function () {
                        var rowData = this.reportModelManager.getRowData(this.selectedIndex + 1);

                        if (rowData) {
                            return this.convertRowData(rowData);
                        }
                        return this.selected();
                    },

                    prev: function () {
                        var rowData = this.reportModelManager.getRowData(this.selectedIndex - 1);

                        if (rowData) {
                            return this.convertRowData(rowData);
                        }
                        return this.selected();
                    },

                    setSelected: function (item) {
                        var index = this.reportModelManager.findRowIndex(item.id);
                        if (index >= 0) {
                            this.selectedIndex = index;
                        }
                    }
                };
            }

            function setUpQuickSearchModel(quickSearch) {
                // If an external quickSeach control is used, use it.
                _.extend(quickSearch, {
                    id: 'reportSearchControl',
                    value: '',
                    oldValue: '',
                    isBusy: false,
                    doQuickSearch: false,
                    onSearchValueChanged: function () {
                        if (quickSearch.value !== quickSearch.oldValue) {
                            quickSearch.doQuickSearch = true;
                            debouncedQuickSearch();
                        }
                    }
                });
            }

            function copyQuickSearchModel(quickSearch) {
                if (model.quickSearch && quickSearch) {
                    model.quickSearch.oldValue = model.quickSearch.value;
                    model.quickSearch.value = quickSearch.value;
                }
            }

            /**
             * Resets the report model.
             */
            function resetModel(options) {
                model.reportMetadata = null;
                model.startIndex = 0;
                model.moreDataAvailable = true;
                model.hasAdHocSorting = false;
                model.hasAdHocAnalyzerConditions = false;
                model.gridOptions.rowData = [];
                model.gridOptions.columnDefinitions = [];
                model.gridOptions.sortInfo = [];
                model.gridOptions.aggregateInfo = {rollup: true};
                model.analyzerOptions.conds = [];
                model.analyzerOptions.analyzerFields = [];
                model.aggregateDataManager = null;
                model.inlineEditingMetadata = {};

                // If the quickSearch has been defined externally use it
                model.quickSearch = (options && options.quickSearch) || {};
                setUpQuickSearchModel(model.quickSearch);
            }

            /**
             * Resets the report model without gridOptions and analyzerOptions.
             */
            function resetBasicModel() {
                model.reportMetadata = null;
                model.startIndex = 0;
                model.moreDataAvailable = true;
                model.hasAdHocSorting = false;
                model.hasAdHocAnalyzerConditions = false;
                model.aggregateDataManager = null;
                setUpQuickSearchModel(model.quickSearch);
            }

            /**
             * Resets the paging info.
             */
            function resetPagingInfo() {
                model.startIndex = 0;
                model.moreDataAvailable = true;

                // Reset the selected item row indexes
                _.forEach(model.selectedItems, function (si) {
                    si.rowIndex = -1;
                });
            }

            /**
             * Resets the selected items.
             */
            function resetSelectedItems() {
                model.selectedItems = [];
            }

            /**
             * Initializes from an existing model
             * @param {object} existingModel The existing model.
             */
            function initializeFromModel(existingModel) {
                model.reportId = existingModel.reportId;
                model.entityTypeId = existingModel.entityTypeId;
                model.reportMetadata = existingModel.reportMetadata;
                // As we are reloading the report, reset the paging info.                
                model.startIndex = 0;
                model.moreDataAvailable = true;
                model.inlineEditingMetadata = {};
                model.selectedItems = existingModel.selectedItems;
                model.gridOptions.columnDefinitions = existingModel.gridOptions.columnDefinitions;
                model.gridOptions.sortInfo = existingModel.gridOptions.sortInfo;
                model.gridOptions.rowData = existingModel.gridOptions.rowData;
                model.gridOptions.selectedItems = existingModel.gridOptions.selectedItems;
                model.gridOptions.multiSelect = existingModel.gridOptions.multiSelect;
                model.gridOptions.decorateActionRequest = existingModel.gridOptions.decorateActionRequest;
                model.gridOptions.aggregateInfo = existingModel.gridOptions.aggregateInfo;
                model.analyzerOptions.conds = existingModel.analyzerOptions.conds;
                model.analyzerOptions.analyzerFields = existingModel.analyzerOptions.analyzerFields;
                model.hasAdHocSorting = existingModel.hasAdHocSorting;
                model.hasAdHocAnalyzerConditions = existingModel.hasAdHocAnalyzerConditions;
                model.aggregateDataManager = existingModel.aggregateDataManager;
                //model.quickSearch = existingModel.quickSearch;
                copyQuickSearchModel(existingModel.quickSearch);
                //setUpQuickSearchModel(model.quickSearch);
            }

            /**
             * Creates the model.
             * @returns {object} The created model.
             */
            function createModel(options) {
                // The model data
                model = {
                    reportId: 0,
                    entityTypeId: 0,
                    reportMetadata: null,
                    isEditMode: false,
                    schemaOnly: false,
                    startIndex: 0,
                    moreDataAvailable: true,
                    selectedItems: [],
                    invalidReportInfos: null,
                    withInvalidReportCondition: false,
                    inlineEditingMetadata: {},
                    // The grid options
                    gridOptions: {
                        rowData: [],
                        useExternalSorting: true,
                        selectedItems: [],
                        columnDefinitions: [],
                        sortInfo: [],
                        multiSelect: false,
                        aggregateInfo: {},
                        existingAggregateRows: [],
                        decorateActionRequest: function (actionRequest) {
                            actionRequest.reportId = model.reportId;
                            if (model.formControlEntity) {
                                if (model.formControlEntity.dataState !== spEntity.DataStateEnum.Create) {
                                    actionRequest.hostIds = [model.formControlEntity.idP];
                                }

                                actionRequest.hostTypeIds = _.map(model.formControlEntity.typesP, 'idP');
                            }
                        }
                    },
                    gridBusyIndicator: {
                        type: 'spinner',
                        placement: 'element'
                    },
                    hasAdHocSorting: false,
                    hasAdHocAnalyzerConditions: false,
                    refreshButtonOptions: {
                        refreshCallback: function () {
                            var isRollupReport = model.aggregateDataManager ? model.aggregateDataManager.hasRollupData() : false;

                            exports.refreshReportData({ includeMetadata: isRollupReport, isRefresh: true });
                        }
                    },
                    // Analyzer options
                    analyzerOptions: {
                        conds: [],
                        analyzerFields: []
                    },
                    aggregateDataManager: null                    
                };

                // If an external quickSearch control is used, use it.
                model.quickSearch = (options && options.quickSearch) || {};
                setUpQuickSearchModel(model.quickSearch);

                return model;
            }

            /**
             * Save the report.
             */
            function saveReport() {
                var requestOptions = {};

                if (!model.reportId) {
                    return;
                }

                setReportRequestOptions(requestOptions);

                model.gridBusyIndicator.isBusy = true;

                // Get the report meta data and initialize the report
                spReportService.putReport(model.reportId, requestOptions).then(function () {
                    model.gridBusyIndicator.isBusy = false;
                }, function (error) {
                    model.gridBusyIndicator.isBusy = false;
                });
            }

            /**
             * Convert the specified string value to the native value
             * @param {object} type. The type of the value to convert.
             * @param {object} value. The string value to convert.
             * @returns {object} The converted value.
             */
            function convertToNativeType(type, value) {
                var result, ids;

                if (_.isNull(value) ||
                    _.isUndefined(value)) {
                    return null;
                }

                switch (type) {
                    case spEntity.DataType.Decimal:
                    case spEntity.DataType.Currency:
                    case spEntity.DataType.Int32:
                        result = (value === '') ? null : Number(value);
                        break;
                    case spEntity.DataType.Time:
                    case spEntity.DataType.Date:
                    case spEntity.DataType.DateTime:
                        result = (value === '') ? null : new Date(value);
                        break;
                    case spEntity.DataType.Bool:
                        result = (value === '') ? false : sp.stringToBoolean(value);
                        break;
                    case 'ChoiceRelationship':
                    case 'InlineRelationship':
                    case 'UserInlineRelationship':
                    case 'StructureLevels':
                        ids = _.keys(value);
                        result = _.map(ids, function (id) {
                            var entity = spEntity.fromId(id);
                            entity.setName(value[id]);

                            return entity;
                        });
                        break;
                    default:
                        result = (value === '') ? null : value;
                }

                return result;
            }

            function getAvailableChoiceEntities(typeId) {
                var choiceInstances = [];

                if (model.reportMetadata.choice) {
                    choiceInstances = model.reportMetadata.choice[typeId];
                }

                return _.map(choiceInstances, function (instance) {
                    var entity = spEntity.fromId(instance.id);
                    entity.setName(instance.name);

                    return entity;
                });
            }

            function getInlineTypePickerReportId(typeId) {
                if (!model.reportMetadata.inline) {
                    return null;
                }

                return model.reportMetadata.inline[typeId];
            }

            // Update the indexes of the selected items
            // Note: This method assumes that all the report data is present on the client
            function updateSelectedItemIndexes(selectedItems) {
                var selectedItemsToProcess;

                if (!model.gridOptions.rowData || !model.gridOptions.rowData.length || !selectedItems || !selectedItems.length) {
                    return;
                }

                selectedItemsToProcess = _.filter(selectedItems, function (si) {
                    return _.isUndefined(si.rowIndex) || si.rowIndex < 0;
                });

                if (!selectedItemsToProcess || !selectedItemsToProcess.length) {
                    return;
                }

                _.forEach(model.gridOptions.rowData, function (row, index) {
                    var selectedItem, selectedItemIndex;

                    if (!selectedItemsToProcess.length) {
                        // No more selected items to process, stop the iteration
                        return false;
                    }

                    // Check if any of the selected items match
                    selectedItem = _.find(selectedItemsToProcess, function (si, siIndex) {
                        var found = (si.eid === row.eid);
                        if (found) {
                            selectedItemIndex = siIndex;
                        }
                        return found;
                    });

                    if (selectedItem) {
                        selectedItem.rowIndex = index;
                        // Remove the item from the list to process
                        selectedItemsToProcess.splice(selectedItemIndex, 1);
                    }
                });
            }

            // Set the selected items
            function setSelectedItems(selectedItems, scrollToFirstItem) {
                var selectedItemsRowIndexes,
                    countSelected = 0,
                    requireUpdate;


                var hasRollupData = model.aggregateDataManager ? model.aggregateDataManager.hasRollupData() : false;

                if (!model.gridOptions.rowData || !model.gridOptions.rowData.length || !selectedItems || !selectedItems.length || !model.gridOptions.dataGrid) {
                    return;
                }

                // Extract the rowIndexes from the selected items
                selectedItemsRowIndexes = _.filter(_.map(selectedItems, 'rowIndex'), function (rowIndex) {
                    return rowIndex >= 0;
                });

                _.forEach(selectedItemsRowIndexes, function (rowIndex, index) {
                    if (!model.gridOptions.dataGrid) {
                        return false;
                    }

                    if (scrollToFirstItem &&
                        index === 0) {
                        model.gridOptions.dataGrid.scrollTo(0);
                    }
                    if (model.gridOptions.dataGrid.selectRow(rowIndex, true, countSelected, true)) {
                        requireUpdate = true;
                        //note: this change is required by bug 24801, the grid will auto scroll to the selected row.     
                        if (!model.isEditMode && !hasRollupData && scrollToFirstItem === true) {
                            model.gridOptions.dataGrid.scrollTo(rowIndex);
                        }
                    }
                    countSelected = countSelected + 1;
                });

                return requireUpdate;
            }

            function setAggregateRows(allAggregateRowState) {
                aggregateRows = allAggregateRowState;
            }

            function getAggregateRows() {
                return aggregateRows;
            }            

            function getTemplateReportIds() {
                return spTenantSettings.getTemplateReportIds().then(function (ids) {
                    templateReportIds = ids;
                });
            }

            function getQuickSearchRowData() {
                var result;

                if (model.destroyed) {
                    return;
                }

                model.quickSearch.isBusy = true;
                quickSearchChanged = false;

                // Only run it in view mode
                if (!model.isEditMode || !model.reportEntity) {
                    exports.resetPagingInfo();                    
                    result = exports.getReportData({
                        includeMetadata: model.aggregateDataManager ? model.aggregateDataManager.hasRollupData() : false,
                        isMobile: model.gridOptions.isMobile
                    });
                }

                if (result) {
                    result.then(function () {
                        if (quickSearchChanged) {
                            // A change has been set, run again
                            getQuickSearchRowData();
                        } else {
                            model.quickSearch.isBusy = false;
                        }
                    }, function () {
                        if (quickSearchChanged) {
                            // A change has been set, run again
                            getQuickSearchRowData();
                        } else {
                            model.quickSearch.isBusy = false;
                        }
                    });
                } else {
                    model.quickSearch.isBusy = false;
                }
            }

            function quickSearch() {
                if (model.quickSearch.value === model.quickSearch.oldValue) {
                    return;
                }

                model.quickSearch.oldValue = model.quickSearch.value;

                if (!model.quickSearch.isBusy) {
                    // Do a search if one is not already running
                    getQuickSearchRowData();
                } else {
                    // A search is already running. Kick off another
                    // one when the current one is done.
                    quickSearchChanged = true;
                }
            }

            //Merge both analyser and external filter conditions.
            function mergeFilterConditions() {
                model.conds = [];
                if (model.analyzerOptions.conds && model.analyzerOptions.conds.length > 0)
                    model.conds.push.apply(model.conds, model.analyzerOptions.conds);
                if (model.externalConds && model.externalConds.length > 0)
                    model.conds.push.apply(model.conds, model.externalConds);
            }

            function updateInlineEditingMetadata(selectedRowIndex) {
                if (!model.isInlineEditing || !model.gridOptions.rowData || !model.gridOptions.rowData.length || selectedRowIndex < 0) {
                    return;
                }

                const selectedRow = model.gridOptions.rowData[selectedRowIndex];
                if (selectedRow && _.has(model.inlineEditingMetadata, selectedRow.eid)) {
                    // Have the form for the selected row, return to avoid unecessary service calls
                    return;
                }

                let renderedRange, startRowIndex, endRowIndex;

                // Get the visible range
                if (model.gridOptions.dataGrid && model.gridOptions.dataGrid.getRenderedRange) {
                    renderedRange = model.gridOptions.dataGrid.getRenderedRange();
                }

                // Dont have the form for the selected row
                // Get the info for rows in the visible range or get 25 rows before and after the selected row

                if (renderedRange) {
                    startRowIndex = renderedRange.topRow;
                    endRowIndex = renderedRange.bottomRow;
                } else {                    
                    const rowCount = 25;
                    startRowIndex = selectedRowIndex - rowCount;
                    if (startRowIndex < 0) {
                        startRowIndex = 0;
                    }
                    endRowIndex = selectedRowIndex + rowCount + 1;
                    if (endRowIndex > model.gridOptions.rowData.length) {
                        endRowIndex = model.gridOptions.rowData.length;
                    }
                }

                getInlineEditingMetadataForEntities(_.map(_.slice(model.gridOptions.rowData, startRowIndex, endRowIndex), 'eid'));
            }
        }
    }
}());