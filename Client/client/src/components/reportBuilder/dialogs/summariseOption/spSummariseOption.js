// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global spReportEntityQueryManager */

(function () {
    'use strict';

    /**
    * Module implementing Summarise Option.
    * 
    * @module spSummariseOptionController
    * @example
        
    Using the spSummariseOption:
    
    spSummariseOptionDialog.showModalDialog(options).then(function(result) {
    });
       
    where calculationOptions is available on the controller with the following properties:
         - selectedNode - {object} - the selected entitytype node to load all relationships for current entity type
         - reportEntity - {object} - current reportEntity object
           
    * 
    */
    angular.module('mod.common.ui.spSummariseOption', ['ui.bootstrap', 'mod.common.alerts', 'ngGrid', 'mod.common.ui.spTreeview', 'mod.common.spEntityService', 'sp.common.spDialog', 'mod.common.alerts'])
     .service('spSummariseOptionService', function () {
         var exports = {};        
         var selectedNodeId = null;
        

         exports.getSelectedNodeId = function () {
             return selectedNodeId;
         };

         exports.setSelectedNodeId = function (selectedTreeNodeId) {
             selectedNodeId = selectedTreeNodeId;
         };

         exports.clearSelectedNodeId = function () {
             selectedNodeId = null;
         };

         return exports;

     })
    .controller("spSummariseOptionController", function ($scope, $uibModalInstance, $templateCache, options, spAlertsService,
                                                         spDialog, spTreeviewManager, spSummariseOptionService, spEntityService) {
        $scope.options = options || {};
        $scope.showAdvancedOption = false;
        $scope.buttonText = 'Show Tree';
        $scope.model = { selectedNode: null, reportEntity: null, treeNode: null, selectedNodeId: 0, isSummarised: false };
        $scope.nameFieldId = 0;
        $scope.reportColumnData = [];
        $scope.reportColumnDefs = [];
        $scope.preSummarise = [];
        $scope.summariseAction = [];
        $scope.treeNodesDict = {};
        $scope.spSummariseOptionService = spSummariseOptionService;
        $scope.gridOptions = {
            data: 'reportColumnData',
            columnDefs: 'reportColumnDefs',
            enableSorting: false,
            enableColumnResize: true,
            enableCellEdit: false,
            enableCellSelection: false,
            enableRowSelection: false,
            rowHeight: 238
        };

        $scope.$watch('options', function () {
            $scope.spSummariseOptionService.clearSelectedNodeId();
            $scope.model.selectedNode = options.selectedNode;
            $scope.model.isSummarised = options.selectedNode.pae != null;
            $scope.model.reportEntity = options.reportEntity;
            $scope.model.treeNode = options.treeNode;
            $scope.model.selectedNodeId = $scope.model.selectedNode.nid;
            $scope.spSummariseOptionService.setSelectedNodeId($scope.model.selectedNode.nid);
            if ($scope.model.treeNode) {
                $scope.buildTreeNodeDicts($scope.model.treeNode[0]);
            }
                       
            //$scope.buildDataGrid();
        });

       

        $scope.buildDataGrid = function()
        {
            $scope.reportColumnData = [];
            $scope.reportColumnDefs = [];
            $scope.preSummarise = [];
            $scope.summariseAction = [];

            if ($scope.model.reportEntity) {
                var aggregateValues = {};
                var displayColumns = [];                

                //combin all unhidden report columns by same column
                displayColumns = $scope.buildDisplayColumns();

                //build reportColumn ref and column data
                _.forEach(displayColumns, function(column) {
                    $scope.reportColumnDefs.push({
                        field: column.columnId.toString(),
                        displayName: column.columnName,
                        width: 140,
                        sortable: false,
                        groupable: false,
                        cellTemplate: $templateCache.get('reportBuilder/dialogs/summariseOption/checkBoxCellTemplate.tpl.html')
                    });
                    //build aggregatemethod options

                    var isDisabled = false;
                    try {
                        if (_.includes($scope.treeNodesDict[$scope.spSummariseOptionService.getSelectedNodeId()], column.entityNodeId)) {
                            isDisabled = false;
                        } else {
                            isDisabled = true;
                        }
                    } catch (e) {
                         isDisabled = false;
                    }
                    var aggregateMethodOptions = $scope.buildAggregateMethodOptions(column, isDisabled);
                    
                    var aggregateValue = {};
                    aggregateValue['aggregateMethodOptions'] = aggregateMethodOptions;
                    

                    

                    try {                        
                        if (_.includes($scope.treeNodesDict[$scope.spSummariseOptionService.getSelectedNodeId()], column.entityNodeId)) {
                            aggregateValue['disable'] = false;
                        } else {
                            aggregateValue['disable'] = true;
                        }
                    } catch(e) {
                        aggregateValue['disable'] = false;
                    }
                    aggregateValues[column.columnId.toString()] = aggregateValue;
                });

                $scope.reportColumnData.push(aggregateValues);
            }          
        };

        $scope.buildDisplayColumns = function() {

            var idFunction, nameFunction;
            var combinColumns = [];
            var sortedReportColumns = _.sortBy($scope.model.reportEntity.getReportColumns(), function (column) { return column.displayOrder(); });
            _.forEach(sortedReportColumns, function (reportColumn) {
                if (!reportColumn.isHidden() && reportColumn.getExpression()) {
                    var expression = reportColumn.getExpression();                    
                    var expressionType = expression.getTypeAlias();                                    
                    var aggregateExpressionType;

                    var isAggregateExpression = (expressionType === 'aggregateExpression' || expressionType === 'core:aggregateExpression');
                    var field, sourceNode;

                    if (isAggregateExpression) {
                        field = expression.getAggregatedExpression().getField();
                        sourceNode = expression.getAggregatedExpression().getSourceNode();

                        aggregateExpressionType = sp.result(expression, 'getAggregatedExpression.getTypeAlias');

                        if (aggregateExpressionType === 'structureViewExpression' || aggregateExpressionType === 'core:structureViewExpression' || (aggregateExpressionType === 'core:resourceExpression' && !field)) {
                            idFunction = function () { return $scope.nameFieldId; };
                            nameFunction = function () { return 'Name'; };
                        
                            field = { id: idFunction, getName: nameFunction, name: 'Name' };
                        }
                    } else {
                        field = expression.getField();                        
                        sourceNode = expression.getSourceNode();

                        if (aggregateExpressionType === 'core:resourceExpression' && !field) {
                            idFunction = function () { return $scope.nameFieldId; };
                            nameFunction = function () { return 'Name'; };

                            field = { id: idFunction, getName: nameFunction, name: 'Name' };
                        }
                    }

                    if (!field && (expressionType === 'resourceExpression' || expressionType === 'core:resourceExpression'))
                    {
                         idFunction = function () { return $scope.nameFieldId; };
                         nameFunction = function () { return 'Name'; };
                        
                         field = { id: idFunction, getName: nameFunction, name: 'Name' };
                         sourceNode = expression.getSourceNode();
                    }

                    if (!field && (expressionType === 'scriptExpression' || expressionType === 'core:scriptExpression')) {
                         idFunction = function () { return -1; };
                         nameFunction = function () { return ''; };

                         field = { id: idFunction, getName: nameFunction, name: '' };
                        sourceNode = null;
                    }                    

                    if (expressionType === 'structureViewExpression' || expressionType === 'core:structureViewExpression') {
                        idFunction = function () { return $scope.nameFieldId; };
                        nameFunction = function () { return 'Name'; };
                        
                        field = { id: idFunction, getName: nameFunction, name: 'Name' };
                        sourceNode = expression.getSourceNode();
                    }

                    var fieldType = '';
                    var resultType = expression.getReportExpressionResultType();
                    
                    if (isAggregateExpression && expression.getAggregatedExpression()) {
                        resultType = expression.getAggregatedExpression().getReportExpressionResultType();
                    }


                    if (resultType) {
                        if (resultType.name) {
                            fieldType = resultType.name;
                        } else {
                            fieldType = resultType.getType ? spReportEntityQueryManager.getTypeNameByArgumentAlias(resultType.getType().getAlias()) : '';
                        }
                    } else {
                        fieldType = 'String';
                    }

                    if (!isAggregateExpression) {
                        //if not aggregate column, add to existColumn List

                        combinColumns.push({ columnId: reportColumn.id(), columnName: reportColumn.getName(), fieldType: fieldType, fieldId: field.id(), fieldName: field.name, entityNodeId: sourceNode? sourceNode.id() : 0, isAggregateExpression: isAggregateExpression, aggregateMethods: ['Show values'] });
                        $scope.preSummarise.push({ columnId: reportColumn.id(), originalColumnId: reportColumn.id(), aggregateMethod: 'Show values' });
                    } else {
                        var aggregateMethod = expression.getAggregateMethod().getAlias ? expression.getAggregateMethod().getAlias() : expression.getAggregateMethod()._id._ns + ':' + expression.getAggregateMethod()._id._alias;
                        var existColumnIndex = existColumn(combinColumns, reportColumn);
                        if (existColumnIndex >= 0) {
                            combinColumns[existColumnIndex].aggregateMethods.push(aggregateMethod);
                            $scope.preSummarise.push({ columnId: combinColumns[existColumnIndex].columnId, originalColumnId: reportColumn.id(), entityNodeId: sourceNode ? sourceNode.id() : 0, aggregateMethod: aggregateMethod });
                        } else {
                            var groupedNode = expression.getSourceNode().getGroupedNode();

                            var originalColumnName = reportColumn.getName();
                            if (originalColumnName.indexOf(':') > -1) {
                                originalColumnName = originalColumnName.split(':')[1].trim();
                            }

                            combinColumns.push({ columnId: reportColumn.id(), columnName: originalColumnName, fieldType: fieldType, fieldId: field ? field.id() : null, fieldName: field ? field.name : '', entityNodeId: sourceNode ? sourceNode.id() : 0, isAggregateExpression: isAggregateExpression, aggregateMethods: [aggregateMethod] });
                            $scope.preSummarise.push({ columnId: reportColumn.id(), originalColumnId: reportColumn.id(), aggregateMethod: aggregateMethod });
                        }


                    }
                }
            });

            return combinColumns;
        };
        
        function existColumn(combinColumns, reportColumn) {
            var existingColumnIndex = -1;
           
            for (var i = 0; i < combinColumns.length; i++) {
                var expression = reportColumn.getExpression();
                var field = null;
                try {
                    field = expression.getField();
                } catch(e) {
                    
                }

                if (!field && expression.getAggregatedExpression() && expression.getAggregatedExpression().getField) {
                    try {
                        field = expression.getAggregatedExpression().getField();
                    } catch(e) {
                        
                    }
                }                

                var groupedNode = expression.getAggregatedExpression().getSourceNode();
                if (((field && combinColumns[i].fieldId === field.id()) || (!field && combinColumns[i].fieldName === 'Name')) &&
                    (groupedNode && combinColumns[i].entityNodeId === groupedNode.id()))
                {
                    existingColumnIndex = i;
                    break;
                }
            }

            return existingColumnIndex;
        }
        
        //build current column's aggregate method options
        $scope.buildAggregateMethodOptions = function (column, isDisabled) {
         
            var aggregateMethodOptions = [];

            var aggregateMethodOption = { name: '', isChecked: false };

            aggregateMethodOption = $scope.buildAggregateMethodOption('Show values', column, false);
          
            aggregateMethodOptions.push(aggregateMethodOption);           
        
            
            aggregateMethodOption = $scope.buildAggregateMethodOption('Count', column, isDisabled);
            aggregateMethodOptions.push(aggregateMethodOption);
            if (column.fieldType === 'Bool' || column.fieldType === 'Boolean') {
                //TODO for boolean field, don't show Count Unique option for not
            } else {
                aggregateMethodOption = $scope.buildAggregateMethodOption('Count unique', column, isDisabled);
                aggregateMethodOptions.push(aggregateMethodOption);
            }
            aggregateMethodOption = $scope.buildAggregateMethodOption('Count all', column, isDisabled);
            aggregateMethodOptions.push(aggregateMethodOption);

            switch (column.fieldType) {
                case 'DateTime':
                case 'Time':
                case 'Date':
                case 'ChoiceRelationship':                    
                    aggregateMethodOption = $scope.buildAggregateMethodOption('Max', column, isDisabled);
                    aggregateMethodOptions.push(aggregateMethodOption);
                    aggregateMethodOption = $scope.buildAggregateMethodOption('Min', column, isDisabled);
                    aggregateMethodOptions.push(aggregateMethodOption);
                    break;
                case 'AutoNumber':
                case 'Number':
                case 'Int32':
                case 'Decimal':
                case 'Currency':                                        
                    aggregateMethodOption = $scope.buildAggregateMethodOption('Sum', column, isDisabled);
                    aggregateMethodOptions.push(aggregateMethodOption);
                    aggregateMethodOption = $scope.buildAggregateMethodOption('Average', column, isDisabled);
                    aggregateMethodOptions.push(aggregateMethodOption);                    
                    aggregateMethodOption = $scope.buildAggregateMethodOption('Max', column, isDisabled);
                    aggregateMethodOptions.push(aggregateMethodOption);
                    aggregateMethodOption = $scope.buildAggregateMethodOption('Min', column, isDisabled);
                    aggregateMethodOptions.push(aggregateMethodOption);
                    break;
            }
                                   
          

            var isDisabledList = isDisabled;

            if ($scope.spSummariseOptionService.getSelectedNodeId() === column.entityNodeId) {
                isDisabledList = false;
            } else {
                isDisabledList = true;
            }

            switch (column.fieldType) {            
                case 'String':
                case 'ChoiceRelationship':
                case 'InlineRelationship':
                case 'UserInlineRelationship':
                case 'Image':
                    aggregateMethodOption = $scope.buildAggregateMethodOption('List', column, isDisabledList);
                    aggregateMethodOptions.push(aggregateMethodOption);
                    break;
            }
            

            return aggregateMethodOptions;
        };

        $scope.getAggregateMethodAlias = function (aggregateOptionName) {

            if (aggregateOptionName.toLowerCase() === 'count all')
                return 'core:aggCount';
            else if (aggregateOptionName.toLowerCase() === 'count')
                return 'core:aggCountWithValues';
            else if (aggregateOptionName.toLowerCase() === 'count unique')
                return 'core:aggCountUniqueItems';
            else
                return 'core:agg' + aggregateOptionName.replace(/ /g, '');

        };


        //build current column's aggregate method option
        $scope.buildAggregateMethodOption = function (aggregateOptionName, column, isDisabled) {
            var aggregateMethodOption = { name: '', isChecked: false, columnId: 0 };
           
            var aggregateOptionConvertName = aggregateOptionName === 'Show values' ? aggregateOptionName : $scope.getAggregateMethodAlias(aggregateOptionName);

            if (_.includes(column.aggregateMethods, aggregateOptionConvertName)) {
                aggregateMethodOption = { name: aggregateOptionName, isChecked: true, columnId: column.columnId, isDisabled: isDisabled, columnName: column.columnName };
            } else {
                aggregateMethodOption = { name: aggregateOptionName, isChecked: false, columnId: column.columnId, isDisabled: isDisabled, columnName: column.columnName };
            }

            return aggregateMethodOption;
        };

        $scope.showAdvance = function () {
            if ($scope.showAdvancedOption) {
                $scope.showAdvancedOption = false;
                $scope.buttonText = 'Show Tree';
            } else {
                $scope.showAdvancedOption = true;
                $scope.buttonText = 'Hide Tree';
            }
        };

        $scope.$watch('spSummariseOptionService.getSelectedNodeId()', function () {
            var selectedNodeId = $scope.spSummariseOptionService.getSelectedNodeId();
            if (selectedNodeId && $scope.model.treeNode) {
                if ($scope.nameFieldId <= 0) {
                    spEntityService.getEntity('core:name', 'id', { hint:'nameField', batch: true }).then(function(field) {
                        $scope.nameFieldId = field.id();
                        $scope.buildDataGrid();
                    });
                } else {
                    $scope.buildDataGrid();
                }

                //check current node issummarize or not
                //isSummarised
                var treeNode = spTreeviewManager.getNode(selectedNodeId, $scope.model.treeNode[0]);
                if (treeNode && treeNode.pae) {
                    $scope.model.selectedNode = treeNode;
                    $scope.model.selectedNodeId = treeNode.nid;
                    $scope.model.isSummarised = true;
                } else {
                    $scope.model.isSummarised = false;
                }

                _.delay(function () {
                    $scope.$apply();
                });
            }
        });

        $scope.buildTreeNodeDicts = function (treeNode) {
            $scope.treeNodesDict[treeNode.nid] = buildTreeNodeDict(treeNode);
            if (treeNode.children && treeNode.children.length > 0) {
                _.forEach(treeNode.children, function (childNode) {
                    $scope.buildTreeNodeDicts(childNode);
                });
            }
        };

        function buildTreeNodeDict(treeNode) {
            var treeNodeDict = [];
            treeNodeDict.push(treeNode.nid);
            if (treeNode.children && treeNode.children.length > 0) {
                _.forEach(treeNode.children, function (childNode) {
                    treeNodeDict = _.union(treeNodeDict, buildTreeNodeDict(childNode));
                });
            }

            return treeNodeDict;
        }

        $scope.clickAggregate = function(option, removeOption) {
            if (option) {
                var addAction = { columnId: option.columnId, aggregateMethod: option.name,  action: 'add', originalColumnId:0 };
                var removeAction = { columnId: option.columnId, aggregateMethod: option.name, action: 'remove', originalColumnId: 0 };
                if (option.isChecked === false || removeOption) {

                    //if same column add action exists, remove this action, otherwise add remove action
                    if (_.find($scope.summariseAction, function (summarise) { return summarise.columnId === addAction.columnId && summarise.aggregateMethod === addAction.aggregateMethod && summarise.action === addAction.action; })) {
                        $scope.summariseAction = _.filter($scope.summariseAction, function (summarise) { return !(summarise.columnId === addAction.columnId && summarise.aggregateMethod === addAction.aggregateMethod && summarise.action === addAction.action); });
                    } else {
                        $scope.summariseAction.push(removeAction);
                    }

                } else {
                    //if same column remove action exists, remove this action
                    if (_.find($scope.summariseAction, function (summarise) { return summarise.columnId === removeAction.columnId && summarise.aggregateMethod === removeAction.aggregateMethod && summarise.action === removeAction.action; })) {
                        $scope.summariseAction = _.filter($scope.summariseAction, function (summarise) { return !(summarise.columnId === removeAction.columnId && summarise.aggregateMethod === removeAction.aggregateMethod && summarise.action === removeAction.action); });
                    }

                    $scope.summariseAction.push(addAction);
                    
                    //if the option is 'Show values' uncheck all other aggregate option
                    //if current option is other, uncheck 'Show values' option
                    if (option.name === 'Show values') {
                        _.forEach($scope.reportColumnData[0][option.columnId].aggregateMethodOptions, function (currentOption) {
                            if (currentOption.name !== 'Show values' && currentOption.isChecked === true) {
                                currentOption.isChecked = false;
                                $scope.clickAggregate(currentOption, true);
                            }
                        });
                    }
                    else {
                        var currentShowValuesOption = _.find($scope.reportColumnData[0][option.columnId].aggregateMethodOptions, function (currentOption) { return currentOption.name === 'Show values'; });
                        if (currentShowValuesOption && currentShowValuesOption.isChecked === true) {
                            currentShowValuesOption.isChecked = false;
                            $scope.clickAggregate(currentShowValuesOption, true);
                        }
                    }
                }
            }
        };

        // click ok button to return calculated script
        $scope.ok = function () {
            
            if ($scope.summariseAction.length > 0 && $scope.preSummarise.length > 0) {
                //filter all existing pre summarise column from action list e.g. user uncheck pre summarise column then check again
                _.forEach($scope.preSummarise, function(preColumnAction) {
                    $scope.summariseAction = _.filter($scope.summariseAction, function (summarise) {
                        var aggregateOptionConvertName = summarise.aggregateMethod === 'Show values' ? summarise.aggregateMethod : $scope.getAggregateMethodAlias(summarise.aggregateMethod);

                        if (summarise.columnId === preColumnAction.columnId && aggregateOptionConvertName === preColumnAction.aggregateMethod && summarise.action === 'remove') {
                            summarise.originalColumnId = preColumnAction.originalColumnId;
                        }

                        return !(summarise.columnId === preColumnAction.columnId && aggregateOptionConvertName === preColumnAction.aggregateMethod && summarise.action === 'add');
                    });                  
                });
            }


            var retResult = { selectedNodeId: $scope.spSummariseOptionService.getSelectedNodeId(), summariseAction: $scope.summariseAction, removeSummarise: false };


            $uibModalInstance.close(retResult);
        };
        
        // click cancel to return report builder
        $scope.cancel = function () {
            $uibModalInstance.close(null);
        };

        $scope.removeSummarise = function() {

            //get all existing aggregate columns' name with current aggregate nodes
            var aggregateColumnNames = '';
            _.forEach($scope.model.reportEntity.getReportColumns(), function(column) {
                var expression = column.getExpression();
                if (expression) {
                    var columnSource = expression.getSourceNode();
                    if (columnSource && columnSource.id() === $scope.model.selectedNode.pae.id()) {
                        aggregateColumnNames += column.getName() + ', ';
                    }
                }
            });
            var retResult;
            if (aggregateColumnNames.length > 0) {
                //remove last two characters
                aggregateColumnNames = aggregateColumnNames.substr(0, aggregateColumnNames.length - 2);


                spDialog.confirmDialog('Remove Summarise','You are about to remove Summarise from ' + aggregateColumnNames + '. Are you sure you wish to continue?').then(function(result) {
                    if (result) {

                        if ($scope.options.selectedNode.nid !== $scope.model.selectedNode.nid) {
                            retResult = { selectedNodeId: $scope.model.selectedNode.nid, summariseAction: null, removeSummarise: true };
                        } else {
                            retResult = { selectedNodeId: null, summariseAction: null, removeSummarise: true };
                        }
                        
                        $uibModalInstance.close(retResult);
                    }
                });

            } else {
                retResult = { selectedNodeId: null, summariseAction: null, removeSummarise: true };
                $uibModalInstance.close(retResult);
            }
            
            
        };

    })
    .factory('spSummariseOptionDialog', function (spDialogService) {
        // setup the dialog
        var exports = {

            showModalDialog: function (options, defaultOverrides) {
                var dialogDefaults = {
                    title: 'Summarise Option',
                    keyboard: true,
                    backdropClick: true,
                    windowClass: 'modal summariseoptiondialog-view',
                    templateUrl: 'reportBuilder/dialogs/summariseOption/spSummariseOption.tpl.html',
                    controller: 'spSummariseOptionController',
                    resolve: {
                        options: function () {
                            return options;
                        }
                    }
                };

                if (defaultOverrides) {
                    angular.extend(dialogDefaults, defaultOverrides);
                }

                return spDialogService.showModalDialog(dialogDefaults);
            }

        };

        return exports;
    });
}());