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
        .controller('AppLauncherController', function ($scope, $window, $state, $timeout, spNavService, spLoginService, spWebService, spThemeService, spXsrf, spMobileContext, titleService, consoleIconService, spLocalStorage) {
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
                    spLocalStorage.setItem(spNavService.getLastUsedAppKey(), navItem.item.id);
                    $scope.go(navItem.item);
                }
            }

            function prepareToNavigate() {
                //if (initialNavAfterLogin)

                var state = $state;
                if (state.current.name === 'landinghome') {
                    return;
                }
                var visibleAppItems = _.filter(spNavService.getNavTree().children, function (node) {
                    return !node.item.hiddenByConfig && !node.item.hidden;
                });

                if (visibleAppItems) {
                    // if last used app is set and it is part of items, then navigate to that app
                    // else check if there are apps other than home and administration. If yes then navigate to first app
                    // else navigate to home

                    // if only one avialable app then navigate to that app
                    if (visibleAppItems.length === 1) {
                        navigateToApp(visibleAppItems[0]);
                        return;
                    }

                    // check last used app
                    var lastUsedAppId = spLocalStorage.getItem(spNavService.getLastUsedAppKey());
                    if (lastUsedAppId) {
                        var lastUsedAppItem = _.find(visibleAppItems, function (i) {
                            return i.item.applicationId.toString() === lastUsedAppId;
                        });

                        if (lastUsedAppItem) {
                            navigateToApp(lastUsedAppItem);
                            return;
                        }
                    }

                    // first available app apart from Home and administration
                    var appItem = _.find(visibleAppItems, function(i) {
                        return i.item.alias !== 'core:homeMenu' && i.item.alias !== 'core:adminMenu';
                    });

                    if (appItem) {
                        navigateToApp(appItem);
                        return;
                    }

                    // navigate to landing home
                    $state.go('landinghome', state.params);
                }
            }

            $timeout(function () {
                prepareToNavigate();
            }, 0);

        });
}());

