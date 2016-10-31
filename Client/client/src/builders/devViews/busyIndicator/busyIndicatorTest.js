// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.busyIndicatorTest', ['mod.common.ui.spBusyIndicator'])
        .controller('busyIndicatorTestController', ['$scope', function ($scope) {            
            $scope.progressIndicator = {                
                type: 'progressBar',
                text: 'Loading...',
                percent: 100
            };

            $scope.spinnerIndicatorBottom = {                
                type: 'spinner',
                text: 'Loading...',
                textPlacement: 'bottom'
            };

            $scope.indicatorElementpopup = {
                type: 'progressBar',
                text: 'Loading...',
                placement: 'element',
                percent: 100
            };

            $scope.indicatorWindowpopup = {
                type: 'spinner',
                text: 'Loading...',
                placement: 'window'
            };

            $scope.showWindowPopup = function () {
                $scope.indicatorWindowpopup.isBusy = true;

                _.delay(function () {
                    $scope.$apply(function () {
                        $scope.indicatorWindowpopup.isBusy = false;
                    });
                }, 1000);
            };
        }]);
}());