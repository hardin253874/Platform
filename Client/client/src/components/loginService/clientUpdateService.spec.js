// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Console|Login|spec:|clientUpdateService', function() {
    "use strict";

    var $cookies;

    beforeEach(module('sp.common.clientUpdateService'));


    //beforeEach(angular.mock.module('ngCookies'));


    beforeEach(function () {
        module('ngCookies');

        //mock $cookies
        module({
            $cookies: {
                store: {},
                put: function (key, value) { this.store[key] = value; },
                get: function (key) { return this.store[key]; },
                remove: function (key) { this.store[key] = undefined; }
            }
        });

        inject(function ($injector) {
            $cookies = $injector.get('$cookies');
        });
    });


    beforeEach(inject(function ($window, $cookies, $timeout) {
        // Setting a cookie for testing
        $cookies.remove('updatingClient');
    }));


    it('can be loaded', inject(function(spClientUpdateService) {
        expect(spClientUpdateService).toBeTruthy();
    }));

    it('attemptClientRefresh failed', inject(function (spClientUpdateService, $cookies) {
        var result1 = spClientUpdateService.attemptClientRefresh();

        expect(result1).toBeTruthy();
        expect($cookies.get('updatingClient')).toBeTruthy();

        var result2 = spClientUpdateService.attemptClientRefresh();
        expect(result2).toBeFalsy();

    }));

    it('checkMinClientVersionAndRefresh refresh required and succeeds', inject(function (spClientUpdateService, $cookies) {

        var result1 = spClientUpdateService.checkMinClientVersionAndRefresh('0.9', '1.0');

        expect(result1).toBeFalsy();
        expect($cookies.get('updatingClient')).toBeTruthy();

        var result2 = spClientUpdateService.checkMinClientVersionAndRefresh('1.0', '1.0');

        expect(result2).toBeTruthy();
        expect($cookies.get('updatingClient')).toBeFalsy();
    }));

    it('checkMinClientVersionAndRefresh refresh required and fails and further attempts blocked', inject(function (spClientUpdateService, $cookies) {

        var result1 = spClientUpdateService.checkMinClientVersionAndRefresh('0.9', '1.0');

        expect(result1).toBeFalsy();
        expect($cookies.get('updatingClient')).toBeTruthy();

        var result2 = spClientUpdateService.checkMinClientVersionAndRefresh('0.9', '1.0');

        expect(result2).toBeTruthy();
        expect($cookies.get('updatingClient')).toBeFalsy();

        expect(spClientUpdateService.areClientUpdatesBlocked()).toBeTruthy();
    }));
});



