// Copyright 2011-2016 Global Software Innovation Pty Ltd
// TODO: figure out where this code came from

(function (console) {
    'use strict';

    var i,
        global = this,
        fnProto = Function.prototype,
        fnApply = fnProto.apply,
        fnBind = fnProto.bind,
        bind = function (context, fn) {
            return fnBind ?
                fnBind.call(fn, context) :
                function () {
                    return fnApply.call(fn, context, arguments);
                };
        },
        methods = 'assert count debug dir dirxml error group groupCollapsed groupEnd info log markTimeline profile profileEnd table time timeEnd trace warn'.split(' '),
        emptyFn = function () {
        },
        empty = {},
        timeCounters;

    for (i = methods.length; i--;) { empty[methods[i]] = emptyFn; }

    if (console) {


        if (!console.time) {
            console.timeCounters = timeCounters = {};

            console.time = function (name, reset) {
                if (name) {
                    var time = +new Date(), key = "KEY" + name.toString();
                    if (reset || !timeCounters[key]) { timeCounters[key] = time; }
                }
            };

            console.timeEnd = function (name) {
                var diff,
                    time = +new Date(),
                    key = "KEY" + name.toString(),
                    timeCounter = timeCounters[key];

                if (timeCounter) {
                    diff = time - timeCounter;
                    console.info(name + ": " + diff + "ms");
                    delete timeCounters[key];
                }
                return diff;
            };
        }

        for (i = methods.length; i--;) {
            console[methods[i]] = methods[i] in console ?
                bind(console, console[methods[i]]) : emptyFn;
        }
        console.disable = function () {
            global.console = empty;
        };
        empty.enable = function () {
            global.console = console;
        };

        empty.disable = console.enable = emptyFn;

    } else {
        console = global.console = empty;
        console.disable = console.enable = emptyFn;
    }

    console.addListener = addListener;

    
    //
    // Add a listener to one of the console methods
    //
    function addListener(method, fn) {
        var original = console[method];
        console[method] = function () {

            // Call the original
            if (original.apply) {
                original.apply(console, arguments);
            } else {
                // Looks like we have IE, so we need to act a little differently
                var message = Array.prototype.slice.apply(arguments).join(' ');
                original(message);
            }

            // Call the listener
            fn(arguments);
        };
    }

    
})(typeof console === 'undefined' ? null : console);

