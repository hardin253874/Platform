(function () {
    var d3, indexOf = [].indexOf || function (item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

    d3 = window.d3;

    if (!d3) {
        throw new Error("d3 not loaded");
    }

    d3.perfGraph = function () {
        var PerfGraph, visitNodes, getNodeDepth, pal;

        //pal = [ "#ffffe4", "#fffee0", "#fffdda", "#fffbd5", "#fff9cf", "#fff8c8", "#fff5c1", "#fff3b9", "#fff0b1", "#ffeda9", "#ffeaa0", "#ffe797", "#ffe38e", "#ffdf84", "#ffda7b", "#ffd572", "#ffd069", "#ffca60", "#fec557", "#febf4e", "#feb847", "#feb23f", "#fdab38", "#fca432", "#fb9d2c", "#fa9626", "#f98f21", "#f7881d", "#f58119", "#f37a16", "#f07313", "#ec6c10", "#e8660e", "#e35f0c", "#de590a", "#d85409", "#d14e08", "#ca4907", "#c34506", "#bb4006", "#b23c05", "#a93805", "#a03505", "#973204", "#8e2f04", "#852c04", "#7d2a04", "#752804", "#6d2604", "#662404" ];
        pal = ["#FFFF00", "#FFF900", "#FFF400", "#FFEF00", "#FFEA00", "#FFE400", "#FFDF00", "#FFDA00", "#FFD500", "#FFD000", "#FFCA00", "#FFC500", "#FFC000", "#FFBB00", "#FFB600", "#FFB000", "#FFAB00", "#FFA600", "#FFA100", "#FF9C00", "#FF9600", "#FF9100", "#FF8C00", "#FF8700", "#FF8200", "#FF7C00", "#FF7700", "#FF7200", "#FF6D00", "#FF6800", "#FF6200", "#FF5D00", "#FF5800", "#FF5300", "#FF4E00", "#FF4800", "#FF4300", "#FF3E00", "#FF3900", "#FF3400", "#FF2E00", "#FF2900", "#FF2400", "#FF1F00", "#FF1A00", "#FF1400", "#FF0F00", "#FF0A00", "#FF0500", "#FF0000"];

        visitNodes = function (nodes, node, depth) {
            nodes.push(node);
            node.depth = depth;
            if (node.children) {
                node.children.forEach(function (c) { visitNodes(nodes, c, depth + 1); });
            }
        };

        getNodeDepth = function (node) { return node.depth; };

        PerfGraph = (function () {
            function PerfGraph() {
                this._generateAccessors(['size', 'margin', 'tooltip']);
                this._ancestors = [];
                this._ancestorDepth = 0;
                this._nodes = [];
                this._size = [1900, 600];
                this._margin = { top: 10, right: 10, bottom: 10, left: 10 };
            }

            PerfGraph.prototype.data = function (data) {
                if (!data) {
                    return this._data;
                }
                if (!this.original) {
                    this.original = data;
                }
                this._data = data;
                this._nodes.length = 0;
                visitNodes(this._nodes, data, this._ancestorDepth);
                return this;
            };

            PerfGraph.prototype.width = function () {
                return this.size()[0] - (this.margin().left + this.margin().right);
            };

            PerfGraph.prototype.height = function () {
                return this.size()[1] - (this.margin().top + this.margin().bottom) - 30;
            };

            PerfGraph.prototype.zoom = function (node) {
                if (this.tooltip()) { this.tip.hide(); }

                // if an ancestor was clicked, reset the zoom.
                if (indexOf.call(this._ancestors, node) >= 0) {
                    this._ancestors.length = 0;
                    this._ancestorDepth = 0;
                    this.data(this.original).render(this._selector);
                } else {
                    var n = this._nodes.indexOf(node);
                    if (n >= 0) {
                        var ancestors = this._ancestors.concat(this._nodes.slice(0, n));
                        this._ancestors = ancestors.filter(function (a) {
                            return a.start <= node.start && a.stop >= node.stop;
                        });
                        this._ancestorDepth = 1 + d3.max(this._ancestors, getNodeDepth);
                        this.data(node).render(this._selector);
                    }
                }

                return this;
            };

            PerfGraph.prototype.render = function (selector) {
                if (!(this._selector || selector)) {
                    throw new Error("The container's selector needs to be provided before rendering");
                }
                if (selector) {
                    this._selector = selector;
                }
                d3.select(selector).select('svg').remove();

                this.fontSize = 0.8;
                this.maxDepth = 1 + d3.max(this._nodes, getNodeDepth);
                this.minTime = d3.min(this._nodes, function (node) { return node.start; });
                this.maxTime = d3.max(this._nodes, function (node) { return node.stop; });
                this.colorPartition = Math.floor(pal.length * (1 / (this.maxDepth - 1)));
                this.timeScale = d3.scale.linear().domain([this.minTime, this.maxTime]).range([0, this.width()]);
                this.depthScale = d3.scale.linear().domain([0, this.maxDepth]).range([this.height(), 0]);
                this.depthSpace = this.depthScale(0) - this.depthScale(1);
                this.container = d3.select(selector)
				.append('svg').attr('class', 'perf-graph').attr('width', this.size()[0]).attr('height', this.size()[1])
				.append('g').attr('transform', "translate(" + (this.margin().left) + ", " + (this.margin().top) + ")");

                var g = this.container.selectAll("g").data(this._nodes).enter().append("g")
				.attr('class', function (d, idx) { if (idx === 0) { return 'root node'; } else { return 'node'; } });

                this._renderNodes(g, {
                    y: (function (_this) { return function (d) { return _this.depthScale(d.depth) - _this.depthSpace; }; })(this),
                    x: (function (_this) { return function (d) { return _this.timeScale(d.start); }; })(this),
                    height: (function (_this) { return function (d) { return _this.depthSpace; }; })(this),
                    width: (function (_this) { return function (d) { return Math.max(1, _this.timeScale(d.stop) - _this.timeScale(d.start)); }; })(this),
                    text: (function (_this) { return function (d) { return d.label; }; })(this),
                    color: (function (_this) {
                        return function (d) {
                            if (!d.depth) return pal[0];
                            var r = Math.floor(Math.random() * _this.colorPartition);
                            var n = (d.depth * _this.colorPartition) - r;
                            if (n >= pal.length) {
                                n = pal.length - 1;
                            }
                            return pal[n];
                        };
                    })(this)
                });

                this._renderAncestors()._enableNavigation();

                if (this.tooltip()) {
                    this._renderTooltip();
                }

                var timeAxis = d3.svg.axis()
				.scale(this.timeScale)
				.orient("bottom");

                d3.select(".perf-graph").append('g')
				.attr('class', 'x axis')
				.attr('transform', "translate(" + (this.margin().left) + ", " + (this.height() + this.margin().top) + ")").call(timeAxis);

                return this;
            };

            PerfGraph.prototype._renderNodes = function (g, attrs) {
                g.append('rect')
				.attr('width', attrs.width)
				.attr('height', attrs.height)
				.attr('x', attrs.x)
				.attr('y', attrs.y)
				.attr('fill', attrs.color)
				.attr('stroke', attrs.color);
                g.append('text')
				.attr('class', 'label')
				.attr('dy', this.fontSize + "em")
				.attr('x', (function (_this) {
				    return function (d) {
				        return attrs.x(d) + 2;
				    };
				})(this))
				.attr('y', (function (_this) {
				    return function (d, idx) {
				        return attrs.y(d, idx) + 2;
				    };
				})(this))
				.style('font-size', this.fontSize + "em").text(attrs.text);
                g.append('rect')
				.attr('class', 'overlay')
				.attr('width', attrs.width)
				.attr('height', attrs.height)
				.attr('x', attrs.x)
				.attr('y', attrs.y);
                return this;
            };

            PerfGraph.prototype._renderTooltip = function () {
                this.tip = d3.tip().attr('class', 'd3-tip').html(this.tooltip())
				.direction((function (_this) { return function (d) { return 's'; }; })(this));
                this.container.call(this.tip);
                this.container.selectAll('.node').on('mouseover', this.tip.show).on('mouseout', this.tip.hide);
                return this;
            };

            PerfGraph.prototype._renderAncestors = function () {
                var g = this.container.selectAll('.ancestor').data(this._ancestors).enter().append('g').attr('class', 'ancestor');
                this._renderNodes(g, {
                    y: (function (_this) { return function (d) { return _this.depthScale(d.depth) - _this.depthSpace; }; })(this),
                    x: (function (_this) { return function (d) { return _this.timeScale(_this.minTime); }; })(this),
                    height: (function (_this) { return function (d) { return _this.depthSpace; }; })(this),
                    width: (function (_this) { return function (d) { return _this.width(); }; })(this),
                    text: (function (_this) { return function (d) { return "↩ " + d.label; }; })(this),
                    color: (function (_this) {
                        return function (d) {
                            if (!d.depth) return pal[0];
                            var r = Math.floor(Math.random() * _this.colorPartition);
                            var n = (d.depth * _this.colorPartition) - r;
                            return pal[n];
                        };
                    })(this)
                });
                return this;
            };

            PerfGraph.prototype._enableNavigation = function () {
                this.container.selectAll('.node').on('click', (function (_this) {
                    return function (d, idx) {
                        d3.event.stopPropagation();
                        if (idx > 0) {
                            return _this.zoom(d);
                        }
                    };
                })(this));
                this.container.selectAll('.ancestor').on('click', (function (_this) {
                    return function (d, idx) {
                        d3.event.stopPropagation();
                        return _this.zoom(_this._ancestors[idx]);
                    };
                })(this));
                return this;
            };

            PerfGraph.prototype._generateAccessors = function (accessors) {
                var accessor, j, len, results;
                results = [];
                for (j = 0, len = accessors.length; j < len; j++) {
                    accessor = accessors[j];
                    results.push(this[accessor] = (function (accessor) {
                        return function (newValue) {
                            if (!arguments.length) {
                                return this["_" + accessor];
                            }
                            this["_" + accessor] = newValue;
                            return this;
                        };
                    })(accessor));
                }
                return results;
            };

            return PerfGraph;
        })();

        return new PerfGraph();
    };

}).call(this);