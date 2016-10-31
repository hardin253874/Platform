// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals EditFormTestHelper */

describe('Edit Form|spec:|imageController', function () {
    //describe('will create', function () {
    'use strict';

    var controller, $scope;

    // Load the modules
    beforeEach(module('ui.bootstrap'));
    beforeEach(module('app.editForm.imageController'));
    beforeEach(module('mockedEntityService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupUnitTests(this, $injector);
    }));

    // Setup the mocked entity service.
    beforeEach(inject(function (spEntityService) {

        var json = {
            id: 'core:templateReport',
            name: 'Template Report'
        };
        spEntityService.mockGetEntityJSON(json);
    }));

    beforeEach(inject(function ($controller, $rootScope) {

        $scope = $rootScope.$new();
        $scope.requestStrings = []; // NOTE, WE SHOULD MOVE THIS OUT OF SCOPE

        $scope.formControl = EditFormTestHelper.DummyRelControlEntity;

        controller = $controller('imageController', { $scope: $scope });
    }));

    it('should create imageController.', inject(function () {
        expect(controller).toBeTruthy();
    }));
    //});
});