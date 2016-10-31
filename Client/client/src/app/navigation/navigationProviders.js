// Copyright 2011-2016 Global Software Innovation Pty Ltd

/*global _, console, angular, sp, spEntity, spReportEntity, spEntityUtils, jsonLookup, jsonRelationship */


(function () {
    'use strict';

    angular.module('mod.app.navigationProviders', [
        'sp.navService',
        'mod.app.reportProperty',
        'sp.common.spEntityHelper',
        'mod.common.spEntityService',
        'mod.common.alerts',
        'mod.app.navigation.appElementDialog',
        'mod.app.navigation.spNavigationElementDialog',
        'mod.app.chartBuilder.controllers.spNewChartDialog',
        'mod.common.ui.spEditFormDialog',
        'mod.common.ui.spChartService',
        'mod.common.ui.spDialogService',
        'mod.app.formBuilder.spNewTypeDialog',
        'mod.common.spXsrf',
        'sp.app.settings'
    ]);

    angular.module('mod.app.navigationProviders')
        .factory('spNavigationBuilderProvider', spNavigationBuilder);

    /* @ngInject */
    function spNavigationBuilder(spNavService, spNavDataService, spEntityHelper, $window, appElementDialog,
                                 spEntityService, $q, $document, $compile, spReportPropertyDialog, spAlertsService, spXsrf, spAppSettings, 
                                 spNavigationElementDialog, spNewChartDialog, spEditFormDialog, spChartService,
                                 spDialogService, spNewTypeDialog, $rootScope) {

        return function spNavigationBuilderOnScope(originalScope) {

            var exports = {};
            var scope = originalScope.$new();
            var fieldAliases = ['core:name', 'core:description',
                'core:hideOnDesktop',
                'core:hideOnTablet',
                'core:hideOnMobile',
                'console:navigationElementIcon',
                'core:inSolution',
                'core:boardShowQuickAdd' // boards
            ];
            var fieldIds = null;
            var currentDropTarget = null;
            var body = $document.find('body');
            var currentInsertionIndicatorPosition = null;
            var canDropInsertionIndicatorPosition = null;
            var insertIndicatorElement;
            var insertIndicatorScope;
            var insertionIndicatorPosition = {
                before: 'before',
                inside: 'inside',
                after: 'after'
            };

            function ensureMissingScopeFieldsAndMethodsExist() {
                scope.middleBusy = scope.middleBusy || {isBusy: false};
                scope.getSelectedTabNode = scope.getSelectedTabNode || angular.noop;
                scope.refreshNavTreeItems = scope.refreshNavTreeItems || angular.noop;
                scope.getSelectedMenuNode = scope.getSelectedMenuNode || angular.noop;
            }

            // Ensure scope fields and methods
            ensureMissingScopeFieldsAndMethodsExist();

            // Ensure that we clean up when the parent scope is destroyed
            originalScope.$on('$destroy', function () {
                scope.$destroy();
                destroyInsertIndicator();
            });


            // Setup drag and drop options for the drop indicator
            // This is required because drop indicator covers the
            // actual item when it is a box.
            scope.insertIndicatorDropOptions = {
                propagateDragEnter: false,
                propagateDragLeave: false,
                propagateDrop: false,
                propagateDragOver: false,
                simpleEventsOnly: true,

                onAllowDrop: function (source, target, dragData, dropData) {
                    return exports.allowDropItem(source, dropData.target, dragData, dropData.data);
                },
                onDrop: function (event, source, target, dragData, dropData) {
                    exports.dropItem(event, source, dropData.target, dragData, dropData.data);
                },
                onDragOver: function (event, source, target, dragData, dropData) {
                    exports.dragOverItem(event, source, dropData.target, dragData, dropData.data);
                },
                onDragLeave: function (event, source, target, dragData, dropData) {
                    exports.dragLeaveItem(event, source, dropData.target, dragData, dropData.data);
                }
            };


            // Set the drop data.
            scope.insertIndicatorDropData = {};


            // Find the nav node with the specified id
            function findNavNode(navItem, id) {
                var result = null;

                // Check if the current nav item is a match
                if (navItem &&
                    navItem.item &&
                    navItem.item.id === id) {
                    return navItem;
                }

                // Enumerate the children
                if (navItem.children) {
                    _.forEach(navItem.children, function (c) {
                        result = findNavNode(c, id);
                        return !result;
                    });
                }

                return result;
            }

            // Find the nav nodes with the specified id
            function findNavNodes(navItem, id) {
                var results = [];

                findNavNodesImpl(navItem, id, results);

                return results;
            }


            // Find the nav nodes with the specified id
            function findNavNodesImpl(navItem, id, results) {
                // Check if the current nav item is a match
                if (navItem &&
                    navItem.item &&
                    navItem.item.id === id) {
                    results.push(navItem);
                }

                // Enumerate the children
                if (navItem.children) {
                    _.forEach(navItem.children, function (c) {
                        findNavNodesImpl(c, id, results);
                    });
                }
            }


            // Gets the navItem for the specified entity and parentItem
            function getNavItemFromEntity(entity, parentItem) {
                var navItem;

                if (parentItem) {
                    // find the nav item by parent item
                    navItem = _.find(parentItem.children, function (childItem) {
                        return childItem.item.id === entity.id();
                    });
                } else {
                    // Find the nav item from the entity id
                    navItem = findNavNode(spNavService.getNavTree(), entity.id());
                }

                return navItem;
            }


            // Updates the navnode items belonging to the specified entity.
            function updateNavNodeItems(entity) {
                if (!entity) {
                    return;
                }

                var navNodes = findNavNodes(spNavService.getNavTree(), entity.id());

                if (!navNodes) {
                    return;
                }

                _.forEach(navNodes, function (navNode) {
                    if (navNode.item) {
                        navNode.item.name = entity.name;
                        navNode.item.entity = entity;
                    }
                });
            }


            // Get the id of the specified field by alias.
            function getFieldId(alias) {
                return fieldIds[alias].eid();
            }


            // Gets the parameters for a new tab given the specified parent nav item
            function getNewTabEntityParams(parentNavItem) {
                var maxTab = _.max(parentNavItem.children, function (cn) {
                    return cn.item.order;
                });

                return {
                    consoleOrder: maxTab && maxTab.item ? maxTab.item.order + 1 : 0,
                    inSolutionId: spNavService.getCurrentApplicationId(),
                    parentFolderId: parentNavItem.item.id
                };
            }


            //
            // Functions to create the navigation Types
            //

            // Tab
            function createNewTabEntity(params) {
                return spEntity.fromJSON(
                    _.chain(createNavElementFragment('New Tab', 'console:navSection', params))
                        .extend({'console:isAppTab': true})
                        .extend({'console:consoleOrder': params.consoleOrder})
                        .valueOf()
                );
            }

            // Nav Section
            function createNewNavSectionEntity(params) {
                return spEntity.fromJSON(
                    _.extend(
                        createNavElementFragment('New Section', 'console:navSection', params),
                        {'console:isAppTab': false}
                    )
                );
            }

            // Private Content Section
            function createNewPrivateContentSectionEntity(params) {
                return spEntity.fromJSON(
                    _.extend(
                        createNavElementFragment('New Personal Section', 'console:privateContentSection', params)                        
                    )
                );
            }

            // Board
            function createNewBoardEntity(params) {
                var json = _.chain(entityFragment('New Board', 'core:board'))
                    .extend({
                        'core:boardColumnDimension': jsonLookup(spEntity.fromJSON({ typeId: 'core:boardDimension' })),
                        'core:boardSwimlaneDimension': jsonLookup(spEntity.fromJSON({ typeId: 'core:boardDimension' })),
                        'core:boardStyleDimension': jsonLookup(spEntity.fromJSON({ typeId: 'core:boardDimension' }))
                    })
                    .extend(folderFragment(params))
                    .extend(mobileFragment())
                    .extend(insolutionFragment(params))
                    .valueOf();

                var entity = spEntity.fromJSON(json);

                entity.setField(getFieldId('core:boardShowQuickAdd'), true, spEntity.DataType.Bool);

                return entity;
            }
            
            var createNewDocumentFolderEntity = _.partial(createNavElement, 'New Document Folder', 'core:documentFolder');
            var createNewScreenEntity = _.partial(createNavElement, 'New Screen', 'console:screen');
            
            // Application (THIS IS ONLY USED FROM THE APPLICATIONS MENUE, NOT FROM THE APPLICATIONS REPORT)
            function createNewApplicationEntity() {
                var entity = spEntity.fromJSON(
                    _.chain(entityFragment('New Application', 'core:solution'))
                        .extend(mobileFragment())
                        .extend({'core:solutionVersionString': '1.0'})
                        .valueOf()
                );

                return entity;
            }
            
            function createNavElement(name, typeid, params) {
                return spEntity.fromJSON(createNavElementFragment(name, typeid, params));
            }

            function createNavElementFragment(name, typeid, params) {
                var result =
                    _.chain(entityFragment(name, typeid))
                        .extend(folderFragment(params))
                        .extend(mobileFragment())
                        .extend(insolutionFragment(params))
                        .extend(privateFragment(typeid))
                        .valueOf();
                return result;
            }

            function entityFragment(name, typeid) {
                return {
                    typeId: typeid,
                    'console:navigationElementIcon': jsonLookup(),
                    'core:name': name,
                    'core:description': ''
                };
            }

            function folderFragment(params) {
                return {
                    'console:resourceInFolder': jsonRelationship([params.parentFolderId])
                };
            }

            function insolutionFragment(params) {
                return {
                    'core:inSolution': jsonLookup(params.inSolutionId)
                };
            }

            function privateFragment(typeid) {
                if (typeid !== 'console:screen')
                    return {};
                return {
                    isPrivatelyOwned: !spAppSettings.publicByDefault
                };
            }

            function mobileFragment() {
                return {
                    'core:hideOnDesktop': false,
                    'core:hideOnTablet': true,
                    'core:hideOnMobile': true
                };
            }

            // Show the new tab entity dialog.
            function showNewTabEntityDialog(parentNavItem) {
                ensureFieldIds().then(function () {
                    var options = {
                        title: 'New Tab',
                        entity: createNewTabEntity(getNewTabEntityParams(parentNavItem))
                    };

                    return showNavigationElementDialog(options).then(function (result) {
                        if (!result) {
                            return;
                        }

                        var navItem = spNavDataService.navItemFromEntity(result.entity);

                        // Create a new nav item from the entity
                        parentNavItem.children.push({
                            item: navItem,
                            parent: parentNavItem,
                            children: []
                        });

                        navItem.href = spNavService.makeLink(navItem);
                    });
                });
            }


            // Get the navigation element icon from the specified entity
            function getNavigationElementIcon(entity) {
                if (!entity.navigationElementIcon) {
                    return null;
                }

                if (_.isArray(entity.navigationElementIcon)) {
                    return _.first(entity.navigationElementIcon);
                }

                return entity.navigationElementIcon;
            }


            // Show the dialog to create a new application.
            // Note: the navigationElement dialog is used.
            function showNewApplicationEntityDialog() {
                ensureFieldIds().then(function () {
                    var navigationElementIcon,
                        options = {
                            title: 'New Application',
                            entity: createNewApplicationEntity(),
                            onBeforeEntitySave: function (appEntity) {
                                // Save the icon id as set in the dialog and return a new application entity with just name and description
                                // that the dialog will save.
                                // Saving the application will result in a topMenu and the
                                // required nav sections to be created
                                navigationElementIcon = getNavigationElementIcon(appEntity);

                                return appEntity;
                            }
                        };

                    return showApplicationDialog(options).then(function (result) {
                        if (!result) {
                            return;
                        }

                        // Get the nav tree and update the topMenu's icon
                        spNavService.requestInitialNavTree().then(function () {
                            var topMenu = _.find(spNavService.getNavTree().children, function (c) {
                                return c.item &&
                                    c.item.typeAlias === 'console:topMenu' &&
                                    c.item.applicationId === result.entity.id();
                            });

                            if (!topMenu || !topMenu.item) {
                                // TopMenu item was not found
                                scope.middleBusy.isBusy = false;
                                return;
                            }

                            if (!navigationElementIcon) {
                                scope.middleBusy.isBusy = false;
                                if (topMenu.children &&
                                    topMenu.children.length &&
                                    topMenu.children[0].item &&
                                    topMenu.children[0].item.state) {
                                    spNavService.navigateToState(topMenu.children[0].item.viewType, topMenu.children[0].item.state.params);
                                }
                                return;
                            }

                            // Update the icon for the topMenu
                            var topMenuEntity = spEntity.fromJSON({
                                id: topMenu.item.id,
                                'console:navigationElementIcon': jsonLookup(navigationElementIcon ? navigationElementIcon.id() : null)
                            });

                            topMenuEntity.setDataState(spEntity.DataStateEnum.Update);

                            spEntityService.putEntity(topMenuEntity).then(function () {
                                scope.middleBusy.isBusy = false;

                                topMenu.item.entity.setLookup(getFieldId('console:navigationElementIcon'), navigationElementIcon ? navigationElementIcon.id() : null);

                                if (topMenu.children &&
                                    topMenu.children.length &&
                                    topMenu.children[0].item &&
                                    topMenu.children[0].item.state) {
                                    spNavService.navigateToState(topMenu.children[0].item.viewType, topMenu.children[0].item.state.params);
                                }
                            }, function (error) {
                                spAlertsService.addAlert('Failed to save top menu ' + topMenu.item.name + 'icon. ' + (error.data.ExceptionMessage || error.data.Message), {
                                    severity: spAlertsService.sev.Error,
                                    expires: true
                                });
                                scope.middleBusy.isBusy = false;
                            });
                        }, function () {
                            scope.middleBusy.isBusy = false;
                        });
                    });
                });
            }


            // Get the parent target node
            function getParentTargetNode(targetNode, position) {
                var targetNodeItem = null;

                switch (position) {
                    case insertionIndicatorPosition.before:
                    case insertionIndicatorPosition.after:
                        targetNodeItem = targetNode.parent;
                        break;
                    case insertionIndicatorPosition.inside:
                        targetNodeItem = targetNode;
                        break;
                }

                return targetNodeItem;
            }


            // Show the new section entity dialog.
            function showNewNavSectionEntityDialog(sourceNode, targetNode, position) {
                ensureFieldIds().then(function () {
                    var targetNodeItem = getParentTargetNode(targetNode, position),
                        options = {
                            title: 'New Section',
                            entity: createNewNavSectionEntity({
                                inSolutionId: spNavService.getCurrentApplicationId(),
                                parentFolderId: targetNodeItem.item.id
                            })
                        };

                    return showNavigationElementDialog(options).then(function (result) {
                        if (!result) {
                            return;
                        }

                        var newSourceNode = {
                            item: spNavDataService.navItemFromEntity(result.entity),
                            children: [],
                            parent: targetNodeItem
                        };

                        moveItem(newSourceNode, targetNode, position, true, true);
                    });
                });
            }                    

            // Show the new private content folder entity dialog.
            function showNewPrivateContentSectionEntityDialog(sourceNode, targetNode, position) {
                ensureFieldIds().then(function () {
                    var targetNodeItem = getParentTargetNode(targetNode, position),
                        options = {
                            title: 'New Personal Section',
                            entity: createNewPrivateContentSectionEntity({
                                inSolutionId: spNavService.getCurrentApplicationId(),
                                parentFolderId: targetNodeItem.item.id
                            })
                        };

                    return showNavigationElementDialog(options).then(function (result) {
                        if (!result) {
                            return;
                        }                        

                        var newSourceNode = {
                            item: spNavDataService.navItemFromEntity(result.entity),
                            children: [],
                            parent: targetNodeItem
                        };                                                

                        moveItem(newSourceNode, targetNode, position, true, true);
                    });
                });
            }

            // Show the new document folder entity dialog.
            function showNewDocumentFolderEntityDialog(sourceNode, targetNode, position) {
                ensureFieldIds().then(function () {
                    var targetNodeItem = getParentTargetNode(targetNode, position),
                        options = {
                            title: 'New Document Folder',
                            entity: createNewDocumentFolderEntity({
                                inSolutionId: spNavService.getCurrentApplicationId(),
                                parentFolderId: targetNodeItem.item.id
                            })
                        };

                    return showNavigationElementDialog(options).then(function (result) {
                        if (!result) {
                            return;
                        }

                        var newSourceNode = {
                            item: spNavDataService.navItemFromEntity(result.entity),
                            children: [],
                            parent: targetNodeItem
                        };

                        moveItem(newSourceNode, targetNode, position, true);
                    });
                });
            }

            // Show the new screen entity dialog.
            function showNewScreenEntityDialog(sourceNode, targetNode, position) {
                ensureFieldIds().then(function () {
                    var targetNodeItem = getParentTargetNode(targetNode, position),
                        options = {
                            title: 'New Screen',
                            entity: createNewScreenEntity({
                                inSolutionId: spNavService.getCurrentApplicationId(),
                                parentFolderId: targetNodeItem.item.id
                            })
                        };

                    return showNavigationElementDialog(options).then(function (result) {
                        if (!result) {
                            return;
                        }

                        var newSourceNode = {
                            item: spNavDataService.navItemFromEntity(result.entity),
                            children: [],
                            parent: targetNodeItem
                        };

                        var parentItem = getParentTargetNode(targetNode, position);
                        var isTopItem = isTopNavItem(parentItem) ? true : false;
                        moveItem(newSourceNode, targetNode, position, true, true).then(function () {
                            navigateToBuilder('screen', 'screenBuilder', newSourceNode.item.id, isTopItem);                            
                        });
                    });
                });
            }


            // Show the new report entity dialog
            function showNewReportEntityDialog(sourceNode, targetNode, position) {
                var targetNodeItem = getParentTargetNode(targetNode, position),
                    options = {
                        folder: targetNodeItem.item.id,
                        solution: spNavService.getCurrentApplicationId()
                    };

                showReportPropertyDialog(options).then(function (result) {
                    if (!result) {
                        return;
                    }

                    result.report.setId(result.reportId);
                    result.report.markAllUnchanged();

                    var newSourceNode = {
                        item: spNavDataService.navItemFromEntity(result.report),
                        children: [],
                        parent: targetNodeItem
                    };

                    
                    var parentItem = getParentTargetNode(targetNode, position);
                    var isTopItem = isTopNavItem(parentItem) ? true : false;
                    // Navigate the report builder
                    moveItem(newSourceNode, targetNode, position, true, true).then(function () {
                        navigateToBuilder('report', 'reportBuilder', newSourceNode.item.id, isTopItem);                      
                    });
                });
            }

            function isTopNavItem(parentItem) {
                if (!parentItem || !parentItem.parent || !parentItem.parent.item)
                    return false;

                return parentItem.parent.item.alias === 'core:homeMenu' || parentItem.parent.item.viewType === 'home';
            }


            function navigateToBuilder(type, buildMode, itemId, isTopItem) {
                if (isTopItem === true) {
                    spNavService.navigateToChildState(type, itemId).finally(function () {
                        spNavService.navigateToChildState(buildMode, itemId);
                    });
                } else {
                    spNavService.navigateToSibling(type, itemId).finally(function () {
                        spNavService.navigateToChildState(buildMode, itemId);
                    });
                }
            }


            // Show the new chart entity dialog
            function showNewChartEntityDialog(sourceNode, targetNode, position) {
                var targetNodeItem = getParentTargetNode(targetNode, position),
                    options = {
                        folder: targetNodeItem.item.id,
                        solution: spNavService.getCurrentApplicationId(),
                        preventBuilderNavigation: true
                    };

                showNewChartPropertyDialog(options).then(function (result) {
                    if (!result) {
                        return;
                    }

                    result.chartEntity.setId(result.chartId);
                    result.chartEntity.markAllUnchanged();

                    var newSourceNode = {
                        item: spNavDataService.navItemFromEntity(result.chartEntity),
                        children: [],
                        parent: targetNodeItem
                    };

                    var parentItem = getParentTargetNode(targetNode, position);
                    var isTopItem = isTopNavItem(parentItem) ? true : false;
                    // Navigate to the chart builder
                    moveItem(newSourceNode, targetNode, position, true, true).then(function () {
                        navigateToBuilder('chart', 'chartBuilder', newSourceNode.item.id, isTopItem);                        
                    });
                });
            }

            function showNewBoardEntityDialog(sourceNode, targetNode, position) {
                var options = {
                    title: 'Board Properties',
                    form: 'console:boardPropertiesForm',
                    formMode: 'edit',
                    optionsEnabled: true,
                    saveEntity: false
                };
                return showNewEntityDialog(sourceNode, targetNode, position, options, createNewBoardEntity).then(function(id) {
                    if (!id) {
                        return $q.when();
                    }
                    return spNavService.navigateToChildState('board', id);
                });
            }

            function showNewEntityDialog(sourceNode, targetNode, position, options, createFn) {
                return ensureFieldIds().then(function() {
                    var targetNodeItem = getParentTargetNode(targetNode, position);

                    options.entity = createFn({
                        inSolutionId: spNavService.getCurrentApplicationId(),
                        parentFolderId: targetNodeItem.item.id
                    });

                    return showEntityDialog(options.entity, options).then(function (result) {
                        if (result !== true) {
                            return $q.when();
                        }

                        return spEntityService.putEntity(options.entity).then(function (id) {
                            options.entity.setId(id);
                            options.entity.markAllUnchanged();

                            var newSourceNode = {
                                item: spNavDataService.navItemFromEntity(options.entity),
                                children: [],
                                parent: targetNodeItem
                            };

                            return moveItem(newSourceNode, targetNode, position, true, true).then(function () {
                                return id;
                            });
                        });
                    });
                });
            }

            function showEntityDialog(entity, options) {
                if (!options.entity) {
                    return $q.when();
                }
                if (!options.saveEntity) {
                    return applyDialogChanges(entity, _.partial(spEditFormDialog.showDialog, options));
                }
                return spEditFormDialog.showDialog(options);
            }

            function applyDialogChanges(entity, callback) {
                var bm = entity.graph.history.addBookmark();
                var result = callback();
                var handleResult = function(r) {
                    if (r === false) {
                        bm.undo();
                    } else {
                        bm.endBookmark();
                    }
                    return r;
                };
                if (result && result.then) {
                    return $q.when(result).then(handleResult);
                } else {
                    handleResult(result);
                    return null;
                }
            }

            // Show the new object dialog
            function showNewDefinition(sourceNode, targetNode, position) {
                var targetNodeItem = getParentTargetNode(targetNode, position);
                var options = {
                    // callback to move the report to the correct location
                    moveItemCallback: function (report) {
                        // move to correct location
                        var newSourceNode = {
                            item: spNavDataService.navItemFromEntity(report),
                            children: [],
                            parent: targetNodeItem
                        };
                        return moveItem(newSourceNode, targetNode, position, true, true);
                    }
                };

                spNewTypeDialog.showDialog(options);
            }


            // Get the display type name
            function getDisplayTypeName(navItem, entity) {
                var typeAlias, isAppTab;

                if (!navItem) {
                    typeAlias = sp.result(entity, 'getType.alias');
                } else {
                    typeAlias = navItem.item.typeAlias;
                    isAppTab = navItem.item.isAppTab;
                }

                switch (typeAlias) {
                    case 'console:navSection':
                        return isAppTab ? 'Tab' : 'Section';
                    case 'console:privateContentSection':
                        return 'Personal Section';
                    case 'core:folder':
                        return 'Folder';
                    case 'core:documentFolder':
                        return 'Document Folder';
                    case 'core:chart':
                        return 'Chart';
                    case 'core:board':
                        return 'Board';
                    case 'console:screen':
                        return 'Screen';
                    case 'console:topMenu':
                        return 'Application';
                    case 'core:report':
                        return 'Report';
                    case 'console:customEditForm':
                        return 'Form';
                    default:
                        return 'Resource';
                }
            }


            // Show the rename entity dialog.
            function showRenameEntityDialog(entity, navItem) {
                ensureFieldIds().then(function () {
                    var options = {
                        title: getDisplayTypeName(navItem, entity) + ' Properties',
                        entity: spEntity.fromJSON({
                            id: entity.id()
                        })
                    };

                    options.entity.setField(getFieldId('core:name'), entity.name, spEntity.DataType.String);
                    options.entity.setField(getFieldId('core:description'), entity.description, spEntity.DataType.String);
                    options.entity.setField(getFieldId('core:hideOnDesktop'), entity.hideOnDesktop, spEntity.DataType.Bool);
                    options.entity.setField(getFieldId('core:hideOnTablet'), entity.hideOnTablet, spEntity.DataType.Bool);
                    options.entity.setField(getFieldId('core:hideOnMobile'), entity.hideOnMobile, spEntity.DataType.Bool);

                    options.entity.setDataState(spEntity.DataStateEnum.Update);

                    return showNavigationElementDialog(options).then(function (result) {
                        if (!result) {
                            return;
                        }

                        if (navItem) {
                            navItem.item.name = result.entity.name;
                            navItem.item.entity.name = result.entity.name;
                            navItem.item.entity.description = result.entity.description;
                            navItem.item.entity.hideOnDesktop = result.entity.hideOnDesktop;
                            navItem.item.entity.hideOnTablet = result.entity.hideOnTablet;
                            navItem.item.entity.hideOnMobile = result.entity.hideOnMobile;
                        }

                        // Only reload if the current nav item is the entity being configured
                        if (isCurrentNavItem(result.entity)) {
                            spNavService.reloadCurrentState();
                        }

                        notifyEntityUpdated(result.entity);
                    });
                });
            }


            // Show the configure application dialog.
            function showConfigureApplicationEntityDialog(entity, navItem) {
                return ensureFieldIds().then(function () {
                    var options = {
                            title: 'Application Properties',
                            entity: spEntity.fromJSON({
                                id: entity.id()
                            })
                        },
                        navigationElementIcon = getNavigationElementIcon(entity),
                        existingName = entity.name;

                    options.entity.setField(getFieldId('core:name'), entity.name, spEntity.DataType.String);
                    options.entity.setField(getFieldId('core:description'), entity.description, spEntity.DataType.String);
                    options.entity.setField(getFieldId('core:hideOnDesktop'), entity.hideOnDesktop, spEntity.DataType.Bool);
                    options.entity.setField(getFieldId('core:hideOnTablet'), entity.hideOnTablet, spEntity.DataType.Bool);
                    options.entity.setField(getFieldId('core:hideOnMobile'), entity.hideOnMobile, spEntity.DataType.Bool);

                    if (navigationElementIcon) {
                        options.entity.setLookup(getFieldId('console:navigationElementIcon'), navigationElementIcon ? navigationElementIcon.id() : null);
                    }

                    options.entity.setDataState(spEntity.DataStateEnum.Update);

                    return showApplicationDialog(options).then(function (result) {
                        var solutionEntity,
                            existingSolutionEntity = entity.inSolution;
                        // navigationElementIcon;

                        if (!result) {
                            return;
                        }

                        navigationElementIcon = getNavigationElementIcon(result.entity);

                        navItem.item.name = result.entity.name;
                        navItem.item.entity.name = result.entity.name;
                        navItem.item.entity.description = result.entity.description;
                        navItem.item.entity.hideOnDesktop = result.entity.hideOnDesktop;
                        navItem.item.entity.hideOnTablet = result.entity.hideOnTablet;
                        navItem.item.entity.hideOnMobile = result.entity.hideOnMobile;
                        navItem.item.entity.setLookup(getFieldId('console:navigationElementIcon'), navigationElementIcon ? navigationElementIcon : null);
                        checkIconEntityStyle(navigationElementIcon);

                        if (entity.inSolution &&
                            existingSolutionEntity.name === existingName) {
                            // Rename the solution as well if the top menu only belongs to one solution
                            // and the solution's name matches the top menu's old name
                            solutionEntity = spEntity.fromJSON({
                                id: existingSolutionEntity.id(),
                                name: result.entity.name || '',
                                description: result.entity.description || '',
                                hideOnDesktop: result.entity.hideOnDesktop,
                                hideOnTablet: result.entity.hideOnTablet,
                                hideOnMobile: result.entity.hideOnMobile
                            });
                            solutionEntity.setLookup('console:applicationIcon', navigationElementIcon ? navigationElementIcon : null);
                            solutionEntity.setDataState(spEntity.DataStateEnum.Update);

                            spEntityService.putEntity(solutionEntity).then(function () {
                                existingSolutionEntity.name = result.entity.name;
                                existingSolutionEntity.setLookup('console:applicationIcon', navigationElementIcon ? navigationElementIcon : null);
                            }, function (error) {
                                spAlertsService.addAlert('Failed to update application ' + existingName + '. ' + (error.data.ExceptionMessage || error.data.Message), {
                                    severity: spAlertsService.sev.Error,
                                    expires: true
                                });
                            });
                        }
                    });
                });
            }

            // Fetch background color of icon if it is not available on client
            function checkIconEntityStyle(iconEntity) {

                if (!iconEntity || !iconEntity.idP || iconEntity.imageBackgroundColor) {
                    return;
                }

                spEntityService.getEntity(iconEntity.idP, 'core:imageBackgroundColor').then(function (result) {
                    iconEntity.imageBackgroundColor = result.imageBackgroundColor;
                });
            }


            // Show the report property dialog
            function showReportPropertyDialog(options) {
                return spReportPropertyDialog.showModalDialog(options);
            }


            // Show the chart property dialog
            function showNewChartPropertyDialog(options) {
                return spNewChartDialog.showDialog(options);
            }

            // Show the name and description dialog
            function showApplicationDialog(options) {
                return appElementDialog.showDialog(options);
            }


            // Show the navigation element dialog
            function showNavigationElementDialog(options) {
                return spNavigationElementDialog.showDialog(options);
            }


            // Returns true if the specified entity matches the current nav item.
            function isCurrentNavItem(entity) {
                if (!entity) {
                    return false;
                }

                var currentItem = spNavService.getCurrentItem();

                return (currentItem && currentItem.id === entity.id());
            }


            // Show the configure report entity dialog
            function showConfigureReportEntityDialog(entity, navItem) {
                var reportRequest = spReportEntity.makeReportRequest();

                spEntityService.getEntity(entity.id(), reportRequest).then(function (reportEntity) {
                    if (!reportEntity) {
                        return;
                    }

                    var options = {
                        reportId: entity.id(),
                        reportEntity: new spReportEntity.Query(reportEntity)
                    };

                    showReportPropertyDialog(options).then(function (result) {
                        if (!result) {
                            return;
                        }

                        var reportEntity = result.reportEntity.getEntity();

                        spEntityService.putEntity(reportEntity).then(function () {
                            spEntity.augment(reportEntity, entity);

                            reportEntity.markAllUnchanged();

                            if (navItem) {
                                navItem.item.name = reportEntity.name;
                                navItem.item.entity = reportEntity;

                                updateNavNodeItems(reportEntity);
                            }

                            // Only reload if the current nav item is the entity being configured
                            if (isCurrentNavItem(reportEntity)) {
                                spNavService.reloadCurrentState();
                            }

                            notifyEntityUpdated(reportEntity);
                        });
                    });
                });
            }

            function showConfigureChartEntityDialog(entity, navItem) {

                spChartService.getChartProperties(entity.id()).then(function (chartEntity) {
                    var options = {
                        title: 'Chart Properties',
                        entity: chartEntity,
                        form: 'core:chartPropertiesForm',
                        optionsEnabled: true,
                        saveEntity: true,
                        formLoaded: function (form) {
                            _.find(spEntityUtils.walkEntities(form), function (e) {
                                return sp.result(e, 'relationshipToRender.nsAlias') === 'core:chartReport';
                            }).readOnlyControl = true;
                        }
                    };

                    spEditFormDialog.showDialog(options).then(function (result) {
                        if (result && navItem) {
                            //update the current navItem
                            spEntity.augment(chartEntity, entity);

                            navItem.item.name = chartEntity.name;
                            navItem.item.entity = chartEntity;

                            updateNavNodeItems(chartEntity);

                            // Only reload if the current nav item is the entity being configured
                            if (isCurrentNavItem(chartEntity)) {
                                spNavService.reloadCurrentState();
                            }
                        }

                        notifyEntityUpdated(chartEntity);
                    });

                });
            }

            function showConfigureBoardEntityDialog(entity, navItem) {
                var properties = 'name, description, hideOnDesktop, hideOnTablet, hideOnMobile, ' +
                    'boardShowQuickAdd, boardReport.name, ' +
                    'console:navigationElementIcon.{alias, name, imageBackgroundColor}, inSolution.name';
                return spEntityService.getEntity(entity.id(), properties, { batch: true, hint: 'board properties' }).then(function (boardEntity) {
                    var options = {
                        title: 'Board Properties',
                        form: 'console:boardPropertiesForm',
                        entity: boardEntity,
                        formMode: 'edit',
                        optionsEnabled: true,
                        saveEntity: true
                    };
                    return showConfigureEntityDialog(entity, navItem, options);
                });
            }

            function showConfigureEntityDialog(entity, navItem, options) {
                return showEntityDialog(entity, options).then(function (result) {
                    if (!result) {
                        return;
                    }

                    var item = options.entity;

                    spEntity.augment(item, entity);

                    if (navItem) {
                        navItem.item.name = item.name;
                        navItem.item.entity = item;

                        updateNavNodeItems(item);

                        if (isCurrentNavItem(item)) {
                            spNavService.reloadCurrentState();
                        }
                    }

                    //notifyEntityUpdated(item);
                });
            }

            // Show the configure screen entity dialog
            function showConfigureNavEntityDialog(entity, navItem) {
                ensureFieldIds().then(function () {
                    var options = {
                            title: getDisplayTypeName(navItem, entity) + ' Properties',
                            entity: spEntity.fromJSON({
                                id: entity.id()
                            })
                        },
                        navigationElementIcon = getNavigationElementIcon(entity);

                    options.entity.setField(getFieldId('core:name'), entity.name, spEntity.DataType.String);
                    options.entity.setField(getFieldId('core:description'), entity.description, spEntity.DataType.String);
                    options.entity.setField(getFieldId('core:hideOnDesktop'), entity.hideOnDesktop, spEntity.DataType.Bool);
                    options.entity.setField(getFieldId('core:hideOnTablet'), entity.hideOnTablet, spEntity.DataType.Bool);
                    options.entity.setField(getFieldId('core:hideOnMobile'), entity.hideOnMobile, spEntity.DataType.Bool);
                    options.entity.setLookup(getFieldId('console:navigationElementIcon'), navigationElementIcon ? navigationElementIcon.id() : null);
                    options.entity.setLookup(getFieldId('core:inSolution'), entity.inSolution || null);

                    options.entity.setDataState(spEntity.DataStateEnum.Update);

                    return showNavigationElementDialog(options).then(function (result) {
                        if (!result) {
                            return;
                        }

                        var navigationElementIcon = getNavigationElementIcon(result.entity);

                        if (navItem) {
                            navItem.item.name = result.entity.name;
                            navItem.item.entity.name = result.entity.name;
                            navItem.item.entity.description = result.entity.description;
                            navItem.item.entity.hideOnDesktop = result.entity.hideOnDesktop;
                            navItem.item.entity.hideOnTablet = result.entity.hideOnTablet;
                            navItem.item.entity.hideOnMobile = result.entity.hideOnMobile;
                            navItem.item.entity.setLookup(getFieldId('core:inSolution'), result.entity.inSolution);
                            navItem.item.entity.setLookup(getFieldId('console:navigationElementIcon'), navigationElementIcon ? navigationElementIcon.id() : null);
                        }

                        // Only reload if the current nav item is the entity being configured
                        if (isCurrentNavItem(result.entity)) {
                            spNavService.reloadCurrentState();
                        }
                    });
                });
            }


            // Gets the insertion indicator position for a nav item.
            function getInsertionIndicatorPositionNavItem(event, target, dragData, dropData) {
                var clientRect = target.getBoundingClientRect(),
                    clientY = event.originalEvent.clientY,
                    height = clientRect.height,
                    sectionEnum,
                    y = clientY - clientRect.top;

                var sections = canTargetContainSource(dropData, dragData) ? 3 : 2;

                var sectionHeight = height / sections;
                var section = Math.floor(y / sectionHeight);

                if (section === 0) {
                    sectionEnum = insertionIndicatorPosition.before;
                } else if (section === sections - 1) {
                    sectionEnum = insertionIndicatorPosition.after;
                } else {
                    sectionEnum = insertionIndicatorPosition.inside;
                }

                return sectionEnum;
            }


            // Gets the insertion indicator position for a tab item.
            function getInsertionIndicatorPositionTabItem(event, target, dragData, dropData) {
                var clientRect = target.getBoundingClientRect(),
                    clientX = event.originalEvent.clientX,
                    width = clientRect.width,
                    sectionEnum,
                    x = clientX - clientRect.left;

                var sections = canTargetContainSource(dropData, dragData) ? 3 : 2;

                var sectionWidth = width / sections;
                var section = Math.floor(x / sectionWidth);

                if (section === 0) {
                    sectionEnum = insertionIndicatorPosition.before;
                } else if (section === sections - 1) {
                    sectionEnum = insertionIndicatorPosition.after;
                } else {
                    sectionEnum = insertionIndicatorPosition.inside;
                }

                return sectionEnum;
            }


            // Returns true if the target can contain the source.
            function canTargetContainSource(targetItem, sourceItem) {
                if (isContainer(targetItem)) {
                    if (isTab(targetItem)) {
                        // Target is a tab item
                        // Target can only contain
                        // non app tab nav sections
                        return isNavSection(sourceItem);
                    } else if (isNavSection(targetItem)) {
                        // Target is a nav section. It can only contain non-nav sections
                        return !isNavSection(sourceItem) && !isTab(sourceItem);
                    } else {
                        return !isNavSection(sourceItem) && !isTab(sourceItem);
                    }
                } else {
                    // Target is not a container
                    return false;
                }
            }


            // Returns true if the item is a container, false otherwise.
            function isContainer(node) {
                if (!node || !node.item) {
                    return false;
                }

                return (node.item.typeAlias === 'console:navSection' || node.item.typeAlias === 'core:folder' || node.item.typeAlias === 'core:documentFolder' || node.item.typeAlias === 'console:privateContentSection');
            }


            // Returns true if the item is a navSection, false otherwise.
            function isNavSection(node) {
                if (!node || !node.item) {
                    return false;
                }

                return (node.item.typeAlias === 'console:navSection') && (!node.item.isAppTab || node.item.depth > 2);
            }

            // Returns true if the item is a privateContentSection, false otherwise.
            function isPrivateContentSection(node) {
                if (!node || !node.item) {
                    return false;
                }

                return node.item.typeAlias === 'console:privateContentSection';
            }

            // Returns true if the item is navSection tab, false otherwise.
            function isTab(node) {
                if (!node || !node.item) {
                    return false;
                }

                return (node.item.typeAlias === 'console:navSection') && (node.item.isAppTab || node.item.depth === 2);
            }


            // Returns true if the item is a topMenu, false otherwise
            function isTopMenu(node) {
                if (!node || !node.item) {
                    return false;
                }

                return (node.item.typeAlias === 'console:topMenu');
            }            


            // Show the insertion indicator for root nav items
            function showInsertionIndicatorRootNavItems(event, target, dragData, dropData) {
                var clientRect;
                var childPresent;

                if (target.children && target.children.length) {
                    clientRect = target.children[target.children.length - 1].getBoundingClientRect();
                    childPresent = true;
                } else {
                    clientRect = target.getBoundingClientRect();
                    childPresent = false;
                }

                scope.insertIndicatorDropData.data = dropData;
                scope.insertIndicatorDropData.target = target;

                if (currentDropTarget === dropData &&
                    currentInsertionIndicatorPosition === insertionIndicatorPosition.inside) {
                    return;
                }

                if (!childPresent) {
                    positionInsertIndicator(clientRect.top, clientRect.left, 4, target.clientWidth);
                } else {
                    positionInsertIndicator(clientRect.bottom, clientRect.left, 4, target.clientWidth);
                }

                currentDropTarget = dropData;
                currentInsertionIndicatorPosition = insertionIndicatorPosition.inside;
            }            

            // Show the insertion indicator for nav items.
            function showInsertionIndicatorNavItems(event, target, dragData, dropData) {
                var clientRect = target.getBoundingClientRect(),
                    iiPosition = getInsertionIndicatorPositionNavItem(event, target, dragData, dropData);

                scope.insertIndicatorDropData.data = dropData;
                scope.insertIndicatorDropData.target = target;

                if (currentDropTarget === dropData &&
                    currentInsertionIndicatorPosition === iiPosition) {
                    return;
                }

                switch (iiPosition) {
                    case insertionIndicatorPosition.before:
                        positionInsertIndicator(clientRect.top, clientRect.left, 4, target.clientWidth);
                        break;
                    case insertionIndicatorPosition.inside:
                        positionInsertIndicator(clientRect.top, clientRect.left, target.clientHeight, target.clientWidth, true);
                        break;
                    case insertionIndicatorPosition.after:
                        positionInsertIndicator(clientRect.bottom - 4, clientRect.left, 4, target.clientWidth);
                        break;
                }

                currentDropTarget = dropData;
                currentInsertionIndicatorPosition = iiPosition;
            }


            // Show the insertion indicator for tab items.
            function showInsertionIndicatorTabItems(event, target, dragData, dropData) {
                var clientRect = target.getBoundingClientRect(),
                    iiPosition = getInsertionIndicatorPositionTabItem(event, target, dragData, dropData);

                scope.insertIndicatorDropData.data = dropData;
                scope.insertIndicatorDropData.target = target;

                if (currentDropTarget === dropData &&
                    currentInsertionIndicatorPosition === iiPosition) {
                    return;
                }

                switch (iiPosition) {
                    case insertionIndicatorPosition.before:
                        positionInsertIndicator(clientRect.top, clientRect.left, target.clientHeight, 4);
                        break;
                    case insertionIndicatorPosition.inside:
                        positionInsertIndicator(clientRect.top, clientRect.left, target.clientHeight, target.clientWidth, true);
                        break;
                    case insertionIndicatorPosition.after:
                        positionInsertIndicator(clientRect.top, clientRect.right - 4, target.clientHeight, 4);
                        break;
                }

                currentDropTarget = dropData;
                currentInsertionIndicatorPosition = iiPosition;
            }


            // get the insertion position.
            function getInsertionIndicatorPosition(event, target, dragData, dropData) {
                if (dropData === 'navTreeRoot') {
                    return insertionIndicatorPosition.inside;
                } else {
                    if (isTab(dropData)) {
                        return getInsertionIndicatorPositionTabItem(event, target, dragData, dropData);
                    } else {
                        return getInsertionIndicatorPositionNavItem(event, target, dragData, dropData);
                    }
                }
            }

            // Show the insertion indicator.
            function showInsertionIndicator(event, target, dragData, dropData) {
                if (dropData === 'navTreeRoot') {
                    showInsertionIndicatorRootNavItems(event, target, dragData, dropData);
                } else {
                    if (isTab(dropData)) {
                        showInsertionIndicatorTabItems(event, target, dragData, dropData);
                    } else {
                        showInsertionIndicatorNavItems(event, target, dragData, dropData);
                    }
                }
            }


            // Hides the insert indicator.
            function hideInsertIndicator() {
                currentDropTarget = null;
                currentInsertionIndicatorPosition = null;                

                if (!insertIndicatorElement) {
                    return;
                }

                insertIndicatorElement.hide();
            }


            // Shows the insert indicator.
            function showInsertIndicator() {
                if (!insertIndicatorElement) {
                    return;
                }

                insertIndicatorElement.show();
            }


            // Positions the insert indicator.
            function positionInsertIndicator(top, left, height, width, isBorder) {
                if (!_.isNumber(top) || !_.isNumber(left) || !_.isNumber(height) || !_.isNumber(width)) {
                    return;
                }

                if (!insertIndicatorElement) {
                    createInsertIndicator();
                }

                if (isBorder) {
                    insertIndicatorElement.removeClass('navInsertionIndicatorLine');
                    insertIndicatorElement.addClass('navInsertionIndicatorBorder');
                } else {
                    insertIndicatorElement.removeClass('navInsertionIndicatorBorder');
                    insertIndicatorElement.addClass('navInsertionIndicatorLine');
                }

                var scrollX = $window.scrollX || 0;
                var scrollY = $window.scrollY || 0;

                insertIndicatorElement.css({
                    width: width,
                    height: height,
                    top: top + scrollY,
                    left: left + scrollX
                }).show();
            }


            // Create the insert indicator.
            function createInsertIndicator() {
                if (insertIndicatorElement) {
                    return;
                }

                insertIndicatorScope = scope.$new();

                scope.$apply(function () {
                    insertIndicatorElement = $compile('<div class="navInsertionIndicator" sp-droppable="insertIndicatorDropOptions" sp-droppable-data="insertIndicatorDropData" />')(insertIndicatorScope);
                    if (insertIndicatorElement) {
                        body.append(insertIndicatorElement);
                        insertIndicatorElement.hide();

                        currentDropTarget = null;
                        currentInsertionIndicatorPosition = null;                        
                    }
                });
            }


            // Destroy the insert indicator.
            function destroyInsertIndicator() {
                currentDropTarget = null;
                currentInsertionIndicatorPosition = null;
                canDropInsertionIndicatorPosition = null;

                if (insertIndicatorElement) {
                    insertIndicatorElement.remove();
                    insertIndicatorElement = null;
                }

                if (insertIndicatorScope) {
                    insertIndicatorScope.$destroy();
                    insertIndicatorScope = null;
                }
            }

            function canDropItemSelfServeModeOnly(dragData, dropData) {
                var typeAlias;
                var isDropPrivateSection = isPrivateContentSection(dropData);

                // Can only drop inside a private content section or
                // next to a child of a private content section
                if ((isDropPrivateSection && canDropInsertionIndicatorPosition !== insertionIndicatorPosition.inside) ||
                    (!isDropPrivateSection && !isPrivateContentSection(dropData.parent))) {
                    return false;
                }                

                var isNew = dragData.source === 'new';

                if (isNew && dragData.canSelfServe) {
                    // Creating a new self serveable item
                    return true;
                } else {
                    typeAlias = sp.result(dragData, 'item.typeAlias');
                    // Rearranging. Can only move stuff in a private content node                    

                    return isPrivateContentSection(dragData.parent) &&
                        (typeAlias === 'core:report' || typeAlias === 'core:chart' || typeAlias === 'console:screen');
                }
            }

            // Returns true if the drag data can be dropped onto the drop data, false otherwise.
            function canDropItem(dragData, dropData) {
                var dropParentNode,
                    selectedTabNode = scope.getSelectedTabNode();

                if (!dragData || !dropData ||
                    dragData === dropData || !spNavService.isSelfServeEditMode) {
                    return false;
                }

                if (!selectedTabNode) {
                    return false;
                }

                // Ensure cannot drop parent into child
                dropParentNode = dropData.parent;
                while (dropParentNode) {
                    if (dropParentNode.item &&
                        dragData.item &&
                        dropParentNode.item.id === dragData.item.id) {
                        return false;
                    }

                    dropParentNode = dropParentNode.parent;
                }

                if (spAppSettings.selfServeNonAdmin) {                    
                    return canDropItemSelfServeModeOnly(dragData, dropData);
                }

                if (isTopMenu(dragData) || isTopMenu(dropData)) {
                    // If the source or target is top menu
                    // can only drop if both are top menu
                    return isTopMenu(dragData) && isTopMenu(dropData);
                }                

                var isNew = dragData.source === 'new';

                if (dropData === 'navTreeRoot') {
                    return true;
                }                

                if (isNew &&
                    isTab(dropData)) {
                    // Cannot drop new items onto tabs
                    return false;
                }

                // Nav sections can only be added at depth 2 and 3
                if (isNavSection(dragData) || isTab(dragData)) {
                    return (dropData.item.depth === 2 || dropData.item.depth === 3);
                }
                else {
                    // Source is not a nav section
                    // Can only be dropped onto non-tab items
                    return !isTab(dropData);
                }
            }

            function doDropItem(sourceNode, targetNode, iiPosition) {

                console.log([
                    'doDropItem',
                    sp.result(sourceNode, 'source'),
                    sp.result(sourceNode, 'item.typeAlias'),
                    sp.result(targetNode, 'item.id'),
                    sp.result(targetNode, 'item.name'),
                    iiPosition
                ].join());

                if (sourceNode.source === 'new') {
                    // Need to create a new item
                    insertNewItem(sourceNode, targetNode, iiPosition);
                } else if (sourceNode.source === 'existing') {
                    // Need to select existing item
                    selectExistingItem(sourceNode, targetNode, iiPosition);
                }
                else {
                    // Move an existing item
                    if (isTopMenu(sourceNode)) {
                        moveTopMenuItem(sourceNode, targetNode, iiPosition);
                    } else {
                        //before move report or folder, check source item's name is exists in target forder
                        var duplicateNameInFolder = false;
                        //this validate function will be removed after we set back the folderUniqueNameKey and reportUniqueNameKey later
                        if (sourceNode.item.viewType === 'report' || sourceNode.item.viewType === 'documentFolder') {
                            duplicateNameInFolder = spNavService.duplicateNameInFolder(targetNode.item.id, sourceNode.item.viewType, sourceNode.item.name);
                            if (duplicateNameInFolder === true) {
                                var itemType = getItemType(sourceNode.item.typeAlias);
                                spAlertsService.addAlert('Failed to move item ' + sourceNode.item.name + ' to the section. There is another ' + itemType + ' with the same name in the same section.', {
                                    severity: spAlertsService.sev.Error,
                                    expires: true
                                });

                            }

                        }
                        if (duplicateNameInFolder === false) {
                            moveItem(sourceNode, targetNode, iiPosition, true, false);
                        }
                    }
                }
            }

            // Handle dropping the drag data onto the drop data.
            function dropItem(event, source, target, dragData, dropData) {
                var iiPosition;

                if (!canDropItem(dragData, dropData)) {
                    return;
                }

                if (dropData === 'navTreeRoot') {
                    var selectedTab = scope.getSelectedTabNode();

                    if (selectedTab && selectedTab.children && selectedTab.children.length) {
                        dropData = _.last(selectedTab.children);
                        iiPosition = insertionIndicatorPosition.after;
                    } else {
                        iiPosition = insertionIndicatorPosition.inside;
                        dropData = selectedTab;
                    }
                } else {
                    if (isTab(dropData)) {
                        iiPosition = getInsertionIndicatorPositionTabItem(event, target, dragData, dropData);
                    } else {
                        iiPosition = getInsertionIndicatorPositionNavItem(event, target, dragData, dropData);
                    }
                }

                scope.$apply(function () {
                    doDropItem(dragData, dropData, iiPosition);
                });

                destroyInsertIndicator();
            }


            // Insert a new item
            function insertNewItem(sourceNode, targetNode, position) {
                if (sourceNode.source !== 'new') {
                    return;
                }

                switch (sourceNode.item.typeAlias) {
                    case 'console:navSection':
                        showNewNavSectionEntityDialog(sourceNode, targetNode, position);
                        break;
                    case 'core:documentFolder':
                        showNewDocumentFolderEntityDialog(sourceNode, targetNode, position);
                        break;
                    case 'console:screen':
                        showDialogIfCanNavigate(showNewScreenEntityDialog, sourceNode, targetNode, position);
                        break;
                    case 'core:report':
                        showDialogIfCanNavigate(showNewReportEntityDialog, sourceNode, targetNode, position);
                        break;
                    case 'core:chart':
                        showDialogIfCanNavigate(showNewChartEntityDialog, sourceNode, targetNode, position);
                        break;
                    case 'core:board':
                        showDialogIfCanNavigate(showNewBoardEntityDialog, sourceNode, targetNode, position);
                        break;
                    case 'core:definition':
                        showDialogIfCanNavigate(showNewDefinition, sourceNode, targetNode, position);
                        break;
                    case 'console:privateContentSection':
                        showNewPrivateContentSectionEntityDialog(sourceNode, targetNode, position);
                        break;
                    default:
                        console.error('unsupported nav item: ' + sourceNode.item.typeAlias);
                }
            }

            // Show the dialog if there are not changes
            function showDialogIfCanNavigate(showDialogFunction, sourceNode, targetNode, position) {
                spNavService.navigateInternal().then(function(navResult) {
                    if (!navResult) {
                        showDialogFunction(sourceNode, targetNode, position);
                        return;
                    }
                });
            }

            // Select existing item
            function selectExistingItem(sourceNode, targetNode, position) {
                if (sourceNode.source !== 'existing') {
                    return;
                }

                switch (sourceNode.item.typeAlias) {
                    case 'core:report':
                        openResourcePickerDialog(sourceNode, targetNode, position, 'console:reportsReport');
                        break;
                    case 'core:chart':
                        openResourcePickerDialog(sourceNode, targetNode, position, 'console:chartsReport');
                        break;
                    case 'core:board':
                        openResourcePickerDialog(sourceNode, targetNode, position, 'core:boardsReport');
                        break;
                    case 'console:screen':
                        openResourcePickerDialog(sourceNode, targetNode, position, 'console:screensReport');
                        break;
                    default:
                        console.error('unsupported nav item: ' + sourceNode.item.typeAlias);
                }
            }


            //open resource picker base on source node type. select existing resource entity
            function openResourcePickerDialog(sourceNode, targetNode, position, reportId) {
                var modalInstanceCtrl = ['$scope', '$uibModalInstance', 'outerScopeReportOptions', function ($scope, $uibModalInstance, outerScopeReportOptions) {
                    $scope.isModalOpened = true;
                    $scope.model = {};
                    $scope.model.reportOptions = outerScopeReportOptions;
                    $scope.ok = function () {
                        $scope.isModalOpened = false;
                        $uibModalInstance.close($scope.model.reportOptions);

                    };

                    $scope.$on('spReportEventGridDoubleClicked', function (event) {
                        event.stopPropagation();

                        $scope.ok();
                    });

                    $scope.cancel = function () {
                        $scope.isModalOpened = false;
                        $uibModalInstance.dismiss('cancel');
                    };

                    $scope.model.reportOptions.cancelDialog = $scope.cancel;
                }];

                var reportOptions = {
                    reportId: reportId,
                    multiSelect: false,
                    isEditMode: false,
                    selectedItems: null,
                    entityTypeId: 'core:report',
                    newButtonInfo: {},
                    isInPicker: true
                };

                var defaults = {
                    templateUrl: 'entityPickers/entityReportPicker/spEntityReportPicker_modal.tpl.html',
                    controller: modalInstanceCtrl,
                    windowClass: 'modal inlineRelationPickerDialog',
                    resolve: {
                        outerScopeReportOptions: function () {
                            return reportOptions;
                        }
                    }
                };

                var options = {};


                spDialogService.showDialog(defaults, options).then(function (result) {

                    if (result && result.selectedItems && result.selectedItems.length > 0) {


                        spEntityService.getEntity(result.selectedItems[0].eid, 'id, name, alias, isOfType.{ name, alias }', {
                            hint: 'report',
                            batch: true
                        }).then(function (entity) {
                            var targetNodeItem = getParentTargetNode(targetNode, position);

                            var newSourceNode = {
                                item: spNavDataService.navItemFromEntity(entity),
                                children: [],
                                parent: targetNodeItem
                            };

                            // Navigate the report object or chart object after copy the navitem to target Node
                            var movedItem = moveItem(newSourceNode, targetNode, position, false, true);

                            if (movedItem) {
                                movedItem.then(function () {
                                    var isTopItem = spNavService.getParentItem() && spNavService.getParentItem().id === spNavService.getCurrentApplicationId() && targetNode.children && targetNode.children.length === 0;

                                    switch (sourceNode.item.typeAlias) {
                                        case 'core:report':
                                            navigateTo('report', newSourceNode.item.id, isTopItem);
                                            break;
                                        case 'core:chart':
                                            navigateTo('chart', newSourceNode.item.id, isTopItem);
                                            break;
                                        case 'core:board':
                                            navigateTo('board', newSourceNode.item.id, isTopItem);
                                            break;
                                        default:
                                            break;
                                    }
                                });
                            }
                        });
                    }
                });
            }


            function navigateTo(type, itemId, isTopItem) {
                if (isTopItem === true) {
                    spNavService.navigateToSibling(type, itemId).finally(function () {
                        spNavService.navigateToChildState(type, itemId);
                        spNavService.navigateToParent();
                    });
                } else {
                    spNavService.navigateToSibling(type, itemId);
                }
            }

            // Ensure that the name and description fields are loaded
            function ensureFieldIds() {
                var deferred;

                if (!fieldIds) {
                    return spEntityService.getEntities(fieldAliases, 'id, alias').then(function (result) {
                        fieldIds = _.keyBy(result, function (f) {
                            return f.alias();
                        });
                    });
                } else {
                    deferred = $q.defer();
                    deferred.resolve();

                    return deferred.promise;
                }
            }


            // Get an entity containing the deltas
            function getEntityDelta(sourceNode, targetNode, position, navItemOrdering) {
                var sourceEntity,
                    existingSourceItem,
                    existingSourceOrder = -1,
                    newIsAppTab = false,
                    existingIsAppTab = false;

                var targetNodeItem = getParentTargetNode(targetNode, position);

                var entityDeltas = spEntity.fromJSON({
                    id: targetNodeItem.item.id,
                    'console:folderContents': jsonRelationship()
                });                

                entityDeltas.setDataState(spEntity.DataStateEnum.Update);

                // Enumerate thru the existing children
                _.forEach(targetNodeItem.children, function (childNavNode) {
                    var addToFolder = false;

                    if (!childNavNode.item.entity) {
                        return true;
                    }

                    if (childNavNode.item.id === sourceNode.item.id) {
                        existingSourceItem = childNavNode;
                        return true;
                    }

                    var childEntity = spEntity.fromJSON({
                        id: childNavNode.item.id
                    });

                    var navItemOrder = navItemOrdering[childNavNode.item.id];

                    // Moving an existing item. Update the console order only
                    if (navItemOrder &&
                        childNavNode.item.order !== navItemOrder.order) {
                        childEntity.setField('console:consoleOrder', navItemOrder.order, spEntity.DataType.Int32);
                        childEntity.setDataState(spEntity.DataStateEnum.Update);
                        addToFolder = true;
                    }

                    if (addToFolder) {
                        entityDeltas.folderContents.add(childEntity);
                    }

                    return true;
                });

                var sourceChildOrder = navItemOrdering[sourceNode.item.id];
                if (existingSourceItem) {
                    existingSourceOrder = existingSourceItem.item.order;
                }

                if (sourceNode.item.typeAlias === 'console:navSection') {
                    // Get the current is app tab
                    existingIsAppTab = sourceNode.item.isAppTab;

                    // If the parent is a top menu then set is app tab to true
                    if (targetNodeItem.item.typeAlias === 'console:topMenu') {
                        newIsAppTab = true;
                    }
                }

                // Update the source item if needed
                if (existingSourceOrder < 0 ||
                    (sourceChildOrder && existingSourceOrder !== sourceChildOrder.order) ||
                    (sourceNode.parent && sourceNode.parent.item.id !== targetNodeItem.item.id) ||
                    (newIsAppTab !== existingIsAppTab)) {

                    // Add source
                    sourceEntity = spEntity.fromJSON({
                        id: sourceNode.item.id
                    });

                    if (sourceNode.parent && sourceNode.parent.item.id !== targetNodeItem.item.id) {

                        sourceEntity.setRelationship('console:resourceInFolder', [targetNodeItem.item.id]);
                    }

                    if (sourceChildOrder && existingSourceOrder !== sourceChildOrder.order) {
                        sourceEntity.setField('console:consoleOrder', sourceChildOrder.order, spEntity.DataType.Int32);
                    }

                    if (newIsAppTab !== existingIsAppTab) {
                        sourceEntity.setField('console:isAppTab', newIsAppTab, spEntity.DataType.Bool);
                    }

                    sourceEntity.setDataState(spEntity.DataStateEnum.Update);

                    entityDeltas.folderContents.add(sourceEntity);
                }

                return entityDeltas;
            }


            // Returns a dictionary with the new entity orderings
            function getNavItemOrdering(sourceNode, targetNode, position) {
                var targetIndex = 0, targetOrders = [];

                var targetNodeParent = getParentTargetNode(targetNode, position);

                _.forEach(targetNodeParent.children, function (c) {
                    // skip the source item
                    if (c.item.id === sourceNode.item.id) {
                        return true;
                    }

                    targetOrders.push({
                        id: c.item.id,
                        order: c.item.order
                    });

                    if (c.item.id === targetNode.item.id) {
                        // Found the target item
                        // Save it's index
                        targetIndex = targetOrders.length - 1;
                    }

                    return true;
                });

                switch (position) {
                    case insertionIndicatorPosition.inside:
                        targetIndex = 0;
                        break;
                    case insertionIndicatorPosition.after:
                        targetIndex = targetIndex + 1;
                        break;
                }

                // Insert the new item into the target orders at the specified index
                targetOrders.splice(targetIndex, 0, {id: sourceNode.item.id, order: sourceNode.item.order});

                // Update the console orders
                _.forEach(targetOrders, function (t, index) {
                    if (t.order !== index) {
                        t.order = index;
                    }
                });

                return _.keyBy(targetOrders, 'id');
            }


            // Apply the changes to navigation model and reload
            function applyChangesToNavModel(sourceNode, targetNode, position, navItemOrdering, isMoveOnly) {
                var parentItem = getParentTargetNode(targetNode, position);

                // The source and target have different parents
                // Remove the source from its parent
                if (sourceNode.parent &&
                    sourceNode.parent !== parentItem &&
                    isMoveOnly === true) {
                    // Remove the source item from the tree
                    _.pull(sourceNode.parent.children, sourceNode);
                }

                //to verify current node is top nav which means the new item is in root level of left navigation  
                if (parentItem.item.typeAlias !== 'console:topMenu') {
                    spNavService.requireRefreshTree();
                    spNavService.refreshTreeBranch(parentItem.item).then(function () {
                        //re-access the top nav when item in root navigation to fix the bug 25190                                               
                        scope.middleBusy.isBusy = false;
                    }, function () {
                        scope.middleBusy.isBusy = false;
                    });
                } else {
                    spNavService.requestInitialNavTree().then(function () {                        
                        scope.middleBusy.isBusy = false;
                    }, function () {
                        scope.middleBusy.isBusy = false;
                    });
                }
            }


            // Move the specified top menu item to the specified position
            function moveTopMenuItem(sourceNode, targetNode, position) {
                var deferred,
                    promises = [];

                // Get the item ordering
                var navItemOrdering = getNavItemOrdering(sourceNode, targetNode, position);
                var targetNodeParent = getParentTargetNode(targetNode, position);

                // Find which top menus are out of order and update them
                _.forEach(targetNodeParent.children, function (topMenuNode) {
                    var topMenuEntity;

                    var navItemOrder = navItemOrdering[topMenuNode.item.id];

                    // Moving an existing item. Update the console order only
                    if (navItemOrder &&
                        topMenuNode.item.order !== navItemOrder.order) {
                        scope.middleBusy.isBusy = true;

                        topMenuEntity = spEntity.fromJSON({
                            id: topMenuNode.item.id,
                            'console:consoleOrder': navItemOrder.order
                        });

                        topMenuEntity.setDataState(spEntity.DataStateEnum.Update);
                        promises.push(spEntityService.putEntity(topMenuEntity));
                    }
                });

                if (promises.length) {
                    return $q.all(promises).then(function () {
                        spNavService.requestInitialNavTree().then(function () {
                            scope.middleBusy.isBusy = false;
                        }, function () {
                            scope.middleBusy.isBusy = false;
                        });
                    }, function () {
                        scope.middleBusy.isBusy = false;
                    });
                } else {
                    deferred = $q.defer();
                    deferred.resolve();

                    return deferred.promise;
                }
            }

            function getItemType(typeAlias) {
                var itemType = 'object';
                switch (typeAlias) {
                    case 'core:chart':
                        itemType = 'chart';
                        break;
                    case 'core:board':
                        itemType = 'board';
                        break;
                    case 'console:screen':
                        itemType = 'screen';
                        break;
                    case 'core:report':
                        itemType = 'report';
                        break;
                    case 'core:documentFolder':
                        itemType = 'document folder';
                        break;
                    case 'console:navSection':
                        itemType = 'section';
                        break;
                    case 'console:privateContentSection':
                        itemType = 'private content section';
                        break;
                }

                return itemType;
            }


            // Move the specified source nav item to the target at the specified position
            function moveItem(sourceNode, targetNode, position, isMoveOnly, isNewItem) {
                var deferred;

                // if the drop target item is root level item, use parent item to check existing report/chart
                var checkNode = isRootLevelItem(targetNode) && targetNode.parent ? targetNode.parent : targetNode;

                // can't add or move existing report/chart to same folder
                // ^^^ AE: but we have to allow for re-ordering? changing to throw alert only when it's an "add" as in "Add existing item"
                //     any subsequent move should be covered by the same-name prevention check
                var existingItem = _.find(checkNode.children, function (childNode) {
                    return childNode.item.id === sourceNode.item.id;
                });

                if (existingItem && isNewItem) {
                    var itemType = getItemType(sourceNode.item.typeAlias);

                    // ??? AE: why is this even an alert? someone tried to add an item to a section that was already there. can't we just ignore it and let them continue?
                    spAlertsService.addAlert('Failed to add item ' + sourceNode.item.name + ' to the section. There is same ' + itemType + ' in this section.', {
                        severity: spAlertsService.sev.Error,
                        expires: true
                    });

                    return null;
                }

                var navItemOrdering = getNavItemOrdering(sourceNode, targetNode, position);

                // Note: targetEntityDelta may be the child for self-serve users, or the parent for admins
                var entityDelta = getEntityDelta(sourceNode, targetNode, position, navItemOrdering);

                if (entityDelta.dataState !== spEntity.DataStateEnum.Unchanged) {
                    scope.middleBusy.isBusy = true;

                    return spEntityService.putEntity(entityDelta).then(function () {
                        // Update model and cached entities
                        applyChangesToNavModel(sourceNode, targetNode, position, navItemOrdering, isMoveOnly || true);
                    }, function (error) {
                        spAlertsService.addAlert('Failed to move item ' + sourceNode.item.name + '. ' + (error.data.ExceptionMessage || error.data.Message), {
                            severity: spAlertsService.sev.Error,
                            expires: true
                        });

                        // An error occured
                        scope.middleBusy.isBusy = false;
                        scope.refreshNavTreeItems();
                    });
                } else {
                    deferred = $q.defer();
                    deferred.resolve();

                    return deferred.promise;
                }
            }

            /**
             * @returns {boolean} true if a new tab can be added, false otherwise
             */
            exports.canAddNewTab = function () {
                var node = scope.getSelectedMenuNode();

                return node &&
                    node.item &&
                    node.item.showTabs &&
                    spNavService.isFullEditMode;
            };


            function isRootLevelItem(targetNode) {
                return targetNode.item.depth === 3 && targetNode.item.typeAlias !== 'core:documentFolder' && targetNode.item.typeAlias !== 'console:navSection' && targetNode.item.typeAlias !== 'console:privateContentSection';
            }

            /**
             * Adds a new tab.
             */
            exports.addNewTab = function () {
                var parentNavItem = scope.getSelectedMenuNode();
                showNewTabEntityDialog(parentNavItem);
            };


            /**
             * Adds a new application.
             */
            exports.addNewApplication = function () {
                showNewApplicationEntityDialog();
            };


            /**
             * Deletes the specified nav item entity.
             * @param {object} entity The entity to delete.
             * @param {boolean} deleteItem True to delete the item, false otherwise
             * @param {object} parentItem The parent navitem of current navitem
             */
            exports.removeNavItem = function (entity, deleteItem, parentItem) {
                var existingSolutionEntity;

                // Sanity check
                if (!spNavService.isSelfServeEditMode) {
                    console.log('Unable to delete nav entity. spNavigationBuilderProvider is not in edit mode.');
                    return;
                }

                if (!entity) {
                    return;
                }

                var navItem = getNavItemFromEntity(entity, parentItem);

                if (navItem &&
                    navItem.item.typeAlias === 'console:topMenu') {
                    if (entity.inSolution) {
                        // Delete the solution entity only if top menu is in one solution
                        // and the name of the top menu matches the solution
                        existingSolutionEntity = entity.inSolution;
                        if (existingSolutionEntity.name === entity.name) {
                            entity = existingSolutionEntity;
                        }
                    }
                }

                var navTree = spNavService.getNavTree();
                if (_.isUndefined(deleteItem) || deleteItem === true) {
                    // Delete the entity
                    spEntityHelper.promptDelete({entity: entity}).then(function (delResult) {
                        if (!delResult) {
                            return;
                        }
                        removeNavItemFromParent(navItem, navTree);

                    });
                } else {
                    //remove folder from current entity's instance of resourceInFolder relationship and update the entity
                    if (parentItem) {
                        entity.resourceInFolder.remove(parentItem.item.id);
                    }
                    //entity.resourceInFolder = _.filter(entity.resourceInFolder, function (folder) {
                    //    return parentItem && folder.idP !== parentItem.item.id;
                    //});

                    spEntityService.putEntity(entity).then(function (id) {
                        if (!id) {
                            return;
                        }
                        removeNavItemFromParent(navItem, navTree);
                    }, function (error) {
                        spAlertsService.addAlert('fail to remove ' + name + ' from folder. ' + (error.data.ExceptionMessage || error.data.Message), {
                            severity: spAlertsService.sev.Info,
                            expires: true
                        });
                    });
                }
            };


            function removeNavItemFromParent(navItem, navTree) {
                var navItemIndexToNavTo, navItemToNavTo, navItemFirstNavTo;

                if ((!navItem || !navItem.parent || sp.result(navItem, 'item.typeAlias') === 'console:topMenu') &&
                    (navTree && navTree.children && navTree.children.length)) {

                    if (navItem && navItem.parent) {
                        _.pull(navItem.parent.children, navItem);
                    }

                    if (navTree.children[0].item &&
                        navTree.children[0].item.state) {
                        spNavService.navigateToState(navTree.children[0].item.viewType, navTree.children[0].item.state.params);
                    }
                } else if (navItem && navItem.parent && navItem.parent.children && navItem.parent.children.length > 0) {
                    navItemIndexToNavTo = _.indexOf(navItem.parent.children, navItem);
                    _.pull(navItem.parent.children, navItem);

                    if (navItemIndexToNavTo > navItem.parent.children.length - 1) {
                        navItemIndexToNavTo = navItem.parent.children.length - 1;
                    }

                    if (navItemIndexToNavTo >= 0) {
                        navItemToNavTo = navItem.parent.children[navItemIndexToNavTo];
                        navItemFirstNavTo = navItem.parent.children[0];
                        if (navItemToNavTo && navItemToNavTo.item) {
                            //the navigate to board should use navigatetostate not navigatetosibling
                            if (((navItemToNavTo.item.typeAlias === 'console:navSection' && !navItemToNavTo.item.isAppTab) ||
                                navItemToNavTo.item.typeAlias === 'core:folder' ||
                                navItemToNavTo.item.typeAlias === 'core:documentFolder' ||
                                navItemToNavTo.item.typeAlias === 'core:board' ||
                                navItemToNavTo.item.typeAlias === 'console:privateContentSection')) {
                                // Cannot navigate to a navSection (that is not an application tab) or a folder.
                                // navigate to the first item of current parent item
                                if (navItemIndexToNavTo !== 0 &&
                                    navItemFirstNavTo.item && navItemFirstNavTo.item.state) {
                                    spNavService.navigateToState(navItemFirstNavTo.item.viewType, navItemFirstNavTo.item.state.params);
                                } else {
                                    if (navItem.parent.item &&
                                        navItem.parent.item.state) {
                                        spNavService.navigateToState(navItem.parent.item.viewType, navItem.parent.item.state.params);
                                    } else {
                                        spNavService.navigateToParent();
                                    }
                                }
                            }
                            else {
                                spNavService.navigateToSibling(navItemToNavTo.item.viewType, navItemToNavTo.item.id);
                            }
                        } else {
                            spNavService.navigateToParent();
                        }
                    } else {
                        if (navItem.parent.item &&
                            navItem.parent.item.state) {
                            spNavService.navigateToState(navItem.parent.item.viewType, navItem.parent.item.state.params);
                        } else {
                            spNavService.navigateToParent();
                        }
                    }
                } else {
                    spNavService.navigateToParent();
                }
            }

            /**
             * Modify the specified nav item entity in it's respective builder.
             * @param {object} entity The entity to modify.
             * @param {object} parentItem The parent nav item.
             */
            exports.modifyNavItem = function (entity, parentItem) {

                // Sanity check
                if (!spNavService.isSelfServeEditMode) {
                    console.log('Unable to modify the nav entity. spNavigationBuilderProvider is not in edit mode.');
                    return;
                }

                if (!entity) {
                    return;
                }

                var navItem = getNavItemFromEntity(entity, parentItem);

                if (!navItem) {
                    return;
                }

                switch (navItem.item.typeAlias) {
                    case 'core:chart':
                        navigateToChildStateIfUnchanaged('chartBuilder', navItem.item.id);
                        break;
                    case 'core:board':
                        navigateToChildStateIfUnchanaged('board', navItem.item.id);
                        break;
                    case 'console:screen':
                        navigateToChildStateIfUnchanaged('screenBuilder', navItem.item.id);
                        break;
                    case 'core:report':
                        navigateToChildStateIfUnchanaged('reportBuilder', navItem.item.id);
                        break;
                }
            };

            
            // Navigates to the child state if the current state is unchanged
            function navigateToChildStateIfUnchanaged(state, navItemId) {
                spNavService.navigateInternal().then(function(navResult) {
                    if (!navResult) {
                        spNavService.navigateToChildState(state, navItemId);
                        return;
                    }
                });
            }


            /**
             * Configures the specified nav item entity.
             * @param {object} entity The entity to configure.
             * @param {object} parentItem The parent nav item.
             * @param {object} defaultType the default nav item type. if can't find the nav item from spNavService.getNavTree(), use the default type
             */
            exports.configureNavItem = function (entity, parentItem, defaultType) {
                var typeAlias;

                // Sanity check
                if (!spNavService.isSelfServeEditMode) {
                    console.log('Unable to configure nav entity. spNavigationBuilderProvider is not in edit mode.');
                    return;
                }

                if (!entity) {
                    return;
                }

                var navItem = getNavItemFromEntity(entity, parentItem);

                if (!navItem) {
                    typeAlias = defaultType ? defaultType : sp.result(entity, 'getType.alias');
                } else {
                    typeAlias = navItem.item.typeAlias;
                }

                switch (typeAlias) {
                    case 'console:navSection':
                    case 'core:documentFolder':
                    case 'console:screen':
                    case 'core:folder':
                    case 'console:privateContentSection':
                        showConfigureNavEntityDialog(entity, navItem);
                        break;
                    case 'core:chart':
                        showConfigureChartEntityDialog(entity, navItem);
                        break;
                    case 'console:topMenu':
                        showConfigureApplicationEntityDialog(entity, navItem);
                        break;
                    case 'core:report':
                        showConfigureReportEntityDialog(entity, navItem);
                        break;
                    case 'core:board':
                        showConfigureBoardEntityDialog(entity, navItem);
                        break;
                    default:
                        showRenameEntityDialog(entity, navItem);
                        break;
                }
            };


            /**
             * Called when a drag operation is started.
             */
            exports.dragStart = function () {
                currentDropTarget = null;
                currentInsertionIndicatorPosition = null;
                canDropInsertionIndicatorPosition = null;
                scope.insertIndicatorDropData.data = null;
                scope.insertIndicatorDropData.target = null;
            };


            /**
             * Called when a drag operation is completed.
             */
            exports.dragEnd = function () {
                destroyInsertIndicator();
            };


            /**
             * Returns true if the source can be dropped onto the target.
             * @param {object} source The source DOM element.
             * @param {object} target The target DOM element.
             * @param {object} dragData The custom data on the source element.
             * @param {object} dropData The custom data on the target element.
             * @returns {boolean} True if the item can be dropped, false otherwise.
             */
            exports.allowDropItem = function (source, target, dragData, dropData) {
                return canDropItem(dragData, dropData);
            };


            /**
             * Handle the drag event.
             * @param {object} event The jquery event representing the drag operation.
             * @param {object} source The source DOM element.
             * @param {object} target The target DOM element.
             * @param {object} dragData The custom data on the source element.
             * @param {object} dropData The custom data on the target element.
             */
            exports.dropItem = function (event, source, target, dragData, dropData) {
                dropItem(event, source, target, dragData, dropData);
            };

            /**
             * This is called by test scripts (at least) to simulate the DnD.
             * The trick wiring this in is avoiding the event related logic, as
             * we have none.
             * @param {object} from Drop source
             * @param {object} to Drop target
             * @param {object} pos Position
             */
            exports.dropItemAction = function (from, to, pos) {
                doDropItem(from, to, pos);
            };


            /**
             * Handle the drag over.
             * @param {object} event The jquery event representing the drag operation.
             * @param {object} source The source DOM element.
             * @param {object} target The target DOM element.
             * @param {object} dragData The custom data on the source element.
             * @param {object} dropData The custom data on the target element.
             */
            exports.dragOverItem = function (event, source, target, dragData, dropData) {
                canDropInsertionIndicatorPosition = getInsertionIndicatorPosition(event, target, dragData, dropData);

                if (canDropItem(dragData, dropData)) {
                    showInsertionIndicator(event, target, dragData, dropData);
                } else {
                    hideInsertIndicator();
                }
            };


            /**
             * Handle the drag enter event.
             */
            exports.dragEnterItem = function () {
                showInsertIndicator();
            };


            /**
             * Handle the drag leave event.            
             */
            exports.dragLeaveItem = function () {
                hideInsertIndicator();
            };


            /**
             * Build the context menu for configuring the item.
             * @param {object} entity The entity to configure.
             * @returns {object} The context menu definition.
             */
            exports.buildConfigureContextMenu = function (entity) {
                var contextMenu = {
                    menuItems: []
                };
                var displayName;
                var typeAlias;
                var validForSelfService = ['core:chart', 'console:screen', 'core:report'];
                var validContextMenuWithModifyAliases = ['core:chart', 'console:screen', 'core:report',
                    'console:customEditForm'];
                var validContextMenuAliases = ['console:navSection', 'console:privateContentSection', 'console:topMenu', 'core:folder',
                    'core:documentFolder', 'core:chart', 'console:screen', 'core:report', 'console:customEditForm',
                    'core:board'];
                var validContextMenuWithPropertiesAliases = ['console:navSection', 'console:privateContentSection', 'console:topMenu', 'core:folder',
                    'core:documentFolder', 'core:chart', 'console:screen', 'core:report', 'core:board'];
                var removeableContextMenuWithPropertiesAlias = ['core:chart', 'core:report'];
                var deletableContextMenuAliases = ['console:navSection', 'console:privateContentSection', 'console:topMenu', 'core:folder',
                    'core:documentFolder', 'core:chart', 'console:screen', 'core:report', 
                    'core:board'];

                if (!entity) {
                    return contextMenu;
                }

                // Find the nav item from the entity
                var navItem = findNavNode(spNavService.getNavTree(), entity.id());

                if (!navItem) {
                    typeAlias = sp.result(entity, 'getType.alias');
                } else {
                    typeAlias = navItem.item.typeAlias;
                }

                // Check if user has permission to manipulate this type
                if (!spAppSettings.selfServeOrAdmin)
                    return contextMenu;
                if (spAppSettings.selfServeNonAdmin) {
                    if (!_.includes(validForSelfService, typeAlias))
                        return contextMenu;
                    if (!entity.isPrivatelyOwned)
                        return contextMenu;
                }

                if (_.includes(validContextMenuAliases, typeAlias)) {
                    displayName = getDisplayTypeName(navItem, entity);

                    if (_.includes(validContextMenuWithModifyAliases, typeAlias)) {
                        contextMenu.menuItems.push({
                            text: 'Modify ' + displayName,
                            icon: 'assets/images/16x16/edit.png',
                            type: 'click',
                            click: 'configMenuModifyEntity()'
                        });
                    }

                    if (_.includes(validContextMenuWithPropertiesAliases, typeAlias)) {
                        contextMenu.menuItems.push({
                            text: displayName + ' Properties',
                            icon: 'assets/images/16x16/properties.svg',
                            type: 'click',
                            click: 'configMenuUpdateEntityProperties()'
                        });
                    }

                    if (_.includes(removeableContextMenuWithPropertiesAlias, typeAlias)) {
                        contextMenu.menuItems.push({
                            text: 'Remove ' + displayName,
                            icon: 'assets/images/16x16/delete.svg',
                            type: 'click',
                            click: 'configMenuRemoveEntity()'
                        });
                    } else if (_.includes(deletableContextMenuAliases, typeAlias)) {
                        contextMenu.menuItems.push({
                            text: 'Delete ' + displayName,
                            icon: 'assets/images/16x16/delete.svg',
                            type: 'click',
                            click: 'configMenuDeleteEntity()'
                        });
                    }
                }

                return contextMenu;
            };

            exports.updateNavItem = function (entity) {
                // Find the nav item from the entity
                var navItem = findNavNode(spNavService.getNavTree(), entity.id());
                var navigationElementIcon = getNavigationElementIcon(entity);

                if (navItem) {
                    navItem.item.entity.setField('core:hideOnDesktop', entity.hideOnDesktop, spEntity.DataType.Bool);
                    navItem.item.entity.setField('core:hideOnTablet', entity.hideOnTablet, spEntity.DataType.Bool);
                    navItem.item.entity.setField('core:hideOnMobile', entity.hideOnMobile, spEntity.DataType.Bool);
                    navItem.item.entity.setLookup('console:navigationElementIcon', navigationElementIcon || null);
                    navItem.item.entity.setLookup('core:inSolution', entity && entity.inSolution ? entity.inSolution : null);
                }
            };


            return exports;
        };

        function notifyEntityUpdated(entity) {
            $rootScope.$broadcast('sp.entity.updated', entity);
        }
    }
}());