// Copyright 2011-2016 Global Software Innovation Pty Ltd

angular.module('mod.app.exportServices', ['mod.common.spWebService', 'mod.common.spXsrf', 'mod.common.spNgUtils', 'mod.common.spFileDownloadService']);

/**
*  @ngdoc service
*  @module spExportService
*  @description This service is used by any Page that needs to export report data.
*/

angular.module('mod.app.exportServices').factory('spExportService', function ($http, $q, $timeout, spWebService, spXsrf, spNgUtils, spFileDownloadService) {
    'use strict';
    var exports = {};

    function webApiRoot() {
        return spWebService.getWebApiRoot();
    }

    /**
   * Export Report data for given report id. 
   * @param {string} format
   * @param {string} reportId
   * @param {object} reportParameter
   * @returns A promise
   *    
   */
    function exportData(format, reportId, reportParameter) {
        if (!reportParameter) {
            return $q.reject('Export information is not provided');
        }
        var entityTypeId = null;
        var relationDetailString = null;
        var postData = {};

        if (reportParameter.entityTypeId) {
            entityTypeId = reportParameter.entityTypeId;
        }

        if (angular.isDefined(reportParameter.relationDetail) &&
              angular.isDefined(reportParameter.relationDetail.eid) &&
              angular.isDefined(reportParameter.relationDetail.relid) &&
              angular.isDefined(reportParameter.relationDetail.direction)) {
            relationDetailString = reportParameter.relationDetail.eid + ',' + reportParameter.relationDetail.relid + ',' + reportParameter.relationDetail.direction;
        }

        if (reportParameter.sort) {
            postData.sort = reportParameter.sort;
        }

        if (reportParameter.conds) {
            postData.conds = reportParameter.conds;
        }

        if (reportParameter.qsearch) {
            postData.qsearch = reportParameter.qsearch;
        }
        return exports.exportReportData(format, reportId, {
            type: entityTypeId,
            relationship: relationDetailString
        }, postData);
    }
    exports.exportData = exportData;

    /**
    * Make a web service call to export report data for given report id. 
    * @param {string} format
    * @param {string} reportId
    * @param {object} params
    * @param {object} postData
    * @returns A promise
    *    
    */
    function exportReportData(format, reportId, params, postData) {
        return $http({
            method: 'POST',
            url: webApiRoot() + '/spapi/data/v2/report/export/' + reportId + '/' + format,
            data: JSON.stringify(postData),
            params: params,
            headers: spWebService.getHeaders()
        }).then(function (response) {
            return response.status === 200 ? response.data : null;
        });
    }
    exports.exportReportData = exportReportData;

    /**
      * Downloads the exported document for a given filehash. 
      * @param {string} fileToken
      * @param {string} fileName
      * @param {string} fileFormat  
      *    
      */
    exports.getExportedDocument = function (fileToken, fileName, fileFormat) {
        if (!fileToken) {
            $q.reject('File token is not provided.');
        }
        var url = webApiRoot() + '/spapi/data/v2/report/export/download/' + fileToken + '/' + encodeURIComponent(spNgUtils.sanitizeUriComponent(fileName)) + '/' + fileFormat;
        spFileDownloadService.downloadFile(spXsrf.addXsrfTokenAsQueryString(url));
    };
    return exports;
});