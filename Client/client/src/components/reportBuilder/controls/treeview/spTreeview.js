// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    
    /**
   *  Module implementing a tree view.
   *  reportbuilder sturcture build control
   * @module spTreeview    
   * @example
       
   Using the spAnalyzer:
   
   &lt;sp-treeview tree-data="tree-data"&gt;&lt;/sp-treeview&gt         

   where tree-data is the report relationship entities tree:
          node-id is the attribute name of entity node id
          node-name is the attribute name of entity node name
          node-state is the attribute of the state, the default value is expand
          expand-icon the image icon of expand
          collapse-icon the image icon of collapse

     
   */
    angular.module('mod.common.ui.spTreeview', [])
        .directive('summariseTreeNode', function ($compile, spSummariseOptionService) {
            return {
                restrict: 'E',
                template: '<li ><div class=\"{{getNodeClass(childitem)}}\" ng-click=\"selectNode(childitem)\">{{childitem.name}} <img  class=\"{{getSummarizeClass(childitem)}}\" ng-src=\"{{getSummarizeIcon(childitem)}}\" /></div></li>',
                link: function (scope, elm, attrs) {
                    scope.spSummariseOptionService = spSummariseOptionService;

                    scope.$watch('childitem.children.length', function () {
                        if (scope.childitem && scope.childitem.children && scope.childitem.children.length > 0) {
                            var children = $compile('<summarise-tree treeitems="childitem.children" selectednodeid="selectednodeid"></summarise-tree>')(scope);
                            elm.append(children);
                        }
                    });

                    scope.getNodeClass = function (childitem) {
                        
                       //return 'summariseNode';
                        if (scope.spSummariseOptionService.getSelectedNodeId() === childitem.nid) {
                            return 'summariseSelectedNode';
                        } else {
                            if (childitem.underSummariseId) {
                                return 'underSummariseNode';
                            } else {
                                return 'summariseNode';
                            }
                        }
                    };

                    scope.getSummarizeClass = function(childitem) {
                        if (childitem && childitem.pae) {
                            return 'displaysummariseicon';
                        } else {
                            return 'hiddensummariseicon';
                        }
                    };

                    scope.getSummarizeIcon = function(childitem) {
                        if (scope.spSummariseOptionService.getSelectedNodeId() === childitem.nid) {
                            
                            return 'assets/images/relationship_summary.png';
                        } else {
                            return 'assets/images/relationship_summary_darken.png';
                        }
                    };

                    scope.selectNode = function(childitem) {
                        //scope.$parent.selectednodeid = childitem.nid;
                        if (childitem.underSummariseId) {
                            //do nothing
                        } else {
                            scope.spSummariseOptionService.setSelectedNodeId(childitem.nid);
                            _.delay(function() {
                                scope.$apply();
                            });
                        }
                    };
                }
            };
        })
        .directive('summariseTree', function ($compile, spSummariseOptionService) {
            return {
                template: '<ul><summarise-tree-node ng-repeat="childitem in treeitems"></summarise-tree-node></ul>',
                restrict: 'E',
                replace: true,
                scope: {
                    treeitems: '=treeitems'
                }, 
                link: function (scope) {
                    scope.spSummariseOptionService = spSummariseOptionService;

                    scope.$watch('treeitems', function() {
                        if (scope.items) {
                            
                        }
                    });
                    
                    scope.$watch('spSummariseOptionService.getSelectedNodeId()', function () {
                        if (scope.spSummariseOptionService.getSelectedNodeId()) {
                            _.delay(function () {
                                scope.$apply();
                            });
                        }
                    });
                }
            };
        })
        .directive('spTree', function (spReportBuilderService) {
            return {
                templateUrl: 'reportBuilder/controls/treeview/spTreeview.tpl.html',
                restrict: 'E',
                replace: true,
                transclude: false,
                controller: 'treeviewController',
                scope: {
                    options: '=options'
                }
                //,link: function (scope, elm) {
                //    scope.$watch('options.treeNodeChanged', function () {
                //        if (scope.options.treeNodeChanged) {
                //            elm.empty();
                //        }
                //    });
                //}
            };
        })
        .directive('spTreeNode', function ($compile, spReportBuilderService) {
            return {
                restrict: 'E',
                templateUrl: 'reportBuilder/controls/treeview/spTreeviewItem.tpl.html',                                
                controller: 'treeviewNodeController',
                transclude: false,
                link: function (scope, elm, attrs) {

                    scope.$watch('node.children.length', function (newVal, oldVal) {
                        scope.updateChildNode();
                    });
               
                    scope.updateChildNode = function() {
                        if (scope.node && scope.node.children.length > 0) {
                            if (elm.children().length === 2) {
                                //use angular element to wrap the last child node of current element to support IE, firefox and chome
                                angular.element(elm.children()[1]).remove();
                            }
                            scope.childOptions = { treeNodes: scope.node.children };

                            var children = $compile('<sp-tree options="childOptions"></sp-tree>')(scope);
                            elm.append(children);
                        } else {
                            scope.childOptions = { treeNodes: [] };
                            
                        }
                    };

                }
            };
        })
        .controller("treeviewController", function ($scope,  spReportBuilderService) {
            $scope.model = { treeviewNodes: [], selectedNode: null, selectedNodeId : 0 };
            $scope.spReportBuilderService = spReportBuilderService;
           
            $scope.$watch('options.treeNodes', function () {
                $scope.model.treeviewNodes = [];                

                if ($scope.options && $scope.options.treeNodes) {
                    $scope.model.treeviewNodes = $scope.options.treeNodes;
                } 
            });

            $scope.$watch('spReportBuilderService.getSelectedNode()', function () {
                if ($scope.spReportBuilderService.getSelectedNode()) {
                    $scope.model.selectedNode = $scope.spReportBuilderService.getSelectedNode();
                    $scope.model.selectedNodeId = $scope.model.selectedNode.nid;
                }
            });
        })
        .controller("treeviewNodeController", function ($scope,  spReportBuilderService) {      
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Treeview Action Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////       
            
        $scope.getNodeClass = function (childnode) {
            if ($scope.options && childnode && childnode.pnid === 0) {
                $scope.nodeToggleImage = "assets/images/fieldgroup_opened.png";
                $scope.toggleStatus = "expand";
                $scope.showChildren = true;
                return 'report-Builder-Treeview-Rootnode';
            } else {
                $scope.nodeToggleImage = null;
                $scope.toggleStatus = null;
                $scope.showChildren = true;
                return 'report-Builder-Treeview-Item';
            }
        };

        $scope.getSelectedNodeStatusClass = function (childnode) {
            if ($scope.options && childnode && childnode.nid === $scope.model.selectedNodeId) {
                if ($scope.options && childnode && childnode.pe) {
                    return 'selectedchildnodearea';
                } else {
                    return 'selectedrootnodearea';
                }
            } else {
                if ($scope.options && childnode && childnode.pe) {
                    return 'childnodearea';
                }
                else {
                    return 'rootnodearea';
                }
            }
        };

        $scope.getSelectedStatusClass = function (childnode) {
            if ($scope.options && childnode !== undefined && childnode && childnode.nid === $scope.model.selectedNodeId) {
                if (childnode.pnid === 0) {
                    return 'selectedrootnodename';
                } else {
                    return 'selectedchildnodename';
                }
            } else {
                if (childnode && childnode.pnid === 0) {
                    return 'rootnodename';
                } else {
                    return 'childnodename';
                }
            }
        };

        $scope.getNodeTypeImageClass = function (childnode) {
            if ($scope.options && childnode) {
                return 'node' + childnode.reltype + 'typeimage';
            } else {
                return 'nodetypeimage';
            }
        };

        $scope.getRemoveButtonClass = function (childnode, selectedNodeId) {
            if ($scope.options && childnode && childnode.nid === $scope.model.selectedNodeId && childnode.reltype !== 'resourceReportNode') {              
                return 'displayremovebutton';
            } else {
                return 'hideremovebutton';
            }
        };

        $scope.getSummarizeClass = function(childnode) {
            if ($scope.options && childnode && childnode.pae) {
                return 'displaysummariseicon';
            } else {
                return 'hiddensummariseicon';
            }
        };

        $scope.selectChildNode = function (childnode) {
            if ($scope.options && childnode && childnode.nid !== $scope.model.selectedNodeId) {
                spReportBuilderService.setAction('updateSelectedNode', childnode, null);
            }
        };

        $scope.toggleChildren = function () {
            if ($scope.toggleStatus === "collapse") {
                $scope.toggleStatus = "expand";
                $scope.nodeToggleImage = "assets/images/fieldgroup_opened.png";
                $scope.showChildren = true;
            } else {
                $scope.toggleStatus = "collapse";
                $scope.nodeToggleImage = "assets/images/fieldgroup_collapsed.png";
                $scope.showChildren = false;
            }

        };

        $scope.removeChildNode = function (childnode) {
            if ($scope.options && childnode) {
                spReportBuilderService.setAction('removeNode', childnode, null);
            }
        };

    });
        
}());