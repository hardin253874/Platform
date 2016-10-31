// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, spUtils */

(function () {
    'use strict';

    /**
    * Module implementing a adminToolbox.    
    *
    * It contains a adminToolbox.
    * @module adminToolbox            
    */
    angular.module('mod.app.adminToolbox', [
        'mod.app.adminToolbox.directives',
        'ui.router'
    ])
        .config(function ($stateProvider) {

            var data = {
                showBreadcrumb: false,
                leftPanelTemplate: 'formBuilder/views/adminToolbox.tpl.html',
                leftPanelTitle: 'Application Toolbox',
                hideAppTabs: true
            };

            $stateProvider.state('adminToolbox', {
                url: '/{tenant}/{eid}/adminToolbox?path',
                templateUrl: 'adminToolbox/templates/adminToolbox.tpl.html',
                data: data
            });
        });
}());