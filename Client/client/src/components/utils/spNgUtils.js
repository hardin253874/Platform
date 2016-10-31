// Copyright 2011-2016 Global Software Innovation Pty Ltd

/**
 * Module for utilities that depend on angular modules.
 *
 * @module spNgUtils
 */
angular.module('mod.common.spNgUtils', ['ng'])
    .service('spNgUtils', function ($q, $document, $window, $timeout, $location) {
        'use strict';

        // Determine if request animation frame is supported
        var requestAnimationFrame = $window.requestAnimationFrame || $window.webkitRequestAnimationFrame;
        var cancelAnimationFrame = $window.cancelAnimationFrame || $window.webkitCancelAnimationFrame || $window.webkitCancelRequestAnimationFrame;
        var rafSupported = !!requestAnimationFrame;

        function supportedRafFunc(fn) {
            var id = requestAnimationFrame(fn);
            return function () {
                cancelAnimationFrame(id);
            };
        }

        function fallbackRafFunc(fn) {
            var timer = $timeout(fn, 16.66, false); // 1000 / 60 = 16.666
            return function () {
                $timeout.cancel(timer);
            };
        }

        var raf = rafSupported ? supportedRafFunc : fallbackRafFunc;

        var exports = {};

        /**
        * @ngdoc method
        * @name setFocusOnFirstVisibleInput
        * @description sets the focus on first visible input in $document
        */
        exports.setFocusOnFirstVisibleInput = function () {
            var inputs = $document.find('input:visible:first');
            if (inputs && inputs.length > 0) {
                var elem = inputs[0];
                if (elem) {                    
                    elem.focus();
                }
            }
        };

        /**
         * @name setFocusOnFirstVisibleInputInElement
         * @description sets the focus on first visible input in given element
         *
         * @param {object} element.
         */
        exports.setFocusOnFirstVisibleInputInElement = function (element) {
            if (element) {
                var inputs = element.find('input:visible:first');
                if (inputs && inputs.length > 0) {
                    var elem = inputs[0];
                    if (elem) {
                        elem.focus();
                    }
                }
            }
        };
        
        /**
         * @name requestAnimationFrame
         * @description calls requestAnimationFrame if it is supported, otherwise falls back to a timeout
         *
         * @param {function} callback.
         */
        exports.requestAnimationFrame = function(fn) {
            return raf(fn);
        };

        /**
         * @name updateUrlSetTenantSegment
         * @description Updates the tenant segment in the URL
         *
         * @param {string} tenant name.
         */
        exports.updateUrlSetTenantSegment = function (tenant) {
            var path,
                segments,
                existingTenant,
                update;
            
            if (!tenant) {
                tenant = '';
            }

            // Get the existing path
            path = $location.path();            

            if (path) {
                // Get the existing tenant from the path
                segments = path.split('/');
                if (segments && segments.length > 1) {
                    existingTenant = segments[1];

                    if (existingTenant !== tenant) {
                        segments[1] = tenant;
                        path = segments.join('/');
                        update = true;
                    }
                } else {
                    path = '/' + tenant;
                    update = true;
                }
            } else {
                path = '/' + tenant;
                update = true;
            }

            if (update) {
                $location.path(path);
            }            
        };


        /**
         * @name sanitizeUriComponent
         * @description Sanitizes the URI component by replacing invalid chars with the 
         * specified replacementChar char.
         *
         * @param {string} component The URI component.
         * @param {string} replacementChar The replacement char. Defaults to _.
         */
        exports.sanitizeUriComponent = function (component, replacementChar) {
            var re = /<|>|\*|%|&|\:|\\\\|\?+/g;

            // Look for <,>,*,%,&,:,\\,? and replace them.
            // This list is the default list of invalid request path chars
            // as defined by System.Web Http config.

            if (component) {
                return component.replace(re, replacementChar || '_');
            } else {
                return component;
            }            
        };

        return exports;
    })

    /** 
    * @ngdoc act upon a right click
    * Warning! This is not reliable on android touch.
    */
    .directive('spRightClick', function ($parse) {
        return function (scope, element, attrs) {
            var fn = $parse(attrs.spRightClick);
            element.bind('contextmenu', function (event) {
                scope.$apply(function () {
                    fn(scope, { $scope: scope, $event: event });
                });
            });
        };
    });



   



        

