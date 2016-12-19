// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, base64, sp, Globalize */

(function () {
    'use strict';

    angular.module('sp.app.settings', [
            'mod.common.spLocalStorage',
            'mod.common.spWebService',
            'mod.common.spEntityService',
            'angularLoad'
    ])
        .value('clientVersion', null)
        .factory('spAppSettings', function (spLocalStorage, spWebService, spEntityService, $rootScope, $q, angularLoad, clientVersion) {

            var exports = {
                server: '',
                clientVersion: clientVersion,	// this value is set in the index file
                sessionInfo: {},
                initialSettings: {},
                fullConfig: null,         // bool, set on first call to getNavConfigEntities, true for admins that see the fullConfig button
                selfServeNonAdmin: null,  // bool, set on first call to getNavConfigEntities, true for self-service power users that are not admins
                selfServeOrAdmin: null,   // bool, set on first call to getNavConfigEntities, true for both self-service power users and admins
                adminToolbox: null,       // bool, set on first call to getNavConfigEntities, true if the admin-toolbox page should be available
                publicByDefault: false    // bool, true if new instances of components should be public by default, false if they should be private by default
            };

            exports.setServer = function updateServer(server) {

                //console.log('spAppSettings: called to set server %o', server, typeof server);

                if (!_.isUndefined(server) && !_.isNull(server)) {

                    if (server && server.indexOf('http') !== 0) {
                        server = 'https://' + server;
                    }

                    this.server = server;

                    console.log('spAppSettings: setting server ' + server);

                    spLocalStorage.setItem('server', server);
                    spWebService.setWebApiRoot(server);
                }
            };

            var validatedCultureKey = 'validatedCulture';               // Key in local storage for the validated culture. Note this key is also used and declared in index.tpl.html
            var loadedCultureKey = 'loadedCulture';                     // Key in loacal storage for the culture that was actually loaded at app start time.

            var lastServerVersionKey = 'lastServerVersion';             // Key in local storage for the validated culture. Note this key is also used and declared in index.tpl.html

            var navConfigEntities = null;            
            
            exports.setInitialSettings = function updateInitialSettings(initialSettings) {
                var refreshApp = false;
                var oldVersion, newVersion;
                
                this.initialSettings = initialSettings;
              
                spLocalStorage.setObject('initialSettings', initialSettings);

                if (initialSettings) {
                    //
                    // Culture
                    //
                    var culture = initialSettings.culture; //  not using '$window.navigator.userLanguage || $window.navigator.language' as it is unreliable. 

                    // ensure the if anything goes wrong the app will load with the default culture and not get stuck in a loop
                    if (culture) {

                        var validateCultureRegex = /^[a-z]{2,3}(?:-[A-Z]{2,3}(?:-[a-zA-Z]{4})?)?$/;

                        var parsed = validateCultureRegex.exec(culture);

                        if (parsed && parsed.length !== 1) {
                            spLocalStorage.setItem(validatedCultureKey, 'unknown');
                            console.error('An invalid culture has been provided: ' + culture);
                            culture = 'en-GB';
                        }
                    } else {
                        culture = 'en-GB';
                    }

                    setCulture(culture);
                   



                    //
                    // Platform version
                    //
                    oldVersion = spLocalStorage.getObject(lastServerVersionKey);
                    newVersion = initialSettings.platformVersion;

                    spLocalStorage.setObject(lastServerVersionKey, newVersion);

                    if (newVersion && oldVersion && oldVersion !== newVersion) {
                        //
                        // If the server platform has changed refresh.
                        //
                        // If we don't have an old version, don't trigger a refresh. This is to prevent unnecessary refreshes if this is the first ever time visiting or
                        // local data has been cleaned. 

                        // Note that there is an edge case where if the local storage is wiped during the update period you could still run 
                        // with the old files. It's a pretty small window, so I don't think it is worth the trade-off of forcing a double refresh
                        // when there isn't a value.
                        //

                        // DISABLING THE REFRESH ITS ANNOYING
                        //refreshApp = true;
                        //console.log(['Server version update detected, refreshing app. Old ', oldVersion, ', New ', newVersion].join());
                    }

                    //
                    // Platform version display

                    var platformVersionDisplay;
                    if (initialSettings.branchName) {
                        platformVersionDisplay = initialSettings.platformVersion + '-' + initialSettings.branchName;
                    } else {
                        platformVersionDisplay = initialSettings.platformVersion;
                    }

                    initialSettings.platformVersionDisplay = platformVersionDisplay;
                    initialSettings.devMode = initialSettings.branchName && initialSettings.branchName.toLowerCase() === 'dev';
                }
                
                if (refreshApp) {
                    $rootScope.$emit('sp.app.restart', 'Session settings have changed');
                }
            };


            function setCulture(culture) {

                var testCulture = $rootScope.__spTestCulture;

                if (testCulture) {
                    console.log('Using test culture:', testCulture);

                }

                culture = testCulture || culture;

                console.log('Loading culture file for: ' + culture);

                var angularPromise = angularLoad.loadScript('lib/angular/i18n/angular-locale_' + culture + '.js').catch(function () {
                    console.log('Failed to load angular culture file for ' + culture + ' - ignoring');
                });

                var globalizePromise = angularLoad.loadScript('lib/globalize/cultures/globalize.culture.' + culture + '.js').then(function () {
                    Globalize.culture(culture);
                }).catch(function () {
                    console.log('Failed to load Globalize culture file for ' + culture + ' - ignoring');
                });

                return $q.all([angularPromise, globalizePromise]);
            }



            exports.setSessionInfo = function (sessionInfo) {
                exports.sessionInfo = sessionInfo;                
            };


            exports.initialise = function () {                
                this.setServer(spLocalStorage.getItem('server'));
                //this.setInitialSettings(spLocalStorage.getObject('initialSettings'));
            };

            exports.getNavConfigEntities = function () {
                if (navConfigEntities) {
                    return $q.when(navConfigEntities);
                } else {
                    return spEntityService.getEntities(['core:fullConfigButton', 'core:selfServiceConfigButton', 'console:adminToolboxStaticPage'], 'id, alias', { hint: 'navConfig', batch: true }).then(function (entities) {

                        exports.fullConfig = !!_.find(entities, { nsAlias: 'core:fullConfigButton' });
                        exports.adminToolbox = !!_.find(entities, { nsAlias: 'console:adminToolboxStaticPage' });
                        var hasSelfServeButton = !!_.find(entities, { nsAlias: 'core:selfServiceConfigButton' });
                        exports.selfServeNonAdmin = hasSelfServeButton && !exports.adminToolbox;
                        exports.selfServeOrAdmin = hasSelfServeButton || exports.adminToolbox;

                        navConfigEntities = entities;
                        exports.publicByDefault = exports.fullConfig;        // content created by full admins is public by default (to be confirmed/decided)
                        return navConfigEntities;
                    });
                }
            };

            /**
             * Returns whether the requested feature is on or not.
             * Returns false for falsy feature.
             * The test for the feature is case-insensitive.
             * @param {string} feature
             * @returns {boolean}
             */
            exports.isFeatureOn = function (feature) {
                if (!feature) return false;
                var featureSwitches = sp.result(exports, 'initialSettings.featureSwitches') || '';
                var re = new RegExp('\\b' + feature + '\\b', 'i');
                return re.test(featureSwitches);
            };


            return exports;
        })
        .factory('spAppError', function () {

            //todo: this is rough and needs merging with the alerter and whatever else we have regarding alerts and errors

            var exports = {
                errors: [],

                haveErrors: function () {
                    return this.errors.length > 0;
                },

                add: function(msg) {
                    console.warn('appError: adding "%s"', msg);
                    this.errors.push(msg);
                },

                clear: function () {
                    this.errors = [];
                },

                // specific error states

                urlError: null
            };

            return exports;
        })
    ;
}());
