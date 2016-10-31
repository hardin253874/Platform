// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Reports|View|Constants|spReportConstants|spec:', function () {
    'use strict';

    // Load the modules
    beforeEach(module('mod.ui.spReportConstants'));
    
    it('verify report constants', inject(function (reportPageSize, contextMenu) {
        expect(reportPageSize.value).toEqual(200);    

        expect(contextMenu).toBeTruthy();
        expect(_.isObject(contextMenu)).toEqual(true);
        expect(_.isArray(contextMenu.menuItems)).toEqual(true);
    }));   
});