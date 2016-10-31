// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals EditFormTestHelper */

describe('Edit Form|spec:|fieldControlOnFormController', function () {
    describe('will create', function () {
        var controller, $scope;

        beforeEach(module('app.editForm.fieldControlOnFormController'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();

            $scope.formControl = EditFormTestHelper.DummyControlEntity;

            $scope.formControl.setField('description', 'control description,', spEntity.DataType.String);
            $scope.formControl.setField('mandatoryControl', true, spEntity.DataType.Bool);
            $scope.formControl.getFieldToRender().setField('allowMultiLines', false, spEntity.DataType.Bool);

            controller = $controller('fieldControlOnFormController', { $scope: $scope });
        }));

        it('should pass a dummy test', inject(function () {
            expect(controller).toBeTruthy();
        }));
    });
});