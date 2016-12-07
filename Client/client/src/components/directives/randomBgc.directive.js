// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    'use strict';

    angular.module('sp.common.directives')
        .directive('randomBgc', function () {
            return {
                controller: function ($element) {
                    $element.css('background-color', getRandomColour());
                }
            };
        });

    function getRandomColour() {
        return '#' + rc(192) + rc(192) + rc(192);

        // returns a random colour between the given bounds, inclusive
        function rc(f = 0, t = 255) {
            return ('0' + Math.floor(Math.random() * (t - f + 1) + f).toString(16)).substr(-2);
        }
    }

}());
