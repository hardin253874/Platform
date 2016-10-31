// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals EditFormTestHelper */

describe('Edit Form|spec:|formRenderControl', function () {
    describe('will create', function () {
        var controller, $scope;

        beforeEach(module('app.editForm.formRenderControl'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();

            $scope.formControl = EditFormTestHelper.DummyControlEntity;

            controller = $controller('formRenderControl', { $scope: $scope });
        }));

        it('should pass a dummy test', inject(function () {
            expect(controller).toBeTruthy();
        }));
    });

    describe('will coordinate disabling edit buttons', function() {

        var controller1, controller2, $scope1, $scope2;

        beforeEach(module('app.editForm.formRenderControl'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope1 = $rootScope.$new();
            $scope2 = $rootScope.$new();

            $scope1.formControl = EditFormTestHelper.DummyControlEntity;
            $scope2.formControl = EditFormTestHelper.DummyControlEntity;

            controller1 = $controller('formRenderControl', { $scope: $scope1 });
            controller2 = $controller('formRenderControl', { $scope: $scope2 });
        }));

        it('when one is editing the other is disabled', inject(function() {
            expect($scope1.isDisabled).toBeFalsy();
            expect($scope2.isDisabled).toBeFalsy();
            $scope1.onEditClick();
            expect($scope1.formMode).toEqual('edit');
            expect($scope1.isDisabled).toBeFalsy();
            expect($scope2.isDisabled).toBeTruthy();
        }));
    });
});