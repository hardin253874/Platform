// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
    * Module containing a caching compile service which caches
    * the link function for a specified html fragment.

    * @module spCachingCompile
    */
    angular.module('mod.common.spCachingCompile', ['ng'])
        .service('spCachingCompile', spCachingCompileService);

    /* @ngInject */
    function spCachingCompileService($compile, $templateCache) {
        var exports = {},
            cache = {};        

        /**
         * Gets the link function using the specified key
         * @param key {string} - the key                     
         * @returns {func} - The compiled link function or null.
         */
        exports.get = function (key) {
            if (!key) {
                return null;
            }

            return cache[key];
        };

        /**
         * Compiles the specified html into a link function and caches it using the specified key
         * @param key {string} - the key            
         * @param html {string} - the html  
         * @returns {func} - The compiled link function.
         */
        exports.compile = function (key, html) {            
            if (!key) {
                return null;
            }

            // Check the cache first
            var linkFunc = cache[key];
            if (!linkFunc) {
                if (!html) {
                    html = $templateCache.get(key);
                    if (!html) {
                        throw 'Template not in $templateCache: ' + key;
                    }
                }
                linkFunc = $compile(html);
                cache[key] = linkFunc;
            }

            return linkFunc;
        };

        return exports;
    }
}());