// Copyright 2011-2016 Global Software Innovation Pty Ltd

(function () {
    'use strict';

    /**
     * 
     * A control that replaces a standard checkbox with an icon indicator.
     * 
     **/
    angular.module('app.controls.spIconCheckbox', ['app.controls', 'mod.common.spCachingCompile'])
        .directive('spIconCheckbox', function (spCachingCompile) {

            /////
            // Directive structure.
            /////
            return {
                restrict: 'AE',
                replace: false,
                transclude: false,
                scope: {
                    title: '@title',
                    icon: '@icon',
                    isChecked: '=',
                    isDisabled: '=?',
                    isReadOnly: '=?'
                },
                link: function (scope, element) {

                    var cachedLinkFunc = spCachingCompile.compile('controls/spIconCheckbox/spIconCheckbox.tpl.html');
                    cachedLinkFunc(scope, function (clone) {
                        element.append(clone);
                    });
                }
            };
        });
}());