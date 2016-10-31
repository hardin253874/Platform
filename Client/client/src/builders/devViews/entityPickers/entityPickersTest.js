// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.entityPickersTest', ['mod.common.ui.spEntityRadioPicker', 'mod.common.ui.spEntityComboPicker', 'mod.common.ui.spEntityCheckBoxPicker', 'mod.common.ui.spEntityMultiComboPicker'])
        .controller('entityRadioPickerTestController', ['$scope', function ($scope) {
            $scope.radioPickerOptions = {
                selectedEntityId: 0,
                selectedEntity: null,
                entityTypeId: 'console:thumbnailSizeEnum'
            };
        }])
        .controller('entityComboPickerTestController', ['$scope', function ($scope) {
            $scope.comboPickerOptions = {
                selectedEntityId: 0,
                selectedEntity: null,
                entityTypeId: 'core:cardinalityEnum'
            };
        }])
        .controller('entityMultiComboPickerTestController', ['$scope', function ($scope) {
            $scope.multiComboPickerOptions = {
                selectedEntityIds: [],
                selectedEntities: [],
                entityTypeId: 'core:person'
            };
        }])
        .controller('entityCheckBoxPickerTestController', ['$scope', function ($scope) {
            $scope.checkBoxPickerOptions = {
                selectedEntityIds: [],
                selectedEntities: [],
                entityTypeId: 'console:thumbnailSizeEnum'
            };
        }]);
}());