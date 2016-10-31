// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Console|Login|spec:|loginService', function() {
    "use strict";

    beforeEach(module('sp.common.loginService'));

    it('can be loaded', inject(function(spLoginService) {
        expect(spLoginService).toBeTruthy();
    }));
});

