// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console */

/**
 *  An AngularJS service exposing the HTML5 localService object and methods.
 *
 * To Do:
 * While we are only supporting the level of browsers that do support localStorage, maybe
 * consider adding implementation for when localStorage isn't available, like when running
 * from file URL with IE9.
 *
 * {@link http://diveintohtml5.info/storage.html}
 *
 * @module spLocalStorage
 */

angular.module('mod.common.spLocalStorage', [])
    .factory('spLocalStorage', function ($window) {
        "use strict";

        var exports = {};

        var localStorageAvailable;
        var sessionStorageAvailable;

        testAllStorages();

        var storage = localStorageAvailable ? $window.localStorage : (sessionStorageAvailable ? $window.sessionStorage : createDummyStore());


        function testAllStorages() {
            try {
                localStorageAvailable = testStorage($window.localStorage);
            } catch (e) { }

            try {
                sessionStorageAvailable = testStorage($window.sessionStorage);
            } catch (e) { }
        }

        function testStorage(store)
        {
            var v = 'ignorethis';
            try {
                store.setItem(v, v);
                store.removeItem(v);
                return true;
            } catch(e) {
                return false;
            }
        }

        function createDummyStore() {
            var store = [];
            return {
                getItem: function (key) { return store[key]; },
                setItem: function (key, value) { store[key] = value; },
                removeItem: function (key) { store[key] = undefined; },
                clear: function () { store.length = 0; }
            };
        }

        exports.testLocalStorage = function() { return localStorageAvailable; };

        /**
         * Retrieves an item from local storage.
         * @param {string} key The key or name the value to lookup.
         * @returns {string} The value stored against the given key, if one.
         */
        exports.getItem = function(key) {
            return storage.getItem(key);
        };


        /**
         * Stores an item into local storage.
         * @param {string} key The key or name the value to lookup.
         * @param {string} value The value to save against the given key.
         */
        exports.setItem = function(key, value) {
            return storage.setItem(key, value);
        };


        /**
         * Removes an item from local storage.
         * @param {string} key The key or name of the item to remove.
         */
        exports.removeItem = function(key) {
            storage.removeItem(key);
        };


        /**
         * Retrieves an object from local storage.
         * @param {string} key The key or name the value to lookup.
         * @returns {object} The value stored against the given key, if one.
         */
        exports.getObject = function(key) {
            
            // Recovering from a possible stray setting of undefined
            if (storage.getItem(key) === 'undefined') {
                storage.removeItem(key);
            }

            var value = JSON.parse(storage.getItem(key) || 'null');
           
            return value;
        };

        /**
         * Stores an object into local storage.
         * @param {string} key The key or name the value to lookup.
         * @param {object} value The value to save against the given key.
         */
        exports.setObject = function(key, value) {
            return this.setItem(key, value ? JSON.stringify(value) : 'null');
        };

        /**
         * Clear all local storage.
         */
        exports.clear = function () {
            storage.clear();
        };

        return exports;

    });