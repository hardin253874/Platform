// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a administration page.
    *
    * @module spAdministration
    * @example

    Using the spAdministration:

    &lt;sp-administration&gt;&lt;/sp-administration&gt

    */
    angular.module('mod.app.administration.directives.spAdministration', [
    ])
        .directive('spAdministration', function (spNavService) {
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {},
                templateUrl: 'administration/directives/spAdministration.tpl.html',
                link: function (scope) {

                    ///
                    // Cancel button has been clicked.
                    ///
                    scope.onCancelClick = function () {
                        spNavService.navigateToState("landing", null);
                    };

                    ///
                    // Link has been clicked.
                    ///
                    scope.linkClicked = function (link) {
                        if (link && _.isString(link)) {
                            switch (link) {
                                case 'applicationLibrary':
                                    {
                                        spNavService.navigateToChildState('appManager', 'console:applicationManagerStaticPage');
                                        break;
                                    }
                                case 'generalSettings':
                                    {
                                        spNavService.navigateToChildState('viewForm', 'tenantGeneralSettingsInstance');
                                        break;
                                    }
                                case 'imageSettings':
                                    {
                                        spNavService.navigateToChildState('viewForm', 'tenantImageSettingsInstance');
                                        break;
                                    }
                                case 'emailSettings':
                                    {
                                        spNavService.navigateToChildState('viewForm', 'tenantEmailSettingsInstance');
                                        break;
                                    }
                                case 'mailboxes':
                                    {
                                        spNavService.navigateToChildState('report', 'console:inboxesReport');
                                        break;
                                    }
                                case 'documentTypes':
                                    {
                                        spNavService.navigateToChildState('report', 'console:documentTypesReport');
                                        break;
                                    }
                                case 'thumbnailSizes':
                                    {
                                        spNavService.navigateToChildState('report', 'thumbnailSizesReport');
                                        break;
                                    }
                                case 'conditionalFormattingIcon':
                                    {
                                        spNavService.navigateToChildState('report', 'console:conditionalFormatIconReport');
                                        break;
                                    }
                                case 'textFieldPattern':
                                    {
                                        spNavService.navigateToChildState('report', 'console:regexReport');
                                        break;
                                    }
                                case 'themeSettings':
                                    {
                                        spNavService.navigateToChildState('report', 'console:consoleThemeSettingsReport');
                                        break;
                                    }
                                case 'manageObjects':
                                    {
                                        spNavService.navigateToChildState('report', 'console:userTypesReport');
                                        break;
                                    }
                                case 'managerelationships':
                                    {
                                        spNavService.navigateToChildState('report', 'console:relationshipsReport');
                                        break;
                                    }
                                case 'manageReports':
                                    {
                                        spNavService.navigateToChildState('report', 'console:reportsReport');
                                        break;
                                    }
                                case 'manageCharts':
                                    {
                                        spNavService.navigateToChildState('report', 'console:chartsReport');
                                        break;
                                    }
                                case 'manageBoards':
                                    {
                                        spNavService.navigateToChildState('report', 'console:boardsReport');
                                        break;
                                    }
                                case 'manageForms':
                                    {
                                        spNavService.navigateToChildState('report', 'console:customFormsReport');
                                        break;
                                    }
                                case 'manageScreens':
                                    {
                                        spNavService.navigateToChildState('report', 'console:screensReport');
                                        break;
                                    }
                                case 'manageChoiceFields':
                                    {
                                        spNavService.navigateToChildState('report', 'console:enumReport');
                                        break;
                                    }
                                case 'manageResourceKeys':
                                    {
                                        spNavService.navigateToChildState('report', 'console:resourceKeysReport');
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


                                case 'accessRules':
                                    {
                                        spNavService.navigateToChildState('securityQueries', 'console:securityQueriesPage');
                                        break;
                                    }
                                case 'customiseui':
                                    {
                                        spNavService.navigateToChildState('securityCustomiseUI', 'console:securityCustomiseUIPage');
                                        break;
                                    }
                                case 'passwordPolicy':
                                    {
                                        spNavService.navigateToChildState('viewForm', 'passwordPolicyInstance');
                                        break;
                                    }
                                case 'auditLog':
                                    {
                                        spNavService.navigateToChildState('report', 'console:auditLogEntriesReport');
                                        break;                                    
                                    }
                                case 'auditLogSettings':
                                    {
                                        spNavService.navigateToChildState('viewForm', 'tenantAuditLogSettingsInstance');
                                        break;
                                    }
                                case 'workflows':
                                    {
                                        spNavService.navigateToChildState('report', 'console:workflowReport');
                                        break;
                                    }
                                case 'triggers':
                                    {
                                        spNavService.navigateToChildState('screen', 'console:triggersScreen');
                                        break;
                                    }
                                case 'schedules':
                                    {
                                        spNavService.navigateToChildState('screen', 'console:schedulesScreen');
                                        break;
                                    }
                                case 'workflowRuns':
                                    {
                                        spNavService.navigateToChildState('report', 'console:workflowRunsReport');
                                        break;
                                    }
                                case 'eventLogSettings':
                                    {
                                        spNavService.navigateToChildState('viewForm', 'core:tenantEventLogSettingsInstance');
                                        break;
                                    }
                                case 'importSpreadsheet':
                                    {
                                        spNavService.navigateToChildState('report', 'core:importConfigReport');
                                        break;
                                    }
                                case 'eventLog':
                                    {
                                        spNavService.navigateToChildState('report', 'console:eventLogReport');
                                        break;
                                    }
                                case 'policies':
                                    {
                                        spNavService.navigateToChildState('report', 'console:policiesReport');
                                        break;
                                    }
                                case 'hierarchies':
                                    {
                                        spNavService.navigateToChildState('report', 'console:hierarchiesReport');
                                        break;
                                    }
                                case 'identityProviders':
                                    {
                                        spNavService.navigateToChildState('report', 'console:identityProvidersReport');
                                        break;
                                    }
                                case 'apis':
                                    {
                                        spNavService.navigateToChildState('report', 'console:apisReport');
                                        break;
                                    }
                                case 'apiKeys':
                                    {
                                        spNavService.navigateToChildState('report', 'console:apiKeysReport');
                                        break;
                                    }
                                case 'securityAuditLog':
                                    {
                                        spNavService.navigateToChildState('report', 'console:auditLogReport');
                                        break;
                                    }

                                case 'importRuns':
                                    {
                                        spNavService.navigateToChildState('report', 'importRunsReport');
                                        break;
                                    }
                                case 'scheduledImport':
                                    {
                                        spNavService.navigateToChildState('report', 'scheduledImportReport');
                                        break;
                                    }

                                case 'surveys':
                                    {
                                        spNavService.navigateToChildState('report', 'surveysReport');
                                        break;
                                    }
                                case 'questionLibraries':
                                    {
                                        spNavService.navigateToChildState('report', 'questionLibrariesReport');
                                        break;
                                    }
                                case 'questionCategories':
                                    {
                                        spNavService.navigateToChildState('report', 'questionCategoriesReport');
                                        break;
                                    }
                                case 'choiceOptions':
                                    {
                                        spNavService.navigateToChildState('report', 'choiceOptionSetReport');
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
                                text: 'Application Library',
                                link: 'applicationLibrary',
                                order: 0
                            }
                        ],
                        settingsLinks: [
                            {
                                text: 'Email Settings',
                                link: 'emailSettings',
                                order: 0
                            },
                            {
                                text: 'General Settings',
                                link: 'generalSettings',
                                order: 1
                            },
                            {
                                text: 'Image Settings',
                                link: 'imageSettings',
                                order: 2
                            },
                            
                            {
                                text: 'Mailboxes',
                                link: 'mailboxes',
                                order: 3
                            },
                            {
                                text: 'Document Types',
                                link: 'documentTypes',
                                order: 4
                            },
                            {
                                text: 'Thumbnail Sizes',
                                link: 'thumbnailSizes',
                                order: 5
                            },
                            {
                                text: 'Conditional Formatting Icons',
                                link: 'conditionalFormattingIcon',
                                order: 6
                            },
                            {
                                text: 'Text Field Patterns',
                                link: 'textFieldPattern',
                                order: 7
                            },
                            {
                                text: 'Themes',
                                link: 'themeSettings',
                                order: 8
                            }
                        ],
                        resourcesLinks: [
                            {
                                text: 'Boards',
                                link: 'manageBoards',
                                order: 0
                            },
                            {
                                text: 'Objects',
                                link: 'manageObjects',
                                order: 1
                            },
                            {
                                text: 'Relationships',
                                link: 'managerelationships',
                                order: 2
                            },
                            {
                                text: 'Reports',
                                link: 'manageReports',
                                order: 3
                            },
                            {
                                text: 'Charts',
                                link: 'manageCharts',
                                order: 4
                            },
                            {
                                text: 'Forms',
                                link: 'manageForms',
                                order: 5
                            },
                            {
                                text: 'Hierarchies',
                                link: 'hierarchies',
                                order: 6
                            },
                            {
                                text: 'Screens',
                                link: 'manageScreens',
                                order: 7
                            },
                            {
                                text: 'Choice Fields',
                                link: 'manageChoiceFields',
                                order: 8
                            },
                            {
                                text: 'Resource Keys',
                                link: 'manageResourceKeys',
                                order: 9
                            }
                        ],
                        securityLinks: [
                            {
                                text: 'Identity Providers',
                                link: 'identityProviders',
                                order: 0
                            },
                            {
                                text: 'Navigation Access',
                                link: 'customiseui',
                                order: 1
                            },
                            {
                                text: 'Password Policy',
                                link: 'passwordPolicy',
                                order: 2
                            },
                            {
                                text: 'Record Access',
                                link: 'accessRules',
                                order: 3
                            },
                            {
                                text: 'Security Audit Log Settings',
                                link: 'auditLogSettings',
                                order: 4
                            },
                            {
                                text: 'Security Audit Log',
                                link: 'securityAuditLog',
                                order: 5
                            },                            
                            {
                                text: 'User Accounts',
                                link: 'userAccounts',
                                order: 6
                            },
                            {
                                text: 'User Roles',
                                link: 'userRoles',
                                order: 7
                            }   
                        ],
                        workflowsLinks: [
                            {
                                text: 'Workflows',
                                link: 'workflows',
                                order: 0
                            },
                            {
                                text: 'Triggers',
                                link: 'triggers',
                                order: 1
                            },
                            {
                                text: 'Schedules',
                                link: 'schedules',
                                order: 2
                            },
                            {
                                text: 'Workflow Runs',
                                link: 'workflowRuns',
                                order: 3
                            } 
                        ],
                        toolsLinks: [
                            {
                                text: 'Event Log Settings',
                                link: 'eventLogSettings',
                                order: 0
                            },
                            {
                                text: 'Policies',
                                link: 'policies',
                                order: 1
                            },
                            {
                                 text: 'Audit Log',
                                 link: 'auditLog',
                                 order: 2
                            },
                            {
                                text: 'Event Log',
                                link: 'eventLog',
                                order: 2
                            }
                        ],
                        integrationsLinks: [
                            {
                                text: 'APIs',
                                link: 'apis',
                                order: 0
                            },
                            {
                                text: 'API Keys',
                                link: 'apiKeys',
                                order: 1
                            },
                            {
                                text: 'Import Spreadsheet',
                                link: 'importSpreadsheet',
                                order: 2
                            },
                            {
                                text: 'Import Runs',
                                link: 'importRuns',
                                order: 3
                            },
                            {
                                text: 'Scheduled Import',
                                link: 'scheduledImport',
                                order: 4
                            }
                        ],
                        surveysLinks: [
                            {
                                text: 'Surveys',
                                link: 'surveys',
                                order: 0
                            },
                            {
                                text: 'Question Libraries',
                                link: 'questionLibraries',
                                order: 1
                            },
                            {
                                text: 'Question Categories',
                                link: 'questionCategories',
                                order: 2
                            },
                            {
                                text: 'Choice Options',
                                link: 'choiceOptions',
                                order: 3
                            }
                        ]
                    };
                }
            };
        });
})();
