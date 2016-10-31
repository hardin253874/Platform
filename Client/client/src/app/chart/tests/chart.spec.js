// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport */

describe('Charts|Viewer|spec|chart view', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.app.chart'));
    beforeEach(module('chart/views/chart.tpl.html'));
    beforeEach(module('mockedEntityService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('controller should load', inject(function ($controller, $rootScope) {

        var scope = $rootScope.$new();
        var controller = $controller('chartController', { $scope: scope });
        expect(controller).toBeTruthy();
    }));

    it('template should load', inject(function ($rootScope, $compile, $templateCache) {
        var template = $templateCache.get('chart/views/chart.tpl.html');
        expect(template).toBeTruthy();

        var scope = $rootScope.$new();
        var element = angular.element(template);
        $compile(element)(scope);
        scope.$digest();

        expect(element[0]).toBeTruthy();
        var chartElem = element.find('sp-chart')[0];
        expect(chartElem).toBeTruthy();
    }));

});