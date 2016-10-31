// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function () {
    'use strict';

    /**
    * Directive to handle popover closing when clicking elsewhere on the document.
    * spPopoverAutoClose must be placed on the element that contains the popover attribute.
    *
    * @module spPopoverAutoClose
    * @example
        
    Using the spPopoverAutoClose:

    &lt;span sp-popover-auto-close popover="Hello World" class="my-popover"&gt;&lt;/span&gt

    */

    angular.module('mod.common.ui.spPopover', [])
        .directive('spPopoverAutoClose', [
            '$document', function($document) {
                return {
                    restrict: 'A',
                    transclude: false,
                    replace: false,
                    scope: false,
                    link: function(scope, element, attr) {

                        attr.$observe('popoverOptions', function(val) {
                            if (!_.isUndefined(val)) {
                                scope.popoverOptions = scope.$eval(attr.popoverOptions) || {};
                            }
                        });

                        scope.$watch('popoverOptions.isOpen', function(newVal, oldVal) {

                            if (newVal === oldVal) {
                                return;
                            }

                            if (newVal) {
                                var initialClick = true;

                                /////
                                // Handle clicking on the element.
                                /////
                                $document.bind('click', function(evt) {
                                    scope.$apply(function() {
                                        if (initialClick) {
                                            initialClick = false;
                                        } else {
                                            if (scope.popoverOptions.isOpen) {

                                                /////
                                                // If clicking anywhere in the popover, cancel the event.
                                                /////
                                                if ($(evt.target).parents('.popover').length) {

                                                    evt.stopPropagation();
                                                    return;
                                                }

                                                scope.popoverOptions.isOpen = false;
                                            }

                                            /////
                                            // Remove the click handler.
                                            /////
                                            $document.unbind('click');
                                        }
                                    });
                                });
                            }
                        });
                    }
                };
            }
        ])
        .directive('spPopoverFixedArrow', function() {
            return {
                restrict: 'A',
                transclude: false,
                replace: false,
                scope: false,
                link: function(scope, element) {

                    scope.$watch('tt_isOpen', function(newVal, oldVal) {

                        var arrow;

                        if (newVal === oldVal) {
                            return;
                        }

                        /////
                        // If the popover is open and the arrow has not been fixed in position as yet...
                        /////
                        if (newVal && !scope.tt_isFixed) {
                            arrow = $('.popover .arrow');

                            if (arrow && arrow.length) {

                                /////
                                // Get the popovers bounding rectangle
                                /////
                                var boundingRect = element.get(0).getBoundingClientRect();

                                if (boundingRect.height > 2) {

                                    /////
                                    // Replace the 'top: 50%' with an actual pixel position.
                                    /////
                                    arrow.css('position', 'fixed');
                                    arrow.css('top', Math.floor(boundingRect.height / 2) + boundingRect.top + 'px');
                                    arrow.css('left', boundingRect.right + 2 + 'px');
                                }

                                scope.tt_isFixed = true;
                            }
                        }

                        /////
                        // Remove the fact that the arrow is fixed so that upon scroll a new
                        // position can be calculated next show.
                        /////
                        if (!newVal && scope.tt_isFixed) {
                            scope.tt_isFixed = false;
                            arrow = $('.popover .arrow');

                            if (arrow && arrow.length) {
                                arrow.css('position', '');
                                arrow.css('top', '');
                                arrow.css('left', '');
                            }
                        }
                    });
                }
            };
        })
        .directive('spPopoverAdjustPosition', function ($window, $timeout) {
            return {
                restrict: 'A',
                transclude: false,
                replace: false,
                scope: false,
                link: function (scope, element, attr) {
                   
                    function updatePositionCallback() {
                        if (!scope.popover.isOpen)
                            return;

                        updatePopupPosition();

                        _.delay(updatePositionCallback, 100);
                    }

                    function updatePopupPosition() {
                        // get the popover.
                        var popover = $('.popover');

                        if (!popover || popover.length === 0 || !scope.popover.isOpen)
                            return;

                        var windowHeight = $window.innerHeight;

                        //get popover position
                        var popoverPosition = popover.get(0).getBoundingClientRect();

                        //if top position is less than 0, set to 0
                        var topPosition = popoverPosition.top;
                        if (topPosition < 0) {
                            popover.css('top', '0px');
                            topPosition = 0;
                        }

                        //if bottom is greater than window height, set the  new height
                        var popoverBottom = topPosition + popoverPosition.height;
                        if (popoverBottom > windowHeight) {
                            var newTop = windowHeight - popoverPosition.height;
                            if (newTop < 0) newTop = 0;
                            popover.css('top', newTop + 'px');
                        }
                    }

                    scope.$watch('popover.isOpen', function (newVal, oldVal) {
                       
                        if (newVal === oldVal) {
                            return;
                        }

                        if (!newVal) {
                            return;
                        }                                           
                        updatePositionCallback();          
                    });
                }
            };
        })
        .directive('spPopoverStayOnScreen', function($window) {
            return {
                restrict: 'A',
                transclude: false,
                replace: false,
                scope: false,
                link: function(scope) {

                    scope.$watch('popoverOptions.isOpen', function (newVal, oldVal) {

                        if (newVal === oldVal) {
                            return;
                        }

                        if (newVal !== true) {
                            return;
                        }

                        var popover = $('.popover');

                        var windowHeight = $window.innerHeight;
                        var popoverPosition = popover.get(0).getBoundingClientRect();
                        var popoverBottom = popoverPosition.top + popoverPosition.height;

                        if (popoverBottom > windowHeight) {
                            var newTop = windowHeight - popoverPosition.height;
                            if (newTop < 0) newTop = 0;
                            popover.css('top', newTop + 'px');
                        }
                    });
                }
            };
        });

}());