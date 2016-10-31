// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, sp*/

// Main page for import resource XML files

(function () {
    'use strict';

    angular.module('mod.app.importXml.controllers.importXml', [
        'mod.app.importXml.services.spImportXml'
    ])
        .controller('importXmlController', ImportXmlController);

    /* @ngInject */
    function ImportXmlController($scope, spImportXml) {
        $scope.model = spImportXml.createModel();
    }

}());
