// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global	angular*/

(function () {
    'use strict';

    angular
        .module('mod.app.importFile', [
            'mod.app.importFile.controllers.importFileWizard'
        ])
        .config(function($stateProvider) {

            $stateProvider.state('importFile', {
                url: '/{tenant}/{eid}/importFile?path',
                templateUrl: 'importFile/controllers/importFileWizard.tpl.html'
            });

            $stateProvider.state('newImportFile', {
                url: '/{tenant}/0/importFile?path',
                templateUrl: 'importFile/controllers/importFileWizard.tpl.html'
            });
        });

}());