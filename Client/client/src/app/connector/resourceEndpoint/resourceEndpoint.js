// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, sp */
(function () {
    'use strict';

    angular.module('mod.app.connector.resourceEndpoint', [
        'ui.router',
        'sp.navService',
        'mod.common.ui.spContextMenu',
        'mod.common.spEntityService',
        'sp.app.settings',
        'mod.app.connector.spResourceEndpointService'
    ]);

    angular.module('mod.app.connector.resourceEndpoint')
        .config(resourceEndpointViewStateConfiguration)
        .controller('resourceEndpointController', ResourceEndpointController);

    /* @ngInject */
    function resourceEndpointViewStateConfiguration($stateProvider) {

        var data = {
            showBreadcrumb: false
        };
        $stateProvider.state('resourceEndpointNew', {
            url: '/{tenant}/{eid}/resourceEndpointNew?path',
            templateUrl: 'connector/resourceEndpoint/resourceEndpoint.tpl.html',
            data: data
        });
        $stateProvider.state('resourceEndpoint', {
            url: '/{tenant}/{eid}/resourceEndpoint?path',
            templateUrl: 'connector/resourceEndpoint/resourceEndpoint.tpl.html',
            data: data
        });
    }

    /* @ngInject */
    function ResourceEndpointController($scope, $stateParams, spNavService, spEntityService, spAppSettings, spResourceEndpointService, spState) {

        var viewModeToolbarButtons, editModeToolbarButtons;

        $scope.model = spResourceEndpointService.createEmptyModel();
        $scope.toolbar = [];
        $scope.svc = spResourceEndpointService;

        function init() {

            // Get basic settings
            var endpointId = spState.name === 'resourceEndpointNew' ? 0 : sp.coerseToNumberOrLeaveAlone(spState.params.eid) || 0;
            var devMode = spAppSettings.initialSettings.devMode;

            // Get API id
            var parentItem = spNavService.getParentItem();
            var parentApiId = sp.result(parentItem, 'id');

            // Create model
            spResourceEndpointService.createModel(endpointId, parentApiId, devMode).then(function (model) {
                $scope.model = model;
            });
        }

        $scope.$watch('model.objectPickerOptions.selectedEntities[0].idP', function (typeId) {
            spResourceEndpointService.typeChanged($scope.model, typeId);
        });

        $scope.$watch('model.displayMode', function () {
            $scope.toolbar = $scope.model.displayMode === 'edit' ? editModeToolbarButtons : viewModeToolbarButtons;
        });

        // Cancel any changes
        $scope.onCancelClick = function () {
            spNavService.navigateToParent();
        };

        // Save any changes
        $scope.onSaveClick = function () {
            $scope.model.endpoint.endpointResourceMapping.mappedType = $scope.model.objectPickerOptions.selectedEntities[0].idP;

            spResourceEndpointService.save($scope.model.endpoint).then(function () {
                spNavService.navigateToParent();
            });            
        };


        // Switch to edit mode
        $scope.onEditClick = function () {
            $scope.displayMode = 'edit';
        };

        viewModeToolbarButtons = [
            {
                text: 'Edit',
                icon: 'assets/images/toolbar_edit.png',
                click: $scope.onEditClick
            }
        ];

        editModeToolbarButtons = [
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

        init();
    }

}());