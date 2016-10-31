// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('editFormWebServices', function () {
    "use strict";

    beforeEach(module('mod.app.editFormWebServices'));

    it('can load service', inject(function (editFormWebServices) {
            expect(editFormWebServices).toBeTruthy();
        }));
    

});

