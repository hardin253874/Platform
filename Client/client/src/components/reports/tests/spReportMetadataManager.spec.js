// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Reports|View|spReportMetadataManager|spec:', function () {
    'use strict';   
    
    beforeEach(module('mod.ui.spReportMetadataManager'));

    function expectColorsAreEqual(c1, c2) {
        expect(c1.a).toBe(c2.a);
        expect(c1.r).toBe(c2.r);
        expect(c1.g).toBe(c2.g);
        expect(c1.b).toBe(c2.b);
    }   
});