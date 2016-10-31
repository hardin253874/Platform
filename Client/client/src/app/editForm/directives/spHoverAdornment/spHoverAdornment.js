// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    angular.module('mod.app.editForm.designerDirectives.spHoverAdornment', [])
        .directive('spHoverAdornment', function () {

            return {
                restrict: 'E',
                transclude: true,
                replace: true,
                templateUrl: function ($element, $attrs) {
                    if ($attrs.src) {
                        return $attrs.src;
                    }

                    return null;
                },
                link: function ($scope, $element, $attrs, $ctrl) {

                    var adornment;
                    var targetElement = null;
                    var applyTo;

                    if ($attrs.applyToParent) {
                        applyTo = $attrs.applyToParent;
                    }

                    adornment = $element;

                    if (applyTo) {
                        targetElement = $element.closest(applyTo);

                    }

                    if (!targetElement) {
                        targetElement = adornment;
                    }

                    if (adornment) {
                        $element.mouseover(
                            function (e) {
                                e.stopPropagation();

                                targetElement.addClass("selected-adornment");

                                adornment.children(".adornment-content").css("opacity", "1");
                                adornment.children(".ui-resizable-handle").css("opacity", "1");

                            }).mouseout(
                                function () {

                                    targetElement.removeClass("selected-adornment");

                                    adornment.children(".adornment-content").css("opacity", "0");
                                    adornment.children(".ui-resizable-handle").css("opacity", "0");
                                });
                    }
                }
            };
        });
}());