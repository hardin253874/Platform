// Copyright 2011-2016 Global Software Innovation Pty Ltd
angular.module('mockedNavService', ['ng']);

angular.module('mockedNavService').factory('spNavService', function ($q) {
    var exports = {};

    var api = [
        'isNavigationPending',
        'continueNavigation',
        'cancelNavigation',
        'getNavTree',
        'getCurrentApplicationId',
        'getBreadcrumb',
        'getCurrentItem',
        'getParentItem',
        'getChildHref',
        'getViewHref',
        'navigateToState',
        'navigateToSibling',
        'navigateToChildState',
        'navigateToParent',
        'canNavigateToParent',
        'flattenTree',
        'findInTree',
        'findInTreeById',
        'forEachItemInTree',
        'mergeTreeChild',
        'getThemes',
        'getCurrentTheme',
        'getMenuNodes'
    ];

    _.forEach(api, function (fn) {
        exports[fn] = function () { throw new Error(fn + ' has not been mocked.'); };
    });

    var navItem = {};
    var themes = [];

    // be warned ... this always returns the same, initially empty, object
    // so if you need different then you'll need to use a different mock
    exports.getCurrentItem = function() {
        return navItem;
    };

    exports.getParentItem = function() {
        return null;
    };

    exports.getThemes = function() {
        return themes;
    };

    exports.getCurrentTheme = function() {
        return _.first(themes);
    };

    return exports;
});

