// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, sp */

(function() {
    'use strict';

    //debug
    function logMouseEvent(message, event) {
        var props = _.flatten(_.map(['target.tagName', 'currentTarget.tagName', 'originalEvent.clientX', 'originalEvent.clientY'], function (p) {
            return ['\n', p, sp.result(event, p)];
        }));
        console.log([message].concat(props).join());
        console.log(message, event);
    }


    /**
    * Directive implementing drag functionality.
    * spDraggable allows DOM elements to be dragged around the browser.
    *
    * @module spDraggable
    * @example
        
    Using the spDraggable:

    &lt;sp-draggable="dragOptions" sp-draggable-data="data" &gt;&lt;/sp-draggable&gt      

    where dragOptions is an object with the following properties:
        - supportTouchEvents {boolean}          - Whether touch based events should be responded to for mobile/tablet support
        - dragImage {object}                    - The glyph that is dragged under the cursor with the following properties
            - url {string}                      - The url of the glyph to display
            - width {number}                    - The width of the glyph to display
            - height {number}                   - The height of the glyph to display
        - onDragStart(event, data) {function}   - Function called when a drag operation has begun where:
            - event {object}                    - The jQuery event representing the 'dragstart' operation.
            - data {object}                     - Custom data (optional) specified by the sp-draggable-data attribute.
        - onDrag(event, data) {function}        - Function called when a drag operation is in progress where:
            - event {object}                    - The jQuery event representing the 'drag' operation.
            - data {object}                     - Custom data (optional) specified by the sp-draggable-data attribute.
        - onDragEnd(event, data) {function}     - Function called when a drag operation completes where:
            - event {object}                    - The jQuery event representing the 'dragend' operation.
            - data {object}                     - Custom data (optional) specified by the sp-draggable-data attribute.
    */
    angular.module('mod.common.ui.spDragDrop')
        .directive('spDraggable', function (spDragDropService, spDragDropTouchService) {
            return {
                restrict: 'A',
                transclude: false,
                replace: true,
                scope: false,
                link: function(scope, element, attrs) {

                    var spDragOptions = {};
                    var spDragData;

                    attrs.$observe('spDraggable', function (val) {
                        if (!_.isUndefined(val)) {

                            /////
                            // Get the attributes from the element.
                            /////
                            spDragOptions = scope.$eval(attrs.spDraggable) || {};

                            /////
                            // Pre-cache the drag image.
                            /////
                            if (spDragOptions.dragImage && spDragOptions.dragImage.url) {
                                var dragImage = document.createElement('img');

                                dragImage.src = spDragOptions.dragImage.url;

                                if (spDragOptions.dragImage.width) {
                                    dragImage.width = spDragOptions.dragImage.width;
                                }

                                if (spDragOptions.dragImage.height) {
                                    dragImage.height = spDragOptions.dragImage.height;
                                }

                                spDragOptions.dragImage.element = dragImage;
                            }

                            /////
                            // 'Drag' event.
                            /////
                            if (spDragOptions.onDrag) {
                                element.on('drag', function (event) {
//                                    console.log('drag');
                                    spDragOptions.onDrag(event, spDragData);
                                });
                            }


                            if (spDragOptions) {

                                /////////////////
                                //
                                // Support for 'touch' events. Wire up touch based events to the html5 drag and drop equivalents.
                                //
                                /////////////////
                                if (spDragOptions.supportTouchEvents === true) {
                                    element.on('touchstart', onDragTouchStart);
                                    element.on('touchmove', onDragTouchMove);
                                    element.on('touchend', onDragTouchEnd);
                                    element.on('touchcancel', onDragTouchCancel);
                                }
                            }
                        }
                    });

                    attrs.$observe('spDraggableData', function (val) {
                        if (val) {

                            /////
                            // Get the attributes from the element.
                            /////
                            spDragData = scope.$eval(attrs.spDraggableData);
                        }
                    });

                    element.attr('draggable', 'true');

                    /////
                    // 'DragStart' event.
                    /////
                    element.on('dragstart', function (event) {

//                        logMouseEvent('dragstart', event);

                        var evt = event.originalEvent;

                        event.stopPropagation();

                        spDragDropService.setSourceElement(element);

                        if (evt && evt.dataTransfer) {

                            evt.dataTransfer.effectAllowed = 'copy';

                            /////
                            // The following line *MUST* be present so that FireFox can support drag and drop.
                            /////
                            evt.dataTransfer.setData('text', 'softwarePlatform');

                            // The test for evt.dataTransfer.setDragImage is because IE doesn't support it

                            if (spDragOptions.dragImage && spDragOptions.dragImage.element && evt.dataTransfer.setDragImage) {
                                evt.dataTransfer.setDragImage(spDragOptions.dragImage.element, 0, 0);
                            }
                        }

                        if (spDragData) {
                            spDragDropService.setDragData(spDragData);
                        }

                        if (spDragOptions.onDragStart) {
                            spDragOptions.onDragStart(event, spDragData);
                        }

                        spDragDropService.startDragOperation();
                    });

                    /////
                    // 'DragEnd' event.
                    /////
                    element.on('dragend', function (event) {

//                        console.log('dragend');


                        event.stopPropagation();

                        if (spDragOptions && spDragOptions.onDragEnd) {
                            spDragOptions.onDragEnd(event, spDragData);
                        }

                        spDragDropService.endDragOperation();
                        spDragDropService.clearDragData();
                        spDragDropService.clearSourceElement();
                    });

                    /////
                    // 'TouchStart' event.
                    /////
                    function onDragTouchStart(event) {
                        spDragDropTouchService.dragTouchStart(event, element[0], spDragOptions);
                    }

                    /////
                    // 'TouchMove' event.
                    /////
                    function onDragTouchMove(event) {
                        spDragDropTouchService.dragTouchMove(event);
                    }

                    /////
                    // 'TouchEnd' event.
                    /////
                    function onDragTouchEnd(event) {
                        spDragDropTouchService.dragTouchEnd(event, element[0]);
                    }

                    /////
                    // 'TouchCancel' event.
                    /////
                    function onDragTouchCancel(event) {
                        spDragDropTouchService.dragTouchEnd(event, element[0], true);
                    }
                }
            };
        });
}());