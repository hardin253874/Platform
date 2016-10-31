// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use	strict';

    /**
    * Module implementing the index of services used in application manager.
    */
    angular.module('mod.app.applicationManager.services',
        [
            'mod.app.applicationManager.services.spAppManagerService',
            'mod.app.applicationManager.services.spAppManagerPublishService',
            'mod.app.applicationManager.services.spAppManagerCardinalityViolationsService'
        ]);
}());