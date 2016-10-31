// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global	angular*/

// This service is responsible for client-side business logic of the import file configuration wizard.
// It is explicitly not responsible for calling various web services.

(function () {
    'use strict';

    angular
        .module('mod.app.importXml.services.spImportXml', [
            'sp.common.fileUpload',
            'mod.common.spWebService',
            'mod.common.alerts',
            'sp.navService'
        ])
        .service('spImportXml', ['spUploadManager', '$q', '$http', 'spWebService', 'spAlertsService', 'spNavService', ImportXmlService]);

    function ImportXmlService(spUploadManager, $q, $http, spWebService, spAlertsService, spNavService) {

        // Create a new empty model that will contain everything being tracked
        function createModel() {

            var uploadSession = spUploadManager.createUploadSession();
            var model = {
                uploadSession: uploadSession,
                fileFilter: '.xml',
                message: '',
                busyIndicator: {
                    type: 'progressBar',
                    text: 'Importing...',
                    placement: 'window',
                    isBusy: false,
                    percent: 0
                },
                onFileUploadComplete: function (fileName, fileUploadId) {
                    return onFileUploadComplete(model, fileName, fileUploadId); // promise
                },
                documentMessage: ''
            };
            return model;
        }

        function onFileUploadComplete(model, fileName, fileUploadId) {
            return callImportXml(fileName, fileUploadId).then(handleSuccess, handleErrorResponse);
        }

        function callImportXml(fileName, fileUploadId) {
            var url = spWebService.getWebApiRoot() + '/spapi/data/v2/importXml';

            var params = {
                fileId: fileUploadId,
                fileName: fileName
            };
            url += '?' + $.param(params);
            return $http({
                method: 'GET',
                url: url,
                headers: spWebService.getHeaders()
            }).then(returnData, returnMessage);
        }

        function returnData(response) {
            return response.status === 200 ? response.data : null;
        }

        function returnMessage(response) {
            return $q.reject(response.statusText);
        }

        function handleSuccess(data) {
            var entity = sp.result(data, 'entities.0');
            if (!entity) {
                handleErrorResponse('Failed to get result details.');
                return;
            }
            var msg = entity.typeName + ' imported: ' + entity.name;
            spAlertsService.addAlert(msg, { expires: 3, severity: spAlertsService.sev.Success });
            spNavService.refreshTree(false);
            return data;
        }

        function handleErrorResponse(errorMessage) {
            spAlertsService.addAlert('A problem occurred: ' + errorMessage, { expires: false, severity: spAlertsService.sev.Error });
            return errorMessage;
        }

        var exports = {
            createModel: createModel
        };
        return exports; 
    }

}());
