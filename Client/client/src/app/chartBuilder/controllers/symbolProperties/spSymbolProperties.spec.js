// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport */

describe('Charts|Builder|spec|controllers|spSymbolProperties', function () {
    'use strict';


    // Load the modules
    beforeEach(module('mod.app.chartBuilder'));
    beforeEach(module('chartBuilder/controllers/symbolProperties/spSymbolProperties.tpl.html'));
    beforeEach(module('colorPicker/spColorPickerDropdown.tpl.html'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('controller should load', inject(function ($controller, $rootScope, spChartBuilderService) {

        var builderModel = spChartBuilderService.createChartModel();

        var scope = $rootScope.$new();
        var controller = $controller('spSymbolPropertiesController', {
            $scope: scope,
            $uibModalInstance: {},
            model: {
                series: builderModel.chart.chartHasSeries[0]
            }
        });
        expect(controller).toBeTruthy();
    }));

    it('template should load', inject(function ($rootScope, $compile, $templateCache) {
        var template = $templateCache.get('chartBuilder/controllers/symbolProperties/spSymbolProperties.tpl.html');
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