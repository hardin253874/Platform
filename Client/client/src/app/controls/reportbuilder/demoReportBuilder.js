// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, spReportEntityQueryManager, spReportEntity */

(function () {
    'use strict';

    angular.module('app.demoReportBuilder', ['ui.router', 'spApps.reportServices', 'mod.common.ui.spTreeview', 'mod.common.ui.spFieldsManager', 'mod.common.ui.spRelationshipPicker', 'mod.common.ui.spEntityComboPicker', 'mod.common.spEntityService', 'mod.ui.spTreeviewManager', 'sp.navService', 'sp.spNavHelper'])


        .constant('ReportBuilderState', {
            name: 'reportBuilder',
            url: '/controls/reportbuilder',
            views: {
                'content@': {
                    controller: 'reportbuilderController',
                    templateUrl: 'controls/reportbuilder/demoReportbuilder.tpl.html'
                }

            }
        })

        .controller('reportbuilderController', function ($scope, spReportService, spRelationshipDialog, spEntityService,
                                                         $stateParams, spNavService) {
            var reportId;
            var reportModel;
            var reportSettingsJason;
            var reportQueryJason;
            var treeNodeJason;
            var treeFamily;
            var treeNode;

            var newnid;
            var roleList;
            var selectdnodeJason;
            var nodeFields;
            $scope.reportId = 0;
            $scope.nodeFields = [];
            $scope.selectedNode = null;
            $scope.selectedNodeId = '';
            $scope.selectedEntity = null;
            $scope.selectedEntityId = 0;
            $scope.selectedRelationshipIds = '';
            $scope.retRelationshipIds = '';
            $scope.nameFieldId = 0;


            $scope.reportPropertyMode = {
                reportName: '',
                reportDescription: '',
                rootEntityId: 0,
                disableOkButton: true,
                showAll: false,
                type: null
            };
            $scope.reportBuilderMode = {
                reportEntity: null,
                treeNode: null,
                selectTreeNode: null,
                selectTreeNodeId: 0,
                treeviewOptions: null,
                fileManagerOptions: null
            };

            // Selected definition
            $scope.reportPickerOptions = {
                selectedEntityId: 0,
                selectedEntity: null,
                entityTypeId: 'core:report'
            };


            //entity picker
            $scope.entityPickerOptions = {
                selectedEntityId: 0,
                selectedEntity: null,
                entityTypeId: 'core:definition'
            };

            //root entity picker
            $scope.rootEntityPickerOptions = {
                selectEntityId: 0,
                selectedEntity: null,
                entityTypeId: 'core:definition'
            };


            $scope.$watch('reportPickerOptions.selectedEntity', function () {
                if ($scope.reportPickerOptions.selectedEntity) {
                    $scope.reportId = $scope.reportPickerOptions.selectedEntity.id();
                    //$scope.getReportModel($scope.reportId);
                    //$scope.getReportEntity($scope.reportId);
                    spNavService.navigateToChildState('reportBuilder', $scope.reportId, $stateParams);
                }
            });

            $scope.$watch('entityPickerOptions.selectedEntity', function () {
                if ($scope.entityPickerOptions.selectedEntity) {
                    $scope.selectedEntity = $scope.entityPickerOptions.selectedEntity;
                    $scope.selectedEntityId = $scope.entityPickerOptions.selectedEntity.id();
                }
            });

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Get Report Entity  / Save Report Entity
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            $scope.getReportEntity = function (rid) {
                var rq = spReportEntity.makeReportRequest();
                spEntityService.getEntity(rid, rq).then(function (reportEntity) {
                    if (reportEntity) {
                        $scope.reportBuilderMode.reportEntity = new spReportEntity.Query(reportEntity);
                        $scope.initializeTreeView();
                    }
                });
            };

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Relationship Picker
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            $scope.pickRelationship = function () {
                if ($scope.selectedEntity) {

                    var ids = [];

                    if ($scope.selectedRelationshipIds) {
                        _.each($scope.selectedRelationshipIds.split(','), function (id) {
                            ids.push(spUtils.convertDbStringToNative('Int32', id));
                        });
                    }


                    var relationshipPickerOptions = {
                        entityTypeId: $scope.selectedEntityId,
                        selectedRelationshipIds: ids
                    };

                    spRelationshipDialog.showModalDialog(relationshipPickerOptions).then(function (result) {
                        if (result && result.retRelationshipIds) {
                            $scope.retRelationshipIds = result.retRelationshipIds.toString();
                        }
                    });

                }
            };


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Create Report / Report Property
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

            $scope.$watch('rootEntityPickerOptions', function () {
                if ($scope.nameFieldId === 0) {
                    spEntityService.getEntity('core:name', 'id', {hint: 'nameField'}).then(function (field) {
                        $scope.nameFieldId = field.id();
                    });
                }
            });

            $scope.$watch('rootEntityPickerOptions.selectedEntity', function () {
                if ($scope.rootEntityPickerOptions.selectedEntity) {
                    $scope.reportPropertyMode.selectedEntityId = $scope.rootEntityPickerOptions.selectedEntity.id();
                }
            });

            $scope.$watch('reportPropertyMode.showAll', function () {
                if ($scope.showAll) {
                    $scope.rootEntityPickerOptions = {
                        selectedEntityId: 0,
                        selectedEntity: null,
                        entityTypeId: 'core:type'
                    };
                } else {
                    $scope.rootEntityPickerOptions = {
                        selectedEntityId: 0,
                        selectedEntity: null,
                        entityTypeId: 'core:definition'
                    };
                }
            });

            $scope.$watch('reportPropertyMode.reportName', function () {
                $scope.disableOkButton();
            });

            $scope.$watch('reportPropertyMode.selectedEntityId', function () {

                spEntityService.getEntity($scope.reportPropertyMode.selectedEntityId, 'id,name').then(
                    function (type) {
                        $scope.reportPropertyMode.type = type;
                    }
                );

                $scope.disableOkButton();
            });

            $scope.disableOkButton = function () {
                if ($scope.reportPropertyMode.reportName && $scope.reportPropertyMode.reportName.length > 0 && $scope.reportPropertyMode.selectedEntityId && $scope.reportPropertyMode.selectedEntityId > 0) {
                    $scope.reportPropertyMode.disableOkButton = false;
                } else {
                    $scope.reportPropertyMode.disableOkButton = true;
                }
            };

            $scope.createReport = function () {
                var rootEntity = $scope.createRootEntity();
                var reportColumns = $scope.createReportColumns(rootEntity);
                var reportConditions = $scope.createReportConditions(rootEntity);
                var report = spEntity.fromJSON({
                    typeId: 'report',
                    name: jsonString($scope.reportPropertyMode.reportName),
                    description: jsonString($scope.reportPropertyMode.reportDescription),
                    rootNode: jsonLookup(rootEntity),
                    reportColumns: jsonRelationship(reportColumns),
                    hasConditions: jsonRelationship(reportConditions)
                });

                spEntityService.putEntity(report).then(function (id) {

                    window.alert(id);

                }, function (error) {
                    window.alert(error);
                })
                    .finally(function () {

                    });
            };

            $scope.createRootEntity = function () {
                var rootEntity = spEntity.fromJSON({
                    typeId: 'resourceReportNode',
                    exactType: false,
                    targetMustExist: false,
                    resourceReportNodeType: jsonLookup($scope.reportPropertyMode.selectedEntityId)
                });
                return rootEntity;
            };

            $scope.createReportColumns = function (rootEntity) {
                var idField = spEntity.fromJSON({
                    typeId: 'reportColumn',
                    columnDisplayOrder: 1,
                    columnIsHidden: true,
                    columnExpression: jsonLookup($scope.createIdExpression(rootEntity))
                });

                var nameField = spEntity.fromJSON({
                    typeId: 'reportColumn',
                    columnDisplayOrder: 2,
                    columnIsHidden: false,
                    columnExpression: jsonLookup($scope.CreateNameExpression(rootEntity))
                });

                return [idField, nameField];
            };

            $scope.createIdExpression = function (rootEntity) {
                return spEntity.fromJSON({
                        typeId: 'idExpression',
                        sourceNode: jsonLookup(rootEntity)
                    }
                );
            };

            $scope.CreateNameExpression = function (rootEntity) {
                return spEntity.fromJSON({
                        typeId: 'fieldExpression',
                        sourceNode: jsonLookup(rootEntity),
                        fieldExpressionField: jsonLookup($scope.nameFieldId)
                    }
                );
            };

            $scope.createReportConditions = function (rootEntity) {
                return [spEntity.fromJSON({
                    typeId: 'reportCondition',
                    conditionDisplayOrder: 1,
                    conditionIsHidden: false,
                    conditionIsLocked: false,
                    conditionExpression: jsonLookup($scope.CreateNameExpression(rootEntity))
                })];
            };


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Treeview from Report Entity
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            $scope.tree = [];
            $scope.selectedNodeId = 0;
            // initialize TreeView, reset the selectedNode and treviewoptions
            $scope.initializeTreeView = function () {
                if ($scope.reportBuilderMode && $scope.reportBuilderMode.reportEntity) {
                    //build TreeView
                    //var reportRootNode = $scope.reportBuilderMode.reportEntity.getRootNode();

                    $scope.reportBuilderMode.treeNode = $scope.getTreeNode($scope.reportBuilderMode.reportEntity.getRootNode());
                    $scope.reportBuilderMode.selectTreeNode = $scope.reportBuilderMode.treeNode;
                    $scope.reportBuilderMode.selectTreeNodeId = $scope.reportBuilderMode.selectTreeNode.nid;
                    $scope.setTreeViewOption($scope.reportBuilderMode.treeNode, $scope.reportBuilderMode.selectTreeNodeId);


                }
            };


            $scope.getTreeNode = function (reportNode, parentReportNode, parentAggregateReportNode) {

                if (reportNode) {

                    var children = [];
                    var node = null;
                    var nodeType = reportNode.getTypeAlias();
                    switch (nodeType) {
                        case 'core:resourceReportNode':


                            children = [];
                            _.each(reportNode.getRelatedReportNodes(), function (relReportNode) {
                                children.push($scope.getTreeNode(relReportNode, reportNode, null));
                            });

                            node = {
                                name: reportNode.getResourceReportNodeType().name,
                                nid: reportNode.id(),
                                etid: reportNode.getResourceReportNodeType().id(),
                                pnid: 0,
                                relid: 0,
                                nodetype: nodeType.replace('core:', ''),
                                reltype: nodeType.replace('core:', ''),
                                ftid: 0,
                                ttid: 0,
                                qe: reportNode,
                                pe: parentReportNode,
                                pae: parentAggregateReportNode,
                                children: children,
                                cols: []
                            };
                            break;

                        case 'core:aggregateReportNode':

                            node = $scope.getTreeNode(reportNode.getGroupedNode(), parentReportNode, reportNode);
                            break;

                        case 'core:relationshipReportNode':

                            children = [];
                            _.each(reportNode.getRelatedReportNodes(), function (relReportNode) {
                                children.push($scope.getTreeNode(relReportNode, reportNode, null));
                            });


                            var relationshipType = '';
                            if (reportNode.getFollowRelationship().isChoiceField() || reportNode.getFollowRelationship().isLookup()) {
                                relationshipType = 'lookups';
                            } else {
                                relationshipType = 'relationships';
                            }

                            node = {
                                name: reportNode.getResourceReportNodeType().name,
                                nid: reportNode.id(),
                                etid: reportNode.getResourceReportNodeType().id(),
                                pnid: parentReportNode.id(),
                                relid: reportNode.getFollowRelationship().getEntity().id(),
                                nodetype: nodeType.replace('core:', ''),
                                reltype: relationshipType,
                                ftid: reportNode.getFollowRelationship().getEntity().getFromType().id(),
                                ttid: reportNode.getFollowRelationship().getEntity().getToType().id(),
                                qe: reportNode,
                                pe: parentReportNode,
                                pae: parentAggregateReportNode,
                                children: children,
                                cols: []
                            };
                            break;


                        case 'core:derivedTypeReportNode':

                            children = [];
                            _.each(reportNode.getRelatedReportNodes(), function (relReportNode) {
                                children.push($scope.getTreeNode(relReportNode, reportNode, null));
                            });

                            node = {
                                name: reportNode.getResourceReportNodeType().name,
                                nid: reportNode.id(),
                                etid: reportNode.getResourceReportNodeType().id(),
                                pnid: parentReportNode.id(),
                                relid: 0,
                                nodetype: nodeType.replace('core:', ''),
                                reltype: 'derivedResources',
                                ftid: 0,
                                ttid: 0,
                                qe: reportNode,
                                pe: parentReportNode,
                                pae: parentAggregateReportNode,
                                children: children,
                                cols: []
                            };
                            break;

                        default:

                            node = null;
                            break;

                    }


                    return node;


                } else {
                    return null;
                }
            };

            // set the treeview options, (the whole treenode and selectednodeid)
            $scope.setTreeViewOption = function (treeNode, selectedNodeId) {
                //$scope.$apply(function () {
                $scope.reportBuilderMode.treeviewOptions = {"treeNodes": [treeNode], "selectedNodeId": selectedNodeId};
                //});
            };

            // set the fileManger options, node field collection, selectedNodeId and current reportentity
            $scope.setFileManagerOption = function (fields, selectedNodeId) {
                $scope.reportBuilderMode.fileManagerOptions = {
                    "nodeFields": fields,
                    "selectedNodeId": selectedNodeId,
                    "reportEntity": $scope.reportBuilderMode.reportEntity
                };
            };

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // File Manager from Report Entity
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
            $scope.$watch('reportBuilderMode.selectTreeNode', function () {

                if ($scope.reportBuilderMode.selectTreeNode) {
                    //the treeNode columns list exists
                    if ($scope.reportBuilderMode.selectTreeNode.cols && $scope.reportBuilderMode.selectTreeNode.cols.length > 0) {
                        // $scope.nodeFields = $scope.reportBuilderMode.selectTreeNode.cols;
                        $scope.setFileManagerOption($scope.reportBuilderMode.selectTreeNode.cols, $scope.reportBuilderMode.selectTreeNode.nid);
                    }
                    else {
                        //get from spResource model
                        spEntityService.getEntity($scope.reportBuilderMode.selectTreeNode.etid, spResource.makeTypeRequest()).then(function (typeEntity) {
                            $scope.getToTypeResourcePromise(new spResource.Type(typeEntity));
                        });
                    }
                }

            });

            //Get ToType Resource Promise to build fields collection of current type
            $scope.getToTypeResourcePromise = function (resource) {
                if (!resource) {
                    return;
                }

                var fields = [];

                resource.getFields().forEach(
                    function (field) {
                        fields.push($scope.getFieldJson(field, resource));
                    }
                );

                resource.getLookups().forEach(
                    function (relationship) {
                        if (!relationship.isHidden()) {
                            var fromType = relationship.getEntity().getFromType();
                            var toType = relationship.getEntity().getToType();
                            var group = relationship.getFieldGroupEntity() ? relationship.getFieldGroupEntity().getName() : 'Relationship Fields'; //Relationship Fields
                            var name = relationship.getName() ? relationship.getName() : toType.name;
                            fields.push({
                                "id": spUtils.newGuid(),
                                "fid": $scope.nameFieldId,
                                "fname": name,
                                "ftype": "InlineRelationship",
                                "isreq": false,
                                "group": group,
                                "svid": 0,
                                "relid": relationship.getEntity().eid().id(),
                                "ftid": fromType.eid().id(),
                                "ttid": toType.eid().id(),
                                "aggm": "",
                                "inrep": false,
                                "inanal": false
                            });
                        }
                    }
                );

                resource.getChoiceFields().forEach(
                    function (relationship) {
                        if (!relationship.isHidden()) {
                            var fromType = relationship.getEntity().getFromType();
                            var toType = relationship.getEntity().getToType();
                            var group = relationship.getFieldGroupEntity() ? relationship.getFieldGroupEntity().getName() : 'Relationship Fields'; //Relationship Fields
                            var name = relationship.getName() ? relationship.getName() : toType.name;
                            fields.push({
                                "id": spUtils.newGuid(),
                                "fid": $scope.nameFieldId,
                                "fname": name,
                                "ftype": "ChoiceRelationship",
                                "isreq": false,
                                "group": group,
                                "svid": 0,
                                "relid": relationship.getEntity().eid().id(),
                                "ftid": fromType.eid().id(),
                                "ttid": toType.eid().id(),
                                "aggm": "",
                                "inrep": false,
                                "inanal": false
                            });
                        }
                    }
                );

                $scope.reportBuilderMode.selectTreeNode.cols = fields;

                // $scope.nodeFields = $scope.reportBuilderMode.treeNode.cols;
                $scope.setFileManagerOption($scope.reportBuilderMode.selectTreeNode.cols, $scope.reportBuilderMode.selectTreeNode.nid);
            };

            $scope.getFieldJson = function (field, resource) {
                var fid, fname, ftype, group, displayName;
                fid = field.getEntity().eid().id();
                fname = field.getName();
                if (fname === "Name") {
                    displayName = resource.getName();
                }

                ftype = $scope.getDatabaseTypeDisplayName(field.getTypes()[0]);

                group = field.getFieldGroupEntity() ? field.getFieldGroupEntity().getName() : 'Default';

                return {
                    "id": spUtils.newGuid(),
                    "fid": fid,
                    "fname": fname,
                    "ftype": ftype,
                    "isreq": field.isRequired(),
                    "group": group,
                    "svid": 0,
                    "relid": 0,
                    "ftid": 0,
                    "ttid": 0,
                    "aggm": "",
                    "inrep": false,
                    "inanal": false,
                    "dname": displayName
                };
            };


            //get field type's database displayname
            $scope.getDatabaseTypeDisplayName = function (fieldType) {

                switch (fieldType.alias()) {
                    case 'core:stringField':
                        return "String";
                    case 'core:intField':
                        return "Int32";
                    case 'core:dateField':
                        return "Date";
                    case 'core:timeField':
                        return "Time";
                    case 'core:dateTimeField':
                        return "DateTime";
                    case 'core:decimalField':
                        return "Decimal";
                    case 'core:currencyField':
                        return "Currency";
                    case 'core:boolField':
                        return "Bool";
                    case 'core:autoNumberField':
                        return "AutoNumber";
                    default:
                        // return empty string
                        return "";
                }
            };
        });

}());

