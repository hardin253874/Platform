// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, $, console, _, base64, sp, Globalize, FastClick */

(function () {
    'use strict';

    window.document.spAppKeyhole = {
        log: function (msg) {
            console.log('keyhole.log: ', msg);
        }
    };

    var getCurrentTenant = function () {
    };

    angular.module('app')

        .config(function ($stateProvider, $urlRouterProvider, $rootScopeProvider, $uibTooltipProvider, $httpProvider) {

            function sanitiseTenantLanding() {
                var routeToTenantLanding = function () {
                    var currentTenant = getCurrentTenant();
                    if (currentTenant) {
                        return '/' + currentTenant + '/';
                    }
                    return false;
                };

                $urlRouterProvider.when('', routeToTenantLanding);
                $urlRouterProvider.when('/', routeToTenantLanding);
                $urlRouterProvider.when('/{tenant}', '/{tenant}/');
            }

            // increase the max digest cycles because edit forms are very deep.
            $rootScopeProvider.digestTtl(20);

            $stateProvider.state('landing', {
                url: '/{tenant}/',

                // the app launcher stuff here and mobile footer is temporary... for alpha/demo
                templateUrl: 'navigation/appLauncher.tpl.html',
                data: {
                    region: {}
                }
            });

            $stateProvider.state('landinghome', {
                url: '/{tenant}/home',

                // the default landing page when logo is clicked. This is to stay on landing page and to avoid re-routing to last used app.
                templateUrl: 'navigation/appLauncher.tpl.html',
                data: {
                    region: {}
                }
            });

            sanitiseTenantLanding();

            // the loginRedirect state exists for when we return from an Open ID login
            $stateProvider.state('loginRedirect', {
                url: '/{tenant}//loginRedirect',
                template: '<div>login redirect: should not see this</div>'
            });

            $uibTooltipProvider.options({
                appendToBody: true,
                popupDelay: 400
            });

            $httpProvider.interceptors.push('spHttpServiceUnavailableInterceptor');
            $httpProvider.interceptors.push('spHttpChallengeInterceptor');

        })
        .run(function ($browser, $rootScope, $window, spState, spAppSettings, spWebService, spLoginService, spLocalStorage) {

            var startTime = new Date();
            var contentLoadedTime;

            $rootScope.appData = $rootScope.appData || {};

            getCurrentTenant = spLoginService.signedInTenant;

            var unhook = $rootScope.$on('contentLoadedTiming', function () {
                contentLoadedTime = new Date().getTime();
                var endTime = new Date().getTime();
                var duration = endTime - startTime;
                console.log('Time until content loaded:', duration, 'ms');
                unhook();
            });

            // expose some access for automated testing

            var fns = $window.document.spAppKeyhole = window.document.spAppKeyhole || {};

            fns.getCurrentStateActions = function (opts) {

                console.log('appKeyHole.getCurrentStateActions: ' + _.keys(spState.data.actionApi).join());

                return {
                    stateName: spState.name,
                    actions: spState.data.actionApi
                };
            };

            fns.executeAction = function (action, opts, cb) {
                var actionFn = spState.data.actionApi && spState.data.actionApi[action],
                    result = actionFn && actionFn(opts);

                if (!actionFn) {
                    console.log('appKeyHole.executeAction: cannot find "' + action + '" in ' + _.keys(spState.data.actionApi).join());
                }

                if (result && result.then) {
                    result.then(function (result) {
                        cb(!!result);
                    });
                } else {
                    cb(!!result);
                }
            };

            fns.setTestMode = function (value, testIdentity) {
                $rootScope.$apply(function () {
                    console.warn('Setting TEST MODE: ' + value);

                    $rootScope.__spTestMode = value;
                    $rootScope.__spTestCulture = value ? 'en-US' : undefined;

                    if (value) {
                        $window.document.spAppKeyhole.$rootScope = $rootScope;
                        spLocalStorage.clear();

                        // this is an attempt to find the current language settings
                        // - almost works except for issues with blocked script calls
                        // - to come back to this... maybe
                        // - thoughts are to have our own service call
                        //                        $.ajax({
                        //                            url: "http://ajaxhttpheaders.appspot.com",
                        //                            dataType: 'jsonp',
                        //                            success: function(headers) {
                        //                                console.log('ajax headers', headers);
                        //                                $rootScope.__spHostLanguage = headers['Accept-Language'];
                        //                            }
                        //                        });
                    }
                    console.warn('Done setting TEST MODE: signedInTenant:' + (spLoginService.signedInTenant() || 'null'));
                });
            };
            fns.setServer = function (server) {
                $rootScope.$apply(function () {
                    spAppSettings.setServer(server);
                });
            };
            fns.setAuthorization = function (auth) {
                console.warn('TEST AUTH being set now. signedInTenant:' + (spLoginService.signedInTenant() || 'null'));
                $rootScope.$apply(function () {
                    spWebService.setHeader('Authorization', auth);
                    console.warn('TEST AUTH was set... signed in = ' + !!spLoginService.getAuthenticatedIdentity(true));
                });
            };
            fns.logout = function () {
                console.warn('testSupport: Logging out');
                $rootScope.$apply(function () {
                    spLocalStorage.removeItem('userIdentity');
                });
            };
        })
        .run(function ($rootScope, $templateCache, $location, $window, titleService, spAppError, spAppSettings,
                       spWebService, spLocalStorage, spLoginService, spMobileContext) {

            var absoluteUrl = $location.absUrl();
            var host = $location.host();
            var path = absoluteUrl.substring(absoluteUrl.indexOf(host) + host.length);
            var start = path.indexOf('/');
            var end = path.indexOf('/sp');
            path = path.substring(start, end - start);

            spWebService.setWebApiRoot(path);

            titleService.setSuffix(' | ReadiNow');

            // set application wide data

            $rootScope.appData = _.extend($rootScope.appData || {}, {
                settings: spAppSettings,
                layoutTemplate: 'navigation/defaultLayout.tpl.html',
                isMobileDevice: false
            });

            // device detection
            spMobileContext.setContextFromWindow($window);
            spMobileContext.setContextFromLocation($location);

            var isMobile = spMobileContext.isMobile;
            var isTablet = spMobileContext.isTablet;

            var body = angular.element($window.document.body);

            if (isMobile) {
                $rootScope.appData.layoutTemplate = 'navigation/mobileLayout.tpl.html';
                body.addClass('sp-mobile-device');
            }

            if (isTablet) {
                $rootScope.appData.layoutTemplate = 'navigation/tabletLayout.tpl.html';
                body.addClass('sp-tablet-device');
            }

            if (isTablet || isMobile) {
                ////
                // Add the fastclick polyfiller to remove the delay on touch screens
                // (On non-touch devices there should be no effect
                $window.addEventListener('load', function () {
                    new FastClick(document.body);
                }, false);
            }


            if (!(isTablet || isMobile)) {
                body.addClass('sp-desktop-device');
            }

            // if (navigator && navigator.geolocation) {
            //     navigator.geolocation.getCurrentPosition(function (position) {
            //         $rootScope.appData.geolocation = {
            //             coords: {
            //                 longitude: position.coords.longitude,
            //                 latitude: position.coords.latitude
            //             }
            //         };
            //     });
            // }

            console.log('userAgent: ', $window.navigator.userAgent);

            // 
            // Set an alternative server

            var server = $location.search().server;
            if (!_.isUndefined(server)) {
                spAppSettings.setServer(server);
            }

            // 
            // Set an specific test local

            var testLocale = $location.search().testLocale;
            if (!_.isUndefined(testLocale)) {
                $rootScope.__spTestCulture = testLocale;
            }

            // restart the whole app, returning to the current URL

            $rootScope.$on('sp.app.restart', function (e, reason) {

                console.log('Restarting app' + (reason ? '. Reason = ' + reason : ''));

                if ($rootScope.__spTestMode) {
                    console.warn('Skipping app restart as running in test mode');

                } else {
                    $window.location.reload();

                }
            });

            spAppSettings.initialise();


            // set active account Id
            //spLoginService.accountId = spLocalStorage.getItem('accountId');     // WHY? This is done by log-in service

        })
        .run(function ($window, $rootScope, spMobileContext) {

            var diagnosticsVisible = false;

            $window.document.addEventListener('keydown', function (e) {

                //console.log('keydown', e.which, e.altKey, e.ctrlKey);

                if (e.which === 68 && e.altKey && e.ctrlKey) {
                    // Ctrl+Alt+Shift+D
                    toggleDiagnostics();
                }
                if (e.which === 76 && e.altKey && e.ctrlKey) {
                    // Ctrl+Alt+Shift+L
                    enableLogCapture();
                }
                if (e.which === 70 && e.altKey && e.ctrlKey) {
                    // Ctrl+Alt+Shift+B
                    provideFeedback();
                    e.preventDefault();
                }
                //if (e.which === 77 && e.altKey && e.ctrlKey) {
                //    // Ctrl+Alt+Shift+M
                //    toggleMobile();
                //    e.preventDefault();
                //}
                if (e.which === 83 && e.altKey && e.ctrlKey) {
                    // Ctrl+Alt+Shift+S
                    //dumpStuff();

                    // dragAndDrop("td[sp-draggable-data='field']:contains(Lookup)", ".sp-form-builder-container:not([field-container])");
                    // dragAndDrop("li[title*='Clone']", ".workflow-design-surface [diagram=diagram]", {x: 100, y: 200});
                    // dragAndDrop("li[title*='Create']", ".workflow-design-surface [diagram=diagram]", {x: 200, y: 200});
                    // dragAndDrop('.nb-entry:contains(Screen)', '.client-nav-panel');
                    // dragAndDrop(".nb-entry:contains('New Chart')", ".client-nav-panel div:contains('RT-Chart-Section')");
                    dragAndDrop(".chart-series .panel[test-series*='Born Bar'] .panel-title div[sp-draggable]",
                        ".chart-series .panel[test-series*='Test'] .panel-title div[sp-draggable]");
                    // dragAndDrop('.nb-entry:contains(New Folder)', '.client-nav-panel div:contains(Home)');
                    // dragAndDrop('.chart-builder-toolbox .item .itemname:contains(Name)', '.series-panel .dropArea');
                    // dragAndDrop('.fb-toolbox-objectsviewer .itemname:contains(AA_Actor Form)', '.sp-form-builder-container-content');
                    // setTimeout(function () {
                    //     $rootScope.$apply(function () {
                    //         dragAndDrop('.fb-toolbox-objectsviewer .itemname:contains(Force - AA_Employee)', '.sp-form-builder-container-content:first');
                    //     });
                    // }, 5000);
                }
            });

            function toggleDiagnostics() {

                var element = $('#diagnostics');

                if (diagnosticsVisible) {
                    element.hide();
                } else {
                    element.show();
                }

                diagnosticsVisible = !diagnosticsVisible;

                element.trigger('isVisible');
            }

            function dumpStuff() {
                function isNgProp(s) {
                    return s[0] === '$';
                }

                function dumpScope(s, indent) {
                    if (!s) return;

                    console.log('scope', indent, s.$id, _.reject(_.keys(s), isNgProp).join());

                    dumpScope(s.$$childHead, indent + ' ');
                    dumpScope(s.$$nextSibling, indent);
                }

                dumpScope($rootScope, '');
            }

            function dragAndDrop(from, to, opts) {
                var spDragDropSimService = angular.element('body').injector().get('spDragDropSimService');
                console.log('sim dragAndDrop: ' + from + ', ' + to);
                spDragDropSimService.dragAndDrop(angular.element(from), angular.element(to), opts);
            }

            function toggleMobile() {
                var body = angular.element($window.document.body);
                var isMobile = spMobileContext.isMobile = !spMobileContext.isMobile;

                if (isMobile) {
                    $rootScope.appData.layoutTemplate = 'navigation/mobileLayout.tpl.html';
                    body.addClass('sp-mobile-device');
                    body.removeClass('sp-desktop-device');
                } else {
                    $rootScope.appData.layoutTemplate = 'navigation/defaultLayout.tpl.html';
                    body.addClass('sp-desktop-device');
                    body.removeClass('sp-mobile-device');
                }
                $rootScope.$broadcast('app.layout');
            }

            function provideFeedback() {
                $rootScope.$broadcast('sp.showFeedbackForm');
            }

            function enableLogCapture() {
                $rootScope.$broadcast('sp.enableLogCapture');
            }

        })
        //
        // This interceptor redirects to the service unavailable page when a 503 is returned.
        .factory('spHttpServiceUnavailableInterceptor', function ($q, $window) {
            return {

                'responseError': function (rejection) {
                    if (rejection.status === 503 && rejection.data && rejection.data.redirectUrl) {
                        $window.location.href = rejection.data.redirectUrl;
                    } else {
                        return $q.reject(rejection);
                    }
                }
            };
        });

}());
