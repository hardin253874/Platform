// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
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
    angular.module('mod.common.ui.spSummariseFieldOption', ['ui.bootstrap', 'mod.common.alerts', 'ngGrid', 'mod.common.ui.spTreeview', 'sp.common.spDialog', 'mod.common.alerts'])
        .controller("spSummariseFieldOptionController", function($scope, $uibModalInstance, $templateCache, options) {
            $scope.options = options || {};
            $scope.model = { field: null, fieldName: '', reportEntity: null, selectedNode : null, aggregateMethodOptions: [] };
            $scope.summariseAction = [];
            $scope.preSummarise = [];
            $scope.$watch('options', function () {
                $scope.model.field = options.field;
                
                if (options.field.dname && options.field.fname === 'Name') {
                    $scope.model.fieldName = options.field.dname;
                } else if (options.field.dname && options.field.fname !== 'Name' && !spUtils.isNullOrUndefined(options.selectedNode.pe)) {
                    if (options.field.dname === options.field.fname) {
                        $scope.model.fieldName = options.field.fname;
                    } else {
                        $scope.model.fieldName = options.field.dname + ' ' + options.field.fname;
                    }
                } else if (!spUtils.isNullOrUndefined(options.selectedNode.pe) && options.field.fname !== 'Name') {
                    if (options.selectedNode.name === options.field.fname) {
                        $scope.model.fieldName = options.field.fname;
                    } else {
                        $scope.model.fieldName = options.selectedNode.name + ' ' + options.field.fname;
                    }
                }
                else {
                    $scope.model.fieldName = options.field.fname;
                }

                $scope.model.reportEntity = options.reportEntity;
                $scope.model.selectedNode = options.selectedNode;
                $scope.buildAggregateMethodList();
            });

            $scope.buildAggregateMethodList = function() {
                var existingsColumns = [];

                _.forEach($scope.model.reportEntity.getReportColumns(), function(reportColumn) {
                    var expression = reportColumn.getExpression();
                    var expressionType = expression.getTypeAlias();
                    var field = $scope.model.field;
                    var isAggregateExpression = (expressionType === 'aggregateExpression' || expressionType === 'core:aggregateExpression');

                    if (!isAggregateExpression) {
                        if (expression.getSourceNode().getFollowRelationship()) {
                            //if the column expression is resourceExpression, check the relationshipid match the expression's followrelationship id
                            if ($scope.model.field.relid === expression.getSourceNode().getFollowRelationship().id() && expression.getField() && field.fid === expression.getField().id()) {
                                existingsColumns.push({ originalColumnId: reportColumn.id(), aggregateMethod: 'Show values' });
                            }
                        } else {
                            //if column expression is fieldExpression, only check sourcenode id is same as current selected node id, if the field is relationship field, make false
                            if (expression.getSourceNode() && $scope.model.selectedNode.nid === expression.getSourceNode().id() && expression.getField() && field.fid === expression.getField().id() && $scope.model.field.relid === 0) {
                                existingsColumns.push({ originalColumnId: reportColumn.id(), aggregateMethod: 'Show values' });
                            }
                        }
                    } else {
                        var aggregateExpression = expression.getAggregatedExpression();
                        var aggregateMethodAlias = expression.getAggregateMethod().getAlias ? expression.getAggregateMethod().getAlias() : expression.getAggregateMethod()._id._ns + ':' + expression.getAggregateMethod()._id._alias;
                        if (aggregateExpression.getSourceNode().getFollowRelationship() && field.relid > 0) {
                            if (aggregateExpression.getField() && field.relid === aggregateExpression.getSourceNode().getFollowRelationship().id() && field.fid === aggregateExpression.getField().id()) {
                                existingsColumns.push({ originalColumnId: reportColumn.id(), aggregateMethod: aggregateMethodAlias });
                            }
                        } else {
                            if (aggregateExpression.getField() && aggregateExpression.getSourceNode() && $scope.model.selectedNode.nid === aggregateExpression.getSourceNode().id() && aggregateExpression.getField() &&  field.fid === aggregateExpression.getField().id() && field.relid === 0) {
                                existingsColumns.push({ originalColumnId: reportColumn.id(), aggregateMethod: aggregateMethodAlias });
                            }
                        }
                    }
                });
                $scope.preSummarise = existingsColumns;
                
                $scope.model.aggregateMethodOptions = $scope.buildAggregateMethodOptions(existingsColumns);
            };

            $scope.buildAggregateMethodOptions = function (existingsColumns) {
                var aggregateMethodOptions = [];

                var aggregateMethodOption = { name: '', isChecked: false };

                aggregateMethodOption = $scope.buildAggregateMethodOption('Show values', existingsColumns);

                aggregateMethodOptions.push(aggregateMethodOption);

                 aggregateMethodOption = $scope.buildAggregateMethodOption('Count', existingsColumns);
                 aggregateMethodOptions.push(aggregateMethodOption);

                 if ($scope.model.field.ftype === 'Bool' || $scope.model.field.ftype === 'Boolean') {
                     //TODO for boolean field, don't show Count Unique option for not
                 } else {
                     aggregateMethodOption = $scope.buildAggregateMethodOption('Count unique', existingsColumns);
                     aggregateMethodOptions.push(aggregateMethodOption);
                 }
            
                aggregateMethodOption = $scope.buildAggregateMethodOption('Count all', existingsColumns);
                aggregateMethodOptions.push(aggregateMethodOption);


                switch ($scope.model.field.ftype) {
                    case 'DateTime':
                    case 'Time':
                    case 'Date':
                    case 'ChoiceRelationship':
                        aggregateMethodOption = $scope.buildAggregateMethodOption('Max', existingsColumns);
                        aggregateMethodOptions.push(aggregateMethodOption);
                        aggregateMethodOption = $scope.buildAggregateMethodOption('Min', existingsColumns);
                        aggregateMethodOptions.push(aggregateMethodOption);
                        break;
                    case 'AutoNumber':
                    case 'Number':
                    case 'Int32':
                    case 'Decimal':
                    case 'Currency':
                        aggregateMethodOption = $scope.buildAggregateMethodOption('Sum', existingsColumns);
                        aggregateMethodOptions.push(aggregateMethodOption);
                        aggregateMethodOption = $scope.buildAggregateMethodOption('Average', existingsColumns);
                        aggregateMethodOptions.push(aggregateMethodOption);                        
                        aggregateMethodOption = $scope.buildAggregateMethodOption('Max', existingsColumns);
                        aggregateMethodOptions.push(aggregateMethodOption);
                        aggregateMethodOption = $scope.buildAggregateMethodOption('Min', existingsColumns);
                        aggregateMethodOptions.push(aggregateMethodOption);
                        break;
                }

               
                

                switch ($scope.model.field.ftype) {
                    case 'String':
                    case 'ChoiceRelationship':
                        aggregateMethodOption = $scope.buildAggregateMethodOption('List', existingsColumns);
                        aggregateMethodOptions.push(aggregateMethodOption);
                        break;
                }


                return aggregateMethodOptions;
            };


            $scope.getAggregateMethodAlias = function(aggregateOptionName) {

                if (aggregateOptionName.toLowerCase() === 'count all')
                    return 'core:aggCount';
                else if (aggregateOptionName.toLowerCase() === 'count')
                    return 'core:aggCountWithValues';
                else if (aggregateOptionName.toLowerCase() === 'count unique')
                    return 'core:aggCountUniqueItems';
                else
                    return 'core:agg' + aggregateOptionName.replace(/ /g, '');

            };


            $scope.buildAggregateMethodOption = function (aggregateOptionName, existingsColumns) {

                var aggregateMethodOption;
                var aggregateOptionConvertName = aggregateOptionName === 'Show values' ? aggregateOptionName : $scope.getAggregateMethodAlias(aggregateOptionName);

                var existColumn = _.find(existingsColumns, function(column) { return column.aggregateMethod === aggregateOptionConvertName; });
                if (existColumn) {
                    aggregateMethodOption = { name: aggregateOptionName, isChecked: true, columnId: existColumn.originalColumnId };
                } else {
                    aggregateMethodOption = { name: aggregateOptionName, isChecked: false, columnId: -1};
                }

                return aggregateMethodOption;

            };

            $scope.clickAggregate = function (option) {
                if (option) {
                    var addAction = { columnId: option.columnId, aggregateMethod: option.name, action: 'add', originalColumnId: option.columnId };
                    var removeAction = { columnId: option.columnId, aggregateMethod: option.name, action: 'remove', originalColumnId: option.columnId };
                    if (option.isChecked === false) {

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
                    }
                }
            };

            // click ok button to return calculated script
            $scope.ok = function () {
                if ($scope.summariseAction.length > 0 && $scope.preSummarise.length > 0) {
                    //filter all existing pre summarise column from action list e.g. user uncheck pre summarise column then check again
                    _.forEach($scope.preSummarise, function (preColumnAction) {
                        $scope.summariseAction = _.filter($scope.summariseAction, function (summarise) {
                            var aggregateOptionConvertName = summarise.aggregateMethod === 'Show values' ? summarise.aggregateMethod : $scope.getAggregateMethodAlias(summarise.aggregateMethod);

                            if (summarise.columnId === preColumnAction.columnId && aggregateOptionConvertName === preColumnAction.aggregateMethod && summarise.action === 'remove') {
                                summarise.originalColumnId = preColumnAction.originalColumnId;
                            }

                            return !(summarise.columnId === preColumnAction.columnId && aggregateOptionConvertName === preColumnAction.aggregateMethod && summarise.action === 'add');
                        });
                    });
                }


                var retResult = { summariseAction: $scope.summariseAction };


                $uibModalInstance.close(retResult);
            };

            // click cancel to return report builder
            $scope.cancel = function () {
                $uibModalInstance.close(null);
            };


        })
        .factory('spSummariseFieldOptionDialog', function (spDialogService) {
            // setup the dialog
            var exports = {

                showModalDialog: function (options, defaultOverrides) {
                    var dialogDefaults = {
                        title: 'Summarise Field Option',
                        keyboard: true,
                        backdropClick: true,
                        windowClass: 'modal summarisefieldoptiondialog-view',
                        templateUrl: 'reportBuilder/dialogs/summariseFieldOption/spSummariseFieldOption.tpl.html',
                        controller: 'spSummariseFieldOptionController',
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