// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp, spEntity, spReportEntityQueryManager, spReportEntity, jsonLookup,
  ReportEntityQueryManager, spReportPropertyDialog */

(function () {
    'use strict';

    angular.module('app.reportBuilder')
        .controller("reportBuilderPageController", ReportBuilderPageController);

    /* @ngInject */
    // ReSharper disable once InconsistentNaming
    function ReportBuilderPageController($scope, $stateParams, spNavService, reportBuilderService,
                                         spAlertsService, spReportPropertyDialog, spDialog,
                                         spEntityService, spReportService, spReportSaveAsDialog,
                                         titleService, navDirtyMessage, spNavDataService,
                                         spThemeService, spState) {

        titleService.setTitle('Report Builder');

        $scope.reporteid = $stateParams.eid;
        $scope.reportpath = $stateParams.path;
        $scope.reportName = '';
        $scope.reportDescription = '';
        $scope.folderId = 0;
        $scope.showTop = true;
        $scope.isSavingReport = false;
        $scope.reportModel = null;
        $scope.reportId = 0;
        $scope.reportBuilderOptions = {"reportEntity": null, "treeNode": null};
        $scope.reportBuilderAction = null;
        $scope.reportOptions = {"reportEntity": null, isEditMode: true, schemaOnly: false};
        $scope.reportBuilderTreeNode = null;
        $scope.spReportBuilderService = reportBuilderService;
        $scope.isSchemaOnlyMode = contains(_.keys($stateParams), "schemaOnly");
        $scope.reportBuilderModel = {
            reportEntity: null,
            reportBuilderTreeNode: null,
            reportOptions: {
                reportEntity: null,
                isEditMode: true,
                schemaOnly: $scope.isSchemaOnlyMode
            },
            gridBusyIndicator: {
                type: 'spinner',
                placement: 'element'
            }
        };

        // Get the current nav item & parent nav item
        $scope.currentNavItem = spNavService.getCurrentItem();
        $scope.parentNavItem = spNavService.getParentItem();

        $scope.isDirty = false;
        try {


            $scope.currentNavItem.isDirty = function () {
                return $scope.isDirty;
            };

            $scope.currentNavItem.dirtyMessage = function () {
                return navDirtyMessage.defaultMsg;
            };
        } catch (e) {

        }

        $scope.nav = spNavService;
        $scope.consoleThemeModel = {
            consoleTheme: null,
            titleStyle: {}
        };
        $scope.$watch('isDirty', function () {
            if ($scope.isDirty === true) {
                $scope.isSavingReport = false;
            }
        });

        $scope.$watch('nav.getThemes()', function (getThemesCompleted) {
            if (getThemesCompleted === true) {
                $scope.consoleThemeModel.titleStyle = spThemeService.getTitleStyle();
            }
        });


        // Selected definition
        $scope.reportPickerOptions = {
            selectedEntityId: 0,
            selectedEntity: null,
            entityTypeId: 'core:report'
        };

        $scope.$watch('reportPickerOptions.selectedEntity', function () {
            if ($scope.reportPickerOptions.selectedEntity) {
                $scope.reportId = $scope.reportPickerOptions.selectedEntity.id();
                //$scope.getReportModel($scope.reportId);
                $scope.getReportEntity($scope.reportId);
            }
        });

        $scope.$watch('reporteid', function () {
            $scope.reportPickerOptions = {
                selectedEntityId: $scope.reporteid,
                selectedEntity: null,
                entityTypeId: 'core:report'
            };

            $scope.reportId = $scope.reporteid;
            $scope.getReportEntity($scope.reportId);
            $scope.showTop = false;
        });

        $scope.$watch('spReportBuilderService.getActionsFromReportBuilder(false).length', function () {
            var actionsFromReportBuilder = $scope.spReportBuilderService.getActionsFromReportBuilder(true);
            if (!actionsFromReportBuilder) {
                return;
            }

            _.forEach(actionsFromReportBuilder, function (actionFromReportBuilder) {
                switch (actionFromReportBuilder.type) {
                    case "setAlert":
                        $scope.addAlert(actionFromReportBuilder.options.errorMessage, spAlertsService.sev.Warning, true);
                        break;
                    case "updateReportEntity":
                        $scope.updateReportEntity(actionFromReportBuilder.options.reportEntity);
                        break;
                }
            });
        });


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Entity
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        $scope.getReportEntity = function (rid) {

            //clear ReportBuilder Service
            $scope.spReportBuilderService.clearReport();
            $scope.spReportBuilderService.clearActionsFromReportBuilder();
            $scope.spReportBuilderService.clearActionsFromReport();
            $scope.reportBuilderModel.gridBusyIndicator.isBusy = true;
            var rq = spReportEntity.makeReportRequest();
            spEntityService.getEntity(rid, rq).then(function (reportEntity) {
                $scope.reportBuilderModel.gridBusyIndicator.isBusy = false;
                if (reportEntity) {
                    $scope.reportName = reportEntity.getName();
                    $scope.reportDescription = (reportEntity.getDescription && reportEntity.getDescription());
                    $scope.reportBuilderModel.reportEntity = new spReportEntity.Query(reportEntity);
                    $scope.initReportEntity();
                    $scope.isSavingReport = false;
                }
            }, function (error) {
                $scope.reportBuilderModel.gridBusyIndicator.isBusy = false;
                var errorMessage = 'An error occurred getting the report';
                if (error && error.data) {
                    errorMessage += ', ' + (error.data.ExceptionMessage || error.data.Message);
                }
                $scope.addAlert(errorMessage, spAlertsService.sev.Error, false);
            }).finally(function () {
            });
        };

        $scope.getReportEntityFromMode = function () {
            if (!$scope.reportBuilderModel.reportEntity) {
                $scope.reportBuilderModel.reportEntity = $scope.spReportBuilderService.getReportEntity();
            }

            return $scope.reportBuilderModel.reportEntity;
        };

        $scope.initReportEntity = function () {
            if ($scope.reportBuilderModel.reportEntity) {
                $scope.reportBuilderModel.reportOptions.schemaOnly = $scope.isSchemaOnlyMode;
                $scope.reportBuilderModel.reportOptions.reportEntity = $scope.reportBuilderModel.reportEntity.getEntity();                
                //$scope.reportBuilderModel.reportOptions.reportId = $scope.reportId;
                $scope.initializeTreeView();
                spReportEntityQueryManager.setEntity($scope.reportBuilderModel.reportEntity);
            }
        };

        //reset reportEntity from ReportEntityQueryManager
        $scope.resetReportEntity = function () {
            $scope.reportBuilderModel.reportEntity = ReportEntityQueryManager.ReportEntity;
            $scope.reportBuilderModel.reportOptions.schemaOnly = $scope.isSchemaOnlyMode;
            $scope.reportBuilderModel.reportOptions.reportEntity = $scope.reportBuilderModel.reportEntity;
            // $scope.reportBuilderModel.reportOptions.reportId = $scope.reportId;
        };

        //init the report tree then set the report entity option to treenode
        $scope.initializeTreeView = function () {
            if ($scope.reportBuilderModel.reportEntity) {
                //build TreeView
                $scope.reportBuilderModel.reportBuilderTreeNode = $scope.getTreeNode($scope.reportBuilderModel.reportEntity.getRootNode());
                //set report entity to report builder toolbox by reportbuilder service
                $scope.spReportBuilderService.setReportEntity($scope.reportId, $scope.reportBuilderModel.reportEntity, $scope.reportBuilderModel.reportBuilderTreeNode);
            }
        };

        //build init report treeview by current report entity report node structure
        $scope.getTreeNode = function (reportNode, parentReportNode, parentAggregateReportNode) {

            if (reportNode && reportNode.getType()) {

                var children = [];
                var node = null;
                var nodeType = reportNode.getTypeAlias();
                switch (nodeType) {
                    case 'core:resourceReportNode':
                    case 'resourceReportNode':

                        children = [];
                        _.each(reportNode.getRelatedReportNodes(), function (relReportNode) {
                            var resourceReportNodeChildNode = $scope.getTreeNode(relReportNode, reportNode, null);
                            if (resourceReportNodeChildNode)
                                children.push(resourceReportNodeChildNode);
                        });
                        children = sp.naturalSort(children, 'name'); // _.sortBy(children, function (child) { return child.name; });
                        node = {
                            name: reportNode.getResourceReportNodeType().name,
                            nid: reportNode.id(),
                            etid: reportNode.getResourceReportNodeType() ? reportNode.getResourceReportNodeType().id() : 0,
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
                            cols: [],
                            followInReverse: false
                        };
                        break;

                    case 'core:aggregateReportNode':
                    case 'aggregateReportNode':
                        node = $scope.getTreeNode(reportNode.getGroupedNode(), parentReportNode, reportNode);
                        break;

                    case 'core:relationshipReportNode':
                    case 'relationshipReportNode':
                        if (reportNode.getFollowRelationship()) {
                            children = [];
                            _.each(reportNode.getRelatedReportNodes(), function (relReportNode) {
                                var relReportNodeChildNode = $scope.getTreeNode(relReportNode, reportNode, null);
                                if (relReportNodeChildNode)
                                    children.push(relReportNodeChildNode);
                            });
                            children = sp.naturalSort(children, 'name'); // _.sortBy(children, function (child) { return child.name; });

                            var relationshipType = '';
                            if (reportNode.getFollowRelationship().isChoiceField() || reportNode.getFollowRelationship().isLookup()) {
                                relationshipType = 'lookups';
                            } else {
                                relationshipType = 'relationships';
                            }

                            node = {
                                name: reportNode.getFollowRelationship().getName(), //reportNode.getResourceReportNodeType().name,
                                nid: reportNode.id(),
                                etid: reportNode.getResourceReportNodeType() ? reportNode.getResourceReportNodeType().id() : 0,
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
                                cols: [],
                                followInReverse: reportNode.getFollowRelationship().isReverse()
                            };
                        } else {
                            node = null;
                        }
                        break;


                    case 'core:derivedTypeReportNode':
                    case 'derivedTypeReportNode':
                        children = [];
                        _.each(reportNode.getRelatedReportNodes(), function (relReportNode) {
                            var derivedReportNodeChildNode = $scope.getTreeNode(relReportNode, reportNode, null);
                            if (derivedReportNodeChildNode)
                                children.push(derivedReportNodeChildNode);

                        });
                        children = sp.naturalSort(children, 'name'); // _.sortBy(children, function (child) { return child.name; });
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
                            cols: [],
                            followInReverse: false
                        };
                        break;


                    case 'core:relationshipInstanceReportNode':
                    case 'relationshipInstanceReportNode':

                        children = [];
                        _.each(reportNode.getRelatedReportNodes(), function (relReportNode) {
                            var relInstanceReportNodeChildNode = $scope.getTreeNode(relReportNode, reportNode, null);
                            if (relInstanceReportNodeChildNode)
                                children.push(relInstanceReportNodeChildNode);
                        });
                        children = sp.naturalSort(children, 'name'); // _.sortBy(children, function (child) { return child.name; });
                        node = {
                            name: reportNode.getResourceReportNodeType().name,
                            nid: reportNode.id(),
                            etid: reportNode.getResourceReportNodeType().id(),
                            pnid: parentReportNode.id(),
                            relid: 0,
                            nodetype: nodeType.replace('core:', ''),
                            reltype: 'relationshipInstance',
                            ftid: 0,
                            ttid: 0,
                            qe: reportNode,
                            pe: parentReportNode,
                            pae: parentAggregateReportNode,
                            children: children,
                            cols: [],
                            followInReverse: false
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

        $scope.reloadReportEntity = function () {
            if ($scope.reportId) {
                $scope.getReportEntity($scope.reportId);
            }
        };

        $scope.saveReportEntity = function () {
            if ($scope.reportBuilderModel.reportEntity && !$scope.isSavingReport) {
                $scope.isSavingReport = true;

                $scope.reportBuilderModel.gridBusyIndicator.isBusy = true;
                //cleanup the alert message on current page
                spAlertsService.removeAlertsWhere({page: 'reportBuilder'});


                //if invalid report informations exists, remove these objects before save
                if ($scope.reportBuilderModel.reportOptions &&
                    $scope.reportBuilderModel.reportOptions.invalidReportInfos &&
                    $scope.reportBuilderModel.reportOptions.invalidReportInfos.length > 0) {

                    //remove invalid nodes
                    _.forEach(_.filter($scope.reportBuilderModel.reportOptions.invalidReportInfos, {type: 'node'}), function (node) {
                        spReportEntityQueryManager.reomveRelationshipByNodeId($scope.reportBuilderModel.reportEntity, $scope.reportBuilderModel.reportEntity.getRootNode(), node.id);
                    });

                    //remove invalid columns
                    _.forEach(_.filter($scope.reportBuilderModel.reportOptions.invalidReportInfos, {type: 'column'}), function (column) {
                        spReportEntityQueryManager.removeReportColumnByColumnId($scope.reportBuilderModel.reportEntity, column.id);
                    });

                    //remove invalid conditions
                    _.forEach(_.filter($scope.reportBuilderModel.reportOptions.invalidReportInfos, {type: 'condition'}), function (condition) {
                        spReportEntityQueryManager.removeReportConditionByConditionId($scope.reportBuilderModel.reportEntity, condition.id);
                    });

                    $scope.reportBuilderModel.reportEntity = $scope.spReportBuilderService.getReportEntity();
                }


                spEntityService.putEntity($scope.reportBuilderModel.reportEntity.getEntity()).then(function (id) {
                    $scope.reportBuilderModel.gridBusyIndicator.isBusy = false;
                    $scope.isDirty = false;

                    // Only set the updatedReport flag if it is defined (by a parent report page for example)
                    // so we don't set the property into the nav item of other pages
                    // (this is my theory on this code - sg)
                    if ($scope.parentNavItem &&
                        $scope.parentNavItem.data && !sp.isNullOrUndefined($scope.parentNavItem.data.updatedReport)) {
                        $scope.parentNavItem.data.updatedReport = true;
                    }

                    var message = 'Report \'' + $scope.reportBuilderModel.reportEntity.getName() + '\' successfully saved.';

                    spReportService.clearReportDataCache(id);
                    $scope.addAlert(message, spAlertsService.sev.Success, true);
                    $scope.getReportEntity(id);
                    //spNavService.navigateToParent();
                }, function (error) {
                    $scope.isSavingReport = false;
                    $scope.reportBuilderModel.gridBusyIndicator.isBusy = false;
                    var errorMessage = 'An error occurred saving the report';
                    if (error && error.data) {
                        errorMessage += ', ' + (error.data.ExceptionMessage || error.data.Message);
                    }
                    $scope.addAlert(errorMessage, spAlertsService.sev.Error, false);
                }).finally(function () {
                });
            }
        };

        $scope.addAlert = function (message, severity, expires) {
            if (severity) {
                spAlertsService.addAlert(message, { expires: (expires || false), severity: severity });
            } else {
                spAlertsService.addAlert(message, { severity: spAlertsService.sev.Warning, expires: true });
            }
        };

        $scope.saveAsReportEntity = function () {
            if ($scope.reportBuilderModel.reportEntity) {
                var reportSaveAsOptions = {
                    reportEntity: $scope.reportBuilderModel.reportEntity
                };

                var container = spNavService.getCurrentItemContainer();
                if (container) {
                    reportSaveAsOptions.containerId = container.id;   
                }                

                spReportSaveAsDialog.showModalDialog(reportSaveAsOptions).then(function (result) {
                    //cleanup the alert message on current page
                    spAlertsService.removeAlertsWhere({page: 'reportBuilder'});
                    if (result) {
                        var newReportId = result.reportId;
                        var containerId = result.containerId;

                        $scope.isDirty = false;
                        if ($scope.parentNavItem.data && !sp.isNullOrUndefined($scope.parentNavItem.data.createNewReport)
                        ) {
                            $scope.parentNavItem.data.createNewReport = true;
                        }
                        else {
                            $scope.parentNavItem.data = _.extend(
                                $scope.parentNavItem.data || {},
                                {
                                    createNewReport: true
                                });
                        }
                        //refresh the navigation treebranch to make the new navitem appears
                        spNavService.requireRefreshTree();
                        spNavService.refreshTreeBranch(containerId ? { id: containerId } : $scope.currentNavItem).then(function() {
                            if (!containerId) {
                                return;
                            }

                            // Performing a save as to a new section
                            var newReportNode = spNavService.findInTreeById(spNavService.getNavTree(), newReportId);
                            if (newReportNode && sp.result(newReportNode, 'item.state.params')) {
                                spNavService.navigateToState('reportBuilder', newReportNode.item.state.params).then(function() {
                                    spState.getPageState().isSavingReportIntoNewContainer = true;
                                });
                            } else {
                                // Existing behaviour
                                spNavService.navigateToSibling('report', newReportId);
                            }
                        });

                        if (!containerId) {
                            spNavService.navigateToSibling('report', newReportId);
                        }
                    }
                });

            }
        };

        $scope.updateReportProperty = function () {
            var reportPropertyeOptions = {
                reportId: $scope.reportId,
                reportEntity: $scope.reportBuilderModel.reportEntity
            };
            spReportPropertyDialog.showModalDialog(reportPropertyeOptions).then(function (result) {
                if (result) {
                    if (result.reportEntity) {
                        $scope.reportBuilderModel.reportEntity = result.reportEntity;
                        $scope.reportName = $scope.reportBuilderModel.reportEntity.getName();
                        $scope.reportDescription = $scope.reportBuilderModel.reportEntity.getDescription();
                        $scope.isDirty = true;
                    }

                }
            });
        };
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Rename Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /**
         * Stops certain characters from being entered into the editable labels.
         */
        $scope.validateinput = function (evt) {
            var e = evt || event;

            if (e.shiftKey) {
                switch (e.which) {
                    case 188:
                    // <
                    case 190:
                        // >
                        e.stopPropagation();
                        e.preventDefault();
                        return false;
                }
            }

            return true;
        };

        /**
         * Change validation.
         */
        $scope.changeValidate = function (value) {

            if (value) {
                return value.replace(/[<>]+/g, '');
            }
            return value;
        };


        /**
         * A report has been renamed.
         */
        $scope.reportRenamed = function (value) {

            if (!value || !$scope.reportBuilderModel.reportEntity) {
                return;
            }
            $scope.reportBuilderModel.reportEntity.getEntity().name = value;
            $scope.reportName = value;

        };

        /**
         * Determines whether the specified report name is valid.
         */
        $scope.isValidReportName = function (newName, oldName) {


            if (newName && oldName && newName.toLowerCase() === oldName.toLowerCase()) {
                return true;
            }

            if (!newName) {              
                $scope.addAlert('Invalid report name specified.', spAlertsService.sev.Warning, true);              
                return false;
            }

            return true;
        };

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Report Build Common Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        $scope.close = function () {

            var report = $scope.reportBuilderModel.reportEntity;
            var currentNavItem = $scope.currentNavItem;
            var parentNavItem = $scope.parentNavItem;

            if (currentNavItem && !parentNavItem.noTreeRefresh) {

                currentNavItem.name = sp.result(report, 'getEntity.name') || "";
                currentNavItem.iconUrl = spNavDataService.getNavItemIconUrl(report.getEntity());

                //clean the parent navitem's params info, and replace by currentitem's params
                // (sg) why are we mucking with parent navItem? ... investigating...
                parentNavItem.state.params = null;

                //if the parent navitem is report reset hasAdHocSorting and hasAdHocAnalyzerConditions flag to false
                var reportModelManager = sp.result(parentNavItem, 'data.reportModelManager');
                if (reportModelManager) {
                    var model = reportModelManager.getModel();
                    if (model) {
                        model.hasAdHocSorting = false;
                        model.hasAdHocAnalyzerConditions = false;
                        if (model.analyzerOptions) {
                            //the analyzer options condition value should be reset to null when close from report build mode
                            model.analyzerOptions.conds = [];
                        }
                        //the existingAggregateRows should be empty array too
                        reportModelManager.setAggregateRows([]);
                    }
                }

                spNavService.requireRefreshTree();
                spNavService.refreshTreeBranch(currentNavItem);

            } else if (!_.isUndefined(parentNavItem.noTreeRefresh)) {
                parentNavItem.noTreeRefresh = false;
            }            
            spAlertsService.removeAlertsWhere({ page: 'reportBuilder' });

            var navParams;

            if (sp.result(currentNavItem, 'data.isSavingReportIntoNewContainer')) {
                // Report builder was loaded as a result of creating a new report via a save as into a new section
                delete currentNavItem.data.isSavingReportIntoNewContainer;
                navParams = sp.result(currentNavItem, 'state.params');
            }

            if (navParams) {
                spNavService.navigateToState('report', currentNavItem.state.params).then(function() {
                    spState.getPageState().syncNavTreeWithItem = true;
                });
            } else {
                spNavService.navigateToParent();
            }
        };

        $scope.navToReport = function () {
            try {
                spNavService.navigateToParent();
            }
            catch (e) {
                console.error('reportBuilder.js: unable to navigate back to the parent.');
            }
        };

        $scope.createReport = function () {

            var reportPropertyeOptions = {reportId: $scope.reportId, reportModel: $scope.reportModel};

            var reportProperty = spReportPropertyDialog.createDialog(reportPropertyeOptions);

            reportProperty.open();
        };


        $scope.updateReportEntity = function (reportEntity) {
            $scope.isDirty = true;
            $scope.reportBuilderModel.reportEntity = reportEntity;
            $scope.reportBuilderModel.reportOptions = {
                reportEntity: $scope.reportBuilderModel.reportEntity.getEntity(),
                isEditMode: true,
                reportEntityChanged: sp.newGuid(),
                schemaOnly: $scope.isSchemaOnlyMode
            };
        };

        $scope.updateReportModelFromReportBuilder = function () {

            $scope.reportBuilderModel.reportOptions.reportObject = $scope.reportModel;
            $scope.isDirty = true;
        };

        $scope.updateReportEntityFromReportBuilder = function () {

            if (!$scope.reportBuilderModel.reportEntity) {
                $scope.reportBuilderModel.reportEntity = $scope.getReportEntityFromMode();
            }

            $scope.reportBuilderModel.reportOptions = {
                reportEntity: $scope.reportBuilderModel.reportEntity.getEntity(),
                isEditMode: true,
                reportEntityChanged: sp.newGuid(),
                schemaOnly: $scope.isSchemaOnlyMode
            };
            $scope.spReportBuilderService.setReportEntity($scope.reportId, $scope.reportBuilderModel.reportEntity, null);

            $scope.isDirty = true;
        };        

        function contains(array, target) {
            if (array && target) {
                return (_.indexOf(target, array) > -1);
            } else {
                return false;
            }
        }
    }
}());
