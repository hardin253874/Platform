// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, Raphael */

var Diagrammer;
(function (Diagrammer) {
    'use strict';

    //todo - move the port handling to a "class"

    var snapToPixels = 11;                      // What size grid to snap to.
    var snapShiftX = 2, snapShiftY = -3;        // Shift snapping to make it align with the grid 


    Diagrammer.fillTemplate = function(template, data) {
        var s = angular.toJson(template);
        for (var key in data) {
            var value = data[key];
            if (_.isString(value)) {
                var re = new RegExp('{{' + key + '}}', 'ig');
                //console.log('fillTemplate: replacing %s with %o', re, value);
                s = s.replace(re, value.replace(/\\/g, '\\\\').replace(/\"/g, '\\"'));
            }
        }
        try {
            return angular.fromJson(s);
        } catch (e) {
            console.error('fillTemplate: fromJson on %s failed: %o', s, e);
            return template;
        }
    };

    var Element = function (diagram, src) {
        this.diagram = diagram;
        this.id = src.id;
        this.updateFromModel(src);
    };

    Element.prototype.updateFromModel = function (src) {
        //console.log('updateFromModel', this.id, src.id, src.name);

        function createGridPath(paper, path) {
            return paper.path(path).attr({
                'stroke': 'grey',
                'stroke-dasharray': '. ',
                'stroke-opacity': 0.4
            });
        }

        if (this.id !== src.id) {
            throw new Error('unexpected mismatch of element ids: ' + this.id + ', ' + src.id);
        }

        // we can have refs in this direction but not the other way - see comment above regarding references
        this.modelElement = src;

        // fill out the template
        var template = this.diagram.elementTemplates[src.template];
        // todo: fix up this label vs name business
        var srcCopy = _.extend({ label: src.name }, src);
        template = Diagrammer.fillTemplate(template, srcCopy);

        if (angular.equals(this.template, template) && this.movable === src.movable && angular.equals(this.tools, src.tools)) {
            //console.log('>>> NOT re-creating the element', this.id, src.id, src.name);
        } else {
            //console.log('>>> re-creating the element', this.id, src.id, src.name);

            this.template = template;

            // Re-create the Raphael element(s).
            // Take a note the relative position for the replacement.
            this.unhighlight();
            this.removeErrorHighlight();

            if (this.shapes) {
                if (this.shapes.length > 0) {
                    this.diagram.prevElement = this.shapes[0].prev;
                }
                this.shapes.remove();
            }

            //console.log('creating shape', this.id, this.template);

            this.shapes = this.diagram.paper.add(this.template);

            //todo - tidy up this metaProperty stuff.
            // - should be able to have more than one on a sub-element
            // - should have generic means to find tagged sub-elements

            // note the element within the set that represents the bounding box
            this.boundingShape = this.shapes;
            this.template.some(_.bind(function (e, i) {
                if (e.metaProperty === 'bounding') {
                    this.boundingShape = this.shapes.items[i];
                    if (src.help) {
                        //console.log('setting element title', src.help);
                        //DON'T do the following. It causes odd crashes.
                        //this.boundingShape.attr('title', src.help);
                    }
                    this.boundingShape.node.setAttribute('data-element-name', src.name);
                    return true;
                }
                return false;
            }, this));

            // note the element within the set that represents the selectable box
            this.selectableShape = this.shapes;
            this.template.some(function (e, i) {
                if (e.metaProperty === 'selectable') {
                    this.selectableShape = this.shapes.items[i];
                    return true;
                }
                return false;
            }, this);


            //debug
//            if (this.boundingShape) {
//                var b = this.getElementBBox();
//                this.shapes.push(this.diagram.paper.rect(b.x, b.y, b.width, b.height));
//            }

            // Restore the shapes relative position (affects what overlaps what)
            if (this.diagram.prevElement &&
                (this.diagram.prevElement.node || _.isArray(this.diagram.prevElement))) {
                this.shapes.insertAfter(this.diagram.prevElement);
            }

            // Get and remember the initial set of transforms for this element set
            // Note - shapes is a Raphael 'set' that has a forEach. Just saying I more than
            // once I have seen this forEach construct and changed to single liner using map
            // that then breaks. It's not an array.
            this.transforms = [];
            this.shapes.forEach(function (el) {
                this.transforms.push(el.matrix.toTransformString());
            }, this);

            // set up event handling

            this.selectableShape.click(this.onClick, this);

            if (src.movable) {
                this.movable = true;
                this.shapes.drag(this.onDragMove, this.onDragStart, this.onDragEnd, this, this, this);
            }

            if (src.tools) {
                this.tools = angular.copy(src.tools);
                this.shapes.hover(this.onMouseOver, this.onMouseOut, this, this);
            }

            this.x = 0;
            this.y = 0;
        }

        // move the element to its current position
        this.move(src.x, src.y);

        // initialise ports if needed, if new or different
        if (!src.outPorts || !this.outPorts || src.outPorts.length !== this.outPorts.length ||
            _.difference(_.map(src.outPorts, 'id'), _.map(this.outPorts, 'id')).length !== 0 ||
            _.difference(_.map(src.outPorts, 'name'), _.map(this.outPorts, 'name')).length !== 0) {

            //console.log('clearing outports for %s', this.id);

            this.removeOutPorts();
            this.outPorts = _.sortBy(angular.copy(src.outPorts || []), 'ordinal');
            this.createOutPorts();
        }

        // track this as the last seen shape - for positioning purposes
        if (this.shapes && this.shapes.length && this.shapes.length > 0) {
            if (!this.diagram.prevElement && !this.diagram.prevElement.node) {
                console.log('null node on', this.shapes);
            }
            this.diagram.prevElement = this.shapes[this.shapes.length - 1];
        }
    };
    Element.prototype.cleanUp = function () {

        this.unhighlight();
        this.removeErrorHighlight();
        this.removeOutPorts();
        if (this.shapes) {
            this.shapes.remove();
            this.inPorts = null;
            this.outPorts = null;
        }
    };




    Element.prototype.move = function (x, y) {

        x = Math.floor((x + snapShiftX) / snapToPixels) * snapToPixels - snapShiftX;
        y = Math.floor((y + snapShiftY) / snapToPixels) * snapToPixels - snapShiftY;

        //console.log('moveTo id=%s (%s, %s) => (%s, %s)', this.id, this.x, this.y, x, y);

        if (isNaN(x) || isNaN(y)) {
            console.trace();
            return;
        }

        // We choose to remove any highlight and re-create after if needed. Logic is simpler
        // and there is typically only one element that is selected so should be fast enough...
        this.unhighlight();
        this.removeErrorHighlight();

        //todo - try allowing only moves that are bigger than a couple of pixels.. it is leaving trails
        if (this.x !== x || this.y !== y) {

            this.x = x;
            this.y = y;
            this.shapes.forEach(function (shape, i) {
                shape.transform('T' + x + ',' + y + (this.transforms[i] || ''));
            }, this);

            this.moveOutPorts();
        }

        if (this.modelElement && this.modelElement.selected) {
            this.highlight();
        }

        if (this.modelElement.errors && this.modelElement.errors.length) {
            this.addErrorHighlight();
        }
    };

    Element.prototype.getElementBBox = function () {
        return this.boundingShape.getBBox();
    };

    Element.prototype.onClick = function (e) {
        this.diagram.setSelectedObject(this);
    };

    Element.prototype.onMouseOver = function (e) {
        this.diagram.showTools(this);
        this.showOutPorts();
    };
    Element.prototype.onMouseOut = function (e) {
        this.diagram.hideTools();
        this.hideOutPorts();
    };

    Element.prototype.onDragStart = function (x, y, e) {
        if (this.shapes) {
            this.ox = this.x;
            this.oy = this.y;
        }
    };
    Element.prototype.onDragMove = function (dx, dy, x, y, e) {

        //console.group('drag element %s', this.id, x, y);
        //console.time('moving');

        this.diagram.hideToolsFast();

        // allow for zoomed view port
        var zoom = this.diagram.zoom();
        if (zoom) {
            dx = dx / zoom;
            dy = dy / zoom;
        }

        x = this.ox + dx;
        y = this.oy + dy;

        // move the element
        this.move(x, y);

        // redraw any connections against the ports of el element
        this.diagram.elementMoving(this, x, y);

        //console.timeEnd('moving');
        //console.groupEnd();
    };
    Element.prototype.onDragEnd = function (e) {
        if (this.ox !== this.x || this.oy !== this.y) {
            this.diagram.elementMoved(this, this.x, this.y);
        }
    };

    Element.prototype.highlight = function () {
        this.removeErrorHighlight();
        if (!this.glow) {
            var glow = { color: 'red', opacity: 1, fill: false, width: 7 };
            if (this.modelElement && this.modelElement.type === 'swimlane') { // YUK!!!
                glow.width = 1;
            }
            this.glow = (this.selectableShape !== this.shapes ? this.selectableShape : this.shapes[0]).glow(glow);
        }
    };

    Element.prototype.unhighlight = function () {
        if (this.glow) {
            this.glow.remove();
            this.glow = null;
        }
    };

    Element.prototype.addErrorHighlight = function () {
        if (!this.glow && !this.errorGlow) {
            var glow = { color: 'purple', opacity: 1, fill: false, width: 10 };
            if (this.modelElement && this.modelElement.type === 'swimlane') { // YUK!!!
                glow.width = 1;
            }
            this.errorGlow = (this.selectableShape !== this.shapes ? this.selectableShape : this.shapes[0]).glow(glow);
        }
    };

    Element.prototype.removeErrorHighlight = function () {
        if (this.errorGlow) {
            this.errorGlow.remove();
            this.errorGlow = null;
        }
    };

    Element.prototype.getOutPort = function (portId) {
        var el = this;

        if (!portId) {
            return { id: null, name: '', shapes: el.boundingShape, isDefault: true };
        }
        if (!el.outPorts) {el.outPorts = [];}
        var port, portIndex;
        _.some(el.outPorts, function (p, i) {
            if (p.id === portId) {
                port = p;
                portIndex = i;
                return true;
            }
            return false;
        });
        return {
            id: portId,
            name: port && port.name !== 'Default Exit Point' ? port.name : '',
            shapes: el.boundingShape,
            isDefault: !el.outPorts.length || el.outPorts[0].id == portId, // eslint-disable-line eqeqeq
            posDg: port ? port.posDg : 90,
            pos: port ? port.pos : null,
            dx: portIndex ? portIndex * 8 : 0,
            dy: portIndex ? portIndex * 8 : 0
        };
    };
    Element.prototype.getInPort = function (portId) {
        var el = this,
            paper = this.diagram.paper;

        if (!portId) {
            return { id: null, shapes: el.boundingShape };
        }
        if (!el.inPorts) {el.inPorts = [];}
        var port = spUtils.findByKey(el.inPorts, 'id', portId);
        if (!port) {
            // should check el.modelElement.outPorts ....
            el.inPorts.push(port = {
                id: portId,
                shapes: paper.circle(0, 0, 10).attr({ 'fill': 'grey' }).transform('T' + el.x + ',' + el.y)
            });
            el.shapes.splice(-1, 0, port.shapes);
        }
        return port;
    };

    Element.prototype.updateOutPortPositions = function () {
        //TODO clean this up!

        /*
         * notes....
         * port.pos is the [x,y] coords of the port, typically on the bbox.
         * port.posDeg is either 0, 90, 180, 270 indicating the side of the bbox the port is on.
         * The reason for using degrees here is historical.
         */

        // how's this for horrible...
        // if there are any port's with ordinal > 90 then place on the bottom with the rest spread along the right
        // otherwise ... see the code
        // the spacing calc (when used) is saying to place exits at 10%, and 90% and spread the rest evenly in between

        var bottomPorts = _.filter(this.outPorts, function (item) {
                return (item.ordinal || 0) > 90;
            }),
            ports = _.filter(this.outPorts, function (item) {
                return (item.ordinal || 0) <= 90;
            }),
            bbox = this.getElementBBox(),
            bottomMargin = bbox.width / 10,
            rightMargin = bbox.height / 10,
            availWidth = 8 * bbox.width / 10,
            availHeight = 8 * bbox.height / 10,
            port, portCount;

        if (bottomPorts.length > 0) {
            // spread the bottom ports out along the bottom
            portCount = bottomPorts.length;
            if (portCount === 1) {
                port = bottomPorts[0];
                port.posDg = 180;
                port.pos = { x: bbox.x + bbox.width / 2, y: bbox.y2 };
            } else {
                // the formula below is saying to place exits at 10%, and 90% and spread the rest evenly in between
                bottomPorts.forEach(function (port, index) {
                    port.posDg = 180;
                    port.pos = { x: bbox.x + bottomMargin + index * availWidth / (portCount - 1), y: bbox.y2 };
                });
            }
            // spread the others along the right side
            portCount = ports.length;
            if (portCount === 1) {
                port = ports[0];
                port.posDg = 90;
                port.pos = { x: bbox.x2, y: bbox.y + bbox.height / 2 };
            } else {
                ports.forEach(function (port, index) {
                    port.posDg = 90;
                    port.pos = { x: bbox.x2, y: bbox.y + rightMargin + index * availHeight / (portCount - 1) };
                });
            }
        } else {
            portCount = ports.length;

            if (portCount === 1) {
                port = ports[0];
                port.posDg = 90;
                port.pos = { x: bbox.x2, y: bbox.y + bbox.height / 2 };

            } else if (portCount === 2) {
                port = ports[0];
                port.posDg = 90;
                port.pos = { x: bbox.x2, y: bbox.y + bbox.height / 2 };

                port = ports[1];
                port.posDg = 180;
                port.pos = { x: bbox.x + bbox.width / 2, y: bbox.y2 };

            } else {
                ports.forEach(function (port, index) {
                    port.posDg = 90;
                    port.pos = { x: bbox.x2, y: bbox.y + rightMargin + index * availHeight / (portCount - 1) };
                });
            }
        }
    };
    Element.prototype.moveOutPorts = function () {
        //todo - move them rather than naive remove and create as this is quite a performance hit
        this.removeOutPorts();
        this.createOutPorts();
    };

    Element.prototype.createOutPorts = function () {

        var el = this,
            paper = this.diagram.paper;

        // outPorts is an array of { id, name, posDg } and the first (index 0) is the default.
        // If posDg is given then it represents the degrees cw from north, e.g. posDg 90 is at 3 o'clock

        // at the moment any defined posDg is being ignored and recalculated based on the # of ports

        //todo - redo this using a template ... ala elements

        if (!el.outPorts) {return;}

        this.updateOutPortPositions();

        el.outPorts.forEach(function (port, index) {

            if (port.shape) {
                return;
            }

            var elementAndPort = {
                    el: el,
                    port: port,
                    onClicked: function (e) {
//                        console.log('port clicked %s %s', this.el.id, this.port.id, e);
                        this.el.onPortClick(this.port, e.clientX, e.clientY);
                    },
                    onDragStart: function (x, y, e) {
//                        console.log('port dragstart %s %s %d %d', this.el.id, this.port.id, x, y);
                        this.el.onPortDragStart(this.port, x, y, e);
                    },
                    onDragMove: function(dx, dy, x, y, e) {
                        this.el.onPortDragMove(this.port, dx, dy, x, y, e);
                    },
                    onDragEnd: function (e) {
                        try {
                            this.el.onPortDragEnd(this.port, e);
                        } catch (error) {
                            // swallow the exception so it doesn't prevent future interactions
                            console.log(error);     
                        }
                    }
                },
                portShape, textShape, clickShape;

            //console.log('%s creating port (%s,%s) shape at %o %o', el.id, port.id, port.name, port.pos, port.posDg);

            var textDy = port.posDg === 90 ? -7 : +7;
            textShape = paper.text(port.pos.x + 5, port.pos.y + textDy, port.label || port.name || '')
                .attr({
                    'text-anchor': 'start',
                    'fill': '#666666',
                    'font-family': "Segoe UI, Verdana",
                    'font-size': "10px",
                    'font-weight': "bold"
                });

            portShape = paper.rect(port.pos.x - 4, port.pos.y - 3, 8, 6)
                .attr({ 'fill': 'black', 'fill-opacity': 0, 'stroke-opacity': 0 })
                .click(elementAndPort.onClicked, elementAndPort);

            // add another larger transparent shape for better clickability

            clickShape = paper.circle(port.pos.x, port.pos.y, 12)
                .attr({ 'stroke-opacity': 0, 'fill': 'blue', 'fill-opacity': 0.01 })
                .click(elementAndPort.onClicked, elementAndPort)
                .drag(elementAndPort.onDragMove, elementAndPort.onDragStart, elementAndPort.onDragEnd, elementAndPort, elementAndPort, elementAndPort)
                .hover(this.showOutPorts, this.hideOutPorts, this, this);

            //console.log('set up dnd for ', elementAndPort);

            port.shapes = paper.set();
            port.shapes.push(portShape, textShape, clickShape);
            port.portShape = portShape;
            port.clickShape = clickShape;
            port.textShape = textShape;

            //console.log('created outport shape on %s', this.id);
        }, this);
    };

    Element.prototype.removeOutPorts = function () {
        var el = this;

        if (!el.outPorts) {return;}
        el.outPorts.forEach(function (port, index) {
            if (port.shapes) {
                port.shapes.remove();
                port.shapes = null;
                port.portShape = null;
            }
        });
    };

    Element.prototype.showOutPorts = function () {
        var el = this;
        if (!el.outPorts) {return;}
        el.outPorts.forEach(function (port, index) {
            if (port.portShape) {
                port.portShape.attr({ 'stroke-opacity': 1, 'fill-opacity': 0.4 });
            }
        });
    };

    Element.prototype.hideOutPorts = function () {
        var el = this;
        if (!el.outPorts) {return;}
        el.outPorts.forEach(function (port, index) {
            if (port.portShape) {
                port.portShape.attr({ 'stroke-opacity': 0, 'fill-opacity': 0.1 });
            }
        });
    };

    Element.prototype.onPortClick = function(port, x, y) {
//        console.log('element.onPortClick %o %o %s %s', this, port, x, y);

        this.diagram.onPortClick(port, this, x, y);
    };

    Element.prototype.onPortDragStart = function (port, x, y, e) {
        //console.log('element.onPortDragStart %o %o %s %s', this, port, x, y);

        if (!port) {
            console.log('??? onPortDragStart this isn\'t as expected', this);
            return;
        }
        // is the port occupied?
        port.dragConnection = _.find(this.diagram.connections, function (c) {
            return c.fromElement === this && c.fromPort.id === port.id;
        });
//        console.log('port is occupied = ', !!port.dragConnection);

        port.dx = 0;
        port.dy = 0;

        if (port.dragConnection) {
            // doing a 'move connection' operation
            // set 'draw from' point to the other end of the connection
            var points = port.dragConnection.getConnectionEndPoints(),
                endPt = points[1];
            port.ox = endPt.x;
            port.oy = endPt.y;
        } else {
            // doing a 'add connection' operation
            port.ox = port.pos.x;
            port.oy = port.pos.y;
//            console.log('doing a new connection', port.ox, port.oy);
        }
    };
    
    Element.prototype.onPortDragMove = function(port, dx, dy, x, y, e) {
        
        //console.log('onPortDragMove %o %o %s %s', this, port, dx, dy);

        if (!port) {
            console.log('??? onPortDragMove this isn\'t as expected', this);
            return;
        }

        // allow for zoomed view port
        var zoom = this.diagram.zoom();
        if (zoom) {
            dx = dx / zoom;
            dy = dy / zoom;
        }
        port.dx = dx;
        port.dy = dy;

        if (!port.ox) {
            return;
        }

        // draw our drag line

        var paper = this.diagram.paper,
            path = 'M' + port.ox + ',' + port.oy + 'L' + (port.pos.x + dx) + ',' + (port.pos.y + dy);
        
        if (!port.dragLine) {
            port.dragLine = paper.path(path).attr('stroke-dasharray', '. ');
        } else {
            port.dragLine.attr('path', path);
        }
    };
    Element.prototype.onPortDragEnd = function (port, e) {


        if (!port || !port.dragLine) {
            console.log('??? onPortDragEnd this isn\'t as expected', this);
            return;
        }

        // Check if nothing has been dragged - if so ignore
        if (!port.dx && !port.dy) {
            clearDragLine(port);
            return;
        }

        if (!port.ox) {
            clearDragLine(port);
            console.log('??? onPortDragEnd this isn\'t as expected', this);
            return;
        }

        var path = port.dragLine,
            endPt = path.getPointAtLength(path.getTotalLength()),
            bbox = { x: endPt.x - 5, y: endPt.y - 5, width: 10, height: 10 };


        var droppedOn = this.diagram.elements.filter(function (el) {
            if (el.movable) { // todo - use a different flag to know if can drop a connection
                if (Raphael.isBBoxIntersect(el.getElementBBox(), bbox)) {
                    return true;
                }
            }
            return false;
        });

        clearDragLine(port);

        console.log('port dropped on', droppedOn);

        if (droppedOn.length && droppedOn[0].modelElement) {

            var droppedOnElem = droppedOn[0].modelElement;

            if (droppedOnElem.template === 'startTemplate') {                        // ignore dropping on start
                return;
            }

            if (port.isStartPort && droppedOnElem.template === 'endTemplate') {      // ignore going from start to end
                return;
            }


            if (port.dragConnection) {
                // update the existing connection

                var c = port.dragConnection;

                // for the moment just grab the first element, later we should check for the port shapes - todo
                if (this.diagram.connectionChanged) {
                    this.diagram.connectionChanged(c, droppedOn[0], null, c.toElement);
                }

            } else {
                // this is a new connection

                // for the moment just grab the first element in this list of dropped on elements, later we should check for the port shapes - todo
                if (this.diagram.newConnection) {
                    this.diagram.newConnection(this, port, droppedOn[0]);
                }
            }
        }


    };

    function clearDragLine(port) {
        if (port && port.dragLine) {
            port.dragLine.remove();
            port.dragLine = null;
        }
    }

    Diagrammer.Element = Element;




}(Diagrammer || (Diagrammer = {})));
