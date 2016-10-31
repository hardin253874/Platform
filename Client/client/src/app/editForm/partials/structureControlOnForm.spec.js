// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals EditFormTestHelper */

describe('Edit Form|spec:|structureControlOnFormController', function () {
    describe('will create', function () {
        var controller, $scope;

        beforeEach(module('app.editForm.structureControlOnFormController'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();

            $scope.formControl = EditFormTestHelper.DummyControlEntity;
            
            controller = $controller('structureControlOnFormController', { $scope: $scope });
        }));

        it('should pass a dummy test', inject(function () { 
            expect(controller).toBeTruthy();
        }));
    });
});