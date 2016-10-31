// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    angular.module('app.tabsTest', ['mod.common.ui.spTabs'])
        .controller('tabsTestController', ['$scope', function ($scope) {
            
            
            
            $scope.tabs = [
                    { name: "Dynamic Title 1", url: "devViews/utcDate/utcDate.tpl.html" },
                    { name: "Dynamic Title 2", url: "editForm/partials/shared/tabControl.tpl.html" },
                    { name: "Edit Form Controls", url: "devViews/editFormControls/editFormControlsTest.tpl.html" }
            ];

        }]);

}());