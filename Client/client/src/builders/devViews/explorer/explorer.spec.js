// Copyright 2011-2016 Global Software Innovation Pty Ltd


describe('Console|explorer view|spec:', function () {
    describe('will create', function () {
        var explorerController, $scope;

        beforeEach(module('app.entityExplorer'));

        beforeEach(inject(function ($controller, $rootScope) {

            $scope = $rootScope.$new();
            explorerController = $controller('EntityExplorerController', { $scope: $scope });
        }));

        it('explorer controller was created ok and basic props exist on scope', inject(function () {
            expect(explorerController).toBeTruthy();
        }));
    });
});

