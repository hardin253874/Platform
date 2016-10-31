// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spReportEntityQueryManager */

(function () {
    'use strict';

    angular.module('mod.common.ui.spReportBuilder', ['ui', 'ui.router', 'mod.common.ui.spTreeview', 'mod.common.ui.spFieldsManager', 'mod.common.ui.spRelationshipPicker', 'mod.common.ui.spRelationshipAdvanced', 'mod.common.spEntityService', 'mod.common.alerts', 'mod.ui.spTreeviewManager', 'mod.common.ui.spCalculatedField', 'mod.common.ui.spSummariseOption', 'mod.common.ui.spSummariseFieldOption', 'mod.common.ui.spRenameColumn', 'mod.common.spTenantSettings'])

    .directive('spReportBuilder', function ($compile) {
        return {
            restrict: 'E',
            templateUrl: 'reportBuilder/spReportBuilder.tpl.html',
            controller: 'reportBuilderController',
            scope: {
                options: '=',
                action: '='
            }
        };
    })
    .service('spReportBuilderService', function () {
        var exports = {};
        var actions = [];
        var selectedNode = null;
        exports.setAction = function (type, node, field) {
            actions.push({ type: type, node: node, field: field });
        };

        exports.getActions = function (clear) {
            var resultActions = actions;

            if (clear) {
                actions = [];
            }

            return resultActions;
        };

        exports.clearActions = function () {
            actions = [];
        };


        exports.getSelectedNode = function() {
            return selectedNode;
        };
        
        exports.setSelectedNode = function (selectedTreeNode) {
            selectedNode = selectedTreeNode;
        };
        
        exports.clearSelectedNode = function () {
            selectedNode = null;
        };

        return exports;

    })
    .controller("reportBuilderController", function ($scope, $stateParams, spAlertsService, spRelationshipDialog,
                                                     spRelationshipAdvancedDialog, spTreeviewManager,
                                                     spEntityService, reportBuilderService, spReportBuilderService,
                                                     spCalculatedFieldDialog, spSummariseOptionDialog,
                                                     spSummariseFieldOptionDialog, spRenameColumnDialog, spTenantSettings) {

        $scope.reportModel = null;
        $scope.reportEntity = null;
        $scope.treeviewOptions = null;      
        $scope.selectedNode = null;      
        $scope.nodeFields = null;
        $scope.nameFieldId = null;
        $scope.reportBuilderService = reportBuilderService;
        $scope.spReportBuilderService = spReportBuilderService;
        $scope.reportBuilderModel = { reportEntity: null, treeNode: null, selectedTreeNode: null, selectedTreeNodeId: 0, treeviewOptions: null, fileManagerOptions: null, isParentAggregated: false };


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Watch options methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////  
        // Watch for options changes
        $scope.$watch('options.reportEntityUpdated', function () {
           
            if ($scope.options && $scope.options.reportEntity) {

                $scope.spReportBuilderService.clearActions();

                $scope.reportBuilderModel.reportEntity = $scope.options.reportEntity;
                if ($scope.reportBuilderModel && $scope.reportBuilderModel.selectedTreeNode) {
                    $scope.setTreeViewOption($scope.reportBuilderModel.selectedTreeNode);
                    $scope.setFileManagerOption($scope.reportBuilderModel.selectedTreeNode.cols, $scope.reportBuilderModel.selectedTreeNode, $scope.reportBuilderModel.selectedTreeNode.etid);                    
                }
            }
            
            if (!$scope.nameFieldId) {
                spTenantSettings.getNameFieldEntity().then(function (name) {
                    $scope.nameFieldId = name.id();
                    spReportEntityQueryManager.setNameFieldId($scope.nameFieldId);
                });                
            }

            spReportEntityQueryManager.setNameFieldId($scope.nameFieldId);
            
        });
        
        $scope.$watch('options.treeNode', function () {

            if ($scope.options && $scope.options.treeNode) {
                $scope.reportBuilderModel.treeNode = $scope.options.treeNode;
                $scope.reportBuilderModel.selectedTreeNode = $scope.reportBuilderModel.treeNode;
                $scope.reportBuilderModel.selectedTreeNodeId = $scope.reportBuilderModel.selectedTreeNode.nid;
                $scope.setTreeViewOption($scope.reportBuilderModel.selectedTreeNode);
            }

        });
             
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Watch Service Action methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////  
        // Watch for report builder service action
        $scope.$watch('spReportBuilderService.getActions(false).length', function () {
            var actions = spReportBuilderService.getActions(true);

            if (!actions) {
                return;
            }
            
            _.forEach(actions, function (action) {
                switch (action.type) {
                case "addColumnToReport":
                    $scope.addColumnToReport(action.field, $scope.reportBuilderModel.selectedTreeNode);
                    break;
                case "removeColumnFromReport":
                    $scope.removeColumnFromReport(action.field);
                    break;
                case "addColumnToAnalyzer":
                    $scope.addColumnToAnalyzer($scope.reportBuilderModel.selectedTreeNode, action.field);
                    break;
                case "removeColumnFromAnalyzer":
                    $scope.removeColumnFromAnalyzer(action.field);
                    break;
                case "updateSelectedNode":
                    $scope.updateSelectedNode(action.node);
                    break;
                case "removeNode":
                    $scope.removeNodeAndRelationship(action.node);
                    break;
                }
            });
        });

        // Watch for report builder service action from report
        $scope.$watch('reportBuilderService.getActionsFromReport(false).length', function () {
            var actionsFromReport = $scope.reportBuilderService.getActionsFromReport(true);
            if (!actionsFromReport) {
                return;
            }

            _.forEach(actionsFromReport, function (actionFromReport) {
                var nodeId = $scope.reportBuilderModel.selectedTreeNode.nid;
                var options;
                switch (actionFromReport.type) {
                case "addColumnByDragDrop":
                    if (actionFromReport.field && actionFromReport.target) {
                        actionFromReport.field.inrep = true;

                        var columnIndex = -1;

                        if (actionFromReport.target.colDef.spColumnDefinition) {

                            var reportColumn = _.find($scope.reportBuilderModel.reportEntity.getReportColumns(), function (selectColumn) {
                                return selectColumn.id().toString() === actionFromReport.target.colDef.spColumnDefinition.columnId.toString();
                            });

                            if (reportColumn) {
                                columnIndex = reportColumn.displayOrder() + 1;
                            }
                        } else {


                            columnIndex = actionFromReport.target ? actionFromReport.target.originalIndex : -1;
                            if (columnIndex > -1) {

                                var unhiddenColumn = spReportEntityQueryManager.getUnHideColumnByIndex($scope.reportBuilderModel.reportEntity, columnIndex);

                                if (unhiddenColumn) {
                                    columnIndex = unhiddenColumn.displayOrder() + 1;
                                }
                            }
                        }

                        var parentAggregate = $scope.getParentAggregateEntity($scope.reportBuilderModel.selectedTreeNode);
                        
                        if (parentAggregate || actionFromReport.field.isAggregated === true) {
                            $scope.updateFieldSummariseOption(actionFromReport.field, columnIndex);
                        } else {
                            $scope.addColumnToReport(actionFromReport.field, $scope.reportBuilderModel.selectedTreeNode, columnIndex);
                        }
                    }

                    break;
                case 'reOrderColumnByDragDrop':
                    if (actionFromReport.field && actionFromReport.target) {
                        var column1 = actionFromReport.field;
                        var column2 = actionFromReport.target;

                        if (column1.colDef.spColumnDefinition && column2.colDef.spColumnDefinition) {
                            var columnId1 = column1.colDef.spColumnDefinition.columnId;
                            var columnId2 = column2.colDef.spColumnDefinition.columnId;

                            options = { columnId1: columnId1, columnId2: columnId2 };
                            //$scope.reportBuilderService.setActionFromReportBuilder('reOrderColumnToReport', null, null, options);
                            $scope.reportBuilderService.reOrderColumnToReport(columnId1, columnId2);
                        }

                    }
                    break;
                case "addAnalyzerByDragDrop":
                    if (actionFromReport.field) {
                        actionFromReport.field.inanal = true;

                        var analyzerFieldIndex = actionFromReport.target ? actionFromReport.target.tag.anlcol.ord : -1;

                        if (analyzerFieldIndex > -1)
                            analyzerFieldIndex++;

                        $scope.addColumnToAnalyzer($scope.reportBuilderModel.selectedTreeNode, actionFromReport.field, analyzerFieldIndex);
                    }

                    break;
                case "reOrderAnalyzerByDragDrop":
                    if (actionFromReport.field && actionFromReport.target) {

                        var conditionId1 = actionFromReport.field.tag.id;
                        var conditionId2 = actionFromReport.target.tag.id;

                        if (conditionId1 > -1 && conditionId2 > -1) {
                            options = { conditionId1: conditionId1, conditionId2: conditionId2 };
                            //$scope.reportBuilderService.setActionFromReportBuilder('reOrderConditionToReport', null, null, options);
                            $scope.reportBuilderService.reOrderConditionToReport(conditionId1, conditionId2);
                        }
                    }


                    break;
                case "updateCalculation":
                    if (actionFromReport.field) {
                        $scope.updateCalculation(actionFromReport.field);
                    }
                    break;
                case "updateSummarise":
                    if (actionFromReport.field) {
                        $scope.updateSummariseOption(actionFromReport.field);
                    }
                    break;
                case "renameColumn":
                    if (actionFromReport.field) {
                        $scope.renameColumn(actionFromReport.field);
                    }
                    break;
                case "removeColumn":
                    if (actionFromReport.field) {
                        $scope.removeColumn(actionFromReport.field);
                    }
                    break;
                case "removeAnalyzer":
                    if (actionFromReport.field) {
                        $scope.removeAnalyzer(actionFromReport.field);
                    }
                    break;
                    case "updateAnalyzerFieldConfig":
                    if (actionFromReport.field) {
                        $scope.updateAnalyzerFieldConfig(actionFromReport.field);
                    }
                    break;
                }
            });
        });
       


      

        //// set the treeview options, (the whole treenode and selectednodeid)
        //$scope.setTreeViewOption = function (treeNode, selectedNodeId) {
        //    $scope.treeviewOptions = { "treeNode": treeNode, "selectedNodeId": selectedNodeId };
        //    $scope.nodeFields = $scope.selectedNode.cols;
        //};

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Entity
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////       

        // set the treeview options, (the whole treenode and selectednodeid)
        $scope.setTreeViewOption = function (selectedNode) {
            //$scope.$apply(function () {
            $scope.reportBuilderModel.treeviewOptions = { treeNodes: [$scope.reportBuilderModel.treeNode],  treeNodeChanged: spUtils.newGuid() };

        };

        $scope.$watch('reportBuilderModel.treeviewOptions', function() {

            if ($scope.reportBuilderModel.treeviewOptions) {
                
            }
        });



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // File Manager from Report Entity
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        $scope.$watch('reportBuilderModel.selectedTreeNode', function () {

            if ($scope.reportBuilderModel.selectedTreeNode) {
                //the treeNode columns list exists
                if ($scope.reportBuilderModel.selectedTreeNode.cols && $scope.reportBuilderModel.selectedTreeNode.cols.length > 0) {
                    // $scope.nodeFields = $scope.reportBuilderModel.selectedTreeNode.cols;
                    $scope.setFileManagerOption($scope.reportBuilderModel.selectedTreeNode.cols, $scope.reportBuilderModel.selectedTreeNode, $scope.reportBuilderModel.selectedTreeNode.etid);
                }
                else {
                    

                    spTreeviewManager.getNodeColumns($scope.reportBuilderModel.selectedTreeNode.etid, $scope.nameFieldId, function (columns) {
                        $scope.reportBuilderModel.selectedTreeNode.cols = columns;
                        $scope.setFileManagerOption($scope.reportBuilderModel.selectedTreeNode.cols, $scope.reportBuilderModel.selectedTreeNode, $scope.reportBuilderModel.selectedTreeNode.etid);
                    });

                  
                }

                $scope.spReportBuilderService.setSelectedNode($scope.reportBuilderModel.selectedTreeNode);
            }

        });

      
        // set the fileManger options, node field collection, selectedNodeId and current reportentity
        $scope.setFileManagerOption = function (fields, selectedNode, entityTypeId) {
            $scope.reportBuilderModel.fileManagerOptions = { "nodeFields": fields, "selectedNode": selectedNode, "treeNode": $scope.reportBuilderModel.treeNode, "entityTypeId": entityTypeId, "reportEntity": $scope.reportBuilderModel.reportEntity, fileManagerChanged: spUtils.newGuid() };
        };


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Relationship Picker
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // pick relationship dialog control
        $scope.pickRelationship = function() {
            if ($scope.reportBuilderModel.selectedTreeNode) {

                var relationshipPickerOptions = { selectedNode: $scope.reportBuilderModel.selectedTreeNode };

                spRelationshipDialog.showModalDialog(relationshipPickerOptions).then(function (result) {
                    if (result) {

                        if (result.relRelationships && result.relRelationships.length > 0) {

                            var addName = result.addName;
                            var nameField = addName ? result.nameField : null;
                            $scope.addNodesAndRelationships(result.relRelationships, nameField);
                            //$scope.$apply();                       
                        }
                        if (result.removeRelationships && result.removeRelationships.length > 0) {
                            var nodes = [];
                            result.removeRelationships.forEach(
                                function (relationship) {
                                    var node = spTreeviewManager.getNodeByRelationship($scope.reportBuilderModel.selectedTreeNode, relationship);
                                    if (node) {
                                        nodes.push(node);
                                    }

                                    //$scope.$apply();
                                }
                            );

                            $scope.removeNodesAndRelationships(nodes);
                        }
                    }
                });

            } else {

                spAlertsService.addAlert('please select a relationship node first', spAlertsService.sev.Error);
                return;

               
            }
        };


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Advanced Window
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // advanced dialog control
        $scope.advanced = function () {
            if ($scope.reportBuilderModel.selectedTreeNode && $scope.reportBuilderModel.selectedTreeNode.qe) {
                
                var advanceOptions = { selectedNode: $scope.reportBuilderModel.selectedTreeNode };

                spRelationshipAdvancedDialog.showModalDialog(advanceOptions).then(function (result) {
                    if (result) {
                        $scope.reportBuilderModel.selectedTreeNode = result.selectedNode;

                        //update Query                    
                        //var options = { entityNodeId: $scope.reportBuilderModel.selectedTreeNodeId, nodeEntity: result.selectedNode.qe };
                        //$scope.reportBuilderService.setActionFromReportBuilder('updateQueryEntity', null, null, options);
                        $scope.reportBuilderService.updateQueryEntity($scope.reportBuilderModel.selectedTreeNodeId, result.selectedNode.qe);
                        // update relationship tree node query entity

                        //$scope.setTreeViewOption($scope.reportBuilderModel.selectedTreeNode);

                    }
                });
            } else {
                spAlertsService.addAlert('please select a relationship node first', spAlertsService.sev.Error);
                return;
            }

        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Calculation Editor
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        $scope.calculationEditor = function () {
            //should not be able to add a calculated field to a summarised node
            var parentAggregateEntity = $scope.getParentAggregateEntity(spTreeviewManager.getNode($scope.reportBuilderModel.selectedTreeNode.qe.id(), $scope.reportBuilderModel.treeNode));
            if ($scope.reportBuilderModel.selectedTreeNode && $scope.reportBuilderModel.selectedTreeNode.qe && !parentAggregateEntity)
            {
                var calculateOptions = { entityTypeId: $scope.reportBuilderModel.selectedTreeNode.etid, stateParams: $stateParams, columnName:'' };
                
                spCalculatedFieldDialog.showModalDialog(calculateOptions).then(function (result) {
                    if (result) {
                        //var options = { entityNode: $scope.reportBuilderModel.selectedTreeNode.qe, script: result.script, type: result.resultType, columnName: result.columnName };
                        //$scope.reportBuilderService.setActionFromReportBuilder('addCalculateColumnToReport', null, null, options);
                        $scope.reportBuilderService.addCalculateColumnToReport($scope.reportBuilderModel.selectedTreeNode.qe, result.script, result.resultType, result.columnName, result.entityTypeId);
                    }
                });

            }else if (parentAggregateEntity) {
                spAlertsService.addAlert('Cannot add a calculated field under summarised node', spAlertsService.sev.Error);
                return;
            }
            else {
                spAlertsService.addAlert('Please select a relationship node first', spAlertsService.sev.Error);
                return;
            }
        };
        
        $scope.updateCalculation = function (selectColumnDefinition) {
            var columnId = selectColumnDefinition.tag.id;
            if (columnId) {
                var reportColumn = _.find($scope.reportBuilderModel.reportEntity.getReportColumns(), function(column) {
                    return column.id().toString() === columnId.toString();
                });
                
                if (reportColumn) {

                    var sourceTypeId;
                    if (reportColumn.getExpression().getSourceNode().getResourceReportNodeType) {
                        sourceTypeId = reportColumn.getExpression().getSourceNode().getResourceReportNodeType().id();
                    } else {
                        var reportNode = spReportEntityQueryManager.getReportNodeById($scope.reportBuilderModel.reportEntity.getRootNode(), reportColumn.getExpression().getSourceNode().id());
                        sourceTypeId = reportNode.getResourceReportNodeType().id();
                    }

                   
                    var script = reportColumn.getExpression().getReportScript();
                    
                    var calculateOptions = { entityTypeId: sourceTypeId, stateParams: $stateParams, script: script, columnName: reportColumn.getName() };
                    
                    spCalculatedFieldDialog.showModalDialog(calculateOptions).then(function (result) {
                        if (result) {
                            //var options = { columnId: reportColumn.id(), script: result.script, type: result.resultType, columnName: result.columnName };
                            //$scope.reportBuilderService.setActionFromReportBuilder('updateCalculateColumnToReport', null, null, options);
                            $scope.reportBuilderService.updateCalculateColumnToReport(reportColumn.id(), result.script, result.resultType, result.columnName, result.entityTypeId);
                        }
                    });
                }
            }
        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Summarise Option
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //create new summarise and load summarise option dialog window
        $scope.summariseOption = function() {
            if ($scope.reportBuilderModel.selectedTreeNode && $scope.reportBuilderModel.selectedTreeNode.qe) {
                setUnderSummarise($scope.reportBuilderModel.treeNode);
                var summariseOptions = { selectedNode: $scope.reportBuilderModel.selectedTreeNode, reportEntity: $scope.reportBuilderModel.reportEntity, treeNode: [$scope.reportBuilderModel.treeNode] };

                spSummariseOptionDialog.showModalDialog(summariseOptions).then(function (result) {
                    if (result) {                       
                        var selectedNodeId = result.selectedNodeId;
                        var summariseAction = result.summariseAction;
                        var removeSummarise = result.removeSummarise;                        
                        var options;
                        if (!removeSummarise) {
                            var treeNode = spTreeviewManager.getNode(selectedNodeId, $scope.reportBuilderModel.treeNode);                            
                            if (treeNode) {                                
                                var aggregateEntity;
                                if (treeNode.pae) {
                                    aggregateEntity = treeNode.pae;
                                    if (aggregateEntity) {
                                        if (summariseAction && summariseAction.length > 0) {
                                            $scope.reportBuilderService.updateAggregateColumns(aggregateEntity, summariseAction);
                                        }                                        
                                    }
                                } else {
                                    //check anychild node has been aggregated first, if exists, swap the aggregate node
                                    var childNodesAggregateEntity = $scope.getChildNodesWithAggregateEntity(treeNode);

                                   aggregateEntity = spReportEntityQueryManager.createAggreateEntity(treeNode.qe.getEntity());
                                    treeNode.pae = aggregateEntity;
                                    if (aggregateEntity) {
                                        $scope.reportBuilderService.createSummarise(treeNode, summariseAction, childNodesAggregateEntity);
                                        //_.forEach(childNodesAggregateEntity, function(childNodeAggregateEntity) {
                                        //    childNodeAggregateEntity.pae = null;
                                        //});
                                    }
                                }
                            }
                        } else {
                           
                            //remove summarise
                            var parentEntity, parentAggregateEntity, selectedTreeNode;
                            // the selected node from summarise option dialog result is not current selected node
                            if (selectedNodeId) {
                                selectedTreeNode = spTreeviewManager.getNode(selectedNodeId, $scope.reportBuilderModel.treeNode);
                                if (selectedTreeNode) {
                                    parentEntity = selectedTreeNode.pe;
                                    parentAggregateEntity = selectedTreeNode.pae;

                                }

                            } else {
                                parentEntity = $scope.reportBuilderModel.selectedTreeNode.pe;
                                parentAggregateEntity = $scope.reportBuilderModel.selectedTreeNode.pae;
                            }
                            if (parentAggregateEntity) {
                                $scope.reportBuilderService.removeSummarise(parentEntity, parentAggregateEntity);
                                if (selectedTreeNode) {
                                    selectedTreeNode.pae = null;
                                } else {
                                    $scope.reportBuilderModel.selectedTreeNode.pae = null;
                                }
                            }
                        }

                        
                    }
                });
            } else {
                spAlertsService.addAlert('Please select a relationship node first', spAlertsService.sev.Error);
                return;
            }
        };

        //update summarise and load summarise option dialog window
        $scope.updateSummariseOption = function (selectColumnDefinition) {
            var columnId = selectColumnDefinition.tag.id;
            if (columnId) {
                var reportColumn = _.find($scope.reportBuilderModel.reportEntity.getReportColumns(), function(column) {
                    return column.id().toString() === columnId.toString();
                });

                if (reportColumn) {
                    //get source Column
                    var expression = reportColumn.getExpression();
                    var sourceNode = expression.getSourceNode();                    
                    if (expression.getTypeAlias() === 'aggregateExpression' || expression.getTypeAlias() === 'core:aggregateExpression') {
                        sourceNode = expression.getAggregatedExpression().getSourceNode();                        
                    }

                    var treeNode = spTreeviewManager.getNode(sourceNode.id(), $scope.reportBuilderModel.treeNode);
                    setUnderSummarise($scope.reportBuilderModel.treeNode);
                    var summariseOptions = { selectedNode: treeNode, reportEntity: $scope.reportBuilderModel.reportEntity, treeNode: [$scope.reportBuilderModel.treeNode] };

                    //if current field treeNode is not selected TreeNode and current treeNode is summarised. reset selelctedTreeNode to current treeNode
                    if (treeNode.nid !== $scope.reportBuilderModel.selectedTreeNode.nid && treeNode.pae) {
                        $scope.updateSelectedNode(treeNode);
                    }

                    spSummariseOptionDialog.showModalDialog(summariseOptions).then(function (result) {
                        if (result) {
                            var selectedNodeId = result.selectedNodeId;
                            var summariseAction = result.summariseAction;
                            var removeSummarise = result.removeSummarise;
                            var options;
                            if (!removeSummarise) {
                                var treeNode = spTreeviewManager.getNode(selectedNodeId, $scope.reportBuilderModel.treeNode);
                                if (treeNode) {
                                    var aggregateEntity;
                                    if (treeNode.pae) {
                                        aggregateEntity = treeNode.pae;
                                        if (aggregateEntity) {
                                            $scope.reportBuilderService.updateAggregateColumns(aggregateEntity, summariseAction);
                                        }
                                    } else {
                                        aggregateEntity = spReportEntityQueryManager.createAggreateEntity(treeNode.qe.getEntity());
                                        treeNode.pae = aggregateEntity;
                                        if (aggregateEntity) {
                                            $scope.reportBuilderService.createSummarise(treeNode, summariseAction);
                                        }
                                    }
                                }
                            } else {
                                //remove summarise
                                var parentEntity, parentAggregateEntity, selectedTreeNode;
                                // the selected node from summarise option dialog result is not current selected node
                                if (selectedNodeId) {
                                    selectedTreeNode = spTreeviewManager.getNode(selectedNodeId, $scope.reportBuilderModel.treeNode);
                                    if (selectedTreeNode) {
                                        parentEntity = selectedTreeNode.pe;
                                        parentAggregateEntity = selectedTreeNode.pae;
                                        
                                    }

                                } else {
                                    parentEntity = $scope.reportBuilderModel.selectedTreeNode.pe;
                                    parentAggregateEntity = $scope.reportBuilderModel.selectedTreeNode.pae;
                                }
                                if (parentAggregateEntity) {
                                    $scope.reportBuilderService.removeSummarise(parentEntity, parentAggregateEntity);
                                    if (selectedTreeNode) {
                                        selectedTreeNode.pae = null;
                                    } else {
                                        $scope.reportBuilderModel.selectedTreeNode.pae = null;
                                    }
                                }
                            }


                        }
                    });

                }
            }
        };

        $scope.updateFieldSummariseOption = function(field, columnIndex) {
            var summariseOptions = { field: field, selectedNode: $scope.reportBuilderModel.selectedTreeNode, reportEntity: $scope.reportBuilderModel.reportEntity };
            var currentNode = null;
            if (field.isAggregated === true) {
                //get node from field relationship id
                if (field.relid > 0) {
                    currentNode = spTreeviewManager.getNodeByRelationshipId(field.relid, $scope.reportBuilderModel.treeNode);                    
                }
                //get node if current node is child node
                if (!currentNode &&
                    $scope.reportBuilderModel.selectedTreeNode &&
                           $scope.reportBuilderModel.selectedTreeNode.pnid > 0) {
                    currentNode = $scope.reportBuilderModel.selectedTreeNode;
                }

                if (currentNode) {
                    summariseOptions.selectedNode = currentNode;
                }
            }

            spSummariseFieldOptionDialog.showModalDialog(summariseOptions).then(function (result) {
                if (result) {
                    var summariseAction = result.summariseAction;
                    
                    var options;
                    var subNodeEntity;
                    var isRelationshipField = false;
                    if (summariseAction) {
                        //if this field is relationship field
                        var node = null;
                        if (field.relid > 0) {
                            node = getRelationshipFieldNode(field);
                            isRelationshipField = true;
                        }
                        //or if current field in child tree node
                        if (!node &&
                            $scope.reportBuilderModel.selectedTreeNode &&
                            $scope.reportBuilderModel.selectedTreeNode.pnid > 0) {
                            node = $scope.reportBuilderModel.selectedTreeNode;
                            isRelationshipField = true;
                        }

                        if (isRelationshipField) {                            
                            //get relationship nodeid is it's exist node
                            if (node) {
                                subNodeEntity = node.qe;                               
                            }
                            else {
                                var relationship = getRelationshipFromRelField(field);

                                var newNode = $scope.addNode(relationship, false);
                                //add new node to reportentity firest
                                if (newNode) {
                                    var parentAggregateEntity = $scope.getParentAggregateEntity(newNode);
                                    //$scope.reportBuilderService.setActionFromReportBuilder('addRelationship', newNode, null, null);
                                    $scope.reportBuilderService.addRelationship(newNode, null, parentAggregateEntity);
                                    subNodeEntity = newNode.qe;
                                }
                            }
                        }

                        if (!spUtils.isNullOrUndefined($scope.reportBuilderModel.selectedTreeNode.pe)  && spUtils.isNullOrUndefined(field.dname)) {
                            field.dname = $scope.reportBuilderModel.selectedTreeNode.name;
                        }

                        var parentAggregate = $scope.getParentAggregateEntity($scope.reportBuilderModel.selectedTreeNode);
                        var currentNodeEntity = $scope.reportBuilderModel.selectedTreeNode.qe;
                        if (currentNode) {
                            if (!parentAggregate) {
                                parentAggregate = currentNode.pae;
                            }
                            currentNodeEntity = currentNode.qe;
                        }
                        //options = { treeNode: $scope.reportBuilderModel.selectedTreeNode, field: field, summariseAction: summariseAction, subNodeEntity: subNodeEntity, insertAfterColumn: columnIndex };
                        //$scope.reportBuilderService.setActionFromReportBuilder('updateAggregateColumnsAfterColumn', null, null, options);
                        $scope.reportBuilderService.updateAggregateColumnsAfterColumn(parentAggregate, field, summariseAction, currentNodeEntity, subNodeEntity, columnIndex);
                    }
                }
            });
        };
        
        function setUnderSummarise (treeNode, underSummariseId) {
            treeNode.underSummariseId = underSummariseId;
            
            if (treeNode.pae) {
                underSummariseId = treeNode.nid;
            }

            _.forEach(treeNode.children, function(childNode) {
                setUnderSummarise(childNode, underSummariseId);
            });

            //if (treeNode.pae)
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Rename Column Option
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        $scope.renameColumn = function(selectColumnDefinition) {
            var columnId = selectColumnDefinition.tag.id;
            if (columnId) {
                var reportColumn = _.find($scope.reportBuilderModel.reportEntity.getReportColumns(), function(column) {
                    return column.id().toString() === columnId.toString();
                });

                if (reportColumn) {
                    var reNameOptions = { reportColumn: reportColumn };

                    spRenameColumnDialog.showModalDialog(reNameOptions).then(function(result) {
                        if (result) {
                            $scope.reportBuilderService.updateColumnNameFromReport(result.columnId, result.columnName);
                        }
                    });
                }
            }
        };
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Drag & drop options
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        $scope.dropOptions = {
            onAllowDrop: function (source, target) {
                if (!target) {
                    return false;
                }

                var t = $(target);

                return true;

            },
            onDragOver: function (event, source, target) {
                if (!target) {
                    return false;
                }

                var t = $(target);

                return true;

            },
            onDrop: function (event, source, target, dragData) {

                if (!target) {
                    return;
                }

                $scope.onDrop(dragData);

                return;
            }
        };

        $scope.onDrop = function (dragData) {
            if (dragData) {
                                
                //reomve column from report
                if (dragData.colDef) {
                    $scope.removeColumn(dragData.colDef.spColumnDefinition);
                }else if (dragData.tag) {                    
                    $scope.removeAnalyzer(dragData);
                }
           }
        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Build ReportNode Static Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        $scope.addNodesAndRelationships = function (relationships, namefield) {
            var nodes = [];
            var nameFields = [];
            _.each(relationships, function (relationship) {
                nodes.push($scope.addNode(relationship, false));
                if (namefield) {
                    nameFields.push(cloneField(namefield, relationship.resourceName));
                }

                //update the relationship field's checkbox
                //if (relationship.relationshipType !== 'relationships') {
                //    var type = relationship.relationshipType === 'lookups' ? 'InlineRelationship' : 'ChoiceRelationship';

                //    $scope.updateFieldByColumn({ fid: namefield.fid, type: type, relid: relationship.rid, tid: relationship.eid }, true);
                //}

            });            


            var parentAggregateEntity = $scope.getParentAggregateEntity($scope.reportBuilderModel.selectedTreeNode);
            //var options = { parentAggregateEntityNode: parentAggregateEntity };
            //$scope.reportBuilderService.setActionFromReportBuilder('addRelationships', nodes, nameFields, options);
            $scope.reportBuilderService.addRelationships(nodes, nameFields, parentAggregateEntity);
        };

        //add node to treeview nodes
        $scope.addNode = function (relationship, replaceSelectedNode) {

            if (relationship) {

                var relatedQueryEntity = createRelatedEntity(relationship);
                var relationshipType = relationship.relationshipType === "choicefields" ? "lookups" : relationship.relationshipType;
                if ($scope.reportBuilderModel.selectedTreeNode.qe && relatedQueryEntity) {
                    $scope.reportBuilderModel.selectedTreeNode.qe.addRelatedReportNode(relatedQueryEntity);
                    var followInReverse = relatedQueryEntity.getFollowInReverse ? relatedQueryEntity.getFollowInReverse() : false;
                    var nodeName = relationship.resourceName ? relationship.resourceName : relationship.typeName;
                    var newNode = spTreeviewManager.newNode(relatedQueryEntity.id(), nodeName, relatedQueryEntity, $scope.reportBuilderModel.selectedTreeNode.nid, $scope.reportBuilderModel.selectedTreeNode.qe, null, relationship.eid, relationship.rid, relationshipType, $scope.reportBuilderModel.selectedTreeNode.etid, relationship.eid, relationship.cols, followInReverse);

                    $scope.reportBuilderModel.treeNode = spTreeviewManager.addNode(newNode, $scope.reportBuilderModel.selectedTreeNode.nid, $scope.reportBuilderModel.treeNode);


                    if (replaceSelectedNode === true) {
                        $scope.reportBuilderModel.selectedTreeNode = newNode;
                    } else {
                        $scope.reportBuilderModel.selectedTreeNode = spTreeviewManager.getNode($scope.reportBuilderModel.selectedTreeNode.nid, $scope.reportBuilderModel.treeNode);
                    }

                    $scope.reportBuilderModel.selectedNodeId = $scope.reportBuilderModel.selectedTreeNode.nid;

                    //$scope.setTreeViewOption($scope.reportBuilderModel.selectedTreeNode);

                    return newNode;
                }
            }

            return null;
        };
        
        // remove node from treenode then call back to update structuredquery and reload report 
        $scope.removeNode = function (treenode) {

           
            if (treenode) {

                //removeNode
                $scope.reportBuilderModel.treeNode = spTreeviewManager.removeNode(treenode, $scope.reportBuilderModel.treeNode);
                $scope.reportBuilderModel.selectedTreeNode = $scope.reportBuilderModel.treeNode;
                $scope.reportBuilderModel.selectedTreeNodeId = $scope.reportBuilderModel.selectedTreeNode.nid;
                //$scope.setTreeViewOption($scope.reportBuilderModel.selectedTreeNode);
            }
           
        };

        $scope.removeNodeAndRelationship = function (node) {

            var treenode = spTreeviewManager.getNode(node.nid, $scope.reportBuilderModel.treeNode);
            var parentAggregateEntity = $scope.getParentAggregateEntity(node);
            //$scope.reportBuilderService.setActionFromReportBuilder('removeRelationship', treenode, null, null);
            $scope.reportBuilderService.removeRelationship(treenode, parentAggregateEntity);
            $scope.removeNode(treenode);
            //back to root node the isParentAggregated flag reset to false
            $scope.reportBuilderModel.isParentAggregated = false;
            
        };

        $scope.removeNodesAndRelationships = function (nodes) {

            var treenodes = [];

            var parentAggregateEntity = (nodes && nodes.length > 0) ? $scope.getParentAggregateEntity(nodes[0]) : null;

            _.each(nodes, function (node) {
                var treenode = spTreeviewManager.getNode(node.nid, $scope.reportBuilderModel.treeNode);
                $scope.removeNode(treenode);
                treenodes.push(treenode);

            });

            //$scope.reportBuilderService.setActionFromReportBuilder('removeRelationships', treenodes, null, null);
            $scope.reportBuilderService.removeRelationships(treenodes, parentAggregateEntity);

         
        };


        //to check current node is summarised or the parent node is summarised or not,  if it is summarised, do not show the calculated button
        $scope.showCalculationEditor = function (treeNode) {
            var show = true;

            if (!treeNode) {
                return show;
            }


            if (treeNode.pae) {
                show = false;
            }
            else if (treeNode.pnid) {
                var parentNode = spTreeviewManager.getNode(treeNode.pnid, $scope.reportBuilderModel.treeNode);
                show = $scope.showCalculationEditor(parentNode);
            }


            return show;

        };

        // update the selected node and refresh fileds 
        $scope.updateSelectedNode = function (treenode) {
            if (treenode) {
                $scope.reportBuilderModel.selectedTreeNode = treenode;
                $scope.reportBuilderModel.selectedTreeNodeId = treenode.nid;
                var parentAggregateEntity = $scope.getParentAggregateEntity(treenode);
               
                if (treenode.pae) {
                    $scope.reportBuilderModel.isParentAggregated = false;
                } else {
                    $scope.reportBuilderModel.isParentAggregated = parentAggregateEntity && treenode.pe ? true : false;
                }
            }
        };
      
        //get parent aggregate entity from current treeNode
        $scope.getParentAggregateEntity = function(treeNode) {
            var parentAggregateEntity = treeNode.pae;
            
            if (parentAggregateEntity) {
                return parentAggregateEntity;
            } else {
                // return 
                return treeNode.pe ? $scope.getParentAggregateEntity(spTreeviewManager.getNode(treeNode.pe.id(), $scope.reportBuilderModel.treeNode)) : null;
            }

        };
        
        //get all childAggregate Entities from current treeNode
        $scope.getChildAggregateEntities = function (treeNode) {
            
            var childAggregateEntities = [];
            
            if (treeNode.pae) {
                childAggregateEntities.push(treeNode.pae);
            } else {
                if (treeNode.children && treeNode.children.length > 0) {
                    _.forEach(treeNode.children, function (childNode) {
                        var aggregateEntity = $scope.getChildAggregateEntities(childNode);
                        if (aggregateEntity) {
                            childAggregateEntities = _.union(childAggregateEntities, aggregateEntity);
                        }

                    });
                }
            }

            return childAggregateEntities;
        };
        
        //get all childAggregate Entities from current treeNode
        $scope.getChildNodesWithAggregateEntity = function (treeNode) {

            var childAggregateEntities = [];

            if (treeNode.pae) {
                childAggregateEntities.push(treeNode);
            } else {
                if (treeNode.children && treeNode.children.length > 0) {
                    _.forEach(treeNode.children, function (childNode) {
                        var aggregateEntity = $scope.getChildNodesWithAggregateEntity(childNode);
                        if (aggregateEntity) {
                            childAggregateEntities = _.union(childAggregateEntities, aggregateEntity);
                        }

                    });
                }
            }

            return childAggregateEntities;
        };
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Build ReportColumn Static Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        function getRelationshipFromRelField(field) {
            var isChoiceField = field.ftype === "ChoiceRelationship";
            var relationshipId = field.relid;
            var entityId;
            var dir = 'Forward';
            
            if (field.isReverse === true) {
                dir = 'Reverse';
                entityId = field.ftid;
            } else {
                dir = 'Forward';
                entityId = field.ttid;
            }

            var relationship = { "rid": relationshipId, "relationshipName": field.fname, "resourceName": field.fname, "relationshipType": "lookups", "isChoiceField": isChoiceField, "isSelected": false, "eid": entityId, "dir": dir, "cols": [] };

            return relationship;
        }

        // add column to report by field and node (or nodeEntity)
        $scope.addColumnToReport = function (field, node, insertAfterColumn) {
            var options = null;
            if (!node) {
                node = $scope.reportBuilderModel.selectedTreeNode;
            }

            //get relationship field and structure view field node id
            if (field.relid > 0 || field.svid > 0) {
                if (!field.fid) {
                    field.fid = $scope.nameFieldId;
                }
                node = getRelationshipFieldNode(field);
                //get relationship nodeid
                if (node) {
                    //options = { entityNode: node.qe, insertAfterColumn: insertAfterColumn, columnId: null, expressionId: null };
                    //$scope.reportBuilderService.setActionFromReportBuilder('addColumnToReport', null, field, options);
                    $scope.reportBuilderService.addColumnToReport(field, node.qe, insertAfterColumn);

                }
                else {


                    var relationship = getRelationshipFromRelField(field);

                    var newNode = $scope.addNode(relationship, false);
                  
                    if (newNode) {                        
                        var parentAggregateEntity = $scope.getParentAggregateEntity(newNode);
                        //$scope.reportBuilderService.setActionFromReportBuilder('addRelationship', newNode, null, null);
                        $scope.reportBuilderService.addRelationship(newNode, null, parentAggregateEntity);

                        if (!field.fid) {
                            field.fid = $scope.nameFieldId;
                        }

                        //options = { entityNode: newNode.qe, insertAfterColumn: insertAfterColumn, columnId: null, expressionId: null };
                        //$scope.reportBuilderService.setActionFromReportBuilder('addColumnToReport', null, field, options);
                        $scope.reportBuilderService.addColumnToReport(field, newNode.qe, insertAfterColumn);
                    }                 
                }

            }
            else {

                if ($scope.reportBuilderModel.selectedTreeNode.pnid > 0)
                {
                    field.dname = $scope.reportBuilderModel.selectedTreeNode.name;
                }

                //options = { entityNode: node.qe, insertAfterColumn: insertAfterColumn, columnId: null, expressionId: null };
                //$scope.reportBuilderService.setActionFromReportBuilder('addColumnToReport', null, field, options);
                $scope.reportBuilderService.addColumnToReport(field, node.qe, insertAfterColumn);

            }
        };
        
        // remove column from report by field
        $scope.removeColumnFromReport = function (field) {
            var entityNode = $scope.reportBuilderModel.selectedTreeNode.qe;

            var options = null;
            if (field.relid > 0 || field.svid > 0) {
                if (!field.fid ) {
                    field.fid = $scope.nameFieldId;
                }
                entityNode = getRelationshipFieldNode(field);
                if (entityNode) {
                    entityNode = entityNode.qe;
                }
            }

            if (!entityNode) {
                entityNode = $scope.reportBuilderModel.selectedTreeNode.qe;
            }

            //options = { entityNode: entityNode };
            //$scope.reportBuilderService.setActionFromReportBuilder('removeColumnFromReport', null, field, options);
           
            $scope.reportBuilderService.removeColumnFromReport(field, entityNode);

        };

        
        //Remove column from report drag & drop
        //Get columnId from columnDefinition.tag then remove from query
        $scope.removeColumn = function (selectColumnDefinition) {
            var columnId = selectColumnDefinition.tag.id;
           
            if (columnId) {
                var parentAggregateEntity = $scope.reportBuilderModel.selectedTreeNode.pae;
                $scope.reportBuilderService.removeColumnById(columnId, true, parentAggregateEntity);
                _.delay(function() {
                    $scope.$apply();
                });
            }
        };

        

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Build ReportCondition Static Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // add column to analyzer by field and node (or nodeEntity)
        $scope.addColumnToAnalyzer = function (node, field, insertAfterAnalyzer) {            
            if (!node) {
                node = $scope.reportBuilderModel.selectedTreeNode;
            }
            
            
            //get relationship field and structure view field node id
            if (field.relid > 0 || field.svid > 0) {
                node = getRelationshipFieldNode(field);
                //get new node

                if (node) {
                    $scope.reportBuilderService.addColumnToAnalyzer(field, node.qe, insertAfterAnalyzer);
                }
                else {

                    var relationship = getRelationshipFromRelField(field);

                    var newNode = $scope.addNode(relationship, false);
                    if (newNode) {
                        
                        var parentAggregateEntity = $scope.getParentAggregateEntity(newNode);
                        $scope.reportBuilderService.addRelationship(newNode, null, parentAggregateEntity);                        

                        if (!field.fid) {
                            field.fid = $scope.nameFieldId;
                        }
                        $scope.reportBuilderService.addColumnToAnalyzer(field, newNode.qe, insertAfterAnalyzer);
                    }                   
                }

            }
            else {

                if($scope.reportBuilderModel.selectedTreeNode.pnid > 0)
                {
                    field.dname = $scope.reportBuilderModel.selectedTreeNode.name;
                }

                $scope.reportBuilderService.addColumnToAnalyzer(field, node.qe, insertAfterAnalyzer);
            }
        };

        // remove column from analzyer by field
        $scope.removeColumnFromAnalyzer = function (field) {
            var entityNode = $scope.reportBuilderModel.selectedTreeNode.qe;

            if (field.relid > 0 || field.svid > 0) {
                if (!field.fid) {
                    field.fid = $scope.nameFieldId;
                }
                entityNode = getRelationshipFieldNode(field);
            }

            if (!entityNode) {
                entityNode = $scope.reportBuilderModel.selectedTreeNode.qe;
            }

            //var options = { entityNode: entityNode };            
            //$scope.reportBuilderService.setActionFromReportBuilder('removeColumnFromAnalyzer', null, field, options);
            $scope.reportBuilderService.removeColumnFromAnalyzer(field, entityNode);
        };
        
        $scope.removeAnalyzer = function (analyzerField) {
            var conditionId = null;
            try {
                conditionId = analyzerField.tag.id;
            } catch (e) {

            }

            if (conditionId) {
                //var conExpression = scope.removeAnalyzerById(expId);
                //var options = { conditionId: conditionId };
                //$scope.reportBuilderService.setActionFromReportBuilder('removeAnalyzerById', null, null, options);
                $scope.reportBuilderService.removeAnalyzerById(conditionId);
                _.delay(function () {
                    $scope.$apply();
                });
            }
        };

        $scope.updateAnalyzerFieldConfig = function (analyzerField) {
            var conditionId = null;
            try {
                conditionId = analyzerField.tag.id;
            } catch (e) {

            }

            if (conditionId) {
                // update analyzer condition
                $scope.reportBuilderService.updateAnalyzerFieldConfiguration(analyzerField,conditionId);
                _.delay(function () {
                    $scope.$apply();
                });
            }
        };
        
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Common Private Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //clone the field object by different fieldname
        function cloneField(field, fieldName) {
            return { "fid": field.fid, "fname": field.fname, "dname": fieldName, "ftype": field.ftype, "isreq": field.isreq, "group": field.group, "svid": field.svid, "relid": field.relid, "ftid": field.ftid, "ttid": field.ttid, "aggm": field.aggm, "inrep": field.inrep, "inanal": field.inanal, "fieldOrder": field.fieldOrder, "isReverse": field.isReverse };
        }

        //create related reportnode by relationship
        function createRelatedEntity (relationship) {
            var relatedQueryEntity = null;
            var relationshipType = relationship.relationshipType === "choicefields" ? "lookups" : relationship.relationshipType;
            if (relationshipType === "relationships" || relationshipType === "lookups") {
               
                relatedQueryEntity = spReportEntityQueryManager.createRelatedEntity(relationship.eid, relationship.rid, relationship.dir);
            }
            else if (relationshipType === "derivedResources") {
                
                relatedQueryEntity = spReportEntityQueryManager.createDerivedTypeEntity(relationship.eid);
            }
            else if (relationshipType === "relationshipInstance") {
              
                relatedQueryEntity = spReportEntityQueryManager.createRelationshipInstance();
            }

            return relatedQueryEntity;
        }
        
        //get related node from relationship field
        function getRelationshipFieldNode (field) {

            var retNode = null;
            if ($scope.reportBuilderModel.selectedTreeNode.children && $scope.reportBuilderModel.selectedTreeNode.children.length > 0) {
                for (var i = 0; i < $scope.reportBuilderModel.selectedTreeNode.children.length; i++) {
                    if ($scope.reportBuilderModel.selectedTreeNode.children[i].relid === field.relid && $scope.reportBuilderModel.selectedTreeNode.children[i].name === field.fname) {
                        //if it is reverse relationship, compare the field forward type id with node entity id, otherwise compare field to type id and node entity id
                        if ($scope.reportBuilderModel.selectedTreeNode.children[i].followInReverse === true && $scope.reportBuilderModel.selectedTreeNode.children[i].etid === field.ftid)
                            retNode = $scope.reportBuilderModel.selectedTreeNode.children[i];
                        else if ($scope.reportBuilderModel.selectedTreeNode.children[i].followInReverse === false && $scope.reportBuilderModel.selectedTreeNode.children[i].etid === field.ttid) {
                            retNode = $scope.reportBuilderModel.selectedTreeNode.children[i];
                        } else if (!$scope.reportBuilderModel.selectedTreeNode.children[i].followInReverse && $scope.reportBuilderModel.selectedTreeNode.children[i].etid === field.ttid) {
                            retNode = $scope.reportBuilderModel.selectedTreeNode.children[i];
                        }
                        break;
                    }
                    else if ($scope.reportBuilderModel.selectedTreeNode.children[i].relid === field.svid && $scope.reportBuilderModel.selectedTreeNode.children[i].etid === field.ttid) {
                        //check structrue view relationship, may changed later
                        retNode = $scope.selectedNode.children[i];
                        break;
                    }
                }
            }
            else {
                retNode = null;
            }
            return retNode;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // old Methods, can be removed after use reportEntity 
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////


        
    });
}());