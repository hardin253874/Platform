// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Module implementing an automatic resizing input control.
    * spAutoSizeInput resizes its width based on the text contained within.
    *
    * @module spAutoSizeInput
    * @example

    Using the spAutoSizeInput:

    &lt;input type="text" ng-model="myModel" sp-auto-size-input /&gt;

    where myModel is the value being bound into the text control.
    */
    angular.module('app.controls.spAutoSizeInput', [])
        .directive('spAutoSizeInput', function ($document) {
            return {
                restrict: 'A',
                require: 'ngModel',
                link: function (scope, element, attrs, ctrl) {
                    var placeholder, span, resize;

                    placeholder = element.attr('placeholder') || '';

                    span = angular.element('<span></span>');
                    span[0].style.cssText = getComputedStyle(element[0]).cssText;
                    span.css('display', 'none')
                        .css('visibility', 'hidden')
                        .css('width', 'auto');

                    $document.find('body').append(span);

                    resize = function (value) {
                        if (value && value.length < placeholder.length) {
                            value = placeholder;
                        }
                        span.text(value);
                        span.css('display', '');
                        try {
                            element.css('width', span.prop('offsetWidth') + 'px');
                        }
                        finally {
                            span.css('display', 'none');
                        }
                    };

                    ctrl.$parsers.unshift(function (value) {
                        resize(value);
                        return value;
                    });

                    ctrl.$formatters.unshift(function(value) {
                        resize(value);
                        return value;
                    });
                }
            };
        });
}());