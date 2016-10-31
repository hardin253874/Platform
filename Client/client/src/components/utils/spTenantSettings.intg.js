// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Internal|spTenantSettings library|intg:', function () {
    'use strict';

    beforeEach(module('mod.common.spTenantSettings'));

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });

    it('service load', inject(function ($rootScope, spTenantSettings) {

        expect(spTenantSettings).toBeTruthy();
    }));

});
