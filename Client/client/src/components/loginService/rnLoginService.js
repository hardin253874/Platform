// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */



(function () {
    'use strict';

    angular.module('sp.common.rnLoginService', ['mod.common.spWebService', 'http-auth-interceptor'])
        .factory('rnLoginService', function ($http, spWebService, authService) {

            var exports = {};

            function baseUri() {
                return spWebService.getWebApiRoot() + '/spapi/data/v1/login/';
            }

            function spSignIn(tenant, username, password, persistent) {
                var url = baseUri() + 'spsignin';                

                var payload = {
                    username: username,
                    password: password,
                    tenant: tenant,
                    persistent: persistent
                };

                return $http({ method: 'POST', url: url, data: payload, ignoreAuthModule: true }).then(handleLoginSuccess);
            }

            /**
            * Login using openid to the readiNow server.
            * returns a promise for {initialSettings, accountInfo}
            */
            function login(tenant, username, password, persistent) {
                function doSignIn() {
                    return spSignIn(tenant, username, password, persistent);
                }

                // Check for IE & Edge
                if (document.documentMode || navigator.userAgent.match(/MSIE|Edge/i)) {
                    // Hack, for IE, do a noop get and then a POST            
                    return $http({ method: 'GET', url: spWebService.getWebApiRoot() + '/spapi/data/v1/console/noop', ignoreAuthModule: true }).then(doSignIn, doSignIn);                    
                } else {
                    return spSignIn(tenant, username, password, persistent);
                }                
            }
            exports.login = login;


            /**
            * Login using an existing cookie. This is used by KMSI
            */
            function loginWithCookie() {
                var url = baseUri() + 'spsignincookie';
                return $http({ method: 'GET', url: url, ignoreAuthModule: true }).then(handleLoginSuccess);
            }
            exports.loginWithCookie = loginWithCookie;


            function handleLoginSuccess(result) {
                var data = result.data;

                authService.loginConfirmed();

                return { initialSettings: data.initialSettings, activeAccountInfo: data.activeAccountInfo, testToken: data.testToken, consoleTimeoutMinutes: data.consoleTimeoutMinutes };

            }


            function signout() {
                var url = baseUri() + 'signout';


                return $http({ method: 'POST', url: url, data: {}, ignoreAuthModule: true }).then(
                    function(result) {
                        return result;
                    },
                    function(err) {
                        console.log('rnLoginServer: signout failed.', err);
                        throw err;
                    });
            }

            function signoutAuth() {
                var url = baseUri() + 'signoutauth';

                return $http({ method: 'POST', url: url, data: {headers: spWebService.getHeaders()}, ignoreAuthModule: true }).then(function(result) {
                    return result;
                },
                function(err) {
                    console.log('rnLoginServer: signoutAuth failed.', err);
                    throw err;
                });
            }

            function doSignout() {
                // Call the authenticated signout method first.
                // If it fails call the anonymous one.
                authService.loginCancelled();

                return signoutAuth().catch(function() {                                    
                    return signout();
                });
            }        

            exports.signout = doSignout;

            function changePassword(tenant, username, oldPassword, newPassword) {
                var url = baseUri() + 'spchangepassword';

                var payload = {
                    username: username,
                    oldpassword: oldPassword,
                    newpassword: newPassword,
                    tenant: tenant                    
                };

                return $http({ method: 'POST', url: url, data: payload, ignoreAuthModule: true });
            }

            exports.changePassword = changePassword;


            function submitEmail(email, tenant) {
                var url = baseUri() + 'spsubmitemail';

                var payload = {
                    email: email,
                    tenant: tenant
                };

                return $http({ method: 'POST', url: url, data: payload, ignoreAuthModule: true });
            }

            exports.submitEmail = submitEmail;


            function resetPassword(key, newPassword, tenant) {
                var url = baseUri() + 'spresetpassword';

                var payload = {
                    key: key,
                    newpassword: newPassword,
                    tenant: tenant
                };

                return $http({ method: 'POST', url: url, data: payload, ignoreAuthModule: true });
            }
            exports.resetPassword = resetPassword;

            return exports;
        });
}());
