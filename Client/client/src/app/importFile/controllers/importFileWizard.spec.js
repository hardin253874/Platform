// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Import Files|spec|controllers|importFileWizard', function () {
    'use strict';

    // Load the modules
    beforeEach(module('ng'));
    beforeEach(module('mod.app.importFile'));
    beforeEach(module('importFile/controllers/importFileWizard.tpl.html'));
    beforeEach(module('importFile/views/wizardNavigation.tpl.html'));
    beforeEach(module('importFile/views/importPage1.tpl.html'));
    beforeEach(module('importFile/views/importPage2.tpl.html'));
    beforeEach(module('importFile/views/importPage3.tpl.html'));
    //beforeEach(module('importFile/views/importPage4.tpl.html'));
    beforeEach(module('fileUpload/spFileUpload.tpl.html'));
    beforeEach(module('mockedNavService'));
    
    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    it('controller should load', inject(function ($controller, $rootScope) {

        var scope = $rootScope.$new();
        var controller = $controller('importFileWizardController', {
            $scope: scope
        });
        expect(controller).toBeTruthy();
    }));

    it('template should load', inject(function ($rootScope, $compile, $templateCache) {
        var template = $templateCache.get('importFile/controllers/importFileWizard.tpl.html');
        expect(template).toBeTruthy();

        var scope = $rootScope.$new();
        var element = angular.element(template);
        $compile(element)(scope);
        scope.$digest();

        expect(element[0]).toBeTruthy();
        //var chartElem = element.find('sp-axis-type-properties')[0];
        //expect(chartElem).toBeTruthy();
    }));

    it('controller walkthrough for new import config', inject(function ($controller, $rootScope) {

        var scope = $rootScope.$new();
        var controller = $controller('importFileWizardController', {
            $scope: scope
        });
        var model = scope.model;
        model.upload.onFileUploadComplete('MyFileName.xlsx', 'myfileuploadid');
        expect(controller).toBeTruthy();
    }));


    describe('isDirty', function () {

        it('is not initially dirty after creating new instance', inject(function ($controller, $rootScope) {

            var scope = $rootScope.$new();
            $controller('importFileWizardController', { $scope: scope });
            var isDirty = scope.navItem.isDirty();
            expect(isDirty).toBe(false);
        }));
    });

    describe('navigation labels', function() {
        var scope;

        beforeEach(inject(function($controller, $rootScope) {
            scope = $rootScope.$new();
            $controller('importFileWizardController', { $scope: scope });
        }));

        it('should not navigate backwards from first page', function () {
            scope.model.nav.curPage = 1;
            scope.handlePrevious();
            expect(scope.model.nav.curPage).toBe(1);
        });

        var nextLinks = ['Next', 'Next', 'Next', 'Import', 'Done'];
        _.forEach(nextLinks, function (label, index) {
            var pageNum = index + 1;
            it('next link should be labelled "' + label + '" on page ' + pageNum, function () {
                scope.model.nav.curPage = pageNum;
                var labelText = scope.getNextLabel();
                expect(labelText).toBe(label);
            });
        });
    });

});