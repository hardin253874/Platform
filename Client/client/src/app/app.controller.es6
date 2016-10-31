// Copyright 2011-2015 Global Software Innovation Pty Ltd
/*global angular, $, console, _, base64, sp, Globalize, FastClick */

(function () {
    'use strict';

    angular.module('app').controller("AppController", AppController);

    function AppController($scope, $rootScope, $state, $stateParams, $location, $window, spAppError,
                           spAlertsService, spLoginService, spLocalStorage, spReauthDialog, spNavService) {

        var currentlyAuthenticating = false;
        
        $scope.$state = $state;
        $scope.$stateParams = $stateParams;

        $scope.spAppError = spAppError;
        $scope.loginService = spLoginService;

        $scope.$on('toggle-fullscreen', () => {
            $state.current.data = $state.current.data || {};
            if ($state.current.data) {
                $state.current.data.fullScreen = !$state.current.data.fullScreen;
                $scope.$root.$broadcast('app.layout');
            }
        });

        // keep the loading screen in place until we are ready
        $scope.hideApplication = true;
        $scope.$watch('spAppError.haveErrors() || appData.isRestarting',  (value) => {
            $scope.hideApplication = value;
        });

        $rootScope.$on('$stateChangeStart', function (event, toState, toParams) {
            if (toState.name === 'landinghome') {
                spLocalStorage.setItem(spNavService.getLastUsedAppKey(), null);
            } else {
                spLocalStorage.setItem(spNavService.getLastUsedAppKey(), spNavService.getCurrentApplicationId());
            }
        });

        //
        // Deal with the tokens timing out and logging out

        function reauthenticate(event, response) {            
            // Ensure that only one dialog opens at a time.
            if (!currentlyAuthenticating && spLoginService.isSignedIn()) {
                console.log('Reauthenticating');

                var identity = spLoginService.getAuthenticatedIdentity();

                currentlyAuthenticating = true;

                if (!spLoginService.isSignedInUsingProvider('core:readiNowIdentityProvider')) {
                    // Logout
                    spLoginService.logout();
                    return;
                }

                spReauthDialog.showModalDialog(identity)
                    .finally(() => {
                        currentlyAuthenticating = false;
                    });
            } else {
                console.log('Skipping reauthenticating, dialog open.');
            }
        }

        //
        // Reauth only if the token has expired. All other errors pass through.
        //
        function reauthenticateIfTokenExpired(event, response) {
            if (response.data && response.data.AdditionalInfo === "TokenExpired") {
                reauthenticate(event, response);
            }
        }

        $rootScope.$on('event:auth-loginRequired', reauthenticate);
        $rootScope.$on('event:auth-forbidden', reauthenticateIfTokenExpired);         // This will force the user to try to log back in, at which point they will see an appropriate message


        if (($location.absUrl().indexOf('https') !== 0)) {
            spAlertsService.addAlert('Warning: not using https', 'warn');
        }

        if (!$window.navigator.cookieEnabled) {
            $scope.spAppError.add('Unable to continue. Your browser must be configured to allow cookies to use this website.');
        }

    }
}());
