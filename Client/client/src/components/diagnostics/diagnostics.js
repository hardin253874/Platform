// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

var ChangedWatch = function (expression, func, currentValue, lastValue, duration) {

    this.expression = expression;
    this.function = func;
    this.value = currentValue;
    this.lastValue = lastValue;
    this.duration = duration;
};

var Watch = function (expression, func, currentValue, lastValue, duration) {

    this.expression = expression;
    this.function = func;
    this.value = currentValue;
    this.lastValue = lastValue;
    this.duration = duration;
};

var WatchData = function (scope, watcher, duration, element) {

    this.scope = scope;
    this.watcher = watcher;
    this.duration = duration;
    this.element = element;
};

Object.defineProperty(WatchData.prototype, 'name', {
    get: function () {
        if (_.isString(this.watcher.exp)) {
            return this.watcher.exp;
        } else if (_.isFunction(this.watcher.exp)) {
            if (this.watcher.exp.exp) {
                return this.watcher.exp.exp;
            } else if (this.watcher.exp.name) {
                return this.watcher.exp.name;
            } else if (_.isFunction(this.watcher.fn)) {
                if (this.watcher.fn.name) {
                    return this.watcher.fn.name;
                }
            }
        }

        return 'Unknown';
    },
    enumerable: true,
    configurable: true
});

