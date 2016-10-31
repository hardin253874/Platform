// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';
    
    //
    // applicationManager
    //
    // The application manager page.
    //
    // @module mod.app.applicationManager
    // @example
    //

    angular.module('mod.app.applicationManager', [
        'mod.app.applicationManager.directives',
        'mod.app.applicationManager.services',
        'ui.router'
    ])
    .config(function ($stateProvider) {
        $stateProvider.state('appManager', {
            url: '/{tenant}/{eid}/appManager?path',
            templateUrl: 'applicationManager/templates/applicationManager.tpl.html',
            data: {}
        });
    });
}());