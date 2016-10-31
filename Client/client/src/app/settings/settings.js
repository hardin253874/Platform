// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _ */
(function () {
    'use strict';

    angular.module('app.settings', ['ui.router', 'titleService', 'mod.common.spWebService'])
        .config(function($stateProvider){
            $stateProvider.state('settings', {
                url: '/{tenant}//settings?path&server&auth',
                templateUrl: 'settings/settings.tpl.html'
            });
        })
        .controller('SettingsController', function SettingsController($scope, $state, $stateParams, spAppSettings, titleService, spWebService) {
            
            titleService.setTitle('Settings');

            // If settings are included in the URL then set save them, otherwise leave alone
//            if (!_.isNull($stateParams.server) && !_.isUndefined($stateParams.server)) {
//                console.log('settings: setting server', $stateParams.server);
//                spAppSettings.server = $stateParams.server;
//            } else {
//                console.log('settings: server is ', $scope.settings.server);
//            }

            $scope.data = {
                server: $stateParams.server,
                tenant: $stateParams.tenant,
                auth: $stateParams.auth
            };

            $scope.$watch('data.server', function (server) {
                if (!_.isNull(server) && !_.isUndefined(server)) {
                    console.log('settings: setting server', server);
                    spAppSettings.setServer(server);
                }
            });

            $scope.$watch('data.auth', function (auth) {
                if (!_.isNull(auth) && !_.isUndefined(auth)) {
                    spWebService.setHeader('Authorization', auth);
                }
            });
        });
}());
