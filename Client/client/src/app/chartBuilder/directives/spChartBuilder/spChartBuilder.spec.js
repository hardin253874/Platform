// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Charts|Builder|spec|directives|spChartBuilder', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.app.chartBuilder'));
    beforeEach(module('chartBuilder/directives/spChartBuilder/spChartBuilder.tpl.html'));
    beforeEach(module('chartBuilder/directives/spSeriesPanel/spSeriesPanel.tpl.html'));
    beforeEach(module('chartBuilder/directives/spChartTypes/spChartTypes.tpl.html'));
    beforeEach(module('chart/spChart.tpl.html'));    

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('should load', inject(function ($rootScope, $compile, spChartBuilderService) {

        var scope = $rootScope.$new();
        scope.model = spChartBuilderService.createChartModel();

        var element = $compile('<sp-chart-builder model="model"></sp-chart-builder>')(scope);
        scope.$digest();

        expect(element[0]).toBeTruthy();
        expect(element.html()).toContain("Primary");
    }));

});