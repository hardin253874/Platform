// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing an entity Composite picker.
    * Displays a given report or a structure view. This directive uses 'spReport' directive.
    * 
    * @module spEntityCompositePicker
    * @example

    Using the spEntityCompositePicker:
    
    This directive uses 'spReport' directive. Pls refer to 'spReport' directive for the valid options.
           
    Options currently used by this directive are:
        - reportId - {number} - the report id or structure view id.
        - multiSelect - {boolean}. True to select many items, false otherwise.
        - selectedItems - {array of object} - the array of objects selected from report or structure view.
        - entityTypeId - {number} - the report will be constrained by the given entityTypeId. e.g. the report could be a 'template report' but it can be constrained to display instances of only 'employee' type.
    * 
    */
    angular.module('mod.common.ui.spEntityCompositePicker', ['mod.common.spEntityService', 'mod.common.spMobile'])
        .directive('spEntityCompositePicker', function ($timeout, spEntityService, spNgUtils, spMobileContext) {

            return {
                restrict: 'E',
                templateUrl: 'entityPickers/entityCompositePicker/spEntityCompositePicker.tpl.html',
                scope: {
                    options: '='
                },
                link: function (scope, iElement, iAttrs) {
                    scope.isMobile = spMobileContext.isMobile;
                    scope.isTablet = spMobileContext.isTablet;
                    scope.isDesktop = !scope.isMobile && !scope.isTablet;
                    scope.isReport = false;
                    scope.isStructureView = false;

                    // functions
                    scope.isSelected = isSelected;
                    scope.getNodeClass = getNodeClass;
                    scope.toggleOpen = toggleOpen;
                    scope.showOpen = showOpen;
                    scope.showClose = showClose;
                    scope.showSpace = showSpace;
                    scope.isNodeOpen = isNodeOpen;
                    scope.showChildren = showChildren;
                    scope.onNodeClicked = onNodeClicked;
                    scope.onNodeDoubleClicked = onNodeDoubleClicked;

                    // mobile
                    scope.onMobileNodeClicked = onMobileNodeClicked;
                    scope.onFilteredNodeClicked = onFilteredNodeClicked;
                    scope.navigateToFilteredNode = navigateToFilteredNode;
                    scope.navigateToParentVisible = navigateToParentVisible;
                    scope.navigateToChildVisible = navigateToChildVisible;
                    scope.navigateToParentNode = navigateToParentNode;
                    scope.navigateToChildNode = navigateToChildNode;

                    scope.filterActive = false;
                    var _nodes = {};
                    var quickSearchChanged;
                    var debouncedQuickSearch = _.debounce(quickSearch, 250);
                    scope.search = {};
                    setUpSearchModel(scope.search);
                    
                    scope.model = {
                        selected: [],
                        nodes: {},
                        root: []
                    };
                    

                    ///////////////////////////////////////////////////////////////////////
                    // Watches and event listeners

                    scope.$watch('search.value', function (newVal) {
                        debouncedQuickSearch();
                    });

                    scope.$watch('options.reportId', function (newVal) {
                        if (newVal) { // handle ns alias
                            spEntityService.getEntity(newVal, 'name, isOfType.alias, followRelationshipInReverse, reportUsesDefinition.{name}, structureHierarchyRelationship.{name, toName, fromName}').then(function (entity) {
                                var alias = entity.isOfType[0].nsAlias;
                                if (alias === 'core:report') {
                                    scope.isReport = true;
                                } else if (alias === 'core:structureView') {
                                    scope.isStructureView = true;
                                    scope.hierarchyRelEntity = entity.structureHierarchyRelationship;
                                    scope.typeEntity = entity.reportUsesDefinition;
                                    var typeId = sp.result(entity, 'reportUsesDefinition.idP');
                                    getTypeName(typeId);

                                    var relId = sp.result(entity, 'structureHierarchyRelationship.idP');
                                    var followInReverse, fwdRelString, revRelString;
                                    followInReverse = entity.followRelationshipInReverse;
                                    if (followInReverse) {
                                        fwdRelString = '-#' + relId;
                                        revRelString = '#' + relId;
                                    } else {
                                        fwdRelString = '#' + relId;
                                        revRelString = '-#' + relId;
                                    }
                                    scope.model.followInReverse = followInReverse ? followInReverse : false;
                                    scope.model.fwdRelString = fwdRelString;
                                    scope.model.revRelString = revRelString;
                                    //var query = 'name, #' + relId + '*.{ name, -#'+ relId + '.name }, -#' + relId + '.name';  // whole tree
                                    //var query = 'name, #' + relId + '.name'; // get only first 2 levels
                                    // get 4 levels in initial load. i.e. root, it's children, grandchildren, great grand children
                                    var query = 'name,' + fwdRelString + '.{ name, ' + fwdRelString + '.{ name, ' + fwdRelString + '.{ name, ' + revRelString + '.name }, ' + revRelString + '.name }, ' + revRelString + '.name }, ' + revRelString + '.name';
                                    var filterString = followInReverse ? "[#" + relId + "] is null" : "[#-" + relId + "] is null"; // to get the root entity, use reverse direction (fromName)
                                    // get root level entities (presumably it should also bring a node as root node if user do not have security access to the parent of a node)
                                    spEntityService.getEntitiesOfType(typeId, query, {
                                        filter: filterString, // "[Child Locations] is null"
                                        hint: 'compositePicker',
                                        isolated: true,
                                        batch: true
                                    }).then(function(typeEntities) {
                                        buildTree(typeEntities, relId);

                                        // get selected items
                                        var selectedItems = scope.options.selectedItems;
                                        if (_nodes && selectedItems && selectedItems.length > 0) {
                                            var toBeFetchedIds = [];
                                            var node;
                                            _.map(selectedItems, function (se) {
                                                 node = _nodes[se.eid];
                                                 if(!node || !node.childrenLoaded) {
                                                     toBeFetchedIds.push(se.eid);
                                                 }
                                            });

                                            if (toBeFetchedIds.length > 0) {
                                                // (in down direction) load selected node, its children and grand children. 
                                                // (going up) load selected node's parent and its children and grand children recursevely
                                                var chdrnGrandchdrnQuery = fwdRelString + '.{ name, ' + fwdRelString + '.{ name, ' + revRelString + '.name }, ' + revRelString + '.name }';  // children and grand children
                                                var chdrnGrandchdrnGreatgrandchdrnQuery = fwdRelString + '.{ name, ' + fwdRelString + '.{ name, ' + fwdRelString + '.{ name, ' + revRelString + '.name }, ' + revRelString + '.name }, ' + revRelString + '.name }';  // children, grand children and great grand children
                                                var revQuery = 'name, ' + chdrnGrandchdrnGreatgrandchdrnQuery + ', ' + revRelString + '*.{ name, ' + chdrnGrandchdrnQuery + ', ' + revRelString + '.{ name, ' + chdrnGrandchdrnQuery + ' } }';

                                                spEntityService.getEntities(toBeFetchedIds, revQuery).then(function(result) {

                                                    if (!result) {
                                                        return;
                                                    }

                                                    //fetch selected nodes and merge with main tree
                                                    loadSelectedNodes(result, relId);
                                                    scope.model.nodes = _nodes;
                                                });
                                            } else {
                                                _.forEach(selectedItems, function(entityNode) {
                                                    // add to selected  
                                                    scope.model.selected.push(entityNode.eid);

                                                    openparentNodeToRoot(entityNode.eid);
                                                });
                                                scope.model.nodes = _nodes;
                                            }

                                            // set current nodes (for mobile)
                                            scope.model.currentNodes = _.map(selectedItems, 'eid');
                                        } else {
                                            scope.model.nodes = _nodes;
                                            // set current nodes (for mobile)
                                            scope.model.currentNodes = scope.model.root;
                                        }


                                    });
                                }
                            });
                        }

                    });
                    
                    ///////////////////////////////////////////////////////////////////////
                    // scope functions
                    function isSelected(node) {
                        return node.selected.indexOf(node.id) !== -1;
                    }

                    function getNodeClass(nodeId) {
                        if (scope.model.selected.indexOf(nodeId) !== -1) {
                            return "structureItem_selected";
                        } else {
                            return "structureItem";
                        }
                    }

                    function toggleOpen(nodeId, parentId) {
                        var node = scope.model.nodes[nodeId];

                        if (!node) {
                            return;
                        }

                        loadNodeAndChildren(node);

                        // root node
                        if (!parentId) {
                            node.isOpen = !node.isOpen;
                            return;
                        }
                        
                        node.isOpenForParent[parentId] = !node.isOpenForParent[parentId];
                    }

                    function showOpen(nodeId, parentId) {
                        var node = scope.model.nodes[nodeId];

                        if (!node) {
                            return false;
                        }

                        // root node
                        if (!parentId) {
                            return (!node.isOpen && node.children.length > 0);
                        }

                        return (!node.isOpenForParent[parentId] && node.children.length > 0);
                    }

                    function showClose(nodeId, parentId) {
                        var node = scope.model.nodes[nodeId];
                        if (!node) {
                            return false;
                        }

                        // root node
                        if (!parentId) {
                            return (node.isOpen && node.children.length > 0);
                        }

                        return (node.isOpenForParent[parentId] && node.children.length > 0);
                    }

                    function showSpace(nodeId) {
                        var node = scope.model.nodes[nodeId];
                        if (node) {
                            return (node.children.length === 0);
                        }
                        return false;
                    }

                    function isNodeOpen(nodeId, parentId) {
                        var node = scope.model.nodes[nodeId];
                        if (!node) {
                            return false;
                        }

                        // root node
                        if (!parentId) {
                            return node.isOpen;
                        }
                        
                        return node.isOpenForParent[parentId];
                    }

                    function onNodeClicked(event, nodeId) {
                        if (event) {
                            scope.ctrlKeyPressed = event.ctrlKey;
                        }

                        // hack to workout if single click or double click. (when using both click and dblclick on the same element. single click fires all the time.)
                        if (scope.clicked) {
                            scope.cancelClick = true;
                            return;
                        }

                        scope.clicked = true;

                        $timeout(function () {
                            if (scope.cancelClick) {
                                scope.cancelClick = false;
                                scope.clicked = false;
                                return;
                            }

                            //do something with single click here
                            var selected = scope.model.selected;
                            if (scope.ctrlKeyPressed) {
                                if (selected.indexOf(nodeId) !== -1) {
                                    scope.model.selected = _.without(selected, nodeId); // remove
                                } else if (scope.options.multiSelect) {
                                    scope.model.selected = selected.concat(nodeId); // add
                                } else {
                                    scope.model.selected = [nodeId];    // replace
                                }
                            } else if (selected.indexOf(nodeId) === -1) {
                                scope.model.selected = [nodeId];    // replace
                            }

                            var selection = [];
                            _.forEach(scope.model.selected, function (selectedId) {
                                selection.push({ eid: selectedId });
                            });

                            scope.options.selectedItems = selection;


                            //clean up
                            scope.cancelClick = false;
                            scope.clicked = false;
                        }, 0);
                    }

                    function onNodeDoubleClicked(nodeId) {
                        var selected = scope.model.selected;
                        if (scope.options.multiSelect && selected.indexOf(nodeId) === -1) {
                            scope.model.selected = selected.concat(nodeId); // add
                        } else if (!scope.options.multiSelect) {
                            scope.model.selected = [nodeId];    // replace
                        }

                        var selection = [];
                        _.forEach(scope.model.selected, function (selectedId) {
                            selection.push({ eid: selectedId });
                        });

                        scope.options.selectedItems = selection;

                        scope.$emit('spEntityCompositePickerEventNodeDoubleClicked', scope.options.selectedItems);
                    }


                    // mobile

                    function onMobileNodeClicked(nodeId) {
                        onNodeDoubleClicked(nodeId);
                    }

                    function navigateToParentVisible(nodeId) {
                        var node = scope.model.nodes[nodeId];
                        var currentNodes = scope.model.currentNodes;

                        if (!node || !currentNodes) {
                            return false;
                        }

                        // should be visible if 
                        // 1. Node is one of the current root nodes.
                        // 2. And has atleast one parent. (i.e. node.parentIds.length > 0) 
                        return (currentNodes.indexOf(nodeId) !== -1 && node.parentIds.length > 0);
                    }

                    function showChildren(nodeId) {
                        // show node children if node is one of the current root nodes.
                        return (scope.model.currentNodes.indexOf(nodeId) !== -1);
                    }

                    function navigateToChildVisible(nodeId) {
                        // 1. if atleast one parent of the node is in current root nodes. (this implies the node is visible as a child of one of the current root nodes) 
                        // 2. AND (node has children OR node doesn't have children loaded(children should have been fetched already ??? otherwise check if children loaded?) )
                        var node = scope.model.nodes[nodeId];
                        var currentNodes = scope.model.currentNodes;

                        if (!node || !currentNodes) {
                            return false;
                        }
                        

                        var parentIsCurrentRoot = false;
                        var i = 0;
                        while (i < node.parentIds.length) {
                            if (currentNodes.indexOf(node.parentIds[i]) !== -1) {
                                parentIsCurrentRoot = true;
                                break;
                            }

                            i++;
                        }

                        return (parentIsCurrentRoot && node.children.length > 0);   // children should have been fetched already ??? otherwise check if children loaded?
                    }

                    function navigateToParentNode(nodeId) {
                        var node = scope.model.nodes[nodeId];
                        var currentNodes = scope.model.currentNodes;
                        var root = scope.model.root;
                        if (!node || !currentNodes || !root) {
                            return;
                        }

                        
                        var parentToNavigateTo;
                        if (node.navigatedFromParent && node.parentIds.indexOf(node.navigatedFromParent) !== -1) {
                            parentToNavigateTo = node.navigatedFromParent;
                        } else if (node.parentIds.length > 0) {
                            parentToNavigateTo = node.parentIds[0];
                        }

                        if (!parentToNavigateTo) {
                            return;
                        }

                        var index = currentNodes.indexOf(nodeId);
                        if (index !== -1) {
                            currentNodes.splice(index, 1, parentToNavigateTo); //replace current node with parentId that we initially navigated from
                        }

                        // special case: in mobile, search for a node and then selcet navigate to that node. Now if navigate to parent recursively and get to root node, only the root of filtered node is visible.
                        // but we want to dispaly all root nodes.
                        if (root.indexOf(parentToNavigateTo) !== -1 && root.length > currentNodes.length) {
                            scope.model.currentNodes = root.slice();
                        }
                    }

                    function navigateToChildNode(nodeId, parentId) {
                        var node = scope.model.nodes[nodeId];
                        var currentNodes = scope.model.currentNodes;

                        if (!node || !currentNodes) {
                            return;
                        }

                        if (parentId) {
                            var index = currentNodes.indexOf(parentId);
                            if (index !== -1) {
                                currentNodes.splice(index, 1, nodeId);  //replace parent with current node
                            } else {
                                currentNodes.push(nodeId);  // add current node
                            }
                        }

                        node.navigatedFromParent = parentId;

                        scope.model.currentNodes = currentNodes;

                        loadNodeAndChildren(node);
                    }

                    function navigateToFilteredNode(nodeId) {
                        var oldModel = scope.oldModel;
                        if (oldModel) {
                            var node = oldModel.nodes[nodeId];
                            if (node && node.childrenLoaded) {
                                // open selected node

                                // close children's children


                                oldModel.currentNodes = [nodeId];
                                // switch the model
                                scope.model = _.pick(oldModel, 'nodes', 'selected', 'root', 'currentNodes', 'followInReverse', 'fwdRelString', 'revRelString');

                                scope.filterActive = false;

                                setUpSearchModel(scope.search);

                                // todo: reset search
                                scope.$apply();

                            } else {
                                // fetch selected node and children
                                // todo: create a common function to 'getSelectedNodes'
                                var toBeFetchedIds = [nodeId];
                                var relId = scope.hierarchyRelEntity.idP;
                                //var revQuery = 'name, ' + scope.model.revRelString + '*.{ name, ' + scope.model.fwdRelString + '*.name, ' + scope.model.revRelString + '*.name }';
                                var fwdRelString = scope.model.fwdRelString;
                                var revRelString = scope.model.revRelString;
                                var chdrnGrandchdrnQuery = fwdRelString + '.{ name, ' + fwdRelString + '.{ name, ' + revRelString + '.name }, ' + revRelString + '.name }';  // children and grand children
                                var chdrnGrandchdrnGreatgrandchdrnQuery = fwdRelString + '.{ name, ' + fwdRelString + '.{ name, ' + fwdRelString + '.{ name, ' + revRelString + '.name }, ' + revRelString + '.name }, ' + revRelString + '.name }';  // children, grand children and great grand children
                                var revQuery = 'name, ' + chdrnGrandchdrnGreatgrandchdrnQuery + ', ' + revRelString + '*.{ name, ' + chdrnGrandchdrnQuery + ', ' + revRelString + '.{ name, ' + chdrnGrandchdrnQuery + ' } }';

                                spEntityService.getEntities(toBeFetchedIds, revQuery).then(function (result) {

                                    if (!result) {
                                        return;
                                    }
                                    _.forEach(result, function (entityNode) {
                                        var selectedNodes = loadNodeAndParent(entityNode, relId);

                                        // merge with already loaded tree nodes
                                        mergeNodes(oldModel.nodes, selectedNodes);
                                    });

                                    oldModel.currentNodes = [nodeId];
                                    // switch the model
                                    scope.model = _.pick(oldModel, 'nodes', 'selected', 'root', 'currentNodes', 'followInReverse', 'fwdRelString', 'revRelString');

                                    scope.filterActive = false;

                                    setUpSearchModel(scope.search);

                                    // todo: reset search
                                    scope.$apply();
                                });

                                // then switch the model
                            }
                        }
                    }

                    function onFilteredNodeClicked(nodeId) {
                        var oldModel = scope.oldModel;
                        if (oldModel) {
                         
                            var selected = oldModel.selected;
                            if (scope.options.multiSelect && selected.indexOf(nodeId) === -1) {
                                oldModel.selected = selected.concat(nodeId); // add
                            } else if (!scope.options.multiSelect) {
                                oldModel.selected = [nodeId];    // replace
                            }

                            var selection = [];
                            _.forEach(oldModel.selected, function (selectedId) {
                                selection.push({ eid: selectedId });
                            });

                            scope.options.selectedItems = selection;

                            scope.$emit('spEntityCompositePickerEventNodeDoubleClicked', scope.options.selectedItems);
                        }
                    }


                    ///////////////////////////////////////////////////////////////////////// 
                    // Implementation functions
                    function getTypeName(typeId) {
                        spEntityService.getEntity(typeId, 'name').then(function (entity) {
                            if (entity) {

                                scope.options.objectName = entity.name === 'Definition' ? 'Object' : entity.name;
                            }
                        });
                    }

                    // this opens selected node's parent (recursively) and stops when it finds a parent which is already open or untill it gets to the root
                    function openparentNodeToRoot(nodeId) {
                        if (!_.isNumber(nodeId)) {
                            return;
                        }
                        //var parentNode;
                        var node = _nodes[nodeId];
                        var parentIds = node.parentIds;

                        if (parentIds.length > 0) {
                            _.forEach(node.parentIds, function (pid) {

                                // update curent node
                                node.isOpenForParent[pid] = true;

                                // parent node
                                openparentNodeToRoot(pid);
                            });
                        } else {
                            // root node
                            node.isOpen = true;
                        }
                        


                        //var parentNode;
                        //var node = _nodes[nodeId];
                        //if (node && node.parentId) {
                        //    parentNode = _nodes[node.parentId];
                        //    if (parentNode && !parentNode.isOpen) {
                        //        parentNode.isOpen = true;

                        //        openparentNodeToRoot(parentNode.id);
                        //    }
                        //}
                    }

                    function loadNodeAndChildren(node) {
                        if (!node.childrenLoaded) {
                            console.error('expected children of node present');
                        }
                        var grandChildren = [];
                        var tempChildNode;
                        _.map(node.children, function (childId) {
                            tempChildNode = scope.model.nodes[childId];
                            if (tempChildNode) {
                                var tempGrandChildren = tempChildNode.children;
                                _.map(tempGrandChildren, function(child) {
                                    var nodee = scope.model.nodes[child];
                                    if ((!nodee || !nodee.childrenLoaded) && grandChildren.indexOf(nodee.id) === -1) {
                                        Array.prototype.push.apply(grandChildren, tempChildNode.children);
                                    }
                                });
                            }
                        });

                        // get children of granchildren
                        if (grandChildren.length > 0) {
                            var hierarchyRelId = sp.result(scope.hierarchyRelEntity, 'idP');
                            var query = 'name, ' + scope.model.fwdRelString + '.name, ' + scope.model.revRelString + '.name'; // also load the children of each node

                            spEntityService.getEntities(grandChildren, query).then(function (entities) {

                                if (!entities) {
                                    return;
                                }

                                var newNodes = {};
                                var sortedEntities = _.sortBy(entities, 'name');
                                _.map(sortedEntities, function (entity) {
                                    var parents = entity.getRelationship({ id: hierarchyRelId, isReverse: !scope.model.followInReverse });
                                    var parentIds = _.map(parents, 'idP');

                                    var isOpenForParent = {};
                                    _.forEach(parentIds, function (pid) {
                                        isOpenForParent[pid] = false;
                                    });
                                    //var parentId = (parents && parents.length > 0) ? parents[0].idP : undefined;

                                    var sortedChildren = _.sortBy(entity.getRelationship({ id: hierarchyRelId, isReverse: scope.model.followInReverse }), 'name');
                                    var childrenNodes = _.map(sortedChildren, 'idP');
                                    //newNodes[entity.idP] = { id: entity.idP, name: entity.name, children: childrenNodes, childrenLoaded: true, isOpen: false, parentId: entity.idP };
                                    newNodes[entity.idP] = { id: entity.idP, name: entity.name, children: childrenNodes, childrenLoaded: true, isOpen: false, parentIds: parentIds, isOpenForParent: isOpenForParent };

                                    _.map(sortedChildren, function (child) {
                                        var eid = entity.idP;
                                        newNodes[child.idP] = { id: child.idP, name: child.name, children: [], childrenLoaded: false, isOpen: false, parentIds: [eid], isOpenForParent: {eid:false} };
                                    });
                                });

                                // merge with already loaded tree nodes
                                mergeNodes(_nodes, newNodes);
                            });
                        }
                    }
                    
                    function getQuickSearchRowData() {
                        if (!scope.typeEntity || !scope.hierarchyRelEntity || !scope.search.value) {
                            // todo:  reset to old model
                            // todo: reset search flags
                            //scope.model.search.isBusy = false;
                            scope.search.isBusy = false;
                            scope.filterActive = false;

                            if (scope.oldModel) {
                                scope.model = _.pick(scope.oldModel, 'nodes', 'selected', 'root', 'currentNodes', 'followInReverse', 'fwdRelString', 'revRelString');
                                scope.$apply();
                            }
                        } else {
                            scope.search.isBusy = true;
                            scope.filterActive = true;
                            quickSearchChanged = false;

                            // keep reference to original model
                            if (!scope.oldModel) {
                                scope.oldModel = _.pick(scope.model, 'nodes', 'selected', 'root', 'currentNodes', 'followInReverse', 'fwdRelString', 'revRelString');
                            }

                            var searchText = scope.search.value;
                            var typeId = scope.typeEntity.idP;
                            var fwdRelString = scope.model.fwdRelString;
                            var revRelString = scope.model.revRelString;
                            var chdrnGrandchdrnQuery = fwdRelString + '.{ name, ' + fwdRelString + '.{ name, ' + revRelString + '.name }, ' + revRelString + '.name }';  // children and grand children
                            var chdrnGrandchdrnGreatgrandchdrnQuery = fwdRelString + '.{ name, ' + fwdRelString + '.{ name, ' + fwdRelString + '.{ name, ' + revRelString + '.name }, ' + revRelString + '.name }, ' + revRelString + '.name }';  // children, grand children and great grand children
                            var revQuery = 'name, ' + chdrnGrandchdrnGreatgrandchdrnQuery + ', ' + revRelString + '*.{ name, ' + chdrnGrandchdrnQuery + ', ' + revRelString + '.{ name, ' + chdrnGrandchdrnQuery + ' } }';

                            var filterString = "[name] like '%" + searchText + "%'";
                            spEntityService.getEntitiesOfType(typeId, revQuery, {
                                filter: filterString,
                                hint: 'compositePicker',
                                batch: true
                            }).then(function (result) {

                                // do something with result
                                // todo: tidy it up 
                                if (scope.isMobile) {
                                    loadFilteredNodesMobile(result);
                                } else {
                                    loadFilteredNodes(result);
                                }

                                if (quickSearchChanged) {
                                    // A change has been set, run again
                                    getQuickSearchRowData();
                                } else {
                                    scope.search.isBusy = false;
                                }
                            });
                        }
                    }

                    function loadFilteredNodes(filteredEntityNodes) {
                        var relId = scope.hierarchyRelEntity.idP;
                        var nodes = {};
                        var root = [];
                        var currentNodes = [];

                        _.forEach(filteredEntityNodes, function (entityNode) {

                            //if (scope.isMobile) {
                            //    root.push(entityNode.idP);
                            //    currentNodes.push(entityNode.idP);  // current nodes act as 'root' nodes in mobile when rendering
                            //} else {
                                var parentNodes = getstructureHierarchyRelationships(entityNode, relId, !scope.model.followInReverse);  // getting relationship in reverse direction gets all the parents
                                var parentNodeIds = _.map(parentNodes, 'idP');
                                var rootNodes = _.intersection(parentNodeIds, scope.oldModel.root); // assuming when the control was first launched, it brought up all the root nodes. And all of the filtered nodes are descendent of one of those root nodes.
                                root = _.union(rootNodes, root);
                            //}

                            nodes = loadFilteredNodeAndParent(nodes, entityNode, relId);
                        });

                        var filteredEntityNodeIds = _.map(filteredEntityNodes, 'idP');
                        _.forEach(nodes, function(node) {
                            if (filteredEntityNodeIds.indexOf(node.id) !== -1) {
                                node.isMatch = true;
                            }
                        });
                        scope.model.nodes = nodes;
                        scope.model.root = _.intersection(scope.oldModel.root, root);   // preserve the sort order of old model
                        scope.model.currentNodes = currentNodes;
                    }

                    function loadFilteredNodeAndParent(allNodes, entityNode, relId) {
                        // e.g. consider heirarchy 
                        //  Australia
                        //     |--Sydney
                        //     |    |--Norwest <-- selected node
                        //     |    |
                        //     |    |--Baulkham Hills
                        //     |
                        //     |--Melbourne
                        var nodes = {};
                        var tempNodes = {};
                        var selfAndParentNodes = getstructureHierarchyRelationships(entityNode, relId, !scope.model.followInReverse); // get relationship in reverse direction to get all parent nodes (Aus, Syd, Norwest in above heirarchy. incl. selected node)
                        
                        // create nodes with parentId ( dont have children info yet. do it once all nodes have been created)
                        _.forEach(selfAndParentNodes, function (entity) {
                            var parents = entity.getRelationship({ id: relId, isReverse: !scope.model.followInReverse }); // may need to revisit direction of relationship
                            var parentIds = _.map(parents, 'idP');

                            var isOpenForParent = {};
                            _.forEach(parentIds, function (pid) {
                                isOpenForParent[pid] = true;
                            });

                            //var parentId = (parents && parents.length > 0) ? parents[0].idP : undefined;
                            nodes[entity.idP] = { id: entity.idP, name: entity.name, children: [], childrenLoaded: true, isOpen: true, parentIds: parentIds, isOpenForParent: isOpenForParent };

                            // make a copy of nodes to loop through
                            tempNodes[entity.idP] = { id: entity.idP, name: entity.name, children: [], childrenLoaded: true, isOpen: true, parentIds: parentIds, isOpenForParent: isOpenForParent };
                        });

                        // update master collection and children info of nodes
                        _.forEach(tempNodes, function (node) {

                            // add to master collection 
                            if (!allNodes[node.id]) {
                                allNodes[node.id] = nodes[node.id];
                            }

                            _.forEach(node.parentIds, function (pid) {
                                var parentNode;

                                // chk if parent exists in master collection and update that (otherwise the parent will get added later)
                                parentNode = allNodes[pid];
                                parentNode = parentNode || nodes[pid];

                                if (parentNode && parentNode.children.indexOf(node.id) === -1) {
                                    parentNode.children.push(node.id);
                                }
                            });

                            //if (node.parentId) {
                            //    var parentNode;

                            //    // chk if parent exists in master collection and update that (otherwise the parent will get added later)
                            //    parentNode = allNodes[node.parentId];
                            //    parentNode = parentNode || nodes[node.parentId];

                            //    if (parentNode && parentNode.children.indexOf(node.id) === -1) {
                            //        parentNode.children.push(node.id);
                            //    }
                            //}
                        });
                        return allNodes;
                    }

                    function loadFilteredNodesMobile(filteredEntityNodes) {
                        var nodes = {};
                        var root = [];
                        var currentNodes = [];

                        _.forEach(filteredEntityNodes, function (entityNode) {

                            root.push(entityNode.idP);
                            currentNodes.push(entityNode.idP);
                            // add to selected  
                            //scope.model.selected.push(entityNode.idP);

                            // get selected nodes
                            var filteredNodes = loadNodeAndParent(entityNode, scope.hierarchyRelEntity.idP, true);

                            // merge with already loaded tree nodes
                            mergeNodes(nodes, filteredNodes);
                        });

                        var filteredEntityNodeIds = _.map(filteredEntityNodes, 'idP');
                        _.forEach(nodes, function (node) {
                            if (filteredEntityNodeIds.indexOf(node.id) !== -1) {
                                node.isMatch = true;
                            }
                        });

                        scope.model.nodes = nodes;
                        scope.model.root = root;
                        scope.model.currentNodes = currentNodes;
                    }
                    
                    function quickSearch() {
                        if (scope.search.value === scope.search.oldValue) {
                            return;
                        }

                        scope.search.oldValue = scope.search.value;

                        if (!scope.search.isBusy) {
                            // Do a search if one is not already running
                            getQuickSearchRowData();
                        } else {
                            // A search is already running. Kick off another
                            // one when the current one is done.
                            quickSearchChanged = true;
                        }
                    }

                    function setUpSearchModel(search) {
                        _.extend(search, {
                            id: 'structureViewSearchControl',
                            value: '',
                            oldValue: '',
                            isBusy: false,
                            onSearchValueChanged: debouncedQuickSearch,
                            //filterActive: false,
                            nodes: {},
                            root: []
                        });
                    }
                    
                    
                    function getstructureHierarchyRelationships(structureView, relationship, isReverse) {
                        return sp.walkGraph(
                            function (e) {
                                var b = e.getRelationship({ id: relationship, isReverse: isReverse });
                                return _.sortBy(b, 'name') || [];
                            },
                            structureView);
                    }

                    function buildTree(rootNodes, hierarchyRelId) {
                        var sortedRoots = _.sortBy(rootNodes, 'name');
                        _.map(sortedRoots, function (rootNode) {
                            var rootNodeId = rootNode.idP;
                            var rootNodeGrandChildrenIds = [];

                            scope.model.root.push(rootNodeId);

                            // get children
                            var rootNodeChildren = rootNode.getRelationship({ id: hierarchyRelId, isReverse: scope.model.followInReverse });
                            var rootNodeChildrenIds = _.map(rootNodeChildren, 'idP');

                            // get grand children
                            _.map(rootNodeChildren, function(rootNodeChild) {
                                var rootNodeGrandChildren = rootNodeChild.getRelationship({ id: hierarchyRelId, isReverse: scope.model.followInReverse });
                                var ids = _.map(rootNodeGrandChildren, 'idP');
                                Array.prototype.push.apply(rootNodeGrandChildrenIds, ids);
                            });

                            // build nodes hierarchy 
                            var flat = getstructureHierarchyRelationships(rootNode, hierarchyRelId, scope.model.followInReverse); // walk graph
                            var sortedChildren, childrenNodes, childrenLoaded, isOpen;
                            _.map(flat, function (entityNode) {
                                sortedChildren = _.sortBy(entityNode.getRelationship({ id: hierarchyRelId, isReverse: scope.model.followInReverse }), 'name');
                                childrenNodes = _.map(sortedChildren, 'idP');

                                // children loaded for root node, its children and grand children
                                childrenLoaded = (entityNode.idP === rootNode.idP) || (rootNodeChildrenIds.indexOf(entityNode.idP) != -1) || (rootNodeGrandChildrenIds.indexOf(entityNode.idP) != -1);
                                isOpen = (entityNode.idP === rootNode.idP);

                                var parents = entityNode.getRelationship({ id: hierarchyRelId, isReverse: !scope.model.followInReverse });
                                var parentIds = _.map(parents, 'idP');
                                //var parentId = (parents && parents.length > 0) ? parents[0].idP : undefined;
                                var isOpenForParent = {};
                                _.forEach(parentIds, function (pid) {
                                    isOpenForParent[pid] = isOpen;
                                });
                                
                                // assuming we are fetching root levels and their children, set childrenLoaded to true (for the root nodes)
                                _nodes[entityNode.idP] = { id: entityNode.idP, name: entityNode.name, children: childrenNodes, childrenLoaded: childrenLoaded, isOpen: isOpen, parentIds: parentIds, isOpenForParent: isOpenForParent };
                            });

                        });
                    }

                    // load the given node and its parent (with children) recursively
                    function loadNodeAndParent(node, relId, directChainOnly) {
                        // e.g. consider heirarchy 
                        //  Australia
                        //     |--Sydney
                        //     |    |--Norwest <-- selected node
                        //     |    |
                        //     |    |--Baulkham Hills
                        //     |
                        //     |--Melbourne
                        var nodes = {};
                        var selectedNodeChildrenIds = [];
                        var selectedNodeGrandChildrenIds = [];

                        // get children of selected node
                        var selectedNodeChildren = node.getRelationship({ id: relId, isReverse: scope.model.followInReverse });
                        selectedNodeChildrenIds = _.map(selectedNodeChildren, 'idP');
                        // get grand children
                        _.map(selectedNodeChildren, function (rootNodeChild) {
                            var rootNodeGrandChildren = rootNodeChild.getRelationship({ id: relId, isReverse: scope.model.followInReverse });
                            var ids = _.map(rootNodeGrandChildren, 'idP');
                            Array.prototype.push.apply(selectedNodeGrandChildrenIds, ids);
                        });

                        var parentEntityNodes = getstructureHierarchyRelationships(node, relId, !scope.model.followInReverse); // get relationship in reverse direction to get all parent nodes (Aus, Syd, Norwest in above heirarchy. incl. selected node)
                        var parentNodeIds = _.map(parentEntityNodes, 'idP');    // all the parent nodes in the chain upto root (that have children fetched)

                        var allEntityNodes = getstructureHierarchyRelationships(parentEntityNodes, relId, scope.model.followInReverse);  // this includes all parent nodes and their children (incl. Melbourne and Baulkham Hills in above heirarchy)

                        _.forEach(allEntityNodes, function (entity) {
                            var parents = entity.getRelationship({ id: relId, isReverse: !scope.model.followInReverse });
                            var parentIds = _.map(parents, 'idP');
                            var isOpenForParent = {};
                            //var parentId = (parents && parents.length > 0) ? parents[0].idP : undefined;

                            var sortedChildren = _.sortBy(entity.getRelationship({ id: relId, isReverse: scope.model.followInReverse }), 'name');
                            var childrenNodes = _.map(sortedChildren, 'idP');

                            var isOpen = parentNodeIds.indexOf(entity.idP) !== -1; // only open selected node and its parent till root
                            _.forEach(parentIds, function (pid) {
                                isOpenForParent[pid] = isOpen;
                            });

                            if (directChainOnly) {

                                childrenNodes = _.intersection(childrenNodes, parentNodeIds);

                                if (parentNodeIds.indexOf(entity.idP) !== -1) {
                                    nodes[entity.idP] = { id: entity.idP, name: entity.name, children: childrenNodes, childrenLoaded: true, isOpen: true, parentIds: parentIds, isOpenForParent: isOpenForParent };
                                }

                            } else {
                                // following have children fetched: selected node, its children and grand children
                                // selected node's parent, parent's children
                                //                          *selected node in question*  *current entity is a child of selected node*        *current entity is a grand child of selected node*         *current entity is one of the parent in the chain to root*    *grandchild of a parent in the chain upto root*
                                var entityHasChildrenLoaded = entity.idP === node.idP || selectedNodeChildrenIds.indexOf(entity.idP) !== -1 || selectedNodeGrandChildrenIds.indexOf(entity.idP) !== -1 || parentNodeIds.indexOf(entity.idP) !== -1;                    // || (parentId && parentNodeIds.indexOf(parentId) !== -1);

                                // *grandchild of a parent in the chain upto root*
                                if (!entityHasChildrenLoaded) {
                                    var i = 0;
                                    while (i < parentIds.length) {
                                        if (parentNodeIds.indexOf(parentIds[i]) !== -1) {
                                            entityHasChildrenLoaded = true;
                                            break;
                                        }
                                            
                                        i++;
                                    }
                                }
                               
                                //var isOpen = parentNodeIds.indexOf(entity.idP) !== -1; // only open selected node and its parent till root
                               
                                //_.forEach(parentIds, function (pid) {
                                //    isOpenForParent[pid] = isOpen;
                                //});

                                nodes[entity.idP] = { id: entity.idP, name: entity.name, children: childrenNodes, childrenLoaded: entityHasChildrenLoaded, isOpen: isOpen, parentIds: parentIds, isOpenForParent: isOpenForParent };
                            }
                        });

                        return nodes;
                    }

                    function mergeNodes(treeNodes, childNodes) {
                        _.forEach(childNodes, function (child) {
                            treeNodes[child.id] = child;
                        });
                    }

                    function loadSelectedNodes(selectedEntityNodes, hierarchyRelId) {
                        _.forEach(selectedEntityNodes, function (entityNode) {
                            // add to selected  
                            scope.model.selected.push(entityNode.idP);

                            // get selected nodes
                            var selectedNodes = loadNodeAndParent(entityNode, hierarchyRelId);

                            // merge with already loaded tree nodes
                            mergeNodes(_nodes, selectedNodes);
                        });
                    }
                }
            };
        });
}());