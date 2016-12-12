// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals entityTestData */

describe('Console|Pickers|spec|spReportPicker directive', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.common.spWebService'));    
    beforeEach(module('mod.common.ui.spReportPicker'));
    beforeEach(module('entityPickers/reportPicker/spReportPicker.tpl.html'));
    beforeEach(module('entityPickers/entityComboPicker/spEntityComboPicker.tpl.html'));
    beforeEach(module('mockedEntityService'));
    
    // Set the mocked data
    beforeEach(inject(function (spEntityService) {
        // Set the data we wish the mock to return
        spEntityService.mockGetInstancesOfTypeRawData('core:report', entityTestData.thumbnailSizesTestData); // or whatever
    }));

    it('should load', inject(function ($rootScope, $compile) {

        var scope = $rootScope;
        scope.report = null;

        var element = angular.element('<sp-report-picker report="report"></sp-report-picker>');
        $compile(element)(scope);
        scope.$digest();

        var elem = element.find('sp-entity-combo-picker');
        expect(elem.length).toBe(1);
    }));

});