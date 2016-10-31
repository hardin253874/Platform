// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, angular, console, sp */

/**
 * A set of AngularJS services related to navigation.
 * @module navigation
 */

(function () {
    "use strict";

    angular.module('sp.navService')
        .factory('spState', spState);

    /* @ngInject */
    function spState($state, spNavService) {

        var exports = {
            removeScopeProperty: removeScopeProperty,
            registerAction: registerAction,
            getPageState: getPageState,
            getComponentState: getComponentState,
            getParentPageState: getParentPageState
        };

        Object.defineProperties(exports, {
            'name': {
                get: function () {
                    return $state.current.name;
                },
                enumerable: true
            }
        });

        Object.defineProperties(exports, {
            'params': {
                get: function () {
                    return $state.params;
                },
                enumerable: true
            }
        });

        Object.defineProperties(exports, {
            'data': {
                get: function () {
                    return $state.current.data;
                },
                enumerable: true
            }
        });

        Object.defineProperties(exports, {
            'navItem': {
                get: function () {
                    assertCurrentNavItem();
                    return spNavService.getCurrentItem();
                },
                enumerable: true
            }
        });

        /**
         * The scope property represents a state object that prototypically inherits from its parent scope
         * which is the scope of the parent nav item.
         *
         * todo - clean up these concepts; navItem, navItem.data, navItem.scope!!??
         */
        Object.defineProperties(exports, {
            'scope': {
                get: function () {
                    return spNavService.getCurrentItem() && spNavService.getCurrentItem().scope;
                },
                enumerable: true
            }
        });

        return exports;

        /**
         * Use this to remove a property from the scope, looking at parent scopes if needed.
         * Stops after the first it finds.
         * @param propertyName
         * @returns {boolean}
         */
        function removeScopeProperty(propertyName) {
            if (!propertyName) {
                return;
            }

            var scope = spNavService.getCurrentItem() && spNavService.getCurrentItem().scope;

            while (scope) {
                if (scope.hasOwnProperty(propertyName)) {
                    delete scope[propertyName];
                    return true;
                }

                scope = scope.$parent;
            }

            return false;
        }

        /**
         * Register a promise returning action function for something that can be done in the
         * current page/state. The function should typically take a single argument that is
         * an object hash.
         * @param name
         * @param fn
         */
        function registerAction(name, fn) {

            $state.current.data = $state.current.data || {};
            $state.current.data.actionApi = $state.current.data.actionApi || {};
            $state.current.data.actionApi[name] = fn;

            console.log('spState: registerAction for state "' + $state.current.name + '", action "' + name + '" added, now have actions:' + _.keys($state.current.data.actionApi));
        }

        /**
         * Get the page state object as something a page can save and restore its state from.
         * This is the current nav item data object, but we want to hide that fact.
         * This will always return an object.
         */
        function getPageState() {
            ensureNavItemData();
            return spNavService.getCurrentItem().data;
        }

        /**
         * Get the page component state object using a key (string) to identity the
         * component on its page.
         * This will always return an object.
         */
        function getComponentState(key) {
            ensureNavItemData(key);
            return spNavService.getCurrentItem().componentData[key];
        }

        /** Get the parent page state. May return null */
        function getParentPageState() {
            return sp.result(spNavService.getParentItem(), 'data');
        }

        /** Ensure the various members exist on the current nav item */
        function ensureNavItemData(key) {
            assertCurrentNavItem();

            var navItem = spNavService.getCurrentItem();
            navItem.data = navItem.data || {};
            if (key) {
                navItem.componentData = navItem.componentData || {};
                navItem.componentData[key] = navItem.componentData[key] || {};
            }
        }

        function assertCurrentNavItem() {
            console.assert(spNavService.getCurrentItem(), 'Unexpected undefined getCurrentItem');
        }
    }
})();

