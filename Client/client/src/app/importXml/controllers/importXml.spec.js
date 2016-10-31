// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Import XML|spec|controllers|importXml', function () {
    'use strict';

    // Load the modules
    beforeEach(module('ng'));
    beforeEach(module('mod.app.importXml'));
    beforeEach(module('importXml/controllers/importXml.tpl.html'));
    beforeEach(module('fileUpload/spFileUpload.tpl.html'));
    beforeEach(module('mockedNavService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('controller should load', inject(function ($controller, $rootScope) {

        var scope = $rootScope.$new();
        var controller = $controller('importXmlController', {
            $scope: scope
        });
        expect(controller).toBeTruthy();
    }));

    it('template should load', inject(function ($rootScope, $compile, $templateCache) {
        var template = $templateCache.get('importXml/controllers/importXml.tpl.html');
        expect(template).toBeTruthy();

        var scope = $rootScope.$new();
        var element = angular.element(template);
        $compile(element)(scope);
        scope.$digest();

        expect(element[0]).toBeTruthy();
    }));

});