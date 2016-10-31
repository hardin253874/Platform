// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.colorPickerTest', ['mod.common.ui.spColorPickers'])
        .controller('colorPickerTestController', ['$scope', function ($scope) {
            $scope.inlineColor = {
                a: 0,
                r: 0,
                g: 0,
                b: 0
            };         
            
            $scope.pickercolor = {
                a: 0,
                r: 0,
                g: 0,
                b: 0
            };


            $scope.popupColorOptions = {
                color: {
                    a: 0,
                    r: 0,
                    g: 0,
                    b: 0
                },
                isOpen: false
            };

            $scope.open = function () {
                $scope.popupColorOptions.isOpen = true;
            };
        }]);
}());