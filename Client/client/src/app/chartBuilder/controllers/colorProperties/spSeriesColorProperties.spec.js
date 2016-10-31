// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport */

describe('Charts|Builder|spec|controllers|spSeriesColorProperties', function () {
    'use strict';


    // Load the modules
    beforeEach(module('mod.app.chartBuilder'));
    beforeEach(module('chartBuilder/controllers/colorProperties/spSeriesColorProperties.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerDropdown.tpl.html'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('controller should load', inject(function ($controller, $rootScope, spChartBuilderService) {

        var builderModel = spChartBuilderService.createChartModel();

        var scope = $rootScope.$new();
        var controller = $controller('spSeriesColorPropertiesController', {
            $scope: scope,
            $uibModalInstance: {},
            model: {
                title: 'Primary',
                series: builderModel.chart.chartHasSeries[0],
                axis: builderModel.chart.chartHasSeries[0].primaryAxis
            }
        });
        expect(controller).toBeTruthy();
    }));

    it('template should load', inject(function ($rootScope, $compile, $templateCache) {
        var template = $templateCache.get('chartBuilder/controllers/colorProperties/spSeriesColorProperties.tpl.html');
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