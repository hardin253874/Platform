// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    //
    // Tenant Rollback
    //
    // The tenant rollback page.
    //
    // @module mod.app.settings.tenantRollback
    // @example
    //

    angular.module('mod.app.settings.tenantRollback', [
        'mod.app.settings.tenantRollback.directives',
        'mod.app.settings.tenantRollback.services',
        'ui.router'
    ])
    .config(function ($stateProvider) {
        $stateProvider.state('tenantRollback', {
            url: '/{tenant}/{eid}/tenantRollback?path',
            templateUrl: 'settings/tenantRollback/templates/tenantRollback.tpl.html',
            data: {}
        });
    });
}());