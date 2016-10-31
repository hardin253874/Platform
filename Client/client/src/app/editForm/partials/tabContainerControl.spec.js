// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals EditFormTestHelper */

describe('Edit Form|spec:|tabContainerController', function () {
    describe('will create', function () {
        var controller, $scope;

        beforeEach(module('app.editForm.tabContainerController'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();

            $scope.formControl = EditFormTestHelper.DummyStructControlEntity;
            
            controller = $controller('tabContainerController', { $scope: $scope });
        }));

        it('should pass a dummy test', inject(function () { 
            expect(controller).toBeTruthy();
        }));
    });
});