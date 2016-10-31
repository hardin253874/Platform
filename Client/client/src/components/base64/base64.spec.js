// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* globals base64 */

describe('Internal', function () {
describe('base64 library', function () {
describe('spec:', function () {
    'use strict';

    it('should exist', function () {
        expect(base64).toBeTruthy();
    });

    describe('encode', function () {
        it('encodeText should exist', function () {
            expect(base64.encodeText).toBeTruthy();
        });
        it('encodeBinary should exist', function () {
            expect(base64.encodeBinary).toBeTruthy();
        });
        it('encodeUtf8 should exist', function () {
            expect(base64.encodeUtf8).toBeTruthy();
        });
        it('encodeText should handle null', function () {
            expect(base64.encodeText(null)).toBeNull();
        });
        it('encodeBinary should handle null', function () {
            expect(base64.encodeBinary(null)).toBeNull();
        });
        it('encodeUtf8 should handle null', function () {
            expect(base64.encodeUtf8(null)).toBeNull();
        });
        it('should work', function () {
            var encoded = base64.encodeText('sure.');
            expect(encoded).toBeTruthy();
            expect(encoded).toEqual('c3VyZS4=');
        });
    });

    describe('decode', function () {
        it('decodeText should exist', function () {
            expect(base64.decodeText).toBeTruthy();
        });
        it('decodeBinary should exist', function () {
            expect(base64.decodeBinary).toBeTruthy();
        });
        it('decodeUtf8 should exist', function () {
            expect(base64.decodeUtf8).toBeTruthy();
        });
        it('decodeText should handle null', function () {
            expect(base64.decodeText(null)).toBeNull();
        });
        it('decodeBinary should handle null', function () {
            expect(base64.decodeBinary(null)).toBeNull();
        });
        it('decodeUtf8 should handle null', function () {
            expect(base64.decodeUtf8(null)).toBeNull();
        });
        it('should work', function () {
            var decoded = base64.decodeText('c3VyZS4=');
            expect(decoded).toBeTruthy();
            expect(decoded).toEqual('sure.');
        });
    });

});
});
});
