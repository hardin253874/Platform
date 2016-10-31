// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Entity Model|mockedEntityService|spec:', function () {

    beforeEach(module('mockedEntityService'));

    // Load the module that contains the directive with a mocked entity service
    beforeEach(inject(function (spEntityService) {
        spEntityService.mockGetEntityJSON([
            { id: 1, name: 'Hello world' },
            { id: 'test:abc', firstName: 'Peter', lastName: 'Aylett' },
            { id: 'core:def', firstName: 'Peter2', lastName: 'Aylett' }
        ]);
    }));

    describe('The mocked entity service', function () {

        it('should load the mocked service', inject(function (spEntityService) {
            expect(spEntityService.mockGetEntity).toBeTruthy();
        }));

        it('mocks getEntity correctly', inject(function ($rootScope, spEntityService) {
            var p1 = spEntityService.getEntity('test:abc', 'name');

            var result1 = {};
            TestSupport.waitCheckReturn($rootScope, p1, result1);

            runs(function () {
                expect(result1.value.getFirstName()).toBe('Peter');
            });
        }));

        it('mocks getEntity correctly when alias without namespace is passed', inject(function ($rootScope, spEntityService) {
            var p1 = spEntityService.getEntity('def', 'name');

            var result1 = {};
            TestSupport.waitCheckReturn($rootScope, p1, result1);

            runs(function () {
                expect(result1.value.getFirstName()).toBe('Peter2');
            });
        }));

    });
});
