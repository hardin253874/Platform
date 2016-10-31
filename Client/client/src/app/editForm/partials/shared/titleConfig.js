// Copyright 2011-2016 Global Software Innovation Pty Ltd
angular.module('app.editForm.titleConfigController', ['mod.app.editForm'])
    .controller('titleConfigController',
        
        function($scope) {
            'use strict';

            $scope.title = $scope.formControl.name;

            $scope.$watch('title', function() {
                if ($scope.formControl) {

                    $scope.formControl.setName($scope.title);
                }
            });
        });
        


    

