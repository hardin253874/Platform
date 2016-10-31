// Copyright 2011-2016 Global Software Innovation Pty Ltd


describe('Console|demo view|spec:', function () {
    describe('will create', function () {
        var demoController, $scope;

        beforeEach(module('app.demo'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();
            demoController = $controller('DemoController', { $scope: $scope });
        }));

        it('demo controller was created ok and basic props exist on scope', inject(function () {
            expect(demoController).toBeTruthy();
            expect($scope.entityType).toBeTruthy();
        }));
    });
});

