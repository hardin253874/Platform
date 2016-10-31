// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global	angular,	console	*/
(function() {
    'use	strict';

    angular.module('mod.app.formBuilder', [
        'mod.app.formBuilder.controllers',
        'mod.app.formBuilder.directives',
        'mod.app.formBuilder.factories',
        'mod.app.formBuilder.providers',
        'mod.app.formBuilder.services'
    ])
        .config(function($stateProvider) {

            /////
            // Set the state provider.
            /////
            $stateProvider.state('formBuilder', {
                url: '/{tenant}/{eid}/formBuilder?definitionId&mode&path',
                templateUrl: 'formBuilder/views/formBuilder.tpl.html',
                data: {
                    leftPanelTemplate: 'formBuilder/views/toolbox.tpl.html',
                    leftPanelTitle: 'Form Builder',
                    hideAppTabs: true,
                    hideAppToolboxButton: true
                }
            });


            $stateProvider.state('screenBuilder', {
                url: '/{tenant}/{eid}/screenBuilder?mode&path',
                templateUrl: 'formBuilder/views/formBuilder.tpl.html',
                data: {
                    leftPanelTemplate: 'formBuilder/views/screenBuilderToolbox.tpl.html',
                    leftPanelTitle: 'Screen Builder',
                    hideAppTabs: true,
                    hideAppToolboxButton: true
                }
            });
        });
}());