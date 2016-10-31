// Copyright 2011-2016 Global Software Innovation Pty Ltd
/**
 * Tests sit right alongside the file they are testing, which is more intuitive
 * and portable than separating `src` and `test` directories. Additionally, the
 * build process will exclude all `.spec.js` files from the build
 * automatically.
 */

describe('Internal|UTC|spec:|utcDummyServiceTest', function () {
    'use strict';

    beforeEach(module('spApps.utcDummyService'));

    it('utcDummyService should exist', inject(function (utcDummyService) {
        expect(utcDummyService).toBeTruthy();
    }));

   
    it('getMsTzFromUrl function in utcDummyService should exist', inject(function (utcDummyService) {
        expect(utcDummyService.getMsTzFromUrl).toBeTruthy(); 
    }));
    

    it('getMsTzFromHeader function in utcDummyService should exist', inject(function (utcDummyService) {
        expect(utcDummyService.getMsTzFromHeader).toBeTruthy();
    }));
    
    
    it('getHeaders function in utcDummyService should exist', inject(function (utcDummyService) {
        expect(utcDummyService.getHeaders).toBeTruthy();
    }));

});