// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /**
    * Service calls relating to the application manager view.       
    * 
    * @module mod.app.applicationManager.services.spAppManagerService    
    */
    angular.module('mod.app.applicationManager.services.spAppManagerService', [
        'ng',
        'mod.common.alerts',
        'mod.common.spWebService',
        'mod.common.spEntityService',
        'sp.navService'
    ])
    .factory('spAppManagerService', function ($http, $q, $window, $parse, spWebService, spAlertsService) {
        var exports = {};

        /////
        // Issues the request
        /////
        function issueRequest(method, url, action, suppressAlert) {
            return $http({
                method: method,
                url: url,
                headers: spWebService.getHeaders()
            }).then(function (response) {
                return response && response.data;
            }, function (error) {
                if (!suppressAlert) {
                    var errorText = 'Failed to ' + action + ' application.';

                    if (error.data && error.data.Message) {
                        errorText += ' ' + error.data.Message;
                    }

                    spAlertsService.addAlert(errorText, 'error');
                }

                throw error && error.data;
            });
        }

        /////
        // Issue a GET request
        /////
        function issueGetRequest(url, action, suppressAlert) {
            return issueRequest('GET', url, action, suppressAlert);
        }

        /////
        // Issue a POST request
        /////
        function issuePostRequest(url, action, suppressAlert) {
            return issueRequest('POST', url, action, suppressAlert);
        }

        /////
        // Gets the application info for Application Manager
        /////
        exports.getApplications = function () {
            return issueGetRequest(spWebService.getWebApiRoot() + '/spapi/data/v1/appManager/', 'get');
        };
        
        /////
        // Publish an application.
        /////
        exports.publishApplication = function(aid) {
            return issuePostRequest(spWebService.getWebApiRoot() + '/spapi/data/v1/appManager/publish/' + aid, 'publish', true);
        };

        /////
        // Deploy an application.
        /////
        exports.deployApplication = function(aid) {
            return issuePostRequest(spWebService.getWebApiRoot() + '/spapi/data/v1/appManager/deploy/' + aid, 'deploy');
        };

        /////
        // Stage an application.
        /////
        exports.stageApplication = function (aid) {
            return issuePostRequest(spWebService.getWebApiRoot() + '/spapi/data/v1/appManager/stage/' + aid, 'stage', true);
        };

        /////
        // Upgrade an application.
        /////
        exports.upgradeApplication = function(vid) {
            return issuePostRequest(spWebService.getWebApiRoot() + '/spapi/data/v1/appManager/upgrade/' + vid, 'upgrade');
        };

        /////
        // Repair an application.
        /////
        exports.repairApplication = function (vid) {
            return issuePostRequest(spWebService.getWebApiRoot() + '/spapi/data/v1/appManager/repair/' + vid, 'repair');
        };

        /////
        // Uninstall an application.
        /////
        exports.uninstallApplication = function (vid) {
            return issuePostRequest(spWebService.getWebApiRoot() + '/spapi/data/v1/appManager/uninstall/' + vid, 'uninstall');
        };

        /////
        // Export an application.
        /////
        exports.exportApplication = function(vid) {
            return issueGetRequest(spWebService.getWebApiRoot() + '/spapi/data/v1/appManager/export/' + vid, 'export');
        };
        
        return exports;
    });
}());