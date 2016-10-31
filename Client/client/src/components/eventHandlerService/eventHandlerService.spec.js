// Copyright 2011-2016 Global Software Innovation Pty Ltd

describe('Internal|eventHandlerService|spec:', function () {
    'use strict';

    beforeEach(module('spApps.eventHandlerService'));


    it('should exist', inject(function (eventHandlerService) {
        expect(eventHandlerService).toBeTruthy();
    }));
    
    it('postEvent should exist', inject(function (eventHandlerService) {
        expect(eventHandlerService.postEvent).toBeTruthy();
    }));
});