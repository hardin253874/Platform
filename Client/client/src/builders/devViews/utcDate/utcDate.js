// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, jstz */
(function () {
    'use strict';

    angular.module('app.utcDate', ['ui.router', 'titleService', 'mod.common.spEntityService', 'spApps.utcDummyService'])
        .config(function ($stateProvider) {
            $stateProvider.state('utcDate', {
                url: '/{tenant}/{eid}/utcDate?path',
                templateUrl: 'devViews/utcDate/utcDate.tpl.html',
                controller: 'UtcDateController'
            });

            // register a nav item for this page (most fields are defaulted, but you can provide your own viewType, iconUrl, href and order)
            window.testNavItems = window.testNavItems || {};
            window.testNavItems.utcDate = { name: 'UTC Date Test' };
        })
        
        .controller('UtcDateController', function UtcDateController($scope, $stateParams, titleService, spEntityService, utcDummyService) {
            
            titleService.setTitle('UTC Date Test Page');
            $scope.model = {};
            $scope.model.name = 'some text here...';

            $scope.model.message = '';
            $scope.model.id = 1;
            
            $scope.model.dateTime = new Date().toDateString();
            
            var timezone = jstz.determine();
            $scope.model.tzTime = timezone.name();

            $scope.model.tzTimeJson = timezone.toString();

           
            $scope.submit = function () {
                $scope.model.message = 'Submitted...';
                utcDummyService.getSingle($scope.model.id, $scope.model.tzTime, function productresult(response) {

                    window.alert(response.data);
                    
                    $scope.model.message = '';
                });

            };

            $scope.getMsTzFromHeader = function () {
                utcDummyService.getMsTzFromHeader().then(function (response) {
                    window.alert(response.data);
                });
                
            };
            
            $scope.getMsTzFromUrl = function () {
                utcDummyService.getMsTzFromUrl($scope.model.tzTime).then(function (response) {
                    window.alert(response.data);
                });

            };
           
        });

}());
