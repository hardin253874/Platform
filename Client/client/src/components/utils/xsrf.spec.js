// Copyright 2011-2016 Global Software Innovation Pty Ltd
describe('Internal|spXsrf library|spec:', function () {
    'use strict';

    var xsrf;

    beforeEach(module('mod.common.spXsrf'));

    beforeEach(inject(function (spXsrf) {
        xsrf = spXsrf;
    }));

    beforeEach(function () {
        this.addMatchers(TestSupport.matchers);
    });

    describe('addQueryStringParam', function() {
        it('returns falsey when falsey url is supplied', function(){
            var actual = xsrf.addQueryStringParam(null);
            expect(actual).toBeFalsy();
        });

        it('returns same url when falsey parameter name is supplied', function(){
            var actual = xsrf.addQueryStringParam('https://www.example.com/v1/data');
            expect(actual).toEqual('https://www.example.com/v1/data');
        });

        it('adds ? to start first query string parameter', function(){
            var actual = xsrf.addQueryStringParam('https://www.example.com/v1/data', 'value', '123');
            expect(actual).toEqual('https://www.example.com/v1/data?value=123');
        });

        it('adds & to chain query string parameters', function(){
            var actual = xsrf.addQueryStringParam('https://www.example.com/v1/data?value1=123', 'value2', '456');
            expect(actual).toEqual('https://www.example.com/v1/data?value1=123&value2=456');
        });

        it('URL encodes parameter name', function(){
            var actual = xsrf.addQueryStringParam('https://www.example.com/v1/data', 'my value', '123');
            expect(actual).toEqual('https://www.example.com/v1/data?my%20value=123');
        });

        it('URL encodes parameter value', function(){
            var actual = xsrf.addQueryStringParam('https://www.example.com/v1/data', 'value', '?=+/ ');
            expect(actual).toEqual('https://www.example.com/v1/data?value=%3F%3D%2B%2F%20');
        });

        it('does NOT add same parameter twice', function(){
            var url = xsrf.addQueryStringParam('https://www.example.com/v1/data', 'value', '123');
            var actual = xsrf.addQueryStringParam(url, 'value', '456');
            expect(actual).toEqual('https://www.example.com/v1/data?value=123');
        });
    });
});
