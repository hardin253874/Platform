// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    "use strict";

    /**
     * Application navigation related directives and other.
     */

    angular.module('sp.app.mobileNavigation', ['hmTouchEvents', 'sp.navService', 'mod.app.navigationProviders', 'mod.app.navigation.directives', 'mod.common.spEntityService', 'mod.common.ui.spUserPasswordDialog', 'sp.common.loginService', 'sp.app.settings', 'hmTouchEvents', 'sp.themeService', 'sp.consoleIconService'])
         .controller('MobileNavController', function ($scope, $timeout, $location, spNavService, $state, spState, spAppSettings, spThemeService, $window, consoleIconService) {

             // ReSharper disable once InconsistentNaming
             var MAX_LIST_LENGTH = 200;             // The maximum length of the nav list, anything more is dropped off
             $scope.appSettings = spAppSettings;
             
             //default logo and style

             $scope.consoleThemeModel = {
                 consoleTheme: null,
                 tenantTheme: null,
                 defaultLogo: 'assets/images/logo_RN.svg',
                 navBarStyle: {},
                 menuStyle: {},
                 titleStyle: {},
                 tabStyle: {},
                 tabItemStyle: {},
                 tabSelectedItemStyle: {},
                 leftNavStyle: {},
                 leftNavItemStyle: {},
                 leftNavSelectedItemStyle: {},
                 leftNavFontStyle: {},
                 leftNavSelectedFontStyle: {},
                 updateFlag: ''
             };
           
            $scope.getTabItemStyle = function (selected) {
                 if (selected) {
                     return $scope.consoleThemeModel.tabSelectedItemStyle;
                 } else {
                     return $scope.consoleThemeModel.tabItemStyle;
                 }
            };
            $scope.getLeftNavItemStyle = function (selected) {
                if (selected) {
                    return $scope.consoleThemeModel.leftNavSelectedItemStyle;
                } else {
                    return $scope.consoleThemeModel.leftNavItemStyle;
                }
            };
            $scope.getLeftNavFontStyle = function (selected) {
                if (selected) {
                    return $scope.consoleThemeModel.leftNavSelectedFontStyle;
                } else {
                    return $scope.consoleThemeModel.leftNavFontStyle;
                }
            };
            $scope.getApplicationTabStyleClass = function () {
                if ($scope.consoleThemeModel.consoleTheme &&
                    $scope.consoleThemeModel.consoleTheme.consoleTopNavigationStyle) {

                    var consoleTopNavigationStyle = spThemeService.getConsoleTopNavigationStyle();

                    if (consoleTopNavigationStyle && consoleTopNavigationStyle !== 'Default') {
                        return consoleTopNavigationStyle;
                    } else {
                        return '';
                    }
                } else {
                    return '';
                }
            };

            $scope.getIconStyle = function (item) {

                if (!item || !item.entity) {
                    return undefined;
                }

                var bColor = consoleIconService.getNavItemIconCssBackgroundColor(item.entity);

                if (bColor) {
                    return { 'background-color': bColor };
                }

                return undefined;
            };

            /**
             * The NavController is an AngularJS controller that prepares the navigation structure
             * in a format suitable for presentation. It is the thing that is aware of our app
             * using three levels; menu, then tabs, then nav tree.
             *
             * We are making assumptions that the nav service tree and paths are never null.
             *
             */

            function isAppLauncher() {
                //temporary to get things in place for some styling, but this is not the way!
                return $state.current.name === 'landing' || $state.current.name === 'landinghome';
            }

            function getBreadcrumb() {
                return spNavService.getBreadcrumb();
            }

            function getNodeForItem(item) {
                
                return item ? spNavService.findInTreeById(spNavService.getNavTree(), item.id) : null;
            }



            function getItemInBreadcrumb(items) {
                if (!items || !items.length) {
                    return null;
                }
                return _.find(getBreadcrumb(), function (item) {
                    return _.some(items, { id: item.id });
                });
            }

            function getActiveNodeAncestor(distance) {
                var bc = getBreadcrumb();
                var node = null;
                if (bc.length > distance) {
                    node = getNodeForItem(bc[bc.length - distance - 1]);
                }
                return node;
            }

            function notifyUpdateLayout() {
                $timeout(function () {
                    $scope.$root.$broadcast('app.layout');

                    $timeout(function () {
                        $scope.$root.$broadcast('app.layout.postAnimate');
                    }, 0);      // WARNING: This will need to change if the animation length changes.
                }, 0);          // Note: If you want to make larger than zero, document reason here.
            }

            function isContainerItem(item) {
                return item && (item.typeAlias === 'console:navSection' || item.typeAlias === 'core:folder' || item.typeAlias === 'core:documentFolder' || item.typeAlias === 'console:privateContentSection');
            }

            function isTopMenu(item) {
                return item && (item.typeAlias === 'console:topMenu');
            }

            function isCurrentItem(item) {
                var currentItem = $scope.nav.getCurrentItem();
                var result =  item && currentItem && item.id === currentItem.id;
                return result;
            }

            var preNavigateSelectedId;

            function isSelected(item) {
                return item.id === preNavigateSelectedId || (!preNavigateSelectedId && isCurrentItem(item));
            }

            $scope.isSelected = isSelected;


            function setShowNav() {
                var node = getNodeForItem($scope.getActiveItem());
                $scope.navData.showNav = !isAppLauncher() && ($scope.navData.userShowNav || (node && (isContainerItem(node.item) || isTopMenu(node.item))));
            }

            // when coming form the app page the first element in the list is selected
            function doDefaultRedirect(itemList) {
                var activeItem = $scope.getActiveItem();
                var nodeForItem = getNodeForItem(activeItem);
                var node = null;

                if (nodeForItem && isTopMenu(nodeForItem.item) && itemList.length) {
                    node = itemList[0];
                }

                //var nodeInTree = _.find(itemList, function (i) { return i.id === activeItem.id; });

                //if (!node && itemList.length) {     // select the first item
                //    node = itemList[0];
                //}                

                if (node && (node !== $scope.getSelectedMenuNode().item)) {

                    if (node.state) {
                        preNavigateSelectedId = node.id;
                        $state.go(node.state.name, node.state.params);
                    }
                }
            }

            $scope.isTopMenu = isTopMenu;
            $scope.isCurrentItem = isCurrentItem;

            $scope.toggleShowNav = function () {
                $scope.navData.userShowNav = !$scope.navData.userShowNav;
                setShowNav();
            };

            $scope.hideNav = function () {
                $scope.navData.userShowNav = false;
                $scope.navData.showNav = false;
            };

            $scope.getMenuItems = function () {
                return _.map(spNavService.getMenuNodes(), 'item');
            };

            $scope.getSelectedMenuItem = function () {
                return getItemInBreadcrumb($scope.getMenuItems());
            };

            $scope.getSelectedMenuNode = function () {
                return getNodeForItem($scope.getSelectedMenuItem());
            };



            $scope.getActiveItem = function () {
                return _.last(getBreadcrumb());
            };


            $scope.isAppLauncher = isAppLauncher;

            $scope.isItemDirty = _.partialRight(_.result, 'isDirty');
            $scope.itemDebug = _.partialRight(_.omit, 'data');

            $scope.parentParentNode = _.partial(getActiveNodeAncestor, 2);

            $scope.$on('sp.nav.showNavigator', function () {
                $scope.navData.userShowNav = true;
                setShowNav();
            });

            $scope.$watch('nav.getBreadcrumb()', function () {
                $scope.navData.userShowNav = false;
                setShowNav();
                notifyUpdateLayout();
            });
            
            $scope.$watch('nav.treeUpdateCount', function () {
               
                if (isAppLauncher()) 
                    return;

                var parentNode;
                var itemList = [];                

                var activeNode = getNodeForItem(_.last(getBreadcrumb()));

                var appNode = getNodeForItem(_.first(getBreadcrumb()));
                $scope.appNode = appNode;

                parentNode = appNode;

                forEachItemInTree(parentNode, addTreeNode);

                $scope.itemList = itemList;

                $scope.parentNode = parentNode;

                setShowNav();

                var firstItemApplicationId = spNavService.getCurrentApplicationId();
                if (firstItemApplicationId && firstItemApplicationId > 0) {
                    var newTheme = $scope.nav.getUpdatedTheme(firstItemApplicationId);
                    var newThemeId = sp.result(newTheme, 'idP');
                    var oldThemeId = sp.result($scope.consoleThemeModel, 'consoleTheme.idP');

                    if (newThemeId !== oldThemeId) {
                        _.delay(function () {
                            $scope.$apply(function () {
                                $scope.consoleThemeModel.consoleTheme = newTheme;
                            });
                        });
                    }
                }

                doDefaultRedirect(itemList);

                function addItem(item, isOpen, level) {

                    if (itemList.length < MAX_LIST_LENGTH) {
                        itemList.push(item);

                        $scope.navTreeItemState[item.id] =
                            _.extend($scope.navTreeItemState[item.id] || {}, {
                                open: isOpen,
                                level: level,
                                cssClass: 'nav-level-' + level + ' nav-type-' + (item.typeAlias || 'unknown').replace(':', '-')
                            });
                    }
                }

                function keepNode(n) {
                    return !(isContainerItem(n.item) || isTopMenu(n.item)) && !n.item.hiddenByConfig;
                }

                function forEachItemInTree(tree, fn) {
                    if (tree) {
                        fn(tree);

                        if (tree.children) {
                            _.each(tree.children, function (t) {
                                forEachItemInTree(t, fn);
                            });
                        }
                    }
                }

                function addTreeNode(node) {
                    if (node.item && keepNode(node))
                        addItem(node.item, node === activeNode, 1);
                }

            });

            $scope.refreshTheme = function () {

                //header area
                $scope.consoleThemeModel.navBarStyle = spThemeService.getNavBarStyle();
                $scope.consoleThemeModel.menuStyle = spThemeService.getMenuStyle();
                $scope.consoleThemeModel.titleStyle = spThemeService.getTitleStyle();


                //top navigation area
                $scope.consoleThemeModel.tabStyle = spThemeService.getTabStyle();
                $scope.consoleThemeModel.tabItemStyle = spThemeService.getTabItemStyle();
                $scope.consoleThemeModel.tabSelectedItemStyle = spThemeService.getTabSelectedItemStyle();

                //Left Navigation
                $scope.consoleThemeModel.leftNavStyle = spThemeService.getMobileLeftNavStyle();
                $scope.consoleThemeModel.leftNavItemStyle = spThemeService.getLeftNavItemStyle();
                $scope.consoleThemeModel.leftNavSelectedItemStyle = spThemeService.getLeftNavSelectedItemStyle();
                $scope.consoleThemeModel.leftNavFontStyle = spThemeService.getLeftNavFontStyle();
                $scope.consoleThemeModel.leftNavSelectedFontStyle = spThemeService.getLeftNavSelectedFontStyle();

            };

            $scope.$watch('consoleThemeModel.consoleTheme', function () {
                if ($scope.consoleThemeModel.consoleTheme) {
                    $scope.refreshTheme();
                }
            });


            $scope.$watch('nav.getThemes()', function (getThemesCompleted) {
                if (getThemesCompleted === true && !$scope.consoleThemeModel.consoleTheme) {
                    $scope.consoleThemeModel.consoleTheme = $scope.nav.getCurrentTheme();
                }
            });


            $scope.$watch('nav.getApplicationUpdated()', function (firstItemApplicationId) {
                if (firstItemApplicationId && firstItemApplicationId > 0) {
                    _.delay(function () {
                        $scope.$apply(function () {
                            $scope.consoleThemeModel.consoleTheme = $scope.nav.getUpdatedTheme(firstItemApplicationId);
                        });
                    });
                } else {
                    _.delay(function () {
                        $scope.$apply(function () {
                            $scope.consoleThemeModel.consoleTheme = $scope.nav.getUpdatedTheme();
                            if ($scope.consoleThemeModel.consoleTheme)
                                spThemeService.setStyle($scope.consoleThemeModel.consoleTheme);
                        });
                    });
                }
            });

            $scope.selectItem = function (item) {
                preNavigateSelectedId = item.id;

                $timeout(function () {
                    $state.go(item.state.name, item.state.params);
                    $location.replace();
                }, 1);       // give the UI a chance to update before navigating
            };

            $scope.itemTapped = function (item) {
                if (isCurrentItem(item)) {
                    $scope.toggleShowNav();
                } else {
                    $scope.selectItem(item);
                }
            };


            $scope.swipeRight = function (event) {
                event.srcEvent.preventDefault();

                if ($scope.navData.showNav) {
                    spNavService.navigateToState("landing", null);
                    $location.replace();
                } else {
                    $scope.navData.showNav = true;
                }
            };

            $scope.swipeLeft = function (event) {
                $scope.navData.userShowNav = false; 
                $scope.navData.showNav = false;
                event.srcEvent.preventDefault();
            };

            $scope.nav = spNavService;
            $scope.$state = $state;
            $scope.spState = spState;

            $scope.navTreeItemState = {};
            $scope.navData = { showNav: !isAppLauncher(), userShowNav: false };

            $scope.busyIndicator = {
                type: 'spinner',
                text: 'Working...',
                placement: 'window',
                isBusy: false
            };           

            $scope.$watch('nav.middleLayoutBusy', function(value) {
                $scope.busyIndicator.isBusy = value;
            });

            var bmSchemeDereg = $scope.$watch('appSettings.sessionInfo.bookmarkScheme', function (bookmarkScheme) {
                if (bookmarkScheme) {
                    spNavService.validateUrlBookmarkScheme();
                    // Deregister the watcher once we have a value. 
                    // Location change events in navService will do the rest.
                    bmSchemeDereg();
                }
            });
      });
}());

