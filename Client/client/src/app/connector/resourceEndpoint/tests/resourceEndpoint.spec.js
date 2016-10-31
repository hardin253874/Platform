// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Connector|spec|resourceEndpoint', function () {
    'use strict';


    // Load the modules
    beforeEach(module('mod.app.connector.resourceEndpoint'));
    beforeEach(module('connector/resourceEndpoint/resourceEndpoint.tpl.html'));
    beforeEach(module('editForm/partials/spInlineRelationshipPicker.tpl.html'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedReportService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    //
    // Setup the mocked entity service.
    //
    beforeEach(inject(function (spEntityService) {

        var json = {
            id: 'templateReport',
            name: 'Template Report',
            description: 'Template Report'
        };

        spEntityService.mockGetEntityJSON(json);
    }));

    it('controller should load', inject(function ($controller, $rootScope) {

        var scope = $rootScope.$new();
        var controller = $controller('resourceEndpointController', {
            $scope: scope
        });
        expect(controller).toBeTruthy();
    }));

    it('template should load', inject(function ($rootScope, $compile, $templateCache) {
        var template = $templateCache.get('connector/resourceEndpoint/resourceEndpoint.tpl.html');
        expect(template).toBeTruthy();

        var scope = $rootScope.$new();
        var element = angular.element(template);
        $compile(element)(scope);
        scope.$digest();

        expect(element[0]).toBeTruthy();
        //var chartElem = element.find('sp-axis-type-properties')[0];
        //expect(chartElem).toBeTruthy();
    }));

});