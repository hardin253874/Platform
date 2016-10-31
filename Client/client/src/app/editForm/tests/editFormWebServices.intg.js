// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, TestSupport */

describe('Entity Model|editFormWebServices|intg:', function () {
    'use strict';

    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spWebService'));
    beforeEach(module('mod.app.editFormWebServices'));

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));
    
    
    describe('getForm from service', function() {

        it('can get a known form', inject(function ($rootScope, editFormWebServices) {
            var result = {};
            
            TestSupport.waitCheckReturn($rootScope,
                editFormWebServices.getFormDefinition('console:userRoleForm'),
                result);

            runs(function() {
                var form = result.value;
                expect(form).toBeTruthy();
                expect(form.getAlias()).toEqual('console:userRoleForm');
                expect(form.getContainedControlsOnForm().length).toBeGreaterThan(0);
            });
        }));

        it('can get a known form in design mode', inject(function ($rootScope, editFormWebServices) {
            var result = {};

            TestSupport.waitCheckReturn($rootScope,
                editFormWebServices.getFormDefinition('console:userRoleForm', true),
                result);

            runs(function () {
                var form = result.value;
                expect(form).toBeTruthy();
                expect(form.getAlias()).toEqual('console:userRoleForm');
                expect(form.getContainedControlsOnForm().length).toBeGreaterThan(0);
            });
        }));

    });


});
