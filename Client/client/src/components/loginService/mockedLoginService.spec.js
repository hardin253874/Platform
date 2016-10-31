// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('mockedLoginService', [])
    .factory('spLoginService', function ($rootScope, $location) {

        var identity = null,
            testIdentity = {
                tenant: 'EDC',
                username: 'TEST-ADMIN',
                provider: 'basicAuth'
            };

        var exports = {
            getDefaultIdentity: function (provider) {
                return {};
            },
            getAuthenticatedIdentity: function () {
                return identity;
            },
            saveRedirect: function (toState, toParams) {
            },
            getPendingTenant: function () {
                return testIdentity.tenant;
            },
            clearRedirect: function () {
            },
            notifyLoggedIn: function () {
                $rootScope.$broadcast('signedin', this.getAuthenticatedIdentity());
            },
            redirectAfterLogin: function () {
            },
            getLoginReturnUrl: function (tenant) {
                return $location.absUrl().split('#')[0] + '#/' + tenant + '//loginRedirect';
            },
            logout: function () {
                $rootScope.$broadcast('signedout');
            },
            beginChallenge: function () {
            },
            endChallenge: function () {
            },

            // test support routines
            setLoggedIn: function () {
                identity = testIdentity;
            },
            setLoggedOut: function () {
                identity = null;
            },
            isSignedIn: function() {
                return !!this.getAuthenticatedIdentity();
            },
            signedInTenant: function() {
                return sp.result(this.getAuthenticatedIdentity(), 'tenant');
            },
            isSignedInAsTenant: function(tenant) {
                return tenant && this.signedInTenant() && tenant.toLowerCase() === this.signedInTenant().toLowerCase();
            }
        };

        return exports;
    });

}());
