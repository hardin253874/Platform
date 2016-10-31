// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular, spUtils, Raphael */

var Diagrammer;
(function (Diagrammer) {
    'use strict';

    var Connection = function (diagram, src) {
        this.diagram = diagram;
        this.id = src.id;
        this.updateFromModel(src);
    };

    Connection.prototype.cleanUp = function (c) {
        this.deselect();
        if (this.shapes) {
            this.shapes.remove();
        }
    };

    Connection.prototype.updateFromModel = function (src) {

        // Notes...
        // We can be called with a null src - it happens when we are redrawing during an element drag.
        // Assume the diagram elements have been synced.

        if (src) {
            this.fromElement = this.diagram.findElement(src.from);
            this.toElement = this.diagram.findElement(src.to);
        }

        if (!this.fromElement || !this.toElement) {
            console.error('warning - connection with missing end point', src);
            return;
        }
        //console.log('outputs for %d', this.id, this.fromElement.outPorts);

        //todo fix this confusing crap (of mine)... src.fromPort is an id whereas this.fromPort is an object!

        var fromPortId = src ? src.fromPort : this.fromPort ? this.fromPort.id : null,
            toPortId = src ? src.toPort : this.toPort ? this.toPort.id : null;

        this.fromPort = this.fromElement.getOutPort(fromPortId);
        this.toPort = this.toElement.getInPort(toPortId);

        this.label = this.fromPort.name;

        this.deselect();
        this.drawConnection();
        if (src && src.selected) {
            this.select();
        }
    };

    Connection.prototype.drawConnection = function () {

        var fromShapes = this.fromPort.shapes,
            toShapes = this.toPort.shapes,
            b1 = fromShapes.getBBox(),
            b2 = toShapes.getBBox();

//        console.log('drawConnection %o %s %o', this, this.id, this.fromPort);

        b1.cx = b1.x + b1.width / 2;
        b1.cy = b1.y + b1.height / 2;
        b2.cx = b2.x + b2.width / 2;
        b2.cy = b2.y + b2.height / 2;

        // rough - but trying to get a screen shot together

        var pt1, pt2, fromPt, endPt,
            points = [],
            exitDg = 90, enterDg = 90,
            minX = 15, minY = 15,           // the shortest arrow before it needs to be broken
            x, y;

        // - determine the points to draw between
        // - have fixed segments out of the enter and into the exit -> segments
        // - pt1 and pt2 is where to calc a path, endPt is the end where to do the arrow

        if (this.fromPort && this.fromPort.pos) {
            exitDg = this.fromPort.posDg;
            fromPt = this.fromPort.pos;
            if (exitDg === 90) {
                pt1 = { x: fromPt.x + minX + (this.fromPort.dx || 0), y: fromPt.y };
            } else if (exitDg === 180) {
                pt1 = { x: fromPt.x, y: fromPt.y + minY };
            } else if (exitDg === 270) {
                pt1 = { x: fromPt.x - minX, y: fromPt.y };
            } else if (exitDg === 0) {
                pt1 = { x: fromPt.x, y: fromPt.y - minY };
            }
        } else {
            exitDg = 90; // 3 o'clock
            fromPt = { x: b1.x2, y: b1.cy };
            pt1 = { x: fromPt.x + minX, y: fromPt.y };
        }

        points.push({ x: fromPt.x, y: fromPt.y });
        points.push(pt1);

        var toHasSouthExit = this.toElement.outPorts.some(function (p) {
            return p.posDg === 180;
        });
        //console.log('to', this.toElement.name, this.toElement.outPorts, toHasSouthExit);

        if (exitDg === 90 && b2.y > b1.y2) {
            enterDg = 0;
            endPt = { x: b2.cx, y: b2.y };
            pt2 = { x: b2.cx, y: b2.y - minY };
        } else if (exitDg === 90 && b2.y2 < b1.y && !toHasSouthExit) {
            enterDg = 180;
            endPt = { x: b2.cx, y: b2.y2 };
            pt2 = { x: b2.cx, y: b2.y2 + minY };
        } else if (exitDg === 180 && b2.y2 < pt1.y && !toHasSouthExit) {
            enterDg = 180;
            endPt = { x: b2.cx, y: b2.y2 };
            pt2 = { x: b2.cx, y: b2.y2 + minY };
        } else if (exitDg === 180 && b2.x < b1.cx && b2.y > b1.y2) {
            enterDg = 0;
            endPt = { x: b2.cx, y: b2.y };
            pt2 = { x: b2.cx, y: b2.y - minY };
        } else {
            enterDg = 270;
            endPt = { x: b2.x, y: b2.cy };
            pt2 = { x: b2.x - minX, y: b2.cy };
        }

        var midPt = {
            x: pt1.x + (pt2.x - pt1.x) / 2,
            y: pt1.y + (pt2.y - pt1.y) / 2
        };

        if (exitDg === 90 && enterDg === 270) {
            if (pt1.x < pt2.x) {
                points.push({ x: midPt.x, y: pt1.y });
                points.push({ x: midPt.x, y: pt2.y });
            } else if (pt1.y < pt2.y) {
                points.push({ x: pt1.x, y: b2.y2 + minY });
                points.push({ x: pt2.x, y: b2.y2 + minY });
            } else {
                points.push({ x: pt1.x, y: b1.y2 + minY });
                points.push({ x: pt2.x, y: b1.y2 + minY });
            }
        } else if (exitDg === 90 && enterDg === 0) {
            if (pt1.x > pt2.x) {
                y = midPt.y + minY + (this.fromPort.dy || 0);
                points.push({ x: pt1.x, y: y });
                points.push({ x: pt2.x, y: y });
            } else {
                points.push({ x: pt2.x, y: pt1.y });
            }

        } else if (exitDg === 90 && enterDg === 180) {
            if (pt1.x < pt2.x) {
                points.push({ x: pt2.x, y: pt1.y });
            } else if (pt1.y < pt2.y) {
                points.push({ x: pt1.x, y: midPt.y });
                points.push({ x: pt2.x, y: midPt.y });
            } else {
                y = b1.y2 + minY * 1.5 + (this.fromPort.dy || 0);
                points.push({ x: pt1.x, y: y });
                points.push({ x: pt2.x, y: y });
            }

        } else if (exitDg === 180 && enterDg === 270) {
            if (pt1.y < pt2.y) {
                points.push({ x: pt1.x, y: pt2.y });
            } else {
                points.push({ x: pt2.x, y: pt1.y });
            }

        } else if (exitDg === 180 && enterDg === 0) {
            points.push({ x: pt1.x, y: midPt.y });
            points.push({ x: pt2.x, y: midPt.y });

        } else if (exitDg === 180 && enterDg === 180) {
            points.push({ x: pt2.x, y: pt1.y });
        }

        points.push(pt2);
        points.push(endPt);

        // save the points for drawing and later use such as when dragging

        this.points = points;
        this.enterDg = enterDg;

        this.drawPath();
    };

    /**
     * Draw a path based on the points array and an arrow head at the end.
     * Note - should calc the angle of the arrow here rather than expect it in enterDg.
     */
    Connection.prototype.drawPath = function () {

        if (!this.points || !this.points.length) {
            return;
        }

        var paper = this.diagram.paper,
            endPt = this.points[this.points.length - 1],
            path, arrowPath,
            l = 9,                          // size of arrow head
            h = l * Math.sqrt(3) / 1.5;

        path = '';
        this.points.forEach(function (pt, i) {
            path += (i === 0 ? 'M' : 'L') + pt.x + ',' + pt.y;
        });

        if (this.shapes && this.shapes.length && this.shapes[0].attr('path').toString() === path) {
            return;
        }

        // clean up before redrawing

        this.unhighlight();
        if (this.shapes) {
            this.shapes.remove();
        }

        // create a new path and arrow

        arrowPath = 'M' + endPt.x + ',' + endPt.y +
            'L' + (endPt.x - h / 2) + ',' + (endPt.y - l) +
            'L' + (endPt.x + h / 2) + ',' + (endPt.y - l) +
            'L' + endPt.x + ',' + endPt.y;

        this.shapes = paper.set();
        this.shapes.push(paper.path(path).attr('stroke', '#5d5d5d'));
        this.shapes.push(paper.path(path).attr({ stroke: '#aaaaaa', 'opacity': 0, 'stroke-width' : 15 }));
        this.shapes.push(paper.path(arrowPath)
            .attr({ 'stroke': '#5d5d5d', 'fill': '#5d5d5d', 'fill-opacity': 1 })
            .transform('R' + this.enterDg + ',' + endPt.x + ',' + endPt.y));

        this.shapes.insertBefore(this.toElement.shapes);

        // add events

        this.shapes.hover(this.onMouseOver, this.onMouseOut, this, this);
        this.shapes.click(this.onClick, this);
    };

    Connection.prototype.select = function () {

        var paper = this.diagram.paper,
            path = this.shapes[0],
            pt1 = path.getPointAtLength(0),
            pt2 = path.getPointAtLength(path.getTotalLength()),
            endPointAttrs;

        this.selected = true;
        this.highlight();

        if (!this.startNode) {
            endPointAttrs = { stroke: 'black', 'stroke-opacity': 0, 'fill': 'black', 'fill-opacity': 0.2 };
            this.startNode = paper.circle(pt1.x, pt1.y, 5).attr(endPointAttrs);
            this.endNode = paper.circle(pt2.x, pt2.y, 5).attr(endPointAttrs);
            this.startNode.drag(this.onDragMove, this.onDragStart, this.onDragEnd, this, this, this);
            this.endNode.drag(this.onDragMove, this.onDragStart, this.onDragEnd, this, this, this);
        }
    };
    
    Connection.prototype.deselect = function () {

        this.selected = false;
        this.unhighlight();
        
        if (this.startNode) {
            this.startNode.remove();
            this.startNode = null;
            this.endNode.remove();
            this.endNode = null;
        }
    };

    Connection.prototype.highlight = function(color) {
        if (!this.glow) {
            var glow = { color: color || 'red', opacity: 1, fill: false, width: 7 };
            this.glow = this.shapes[0].glow(glow);
            this.glow.click(this.onClick, this);
            this.glow.hover(this.onMouseOver, this.onMouseOut, this, this);
        }
    };

    Connection.prototype.unhighlight = function () {
        if (this.glow) {
            this.glow.remove();
            this.glow = null;
        }
    };

    Connection.prototype.getConnectionEndPoints = function () {
        if (!this.points || !this.points.length) {
            throw new Error('Unexpected missing connection or connection points array');
        }
        return [this.points[0], this.points[this.points.length - 1]];
    };

    Connection.prototype.onMouseOver = function (e) {
        if (this.hideConnectionHighlightTimeout) {
            clearTimeout(this.hideConnectionHighlightTimeout);
            this.hideConnectionHighlightTimeout = null;
        }
        this.highlight('grey');
    };

    Connection.prototype.onMouseOut = function (e) {
        if (!this.hideConnectionHighlightTimeout) {
            var that = this;
            this.hideConnectionHighlightTimeout = setTimeout(function () {
                if (!that.selected) {
                    that.unhighlight();
                }
            }, 500);
        }
    };

    Connection.prototype.onClick = function (e) {
        this.diagram.setSelectedObject(this);
    };

    Connection.prototype.onDragStart = function (x, y, e) {

        //console.log('connection.onDragStart %d %d %o', x, y, e.target);

        if (e.target !== this.endNode.node && e.target !== this.startNode.node) {
            console.error('unexpected connection.onDragStart with unexpected target');
            return;
        }

        // save our dragging state
        // including the connection end points and noting the one that is being dragged
        this.diagram.draggingConnection = this;
        this.dragging = {
            points: this.getConnectionEndPoints(),
            movingIndex: e.target === this.endNode.node ? 1 : 0
        };
    };
    Connection.prototype.onDragMove = function (dx, dy, x, y, e) {

        //console.log('connection.onDragMove x=%d y=%d target=%o dragging=%o', dx, dy, e.target, this.dragging);

        if (!this.dragging) {
            return;
        }

        var paper = this.diagram.paper,
            dragging = this.dragging,
            pointToMove = dragging.points[dragging.movingIndex],
            otherPoint = dragging.points[(dragging.movingIndex + 1) % 2];

        // allow for zoomed view port
        var zoom = this.diagram.zoom();
        if (zoom) {
            dx = dx / zoom;
            dy = dy / zoom;
        }

        // draw a drag line from the 'other end' of the connection to the move pos

        var path = 'M' + otherPoint.x + ',' + otherPoint.y + 'L' + (pointToMove.x + dx) + ',' + (pointToMove.y + dy);
        if (!dragging.dragLine) {
            dragging.dragLine = paper.path(path).attr('stroke-dasharray', '. ');
        } else {
            dragging.dragLine.attr('path', path);
        }
    };

    Connection.prototype.onDragEnd = function (e) {

        //console.log('connection.onDragEnd %o %o', this, e);

        if (this.diagram.draggingConnection !== this || !this.dragging || !this.dragging.dragLine) {
            return;
        }

        this.diagram.draggingConnection = null;

        var dragging = this.dragging,
            path = dragging.dragLine,
            endPt = path.getPointAtLength(path.getTotalLength()),
            bbox = { x: endPt.x - 5, y: endPt.y - 5, width: 10, height: 10 };

        dragging.dragLine.remove();
        dragging.dragLine = null;

        var droppedOn = this.diagram.elements.filter(function (el) {
            if (el.movable) { // todo - use a different flag to know if can drop a connection
                if (Raphael.isBBoxIntersect(el.getElementBBox(), bbox)) {
                    return true;
                }
            }
            return false;
        });

        if (droppedOn.length) {
            // for the moment just grab the first element, later we should check for the port shapes - todo
            if (this.diagram.connectionChanged) {
                if (dragging.movingIndex === 1) {
                    this.diagram.connectionChanged(this, this.fromElement, this.fromPort, droppedOn[0]);
                } else {
                    //todo - work out the from exit point
                    this.diagram.connectionChanged(this, droppedOn[0], null, this.toElement);
                }
            }
        }
    };

    Diagrammer.Connection = Connection;

}(Diagrammer || (Diagrammer = {})));
