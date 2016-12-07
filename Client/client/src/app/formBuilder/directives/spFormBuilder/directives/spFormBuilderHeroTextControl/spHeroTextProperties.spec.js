// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Form Builder|spec|Hero Text|spHeroTextProperties', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.app.heroText.spHeroTextProperties'));
    beforeEach(module('controls/templates/spTypeaheadPopupTemplate.tpl.html'));
    beforeEach(module('formBuilder/directives/spFormBuilder/directives/spFormBuilderHeroTextControl/spHeroTextProperties.tpl.html'));
    beforeEach(module('editForm/partials/spInlineRelationshipPicker.tpl.html'));    
    beforeEach(module('editForm/directives/spHeroTextControl/spHeroTextControl.tpl.html'));
    beforeEach(module('mockedEntityService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    beforeEach(inject(function (spEntityService) {
        spEntityService.mockTemplateReport();
    }));

    it('controller should load', inject(function ($controller, $rootScope) {

        var heroTextControl = spEntity.fromJSON({            
        });

        var scope = $rootScope.$new();
        var controller = $controller('spHeroTextPropertiesController', {
            $scope: scope,
            $uibModalInstance: {},
            heroTextControl: heroTextControl
        });
        expect(controller).toBeTruthy();
    }));

    it('template should load', inject(function ($rootScope, $compile, $templateCache) {
        var template = $templateCache.get('formBuilder/directives/spFormBuilder/directives/spFormBuilderHeroTextControl/spHeroTextProperties.tpl.html');
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