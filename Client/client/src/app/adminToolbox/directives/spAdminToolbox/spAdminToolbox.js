// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a admin toolbox control.
    *
    * @module spAdminToolbox
    * @example

    Using the spAdminToolbox:

    &lt;sp-admin-toolbox&gt;&lt;/sp-admin-toolbox&gt

    */
    angular.module('mod.app.adminToolbox.directives.spAdminToolbox', [
    ])
        .directive('spAdminToolbox', function (spNavService, spDocumentationService) {
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {},
                templateUrl: 'adminToolbox/directives/spAdminToolbox/spAdminToolbox.tpl.html',
                link: function (scope) {
                    scope.docoService = spDocumentationService;

                    ///
                    // Cancel button has been clicked.
                    ///
                    scope.onCancelClick = function () {
                        if (spNavService.getParentItem()) {
                            if (spNavService.getParentFolder()) {
                                spNavService.refreshTreeBranch(spNavService.getParentFolder());
                            } 
                            spNavService.navigateToParent();
                        } else {
                            spNavService.navigateToState("landing", null);                            
                        }
                    };

                    ///
                    // Link has been clicked.
                    ///
                    scope.linkClicked = function (link) {
                        if (link && _.isString(link)) {
                            switch (link) {
                                case 'manageApps':
                                    {
                                        spNavService.navigateToChildState('appManager', 'console:applicationManagerStaticPage');
                                        break;
                                    }
                                case 'manageObjects':
                                    {
                                        spNavService.navigateToChildState('report', 'console:userTypesReport');
                                        break;
                                    }
                                case 'workflows':
                                    {
                                        spNavService.navigateToChildState('report', 'console:workflowReport');
                                        break;
                                    }
                                case 'themeSettings':
                                    {
                                        spNavService.navigateToChildState('report', 'console:consoleThemeSettingsReport');
                                        break;
                                    }
                                case 'emailSettings':
                                    {
                                        spNavService.navigateToChildState('viewForm', 'tenantEmailSettingsInstance');
                                        break;
                                    }
                                case 'generalSettings':
                                    {
                                        spNavService.navigateToChildState('viewForm', 'tenantGeneralSettingsInstance');
                                        break;
                                    }
                                case 'accessRules':
                                    {
                                        spNavService.navigateToChildState('securityQueries', 'console:securityQueriesPage');
                                        break;
                                    }
                                case 'passwordPolicy':
                                    {
                                        spNavService.navigateToChildState('viewForm', 'passwordPolicyInstance');
                                        break;
                                    }
                                case 'userAccounts':
                                    {
                                        spNavService.navigateToChildState('report', 'console:userAccountReport');
                                        break;
                                    }
                                case 'userRoles':
                                    {
                                        spNavService.navigateToChildState('report', 'console:userRoleReport');
                                        break;
                                    }
                                case 'importSpreadsheet':
                                    {
                                        spNavService.navigateToChildState('report', 'core:importConfigReport');
                                        break;
                                    }
                                default:
                                    break;
                            }
                        }
                    };

                    ///
                    // Set the different link groups
                    ///
                    scope.model = {
                        applicationLinks: [
                            {
                                text: 'Manage Applications',
                                link: 'manageApps',
                                order: 0
                            },
                            {
                                text: 'Manage Objects',
                                link: 'manageObjects',
                                order: 1
                            },
                            {
                                text: 'Workflows',
                                link: 'workflows',
                                order: 2
                            }
                        ],
                        settingsLinks: [
                            {
                                text: 'Theme Settings',
                                link: 'themeSettings',
                                order: 0
                            },
                            {
                                text: 'Email Settings',
                                link: 'emailSettings',
                                order: 1
                            },
                            {
                                text: 'General Settings',
                                link: 'generalSettings',
                                order: 2
                            }
                        ],
                        securityLinks: [
                            {
                                text: 'Access Rules',
                                link: 'accessRules',
                                order: 0
                            },
                            {
                                text: 'Password Policy',
                                link: 'passwordPolicy',
                                order: 1
                            },
                            {
                                text: 'User Accounts',
                                link: 'userAccounts',
                                order: 2
                            },
                            {
                                text: 'User Roles',
                                link: 'userRoles',
                                order: 3
                            }
                        ],
                        toolsLinks: [
                            {
                                text: 'Import Spreadsheet',
                                link: 'importSpreadsheet',
                                order: 0
                            }
                        ]
                    };
                }
            };
        });
})();
