// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals EditFormTestHelper */

describe('Edit Form|spec:|relationshipControlOnForm', function () {
    describe('will create', function () {
        var controller, $scope;

        beforeEach(module('app.editForm.relationshipControlOnForm'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();
            $scope.requestStrings = []; // NOTE, WE SHOULD MOVE THIS OUT OF SCOPE

            $scope.formControl = EditFormTestHelper.DummyRelControlEntity;

            controller = $controller('relationshipControlOnForm', { $scope: $scope });
        }));

        it('should pass a dummy test', inject(function () {
            expect(controller).toBeTruthy();
        }));
    });
});