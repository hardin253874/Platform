// Copyright 2011-2016 Global Software Innovation Pty Ltd
(function () {
    'use strict';

    /**
     * Module implementing a popup stack manager.        
     *
     * @module spPopupStackManager                
     *
     */
    angular.module('mod.common.ui.spPopupStackManager', [])
        .service('spPopupStackManager', function () {            
            var exports = {},
                popups = [];


            /**
            * Pushes the specified element to the top of the stack. 
            * @param {object} element. The html element to push onto the stack.
            */
            exports.pushPopup = function (element) {
                if (!element) {
                    return;
                }

                popups.unshift(element);
            };


            /**
            * Removes the specified element from the stack.               
            * @param {object} element. The html element to pop from the stack.
            */
            exports.popPopup = function (element) {
                if (!element) {
                    return;
                }

                _.remove(popups, function (pu) {
                    return pu === element;
                });                
            };


            /**
            * Returns the element at the top of the stack.  
            * @returns {object} element. The html element at the top of the stack.
            */
            exports.peekPopup = function () {
                return _.first(popups);
            };


            return exports;
        });
}());