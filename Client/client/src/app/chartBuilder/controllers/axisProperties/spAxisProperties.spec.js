// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport */

describe('Charts|Builder|spec|controllers|spAxisProperties', function () {
    'use strict';

    
    // Load the modules
    beforeEach(module('mod.app.chartBuilder')); 
    beforeEach(module('chartBuilder/controllers/axisProperties/spAxisProperties.tpl.html'));
    beforeEach(module('chartBuilder/directives/spAxisTypeProperties/spAxisTypeProperties.tpl.html'));
    beforeEach(module('chartBuilder/directives/spStackProperties/spStackProperties.tpl.html'));
    beforeEach(module('chartBuilder/directives/spDataLabelProperties/spDataLabelProperties.tpl.html'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('controller should load', inject(function ($controller, $rootScope, spChartBuilderService) {

        var builderModel = spChartBuilderService.createChartModel();

        var scope = $rootScope.$new();
        var controller = $controller('spAxisPropertiesController', {
            $scope: scope,
            $uibModalInstance: { },
            model: {
                title: 'Primary',
                series: builderModel.chart.chartHasSeries[0],
                axis: builderModel.chart.chartHasSeries[0].primaryAxis
            }
        });
        expect(controller).toBeTruthy();
    }));

    it('template should load', inject(function ($rootScope, $compile, $templateCache) {
        var template = $templateCache.get('chartBuilder/controllers/axisProperties/spAxisProperties.tpl.html');
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