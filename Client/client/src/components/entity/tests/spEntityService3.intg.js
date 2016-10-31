// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, module, describe, beforeEach, it, inject, expect, runs, waitsFor, TestSupport, sp, spEntity */

describe('Entity Model|spEntityService3|intg:', function () {
    'use strict';


    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spEntityService'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));

    it('100: can get a known entity (oldshared:employee)', inject(function (spEntityService) {

        TestSupport.wait(
            spEntityService.getEntity('test:steveGibbon', 'name,oldshared:employeesManager.name,isOfType.relationships.*')
            .then(function(entity) {
                expect(entity).toBeTruthy();
                console.log(spEntity.toJSON(entity));
                console.log(entity);
                expect(entity.getEmployeesManager()).toBeTruthy();
            }));
    }));
});
