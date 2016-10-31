// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, sp */

(function() {
    "use strict";

    angular.module("app.controls.dialog.spNavContainerPickerDialog")
        .controller("spNavContainerPickerDialogController", spNavContainerPickerDialogController);

    /* @ngInject */
    function spNavContainerPickerDialogController($scope, $uibModalInstance, options, spNavService, focus, $timeout) {
        $scope.model = {
            selectedTopMenuNode: null,
            selectedNavTreeItems: [],
            applicationNavTreeItemCache: {},
            selectedNode: null,
            menuNodes: [],
            options: options || {}
        };

        const model = $scope.model;

        // Handle top menu node changes
        $scope.onTopMenuNodeChanged = function() {
            const selectedTopMenuNodeId = sp.result(model, "selectedTopMenuNode.item.id");

            if (!selectedTopMenuNodeId) {
                model.selectedNavTreeItems = [];
                return;
            }            

            model.selectedNavTreeItems = model.applicationNavTreeItemCache[selectedTopMenuNodeId] || [];
        };

        // Get the classes for the anchor node
        $scope.getAClass = function(node) {
            const classes = ["navLink"];

            if (model.selectedNode &&
                node === model.selectedNode) {
                classes.push("selectedNode");
            }

            return _.join(classes, " ");
        };

        // Get the open closed icon
        $scope.getOpenClosedIcon = function(node) {
            return node && node.isOpen ? "assets/images/hierarchy_opened.png" : "assets/images/hierarchy_collapsed.png";
        };

        // Get open closed icon style
        $scope.getOpenClosedIconStyle = function(node) {
            return {
                'visibility': $scope.canOpen(node) ? "visible" : "hidden"
            };
        };

        // Get node style
        $scope.getNodeStyle = function (node) {
            const depth = sp.result(node, "item.depth");

            if (!depth || depth < 2) {
                return null;
            }

            return {
                'margin-left': ((depth - 2) * 20) + "px"
            };
        };

        // True if node can be opened, false otherwise
        $scope.canOpen = function(node) {
            return node && node.children && node.children.length;
        };

        // Toggle node is open
        $scope.toggleIsOpen = function(node) {
            if (!$scope.canOpen(node)) {
                return;
            }

            node.isOpen = !node.isOpen;
        };

        // True if ok button is disabled, false otherwise
        $scope.isOkDisabled = function() {
            return !model.selectedNode || !model.selectedNode.item;
        };

        // Get focus id for node
        $scope.getNodeFocusId = function(node) {
            return `navContainerPicker_${sp.result(node, "item.id")}`;
        };

        // Get current path
        $scope.getCurrentPath = function() {
            if (!model.selectedNode) {
                return "Empty";
            }

            // calculate path up to root
            const nodeNames = [];

            let currentNode = model.selectedNode;

            while (currentNode) {
                nodeNames.push(currentNode.item.name);
                currentNode = currentNode.parent;
            }

            return _.join(_.reverse(nodeNames), "/");
        };

        // Ok callback
        $scope.ok = function () {
            if ($scope.isOkDisabled()) {
                return;
            }

            $uibModalInstance.close(model.selectedNode.item.id);
        };

        // Close callback
        $scope.close = function() {
            $uibModalInstance.close(null);
        };

        // Node click callback
        $scope.onNodeClick = function(node) {
            if (!canSelect(node)) {
                return;
            }

            model.selectedNode = node;
        };

        function createNavTreeNodes() {
            // Get the app nodes
            const menuNodes = [];

            // Clone and cache the top menu and nav sections the user has access to
            _.forEach(getMenuNodes(), function(menuNode) {
                const topMenuNodeId = sp.result(menuNode, "item.id");

                if (!topMenuNodeId) {
                    return;
                }

                const navTreeForMenuNode = cloneNode(menuNode);
                model.applicationNavTreeItemCache[topMenuNodeId] = navTreeForMenuNode;

                if (sp.result(navTreeForMenuNode, "children.length")) {
                    menuNodes.push(menuNode);
                }
            });

            // Show app nodes that actually have content
            model.menuNodes = menuNodes;
        }

        function initialise() {
            createNavTreeNodes();            

            if (!model.options.selectedContainerId) {
                return;
            }

            const pathInReverse = [];
            const selectedNode = spNavService.findInTreeById(spNavService.getNavTree(), model.options.selectedContainerId, pathInReverse);

            if (!selectedNode) {
                return;
            }

            // Find topMenuid for selected node
            const topMenuNodeId = sp.result(_.findLast(pathInReverse, n => sp.result(n, "item.typeAlias") === "console:topMenu"), "item.id");

            if (!topMenuNodeId) {
                return;
            }

            // Get the top menu node
            const topMenuNode = _.find(model.menuNodes, n => sp.result(n, "item.id") === topMenuNodeId);

            if (!topMenuNode) {
                return;
            }

            initializeSelectedItems(topMenuNode, model.options.selectedContainerId);
        }

        function initializeSelectedItems(topMenuNode, selectedContainerId) {
            model.selectedTopMenuNode = topMenuNode;

            $scope.onTopMenuNodeChanged();

            // Find and set the select node        
            let nodesToProcess = model.selectedNavTreeItems ? _.concat(model.selectedNavTreeItems) : [];

            while (nodesToProcess.length) {
                const node = _.head(_.pullAt(nodesToProcess, 0));

                if (node.item && node.item.id === selectedContainerId) {
                    model.selectedNode = node;
                    break;
                }

                if (node.children && node.children.length) {
                    nodesToProcess = _.concat(nodesToProcess, node.children);
                }
            }

            if (model.selectedNode) {
                $timeout(() => focus($scope.getNodeFocusId(model.selectedNode)), 200);
            }
        }

        function cloneItem(item) {
            if (!item) {
                return null;
            }

            const typeAlias = item.typeAlias;

            return {
                alias: item.alias,
                applicationId: item.applicationId,
                depth: item.depth,
                hidden: item.hidden,
                hiddenByConfig: item.hiddenByConfig,
                iconUrl: typeAlias === "console:privateContentSection" ? "assets/images/nav/PrivateFolder.svg" : "assets/images/nav/Folder.svg",
                id: item.id,
                name: item.name,
                order: item.order,
                typeAlias: item.typeAlias                      
            };
        }

        function cloneNode(sourceNode) {
            if (!sourceNode || !sourceNode.item) {
                return null;
            }

            const typeAlias = sp.result(sourceNode, "item.typeAlias");
            const depth = sp.result(sourceNode, "item.depth");

            if (depth > 3) {
                return null;
            }

            if (model.options.isSelfServeMode) {
                if ((typeAlias !== "console:navSection" || depth !== 2) &&
                    typeAlias !== "console:privateContentSection" &&
                    typeAlias !== "console:topMenu") {
                    return null;
                }
            } else {
                if (typeAlias !== "console:navSection" &&
                    typeAlias !== "console:privateContentSection" &&
                    typeAlias !== "console:topMenu") {
                    return null;
                }
            }

            const clonedNode = {
                item: cloneItem(sourceNode.item),
                children: [],
                parent: null,
                isOpen: true
            };

            _.forEach(sourceNode.children, function(sourceChildNode) {
                const clonedChildNode = cloneNode(sourceChildNode);
                if (clonedChildNode) {
                    clonedNode.children.push(clonedChildNode);
                    clonedChildNode.parent = clonedNode;
                }
            });

            spNavService.sortNavNodes(clonedNode.children);

            if (model.options.isSelfServeMode &&
                !clonedNode.children.length &&
                ((typeAlias === "console:navSection" &&
                        depth === 2) ||
                    typeAlias === "console:topMenu")) {
                return null;
            }

            return clonedNode;
        }

        function getMenuNodes() {
            const menuNodes = spNavService.getMenuNodes(true);

            return _.filter(menuNodes, n => n.item && !n.item.hiddenByConfig);
        }

        function canSelect(node) {
            if (!node || !node.item) {
                return false;
            }

            const typeAlias = sp.result(node, "item.typeAlias");

            if (model.options.isSelfServeMode) {
                return typeAlias === "console:privateContentSection";
            } else {
                return typeAlias === "console:navSection" || typeAlias === "console:privateContentSection";
            }
        }

        initialise();
    }
})();