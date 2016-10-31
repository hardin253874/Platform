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

    angular.module('mod.app.importXml', [
        'mod.app.importXml.controllers.importXml',
        'mod.app.importXml.services.spImportXml',
        'ui.router'
    ])
    .config(function ($stateProvider) {
        $stateProvider.state('importXml', {
            url: '/{tenant}/{eid}/importXml?path',
            templateUrl: 'importXml/controllers/importXml.tpl.html'
        });
    });
}());