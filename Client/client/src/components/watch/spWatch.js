// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, sp */

(function () {
    'use strict';

    /**
    * Module implementing custom watcher functionality.
    * spWatch allows watchers to be created and deleted based on external factors.
    *
    * @module spWatch
    * @example
    * 
    * Example 1 - (childExpression1, childExpression2 and childExpression only run when there is a change to parentExpression (after the initial bind).)
    *   <div sp-watch="parentExpression">
    *       <div sp-watch-text="childExpression1"></div>
    *       <div sp-watch-if="childExpression2"></div>
    *       <div sp-watch-class="childExpression"></div>
    *   </div>
    * 
    * Example 2 - (Support nested expression groups. child1Expression will only run when parent1Expression changes (after the initial bind). child2Expression will only run when parent2Expression changes (after the initial bind).)
    *   <div sp-watch="{name: 'Parent1', expr: 'parent1Expression'}">
    *       <div sp-watch="{name: 'Parent2', expr: 'parent2Expression'}">
    *           <div sp-watch-text="{name: 'Parent1', expr: 'child1Expression'}"></div>
    *           <div sp-watch-text="{name: 'Parent2', expr: 'child2Expression'}"></div>
    *       </div>
    *   </div>
    * 
    *  Example 3 - (childExpression only runs when there is a change to parentExpression (it will NOT run during initial bind).)
    *   <div sp-watch="parentExpression">
    *       <div sp-watch-text="{expr: 'childExpression', bindImmediately: false}"></div>
    *   </div>
    * 
    */

    var spWatch = angular.module('mod.common.misc.spWatch', []);

    /////
    // Create the directives linking them on demand.
    /////
    function createDirective(directive) {

        spWatch.directive(directive.name, function () {
            return {
                restrict: 'A',
                replace: false,
                transclude: false,
                scope: false,
                require: '^spWatch',    // Require a parent sp-watch directive.
                link: function (scope, element, attrs, ctrl) {

                    /////
                    // Name defaults to the controllers unique id.
                    /////
                    var name = ctrl.id;
                    var expr;
                    var bindImmediately = true;

                    var attr = scope.$eval(attrs[directive.name]);


                    if (!attr) {
                        throw '\'' + directive.name + '\' requires a binding expression or an object of type \'{spWatch: "key", expr: "watch-expression"}\'';
                    }

                    if (_.isObject(attr) && _.has(attr, 'expr')) {

                        expr = attr.expr;

                        if (_.has(attr, 'name')) {
                            name = attr.name;
                        }

                        if (_.has(attr, 'bindImmediately')) {
                            bindImmediately = attr.bindImmediately;
                        }
                    } else {
                        expr = attr;
                    }

                    var subscriber = {
                        element: element,
                        binding: directive.binding,
                        expr: expr,
                        scope: scope
                    };

                    registerWatch(ctrl, name, subscriber);

                    if (bindImmediately) {
                        bind(scope, subscriber);
                    }

                    /////
                    // Cleanup.
                    /////
                    scope.$on('$destroy', function () {

                        removeSubscriber(ctrl, name, subscriber);
                    });
                }
            };
        });
    }

    /////
    // Register a watch (may or may not already be registered) and an optional subscriber.
    /////
    function registerWatch(ctrl, watch, subscriber) {

        if (!ctrl || !watch) {
            return;
        }

        ctrl.watchers = ctrl.watchers || {};

        if (!_.has(ctrl.watchers, watch)) {

            /////
            // Watcher not currently registered.
            /////
            ctrl.watchers[watch] = {
                subscribers: []
            };
        }

        /////
        // Add subscriber.
        /////
        if (subscriber) {
            addSubscriber(ctrl, watch, subscriber);
        }
    }

    /////
    // Unregister a watch.
    /////
    function unregisterWatch(ctrl, watch) {

        if (!ctrl || !watch) {
            return;
        }

        if (ctrl.watchers && _.has(ctrl.watchers, watch)) {

            /////
            // Watcher not currently registered.
            /////
            delete ctrl.watchers[watch];
        }
    }

    /////
    // Add a subscriber to the watcher list.
    /////
    function addSubscriber(ctrl, watch, subscriber) {
        if (!ctrl || !watch || !subscriber) {
            return;
        }

        if (ctrl.watchers && _.has(ctrl.watchers, watch) && ctrl.watchers[watch].subscribers) {
            ctrl.watchers[watch].subscribers.push(subscriber);
        }
    }

    /////
    // Remove a subscriber from the watcher list.
    /////
    function removeSubscriber(ctrl, watch, subscriber) {
        if (!ctrl || !watch || !subscriber) {
            return;
        }

        if (ctrl.watchers && _.has(ctrl.watchers, watch) && ctrl.watchers[watch].subscribers) {
            var index = ctrl.watchers[watch].subscribers.indexOf(subscriber);

            if (index !== -1) {
                ctrl.watchers[watch].subscribers.splice(index, 1);
            }
        }
    }

    /////
    // Bind the directive (using the directives specific bind callback).
    /////
    function bind(scope, subscriber) {

        if (!scope || !subscriber) {
            return;
        }

        var targetScope = subscriber.scope || scope;

        var bindingValue = targetScope.$eval(subscriber.expr);
        var canBind = bindingValue !== undefined;

        /////
        // If the expression yielded a value run the directives binding callback.
        /////
        if (canBind) {
            return subscriber.binding(subscriber.element, bindingValue);
        }

        var watcher = subscriber.expr;

        /////
        // Wait for the value to become available.
        /////
        var watcherHandle = scope.$watch(watcher, function (newValue) {
            if (newValue === undefined) {
                return;
            }

            removeWatcher();

            return bind(scope, subscriber);
        });

        function removeWatcher() {
            if (watcherHandle) {
                watcherHandle();
            }
        }

        scope.$on("$destroy", removeWatcher);
    }

    /////
    // Available directives.
    /////
    var directives = [
        {
            name: 'spWatchText',
            binding: function (element, value) {
                element.text(value !== null ? value : "");
            }
        },
        {
            name: 'spWatchHtml',
            binding: function (element, value) {
                element.html(value);
            }
        },
        {
            name: 'spWatchSrc',
            binding: function (element, value) {
                element.attr('src', value);
            }
        },
        {
            name: 'spWatchHref',
            binding: function (element, value) {
                element.attr('href', value);
            }
        },
        {
            name: 'spWatchTitle',
            binding: function (element, value) {
                element.attr('title', value);
            }
        },
        {
            name: 'spWatchAlt',
            binding: function (element, value) {
                element.attr('alt', value);
            }
        },
        {
            name: 'spWatchId',
            binding: function (element, value) {
                element.attr('id', value);
            }
        },
        {
            name: 'spWatchIf',
            binding: function (element, value) {

                if (!this.parentElement) {
                    this.parentElement = element.parent();
                }

                if (!value) {
                    element.remove();
                } else {
                    if (this.parentElement && this.parentElement.length) {
                        this.parentElement.append(element);
                    }
                }
            }
        },
        {
            name: 'spWatchClass',
            binding: function (element, value) {
                if (angular.isObject(value) && !angular.isArray(value)) {
                    var results = [];
                    angular.forEach(value, function (val, index) {
                        if (val) results.push(index);
                    });
                    value = results;
                }
                if (value) {
                    element.addClass(angular.isArray(value) ? value.join(' ') : value);
                }
            }
        },
        {
            name: 'spWatchStyle',
            binding: function (element, value) {
                element.css(value);
            }
        },
        {
            name: 'spWatchShow',
            binding: function (element, value) {
                if (value) {
                    element.css('display', '');
                } else {
                    element.css('display', 'none');
                }
            }
        },
        {
            name: 'spWatchHide',
            binding: function (element, value) {
                if (value) {
                    element.css('display', 'none');
                } else {
                    element.css('display', '');
                }
            }
        }
    ];

    /////
    // Setup the controller to provide cross-directive communication.
    /////
    spWatch.controller('spWatchController', function ($scope, spWatchService) {

        Object.defineProperty(this, 'watchers', {
            get: function () {
                return spWatchService.watchers;
            },
            set: function (newVal) {
                spWatchService.watchers = newVal;
            },
            enumerable: true,
            configurable: true
        });

        /////
        // Assign the controller a unique id.
        /////
        this.id = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8); // jshint ignore:line
            return v.toString(16);
        });
    });

    /////
    // Service to provide cross-controller communication.
    /////
    spWatch.service('spWatchService', function () {
        var exports = {};
        
        exports.watchers = {};

        return exports;
    });

    /////
    // Parent sp-watch directive.
    /////
    spWatch.directive('spWatch', function () {
        return {
            restrict: 'A',
            replace: false,
            transclude: false,
            scope: false,
            controller: 'spWatchController',
            link: function (scope, element, attrs, ctrl) {

                var name = ctrl.id;
                var expr;

                var attr = scope.$eval(attrs.spWatch);

                if (!attr) {
                    throw 'spWatch requires a binding expression or an object of type \'{name: "key", expr: "watch-expression"}\'';
                }

                if (_.isObject(attr) && _.has(attr, 'expr')) {

                    expr = attr.expr;

                    if (_.has(attr, 'name')) {
                        name = attr.name;
                    }

                } else {
                    expr = attr;
                }

                registerWatch(ctrl, name);

                /////
                // Watch the parent expression.
                /////
                var watchHandler = scope.$watch(expr, function (watchVal, oldWatchVal) {

                    if (watchVal === oldWatchVal) {
                        return;
                    }

                    /////
                    // Bind each subscriber.
                    /////
                    angular.forEach(ctrl.watchers[name].subscribers, function (subscriber) {
                        bind(scope, subscriber);
                    });
                });

                /////
                // Cleanup.
                /////
                scope.$on('$destroy', function () {
                    if (watchHandler) {
                        watchHandler();
                    }

                    unregisterWatch(ctrl, name);
                });
            }
        };
    });

    /////
    // Create the directives.
    /////
    angular.forEach(directives, createDirective);

}());