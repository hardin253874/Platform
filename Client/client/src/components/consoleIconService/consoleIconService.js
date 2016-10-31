// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

/**
 * A set of AngularJS services related to theme.
 * @module theme
 */
(function () {
    "use strict";
    angular.module('sp.consoleIconService', [])
        .service('consoleIconService', function (spWebService) {
            var exports = {};
            
            exports.getNavItemIconUrlAndBackColor = function (e) {
                if (!e) {
                    return null;
                }
                var result = {};
               
                // navigationElementIcon
                if (e.navigationElementIcon) {
                    result.iconUrl = spWebService.getImageApiIconUrl(e.navigationElementIcon.idP);
                    result.iconBackgroundColor = e.navigationElementIcon.imageBackgroundColor;
                    return result;
                }

               // check definition icon and background color
                var definitionIconInfo = getDefinitionIconIdAndColor(e);
                if (definitionIconInfo) {
                    var iconId = definitionIconInfo.iconId;
                    if (iconId && _.isNumber(iconId)) {
                        result.iconUrl = spWebService.getImageApiIconUrl(iconId);

                        // only return color if a valid icon is set
                        result.iconBackgroundColor = definitionIconInfo.iconBackgroundColor;

                        return result;
                    }
                }

                // Check resource behavior
                var behavior = e.resourceConsoleBehavior;
                if (behavior) {
                    var resBehaviorIcon = sp.result(behavior, 'treeIcon');
                    if (resBehaviorIcon) {
                        var resBehaviorIconId = resBehaviorIcon.idP;
                        if (resBehaviorIconId && _.isNumber(resBehaviorIconId)) {
                            result.iconUrl = spWebService.getImageApiIconUrl(resBehaviorIconId);

                            // only return color if a valid icon is set
                            result.iconBackgroundColor = resBehaviorIcon.imageBackgroundColor;
                            return result;
                        }
                    }else {
                        result.iconUrl = behavior.treeIconUrl;
                        result.iconBackgroundColor = behavior.treeIconBackgroundColor;
                    }
                }

                // Fallback to type behavior
                if (!result.iconUrl) {
                    var types = e.isOfType;
                    if (types) {
                        behavior = types[0].typeConsoleBehavior;
                        if (behavior) {
                            var typeBehaviorIcon = sp.result(behavior, 'treeIcon');
                            if (typeBehaviorIcon) {
                                var typeBehaviorIconId = typeBehaviorIcon.idP;
                                if (typeBehaviorIconId && _.isNumber(typeBehaviorIconId)) {
                                    result.iconUrl = spWebService.getImageApiIconUrl(typeBehaviorIconId);

                                    // only return color if a valid icon is set
                                    result.iconBackgroundColor = typeBehaviorIcon.imageBackgroundColor;
                                    return result;
                                }
                            }else {
                                result.iconUrl = behavior.treeIconUrl;
                                result.iconBackgroundColor = behavior.treeIconBackgroundColor;
                            }
                        }
                    }
                }

                // Reformat
                if (result.iconUrl) {
                    result.iconUrl = _.last(result.iconUrl.replace('component/', '').split(';'));
                }

                // Fallbakc
                result.iconUrl = result.iconUrl || 'assets/images/default_app.png'; // default
                return result;
            };

            exports.getNavItemIconUrl = function(e) {
                if (!e) {
                    return null;
                }

                var iconInfo = exports.getNavItemIconUrlAndBackColor(e);

                return sp.result(iconInfo, 'iconUrl');
            };

            exports.getNavItemIconCssBackgroundColor = function (e) {
                if (!e) {
                    return null;
                }

                var iconInfo = exports.getNavItemIconUrlAndBackColor(e);

                var color = sp.result(iconInfo, 'iconBackgroundColor');

                if (color) {
                    return sp.getCssColorFromARGBString(color);
                }

                return null;
            };
           
            function getDefinitionIconIdAndColor(e) {
                var alias;
                var types = e.isOfType;
                if (types) {
                    alias = e.isOfType[0].nsAlias;
                }

                var iconEntity;
                var result = {
                    iconId: undefined,
                    iconBackgroundColor: undefined
                };
                
                if (!alias) {
                    return result;
                }
                
                switch (alias) {
                    case 'console:customEditForm':
                        {
                            iconEntity = sp.result(e, 'typeToEditWithForm.typeConsoleBehavior.treeIcon');
                            break;
                        }
                    case 'core:report':
                        {
                            iconEntity = sp.result(e, 'reportUsesDefinition.typeConsoleBehavior.treeIcon');
                            break;
                        }
                    case 'core:chart':
                        {
                            iconEntity = sp.result(e, 'chartReport.reportUsesDefinition.typeConsoleBehavior.treeIcon');
                            break;
                        }
                    case 'core:board':
                        {
                            iconEntity = sp.result(e, 'boardReport.reportUsesDefinition.typeConsoleBehavior.treeIcon');
                            break;
                        }
                }

                if (iconEntity) {
                    result.iconId = iconEntity.idP;
                    result.iconBackgroundColor = iconEntity.imageBackgroundColor;
                }

                return result;
            }
            
            return exports;

        });
})();