(function () {
    'use strict';

    angular.module('mod.diagnostics', ['mod.common.spEntityService', 'mod.common.spReportDataCacheService'])
        .directive('spDiagnostics', function ($rootScope, $timeout, spEntityService, spReportDataCacheService) {
            return {
                restrict: 'E',
                transclude: false,
                replace: true,
                templateUrl: 'diagnostics/diagnostics.tpl.html',
                scope: {
                },
                link: function (scope, element) {
                    $(element).draggable();

                    /////
                    // Default refresh interval.
                    /////
                    var refreshInterval = 1000;
                    scope.dumpedWatchers = [];

                    scope.instrument = false;
                    scope.scopeCount = '...';
                    scope.watcherCount = '...';
                    scope.cycleTime = '...';
                    scope.totalDisplayed = 5;
                    scope.showDump = false;
                    scope.lastSelectedWatcher = null;
                    scope.lastSelectedWatcherBorder = null;
                    scope.spEntityService = spEntityService;
                    scope.spReportDataCacheService = spReportDataCacheService;

                    scope.digestRate = '...';
                    var digestRateCount = 0;
                    var digestRateStart;

                    /////
                    // Dump the current watchers.
                    /////
                    scope.dump = function() {
                        var retVal = scan(true, true);

                        scope.dumpedWatchers = retVal.dumped.splice(0);
                        scope.showDump = true;
                    };

                    /////
                    // Clear the dumped watchers.
                    /////
                    scope.clear = function () {
                        scope.dumpedWatchers = [];
                        scope.showDump = false;
                        restoreBorder();
                    };
                    
                    /////
                    // Restore the border of the element with the watcher.
                    /////
                    function restoreBorder() {
                        if (scope.lastSelectedWatcher) {
                            scope.lastSelectedWatcher.element.css('border', scope.lastSelectedWatcherBorder);
                        }
                    }

                    /////
                    // Hover over a dumped watcher.
                    /////
                    scope.watcherHover = function (dumpedWatcher) {
                        if (!dumpedWatcher || dumpedWatcher === scope.lastSelectedWatcher) {
                            return;
                        }

                        restoreBorder();

                        if (dumpedWatcher.element) {
                            scope.lastSelectedWatcherBorder = dumpedWatcher.element.css('border');
                            scope.lastSelectedWatcher = dumpedWatcher;

                            dumpedWatcher.element.css('border', '2px solid red');
                        }
                    };

                    /////
                    // Get the watcher tooltip.
                    /////
                    scope.getTooltip = function (dumpedWatcher) {
                        if (_.isObject(dumpedWatcher.watcher.last)) {
                            return 'Last Value: ' + JSON.stringify(dumpedWatcher.watcher.last);
                        } else if (_.isString(dumpedWatcher.watcher.last)) {
                            return 'Last Value: \"' + dumpedWatcher.watcher.last.trim() + '\"';
                        } else {
                            return 'Last Value: ' + dumpedWatcher.watcher.last;
                    }
                    };

                    /////
                    // Clears cached report data from local storage.
                    /////
                    scope.clearReportData = function() {
                        spReportDataCacheService.clearData();
                    };

                    /////
                    // Run the watcher.
                    /////
                    function runWatch(current, watcher, el, dumped, dump) {

                        var startTime = new Date();

                        var value = watcher.origGet(current);

                        var endTime = new Date();

                        /////
                        // time difference in ms
                        /////
                        var duration = endTime - startTime;

                        if (value !== watcher.last) {
                            if (!angular.equals(value, watcher.last) && !(typeof value === 'number' && typeof watcher.last === 'number' && isNaN(value) && isNaN(watcher.last))) {
                                console.log(new ChangedWatch(watcher.origGet, watcher.fn, value, watcher.last, duration));
                            }
                        }

                        if (duration > 100) {
                            console.error('Watcher took', duration, 'ms to run.', new Watch(watcher.origGet, watcher.fn, value, watcher.last, duration));
                        } else if (duration > 10) {
                            console.warn('Watcher took', duration, 'ms to run.', new Watch(watcher.origGet, watcher.fn, value, watcher.last, duration));
                        }

                        if (dump) {
                            var watchData = new WatchData(current, watcher, duration, el);
                            dumped.push(watchData);
                        }

                        return value;
                    }

                    /////
                    // Scan for watchers.
                    /////
                    function scan(instrument, dump) {
                        var root = $(document.getElementsByTagName('body'));
                        var watchers = [];
                        var scopes = [];
                        var dumped = [];

                        runScan(root, watchers, scopes, dumped, instrument, dump);

                        return {
                            root: root,
                            watchers: watchers,
                            scopes: scopes,
                            dumped: dumped
                        };
                    }

                    /////
                    // Run the scan with the default values.
                    /////
                    function runScan(el, watchers, scopes, dumped, instrument, dump) {
                        
                        var elScope;
                        if (el.data().hasOwnProperty('$scope') || el.data().hasOwnProperty('$isolateScope')) {
                            elScope = el.data().$scope || el.data().$isolateScope;
                            if (scopes.indexOf(elScope) === -1) {
                                scopes.push(elScope);
                            }
                            angular.forEach(elScope.$$watchers, function (watcher) {
                                if (instrument) {
                                    if (!watcher.hasOwnProperty('origGet')) {
                                        watcher.origGet = watcher.get;
                                        watcher.get = function (current) {
                                            return runWatch(current, watcher, el, dumped, dump);
                                        };
                                    }

                                    if (dump) {
                                        runWatch(elScope, watcher, el, dumped, dump);
                                    }
                                } else {
                                    if (watcher.origGet) {
                                        watcher.get = watcher.origGet;
                                        delete watcher.origGet;
                                    }
                                }

                                if (watchers.indexOf(watcher) === -1) {
                                    watchers.push(watcher);
                                }
                            });
                        }

                        angular.forEach(el.children(), function (childElement) {
                            runScan($(childElement), watchers, scopes, dumped, instrument, dump);
                        });
                    }

                    /////
                    // Refresh the counters.
                    /////
                    function refresh() {

                        try {
                            var retVal = scan(!!scope.instrument, false);

                            /////
                            // record start time
                            /////
                            var startTime = new Date();

                            $rootScope.$apply();

                            /////
                            // later record end time
                            /////
                            var endTime = new Date();

                            /////
                            // time difference in ms
                            /////
                            var duration = endTime - startTime;

                            scope.scopeCount = retVal.scopes.length;
                            scope.watcherCount = retVal.watchers.length;
                            scope.cycleTime = duration;

                            /////
                            // digest count 
                            ////
                            var timeSinceLastRate = startTime - digestRateStart;
                            var rate = Math.floor(1000 * (digestRateCount - 1) / timeSinceLastRate); // ignore the interval watch
                            scope.digestRate = (rate + ' ' + scope.digestRate).substring(0,20);

                            resetDigestRateCounter();


                        } catch(error) {
                            console.log('Error running diagnostics refresh');
                        }

                        if (element.is(":visible")) {
                            $timeout(refresh, refreshInterval);
                        }
                    }

                    /////
                    // Add digest count
                    /////
                    var unhookDigestRateCounter = function () { }; // initially do nothing - this variable function gets replaced.

                    function hookDigestRateCounter() {
                        resetDigestRateCounter();
                        unhookDigestRateCounter = scope.$watch(function () { digestRateCount++; });
                    }

                    function resetDigestRateCounter() {
                        digestRateCount = 0;
                        digestRateStart = new Date();
                    }
                    /////
                    // Visibility handler.
                    /////
                    function isVisible() {

                        if (element.is(":visible")) {
                            hookDigestRateCounter(); 
                            refresh();
                        } else {
                            restoreBorder();
                            unhookDigestRateCounter();
                        }
                    }


                    /////
                    // Bind to the isVisible pseudo event.
                    /////
                    element.bind('isVisible', isVisible);
                }
            };
        });
}());