// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, _, sp, spEntity */

(function() {
    'use strict';

    angular.module('app.security.directives.spSecurityAllowDisplayControl', [
            'sp.navService',
            'mod.common.ui.spBusyIndicator',
            'mod.common.spEntityService',
            'ngGrid',
            'mod.common.ui.spContextMenu',
            'mod.common.alerts'
        ])
        .directive('spSecurityAllowDisplayControl', function() {
            return {
                restrict: 'E',
                replace: true,
                scope: {
                    options: '='
                },
                templateUrl: 'security/directives/spSecurityAllowDisplayControl/spSecurityAllowDisplayControl.tpl.html',
                controller: 'spSecurityAllowDisplayControlController'
            };
        })
        .controller('spSecurityAllowDisplayControlController', function ($scope, spNavService, spNavDataService, spEntityService, spAlertsService, $templateCache, $q) {

            // Define the data queries
            var treeQueryData = {
                    basicFields: 'alias,name,isOfType.{alias,name}',
                    navElementFields: '<%=basicFields%>,k:consoleOrder,k:isAppTab,k:navigationElementIcon.{alias, name},allowedDisplayBy.{id}',
                    behavior: '<%=basicFields%>,k:treeIconUrl,k:html5ViewId,k:consoleBehaviorHidden',
                    behaviorRels: 'k:resourceConsoleBehavior,isOfType.k:typeConsoleBehavior'
                },
                navNodesQuery = processTemplate('allowedDisplayBy.{id},k:folderContents*.{<%=navElementFields%>,{<%=behaviorRels%>}.{<%=behavior%>}}', treeQueryData);

            var subjectFields = 'name, alias, isOfType.{name, alias}';

            var originalIsDirty;

            if (!$scope.options) {
                $scope.options = {
                    mode: 'view',
                    isInDesign: false
                };
            }

            // The model
            $scope.model = {
                contextMenu: {
                    menuItems: [
                        {
                            text: 'Select all',
                            type: 'click',
                            click: 'onSelectAll(row.entity, true)',
                            disabled: '!canModifySelections()'
                        },
                        {
                            text: 'Deselect all',
                            type: 'click',
                            click: 'onSelectAll(row.entity, false)',
                            disabled: '!canModifySelections()'
                        }
                    ]
                },
                // Entire list of subjects
                availableSubjects: null,
                // Filtered subjects based on includeUsers options
                filteredSubjects: null,
                // The selected subject
                selectedSubject: null,
                // True to show users as well as roles
                includeUsers: false,
                // The currently selected application
                selectedTopMenuNode: null,
                // Cache of all nav items keyed off topMenu node id
                navNodeDataCache: {},
                // The nav items for the currently selected topMenu
                navNodeData: null,
                // allowedDisplayBy state per topMenu per subject
                // This is used to add/remove the allowDisplay relationship from
                // subjects to topMenu
                topMenusState: {},
                busyIndicator: {
                    type: 'spinner',
                    placement: 'element',
                    isBusy: false
                },
                gridOptions: {
                    showReportHeader: false,
                    enableRowSelection: false,
                    enableSorting: false,
                    data: 'model.navNodeData',                    
                    columnDefs: [
                        {
                            field: 'item.name',
                            displayName: 'Name',
                            cellTemplate: $templateCache.get('security/directives/spSecurityAllowDisplayControl/securityAllowDisplayGridCell.tpl.html')
                        }
                    ]
                }
            };

            if (sp.result($scope, '$root.__spTestMode')) {
                $scope.model.gridOptions.virtualizationThreshold = 500;
            }
            
            // Filter subjects to show users as well as roles
            function filterSubjects(subjects, includeUsers) {
                return _.filter(subjects, function(subject) {
                    return includeUsers ? true : subject.isOfType[0].nsAlias === 'core:role';
                });
            }
            
            // Expand template parameters
            function processTemplate(template, data) {
                // run twice to allow one level of template parameters within the data
                return _.template(_.template(template)(data))(data);
            }

            // Initialize the controller
            function initialize() {
                if (!$scope.options.subject) {
                    // Get all the subjects
                    spEntityService.getEntitiesOfType('core:subject', subjectFields, { isolated: true, filter: 'true' }).then(function (entities) {
                        $scope.model.availableSubjects = _.sortBy(entities, 'name');
                        // Append the type name to each of the subjects
                        _.forEach($scope.model.availableSubjects, function (s) {
                            s.displayName = s.name + ' (' + s.isOfType[0].name + ')';
                        });
                        $scope.model.filteredSubjects = filterSubjects($scope.model.availableSubjects, $scope.model.includeUsers);
                    }, function (error) {
                        spAlertsService.addAlert('An error occurred getting the roles and users: ' + (error.data.ExceptionMessage || error.data.Message), { expires: false, severity: spAlertsService.sev.Error });
                    });
                }

                var currentNavItem = spNavService.getCurrentItem();
                if (currentNavItem) {
                    
                    originalIsDirty = currentNavItem.isDirty;

                    // Initialize the dirty handler
                    currentNavItem.isDirty = isDirty;
                }
            }
            
            // Creates the initial topMenu state.
            // This is used to store the count of
            // enabled items per topMenu per subject.
            function createTopMenuState(topMenuEntity) {
                if (!topMenuEntity) {
                    throw new Error('The top menu entity must be specified.');
                }

                // Enumerate each of the subjects that currently
                // allow display of the topMenu node
                _.forEach(topMenuEntity.allowedDisplayBy, function(s) {
                    var topMenuSubjState = getTopMenuSubjectState(topMenuEntity.id(), s.id());
                    topMenuSubjState.allowDisplay = true;
                });
            }
            
            // Gets the state for the specified top menu and subject
            function getTopMenuSubjectState(topMenuId, subjectId) {
                var topMenuState, topMenuSubjState;

                if (!topMenuId || !subjectId) {
                    throw new Error('The top menu and subject must be specified.');
                }

                topMenuState = $scope.model.topMenusState[topMenuId];
                if (!topMenuState) {
                    topMenuState = {};
                    $scope.model.topMenusState[topMenuId] = topMenuState;
                }

                topMenuSubjState = topMenuState[subjectId];
                if (!topMenuSubjState) {
                    topMenuSubjState = {
                        countSelections: 0,
                        allowDisplay: undefined
                    };
                    topMenuState[subjectId] = topMenuSubjState;
                }

                return topMenuSubjState;
            }
            
            // Get the navigation items for the current application node
            function getApplicationNavItems(topMenuNode) {
                if (!topMenuNode || !topMenuNode.item) {
                    throw new Error('The top menu node is not valid.');
                }

                $scope.model.busyIndicator.isBusy = true;

                return spEntityService.getEntity(topMenuNode.item.id, navNodesQuery, { hint: 'securityCustomUIItems', batch: true, isolated: true }).then(function(entity) {
                    var depth = 0,
                        nodeList = [];

                    createTopMenuState(entity);

                    // Flatten the tree so that it can be displayed in the grid
                    flattenEntityTree(entity, entity.folderContents, nodeList, null, depth);

                    // Ensure the tenant Rollback page is never delegated out.
                    var node = _.find(nodeList,
                        function (node) {
                            return node.item.alias === 'console:tenantRollbackStaticPage';
                        });

                    if (node) {
                        _.remove(nodeList, node);
                    }

                    // Cache the items
                    $scope.model.navNodeDataCache[topMenuNode.item.id] = nodeList;

                    $scope.model.navNodeData = nodeList;
                }, function(error) {
                    spAlertsService.addAlert('An error occurred getting the items to secure: ' + (error.data.ExceptionMessage || error.data.Message), { expires: false, severity: spAlertsService.sev.Error });
                }).finally(function() {
                    $scope.model.busyIndicator.isBusy = false;
                });
            }
            
            // Get the id of the selected subject
            function getSelectedSubjectId() {
                return $scope.model.selectedSubject ? $scope.model.selectedSubject.id() : 0;
            }
            
            // Return true if the specified node is selected for the specified subject
            // false otherwise
            function getNodeSelected(node, subjectId) {
                var state;

                if (!node || !node.allowedDisplaySubjects || !subjectId) {
                    return false;
                }

                state = node.allowedDisplaySubjects[subjectId];

                return state === spEntity.DataStateEnum.Create ||
                    state === spEntity.DataStateEnum.Unchanged;
            }
            
            // Set the selected state for the specified node for the specified subject            
            function setNodeSelected(node, subjectId, isSelected) {
                var currentValue, topMenuSubjState;

                if (!node || !node.allowedDisplaySubjects || !subjectId) {
                    throw new Error('The selected node and subject must be specified.');
                }

                if (node.topMenuEntity) {
                    topMenuSubjState = getTopMenuSubjectState(node.topMenuEntity.id(), subjectId);
                }

                currentValue = node.allowedDisplaySubjects[subjectId];

                if (isSelected) {
                    // Add the relationship
                    if (currentValue === spEntity.DataStateEnum.Delete) {
                        node.allowedDisplaySubjects[subjectId] = spEntity.DataStateEnum.Unchanged;
                    } else {
                        node.allowedDisplaySubjects[subjectId] = spEntity.DataStateEnum.Create;
                    }

                    // Increment the number of selections for this app and this subject
                    if (topMenuSubjState) {
                        topMenuSubjState.countSelections = topMenuSubjState.countSelections + 1;
                    }
                } else {
                    // Delete the relationship
                    if (currentValue === spEntity.DataStateEnum.Create) {
                        delete node.allowedDisplaySubjects[subjectId];
                    } else if (currentValue === spEntity.DataStateEnum.Unchanged) {
                        node.allowedDisplaySubjects[subjectId] = spEntity.DataStateEnum.Delete;
                    }

                    // Decrement the number of selections for this app and this subject
                    if (topMenuSubjState &&
                        topMenuSubjState.countSelections &&
                        (currentValue === spEntity.DataStateEnum.Create ||
                            currentValue === spEntity.DataStateEnum.Unchanged)) {
                        topMenuSubjState.countSelections = topMenuSubjState.countSelections - 1;
                    }
                }
            }
            
            // Set the state for the specified node for the specified subject
            function setNodeState(node, subjectId, stateEnum) {
                var topMenuSubjState;

                if (!node || !node.allowedDisplaySubjects || !subjectId) {
                    throw new Error('The selected node and subject must be specified.');
                }

                node.allowedDisplaySubjects[subjectId] = stateEnum;

                if (node.topMenuEntity) {
                    topMenuSubjState = getTopMenuSubjectState(node.topMenuEntity.id(), subjectId);
                    topMenuSubjState.countSelections = topMenuSubjState.countSelections + 1;
                }
            }
            
            // Flatten the entity tree into a list
            function flattenEntityTree(topMenuEntity, entities, nodeList, parentNode, depth) {
                var navNodes;

                if (!entities || !nodeList) {
                    return;
                }
                //remove the duplicate entities from node tree.
                entities = _.filter(entities, function (entity) {
                    return ((parentNode == null) ||
                              (entity != null &&
                              parentNode != null &&
                              parentNode.item != null &&
                              entity.id() != parentNode.item.id));
                });

                // Convert the entities to navitems and sort
                navNodes = spNavService.sortNavNodes(_.map(entities, function (entity) {
                    return {
                        item: spNavDataService.navItemFromEntity(entity)
                    };
                }));

                _.forEach(navNodes, function (navNode) {
                    var childEntities,
                        node = {
                            item: navNode.item,
                            depth: depth,
                            parent: parentNode,
                            allowedDisplaySubjects: {},
                            topMenuEntity: topMenuEntity
                        },
                        subjects = navNode.item.entity.allowedDisplayBy;

                    // Any existing subjects start off with a state of unchanged
                    _.forEach(subjects, function(s) {
                        setNodeState(node, s.id(), spEntity.DataStateEnum.Unchanged);
                    });

                    Object.defineProperty(node, 'selected', {
                        get: function() {
                            return getNodeSelected(this, getSelectedSubjectId());
                        },
                        set: function(val) {
                            setNodeSelected(this, getSelectedSubjectId(), val);
                        },
                        enumerable: true,
                        configurable: true
                    });

                    nodeList.push(node);

                    childEntities = navNode.item.entity.folderContents;
                    
                    flattenEntityTree(topMenuEntity, childEntities, nodeList, node, depth + 1);
                });
            }
            
            // Returns true if the page is dirty
            function isDirty() {
                var dirty = false;

                if (originalIsDirty) {
                    dirty = originalIsDirty();
                }

                if (!dirty) {
                    dirty = _.some(_.values($scope.model.navNodeDataCache), function (navNodesData) {
                        return _.some(navNodesData, function (navNode) {
                            return _.some(_.values(navNode.allowedDisplaySubjects), function (state) {
                                return state !== spEntity.DataStateEnum.Unchanged;
                            });
                        });
                    });
                }

                return dirty;
            }
            
            // Switch to view mode and get data from server.
            function resetToViewMode() {
                $scope.options.mode = 'view';

                if (isDirty()) {
                    // Reset and get changes from server
                    $scope.model.navNodeDataCache = {};                    
                    $scope.model.topMenusState = {};

                    $scope.onTopMenuNodeChanged();
                }
            }            
            
            // Cancel any changes
            function onCancel() {                
                resetToViewMode();                
            }
            
            // Save any changes
            function onSave() {
                var promises = [], subjectEntities = {};

                if ($scope.options.subject) {
                    subjectEntities[$scope.options.subject.idP] = $scope.options.subject;
                }

                // Create and cache the subject entity
                function createSubjectEntity(subjectId) {
                    var subjectEntity = subjectEntities[subjectId];

                    if (!subjectEntity) {
                        subjectEntity = spEntity.fromJSON({
                            id: subjectId
                        });
                        subjectEntity.registerRelationship('core:allowDisplay');
                        subjectEntity.setDataState(spEntity.DataStateEnum.Update);

                        subjectEntities[subjectId] = subjectEntity;
                    }

                    return subjectEntity;
                }

                // Create a new resource
                function createResource(id) {
                    var resource = spEntity.fromJSON({
                        id: id
                    });

                    resource.setDataState(spEntity.DataStateEnum.Unchanged);

                    return resource;
                }
                
                // Sanity check
                if ($scope.options.mode !== 'edit') {
                    return;
                }

                // Add/remove any relationships to topMenus
                _.forOwn($scope.model.topMenusState, function(topMenuState, topMenuId) {
                    _.forOwn(topMenuState, function(topMenuSubjState, subjectId) {
                        var subjectEntity, topMenu;

                        if (!topMenuSubjState.countSelections) {
                            // There are no selections for this topMenu and subject
                            // Remove the relationship from the subject to the topMenu
                            if (topMenuSubjState.allowDisplay) {
                                subjectEntity = createSubjectEntity(subjectId);
                                topMenu = createResource(topMenuId);
                                subjectEntity.allowDisplay.remove(topMenu);
                            }
                        } else {
                            // There are selections for this topMenu and subject
                            // Add the relationship from the subject to the topMenu
                            if (!topMenuSubjState.allowDisplay) {
                                subjectEntity = createSubjectEntity(subjectId);
                                topMenu = createResource(topMenuId);
                                subjectEntity.allowDisplay.add(topMenu);
                            }
                        }
                    });
                });

                // Add/remove any relationships to resources
                _.forOwn($scope.model.navNodeDataCache, function(navNodesData) {
                    _.forEach(navNodesData, function(navNode) {
                        _.forOwn(navNode.allowedDisplaySubjects, function(state, subjectId) {
                            var subjectEntity, resource;

                            if (state === spEntity.DataStateEnum.Unchanged) {
                                return true;
                            }

                            subjectEntity = createSubjectEntity(subjectId);
                            resource = createResource(navNode.item.id);

                            switch (state) {
                            case spEntity.DataStateEnum.Create:
                                subjectEntity.allowDisplay.add(resource);
                                break;
                            case spEntity.DataStateEnum.Delete:
                                subjectEntity.allowDisplay.remove(resource);
                                break;
                            }

                            return true;
                        });
                    });
                });

                _.forOwn(subjectEntities, function (subjectEntity) {
                    // save, if not under control of edit form
                    if (subjectEntity.dataState !== spEntity.DataStateEnum.Create) {
                        promises.push(spEntityService.putEntity(subjectEntity));
                    }
                });

                $scope.model.busyIndicator.isBusy = true;

                $q.all(promises).then(function () {
                    $scope.model.busyIndicator.isBusy = false;
                    spAlertsService.addAlert('UI customisations saved.', { expires: true, severity: spAlertsService.sev.Success });
                    resetToViewMode();
                }, function (error) {
                    $scope.model.busyIndicator.isBusy = false;
                    spAlertsService.addAlert('An error occurred saving the UI customisations: ' + (error.data.ExceptionMessage || error.data.Message),
                    { expires: false, severity: spAlertsService.sev.Error });
                });
            }
            
            // Switch to edit mode
            function onEdit() {
                $scope.options.mode = 'edit';
            }
            
            // Called when current application is changed
            $scope.onTopMenuNodeChanged = function() {
                var cachedNavNodeData;

                if (!$scope.model.selectedTopMenuNode ||
                    !$scope.model.selectedTopMenuNode.item) {
                    $scope.model.navNodeData = null;
                    return;
                }

                cachedNavNodeData = $scope.model.navNodeDataCache[$scope.model.selectedTopMenuNode.item.id];

                if (!cachedNavNodeData) {
                    getApplicationNavItems($scope.model.selectedTopMenuNode);
                } else {
                    $scope.model.navNodeData = cachedNavNodeData;
                }
            };

            // Get the path for the current node
            // Used in a tooltip.
            $scope.getNodePath = function(node) {
                var path,
                    parentName;

                if (!node ||
                    !node.item) {
                    return '';
                }

                path = node.item.name;

                while (node && node.parent) {
                    parentName = sp.result(node, 'parent.item.name');

                    path = parentName + ' > ' + path;

                    node = node.parent;
                }

                return path;
            };

            // Handle any actions
            $scope.$on('allowDisplayAction', function(event, args) {
                if (!args || !args.action) {
                    return;
                }

                switch (args.action) {
                case 'save':
                    onSave();
                    break;
                case 'cancel':
                    onCancel();
                    break;
                case 'edit':
                    onEdit();
                    break;
                }
            });

            $scope.$watch('options.subject', function () {
                if ($scope.options.subject) {
                    $scope.model.selectedSubject = $scope.options.subject;
                }
            });

            // Returns true if the selections can be changed, false otherwise
            $scope.canModifySelections = function() {
                return $scope.model.selectedSubject &&
                       $scope.model.navNodeData &&
                       $scope.options.mode === 'edit';
            };
            
            // Select/deselect all children of the current row
            $scope.onSelectAll = function(node, val) {
                var currentIndex, i, child;

                if (!node || !$scope.canModifySelections()) {
                    return;
                }

                // check the current row
                node.selected = val;
                // check the parent row (bug 25838)
                if (node && node.parent)
                    node.parent.selected = val;

                currentIndex = _.indexOf($scope.model.navNodeData, node);

                if (currentIndex < 0) {
                    return;
                }

                // Check all children
                for (i = currentIndex + 1; i < $scope.model.navNodeData.length; i = i + 1) {
                    child = $scope.model.navNodeData[i];

                    if (!child || child.depth <= node.depth) {
                        break;
                    }

                    child.selected = val;
                }
            };
            
            // The include users checked box has changed
            $scope.onIncludeUsersChanged = function() {
                $scope.model.filteredSubjects = filterSubjects($scope.model.availableSubjects, $scope.model.includeUsers);
            };
            
            // Get the list of applications, include an index to help with the SELECT option track by
            $scope.getMenuNodes = function () {
                return _.map(spNavService.getMenuNodes(true), function (node, index) {
                    return _.merge(node, {index:index});
                });                
            };
            
            // Called when the selected property of a node has changed
            // due to user interaction.
            $scope.onNodeSelectedChanged = function(node) {
                if (!node || !node.selected) {
                    return;
                }

                // Check all the items up the tree                                
                // when the curent node is selected
                while (node && node.parent) {                    
                    node = node.parent;
                    node.selected = true;
                }                
            };

            initialize();
        });
}());