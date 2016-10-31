// Copyright 2011-2016 Global Software Innovation Pty Ltd


describe('Console|home section|spec:', function () {
    describe('will create', function () {
        var HomeController, $scope;

        beforeEach(module('app.home'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();
            HomeController = $controller('HomeController', { $scope: $scope });
        }));

        it('should pass a dummy test', inject(function () {
            expect(HomeController).toBeTruthy();
        }));
    });
});

