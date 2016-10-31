// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

/**
 * A set of AngularJS services related to theme.
 * @module theme
 */
(function () {
    "use strict";
    angular.module('sp.themeService', ['ui.router', 'mod.common.spEntityService', 'sp.common.loginService', 'mod.common.spTenantSettings', 'sp.app.settings', 'sp.app.navigation'])
        .service('spThemeService', function (spWebService) {
            var exports = {};
            var defaultDarkBackgroundColor = 'rgba(50, 50, 50, 0.4)';   // default dark background color for icons on a light color bacground of left navigation area
            var currentTheme;
            var currentNavBarStyle = {};
            var currentMenuStyle = {};
            var currentAppLauncherStyle = {};
            var currentTitleStyle = {};
            var currentHeadingStyle = {};
            var currentTabItemStyle = {};
            var currentTabStyle = {};
            var currentTopNavigationStyle = {};
            var currentTabSelectedItemStyle = {};
            var currentLeftNavStyle = {};
            var isLeftNavAreaBackColorLight = false;    // it assumes out of box default background color is dark. This flag gets calculated if tenant has defined a color for left navigation area background color in current theme.
            var isMobileLeftNavAreaBackcolorLight = false;
            var mobileLeftNavStyle = {};
            var currentLeftNavItemStyle = {};
            var currentLeftNavSelectedItemStyle = {};
            var currentLeftNavFontStyle = {};
            var currentLeftNavSelectedFontStyle = {};
            var currentGeneralSelectedTabStyle = {};
            var currentGeneralTabStyle = {};
            var currentTabControlLineColor = "";
            var currentConsoleTopNavigationStyle = "";
            var currentGeneralSelectedTabHeadingStyle = {};
            var currentGeneralTabHeadingStyle = {};
            var currentReportheaderStyle = {};

            var currentDefaultLogo = 'assets/images/logo_RN_main.png';
            exports.setStyle = function (consoleTheme)
            {         
                currentTheme = null;        
                if (consoleTheme) {
                    currentTheme = consoleTheme;
                    ///////////////////////////////////////////////
                    ////header area
                    ///////////////////////////////////////////////
                    currentDefaultLogo = 'assets/images/logo_RN_main.png';
                    currentNavBarStyle = {};
                    currentMenuStyle = {};
                    currentAppLauncherStyle = {};
                    currentTitleStyle = {};
                    currentHeadingStyle = {};
                    //logo
                    var consoleLogoImage = consoleTheme.consoleLogoImage;
                    if (consoleLogoImage) {
                        var logoImageUrl = spWebService.getImageApiUrl(consoleLogoImage.idP);
                        if (logoImageUrl && logoImageUrl.length > 0) {
                            currentDefaultLogo = logoImageUrl;
                        }
                    } else {
                        currentDefaultLogo = 'assets/images/logo_RN_main.png';
                    }

                    //background colour
                    var headerBackgroundColour = consoleTheme.consoleHeaderBackgroundColor;
                    if (headerBackgroundColour) {
                        currentNavBarStyle['background'] = spUtils.getCssColorFromARGBString(headerBackgroundColour);
                        currentNavBarStyle['background-color'] = spUtils.getCssColorFromARGBString(headerBackgroundColour);
                        currentAppLauncherStyle['background'] = spUtils.getCssColorFromARGBString(headerBackgroundColour);
                        currentAppLauncherStyle['background-color'] = spUtils.getCssColorFromARGBString(headerBackgroundColour);
                    }

                    //background image
                    var headerBackgroundImage = consoleTheme.consoleHeaderBackgroundImage;
                    if (headerBackgroundImage) {
                        var headerBackgroundImageUrl = spWebService.getImageApiUrl(headerBackgroundImage.idP);
                        if (headerBackgroundImageUrl && headerBackgroundImageUrl.length > 0) {
                            currentNavBarStyle['background-image'] = 'url(\'' + headerBackgroundImageUrl + '\')';
                            currentNavBarStyle['background-position'] = 'left top';
                            currentAppLauncherStyle['background-image'] = 'url(\'' + headerBackgroundImageUrl + '\')';
                            currentAppLauncherStyle['background-position'] = 'left top';
                            var headerBackgroundImageRepeat = consoleTheme.consoleHeaderBackgroundImageRepeat;
                            if (headerBackgroundImageRepeat === true) {
                                currentNavBarStyle['background-repeat'] = 'repeat-x';
                                currentAppLauncherStyle['background-repeat'] = 'repeat-x';
                            } else {
                                currentNavBarStyle['background-repeat'] = 'no-repeat';
                                currentAppLauncherStyle['background-repeat'] = 'no-repeat';
                            }
                        }
                    }

                    //menu text colour
                    var headerMenuTextColour = consoleTheme.consoleHeaderMenuTextColor;
                    if (headerMenuTextColour) {
                        currentMenuStyle['color'] = spUtils.getCssColorFromARGBString(headerMenuTextColour);
                    }

                    ///////////////////////////////////////////////
                    ////top navigation area
                    ///////////////////////////////////////////////
                    currentTabStyle = {};
                    currentTopNavigationStyle = {};
                    currentTabItemStyle = {};
                    currentTabSelectedItemStyle = {};
                    currentConsoleTopNavigationStyle = "";
                    //background colour
                    var topNavigationAreaBackgroundColor = consoleTheme.consoleTopNavigationAreaBackgroundColor;
                    if (topNavigationAreaBackgroundColor) {
                        currentTabStyle['background'] = spUtils.getCssColorFromARGBString(topNavigationAreaBackgroundColor);
                        currentTabStyle['background-color'] = spUtils.getCssColorFromARGBString(topNavigationAreaBackgroundColor);
                        currentTopNavigationStyle['background'] = spUtils.getCssColorFromARGBString(topNavigationAreaBackgroundColor);
                        currentTopNavigationStyle['background-color'] = spUtils.getCssColorFromARGBString(topNavigationAreaBackgroundColor);
                    }

                    //background image
                    var topBackgroundImage = consoleTheme.consoleTopBackgroundImage;
                    if (topBackgroundImage) {
                        var topBackgroundImageUrl = spWebService.getImageApiUrl(topBackgroundImage.idP);
                        if (topBackgroundImageUrl && topBackgroundImageUrl.length > 0) {
                            currentTabStyle['background-image'] = 'url(\'' + topBackgroundImageUrl + '\')';
                            currentTabStyle['background-position'] = 'left top';
                            currentTopNavigationStyle['background-image-url'] = topBackgroundImageUrl;
                            currentTopNavigationStyle['background-image'] = 'url(\'' + topBackgroundImageUrl + '\')';
                            currentTopNavigationStyle['background-position'] = 'left top';
                            var topBackgroundImageRepeat = consoleTheme.consoleTopBackgroundImageRepeat;
                            if (topBackgroundImageRepeat === true) {
                                currentTabStyle['background-repeat'] = 'repeat-x';
                                currentTopNavigationStyle['background-repeat'] = 'repeat-x';
                            } else {
                                currentTabStyle['background-repeat'] = 'no-repeat';
                                currentTopNavigationStyle['background-repeat'] = 'no-repeat';
                            }
                        }
                    }

                    //selected tab colour
                    var topNavigationSelectedTabColor = consoleTheme.consoleTopNavigationSelectedTabColor;
                    if (topNavigationSelectedTabColor) {
                        currentTabSelectedItemStyle['background'] = spUtils.getCssColorFromARGBString(topNavigationSelectedTabColor);
                        currentTabSelectedItemStyle['background-color'] = spUtils.getCssColorFromARGBString(topNavigationSelectedTabColor);
                    }

                    var topNavigationSelectedTabBorderColor = consoleTheme.consoleTopNavigationSelectedTabBorderColor;
                    if (topNavigationSelectedTabBorderColor) {
                        currentTabSelectedItemStyle['border-bottom-color'] = spUtils.getCssColorFromARGBString(topNavigationSelectedTabBorderColor);
                        
                    }
                    //select tab font colour
                    var topNavigationSelectedTabFontColor = consoleTheme.consoleTopNavigationSelectedTabFontColor;
                    if (topNavigationSelectedTabFontColor) {
                        currentTabSelectedItemStyle['color'] = spUtils.getCssColorFromARGBString(topNavigationSelectedTabFontColor);
                    }

                    //unselected tab colour
                    var topNavigationUnselectedTabColor = consoleTheme.consoleTopNavigationUnselectedTabColor;
                    if (topNavigationUnselectedTabColor) {
                        currentTabItemStyle['background'] = spUtils.getCssColorFromARGBString(topNavigationUnselectedTabColor);
                        currentTabItemStyle['background-color'] = spUtils.getCssColorFromARGBString(topNavigationUnselectedTabColor);
                    }

                    //unselect tab font colour
                    var topNavigationUnselectedTabFontColor = consoleTheme.consoleTopNavigationUnselectedTabFontColor;
                    if (topNavigationUnselectedTabFontColor) {
                        currentTabItemStyle['color'] = spUtils.getCssColorFromARGBString(topNavigationUnselectedTabFontColor);
                    }

                    if (consoleTheme.consoleTopNavigationStyle) {
                        currentConsoleTopNavigationStyle = consoleTheme.consoleTopNavigationStyle.name;
                    }

                    ///////////////////////////////////////////////
                    ////Left Navigation area
                    ///////////////////////////////////////////////
                   
                    currentLeftNavStyle = {};
                    mobileLeftNavStyle = {};
                    currentLeftNavItemStyle = {};
                    currentLeftNavSelectedItemStyle = {};
                    currentLeftNavFontStyle = {};
                    currentLeftNavSelectedFontStyle = {};

                    //background colour
                    var leftNavigationAreaBackgroundColor = consoleTheme.consoleLeftNavigationAreaBackgroundColor;
                    if (leftNavigationAreaBackgroundColor) {
                        currentLeftNavStyle['background'] = spUtils.getCssColorFromARGBString(leftNavigationAreaBackgroundColor);
                        currentLeftNavStyle['background-color'] = spUtils.getCssColorFromARGBString(leftNavigationAreaBackgroundColor);

                        var leftColor = spUtils.getColorFromARGBString(leftNavigationAreaBackgroundColor);
                        isLeftNavAreaBackColorLight = spUtils.isColorLighterThanMiddleGray(leftColor);
                    }

                    var mobileLeftNavigationAreaBackgrouundColor = consoleTheme.mobileLeftNavigationAreaBackgroundColor;
                    if (mobileLeftNavigationAreaBackgrouundColor) {
                        mobileLeftNavStyle['background'] = spUtils.getCssColorFromARGBString(mobileLeftNavigationAreaBackgrouundColor);
                        mobileLeftNavStyle['background-color'] = spUtils.getCssColorFromARGBString(mobileLeftNavigationAreaBackgrouundColor);

                        var mobLeftColor = spUtils.getColorFromARGBString(mobileLeftNavigationAreaBackgrouundColor);
                        isMobileLeftNavAreaBackcolorLight = spUtils.isColorLighterThanMiddleGray(mobLeftColor);
                    }

                    //background image
                    var leftBackgroundImage = consoleTheme.consoleLeftBackgroundImage;
                    if (leftBackgroundImage) {
                        var leftBackgroundImageUrl = spWebService.getImageApiUrl(leftBackgroundImage.idP);
                        if (leftBackgroundImageUrl && leftBackgroundImageUrl.length > 0) {
                            currentLeftNavStyle['background-image'] = 'url(\'' + leftBackgroundImageUrl + '\')';
                            currentLeftNavStyle['background-position'] = 'left top';
                            mobileLeftNavStyle['background-image'] = 'url(\'' + leftBackgroundImageUrl + '\')';
                            mobileLeftNavStyle['background-position'] = 'left top';
                            var leftBackgroundImageRepeat = consoleTheme.consoleLeftBackgroundImageRepeat;
                            if (leftBackgroundImageRepeat === true) {
                                currentLeftNavStyle['background-repeat'] = 'repeat-y';
                                mobileLeftNavStyle['background-repeat'] = 'repeat-y';
                            } else {
                                currentLeftNavStyle['background-repeat'] = 'no-repeat';
                                mobileLeftNavStyle['background-repeat'] = 'no-repeat';
                            }
                        }
                    }

                    // font Colour
                    var leftNavigationAreaFontColor = consoleTheme.consoleLeftNavigationAreaFontColor;
                    if (leftNavigationAreaFontColor) {
                        currentLeftNavFontStyle['color'] = spUtils.getCssColorFromARGBString(leftNavigationAreaFontColor);
                    }

                    // selected nav item font colour
                    var leftNavigationSelectedFontColor = consoleTheme.consoleLeftNavigationSelectedFontColor;
                    if (leftNavigationSelectedFontColor) {
                        currentLeftNavSelectedFontStyle['color'] = spUtils.getCssColorFromARGBString(leftNavigationSelectedFontColor);
                    }

                    // selected nav item element colour
                    var leftNavigationSelectedElementColor = consoleTheme.consoleLeftNavigationSelectedElementColor;
                    if (leftNavigationSelectedElementColor) {
                        currentLeftNavSelectedItemStyle['background'] = spUtils.getCssColorFromARGBString(leftNavigationSelectedElementColor);
                        currentLeftNavSelectedItemStyle['background-color'] = spUtils.getCssColorFromARGBString(leftNavigationSelectedElementColor);
                        currentLeftNavSelectedFontStyle['background'] = spUtils.getCssColorFromARGBString(leftNavigationSelectedElementColor);
                        currentLeftNavSelectedFontStyle['background-color'] = spUtils.getCssColorFromARGBString(leftNavigationSelectedElementColor);
                    }

                    ///////////////////////////////////////////////
                    ////general content area
                    ///////////////////////////////////////////////
                   
                    currentTitleStyle = {};
                    currentHeadingStyle = {};
                    currentGeneralSelectedTabStyle = {};
                    currentGeneralTabStyle = {};
                    currentGeneralSelectedTabHeadingStyle = {};
                    currentGeneralTabHeadingStyle = {};
                    currentTabControlLineColor = "";
                    currentReportheaderStyle = {};
                    var titleFontColor = consoleTheme.consoleGeneralContentAreaTitleFontColor;
                    if (titleFontColor) {
                        currentTitleStyle['color'] = spUtils.getCssColorFromARGBString(titleFontColor);
                    }

                    var headingFontColor = consoleTheme.consoleGeneralContentAreaContainerHeadingFontColor;
                    if (headingFontColor) {
                        currentHeadingStyle['color'] = spUtils.getCssColorFromARGBString(headingFontColor);
                    }

                    var headingLineColor = consoleTheme.consoleGeneralContentAreaContainerHeadingLineColor;
                    if (headingLineColor) {
                        currentHeadingStyle['border-bottom-color'] = spUtils.getCssColorFromARGBString(headingLineColor);
                    }

                    var selectedTabColor = consoleTheme.consoleGeneralContentAreaSelectedTabColor;
                    if (selectedTabColor) {
                        currentGeneralSelectedTabStyle['background'] = spUtils.getCssColorFromARGBString(selectedTabColor);
                        currentGeneralSelectedTabStyle['background-color'] = spUtils.getCssColorFromARGBString(selectedTabColor);
                        currentGeneralSelectedTabHeadingStyle['background'] = spUtils.getCssColorFromARGBString(selectedTabColor);
                        currentGeneralSelectedTabHeadingStyle['background-color'] = spUtils.getCssColorFromARGBString(selectedTabColor);
                    }
                   
                    var unSelectedTabColor = consoleTheme.consoleGeneralContentAreaUnselectedTabColor;
                    if (unSelectedTabColor) {
                        currentGeneralTabStyle['background'] = spUtils.getCssColorFromARGBString(unSelectedTabColor);
                        currentGeneralTabStyle['background-color'] = spUtils.getCssColorFromARGBString(unSelectedTabColor);
                        currentGeneralTabHeadingStyle['background'] = spUtils.getCssColorFromARGBString(selectedTabColor);
                        currentGeneralTabHeadingStyle['background-color'] = spUtils.getCssColorFromARGBString(selectedTabColor);
                    }

                    var selectedTabFontColor = consoleTheme.consoleGeneralContentAreaSelectedTabFontColor;
                    if (selectedTabFontColor) {
                        currentGeneralSelectedTabHeadingStyle['color'] = spUtils.getCssColorFromARGBString(selectedTabFontColor);
                    }

                    var unSelectedTabFontColor = consoleTheme.consoleGeneralContentAreaUnselectedTabFontColor;
                    if (unSelectedTabFontColor) {
                        currentGeneralTabHeadingStyle['color'] = spUtils.getCssColorFromARGBString(unSelectedTabFontColor);
                    }

                    var tabControlLineColor = consoleTheme.consoleGeneralContentAreaTabControlLineColor;
                    if (tabControlLineColor) {
                        currentTabControlLineColor = spUtils.getCssColorFromARGBString(tabControlLineColor);
                    }                    

                    var reportHeaderColor = consoleTheme.consoleGeneralContentAreaReportHeaderColor;
                    if (reportHeaderColor) {
                        currentReportheaderStyle['background'] = spUtils.getCssColorFromARGBString(reportHeaderColor);
                        currentReportheaderStyle['background-color'] = spUtils.getCssColorFromARGBString(reportHeaderColor);
                    }

                    var reportHeaderFontColor = consoleTheme.consoleGeneralContentAreaReportHeaderFontColor;
                    if (reportHeaderFontColor) {
                        currentReportheaderStyle['color'] = spUtils.getCssColorFromARGBString(reportHeaderFontColor);
                    }

                }


            };

            exports.getConsoleTheme = function() {
                return currentTheme;
            };

            exports.getDefaultLogo = function() {
                return currentDefaultLogo;
            };

            exports.getTitleStyle = function () {
                return currentTitleStyle;
            };

            exports.getDefaultDarkBackgroundColor = function () {
                return defaultDarkBackgroundColor;
            };

            exports.getNavBarStyle = function () {
                return currentNavBarStyle;
            };
            exports.getAppLauncherStyle = function () {
                return currentAppLauncherStyle;
            };

            exports.getMenuStyle = function () {
                return currentMenuStyle;
            };

            exports.getHeadingStyle = function() {
                return currentHeadingStyle;
            };

            exports.getTabStyle = function () {
                return currentTabStyle;
            };

            exports.getTopNavigationStyle = function () {
                return currentTopNavigationStyle;
            };

            exports.getTabItemStyle = function () {
                return currentTabItemStyle;
            };

            exports.getTabSelectedItemStyle = function () {
                return currentTabSelectedItemStyle;
            };

        exports.getLeftNavStyle = function() {
            return currentLeftNavStyle;
        };
        exports.getIsLeftNavAreaBackColorLight = function () {
            return isLeftNavAreaBackColorLight;
        };

        exports.getMobileLeftNavStyle = function () {
            return mobileLeftNavStyle;
        };
        exports.getIsMobileLeftNavAreaBackcolorLight = function () {
            return isMobileLeftNavAreaBackcolorLight;
        };

        exports.getLeftNavItemStyle = function() {
            return currentLeftNavItemStyle;
        };

        exports.getLeftNavSelectedItemStyle = function() {
            return currentLeftNavSelectedItemStyle;
        };

        exports.getLeftNavFontStyle = function() {
            return currentLeftNavFontStyle;
        };

        exports.getLeftNavSelectedFontStyle = function() {
            return currentLeftNavSelectedFontStyle;
        };

        exports.getGeneralTabStyle = function() {
            return currentGeneralTabStyle;
        };

        exports.getTabControlLineColor = function() {
            return currentTabControlLineColor;
        };

        exports.getConsoleTopNavigationStyle = function() {
            return currentConsoleTopNavigationStyle;
        };

        exports.getGeneralTabHeadingStyle = function() {
            return currentGeneralTabHeadingStyle;
        };

        exports.getGeneralSelectedTabStyle = function() {
            return currentGeneralSelectedTabStyle;
        };

        exports.getGeneralSelectedTabHeadingStyle = function() {
            return currentGeneralSelectedTabHeadingStyle;
        };

        exports.getReportHeaderStyle = function() {
            return currentReportheaderStyle;
        };
            return exports;

        });


})();