// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global	angular*/

// This service is responsible for client-side business logic of the export file configuration wizard.
// It is explicitly not responsible for calling various web services.

(function () {
    'use strict';

    angular
        .module('mod.app.spExportXml', [
            'mod.common.spXsrf',
            'mod.common.spWebService',
            'mod.common.spEntityService'
        ])
        .service('spExportXml', ['$q', '$http', 'spXsrf', 'spWebService', 'spEntityService', ExportXmlService]);

    function ExportXmlService($q, $http, spXsrf, spWebService, spEntityService) {

        function exportEntities(ids) {
            var idList = _.map(ids, spEntityService.getEidUrl).join('+');
            var uri = spWebService.getWebApiRoot() + '/spapi/data/v2/exportXml?ids=' + idList;
            var turi = spXsrf.addXsrfTokenAsQueryString(uri);
            var win = window.open(turi, '_blank');
            win.focus();
        }

        var exports = {
            exportEntities: exportEntities
        };
        return exports;
    }

}());
