// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Internal|spWebService service|spec:', function () {
    'use strict';

    var $injector, spWebService;

    beforeEach(function () {

        $injector = angular.injector(['ng', 'mod.common.spWebService']);
        spWebService = $injector.get('spWebService');

        this.addMatchers(TestSupport.matchers);
    });

    it('should be loadable', function () {
        expect(spWebService).toBeTruthy();

        expect(spWebService.setWebApiRoot).toBeTruthy();
        expect(spWebService.getWebApiRoot).toBeTruthy();
        expect(spWebService.getHeaders).toBeTruthy();
    });

});
