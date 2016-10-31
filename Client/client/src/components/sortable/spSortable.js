// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /**
    * Directive implementing sortable functionality.
    * spSortable allows DOM elements to be reordered by dragging them around.
    *
    * @module spSortable
    * @example
        
    Using spSortable:

    &lt;sp-sortable="sortOptions" &gt;&lt;/sp-sortable&gt      

    where sortOptions is an object containing the jQuery sortable options (http://api.jqueryui.com/sortable/) augmented with the following properties:
        - onSortComplete(event, element, fromIndex, toIndex) {function} - Fired upon sort completion.
            - event {object}                                            - Event raised by jQuery-ui.
            - element {object}                                          - The DOM element being sorted.
            - fromIndex {number}                                        - The index in the array from which this element came.
            - toIndex {number}                                          - The index in the array to which this element went.
    */
    angular.module('mod.common.ui.spSortable', [])
        .directive('spSortable', function() {
            return {
                restrict: 'A',
                transclude: false,
                replace: true,
                scope: false,
                link: function ($scope, $element, $attrs) {
                    var options = null;

                    if ($attrs.spSortable) {

                        options = $scope.$eval($attrs.spSortable);
                    }

                    /////
                    // Don't mark the element as sortable if the option is disabled.
                    /////
                    if (!options || (!options.disabled)) {

                        /////
                        // Mark the element as 'sortable' with regard to jQueryUI.
                        /////
                        $element.sortable(options);

                        if (options && options.onSortComplete) {

                            $element.on("sortdeactivate", function (event, ui) {
                                
                                /////
                                // Stop the event from propagating to the parent containers.
                                /////
                                if (event.stopPropagation) {

                                    event.stopPropagation();
                                } else {

                                    event.cancelBubble = true;
                                }

                                /////
                                // Determine the 'from' and 'to' indexes.
                                /////
                                var fromIndex = angular.element(ui.item).scope().$index;
                                var toIndex = $element.children().index(ui.item);

                                /////
                                // Fire the user-defined callback.
                                /////
                                options.onSortComplete(event, ui, fromIndex, toIndex);
                            });
                        }
                    }
                }
            };
        });
}());