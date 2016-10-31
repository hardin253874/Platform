// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /////
    // EditForm controls test controller.
    /////
    angular.module('app.editableLabelTest', [])
        .controller('editableLabelTestController', function($scope) {

            $scope.model = {
                value: 'Click to edit'
            };
        });
}());