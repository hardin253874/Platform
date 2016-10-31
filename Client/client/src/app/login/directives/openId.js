// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () { 
    'use strict';

    angular.module('mod.app.login.directive', ['ngCookies', 'sp.common.loginService', 'sp.common.rnLoginService', 'mod.common.spWebService', 'mod.common.spLocalStorage', 'mod.common.spMobile', 'mod.common.spNgUtils'])
        .directive('spOpenId', function () {
            return {
                restrict: 'ACE',
                transclude: true,
                templateUrl: 'login/directives/openId.tpl.html',
                scope: true,
                replace: true,
                controller: function ($scope, $element, $attrs, $transclude, $http, $window, $location, $state, $stateParams, $cookies, spAppSettings, spLoginService, rnLoginService, spWebService, spLocalStorage, spMobileContext, spNgUtils) {

                    var TryAuthCookieKey = 'tryAuthCookie';
                    var LastSelectedIdentityProviderKey = 'lastSelectedIdentityProviderKey';
                    var tenantChanged = false;

                    /////
                    // The directive scope will inherit (prototypically) a reference to the object, rather than a copy of the primitive's value.
                    /////
                    $scope.model = {
                        username: null,
                        password: null,
                        email: null,
                        persistent: false,
                        tenant: null,
                        signingIn: false,
                        loginError: null,
                        changingPassword: false,
                        forgetPasswordEmail: false,
                        showForgetPasswordLink: false,
                        resetPassword: false,
                        newPassword: null,
                        key: null,
                        confirmNewPassword: null,
                        disableSubmit: false,
                        identityProvider: null,
                        identityProviders: [],
                        isReadiNowIdentityProvider: true
                    };

                    $scope.spMobileContext = spMobileContext;
                    $scope.spLocationSearchObject = $location ? $location.search() : null;

                    // Ensure the tenant in the model matches the one in the URL.
                    $scope.$watch('$stateParams.tenant', function (tenant) {
                        if (tenant) $scope.model.tenant = tenant;
                    });

                    $scope.$watch('spLocationSearchObject', function () {
                        if ($scope.spLocationSearchObject && $scope.spLocationSearchObject.type && $scope.spLocationSearchObject.type === 'reset' && $scope.spLocationSearchObject.key) {

                            $scope.model.resetPassword = true;

                            //get the user reset password key
                            $scope.model.key = $scope.spLocationSearchObject.key;
                        }
                    });

                    /////
                    // watch and verify the email value, if the email is not valid, disable the submit buttom
                    /////
                    $scope.$watch('model.email', function () {
                        //disable the email validation test by the latest request
                        $scope.model.disableSubmit = false;
                        //var EMAIL_REGEXP = /^[_a-z0-9]+(\.[_a-z0-9]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,5})$/;
                        //if ($scope.model.email && EMAIL_REGEXP.test($scope.model.email))
                        //    $scope.model.disableSubmit = false;
                        //else
                        //    $scope.model.disableSubmit = true;
                    });

                    $scope.showForgetPasswordEmail = function () {
                        $scope.model.loginError = null;
                        $scope.model.showForgetPasswordLink = false;
                        $scope.model.forgetPasswordEmail = true;
                    };

                    /////
                    // The sent email button on the ReadiNow provider was clicked.
                    /////
                    $scope.submitForgetPasswordEmailClick = function () {
                        $scope.model.loginError = null;

                        if (!$scope.model.email) {
                            $scope.model.loginError = 'Invalid email.';
                            return;
                        }

                        if (!$scope.model.tenant) {
                            $scope.model.loginError = 'Invalid tenant.';
                            return;
                        }

                        $scope.model.signingIn = true;


                        /////
                        // Post a request to the LoginController requesting a password change
                        /////
                        spLoginService.readiNowSubmitForgetPasswordEmail($scope.model.email, $scope.model.tenant)
                            .then(
                                function () {   // success
                                    $scope.model.signingIn = false;
                                    $scope.model.forgetPasswordEmail = false;
                                    $scope.model.email = null;
                                },
                                function (result) {    // fail
                                    console.log('submit email error: ', angular.toJson(result));

                                    $scope.model.signingIn = false;

                                    switch (result.status) {
                                        case 401:
                                        case 403:
                                            $scope.model.loginError = result.data.Message;
                                            break;

                                        case 404:
                                            $scope.model.loginError = result.data;
                                            break;

                                        default:
                                            if (result.data.ValidationError) {
                                                $scope.model.loginError = result.data.Message;
                                            } else if (result.data) {
                                                $scope.model.loginError = result.data;
                                            }
                                            else {
                                                // This does not represent a data leakage security risk as the response is trivially obtained by other means.
                                                $scope.model.loginError = 'Unexpected server error: ' + result.status;
                                            }
                                            break;
                                    }
                                });


                    };

                    /////
                    // The change password button on the ReadiNow provider was clicked.
                    /////
                    $scope.readiNowChangePasswordClick = function () {
                        $scope.model.loginError = null;

                        /////
                        // Perform some input validation.
                        /////
                        if (!$scope.model.username) {
                            $scope.model.loginError = 'Invalid username.';
                            return;
                        }

                        if (!$scope.model.tenant) {
                            $scope.model.loginError = 'Invalid tenant.';
                            return;
                        }

                        if ($scope.model.password === $scope.model.newPassword) {
                            $scope.model.loginError = 'The new password must be different to the old password.';
                            return;
                        }

                        if (!$scope.model.newPassword) {
                            $scope.model.loginError = 'New password is required.';
                        }

                        if (!$scope.model.confirmNewPassword) {
                            $scope.model.loginError = 'Confirm password is required.';
                        }

                        if ($scope.model.newPassword !== $scope.model.confirmNewPassword) {
                            $scope.model.loginError = 'The new passwords do not match.';
                            return;
                        }

                        $scope.model.signingIn = true;

                        /////
                        // Post a request to the LoginController requesting a password change
                        /////
                        spLoginService.readiNowChangePassword($scope.model.tenant, $scope.model.username, $scope.model.password, $scope.model.newPassword)
                            .then(
                                function () {   // success
                                    $scope.model.signingIn = false;
                                    $scope.model.changingPassword = false;
                                    $scope.model.password = null;
                                    $scope.model.newPassword = null;
                                    $scope.model.confirmNewPassword = null;
                                },
                                function (result) {    // fail
                                    console.log('change password error: ', angular.toJson(result));

                                    $scope.model.signingIn = false;

                                    switch (result.status) {
                                        case 401:
                                        case 403:
                                            $scope.model.loginError = result.data.Message;
                                            break;

                                        case 404:
                                            $scope.model.loginError = 'Unable to connect to server.';
                                            break;

                                        default:
                                            if (result.data.ValidationError) {
                                                $scope.model.loginError = result.data.Message;
                                            } else {
                                                // This does not represent a data leakage security risk as the response is trivially obtained by other means.
                                                $scope.model.loginError = 'Unexpected server error: ' + result.status;
                                            }
                                            break;
                                    }
                                });
                    };


                    /////
                    // The Reset password button on the ReadiNow provider was clicked.
                    /////
                    $scope.readiNowResetPasswordClick = function () {
                        $scope.model.loginError = null;

                        /////
                        // Perform some input validation.
                        /////
                        if (!$scope.model.key) {
                            $scope.model.loginError = 'The account key is invalid.';
                        }


                        if (!$scope.model.newPassword) {
                            $scope.model.loginError = 'New password is required.';
                        }

                        if (!$scope.model.confirmNewPassword) {
                            $scope.model.loginError = 'Confirm password is required.';
                        }

                        if ($scope.model.newPassword !== $scope.model.confirmNewPassword) {
                            $scope.model.loginError = 'The new password does not match the confirm password. Please type both passwords again.';
                            return;
                        }

                        $scope.model.signingIn = true;

                        /////
                        // Post a request to the LoginController requesting a password change
                        /////
                        spLoginService.readiNowResetPassword($scope.model.key, $scope.model.newPassword, $scope.model.tenant)
                            .then(
                                function () {   // success
                                    $scope.model.signingIn = false;
                                    $scope.model.resetPassword = false;
                                    $scope.model.key = null;
                                    $scope.model.newPassword = null;
                                    $scope.model.confirmNewPassword = null;
                                },
                                function (result) {    // fail
                                    console.log('reset password error: ', angular.toJson(result));

                                    $scope.model.signingIn = false;

                                    switch (result.status) {
                                        case 401:
                                        case 403:
                                            $scope.model.loginError = result.data.Message;
                                            break;

                                        case 404:
                                            $scope.model.loginError = result.data;
                                            break;

                                        default:
                                            if (result.data.ValidationError) {
                                                $scope.model.loginError = result.data.Message;
                                            } else if (result.data) {
                                                $scope.model.loginError = result.data;
                                            }

                                            else {
                                                // This does not represent a data leakage security risk as the response is trivially obtained by other means.
                                                $scope.model.loginError = 'Unexpected server error: ' + result.status;
                                            }
                                            break;
                                    }
                                });
                    };

                    /**
                     * Gets the identity providers for the specified tenant
                     * @param {} tenant
                     * @returns {}
                     */
                    function getIdentityProvidersForTenant(tenant) {
                        if (!tenant) {
                            return;
                        }

                        $http({
                                method: 'GET',
                                url: spWebService.getWebApiRoot() + '/spapi/data/v1/login/idproviders',
                                params: {
                                    tenant: tenant
                                }
                            })
                            .success(function (result) {
                                var lastSelectedIdProvider = null;

                                $scope.model.identityProviders = _.sortBy(result.identityProviders, ['ordinal', 'name']);

                                if (spLocalStorage.testLocalStorage()) {
                                    lastSelectedIdProvider = spLocalStorage.getItem(LastSelectedIdentityProviderKey);
                                    $scope.model.identityProvider = _.find($scope.model.identityProviders, function (value) { return value.name === lastSelectedIdProvider; });
                                }
                                
                                if (!$scope.model.identityProvider) {
                                    $scope.model.identityProvider = _.first($scope.model.identityProviders);
                                }
                                
                                $scope.onIdentityProviderChanged($scope.model.identityProvider);
                            })
                            .error(function (result) {
                                $scope.model.loginError = 'Unexpected server error: ' + result.status;
                                $scope.model.identityProviders = [];
                                $scope.model.identityProvider = null;

                                $scope.onIdentityProviderChanged($scope.model.identityProvider);
                            });
                    }


                    // Watch for tenant changes and update the list of providers
                    $scope.$watch('model.tenant', _.debounce(function (tenant) {
                        if (!tenant) {
                            return;
                        }

                        // Get list of identity providers for tenant
                        getIdentityProvidersForTenant(tenant);
                    }, 200));


                    // Watch for current provider
                    $scope.onIdentityProviderChanged = function (identityProvider) {
                        if (!sp.result($scope, 'model.identityProviders.length')) {
                            // Have no providers default to native
                            $scope.model.isReadiNowIdentityProvider = true;
                            return;
                        }

                        if (sp.result(identityProvider, 'typeAlias') === 'core:readiNowIdentityProvider') {
                            $scope.model.isReadiNowIdentityProvider = true;
                        } else {
                            $scope.model.isReadiNowIdentityProvider = false;
                        }
                    };

                    /////
                    // The login button was clicked.
                    /////
                    $scope.loginClick = function () {

                        if (!$scope.model.identityProvider &&
                            sp.result($scope, 'model.identityProviders.length')) {
                            $scope.model.loginError = 'Invalid identity provider.';
                            return;
                        }

                        if ($scope.model.isReadiNowIdentityProvider) {
                            $scope.readiNowLogin();
                        } else {
                            $scope.identityProviderLogin();
                        }
                    };

                    /////
                    // Login using the readi now login provider
                    /////
                    $scope.readiNowLogin = function () {

                        $scope.model.loginError = null;

                        /////
                        // Perform some input validation.
                        /////
                        if (!$scope.model.username) {
                            $scope.model.loginError = 'Invalid username.';
                            return;
                        }

                        if (!$scope.model.tenant) {
                            $scope.model.loginError = 'Invalid tenant.';
                            return;
                        }

                        if (tenantChanged) {
                            // Keep the url in sync.
                            // Shouldn't need to do a $scope.apply but Iphone 6 this seems to stop the keyboard from appearing after log-in. (see 27241)
                            spNgUtils.updateUrlSetTenantSegment($scope.model.tenant);
                            tenantChanged = false;
                        }

                        $scope.model.signingIn = true;
                        $scope.model.changingPassword = false;
                        $scope.model.showForgetPasswordLink = false;


                        /////
                        // Post a request to the LoginController requesting a login for the specified payload.
                        /////
                        spLoginService.readiNowLogin($scope.model.tenant, $scope.model.username, $scope.model.password, $scope.model.persistent)
                            .then(
                                function () {   // success
                                    addLoginCookies($scope.model.persistent, $scope.model.identityProvider.name);
                                },
                                function (result) {    // fail
                                    console.log('openId error: ', angular.toJson(result));

                                    $scope.model.signingIn = false;


                                    switch (result.status) {
                                        case 401:
                                            if (result.data.PasswordExpired) {
                                                $scope.model.password = null;
                                                $scope.model.newPassword = null;
                                                $scope.model.confirmNewPassword = null;
                                                $scope.model.changingPassword = true;
                                                $scope.model.loginError = 'Your password has expired. You must change your password before signing in.';
                                            } else {
                                                $scope.model.showForgetPasswordLink = true;
                                                $scope.model.loginError = result.data.Message;
                                            }
                                            break;

                                        case 403:
                                            $scope.model.loginError = result.data.Message;
                                            break;

                                        case 404:
                                            $scope.model.loginError = 'Unable to connect to server.';
                                            break;

                                        default:
                                            // This does not represent a data leakage security risk as the response is trivially obtained by other means.
                                            $scope.model.loginError = 'Unexpected server error: ' + result.status;
                                            break;
                                    }
                                });
                    };


                    // Login using the chosen identity provider
                    $scope.identityProviderLogin = function() {
                        $scope.model.loginError = null;

                        /////
                        // Perform some input validation.
                        /////

                        if ($scope.model.identityProvider.typeAlias !== 'core:oidcIdentityProvider') {
                            $scope.model.loginError = 'Invalid identity provider.';
                            return;
                        }

                        if (!$scope.model.tenant) {
                            $scope.model.loginError = 'Invalid tenant.';
                            return;
                        }

                        if (tenantChanged) {
                            // Keep the url in sync.
                            // Shouldn't need to do a $scope.apply but Iphone 6 this seems to stop the keyboard from appearing after log-in. (see 27241)
                            spNgUtils.updateUrlSetTenantSegment($scope.model.tenant);
                            tenantChanged = false;
                        }

                        $scope.model.signingIn = true;
                        $scope.model.changingPassword = false;
                        $scope.model.showForgetPasswordLink = false;


                        /////
                        // Post a request to the LoginController requesting a login for the specified payload.
                        /////
                        spLoginService.identityProviderLogin($scope.model.tenant, $scope.model.identityProvider.id)
                            .then(
                                function () {   // success

                                    addLoginCookies($scope.model.persistent, $scope.model.identityProvider.name);
                                },
                                function (result) {    // fail                                    
                                    $scope.model.signingIn = false;

                                    switch (result.status) {
                                        case 401:                                            
                                            $scope.model.loginError = result.data.Message;
                                            break;

                                        case 403:
                                            $scope.model.loginError = result.data.Message;
                                            break;

                                        case 404:
                                            $scope.model.loginError = 'Unable to connect to server.';
                                            break;

                                        default:
                                            // This does not represent a data leakage security risk as the response is trivially obtained by other means.
                                            $scope.model.loginError = 'Unexpected server error: ' + result.status;
                                            break;
                                    }
                                });
                    };



                    $scope.onTenantChanged = function () {
                        tenantChanged = true;
                    };

                    /**
                    * Add the session and local storage information post login login cookies
                    */
                    function addLoginCookies(persistent, identityProviderName) {
                        if (spLocalStorage.testLocalStorage()) {
                            spLocalStorage.setItem(TryAuthCookieKey, persistent);
                            spLocalStorage.setItem(LastSelectedIdentityProviderKey, identityProviderName);
                        }

                        $cookies.put(TryAuthCookieKey, true);
                    }

                    /////
                    // Initialize the controller.
                    /////
                    function initialize() {
                        var search;                        
                        var errorType;

                        if (_.endsWith(_.toLower($location.path()), '//loginredirect')) {
                            if (spLoginService.isSignedIn()) {
                              $scope.model.showSignin = false;
                              return;
                            }

                            search = $location.search();
                            if (_.has(search, 'error')) {       
                              errorType = _.toLower(search.error);
                              if (errorType === 'idpconfigerror') {
                                $scope.model.loginError = 'The identity provider configuration appears to be invalid, please contact your administrator.';
                              } else {
                                $scope.model.loginError = 'Login failed. The user name may be incorrect or the account may be locked, disabled or expired.';
                              }                              
                            }
                        }

                        //
                        // If we appear to have Keep Me Signed In Set (KMSI) attempt to log in using the existing cookie.
                        //
                        var haveLocalStorage = spLocalStorage.testLocalStorage();
                        
                        $scope.kmsiHidden = !haveLocalStorage;

                        var tryCookie = haveLocalStorage && spLocalStorage.getItem(TryAuthCookieKey) == 'true';
                        var trySessionCookie = $cookies.get(TryAuthCookieKey) == 'true';


                        if (trySessionCookie || tryCookie) {                                      
                            spLoginService.readiNowLoginWithCookie().then(
                                function () { },     // success
                                function (err) {     // failure
                                    $scope.model.showSignin = true;
                                    if (tryCookie)
                                        spLocalStorage.removeItem(TryAuthCookieKey);

                                    if (trySessionCookie )
                                        $cookies.remove(TryAuthCookieKey);
                                });
                        } else {
                            $scope.model.showSignin = true;
                        }
                    }

                    initialize();
                }
            };
        })
        .directive('spOpenIdBackgroundIcon', function () {
            return {
                restrict: 'A',
                transclude: false,
                scope: false,
                replace: false,
                link: function (scope, element) {
                    element.css('background', '#efefef url(' + scope.model.openIdInputIcon + ') no-repeat scroll 0 50%');
                }
            };
        });

    }());