// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular */

(function() {
    'use strict';

    /**
    * Directive implementing drop functionality.
    * spDroppable allows DOM elements being dragged with spDraggable to be dropped on the specified element.
    *
    * @module spDroppable
    * @example
        
    Using the spDroppable:

    &lt;sp-droppable="dropOptions" sp-droppable-data="data" &gt;&lt;/sp-droppable&gt      

    where dropOptions is an object with the following properties:

        -propagateDragEnter {boolean}                                       - Whether the 'dragenter' event should propagate to the parent elements.
                                                                            - (optional) Default 'false'.
        -performDefaultDragEnter {boolean}                                  - Whether the browser should perform the default action for the 'dragenter' event.
                                                                            - (optional) Default 'false'.
        -propagateDragLeave {boolean}                                       - Whether the 'dragleave' event should propagate to the parent elements.
                                                                            - (optional) Default 'false'.
        -performDefaultDragLeave {boolean}                                  - Whether the browser should perform the default action for the 'dragleave' event.
                                                                            - (optional) Default 'false'.
        -propagateDragOver {boolean}                                        - Whether the 'dragover' event should propagate to the parent elements.
                                                                            - (optional) Default 'false'.
        -performDefaultDragOver {boolean}                                   - Whether the browser should perform the default action for the 'dragover' event.
                                                                            - (optional) Default 'false'.
        -propagateDrop {boolean}                                            - Whether the 'drop' event should propagate to the parent elements.
                                                                            - (optional) Default 'false'.
        -performDefaultDrop {boolean}                                       - Whether the browser should perform the default action for the 'drop' event.
                                                                            - (optional) Default 'false'.
        -simpleEventsOnly {boolean}                                         - Whether the events are filtered to only include enter/exit events that occur direectly on the element.
                                                                            - (optional) Default 'false'.
        -supportTouchEvents {boolean}                                       - Whether touch based events should be responded to for mobile/tablet support.
                                                                            - (optional) Default 'false'.

    and the following methods:

        - onDragEnter(event, source, target, dragData, dropData) {function} - Function called when a drag operation enters the specified DOM element where:
            - event {object}                                                - The jQuery event representing the 'dragenter' operation.
            - source {object}                                               - The DOM element that is being dragged.
            - target {object}                                               - The DOM element that the dragged element has entered.
            - dragData {object}                                             - Custom data (optional) specified by the sp-draggable-data attribute on the source DOM element.
            - dropData {object}                                             - Custom data (optional) specified by the sp-droppable-data attribute on the target DOM element.
        - onDragLeave(event, source, target, dragData, dropData) {function} - Function called when a drag operation leaves the specified DOM element where:
            - event {object}                                                - The jQuery event representing the 'dragleave' operation.
            - source {object}                                               - The DOM element that is being dragged.
            - target {object}                                               - The DOM element that the dragged element has left.
            - dragData {object}                                             - Custom data (optional) specified by the sp-draggable-data attribute on the source DOM element.
            - dropData {object}                                             - Custom data (optional) specified by the sp-droppable-data attribute on the target DOM element.
        - onDragOver(event, source, target, dragData, dropData) {function}  - Function called when a drag operation is currently above the specified element where:
            - event {object}                                                - The jQuery event representing the 'dragover' operation.
            - source {object}                                               - The DOM element that is being dragged.
            - target {object}                                               - The DOM element that the dragged element is over.
            - dragData {object}                                             - Custom data (optional) specified by the sp-draggable-data attribute on the source DOM element.
            - dropData {object}                                             - Custom data (optional) specified by the sp-droppable-data attribute on the target DOM element.
        - onDrop(event, source, target, dragData, dropData) {function}      - Function called when a DOM element is dropped where:
            - event {object}                                                - The jQuery event representing the 'drop' operation.
            - source {object}                                               - The DOM element that is being dragged.
            - target {object}                                               - The DOM element that the dragged element was dropped on.
            - dragData {object}                                             - Custom data (optional) specified by the sp-draggable-data attribute on the source DOM element.
            - dropData {object}                                             - Custom data (optional) specified by the sp-droppable-data attribute on the target DOM element.
        - onAllowDrop(source, target, dragData, dropData) {function}        - Function called to determining whether the dragged DOM element can be dropped on the current element where:
            - source {object}                                               - The DOM element that is being dragged.
            - target {object}                                               - The DOM element that the dragged element was dropped on.
            - dragData {object}                                             - Custom data (optional) specified by the sp-draggable-data attribute on the source DOM element.
            - dropData {object}                                             - Custom data (optional) specified by the sp-droppable-data attribute on the target DOM element.
    */
    angular.module('mod.common.ui.spDragDrop')
        .directive('spDroppable', function (spDragDropService, spDragDropTouchService) {
            return {
                restrict: 'A',
                transclude: false,
                replace: false,
                scope: false,
                link: function(scope, element, attrs) {

                    var spDropOptions = {};
                    var spDropData;

                    /////
                    // Raises the event to the specified callback (if one exists)
                    /////
                    function raiseEvent(event, callback, filter, dropEffect, dropData, propagate, performDefault) {
                        var evt;
                        var source;
                        var dragData;
                        var returnValue = true;

                        evt = event.originalEvent;

                        if (evt) {

                            if (!performDefault && event.preventDefault) {
                                event.preventDefault();
                            }

                            if (!propagate && event.stopPropagation) {
                                event.stopPropagation();
                            }

                            source = spDragDropService.getSourceElement();

                            if (!source) {
                                evt.dataTransfer.dropEffect = 'none';
                                //return true;
                            }

                            dragData = spDragDropService.getDragData();

                            if (filter) {
                                if (!filter(source, event.currentTarget, dragData, dropData)) {
                                    evt.dataTransfer.dropEffect = 'none';
                                }
                            }

                            if (dropEffect &&  evt.dataTransfer.dropEffect !== 'none') {
                                dropEffect(evt);
                            }
                        } else {
                            source = null;
                            dragData = null;
                        }

                        if (callback) {
                            returnValue = callback(event, source, event.currentTarget, dragData, dropData);
                        }

                        return returnValue;
                    }

                    attrs.$observe('spDroppable', function () {

                        /////
                        // Get the attributes from the element.
                        /////
                        spDropOptions = scope.$eval(attrs.spDroppable) || {};

                        if (spDropOptions) {

                            /////////////////
                            //
                            // Support for 'touch' events. Wire up touch based events to the html5 drag and drop equivalents.
                            //
                            /////////////////
                            if (spDropOptions.supportTouchEvents === true) {
                                element.on('touchmove', onDropTouchMove);
                                element.on('touchend', onDropTouchEnd);
                            }
                        }
                    });

                    attrs.$observe('spDroppableData', function (val) {
                        if (val)

                            /////
                            // Get the attributes from the element.
                            /////
                            spDropData = scope.$eval(attrs.spDroppableData);
                    });

                    /////
                    // DragEnter event.
                    /////
                    element.on("dragenter", function (event) {

//                        console.log('dragenter');

                        var isSimpleEvent = true;

                        if (spDropOptions.simpleEventsOnly) {
                            isSimpleEvent = spDragDropService.dragEnter(event.currentTarget);
                        }

                        if (isSimpleEvent) {
                            return raiseEvent(event, spDropOptions.onDragEnter, spDropOptions.onAllowDrop, function(evt) {
                                if (evt.dataTransfer) {
                                    evt.dataTransfer.dropEffect = 'copy';
                                }
                            }, spDropData, !!spDropOptions.propagateDragEnter, !!spDropOptions.performDefaultDragEnter);
                        }

                        return !!spDropOptions.propagateDragEnter;
                    });

                    /////
                    // DragLeave event.
                    /////
                    element.on("dragleave", function (event) {

//                        console.log('dragleave');

                        var isSimpleEvent = true;

                        if (spDropOptions.simpleEventsOnly) {
                            isSimpleEvent = spDragDropService.dragLeave(event.currentTarget);
                        }

                        if (isSimpleEvent) {
                            return raiseEvent(event, spDropOptions.onDragLeave, null, null, spDropData, !!spDropOptions.propagateDragLeave, !!spDropOptions.performDefaultDragLeave);
                        }

                        return !!spDropOptions.propagateDragLeave;
                    });

                    /////
                    // DragOver event.
                    /////
                    element.on("dragover", function (event) {

//                        console.log('dragover');

                        return raiseEvent(event, spDropOptions.onDragOver, spDropOptions.onAllowDrop, null, spDropData, !!spDropOptions.propagateDragOver, !!spDropOptions.performDefaultDragOver);
                    });

                    /////
                    // Drop event.
                    /////
                    element.on("drop", function (event) {

//                        console.log('drop');

                        return raiseEvent(event, spDropOptions.onDrop, null, null, spDropData, !!spDropOptions.propagateDrop, !!spDropOptions.performDefaultDrop);
                    });
                    
                    /////
                    // 'TouchMove' event.
                    /////
                    function onDropTouchMove(event) {
                        spDragDropTouchService.dropTouchMove(event);
                    }

                    /////
                    // 'TouchEnd' event.
                    /////
                    function onDropTouchEnd(event) {
                        spDragDropTouchService.dropTouchEnd(event, spDropOptions);
                    }
                }
            };
        });
}());