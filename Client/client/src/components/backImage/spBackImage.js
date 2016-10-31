// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    angular.module('mod.common.ui.spBackImage', [])
        .directive('spBackImage', function() {
            return {
                restrict: 'A',
                transclude: false,
                replace: false,
                scope: false,
                link: function(scope, element, attrs) {
                    attrs.$observe('spBackImage', function(value) {
                        element.css({
                            'background-image': 'url(' + value + ')',
                            'background-size': 'cover'
                        });
                    });
                }
            };
        });
}());