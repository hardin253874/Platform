// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module containing color picker utility functions.       
    * 
    * @module spColorPickerUtils    
    */
    angular.module('mod.common.ui.spColorPickerUtils', [])
        .factory('spColorPickerUtils', function () {
            var exports = {};

            /**
            * Gets the specified color as a css rgba string.  
            *
            * @param {object} color The color.            
            * @returns {string} rgba css string representing the color.            
            */
            exports.getCssColorFromRgba = function(color) {
                var r = 0, g = 0, b = 0, a = 1;

                if (!color) {
                    return null;
                }

                if (color.r) {
                    r = color.r;
                }

                if (color.g) {
                    g = color.g;
                }

                if (color.b) {
                    b = color.b;
                }

                if (color.a >= 0) {
                    a = color.a / 255;
                }

                return 'rgba(' + r + ',' + g + ',' + b + ',' + a + ')';
            };

            
            /**
            * Return true if the two colors are equal, false otherwise.
            *
            * @param {object} c1 The first color to compare.
            * @param {object} c2 The second color to compare.            
            * @returns {bool} True if equal false otherwise.            
            */
            exports.areColorsEqual = function (c1, c2) {
                if (c1 === c2) {
                    return true;
                }

                if (!c1 || !c2) {
                    return false;
                }

                return c1.a === c2.a &&
                    c1.r === c2.r &&
                    c1.g === c2.g &&
                    c1.b === c2.b;
            };


            /**
            * Copy the source color to the destination
            *
            * @param {object} src The source color.
            * @param {object} dest The destintaion color.    
            */
            exports.copyColor = function (src, dest) {
                if (!dest) {
                    return;
                }

                if (!src) {
                    dest.a = 255;
                    dest.r = 0;
                    dest.g = 0;
                    dest.b = 0;
                } else {
                    dest.a = src.a;
                    dest.r = src.r;
                    dest.g = src.g;
                    dest.b = src.b;                    
                }
            };

            return exports;
        });
}());