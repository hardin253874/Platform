// Copyright 2011-2016 Global Software Innovation Pty Ltd
/*global _, console, angular */

var Diagrammer;
(function (Diagrammer) {
    'use strict';

    var Tool = function (diagram, src) {
        this.diagram = diagram;
        this.id = src.id;
        this.updateFromModel(src);
    };

    Tool.prototype.cleanUp = function () {
        if (this.shapes) {
            this.shapes.remove();
            this.shapes = null;
        }
    };

    Tool.prototype.updateFromModel = function (src) {

        var template = this.diagram.elementTemplates[src.template];
        //todo - fix up this label vs name business
        src.label = src.name;
        template = Diagrammer.fillTemplate(template, src);

        if (!angular.equals(this.template, template)) {

            this.template = template;
            this.cleanUp(); // since we are recreating....
            this.shapes = this.diagram.paper.add(template);
            this.shapes.attr('opacity', 0);
            this.x = 0;
            this.y = 0;
            this.position = src.position;

            // Get and remember the initial set of transforms for this element set
            this.transforms = [];
            this.shapes.forEach(function (el) {
                this.transforms.push(el.matrix.toTransformString());
            }, this);

            // event handlers
            this.shapes.hover(this.onMouseOver, this.onMouseOut, this, this);
            this.shapes.click(this.onClick, this);
        }
    };

    Tool.prototype.attachTool = function (target) {

        var tool = this;

        //console.log('attach tool ', tool.id, tool.targetShape, target.id);
        if (tool.targetShape) {
            this.detachTool(true);
        }
        if (!target.tools || !_.some(target.tools, function (t) {
            return t === tool.id;
        })) {
            return;
        }
        tool.targetShape = target;
        if (tool.shapes) {
            //todo - add some positioning info rather than top right
            var bbox = target.getElementBBox(),
                x = bbox.x2, y = bbox.y,
                p = tool.position;
            tool.shapes.insertAfter(target.shapes);
            if (p) {
                // expect members a: the angle from 12 oclock, and r a multiplier of radius
                // think of it like a position on a clock.... no reason for this way of doing it, just made it up then
                // todo - calc better, just rough for now
                if (p.a > 0 && p.a < 180) {
                    x = bbox.x2;
                } else {
                    x = bbox.x;
                }
                if (p.a > 270 || p.a < 90) {
                    y = bbox.y;
                } else {
                    y = bbox.y2;
                }
            }
            tool.move(x, y);
            tool.shapes.animate({ 'opacity': 0.8 }, 500);
        }
    };

    Tool.prototype.detachTool = function (fast) {
        var tool = this;
        //console.log('detach tool ', tool.id, tool.targetShape, fast);
        tool.targetShape = null;
        if (tool.shapes) {
            tool.shapes.animate({ 'opacity': 0 }, fast ? 0 : 500);
        }
    };

    Tool.prototype.move = function (x, y) {
        console.log('tool moveTo %s %s,%s => %s,%s', this.id, this.x, this.y, x, y);

        if (this.x !== x || this.y !== y) {

            this.x = x;
            this.y = y;
            this.shapes.forEach(function (shape, i) {
                shape.transform('T' + x + ',' + y + (this.transforms[i] || ''));
            }, this);
        }
    };

    Tool.prototype.onMouseOver = function (e) {
        if (this.targetShape) {
            this.diagram.showTools(this.targetShape);
        }
    };
    Tool.prototype.onMouseOut = function (e) {
        this.diagram.hideTools();
    };

    Tool.prototype.onClick = function (e) {
        console.log('tool clicked %s %d %d', this.id, e.x, e.y);
        if (this.targetShape) {
            this.diagram.toolClick(this, e.x, e.y);
        }
    };

    Diagrammer.Tool = Tool;

}(Diagrammer || (Diagrammer = {})));
