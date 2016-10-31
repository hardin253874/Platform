// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*jslint newcap: true */
/*global _, console, angular, spUtils, Raphael */

//todo - stop creating and destroying elements when highlighting things... maybe to show and hide instead

var Diagrammer;
(function (Diagrammer) {
    'use strict';

    var Diagram = function (options) {

        this.width = options.width || 800;
        this.height = options.height || 600;
        this._zoom = options.zoom || 1;

        // save the options object... in case it has any optional callbacks
        this.options = options;

        this.paper = Raphael(options.target, this.width, this.height);

        // the background is larger than the display. This is to get around issues when trying to resize the background object.
        this.backgroundElement = this.paper.rect(0, 0, 10000, 10000).attr({ 'stroke-width': 0, 'fill-opacity': 1 });
        var be = this.backgroundElement;

        // This is a HACK! It's the only way I could use SVG patterns with Raphael
        setTimeout(function () {
            be.node.attributes.fill.value = 'url(#grid)';
        }, 1);

        if (options.onSelected) {
            this.backgroundElement.click(function () {
                console.log('background clicked');
                options.onSelected(null);
            });
        }

        this.elementTemplates = options.elementTemplates || [];

        this.elements = [];
        this.connections = [];
        this.tools = [];

        // track the last visited element for positioning purposes
        this.prevElement = null;

        // some visual state helpers for the diagram
        this.hideToolsTimeout = null;
        this.draggingConnection = null;
    };

    Diagram.prototype.onDragOver = function(event, data) {
        var x = event.offsetX,
            y = event.offsetY;

        data.x = x;
        data.y = y;
//        console.log('diagram - ondragover', this.maybeShape);
        if (this.maybeShape) {
            //this.maybeShape.attr({x: x, y: y});
            //this.maybeShape.move(x, y);
            this.updateObject(this.maybeShape, data);
        } else {
//            var template = this.elementTemplates['activityTemplate'];
//            var src = { name: data.name, label: data.name, x: x, y: y };
//            template = Diagrammer.fillTemplate(template, src);
//            this.maybeShape = this.paper.add(template);
//            //this.maybeShape = this.paper.rect(x, y, 20, 20).attr({fill: 'red', opacity: 0.5});
            this.maybeShape = this.createElement(data);
            this.elements.push(this.maybeShape);
        }
    };

    Diagram.prototype.onDragLeave = function(event, data) {
//        console.log('diagram - ondragleave', this.maybeShape);
        if (this.maybeShape) {
            //this.maybeShape.remove();
            this.elements = _.without(this.elements, this.maybeShape);
            this.removeObject(this.maybeShape);
            this.maybeShape = null;
        }
    };

    Diagram.prototype.findElement = function(id) {
        return spUtils.findByKey(this.elements, 'id', id);
    };

    Diagram.prototype.findConnection = function(id) {
        return spUtils.findByKey(this.connections, 'id', id);
    };

    Diagram.prototype.zoom = function (zoom) {
        if (_.isUndefined(zoom)) {
            return this._zoom;
        }
        this._zoom = zoom;
        this.paper.setViewBox(0, 0, (this.width / zoom), (this.height / zoom), false);
        this.paper.canvas.setAttribute('preserveAspectRatio', 'none');            // workaround see https://github.com/DmitryBaranovskiy/raphael/issues/649
                        
        return this;
    };


    Diagram.prototype.setDimentions = function (width, height) {

        this.width = width;
        this.height = height;
        this.paper.setSize(this.width, this.height);
        this.zoom(this._zoom);
        return this;
    };

    Diagram.prototype.compareById = function (a, b) {
        return a.id === b.id ? 0 : a.id < b.id ? -1 : 1;
    };

    Diagram.prototype.createElement = function (src) {
        return new Diagrammer.Element(this, src);
    };

    Diagram.prototype.createConnection = function (src) {
        return new Diagrammer.Connection(this, src);
    };

    Diagram.prototype.createTool = function (src) {
        return new Diagrammer.Tool(this, src);
    };

    Diagram.prototype.removeObject = function (dest) {
        dest.cleanUp();
    };

    Diagram.prototype.updateObject = function (dest, src) {
        dest.updateFromModel(src);
    };

    Diagram.prototype.syncDiagram = function (srcDiagramModel) {

        this.prevElement = this.backgroundElement;

        console.time('syncDiagram');

        // note - sync elements before the connections
        //console.time('syncDiagram - elements');
        spUtils.syncArrays(srcDiagramModel.elements, this.elements, this.compareById, this.createElement, this.updateObject, this.removeObject, this);
        //console.timeEnd('syncDiagram - elements');

        //console.time('syncDiagram - connections');
        spUtils.syncArrays(srcDiagramModel.connections, this.connections, this.compareById, this.createConnection, this.updateObject, this.removeObject, this);
        //console.timeEnd('syncDiagram - connections');

        spUtils.syncArrays(srcDiagramModel.tools, this.tools, this.compareById, this.createTool, this.updateObject, this.removeObject, this);

        console.timeEnd('syncDiagram');
    };

    Diagram.prototype.onPortClick = function (el, port, x, y) {
        if (this.options.onPortClick) {
            this.options.onPortClick(el, port, x, y);
        }
    };

    Diagram.prototype.toolClick = function (tool, x, y) {
        if (this.options.onToolClick) {
            this.options.onToolClick(tool, x, y);
        }
    };

    Diagram.prototype.elementMoved = function (el, x, y) {
        if (this.options.onElementMoved) {
            this.options.onElementMoved(el, x, y);
        }
    };

    Diagram.prototype.connectionChanged = function (c, el1, port, el2) {
        if (this.options.onConnectionChanged) {
            this.options.onConnectionChanged(c, el1, port, el2);
        }
    };

    Diagram.prototype.newConnection = function (el1, port, el2) {
        if (this.options.onConnectionCreated) {
            this.options.onConnectionCreated(el1, port, el2);
        }
    };

    Diagram.prototype.setSelectedObject = function (obj) {
        if (this.options.onSelected) {
            this.options.onSelected(obj);
        }
    };

    Diagram.prototype.elementMoving = function (el, x, y) {
        this.updateConnectionsForElement(el);
    };

    Diagram.prototype.updateConnectionsForElement = function (el) {
        _.each(this.connections, _.bind(function (c) {
            if (c.fromElement === el || c.toElement === el) {
                c.updateFromModel();
            }
        }, this));
    };

    Diagram.prototype.showTools = function (target) {
        if (this.hideToolsTimeout) {
            clearTimeout(this.hideToolsTimeout);
            this.hideToolsTimeout = null;
        }
        this.tools.forEach(function (tool) {
            tool.attachTool(target);
        });
    };

    Diagram.prototype.hideTools = function () {
        if (!this.hideToolsTimeout) {
            var that = this;
            this.hideToolsTimeout = setTimeout(function () {
                that.tools.forEach(function (tool) {
                    tool.detachTool();
                });
                that.hideToolsTimeout = null;
            }, 300);
        }
    };

    Diagram.prototype.hideToolsFast = function () {
        if (this.hideToolsTimeout) {
            clearTimeout(this.hideToolsTimeout);
            this.hideToolsTimeout = null;
        }
        this.tools.forEach(function (tool) {
            tool.detachTool(true);
        });
    };

    Diagrammer.Diagram = Diagram;

}(Diagrammer || (Diagrammer = {})));
