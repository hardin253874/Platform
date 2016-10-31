// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function() {
    'use strict';

    /**
    * Service calls relating to the report template view.       
    * 
    * @module reportTemplateService    
    */
    angular.module('mod.app.reportTemplateService', ['ng', 'mod.common.alerts', 'mod.common.spWebService', 'mod.common.spEntityService', 'sp.navService', 'mod.common.spXsrf', 'mod.common.spFileDownloadService'])
    .factory('rptTemplateService', function ($http, $q, $window, $parse, $timeout, spWebService, spAlertsService, spXsrf, spFileDownloadService) {
        var exports = {};

        exports.generateDocument = function (templateId, selectedIds) {
            var url = spWebService.getWebApiRoot() + '/spapi/data/v1/reportTemplate?template=' + templateId;

            if (selectedIds && selectedIds.length > 0) {
                url += '&resource=' + selectedIds[0];
            }
            
            return $http({
                method: 'GET',
                url: url,
                headers: spWebService.getHeaders()
            }).then(function (response) {
                if (angular.isDefined(response) && angular.isDefined(response.data)) {
                    // response.data is token with quotes, such as "cac344ea97dc411e9981c43917e0fd03"
                    return response.data ? response.data.replace(/"/g, "") : null;
                }
                return null;
            });
        };

        exports.getLongRunningProgress = function (token) {
            var url = spWebService.getWebApiRoot() + '/spapi/data/v1/longRunning/' + token;
            
            return $http({
                method: 'GET',
                url: url,
                headers: spWebService.getHeaders()
            }).then(function (response) {
                if (angular.isDefined(response) && angular.isDefined(response.data)) {
                    return response.data;
                }
                return null;
            });
        };
        
        exports.getGeneratedDocument = function (docId) {
            return $timeout(function() {                
                spFileDownloadService.downloadFile(spXsrf.addXsrfTokenAsQueryString(spWebService.getWebApiRoot() + '/spapi/data/v2/file/' + docId));
            }, 50);
        };
        
        return exports;
    });
}());