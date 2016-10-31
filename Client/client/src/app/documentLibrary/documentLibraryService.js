// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /**
    * Service calls relating to the application manager view.       
    * 
    * @module documentLibraryService    
    */
    angular.module('mod.app.documentLibraryService', ['ng', 'mod.common.alerts', 'mod.common.spWebService', 'mod.common.spEntityService', 'sp.navService'])
    .factory('docLibraryService', function ($http, $q, $window, $parse, spWebService, spAlertsService) {
        var exports = {};

        // TODO:- Add in functionality for locking/unlocking/download/other stuff
        
        return exports;
    });
}());