// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Form Builder|spec:|editFormDesigner', function () {
    describe('will create', function () {
        var editFormDesignerController, $scope;

        beforeEach(module('mod.app.editFormDesigner'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();
            editFormDesignerController = $controller('editFormDesignerController', { $scope: $scope });
        }));

        it('should pass a dummy test', inject(function () { 
            expect(editFormDesignerController).toBeTruthy();
        }));
    });
});