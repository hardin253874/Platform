// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    angular.module('mod.common.ui.spFocus', [])
        .directive('spFocus', function () {
            return function(scope, elem, attr) {
                scope.$on('spFocus', function (e, name) {
                    if (name === scope.$eval(attr.spFocus)) {
                        elem[0].focus();
                    }
                });
            };
        })
        .directive('spBlur', function() {
            return function(scope, elem, attr) {
                scope.$on('spBlur', function(e, name) {
                    if (name === attr.spBlur) {
                        elem[0].blur();
                    }
                });
            };
        })
        .directive('spAutoFocus', function () {
            return function(scope, elem) {
                elem[0].focus();
            };
        })
        .factory('focus', function($rootScope, $timeout) {
            return function(name) {
                $timeout(function() {
                    $rootScope.$broadcast('spFocus', name);
                });
            };
        })
        .factory('blur', function($rootScope, $timeout) {
            return function(name) {
                $timeout(function() {
                    $rootScope.$broadcast('spBlur', name);
                });
            };
        });
}());