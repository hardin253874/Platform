// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console */

(function () {
    "use strict";

    angular.module('mod.app.navigation.directives', ['mod.common.spCachingCompile'])
        .directive('spNavConfigPanel', function (spCachingCompile) {
            return {
                restrict: 'E',
                scope: {                    
                    entity: '=',
                    navBuilderProvider: '=',
                    parentItem: '=',
                    emitMenuMessages: '@'
                },
                link: function ($scope, $element) {
                    var parent = $element.parent();
                    var navPanel;

                    var cachedLinkFunc = spCachingCompile.compile('navigation/directives/spNavConfigPanel/spNavConfigPanel.tpl.html');
                    cachedLinkFunc($scope, function (clone) {
                        navPanel = clone;
                        $element.append(clone);
                    });

                    $scope.model = {
                        configContextMenu: null,
                        configContextMenuIsOpen: false
                    };                    


                    function parentElementEnter() {
                        navPanel.css('visibility', 'visible');
                    }


                    function parentElementExit() {                        
                        if ($scope.model.configContextMenuIsOpen) {
                            // Don't hide the config button if the context menu is open
                            return;
                        }

                        // Hide the config panel
                        navPanel.css('visibility', 'hidden');
                    }

                    function emitMenuItemMessage() {
                        if ($scope.emitMenuMessages) {
                            $scope.$emit('onNavConfigPanelMenuItem');
                        }
                    }

                    $scope.$watch('model.configContextMenuIsOpen', function () {
                        if (!$scope.model.configContextMenuIsOpen) {
                            navPanel.css('visibility', 'hidden');
                        }
                    });


                    if ($scope.navBuilderProvider &&
                        $scope.entity) {
                        $scope.model.configContextMenu = $scope.navBuilderProvider.buildConfigureContextMenu($scope.entity);
                    }


                    parent.hover(parentElementEnter, parentElementExit);


                    $scope.click = function (event) {
                        event.stopPropagation();
                        event.preventDefault();
                    };


                    $scope.configMenuDeleteEntity = function () {
                        if ($scope.navBuilderProvider) {                  
                            emitMenuItemMessage();
                            $scope.navBuilderProvider.removeNavItem($scope.entity, true);                                                        
                        }
                    };

                    $scope.configMenuRemoveEntity = function () {
                        if ($scope.navBuilderProvider) {
                            emitMenuItemMessage();
                            $scope.navBuilderProvider.removeNavItem($scope.entity, false, $scope.parentItem);                            
                        }
                    };


                    $scope.configMenuUpdateEntityProperties = function () {
                        if ($scope.navBuilderProvider) {
                            emitMenuItemMessage();
                            $scope.navBuilderProvider.configureNavItem($scope.entity, $scope.parentItem);                            
                        }
                    };


                    $scope.configMenuModifyEntity = function () {
                        if ($scope.navBuilderProvider) {
                            emitMenuItemMessage();
                            $scope.navBuilderProvider.modifyNavItem($scope.entity, $scope.parentItem);                            
                        }
                    };


                    $scope.$on('$destroy', function () {
                        if (parent) {
                            parent.off('mouseenter mouseleave');
                            parent = null;
                        }
                    });
                }
            };
        });
}());