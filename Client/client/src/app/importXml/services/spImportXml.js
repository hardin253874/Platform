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
                result: '',
                uploadSession: uploadSession,
                fileFilter: '.xml',
                fileName: '',
                fileUploadId: '',
                message: '',
                busyIndicator: {
                    type: 'progressBar',
                    text: 'Importing...',
                    placement: 'window',
                    isBusy: false,
                    percent: 0
                },
                onFileUploadComplete: function(fileName, fileUploadId) {
                    return onFileUploadComplete(model, fileName, fileUploadId); // promise
                },
                documentMessage: '',
                ignoreDependencies: function() {
                    return ignoreDependencies(model);
                },
                showIgnoreDeps: false
            };
            return model;
        }

        function onFileUploadComplete(model, fileName, fileUploadId) {
            model.fileName = fileName;
            model.fileUploadId = fileUploadId;
            return runImport(model, false);
        }

        function runImport(model, ignoreDeps) {
            model.result = '';
            return callImportXml(model.fileName, model.fileUploadId, ignoreDeps).then(
                function (data) { return handleSuccess(model, data); },
                function (error) { return handleError(model, error); });
        }

        function callImportXml(fileName, fileUploadId, ignoreDeps) {
            var url = spWebService.getWebApiRoot() + '/spapi/data/v2/importXml';

            var params = {
                fileId: fileUploadId,
                fileName: fileName
            };
            if (ignoreDeps)
                params.ignoreDeps = true;
            url += '?' + $.param(params);
            return $http({
                method: 'GET',
                url: url,
                headers: spWebService.getHeaders()
            }).then(returnData, returnMessage);
        }

        function ignoreDependencies(model) {
            return runImport(model, true);
        }

        function returnData(response) {
            return response.status === 200 ? response.data : null;
        }

        function returnMessage(response) {
            return $q.reject(response.statusText);
        }

        function handleSuccess(model, data) {
            model.showIgnoreDeps = false;
            var entity = sp.result(data, 'entities.0');
            if (data.message) {
                handleError(model, data.message);
                return;
            }
            if (!entity) {
                handleError(model, 'Failed to get result details.');
                return;
            }
            var msg = entity.typeName + ' imported: ' + entity.name;
            model.result = 'success';
            model.message = msg;
            //spAlertsService.addAlert(msg, { expires: 3, severity: spAlertsService.sev.Success });
            spNavService.refreshTree(false);
            return data;
        }

        function handleError(model, errorMessage) {
            var msg = 'A problem occurred: ' + errorMessage;
            model.result = 'error';
            model.message = msg;
            model.showIgnoreDeps = errorMessage.includes('dependencies');
            //spAlertsService.addAlert(msg, { expires: false, severity: spAlertsService.sev.Error });
            return errorMessage;
        }

        var exports = {
            createModel: createModel
        };
        return exports; 
    }

}());
