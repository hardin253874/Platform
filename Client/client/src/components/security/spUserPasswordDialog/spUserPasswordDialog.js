// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module implementing the user password dialog.    
    * 
    * @module spUserPasswordDialog   
    * @example
        
    Using the spUserPasswordDialog:
    
    spUserPasswordDialog.showModalDialog(options).then(function(result){});
    where options is an object with the following properties:
        - accountId - the id of the account
        - changePasswordAtNextLogon - true if the password is being changed after a logon
      
    where result is:
        - true, if the password was changed
        - false, if cancel is clicked        
    */
    angular.module('mod.common.ui.spUserPasswordDialog', [
        'ui.bootstrap',
        'mod.common.spWebService',
        'mod.common.ui.spDialogService',
        'mod.common.ui.spMoveableModal',
        'mod.common.ui.spBusyIndicator'        
    ])
        .controller('spUserPasswordDialogController', function ($scope, $uibModalInstance, $http, options, spWebService) {
            
            // Setup the dialog model
            $scope.model = {
                accountId: options ? options.accountId : -1,
                password: null,
                confirmPassword: null,
                changePasswordAtNextLogon: options && options.changePasswordAtNextLogon,
                errors: [],
                busyIndicator: {
                    type: 'spinner',
                    text: 'Please wait...',
                    placement: 'element',
                    isBusy: false
                }
            };


            // Cancel click handler
            $scope.cancel = function () {
                if ($scope.model.changePasswordAtNextLogon) {
                    return;
                }

                $scope.model.busyIndicator.isBusy = false;
                $uibModalInstance.close(false);
            };


            // Clear any errors
            $scope.clearErrors = function () {
                $scope.model.errors = [];
            };                        


            // Ok click handler
            $scope.ok = function () {
                $scope.clearErrors();

                // Validate the settings
                validateAndSave();                
            };


            function validateAndSave() {
                var url;
                var data;

                if (!$scope.model.password &&
                    !$scope.model.confirmPassword) {
                    addError("A value for the password must be specified.");
                    return;
                }

                if ($scope.model.password !== $scope.model.confirmPassword) {
                    addError("The passwords do not match.");
                    return;
                }

                url = spWebService.getWebApiRoot() + '/spapi/data/v1/password';
                data = {
                    password: $scope.model.password
                };

                $scope.model.busyIndicator.isBusy = true;

                return $http({
                    method: 'POST',
                    url: url,
                    data: data,
                    headers: spWebService.getHeaders()
                }).then(function () {                    
                    $uibModalInstance.close(true);
                }, function (error) {
                    addError(error.data.ExceptionMessage || error.data.Message);
                }).finally(function () {
                    $scope.model.busyIndicator.isBusy = false;
                });
            }

            // Add an error
            function addError(errorMsg) {
                $scope.model.errors.push({ type: 'error', msg: errorMsg });
            }
        })
        .factory('spUserPasswordDialog', function (spDialogService) {
            // setup the dialog
            var exports = {
                showModalDialog: function (options, defaultOverrides) {                    
                    // Disable escape and background click if changePasswordAtNextLogon
                    var changePasswordAtNextLogon = options && options.changePasswordAtNextLogon,
                        dialogDefaults = {
                            windowClass: 'spUserPasswordDialog',
                            backdrop: changePasswordAtNextLogon ? 'static' : true,
                            keyboard: !changePasswordAtNextLogon,
                            templateUrl: 'security/spUserPasswordDialog/spUserPasswordDialog.tpl.html',
                            controller: 'spUserPasswordDialogController',
                            resolve: {
                                options: function () {
                                    return options;
                                }
                            }
                        };

                    if (defaultOverrides) {
                        angular.extend(dialogDefaults, defaultOverrides);
                    }

                    return spDialogService.showModalDialog(dialogDefaults);
                }
            };

            return exports;
        });
}());