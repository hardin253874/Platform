// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing the user reauthentication dialog NOTE: IF DOES NOT CURRENTLY HANDLE OPENID.    
    * 
    * @module spReauthDialog   
    * @example
        
    Using the spReauthDialog:
    
    spReauthDialog.showModalDialog(tenant, username, persistent).then(function(result){});
          
    where result is:
        - true, if authenticated
        - false, if user chose to log out. (Warning the logout may beat the response to the caller.       
    */
    angular.module('mod.common.ui.spReauthDialog', [
        'ui.bootstrap',
        'mod.common.ui.spDialogService',
        'mod.common.ui.spMoveableModal',
        'mod.common.ui.spBusyIndicator',
        'sp.common.rnLoginService',
        'sp.common.clientUpdateService',
        'sp.app.settings',
    ])
        .controller('spReauthDialogController', function ($scope, $uibModalInstance, options, rnLoginService, spLoginService, spClientUpdateService, spAppSettings) {
            
            // Setup the dialog model
            $scope.model = {
                tenant: options.tenant,
                username: options.username,
                persistent: options.persistent,
                password: null,
                errors: [],
                busyIndicator: {
                    type: 'spinner',
                    text: 'Signing in...',
                    placement: 'element',
                    isBusy: false
                }
            };


            // Cancel click handler
            $scope.signout = function () {
                $scope.model.busyIndicator.isBusy = false;
                spLoginService.logout()
                    .then(function () {
                        // don't try to close the dialog as the page is being redirected
                    },
                    function (error) {
                        addError(error.statusText);
                    });
            };


            // Clear any errors
            $scope.clearErrors = function () {
                $scope.model.errors = [];
            };                        


            // Ok click handler
            $scope.ok = function () {
                $scope.clearErrors();

                // Validate the settings
                authenticate();                
            };


            function authenticate() {
                
                $scope.model.busyIndicator.isBusy = true;                

                rnLoginService.login($scope.model.tenant, $scope.model.username, $scope.model.password, $scope.model.persistent).then(function (data) {

                    $uibModalInstance.close(true);

                    // Check min client version
                    spClientUpdateService.checkMinClientVersionAndRefresh(spAppSettings.clientVersion, data.initialSettings.requiredClientVersion);

                }, function (error) {

                    $scope.model.spSigningIn = false;
                                    
                    switch (error.status) {
                        case 401:
                        case 403:
                            addError(error.data.Message);
                            break;

                        default:
                            addError('Unexpected server error: ' + error.status);
                            break;
                    }

                }).finally(function () {
                    $scope.model.busyIndicator.isBusy = false;
                });
            }


            // Add an error
            function addError(errorMsg) {
                $scope.model.errors.push({ type: 'error', msg: errorMsg });
            }

        })
        .factory('spReauthDialog', function (spDialogService) {
            // setup the dialog
            var exports = {
                showModalDialog: function (options) {
                    // Disable escape and background click if changePasswordAtNextLogon
                    var 
                        dialogDefaults = {
                            windowClass: 'spReauthDialog',
                            backdropClass: 'spReauthBackdrop',
                            keyboard: false,
                            backdrop: 'static',
                            templateUrl: 'login/directives/spReauthDialog.tpl.html',
                            controller: 'spReauthDialogController',
                            resolve: {
                                options: function () {
                                    return options;
                                }
                            }
                        };



                    return spDialogService.showModalDialog(dialogDefaults);
                }
            };

            return exports;
        });
}());