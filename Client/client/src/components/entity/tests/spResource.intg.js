// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Entity Model|spResource|intg:', function () {
    'use strict';

    var $injector, $http, $rootScope, $q,
        spWebService, spEntityService, spResource;

    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spEntityService'));
    beforeEach(module('mod.common.spResource'));

    beforeEach(inject(function ($injector) {

        // load a DI table with the modules needed
        $http = $injector.get('$http');
        $rootScope = $injector.get('$rootScope');
        spWebService = $injector.get('spWebService');
        spEntityService = $injector.get('spEntityService');
        spResource = $injector.get('spResource');

        TestSupport.setupIntgTests(this, $injector);
    }));


    describe('getAncestorsAndSelf', function () {

        it('should work for simple inheritance chain', function () {
            var result = {};
            TestSupport.waitCheckReturn($rootScope,
                spEntityService.getEntity('test:manager', 'name, inherits*.{name}'),
                result);

            runs(function () {
                var type = result.value;
                var res = spResource.getAncestorsAndSelf(type);
                expect(res).toBeArray(6);
                expect(res[0].getName()).toBe('AA_Manager');
                expect(res[1].getName()).toBe('AA_Employee');
                expect(res[2].getName()).toBe('AA_Person');
                expect(res[3].getName()).toBe('AA_Actor');
                expect(res[4].getName()).toBe('Editable Resource');
                expect(res[5].getName()).toBe('Resource');
            });
        });
    });


    describe('getDerivedTypesAndSelf', function () {

        it('should work for simple inheritance chain', function () {
            var result = {};
            TestSupport.waitCheckReturn($rootScope,
                spEntityService.getEntity('test:employee', 'name, derivedTypes*.{name}'),
                result);

            runs(function () {
                var type = result.value;
                var res = spResource.getDerivedTypesAndSelf(type);
                expect(res).toBeArray(2);
                expect(res[0].getName()).toBe('AA_Employee');
                expect(res[1].getName()).toBe('AA_Manager');
            });
        });
    });


});