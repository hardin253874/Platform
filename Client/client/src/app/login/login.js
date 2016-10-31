// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, base64 */
(function() {
    'use strict';

    angular.module('app.login', ['ui.router', 'titleService', 'mod.app.login.directive', 'mod.common.spMobile'])
        .controller('LoginController', function LoginController($rootScope, $scope, $stateParams, $timeout, titleService, spMobileContext) {

            titleService.setTitle('Login');

            $scope.error = {};
            $scope.spMobileContext = spMobileContext;            
        });
       
}());