// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /**
    * Module implementing the hero text properties dialog.
    * 
    * @module spHeroTextProperties
    * @example
    var model = { control: heroTextControlEntity }
    spHeroTextProperties.showDialog(model).then(function(result) {
    });
       
    */
    angular.module('mod.app.heroText.spHeroTextProperties', [
            'ui.bootstrap',
            'mod.common.ui.spDialogService',
            'mod.common.ui.spReportPicker',
            'mod.common.spHeroTextBuilderService'
        ])
        .controller("spHeroTextPropertiesController", ['$scope', '$uibModalInstance', 'spHeroTextBuilderService', 'heroTextControl', spHeroTextPropertiesController])
        .factory('spHeroTextProperties', ['spDialogService', spHeroTextPropertiesFactory]);

    //
    // spHeroTextPropertiesController
    //
    function spHeroTextPropertiesController($scope, $uibModalInstance, spHeroTextBuilderService, heroTextControl) {

        // model defines .heroTextControl, .series and .axis
        var model = spHeroTextBuilderService.createModel(heroTextControl);

        $scope.model = model;
        $scope.svc = spHeroTextBuilderService;

        // Handle OK
        $scope.ok = function () {
            if (spHeroTextBuilderService.okEnabled(model)) {
                spHeroTextBuilderService.applyChanges(model);
                heroTextControl.refreshTrigger = {}; // or some other randomly changing value
                $uibModalInstance.close(true);
            }
        };

        // Handle Cancel
        $scope.cancel = function() {
            $uibModalInstance.close(false);
        };

        // okDisabled
        $scope.okDisabled = function () {
            return !spHeroTextBuilderService.okEnabled(model);
        };

        $scope.$watch("model.reportPickerOptions.selectedEntities", function () {
            spHeroTextBuilderService.reportChanged(model);
        });

        spHeroTextBuilderService.initModel(model);
    }

    // 
    // spHeroTextProperties factory
    //
    function spHeroTextPropertiesFactory(spDialogService) {

        // Show dialog, return promise
        function showDialog(options) {
            var defaults = {
                templateUrl: 'formBuilder/directives/spFormBuilder/directives/spFormBuilderHeroTextControl/spHeroTextProperties.tpl.html',
                controller: 'spHeroTextPropertiesController',
                resolve: {
                    heroTextControl: function () { return options.heroTextControl; }
                }
            };
            return spDialogService.showDialog(defaults);
        }

        return {
            showDialog: showDialog
        };
    }

}());