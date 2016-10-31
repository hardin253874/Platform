// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals EditFormTestHelper */

describe('Edit Form|spec:|singlePickerController', function () {
    describe('will create', function () {
        var controller, $scope;

        beforeEach(module('app.editForm.singlePickerController'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();
            
            $scope.formControl = EditFormTestHelper.DummyRelControlEntity;

            controller = $controller('singlePickerController', { $scope: $scope });
        }));

        it('should pass a dummy test', inject(function () {
            expect(controller).toBeTruthy();
        }));
    });
});