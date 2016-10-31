// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Console|spDialog|spec', function () {
    'use strict';

    beforeEach(module('sp.common.spDialog'));

    it('should exist', inject(function(spDialog) {
        expect(spDialog).toBeTruthy();
    }));

    it('showUserError should exist', inject(function (spDialog) {
        expect(spDialog.showUserError).toBeTruthy();
    }));
    
    it('showInternalError should exist', inject(function (spDialog) {
        expect(spDialog.showInternalError).toBeTruthy();
    }));
    
    it('confirmdialog should exist', inject(function (spDialog) {
        expect(spDialog.confirmDialog).toBeTruthy();
    }));

   

});