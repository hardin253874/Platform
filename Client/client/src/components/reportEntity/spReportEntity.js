// Copyright 2011-2016 Global Software Innovation Pty Ltd
/**
 * Module for manipulating Software Platform Report entity object.
 * @namespace spReportEntity
 */
var spReportEntity;

(function (spReportEntity) {
    /**
     * Makes a request string for querying various report schema information.
     *
     * @param {object} options An object of options for what to include and exclude.
     *
     * @example
     * <pre>
     * var opts = {  // these are the defaults
     *      nodes: true,
     *      columns: true,
     *      orderBys: true,
     *      conditions: true,
     *      analyzerFields: true,
     *      columnFormats: true
     *      
     *  };
     * var rqString = spReportEntity.makeTypeRequest(opts);
     * </pre>
     *
     * @function
     * @name spReportEntity.makeReportRequest
     */
    spReportEntity.makeReportRequest = function makeReportRequest(options) {

        var opts = _.defaults(options || {}, {            
            nodes: true,
            columns: true,
            orderBys: true,
            conditions: true,            
            columnFormatingRule: true,
            columnDisplayFormating: true,
            clientAggregate: true,  //TODO switch to true when Martin finish all clientAggregate function on server side
            XMLFields: false,
            reportInfo: true
        });

        var variables = '';

        variables += 'let @NA = { name, alias } ';
        variables += 'let @NAMETYPE = { name, isOfType.@NA } ';
        
        variables += 'let @REPORTEXPRESULTTYPE = {' +
            '       name,alias,' +
            '       isOfType.@NA,' +
            '       numberDecimalPlaces,' +
            '       conformsToType.{name,isOfType.@NA},' +
            '       resourceArgumentValue.@NA,' +
            '       resourceListParameterValues.@NA,' +
            '       stringParameterValue, intParameterValue, decimalParameterValue, boolParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue' +
            '    } ';

        variables += 'let @EXPR = {' +
                    'isOfType.@NA,' +
                    'reportExpressionResultType.@REPORTEXPRESULTTYPE,' +
                    'sourceNode.id,' +
                    'fieldExpressionField.@NA,' +
                    'reportScript,' +
                    'expressionReferencesColumn.id,' +
                    'aggregateMethod.alias,' +
                    'parentGroupedBy.@NA,' +
                    'structureViewExpressionSourceNode.id,' +
                    'structureViewExpressionStructureView.id,' +
                    'aggregatedExpression*.{' +
                    '    isOfType.@NA,' +
                    '    reportExpressionResultType.@REPORTEXPRESULTTYPE,' +
                    '    sourceNode.id,' +
                    '    fieldExpressionField.id,' +
                    '    reportScript,' +
                    '    expressionReferencesColumn.id,' +
                    '    aggregateMethod.{name,alias,isOfType.@NA}' +
                    '}' +
                '} ';


        variables += 'let @CONDITION = {' +
                    'name,' +
                    'isOfType.@NA,' +
                    'conditionDisplayOrder,' +
                    'conditionIsHidden,' +
                    'conditionIsLocked,' +
                    'conditionExpression.@EXPR, ' +
                    'operator.{isOfType.alias,alias},' +
                    'columnForCondition.@NA,' +
                    'conditionParameterPicker.@NA,' +
                    'conditionParameter.{' +
                        'isOfType.@NA,' +
                        'paramTypeAndDefault.@REPORTEXPRESULTTYPE' + 
                     '}' +
                '} ';


        var ACTIONHOST = '{ k:resourceConsoleBehavior, k:selectionBehavior }' +
                                '.{ isOfType.@NA,' +
                                'k:behaviorActionMenu.{ ' +
                                '   isOfType.@NA,' +
                                '   k:showNewActionsButton, k:showExportActionsButton, k:showEditInlineActionsButton,' +
                                '   { k:menuItems, k:suppressedActions, k:includeActionsAsButtons }.{ ' +
                                '      { name, ' +
                                '        isOfType.@NA,' +
                                '        k:menuIconUrl, ' +
                                '        k:isActionButton, ' +
                                '        k:isMenuSeparator, ' +
                                '        { k:actionMenuItemToWorkflow,' +
                                '          k:actionMenuItemToReportTemplate }.{name,isOfType.@NA } ' +
								'      } ' +
                                '   }, ' +
                                '   { k:includeTypesForNewButtons, k:suppressedTypesForNewMenu }.id ' +
                                '}} ';

        var rq = '';
       
        if (opts.nodes) {
            rq += ', {rootNode,rootNode.relatedReportNodes*,rootNode.groupedNode*}.{' +
                      'name,' +
                      'isOfType.@NA,' +
                      'exactType,' +
                      'targetMustExist,' +
                      'targetNeedNotExist,' +
                      'resourceReportNodeType.@NAMETYPE,' +
                      'followInReverse,' +
                      'followRecursive,' +
                      'parentAggregatedNode.@NA,' +
                      'includeSelfInRecursive,' +
		              'constrainParent,' +
		              'checkExistenceOnly,' +
		              'followRelationship.{name, cardinality.alias, fromName, toName,{fromType, toType}.{name,alias,inherits.alias}},' +
                      'groupedBy.@EXPR' +
                  '} ';
        }

        if (opts.columns) {


            var columnFormattingRuleRq = '';
            if (opts.columnFormatingRule) {
                columnFormattingRuleRq += ', columnFormattingRule.{' +
                        'isOfType.@NA,' +
                        'barColor,' +
                        '{barMinValue, barMaxValue}.{isOfType.@NA, intParameterValue, decimalParameterValue, dateTimeParameterValue, timeParameterValue, dateParameterValue },' +
                        'iconRules.iconRuleImage.@NAMETYPE,' +
                        'iconRules.iconRuleCFIcon.@NAMETYPE,' +
                        'colorRules.{' +
                            'isOfType.@NA,' +
                            'colorRuleForeground,' +
                            'colorRuleBackground' +
                            '},' +
                        '{iconRules, colorRules}.{' +
                            'isOfType.@NA,' +
                            'rulePriority,' +
                            'ruleCondition.@CONDITION' +
                            '}' +
                    '}';
            }

            var columnDisplayFormatRq = '';
            if (opts.columnDisplayFormating) {
                columnDisplayFormatRq += ', columnDisplayFormat.{' +
                        'isOfType.@NA,' +
                        'columnShowText, ' +
                        //'columnFormatString,' +
                        '{ formatImageScale,' +
                        '  dateColumnFormat,' +
                        '  timeColumnFormat,' +
                        '  dateTimeColumnFormat,' +
			            '  formatAlignment,' +
                        '  formatImageSize' +
                        '}.alias,' +
                        'formatAlignment.{name,alias, isOfType.@NA},' +
                        'formatDecimalPlaces,' +
			            'formatPrefix,' +
			            'formatSuffix,' +
			            'maxLineCount' +
                    '}';
            }

            var clientAggregateRq = '';
            if (opts.clientAggregate) {
                clientAggregateRq += ', columnGrouping.{' +
                        'isOfType.@NA,' +
                        'groupingPriority,' +
                        'groupingCollapsed,' +
                        'groupingMethod.@NA' +
                    '}';
                clientAggregateRq += ', columnRollup.{' +
                        'isOfType.@NA,' +
                        'rollupMethod.@NA' +
                    '}';
            }
           
            rq += ', reportColumns.{' +
                      'name,' +
                      'isOfType.@NA,' +
                      'columnDisplayOrder,' +
                      'columnIsHidden,' +
                      'columnExpression.@EXPR ' +
                      columnFormattingRuleRq + columnDisplayFormatRq + clientAggregateRq +
                '} ';
        }

        if (opts.orderBys) {
            rq += ', reportOrderBys.{isOfType.@NA, reverseOrder, orderPriority, orderByExpression.@EXPR }';
        }

        if (opts.conditions) {           
            rq += ', hasConditions.@CONDITION';
        }

        if (opts.reportInfo) {
            rq += ', k:resourceInFolder.@NAMETYPE';
            rq += ', reportUsesDefinition.@NAMETYPE';
            rq += ', isDefaultDisplayReportForTypes.@NAMETYPE';
            rq += ', isDefaultPickerReportForTypes.@NAMETYPE';
            rq += ', k:navigationElementIcon.{name, isOfType.@NA},hideOnDesktop,hideOnTablet,hideOnMobile,isPrivatelyOwned';
            rq += ', inSolution.@NAMETYPE';
            rq += ', reportStyle.@NAMETYPE';
            rq += ', ' + ACTIONHOST;
            rq += ', resourceViewerConsoleForm.{name, alias, description, isOfType.@NA}';
        }

        var res = '';

        var xmlFields = '';
        if (opts.XMLFields) {
            xmlFields += ', analyzerXml, dataViewXml , queryXml';
        }

        var clientAggregateFields = '';
        if (opts.clientAggregate) {
            clientAggregateFields += ', rollupSubTotals,rollupGrandTotals, rollupRowCounts, rollupRowLabels, rollupOptionLabels';
        }

        res = variables + '{name, description, alias, isOfType.@NA, hideActionBar, hideReportHeader ' + clientAggregateFields + xmlFields + rq + '}';
        return res;
    };
    
    spReportEntity.cloneTypes = ['report', 'reportColumn', 'reportOrderBy', 'reportCondition',
          'reportNode', 'resourceReportNode', 'relationshipReportNode', 'derivedTypeReportNode', 'relationshipInstanceReportNode', 'aggregateReportNode', 
          'reportExpression', 'nodeExpression', 'idExpression', 'fieldExpression', 'resourceExpression', 'scriptExpression', 'columnReferenceExpression', 'aggregateExpression', 'structureViewExpression',
          'displayFormat', 'formattingRule', 'conditionBasedRule', 'barFormattingRule', 'iconFormattingRule', 'iconRule', 'colorFormattingRule', 'colorRule', 'imageFormattingRule',
          'formattingRule', 'formatImageSize', 'stringArgument', 'integerArgument', 'decimalArgument', 'currencyArgument', 'boolArgument', 'dateTimeArgument', 'timeArgument', 'parameter',
          'dateArgument', 'guidArgument', 'resourceArgument', 'resourceListArgument', 'reportRowGroup', 'reportRollup', 'consoleBehavior', 'actionMenu', 'actionMenuItem', 'generateDocumentActionMenuItem', 'workflowActionMenuItem'];
    
    spReportEntity.cloneRelationship = ['rootNode', 'relatedReportNodes', 'reportColumns', 'columnExpression', 'columnDisplayFormat', 'columnFormattingRule', 'followRelationship', 'resourceInFolder', 'resourceViewerConsoleForm', 'inSolution', 'parentAggregatedNode',
           'reportOrderBys', 'hasConditions', 'conditionExpression', 'operator', 'conditionParameter', 'columnForCondition', 'resourceReportNodeType', 'groupedBy', 'groupedNode', 'reportUsesDefinition', 'navigationElementIcon', 'formatAlignment',
           'ruleImageScale', 'formatImageScale', 'dateColumnFormat', 'timeColumnFormat', 'dateTimeColumnFormat', 'formatAlignment', 'formatImageSize', 'formattingRule', 'barMaxValue', 'iconRules', 'colorRules', 'barMinValue', 'ruleCondition', 'iconRuleImage', 'iconRuleCFIcon',
           'aggregateMethod', 'fieldExpressionField', 'sourceNode', 'reportExpressionResultType', 'expressionReferencesColumn', 'aggregatedExpression', 'columnRollup', 'rollupMethod', 'columnGrouping', 'groupingMethod', 'groupingPriority', 'groupingCollapsed', 'paramTypeAndDefault',
           'resourceListParameterValues', 'resourceArgumentValue','orderByExpression', 'cardinality', 'inherits', 'fromType', 'toType', 'conformsToType', 'reportStyle',
           'resourceConsoleBehavior', 'selectionBehavior', 'behaviorActionMenu', 'menuItems', 'suppressedActions', 'includeActionsAsButtons', 'actionMenuItemToWorkflow', 'actionMenuItemToReportTemplate', 'includeTypesForNewButtons', 'suppressedTypesForNewMenu', 'structureViewExpressionSourceNode', 'structureViewExpressionStructureView'];

    // Sets for fast lookup (we don't care what the value is, but as it happens, it's the same as the key)
    spReportEntity.cloneTypesSet = _.keyBy(spReportEntity.cloneTypes);
    spReportEntity.cloneRelationshipSet = _.keyBy(spReportEntity.cloneRelationship);

    /**
         * clone a reportentity from orignal report entity object,
         * cloneDeep the orignal reportentity first, then updated  report entities and report relationship instances
         *
         * @param {spEntity.Entity} entity The entity of report to clone from.
         * @returns {spEntity.Entity} the cloned report entity
         *
         * @function
         * @name spReportEntity#cloneReportEntity
         */
    spReportEntity.cloneReportEntity = function cloneReportEntity(reportEntity) {
        //cloneDeep reportentity
        var tempReportEntity = reportEntity.cloneDeep({ preserveFieldTracking: false, includeDeleted: false });
        
        var entities = spEntityUtils.walkEntities(tempReportEntity);
     
        var unUpdatedEntities = '';
        var unUpdatedRelationships = '';
        var debug = false;
        //update all entity with new id and set datastate to create
        _.forEach(entities, function (e) {
            if (e) {                
                var type = e.getType().getAlias();
                if (spReportEntity.cloneTypesSet[type]) {
                    if (e.getDataState() !== spEntity.DataStateEnum.Delete) {
                        e.setId(spEntity._getNextId());
                        e.setDataState(spEntity.DataStateEnum.Create);
                    }
                } else {
                    if (debug) {
                        unUpdatedEntities += 'type: ' + type + ' ,summary: ' + e.debugString + ";";
                    }
                }

                //update all relationship instance datastate to create
                _.forEach(entities, function(e) {
                    if (e) {
                        e._relationships.forEach(function(r) {                            
                            var relationshipType = r.id.getAlias();
                            if (spReportEntity.cloneRelationshipSet[relationshipType]) {                               
                                r.removeExisting = true;
                                r.getInstances().forEach(function (ri) {
                                    if (ri.getDataState() !== spEntity.DataStateEnum.Delete) {
                                        ri.setDataState(spEntity.DataStateEnum.Create);
                                    }                                    
                                });
                            } else {
                                if (debug && r) {
                                    unUpdatedRelationships += r.id._alias + "; ";
                                }
                            }
                        });
                    }
                });
            }
        });


        if (debug) {           
            console.log(unUpdatedEntities);
            console.log(unUpdatedRelationships);
        }
       
        return tempReportEntity;

    };
    
    spReportEntity.cloneReportEntityNugget = function cloneReportEntityNugget(reportEntity) {
        //cloneDeep reportentity
        var tempReportEntity = reportEntity.cloneDeep({ preserveFieldTracking: false });

        var entities = spEntityUtils.walkEntities(tempReportEntity);

        var deletedEntities = '';
        var unUpdatedEntities = '';
        var unUpdatedRelationships = '';
        var debug = false;
        //update all entity with new id and set datastate to create
        _.forEach(entities, function (e) {
            if (e) {
                var type = e.getType().getAlias();
                if (spReportEntity.cloneTypesSet[type]) {
                    if (e.getDataState() !== spEntity.DataStateEnum.Delete) {
                        e.setDataState(spEntity.DataStateEnum.Create);
                    }
                } else {
                    if (debug) {
                        unUpdatedEntities += 'type: ' + type + ' ,summary: ' + e.debugString + ";";
                    }
                }


                //update all relationship instance datastate to create
                _.forEach(entities, function(e) {
                    if (e) {
                        e._relationships.forEach(function (r) {
                            
                           
                            var relationshipType = r.id.getAlias();
                            if (spReportEntity.cloneRelationshipSet[relationshipType]) {
                                r.removeExisting = true;
                                r.getInstances().forEach(function (ri) {
                                    if (ri.getDataState() !== spEntity.DataStateEnum.Delete) {
                                        ri.setDataState(spEntity.DataStateEnum.Create);
                                    }
                                });
                            } else {
                                if (debug && r) {
                                    unUpdatedRelationships += r.id._alias + "; ";
                                }
                            }
                        });
                    }
                });
            }
        });
        
        if (debug) {
            console.log(deletedEntities);
            console.log(unUpdatedEntities);
            console.log(unUpdatedRelationships);
        }
        return tempReportEntity;

    };

})(spReportEntity || (spReportEntity = {}));

angular.module('mod.common.spReportEntity', ['ng', 'mod.common.spEntityService']);
angular.module('mod.common.spReportEntity').factory('spReportEntity', function () {
    'use strict';

    /**
     *  A set of APIs for manipulating report entity object.
     *  @module spReportEntity
     */
    var exports = _.clone(spReportEntity);
    return exports;

});
