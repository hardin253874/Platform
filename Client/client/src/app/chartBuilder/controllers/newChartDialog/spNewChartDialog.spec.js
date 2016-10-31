// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Charts|Builder|spec|controllers|spNewChartDialog', function () {
    'use strict';


    // Load the modules
    beforeEach(module('mod.app.chartBuilder'));
    beforeEach(module('chartBuilder/controllers/newChartDialog/spNewChartDialog.tpl.html'));
    beforeEach(module('app-templates'));
    beforeEach(module('component-templates'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    // Currently causes tests to fail because it launches async data requests 

    //it('controller should load', inject(function ($controller, $rootScope) {

    //    var scope = $rootScope.$new();
    //    var controller = $controller('spNewChartDialogController', {
    //        $scope: scope,
    //        $uibModalInstance: {},
    //        options: {}
    //    });
    //    expect(controller).toBeTruthy();
    //}));


    // Currently fails on sp-busy-indicator ... for reasons unknown the busyIndicator linker runs before the spNewChartsDialog linker, so the scope doesn't have its stuff

    //it('template should load', inject(function ($rootScope, $compile, $templateCache) {
    //    var template = $templateCache.get('chartBuilder/controllers/newChartDialog/spNewChartDialog.tpl.html');
    //    expect(template).toBeTruthy();

    //    var scope = $rootScope.$new();
    //    var element = angular.element(template);
    //    $compile(element)(scope);
    //    scope.$digest();

    //    expect(element[0]).toBeTruthy();
    //    //var chartElem = element.find('sp-axis-type-properties')[0];
    //    //expect(chartElem).toBeTruthy();
    //}));

});