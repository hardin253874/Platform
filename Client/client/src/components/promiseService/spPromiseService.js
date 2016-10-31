// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

(function () {
    "use strict";

    angular.module('mod.services.promiseService', [])
        .factory('spPromiseService', function ($q, $rootScope, $timeout) {

            var watches = {};
            var nextId = 0;
            var exports = {};

            // Add ourselves to the $rootScope so we can watch stuff if needed.
            $rootScope.spPromiseService = exports;

            // internal, but exposed so we can watch it
            exports.__watches = watches;

            /**
             * Return a promise that will resolve when the given prop on the given object
             * passes the isInitialised predicate.
             * @todo - add timeouts
             */
            exports.when = function (obj, prop, isInitialised) {
                var value, id, watch, valueFn;

                valueFn = _.partial(sp.result, obj, prop);

                // default to the 'value' property or fn
                prop = prop || 'value';

                // default the initialised test to whether is undefined or not
                isInitialised = isInitialised || sp.negate(_.isUndefined);

                // if already have a value then resolve it immediately

                value = valueFn();
                if (isInitialised(value, obj, prop)) {
                    return $q.when(value);
                }

                // set up a watch and resolve when it gets a value
                id = (nextId += 1);
                watch = {
                    prop: prop, // here only for debug logging
                    valueFn: valueFn,
                    deferred: $q.defer(),
                    cancelWatch: $rootScope.$watch('spPromiseService.__watches[' + id + '].valueFn()', function (value, prev) {

//                        console.log('spPromiseService.when, watch fired for id=%o, watch=%o => value=%o', id, watch,
//                            _.cloneDeep(value), _.cloneDeep(prev),
//                            _.isArray(value) ? value.length : 'not an array');

                        if (isInitialised(value, obj, prop)) {
                            watch.deferred.resolve(value);
                            watch.cancelWatch();
                            delete watches[id];
//                            console.log('spPromiseService: deleted watch, watches now', _.cloneDeep(watches));
                        }
                    })
                };
                watches[id] = watch;
                return watch.deferred.promise;
            };

            /**
             * Return a promise that resolves once the promise returning function fn
             * returns a result that passes the given predicate. The function will be
             * called up to the number of retries with a timeout based wait in between.
             * @param fn - the promise returning function
             * @param pred - a predicate function (returns bool) taking the resolved value of fn
             * @param retries - the max number of times fn will be executed
             * @param wait - the timeout in msec between each run of fn
             * @param backoff - the amount to back-off on each poll. 1 means no back-off. 2 means wait twice as long on each poll.
             * @param timeoutfn - needed as $timeout isn't working in intg tests so let test provide own
             * @returns promise with the resolved result of the final call to the fn, whether the predicate passed
             *          or we exceeded the retry count
             */
            exports.poll = function poll(fn, pred, retries, wait, backoff, timeoutfn) {
                timeoutfn = timeoutfn || $timeout;
                backoff = backoff || 1.0 ;              // default to no back-off

                return fn().then(function (result) {
                    if (!pred(result) && retries > 0) {
                        return timeoutfn(_.partial(poll, fn, pred, retries - 1, wait * backoff, backoff, timeoutfn), wait || 500);
                    }
                    return result;
                });
            };

            return exports;
        });
}());
