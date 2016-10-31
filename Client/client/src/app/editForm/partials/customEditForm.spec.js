// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals EditFormTestHelper */

describe('Edit Form|spec:|customEditFormController', function () {
    describe('will create', function () {
        var controller, $scope;

        beforeEach(module('app.editForm.customEditFormController'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();

            $scope.formControl = EditFormTestHelper.DummyControlEntity;

            controller = $controller('customEditFormController', { $scope: $scope });
        }));

        it('should pass a dummy test', inject(function () {
            expect(controller).toBeTruthy();
        }));
    });
});