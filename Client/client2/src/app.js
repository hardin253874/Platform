import angular from 'angular';
require("bootstrap-loader");

angular.module('app', [
        require('angular-ui-router'),
        require('./home').default
    ]
)
    .config(function routing($urlRouterProvider, $locationProvider) {
        $locationProvider.html5Mode({
            enabled: true,
            requireBase: false
        });
        $urlRouterProvider.otherwise('/');
    });
