// Copyright 2011-2015 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    angular.module('sp.common.ui.spWatchLimiter', ['mod.common.spCachingCompile', 'mod.common.spEntityService'])
        .directive('spWatchLimiter', function () {
            return {
                restrict: 'A',
                scope: {
                },
                link: function (scope, element) {

                    //
                    // This was another attempt at speeding up Boards. Instead of the sliding window of actual elements, it moves watchers that aren't
                    // currently visible out of consideration for the digest cycle. However, due to the search/filter and dynamic nature of Boards
                    // this started to become very complicated to manage outweighing the potential payoff.
                    //

                    var win = $(window);
                    var timer = null;
                    var debug = true;
                    var keyedWatchers = {};

                    var clearTimer = function() {
                        if (timer) {
                            clearTimeout(timer);
                            timer = null;
                        }
                    };

                    var stopWatching = function(event, key, childScope) {
                        if (!keyedWatchers[key]) {
                            keyedWatchers[key] = childScope.$$watchers;
                            childScope.$$watchers = [];
                        }
                    };

                    var startWatching = function(event, key, childScope) {
                        if (keyedWatchers[key]) {
                            childScope.$$watchers = keyedWatchers[key];
                            delete keyedWatchers[key];
                        }
                    };

                    var isOutsideContainer = function(offset, height, containerOffset, containerHeight) {
                        return offset.top + height <= containerOffset.top || offset.top >= containerOffset.top + containerHeight;
                    };

                    var checkChildScopes = function () {
                        console.log('spWatchLimiter.checkChildScopes');

                        var boardOffset = element.offset();
                        var boardHeight = element.height();
                        var scopes = _.map(element.find('.ng-scope, .ng-isolate-scope'), function (el) {
                            return {
                                scope: angular.element(el).scope(),
                                el: el
                            };
                        });

                        _.each(scopes, function (s) {
                            var el = $(s.el);
                            var offset = el.offset();
                            var height = el.height();

                            if (!el.attr('watch-key')) {
                                el.attr('watch-key', _.uniqueId());
                            }

                            var event = (isOutsideContainer(offset, height, boardOffset, boardHeight) || !el.is(':visible')) ? 'watch-stop' : 'watch-start';

                            if (debug) {
                                if (event === 'watch-stop') {
                                    el.css("background-color", 'rgba(255, 0, 0, 0.05)');
                                } else if (event === 'watch-start') {
                                    el.css("background-color", 'rgba(0, 255, 0, 0.05)');
                                }
                            }

                            element.trigger(event, [el.attr('watch-key'), s.scope]);
                        });
                    };

                    var checkChildScopesDebounced = _.debounce(checkChildScopes, 100);

                    var onScroll = function() {
                        clearTimer();
                        timer = setTimeout(checkChildScopesDebounced, 50);
                    };

                    var onRebuild = function() {
                        console.log('spWatchLimiter.onRebuild');

                        keyedWatchers = null;
                        keyedWatchers = {};
                        checkChildScopesDebounced();
                    };

                    var onRebuildDebounced = _.debounce(onRebuild, 50);

                    scope.$on('sp-rebuild-watch-limits', onRebuildDebounced);

                    scope.$on('$destroy', function () {
                        win.unbind('resize', checkChildScopesDebounced);
                        element.unbind('scroll', onScroll);
                        element.off('watch-stop');
                        element.off('watch-start');
                        clearTimer();
                        keyedWatchers = null;
                    });

                    element.on('watch-stop', stopWatching);
                    element.on('watch-start', startWatching);
                    element.bind('scroll', onScroll);
                    win.bind('resize', checkChildScopesDebounced);
                }
            };
        });
}());