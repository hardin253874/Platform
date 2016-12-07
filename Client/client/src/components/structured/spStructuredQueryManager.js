// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, spReportEntity */

/**
* Module implementing a ReportEntityQueryManager functions.    
*         
*/

function ReportEntityQueryManager() {
    this.ReportEntity = null;
    this.NameFieldId = null;
}

var spReportEntityQueryManager = spReportEntityQueryManager || new ReportEntityQueryManager();

(function (spReportEntityQueryManager) {
    //init ReportEntity
    spReportEntityQueryManager.setEntity = function (reportEntity) {
        this.ReportEntity = reportEntity;
    };

    spReportEntityQueryManager.setNameFieldId = function (nameFieldId) {
        this.NameFieldId = nameFieldId;
    };

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // ReportNode Manager Static Methods
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /** add new node to report query
    * @param {object} node the Report Builder TreeNode object
    */
    spReportEntityQueryManager.addNodeToQuery = function (reportEntity, node, namefield) {
        spReportEntityQueryManager.setEntity(reportEntity);

        spReportEntityQueryManager.addRelationship(reportEntity.getRootNode(), node);

        //add name field in query
        if (namefield) {
            spReportEntityQueryManager.addColumnToReport(reportEntity, namefield, node.qe, -1);
        }
    };

    /** add new entity to report query by resource node
    * @param {object} parentEntity  the parent Entity
    * @param {object} selectedTreeNode of the Report Builder Tree
    */
    spReportEntityQueryManager.addRelationship = function (parentEntity, selectedTreeNode) {
        if (parentEntity) {
            if (parentEntity.id() === selectedTreeNode.pe.id()) {

                var newRelatedEntity = null;
                if (selectedTreeNode.qe) {
                    newRelatedEntity = selectedTreeNode.qe;
                } else {
                    newRelatedEntity = spReportEntityQueryManager.createRelatedEntity(selectedTreeNode.etid, selectedTreeNode.relid);
                    selectedTreeNode.qe = newRelatedEntity;
                }

                var existingsRelationshipNodes = _.find(parentEntity.getRelatedReportNodes(function (relatedReportNode) {
                    return relatedReportNode.getEntity().id() === newRelatedEntity.id();
                }));

                if (!existingsRelationshipNodes) {
                    parentEntity.addRelatedReportNode(newRelatedEntity);
                }

            }
            else {
                if (parentEntity.getRelatedReportNodes()) {

                    _.each(parentEntity.getRelatedReportNodes(), function (relatedRportNode) {
                        spReportEntityQueryManager.addRelationship(relatedRportNode, selectedTreeNode);
                    });
                }
            }
        }
    };

    spReportEntityQueryManager.reomveNodeFromQuery = function (reportEntity, node) {

        if (node.pae) {
            spReportEntityQueryManager.removeReportColumnBySourceNodeId(reportEntity, node.pae.id());
            reportEntity.updateReportColumns();
            spReportEntityQueryManager.removeSummarise(reportEntity, node.pe, node.pae);            
        }

        if (node.pe) {
            if (node.children && node.children.length > 0) {
                _.each(node.children, function (childNode) {
                    spReportEntityQueryManager.reomveRelationship(reportEntity, node.qe, childNode);
                });
            }

            spReportEntityQueryManager.reomveRelationship(reportEntity, reportEntity.getRootNode(), node);

        }
    };

    //Remove relationship by nodeId
    spReportEntityQueryManager.reomveRelationshipByNodeId = function (reportEntity, parentEntity, nodeId) {
        
        //if current parent node entity is aggregate node, get the groupedNode property
        if (parentEntity && parentEntity.getTypeAlias() === 'core:aggregateReportNode') {
            parentEntity = parentEntity.getGroupedNode();
        }

        if (parentEntity && parentEntity.getRelatedReportNodes) {           
            //find current nodeId exists in relatedReportNodes
            var existingNode = _.filter(parentEntity.getRelatedReportNodes(), function(node) { return node.id() === nodeId; });

            if (existingNode && existingNode.length > 0) {
                //remove all reportcolumn, reportcondition, reportorderby first
                spReportEntityQueryManager.removeReportColumnBySourceNodeId(reportEntity, nodeId, true);
                spReportEntityQueryManager.removeReportConditionBySourceNodeId(reportEntity, nodeId, true);

                parentEntity.removeRelatedReportNodeById(nodeId);
            } else {
                if (parentEntity.getRelatedReportNodes()) {
                    _.each(parentEntity.getRelatedReportNodes(), function (relatedRportNode) {
                        spReportEntityQueryManager.reomveRelationshipByNodeId(reportEntity, relatedRportNode, nodeId);
                    });
                }
            }           
        }
    };


    spReportEntityQueryManager.reomveRelationship = function (reportEntity, parentEntity, selectedTreeNode) {
        if (parentEntity) {
            spReportEntityQueryManager.setEntity(reportEntity);

            var parentNodeId = selectedTreeNode.pe.id();
            var nodeId = selectedTreeNode.qe.id();
            if (parentEntity.id() === parentNodeId) {

                //remove all reportcolumn, reportcondition, reportorderby first
                spReportEntityQueryManager.removeReportColumnBySourceNodeId(reportEntity, nodeId, true);
                spReportEntityQueryManager.removeReportConditionBySourceNodeId(reportEntity, nodeId, true);

                //remove reportnode from reportEntity
                parentEntity.removeRelatedReportNodeById(nodeId);
            } else if (parentEntity.getTypeAlias() === 'core:aggregateReportNode' && parentEntity.getGroupedNode && parentEntity.getGroupedNode().id() === parentNodeId) {
                //remove all reportcolumn, reportcondition, reportorderby first
                spReportEntityQueryManager.removeReportColumnBySourceNodeId(reportEntity, nodeId, true);
                spReportEntityQueryManager.removeReportConditionBySourceNodeId(reportEntity, nodeId, true);

                //remove reportnode from reportEntity
                parentEntity.removeRelatedReportNodeById(nodeId);
            }
            else if (parentEntity.getTypeAlias() === 'core:aggregateReportNode' && parentEntity.getGroupedNode && spReportEntityQueryManager.resourceNodeUnderAggregated(parentEntity.getGroupedNode(), parentNodeId)) {
                //remove all reportcolumn, reportcondition, reportorderby from aggregate node
                spReportEntityQueryManager.removeReportColumnByAggregateNodeId(reportEntity, parentEntity.id(), nodeId, true);
                spReportEntityQueryManager.removeReportConditionBySourceNodeId(reportEntity, nodeId, true);

                //remove reportnode from aggregated grouped reportEntity
                parentEntity.getGroupedNode().removeRelatedReportNodeById(nodeId);
            }
            else {
                if (parentEntity.getRelatedReportNodes()) {
                    _.each(parentEntity.getRelatedReportNodes(), function (relatedRportNode) {
                        spReportEntityQueryManager.reomveRelationship(reportEntity, relatedRportNode, selectedTreeNode);
                    });
                }
            }
        }
    };

    spReportEntityQueryManager.getReportNodeById = function (reportNode, nodeId) {
        var selectedReportNode = null;

        if (reportNode) {
            if (reportNode.id() === nodeId) {
                selectedReportNode = reportNode;
            }
            else {
                // COMMENTED the following out as parentEntity isn't defined and so I am assuming
                // this code is never executed. Will get the original author to check

                // if (parentEntity.getRelatedReportNodes()) {
                //     _.each(parentEntity.getRelatedReportNodes(), function (relatedRportNode) {
                //         var node = spReportEntityQueryManager.getReportNodeById(relatedRportNode, nodeId);
                //         if (!node) {
                //             selectedReportNode = node;
                //         }
                //     });
                // }
            }
        }

        return selectedReportNode;
    };

    spReportEntityQueryManager.updateNodeEntityToQuery = function (reportEntity, nodeEntity) {
        if (nodeEntity) {
            spReportEntityQueryManager.setEntity(reportEntity);

            spReportEntityQueryManager.updateRelationship(reportEntity.getRootNode(), nodeEntity.getEntity());

        }
    };

    spReportEntityQueryManager.updateRelationship = function (parentEntity, nodeEntity) {
        if (parentEntity) {
            if (parentEntity.id() === nodeEntity.id()) {
                parentEntity._reportNodeEntity = nodeEntity;
            }
            else {
                if (parentEntity.getGroupedNode()) {
                    spReportEntityQueryManager.updateRelationship(parentEntity.getGroupedNode(), nodeEntity);
                } else {
                    if (parentEntity.getRelatedReportNodes()) {
                        _.each(parentEntity.getRelatedReportNodes(), function (relatedRportNode) {
                            spReportEntityQueryManager.updateRelationship(relatedRportNode, nodeEntity);
                        });
                    }
                }
            }
        }
    };

    spReportEntityQueryManager.addAggregateNodeToQuery = function (reportEntity, treeNode) {
        spReportEntityQueryManager.setEntity(reportEntity);

        //create aggregate node
        if (!treeNode.pae) {
            treeNode.pae = spReportEntityQueryManager.createAggreateEntity(treeNode.qe.getEntity());
        }

        //if the root node is aggregated
        if (treeNode.nid === reportEntity.getRootNode().id()) {
            reportEntity.getEntity().rootNode = treeNode.pae.getEntity();
            reportEntity.updateRootNode();

            //remove _id column in reportcolumns array
            var idColumn = _.find(reportEntity.getReportColumns(), function (selectColumn) {
                return (selectColumn.getExpression().getTypeAlias() === 'idExpression' || selectColumn.getExpression().getTypeAlias() === 'core:idExpression') && selectColumn.isHidden() === true;
            });

            if (idColumn) {
                reportEntity.removeReportColumn(idColumn);
            }


        } else {
            spReportEntityQueryManager.addAggregateNode(reportEntity.getRootNode(), treeNode);
        }

        return treeNode.pae;
    };

    spReportEntityQueryManager.addAggregateNode = function (parentReportNode, treeNode) {

        if (parentReportNode) {

            var existReportNode = _.find(parentReportNode.getRelatedReportNodes(), function (relatedReportNode) {
                return relatedReportNode.id() === treeNode.nid;
            });

            if (existReportNode) {
                parentReportNode.setSummariseReportNode(treeNode.qe.getEntity(), treeNode.pae.getEntity());
            } else {
                _.forEach(parentReportNode.getRelatedReportNodes(), function (relatedReportNode) {
                    spReportEntityQueryManager.addAggregateNode(relatedReportNode, treeNode);
                });
            }

        }

    };

    spReportEntityQueryManager.updateAggregateGroupBy = function (reportEntity, aggregateReportNode) {

        var relatedNodeIds = spReportEntityQueryManager.getAggregateGroupedNodeIds(aggregateReportNode);

        var expressions = [];

        _.forEach(reportEntity.getReportColumns(), function (column) {
            var expression = column.getExpression();
            if (expression) {
                var expressionType = expression.getTypeAlias();
                if (expression.getSourceNode()) {
                    if (_.includes(relatedNodeIds, expression.getSourceNode().id())) {                       
                        if (expressionType !== 'aggregateExpression' && expressionType !== 'core:aggregateExpression')
                            expressions.push(expression);
                        else if (expression.getAggregateMethod() &&
                            expression.getAggregateMethod()._id.getAlias() !== 'aggCount' &&
                            expression.getAggregateMethod()._id.getAlias() !== 'aggAverage' &&
                            expression.getAggregateMethod()._id.getAlias() !== 'aggSum') {
                            expressions.push(expression);
                        }
                    }
                }
            }

        });

        var existingExpressions = aggregateReportNode.getEntity().getGroupedBy();
        //remove all existing groupby expressions
        _.forEach(existingExpressions, function (existingExpression) {            
            aggregateReportNode.getEntity().getGroupedBy().remove(existingExpression);
        });

        //add all related groupby expressions
        _.forEach(expressions, function (newExpression) {
            if (newExpression) {
               
                //clone the existing column expression, otherwise after remove summarise and delete the aggregate node, will cascade all groupedby column expression
                //var newExpressionEntity = spReportEntityQueryManager.cloneExpressionEntity(newExpression);
                var newExpressionEntity = newExpression.getEntity();
                //before add expression as gropuedby, remove it's parentGroupedBy if exists, otherwise it will cause Cardinality Violation error
                removeParentGroupedBy(newExpressionEntity, aggregateReportNode);

                aggregateReportNode.getEntity().getGroupedBy().add(newExpressionEntity);
            }
        });
    };

    function removeParentGroupedBy(expression, aggregateReportNode) {
        if (expression &&
            expression.getRelationship &&
            expression.getRelationship('parentGroupedBy') &&
            expression.getRelationship('parentGroupedBy').remove) {
            var parentGroupedBys = expression.getRelationship('parentGroupedBy');
            _.forEach(parentGroupedBys, function (parentEntity) {
                if (parentEntity.id() !== aggregateReportNode.id()) {
                    expression.getRelationship('parentGroupedBy').remove(parentEntity);
                }
            });            
        }
    }   

    function clearParentAggregatedNode(node) {
        if (node &&
            node.getRelationship &&
            node.getRelationship('parentAggregatedNode') &&
            node.getRelationship('parentAggregatedNode').clear) {
            node.getRelationship('parentAggregatedNode').clear();
        }
    }

    function deleteParentAggregatedNode(node) {
        if (node &&
            node.getRelationship &&
            node.getRelationship('parentAggregatedNode') &&
            node.getRelationship('parentAggregatedNode').deleteExisting) {
            node.getRelationship('parentAggregatedNode').deleteExisting();
        }
    }

    spReportEntityQueryManager.getAggregateGroupedNodeIds = function (reportNode) {       
        var nodeIds = [];
        if (reportNode) {
            nodeIds.push(reportNode.id());

            var nodeType = '';

            if (reportNode.getType()) {
                nodeType = reportNode.getTypeAlias();
            } else {
                nodeType = reportNode.getEntity()._typeIds[0].getAlias();
            }

            if (nodeType === "aggregateReportNode" || nodeType === "core:aggregateReportNode") {
                nodeIds = _.union(nodeIds, spReportEntityQueryManager.getAggregateGroupedNodeIds(reportNode.getGroupedNode()));
            } else {
                _.forEach(reportNode.getRelatedReportNodes(), function(relatedNode) {
                    nodeIds = _.union(nodeIds, spReportEntityQueryManager.getAggregateGroupedNodeIds(relatedNode));
                });
            }
        }
        return nodeIds;
    };

    spReportEntityQueryManager.removeSummarise = function (reportEntity, parentReportNode, aggregateReportNode) {
        spReportEntityQueryManager.setEntity(reportEntity);

        var aggregateReportNodeId = aggregateReportNode.id();
        var groupedNode = aggregateReportNode.getGroupedNode();
        // the rootnode is aggregated node,
        if (!parentReportNode) {
            var groupedNodeEntity = groupedNode.getEntity();
            reportEntity.getEntity().rootNode = groupedNodeEntity;
            //groupedNode.removeParentAggregatedNode();
            //todo: should use deleteExisting for parent aggregatedNode. However it will cascade delete the grouped by expression of column   
            //use removeExisting to replace deleteExisting for groupedNode remove parent aggregateNode relationship first
            //in updateAggregateGroupBy method, don't directly add column expression into aggregate groupedby relationship, clone first and add the clone object.            
            clearParentAggregatedNode(groupedNodeEntity);
            reportEntity.getEntity().rootNode.groupedNode = null;
            reportEntity.updateRootNode();                                  

        } else {
            //loop through report Node tree to remove aggregate node.          
            spReportEntityQueryManager.removeAggregateNode(reportEntity.getRootNode(), parentReportNode.id(), aggregateReportNode);
        }

        return aggregateReportNodeId;
    };

    spReportEntityQueryManager.removeAggregateNode = function (parentReportNode, parentReportNodeId, aggregateReportNode) {
        if (parentReportNode) {

            if (parentReportNode.id() === parentReportNodeId) {
                parentReportNode.removeSummariseReportNode(aggregateReportNode);
            }
            else if (parentReportNode.getGroupedNode && parentReportNode.getGroupedNode()) {
                if (parentReportNode.getGroupedNode().id() === parentReportNodeId) {
                    spReportEntityQueryManager.removeAggregateNode(parentReportNode.getGroupedNode(), parentReportNodeId, aggregateReportNode);
                } else {
                    _.forEach(parentReportNode.getGroupedNode().getRelatedReportNodes(), function (groupedRelatedReportNode) {
                        spReportEntityQueryManager.removeAggregateNode(groupedRelatedReportNode, parentReportNodeId, aggregateReportNode);
                    });
                }
            }
            else {
                _.forEach(parentReportNode.getRelatedReportNodes(), function (relatedReportNode) {
                    spReportEntityQueryManager.removeAggregateNode(relatedReportNode, parentReportNodeId, aggregateReportNode);
                });
            }

        }
    };

    spReportEntityQueryManager.removeAggregateColumnsByAggregateNodeId = function (reportEntity, aggregateReportNodeId) {

        //Get all related aggregate Columns

        var groupedExpressions = [];

        _.forEach(reportEntity.getReportColumns(), function (reportColumn) {
            var reportColumnExpression = reportColumn.getExpression();
            if (reportColumn.getEntity() && reportColumn.isHidden() === false && reportColumnExpression && reportColumnExpression.getSourceNode() && reportColumnExpression.getSourceNode().id() === aggregateReportNodeId) {
                var aggregateExpression = reportColumnExpression.getAggregatedExpression();

                var originalColumnDisplayFormat = null;

                if (reportColumn.getEntity().columnDisplayFormat && reportColumn.getEntity().columnDisplayFormat.formatAlignment) {
                    originalColumnDisplayFormat = getOriginalColumnDisplayFormat(reportColumn);
                }

                //get original column rollup
                var originalColumnRollup = null;
                if (reportColumn.getEntity().columnRollup && reportColumn.getEntity().columnRollup.length > 0 &&  aggregateExpression.getEntity() && aggregateExpression.getEntity().reportExpressionResultType) {
                    originalColumnRollup = getOriginalColumnRollups(reportColumn.getEntity().columnRollup, aggregateExpression.getEntity().reportExpressionResultType.name);
                }


                if (aggregateExpression) {
                    var existGroupedExpression = _.find(groupedExpressions, function (groupedExpression) {
                        var aggExpTypeAlias = sp.result(groupedExpression, 'expression.getTypeAlias');
                        var groupExpFieldId = sp.result(groupedExpression, 'expression.getField.id');
                        var aggExpFieldId = sp.result(aggregateExpression, 'getField.id');

                        return groupedExpression.expression.getSourceNode().id() === aggregateExpression.getSourceNode().id() &&
                            ((groupExpFieldId === aggExpFieldId && groupExpFieldId) || (aggExpTypeAlias === 'core:structureViewExpression'));
                    });

                    if (!existGroupedExpression) {
                        var originalName = getOriginalColumnName(reportColumn);
                        groupedExpressions.push({ name: originalName, expression: aggregateExpression, displayOrder: reportColumn.displayOrder(), originalColumnDisplayFormat: originalColumnDisplayFormat, originalColumnRollup: originalColumnRollup });
                    }
                }
            }
        });

        //remove all aggregated reportcolumn, reportcondition, reportorderby first, but don't change the order
        spReportEntityQueryManager.removeReportColumnBySourceNodeId(reportEntity, aggregateReportNodeId, false);
        spReportEntityQueryManager.removeReportConditionBySourceNodeId(reportEntity, aggregateReportNodeId, false);
                
        if (groupedExpressions.length > 0) {
            _.forEach(groupedExpressions, function(groupedExpression) {
                if (groupedExpression.expression) {
                    var existColumn = _.find(reportEntity.getReportColumns(), function(column) {
                        var columnExpression = column.getExpression();

                        if (columnExpression.getSourceNode().id() !== groupedExpression.expression.getSourceNode().id()) {
                            return false;
                        }

                        if (groupedExpression.expression.getField()) {
                            return columnExpression.getField() && columnExpression.getField().id() === groupedExpression.expression.getField().id();
                        } else if (groupedExpression.expression.getTypeAlias() === 'core:resourceExpression') {
                            return true;
                        } else {
                            return false;
                        }
                        
                    });                

                    if (!existColumn) {
                        //if the expression datastate is not create. this expression will be removed by aggregated Column remove. 
                        //clone the expression to set in aggregatedExpression
                        var cloneGroupedExpression;
                        if (groupedExpression.expression && groupedExpression.expression.getEntity && groupedExpression.expression.getEntity() && groupedExpression.expression.getEntity().getDataState() !== spEntity.DataStateEnum.Create) {
                            cloneGroupedExpression = spReportEntityQueryManager.cloneExpression(groupedExpression.expression);
                        }


                        var reportColumn = spEntity.fromJSON({
                            typeId: 'reportColumn',
                            name: groupedExpression.name ? groupedExpression.name : jsonString(),
                            columnDisplayOrder: groupedExpression.displayOrder ? groupedExpression.displayOrder : 0,
                            columnIsHidden: false,
                            columnExpression: cloneGroupedExpression ? jsonLookup(cloneGroupedExpression.getEntity()) : jsonLookup(groupedExpression.expression.getEntity()),
                            columnFormattingRule: jsonLookup(),
                            columnDisplayFormat: groupedExpression.originalColumnDisplayFormat ? jsonLookup(groupedExpression.originalColumnDisplayFormat) : jsonLookup(),
                            columnGrouping: jsonRelationship(),
                            columnRollup: groupedExpression.originalColumnRollup ? groupedExpression.originalColumnRollup : jsonRelationship()
                        });

                        reportEntity.addReportColumn(new spReportEntity.ReportColumn(reportColumn));
                    }
                }
            });
        }
    };

    spReportEntityQueryManager.updateAggregateColumnsByAggregateNodeId = function (reportEntity, aggregateReportNodeId, newAggregateReportNode) {

        //Get all related aggregate Columns
        _.forEach(reportEntity.getReportColumns(), function (reportColumn) {
            var reportColumnExpression = reportColumn.getExpression();
            if (reportColumnExpression && reportColumnExpression.getSourceNode() && reportColumnExpression.getSourceNode().id() === aggregateReportNodeId) {
                //update source Node to new aggregateNode
                if (sp.result(reportColumnExpression, 'getEntity.type.alias') !== 'core:structureViewExpression') {
                    reportColumn.getExpression().getEntity().sourceNode = newAggregateReportNode.getEntity();
                } else {
                    reportColumn.getExpression().getEntity().structureViewExpressionSourceNode = newAggregateReportNode.getEntity();
                }
                
            }
        });

        reportEntity.updateReportColumns();

    };
   
    
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // ReportColumn Manager Static Methods
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /** add column to query by ReportColumnNodeItem and enttity nodeid
  * @param {Object} reportEntity The the reportentity object
  * @param {Object} column The ReportColumnNodeItem
  * @param {Object} nodeEntity The entity node
  */
    spReportEntityQueryManager.addColumnToReport = function (reportEntity, column, nodeEntity, insertAfterColumn) {
        // Caution: neither reportEntity nor nodeEntity are actually entities!!
        var newReportColumn;

        if (insertAfterColumn > 0) {
            //update all reportcolumn's columnDisplayOrder which greater equal than it
            _.forEach(reportEntity.getReportColumns(), function (reportColumn) {
                var columnDisplayOrder = reportColumn.displayOrder();
                if (reportColumn && columnDisplayOrder >= insertAfterColumn) {
                    reportColumn.getEntity().columnDisplayOrder = columnDisplayOrder + 1;
                }
            });
        }
        spReportEntityQueryManager.setEntity(reportEntity);

        var disname = '';

        if (column.dname && column.dname.length > 0) {
            if (column.fname === 'Name') {
                disname = column.dname;
            } else {
                disname = column.dname + ' ' + column.fname;
            }
        }
        else {
            disname = column.fname;
        }
        
       

        var expressionType = 'fieldExpression';
        var fieldType = column.ftype;
        if (column.ftype === 'InlineRelationship' || column.ftype === 'UserInlineRelationship' || column.ftype === 'ChoiceRelationship' || column.ftype === 'Image') {
            expressionType = 'resourceExpression';
        } else if (column.dname && column.dname.length > 0 && column.fname === 'Name' && column.ftype === 'String') {
            //required, the root node name field should be fieldExpression
            if (reportEntity.getRootNode() && reportEntity.getRootNode().id() === nodeEntity.id()) {
                expressionType = 'fieldExpression';
            } else {
                expressionType = 'resourceExpression';
                fieldType = 'InlineRelationship';
            }
        }

        newReportColumn = spReportEntityQueryManager.createReportColumn(expressionType, nodeEntity, disname, column.fid, fieldType, false, column.ttid, insertAfterColumn);

        reportEntity.addReportColumn(newReportColumn);

        return newReportColumn;
    };

    /** add calculated column to query by script and fieldType
     * @param {Object} reportEntity The the reportentity object
     * @param {Object} nodeEntity The entity node
     * @param {string} script The calculate report script
     * @param {string} fieldType The fieldType
     */
    spReportEntityQueryManager.addCalculateColumnToReport = function (reportEntity, nodeEntity, script, fieldType, columnName, entityTypeId) {
        var displayOrder = 0;
        if (reportEntity && reportEntity.getReportColumns()) {
            var sortedReportColumns = _.sortBy(reportEntity.getReportColumns(), function (column) { return column.displayOrder(); });
            displayOrder = sortedReportColumns[sortedReportColumns.length - 1].displayOrder() + 1;
        } else {
            displayOrder = 0;
        }

        // get field result type
        var fieldresultType = spReportEntityQueryManager.createActivityArgument(false, fieldType, null, entityTypeId);
        var sourceNodeDataEntity = nodeEntity.getEntity ? nodeEntity.getEntity() : nodeEntity;
        var sourceNodeData = jsonLookup(sourceNodeDataEntity);

        // build script exression JSON
        var expression = spEntity.fromJSON({
            typeId: 'scriptExpression',
            reportScript: script,
            sourceNode: sourceNodeData,
            reportExpressionResultType: jsonLookup(fieldresultType)
        });

        // build report column JSON
        var reportColumn = spEntity.fromJSON({
            typeId: 'reportColumn',
            name: columnName,
            columnDisplayOrder: displayOrder,
            columnIsHidden: false,
            columnExpression: jsonLookup(expression),
            columnFormattingRule: jsonLookup(),
            columnDisplayFormat: jsonLookup(),
            columnGrouping: jsonRelationship(),
            columnRollup: jsonRelationship()
        });

        var calculatedColumn = new spReportEntity.ReportColumn(reportColumn);

        reportEntity.addReportColumn(calculatedColumn);


        //create a columnreference condition
        var conditionExpression = spEntity.fromJSON({
            typeId: 'columnReferenceExpression',
            expressionReferencesColumn: jsonLookup(reportColumn)
        });


        var conditionDisplayOrder = 0;
        if (reportEntity && reportEntity.getReportConditions()) {

            var sortedReportConditions = _.sortBy(reportEntity.getReportConditions(), function (condition) { return condition.displayOrder(); });
            if (sortedReportConditions.length >= 1) {
                conditionDisplayOrder = sortedReportConditions[sortedReportConditions.length - 1].displayOrder() + 1;
            }
        }
        else {
            conditionDisplayOrder = 0;
        }

        var conditionParameter = spReportEntityQueryManager.createReportConditionParameter(fieldType, null, entityTypeId);

        var reportCondition = spEntity.fromJSON({
            typeId: 'reportCondition',
            name: columnName,
            conditionDisplayOrder: conditionDisplayOrder,
            conditionIsHidden: false,
            conditionIsLocked: false,
            columnForCondition: jsonLookup(calculatedColumn.getEntity()),
            conditionExpression: jsonLookup(conditionExpression),
            operator: jsonLookup(),
            conditionParameter: jsonLookup(conditionParameter)
        });

        var calculatedCondition = new spReportEntity.ReportCondition(reportCondition);

        reportEntity.addReportCondition(calculatedCondition);

        return calculatedColumn;
    };

    /** update calculated column to query by new script and fieldType
    * @param {Object} reportEntity The the reportentity object
    * @param {long} columnId The columnId
    * @param {string} script The calculate report script
    * @param {string} fieldType The fieldType
    */
    spReportEntityQueryManager.updateCalculateColumnToReport = function (reportEntity, columnId, script, fieldType, columnName, entityTypeId) {
        var reportColumn = _.find(reportEntity.getReportColumns(), function (column) {
            return column.id() === columnId;
        });

        if (reportColumn) {
            var reportColumnExpression = reportColumn.getExpression();
            var fieldresultType = spReportEntityQueryManager.createActivityArgument(false, fieldType, null, entityTypeId);
            //update fieldresult type and reportscript
            reportColumn.getEntity().setName(columnName);
            reportColumnExpression.getEntity().setReportExpressionResultType(fieldresultType);
            reportColumnExpression.getEntity().setReportScript(script);
        }
    };

    /** add column to query by ReportColumnNodeItem and enttity nodeid
      * @param {Object} reportEntity The the reportentity object
      * @param {Object} column The ReportColumnNodeItem
      * @param {Object} nodeEntity The entity node
      */
    spReportEntityQueryManager.getExistingReportColumn = function (reportEntity, column, nodeEntity) {
        spReportEntityQueryManager.setEntity(reportEntity);

        var currentNodeEntity = nodeEntity;
        if (nodeEntity.qe) {
            currentNodeEntity = nodeEntity.qe;
        }


        var selectedColumns = _.find(reportEntity.getReportColumns(), function (reportColumn) {
            return spReportEntityQueryManager.areEqualReportExpression(currentNodeEntity.id(), column.fid, reportColumn.getExpression(), column);
        });

        if (selectedColumns && selectedColumns.length > 0) {
            return selectedColumns[0];
        } else if (selectedColumns) {
            return selectedColumns;
        }
        else {
            return null;
        }
    };

    /** add aggregate column by original column
    * @param {Object} reportEntity The the reportentity object
    * @param {Object} aggregateNode The aggregateNode
    * @param {Object} nodeEntity The entity node
    */
    spReportEntityQueryManager.addAggregateColumn = function (reportEntity, aggregateNode, summariseAction) {
        var newAggregateColumn;
        var expression;

        var originalColumn = _.find(reportEntity.getReportColumns(), function (selectColumn) {
            return selectColumn.id() === summariseAction.columnId;
        });
        if (originalColumn && originalColumn.getEntity()) {

            expression = originalColumn.getExpression();
            var expressionType = expression.getTypeAlias();
            var aggregateMethod;

            var displayOrder = 0;

            if (this.ReportEntity && this.ReportEntity.getReportColumns() && originalColumn.displayOrder) {
                displayOrder = originalColumn.displayOrder();
            } else {
                displayOrder = 0;
            }

            var originalColumnName = originalColumn.getName();
            if (expressionType === 'aggregateExpression' || expressionType === 'core:aggregateExpression') {
                originalColumnName = getOriginalColumnName(originalColumn);
            }

            //get original column display format
            var originalColumnDisplayFormat = null;

            if (originalColumn.getEntity().columnDisplayFormat && originalColumn.getEntity().columnDisplayFormat.formatAlignment) {                
                originalColumnDisplayFormat = getOriginalColumnDisplayFormat(originalColumn);
            }

            //get original column rollup
            var originalColumnRollup = null;
            var aggregateExpression = expression.getAggregatedExpression() ? expression.getAggregatedExpression() : expression;
            if (originalColumn.getEntity().columnRollup && originalColumn.getEntity().columnRollup.length > 0 && aggregateExpression.getEntity() && aggregateExpression.getEntity().reportExpressionResultType) {
                originalColumnRollup = getOriginalColumnRollups(originalColumn.getEntity().columnRollup, aggregateExpression.getEntity().reportExpressionResultType.name);
            }

            if (summariseAction.aggregateMethod === 'Show values') {

                //was aggregate column change back to normal column
                if (expressionType === 'aggregateExpression' || expressionType === 'core:aggregateExpression') {

                    //if the expression datastate is not create. this expression will be removed by aggregated Column remove. 
                    //clone the expression to set in aggregatedExpression
                    var cloneAggregatedExpression;
                    if (expression &&
                        expression.getAggregatedExpression() &&
                        expression.getAggregatedExpression().getEntity &&
                        expression.getAggregatedExpression().getEntity() &&
                        expression.getAggregatedExpression().getEntity().getDataState() !== spEntity.DataStateEnum.Create) {
                        cloneAggregatedExpression = spReportEntityQueryManager.cloneExpression(expression.getAggregatedExpression());
                    }

                   
                    var reportColumn = spEntity.fromJSON({
                        typeId: 'reportColumn',
                        name: originalColumnName,
                        columnDisplayOrder: displayOrder,
                        columnIsHidden: false,
                        columnExpression: cloneAggregatedExpression ? jsonLookup(cloneAggregatedExpression.getEntity()) : jsonLookup(expression.getAggregatedExpression().getEntity()),
                        columnFormattingRule: jsonLookup(),
                        columnDisplayFormat: originalColumnDisplayFormat ? jsonLookup(originalColumnDisplayFormat) : jsonLookup(),
                        columnGrouping: jsonRelationship(),
                        columnRollup: originalColumnRollup ? originalColumnRollup : jsonRelationship()
                    });

                    newAggregateColumn = new spReportEntity.ReportColumn(reportColumn);


                } else {
                    // was other column, don't change anthing
                }


            }
            else {

                var aggregateMethodAlias = getAggregateMethodAlias(summariseAction.aggregateMethod);

                //always create new aggregaet column because exist aggregate column will be removed              
                
                //create aggregate column
                var displayName = summariseAction.aggregateMethod + ': ' + originalColumnName;


                aggregateMethod = spEntity.fromJSON({
                    typeId: 'aggregateMethodEnum',
                    id: aggregateMethodAlias
                });

                var fieldresultType;

                if (aggregateMethodAlias === 'aggCount' ||
                    //aggregateMethodAlias === 'aggSum' ||
                    //aggregateMethodAlias === 'aggAverage' ||
                    aggregateMethodAlias === 'aggCountWithValues' ||
                    aggregateMethodAlias === 'aggCountUniqueItems') {


                    fieldresultType = spReportEntityQueryManager.createActivityArgument(false, 'Number');
                } else {
                    fieldresultType = expression.getEntity().getReportExpressionResultType();
                }




                //if the expression datastate is not create. this expression will be removed by original Column remove. 
                //clone the expression to set in aggregatedExpression
                var currentExpression;
                var aggregatedExpression;
                currentExpression = (expressionType === 'aggregateExpression' || expressionType === 'core:aggregateExpression') ? expression.getAggregatedExpression() : expression;


                if (expression && expression.getEntity && expression.getEntity() && expression.getEntity().getDataState() !== spEntity.DataStateEnum.Create) {
                    aggregatedExpression = spReportEntityQueryManager.cloneExpression(currentExpression);
                } else {
                    aggregatedExpression = currentExpression;
                }
                if (!aggregatedExpression) {
                    aggregatedExpression = expression;
                }

               
                var newAggregateExpression = spEntity.fromJSON({
                    typeId: 'aggregateExpression',
                    sourceNode: jsonLookup(aggregateNode.getEntity()),
                    aggregateMethod: jsonLookup(aggregateMethod),
                    reportExpressionResultType: jsonLookup(fieldresultType),
                    aggregatedExpression: jsonLookup(aggregatedExpression.getEntity())
                });

               

                var aggregateColumn = spEntity.fromJSON({
                    typeId: 'reportColumn',
                    name: displayName,
                    columnDisplayOrder: displayOrder,
                    columnIsHidden: false,
                    columnExpression: jsonLookup(newAggregateExpression),
                    columnFormattingRule: jsonLookup(),
                    columnDisplayFormat: originalColumnDisplayFormat ? jsonLookup(originalColumnDisplayFormat) : jsonLookup(),
                    columnGrouping: jsonRelationship(),
                    columnRollup: originalColumnRollup? originalColumnRollup : jsonRelationship()
                });

                newAggregateColumn = new spReportEntity.ReportColumn(aggregateColumn);

                
            }
        } else {
            //do nothing, check what caused the issue
        }

        if (newAggregateColumn) {            
            reportEntity.addReportColumn(newAggregateColumn);
        }
    };

    function getOriginalColumnName(column) {
        var originalName = column.getName();
        try {
            //the aggregated column name is  AggregateMethod : originalName  (e.g. Count: Number Field)
            //if user changed aggregated column name to any other format, then leave as column name
            originalName = originalName.split(":").length > 1 ? originalName.split(":")[1] : originalName;
        } catch (e) {

        }
        return originalName.trim();
    }

    function getOriginalColumnDisplayFormat(originalColumn) {
        var originalColumnAlignment = originalColumn.getEntity().columnDisplayFormat.formatAlignment;
        var enumAlignment = null;
        var alignmentName = originalColumnAlignment.name ? originalColumnAlignment.name : originalColumnAlignment.alias();

        switch (alignmentName) {
            case 'Left':
            case 'core:alignLeft':
                enumAlignment = spEntity.fromJSON({
                    typeId: 'alignEnum',
                    id: 'alignLeft',
                    name: originalColumnAlignment.name ? originalColumnAlignment.name : "Left"
                });
                break;
            case 'Right':
            case 'core:alignRight':
                enumAlignment = spEntity.fromJSON({
                    typeId: 'alignEnum',
                    id: 'alignRight',
                    name: originalColumnAlignment.name ? originalColumnAlignment.name : "Right"
                });
                break;
            case 'Centre':
            case 'core:alignCentre':
                enumAlignment = spEntity.fromJSON({
                    typeId: 'alignEnum',
                    id: 'alignCentre',
                    name: originalColumnAlignment.name ? originalColumnAlignment.name : "Centre"
                });
                break;
            default:
                enumAlignment = spEntity.fromJSON({
                    typeId: 'alignEnum',
                    id: 'alignAutomatic'
                });
                break;

        }        

        return spEntity.fromJSON({
            typeId: 'displayFormat',
            columnShowText: true,
            disableDefaultFormat: false,
            formatDecimalPlaces: 0,
            formatPrefix: jsonString(),
            formatSuffix: jsonString(),
            maxLineCount: 0,
            formatImageScale: jsonLookup(),
            dateColumnFormat: jsonLookup(),
            timeColumnFormat: jsonLookup(),
            dateTimeColumnFormat: jsonLookup(),
            formatAlignment: jsonLookup(enumAlignment),
            formatImageSize: jsonLookup(),
            entityListColumnFormat: jsonLookup()
        });
    }

    function getOriginalColumnRollups (reportRollups, resultType) {
        // init the rollups
        var columnRollups = [];

        // Add existing rollup methods to new report column
        _.forEach(reportRollups, function (reportRollup) {
            var rollupMethod = reportRollup.getRollupMethod() ? reportRollup.getRollupMethod().getAlias() : null;
            if (rollupMethod) {
                var newRollup = spReportEntityQueryManager.createColumnRollup(rollupMethod);
                //filter out the rollup method which not match the current result type
                if (resultType && isRollupMethodForResultType(resultType, rollupMethod)) {
                    columnRollups.push(newRollup);
                }
            }
        });

        return columnRollups;
    }

    function getAggregateMethodAlias(aggregateOptionName) {

        if (aggregateOptionName.toLowerCase() === 'count all')
            return 'core:aggCount';
        else if (aggregateOptionName.toLowerCase() === 'count')
            return 'core:aggCountWithValues';
        else if (aggregateOptionName.toLowerCase() === 'count unique')
            return 'core:aggCountUniqueItems';
        else
            return 'agg' + aggregateOptionName.replace(/ /g, '');

    }

    //check the existing rollupMethod still suit for current result type
    function isRollupMethodForResultType(resultType, rollupMethod) {
        var rollupMethodForResultType = false;

        var isNumeric = false,
            isDateTime = false,
            isChoiceType = false,
            isBoolean = false;

        switch (resultType) {
            case spEntity.DataType.Bool:
                isBoolean = true;
                break;
            case spEntity.DataType.Decimal:
            case spEntity.DataType.Currency:
            case spEntity.DataType.Int32:
                isNumeric = true;
                break;
            case spEntity.DataType.Date:
            case spEntity.DataType.Time:
            case spEntity.DataType.DateTime:
                isDateTime = true;
                break;
            case 'ChoiceRelationship':
                isChoiceType = true;
                break;
        }



        switch (rollupMethod) {
            case 'core:aggSum':
            case 'core:aggAverage':         
                rollupMethodForResultType = isNumeric;
                break;
            case 'core:aggMin':
            case 'core:aggMax':
                rollupMethodForResultType = isNumeric || isDateTime || isChoiceType;
                break;          
            case 'core:aggCountUniqueItems':
                rollupMethodForResultType = !isBoolean;
                break;        
            default:
                rollupMethodForResultType = true;
                break;
        }     

        return rollupMethodForResultType;
    }

    /** add aggregate column by field insert after column
    * @param {Object} reportEntity The the reportentity object
    * @param {Object} aggregateNode The aggregateNode
    * @param {Object} nodeEntity The entity node
    */
    spReportEntityQueryManager.addAggregateColumnAfterColumn = function (reportEntity, aggregateNode, field, summariseAction, currentNodeEntity, subNodeEntity, insertAfterColumn) {

        if (insertAfterColumn > 0) {
            //update all reportcolumn's columnDisplayOrder which greater equal than it
            _.forEach(reportEntity.getReportColumns(), function (reportColumn) {
                var columnDisplayOrder = reportColumn.displayOrder();
                if (reportColumn && columnDisplayOrder >= insertAfterColumn) {
                    reportColumn.getEntity().columnDisplayOrder = columnDisplayOrder + 1;
                }
            });
        }
        spReportEntityQueryManager.setEntity(reportEntity);

        var disname = field.fname;

        if (field.dname && field.fname === 'Name') {
            disname = field.dname;
        } else if (field.dname && field.fname !== 'Name') {
            disname = field.dname + ' ' + field.fname;
        }
       
        var displayName = summariseAction.aggregateMethod === 'Show values' ? disname : summariseAction.aggregateMethod + ': ' + disname;
        var expressionType = 'fieldExpression';
        var sourceNodeEntity, aggregateColumn;

        if (field.relid > 0) {
            sourceNodeEntity = subNodeEntity.getEntity();
        } else {
            sourceNodeEntity = currentNodeEntity.getEntity();
        }
        if (field.ftype === 'InlineRelationship' || field.ftype === 'UserInlineRelationship' || field.ftype === 'ChoiceRelationship' || field.ftype === 'Image') {
            expressionType = 'resourceExpression';
        }
        var aggregateMethodAlias = summariseAction.aggregateMethod === 'Show values' ? '' : getAggregateMethodAlias(summariseAction.aggregateMethod);
        var expression = spReportEntityQueryManager.createExpression(false, expressionType, sourceNodeEntity, field.fid, field.ftype, null, field.ttid);

        //if the aggregate method is show value, add normal fieldExpression report otherwise add an aggreagte expression field
        if (summariseAction.aggregateMethod === 'Show values') {
            aggregateColumn = spEntity.fromJSON({
                typeId: 'reportColumn',
                name: displayName,
                columnDisplayOrder: insertAfterColumn,
                columnIsHidden: false,
                columnExpression: jsonLookup(expression),
                columnFormattingRule: jsonLookup(),
                columnDisplayFormat: jsonLookup(),
                columnGrouping: jsonRelationship(),
                columnRollup: jsonRelationship()
            });
        } else {
            var aggregateMethod = spEntity.fromJSON({
                typeId: 'aggregateMethodEnum',
                id: aggregateMethodAlias
            });

            var fieldresultType;

            if (aggregateMethodAlias === 'aggCount' ||
                aggregateMethodAlias === 'aggCountWithValues' ||
                aggregateMethodAlias === 'aggCountUniqueItems') {


                fieldresultType = spReportEntityQueryManager.createActivityArgument(false, 'Number');
            } else {
                fieldresultType = expression.getReportExpressionResultType();
            }

            var newAggregateExpression = spEntity.fromJSON({
                typeId: 'aggregateExpression',
                sourceNode: jsonLookup(aggregateNode.getEntity()),
                aggregateMethod: jsonLookup(aggregateMethod),
                reportExpressionResultType: jsonLookup(fieldresultType),
                aggregatedExpression: jsonLookup(expression)
            });

            aggregateColumn = spEntity.fromJSON({
                typeId: 'reportColumn',
                name: displayName,
                columnDisplayOrder: insertAfterColumn,
                columnIsHidden: false,
                columnExpression: jsonLookup(newAggregateExpression),
                columnFormattingRule: jsonLookup(),
                columnDisplayFormat: jsonLookup(),
                columnGrouping: jsonRelationship(),
                columnRollup: jsonRelationship()
            });
        }



        var newAggregateColumn = new spReportEntity.ReportColumn(aggregateColumn);
        reportEntity.addReportColumn(newAggregateColumn);
    };
    /**
        * remove report column by source node id
        * @function
        * @name spReportEntityQueryManager.removeReportColumnBySourceNodeId
        */
    spReportEntityQueryManager.removeReportColumnBySourceNodeId = function (reportEntity, nodeId, reorder) {
        spReportEntityQueryManager.setEntity(reportEntity);

        _.forEach(reportEntity.getReportColumns(), function (reportColumn) {
            var reportColumnId = reportColumn.id();
            var reportColumnEntity = reportColumn.getEntity();
            var reportColumnExpression = reportColumn.getExpression();
            if (reportColumnExpression && reportColumnExpression.getSourceNode() &&  reportColumnExpression.getSourceNode().id() === nodeId) {

                //remove reportOrderBy by reportColumnId
                spReportEntityQueryManager.removeReportOrderByByReportColumnId(reportEntity, reportColumnId);
                spReportEntityQueryManager.removeReportConditionsByReportColumnId(reportEntity, reportColumnId);
                var columnDisplayOrder = reportColumn.displayOrder();
                reportEntity.removeReportColumn(reportColumn);
                if (!angular.isDefined(reorder) || reorder === true) {
                    spReportEntityQueryManager.reduceColumnDisplayOrder(reportEntity, columnDisplayOrder);
                }
            }
        }
       );
    };

    spReportEntityQueryManager.removeReportColumnByAggregateNodeId = function (reportEntity, aggregatedNodeId, nodeId, reorder) {
        spReportEntityQueryManager.setEntity(reportEntity);

        _.forEach(reportEntity.getReportColumns(), function (reportColumn) {
            var reportColumnId = reportColumn.id();
            var reportColumnEntity = reportColumn.getEntity();
            var reportColumnExpression = reportColumn.getExpression();
            if (reportColumnExpression && reportColumnExpression.getSourceNode() && reportColumnExpression.getSourceNode().id() === aggregatedNodeId &&
                reportColumnExpression.getAggregatedExpression() &&
                reportColumnExpression.getAggregatedExpression().getSourceNode() &&
                reportColumnExpression.getAggregatedExpression().getSourceNode().id() === nodeId) {

                //remove reportOrderBy by reportColumnId
                spReportEntityQueryManager.removeReportOrderByByReportColumnId(reportEntity, reportColumnId);
                spReportEntityQueryManager.removeReportConditionsByReportColumnId(reportEntity, reportColumnId);
                var columnDisplayOrder = reportColumn.displayOrder();
                reportEntity.removeReportColumn(reportColumn);
                if (!angular.isDefined(reorder) || reorder === true) {
                    spReportEntityQueryManager.reduceColumnDisplayOrder(reportEntity, columnDisplayOrder);
                }
            }
        }
       );
    };

    spReportEntityQueryManager.resourceNodeUnderAggregated = function (nodeEntity, nodeId) {
        if (nodeEntity === null)
            return false;

        var isMatched = false;
        if (nodeEntity.id() === nodeId)
            isMatched = true;
        else if (nodeEntity.getRelatedReportNodes && nodeEntity.getRelatedReportNodes().length > 0) {
            var existingReportColumn = _.find(nodeEntity.getRelatedReportNodes(), function (childNode) {
                return childNode.id() === nodeId || spReportEntityQueryManager.resourceNodeUnderAggregated(childNode, nodeId);
            });

            if (existingReportColumn)
                isMatched = true;           
        }

        return isMatched;
    };

    spReportEntityQueryManager.reduceColumnDisplayOrder = function (reportEntity, columnDisplayOrder) {
        spReportEntityQueryManager.setEntity(reportEntity);

        _.forEach(reportEntity.getReportColumns(), function (reportColumn) {
            var currentColumnDisplayOrder = reportColumn.displayOrder();
            if (reportColumn && currentColumnDisplayOrder > columnDisplayOrder) {
                reportColumn.getEntity().columnDisplayOrder = currentColumnDisplayOrder - 1;
            }
        });
    };

    spReportEntityQueryManager.increaseColumnDisplayOrder = function (reportEntity, columnId, columnDisplayOrder) {
        if (columnDisplayOrder > -1) {
            var allOtherColumns = _.filter(reportEntity.getReportColumns(), function (column) {
                return column.id() !== columnId && column.displayOrder() >= columnDisplayOrder;
            });

            _.forEach(allOtherColumns, function (reportColumn) {
                reportColumn.getEntity().columnDisplayOrder++;
            });

            reportEntity.updateReportColumns();
        }
    };

    /**
           * remove report column by report column id
           * @function
           * @name spReportEntityQueryManager.removeReportColumnByColumnId
           */
    spReportEntityQueryManager.removeReportColumnByColumnId = function (reportEntity, reportColumnId) {
        spReportEntityQueryManager.setEntity(reportEntity);

        _.forEach(reportEntity.getReportColumns(), function (reportColumn) {

            var reportColumnEntity = reportColumn.getEntity();
            var reportColumnExpression = reportColumn.getExpression();
            if (reportColumn.id() === reportColumnId) {

                //remove reportOrderBy by reportColumnId
                spReportEntityQueryManager.removeReportOrderByByReportColumnId(reportEntity, reportColumnId);
                spReportEntityQueryManager.removeReportConditionsByReportColumnId(reportEntity, reportColumnId);
                var columnDisplayOrder = reportColumn.displayOrder();
                reportEntity.removeReportColumn(reportColumn);
                spReportEntityQueryManager.reduceColumnDisplayOrder(reportEntity, columnDisplayOrder);
            }
        }
      );
    };


    /**
      * update report column's condtionformatting by source column id
      * @function
      * @name spReportEntity.Query#updateConditionFormatting
      */
    spReportEntityQueryManager.updateConditionFormatting = function (reportEntity, columnId, conditionFormat) {
        spReportEntityQueryManager.setEntity(reportEntity);

        var existingReportColumn = _.find(reportEntity.getReportColumns(), function (reportColumn) {
            return reportColumn.id().toString() === columnId.toString();
        });

        if (existingReportColumn) {
            existingReportColumn.setColumnFormattingRule(conditionFormat);
        }
    };

    /**
    * update report column's condtionformatting by source column id
    * @function
    * @name spReportEntity.Query#updateColumnDisplayFormat
    */
    spReportEntityQueryManager.updateColumnDisplayFormat = function (reportEntity, columnId, columnDisplayFormat) {
        spReportEntityQueryManager.setEntity(reportEntity);

        var existingReportColumn = _.find(reportEntity.getReportColumns(), function (reportColumn) {
            return reportColumn.id().toString() === columnId.toString();
        });

        if (existingReportColumn) {
            existingReportColumn.setColumnDisplayFormat(columnDisplayFormat);
        }
    };



    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // ReportCondition Manager Static Methods
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /** add condition to query by ReportColumnNodeItem and enttity nodeid
       * @param {Object} reportEntity The the reportentity object
       * @param {Object} column The ReportColumnNodeItem
       * @param {Object} nodeEntity The entity node
       */
    spReportEntityQueryManager.addCondition = function (reportEntity, column, nodeEntity, insertAfterCondition) {

        var newReportCondition;

        if (insertAfterCondition > 0) {
            //update all reportCondition's conditionDisplayOrder which greater equal than it
            _.each(reportEntity.getReportConditions(), function (reportCondition) {
                var conditionDisplayOrder = reportCondition.displayOrder();
                if (reportCondition && conditionDisplayOrder >= insertAfterCondition) {
                    reportCondition.getEntity().conditionDisplayOrder = conditionDisplayOrder + 1;
                }
            });
        }
        spReportEntityQueryManager.setEntity(reportEntity);



        var disname = '';

        if (column.dname && column.dname.length > 0) {
            if (column.fname === 'Name') {
                disname = column.dname;
            } else {
                disname = column.dname + ' ' + column.fname;
            }
        }
        else {
            disname = column.fname;
        }

        var expressionType = 'fieldExpression';
        var fieldType = column.ftype;
        if (column.ftype === 'InlineRelationship' || column.ftype === 'UserInlineRelationship' || column.ftype === 'ChoiceRelationship' || column.ftype === 'Image') {
            expressionType = 'resourceExpression';
        } else if (column.dname && column.dname.length > 0 && column.fname === 'Name' && column.ftype === 'String') {
            //required, the root node name field should be fieldExpression
            if (reportEntity.getRootNode() && reportEntity.getRootNode().id() === nodeEntity.id()) {
                expressionType = 'fieldExpression';
            } else {                
                expressionType = 'resourceExpression';
                fieldType = 'InlineRelationship';
            }            
        }

        //find existing reportColumn from report
        var selectedColumn = spReportEntityQueryManager.getExistingReportColumn(reportEntity, column, nodeEntity);
        var structureViewId;

        if (selectedColumn) {
            var existingExpression = selectedColumn.getEntity().getColumnExpression();
            if (existingExpression &&
                existingExpression.type &&
                existingExpression.type.alias() === 'core:structureViewExpression') {
                expressionType = 'structureViewExpression';
                if (existingExpression.structureViewExpressionStructureView) {
                    structureViewId = existingExpression.structureViewExpressionStructureView.id();
                }                
            }
        }
                   
        newReportCondition = spReportEntityQueryManager.createReportCondition(expressionType, nodeEntity, disname, column.fid, fieldType, column.isReverse === true ? column.ftid : column.ttid, insertAfterCondition, structureViewId);
        
        if (selectedColumn) {
            newReportCondition.getEntity().columnForCondition = selectedColumn.getEntity();
        }

        reportEntity.addReportCondition(newReportCondition);

        return newReportCondition;
    };


    spReportEntityQueryManager.increaseConditionDisplayOrder = function (reportEntity, conditionId, conditionDisplayOrder) {
        if (conditionDisplayOrder > -1) {
            var allOtherConditions = _.filter(reportEntity.getReportConditions(), function (condition) {
                return condition.id() !== conditionId && condition.displayOrder() >= conditionDisplayOrder;
            });

            _.forEach(allOtherConditions, function (reportCondition) {
                reportCondition.getEntity().conditionDisplayOrder++;
            });

            reportEntity.updateReportConditions();
        }
    };

    /**
     * update the condition argument value
      * @param {Object} reportEntity The the reportentity object
      * @param {Object} condtion id The ReportColumnCondtion Id
      * @param {Object} operator The condtion operator
      * @param {Object} type The condtion type
      * @param {Object} type The condtion argument type
      * @param {value} type The condtion value
     */
    spReportEntityQueryManager.updateCondition = function (reportEntity, conditionId, operator, type, argtype, value) {
        spReportEntityQueryManager.setEntity(reportEntity);
        var sourceNodeTypeId = null;
        var existingReportCondition = _.find(reportEntity.getReportConditions(), function (reportCondition) {
            return reportCondition.id().toString() === conditionId.toString();
        });

        if (existingReportCondition) {
            //reset the condition
            if (operator === 'Unspecified') {
                existingReportCondition.reset();
            }
            else {
                //update condition
                var operatorEntity = spReportEntityQueryManager.createOperator(operator);
                var conditionParameter;
                if (existingReportCondition.getConditionParameter() && existingReportCondition.getConditionParameter().getParamTypeAndDefault()) {
                    conditionParameter = existingReportCondition.getConditionParameter();
                    //id field type is same as condtion argument type, only update the value
                    if (type === argtype) {
                        if (type === 'ChoiceRelationship' || type === 'InlineRelationship' || type === 'UserInlineRelationship' || type === 'Image' || type === 'StructureLevels') {
                            //recreate the argument because exist condtion argument type may not contain confirmsToType, need retrive from current condtion source node type

                            var confirmsToTypeValue;

                            if (conditionParameter.getParamTypeAndDefault().getConformsToType) {
                                confirmsToTypeValue = jsonLookup(conditionParameter.getParamTypeAndDefault().getConformsToType());
                            } else {
                                sourceNodeTypeId = getEntityTypeId(existingReportCondition.getExpression());
                                if (sourceNodeTypeId > 0) {
                                    confirmsToTypeValue = jsonLookup(sourceNodeTypeId);
                                } else {
                                    confirmsToTypeValue = jsonLookup();
                                }
                            }


                            var activityArgument = spEntity.fromJSON({
                                typeId: 'resourceListArgument',
                                name: type,
                                resourceListParameterValues: jsonRelationship(spReportEntityQueryManager.buildresourceListValues(value)),
                                conformsToType: confirmsToTypeValue
                            });

                            conditionParameter = spEntity.fromJSON({
                                typeId: 'parameter',
                                paramTypeAndDefault: activityArgument ? jsonLookup(activityArgument) : jsonLookup()
                            });
                        } else {
                            spReportEntityQueryManager.updateActivityArgument(conditionParameter.getParamTypeAndDefault(), type, value);
                        }
                    } else {
                        if (argtype === 'ChoiceRelationship' || argtype === 'InlineRelationship' || argtype === 'UserInlineRelationship' || argtype === 'Image' || argtype === 'StructureLevels') {
                            sourceNodeTypeId = getEntityTypeId(existingReportCondition.getExpression());
                        }

                        conditionParameter = spReportEntityQueryManager.createReportConditionParameter(argtype, value, sourceNodeTypeId);
                    }

                } else {
                    if (argtype === 'ChoiceRelationship' || argtype === 'InlineRelationship' || argtype === 'UserInlineRelationship' || argtype === 'Image' || argtype === 'StructureLevels') {
                        sourceNodeTypeId = getEntityTypeId(existingReportCondition.getExpression());
                    }
                    conditionParameter = spReportEntityQueryManager.createReportConditionParameter(argtype, value, sourceNodeTypeId);
                }
                existingReportCondition.apply(operatorEntity, conditionParameter);
            }
        }
    };

    function getEntityTypeId(expression) {
        var conditionExpression;
        if (expression.getTypeAlias() === 'core:aggregateExpression' || expression.getTypeAlias() === 'aggregateExpression') {
            conditionExpression = expression.getAggregatedExpression();
        } else {
            conditionExpression = expression;
        }

        if (conditionExpression) {
            try {
                if (sp.result(conditionExpression, 'getTypeAlias') === 'core:structureViewExpression') {
                    return sp.result(conditionExpression, 'getEntity.structureViewExpressionSourceNode.resourceReportNodeType.id') || 0;
                } else {
                    return conditionExpression.getSourceNode().getResourceReportNodeType().id();
                }                
            } catch (e) {
                return 0;
            }
        } else {
            return 0;
        }
    }


    /**
      * reset the report condition by reset the condtion operator to null and value of paramTypeAndDefault to null     
      */
    spReportEntityQueryManager.resetConditionById = function (reportEntity, conditionId) {
        spReportEntityQueryManager.setEntity(reportEntity);

        var existingReportCondition = _.find(reportEntity.getReportConditions(), function (reportCondition) {
            return reportCondition.id().toString() === conditionId.toString();
        });

        if (existingReportCondition) {
            
           
            
            var sourceNodeTypeId = null;
            var conditionParameter = null;
            if (existingReportCondition.getConditionParameter()) {                
                conditionParameter = existingReportCondition.getConditionParameter();
                if (existingReportCondition.getConditionParameter().getParamTypeAndDefault()) {
                    //id field type is same as condtion argument type, only update the value
                    spReportEntityQueryManager.resetActivityArgument(conditionParameter.getParamTypeAndDefault());
                    
                }
                existingReportCondition.apply(null, conditionParameter);
            } 
            



            //this._reportConditionEntity.setOperator(null);
            //this._reportConditionEntity.setConditionParameter(null);
            //existingReportCondition.reset();
        }
    };

    /** add condition to query by ReportColumnNodeItem and enttity nodeid
       * @param {Object} reportEntity The the reportentity object
       * @param {Object} column The ReportColumnNodeItem
       * @param {Object} nodeEntity The entity node
       */
    spReportEntityQueryManager.getExistingReportCondition = function (reportEntity, column, nodeEntity) {
        spReportEntityQueryManager.setEntity(reportEntity);

        var selectedConditions = _.find(reportEntity.getReportConditions(), function (reportCondition) {
            return spReportEntityQueryManager.areEqualReportExpression(nodeEntity.id(), column.fid, reportCondition.getExpression(), column);
        });

        if (selectedConditions && selectedConditions.length > 0) {
            return selectedConditions[0];
        } else if (selectedConditions) {
            return selectedConditions;
        }
        else {
            return null;
        }
    };

    spReportEntityQueryManager.updateReportCondition = function (reportEntity, conditionId, condition) {
        _.each(reportEntity.getReportConditions(), function (reportCondtion) {
            //only reset all unHidden unLocked report condition
            if (reportCondtion && reportCondtion.id() === conditionId) {
                reportCondtion.getEntity().setOperator(jsonLookup(condition.operator));
                reportCondtion.getEntity().setConditionParameter(jsonLookup(null));

            }
        });
    };

    /**
          * remove report condition by source node id
          * @function
          * @name spReportEntity.Query#removeReportConditionBySourceNodeId
          */
    spReportEntityQueryManager.removeReportConditionBySourceNodeId = function (reportEntity, nodeId, reorder) {
        spReportEntityQueryManager.setEntity(reportEntity);

        _.each(reportEntity.getReportConditions(), function (reportCondition) {

            var reportConditionExpression = reportCondition.getExpression();
            if (reportConditionExpression && reportConditionExpression.getSourceNode() && reportConditionExpression.getSourceNode().id() === nodeId) {
                var conditionDisplayOrder = reportCondition.displayOrder();
                reportEntity.removeReportCondition(reportCondition);
                if (!angular.isDefined(reorder) || reorder === true) {
                    spReportEntityQueryManager.reduceConditionDisplayOrder(reportEntity, conditionDisplayOrder);
                }
            }
        }
       );
    };


    spReportEntityQueryManager.reduceConditionDisplayOrder = function (reportEntity, conditionDisplayOrder) {
        spReportEntityQueryManager.setEntity(reportEntity);

        _.forEach(reportEntity.getReportConditions(), function (reportCondition) {
            var currentConditionDisplayOrder = reportCondition.displayOrder();
            if (reportCondition && currentConditionDisplayOrder > conditionDisplayOrder) {
                reportCondition.getEntity().conditionDisplayOrder = currentConditionDisplayOrder - 1;
            }
        });
    };

    /**
          * remove report condition by source node id
          * @function
          * @name spReportEntity.Query#removeReportConditionByConditionId
          */
    spReportEntityQueryManager.removeReportConditionByConditionId = function (reportEntity, conditionId) {
        spReportEntityQueryManager.setEntity(reportEntity);
        _.each(reportEntity.getReportConditions(), function (reportCondition) {

            if (reportCondition.id() === conditionId) {
                var conditionDisplayOrder = reportCondition.displayOrder();
                reportEntity.removeReportCondition(reportCondition);
                spReportEntityQueryManager.reduceConditionDisplayOrder(reportEntity, conditionDisplayOrder);
            }
        }
       );
    };

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Report Orderby Manager Static Methods
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    spReportEntityQueryManager.getUnHideColumnByIndex = function (reportEntity, index) {
        spReportEntityQueryManager.setEntity(reportEntity);
        var selectColumn = null;

        var unHiddenColumns = _.filter(reportEntity.getReportColumns(), function (column) {
            return column.isHidden() === false;
        });
        unHiddenColumns = _.sortBy(unHiddenColumns, function (column) { return column.displayOrder(); });
        try {
            selectColumn = unHiddenColumns[index];
        } catch (e) {

        }

        return selectColumn;

    };


    spReportEntityQueryManager.addOrderByToReport = function (reportEntity, columnId, isReverseOrder, orderPriority) {
        spReportEntityQueryManager.setEntity(reportEntity);
        var existingOrderBy = _.find(reportEntity.getReportOrderBys(), function (orderBy) {
            var orderByExpression = orderBy.getExpression();
            return orderByExpression && orderByExpression.getReferencesColumn() && orderByExpression.getReferencesColumn().id() === columnId;
        });

        if (existingOrderBy) {
            existingOrderBy.getEntity().reverseOrder = isReverseOrder;
            if (angular.isDefined(orderPriority)) {
                existingOrderBy.getEntity().orderPriority = orderPriority;
            }
            reportEntity.updateReportOrderBys();
        }
        else {
            if (!angular.isDefined(orderPriority)) {
                if (reportEntity && reportEntity.getReportOrderBys() && reportEntity.getReportOrderBys().length > 0) {
                    var sortedReportOrders = _.sortBy(reportEntity.getReportOrderBys(), function (orderby) { return orderby.getOrderPriority(); });
                    orderPriority = sortedReportOrders[sortedReportOrders.length - 1].getOrderPriority() + 1;
                } else {
                    orderPriority = 0;
                }
            }

            var reportOrderBy = spReportEntityQueryManager.createReportOrderBy(columnId, isReverseOrder, orderPriority);
            reportEntity.addReportOrderBy(reportOrderBy);
        }
    };



    /**
         * remove report orderby by orderby id
         * @function
         * @name spReportEntity.Query#removeReportOrderByById
         */
    spReportEntityQueryManager.removeReportOrderByById = function (reportEntity, reportOrderById) {
        spReportEntityQueryManager.setEntity(reportEntity);
        _.each(reportEntity.getReportOrderBys(), function (reportOrderBy) {
            if (reportOrderBy.id() === reportOrderById) {
                reportEntity.removeReportOrderBy(reportOrderBy);
            }
        }
       );
    };

    /**
        * remove report orderby by source column id
        * @function
        * @name spReportEntity.Query#removeReportOrderByByReportColumnId
        */
    spReportEntityQueryManager.removeReportOrderByByReportColumnId = function (reportEntity, reportColumnId) {
        spReportEntityQueryManager.setEntity(reportEntity);
        _.forEach(reportEntity.getReportOrderBys(), function (reportOrderBy) {
            var reportOrderByExpression = reportOrderBy.getExpression();
            if (reportOrderByExpression && reportOrderByExpression.getReferencesColumn && reportOrderByExpression.getReferencesColumn() && reportOrderByExpression.getReferencesColumn().id() === reportColumnId) {
                reportEntity.removeReportOrderBy(reportOrderBy);
            }
        }
       );        
    };


    /**
        * remove report conditions by source column id
        * @function
        * @name spReportEntity.Query#removeReportConditionsByReportColumnId
        */
    spReportEntityQueryManager.removeReportConditionsByReportColumnId = function (reportEntity, reportColumnId) {
        spReportEntityQueryManager.setEntity(reportEntity);        
        _.forEach(reportEntity.getReportConditions(), function (reportCondition) {
            var reportConditionExpression = reportCondition.getExpression();
            if (reportConditionExpression && reportConditionExpression.getReferencesColumn && reportConditionExpression.getReferencesColumn() && reportConditionExpression.getReferencesColumn().id() === reportColumnId) {
                reportEntity.removeReportCondition(reportCondition);
            }
        }
       );
    };


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Public Static Methods for Report Entity
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /** Compare the equal report expression
   *
   * @param {Object} selectedEntityId The selected entity node id
   * @returns {object} The the root reportNode of reportEntity.
   *
   * @function
   * @name  spReportEntityQueryManager.createRootEntity
  */
    spReportEntityQueryManager.areEqualReportExpression = function (nodeId, fieldId, reportExpression, column) {
        if (reportExpression) {
            var areEqual = false;
            var structureViewSourceNode;
            switch (reportExpression.getTypeAlias())
            {
                case 'core:idExpression':
                    areEqual = false; //
                    break;
                case 'core.columnReferenceExpression':
                    areEqual = false; //
                    break;
                case 'core.scriptExpression':
                    areEqual = false; //
                    break;
                case 'core:resourceExpression':
                    areEqual = reportExpression.getSourceNode().id() === nodeId && (!reportExpression.getField() || reportExpression.getField().id() === fieldId);
                    break;
                case 'core:fieldExpression':
                    areEqual = reportExpression.getSourceNode().id() === nodeId && reportExpression.getField().id() === fieldId;
                    break;
                case 'core:structureViewExpression':
                    structureViewSourceNode = reportExpression.getEntity().structureViewExpressionSourceNode;
                    areEqual = (structureViewSourceNode &&
                        structureViewSourceNode.id() === nodeId &&
                        fieldId === spReportEntityQueryManager.NameFieldId &&
                        column &&
                        (column.ftype === 'String' || column.ftype === 'InlineRelationship'));
                    break;
                default:
                    areEqual = false;
                    break;
            }

            return areEqual;

            //return reportExpression.getSourceNode().id() === nodeId &&
            //    (!reportExpression.getField() || reportExpression.getField().id() === fieldId);
        }
        else {
            return false;
        }

    };


    /** Create RootNode of ReportEntity
    *
    * @param {Object} selectedEntityId The selected entity node id
    * @returns {object} The the root reportNode of reportEntity.
    *
    * @function
    * @name  spReportEntityQueryManager.createRootEntity
   */
    spReportEntityQueryManager.createRootEntity = function (entityId) {
        var rootEntity = spEntity.fromJSON({
            typeId: 'resourceReportNode',
            exactType: false,
            targetMustExist: false,
            resourceReportNodeType: jsonLookup(entityId)
        });
        return rootEntity;
    };

    /** create a relatedentity for query
    *
    * @param {long} nodeTypeId The node typeid
    * @param {long} relationshipId The relationship Id
    * @param {string} reldir The relationship direction
    * @returns {object} The the related reportEntity.
    *
    * @function
    * @name  spReportEntityQueryManager.createRelatedEntity
   */
    spReportEntityQueryManager.createRelatedEntity = function (nodeTypeId, relationshipId, reldir) {

        var isReverse = reldir === 'Reverse' ? true : false;

        var relatedEntity = spEntity.fromJSON({
            typeId: 'relationshipReportNode',
            exactType: false,
            targetMustExist: false,
            targetNeedNotExist: false,
            followInReverse: isReverse,
            followRecursive: false,
            includeSelfInRecursive: false,
            constrainParent: false,
            checkExistenceOnly: false,
            followRelationship: jsonLookup(relationshipId),
            resourceReportNodeType: jsonLookup(nodeTypeId),
            relatedReportNodes: jsonRelationship()
        });

        return new spReportEntity.ReportNode(relatedEntity);
    };



    spReportEntityQueryManager.createDerivedTypeEntity = function (entityId) {
        var derivedTypeEntity = spEntity.fromJSON({
            typeId: 'derivedTypeReportNode',
            exactType: false,
            targetMustExist: false,
            resourceReportNodeType: jsonLookup(entityId),
            relatedReportNodes: jsonRelationship()
        });
        return new spReportEntity.ReportNode(derivedTypeEntity);
    };

    spReportEntityQueryManager.createCustomJoinReportNode = function (nodeTypeId, predicateScript) {
        var defaultJoinScript = 'true'; // cross join
        var customJoinReportNode = spEntity.fromJSON({
            typeId: 'customJoinReportNode',
            exactType: false,
            targetMustExist: false,
            targetNeedNotExist: false,
            joinPredicateCalculation: jsonString(predicateScript || defaultJoinScript),
            resourceReportNodeType: jsonLookup(nodeTypeId),
            relatedReportNodes: jsonRelationship()
        });
        return new spReportEntity.ReportNode(customJoinReportNode);
    };

    spReportEntityQueryManager.createAggreateEntity = function (reportNode) {

        clearParentAggregatedNode(reportNode);

        var aggregateNode = spEntity.fromJSON({
            typeId: 'aggregateReportNode',
            groupedNode: jsonLookup(reportNode),
            groupedBy: jsonRelationship(),
            relatedReportNodes: jsonRelationship()
        });

        return new spReportEntity.ReportNode(aggregateNode);
    };

    /** create initialize Report Columns (id column and name column)
    *
    * @param {Object} rootEntity The selected entity node id
    * @param {long} nameFieldId to create name column
    * @returns [{object}] The report columns array which contain id column and name column
    *
    * @function
    * @name  spReportEntityQueryManager.createInitializeReportColumns
   */
    spReportEntityQueryManager.createInitializeReportColumns = function (rootEntity, nameFieldId, selectedEntityName) {
        var idField = spReportEntityQueryManager.createReportColumn('idExpression', rootEntity, '_id', '', null, true, null, 0);

        var rootEntityName = selectedEntityName ?selectedEntityName : 'Name';

        var nameField = spReportEntityQueryManager.createReportColumn('fieldExpression', rootEntity, rootEntityName, nameFieldId, 'String', false, null, 1);

        return [idField.getEntity(), nameField.getEntity()];
    };

    spReportEntityQueryManager.createInitializeReportCondition = function (rootEntity, nameFieldId, selectedEntityName) {
        var rootEntityName = selectedEntityName ? selectedEntityName : 'Name';

        var nameCondition = spReportEntityQueryManager.createReportCondition('fieldExpression', rootEntity, rootEntityName, nameFieldId, 'String', 0, 0);
        return [nameCondition.getEntity()];
    };


    /** create Report Column 
   * @param {string} report column expression type  e.g. idExpression, fieldExpression
   * @param {Object} rootEntity entity node
   * @param {long} fieldId of the report column
   * @param {bool} is Hidden 
   * @param {int}  the display order of report column
   * @returns [{object}] The report column
   *
   * @function
   * @name  spReportEntityQueryManager.createReportColumn
  */
    spReportEntityQueryManager.createReportColumn = function (type, entity, fieldName, fieldId, fieldType, isHidden, typeEntityId, displayOrder) {

        if (displayOrder === null || displayOrder === -1) {
            if (this.ReportEntity && this.ReportEntity.getReportColumns()) {
                var sortedReportColumns = _.sortBy(this.ReportEntity.getReportColumns(), function (column) { return column.displayOrder(); });
                displayOrder = sortedReportColumns[sortedReportColumns.length - 1].displayOrder() + 1;
            } else {
                displayOrder = 0;
            }
        }

        var expression = spReportEntityQueryManager.createExpression(false, type, entity, fieldId, fieldType, null, typeEntityId);

        var reportColumn = spEntity.fromJSON({
            typeId: 'reportColumn',
            name: fieldName,
            columnDisplayOrder: displayOrder,
            columnIsHidden: isHidden ? isHidden : false,
            columnExpression: jsonLookup(expression),
            columnFormattingRule: jsonLookup(),
            columnDisplayFormat: jsonLookup(),
            columnGrouping: jsonRelationship(),
            columnRollup: jsonRelationship()
        });

        return new spReportEntity.ReportColumn(reportColumn);

    };

    /** create Report Condition 
       * @param {string} report column expression type  e.g. idExpression, fieldExpression
       * @param {Object} rootEntity entity node
       * @param {long} fieldId of the report condition   
       * @param {int}  the display order of report condition
       * @returns [{object}] The report condition
       *
       * @function
       * @name  spReportEntityQueryManager.createReportCondition
      */
    spReportEntityQueryManager.createReportCondition = function (type, entity, fieldName, fieldId, fieldType, typeEntityId, displayOrder, structureViewId) {

        if (displayOrder === null || displayOrder < 0) {
            if (this.ReportEntity && this.ReportEntity.getReportConditions()) {

                var sortedReportConditions = _.sortBy(this.ReportEntity.getReportConditions(), function (condition) { return condition.displayOrder(); });
                if (sortedReportConditions.length >= 1) {
                    displayOrder = sortedReportConditions[sortedReportConditions.length - 1].displayOrder() + 1;
                }
            } else {
                displayOrder = 0;
            }
        }

        var expression = spReportEntityQueryManager.createExpression(true, type, entity, fieldId, fieldType, null, typeEntityId, structureViewId);
        var conditionParameter = spReportEntityQueryManager.createReportConditionParameter(fieldType, null, typeEntityId);
        var reportCondition = spEntity.fromJSON({
            typeId: 'reportCondition',
            name: fieldName,
            conditionDisplayOrder: displayOrder,
            conditionIsHidden: false,
            conditionIsLocked: false,
            conditionExpression: jsonLookup(expression),
            operator: jsonLookup(),
            conditionParameter: jsonLookup(conditionParameter)
        });

        return new spReportEntity.ReportCondition(reportCondition);
    };

    /** create Report Orderby 
      * @param {Object} column sortInfo obect
      * @returns [{long}] The report column id
      *
      * @function
      * @name  spReportEntityQueryManager.createReportOrderBy
     */
    spReportEntityQueryManager.createReportOrderBy = function (columnId, isReverseOrder, orderPriority) {

        var orderByExpression = spReportEntityQueryManager.createExpression(false, 'columnReferenceExpression', null, null, null, columnId);

        var reportOrderBy = spEntity.fromJSON({
            typeId: 'reportOrderBy',
            reverseOrder: isReverseOrder,
            orderPriority: orderPriority,
            orderByExpression: jsonLookup(orderByExpression)
        });


        return new spReportEntity.ReportOrderBy(reportOrderBy);
    };



    /** create Report Expression 
      * @param {string} report column expression type  e.g. idExpression, fieldExpression
      * @param {Object} rootEntity entity node
      * @param {long} fieldId of the report column      
      * @returns [{object}] The report column
      *
      * @function
      * @name  spReportEntityQueryManager.createExpression
     */
    spReportEntityQueryManager.createExpression = function (isConditionArgumentType, type, entity, fieldId, fieldType, reportColumnId, typeEntityId, structureViewId) {

        var expression = null,
            reportColumn;

        var sourceNodeDataEntity = null;

        if (entity) {
            sourceNodeDataEntity = entity.getEntity ? entity.getEntity() : entity;
        }
        var sourceNodeData = jsonLookup(sourceNodeDataEntity);

        switch (type) {
            case 'idExpression':
                expression = spEntity.fromJSON({
                    typeId: type,
                    sourceNode: sourceNodeData
                });
                break;
            case 'fieldExpression':
                var fieldresultType = spReportEntityQueryManager.createActivityArgument(isConditionArgumentType, fieldType);

                expression = spEntity.fromJSON({
                    typeId: type,
                    sourceNode: sourceNodeData,
                    fieldExpressionField: jsonLookup(fieldId),
                    reportExpressionResultType: jsonLookup(fieldresultType)
                });
                break;
            case 'resourceExpression':
                var resultType = spReportEntityQueryManager.createActivityArgument(isConditionArgumentType, fieldType, null, typeEntityId);

                expression = spEntity.fromJSON({
                    typeId: type,
                    sourceNode: sourceNodeData,
                    reportExpressionResultType: jsonLookup(resultType)
                });
                break;
            case 'columnReferenceExpression':
                expression = spEntity.fromJSON({
                    typeId: type,
                    expressionReferencesColumn: jsonLookup()
                });

                reportColumn = _.find(this.ReportEntity.getReportColumns(), function (column) {
                    return column.id().toString() === reportColumnId.toString();
                });

                if (reportColumn) {
                    expression.expressionReferencesColumn = reportColumn.getEntity();
                }                
                break;
            case 'structureViewExpression':
                var svResultType = spReportEntityQueryManager.createActivityArgument(isConditionArgumentType, fieldType, null, typeEntityId);

                expression = spEntity.fromJSON({
                    typeId: type,
                    structureViewExpressionSourceNode: sourceNodeData,
                    structureViewExpressionStructureView: jsonLookup(structureViewId),
                    reportExpressionResultType: jsonLookup(svResultType)
                });
                break;
        }


        return expression;
    };

    spReportEntityQueryManager.cloneExpression = function(expression) {
        var cloneExpression;
        if (expression && expression.getEntity && expression.getEntity() && expression.getEntity().getDataState() !== spEntity.DataStateEnum.Create) {
            var expressionType = sp.result(expression, 'getEntity.type.alias');
            if (expressionType !== 'core:structureViewExpression') {
                //remove fieldExpressionField relationship from resourceExpression
                if (expressionType !== 'core:resourceExpression') {
                    cloneExpression = new spReportEntity.Expression(spEntity.fromJSON({
                        typeId: expression.getTypeAlias(),
                        sourceNode: expression.getEntity().sourceNode,
                        fieldExpressionField: expression.getEntity().fieldExpressionField ? expression.getEntity().fieldExpressionField : jsonLookup(),
                        reportExpressionResultType: expression.getEntity().reportExpressionResultType
                    }));
                } else {
                    cloneExpression = new spReportEntity.Expression(spEntity.fromJSON({
                        typeId: expression.getTypeAlias(),
                        sourceNode: expression.getEntity().sourceNode,
                        reportExpressionResultType: expression.getEntity().reportExpressionResultType
                    }));
                }
                
            } else {
                cloneExpression = new spReportEntity.Expression(spEntity.fromJSON({
                    typeId: expression.getTypeAlias(),
                    structureViewExpressionSourceNode: jsonLookup(expression.getEntity().structureViewExpressionSourceNode),
                    structureViewExpressionStructureView: jsonLookup(expression.getEntity().structureViewExpressionStructureView),
                    reportExpressionResultType: jsonLookup(expression.getEntity().reportExpressionResultType)
                }));
            }            
        }

        return cloneExpression;
    };

    spReportEntityQueryManager.cloneExpressionEntity = function (expression, copyParentGroupedBy) {
        var cloneExpression = null;
        if (expression && expression.getEntity()) {
            var expressionEntity = expression.getEntity();

            cloneExpression = spEntity.fromJSON({
                typeId: expression.getTypeAlias(),
                sourceNode: expressionEntity.sourceNode ? expressionEntity.sourceNode : jsonLookup(),
                fieldExpressionField: expressionEntity.fieldExpressionField ? expressionEntity.fieldExpressionField : jsonLookup(),
                reportExpressionResultType: expressionEntity.reportExpressionResultType,
                reportScript: expressionEntity.reportScript ? expressionEntity.reportScript : jsonString(),
                expressionReferencesColumn: expressionEntity.expressionReferencesColumn ? expressionEntity.expressionReferencesColumn : jsonLookup(),
                aggregateMethod: expressionEntity.aggregateMethod ? expressionEntity.aggregateMethod : jsonLookup(),
                parentGroupedBy: (copyParentGroupedBy && expressionEntity.parentGroupedBy) ? expressionEntity.parentGroupedBy : jsonLookup(), //don't clone the parentGroupedBy info if the flag is off
                aggregatedExpression: expressionEntity.aggregatedExpression ? expressionEntity.aggregatedExpression : jsonLookup()
            });
        }

        return cloneExpression;
    };

    spReportEntityQueryManager.resortReportColumns = function (reportEntity) {
        spReportEntityQueryManager.setEntity(reportEntity);
        var sortedReportColumns = _.sortBy(reportEntity.getReportColumns(), function (column) { return column.displayOrder(); });
        for (var i = 0; i < sortedReportColumns.length; i++) {
            if (sortedReportColumns[i].getEntity()) {
                sortedReportColumns[i].getEntity().columnDisplayOrder = i;
            }
        }

    };

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // ReportColumn FormattingRule Manager Static Methods
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

    spReportEntityQueryManager.createColumnFormattingRule = function (condFormatting, type, entityTypeId) {

        var columnFormattingRules = null;

        switch (condFormatting.format) {
            case 'Highlight':
                columnFormattingRules = spReportEntityQueryManager.createColorFormattingRule(condFormatting.highlightRules, type, entityTypeId);
                break;
            case 'Icon':
                columnFormattingRules = spReportEntityQueryManager.createIconFormattingRule(condFormatting.iconRules, type, entityTypeId);
                break;
            case 'ProgressBar':
                columnFormattingRules = spReportEntityQueryManager.createProgressBar(condFormatting.progressBarRule, type, entityTypeId);
                break;
            case 'None':
                columnFormattingRules = null;
                break;

        }

        return columnFormattingRules;
    };

    spReportEntityQueryManager.createColorFormattingRule = function (highlightRules, type, entityTypeId) {

        var colorRules = spReportEntityQueryManager.createColorRules(highlightRules, type, entityTypeId);

        var colorFormattingRule = spEntity.fromJSON({
            typeId: 'colorFormattingRule',
            colorRules: jsonRelationship(colorRules)
        });

        return new spReportEntity.ColumnFormattingRule(colorFormattingRule);
    };

    spReportEntityQueryManager.createColorRules = function (highlightRules, type, entityTypeId) {
        return _.map(highlightRules, function (highlightRule, index) {
            return spReportEntityQueryManager.createColorRule(highlightRule, type, entityTypeId, index);
        });
    };

    spReportEntityQueryManager.createColorRule = function (highlightRule, type, entityTypeId, index) {

        var foregroundColor = spReportEntityQueryManager.convertColorValueToString(highlightRule.color.foregroundColor.a) +
            spReportEntityQueryManager.convertColorValueToString(highlightRule.color.foregroundColor.r) +
            spReportEntityQueryManager.convertColorValueToString(highlightRule.color.foregroundColor.g) +
            spReportEntityQueryManager.convertColorValueToString(highlightRule.color.foregroundColor.b);

        var backgroundColor = spReportEntityQueryManager.convertColorValueToString(highlightRule.color.backgroundColor.a) +
            spReportEntityQueryManager.convertColorValueToString(highlightRule.color.backgroundColor.r) +
            spReportEntityQueryManager.convertColorValueToString(highlightRule.color.backgroundColor.g) +
            spReportEntityQueryManager.convertColorValueToString(highlightRule.color.backgroundColor.b);

        var argType = highlightRule.type ? highlightRule.type : type;

        var condition = spReportEntityQueryManager.createCondition(highlightRule.operator, argType, highlightRule.value, entityTypeId);

        var colorRule = spEntity.fromJSON({
            typeId: 'colorRule',
            colorRuleForeground: jsonString(foregroundColor),
            colorRuleBackground: jsonString(backgroundColor),
            ruleCondition: jsonLookup(condition),
            rulePriority: index
        });

        return colorRule;
    };

    spReportEntityQueryManager.convertColorValueToString = function (value) {
        var retString = value.toString(16);
        if (retString === '0')
            retString = '00';

        return retString;
    };

    spReportEntityQueryManager.createIconFormattingRule = function (iconRules, type, entityTypeId) {

        var iconFormattingRules = spReportEntityQueryManager.createIconRules(iconRules, type, entityTypeId);

        var iconFormattingRule = spEntity.fromJSON({
            typeId: 'iconFormattingRule',
            iconRules: jsonRelationship(iconFormattingRules)
        });

        return new spReportEntity.ColumnFormattingRule(iconFormattingRule);
    };

    spReportEntityQueryManager.createIconRules = function (iconRules, type, entityTypeId) {
        return _.map(iconRules, function (iconRule, index) {
            return spReportEntityQueryManager.createIconRule(iconRule, type, entityTypeId, index);
        });
    };

    spReportEntityQueryManager.createIconRule = function (iconRule, type, entityTypeId, index) {

        var argType = iconRule.type ? iconRule.type : type;
        var condition = spReportEntityQueryManager.createCondition(iconRule.operator, argType, iconRule.value, entityTypeId);

        var iconTypeRule = spEntity.fromJSON({
            typeId: 'iconRule',
            iconRuleImage: iconRule.imgId > 0 ? jsonLookup(iconRule.imgId) : jsonLookup(),
            iconRuleCFIcon: iconRule.cfId > 0 ? jsonLookup(iconRule.cfId) : jsonLookup(),
            ruleCondition: jsonLookup(condition),
            rulePriority: index
        });

        return iconTypeRule;
    };

    spReportEntityQueryManager.createProgressBar = function (progressBarRule, type, entityTypeId) {
        var barMinValue = spReportEntityQueryManager.createActivityArgument(true, type, progressBarRule.minimumValue, entityTypeId);
        var barMaxValue = spReportEntityQueryManager.createActivityArgument(true, type, progressBarRule.maximumValue, entityTypeId);
        var barColor = spReportEntityQueryManager.convertColorValueToString(progressBarRule.color.a) +
            spReportEntityQueryManager.convertColorValueToString(progressBarRule.color.r) +
            spReportEntityQueryManager.convertColorValueToString(progressBarRule.color.g) +
            spReportEntityQueryManager.convertColorValueToString(progressBarRule.color.b);

        var barFormattingRule = spEntity.fromJSON({
            typeId: 'barFormattingRule',
            barColor: barColor,
            barMinValue: jsonLookup(barMinValue),
            barMaxValue: jsonLookup(barMaxValue)
        });

        return new spReportEntity.ColumnFormattingRule(barFormattingRule);
    };

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // ReportColumn ColumnDisplayFormat Manager Static Methods
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

    spReportEntityQueryManager.createImageFormattingRule = function (columnDisplayFormat) {
        var imageScaleEnum = spEntity.fromJSON({
            typeId: 'imageScaleEnum',
            id: columnDisplayFormat.imageScaleId
        });

        var imageSizeEnum = spEntity.fromJSON({
            typeId: 'console:thumbnailSizeEnum',
            id: columnDisplayFormat.imageSizeId
        });

        var imageFormattingRule = spEntity.fromJSON({
            typeId: 'imageFormattingRule',
            ruleImageScale: jsonLookup(imageScaleEnum)
        });

        return new spReportEntity.ColumnFormattingRule(imageFormattingRule);
    };

    spReportEntityQueryManager.createColumnDisplayFormat = function (columnDisplayFormat, conditionFormatting, type) {
        var enumAlignment = spEntity.fromJSON({
            typeId: 'alignEnum',
            id: 'alignAutomatic'
        });


        var displayFormat = spEntity.fromJSON({
            typeId: 'displayFormat',
            columnShowText: true,
            disableDefaultFormat: false,
            formatDecimalPlaces: 0,
            formatPrefix: jsonString(),
            formatSuffix: jsonString(),
            maxLineCount: 0,
            formatImageScale: jsonLookup(),
            dateColumnFormat: jsonLookup(),
            timeColumnFormat: jsonLookup(),
            dateTimeColumnFormat: jsonLookup(),
            formatAlignment: jsonLookup(enumAlignment),
            formatImageSize: jsonLookup(),
            entityListColumnFormat: jsonLookup()
        });

        if (conditionFormatting) {
            displayFormat.setColumnShowText(conditionFormatting.displayText);
            displayFormat.setDisableDefaultFormat(conditionFormatting.disableDefaultFormat);
        }

        if (columnDisplayFormat) {
            if (columnDisplayFormat.prefix)
                displayFormat.setFormatPrefix(columnDisplayFormat.prefix);

            if (columnDisplayFormat.suffix)
                displayFormat.setFormatSuffix(columnDisplayFormat.suffix);

            if (columnDisplayFormat.decimalPlaces)
                displayFormat.setFormatDecimalPlaces(columnDisplayFormat.decimalPlaces);

            if (columnDisplayFormat.lines)
                displayFormat.setMaxLineCount(columnDisplayFormat.lines);

            if (columnDisplayFormat.alignment) {
                switch (columnDisplayFormat.alignment) {
                    case 'Left':
                        enumAlignment = spEntity.fromJSON({
                            typeId: 'alignEnum',
                            id: 'alignLeft'
                        });
                        break;
                    case 'Right':
                        enumAlignment = spEntity.fromJSON({
                            typeId: 'alignEnum',
                            id: 'alignRight'
                        });
                        break;
                    case 'Centre':
                        enumAlignment = spEntity.fromJSON({
                            typeId: 'alignEnum',
                            id: 'alignCentre'
                        });
                        break;
                    default:
                        enumAlignment = spEntity.fromJSON({
                            typeId: 'alignEnum',
                            id: 'alignAutomatic'
                        });
                        break;

                }

                displayFormat.formatAlignment = enumAlignment;
            }


            if (type === 'DateTime' && columnDisplayFormat.dateTimeFormatName) {
                var enumDateTimeColumnFormat = spEntity.fromJSON({
                    typeId: 'dateTimeColFmtEnum',
                    id: columnDisplayFormat.dateTimeFormatName
                });

                displayFormat.setDateTimeColumnFormat(enumDateTimeColumnFormat);
            }

            if (type === 'Time' && columnDisplayFormat.dateTimeFormatName) {
                var enumTimeColumnFormat = spEntity.fromJSON({
                    typeId: 'timeColFmtEnum',
                    id: columnDisplayFormat.dateTimeFormatName
                });

                displayFormat.setTimeColumnFormat(enumTimeColumnFormat);
            }

            if (type === 'Date' && columnDisplayFormat.dateTimeFormatName) {
                var enumDateColumnFormat = spEntity.fromJSON({
                    typeId: 'dateColFmtEnum',
                    id: columnDisplayFormat.dateTimeFormatName
                });

                displayFormat.setDateColumnFormat(enumDateColumnFormat);
            }

            if (type === 'Image') {

                if (columnDisplayFormat.imageScaleId) {
                    var imageScaleEnum = spEntity.fromJSON({
                        typeId: 'imageScaleEnum',
                        id: columnDisplayFormat.imageScaleId
                    });
                    displayFormat.formatImageScale = imageScaleEnum;
                }

                if (columnDisplayFormat.imageSizeId) {
                    var imageSize = spEntity.fromJSON({
                        typeId: 'console:thumbnailSizeEnum',
                        id: columnDisplayFormat.imageSizeId
                    });

                    displayFormat.formatImageSize = imageSize;
                }
            }

            if ((type === "ChoiceRelationship" ||
                type === "InlineRelationship" ||
                type === "UserInlineRelationship" ||
                type === "StructureLevels") &&
                columnDisplayFormat.entityListFormatId) {
                const entityListFormatEnum = spEntity.fromJSON({
                    typeId: "entityListColFmtEnum",
                    id: columnDisplayFormat.entityListFormatId
                });

                displayFormat.setEntityListColumnFormat(entityListFormatEnum);
            }
        }

        return new spReportEntity.ColumnDisplayFormat(displayFormat);
    };

    spReportEntityQueryManager.resetActivityArgument = function (activityArgument) {
        
        if (activityArgument.stringParameterValue) {
            activityArgument.stringParameterValue = null;
        }
        
        if (activityArgument.intParameterValue) {
            activityArgument.intParameterValue = null;
        }
        
        if (activityArgument.decimalParameterValue) {
            activityArgument.decimalParameterValue = null;
        }
        
        if (activityArgument.boolParameterValue) {
            activityArgument.boolParameterValue = null;
        }
        
        if (activityArgument.dateTimeParameterValue) {
            activityArgument.dateTimeParameterValue = null;
        }
        
        if (activityArgument.timeParameterValue) {
            activityArgument.timeParameterValue = null;
        }
        
        if (activityArgument.dateParameterValue) {
            activityArgument.dateParameterValue = null;
        }
        
        if (activityArgument.guidParameterValue) {
            activityArgument.guidParameterValue = null;
        }
        
        if (activityArgument.resourceListParameterValues) {
            activityArgument.resourceListParameterValues = null;
        }     
    };

    spReportEntityQueryManager.updateActivityArgument = function (activityArgument, type, value) {
        switch (type) {
            case 'String':
                activityArgument.stringParameterValue = value;//jsonString(value);
                break;
            case 'AutoNumber':
            case 'Number':
            case 'Int32':
                activityArgument.intParameterValue = value;//jsonInt(value);
                break;
            case 'Decimal':
            case 'Currency':
                activityArgument.decimalParameterValue = value;//jsonDecimal(value);
                break;
            case 'Bool':
            case 'Boolean':
                activityArgument.boolParameterValue = value;//jsonBool(value);
                break;
            case 'DateTime':

                activityArgument.dateTimeParameterValue = value;// jsonDateTime(value);
                break;
            case 'Time':
                activityArgument.timeParameterValue = value;//jsonTime(value);
                break;
            case 'Date':
                activityArgument.dateParameterValue = _.isDate(value) ? spUtils.translateToUtc(value) : value;//jsonDate(value);
                break;
            case 'GUID':
                activityArgument.guidParameterValue = value;// jsonGuid(value);
                break;
            case 'InlineRelationship':
            case 'UserInlineRelationship':
            case 'ChoiceRelationship':
            case 'Image':
            case 'StructureLevels':
                activityArgument.resourceListParameterValues = jsonRelationship(spReportEntityQueryManager.buildresourceListValues(value));
                break;
        }
    };

    spReportEntityQueryManager.createActivityArgument = function (isConditionArgumentType, type, value, typeEntityId, decimalPlace) {
        var activityArgument = null;
        switch (type) {
            case 'String':
                activityArgument = spEntity.fromJSON({
                    typeId: 'stringArgument',
                    name: type,
                    stringParameterValue: jsonString(value)
                });
                break;
            case 'AutoNumber':
            case 'Number':
            case 'Int32':
                activityArgument = spEntity.fromJSON({
                    typeId: 'integerArgument',
                    name: type,
                    intParameterValue: jsonInt(value)
                });
                break;
            case 'Decimal':
                activityArgument = spEntity.fromJSON({
                    typeId: 'decimalArgument',
                    name: type,
                    numberDecimalPlaces: jsonInt(decimalPlace),
                    decimalParameterValue: jsonDecimal(value)
                });
                break;
            case 'Currency':
                activityArgument = spEntity.fromJSON({
                    typeId: 'currencyArgument',
                    name: type,
                    numberDecimalPlaces: jsonInt(decimalPlace),
                    decimalParameterValue: jsonDecimal(value)
                });
                break;
            case 'Bool':
            case 'Boolean':
                activityArgument = spEntity.fromJSON({
                    typeId: 'boolArgument',
                    name: type,
                    boolParameterValue: jsonBool(value)
                });
                break;
            case 'DateTime':
                activityArgument = spEntity.fromJSON({
                    typeId: 'dateTimeArgument',
                    name: type,
                    dateTimeParameterValue: jsonDateTime(value)
                });
                break;
            case 'Time':
                activityArgument = spEntity.fromJSON({
                    typeId: 'timeArgument',
                    name: type,
                    timeParameterValue: jsonTime(value)
                });
                break;
            case 'Date':
                value = _.isDate(value) ? spUtils.translateToUtc(value) : value;

                activityArgument = spEntity.fromJSON({
                    typeId: 'dateArgument',
                    name: type,
                    dateParameterValue: jsonDate(value)
                });
                break;
            case 'GUID':
                activityArgument = spEntity.fromJSON({
                    typeId: 'guidArgument',
                    name: type,
                    guidParameterValue: jsonGuid(value)
                });
                break;
            case 'InlineRelationship':
            case 'UserInlineRelationship':
            case 'Image':
            case 'StructureLevels':
                if (isConditionArgumentType) {
                    activityArgument = spEntity.fromJSON({
                        typeId: 'resourceListArgument',
                        name: type,
                        resourceListParameterValues: jsonRelationship(spReportEntityQueryManager.buildresourceListValues(value)),
                        conformsToType: jsonLookup(typeEntityId)
                    });
                } else {
                    activityArgument = spEntity.fromJSON({
                        typeId: 'resourceArgument',
                        name: type,
                        resourceArgumentValue: jsonLookup(value),
                        conformsToType: jsonLookup(typeEntityId)
                    });
                }
                break;
            case 'ChoiceRelationship':
                if (isConditionArgumentType) {
                    activityArgument = spEntity.fromJSON({
                        typeId: 'resourceListArgument',
                        name: type,
                        resourceListParameterValues: jsonRelationship(spReportEntityQueryManager.buildresourceListValues(value)),
                        conformsToType: jsonLookup(typeEntityId)
                    });

                } else {

                    activityArgument = spEntity.fromJSON({
                        typeId: 'resourceArgument',
                        name: type,
                        resourceArgumentValue: jsonLookup(value),
                        conformsToType: jsonLookup(typeEntityId)
                    });
                }
                break;
        }

        return activityArgument;
    };

    //convert Object {8687: "Solo", 9160: "Pepsi"} to key key array
    //parseInt(JSON.stringify(values).substring(1, JSON.stringify(values).length-1).split(',')[0].split(':')[0].replace(/"/g,''))
    spReportEntityQueryManager.buildresourceListValues = function (values) {
        if (_.isArray(values)) {
            if (spEntity.isEntity(values[0])) {
                return values;
            } else {
                return _.map(_.keys(values), function (value) {
                    if (spEntity.isEntity(value))
                        return value.id();
                    else if (_.isNumber(value)) {
                        return value;
                    } else {
                        return parseInt(value, 10);
                    }
                });
            }
        }

        if (_.isObject(values)) {
            return _.map(_.keys(values), function (value) {
                if (spEntity.isEntity(value))
                    return value.id();
                else if (_.isNumber(value)) {
                    return value;
                } else {
                    return parseInt(value, 10);
                }
            });
        }

        return null;
    };


    spReportEntityQueryManager.getTypeNameByArgumentAlias = function (typeArgumentAlias) {
        var typeName = 'UnKnown';
        switch (typeArgumentAlias) {
            case 'core:stringArgument':
            case 'stringArgument':
                typeName = 'String';
                break;
            case 'core:integerArgument':
            case 'integerArgument':
                typeName = 'Number';
                break;
            case 'core:decimalArgument':
            case 'decimalArgument':
                typeName = 'Decimal';
                break;
            case 'core:currencyArgument':
            case 'currencyArgument':
                typeName = 'Currency';
                break;
            case 'core:boolArgument':
            case 'boolArgument':
                typeName = 'Boolean';
                break;
            case 'core:dateTimeArgument':
            case 'dateTimeArgument':
                typeName = 'DateTime';
                break;
            case 'core:timeArgument':
            case 'timeArgument':
                typeName = 'Time';
                break;
            case 'core:dateArgument':
            case 'dateArgument':
                typeName = 'Date';
                break;
            case 'core:guidArgument':
            case 'guidArgument':
                typeName = 'GUID';
                break;
            case 'core:resourceArgument':
            case 'resourceArgument':
                typeName = 'InlineRelationship';
                break;
        }

        return typeName;
    };

    spReportEntityQueryManager.createOperator = function (operator) {
        var enumOperator = null;
        switch (operator) {
            case 'Equal':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operEqual'
                });
                break;
            case 'NotEqual':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operNotEqual'
                });
                break;
            case 'GreaterThan':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operGreaterThan'
                });
                break;
            case 'GreaterThanOrEqual':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operGreaterThanOrEqual'
                });
                break;
            case 'LessThan':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operLessThan'
                });
                break;
            case 'LessThanOrEqual':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operLessThanOrEqual'
                });
                break;
            case 'Contains':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operContains'
                });
                break;
            case 'StartsWith':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operStartsWith'
                });
                break;
            case 'EndsWith':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operEndsWith'
                });
                break;
            case 'Soundex':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operSoundex'
                });
                break;
            case 'IsTrue':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operIsTrue'
                });
                break;
            case 'IsFalse':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operIsFalse'
                });
                break;
            case 'IsNotNull':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operIsNotNull'
                });
                break;
            case 'IsNull':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operIsNull'
                });
                break;
            case 'AnyAboveStructureLevel':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operAnyAboveStructureLevel'
                });
                break;
            case 'AnyAtOrAboveStructureLevel':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operAnyAtOrAboveStructureLevel'
                });
                break;
            case 'AnyBelowStructureLevel':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operAnyBelowStructureLevel'
                });
                break;
            case 'AnyAtOrBelowStructureLevel':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operAnyAtOrBelowStructureLevel'
                });
                break;
            case 'FullTextSearch':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operFullTextSearch'
                });
                break;
            case 'AnyOf':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operAnyOf'
                });
                break;
            case 'AnyExcept':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operAnyExcept'
                });
                break;
            case 'CurrentUser':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operCurrentUser'
                });
                break;
            case 'CurrentEmployee':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operCurrentEmployee'
                });
                break;
            case 'DateEquals':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operDateEquals'
                });
                break;
            case 'Today':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operToday'
                });
                break;
            case 'ThisWeek':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operThisWeek'
                });
                break;
            case 'ThisMonth':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operThisMonth'
                });
                break;
            case 'ThisQuarter':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operThisQuarter'
                });
                break;
            case 'ThisYear':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operThisYear'
                });
                break;
            case 'CurrentFinancialYear':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operCurrentFinancialYear'
                });
                break;
            case 'LastNDays':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operLastNDays'
                });
                break;
            case 'LastNDaysTillNow':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operLastNDaysTillNow'
                });
                break;
            case 'NextNDays':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operNextNDays'
                });
                break;
            case 'NextNDaysFromNow':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operNextNDaysFromNow'
                });
                break;
            case 'LastNWeeks':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operLastNWeeks'
                });
                break;
            case 'NextNWeeks':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operNextNWeeks'
                });
                break;
            case 'LastNMonths':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operLastNMonths'
                });
                break;           
            case 'NextNMonths':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operNextNMonths'
                });
                break;
            case 'LastNQuarters':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operLastNQuarters'
                });
                break;
            case 'NextNQuarters':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operNextNQuarters'
                });
                break;
            case 'LastNYears':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operLastNYears'
                });
                break;
            case 'NextNYears':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operNextNYears'
                });
                break;
            case 'LastNFinancialYears':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operLastNFinancialYears'
                });
                break;
            case 'NextNFinancialYears':
                enumOperator = spEntity.fromJSON({
                    typeId: 'operatorEnum',
                    id: 'operNextNFinancialYears'
                });
                break;

        }
        return enumOperator;
    };

    spReportEntityQueryManager.createReportConditionParameter = function (type, value, typeId, decimalPlace) {
        var reportConditionParameter = null;
        var activityArgument = spReportEntityQueryManager.createActivityArgument(true, type, value, typeId, decimalPlace);


        reportConditionParameter = spEntity.fromJSON({
            typeId: 'parameter',
            paramTypeAndDefault: activityArgument ? jsonLookup(activityArgument) : jsonLookup()
        });

        return reportConditionParameter;
    };

    spReportEntityQueryManager.createCondition = function (operator, type, value, entityTypeId) {
        var operatorEntity = spReportEntityQueryManager.createOperator(operator);
        var conditionParameter = null;
        if (operator !== "Unspecified") {
            conditionParameter = spReportEntityQueryManager.createReportConditionParameter(type, value, entityTypeId);
        }

        var reportCondition = spEntity.fromJSON({
            typeId: 'reportCondition',
            conditionDisplayOrder: 0,
            conditionIsHidden: false,
            conditionIsLocked: false,
            conditionExpression: jsonLookup(),
            columnForCondition: jsonLookup(),
            operator: jsonLookup(operatorEntity),
            conditionParameter: jsonLookup(conditionParameter)
        });

        return reportCondition;
    };

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Client Aggregate Manager Static Methods
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    spReportEntityQueryManager.createColumnGrouping = function (reportEntity, groupingMethod) {
        spReportEntityQueryManager.setEntity(reportEntity);
        var groupingPriority = 0;
        var columnGroupings = reportEntity.getReportColumnGroupings();
        if (this.ReportEntity && columnGroupings && columnGroupings.length > 0) {
            var sortedReportColumnGroupings = _.sortBy(columnGroupings, function (columnGrouping) { return columnGrouping.getGroupingPriority(); });
            groupingPriority = sortedReportColumnGroupings[sortedReportColumnGroupings.length - 1].getGroupingPriority() + 1;
        } else {
            groupingPriority = 0;
        }


        var enumgroupingMethod = spEntity.fromJSON({
            typeId: 'groupingMethodEnum',
            id: groupingMethod
        });

        var reportRowGroup = spEntity.fromJSON({
            typeId: 'reportRowGroup',
            groupingMethod: jsonLookup(enumgroupingMethod),
            groupingPriority: groupingPriority
        });

        return reportRowGroup;
    };

    spReportEntityQueryManager.createColumnRollup = function (rollupMethod) {
        var aggregateMethodEnum = spEntity.fromJSON({
            typeId: 'aggregateMethodEnum',
            id: rollupMethod
        });

        var reportRollup = spEntity.fromJSON({
            typeId: 'reportRollup',
            rollupMethod: jsonLookup(aggregateMethodEnum)
        });

        return reportRollup;
    };

    spReportEntityQueryManager.setReportColumnRollups = function (reportColumnId, rollupMethods) {
        var columnRollups,
            rollupMethodsDict = _.keyBy(rollupMethods),
            newRollupMethodsDict = _.keyBy(rollupMethods),
            rollupEntitiesToRemove = [],
            reportColumn,
            haveChanges = false;

        // Find the report column entity
        reportColumn = _.find(this.ReportEntity.getReportColumns(), function (reportColumn) {
            return reportColumn.id().toString() === reportColumnId.toString();
        });

        // Column not found return
        if (!reportColumn) {
            return haveChanges;
        }

        // Get the rollups
        columnRollups = reportColumn.getColumnRollup();

        if (!columnRollups) {
            return haveChanges;
        }

        // Enumerate through the existing rollups
        _.forEach(columnRollups.getInstances(), function (rollupRelInstance) {
            var rollupMethod = rollupRelInstance.entity.getRollupMethod(),
                rollupMethodAlias;

            if (!rollupMethod) {
                return true;
            }

            rollupMethodAlias = rollupMethod.eid().getAlias();

            if (_.has(rollupMethodsDict, rollupMethodAlias)) {
                // Already exists.
                // Resurrect any deleted instances
                if (rollupRelInstance.getDataState() === spEntity.DataStateEnum.Delete) {
                    rollupRelInstance.setDataState(spEntity.DataStateEnum.Unchanged);
                    haveChanges = true;
                }
                // Remove it from the dictionary of new methods
                delete newRollupMethodsDict[rollupMethodAlias];
            } else {
                rollupEntitiesToRemove.push(rollupRelInstance.entity);
                haveChanges = true;
            }
        });

        // Remove deleted rollups
        _.forEach(rollupEntitiesToRemove, function (re) {
            columnRollups.remove(re);
        });

        // Add new rollup methods
        _.forEach(_.keys(newRollupMethodsDict), function (method) {
            var newRollup = spReportEntityQueryManager.createColumnRollup(method);
            columnRollups.add(newRollup);
            haveChanges = true;
        });

        return haveChanges;
    };

    spReportEntityQueryManager.addReportGroupingColumn = function (reportColumnId, groupMethod) {
        var columnGroupings,
            columnGroupingRelInstance,
            reportColumn,
            newEnumGroupingMethod,
            newReportRowGroup,
            groupingPriority = 0,
            allColumnGroupings,
            sortedReportColumnGroupings = [],
            haveChanges = false;

        // Find the report column entity
        reportColumn = _.find(this.ReportEntity.getReportColumns(), function (reportColumn) {
            return reportColumn.id().toString() === reportColumnId.toString();
        });

        // Column not found return
        if (!reportColumn) {
            return haveChanges;
        }

        // Hide the column
        reportColumn.setIsHidden(true);

        // Get the groups for this column
        columnGroupings = reportColumn.getColumnGrouping();

        if (!columnGroupings) {
            return haveChanges;
        }

        // See if the column already has the specified group method
        columnGroupingRelInstance = _.find(columnGroupings.getInstances(), function (groupingRelInstance) {
            var groupingMethod = groupingRelInstance.entity.getGroupingMethod();

            if (!groupingMethod) {
                return false;
            }

            return groupingMethod.eid().getAlias() === groupMethod;
        });

        // Find the next available priority
        allColumnGroupings = this.ReportEntity.getReportColumnGroupings();
        if (allColumnGroupings && allColumnGroupings.length > 0) {
            sortedReportColumnGroupings = _.sortBy(allColumnGroupings, function (columnGrouping) {
                return columnGrouping.getGroupingPriority();
            });
            groupingPriority = sortedReportColumnGroupings[sortedReportColumnGroupings.length - 1].getGroupingPriority() + 1;
        } else {
            groupingPriority = 0;
        }

        if (!columnGroupingRelInstance) {
            // Create a new grouping
            newEnumGroupingMethod = spEntity.fromJSON({
                typeId: 'groupingMethodEnum',
                id: groupMethod
            });

            newReportRowGroup = spEntity.fromJSON({
                typeId: 'reportRowGroup',
                groupingMethod: jsonLookup(newEnumGroupingMethod),
                groupingPriority: groupingPriority
            });

            columnGroupings.add(newReportRowGroup);

            haveChanges = true;
        } else {
            // Update the state and set the priority
            if (columnGroupingRelInstance.getDataState() === spEntity.DataStateEnum.Delete) {
                columnGroupingRelInstance.setDataState(spEntity.DataStateEnum.Unchanged);
            }

            columnGroupingRelInstance.entity.setGroupingPriority(groupingPriority);

            haveChanges = true;
        }

        return haveChanges;
    };


    spReportEntityQueryManager.removeReportColumnGrouping = function (reportColumnId) {
        var currentReportColumn = _.find(this.ReportEntity.getReportColumns(), function (reportColumn) {
            return reportColumn.id().toString() === reportColumnId.toString();
        });

        if (currentReportColumn) {
            // Unhide the column
            currentReportColumn.setIsHidden(false);
            currentReportColumn.removeColumnGrouping();
        }
    };


    spReportEntityQueryManager.setReportColumnGroupingCollapsedState = function (reportColumnId, collapsed) {
        var currentReportColumn;

        if (!reportColumnId || !this.ReportEntity) {
            return;
        }

        currentReportColumn = _.find(this.ReportEntity.getReportColumns(), function (reportColumn) {
            return reportColumn.id().toString() === reportColumnId.toString();
        });

        if (currentReportColumn) {            
            currentReportColumn.setColumnGroupingCollapsedState(collapsed);
        }
    };

})(spReportEntityQueryManager);