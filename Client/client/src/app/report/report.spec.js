// Copyright 2011-2016 Global Software Innovation Pty Ltd


describe('Reports|report view|spec', function () {
    describe('will create', function () {
        var ReportController, $scope;

        beforeEach(module('app.report'));
        beforeEach(module('mockedNavService'));

        beforeEach(inject(function ($injector) {
            TestSupport.setupUnitTests(this, $injector);
        }));

        beforeEach(inject(function ($controller, $rootScope) {
            $scope = $rootScope.$new();
            ReportController = $controller('ReportController', { $scope: $scope });
        }));

        it('report controller was created ok and basic props exist on scope', inject(function () {
            expect(ReportController).toBeTruthy();
        }));
    });
});

