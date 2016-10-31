// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Charts|Builder|spec|directives|spSeriesPanel', function () {
    'use strict';


    // Load the modules
    beforeEach(module('mod.app.chartBuilder'));
    beforeEach(module('chartBuilder/directives/spSeriesPanel/spSeriesPanel.tpl.html'));
    beforeEach(module('chartBuilder/directives/spChartTypes/spChartTypes.tpl.html'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('directive should load', inject(function ($compile, $rootScope, spChartBuilderService) {

        var scope = $rootScope.$new();
        scope.model = spChartBuilderService.createChartModel();

        var element = $compile('<sp-series-panel model="model"></sp-series-panel>')(scope);
        scope.$digest();

        expect(element[0]).toBeTruthy();
        expect(element.html()).toContain("Primary");
    }));

});