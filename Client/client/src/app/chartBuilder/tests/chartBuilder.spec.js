// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Charts|Builder|spec|controllers|chartBuilder', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.app.chartBuilder'));
    beforeEach(module('chartBuilder/views/chartBuilder.tpl.html'));
    beforeEach(module('chartBuilder/directives/spChartBuilder/spChartBuilder.tpl.html'));
    beforeEach(module('chartBuilder/directives/spSeriesPanel/spSeriesPanel.tpl.html'));
    beforeEach(module('chartBuilder/directives/spChartTypes/spChartTypes.tpl.html'));
    beforeEach(module('chart/spChart.tpl.html'));    
    beforeEach(module('mockedNavService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('controller should load', inject(function ($controller, $rootScope, spChartBuilderService) {

        var builderModel = spChartBuilderService.createChartModel();

        var scope = $rootScope.$new();
        var controller = $controller('chartBuilderPageController', {
            $scope: scope
        });
        expect(controller).toBeTruthy();
    }));

    it('template should load', inject(function ($rootScope, $compile, $templateCache) {
        var template = $templateCache.get('chartBuilder/views/chartBuilder.tpl.html');
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