// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function() {
    'use strict';

    /**
    * Module implementing the security customise UI view.
    *
    * @module securityCustomiseUI
    */
    angular.module('app.securityCustomiseUI', ['app.security.directives.spSecurityAllowDisplayControl','sp.navService','mod.common.alerts'])
        .config(function ($stateProvider) {
            $stateProvider.state('securityCustomiseUI', {
                url: '/{tenant}/{eid}/securityCustomiseUI?path',
                templateUrl: 'security/securityCustomiseUI.tpl.html'
            });
        })
        .controller('securityCustomiseUIController', function ($scope, spNavService, spAlertsService, $state) {

            function raiseActionEvent(action) {
                $scope.$broadcast('allowDisplayAction', { action: action });
            }

            // Remove any alerts that were generated by this page.
            function clearPageAlerts() {
                spAlertsService.removeAlertsWhere({ page: $state.current });
            }
            
            // The model
            $scope.model = {
                allowDisplayOptions: {
                    mode: 'view'
                },
                toolbar: {}
            };            
            
            // Cancel any changes
            $scope.onCancelClick = function () {
                spNavService.navigateInternal().then(function () {
                    clearPageAlerts();
                    raiseActionEvent('cancel');
                });                
            };
            
            // Save any changes
            $scope.onSaveClick = function() {
                raiseActionEvent('save');
            };
            
            // Switch to edit mode
            $scope.onEditClick = function() {
                raiseActionEvent('edit');
            };

            var viewModeToolbarButtons = [
                {
                    text: 'Edit',
                    icon: 'assets/images/toolbar_edit.png',
                    click: $scope.onEditClick
                }
            ];

            var editModeToolbarButtons = [
                {
                    text: 'Save',
                    icon: 'assets/images/toolbar_save.png',
                    click: $scope.onSaveClick
                },
                {
                    text: 'Cancel',
                    icon: 'assets/images/toolbar_close.png',
                    click: $scope.onCancelClick
                }
            ];
            
            // Update the toolbar buttons when the mode changes
            $scope.$watch('model.allowDisplayOptions.mode', function() {
                $scope.model.toolbar.buttons = $scope.model.allowDisplayOptions.mode === 'view' ? viewModeToolbarButtons : editModeToolbarButtons;
            });
        });
}());