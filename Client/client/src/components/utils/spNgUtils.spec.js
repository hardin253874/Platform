// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global sp, _, describe, it, beforeEach, TestSupport, expect */

describe('Internal|spNgUtils library|spec:', function () {
    'use strict';

    beforeEach(module('mod.common.spNgUtils'));    

    describe('sanitizeUriComponent', function () {
        it('null component', inject(function (spNgUtils) {            
            expect(spNgUtils.sanitizeUriComponent(null)).toBe(null);            
        }));
        it('only valid chars, default replacement', inject(function (spNgUtils) {            
            expect(spNgUtils.sanitizeUriComponent('/sp/test/a')).toBe('/sp/test/a');
        }));
        it('only valid chars, new replacement', inject(function (spNgUtils) {
            expect(spNgUtils.sanitizeUriComponent('/sp/test/a','=')).toBe('/sp/test/a');
        }));
        it('invalid chars, default replacement', inject(function (spNgUtils) {
            expect(spNgUtils.sanitizeUriComponent('/sp/test/1<2>3*4%5&6:7\\\\8?')).toBe('/sp/test/1_2_3_4_5_6_7_8_');
        }));
        it('only valid chars, new replacement', inject(function (spNgUtils) {
            expect(spNgUtils.sanitizeUriComponent('/sp/test/1<2>3*4%5&6:7\\\\8?','=')).toBe('/sp/test/1=2=3=4=5=6=7=8=');
        }));
    });   
});
