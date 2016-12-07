// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function() {
    "use strict";

    angular.module('sp.app.appLauncher', ['sp.navService', 'sp.themeService', 'mod.common.spXsrf', 'mod.common.spMobile', 'titleService', 'sp.consoleIconService', 'mod.common.spLocalStorage'])
        .directive('sp-tile-box', function () {
            return {
                restrict: 'E',
                scope: {
                    tiles: '='
                },
                link: function (scope, el, attrs) {
                }
            };
        })
        .controller('AppLauncherController', function ($scope, $window, $state, $timeout, spNavService, spLoginService, spWebService, spThemeService, spXsrf, spMobileContext, titleService, consoleIconService, spLocalStorage, spDocumentationService) {
            $scope.docoService = spDocumentationService;        

            $scope.go = function (item) {
                if (item && item.state) {
                    $state.go(item.state.name, item.state.params);
                } else if (item && item.href) {
                    $window.location.href = spXsrf.addXsrfTokenAsQueryString(item.href);
                }
            };

            $scope.spNavService = spNavService;
            $scope.spLoginService = spLoginService;
            $scope.isMobileDevice = spMobileContext.isMobile;
            $scope.isTouchDevice = spMobileContext.isMobile || spMobileContext.isTablet;

            var defaultNavPending = false;
            
            $scope.consoleThemeModel = {
                consoleTheme: null,
                appLauncherStyle: {}
            };


            $scope.$watch('spNavService.getThemes()', function (getThemesCompleted) {
                if (getThemesCompleted === true ) {
                    if (!$scope.consoleThemeModel.consoleTheme)
                        $scope.consoleThemeModel.consoleTheme = $scope.nav.getCurrentTheme();

                    $scope.consoleThemeModel.appLauncherStyle = spThemeService.getAppLauncherStyle();
                }
            });


            $scope.$watch('spNavService.getApplicationUpdated()', function (firstItemApplicationId) {
                if (firstItemApplicationId && firstItemApplicationId > 0) {
                    _.delay(function () {
                        $scope.$apply(function () {
                            $scope.consoleThemeModel.consoleTheme = $scope.spNavService.getUpdatedTheme(firstItemApplicationId);
                        });
                    });
                } else {
                    _.delay(function () {
                        $scope.$apply(function () {
                            $scope.consoleThemeModel.consoleTheme = $scope.spNavService.getUpdatedTheme();
                            if ($scope.consoleThemeModel.consoleTheme)
                                spThemeService.setStyle($scope.consoleThemeModel.consoleTheme);
                        });
                    });
                }
            });

          


            $scope.$watch('spNavService.getNavTree().children', function (items) {
                if (items) {

                    // check pending nav
                    if (items.length > 0 && defaultNavPending) {
                        console.log('DEBUG login: nav tree loaded');
                        defaultNavPending = false;
                        prepareToNavigate();
                    }

                    $scope.appItems = _.filter(_.map(items, 'item'), function (item) {
                        return !item.hidden || item.name === 'Administration' || item.name === 'Help'; //todo - data drive this
                    });
                    
                    var firstItemApplicationId = spNavService.getCurrentApplicationId();
                    if (firstItemApplicationId && firstItemApplicationId > 0) {
                        _.delay(function () {
                            $scope.$apply(function () {
                                $scope.consoleThemeModel.consoleTheme = $scope.spNavService.getUpdatedTheme(firstItemApplicationId);
                            });
                        });
                    }
                }
            });

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
            
            $scope.signOut = function() {
                spLoginService.logout();
            };

            $scope.toggleVersion = function () {
                $scope.showVersion = !$scope.showVersion;
            };

            $scope.switchToDesktop = function () {
                spNavService.navigateToDesktopSite();
            };

            titleService.setTitle('Console');

            function navigateToApp(navItem) {
                if (navItem) {
                    if (spLoginService.accountId) {
                        spLocalStorage.setObject(spNavService.getLastUsedAppMenuKey(), { userAccountId: spLoginService.accountId, lastUsedAppMenuId: navItem.item.id });
                    }
                    $scope.go(navItem.item);
                }
            }

            function prepareToNavigate() {
                // todo: remove console logs after qa testing is complete for testing login behavior
                var state = $state;
                if (state.current.name === 'landinghome') {
                    console.log('DEBUG login: navigating to landing home');
                    return;
                }

                // navigate to user's default landing page if available
                var navSectionId = sp.result(spLoginService, 'defaultUserLandingInfo.defaultNavSection.idP');
                var navElementId = sp.result(spLoginService, 'defaultUserLandingInfo.defaultNavElement.idP');

                if (navSectionId && navElementId) {
                    console.log('DEBUG login: Valid selection of default user nav section and page are  available.');
                    var navTree = spNavService.getNavTree();

                    // if nav tree fetched already then find the nav element (leaf node) under nav section to navigate to
                    if (navTree.children.length > 0) {
                        console.log('DEBUG login: nav tree available.');
                        // find the page
                        var navNode = spNavService.findInTreeByNodeIdAndParentNodeId(navTree, function (node) {
                            var parentItemId = sp.result(node, 'parent.item.id');
                            var nodeId = sp.result(node, 'item.id');
                            return (nodeId === navElementId && parentItemId === navSectionId);
                        });

                        // navigate
                        if (navNode && navNode.item) {
                            console.log('DEBUG login: Navigating to user selected nav section and page.');
                            $scope.go(navNode.item);
                            return;
                        }

                        console.log('DEBUG login: User selected page not found in nav tree. Doing default nav.');

                    } else {
                        // set flag to indicate nav pending. Clear this flag in the 'spNavService.getNavTree().children' watch above and navigate to the node.
                        console.log('DEBUG login: nav tree not loaded yet.');
                        defaultNavPending = true;
                        return;
                    }
                }

                var visibleAppItems = _.filter(spNavService.getNavTree().children, function (node) {
                    return (!node.item.hiddenByConfig && !node.item.hidden) || node.item.alias === 'core:adminMenu'; // *adminMenu is special?
                });
                
                if (visibleAppItems && visibleAppItems.length > 0) {

                    // if only one avialable app then navigate to that app
                    if (visibleAppItems.length === 1) {
                        navigateToApp(visibleAppItems[0]);
                        return;
                    }
                    console.log('DEBUG login: ONLY ONE app is available? : FALSE');

                    // check last used app
                    var lastUsedAppMenuInfo = spLocalStorage.getObject(spNavService.getLastUsedAppMenuKey());

                    if (lastUsedAppMenuInfo && lastUsedAppMenuInfo.lastUsedAppMenuId && lastUsedAppMenuInfo.userAccountId === spLoginService.accountId) {
                        var lastUsedAppItem = _.find(visibleAppItems, function(i) {
                            return i.item.id === lastUsedAppMenuInfo.lastUsedAppMenuId;
                        });

                        if (lastUsedAppItem) {
                            console.log('DEBUG login: Navigating to last used appMenuId: ' + lastUsedAppMenuInfo.lastUsedAppMenuId);
                            navigateToApp(lastUsedAppItem);
                            return;
                        }
                    }

                    console.log('DEBUG login: Did not find any last used app id.');

                    //** some special requirements **//
                    // if only one app available apart from home, administration, documentFolder and shared then navigate to that app
                    var filteredApps = _.filter(visibleAppItems, function(i) {
                        return i.item.alias !== 'core:homeMenu' && i.item.alias !== 'core:adminMenu' && i.item.alias !== 'core:documentsMenu' && !(i.item.name === 'Shared' && i.item.viewType === 'home');
                    });

                    console.log('DEBUG login: filteredApps.length is: ' + filteredApps.length);
                    if (filteredApps.length === 1) {
                        console.log('DEBUG login: navigating to first available app which is not home, shared, documentMenu or admin menu.');
                        navigateToApp(filteredApps[0]);
                        return;
                    }
                    console.log('DEBUG login: More than 1 available app which is not home, shared, documentMenu or admin menu. Navigating to landing home.');

                    // navigate to landing home
                    $state.go('landinghome', state.params);
                } else {
                    // set flag to indicate nav pending. Clear this flag in the 'spNavService.getNavTree().children' watch above and navigate to the node.
                    console.log('DEBUG login: no visible apps available.. nav tree not loaded yet?');
                    defaultNavPending = true;
                    return;
                }
            }

            $timeout(function () {
                prepareToNavigate();
            }, 0);

        });
}());

