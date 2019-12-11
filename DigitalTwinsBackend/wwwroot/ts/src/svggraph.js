"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
///<reference path="../typings/FileSaver/FileSaver.d.ts"/>
///<amd-dependency path="filesaver"/>
var G = require("./ggraph");
/** This class, and its concrete subclasses, correlate a geometry object with the SVG object that's currently rendering it.
Note that while the geometry object persists for the duration of the graph, the SVG object can be replaced. */
var RenderElement = /** @class */ (function () {
    function RenderElement(group) {
        this.group = group;
    }
    return RenderElement;
}());
var RenderNode = /** @class */ (function (_super) {
    __extends(RenderNode, _super);
    function RenderNode(node, group) {
        var _this = _super.call(this, group) || this;
        _this.node = node;
        return _this;
    }
    RenderNode.prototype.getGeometryElement = function () { return this.node; };
    return RenderNode;
}(RenderElement));
var RenderEdge = /** @class */ (function (_super) {
    __extends(RenderEdge, _super);
    function RenderEdge(edge, group) {
        var _this = _super.call(this, group) || this;
        _this.edge = edge;
        return _this;
    }
    RenderEdge.prototype.getGeometryElement = function () { return this.edge; };
    return RenderEdge;
}(RenderElement));
var RenderEdgeLabel = /** @class */ (function (_super) {
    __extends(RenderEdgeLabel, _super);
    function RenderEdgeLabel(edge, group) {
        var _this = _super.call(this, group) || this;
        _this.edge = edge;
        return _this;
    }
    RenderEdgeLabel.prototype.getGeometryElement = function () { return this.edge.label; };
    return RenderEdgeLabel;
}(RenderElement));
/** Renderer that targets SVG. */
var SVGGraph = /** @class */ (function () {
    function SVGGraph(container, graph) {
        this.grid = false;
        this.allowEditing = true;
        this.edgeRoutingCallback = null;
        this.layoutStartedCallback = null;
        this.workStoppedCallback = null;
        /** Set this to draw custom labels. Return true to suppress default label rendering, or false to render as default. Note
        that in order to draw a custom label, the node or edge needs to have a label to begin with. The easiest way is to create it
        with a label equal to "". If the element does not have any label at all, this function will not get invoked.
        @param svg The SVG container for the graph.
        @param parent The SVG element that contains the label.
        @param label The label.
        @param owner The element to which this label belongs. */
        this.customDrawLabel = null;
        this.style = "text { stroke: black; fill: black; stroke-width: 0; font-size: 15px; font-family: Verdana, Arial, sans-serif }";
        /** This callback gets invoked when the user clicks on a node. */
        this.onNodeClick = function (n) { };
        /** This callback gets invoked when the user clicks on an edge. */
        this.onEdgeClick = function (e) { };
        /** The point where the mouse cursor is at the moment, in graph space. */
        this.mousePoint = null;
        /** The graph element that's currently under the mouse cursor, if any. */
        this.elementUnderMouseCursor = null;
        this.container = container;
        this.container.style.position = "relative";
        this.graph = graph === undefined ? null : graph;
        var workingText = document.createTextNode("LAYOUT IN PROGRESS");
        var workingSpan = document.createElement("span");
        workingSpan.setAttribute("style", "position: absolute; top: 50%; width: 100%; text-align: center; z-index: 10");
        workingSpan.style.visibility = "hidden";
        workingSpan.appendChild(workingText);
        this.workingSpan = workingSpan;
        this.container.appendChild(this.workingSpan);
        this.hookUpMouseEvents();
    }
    SVGGraph.prototype.getGraph = function () { return this.graph; };
    SVGGraph.prototype.setGraph = function (graph) {
        var _this = this;
        if (this.graph != null) {
            this.graph.edgeRoutingCallbacks.remove(this.edgeRoutingCallback);
            this.graph.layoutStartedCallbacks.remove(this.layoutStartedCallback);
            this.graph.workStoppedCallbacks.remove(this.workStoppedCallback);
        }
        this.graph = graph;
        var that = this;
        this.edgeRoutingCallback = function (edges) {
            if (edges != null)
                for (var e in edges)
                    that.redrawElement(that.renderEdges[edges[e]]);
        };
        this.graph.edgeRoutingCallbacks.add(this.edgeRoutingCallback);
        this.layoutStartedCallback = function () {
            if (_this.graph.nodes.length > 0)
                that.workingSpan.style.visibility = "visible";
        };
        this.graph.layoutStartedCallbacks.add(this.layoutStartedCallback);
        this.workStoppedCallback = function () {
            that.workingSpan.style.visibility = "hidden";
        };
        this.graph.workStoppedCallbacks.add(this.workStoppedCallback);
    };
    SVGGraph.prototype.getSVGString = function () {
        if (this.svg == null)
            return null;
        var currentViewBox = this.svg.getAttribute("viewBox");
        var currentPreserve = this.svg.getAttribute("preserveAspectRatio");
        var bbox = this.graph.boundingBox;
        var offsetX = bbox.x;
        var offsetY = bbox.y;
        var maxViewBox = "" + offsetX + " " + offsetY + " " + bbox.width + " " + bbox.height;
        this.svg.setAttribute("viewBox", maxViewBox);
        this.svg.removeAttribute("preserveAspectRatio");
        var ret = (new XMLSerializer()).serializeToString(this.svg);
        this.svg.setAttribute("viewBox", currentViewBox);
        this.svg.setAttribute("preserveAspectRatio", currentPreserve);
        return ret;
    };
    SVGGraph.prototype.saveAsSVG = function (fileName) {
        fileName = fileName || "graph.svg";
        var svgString = this.getSVGString();
        var blob = new Blob([svgString], { type: "image/svg+xml" });
        saveAs(blob, fileName);
    };
    SVGGraph.prototype.pathEllipse = function (ellipse, continuous) {
        // Note that MSAGL's representation of ellipses can handle axes that are not horizontal or vertical - but at the moment I can't.
        var center = ellipse.center;
        // Grab the horizontal and vertical axes. These could be either A or B.
        var yAxis = (ellipse.axisB.y == 0) ? ellipse.axisA.y : ellipse.axisB.y;
        var xAxis = (ellipse.axisA.x == 0) ? ellipse.axisB.x : ellipse.axisA.x;
        // Grab their absolute values.
        yAxis = Math.abs(yAxis);
        xAxis = Math.abs(xAxis);
        // Degenerate case: do nothing. Note that it still works if I just proceed from here, but it's a waste of time.
        if (yAxis == 0 || xAxis == 0)
            return "";
        // Grab flags that describe the direction of the arc and axes. I'm going to use these to rotate and flip my way back to the
        // normal case.
        var counterClockwise = ellipse.axisA.x * ellipse.axisB.y - ellipse.axisB.x * ellipse.axisA.y > 0;
        var aHorz = ellipse.axisA.x != 0;
        var aPos = ellipse.axisA.x > 0 || ellipse.axisA.y > 0;
        var parStart = ellipse.parStart;
        var parEnd = ellipse.parEnd;
        var path = "";
        // The SVG path command is unable to draw a complete ellipse (or an ellipse that is very close to complete), so I need to treat it as a special case.
        var isFullEllipse = Math.abs(Math.abs(parEnd - parStart) - 2 * Math.PI) < 0.01;
        if (isFullEllipse) {
            var firstHalf = new G.GEllipse(ellipse);
            var secondHalf = new G.GEllipse(ellipse);
            firstHalf.parEnd = (ellipse.parStart + ellipse.parEnd) / 2;
            secondHalf.parStart = (ellipse.parStart + ellipse.parEnd) / 2;
            path += this.pathEllipse(firstHalf, continuous);
            path += this.pathEllipse(secondHalf, true);
        }
        else {
            // Rotate/flip the angle so that I can get back to the normal case (i.e. A horizontal positive, B vertical positive).
            var rots = aHorz ? aPos ? 0 : 2 : (aPos == counterClockwise) ? 1 : 3;
            parStart += Math.PI * rots / 2;
            parEnd += Math.PI * rots / 2;
            if (!counterClockwise) {
                parStart = -parStart;
                parEnd = -parEnd;
            }
            // Proceed as in the normal case.
            var startX = center.x + xAxis * Math.cos(parStart);
            var startY = center.y + yAxis * Math.sin(parStart);
            var endX = center.x + xAxis * Math.cos(parEnd);
            var endY = center.y + yAxis * Math.sin(parEnd);
            var largeArc = Math.abs(parEnd - parStart) > Math.PI;
            var sweepFlag = counterClockwise;
            path += (continuous ? " L" : " M") + startX + " " + startY;
            path += " A" + xAxis + " " + yAxis;
            path += " 0"; // x-axis-rotation
            path += largeArc ? " 1" : " 0";
            path += sweepFlag ? " 1" : " 0";
            path += " " + endX + " " + endY;
        }
        return path;
    };
    SVGGraph.prototype.pathLine = function (line, continuous) {
        var start = line.start;
        var end = line.end;
        var path = continuous ? "" : (" M" + start.x + " " + start.y);
        path += " L" + end.x + " " + end.y;
        return path;
    };
    SVGGraph.prototype.pathBezier = function (bezier, continuous) {
        var start = bezier.start;
        var p1 = bezier.p1;
        var p2 = bezier.p2;
        var p3 = bezier.p3;
        var path = (continuous ? " L" : " M") + start.x + " " + start.y;
        path += " C" + p1.x + " " + p1.y + " " + p2.x + " " + p2.y + " " + p3.x + " " + p3.y;
        return path;
    };
    SVGGraph.prototype.pathSegmentedCurve = function (curve, continuous) {
        var path = "";
        for (var i = 0; i < curve.segments.length; i++)
            path += this.pathCurve(curve.segments[i], continuous || path != "");
        return path;
    };
    SVGGraph.prototype.pathPolyline = function (polyline, continuous) {
        var start = polyline.start;
        var path = " M" + start.x + " " + start.y;
        for (var i = 0; i < polyline.points.length; i++) {
            var point = polyline.points[i];
            path += " L" + point.x + " " + point.y;
        }
        if (polyline.closed)
            path + " F";
        return path;
    };
    SVGGraph.prototype.pathRoundedRect = function (roundedRect, continuous) {
        var curve = roundedRect.getCurve();
        return this.pathSegmentedCurve(curve, continuous);
    };
    SVGGraph.prototype.pathCurve = function (curve, continuous) {
        if (curve == null)
            return "";
        if (curve.curvetype === "SegmentedCurve")
            return this.pathSegmentedCurve(curve, continuous);
        else if (curve.curvetype === "Polyline")
            return this.pathPolyline(curve, continuous);
        else if (curve.curvetype === "Bezier")
            return this.pathBezier(curve, continuous);
        else if (curve.curvetype === "Line")
            return this.pathLine(curve, continuous);
        else if (curve.curvetype === "Ellipse")
            return this.pathEllipse(curve, continuous);
        else if (curve.curvetype === "RoundedRect")
            return this.pathRoundedRect(curve, continuous);
        else
            throw "unknown curve type: " + curve.curvetype;
    };
    SVGGraph.prototype.drawLabel = function (parent, label, owner) {
        var g = document.createElementNS(SVGGraph.SVGNS, "g");
        if (this.customDrawLabel == null || !this.customDrawLabel(this.svg, parent, label, owner)) {
            var text = document.createElementNS(SVGGraph.SVGNS, "text");
            text.setAttribute("x", label.bounds.x.toString());
            text.setAttribute("y", (label.bounds.y + label.bounds.height).toString());
            text.textContent = label.content;
            text.setAttribute("style", "fill: " + (label.fill == "" ? "black" : label.fill + "; text-anchor: start"));
            g.appendChild(text);
        }
        parent.appendChild(g);
        // If this is an edge label, I need to construct an appropriate RenderEdgeLabel object.
        if (owner instanceof G.GEdge) {
            var edge = owner;
            if (this.renderEdgeLabels[edge.id] == null)
                this.renderEdgeLabels[edge.id] = new RenderEdgeLabel(edge, g);
            var renderLabel = this.renderEdgeLabels[edge.id];
            this.renderEdgeLabels[edge.id].group = g;
            var that = this;
            g.onmouseover = function (e) { that.onEdgeLabelMouseOver(renderLabel, e); };
            g.onmouseout = function (e) { that.onEdgeLabelMouseOut(renderLabel, e); };
        }
    };
    SVGGraph.prototype.drawNode = function (parent, node) {
        var g = document.createElementNS(SVGGraph.SVGNS, "g");
        var nodeCopy = node;
        var that = this;
        g.onclick = function () { that.onNodeClick(nodeCopy); };
        var curve = node.boundaryCurve;
        var pathString = this.pathCurve(curve, false) + "Z";
        var pathStyle = "stroke: " + node.stroke + "; fill: " + (node.fill == "" ? "none" : node.fill) + "; stroke-width: " + node.thickness + "; stroke-linejoin: miter; stroke-miterlimit: 2.0";
        if (node.shape != null && node.shape.multi > 0) {
            var path = document.createElementNS(SVGGraph.SVGNS, "path");
            path.setAttribute("d", pathString);
            path.setAttribute("transform", "translate(5,5)");
            path.setAttribute("style", pathStyle);
            g.appendChild(path);
        }
        var path = document.createElementNS(SVGGraph.SVGNS, "path");
        path.setAttribute("d", pathString);
        path.setAttribute("style", pathStyle);
        g.appendChild(path);
        if (node.label !== null)
            this.drawLabel(g, node.label, node);
        if (node.tooltip != null) {
            var title = document.createElementNS(SVGGraph.SVGNS, "title");
            title.textContent = node.tooltip;
            g.appendChild(title);
        }
        parent.appendChild(g);
        // Construct the appropriate RenderNode object.
        if (this.renderNodes[node.id] == null)
            this.renderNodes[node.id] = new RenderNode(node, g);
        this.renderNodes[node.id].group = g;
        var renderNode = this.renderNodes[node.id];
        g.onclick = function () { that.onNodeClick(renderNode.node); };
        g.onmouseover = function (e) { that.onNodeMouseOver(renderNode, e); };
        g.onmouseout = function (e) { that.onNodeMouseOut(renderNode, e); };
        var cluster = node;
        if (cluster.children !== undefined)
            for (var i = 0; i < cluster.children.length; i++)
                this.drawNode(parent, cluster.children[i]);
    };
    SVGGraph.prototype.drawArrow = function (parent, arrowHead, style) {
        // start is the base of the arrowhead
        var start = arrowHead.start;
        // end is the point where the arrowhead touches the target
        var end = arrowHead.end;
        if (start == null || end == null)
            return;
        // dir is the vector from start to end
        var dir = new G.GPoint({ x: start.x - end.x, y: start.y - end.y });
        // offset (x and y) is the vector from the start to the side
        var offsetX = -dir.y * Math.tan(25 * 0.5 * (Math.PI / 180));
        var offsetY = dir.x * Math.tan(25 * 0.5 * (Math.PI / 180));
        var pathString = "";
        if (arrowHead.style == "tee") {
            pathString += " M" + (start.x + offsetX) + " " + (start.y + offsetY);
            pathString += " L" + (start.x - offsetX) + " " + (start.y - offsetY);
        }
        else if (arrowHead.style == "diamond") {
            pathString += " M" + (start.x) + " " + (start.y);
            pathString += " L" + (start.x - (offsetX + dir.x / 2)) + " " + (start.y - (offsetY + dir.y / 2));
            pathString += " L" + (end.x) + " " + (end.y);
            pathString += " L" + (start.x - (-offsetX + dir.x / 2)) + " " + (start.y - (-offsetY + dir.y / 2));
            pathString += " Z";
        }
        else {
            pathString += " M" + (start.x + offsetX) + " " + (start.y + offsetY);
            pathString += " L" + end.x + " " + end.y;
            pathString += " L" + (start.x - offsetX) + " " + (start.y - offsetY);
            if (arrowHead.closed)
                pathString += " Z";
            else {
                pathString += " M" + start.x + " " + start.y;
                pathString += " L" + end.x + " " + end.y;
            }
        }
        var path = document.createElementNS(SVGGraph.SVGNS, "path");
        path.setAttribute("d", pathString);
        if (arrowHead.dash != null)
            style += "; stroke-dasharray: " + arrowHead.dash;
        path.setAttribute("style", style);
        parent.appendChild(path);
    };
    SVGGraph.prototype.drawEdge = function (parent, edge) {
        var curve = edge.curve;
        if (curve == null) {
            console.log("MSAGL warning: did not receive a curve for edge " + edge.id);
            return;
        }
        var g = document.createElementNS(SVGGraph.SVGNS, "g");
        var edgeCopy = edge;
        var that = this;
        g.onclick = function () { that.onEdgeClick(edgeCopy); };
        var pathString = this.pathCurve(curve, false);
        var path = document.createElementNS(SVGGraph.SVGNS, "path");
        path.setAttribute("d", pathString);
        var style = "stroke: " + edge.stroke + "; stroke-width: " + edge.thickness + "; fill: none";
        if (edge.dash != null)
            style += "; stroke-dasharray: " + edge.dash;
        path.setAttribute("style", style);
        g.appendChild(path);
        if (edge.arrowHeadAtTarget != null)
            this.drawArrow(g, edge.arrowHeadAtTarget, "stroke: " + edge.stroke + "; stroke-width: " + edge.thickness + "; fill: " + (edge.arrowHeadAtTarget.fill ? edge.stroke : "none"));
        if (edge.arrowHeadAtSource != null)
            this.drawArrow(g, edge.arrowHeadAtSource, "stroke: " + edge.stroke + "; stroke-width: " + edge.thickness + "; fill: " + (edge.arrowHeadAtSource.fill ? edge.stroke : "none"));
        if (edge.label != null)
            this.drawLabel(this.svg, edge.label, edge);
        if (edge.tooltip != null) {
            var title = document.createElementNS(SVGGraph.SVGNS, "title");
            title.textContent = edge.tooltip;
            g.appendChild(title);
        }
        parent.appendChild(g);
        // Construct the appropriate RenderEdge object.
        if (this.renderEdges[edge.id] == null)
            this.renderEdges[edge.id] = new RenderEdge(edge, g);
        var renderEdge = this.renderEdges[edge.id];
        renderEdge.group = g;
        g.onclick = function () { that.onEdgeClick(renderEdge.edge); };
        g.onmouseover = function (e) { that.onEdgeMouseOver(renderEdge, e); };
        g.onmouseout = function (e) { that.onEdgeMouseOut(renderEdge, e); };
    };
    SVGGraph.prototype.drawGrid = function (parent) {
        for (var x = 0; x < 10; x++)
            for (var y = 0; y < 10; y++) {
                var circle = document.createElementNS(SVGGraph.SVGNS, "circle");
                circle.setAttribute("r", "1");
                circle.setAttribute("x", (x * 100).toString());
                circle.setAttribute("y", (y * 100).toString());
                circle.setAttribute("style", "fill: black; stroke: black; stroke-width: 1");
                parent.appendChild(circle);
            }
    };
    SVGGraph.prototype.populateGraph = function () {
        if (this.style != null) {
            var style = document.createElementNS(SVGGraph.SVGNS, "style");
            var styleText = document.createTextNode(this.style);
            style.appendChild(styleText);
            this.svg.appendChild(style);
        }
        this.renderNodes = {};
        this.renderEdges = {};
        this.renderEdgeLabels = {};
        for (var i = 0; i < this.graph.nodes.length; i++)
            this.drawNode(this.svg, this.graph.nodes[i]);
        for (var i = 0; i < this.graph.edges.length; i++)
            this.drawEdge(this.svg, this.graph.edges[i]);
    };
    SVGGraph.prototype.drawGraph = function () {
        while (this.svg != null && this.svg.childElementCount > 0)
            this.svg.removeChild(this.svg.firstChild);
        if (this.grid)
            this.drawGrid(this.svg);
        if (this.graph == null)
            return;
        if (this.svg == null) {
            this.svg = document.createElementNS(SVGGraph.SVGNS, "svg");
            this.container.appendChild(this.svg);
        }
        var bbox = this.graph.boundingBox;
        var offsetX = bbox.x;
        var offsetY = bbox.y;
        var width = this.container.offsetWidth;
        var height = this.container.offsetHeight;
        //this.svg.setAttribute("style", "width: " + width + "px; height: " + height + "px");
        //this.svg.setAttribute("style", "width: 100%; height: 100%");
        var viewBox = "" + offsetX + " " + offsetY + " " + bbox.width + " " + bbox.height;
        this.svg.setAttribute("viewBox", viewBox);
        this.populateGraph();
    };
    /** Registers several mouse events on the container, to handle editing. */
    SVGGraph.prototype.hookUpMouseEvents = function () {
        var that = this;
        // Note: the SVG element does not have onmouseleave, and onmouseout is useless because it fires on moving to children.
        this.container.onmousemove = function (e) { that.onMouseMove(e); };
        this.container.onmouseleave = function (e) { that.onMouseOut(e); };
        this.container.onmousedown = function (e) { that.onMouseDown(e); };
        this.container.onmouseup = function (e) { that.onMouseUp(e); };
        this.container.ondblclick = function (e) { that.onMouseDblClick(e); };
    };
    /** Returns true if the SVG node contains the specified group as a child. I need this because SVG elements do not seem
    to have a method to test if they contain a specific child, at least in IE. */
    SVGGraph.prototype.containsGroup = function (g) {
        if (this.svg.contains != null)
            return this.svg.contains(g);
        for (var i = 0; i < this.svg.childNodes.length; i++)
            if (this.svg.childNodes[i] == g)
                return true;
        return false;
    };
    /** Redraws a single graph element. Used for editing. This is done by removing the element's group, and making a new
    one. */
    SVGGraph.prototype.redrawElement = function (el) {
        if (el instanceof RenderNode) {
            var renderNode = el;
            if (this.containsGroup(renderNode.group))
                this.svg.removeChild(renderNode.group);
            this.drawNode(this.svg, renderNode.node);
        }
        else if (el instanceof RenderEdge) {
            var renderEdge = el;
            if (this.containsGroup(renderEdge.group))
                this.svg.removeChild(renderEdge.group);
            // In the case of edges, I also need to redraw the label.
            var renderLabel = this.renderEdgeLabels[renderEdge.edge.id];
            if (renderLabel != null)
                this.svg.removeChild(renderLabel.group);
            this.drawEdge(this.svg, renderEdge.edge);
            // Also, if it is being edited, I'll need to redraw the control points.
            if (this.edgeEditEdge == renderEdge)
                this.drawPolylineCircles();
        }
        else if (el instanceof RenderEdgeLabel) {
            var renderEdgeLabel = el;
            if (this.containsGroup(renderEdgeLabel.group))
                this.svg.removeChild(renderEdgeLabel.group);
            this.drawLabel(this.svg, renderEdgeLabel.edge.label, renderEdgeLabel.edge);
        }
    };
    /** Returns the current mouse coordinates, in graph space. If null, the mouse is outside the graph. */
    SVGGraph.prototype.getMousePoint = function () { return this.mousePoint; };
    ;
    /** Returns the graph element that is currently under the mouse cursor. This can be a node, an edge, or an edge label. Note
    that node labels are just considered part of the node. If null, the mouse is over blank space, or not over the graph. */
    SVGGraph.prototype.getObjectUnderMouseCursor = function () {
        return this.elementUnderMouseCursor == null ? null : this.elementUnderMouseCursor.getGeometryElement();
    };
    ;
    /** Converts a point from a MouseEvent into graph space coordinates. */
    SVGGraph.prototype.getGraphPoint = function (e) {
        // I'm using the SVG transformation facilities, to make sure all transformations are
        // accounted for. First, make a SVG point with the mouse coordinates...
        var clientPoint = this.svg.createSVGPoint();
        clientPoint.x = e.clientX;
        clientPoint.y = e.clientY;
        // Then, reverse the current transformation matrix...
        var matrix = this.svg.getScreenCTM().inverse();
        // Then, apply the reversed matrix to the point, obtaining the new point in graph space.
        var graphPoint = clientPoint.matrixTransform(matrix);
        return new G.GPoint({ x: graphPoint.x, y: graphPoint.y });
    };
    ;
    // Mouse event handlers.
    SVGGraph.prototype.onMouseMove = function (e) {
        if (this.svg == null)
            return;
        // Update the mouse point.
        this.mousePoint = this.getGraphPoint(e);
        // Do dragging, if needed.
        this.doDrag();
    };
    ;
    SVGGraph.prototype.onMouseOut = function (e) {
        if (this.svg == null)
            return;
        // Clear the mouse data.
        this.mousePoint = null;
        this.elementUnderMouseCursor = null;
        // End dragging, if needed.
        this.endDrag();
    };
    ;
    SVGGraph.prototype.onMouseDown = function (e) {
        if (this.svg == null)
            return;
        // Store the point where the mouse went down.
        this.mouseDownPoint = new G.GPoint(this.getGraphPoint(e));
        // Begin dragging, if needed.
        if (this.allowEditing)
            this.beginDrag();
    };
    ;
    SVGGraph.prototype.onMouseUp = function (e) {
        if (this.svg == null)
            return;
        // End dragging, if needed.
        this.endDrag();
    };
    ;
    SVGGraph.prototype.onMouseDblClick = function (e) {
        if (this.svg == null)
            return;
        // If an edge is being edited, interpret the double click as an edge corner event. It may be
        // an insertion or a deletion.
        if (this.edgeEditEdge != null)
            this.edgeControlPointEvent(this.getGraphPoint(e));
    };
    SVGGraph.prototype.onNodeMouseOver = function (n, e) {
        if (this.svg == null)
            return;
        // Update the object under mouse cursor.
        this.elementUnderMouseCursor = n;
    };
    ;
    SVGGraph.prototype.onNodeMouseOut = function (n, e) {
        if (this.svg == null)
            return;
        // Clear the object under mouse cursor.
        this.elementUnderMouseCursor = null;
    };
    ;
    SVGGraph.prototype.onEdgeMouseOver = function (ed, e) {
        if (this.svg == null)
            return;
        // Update the object under mouse cursor.
        this.elementUnderMouseCursor = ed;
        // If needed, begin editing the edge.
        if (this.allowEditing)
            this.enterEdgeEditMode(ed);
    };
    ;
    SVGGraph.prototype.onEdgeMouseOut = function (ed, e) {
        if (this.svg == null)
            return;
        // Start the timeout to exit edge edit mode.
        this.beginExitEdgeEditMode();
        // Clear the object under mouse cursor.
        this.elementUnderMouseCursor = null;
    };
    ;
    SVGGraph.prototype.onEdgeLabelMouseOver = function (l, e) {
        if (this.svg == null)
            return;
        // Update the object under mouse cursor.
        this.elementUnderMouseCursor = l;
    };
    ;
    SVGGraph.prototype.onEdgeLabelMouseOut = function (l, e) {
        if (this.svg == null)
            return;
        // Clear the object under mouse cursor.
        this.elementUnderMouseCursor = null;
    };
    ;
    /** Returns the object that is currently being dragged (or null if nothing is being dragged). */
    SVGGraph.prototype.getDragObject = function () { return this.dragElement == null ? null : this.dragElement.getGeometryElement(); };
    ;
    /** Begins a drag operation on the object that is currently under the mouse cursor, if it is a draggable object. */
    SVGGraph.prototype.beginDrag = function () {
        if (this.elementUnderMouseCursor == null)
            return;
        // Get the geometry object being dragged.
        var geometryElement = this.elementUnderMouseCursor.getGeometryElement();
        // Start a geometry move operation.
        this.graph.startMoveElement(geometryElement, this.mouseDownPoint);
        // Store the drag element.
        this.dragElement = this.elementUnderMouseCursor;
    };
    ;
    /** Updates the position of the object that is currently being dragged, according to the current mouse position. */
    SVGGraph.prototype.doDrag = function () {
        if (this.dragElement == null)
            return;
        // Compute the delta.
        var delta = this.mousePoint.sub(this.mouseDownPoint);
        // Perform the geometry move operation.
        this.graph.moveElements(delta);
        // Redraw the affected element.
        this.redrawElement(this.dragElement);
    };
    ;
    /** Ends the current drag operation, if any. After calling this, further mouse movements will not move any object. */
    SVGGraph.prototype.endDrag = function () {
        // End the geometry move operation.
        this.graph.endMoveElements();
        // Clear the drag element.
        this.dragElement = null;
    };
    ;
    SVGGraph.prototype.isEditingEdge = function () {
        return this.edgeEditEdge != null;
    };
    /** Draws the control points for the edge that is currently being edited. */
    SVGGraph.prototype.drawPolylineCircles = function () {
        if (this.edgeEditEdge == null)
            return;
        var group = this.edgeEditEdge.group;
        var points = this.graph.getPolyline(this.edgeEditEdge.edge.id);
        // I want to move existing circles in preference to deleting and recreating them. This avoids needless mouseout/mouseover events
        // as circles disappear and appear right under the cursor. I'll start by getting all of the circles that are currently present.
        // Note that I am assuming that all Circle elements in the edge group are control point renderings; this should be a safe
        // assumption. If there are circles as part of a custom labels, they will be in a subgroup.
        var existingCircles = [];
        for (var i = 0; i < group.childNodes.length; i++)
            if (group.childNodes[i].nodeName == "circle")
                existingCircles.push(group.childNodes[i]);
        for (var i = 0; i < points.length; i++) {
            var point = points[i];
            var c = i < existingCircles.length ? existingCircles[i] : document.createElementNS(SVGGraph.SVGNS, "circle");
            c.setAttribute("r", G.GGraph.EdgeEditCircleRadius.toString());
            c.setAttribute("cx", point.x.toString());
            c.setAttribute("cy", point.y.toString());
            // The fill needs to be explicitly set to transparent. If it is null, the circle will not catch mouse events properly.
            c.setAttribute("style", "stroke: #5555FF; stroke-width: 1px; fill: transparent");
            // If control points have actually been added, they need to be added to the edge group.
            if (i >= existingCircles.length)
                group.insertBefore(c, group.childNodes[0]);
        }
        // If control points have actually been removed, they need to be removed from the edge group.
        for (var i = points.length; i < existingCircles.length; i++)
            group.removeChild(existingCircles[i]);
    };
    /** Removes the control point circles from the edge that's currently being edited. */
    SVGGraph.prototype.clearPolylineCircles = function () {
        if (this.edgeEditEdge == null)
            return;
        // First, make a list of these circles.
        var circles = [];
        var group = this.edgeEditEdge.group;
        for (var i = 0; i < group.childNodes.length; i++)
            if (group.childNodes[i].nodeName == "circle")
                circles.push(group.childNodes[i]);
        // Then remove them.
        for (var i = 0; i < circles.length; i++)
            group.removeChild(circles[i]);
    };
    /** Starts editing an edge. */
    SVGGraph.prototype.enterEdgeEditMode = function (ed) {
        if (this.edgeEditEdge == ed) {
            // I am already editing this edge. I just need to clear the timeout, if any.
            clearTimeout(this.edgeEditModeTimeout);
            this.edgeEditModeTimeout = 0;
        }
        // If the user is attempting to start editing another edge, I'll stop right here. They need to wait
        // for the current edge to exit edit mode.
        if (this.edgeEditEdge != null && this.edgeEditEdge != ed)
            return;
        // Mark this as the edge being edited.
        this.edgeEditEdge = ed;
        // Show the control point circles.
        this.drawPolylineCircles();
    };
    /** Exit edge editing mode immediately. */
    SVGGraph.prototype.exitEdgeEditMode = function () {
        var ed = this.edgeEditEdge;
        if (ed == null)
            return;
        // Clear the timeout, it's no longer needed.
        clearTimeout(this.edgeEditModeTimeout);
        // Get rid of the circles.
        this.clearPolylineCircles();
        // Reset the edge editing data.
        this.edgeEditModeTimeout = 0;
        this.edgeEditEdge = null;
    };
    /** Sets a timeout to exit edge edit mode. */
    SVGGraph.prototype.beginExitEdgeEditMode = function () {
        var that = this;
        // TODO: what if there already is a timeout at this point? Need to test.
        this.edgeEditModeTimeout = setTimeout(function () { return that.exitEdgeEditMode(); }, SVGGraph.ExitEdgeModeTimeout);
    };
    /** Handles an attempt to insert/remove an edge control point. */
    SVGGraph.prototype.edgeControlPointEvent = function (point) {
        // First, check if the click was right on a control point. 
        var clickedPoint = this.graph.getControlPointAt(this.edgeEditEdge.edge, point);
        if (clickedPoint != null) {
            this.graph.delEdgeControlPoint(this.edgeEditEdge.edge.id, clickedPoint);
            // Note that at this point the mouse will usually be outside the edge, but no other mouse event will fire
            // unless the user moves the mouse. So I need to behave as if it had moved outside the edge: clear the
            // elementUnderMouseCursor, and start the edge editing timeout. Note that, technically, it is possible for
            // the mouse cursor to still be resting on the edge; however, I think this is the best compromise that
            // can be done without having to use hit testing. The SVG spec has hit testing, but it does not work in
            // Mozilla, and at the moment I do not want to reimplement hit testing.
            this.elementUnderMouseCursor = null;
            this.beginExitEdgeEditMode();
        }
        else {
            // The click was not inside any control point. Make a new control point.
            this.graph.addEdgeControlPoint(this.edgeEditEdge.edge.id, point);
            // Note that at this point the mouse will certainly be inside a control point, which means it will
            // technically be on the edge. So I should clear the timeout; otherwise, it will still be ticking unless
            // the user moves the mouse.
            clearTimeout(this.edgeEditModeTimeout);
            this.edgeEditModeTimeout = 0;
            // Set the object under the mouse cursor to be the edge currently being edited. This because it is null
            // at this point (the user has clicked outside the edge), and it will incorrectly remain null even after
            // adding the control point, unless the user moves the mouse.
            this.elementUnderMouseCursor = this.edgeEditEdge;
        }
    };
    SVGGraph.SVGNS = "http://www.w3.org/2000/svg";
    /** This is the timeout (in msecs) during which the user can move the mouse away from the edge and still be
    editing the edge. */
    SVGGraph.ExitEdgeModeTimeout = 2000;
    return SVGGraph;
}());
module.exports = SVGGraph;
//# sourceMappingURL=svggraph.js.map