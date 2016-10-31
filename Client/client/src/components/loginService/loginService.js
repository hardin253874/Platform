// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */


// Notes
// The behaviour around being able to authenticate with as a diff tenant or user has changed.
// This service has been adjusted for that behaviour but not yet refactored to do it cleanly.

(function () {
    'use strict';

    angular.module('sp.common.loginService', [
        'ui.router',
        'ngCookies',
        'mod.common.spWebService',
        'mod.common.spLocalStorage',
        'sp.app.settings',
        'mod.common.spEntityService',
        'mod.common.spConsoleService',
        'sp.common.rnLoginService',
        'sp.common.clientUpdateService',
    ])
        .value("spLoginServiceSettings", {
            consoleTimeoutMinutes: 60,
            cookiePollTimeout: 2
        })
        .factory('spLoginService', function ($location, $rootScope, $timeout, $interval, $http, $state, $window, $cookies, spWebService, spLocalStorage, spAppSettings, spAppError, spEntityService, spConsoleService, rnLoginService, spLoginServiceSettings, spClientUpdateService) {
           
            var redirectStateKey = 'redirectState';
            var redirectState = spLocalStorage.getObject(redirectStateKey);
            var challengeHandle;
            var authenticatedIdentity;
            var restarting = false;

            var testIdentity = {
                tenant: 'EDC',
                username: 'TEST-ADMIN',
                provider: 'basicAuth'
            };

            var putLoginCookie = _.partial(spUtils.putCookie, 'restartId');
            var getLoginCookie = _.partial(spUtils.getCookie, 'restartId');
            var removeLoginCookie = _.partial(spUtils.deleteCookie, 'restartId');

            var cookiePollTimerHandle;
            var lastCookieValue;

            var exports = {};
            //exports.accountId = spLocalStorage.getItem('accountId');            // WHYY?
            exports.changePasswordAtNextLogon = null;
            exports.loggedInAccountDisplayText = null;
            exports.identityProviderLogin = identityProviderLogin;

            $rootScope.$on('event:issueChallenge', challenge);

            /**
            * Login using openid to the readiNow server
            *
            */
            function readiNowLogin(tenant, username, password, persistent, requestTestToken) {

                return rnLoginService.login(tenant, username, password, persistent)
                            .then(_.partial(completeLogin, persistent, requestTestToken));
            }

            exports.readiNowLogin = readiNowLogin;


            /**
            * Login using the Cookie as part of Keep Me Signed In
            *
            */
            function readiNowLoginWithCookie() {

                return rnLoginService.loginWithCookie()
                            .then(_.partial(completeLogin, true, false));
            }

            exports.readiNowLoginWithCookie = readiNowLoginWithCookie;

            //
            // Use to speed up login during intg tests.
            // 
            function testSupportReadiNowLogin(oldResult) {
                return completeLogin(false, true, oldResult);
            }
            exports.testSupportReadiNowLogin = testSupportReadiNowLogin;

            //
            // Finish the login process
            //
            function completeLogin(persistent, requestTestToken, data) {

                putLoginCookie(data.activeAccountInfo.tenant + '/' + data.activeAccountInfo.accountId);

                // Check min client version
                if (!spClientUpdateService.checkMinClientVersionAndRefresh(spAppSettings.clientVersion, data.initialSettings.requiredClientVersion)) {
                    // the version didn't match, stop any further processing.
                    return data;
                }

                // test support
                // The authorization header is only used during integration tests because cookies are not supported by the Karma test runner
                if (requestTestToken && data.testToken) {
                    spWebService.setHeader('Authorization', data.testToken);
                }
                // test support

                spAppSettings.setInitialSettings(data.initialSettings);

                setAuthenticatedIdentity(
                    data.activeAccountInfo.tenant,
                    data.activeAccountInfo.username,
                    data.activeAccountInfo.provider,                    
                    data.activeAccountInfo.accountId,
                    persistent
                );

                updateAppDataIdentity(data.activeAccountInfo.tenant, data.activeAccountInfo.username); // NEED TO FIND OUT WHO IS USING THIS AND MOVE THEM TO THE SERVICE


                if (!requestTestToken) {                    // if we are using the testToken then cookies are not supported so we can't poll for them
                    startCookiePollForLogout();
                }

                notifyLoggedIn();

                if (data.consoleTimeoutMinutes)
                    spLoginServiceSettings.consoleTimeoutMinutes = data.consoleTimeoutMinutes;

                // do some tasks like set loggedInAccountDisplayText (either username or account holder name), changePasswordAtNextLogon
                performTasksOnSuccessfulLogin();            

                return data;
            }


            /**
             * If we are authenticated then return an object with the details otherwise return null.
             */
            function getAuthenticatedIdentity() {
                return authenticatedIdentity;
            }

            exports.getAuthenticatedIdentity = getAuthenticatedIdentity;




            // set the indentity, update the local store if we are persisting it.
            function setAuthenticatedIdentity(tenant, username, provider, accountId, persistent) {
                authenticatedIdentity = {
                    tenant: tenant,
                    username: username,
                    provider: provider,                    
                    persistent: persistent
                };

                exports.accountId = accountId;

                if (persistent) {
                    spLocalStorage.setItem('accountId', accountId);

                    spLocalStorage.setObject('userIdentity', {
                        tenant: tenant,
                        username: username,
                        provider: provider,                        
                        persistent: persistent
                    });
                }
            }

            function clearPersistentIdentity() {
                if (spLocalStorage.testLocalStorage()) {
                    spLocalStorage.removeItem('userIdentity');
                    spLocalStorage.removeItem('accountId');
                    spLocalStorage.removeItem('tryAuthCookie');
                }
                $cookies.remove('tryAuthCookie');
            }
           

            /**
             * Save the redirect state and parameters for use post login.
             */
            function saveRedirect(toState, toParams) {

                if (toState && toState.name && toParams && toParams.tenant) {
                    redirectState = { toState: toState, toParams: toParams };
                    spLocalStorage.setObject(redirectStateKey, redirectState);
                } else {
                    clearRedirect();
                }
            }

            exports.saveRedirect = saveRedirect;

            function getPendingTenant() {
                return redirectState ? redirectState.toParams.tenant : undefined;
            }

            exports.getPendingTenant = getPendingTenant;

            /**
             * Clear the redirect state and parameters.
             */
            function clearRedirect() {
                redirectState = null;
                spLocalStorage.removeItem(redirectStateKey);
            }

            exports.clearRedirect = clearRedirect;

            /**
             * Send notifications and perform any pending redirects.
             */
            function notifyLoggedIn() {
                $rootScope.$broadcast('signedin', getAuthenticatedIdentity());
            }

            exports.notifyLoggedIn = notifyLoggedIn;

            function notifyLoggedOut() {
                $rootScope.$broadcast('signedout');
            }

            exports.notifyLoggedOut = notifyLoggedOut;

            /**
             * Redirect to the saved redirect path, if one. Typically called post successful login.
             */
            function redirectAfterLogin() {

                var identity = getAuthenticatedIdentity();

                if (!identity) {
                    spAppError.add('Attempted redirect after signed in');

                } else {

                    if (redirectState && redirectState.toParams.tenant.toLowerCase() === identity.tenant.toLowerCase()) {
                        $state.go(redirectState.toState.name, redirectState.toParams);
                    } else {
                        $state.go('home', { tenant: identity.tenant });
                    }

                }
            }

            exports.redirectAfterLogin = redirectAfterLogin;

            function getBaseUrl() {
                return $location.absUrl().split('#')[0];
            }

            function getHomeUrl(tenant) {
                return getBaseUrl() + '#/' + tenant;
            }

            function getLoginReturnUrl(tenant) {
                return getHomeUrl(tenant) + '//loginRedirect';
            }

            exports.getLoginReturnUrl = getLoginReturnUrl;

            /**
             * Sign out, with optional reload of the application (defaults to reload).
             * If it does reload it will reload to the base URL for the tenant in the state/URL.
             */
            function logout(reload) {
                removeLoginCookie();

                return rnLoginService.signout()
                    .then(function () {
                        reload = !_.isUndefined(reload) ? reload : true;

                        if (authenticatedIdentity && authenticatedIdentity.persistent) {
                            clearPersistentIdentity();
                        }

                        spAppSettings.setInitialSettings(null);
                        spAppSettings.setSessionInfo(null);
                        exports.accountId = undefined;
                        exports.changePasswordAtNextLogon = null;
                        exports.loggedInAccountDisplayText = null;
                        notifyLoggedOut();

                        if (reload && !restarting) {
                            restarting = true;
                            console.log('logout... RESTARTING');

                            ($rootScope.appData || {}).isRestarting = restarting;

                            // $rootScope.$emit('sp.app.restart', 'Logging out');   // THIS ISN"T WORKING IN RT TESTS BUT IT IS WHAT WE SHOULD USER

                            $window.location.href = getHomeUrl(sp.result($state, 'params.tenant'));
                            $window.location.reload();
                        }
                    });
            }

            exports.logout = logout;            

            function isSignedIn() {
                return !!getAuthenticatedIdentity();
            }

            exports.isSignedIn = isSignedIn;

            function signedInTenant() {
                var identity = getAuthenticatedIdentity();
                return identity && identity.tenant;
            }

            exports.signedInTenant = signedInTenant;

            function isSignedInAsTenant(tenant) {
                return tenant && signedInTenant() && tenant.toLowerCase() === signedInTenant().toLowerCase();
            }

            exports.isSignedInAsTenant = isSignedInAsTenant;
            
            function readiNowChangePassword(tenant, username, oldPassword, newPassword) {
                return rnLoginService.changePassword(tenant, username, oldPassword, newPassword);                            
            }

            function readiNowSubmitForgetPasswordEmail(email, tenant) {
                return rnLoginService.submitEmail(email, tenant);
            }

            function readiNowResetPassword(key, newPassword, tenant) {
                return rnLoginService.resetPassword(key, newPassword, tenant);
            }

            exports.readiNowChangePassword = readiNowChangePassword;
            exports.readiNowSubmitForgetPasswordEmail = readiNowSubmitForgetPasswordEmail;
            exports.readiNowResetPassword = readiNowResetPassword;
            var intervalTimer;
                  
            function updateAppDataIdentity(tenant, username) {

                // save some data in the root scope for convenience
                // todo - stop doing this... find out who is using it
                $rootScope.appData = _.extend($rootScope.appData || {}, {
                    authenticated: true,
                    tenant: tenant,
                    username: username
                });
            }

            /**
            * Perform tasks on successful login.
            */
            function performTasksOnSuccessfulLogin() {
                exports.changePasswordAtNextLogon = null;
                exports.loggedInAccountDisplayText = null;

                //spWebService.setHeader('X-ReadiNow-Client-Version', spAppSettings.initialSettings.platformVersion); // For information purposes only

                if (exports.isSignedIn() &&
                    exports.accountId) {

                    spConsoleService.getSessionInfo().then(function (sessionInfo) {
                        spAppSettings.setSessionInfo(sessionInfo);
                    });

                    spEntityService.getEntity(exports.accountId, 'changePasswordAtNextLogon, accountHolder.name', { hint: 'checklogin', batch: true }).then(function (account) {
                        if (account) {
                            // Check to see if the user must change the password at login time and caches it locally.
                            exports.changePasswordAtNextLogon = account.changePasswordAtNextLogon;

                            // if the current user account is linked to an accountholder then use the accountholder name else use username for display text
                            var accountHolder = account.getLookup('core:accountHolder');
                            if (accountHolder && accountHolder.idP) {
                                exports.accountHolderId = accountHolder.idP;
                            }
                            if (accountHolder && accountHolder.name) {
                                exports.loggedInAccountDisplayText = accountHolder.name;
                            }
                            else {
                                exports.loggedInAccountDisplayText = exports.getAuthenticatedIdentity().username;
                            }
                        }
                    });
                }
            }
            
            /**
             * Sends the Challenge request. Rely on the reauth to handle the result.
             */
            function challenge() {
                var url = spWebService.getWebApiRoot() + '/spapi/data/v1/login/challenge';
                if (spAppSettings.clientVersion) {
                    url += "?clientVersion=" + spAppSettings.clientVersion;                     // push up the client version. Used to force a refresh
                }
                    
                $http({
                    method: 'GET',
                    url: url,
                });
            }


            //
            // Polling for cookies
            // We are keeping track of our own cookie to determine if someone has logged in as a different user or has logged out of another tab
            //


            function cookiePollTest() {
                var cookieValue = getLoginCookie();

                if (cookieValue != lastCookieValue) {
                    endCookiePoll();
                    $rootScope.$emit('sp.app.restart', 'The XsrfToken cookie is missing, this indicates the user has logged off in another session.');
                } 
            }

            function startCookiePoll() {
                if (!cookiePollTimerHandle) {
                    cookiePollTimerHandle = $window.setInterval(cookiePollTest, spLoginServiceSettings.cookiePollTimeout * 1000);
                }
            }

            function endCookiePoll() {
                if (cookiePollTimerHandle) {
                    $window.clearInterval(cookiePollTimerHandle);
                    cookiePollTimerHandle = null;
                }
            }

            //
            // Look at a cookie to determine if a log-off has happened in another windw.
            function startCookiePollForLogout() {
                lastCookieValue = getLoginCookie();

                startCookiePoll();
            }
            

            /*
            Login using the specified identity provider
            */
            function identityProviderLogin(tenant, identityProviderId) {
                return $http({
                    method: 'POST',
                    url: spWebService.getWebApiRoot() + '/spapi/data/v1/login/oidc/authorizeurl',
                    ignoreAuthModule: true,
                    data: {
                        tenant: tenant,
                        idpId: identityProviderId,
                        redirectUrl: getLoginReturnUrl(tenant)
                    }
                }).then(function(response) {
                    $window.location = response.data;
                });
            }
         
            exports.isSignedInUsingProvider = isSignedInUsingProvider;

            function isSignedInUsingProvider(provider) {
                var identity = getAuthenticatedIdentity();
                var signedInProvider = sp.result(identity, 'provider');
                return provider && signedInProvider && signedInProvider.toLowerCase() === provider.toLowerCase();
            }

            return exports;
        })

        //
        // This interceptor keeps track of requests and fires off a challenge x minutes after the last request was made to trigger the reauth behaviour.
        //
        .factory('spHttpChallengeInterceptor', function ($window, $rootScope, $q, spLoginServiceSettings, spClientUpdateService) {
            //var ChallengeTimeout = 60 * 60 * 1000;                           // This should be change to be injected by the login process. THIS NEEDS TO MATCH THE SERVER VALUE

            var timerHandle;
            var lastResponseTime;
            var lastTimeoutValue;

            function issueChallenge() {
               $rootScope.$broadcast('event:issueChallenge');
            }


            function createTimeout() {
                if (spLoginServiceSettings.consoleTimeoutMinutes) {                                        
                    timerHandle = $window.setTimeout(issueChallenge, spLoginServiceSettings.consoleTimeoutMinutes * 60 * 1000);
                }
            }

            function cancelTimeout() {                
                if (timerHandle) {
                    $window.clearTimeout(timerHandle);
                    timerHandle = null;
                }
            }

            function createTimeoutDebounced() {
                var timeNow = new Date();            

                var doSetTimeout = !lastResponseTime || (timeNow - lastResponseTime > 5000) || (lastTimeoutValue !== spLoginServiceSettings.consoleTimeoutMinutes);

                if (doSetTimeout) {                        
                    // move the delay on
                    cancelTimeout();
                    createTimeout();

                    lastResponseTime = timeNow;
                    lastTimeoutValue = spLoginServiceSettings.consoleTimeoutMinutes;
                }
            }

            return {

                'response': function (response) {
                    createTimeoutDebounced();                 

                    return response;
                },

                'responseError': function (rejection) {     // stale login token
                    if (rejection.status === 401 ||
                        (rejection.status === 403 && sp.result(rejection, 'data.AdditionalInfo') === 'TokenExpired')) {         // reauth required
                        cancelTimeout();
                    } else {

                        createTimeoutDebounced();
                    }

                    return $q.reject(rejection);
                }
            };
        });



       

}());
