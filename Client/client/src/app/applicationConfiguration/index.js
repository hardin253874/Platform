// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    //
    // applicationConfiguration
    //
    // The application configuration page.
    //
    // @module mod.app.applicationConfiguration
    // @example
    //

    angular.module('mod.app.applicationConfiguration', [
        'mod.app.applicationConfiguration.directives',
        'mod.app.applicationConfiguration.services',
        'ui.router'
    ])
    .config(function ($stateProvider) {
        $stateProvider.state('appConfiguration', {
            url: '/{tenant}/{eid}/appConfiguration?path',
            templateUrl: 'applicationConfiguration/templates/applicationConfiguration.tpl.html'
        });
    });
}());