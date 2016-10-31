// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing a customizable child selector.
    * spHoverChildSelector provides a reusable child selector that does not propagate to any parents.
    *
    * @module spHoverChildSelector
    * @example

    Using the spHoverChildSelector:

    &lt;sp-child-selector="options"&gt;&lt;/sp-child-selector&gt

    where options is an object containing the following values:

    className {string}          -   The class name associated with the selector.
    hoverClassName {string}     -   The class to be applied when hovering.
    childSelector {string}      -   (Optional) Selector used to refine the elements that the hoverClassName will be applied to.
        
    */
    angular.module('mod.common.ui.spHoverChildSelector', [])
        .directive('spHoverChildSelector', function () {
            return {
                restrict: 'A',
                transclude: false,
                replace: true,
                scope: false,
                link: function (scope, element, attrs) {

                    var options = {};

                    attrs.$observe('spHoverChildSelector', function (val) {
                        if (val) {

                            /////
                            // Get the attributes from the element.
                            /////
                            options = scope.$eval(attrs.spHoverChildSelector) || {};

                            if (options && options.className && options.hoverClassName) {
                                element.hover(
                                    function () {
                                        var elm = $(this);

                                        if (options.childSelector) {
                                            elm.parents('.' + options.className).find(options.childSelector).removeClass(options.hoverClassName);
                                            elm.find(options.childSelector).addClass(options.hoverClassName);
                                        } else {
                                            elm.parents('.' + options.className).removeClass(options.hoverClassName);
                                            elm.addClass(options.hoverClassName);
                                        }
                                    },
                                    function () {
                                        var elm = $(this);

                                        if (options.childSelector) {
                                            elm.find(options.childSelector).removeClass(options.hoverClassName);
                                            elm.parents('.' + options.className + ':first').find(options.childSelector).addClass(options.hoverClassName);
                                        } else {
                                            elm.removeClass(options.hoverClassName);
                                            elm.parents('.' + options.className + ':first').addClass(options.hoverClassName);
                                        }
                                    }
                                );
                            }
                        }
                    });
                }
            };
        });
}());