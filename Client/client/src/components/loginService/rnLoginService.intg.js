// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Console|Login|intg:|rnLoginService', function () {
    "use strict";

    beforeEach(module('sp.common.rnLoginService'));

    it('can be loaded', inject(function (rnLoginService) {
        expect(rnLoginService).toBeTruthy();
    }));

//    it('can login', inject(function ($injector, rnLoginService) {
//
//        TestSupport.setupIntgTests(this, $injector, { skipLogin: true });
//
//        var login = rnLoginService.login('EDC', 'Administrator', 'Password-goes-here', true); // don't login
//
//        TestSupport.wait(login.then(
//            function (result) {
//                expect(result.activeAccountInfo).toBeTruthy();
//                expect(result.initialSettings).toBeTruthy();
//            }));
//    }));
});

