// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global	angular*/

// This service is responsible for server communications when uploading files.

(function () {
    'use strict';

    angular
        .module('mod.app.importFile.services.spImportFileWebService', [
            'ng',
            'mod.common.spEntityService',
            'mod.common.spWebService'
        ])
        .service('spImportFileWebService', ['$http', '$q', 'spWebService', 'spEntityService', ImportFileWebService]);

    function ImportFileWebService($http, $q, spWebService, spEntityService) {

        function webApiRoot() {
            return spWebService.getWebApiRoot();
        }

        function returnData(response) {
            return response.status === 200 ? response.data : null;
        }

        function returnMessage(response) {
            return $q.reject(response.statusText);
        }

        function loadImportConfig(importConfigId) {
            var rq = 'let @TYPE = { name, isOfType.alias } ' +
                     'let @MAPPEDREL = { fromType.id, toType.id } ' +
                     'let @MEMBERMAPPING = { name, isOfType.alias, mappedField.id, mappedRelationship.@MAPPEDREL, mapRelationshipInReverse, mappedRelationshipLookupField.name } ' +
                     'let @RESOURCEMAPPING = { importMergeExisting, importHeadingRow, importDataRow, importLastDataRow, mappingSourceReference, mappingSuppressWorkflows, mappedType.@TYPE, resourceMemberMappings.@MEMBERMAPPING } ' +
                     'let @IMPORTCONFIG = { name, importFileType.alias, importConfigMapping.@RESOURCEMAPPING } ' +
                     '@IMPORTCONFIG';

            return spEntityService.getEntity(importConfigId, rq, { hint: 'importConfig' }).then(_.identity,
                function () {
                    return $q.reject('Import configuration could not be loaded.');
                });
        }

        // Get spreadsheet info
        // Returns: instance of WebApi.Controllers.ImportSpreadsheet.SpreadsheetInfo
        // sheetName - optional name of sheet to first load sample data for
        function getSpreadsheetInfo(fileUploadId, importFileFormat, fileName, sheetId) {
            if (!fileUploadId) {
                return $q.reject('no file Upload id');
            }
            if (!importFileFormat) {
                return $q.reject('file format is not specified');
            }
            if (!fileName) {
                return $q.reject('filename is not specified');
            }

            var url = webApiRoot() + '/spapi/data/v2/importSpreadsheet/sheet';
            var params = {
                fileId: fileUploadId,
                fileFormat: importFileFormat,
                fileName: fileName,
                sheet: sheetId
            };
            url += '?' + $.param(params);
            return $http({
                method: 'GET',
                url: url,
                headers: spWebService.getHeaders()
            }).then(returnData, returnMessage);
        }

        // Get preview data of the spreadsheet sheet.
        function getSampleTable(fileUploadId, headerRow, dataRow, lastRow, fileFormat, sheet) {
            if (!fileUploadId) {
                return $q.reject('no file Upload id');
            }
            if (!fileFormat) {
                return $q.reject('file format is not specified');
            }
            var url = webApiRoot() + '/spapi/data/v2/importSpreadsheet/sample';
            var params = {
                hrow: headerRow,
                drow: dataRow,
                last: lastRow,
                fileId: fileUploadId,
                fileFormat: fileFormat,
                sheet: sheet
            };
            url += '?' + $.param(params);

            return $http({
                method: 'GET',
                url: url,
                headers: spWebService.getHeaders()
            }).then(returnData, returnMessage);
        }

        // Import the spreadsheet data.
        function importData(importConfigId, fileUploadId, fileUploadName, testRun) {
            if (!importConfigId) {
                return $q.reject('No file config id');
            }
            if (!fileUploadId) {
                return $q.reject('No file upload token');
            }

            var params = {
                config: importConfigId,
                file: fileUploadId,
                filename: fileUploadName
            };
            if (testRun)
                params.testrun = true;
            
            var url = '/spapi/data/v2/importSpreadsheet/import?' + $.param(params);

            return $http({
                method: 'GET',
                url: webApiRoot() + url,
                headers: spWebService.getHeaders()
            }).then(returnData, returnMessage);
        }

        // Get import status
        function getImportStatus(importTaskId) {

            if (!importTaskId) {
                return $q.reject('Import taskId is not provided');
            }
            return $http({
                method: 'GET',
                url: webApiRoot() + '/spapi/data/v2/importSpreadsheet/import/' + importTaskId,
                headers: spWebService.getHeaders()
            }).then(returnData, returnMessage);
        }

        // Cancel the import operation.
        function cancelImport(importTaskId) {

            if (!importTaskId) {
                return $q.reject('Import taskId is not provided');
            }
            return $http({
                method: 'GET',
                url: webApiRoot() + '/spapi/data/v2/importSpreadsheet/cancel/' + importTaskId,
                headers: spWebService.getHeaders()
            }).then(returnData, returnMessage);
        }

        // Import the spreadsheet data.
        function getTestFileId() {
            return $http({
                method: 'GET',
                url: webApiRoot() + '/spapi/data/v2/test/importSpreadsheet/testFile',
                headers: spWebService.getHeaders()
            }).then(returnData, returnMessage);
        }

        var exports = {
            loadImportConfig: loadImportConfig,
            getSpreadsheetInfo: getSpreadsheetInfo,
            getSampleTable: getSampleTable,
            importData: importData,
            getImportStatus: getImportStatus,
            cancelImport: cancelImport,
            getTestFileId: getTestFileId
        };
        return exports;
    }

}());
