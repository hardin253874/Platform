// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global describe, beforeEach, expect, it, module, inject */

describe('Internal|localStorage library|spec:', function () {
    'use strict';

    var key = 'thisShouldntBeDefinedXXX',
        value;

    beforeEach(module('mod.common.spLocalStorage'));

    it('should get the same value it just set and clear when done', inject(function (spLocalStorage) {
        value = 'someArbitraryValue';

        expect(spLocalStorage).toBeTruthy();
        expect(spLocalStorage.getItem(key)).toBeNull();

        spLocalStorage.setItem(key, value);
        expect(spLocalStorage.getItem(key)).toBe(value);

        spLocalStorage.removeItem(key);
        expect(spLocalStorage.getItem(key)).toBeNull();
    }));

    it('should get the same object it just set and clear when done', inject(function (spLocalStorage) {
        value = { a: 'hello' };

        expect(spLocalStorage.getObject(key)).toBeNull();

        spLocalStorage.setObject(key, value);
        expect(spLocalStorage.getObject(key)).toBeTruthy();
        expect(spLocalStorage.getObject(key).a).toBeTruthy('hello');

        spLocalStorage.removeItem(key);
        expect(spLocalStorage.getObject(key)).toBeNull();
    }));

    it('should not fail if setting and getting null or setting undefined', inject(function (spLocalStorage) {

        expect(spLocalStorage.getObject(key)).toBeNull();

        spLocalStorage.setObject(key, null);
        expect(spLocalStorage.getObject(key)).toBeNull();

        spLocalStorage.setObject(key, undefined);
        expect(spLocalStorage.getObject(key)).toBeNull();

        spLocalStorage.removeItem(key);
        expect(spLocalStorage.getObject(key)).toBeNull();
    }));
});
