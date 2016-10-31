// Copyright 2011-2016 Global Software Innovation Pty Ltd
/* jshint bitwise:false */

/**
 *  Library for encoding/decoding base64.
 *  * This is a third-party library that has been adapted. Need to figure out where it came from.
 *  @namespace base64
 */

var base64;
(function(base64) {

    var _keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

    /**
     * Accepts a string, converts it to UTF8 and returns the base64 encoded result.
     * @param {string} input text
     * @returns {string} Base 64 encoded string
     */
    base64.encodeText = function encodeText(input) {
        var binary = base64.encodeUtf8(input);
        return base64.encodeBinary(binary);
    };


    /**
     * Accepts an array of binary data, returns the base64 encoded result.
     * @param {binary} array of binary data
     * @returns {string} Base 64 encoded string
     */
    base64.encodeBinary = function encodeBinary(input) {
        if (!input)
            return null;

        var output = "";
        var chr1, chr2, chr3, enc1, enc2, enc3, enc4;
        var i = 0;

        while (i < input.length) {

            chr1 = input.charCodeAt(i++);
            chr2 = input.charCodeAt(i++);
            chr3 = input.charCodeAt(i++);

            enc1 = chr1 >> 2;
            enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
            enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
            enc4 = chr3 & 63;

            if (isNaN(chr2)) {
                enc3 = enc4 = 64;
            } else if (isNaN(chr3)) {
                enc4 = 64;
            }

            output = output + _keyStr.charAt(enc1) + _keyStr.charAt(enc2) + _keyStr.charAt(enc3) + _keyStr.charAt(enc4);
        }

        return output;
    };


    /**
     * Accepts base 64 encoded UTF 8 data, and returns the original text
     * @param {string} Base 64 encoded UTF8
     * @returns {string} original text
     */
    base64.decodeText = function decodeText(input) {
        var binary = base64.decodeBinary(input);
        return base64.decodeUtf8(binary);
    };


    /**
     * Accepts base 64 encoded data, and returns the original binary data as an array
     * @param {string} Base 64 encoded data
     * @returns {binary} Array of binary data
     */
    base64.decodeBinary = function decodeBinary(input) {
        if (!input)
            return null;

        var output = "";
        var chr1, chr2, chr3;
        var enc1, enc2, enc3, enc4;
        var i = 0;

        input = input.replace(/[^A-Za-z0-9\+\/\=]/g, "");

        while (i < input.length) {

            enc1 = _keyStr.indexOf(input.charAt(i++));
            enc2 = _keyStr.indexOf(input.charAt(i++));
            enc3 = _keyStr.indexOf(input.charAt(i++));
            enc4 = _keyStr.indexOf(input.charAt(i++));

            chr1 = (enc1 << 2) | (enc2 >> 4);
            chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
            chr3 = ((enc3 & 3) << 6) | enc4;

            output = output + String.fromCharCode(chr1);

            if (enc3 != 64) {
                output = output + String.fromCharCode(chr2);
            }
            if (enc4 != 64) {
                output = output + String.fromCharCode(chr3);
            }

        }

        return output;
    };


    /**
     * Takes a string and returns the UTF8 data as an array
     * @param {string} A string
     * @returns {binary} UTF8 binary data
     */
    base64.encodeUtf8 = function encodeUtf8(string) {
        if (!string)
            return null;

        string = string.replace(/\r\n/g, "\n");
        var utftext = "";

        for (var n = 0; n < string.length; n++) {

            var c = string.charCodeAt(n);

            if (c < 128) {
                utftext += String.fromCharCode(c);
            } else if ((c > 127) && (c < 2048)) {
                utftext += String.fromCharCode((c >> 6) | 192);
                utftext += String.fromCharCode((c & 63) | 128);
            } else {
                utftext += String.fromCharCode((c >> 12) | 224);
                utftext += String.fromCharCode(((c >> 6) & 63) | 128);
                utftext += String.fromCharCode((c & 63) | 128);
            }
        }

        return utftext;
    };


    /**
     * Takes UTF8 binary data and returns the original string
     * @param {binary} UTF8 binary data
     * @returns {string} A string
     */
    base64.decodeUtf8 = function decodeUtf8(utftext) {
        if (!utftext)
            return null;

        var string = "";
        var i = 0;
        var c = 0, c1 = 0, c2 = 0, c3 = 0;

        while (i < utftext.length) {

            c = utftext.charCodeAt(i);

            if (c < 128) {
                string += String.fromCharCode(c);
                i++;
            } else if ((c > 191) && (c < 224)) {
                c2 = utftext.charCodeAt(i + 1);
                string += String.fromCharCode(((c & 31) << 6) | (c2 & 63));
                i += 2;
            } else {
                c2 = utftext.charCodeAt(i + 1);
                c3 = utftext.charCodeAt(i + 2);
                string += String.fromCharCode(((c & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63));
                i += 3;
            }
        }

        return string;
    };

})(base64 || (base64 = {}));
