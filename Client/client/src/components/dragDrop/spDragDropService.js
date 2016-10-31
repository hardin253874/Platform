// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global console, _, angular, sp */

(function() {
    'use strict';

    /**
    * Service providing communication between spDraggable and spDroppable DOM elements.
    * spDragDropService allows DOM elements being dragged with spDraggable to pass custom data to DOM drop elements.
    *
    * @module spDragDropService
    */
    angular.module('mod.common.ui.spDragDrop', ['mod.common.spUuidService'])
        .service('spDragDropService', function() {
            var exports = {};
            var depthMonitor;

            /**
             * Stores the specified drag data.
             * @param {Object} data - Data to be stored.
             */
            exports.setDragData = function(data) {
                exports.dragData = data;
            };

            /**
             * Retrieves the drag data.
             */
            exports.getDragData = function() {
                return exports.dragData;
            };

            /**
             * Clears the drag data from the service.
             */
            exports.clearDragData = function() {
                delete exports.dragData;
            };

            /**
             * Stores the source element.
             * @param {object} element - Source element to be stored.
             */
            exports.setSourceElement = function (element) {
                exports.sourceElement = element;
            };

            /**
             * Retrieves the source element.
             */
            exports.getSourceElement = function () {
                return exports.sourceElement;
            };

            /**
             * Clears the source element from the service.
             */
            exports.clearSourceElement = function () {
                delete exports.sourceElement;
            };

            /**
             * Starts the drag operation.
             */
            exports.startDragOperation = function() {
                depthMonitor = {};
            };

            /**
             * Ends the drag operation.
             */
            exports.endDragOperation = function() {
                depthMonitor = {};
            };

            /**
             * Drag enter operation.
             */
            exports.dragEnter = function (target) {
                if (!depthMonitor) return false;

                var existingTarget = depthMonitor[target.id];

                if (!existingTarget) {
                    existingTarget = 1;
                } else {
                    existingTarget = existingTarget + 1;
                }

                depthMonitor[target.id] = existingTarget;

                return existingTarget === 1;
            };

            /**
             * Drag leave operation.
             */
            exports.dragLeave = function (target) {
                if (!depthMonitor) return false;

                var existingTarget = depthMonitor[target.id];

                if (!existingTarget) {
                    console.log('no existing entry');
                } else {
                    existingTarget = existingTarget - 1;
                }

                if (existingTarget === 0) {
                    delete depthMonitor[target.id];
                } else {
                    depthMonitor[target.id] = existingTarget;
                }

                return existingTarget === 0;
            };

            return exports;
        })
        .factory('spDragDropTouchService', function ($timeout, spDragDropService) {
            var exports = {};

            // for mozilla drag and drop events
            var dragData = {};

            // dataTransfer object
            var dataTransfer = {
                getData: function(type) {
                    return dragData[type];
                }.bind(this)
            };

            // dataTransfer object for the 'dragstart' event
            var dataTransferStart = {
                setData: function(type, val) {
                    dragData[type] = val;
                    return val;
                }.bind(this),
                dropEffect: "move"
            };

            // contest between dragend and drop
            var dropping = false;

            // the element being dragged as a visual indicator
            var dragging = null;

            // offset information about the position of the element being dragged
            var offset = null;

            // a timer to introduce a 'hold' affect before beginning a drag operation
            var timer = null;

            // tracks the potential drop targets from the last event
            var currentDropTargets = [];

            /////
            // Creates a drag / drop event appropriate for the requested type.
            /////
            function createDragDropEvent(type) {
                var dndEvent = document.createEvent("Event");
                dndEvent.initEvent(type, true, true);
                if (type !== "dragend") {
                    if (type !== "dragstart") {
                        dndEvent.dataTransfer = dataTransfer;
                    } else {
                        dndEvent.dataTransfer = dataTransferStart;
                    }
                }
                return dndEvent;
            }

            /////
            // Cancels and clears the timer used to simulate the 'hold' affect prior to drag.
            /////
            function clearDragTimer() {
                if (timer) {
                    clearTimeout(timer);
                    timer = null;
                }
            }

            /////
            // Creates a visual indicator to communicate the element and information being dragged.
            /////
            function createIndicator(event, el, dragOptions) {
                var evt = event.originalEvent || event;
                
                if (dragOptions && dragOptions.dragImage && dragOptions.dragImage.element) {

                    var imgWidth, imgHeight;
                    var offsetX = 0;
                    var offsetY = 0;

                    if (dragOptions.dragImage.width) {
                        imgWidth = dragOptions.dragImage.width;
                        if (imgWidth) {
                            offsetX = imgWidth / 2;
                        }
                    }

                    if (dragOptions.dragImage.height) {
                        imgHeight = dragOptions.dragImage.height;
                        if (imgHeight) {
                            offsetY = imgHeight / 2;
                        }
                    }

                    offset = {
                        originalX: evt.changedTouches[0].pageX,
                        originalY: evt.changedTouches[0].pageY,
                        x: offsetX,
                        y: offsetY
                    };

                    dragging = $(document.createElement('div'));

                    var img = $(dragOptions.dragImage.element).clone();
                    dragging[0].appendChild(img[0]);

                    dragging.css({
                        position: 'absolute',
                        width: imgWidth || 'auto',
                        height: imgHeight || 'auto',
                        left: evt.changedTouches[0].pageX - offsetX,
                        top: evt.changedTouches[0].pageY - offsetY,
                        'pointer-events': 'none'
                    });
                } else {

                    var rect = el.getBoundingClientRect();

                    offset = {
                        originalX: rect.left,
                        originalY: rect.top,
                        x: evt.changedTouches[0].pageX - rect.left,
                        y: evt.changedTouches[0].pageY - rect.top
                    };

                    dragging = $(el).clone();
                    dragging.addClass('dragging');
                    dragging.css({
                        width: rect.width,
                        height: rect.height,
                        left: rect.left,
                        top: rect.top
                    });
                }

                document.body.appendChild(dragging[0]);
            }

            /////
            // Updates the visual indicator of the drag to the latest touch position.
            /////
            function moveIndicator(event) {
                var evt = event.originalEvent || event;

                if (!evt) {
                    return;
                }

                if (!offset || !dragging) {
                    return;
                }

                dragging.css({
                    top: evt.changedTouches[0].pageY - offset.y,
                    left: evt.changedTouches[0].pageX - offset.x
                });
            }

            /////
            // Destroys the visual indicator.
            /////
            function removeIndicator() {
                if (dragging) {
                    dragging.remove();
                    dragging = null;
                }
            }

            /////
            // Indicates a valid drag operation is taking place.
            /////
            function isDragging() {

                var source = spDragDropService.getSourceElement();
                if (!source) {
                    return false;
                }

                // ignore if we haven't calced the offset or have a dragging element
                if (!offset || !dragging) {
                    return false;
                }

                return true;
            }

            /////
            // Cleans up the internals relevant to a dragging.
            /////
            function cleanUpDrag(cancel) {
                
                clearDragTimer();

                if (cancel === true) {
                    removeIndicator();
                } else {
                    if (dragging) {

                        // snap-back
                        dragging.css({
                            top: offset.originalY,
                            left: offset.originalX,
                            opacity: 0,
                            transition: 'all 0.5s',
                            '-webkit-transition': 'all 0.5s'
                        });

                        $timeout(removeIndicator, 500);
                    }
                }

                dragData = {};
            }

            /////
            // Cleans up the internals relevant to a dropping.
            /////
            function cleanUpDrop() {

                removeIndicator();

                dragData = {};

                currentDropTargets.length = 0;
            }

            /////
            // Scrolls a parent container if needed.
            /////
            function scrollParentContainer(event, el) {
                var evt = event.originalEvent || event;

                var parentScroll = $(el).scrollParent();

                if (parentScroll) {

                    if (parentScroll[0] !== document && parentScroll[0].scrollHeight) {

                        // should scroll (up and down only right now)
                        var scroll = 0;
                        var parentRect = parentScroll[0].getBoundingClientRect();
                        var parentHeight = parentScroll.height();

                        var y = evt.changedTouches[0].pageY;
                        if (y < (parentRect.top + (0.25 * parentHeight))) {
                            // in the upper quarter; scroll up
                            scroll = -10;
                        } else {
                            if (y > (parentRect.top + (0.75 * parentHeight))) {
                                // in the lower quarter; scroll down
                                scroll = 10;
                            }
                        }

                        // todo: it might be better to setTimeout and cancel when we leave the sensitive region
                        // this implementation means the user kind of has to keep wiggling to scroll further.
                        // but having said that, it works.
                        if (scroll !== 0) {
                            var newScrollTop = parentScroll.scrollTop() + scroll;

                            var maxScrollTop = parentScroll[0].scrollHeight - parentHeight;
                            if (newScrollTop > maxScrollTop) {
                                newScrollTop = maxScrollTop;
                            }
                            if (newScrollTop < 0) {
                                newScrollTop = 0;
                            }

                            parentScroll.scrollTop(newScrollTop);
                        }
                    }
                }
            }

            exports.dragTouchStart = function (event, el, dragOptions) {
                var evt = event.originalEvent || event;

                dropping = false;

                // ignore multi-touch
                if (evt.touches.length !== 1) {
                    return;
                }

                // ignore if already waiting for a timer
                if (timer) {
                    return;
                }

                // ignore if already dragging
                if (isDragging()) {
                    return;
                }

                // wait for touch and hold before dragging
                timer = setTimeout(function () {

                    // are we already dragging something?
                    if (dragging) {
                        return;
                    }

                    event.preventDefault();
                    
                    createIndicator(event, el, dragOptions);

                    var dragStart = createDragDropEvent("dragstart");
                    el.dispatchEvent(dragStart);

                }, 500);
            };

            exports.dragTouchMove = function(event) {

                // stop the 'hold' timer
                clearDragTimer();

                // ignore if not dragging
                if (!isDragging()) {
                    return;
                }

                event.preventDefault();
                
                // update the visual indicator for the drag operation
                moveIndicator(event);
            };

            exports.dragTouchEnd = function (event, el, cancel) {

                cleanUpDrag(cancel);

                // this may also be a drop... try to position ourselves after it
                $timeout(function() {
                    var dragEnd = createDragDropEvent("dragend");
                    el.dispatchEvent(dragEnd);
                }, 100);
            };

            exports.dropTouchMove = function(event) {
                var evt = event.originalEvent || event;

                // ignore if not dragging
                if (!isDragging()) {
                    return;
                }

                // evaluate the potential drop targets
                var newDropTargets = [];

                var el = document.elementFromPoint(evt.changedTouches[0].pageX, evt.changedTouches[0].pageY);

                if ($(el).attr('sp-droppable')) {
                    newDropTargets.push(el);
                }

                var parentTargets = $(el).parents('[sp-droppable]');
                _.forEach(parentTargets, function (p) {
                    if (newDropTargets.indexOf(p) < 0) {
                        newDropTargets.push(p);
                    }
                });

                // evaluate enter, leave and over lists
                var entering = [];
                var leaving = [];
                var over = [];

                _.forEach(newDropTargets, function (t) {
                    if (currentDropTargets.indexOf(t) < 0) {
                        if (entering.indexOf(t) < 0) {
                            entering.push(t);
                        }
                    } else {
                        if (over.indexOf(t) < 0) {
                            over.push(t);
                        }
                    }
                });

                _.forEach(currentDropTargets, function (t) {
                    if (newDropTargets.indexOf(t) < 0 && leaving.indexOf(t) < 0) {
                        leaving.push(t);
                    }
                });

                currentDropTargets.length = 0;
                currentDropTargets = newDropTargets;

                // sending dragleaving events
                _.forEach(leaving, function (leavingTarget) {
                    var dragLeave = createDragDropEvent("dragleave");
                    leavingTarget.dispatchEvent(dragLeave);
                });

                // scroll the most immediate parent container that may need it
                scrollParentContainer(event, el);

                if (!currentDropTargets.length) {
                    return;
                }

                // sending dragenter events
                _.forEach(entering, function (enteringTarget) {
                    var dragEnter = createDragDropEvent("dragenter");
                    enteringTarget.dispatchEvent(dragEnter);
                });

                // sending dragover events
                var dragOver = createDragDropEvent("dragover");

                // using 'MouseEvent' and 'initMouseEvent' seems to lock on an actual 'touch' device (i.e. iPad. fine in emulator)
                var rect = currentDropTargets[0].getBoundingClientRect();

                // manually set any positioning info we need here
                dragOver.offsetY = evt.changedTouches[0].pageY - rect.top;
                dragOver.offsetX = evt.changedTouches[0].pageX - rect.left;

                currentDropTargets[0].dispatchEvent(dragOver);

                event.preventDefault();
            };

            exports.dropTouchEnd = function(event, dropOptions) {
                var evt = event.originalEvent || event;

                // ignore if not dragging
                if (!isDragging()) {
                    return;
                }
                
                // if the drop is allowed, then process as a drop.
                var target;
                var el = document.elementFromPoint(evt.changedTouches[0].pageX, evt.changedTouches[0].pageY);
                if ($(el).attr('sp-droppable')) {
                    target = el;
                } else {
                    target = $(el).closest('[sp-droppable]');
                }

                if (!target) {
                    return;
                }

                var dropData;
                var dropDataExpr = $(target).attr('sp-droppable-data');
                if (dropDataExpr) {
                    dropData = $(target).scope().$eval(dropDataExpr);
                }
                
                var source = spDragDropService.getSourceElement();

                if (dropOptions.onAllowDrop) {
                    dropping = dropOptions.onAllowDrop(source, target, spDragDropService.getDragData(), dropData);
                }

                if (dropping) {

                    cleanUpDrop();
                    
                    var drop = createDragDropEvent("drop");

                    if (target.dispatchEvent) {
                        target.dispatchEvent(drop);
                    } else {
                        el.dispatchEvent(drop);
                    }
                }
            };

            return exports;
        })
        .factory('spDragDropSimService', function (spDragDropService) {

            var exports = {};

            function logMouseEvent(message, event) {
                var props = _.flatten(_.map(['target.tagName', 'currentTarget.tagName', 'originalEvent.clientX', 'originalEvent.clientY'], function (p) {
                    return ['\n', p, sp.result(event, p)];
                }));
                console.log([message].concat(props).join());
                console.log(message, event);
            }

            var defaultOptions = {
                pointerX: 0,
                pointerY: 0,
                button: 0,
                ctrlKey: false,
                altKey: false,
                shiftKey: false,
                metaKey: false,
                bubbles: true,
                cancelable: true,
                view: window,
                detail: 0,
                screenX: 0,
                screenY: 0,
                clientX: 1,
                clientY: 1
            };

            function findOffCenter(elem, ratios) {
                var offset,
                    document = $(elem.ownerDocument);

                ratios = ratios || { x: 0.5, y: 0.5 };

                elem = $(elem);
                offset = elem.offset();

                return {
                    x: offset.left + elem.outerWidth() * ratios.x - document.scrollLeft(),
                    y: offset.top + elem.outerHeight() * ratios.y - document.scrollTop()
                };
            }

            function createMouseEvent(eventName, element) {
                var options = _.extend(defaultOptions, arguments[2] || {});
                var oEvent, eventType = null;

                if (document.createEvent) {
                    oEvent = document.createEvent('MouseEvents');
                    oEvent.initMouseEvent(eventName, options.bubbles, options.cancelable, document.defaultView,
                        options.button, options.pointerX, options.pointerY, options.pointerX, options.pointerY,
                        options.ctrlKey, options.altKey, options.shiftKey, options.metaKey, options.button, element);
                } else {
                    options.clientX = options.pointerX;
                    options.clientY = options.pointerY;
                    var evt = document.createEventObject();
                    oEvent = _.extend(evt, options);
                }
                return oEvent;
            }

            // if opts has x or y then use that
            // else if opts has ratios map with x and y then use those to
            // find a point within the element
            // otherwise return the center
            function createDragDropEvent(eventName, element, opts) {
                var ratios = { x: 1/2, y: 1/2 };
                opts = opts || {};
                if (opts.ratios) ratios = _.defaults(opts.ratios, ratios);
                var center = _.extend(findOffCenter(element, ratios), opts);
                var pos = { pointerX: center.x, pointerY: center.y };
                console.log(['simulate mouse event', eventName, 'at', pos.pointerX, pos.pointerY].join());
                var event = createMouseEvent(eventName, element, pos);
                return _.extend({ originalEvent: event }, event);
            }

            // hasn't been tested ever
            function simulateEvent(element, eventName) {
                var event = createMouseEvent(eventName);
                if (element.dispatchEvent) {
                    element.dispatchEvent(event);
                } else {
                    element.fireEvent('on' + eventName, event);
                }
                return element;
            }

            function dragAndDrop(fromEl, toElOrPos, opts) {

                var dragOptions, dragData, dropOptions, dropData;
                var toEl = toElOrPos; // assume just el right now todo - handle position
                var el, elWithScope, scope;

                // todo - use the options!
                opts = _.extend({ moves: 3 }, opts || {});

                function callDraggableHandler(handlerName, eventName) {
                    if (dragOptions && dragOptions[handlerName]) {
                        dragOptions[handlerName](createDragDropEvent(eventName, fromEl[0], opts), dragData);
                    }
                }

                function callDroppableHandler(handlerName, eventName) {
                    if (dropOptions && dropOptions[handlerName]) {
                        var event = createDragDropEvent(eventName, toEl[0], opts);
                        try {
                            dropOptions[handlerName](event,
                                spDragDropService.getSourceElement(),
                                toEl[0],
                                dragData,
                                dropData);
                        } catch (e) {
                            console.error('SimDndService: Exception calling drop handler: ' + e);
                            console.error('SimDndService: drop handler: ' + _.isFunction(dropOptions[handlerName]));
                            console.error('SimDndService: type drop handler: ' + typeof dropOptions[handlerName]);
                            console.error('SimDndService: drop function: ' + dropOptions[handlerName]);
                            console.error('SimDndService: drop target: ' + toEl[0].className);
                            //console.log('SimDndService: from=\n' + fromEl[0].outerHTML + ' to=\n' + toEl[0].outerHTML);
                            throw e;
                        }
                    }
                }

                function getClosestScope(el) {
                    var elWithScope = el.closest('.ng-scope,.ng-isolate-scope');
                    return elWithScope && elWithScope.data().$scope ||
                        elWithScope.data().$isolateScope ||
                        elWithScope.data().$isolateScopeNoTemplate;
                }

                console.log('dnd fromEl ' + fromEl.length);
                console.log('dnd toEl ' + toEl.length);

                if (!fromEl || fromEl.length === 0 || !toEl || toEl.length === 0) {
                    console.warn('simulateDragAndDrop: missing arguments', fromEl, toEl);
                    //throw new Error('simulateDragAndDrop: missing arguments');
                    return false;
                }

                // Ensure the from and to elements are using spDraggable and spDroppable resp
                // If we have been given a element collection with more than one then use the
                // last as the theory is that it is the deepest. Then we look back for draggable etc.

                fromEl = fromEl.filter(':last').closest('[sp-draggable]');
                toEl = toEl.filter(':last').closest('[sp-droppable]');

                if (!fromEl || fromEl.length === 0 || !toEl || toEl.length === 0) {
                    console.warn('simulateDragAndDrop: cannot find sp-draggable or sp-droppable');
                    //throw new Error('simulateDragAndDrop: cannot find sp-draggable or sp-droppable');
                    return false;
                }

                // Get the spDraggable and spDroppable attributes, looking up the
                // DOM tree if needed to find a scope to evaluate on.

                el = fromEl[0];
                scope = getClosestScope(fromEl);

//                console.log('from el', [el], 'el with scope', elWithScope, 'scope', scope);
//                console.log(['from el', el.className, 'el with scope', elWithScope[0].className].join());
                console.log('dragOpts ' + el.getAttribute('sp-draggable'));
                console.log('dragData ' + el.getAttribute('sp-draggable-data'));

                if (scope) {
                    dragOptions = scope.$eval(el.getAttribute('sp-draggable'));
                    dragData = scope.$eval(el.getAttribute('sp-draggable-data'));
                }

                el = toEl[0];
                scope = getClosestScope(toEl);

//                console.log('to el', [el], 'el with scope', elWithScope, 'scope', scope);
//                console.log(['to el', el.className, 'el with scope', elWithScope[0].className].join());
                console.log('dropping on scope id ' + sp.result(scope, '$id'));
                console.log('dropOpts ' + el.getAttribute('sp-droppable'));
                console.log('dropData ' + el.getAttribute('sp-droppable-data'));

                if (scope) {
                    dropOptions = scope.$eval(el.getAttribute('sp-droppable'));
                    dropData = scope.$eval(el.getAttribute('sp-droppable-data'));
                }

//                console.log('dragging', dragOptions, dragData);
//                console.log('dropping', dropOptions, dropData);

                // If we have the minimum needed data then simulate the drop.
                // todo - simulate some dragging

                if (!dragData || !dropOptions) {
                    console.warn('simulateDragAndDrop: missing either dragData or dropOptions');
                    return false;
                }

                // start
                spDragDropService.setSourceElement(fromEl);
                spDragDropService.setDragData(dragData);
                callDraggableHandler('onDragStart', 'dragstart');
                spDragDropService.startDragOperation();

                // drag
                callDraggableHandler('onDrag', 'drag');

                // dragenter
                spDragDropService.dragEnter(toEl[0]);
                callDroppableHandler('onDragEnter', 'dragenter');

                // dragover
                //todo - adjust the coords... and maybe do some steps
                callDroppableHandler('onDragOver', 'dragover');

                // dragleave
                callDroppableHandler('onDragLeave', 'dragleave');

                // drop
                callDroppableHandler('onDrop', 'drop');

                // dragend
                callDraggableHandler('onDragEnd', 'dragend');
                spDragDropService.endDragOperation();
                spDragDropService.clearDragData();
                spDragDropService.clearSourceElement();

                return true;
            }

            exports.dragAndDrop = dragAndDrop;

            return exports;
        });
}());