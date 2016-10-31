// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.refreshButtonTest', ['mod.common.ui.spRefreshButton'])
        .controller('refreshButtonTestController', function ($scope, refreshMenuItemConfig) {
            $scope.refreshMenuItemConfig = _.cloneDeep(refreshMenuItemConfig);            
            $scope.counter = 0;
            $scope.options = {
                refreshCallback: function () {
                    $scope.counter++;
                    $scope.time = Date.now();
                },
                autoRefreshTimeoutMin: 0
            };

            $scope.applyConfigChanges = function () {
                _.forEach($scope.refreshMenuItemConfig.menuItemConfig, function (mi, index) {
                    refreshMenuItemConfig.menuItemConfig[index].timeoutMin = mi.timeoutMin;
                    refreshMenuItemConfig.menuItemConfig[index].menuItemText = mi.menuItemText;
                    refreshMenuItemConfig.menuItemConfig[index].buttonText = mi.buttonText;                                            
                });                
            };
        });
}());