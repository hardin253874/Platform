// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals visDataTestData */

describe('Edit Form|spec|Hero Text|spHeroTextControl', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.app.editForm.designerDirectives.spHeroTextControl'));
    beforeEach(module('editForm/directives/spHeroTextControl/spHeroTextControl.tpl.html'));
    beforeEach(module('mockedEntityService'));
    beforeEach(module('mockedReportService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    var mockReportId = 9999; // or what

    function mockControl() {
        var json = {
            id: 123,
            typeId: 'heroTextControl',
            name: 'My Label',
            heroTextReport: jsonLookup(mockReportId),
            heroTextSource: {
                typeId: 'chartSource',
                name: 'Source name',
                chartReportColumn: jsonLookup(),
                specialChartSource: jsonLookup(),
                sourceAggMethod: jsonLookup()
            },
            heroTextStyle: 'style2'
        };
        return spEntity.fromJSON(json);
    }

    it('should load with empty control', inject(function ($rootScope, $compile) {

        var scope = $rootScope.$new();
        scope.control = spEntity.fromJSON({});

        var elements = $compile('<sp-hero-text-control form-control="control"></sp-hero-text-control>')(scope);
        scope.$digest();

        var element = angular.element(elements[0]);

        expect(element.find('.hero-text-control').attr('class')).toContain('hero-text-style1');
        expect(element.find('.hero-title').html()).toBe('Title');
        expect(element.find('.hero-data').html()).toBe('-');

        // <div class="hero-text-control hero-text-style1">
        //  <div class="hero-title ng-binding">Title</div>
        //  <div class="hero-data ng-binding" ng-click="dataClicked()">-</div>
        // </div>

        // Test destroy
        scope.$destroy();
    }));

    it('should load with unsaved control', inject(function ($rootScope, $compile, spReportService) {

        var heroTextControl = mockControl();
        heroTextControl.heroTextSource.chartReportColumn = 16990; // a column that happenes to be summed in the report test data
        heroTextControl.heroTextSource.sourceAggMethod = 'core:aggSum';
        spReportService.mockGetReportData(mockReportId, visDataTestData.allFieldsPivoted);

        var scope = $rootScope.$new();

        // control that only contains ID
        scope.control = heroTextControl;

        var elements = $compile('<sp-hero-text-control form-control="control"></sp-hero-text-control>')(scope);
        scope.$digest();

        var element = angular.element(elements[0]);

        expect(element.find('.hero-text-control').attr('class')).toContain('hero-text-style2');
        expect(element.find('.hero-title').html()).toBe('My Label');
        expect(element.find('.hero-data').html()).toBe('1');
    }));

    it('should load with a populated control', inject(function ($rootScope, $compile, spEntityService, spReportService) {
        var heroTextControl = mockControl();
        heroTextControl.heroTextSource.chartReportColumn = 16990; // a column that happenes to be summed in the report test data
        heroTextControl.heroTextSource.sourceAggMethod = 'core:aggSum';
        spEntityService.mockGetEntity(heroTextControl);
        spReportService.mockGetReportData(mockReportId, visDataTestData.allFieldsPivoted);

        var scope = $rootScope.$new();

        // control that only contains ID
        scope.control = spEntity.fromJSON({ id: heroTextControl.idP });

        var elements = $compile('<sp-hero-text-control form-control="control"></sp-hero-text-control>')(scope);
        scope.$digest();

        var element = angular.element(elements[0]);

        expect(element.find('.hero-text-control').attr('class')).toContain('hero-text-style2');
        expect(element.find('.hero-title').html()).toBe('My Label');
        expect(element.find('.hero-data').html()).toBe('1');
    }));

});