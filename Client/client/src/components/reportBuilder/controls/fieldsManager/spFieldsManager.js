// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing a fields manager.
    * reportbuilder sturcture build control
    * 
    * @module spFieldsManager
    * @example
        
    Using the spFieldsManager:
    
    &lt;sp-field-manager node-fields=""&gt

   
    */
    angular.module('mod.common.ui.spFieldsManager', ['ui.router', 'ui.bootstrap', 'mod.ui.spTreeviewManager','app.controls.spSearchControl','mod.common.spTenantSettings'])
        .filter('range', function() {
            return function(input, min, max) {
                var range = [];
                min = parseInt(min,10); //Make string input int
                max = parseInt(max, 10);
                for (var i = min; i <= max; i++) {
                    if (input[i] !== undefined) {
                        range.push(input[i]);
                    }
                }
                return range;
            };
        })
        .directive('spFieldsManager', function ($compile, spReportBuilderService, $timeout, spTreeviewManager, spTenantSettings) {
        return {
            restrict: 'E', //Element
            scope: {
                nodeFields: '=',
                options: '='
            },            
            templateUrl: 'reportBuilder/controls/fieldsManager/spFieldsManager.tpl.html',
            link: function (scope, element, attrs) {
                var setActionsTimeoutPromise, nameFieldEntity;

                spTenantSettings.getNameFieldEntity().then(function (name) {
                    nameFieldEntity = name;
                });

                scope.actions = [];
                
                // Set the report builder actions
                function setActions() {
                    var actionsCopy = scope.actions;
                    scope.actions = [];
                    
                    _.forEach(actionsCopy, function (action) {
                        spReportBuilderService.setAction(action.type, null, action.field);
                    });                    
                }


                // Debounced setActions method
                function setActionsDebounced(timeout) {
                    // Cancel current timeout
                    if (setActionsTimeoutPromise) {
                        $timeout.cancel(setActionsTimeoutPromise);
                        setActionsTimeoutPromise = null;
                    }

                    // Reset the timeout
                    setActionsTimeoutPromise = $timeout(function () {
                        if (setActionsTimeoutPromise) {
                            setActionsTimeoutPromise = null;
                            setActions();
                        }
                    }, timeout);
                }


                // Watch for changes in the actions
                scope.$watch('actions.length', function () {
                    if (scope.actions.length) {
                        // Kick off an update
                        setActionsDebounced(400);
                    }
                });


                // Kick off an update
                scope.onMouseMove = function () {
                    if (scope.actions.length) {
                        setActionsDebounced(1000);
                    }
                };


                function addAction(type, field) {
                    // Find existing action
                    var existingAction = _.find(scope.actions, function (a) {
                        return a.type === type && a.field === field;
                    });

                    // No existing action found, so add new
                    if (!existingAction) {
                        scope.actions.push({
                            type: type,
                            field: field
                        });
                    }
                }


                scope.processReportColumnChanged = function (field, e) {
                    if (e.target.checked === true) {
                        field.inrep = true;
                        addAction('addColumnToReport', field);                        
                    } else {
                        
                        field.inrep = false;
                        addAction('removeColumnFromReport', field);
                    }
                };

                scope.processAnalyzerColumnChanged = function (field, e) {

                    if (e.target.checked === true) {
                        field.inanal = true;

                        addAction('addColumnToAnalyzer', field);                                             
                    } else {
                        field.inanal = false;

                        addAction('removeColumnFromAnalyzer', field);                        
                    }
                };

                scope.toggleChildren = function(index) {

                    if (scope.groupShowFields[index] === true)
                    {
                        scope.groupShowFields[index] = false;
                        scope.groupToggleImage[index] = 'assets/images/fieldgroup_collapsed.png';
                    }
                    else {
                        scope.groupShowFields[index] = true;
                        scope.groupToggleImage[index] = 'assets/images/fieldgroup_opened.png';
                    }

                };

                scope.getToggleImage = function(index) {
                    if (index > -1 && scope.groupToggleImage[index] !== undefined && scope.groupToggleImage[index] !== null) {
                        return scope.groupToggleImage[index];
                    } else {
                        return 'assets/images/fieldgroup_opened.png';
                    }

                };
                scope.model = scope.model || {};
                scope.model.search = {
                    value: null,
                    id: 'searchNodeFields'
                };
                scope.filteredNodeFields = [];
                scope.groupedNodeFields = [];
                scope.groupShowFields = [];
                scope.groupToggleImage = [];
                scope.reportEntity = null;
                scope.entityTypeId = 0;
                scope.isAggregateNode = false;
                scope.dragOptions = {
                    onDragStart: function () {

                    },
                    onDragEnd: function () {

                    }
                };
                               

                // Watch the search text to filter node fields by field name
                scope.$watch('model.search.value', function () {

                    //load filtered node fields
                    if (scope.options && scope.options.nodeFields && scope.options.nodeFields.length > 0) {
                        scope.filteredNodeFields = filterNodeFields(scope.options.nodeFields);
                        scope.loadNodeFields(scope.filteredNodeFields, scope.options.selectedNode);
                    }
                });


                function filterNodeFields(nodeFields) {
                    if (scope.model && scope.model.search && scope.model.search.value && scope.model.search.value.length > 0) {
                        return _.filter(nodeFields, function (nodeField) {
                            return (
                                nodeField.fname.toLowerCase().indexOf(scope.model.search.value.toLowerCase()) >= 0
                            );
                        });
                    } else {
                        return nodeFields;
                    }
                }


                scope.$watch('options.fileManagerChanged', function () {
                    if (scope.options && scope.options.nodeFields && scope.options.nodeFields.length > 0) {
                        scope.reportEntity = scope.options.reportEntity;
                        scope.entityTypeId = scope.options.entityTypeId;

                        var parentAggregateEntity = scope.getParentAggregateEntity(scope.options.selectedNode);

                        if (parentAggregateEntity) {
                            scope.isAggregateNode = true;
                        } else {
                            scope.isAggregateNode = false;
                        }
                        scope.filteredNodeFields = filterNodeFields(scope.options.nodeFields);
                        scope.loadNodeFields(scope.filteredNodeFields, scope.options.selectedNode);                        
                    }
                });

                scope.loadNodeFields = function (nodefields, selectedNode) {
                    // Reset
                    var groups = [];
                    var bringToTop = {
                        'Default': '_0',
                        'Name': ' _0',
                        'Description': ' _1'
                    };

                    _.each(nodefields, function (field) {

                        if (scope.reportEntity && field) {
                            // Update field's inanal && inrep property
                            field.inrep = scope.fieldInReport(field, selectedNode);
                            field.inanal = scope.fieldInAnalyzer(field, selectedNode);
                        }

                        field.isAggregated = scope.isAggregateNode;

                        //if current field is relationship field (lookup/choicefield), need double check current field's relationship node is aggregated or not
                        //it current node isAggregateNode, skip this check
                        if (scope.isAggregateNode === false && field.relid && field.relid > 0) {
                            field.isAggregated = scope.fieldRelationshipIsAggregated(field.relid);
                        }


                        var fieldGroup = field.group;
                        //as per the requirement for the task 21899, the field which in unallocated group should be set in default group
                        if (fieldGroup === 'Unallocated') {
                            fieldGroup = "Default";
                        }
                        // Find/create a group
                        var group = _.find(groups, { name: fieldGroup });
                        if (!group && fieldGroup !== 'Structure Views') {
                            group = { name: fieldGroup, fields: [] };
                            groups.push(group);
                        }

                        // Add the field to the group
                        group.fields.push(field);
                    });

                    // Sort groups
                    groups = sp.naturalSort(groups, function (group) {
                        return bringToTop[group.name] || group.name;
                    });

                    // Sort fields in groups
                    _.each(groups, function (group) {
                        group.fields = sp.naturalSort(group.fields, function (field) {
                            return bringToTop[field.fname] || field.fname;
                        });
                    });

                    scope.groupedNodeFields = groups;
                    scope.groupShowFields = _.map(groups, _.constant(true));
                    scope.groupToggleImage = _.map(groups, _.constant('assets/images/fieldgroup_opened.png'));
                };

                scope.fieldRelationshipIsAggregated = function (relationshipId)
                {
                    var isAggregatedField = false;

                    var getNodeAggregateEntity = scope.getNodeAggregateEntityByRelationship(relationshipId, scope.options.treeNode);

                    if (getNodeAggregateEntity) {
                        isAggregatedField = true;
                    }

                    return isAggregatedField;
                };


                scope.getNodeAggregateEntityByRelationship = function(relationshipId, treeNode) {

                    var nodeAggregateEntity = null;

                    if (treeNode.relid === relationshipId) {
                        nodeAggregateEntity = treeNode.pae;
                        return nodeAggregateEntity;
                    } else {
                        if (treeNode.children && treeNode.children.length > 0) {
                            _.each(treeNode.children, function(childNode) {
                                var childNodeAggregateEntity = scope.getNodeAggregateEntityByRelationship(relationshipId, childNode);
                                if (childNodeAggregateEntity) {
                                    nodeAggregateEntity = childNodeAggregateEntity;
                                }
                            });
                            return nodeAggregateEntity;

                        } else {
                            return null;
                        }
                    }

                };
             
                scope.fieldInReport = function(field, node) {

                    if (scope.reportEntity) {                       
                        var selectedColumns = _.find(scope.reportEntity.getReportColumns(), function (reportColumn) {

                            var columnExpression = reportColumn.getExpression();
                           
                            return columnExpression &&scope.compareExpression(columnExpression, field, node);
                           
                        });
                        
                        if (selectedColumns ) {
                            return true;
                        } else {
                            return false;
                        }
                    }
                    return false;
                };

                //get parent aggregate entity from current treeNode
                scope.getParentAggregateEntity = function (treeNode) {
                    var parentAggregateEntity = treeNode.pae;

                    if (parentAggregateEntity) {
                        return parentAggregateEntity;
                    } else {
                        // return 
                        return treeNode.pe ? scope.getParentAggregateEntity(spTreeviewManager.getNode(treeNode.pe.id(), scope.options.treeNode)) : null;
                    }

                };

                scope.compareExpression = function (exp, field, node) {
                    try {
                        if ( (exp.getTypeAlias() === 'core:fieldExpression' || exp.getTypeAlias() === 'fieldExpression') && field.relid === 0)
                        {
                            return exp.getSourceNode().id() === node.nid && exp.getField().id() === field.fid;
                        }
                        else if ((exp.getTypeAlias() === 'core:resourceExpression' || exp.getTypeAlias() === 'resourceExpression') && field.relid > 0) {

                            var relatedCurrentNode = _.find(node.qe.getRelatedReportNodes(), function(reportNode) {
                                if (reportNode.getGroupedNode && reportNode.getGroupedNode()) {
                                    return reportNode.getGroupedNode().id() === exp.getSourceNode().id() ||  reportNode.id() === exp.getSourceNode().id();
                                } else {
                                    return reportNode.id() === exp.getSourceNode().id();
                                }
                            });
                            var compareRelationship = scope.compareRelationship(exp, field);
                            if (!field.fid && compareRelationship && relatedCurrentNode) {                                
                                return true;
                            } 
                            else if (compareRelationship && relatedCurrentNode) {
                                return true;
                            } else {
                                return false;
                            }
                        }
                        else if ((exp.getTypeAlias() === 'core:resourceExpression' || exp.getTypeAlias() === 'resourceExpression') && field.relid === 0 && field.fname === 'Name') {
                            if (exp.getSourceNode().id() === node.nid) {
                                return true;
                            } else if (exp.getSourceNode().id() === node.nid) {
                                return true;
                            } else {
                                return false;
                            }
                        }
                        else if ((exp.getTypeAlias() === 'core:aggregateExpression' || exp.getTypeAlias() === 'aggregateExpression') && exp.getAggregatedExpression()) {
                            return scope.compareExpression(exp.getAggregatedExpression(), field, node);
                        }
                        //else if (exp.getIsOfType()[0].alias() === 'core:idExpression') {
                        //    return exp.getSourceNode().id() === nodeId;
                        //}
                        else if ((exp.getTypeAlias() === 'core:columnReferenceExpression' || exp.getTypeAlias() === 'columnReferenceExpression')) {

                            var columnExpression = exp.getExpressionReferencesColumn().getColumnExpression();
                            if (columnExpression) {
                                return scope.compareExpression(columnExpression, field, node);
                            } else {
                                return false;
                            }                            
                        }
                        else if ((exp.getTypeAlias() === 'core:structureViewExpression' || exp.getTypeAlias() === 'structureViewExpression')) {                            
                            var structureViewSourceNode = exp.getEntity().structureViewExpressionSourceNode;
                            var sourceNodeId, childNode;

                            if (structureViewSourceNode) {
                                sourceNodeId = structureViewSourceNode.id();
                                // Handle Name field of current node
                                if (sourceNodeId === node.nid &&
                                    field.fid === nameFieldEntity.id() &&
                                    field.ftype === 'String') {
                                    return true;
                                }

                                // Handle inline relationship
                                if (field.ftype === 'InlineRelationship') {
                                    childNode = _.find(node.children, function (n) {
                                        return n.nid === sourceNodeId && n.etid === field.ttid && n.reltype === 'lookups';
                                    });

                                    if (childNode) {
                                        return true;
                                    }
                                }                                
                            }

                            return false;
                        }
                        else {
                            return false;
                        }
                    } catch(e) {
                        return false;
                    }
                };


                scope.compareRelationship = function(expression, field) {
                    //add function to compare the relationship name and field name is match
                    //it is used for the fromType and toType of relationship is same, have double check the relationship is forward or reverse to 
                    //confirm the report column expression is matched field or not
                    var sameRelationshipName = true;

                    var relationshipName = expression.getSourceNode().getFollowRelationship().name;
                    try {
                        relationshipName = expression.getSourceNode().followInReverse ? expression.getSourceNode().getFollowRelationship().fromName : expression.getSourceNode().getFollowRelationship().toName;
                    } catch (ex) {
                        relationshipName = expression.getSourceNode().getFollowRelationship().name;
                    }

                    if (relationshipName) {
                        sameRelationshipName = relationshipName === field.fname;
                    }

                    return expression.getSourceNode().getFollowRelationship().id() === field.relid && sameRelationshipName;
                };

                scope.fieldInAnalyzer = function (field, nodeId) {

                    if (scope.reportEntity) {
                        var selectedConditions = _.find(scope.reportEntity.getReportConditions(), function (reportCondition) {
                            var conditionExpression = reportCondition.getExpression();

                            return conditionExpression &&  scope.compareExpression(conditionExpression, field, nodeId);
                           
                        });
                        
                        if (selectedConditions) {
                            return true;
                        } else {
                            return false;
                        }
                    }
                    return false;
                };


                scope.getGroupNameStyleClass = function(group) {
                    if (group.name === 'Default' || group.name === 'Unallocated')
                        return 'readonlygroupname';
                    else {
                        return 'groupname';
                    }
                };
                
                scope.getItemNameStyleClass = function (field) {
                    if (field.group === 'Default')
                        return 'readonlyitemname';
                    else {
                        return 'itemname';
                    }
                };

                scope.convertTypeToImageUrl = spUtils.convertTypeToImageUrl;
            }
        };
    });
}());