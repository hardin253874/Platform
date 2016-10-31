// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Charts|Builder|spec|spChartBuilderToolbox directive', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.app.chartBuilder'));
    beforeEach(module('chartBuilder/directives/spChartBuilderToolbox/spChartBuilderToolbox.tpl.html'));
    beforeEach(module('chartBuilder/directives/spChartBuilderAvailableSources/spChartBuilderAvailableSources.tpl.html'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('should load', inject(function ($rootScope, $compile, spChartBuilderService) {

        var scope = $rootScope;
        var model = spChartBuilderService.createChartModel();

        scope.model = model;
        var element = angular.element('<sp-chart-builder-toolbox model="model"></sp-chart-builder-toolbox>');
        $compile(element)(scope);
        scope.$digest();

        expect(element[0].localName).toBe('sp-chart-builder-toolbox');
    }));

});