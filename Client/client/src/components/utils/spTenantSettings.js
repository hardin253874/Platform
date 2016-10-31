// Copyright 2011-2016 Global Software Innovation Pty Ltd

/**
 * Module for getting the tenant specific settings.
 * All the calls are async and will not complete until the user logs in. Additionally, on logout all pending promises are rejected. 
 * New requests after a log-out will wait until a log-in has completed sucessfully.
 * @module spTenantSettings
 */
angular.module('mod.common.spTenantSettings', ['mod.common.spEntityService', 'sp.app.settings'])
    .service('spTenantSettings', function($q, spEntityService, $rootScope, spAppSettings) {
        'use strict';

        var exports = {};

        var settingsEntityId = 'core:tenantGeneralSettingsInstance';
        
        var themeRequestString = 'name, description, alias,' +
            'k:consoleHeaderBackgroundColor,' +
            'k:consoleHeaderMenuTextColor,' +
            'k:consoleLogoImage.{name,alias},' +
            'k:consoleHeaderBackgroundImage.{name,alias},' +
            'k:consoleTopNavigationStyle.{name, alias},' +
            'k:consoleTopNavigationAreaBackgroundColor,' +
            'k:consoleTopNavigationSelectedTabColor,' +
            'k:consoleTopNavigationSelectedTabBorderColor,' +
            'k:consoleTopNavigationSelectedTabFontColor,' +
            'k:consoleTopNavigationUnselectedTabColor,' +
            'k:consoleTopNavigationUnselectedTabFontColor,' +
            'k:consoleTopBackgroundImage.{name,alias},' +
            'k:consoleLeftNavigationAreaBackgroundColor,' +
            'k:mobileLeftNavigationAreaBackgroundColor,' +
            'k:consoleLeftNavigationAreaFontColor,' +
            'k:consoleLeftNavigationSelectedElementColor,' +
            'k:consoleLeftNavigationSelectedFontColor,' +
            'k:consoleLeftBackgroundImage.{name,alias},' +
            'k:consoleGeneralContentAreaTitleFontColor,' +
            'k:consoleGeneralContentAreaContainerHeadingFontColor,' +
            'k:consoleGeneralContentAreaContainerHeadingLineColor,' +
            'k:consoleGeneralContentAreaSelectedTabColor,' +
            'k:consoleGeneralContentAreaSelectedTabFontColor,' +
            'k:consoleGeneralContentAreaUnselectedTabColor,' +
            'k:consoleGeneralContentAreaUnselectedTabFontColor,' +
            'k:consoleGeneralContentAreaTabControlLineColor,' +
            'k:consoleGeneralContentAreaReportHeaderColor,' +
            'k:consoleHeaderBackgroundImageRepeat,' +
            'k:consoleTopBackgroundImageRepeat,' +
            'k:consoleLeftBackgroundImageRepeat,' +
            'k:consoleGeneralContentAreaReportHeaderFontColor';

        var requestString = 'finYearStartMonth.id, tenantCurrencySymbol, k:tenantConsoleThemeSettings.{' + themeRequestString + '}';
        var solutionRequestString = 'name, alias, k:solutionConsoleTheme.{' + themeRequestString + '}';


        var deferred;
        var solutionThemesDefered;

        var templateReportIds = null;
        var nameFieldEntity = null;

        function initDeferred() {
            deferred = $q.defer();
        }
        
        function initSolutionThemeDefered() {
            solutionThemesDefered = $q.defer();
        }
        
        function getSettings() {
            var myDeferred = deferred;
            spEntityService.getEntity(settingsEntityId, requestString, { hint: 'tenantSettings', batch: true }).then(function (settings) {
                // If the promise has been updated then it has already been resolved as rejected, so don't resolve it again. (not sure if this is strictly necessary)
                if (myDeferred === deferred) {
                    deferred.resolve(settings);
                } 
            });
        }

        function getSolutionThemes()
        {
            var mySolutionThemesDefered = solutionThemesDefered;
            spEntityService.getInstancesOfType('core:solution', solutionRequestString, { hint: 'solnThemes', batch: true }).then(function (solutions) {
                // If the promise has been updated then it has already been resolved as rejected, so don't resolve it again. (not sure if this is strictly necessary)
                if (mySolutionThemesDefered === solutionThemesDefered) {
                    solutionThemesDefered.resolve(solutions);
                }
            });
        }


        /**
        * Get a promise for the tenant setting for the start of the finanical year.
        *
        * @returns {Promise} A promise for the  financial year start month entity
        */
        exports.getFinYearStartMonth = function() {
            return deferred.promise.then(function (settings) {
                return settings.getFinYearStartMonth();
            });
        };
        
        /**
        * Get a promise for the tenant setting for the symbol used for a currency.
        *
        * @returns {Promise} A promise  for the symbol used for a currency
        */
        exports.getCurrencySymbol = function () {
            return deferred.promise.then(function (settings) {
                return settings.getTenantCurrencySymbol();
            });
        };

        /**
        * Get a promise for the tenant setting for the console theme.
        *
        * @returns {Promise} A promise for the console theme
        */
        exports.getTenantTheme = function() {
            return deferred.promise.then(function (settings) {
                return settings.getTenantConsoleThemeSettings();
            });
        };

        exports.getSolutionThemes = function() {
            return solutionThemesDefered.promise.then(function (themes) {
                return themes;
            });
        };      

        exports.getTemplateReportIds = function () {
            var reportDeferred;

            if (templateReportIds) {
                reportDeferred = $q.defer();
                reportDeferred.resolve(templateReportIds);
                return reportDeferred.promise;
            } else {
                return spEntityService.getEntities(['core:resourceReport', 'core:templateReport', 'console:enumValuesReport'], 'id, alias', { hint: 'templateReportIds', batch: true }).then(function (entities) {
                    var ids = {};

                    _.forEach(entities, function (e) {
                        ids[e.id()] = e.id();
                        ids[e.alias()] = e.id();
                    });

                    templateReportIds = ids;
                    return templateReportIds;
                });
            }
        };

        exports.getNameFieldEntity = function () {
            var nameFieldDefered;

            if (nameFieldEntity) {
                nameFieldDefered = $q.defer();
                nameFieldDefered.resolve(nameFieldEntity);
                return nameFieldDefered.promise;
            } else {
                return spEntityService.getEntity('core:name', 'alias, name, description', {
                    hint: 'efsNameField',
                    batch: true
                }).then(function (entity) {
                    nameFieldEntity = entity;
                    return nameFieldEntity;
                });
            }
        };


        initDeferred();
        initSolutionThemeDefered();            

        $rootScope.$on('signedin', function (authId) {
            initDeferred();
            initSolutionThemeDefered();
            getSettings();
            getSolutionThemes();
            spAppSettings.getNavConfigEntities();
            exports.getNameFieldEntity();
        });

        $rootScope.$on('signedout', function () {
            // create a new promise and send out rejects for the old one.
            var oldDeferred = deferred;
            initDeferred();
            initSolutionThemeDefered();

            deferred.reject('loggedOut');
            solutionThemesDefered.reject('loggedOut');

            oldDeferred.reject('loggedOut');
        });
        
        return exports;
    });

