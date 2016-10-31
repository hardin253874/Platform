// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    "use strict";

    /**
     * Application navigation related directives and other.
     */

    angular.module('sp.app.navigation', [
        'hmTouchEvents', 'sp.navService', 'mod.app.navigationProviders',
        'mod.app.navigation.directives', 'mod.common.spEntityService', 'mod.common.ui.spUserPasswordDialog',
        'sp.common.loginService', 'sp.app.settings', 'sp.themeService',
        'mod.featureSwitch', 'sp.consoleIconService', 'mod.common.ui.spFocus'
    ])
        .controller('NavController', function ($scope, $timeout, $location, $q, spNavService, $state, spState,
                                               spNavigationBuilderProvider, $window, newNavItemPopoverEntries, spWebService,
                                               spEntityService, spUserPasswordDialog, spLoginService, spAppSettings,
                                               spThemeService, rnFeatureSwitch, consoleIconService, focus) {

            var navigationBuilderProvider;
            var navTreeItemState = {};
            var treeItemDepthOffset = 0;

            //
            // Board
            var boardFeatureEnabled = true; // rnFeatureSwitch.isFeatureOn('boardFeature');
            var fsSelfServeEnabled = rnFeatureSwitch.isFeatureOn('fsSelfServe');

            $scope.newNavItemPopoverEntries = newNavItemPopoverEntries;

            if (!boardFeatureEnabled) {
                $scope.newNavItemPopoverEntries = _.reject($scope.newNavItemPopoverEntries, function (p) {
                    return /task board/ig.test(p.name);
                });
            }

            if (!fsSelfServeEnabled) {
                $scope.newNavItemPopoverEntries = _.reject($scope.newNavItemPopoverEntries, function (p) {
                    return /privateContentSection/ig.test(p.item.typeAlias);
                });
            }

            //
            // Submit a bug
            $scope.submitABugEnabled = rnFeatureSwitch.isFeatureOn('submitABug');


            // callback to ensure fullConfig setting is loaded
            spAppSettings.getNavConfigEntities().then(function () {
                if (!spAppSettings.fullConfig) {
                    // only show self-serve options if fullConfig is not available
                    $scope.newNavItemPopoverEntries = _.filter($scope.newNavItemPopoverEntries, function (p) {
                        return p.canSelfServe;
                    });
                }
            });

            $scope.loginService = spLoginService;
            $scope.appSettings = spAppSettings;
            $scope.appMenu = {
                isAppMenuOpen: false
            };

            $scope.consoleThemeModel = {
                consoleTheme: null,
                tenantTheme: null,
                defaultLogo: null,
                navBarStyle: {},
                menuStyle: {},
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

            $scope.isNavItemOpen = function (node) {
                return getNavTreeItemState(node).open;
            };

            $scope.viewDropOptions = {
                onDragOver: function () {
                    $scope.$broadcast('navViewDragOver');
                },
                onAllowDrop: function () {
                    return false;
                }
            };

            $scope.submitBug = function () {
                $scope.$root.$broadcast('sp.showFeedbackForm');
            };

            $scope.isHomePageHeader = function () {
                var homePageStyle = {};
                if (!spNavService.canNavigateToParent()) {
                    //home page and hide flag is true, set display to none, otherwise only set the background color is transparent.                 
                    homePageStyle['background-color'] = 'transparent';
                } else {
                    homePageStyle = spThemeService.getTopNavigationStyle();
                }
                return homePageStyle;
            };

            $scope.isHomePageTopNav = function () {
                var homePageStyle = {};
                if (!spNavService.canNavigateToParent()) {
                    //home page and hide flag is true, set display to none, otherwise only set the background color is transparent.
                    homePageStyle['display'] = 'none';
                    homePageStyle['background-color'] = 'transparent';
                } else {
                    var topNavStyle = spThemeService.getTopNavigationStyle();
                    if (topNavStyle && topNavStyle['background-image'])
                        homePageStyle['fill'] = 'url(#topBgImage)';
                    else
                        homePageStyle['fill'] = topNavStyle ? topNavStyle['background-color'] : null;

                }
                return homePageStyle;
            };

            $scope.getTopBgImage = function () {
                var topBgImage = '';
                var topNavStyle = spThemeService.getTopNavigationStyle();
                if (spNavService.canNavigateToParent() && topNavStyle && topNavStyle['background-image-url']) {
                    topBgImage = topNavStyle['background-image-url'];
                }

                return topBgImage;
            };




            $scope.getAppMenuItemClass = function (i) {
                var classes = [];
                var id;
                var depth = -1;
                var typeAlias;
                var selectedMenuItem = $scope.getSelectedMenuItem();

                if (i && i.item) {
                    id = i.item.id;
                    depth = i.item.depth;
                    typeAlias = i.item.typeAlias;
                }

                if (selectedMenuItem && id === selectedMenuItem.id) {
                    classes.push('selected');
                }

                if (depth >= 0) {
                    classes.push('nav-level-' + depth);   
                }

                if (typeAlias) {
                    classes.push('nav-type-' + typeAlias.replace(':', '-'));
                }

                return classes.join(' ');
            };

            $scope.getTabItemClass = function (i) {
                var classes = [];
                var id;
                var depth = -1;
                var typeAlias;
                var selectedTabItem = $scope.getSelectedTabItem();

                if (i && i.item) {
                    id = i.item.id;
                    depth = i.item.depth;
                    typeAlias = i.item.typeAlias;
                }

                if (selectedTabItem && id === selectedTabItem.id) {
                    classes.push('selected');
                    classes.push('active');
                }

                if (depth >= 0) {
                    classes.push('nav-level-' + depth);   
                }

                if (typeAlias) {
                    classes.push('nav-type-' + typeAlias.replace(':', '-'));
                }

                return classes.join(' ');
            };

            $scope.getNavItemClass = function (i) {
                var classes = [];
                var id;
                var depth = -1;
                var typeAlias;
                var selected;
                var canNavigateToItem = true;
                var activeItem = $scope.getActiveItem();

                getNavTreeItemState(i);

                if (i && i.item) {
                    id = i.item.id;
                    depth = i.item.depth - treeItemDepthOffset;
                    typeAlias = i.item.typeAlias;
                    selected = i.item.selected;
                    canNavigateToItem = spNavService.canNavigateToItem(i.item);
                }

                if (selected) {
                    classes.push('selected');
                }

                if (activeItem && id === activeItem.id && canNavigateToItem) {
                    classes.push('active');
                }

                if (depth >= 0) {
                    classes.push('nav-level-' + depth);   
                }

                if (typeAlias) {
                    classes.push('nav-type-' + typeAlias.replace(':', '-'));                    
                }

                return classes.join(' ');
            };

            $scope.getNavItemAClass = function (i) {
                var classes = [];
                var id;
                var canNavigateToItem = true;
                var activeItem = $scope.getActiveItem();
                var itemState = getNavTreeItemState(i);
                var typeAlias;

                classes.push('nav-link');

                if (i && i.item) {
                    id = i.item.id;
                    canNavigateToItem = spNavService.canNavigateToItem(i.item);
                    typeAlias = i.item.typeAlias;
                }

                if (activeItem && id === activeItem.id && canNavigateToItem) {
                    classes.push('active');
                }

                if (typeAlias === 'console:navSection' || typeAlias === 'core:folder' || typeAlias === 'console:privateContentSection') {
                    classes.push(itemState.open ? 'open' : 'closed');
                }

                if (itemState.nochildren) {
                    classes.push('nochildren');
                }

                return classes.join(' ');
            };

            /**
             * The NavController is an AngularJS controller that prepares the navigation structure
             * in a format suitable for presentation. It is the thing that is aware of our app
             * using three levels; menu, then tabs, then nav tree.
             *
             * We are making assumptions that the nav service tree and paths are never null.
             *
             */

            function populateNavConfigButton() {
                var navConfigButton = _.find(spState.data.appTools, {_id: 'NavConfigButton'});

                if (!navConfigButton) {
                    spState.data.appTools.push({
                        _id: 'NavConfigButton',
                        text: 'Configure',
                        order: 1,
                        icon: 'assets/images/settings.png',
                        cssClass: 'navConfigButton-' + (spNavService.isSelfServeEditMode ? 'edit' : 'view'),
                        click: function () {
                            spNavService.toggleIsEditMode();
                            this.cssClass = 'navConfigButton-' + (spNavService.isSelfServeEditMode ? 'edit' : 'view');
                        }
                    });
                } else {
                    navConfigButton.cssClass = 'navConfigButton-' + (spNavService.isSelfServeEditMode ? 'edit' : 'view');
                }
            }

            
            function populateAdminToolboxButton(adminToolboxStaticPageEntityId) {
                if (spState.data.appTools) {
                    var navAdminToolboxButton = _.find(spState.data.appTools, {_id: 'NavAdminToolboxButton'});
                    var isAdminToolboxActive = spState.name === 'adminToolbox';

                    if (!navAdminToolboxButton) {
                        spState.data.appTools.push({
                            _id: 'NavAdminToolboxButton',
                            text: 'Configure',
                            hidden: hideAdminToolboxButton(),
                            order: 0,

                            icon: 'assets/images/icon_toolbox_w.png',
                            cssClass: 'navAdminToolboxButton-' + (isAdminToolboxActive ? 'edit' : 'view'),
                            click: function () {
                                if (isAdminToolboxActive) {
                                    if (spNavService.getParentItem()) {
                                        if (spNavService.getParentFolder()) {
                                            spNavService.refreshTreeBranch(spNavService.getParentFolder());
                                        }
                                        spNavService.navigateToParent();
                                    } else {
                                        spNavService.navigateToState("landing", null);
                                    }
                                }
                                else {
                                    spNavService.navigateToSibling('adminToolbox', adminToolboxStaticPageEntityId);
                                }
                            }
                        });
                    } else {
                        navAdminToolboxButton.cssClass = 'navAdminToolboxButton-' + (isAdminToolboxActive ? 'edit' : 'view');
                        navAdminToolboxButton.hidden = hideAdminToolboxButton();
                    }
                }
            }

            function populateNavConfigButtons() {
                if (!spState.data) {
                    return;
                }

                spAppSettings.getNavConfigEntities().then(function (entities) {

                    spState.data.appTools = spState.data.appTools || [];

                    // both self-service power users and full admins can see the configure 'wrench' icon
                    if (spAppSettings.selfServeOrAdmin) {
                        populateNavConfigButton();

                        // only load admin toolbox if user has access to 'fullConfigButton'
                        var adminToolboxStaticPageEntity = _.find(entities, { nsAlias: 'console:adminToolboxStaticPage' });
                        if (spAppSettings.fullConfig && spAppSettings.adminToolbox) {
                            populateAdminToolboxButton(adminToolboxStaticPageEntity.idP);
                        }
                    }

                }, function (error) {
                    if (error === 404) {
                        if (spState.data.appTools) {
                            _.remove(spState.data.appTools, { _id: 'NavConfigButton' });
                            _.remove(spState.data.appTools, { _id: 'NavAdminToolboxButton' });
                        }
                    }
                });
            }

            $scope.$watch('nav.isFullEditMode', function (newVal, oldVal) {
                if (newVal === oldVal)
                    return;

                if (spState.data.appTools) {
                    var navAdminToolboxButton = _.find(spState.data.appTools, {_id: 'NavAdminToolboxButton'});
                    if (!navAdminToolboxButton)
                        return;

                    if (newVal) {
                        navAdminToolboxButton.hidden = hideAdminToolboxButton();
                    }
                    else {
                        navAdminToolboxButton.hidden = hideAdminToolboxButton();
                        $scope.nav.isAdminToolboxActive = false;
                    }
                }
            });

            function hideAdminToolboxButton() {
                return !spNavService.isFullEditMode || sp.result(spState, 'data.hideAppToolboxButton');
            }

            function getBreadcrumb() {
                return spNavService.getBreadcrumb();
            }

            $scope.newComponentMenuAvailable = function newComponentMenuAvailable() {
                var nav = $scope.nav;                
                return nav.isFullEditMode || (nav.isSelfServeEditMode && nav.havePrivateContentSectionsInTree());
            };

            /**
             * Get the nav tree node that matches the given item (object with "id" prop)
             */
            function getNodeForItem(item) {
                return item ? spNavService.findInTreeById(spNavService.getNavTree(), item.id) : null;
            }

            /**
             * Find the first item in the breadcrumb that appears in the given list of items.
             */
            function getItemInBreadcrumb(items) {
                if (!items || !items.length) {
                    return null;
                }
                return _.find(getBreadcrumb(), function (item) {
                    return _.some(items, {id: item.id});
                });
            }

            function isAppLauncher() {
                //temporary to get things in place for some styling, but this is not the way!
                return $state.current.name === 'landing' || $state.current.name === 'landinghome';
            }

            function isContainer(item) {
                return item && (item.typeAlias === 'console:navSection' || item.typeAlias === 'core:folder' || item.typeAlias === 'core:documentFolder' || item.typeAlias === 'console:privateContentSection');
            }

            function notifyUpdateLayout() {
                $timeout(function () {
                    $scope.$root.$broadcast('app.layout');

                    $timeout(function () {
                        $scope.$root.$broadcast('app.layout.postAnimate');
                    }, 0);      // WARNING: This will need to change if the animation length changes.
                }, 0);          // Note: If you want to make larger than zero, document reason here.
            }

            function middleLayoutBusy(newValue) {
                if ($scope.$root.__spTestMode) {
                    console.log('navigation: in testMode so ignoring setBusy: ' + newValue);
                    newValue = false;
                }
                $scope.middleBusy.isBusy = newValue;
            }

            function setShowNav() {
                $scope.navData.showNav = $scope.navData.userShowNav;// && !$scope.navData.hideNav;
            }

            function setHideNav(navTreeItems) {
                var emptyTree = !navTreeItems || !navTreeItems.length || (navTreeItems.length === 1 && !isContainer(navTreeItems[0].item));
                $scope.navData.hideNav = isAppLauncher() || !!sp.result(spState, 'data.leftPanelTemplate') ||
                    (!spNavService.isFullEditMode && (emptyTree || sp.result(spState, 'data.fullScreen')));
            }

            function forEachItemInTree(tree, fn, depth) {
                if (tree) {
                    depth = depth || 0;
                    fn(tree, depth, tree);
                    if (tree.children) {
                        depth += 1;
                        _.each(tree.children, function (t) {
                            forEachItemInTree(t, fn, depth);
                        });
                    }
                }
            }


            function findFirstReportScreenOrChartInTree(tree) {
                if (!tree) {
                    return null;
                }

                return spNavService.findInTree(tree, function (item) {
                    return item && (item.typeAlias === 'core:chart' || item.typeAlias === 'console:screen' ||
                        item.typeAlias === 'core:report' || item.typeAlias === 'console:staticPage' ||
                        item.typeAlias === 'core:board');
                });
            }


            /**
             * Do any default redirection/navigation based on the current item.
             * Replaces the existing browser history entry so browser back works.
             */
            function doDefaultRedirect() {

                var node = getNodeForItem($scope.getActiveItem());
                var state, childNode;

                if (node) {
                    if (node === $scope.getSelectedTabNode()) {
                        // The current node is a tab.                        
                        // Find the first report, chart or screen
                        childNode = findFirstReportScreenOrChartInTree(node);

                        if (childNode) {
                            state = sp.result(childNode, 'item.state');
                        } else {
                            state = sp.result(node, 'children.0.item.state');
                        }
                    } else if (node === $scope.getSelectedMenuNode()) {
                        // Get the first tab
                        state = sp.result(node, 'children.0.item.state');
                    }

                    if (state) {
                        $state.go(state.name, state.params);
                        $location.replace();
                    }
                }
            }            

            $scope.getMenuNodes = function (ordered) {
                return spNavService.getMenuNodes(ordered);
            };

            $scope.getMenuItems = function (ordered) {
                return _.map($scope.getMenuNodes(ordered), 'item');
            };

            $scope.getSelectedMenuItem = function () {
                return getItemInBreadcrumb($scope.getMenuItems());
            };

            $scope.getSelectedMenuItemIconUrl = function () {
                var selectedMenuItem;
                var selectedMenuNode;

                selectedMenuItem = $scope.getSelectedMenuItem();

                if (selectedMenuItem) {
                    selectedMenuNode = _.find($scope.getMenuNodes(true), function (node) {
                        return node.item.id === selectedMenuItem.id;
                    });
                }

                if (selectedMenuNode) {
                    return selectedMenuNode.item.iconUrl;
                } else {
                    return null;
                }
            };

            $scope.getSelectedMenuNode = function () {
                return getNodeForItem($scope.getSelectedMenuItem());
            };

            $scope.getTabItems = function (order) {
                var node = $scope.getSelectedMenuNode();
                var result = node && node.item && node.item.showTabs ? _.map(node.children, 'item') : [];
                if (order) {
                    result = _.sortBy(result, 'order');
                }
                return result;
            };

            $scope.getTabNodes = function (order) {
                var node = $scope.getSelectedMenuNode();
                var result = node && node.item && node.item.showTabs ? node.children : [];
                if (order) {
                    result = _.sortBy(result, function (n) {
                        return n.item.order;
                    });
                }
                return result;
            };

            $scope.getSelectedTabItem = function () {
                return getItemInBreadcrumb($scope.getTabItems());
            };

            $scope.getSelectedTabNode = function () {
                return getNodeForItem($scope.getSelectedTabItem());
            };

            $scope.getActiveItem = function () {
                return _.last(getBreadcrumb());
            };

            $scope.toggleShowNav = function () {
                $scope.navData.userShowNav = !$scope.navData.userShowNav;
                setShowNav();
            };

            $scope.toggleOpen = function (item) {
                var itemState = getNavTreeItemState(item);
                itemState.open = !itemState.open;
            };            

            $scope.refreshTheme = function () {

                //header area
                $scope.consoleThemeModel.navBarStyle = spThemeService.getNavBarStyle();
                $scope.consoleThemeModel.menuStyle = spThemeService.getMenuStyle();
                $scope.consoleThemeModel.defaultLogo = spThemeService.getDefaultLogo();


                //top navigation area
                $scope.consoleThemeModel.tabStyle = spThemeService.getTabStyle();
                $scope.consoleThemeModel.tabItemStyle = spThemeService.getTabItemStyle();
                $scope.consoleThemeModel.tabSelectedItemStyle = spThemeService.getTabSelectedItemStyle();

                //Left Navigation
                $scope.consoleThemeModel.leftNavStyle = spThemeService.getLeftNavStyle();
                $scope.consoleThemeModel.leftNavItemStyle = spThemeService.getLeftNavItemStyle();
                $scope.consoleThemeModel.leftNavSelectedItemStyle = spThemeService.getLeftNavSelectedItemStyle();
                $scope.consoleThemeModel.leftNavFontStyle = spThemeService.getLeftNavFontStyle();
                $scope.consoleThemeModel.leftNavSelectedFontStyle = spThemeService.getLeftNavSelectedFontStyle();

            };

            $scope.getTabItemStyle = function (selected) {
                if (selected === true) {
                    return $scope.consoleThemeModel.tabSelectedItemStyle;
                } else {
                    return $scope.consoleThemeModel.tabItemStyle;
                }
            };

            $scope.getLeftNavItemStyle = function (selected) {
                if (selected === true) {
                    return $scope.consoleThemeModel.leftNavSelectedItemStyle;
                } else {
                    return $scope.consoleThemeModel.leftNavItemStyle;
                }
            };

            $scope.getLeftNavFontStyle = function (selected) {
                if (selected === true) {
                    return $scope.consoleThemeModel.leftNavSelectedFontStyle;
                } else {
                    return $scope.consoleThemeModel.leftNavFontStyle;
                }
            };

            function getItemIconBackgroundColorInt(item) {
                if (!item || !item.entity) {
                    return undefined;
                }
                return consoleIconService.getNavItemIconCssBackgroundColor(item.entity);
            }

            $scope.setIconStyle = function (isItemActive, item, showBackColor) {

                if (isItemActive || !item) {
                    return;
                }
                
                if (showBackColor) {
                    item.iconStyle = item.iconStyle || {};
                    var imgBackColor = getItemIconBackgroundColorInt(item);
                    item.iconStyle['background-color'] = imgBackColor;
                } else {
                    item.iconStyle = undefined;
                }
            };

            $scope.getSelectedIconStyle = function (item) {
                if (!item || !item.entity) {
                    return undefined;
                }

                // hack: set item.iconStyle to null. On unselected, the style is bound to 'item.iconStyle' not this function.
                // so making it null to make sure it is still not showing backcolor.
                item.iconStyle = undefined;

                var bColor = getItemIconBackgroundColorInt(item);

                if (bColor) {
                    return { 'background-color': bColor };
                }

                return undefined;
            };

            function getDarkBackColorIfApplicable() {
                // chk if the left navigation area has light coloured background
                var isLightColor = spThemeService.getIsLeftNavAreaBackColorLight();
                if (isLightColor) {
                    return spThemeService.getDefaultDarkBackgroundColor();
                }
                return null;
            }
            

            $scope.getUnSelectedIconStyle = function (item) {
                if (!item || !item.entity) {
                    return undefined;
                }
                var bColor;

                if (item.isMouseIn) {
                    bColor = getItemIconBackgroundColorInt(item);
                } else {
                    bColor = getDarkBackColorIfApplicable();
                }

                if (bColor) {
                    return { 'background-color': bColor };
                }

                return null;
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

            // function takes an item as an argument
            $scope.isItemDirty = _.partialRight(_.result, 'isDirty');

            // function takes an item as an argument
            $scope.itemDebug = _.partialRight(_.omit, 'data');

            $scope.$watch('navTreeItems', function (navTreeItems) {
                setHideNav(navTreeItems);
                setShowNav();
            });

            $scope.$watch('navData.showNav', notifyUpdateLayout);
            $scope.$watch('navData.hideNav', notifyUpdateLayout);
            $scope.$watch('getTabItems().length', notifyUpdateLayout);
            $scope.$watch('nav.isFullEditMode', function () {
                var selectedTabNode = $scope.getSelectedTabNode();
                setHideNav(selectedTabNode ? selectedTabNode.children : null);
                notifyUpdateLayout();
            });

            $scope.$watch('nav.middleLayoutBusy', middleLayoutBusy);

            $scope.refreshNavTreeItems = function () {
                var currentItem = $scope.getActiveItem();
                var menuNode = $scope.getSelectedMenuNode();
                var rootNode = sp.result(menuNode, 'item.showTabs') ? $scope.getSelectedTabNode() : menuNode;
                var rootNodes = [];                

                forEachItemInTree(rootNode, function (node, depth) {
                    if (node) {
                        // Add it to the $scope.navTreeItemState object.
                        var navItemState = getNavTreeItemState(node);

                        // we really only need to calc this once as they should all be the same...
                        treeItemDepthOffset = node.item.depth - depth;

                        if (_.isUndefined(navItemState.open)) {
                            navItemState.open = node.item.id === currentItem.id;
                            navItemState.nochildren = false; // TODO: Remove the arrow when there are no children for a folder.
                        }

                        // Add the top level to the root nodes.
                        if (depth === 1) {
                            rootNodes.push(node);
                        }
                    }
                });

                $scope.navTreeItems = rootNodes;
                $scope.selectedItems = _.filter(rootNode, { selected: true });

                syncNavTreeWithItem(currentItem);
            };

            function syncNavTreeWithItem(item) {
                if (!item || !sp.result(item, 'data.syncNavTreeWithItem')) {
                    return;
                }

                delete item.data.syncNavTreeWithItem;

                var parentContainer = $scope.nav.getCurrentItemContainer();

                if (!parentContainer) {
                    return;
                }

                var navItemState = getNavTreeItemState({
                    item: {
                        id: parentContainer.id
                    }
                });
                navItemState.open = true;

                var focusId = $scope.getNodeFocusId(item.state);
                if (!focusId) {
                    return;
                }

                $timeout(function() { focus(focusId); }, 200);
            }

            $scope.$watch('nav.treeUpdateCount', function () {

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

                $scope.refreshNavTreeItems();
                doDefaultRedirect();
            });

            $scope.$watch('nav.getBreadcrumb()', function () {
                notifyUpdateLayout();
            });

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
                }
            });

            $scope.$watch('nav.getNavTree().children', function () {

                var firstItemApplicationId = spNavService.getCurrentApplicationId();
                if (firstItemApplicationId && firstItemApplicationId > 0) {
                    _.delay(function () {
                        $scope.$apply(function () {
                            $scope.consoleThemeModel.consoleTheme = $scope.nav.getUpdatedTheme(firstItemApplicationId);
                        });
                    });
                }
            });


            $scope.$watch('spState.data', function (currentData) {
                if (currentData) {

                    var currentTheme = spNavService.getCurrentTheme();
                    if (currentTheme) {
                        $scope.consoleThemeModel.consoleTheme = currentTheme;
                    }

                    // Populate the nav config buttons
                    populateNavConfigButtons();

                    $scope.$root.$broadcast('contentLoadedTiming');       // Used for timing load time

                }
            });

            $scope.$on('sp.nav.showNavigator', function () {

                // this message occurs on a failed attempt to nav to parent
                // so let's go to the current app landing page

                var state = sp.result($scope.getSelectedMenuNode(), 'item.state');
                if (state) {
                    $state.go(state.name, state.params);
                    $location.replace();
                }

            });

            $scope.dragOptions = {
                onDragEnd: function () {
                    navigationBuilderProvider.dragEnd();
                },
                onDragStart: function () {
                    navigationBuilderProvider.dragStart();
                }
            };

            $scope.dropOptions = {
                simpleEventsOnly: true,
                propagateDragEnter: false,
                propagateDragLeave: false,
                propagateDrop: false,
                propagateDragOver: false,

                onAllowDrop: function (source, target, dragData, dropData) {
                    return navigationBuilderProvider.allowDropItem(source, target, dragData, dropData);
                },
                onDrop: function (event, source, target, dragData, dropData) {
                    navigationBuilderProvider.dropItem(event, source, target, dragData, dropData);
                },
                onDragOver: function (event, source, target, dragData, dropData) {
                    navigationBuilderProvider.dragOverItem(event, source, target, dragData, dropData);
                    return false;
                },
                onDragEnter: function (event, source, target, dragData, dropData) {
                    navigationBuilderProvider.dragEnterItem(event, source, target, dragData, dropData);
                },
                onDragLeave: function (event, source, target, dragData, dropData) {
                    navigationBuilderProvider.dragLeaveItem(event, source, target, dragData, dropData);
                }
            };

            $scope.middleBusy = {
                type: 'spinner',
                text: 'Working...',
                placement: 'window',
                isBusy: false
            };

            $scope.nav = spNavService;
            $scope.navData = {userShowNav: true, showNav: true, hideNav: true};
            $scope.$state = $state;
            $scope.spState = spState;
            $scope.navTreeItems = [];
            $scope.popover = {
                isOpen: false
            };

            navigationBuilderProvider = spNavigationBuilderProvider($scope);
            $scope.navBuilderProvider = navigationBuilderProvider;


            // Show the user password dialog
            $scope.showChangePasswordDialog = function (changePasswordAtNextLogon) {
                if (!spLoginService.isSignedIn() || !spLoginService.accountId) {
                    return null;
                }

                var options = {
                    accountId: spLoginService.accountId,
                    changePasswordAtNextLogon: changePasswordAtNextLogon
                };

                return spUserPasswordDialog.showModalDialog(options).then(function (result) {
                    if (!result) {
                        // Cancelled or no password was specified
                        // If we somehow get here then logout if we are required to change the password
                        if (options.changePasswordAtNextLogon) {
                            spLoginService.logout();
                        }
                    } else {
                        // The password was changed
                        if (options.changePasswordAtNextLogon) {
                            // Reset the change password at logon
                            spLoginService.changePasswordAtNextLogon = false;
                        }
                    }

                    return result;
                }, function () {
                    // If we somehow get here then logout if we are required to change the password
                    if (options.changePasswordAtNextLogon) {
                        spLoginService.logout();
                    }
                });
            };

            // Order the menu nodes, especially for the drop down navigation menu.
            $scope.menuNodesDropdownValueFunction = function (node) {
                // See 27656 - sadly this has been rolled out again, because we lost the ability to reorder home-screen items.
                // to re-add, update header.tpl.html ~45:    i in getMenuNodes(true) | orderBy:menuNodesDropdownValueFunction
                
                var name = node.item.name;

                var pre = '1_';
                if (name === "Home")
                    pre = '0_';
                if (name === "Administration")
                    pre = '9_';
                if (name === "Documents")
                    pre = '8_';

                return pre + name;
            };

            // Show the user password dialog if the change at next logon value is true.
            $scope.$watch('loginService.changePasswordAtNextLogon', function (value) {
                if (value) {
                    $scope.showChangePasswordDialog(value);
                }
            });

            $scope.$on('$stateChangeSuccess', function () {

                // Since nav is not related to a specific state and since we have set this page
                // action stuff up to use the current state data to hold registered actions then
                // we need to register this each time there is a new state. Seems to work :)

                //TODO - add a moveNavItem action

                spState.registerAction('addNewNavItem', function (opts) {

                    //TODO - the opts object includes the name of the intended parent item... use it

                    var from = {source: 'new', item: {typeAlias: opts.typeAlias}};
                    var to = _.last($scope.getSelectedTabNode().children);
                    var pos = 'after';
                    navigationBuilderProvider.dropItemAction(from, to, pos);


                    return $q.when(true);
                });

            });

            $scope.$on('onNavConfigPanelMenuItem', function () {                
                $scope.appMenu.isAppMenuOpen = false;    
            });

            $scope.getNodeFocusId = function (state) {
                if (!state) {
                    return null;
                }

                var eid = sp.result(state, "params.eid");
                var path = sp.result(state, "params.path");

                if (!eid || !path) {
                    return null;
                }

                return "navigation_" + state.name + "_" + eid + "_" + path;
            };

            var bmSchemeDereg = $scope.$watch('appSettings.sessionInfo.bookmarkScheme', function (bookmarkScheme) {
                if (bookmarkScheme) {
                    spNavService.validateUrlBookmarkScheme();
                    // Deregister the watcher once we have a value. 
                    // Location change events in navService will do the rest.
                    bmSchemeDereg();
                }
            });

            function getNavTreeItemState(node) {
                console.assert(node && node.item);
                var key = node.item.id;
                navTreeItemState[key] = navTreeItemState[key] || {};
                return navTreeItemState[key];
            }
        })

        .constant('navDirtyMessage',
        {
            defaultMsg: 'You have unsaved changes, are you sure you want to leave?'
        })
        .constant('newNavItemPopoverEntries', [ // if you change contents, please change .nb-popover-entries min-height also. (yes, i know)
            {
                name: 'New Screen',
                icon: 'assets/images/16x16/screen_new.png',
                item: {
                    typeAlias: 'console:screen'
                },
                source: 'new',
                canSelfServe: true
            },
            {
                name: 'Existing Screen',
                icon: 'assets/images/16x16/screen_existing.png',
                item: {
                    typeAlias: 'console:screen'
                },
                source: 'existing',
                canSelfServe: false
            },
            {
                name: 'New Report',
                icon: 'assets/images/16x16/NewReport.png',
                item: {
                    typeAlias: 'core:report'
                },
                source: 'new',
                canSelfServe: true
            },
            {
                name: 'Existing Report',
                icon: 'assets/images/16x16/report_existing.png',
                item: {
                    typeAlias: 'core:report'
                },
                source: 'existing',
                canSelfServe: false
            },
            {
                name: 'New Chart',
                icon: 'assets/images/16x16/chart_new.png',
                item: {
                    typeAlias: 'core:chart'
                },
                source: 'new',
                canSelfServe: true
            },
            {
                name: 'Existing Chart',
                icon: 'assets/images/16x16/chart_existing.png',
                item: {
                    typeAlias: 'core:chart'
                },
                source: 'existing',
                canSelfServe: false
            },
            {
                name: 'New Board',
                icon: 'assets/images/16x16/board_new.png',
                item: {
                    typeAlias: 'core:board'
                },
                source: 'new',
                canSelfServe: false
            },
            {
                name: 'New Document Folder',
                icon: 'assets/images/16x16/NewDocFolder.png',
                item: {
                    typeAlias: 'core:documentFolder'
                },
                source: 'new',
                canSelfServe: false
            },
            {
                name: 'New Section',
                icon: 'assets/images/16x16/NewSection.png',
                item: {
                    typeAlias: 'console:navSection',
                    isAppTab: false
                },
                source: 'new',
                canSelfServe: false
            },            
            {
                name: 'New Personal Section',
                icon: 'assets/images/16x16/NewPrivateContentSection.png',
                item: {
                    typeAlias: 'console:privateContentSection'
                },
                source: 'new',
                canSelfServe: false
            },
            {
                name: 'New Object',
                icon: 'assets/images/16x16/NewObject.png',
                item: {
                    typeAlias: 'core:definition',
                    isAppTab: false
                },
                source: 'new',
                canSelfServe: false
            }
        ]);
}());

