// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, spUtils */

(function () {
    'use strict';

    /**
    * Module implementing a administration page.    
    *
    * It contains a administration page.
    * @module administration            
    */
    angular.module('mod.app.administration', [
        'mod.app.administration.directives',
        'ui.router'
    ])
        .config(function ($stateProvider) {          
            $stateProvider.state('administration', {
                url: '/{tenant}/{eid}/administration?path',
                templateUrl: 'administration/templates/administration.tpl.html'
            });           
        });

}());