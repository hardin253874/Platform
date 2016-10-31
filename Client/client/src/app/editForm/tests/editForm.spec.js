// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Edit Form|spec:|editForm', function () {
    describe('will create', function () {
        var editFormController, $scope;

        beforeEach(module('mod.app.editForm'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();
            $scope.alerter = { alerts: [] };
            editFormController = $controller('editFormController', { $scope: $scope, $element: $('<div></div>') });
        }));

        it('should pass a dummy test', inject(function () { 
            expect(editFormController).toBeTruthy();
        }));
    });
});