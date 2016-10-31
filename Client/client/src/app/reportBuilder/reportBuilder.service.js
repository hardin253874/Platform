// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, spReportEntityQueryManager, spReportEntity, jsonLookup,
  ReportEntityQueryManager, spReportPropertyDialog */

(function () {
    'use strict';

    angular.module('app.reportBuilder')
        .service('reportBuilderService', reportBuilderService);

    /* @ngInject */
    function reportBuilderService() {
        var exports = {};
        var currentReportModel = null;
        var currentReportEntity = null;
        var currentReportTreeNode = null;
        var currentReportId = null;
        var reportEntityUpdated = null;
        var actionsFromReportBuilder = [];
        var actionsFromReport = [];
        exports.setReportModel = function (rid, reportModel) {
            if (rid && reportModel) {
                currentReportId = rid;
                currentReportModel = reportModel;
            }

        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Update ReportEntity
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        exports.setReportEntity = function (rid, reportEntity, reportTreeNode) {
            if (reportEntity) {
                if (rid > 0) {
                    currentReportId = rid;
                }
                currentReportEntity = reportEntity;
                reportEntityUpdated = sp.newGuid();
                if (reportTreeNode) {
                    currentReportTreeNode = reportTreeNode;
                }
            }

        };

        //update the reportEntity from report, the notice report builder updated
        exports.setReportEntityFromReport = function (reportEntity) {
            currentReportEntity = reportEntity;
            exports.noticeReportBuilder();
        };

        //notice reportbuilder the reportEntity is updated
        exports.noticeReportBuilder = function () {
            reportEntityUpdated = sp.newGuid();
            exports.setActionFromReportBuilder('updateReportEntity', null, null, {reportEntity: currentReportEntity});
        };

        exports.getReportBuilderOptions = function () {

            if (currentReportModel && currentReportEntity) {
                return {"reportModel": currentReportModel, "reportEntity": currentReportEntity};
            }
            else if (currentReportEntity) {
                return {"reportEntity": currentReportEntity};
            }
            else if (currentReportModel) {
                return {"reportModel": currentReportModel};
            }
            else {
                return null;
            }
        };

        exports.getReportModel = function () {
            return currentReportModel;
        };

        exports.getReportEntity = function () {
            return currentReportEntity;
        };

        exports.getReportEntityUpdated = function () {
            return reportEntityUpdated;
        };

        exports.getReportTreeNode = function () {
            return currentReportTreeNode;
        };

        exports.getReportId = function () {
            return currentReportId;
        };

        exports.clearReport = function () {
            currentReportId = null;
            currentReportModel = null;
            currentReportEntity = null;
            currentReportTreeNode = null;
        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Update Report Builder Action
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        exports.setActionFromReportBuilder = function (type, node, field, options) {
            actionsFromReportBuilder.push({type: type, node: node, field: field, options: options});
        };

        exports.getActionsFromReportBuilder = function (clear) {
            var resultActions = actionsFromReportBuilder;

            if (clear) {
                actionsFromReportBuilder = [];
            }

            return resultActions;
        };

        exports.clearActionsFromReportBuilder = function () {
            actionsFromReportBuilder = [];
        };

        exports.setActionFromReport = function (type, field, column) {
            actionsFromReport.push({type: type, field: field, target: column});
        };

        exports.getActionsFromReport = function (clear) {
            var resultActions = actionsFromReport;

            if (clear) {
                actionsFromReport = [];
            }

            return resultActions;
        };

        exports.clearActionsFromReport = function () {
            actionsFromReport = [];
        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Query Manager functions
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Node Update Common Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //add relationship in report entity, add the namefield if exist
        exports.addRelationship = function (node, namefield, parentAggregateEntityNode) {
            if (currentReportEntity) {
                spReportEntityQueryManager.addNodeToQuery(currentReportEntity, node, namefield);

                if (parentAggregateEntityNode) {
                    spReportEntityQueryManager.updateAggregateGroupBy(currentReportEntity, parentAggregateEntityNode);
                }

                exports.noticeReportBuilder();
            }
        };

        //add relationships in report entity
        exports.addRelationships = function (nodes, namefields, parentAggregateEntityNode) {
            if (currentReportEntity) {
                for (var i = 0; i < nodes.length; i++) {
                    if (namefields && namefields.length === nodes.length) {
                        spReportEntityQueryManager.addNodeToQuery(currentReportEntity, nodes[i], namefields[i]);
                    } else {
                        spReportEntityQueryManager.addNodeToQuery(currentReportEntity, nodes[i], null);
                    }
                }

                if (parentAggregateEntityNode) {
                    spReportEntityQueryManager.updateAggregateGroupBy(currentReportEntity, parentAggregateEntityNode);
                }

                exports.noticeReportBuilder();
            }
        };

        //remove relationship from report
        exports.removeRelationship = function (node, parentAggregateEntityNode) {
            if (currentReportEntity) {
                spReportEntityQueryManager.reomveNodeFromQuery(currentReportEntity, node);

                if (parentAggregateEntityNode) {
                    spReportEntityQueryManager.updateAggregateGroupBy(currentReportEntity, parentAggregateEntityNode);
                }

                exports.noticeReportBuilder();
            }
        };

        //remove relationsihps from report
        exports.removeRelationships = function (nodes, parentAggregateEntityNode) {
            if (currentReportEntity) {
                _.forEach(nodes, function (node) {
                    spReportEntityQueryManager.reomveNodeFromQuery(currentReportEntity, node);
                });

                if (parentAggregateEntityNode) {
                    spReportEntityQueryManager.updateAggregateGroupBy(currentReportEntity, parentAggregateEntityNode);
                }

                exports.noticeReportBuilder();
            }
        };

        //update report node entity
        exports.updateQueryEntity = function (nodeId, nodeEntity) {
            if (currentReportEntity) {
                spReportEntityQueryManager.updateNodeEntityToQuery(currentReportEntity, nodeEntity);

                exports.noticeReportBuilder();
            }

        };
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Column Update Common Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //check current column exists in node
        exports.existsReportColumn = function (column, node) {
            if (currentReportEntity) {
                return spReportEntityQueryManager.getExistingReportColumn(currentReportEntity, column, node);
            }
            return null;
        };

        //add new reportcolumn in report entity
        exports.addColumnToReport = function (column, entityNode, insertAfterColumn) {
            if (currentReportEntity) {
                if (insertAfterColumn === undefined || insertAfterColumn === null) {
                    insertAfterColumn = -1;
                }

                spReportEntityQueryManager.addColumnToReport(currentReportEntity, column, entityNode, insertAfterColumn);

                exports.noticeReportBuilder();
            }
        };

        //remove reportcolumn in report entity
        exports.removeColumnFromReport = function (column, entityNode) {
            if (currentReportEntity) {
                var selectColumn = spReportEntityQueryManager.getExistingReportColumn(currentReportEntity, column, entityNode);

                if (!selectColumn)
                    return;

                var unHiddenColumns = _.filter(currentReportEntity.getReportColumns(), function (selectedColumn) {
                    return selectedColumn.isHidden() === false;
                });

                if (unHiddenColumns && unHiddenColumns.length === 1) {
                    exports.setActionFromReportBuilder('setAlert', null, null, {errorMessage: 'Cannot remove last column in report.'});
                    exports.noticeReportBuilder();
                } else {
                    spReportEntityQueryManager.removeReportOrderByByReportColumnId(currentReportEntity, selectColumn.id());
                    spReportEntityQueryManager.removeReportConditionsByReportColumnId(currentReportEntity, selectColumn.id());
                    var columnDisplayOrder = (selectColumn.displayOrder && selectColumn.displayOrder()) ? selectColumn.displayOrder() : -1;
                    currentReportEntity.removeReportColumn(selectColumn);
                    if (columnDisplayOrder > -1) {
                        spReportEntityQueryManager.reduceColumnDisplayOrder(currentReportEntity, columnDisplayOrder);
                    }
                    exports.noticeReportBuilder();
                }
            }
        };

        //remove reportcolumn by columnid
        exports.removeColumnById = function (columnId, autoRefresh, parentAggregateEntity) {
            if (currentReportEntity) {
                if (autoRefresh === undefined || autoRefresh === null)
                    autoRefresh = true;

                var selectedColumn = _.find(currentReportEntity.getReportColumns(), function (column) {
                    return column.id().toString() === columnId.toString();
                });
                if (selectedColumn) {
                    var unHiddenColumns = _.filter(currentReportEntity.getReportColumns(), function (column) {
                        return column.isHidden() === false;
                    });

                    if (unHiddenColumns && unHiddenColumns.length === 1) {
                        exports.setActionFromReportBuilder('setAlert', null, null, {errorMessage: 'Cannot remove last column in report.'});
                        autoRefresh = true;
                    } else {
                        spReportEntityQueryManager.removeReportOrderByByReportColumnId(currentReportEntity, selectedColumn.id());
                        spReportEntityQueryManager.removeReportConditionsByReportColumnId(currentReportEntity, selectedColumn.id());
                        if (parentAggregateEntity && parentAggregateEntity.getEntity().getGroupedBy()) {
                            if (parentAggregateEntity.getEntity().getGroupedBy().length > 0) {
                                parentAggregateEntity.getEntity().getGroupedBy().remove(selectedColumn.getExpression().getEntity());
                            }
                        }
                        currentReportEntity.removeReportColumn(selectedColumn);

                    }
                }
                if (autoRefresh) {
                    exports.noticeReportBuilder();
                }
            }
        };

        //change report column's display order value
        exports.reOrderColumnToReport = function (column1, column2) {
            if (currentReportEntity) {
                var reportColumn1, reportColumn2;
                var reportColumnOrder1 = -1, reportColumnOrder2 = -1;

                reportColumn1 = _.find(currentReportEntity.getReportColumns(), function (column) {
                    return column.id().toString() === column1.toString();
                });

                if (reportColumn1)
                    reportColumnOrder1 = reportColumn1.displayOrder();

                reportColumn2 = _.find(currentReportEntity.getReportColumns(), function (column) {
                    return column.id().toString() === column2.toString();
                });

                if (reportColumn2)
                    reportColumnOrder2 = reportColumn2.displayOrder();


                if (reportColumnOrder1 > -1 && reportColumnOrder2 > -1) {
                    reportColumn1.getEntity().columnDisplayOrder = reportColumnOrder2;
                    spReportEntityQueryManager.increaseColumnDisplayOrder(currentReportEntity, reportColumn1.id(), reportColumnOrder2);
                    //reportColumn2.getEntity().columnDisplayOrder = reportColumnOrder1;
                    //increase all other column's index


                    exports.noticeReportBuilder();
                }
            }
        };

        //update column name from report and update columnForCondition relationship if not exists
        exports.updateColumnNameFromReport = function (columnId, columnName) {
            if (currentReportEntity) {
                var reportColumn = _.find(currentReportEntity.getReportColumns(), function (column) {
                    return column.id().toString() === columnId.toString();
                });

                if (reportColumn) {
                    reportColumn.getEntity().name = columnName;

                    if (reportColumn.getExpression() && reportColumn.getExpression().getSourceNode()) {
                        var columnSourceNodeId = reportColumn.getExpression().getSourceNode() ? reportColumn.getExpression().getSourceNode().id() : 0;
                        var columnFieldId = (reportColumn.getExpression().getField && reportColumn.getExpression().getField()) ? reportColumn.getExpression().getField().id() : 0;
                        //find report condition with same column expression
                        var reportCondition = _.find(currentReportEntity.getReportConditions(), function (condition) {
                            var conditionExpression = condition.getExpression();
                            var columnForCondition = condition.getEntity().columnForCondition;
                            if (conditionExpression) {
                                if (conditionExpression.getTypeAlias() === 'core:columnReferenceExpression' || conditionExpression.getTypeAlias() === 'columnReferenceExpression') {
                                    return (!columnForCondition && conditionExpression._expressionEntity.expressionReferencesColumn.id().toString() === columnId.toString());
                                } else {
                                    var conditionSourceNodeId = conditionExpression.getSourceNode() ? conditionExpression.getSourceNode().id() : -1;

                                    var conditionFieldId = (conditionExpression.getField && conditionExpression.getField()) ? conditionExpression.getField().id() : -1;

                                    //same expresion sourcenode id and field id but without columnForCondtion was setup
                                    //if expression is resourceExpression, only compare sourcenodeid
                                    if (conditionExpression.getTypeAlias() === 'core:resourceExpression' || conditionExpression.getTypeAlias() === 'resourceExpression') {
                                        return (!columnForCondition && conditionSourceNodeId === columnSourceNodeId);
                                    } else if (conditionExpression.getTypeAlias() === 'core:scriptExpression' || conditionExpression.getTypeAlias() === 'scriptExpression') {
                                        return (!columnForCondition && conditionSourceNodeId === columnSourceNodeId);
                                    } else {
                                        return (!columnForCondition && conditionSourceNodeId === columnSourceNodeId && columnFieldId === conditionFieldId);
                                    }
                                }
                            } else {
                                return null;
                            }
                        });

                        if (reportCondition) {
                            if (!reportCondition.getEntity().columnForCondition) {
                                reportCondition.getEntity().columnForCondition = reportColumn.getEntity();
                            }
                        }

                        //if current condition's columnForCondition is current column and this column is new column
                        reportCondition = _.find(currentReportEntity.getReportConditions(), function (condition) {
                            if (condition.getEntity().columnForCondition && condition.getEntity().columnForCondition.id() === reportColumn.getEntity().id() && condition.getEntity().getDataState() === spEntity.DataStateEnum.Create && condition.getEntity().columnForCondition.getDataState() === spEntity.DataStateEnum.Create) {
                                return condition;
                            } else {
                                return null;
                            }
                        });

                        if (reportCondition) {
                            reportCondition.getEntity().name = columnName;
                        }
                    }
                }

                exports.noticeReportBuilder();
            }


        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Calculated Report Column Update Common Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // add calculate column to report by script and type
        exports.addCalculateColumnToReport = function (nodeEntity, script, type, columnName, entityTypeId) {
            if (currentReportEntity) {
                spReportEntityQueryManager.addCalculateColumnToReport(currentReportEntity, nodeEntity, script, type, columnName, entityTypeId);

                exports.noticeReportBuilder();
            }

        };
        //update calculate column in report by script and type
        exports.updateCalculateColumnToReport = function (columnId, script, type, columnName, entityTypeId) {
            if (currentReportEntity) {
                spReportEntityQueryManager.updateCalculateColumnToReport(currentReportEntity, columnId, script, type, columnName, entityTypeId);

                exports.noticeReportBuilder();
            }
        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Condition and Analyzer Update Common Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //add column to analyzer
        exports.addColumnToAnalyzer = function (column, entityNode, insertAfterAnalyzer) {
            if (currentReportEntity) {
                if (insertAfterAnalyzer === undefined || insertAfterAnalyzer === null) {
                    insertAfterAnalyzer = -1;
                }
                spReportEntityQueryManager.addCondition(currentReportEntity, column, entityNode, insertAfterAnalyzer);

                exports.noticeReportBuilder();
            }
        };

        //remove column from analyzer
        exports.removeColumnFromAnalyzer = function (column, entityNode) {
            if (currentReportEntity) {
                var reportCondition = spReportEntityQueryManager.getExistingReportCondition(currentReportEntity, column, entityNode.qe ? entityNode.qe : entityNode);

                currentReportEntity.removeReportCondition(reportCondition);

                exports.noticeReportBuilder();
            }

        };

        //remove column from analyzer by condtion id
        exports.removeAnalyzerById = function (conditionId) {
            if (currentReportEntity) {
                currentReportEntity.removeReportConditionByConditionId(conditionId);
                exports.noticeReportBuilder();
            }
        };

        // update analyzer field configuration
        exports.updateAnalyzerFieldConfiguration = function (analyzerField, conditionId) {
            if (currentReportEntity) {

                var reportCondition = _.find(currentReportEntity.getReportConditions(), function (condition) {
                    return condition.id().toString() === conditionId.toString();
                });

                if (reportCondition) {
                    var conditionEntity = reportCondition.getEntity();
                    if (conditionEntity) {
                        conditionEntity.setLookup('core:conditionParameterPicker', analyzerField.pickerReportId);
                    }
                }
                exports.noticeReportBuilder();
            }
        };

        //reorder the report condition's conditionOrder
        exports.reOrderConditionToReport = function (condition1, condition2) {
            if (currentReportEntity) {
                var reportCondition1, reportCondition2;
                var reportConditionOrder1 = -1, reportConditionOrder2 = -1;

                reportCondition1 = _.find(currentReportEntity.getReportConditions(), function (condition) {
                    return condition.id().toString() === condition1.toString();
                });

                if (reportCondition1)
                    reportConditionOrder1 = reportCondition1.displayOrder();

                reportCondition2 = _.find(currentReportEntity.getReportConditions(), function (condition) {
                    return condition.id().toString() === condition2.toString();
                });

                if (reportCondition2)
                    reportConditionOrder2 = reportCondition2.displayOrder();

                if (reportConditionOrder1 > -1 && reportConditionOrder2 > -1) {
                    reportCondition1.getEntity().conditionDisplayOrder = reportConditionOrder2;
                    //reportCondition2.getEntity().conditionDisplayOrder = reportConditionOrder1;
                    spReportEntityQueryManager.increaseConditionDisplayOrder(currentReportEntity, reportCondition1.id(), reportConditionOrder2);

                    exports.noticeReportBuilder();
                }
            }
        };

        //apply analzyer to search
        exports.applyAnalysers = function (conditions) {
            if (currentReportEntity) {
                var existingConditions = _.map(currentReportEntity.getReportConditions(), function (condition) {
                    try {
                        return condition.id();
                    } catch (e) {
                        return 0;
                    }
                });


                //apply conditions
                _.forEach(conditions, function (condition) {
                    if (condition) {

                        spReportEntityQueryManager.updateCondition(currentReportEntity, condition.expid, condition.oper, condition.type, condition.argtype, condition.values ? condition.values : condition.value);
                        if (existingConditions && existingConditions.length > 0) {
                            //remove current condition from existingcondtion list
                            try {
                                existingConditions = _.without(existingConditions, parseInt(condition.expid, 10));

                            } catch (e) {

                            }
                        }
                    }
                });

                //reset all other conditions
                if (existingConditions && existingConditions.length > 0) {
                    _.forEach(existingConditions, function (conditionId) {
                        if (conditionId > 0) {
                            spReportEntityQueryManager.resetConditionById(currentReportEntity, conditionId);
                        }
                    });
                }

                exports.noticeReportBuilder();
            }
        };

        exports.updateReportConditions = function (conditionIds, conditions) {
            if (currentReportEntity) {
                for (var i = 0; i < conditionIds.length; i++) {
                    exports.updateReportCondition(conditionIds[i], conditions[i]);
                }
                exports.noticeReportBuilder();
            }
        };

        exports.updateReportCondition = function (conditionId, condition) {
            if (currentReportEntity) {
                _.forEach(currentReportEntity.getReportConditions(), function (reportCondtion) {
                    //only reset all unHidden unLocked report condition
                    if (reportCondtion && reportCondtion.id() === conditionId) {
                        reportCondtion.getEntity().setOperator(condition.operator);
                        var parameter = spReportEntityQueryManager.createReportConditionParameter(condition.type, condition.value, condition.decimalPlace);
                        reportCondtion.getEntity().setConditionParameter(jsonLookup(parameter.id()));

                    }
                });
            }
        };

        //reset all report conditions
        exports.resetReportConditions = function (conditionId, condition) {
            if (currentReportEntity) {
                _.forEach(currentReportEntity.getReportConditions(), function (reportCondtion) {
                    //only reset all unHidden unLocked report condition
                    if (reportCondtion && !reportCondtion.isHidden && !reportCondtion.isLocked) {
                        reportCondtion.getEntity().setOperator(null);
                        reportCondtion.getEntity().setConditionParameter(null);

                    }
                });

                exports.noticeReportBuilder();
            }
        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Orderby Update Common Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //update orderby info to report orderby object
        exports.updateOrderByToReport = function (sortInfo) {
            if (currentReportEntity) {

                var existingOrderByColumns = _.map(currentReportEntity.getReportOrderBys(), function (orderBy) {
                    try {
                        return orderBy.getExpression().getReferencesColumn().id();
                    } catch (e) {
                        return 0;
                    }
                });

                for (var i = 0; i < sortInfo.length; i++) {

                    var columnId = parseInt(sortInfo[i].columnId, 10);

                    if (columnId) {
                        var isReverseOrder = sortInfo[i].sortDirection === 'asc' ? false : true;
                        spReportEntityQueryManager.addOrderByToReport(currentReportEntity, columnId, isReverseOrder, i);
                        if (existingOrderByColumns && existingOrderByColumns.length > 0) {
                            existingOrderByColumns = _.without(existingOrderByColumns, columnId);
                        }
                    }
                }


                if (existingOrderByColumns && existingOrderByColumns.length > 0) {
                    _.forEach(existingOrderByColumns, function (currentColumnId) {
                        if (currentColumnId > 0) {
                            spReportEntityQueryManager.removeReportOrderByByReportColumnId(currentReportEntity, currentColumnId);
                            spReportEntityQueryManager.removeReportConditionsByReportColumnId(currentReportEntity, currentColumnId);
                        }
                    });
                }

                exports.noticeReportBuilder();
            }

        };

        //add new report orderby to report
        exports.addOrderByToReport = function (columnId, sortInfo) {
            if (currentReportEntity) {
                var reportOrderBy = null;

                _.each(currentReportEntity.getReportOrderBys(), function (orderBy) {
                    var orderByExpression = orderBy.getExpression();
                    if (orderByExpression && orderByExpression.getReferencesColumn() && orderByExpression.getReferencesColumn().id() === columnId) {
                        reportOrderBy = orderBy;
                        orderBy.getEntity().setReverseOrder(sortInfo.reverseOrder);
                        orderBy.getEntity().setOrderPriority(sortInfo.orderPriority);
                    }
                });

                if (!reportOrderBy) {
                    reportOrderBy = spReportEntityQueryManager.createReportOrderBy(sortInfo, columnId);

                    currentReportEntity.addReportOrderBy(reportOrderBy);
                }

                exports.noticeReportBuilder();
            }
        };
        //remove report orderby from report
        exports.removeOrderByFromReport = function (columnId) {
            if (currentReportEntity) {

                spReportEntityQueryManager.removeReportOrderByByReportColumnId(currentReportEntity, columnId);                
                exports.noticeReportBuilder();
            }

        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Rollup Update Common Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Update the report rollup options
        exports.updateReportRollupOptions = function (rollupRowCounts, rollupRowLabels) {
            if (currentReportEntity) {

                var entity = currentReportEntity.getEntity();

                if (!_.isUndefined(rollupRowCounts) && !_.isNull(rollupRowCounts)) {
                    entity.rollupRowCounts = rollupRowCounts;
                }

                if (!_.isUndefined(rollupRowLabels) && !_.isNull(rollupRowLabels)) {
                    entity.rollupRowLabels = rollupRowLabels;
                }
            }
        };

        // Update the report sort order to match the group order.
        exports.updateSortOrderToMatchGroupingOrder = function () {
            if (currentReportEntity) {
                var sortInfoArray = [],
                    existingReportOrderBys,
                    sortedReportOrderBys,
                    sortedReportColumnGroupings = [],
                    allColumnGroupings,
                    reportEntity = currentReportEntity;

                // Update the sort order to match group by order

                // Get the column ordering in sorted order
                existingReportOrderBys = _.filter(reportEntity.getReportOrderBys(), function (orderBy) {
                    var orderByExpression = orderBy.getExpression();
                    return orderByExpression &&
                        orderByExpression.getReferencesColumn() &&
                        orderByExpression.getReferencesColumn().id();
                });

                sortedReportOrderBys = _.sortBy(existingReportOrderBys, function (orderby) {
                    return orderby.getOrderPriority();
                });

                sortInfoArray = _.map(sortedReportOrderBys, function (orderBy) {
                    var orderByExpression = orderBy.getExpression();
                    return {
                        columnId: orderByExpression.getReferencesColumn().id(),
                        sortDirection: orderBy.getReverseOrder() ? 'desc' : 'asc'
                    };
                });

                // Get the grouping in sorted order
                allColumnGroupings = reportEntity.getReportColumnGroupings();

                // Get all the group by columns in the sorted order
                if (allColumnGroupings && allColumnGroupings.length > 0) {
                    sortedReportColumnGroupings = _.sortBy(allColumnGroupings, function (columnGrouping) {
                        return columnGrouping.getGroupingPriority();
                    });
                }

                _.forEach(sortedReportColumnGroupings, function (columnGrouping, groupIndex) {
                    var groupColumnId, sortIndex, sortInfo;

                    // Find the group column
                    var groupColumn = _.find(reportEntity.getReportColumns(), function (reportColumn) {
                        return _.find(reportColumn.getColumnGrouping(), function (grouping) {
                            return grouping.id() === columnGrouping.id();
                        });
                    });

                    // Get the id
                    groupColumnId = groupColumn.id();

                    sortIndex = _.findIndex(sortInfoArray, function (si) {
                        return si.columnId === groupColumnId;
                    });

                    if (sortIndex !== groupIndex) {
                        if (sortIndex === -1) {
                            // Sort info for the specified grouped column
                            // does not exist add it.
                            sortInfoArray.splice(groupIndex, 0, {
                                columnId: groupColumnId,
                                sortDirection: 'asc'
                            });
                        } else {
                            // Remove it from it's current index and add it
                            // to the correct index
                            sortInfo = sortInfoArray[sortIndex];
                            sortInfoArray.splice(sortIndex, 1);
                            sortInfoArray.splice(groupIndex, 0, sortInfo);
                        }
                    }
                });

                // Update the sort order
                exports.updateOrderByToReport(sortInfoArray);
            }
        };

        // Add a grouping column
        exports.addColumnGrouping = function (columnId, groupingMethod) {
            if (currentReportEntity) {
                // Remove aggregates
                exports.setReportColumnRollups(columnId, null, undefined, undefined, undefined, true);

                if (spReportEntityQueryManager.addReportGroupingColumn(columnId, groupingMethod)) {
                    exports.updateSortOrderToMatchGroupingOrder();
                }
            }
        };

        // Remove column grouping
        exports.removeColumnGrouping = function (columnId) {
            if (currentReportEntity) {
                var currentReportColumn = _.find(currentReportEntity.getReportColumns(), function (reportColumn) {
                    return reportColumn.id().toString() === columnId.toString();
                });

                if (currentReportColumn) {
                    exports.setColumnGroupingCollapsedState(columnId, false);
                    spReportEntityQueryManager.removeReportColumnGrouping(columnId);
                    exports.removeOrderByFromReport(currentReportColumn.id());
                }
            }
        };

        // Set column grouping collapsed state
        exports.setColumnGroupingCollapsedState = function (columnId, collapsed) {
            var currentReportColumn;

            if (!currentReportEntity || !columnId) {
                return;
            }

            currentReportColumn = _.find(currentReportEntity.getReportColumns(), function (reportColumn) {
                return reportColumn.id().toString() === columnId.toString();
            });

            if (currentReportColumn) {
                spReportEntityQueryManager.setReportColumnGroupingCollapsedState(columnId, collapsed);
            }
        };

        // Set the report column rollups
        exports.setReportColumnRollups = function (columnId, rollupMethods, rollupSubTotals, rollupGrandTotals, showOptionLabels, suppressBuilderNotification) {
            var notifyBuilder;

            if (currentReportEntity) {
                var reportEntity;

                reportEntity = currentReportEntity.getEntity();

                if (!_.isUndefined(rollupSubTotals) && !_.isNull(rollupSubTotals)) {
                    reportEntity.rollupSubTotals = rollupSubTotals;
                    notifyBuilder = true;
                }

                if (!_.isUndefined(showOptionLabels) && !_.isNull(showOptionLabels)) {
                    reportEntity.rollupOptionLabels = showOptionLabels;
                    notifyBuilder = true;
                }

                if (!_.isUndefined(rollupGrandTotals) && !_.isNull(rollupGrandTotals)) {
                    reportEntity.rollupGrandTotals = rollupGrandTotals;
                    notifyBuilder = true;
                }

                if (spReportEntityQueryManager.setReportColumnRollups(columnId, rollupMethods)) {
                    notifyBuilder = true;
                }

                if (notifyBuilder && !suppressBuilderNotification) {
                    exports.noticeReportBuilder();
                }
            }
        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Column condition format and display format update Common Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        exports.updateConditionFormattingFromReport = function (columnId, conditionFormatting) {
            if (currentReportEntity && conditionFormatting) {

                var existingReportColumn = _.find(currentReportEntity.getReportColumns(), function (reportColumn) {
                    return reportColumn.id().toString() === columnId.toString();
                });

                var typeArgument = existingReportColumn.getExpression().getEntity().getReportExpressionResultType();
                var typeArgumentAlias = (typeArgument.getIsOfType && typeArgument.getIsOfType()) ? typeArgument.getIsOfType()[0].getAlias() : typeArgument.getType().getAlias();
                var type = spReportEntityQueryManager.getTypeNameByArgumentAlias(typeArgumentAlias);
                var columnFormattingRule = spReportEntityQueryManager.createColumnFormattingRule(conditionFormatting, type);

                if (existingReportColumn) {
                    exports.noticeReportBuilder();
                }
            }
        };

        exports.updateColumnDisplayFormat = function (columnId, columnType, columnDisplayFormat, conditionFormatting) {
            if (currentReportEntity) {
                var existingReportColumn = _.find(currentReportEntity.getReportColumns(), function (reportColumn) {
                    return reportColumn.id().toString() === columnId.toString();
                });
                if (existingReportColumn) {

                    var typeArgument = existingReportColumn.getExpression().getEntity().getReportExpressionResultType();
                    var typeArgumentAlias;
                    if (typeArgument) {
                        typeArgumentAlias = (typeArgument.getIsOfType && typeArgument.getIsOfType()) ? typeArgument.getIsOfType()[0].getAlias() : typeArgument.getType().getAlias();
                    } else {
                        typeArgumentAlias = columnType;
                    }

                    //if column expression is resourceExpress and the resultType Argument is stringArgument, this resultType should be convert as resourceArgument
                    if (existingReportColumn.getExpression().getTypeAlias() === 'core:resourceExpression' && typeArgumentAlias === 'core:stringArgument') {
                        typeArgumentAlias = 'core:resourceArgument';
                    }

                    // If the column is a field expression but the column type is an inline relationship, convert the type to be a resource argument.
                    // This can happen if applying an inline relationship operator to the name column of the root entity
                    if (existingReportColumn.getExpression().getTypeAlias() === 'core:fieldExpression' && columnType === 'InlineRelationship') {
                        typeArgumentAlias = 'core:resourceArgument';
                    }

                    var type = '';
                    var entityTypeId;
                    if (typeArgumentAlias === 'core:resourceArgument' || typeArgumentAlias === 'resourceArgument' || columnType === 'StructureLevels') {
                        type = columnType;
                        if (columnType === 'StructureLevels') {
                            entityTypeId = sp.result(existingReportColumn.getExpression(), 'getEntity.structureViewExpressionSourceNode.resourceReportNodeType.id');
                        } else {
                            entityTypeId = existingReportColumn.getExpression().getEntityTypeId();
                        }                                                
                    } else {
                        type = spReportEntityQueryManager.getTypeNameByArgumentAlias(typeArgumentAlias);
                    }

                    if (conditionFormatting) {
                        var columnFormattingRule = spReportEntityQueryManager.createColumnFormattingRule(conditionFormatting, type, entityTypeId);
                        existingReportColumn.setColumnFormattingRule(columnFormattingRule);
                    }


                    var displayFormat = spReportEntityQueryManager.createColumnDisplayFormat(columnDisplayFormat, conditionFormatting, type);
                    existingReportColumn.setColumnDisplayFormat(displayFormat);
                    
                    exports.noticeReportBuilder();
                }
            }
        };

        exports.getStructureViewIdForColumn = function (columnId) {
            var existingReportColumn, existingExpression, existingExpressionTypeAlias;

            if (!currentReportEntity) {
                return 0;
            }

            existingReportColumn = _.find(currentReportEntity.getReportColumns(), function (reportColumn) {
                return reportColumn.id().toString() === columnId.toString();
            });

            if (!existingReportColumn) {
                return 0;
            }

            existingExpression = existingReportColumn.getEntity().columnExpression;
            if (existingExpression && existingExpression.type) {
                existingExpressionTypeAlias = existingExpression.type.alias();
                if (existingExpressionTypeAlias === 'core:structureViewExpression') {
                    return sp.result(existingExpression, 'structureViewExpressionStructureView.id') || 0;
                }
            }

            return 0;
        };

        exports.updateStructureViewExpression = function (columnId, structureViewId, nameFieldId) {
            var existingReportColumn, newExpression, newExpressionType,
                existingExpression, existingExpressionTypeAlias, sourceNode;

            if (!currentReportEntity) {
                return;
            }

            existingReportColumn = _.find(currentReportEntity.getReportColumns(), function (reportColumn) {
                return reportColumn.id().toString() === columnId.toString();
            });

            if (!existingReportColumn) {
                return;
            }                                    

            existingExpression = existingReportColumn.getEntity().columnExpression;
            if (existingExpression && existingExpression.type) {
                existingExpressionTypeAlias = existingExpression.type.alias();
            } else {
                return;
            }
            
            if (!structureViewId) {
                // No structure view is specified
                if (existingExpressionTypeAlias !== 'core:structureViewExpression') {
                    // The expression is not a structure view so return.
                    sourceNode = existingExpression.getSourceNode();
                } else {
                    // The expression is not a structure view.
                    // Revert back to a resource or field expression on the name field        
                    if (currentReportEntity.getRootNode() && currentReportEntity.getRootNode().id() === existingExpression.structureViewExpressionSourceNode.id()) {
                        newExpressionType = 'fieldExpression';                                                        
                        newExpression = spEntity.fromJSON({
                            typeId: newExpressionType,
                            fieldExpressionField: jsonLookup(nameFieldId),
                            sourceNode: jsonLookup(existingExpression.structureViewExpressionSourceNode),
                            reportExpressionResultType: jsonLookup(existingExpression.reportExpressionResultType)
                        });
                    } else {
                        newExpressionType = 'resourceExpression';
                        // Remove fieldExpressionField for resourceExpression
                        newExpression = spEntity.fromJSON({
                            typeId: newExpressionType,
                            sourceNode: jsonLookup(existingExpression.structureViewExpressionSourceNode),
                            reportExpressionResultType: jsonLookup(existingExpression.reportExpressionResultType)
                        });
                    }                                        

                    

                    existingReportColumn.getEntity().setColumnExpression(newExpression);
                    
                    sourceNode = existingExpression.structureViewExpressionSourceNode;
                }                
            } else {
                // A structure view is specified
                if (existingExpressionTypeAlias !== 'core:structureViewExpression') {
                    // The expression is not a structure view so create a new one
                    newExpression = spEntity.fromJSON({
                        typeId: 'structureViewExpression',
                        structureViewExpressionSourceNode: jsonLookup(existingExpression.getSourceNode()),
                        structureViewExpressionStructureView: jsonLookup(structureViewId),
                        reportExpressionResultType: jsonLookup(existingExpression.reportExpressionResultType)
                    });                    

                    existingReportColumn.getEntity().setColumnExpression(newExpression);
                    
                    sourceNode = existingExpression.getSourceNode();
                } else {
                    // The expression is already a structure view, update it.                    
                    if (sp.result(existingExpression, 'structureViewExpressionStructureView.id') !== structureViewId) {
                        existingExpression.structureViewExpressionStructureView = jsonLookup(structureViewId);             
                    }                    

                    sourceNode = existingExpression.structureViewExpressionSourceNode;
                }
            }

            updateConditionStructureViewExpression(currentReportEntity, sourceNode, nameFieldId, structureViewId);
        };

        // Update any condition for the name field 
        function updateConditionStructureViewExpression(reportEntity, columnSourceNode, nameFieldId, structureViewId) {
            var existingExpression, existingExpressionTypeAlias,
                newExpression, newExpressionType;

            if (!reportEntity || !columnSourceNode) {
                return;
            }

            // Find any analyser condition for this column
            var existingCondition = _.find(reportEntity.getReportConditions(), function (reportCondition) {
                var conditionSourceNodeId;
                var conditionExpressionTypeAlias = sp.result(reportCondition, 'getEntity.conditionExpression.type.alias');

                if (conditionExpressionTypeAlias !== 'core:structureViewExpression') {
                    conditionSourceNodeId = sp.result(reportCondition, 'getEntity.conditionExpression.sourceNode.id');
                } else {
                    conditionSourceNodeId = sp.result(reportCondition, 'getEntity.conditionExpression.structureViewExpressionSourceNode.id');
                }
                
                return conditionSourceNodeId === sp.result(columnSourceNode, 'id');
            });            

            if (!existingCondition) {
                return;
            }

            existingExpression = sp.result(existingCondition, 'getEntity.conditionExpression');
            if (existingExpression && existingExpression.type) {
                existingExpressionTypeAlias = existingExpression.type.alias();
            } else {
                return;
            }

            if (existingExpressionTypeAlias === 'core:structureViewExpression' && !sp.result(existingCondition, 'getEntity.conditionExpression.structureViewExpressionSourceNode')) {
                return;
            }                      

            // Update the existingCondition
            if (!structureViewId) {
                // No structure view is specified
                if (existingExpressionTypeAlias !== 'core:structureViewExpression') {
                    // The expression is not a structure view so return.
                    return;
                } else {
                    if (currentReportEntity.getRootNode() && currentReportEntity.getRootNode().id() === existingExpression.structureViewExpressionSourceNode.id()) {
                        newExpressionType = 'fieldExpression';
                    } else {
                        newExpressionType = 'resourceExpression';
                    }

                    // The expression is a structure view.
                    // Revert back to a resource or field expression on the name field                                        
                    newExpression = spEntity.fromJSON({
                        typeId: newExpressionType,
                        fieldExpressionField: jsonLookup(nameFieldId),
                        sourceNode: jsonLookup(existingExpression.structureViewExpressionSourceNode),
                        reportExpressionResultType: jsonLookup(existingExpression.reportExpressionResultType)
                    });

                    existingCondition.getEntity().setOperator(null);
                    existingCondition.getEntity().setConditionParameter(null);
                    existingCondition.getEntity().setConditionExpression(newExpression);                    
                }
            } else {
                // A structure view is specified
                if (existingExpressionTypeAlias !== 'core:structureViewExpression') {
                    // The expression is not a structure view so create a new one
                    newExpression = spEntity.fromJSON({
                        typeId: 'structureViewExpression',
                        structureViewExpressionSourceNode: jsonLookup(existingExpression.getSourceNode()),
                        structureViewExpressionStructureView: jsonLookup(structureViewId),
                        reportExpressionResultType: jsonLookup(existingExpression.reportExpressionResultType)
                    });

                    existingCondition.getEntity().setOperator(null);
                    existingCondition.getEntity().setConditionParameter(null);
                    existingCondition.getEntity().setConditionExpression(newExpression);                    
                } else {
                    // The expression is already a structure view, update it.                    
                    if (sp.result(existingExpression, 'structureViewExpressionStructureView.id') !== structureViewId) {
                        existingExpression.structureViewExpressionStructureView = jsonLookup(structureViewId);
                    }                    
                }
            }            
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Summarise update Common Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //create summarise node and related aggregated column in report
        exports.createSummarise = function (treeNode, summariseActions, childNodesAggregateEntity) {
            if (currentReportEntity) {
                var aggregateReportNode = spReportEntityQueryManager.addAggregateNodeToQuery(currentReportEntity, treeNode);

                if (childNodesAggregateEntity && childNodesAggregateEntity.length > 0) {
                    _.forEach(childNodesAggregateEntity, function (childNodeAggregateEntity) {
                        if (childNodeAggregateEntity.pe && childNodeAggregateEntity.pae) {
                            var aggregateReportNodeId = spReportEntityQueryManager.removeSummarise(currentReportEntity, childNodeAggregateEntity.pe, childNodeAggregateEntity.pae);
                            spReportEntityQueryManager.updateAggregateColumnsByAggregateNodeId(currentReportEntity, aggregateReportNodeId, aggregateReportNode);
                            childNodeAggregateEntity.pae = null;
                        }
                    });
                }

                exports.updateAggregateColumns(aggregateReportNode, summariseActions);
            }

        };


        //update all summarise action e.g. remove column, add/update aggregate column
        exports.updateAggregateColumns = function (aggregateReportNode, summariseActions) {
            if (currentReportEntity) {
                var removeActions = [];

                //sort all summariseActions, add first, then remove
                summariseActions = _.sortBy(summariseActions, 'action'); // _.sortBy(summariseActions, function (summariseAction) {return summariseActions.action;});

                _.forEach(summariseActions, function (summariseAction) {
                    if (summariseAction.action === 'remove') {
                        //make all remove action at last
                        exports.removeColumnById(summariseAction.originalColumnId, false, aggregateReportNode);
                        //removeActions.push(summariseAction);
                    } else {
                        spReportEntityQueryManager.addAggregateColumn(currentReportEntity, aggregateReportNode, summariseAction);
                    }
                });

                spReportEntityQueryManager.resortReportColumns(currentReportEntity);
                spReportEntityQueryManager.updateAggregateGroupBy(currentReportEntity, aggregateReportNode);

                exports.noticeReportBuilder();
            }
        };

        //drag the aggregate columns to report and insert after target column
        exports.updateAggregateColumnsAfterColumn = function (aggregateReportNode, field, summariseActions, currentNodeEntity, subNodeEntity, insertAfterColumn) {
            if (currentReportEntity) {
                //sort all summariseActions, add first, then remove
                summariseActions = _.sortBy(summariseActions, 'action'); // _.sortBy(summariseActions, function (summariseAction) {return summariseActions.action;});

                _.forEach(summariseActions, function (summariseAction) {
                    if (summariseAction.action === 'remove') {
                        //make all remove action at last
                        exports.removeColumnById(summariseAction.originalColumnId, false);
                        //removeActions.push(summariseAction);
                    } else {
                        if (summariseAction.originalColumnId > 0) {
                            spReportEntityQueryManager.addAggregateColumn(currentReportEntity, aggregateReportNode, summariseAction);
                        } else {
                            spReportEntityQueryManager.addAggregateColumnAfterColumn(currentReportEntity, aggregateReportNode, field, summariseAction, currentNodeEntity, subNodeEntity, insertAfterColumn);
                            insertAfterColumn++;
                        }
                    }
                });

                spReportEntityQueryManager.updateAggregateGroupBy(currentReportEntity, aggregateReportNode);
                exports.noticeReportBuilder();
            }

        };

        //remove summarise node and all related aggregate columns, then create grouped by column
        exports.removeSummarise = function (parentReportNode, aggregateReportNode) {
            if (currentReportEntity) {
                var aggregateReportNodeId = spReportEntityQueryManager.removeSummarise(currentReportEntity, parentReportNode, aggregateReportNode);
                spReportEntityQueryManager.removeAggregateColumnsByAggregateNodeId(currentReportEntity, aggregateReportNodeId);
                // the rootnode is aggregated node,
                if (!parentReportNode) {
                    //add back the id column
                    var idField = spReportEntityQueryManager.createReportColumn('idExpression', currentReportEntity.getEntity().rootNode, '_id', '', null, true, null, -2);
                    currentReportEntity.addReportColumn(idField);
                    spReportEntityQueryManager.resortReportColumns(currentReportEntity);
                }
                exports.noticeReportBuilder();
            }
        };

        return exports;
    }
}());
