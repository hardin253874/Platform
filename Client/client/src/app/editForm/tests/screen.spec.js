// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Edit Form|spec:|screen', function () {
    describe('will create', function () {
        var screenController, $scope;

        beforeEach(module('mod.app.screen'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();
            $scope.alerter = { alerts: [] };
            screenController = $controller('screenController', { $scope: $scope, $element: $('<div></div>') });
        }));

        it('should pass a dummy test', inject(function () { 
            expect(screenController).toBeTruthy();
        }));
    });
});