// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global angular, console, _, Diagrammer, Raphael, sp */

/**
 *  Contains Angular directives related to diagramming.
 *  These are more tightly coupled to the needs of workflow than originally hoped, but well there you go.
 * @file
 */

(function () {
    'use strict';

    var module = angular.module('sp.common.workflow.diagramDirectives', []);

    module.directive('diagramWidget', function (elementTemplates) {

        // Expects a diagram model and renders it.
        // Notifies the model when various UI events occur such as
        // moving elements, dnd, adding connections and invoking tools

        /**
         * An AngularJS directive that uses uses SVG elements (via RaphaelJS) to render an interactive diagram of the diagram model.
         * @exports diagramWidget
         * @todo rename this sucker
         */
        return {
            restrict: 'E',
            scope: {
                diagramModel: '=diagram',
                diagramWidth: '=width',
                diagramHeight: '=height',
                zoom: '='
            },
            replace: true,
            templateUrl: 'workflow/diagrammer/diagramWidgets.tpl.html',
            link: function (scope, el, attrs) {

                var diagram = null;
                var selectedObject = null;

                function getEventPosition(e) {
                    var target = e.relatedTarget || e.target || e.srcElement,
                        rect = target.getBoundingClientRect(),
                        offsetX = e.clientX - rect.left,
                        offsetY = e.clientY - rect.top,
                        pos = { x: offsetX, y: offsetY };

//                    console.log('getEventPosition', e, pos);

                    // this isn't quite right yet.... for the moment use the following
                    pos = { x: e.layerX, y: e.layerY };

                    return pos;
                }

                function getZoomedPos(pos, zoom) {
                    return zoom ? { x: pos.x / zoom, y: pos.y / zoom } : pos;
                }

                function findElementsAt(x, y) {

                    // Find and return the elements the given location.
                    // Include hit testing anything that makes sense, like connection ports
                    // and text labels.
                    // issue - having a problem with the text label... it seems that the event coords
                    // we get when we drop on the text label are relative to the text label
                    // issue - only working on chrome right now

                    var bbox = { x: x - 1, y: y - 1, width: 2, height: 2 };
                    // find the elements that are located at that position
                    var elements = _.filter(diagram.elements, function (el) {
                        //todo- better flag than 'movable' to indicate a possible target of a drop
                        //return el.movable && Raphael.isBBoxIntersect(el.getElementBBox(), bbox);
//                        return el.movable && Raphael.isPointInsideBBox(el.getElementBBox(), x, y);
                        return el.movable && Raphael.isPointInsideBBox(el.getElementBBox(), x, y) ||
                            _.some(el.outPorts, function (port) {
//                                console.log('textshape', port.textShape.node, x, y, port.textShape && port.textShape.getBBox());
//                                return port.clickShape && Raphael.isPointInsideBBox(port.clickShape.getBBox(), x, y) ||
//                                    port.textShape && Raphael.isPointInsideBBox(port.textShape.getBBox(), x, y);
                                return port.clickShape && Raphael.isPointInsideBBox(port.clickShape.getBBox(), x, y);
                            });
                    });
                    // find the closest exit point on each element to the given location
                    var elementsAndExits = _.map(elements, function (el) {
                        var closestPort = _(el.outPorts)
                            .map(function (port) {
                                var portBbox = port.portShape && port.portShape.getBBox();
                                var dist = Math.sqrt(Math.pow(x - portBbox.cx, 2) + Math.pow(y - portBbox.cy, 2));
                                console.log('port with dist', x, y, dist, port.id, port.name);
                                return {
                                    port: port,
                                    dist: dist
                                };
                            })
                            .sortBy('dist')
                            .first();
//                        console.log('returning port', sp.result(closestPort, 'port.id'));
                        return { element: el, port: _.result(closestPort, 'port') };
                    });
                    return elementsAndExits;
                }

                scope.dropOptions = {
                    simpleEventsOnly: false,
                    propagateDragEnter: false,
                    propagateDragLeave: false,
                    propagateDrop: false,
                    propagateDragOver: false,

                    onAllowDrop: function (source, target, dragData, dropData) {
                        return dragData && dragData.id;
                    },
                    onDrop: function (event, source, target, dragData, dropData) {


                        var e = event.originalEvent,
                            pos = getZoomedPos(getEventPosition(e), diagram.zoom()),
                            droppedOn = findElementsAt(pos.x, pos.y),
                            dropTarget = _.first(droppedOn);

//                        console.log('DROP', arguments);
//                        console.log('DROP pos', pos);
//                        console.log('DROP', dragData, event, sp.result(dropTarget, 'element.modelElement.id'),
//                            _.map(droppedOn, function (t) {
//                                return {
//                                    id: sp.result(t, 'element.modelElement.id'),
//                                    port: t.outPort
//                                };
//                            }));

                        scope.$root.$broadcast('wb.droppedOnCanvas', {
                            dragData: dragData,
                            x: pos.x, y: pos.y,
                            elementId: sp.result(dropTarget, 'element.modelElement.id'),
                            portId: sp.result(dropTarget, 'port.id')
                        });
                    },
                    onDragOver: function (event, source, target, dragData, dropData) {
//                        if (dragData && dragData.id) {
//                            var e = event.originalEvent;
//                            //console.log('diagram: dragover', dragData.id, dragData.name, dropData, e.layerX, e.layerY);
//                            if (diagram && scope.diagramModel && scope.diagramModel.getElementForType) {
//                                var elementData = scope.diagramModel.getElementForType(dragData.id);
//                                diagram.onDragOver(e, elementData);
//                            }
//                            return true;
//                        }
//                        return false;
                    },
                    onDragEnter: function (event, source, target, dragData, dropData) {
//                        console.log('diagram: dragenter');
                    },
                    onDragLeave: function (event, source, target, dragData, dropData) {
//                        if (diagram) {
//                            diagram.onDragLeave(event.originalEvent, dragData);
//                        }
                    }
                };

                // We need to track any visual objects ourselves and NOT add them to the diagram model object (or its children).
                // The reason is the diagram is a watched model object and sticking DOM or otherwise complex objects hurts
                // if not breaks the Angular binding system.
                // The reason I'm saying is that the original version of this added Raphael element references to the
                // diagram.elements themselves rather than maintaining parallel data sets. Things went pear-shaped.

                function notifyAngular() {
                    // events such as clicked and moved may be called via a Raphael event and that means
                    // outside of Angular and if so then we need to $apply
                    if (!scope.$$phase) {
                        scope.$apply();
                    }
                }

                function setSelectedObject(obj) {

                    function notifySetSelectedObject(obj) {
                        // only supporting a single selected object, atm
                        scope.diagramModel.clearSelection();
                        if (obj) {
                            scope.diagramModel.addToSelection(obj.id);
                        }
                    }

                    if (selectedObject === obj) {
                        return;
                    }

                    if (obj && !obj.id) {
                        return;
                    }

                    notifySetSelectedObject(obj);
                    notifyAngular();
                }

                function elementMoved(el, x, y) {

                    function notifyElementMoved(el, x, y) {
                        scope.diagramModel.elementMoved(el.id, x, y);
                    }

                    notifyElementMoved(el, x, y);
                    notifyAngular();
                }

                function portClicked(port, el, x, y) {

                    function notifyPortClicked(port, el, x, y) {
                        scope.diagramModel.portClicked(port.id, el.id, x, y);
                    }

                    notifyPortClicked(port, el, x, y);
                    notifyAngular();
                }

                function connectionChanged(c, el1, port, el2) {

                    function notifyConnectionChanged(c, el1, port, el2) {
                        scope.diagramModel.connectionChanged(c.id, el1.id, port ? port.id : null, el2.id);
                    }

                    notifyConnectionChanged(c, el1, port, el2);
                    notifyAngular();
                }

                function connectionAdded(el1, port, el2) {

                    function notifyConnectionAdded(el1, port, el2) {
                        scope.diagramModel.newConnection(el1.id, port ? port.id : null, el2.id);
                    }

                    notifyConnectionAdded(el1, port, el2);
                    notifyAngular();
                }

                
                function syncModel() {
                    //console.log('watch on "diagramModel" fired', value);

                    if (!diagram) {

                        //console.log('creating SVG surface at zoom', scope.zoom);

                        diagram = new Diagrammer.Diagram({
                            target: el[0],
                            width: scope.diagramWidth,
                            height: scope.diagramHeight,
                            elementTemplates: elementTemplates,
                            onSelected: function (obj) {
                                //console.log('on object selected: obj=%o, this =%o', obj, this);
                                setSelectedObject(obj);
                            },
                            onConnectionChanged: function (c, el1, port, el2) {
                                //console.log('on connection changed: ', c, el1, port, el2);
                                connectionChanged(c, el1, port, el2);
                            },
                            onConnectionCreated: function (el1, port, el2) {
                                //console.log('on connection created: ', el1, port, el2);
                                connectionAdded(el1, port, el2);
                            },
                            onElementMoved: function (el, x, y) {
                                //console.log('on element moved: ', el, x, y, this);
                                elementMoved(el, x, y);
                            },
                            onPortClick: function (port, el, x, y) {
                                //console.log('on port clicked: ', port, el, x, y);
                                portClicked(port, el, x, y);
                            },
                            onToolClick: function (tool, x, y) {
                                //console.log('on tool clicked: ', tool, x, y);
                            }
                        });
                    }

                    selectedObject = null;
                    diagram.syncDiagram(scope.diagramModel);
                }

                function resize() {
                    if (scope.diagramWidth && scope.diagramHeight) {
                        diagram.setDimentions(scope.diagramWidth, scope.diagramHeight);
                    }
                }

                scope.$watch('diagramModel', syncModel);

                scope.$watch('diagramWidth', resize);
                scope.$watch('diagramHeight', resize);

                scope.$watch('zoom', function (zoom) {
                    //console.log('watch on "zoom" fired', zoom);
                    if (zoom && diagram) {
                        //console.log('zooming SVG surface', zoom);
                        diagram.zoom(zoom);
                    }
                });
            }
        };

    });

    module.directive('simpleDiagramWidget', function () {
        /**
         * Non-graphical rendering of the diagram model. A debugging aide.
         * @exports simpleDiagramWidget
         *
         * No code needed here as all the work is done in the template.
         * Expects a diagram model and renders it.
         * Notifies the model when various UI events occur such as
         * moving elements, dnd, adding connections and invoking tools
         */
        return {
            restrict: 'E',
            scope: {
                /**
                 * *diagram* is the bindable attribute to the diagram model
                 * @memberof module:simpleDiagramWidget
                 */
                diagram: '='
            },
            replace: true,
            /** the vew template to render the diagram
             * @memberof module:simpleDiagramWidget
             */
            templateUrl: 'workflow/diagrammer/simpleDiagramWidget.tpl.html',
            link: function (scope, el, attrs) {
            }
        };
    });
}());