// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, TestSupport */

describe('Console|spConsoleService|intg:', function () {
    'use strict';

    beforeEach(module('sp.common.loginService'));
    beforeEach(module('mod.common.spConsoleService'));    

    beforeEach(inject(function ($injector) {
        TestSupport.setupIntgTests(this, $injector);
    }));
    

    it('should get the session info', inject(function (spConsoleService) {

        TestSupport.wait(
            spConsoleService.getSessionInfo()
            .then(function(sessionInfo) {
                expect(sessionInfo.bookmarkScheme).toBeTruthy();
                expect(sessionInfo.platformVersion).toBeTruthy();
            }));
    }));    
});

