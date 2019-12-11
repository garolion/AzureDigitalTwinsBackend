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
Object.defineProperty(exports, "__esModule", { value: true });
/** A GPoint represents coordinates in 2D space. */
var GPoint = /** @class */ (function () {
    function GPoint(p) {
        this.x = p.x === undefined ? 0 : p.x;
        this.y = p.y === undefined ? 0 : p.y;
    }
    GPoint.prototype.equals = function (other) {
        return this.x == other.x && this.y == other.y;
    };
    /** Vector sum. */
    GPoint.prototype.add = function (other) {
        return new GPoint({ x: this.x + other.x, y: this.y + other.y });
    };
    /** Vector subtraction. */
    GPoint.prototype.sub = function (other) {
        return new GPoint({ x: this.x - other.x, y: this.y - other.y });
    };
    /** Scalar division. */
    GPoint.prototype.div = function (op) {
        return new GPoint({ x: this.x / op, y: this.y / op });
    };
    /** Scalar multiplication. */
    GPoint.prototype.mul = function (op) {
        return new GPoint({ x: this.x * op, y: this.y * op });
    };
    /** Vector multiplication. */
    GPoint.prototype.vmul = function (other) {
        return this.x * other.x + this.y * other.y;
    };
    /** Distance squared. */
    GPoint.prototype.dist2 = function (other) {
        var d = this.sub(other);
        return d.x * d.x + d.y * d.y;
    };
    GPoint.prototype.closestParameter = function (start, end) {
        var bc = end.sub(start);
        var ba = this.sub(start);
        var c1 = bc.vmul(ba);
        if (c1 <= 0.1)
            return 0;
        var c2 = bc.vmul(bc);
        if (c2 <= c1 + 0.1)
            return 1;
        return c1 / c2;
    };
    /** If the area is negative then C lies to the right of the line [cornerA, cornerB] or, in other words, the triangle (A, B, C) is oriented clockwise. If it is positive then C lies to the left of the line [A,B], and the triangle (A, B, C) is oriented counter-clockwise. If zero, A, B and C are colinear. */
    GPoint.signedDoubledTriangleArea = function (cornerA, cornerB, cornerC) {
        return (cornerB.x - cornerA.x) * (cornerC.y - cornerA.y) - (cornerC.x - cornerA.x) * (cornerB.y - cornerA.y);
    };
    GPoint.origin = new GPoint({ x: 0, y: 0 });
    return GPoint;
}());
exports.GPoint = GPoint;
/** A GRect represents a rectangular region in 2D space. */
var GRect = /** @class */ (function () {
    function GRect(r) {
        this.x = r.x === undefined ? 0 : r.x;
        this.y = r.y === undefined ? 0 : r.y;
        this.width = r.width === undefined ? 0 : r.width;
        this.height = r.height === undefined ? 0 : r.height;
    }
    GRect.prototype.getTopLeft = function () {
        return new GPoint({ x: this.x, y: this.y });
    };
    GRect.prototype.getBottomRight = function () {
        return new GPoint({ x: this.getRight(), y: this.getBottom() });
    };
    GRect.prototype.getBottom = function () {
        return this.y + this.height;
    };
    GRect.prototype.getRight = function () {
        return this.x + this.width;
    };
    GRect.prototype.getCenter = function () {
        return new GPoint({ x: this.x + this.width / 2, y: this.y + this.height / 2 });
    };
    GRect.prototype.setCenter = function (p) {
        var delta = p.sub(this.getCenter());
        this.x += delta.x;
        this.y += delta.y;
    };
    /** Combines this GRect with another GRect, returning the smallest GRect that contains both of them. */
    GRect.prototype.extend = function (other) {
        if (other == null)
            return this;
        return new GRect({
            x: Math.min(this.x, other.x),
            y: Math.min(this.y, other.y),
            width: Math.max(this.getRight(), other.getRight()) - Math.min(this.x, other.x),
            height: Math.max(this.getBottom(), other.getBottom()) - Math.min(this.y, other.y)
        });
    };
    /** Returns the smalles GRect that contains both this GRect and the given GPoint. */
    GRect.prototype.extendP = function (point) {
        return this.extend(new GRect({ x: point.x, y: point.y, width: 0, height: 0 }));
    };
    GRect.zero = new GRect({ x: 0, y: 0, width: 0, height: 0 });
    return GRect;
}());
exports.GRect = GRect;
/** A GCurve describes a curve. */
var GCurve = /** @class */ (function () {
    function GCurve(curvetype) {
        if (curvetype === undefined)
            throw new Error("Undefined curve type");
        this.curvetype = curvetype;
    }
    /** Constructs a concrete curve, based on the ICurve passed. This behaves similarly to the constructors of other types, but because this also needs to decide on a type, I cannot use the constructor directly. */
    GCurve.ofCurve = function (curve) {
        if (curve == null || curve === undefined)
            return null;
        var ret;
        if (curve.curvetype == "Ellipse")
            ret = new GEllipse(curve);
        else if (curve.curvetype == "Line")
            ret = new GLine(curve);
        else if (curve.curvetype == "Bezier")
            ret = new GBezier(curve);
        else if (curve.curvetype == "Polyline")
            ret = new GPolyline(curve);
        else if (curve.curvetype == "SegmentedCurve")
            ret = new GSegmentedCurve(curve);
        else if (curve.curvetype == "RoundedRect")
            ret = new GRoundedRect(curve);
        return ret;
    };
    return GCurve;
}());
exports.GCurve = GCurve;
/** A GCurve that represents an ellipse. Note that the data format would support ellipses with axes that are not either vertical or horizontal, but in practice the MSAGL engine doesn't deal with those. So axisA and axisB should be vertical or horizontal vectors. Also, you can use parStart and parEnd to define portions of an ellipse. */
var GEllipse = /** @class */ (function (_super) {
    __extends(GEllipse, _super);
    function GEllipse(ellipse) {
        var _this = _super.call(this, "Ellipse") || this;
        _this.center = ellipse.center === undefined ? GPoint.origin : new GPoint(ellipse.center);
        _this.axisA = ellipse.axisA === undefined ? GPoint.origin : new GPoint(ellipse.axisA);
        _this.axisB = ellipse.axisB === undefined ? GPoint.origin : new GPoint(ellipse.axisB);
        _this.parStart = ellipse.parStart === undefined ? 0 : ellipse.parStart;
        _this.parEnd = ellipse.parStart === undefined ? Math.PI * 2 : ellipse.parEnd;
        return _this;
    }
    GEllipse.prototype.getCenter = function () {
        return this.center;
    };
    GEllipse.prototype.getStart = function () {
        return this.center.add(this.axisA.mul(Math.cos(this.parStart))).add(this.axisB.mul(Math.sin(this.parStart)));
    };
    GEllipse.prototype.getEnd = function () {
        return this.center.add(this.axisA.mul(Math.cos(this.parEnd))).add(this.axisB.mul(Math.sin(this.parEnd)));
    };
    GEllipse.prototype.setCenter = function (p) {
        this.center = new GPoint(p);
    };
    GEllipse.prototype.getBoundingBox = function () {
        var width = 2 * Math.max(Math.abs(this.axisA.x), Math.abs(this.axisB.x));
        var height = 2 * Math.max(Math.abs(this.axisA.y), Math.abs(this.axisB.y));
        var p = this.center.sub({ x: width / 2, y: height / 2 });
        return new GRect({ x: p.x, y: p.y, width: width, height: height });
    };
    /** A helper method that makes a complete ellipse with the given width and height. */
    GEllipse.make = function (width, height) {
        return new GEllipse({ center: GPoint.origin, axisA: new GPoint({ x: width / 2, y: 0 }), axisB: new GPoint({ x: 0, y: height / 2 }), parStart: 0, parEnd: Math.PI * 2 });
    };
    return GEllipse;
}(GCurve));
exports.GEllipse = GEllipse;
/** A GLine represents a GCurve that is a segment. */
var GLine = /** @class */ (function (_super) {
    __extends(GLine, _super);
    function GLine(line) {
        var _this = _super.call(this, "Line") || this;
        _this.start = line.start === undefined ? GPoint.origin : new GPoint(line.start);
        _this.end = line.end === undefined ? GPoint.origin : new GPoint(line.end);
        return _this;
    }
    GLine.prototype.getCenter = function () {
        return this.start.add(this.end).div(2);
    };
    GLine.prototype.getStart = function () {
        return this.start;
    };
    GLine.prototype.getEnd = function () {
        return this.end;
    };
    GLine.prototype.setCenter = function (p) {
        var delta = p.sub(this.getCenter());
        this.start = this.start.add(delta);
        this.end = this.end.add(delta);
    };
    GLine.prototype.getBoundingBox = function () {
        var ret = new GRect({ x: this.start.x, y: this.start.y, width: 0, height: 0 });
        ret = ret.extendP(this.end);
        return ret;
    };
    return GLine;
}(GCurve));
exports.GLine = GLine;
/** A GPolyline represents a GCurve that is a sequence of contiguous segments. */
var GPolyline = /** @class */ (function (_super) {
    __extends(GPolyline, _super);
    function GPolyline(polyline) {
        var _this = _super.call(this, "Polyline") || this;
        _this.start = polyline.start === undefined ? GPoint.origin : new GPoint(polyline.start);
        _this.points = [];
        for (var i = 0; i < polyline.points.length; i++)
            _this.points.push(new GPoint(polyline.points[i]));
        _this.closed = polyline.closed === undefined ? false : polyline.closed;
        return _this;
    }
    GPolyline.prototype.getCenter = function () {
        var ret = this.start;
        for (var i = 0; i < this.points.length; i++)
            ret = ret.add(this.points[i]);
        ret = ret.div(1 + this.points.length);
        return ret;
    };
    GPolyline.prototype.getStart = function () {
        return this.start;
    };
    GPolyline.prototype.getEnd = function () {
        return this.points[this.points.length - 1];
    };
    GPolyline.prototype.setCenter = function (p) {
        var delta = p.sub(this.getCenter());
        for (var i = 0; i < this.points.length; i++)
            this.points[i] = this.points[i].add(delta);
    };
    GPolyline.prototype.getBoundingBox = function () {
        var ret = new GRect({ x: this.points[0].x, y: this.points[0].y, height: 0, width: 0 });
        for (var i = 1; i < this.points.length; i++)
            ret = ret.extendP(this.points[i]);
        return ret;
    };
    return GPolyline;
}(GCurve));
exports.GPolyline = GPolyline;
/** A GRoundedRect represents a GCurve that is a rectangle that may have rounded corners. Technically, this is just a handy helper... the same shape can be represented with a composition of simpler objects. */
var GRoundedRect = /** @class */ (function (_super) {
    __extends(GRoundedRect, _super);
    function GRoundedRect(roundedRect) {
        var _this = _super.call(this, "RoundedRect") || this;
        _this.bounds = roundedRect.bounds === undefined ? GRect.zero : new GRect(roundedRect.bounds);
        _this.radiusX = roundedRect.radiusX === undefined ? 0 : roundedRect.radiusX;
        _this.radiusY = roundedRect.radiusY === undefined ? 0 : roundedRect.radiusY;
        return _this;
    }
    GRoundedRect.prototype.getCenter = function () {
        return this.bounds.getCenter();
    };
    GRoundedRect.prototype.getStart = function () {
        throw new Error("getStart not supported by RoundedRect");
    };
    GRoundedRect.prototype.getEnd = function () {
        throw new Error("getEnd not supported by RoundedRect");
    };
    GRoundedRect.prototype.setCenter = function (p) {
        this.bounds.setCenter(p);
    };
    GRoundedRect.prototype.getBoundingBox = function () {
        return this.bounds;
    };
    /** Converts this to a GSegmentedCurve (a composition of simpler objects). */
    GRoundedRect.prototype.getCurve = function () {
        var segments = [];
        var axisA = new GPoint({ x: this.radiusX, y: 0 });
        var axisB = new GPoint({ x: 0, y: this.radiusY });
        var innerBounds = new GRect({ x: this.bounds.x + this.radiusX, y: this.bounds.y + this.radiusY, width: this.bounds.width - this.radiusX * 2, height: this.bounds.height - this.radiusY * 2 });
        segments.push(new GEllipse({ axisA: axisA, axisB: axisB, center: new GPoint({ x: innerBounds.x, y: innerBounds.y }), parStart: Math.PI, parEnd: Math.PI * 3 / 2 }));
        segments.push(new GLine({ start: new GPoint({ x: innerBounds.x, y: this.bounds.y }), end: new GPoint({ x: innerBounds.x + innerBounds.width, y: this.bounds.y }) }));
        segments.push(new GEllipse({ axisA: axisA, axisB: axisB, center: new GPoint({ x: innerBounds.x + innerBounds.width, y: innerBounds.y }), parStart: Math.PI * 3 / 2, parEnd: 2 * Math.PI }));
        segments.push(new GLine({ start: new GPoint({ x: this.bounds.x + this.bounds.width, y: innerBounds.y }), end: new GPoint({ x: this.bounds.x + this.bounds.width, y: innerBounds.y + innerBounds.height }) }));
        segments.push(new GEllipse({ axisA: axisA, axisB: axisB, center: new GPoint({ x: innerBounds.x + innerBounds.width, y: innerBounds.y + innerBounds.height }), parStart: 0, parEnd: Math.PI / 2 }));
        segments.push(new GLine({ start: new GPoint({ x: innerBounds.x + innerBounds.width, y: this.bounds.y + this.bounds.height }), end: new GPoint({ x: innerBounds.x, y: this.bounds.y + this.bounds.height }) }));
        segments.push(new GEllipse({ axisA: axisA, axisB: axisB, center: new GPoint({ x: innerBounds.x, y: innerBounds.y + innerBounds.height }), parStart: Math.PI / 2, parEnd: Math.PI }));
        segments.push(new GLine({ start: new GPoint({ x: this.bounds.x, y: innerBounds.y + innerBounds.height }), end: new GPoint({ x: this.bounds.x, y: innerBounds.y }) }));
        return new GSegmentedCurve({ segments: segments });
    };
    return GRoundedRect;
}(GCurve));
exports.GRoundedRect = GRoundedRect;
/** A GBezier is a GCurve that represents a Bezier segment. */
var GBezier = /** @class */ (function (_super) {
    __extends(GBezier, _super);
    function GBezier(bezier) {
        var _this = _super.call(this, "Bezier") || this;
        _this.start = bezier.start === undefined ? GPoint.origin : new GPoint(bezier.start);
        _this.p1 = bezier.p1 === undefined ? GPoint.origin : new GPoint(bezier.p1);
        _this.p2 = bezier.p2 === undefined ? GPoint.origin : new GPoint(bezier.p2);
        _this.p3 = bezier.p3 === undefined ? GPoint.origin : new GPoint(bezier.p3);
        return _this;
    }
    GBezier.prototype.getCenter = function () {
        var ret = this.start;
        ret = ret.add(this.p1);
        ret = ret.add(this.p2);
        ret = ret.add(this.p3);
        ret = ret.div(4);
        return ret;
    };
    GBezier.prototype.getStart = function () {
        return this.start;
    };
    GBezier.prototype.getEnd = function () {
        return this.p3;
    };
    GBezier.prototype.setCenter = function (p) {
        var delta = p.sub(this.getCenter());
        this.start = this.start.add(delta);
        this.p1 = this.p1.add(delta);
        this.p2 = this.p2.add(delta);
        this.p3 = this.p3.add(delta);
    };
    GBezier.prototype.getBoundingBox = function () {
        var ret = new GRect({ x: this.start.x, y: this.start.y, width: 0, height: 0 });
        ret = ret.extendP(this.p1);
        ret = ret.extendP(this.p2);
        ret = ret.extendP(this.p3);
        return ret;
    };
    return GBezier;
}(GCurve));
exports.GBezier = GBezier;
/** A GSegmentedCurve is a GCurve that's actually a sequence of simpler GCurves. */
var GSegmentedCurve = /** @class */ (function (_super) {
    __extends(GSegmentedCurve, _super);
    function GSegmentedCurve(segmentedCurve) {
        var _this = _super.call(this, "SegmentedCurve") || this;
        _this.segments = [];
        for (var i = 0; i < segmentedCurve.segments.length; i++)
            _this.segments.push(GCurve.ofCurve(segmentedCurve.segments[i]));
        return _this;
    }
    GSegmentedCurve.prototype.getCenter = function () {
        var ret = GPoint.origin;
        for (var i = 0; i < this.segments.length; i++)
            ret = ret.add(this.segments[i].getCenter());
        ret = ret.div(this.segments.length);
        return ret;
    };
    GSegmentedCurve.prototype.getStart = function () {
        return this.segments[0].getStart();
    };
    GSegmentedCurve.prototype.getEnd = function () {
        return this.segments[this.segments.length - 1].getEnd();
    };
    GSegmentedCurve.prototype.setCenter = function (p) {
        throw new Error("setCenter not supported by SegmentedCurve");
    };
    GSegmentedCurve.prototype.getBoundingBox = function () {
        var ret = this.segments[0].getBoundingBox();
        for (var i = 1; i < this.segments.length; i++)
            ret = ret.extend(this.segments[i].getBoundingBox());
        return ret;
    };
    return GSegmentedCurve;
}(GCurve));
exports.GSegmentedCurve = GSegmentedCurve;
/** A GLabel represents a label. */
var GLabel = /** @class */ (function () {
    function GLabel(label) {
        if (typeof (label) == "string")
            this.content = label;
        else {
            this.bounds = label.bounds == undefined || label.bounds == GRect.zero ? GRect.zero : new GRect(label.bounds);
            this.tooltip = label.tooltip === undefined ? null : label.tooltip;
            this.content = label.content;
            this.fill = label.fill === undefined ? "Black" : label.fill;
        }
    }
    return GLabel;
}());
exports.GLabel = GLabel;
/** A GShape is the shape of a node's boundary, in an abstract sense (as opposed to a GCurve, which is a concrete curve). You can think of this as a description of how a GCurve should eventually be built to correctly encircle a node's label. */
var GShape = /** @class */ (function () {
    function GShape() {
        this.radiusX = 0;
        this.radiusY = 0;
        this.multi = 0;
    }
    /** Helper that gives you a rectangular shape. */
    GShape.GetRect = function () {
        var ret = new GShape();
        ret.shape = "rect";
        return ret;
    };
    /** Helper that gives you a rectangular shape with rounded corners. */
    GShape.GetRoundedRect = function (radiusX, radiusY) {
        var ret = new GShape();
        ret.shape = "rect";
        ret.radiusX = radiusX === undefined ? 5 : radiusX;
        ret.radiusY = radiusY === undefined ? 5 : radiusY;
        return ret;
    };
    /** Helper that gives you a rectangular shape with rounded corners, with radii that are based on the label size. */
    GShape.GetMaxRoundedRect = function () {
        var ret = new GShape();
        ret.shape = "rect";
        ret.radiusX = null;
        ret.radiusY = null;
        return ret;
    };
    /** Helper that gives you a shape from some special strings (rect, roundedrect and maxroundedrect). */
    GShape.FromString = function (shape) {
        if (shape == "rect")
            return GShape.GetRect();
        else if (shape == "roundedrect")
            return GShape.GetRoundedRect();
        else if (shape == "maxroundedrect")
            return GShape.GetMaxRoundedRect();
        return null;
    };
    GShape.RectShape = "rect";
    return GShape;
}());
exports.GShape = GShape;
/** A GNode represents a graph node. */
var GNode = /** @class */ (function () {
    function GNode(node) {
        if (node.id === undefined)
            throw new Error("Undefined node id");
        this.id = node.id;
        this.tooltip = node.tooltip === undefined ? null : node.tooltip;
        this.shape = node.shape === undefined ? null : typeof (node.shape) == "string" ? GShape.FromString(node.shape) : node.shape;
        this.boundaryCurve = GCurve.ofCurve(node.boundaryCurve);
        this.label = node.label === undefined ? null : node.label == null ? null : typeof (node.label) == "string" ? new GLabel({ content: node.label }) : new GLabel(node.label);
        this.labelMargin = node.labelMargin === undefined ? 5 : node.labelMargin;
        this.thickness = node.thickness == undefined ? 1 : node.thickness;
        this.fill = node.fill === undefined ? "White" : node.fill;
        this.stroke = node.stroke === undefined ? "Black" : node.stroke;
        this.nodeType = node.nodeType === undefined ? "Space" : node.nodeType;
    }
    /** Type check: returns true if this is actually a cluster. */
    GNode.prototype.isCluster = function () {
        return this.children !== undefined;
    };
    return GNode;
}());
exports.GNode = GNode;
var GClusterMargin = /** @class */ (function () {
    function GClusterMargin(clusterMargin) {
        this.bottom = clusterMargin.bottom == null ? 0 : clusterMargin.bottom;
        this.top = clusterMargin.top == null ? 0 : clusterMargin.top;
        this.left = clusterMargin.left == null ? 0 : clusterMargin.left;
        this.right = clusterMargin.right == null ? 0 : clusterMargin.right;
        this.minWidth = clusterMargin.minWidth == null ? 0 : clusterMargin.minWidth;
        this.minHeight = clusterMargin.minHeight == null ? 0 : clusterMargin.minHeight;
    }
    return GClusterMargin;
}());
exports.GClusterMargin = GClusterMargin;
/** A GCluster is a GNode that's actually a cluster. Note that if you add children via the constructor, you'll lose custom fields. If your nodes have custom fields, use CGluster.addChild instead. */
var GCluster = /** @class */ (function (_super) {
    __extends(GCluster, _super);
    function GCluster(cluster) {
        var _this = _super.call(this, cluster) || this;
        _this.margin = new GClusterMargin(cluster.margin == null ? {} : cluster.margin);
        _this.children = [];
        if (cluster.children != null)
            for (var i = 0; i < cluster.children.length; i++)
                if (cluster.children[i].children !== undefined)
                    _this.children.push(new GCluster(cluster.children[i]));
                else
                    _this.children.push(new GNode(cluster.children[i]));
        return _this;
    }
    GCluster.prototype.addChild = function (n) {
        this.children.push(n);
    };
    return GCluster;
}(GNode));
exports.GCluster = GCluster;
/** A GArrowHead represents an edge's arrowhead. */
var GArrowHead = /** @class */ (function () {
    function GArrowHead(arrowHead) {
        this.start = arrowHead.start == undefined ? null : arrowHead.start;
        this.end = arrowHead.end == undefined ? null : arrowHead.end;
        this.closed = arrowHead.closed == undefined ? false : arrowHead.closed;
        this.fill = arrowHead.fill == undefined ? false : arrowHead.fill;
        this.dash = arrowHead.dash == undefined ? null : arrowHead.dash;
        this.style = arrowHead.style == undefined ? "standard" : arrowHead.style;
    }
    /** Standard arrowhead (empty, open). */
    GArrowHead.standard = new GArrowHead({});
    /** Closed arrowhead. */
    GArrowHead.closed = new GArrowHead({ closed: true });
    /** Filled arrowhead. */
    GArrowHead.filled = new GArrowHead({ closed: true, fill: true });
    /** Tee-shaped arrowhead. The closed and fill flags have no effect on this arrowhead. */
    GArrowHead.tee = new GArrowHead({ style: "tee" });
    /** Diamond-shaped arrowhead (empty). The closed flag has no effect on this arrowhead. */
    GArrowHead.diamond = new GArrowHead({ style: "diamond" });
    /** Diamond-shaped arrowhead (filled). The closed flag has no effect on this arrowhead. */
    GArrowHead.diamondFilled = new GArrowHead({ style: "diamond", fill: true });
    return GArrowHead;
}());
exports.GArrowHead = GArrowHead;
/** A GEdge represents a graph edge. */
var GEdge = /** @class */ (function () {
    function GEdge(edge) {
        if (edge.id === undefined)
            throw new Error("Undefined edge id");
        if (edge.source === undefined)
            throw new Error("Undefined edge source");
        if (edge.target === undefined)
            throw new Error("Undefined edge target");
        this.id = edge.id;
        this.tooltip = edge.tooltip === undefined ? null : edge.tooltip;
        this.source = edge.source;
        this.target = edge.target;
        this.label = edge.label === undefined || edge.label == null ? null : typeof (edge.label) == "string" ? new GLabel({ content: edge.label }) : new GLabel(edge.label);
        this.arrowHeadAtTarget = edge.arrowHeadAtTarget === undefined ? GArrowHead.standard : edge.arrowHeadAtTarget == null ? null : new GArrowHead(edge.arrowHeadAtTarget);
        this.arrowHeadAtSource = edge.arrowHeadAtSource === undefined || edge.arrowHeadAtSource == null ? null : new GArrowHead(edge.arrowHeadAtSource);
        this.thickness = edge.thickness == undefined ? 1 : edge.thickness;
        this.dash = edge.dash == undefined ? null : edge.dash;
        this.curve = edge.curve === undefined ? null : GCurve.ofCurve(edge.curve);
        this.stroke = edge.stroke === undefined ? "Black" : edge.stroke;
    }
    return GEdge;
}());
exports.GEdge = GEdge;
/** A GPlaneTransformation represents a setting for applying a transformation to the graph. */
var GPlaneTransformation = /** @class */ (function () {
    function GPlaneTransformation(transformation) {
        if (transformation.rotation !== undefined) {
            var angle = transformation.rotation;
            var cos = Math.cos(angle);
            var sin = Math.sin(angle);
            this.m00 = cos;
            this.m01 = -sin;
            this.m02 = 0;
            this.m10 = sin;
            this.m11 = cos;
            this.m12 = 0;
        }
        else {
            this.m00 = transformation.m00 === undefined ? -1 : transformation.m00;
            this.m01 = transformation.m01 === undefined ? -1 : transformation.m01;
            this.m02 = transformation.m02 === undefined ? -1 : transformation.m02;
            this.m10 = transformation.m10 === undefined ? -1 : transformation.m10;
            this.m11 = transformation.m11 === undefined ? -1 : transformation.m11;
            this.m12 = transformation.m12 === undefined ? -1 : transformation.m12;
        }
    }
    /** Helper: the default transformation, which orientates the graph top-to-bottom. */
    GPlaneTransformation.defaultTransformation = new GPlaneTransformation({ m00: -1, m01: 0, m02: 0, m10: 0, m11: -1, m12: 0 });
    /** Helper: a transformation that orientates the graph left-to-right. */
    GPlaneTransformation.ninetyDegreesTransformation = new GPlaneTransformation({ m00: 0, m01: -1, m02: 0, m10: 1, m11: 0, m12: 0 });
    return GPlaneTransformation;
}());
exports.GPlaneTransformation = GPlaneTransformation;
/** An GUpDownConstraint represents a setting to put an Up-Down constraint on a graph layout. */
var GUpDownConstraint = /** @class */ (function () {
    function GUpDownConstraint(upDownConstraint) {
        this.upNode = upDownConstraint.upNode;
        this.downNode = upDownConstraint.downNode;
    }
    return GUpDownConstraint;
}());
exports.GUpDownConstraint = GUpDownConstraint;
/** GSettings represents a graph's layout settings. */
var GSettings = /** @class */ (function () {
    function GSettings(settings) {
        this.layout = settings.layout === undefined ? GSettings.sugiyamaLayout : settings.layout;
        this.transformation = settings.transformation === undefined ? GPlaneTransformation.defaultTransformation : settings.transformation;
        this.routing = settings.routing === undefined ? GSettings.sugiyamaSplinesRouting : settings.routing;
        this.aspectRatio = settings.aspectRatio === undefined ? 0.0 : settings.aspectRatio;
        this.upDownConstraints = [];
        if (settings.upDownConstraints !== undefined) {
            for (var i = 0; i < settings.upDownConstraints.length; i++) {
                var upDownConstraint = new GUpDownConstraint(settings.upDownConstraints[i]);
                this.upDownConstraints.push(upDownConstraint);
            }
        }
    }
    GSettings.sugiyamaLayout = "sugiyama";
    GSettings.mdsLayout = "mds";
    GSettings.splinesRouting = "splines";
    GSettings.splinesBundlingRouting = "splinesbundling";
    GSettings.straightLineRouting = "straightline";
    GSettings.sugiyamaSplinesRouting = "sugiyamasplines";
    GSettings.rectilinearRouting = "rectilinear";
    GSettings.rectilinearToCenterRouting = "rectilineartocenter";
    return GSettings;
}());
exports.GSettings = GSettings;
/** This is a helper for internal use, which decorates nodes with their edges. */
var GNodeInternal = /** @class */ (function () {
    function GNodeInternal() {
    }
    return GNodeInternal;
}());
/** This is a helper for internal use, which decorates edges with their polylines. */
var GEdgeInternal = /** @class */ (function () {
    function GEdgeInternal() {
    }
    return GEdgeInternal;
}());
/** A MoveElementToken stores the information needed to perform dragging operations on a geometry element. */
var MoveElementToken = /** @class */ (function () {
    function MoveElementToken() {
    }
    return MoveElementToken;
}());
/** This token stores the information needed to move a node: a reference to the node, and the original coordinates of the node. */
var MoveNodeToken = /** @class */ (function (_super) {
    __extends(MoveNodeToken, _super);
    function MoveNodeToken() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    return MoveNodeToken;
}(MoveElementToken));
/** This token stores the information needed to move an edge label: a reference to the label, and the original coordinates of the label. */
var MoveEdgeLabelToken = /** @class */ (function (_super) {
    __extends(MoveEdgeLabelToken, _super);
    function MoveEdgeLabelToken() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    return MoveEdgeLabelToken;
}(MoveElementToken));
/** This token stores the information needed to move an edge control point: a reference to the edge, the original polyline, and the point on the original polyline that's being edited. */
var MoveEdgeToken = /** @class */ (function (_super) {
    __extends(MoveEdgeToken, _super);
    function MoveEdgeToken() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    return MoveEdgeToken;
}(MoveElementToken));
/** This class is a simple implementation of a callback set. Note that there are libraries to do this, e.g. jQuery, but I'd rather not acquire a dependency on jQuery just for this. */
var CallbackSet = /** @class */ (function () {
    function CallbackSet() {
        this.callbacks = [];
    }
    /** Adds a callback to the list. If you want to be able to remove the callback later, you'll need to store a reference to it. */
    CallbackSet.prototype.add = function (callback) {
        if (callback == null)
            return;
        this.callbacks.push(callback);
    };
    /** Removes a callback from the list. */
    CallbackSet.prototype.remove = function (callback) {
        if (callback == null)
            return;
        var idx = this.callbacks.indexOf(callback);
        if (idx >= 0)
            this.callbacks.splice(idx);
    };
    /** Fires all of the callbacks, with the given parameter. */
    CallbackSet.prototype.fire = function (par) {
        for (var i = 0; i < this.callbacks.length; i++)
            this.callbacks[i](par);
    };
    CallbackSet.prototype.count = function () {
        return this.callbacks.length;
    };
    return CallbackSet;
}());
exports.CallbackSet = CallbackSet;
/** A GGraph represents a graph, plus its layout settings, and provides methods to manipulate it. */
var GGraph = /** @class */ (function () {
    function GGraph() {
        /** The web worker that performs layout operations. There's at most one of these at any given time. */
        this.worker = null;
        this.working = false;
        /** Callbacks you can set to be notified when a layout operation is starting. */
        this.layoutStartedCallbacks = new CallbackSet();
        /** Callbacks you can set to be notified when a layout operation is complete. */
        this.layoutCallbacks = new CallbackSet();
        /** Callbacks you can set to be notified when an edge routing operation is complete. The set of routed edges is passed as a parameter. If it is null, it means that all edges in the graph were routed. Note that edge routing may happen after being invoked by the user program, but it may also happen as a consequence of moving a node. */
        this.edgeRoutingCallbacks = new CallbackSet();
        /** Callbacks you can set to be notified when the engine starts an asynchronous operation. */
        this.workStartedCallbacks = new CallbackSet();
        /** Callbacks you can set to be notified when the engine ends an asynchronous operation. */
        this.workStoppedCallbacks = new CallbackSet();
        /** A list of the current move tokens. Note that currently only one object can be moved at the same time. This may change in the future. */
        this.moveTokens = [];
        /** The callback that's waiting to attempt to start edge routing again, if it could not be started immediately. */
        this.delayCheckRouteEdges = null;
        /** The callback that's waiting to attempt to start edge rebuild again, if it could not be started immediately. */
        this.delayCheckRebuildEdge = null;
        this.nodesMap = {};
        this.edgesMap = {};
        this.nodes = [];
        this.edges = [];
        this.boundingBox = GRect.zero;
        this.settings = new GSettings({ transformation: { m00: -1, m01: 0, m02: 0, m10: 0, m11: -1, m12: 0 } });
    }
    /** Add a node to the node map. Recursively map cluster children. */
    GGraph.prototype.mapNode = function (node) {
        this.nodesMap[node.id] = { node: node, outEdges: [], inEdges: [], selfEdges: [] };
        var children = node.children;
        if (children !== undefined)
            for (var _i = 0, children_1 = children; _i < children_1.length; _i++) {
                var child = children_1[_i];
                this.mapNode(child);
            }
    };
    /** Add a node to the graph. */
    GGraph.prototype.addNode = function (node) {
        this.nodes.push(node);
        this.mapNode(node);
        var children = node.children;
        if (children !== undefined)
            for (var _i = 0, children_2 = children; _i < children_2.length; _i++) {
                var child = children_2[_i];
                this.mapNode(child);
            }
    };
    /** Returns a node, given its ID. */
    GGraph.prototype.getNode = function (id) {
        var nodeInternal = this.nodesMap[id];
        return nodeInternal == null ? null : nodeInternal.node;
    };
    /** Gets a node's in-edges, given its ID. */
    GGraph.prototype.getInEdges = function (nodeId) {
        var nodeInternal = this.nodesMap[nodeId];
        return nodeInternal == null ? null : nodeInternal.inEdges;
    };
    /** Gets a node's out-edges, given its ID. */
    GGraph.prototype.getOutEdges = function (nodeId) {
        var nodeInternal = this.nodesMap[nodeId];
        return nodeInternal == null ? null : nodeInternal.outEdges;
    };
    /** Gets a node's self-edges, given its ID. */
    GGraph.prototype.getSelfEdges = function (nodeId) {
        var nodeInternal = this.nodesMap[nodeId];
        return nodeInternal == null ? null : nodeInternal.selfEdges;
    };
    /** Adds an edge to the graph. The nodes must exist! */
    GGraph.prototype.addEdge = function (edge) {
        if (this.nodesMap[edge.source] == null)
            throw new Error("Undefined node " + edge.source);
        if (this.nodesMap[edge.target] == null)
            throw new Error("Undefined node " + edge.target);
        if (this.edgesMap[edge.id] != null)
            throw new Error("Edge " + edge.id + " already exists");
        this.edgesMap[edge.id] = { edge: edge, polyline: null };
        this.edges.push(edge);
        if (edge.source == edge.target)
            this.nodesMap[edge.source].selfEdges.push(edge.id);
        else {
            this.nodesMap[edge.source].outEdges.push(edge.id);
            this.nodesMap[edge.target].inEdges.push(edge.id);
        }
    };
    /** Returns an edge, given its ID. */
    GGraph.prototype.getEdge = function (id) {
        var edgeInternal = this.edgesMap[id];
        return edgeInternal == null ? null : edgeInternal.edge;
    };
    /** Gets the JSON representation of the graph. */
    GGraph.prototype.getJSON = function () {
        var igraph = { nodes: this.nodes, edges: this.edges, boundingBox: this.boundingBox, settings: this.settings };
        var ret = JSON.stringify(igraph);
        return ret;
    };
    /** Rebuilds the GGraph out of a JSON string. */
    GGraph.ofJSON = function (json) {
        var igraph = JSON.parse(json);
        if (igraph.edges === undefined)
            igraph.edges = [];
        var ret = new GGraph();
        ret.boundingBox = new GRect(igraph.boundingBox === undefined ? GRect.zero : igraph.boundingBox);
        ret.settings = new GSettings(igraph.settings === undefined ? {} : igraph.settings);
        for (var i = 0; i < igraph.nodes.length; i++) {
            var inode = igraph.nodes[i];
            if (inode.children !== undefined) {
                var gcluster = new GCluster(inode);
                ret.addNode(gcluster);
            }
            else {
                var gnode = new GNode(inode);
                ret.addNode(gnode);
            }
        }
        for (var i = 0; i < igraph.edges.length; i++) {
            var iedge = igraph.edges[i];
            var gedge = new GEdge(iedge);
            ret.addEdge(gedge);
        }
        return ret;
    };
    /** Creates boundaries for all nodes, based on their shape and label. */
    GGraph.prototype.createNodeBoundariesRec = function (node, sizer) {
        var cluster = node;
        if (node.boundaryCurve == null) {
            if (node.label != null && node.label.bounds == GRect.zero && sizer !== undefined) {
                var labelSize = sizer(node.label, node);
                node.label.bounds = new GRect({ x: 0, y: 0, width: labelSize.x, height: labelSize.y });
            }
            // Do not create boundaries for clusters. The layout algorithm will handle that.
            if (cluster.children == null) {
                var labelWidth = node.label == null ? 0 : node.label.bounds.width;
                var labelHeight = node.label == null ? 0 : node.label.bounds.height;
                labelWidth += 2 * node.labelMargin;
                labelHeight += 2 * node.labelMargin;
                var boundary;
                if (node.shape != null && node.shape.shape == GShape.RectShape) {
                    var radiusX = node.shape.radiusX;
                    var radiusY = node.shape.radiusY;
                    if (radiusX == null && radiusY == null) {
                        var k = Math.min(labelWidth, labelHeight);
                        radiusX = radiusY = k / 2;
                    }
                    boundary = new GRoundedRect({
                        bounds: new GRect({ x: 0, y: 0, width: labelWidth, height: labelHeight }), radiusX: radiusX, radiusY: radiusY
                    });
                }
                else
                    boundary = GEllipse.make(labelWidth * Math.sqrt(2), labelHeight * Math.sqrt(2));
                node.boundaryCurve = boundary;
            }
        }
        if (cluster.children != null)
            for (var i = 0; i < cluster.children.length; i++)
                this.createNodeBoundariesRec(cluster.children[i], sizer);
    };
    /** Creates the node boundaries for nodes that don't have one. If the node has a label, it will first compute the label's size based on the provided size function, and then create an appropriate boundary. There are several predefined sizers, corresponding to the most common ways of sizing text. */
    GGraph.prototype.createNodeBoundaries = function (sizer) {
        for (var i = 0; i < this.nodes.length; i++)
            this.createNodeBoundariesRec(this.nodes[i], sizer);
        // Assign size to edge labels too.
        if (sizer !== undefined) {
            for (var i = 0; i < this.edges.length; i++) {
                var edge = this.edges[i];
                if (edge.label != null && edge.label.bounds == GRect.zero) {
                    var labelSize = sizer(edge.label, edge);
                    edge.label.bounds = new GRect({ width: labelSize.x, height: labelSize.y });
                }
            }
        }
    };
    /** A sizer function that calculates text sizes as Canvas text. */
    GGraph.contextSizer = function (context, label) {
        return { x: context.measureText(label.content).width, y: parseInt(context.font) };
    };
    /** Creates node boundaries using the contextSizer on the given Canvas context. */
    GGraph.prototype.createNodeBoundariesFromContext = function (context) {
        var selfMadeContext = (context === undefined);
        if (selfMadeContext) {
            var canvas = document.createElement('canvas');
            document.body.appendChild(canvas);
            context = canvas.getContext("2d");
        }
        this.createNodeBoundaries(function (label) { return GGraph.contextSizer(context, label); });
        if (selfMadeContext)
            document.body.removeChild(canvas);
    };
    /** A sizer function that calculates text size as HTML text. */
    GGraph.divSizer = function (div, label) {
        div.innerText = label.content;
        return { x: div.clientWidth, y: div.clientHeight };
    };
    /** Creates node boundaries using the divSizer on the given div. */
    GGraph.prototype.createNodeBoundariesFromDiv = function (div) {
        var selfMadeDiv = (div === undefined);
        if (selfMadeDiv) {
            div = document.createElement('div');
            div.setAttribute('style', 'float:left');
            document.body.appendChild(div);
        }
        this.createNodeBoundaries(function (label) { return GGraph.divSizer(div, label); });
        if (selfMadeDiv)
            document.body.removeChild(div);
    };
    /** A sizer function that calculates text size as SVG text. */
    GGraph.SVGSizer = function (svg, label) {
        var element = document.createElementNS('http://www.w3.org/2000/svg', 'text');
        element.setAttribute('fill', 'black');
        var textNode = document.createTextNode(label.content);
        element.appendChild(textNode);
        svg.appendChild(element);
        var bbox = element.getBBox();
        var ret = { x: bbox.width, y: bbox.height };
        svg.removeChild(element);
        if (ret.y > 6)
            ret.y -= 6; // Hack: offset miscalculated height.
        if (label.content.length == 1)
            ret.x = ret.y; // Hack: make single-letter nodes round.
        return ret;
    };
    /** Creates node boundaries using the svgSizer on the given SVG element. You can also not give this function the SVG element, in which case it will use a temporary one. In this case, you can give it a style declaration that will be used for the temporary SVG element.*/
    GGraph.prototype.createNodeBoundariesFromSVG = function (svg, style) {
        var selfMadeSvg = (svg === undefined);
        if (selfMadeSvg) {
            svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
            if (style !== undefined) {
                svg.style.font = style.font;
                svg.style.fontFamily = style.fontFamily;
                svg.style.fontFeatureSettings = style.fontFeatureSettings;
                svg.style.fontSize = style.fontSize;
                svg.style.fontSizeAdjust = style.fontSizeAdjust;
                svg.style.fontStretch = style.fontStretch;
                svg.style.fontStyle = style.fontStyle;
                svg.style.fontVariant = style.fontVariant;
                svg.style.fontWeight = style.fontWeight;
            }
            document.body.appendChild(svg);
        }
        this.createNodeBoundaries(function (label) { return GGraph.SVGSizer(svg, label); });
        if (selfMadeSvg)
            document.body.removeChild(svg);
    };
    /** Creates node boundaries for text that will be SVG text, placed in an SVG element in the given HTML container. Warning: you are responsible for the container to be valid for this purpose. E.g. it should not be hidden, or the results won't be valid.*/
    GGraph.prototype.createNodeBoundariesForSVGInContainer = function (container) {
        var svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        container.appendChild(svg);
        this.createNodeBoundaries(function (label) { return GGraph.SVGSizer(svg, label); });
        container.removeChild(svg);
    };
    /** Creates node boundaries for text that will be SVG text, placed in an SVG element at the top level of the DOM. This guarantees that the SVG element is not hidden, but it will not use the same style as the real container. You can pass a style to use for font styling. */
    GGraph.prototype.createNodeBoundariesForSVGWithStyle = function (style) {
        var svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        if (style != null) {
            svg.style.font = style.font;
            svg.style.fontFamily = style.fontFamily;
            svg.style.fontFeatureSettings = style.fontFeatureSettings;
            svg.style.fontSize = style.fontSize;
            svg.style.fontSizeAdjust = style.fontSizeAdjust;
            svg.style.fontStretch = style.fontStretch;
            svg.style.fontStyle = style.fontStyle;
            svg.style.fontVariant = style.fontVariant;
            svg.style.fontWeight = style.fontWeight;
        }
        document.body.appendChild(svg);
        this.createNodeBoundaries(function (label, owner) { return GGraph.SVGSizer(svg, label); });
        document.body.removeChild(svg);
    };
    /** Aborts a layout operation, if there is one ongoing. You should call this to dispose the worker when you are no longer using the graph, because there is an apparent bug in Chrome that causes workers not to be garbage collected unless they are terminated. If this Chrome issue gets fixed, then calling stopLayoutGraph for disposal becomes unnecessary. */
    GGraph.prototype.stopLayoutGraph = function () {
        if (this.worker != null) {
            this.worker.terminate();
            this.worker = null;
        }
        this.setWorking(false);
    };
    GGraph.prototype.workerCallback = function (msg) {
        var data = msg.data;
        if (data.msgtype == "RunLayout") {
            var runLayoutMsg = data;
            // data.graph contains a string that is the JSON string for an IGraph. Deserialize it into a GGraph. This GGraph doesn't directly replace myself; I just want to copy its curves over mine. This way, the user can keep using this GGraph.
            var gs = GGraph.ofJSON(runLayoutMsg.graph);
            // Copy its bounding box to me, extending the margins a little bit.
            this.boundingBox = new GRect({
                x: gs.boundingBox.x - 10, y: gs.boundingBox.y - 10, width: gs.boundingBox.width + 20, height: gs.boundingBox.height + 20
            });
            // Copy all of the curves of the nodes, including the label boundaries.
            for (var nodeId in gs.nodesMap) {
                var workerNode = gs.nodesMap[nodeId];
                var myNode = this.getNode(nodeId);
                myNode.boundaryCurve = workerNode.node.boundaryCurve;
                if (myNode.label != null)
                    myNode.label.bounds = workerNode.node.label.bounds;
            }
            // Copy all of the curves of the edges, including the label boundaries and the arrowheads.
            for (var edgeId in gs.edgesMap) {
                var workerEdge = gs.edgesMap[edgeId];
                var myEdge = this.getEdge(edgeId);
                myEdge.curve = workerEdge.edge.curve;
                if (myEdge.label != null)
                    myEdge.label.bounds = workerEdge.edge.label.bounds;
                if (myEdge.arrowHeadAtSource != null)
                    myEdge.arrowHeadAtSource = workerEdge.edge.arrowHeadAtSource;
                if (myEdge.arrowHeadAtTarget != null)
                    myEdge.arrowHeadAtTarget = workerEdge.edge.arrowHeadAtTarget;
            }
            // Invoke the user callbacks.
            this.layoutCallbacks.fire();
        }
        else if (data.msgtype == "RouteEdges") {
            var routeEdgesMsg = data;
            // data.graph contains a string that is the JSON string for an IGraph. Deserialize it into a GGraph. This GGraph doesn't directly replace myself; I just want to copy its curves over mine. This way, the user can keep using this GGraph.
            var gs = GGraph.ofJSON(routeEdgesMsg.graph);
            // Copy all of the curves of the edges, including the label boundaries and the arrowheads.
            for (var edgeId in gs.edgesMap) {
                // data.graph contains a string that is the JSON string for an IGraph. Deserialize it into a GGraph. This GGraph doesn't directly replace myself; I just want to copy its curves over mine. This way, the user can keep using this GGraph.
                var gs = GGraph.ofJSON(routeEdgesMsg.graph);
                var workerEdge = gs.edgesMap[edgeId];
                if (routeEdgesMsg.edges == null || routeEdgesMsg.edges.length == 0 || routeEdgesMsg.edges.indexOf(workerEdge.edge.id) >= 0) {
                    var edgeInternal = this.edgesMap[edgeId];
                    var myEdge = edgeInternal.edge;
                    myEdge.curve = workerEdge.edge.curve;
                    edgeInternal.polyline = null;
                    if (myEdge.label != null)
                        myEdge.label.bounds = workerEdge.edge.label.bounds;
                    if (myEdge.arrowHeadAtSource != null)
                        myEdge.arrowHeadAtSource = workerEdge.edge.arrowHeadAtSource;
                    if (myEdge.arrowHeadAtTarget != null)
                        myEdge.arrowHeadAtTarget = workerEdge.edge.arrowHeadAtTarget;
                }
            }
            // Invoke the user callbacks.
            this.edgeRoutingCallbacks.fire(routeEdgesMsg.edges);
        }
        else if (data.msgtype == "SetPolyline") {
            var setPolylineMsg = data;
            var edge = this.edgesMap[setPolylineMsg.edge].edge;
            var curve = JSON.parse(setPolylineMsg.curve);
            curve = GCurve.ofCurve(curve);
            edge.curve = curve;
            if (setPolylineMsg.sourceArrowHeadStart != null)
                edge.arrowHeadAtSource.start = JSON.parse(setPolylineMsg.sourceArrowHeadStart);
            if (setPolylineMsg.sourceArrowHeadEnd != null)
                edge.arrowHeadAtSource.end = JSON.parse(setPolylineMsg.sourceArrowHeadEnd);
            if (setPolylineMsg.targetArrowHeadStart != null)
                edge.arrowHeadAtTarget.start = JSON.parse(setPolylineMsg.targetArrowHeadStart);
            if (setPolylineMsg.targetArrowHeadEnd != null)
                edge.arrowHeadAtTarget.end = JSON.parse(setPolylineMsg.targetArrowHeadEnd);
            this.edgeRoutingCallbacks.fire([edge.id]);
        }
        this.setWorking(false);
    };
    /** Ensures that a layout worker is present and ready. */
    GGraph.prototype.ensureWorkerReady = function () {
        var _this = this;
        // Stop any currently active layout operations.
        if (this.working)
            this.stopLayoutGraph();
        if (this.worker == null) {
            this.worker = new Worker(require.toUrl("./workerBoot.js"));
            // Hook up to messages from the worker.
            var that = this;
            this.worker.addEventListener('message', function (ev) { return _this.workerCallback(ev); });
        }
    };
    GGraph.prototype.setWorking = function (working) {
        this.working = working;
        if (working)
            this.workStartedCallbacks.fire();
        else
            this.workStoppedCallbacks.fire();
    };
    /** Starts running layout on the graph. The layout callback will be invoked when the layout operation is done. */
    GGraph.prototype.beginLayoutGraph = function () {
        this.ensureWorkerReady();
        this.setWorking(true);
        this.layoutStartedCallbacks.fire();
        // Serialize the graph.
        var serialisedGraph = this.getJSON();
        // Send the worker the serialized graph to layout.
        var msg = { msgtype: "RunLayout", graph: serialisedGraph };
        this.worker.postMessage(msg);
    };
    /** Starts running edge routing on the graph. The edge routing callback will be invoked when the edge routing operation is done. */
    GGraph.prototype.beginEdgeRouting = function (edges) {
        this.ensureWorkerReady();
        this.setWorking(true);
        // Serialize the graph.
        var serialisedGraph = this.getJSON();
        // Send the worker the serialized graph to layout.
        var msg = { msgtype: "RouteEdges", graph: serialisedGraph, edges: edges };
        this.worker.postMessage(msg);
    };
    /** Starts generating an edge from its current control points. The edge routing callback will be invoked when the operation is done. */
    GGraph.prototype.beginRebuildEdgeCurve = function (edge) {
        this.ensureWorkerReady();
        this.setWorking(true);
        var serialisedGraph = this.getJSON();
        var points = this.edgesMap[edge].polyline;
        var msg = { msgtype: "SetPolyline", graph: serialisedGraph, edge: edge, polyline: JSON.stringify(points) };
        this.worker.postMessage(msg);
    };
    /** Returns the control point of the given edge that's within an editing circle radius from the given point. If more than one such points exist, the one which has the closest centre is returned. If no such point exists, returns null. */
    GGraph.prototype.getControlPointAt = function (edge, point) {
        var points = this.getPolyline(edge.id);
        // Iterate over points, comparing the squared distance.
        var ret = points[0];
        var dret = ret.dist2(point);
        for (var i = 1; i < points.length; i++) {
            var d = points[i].dist2(point);
            if (d < dret) {
                ret = points[i];
                dret = d;
            }
        }
        // Compare the closest distance with the edge edit circle radius.
        if (dret > GGraph.EdgeEditCircleRadius * GGraph.EdgeEditCircleRadius)
            ret = null;
        return ret;
    };
    /** This function selects an element for moving. After calling this, you can call moveElement to apply a delta to the position of the element. You can select multiple elements and then move them all in one operation, but you should not move elements between selections (in that case, call endMoveElements and then select them all again). */
    GGraph.prototype.startMoveElement = function (el, mousePoint) {
        if (el instanceof GNode) {
            // In the case of nodes, I need to make a note of the original center location of the node.
            var node = el;
            var mnt = new MoveNodeToken();
            mnt.node = node;
            mnt.originalBoundsCenter = node.boundaryCurve.getCenter();
            this.moveTokens.push(mnt);
        }
        else if (el instanceof GLabel) {
            // In the case of labels (which means edge labels, as node labels cannot be moved independently), I need to make a note of the original center location of the label.
            var label = el;
            var melt = new MoveEdgeLabelToken();
            melt.label = label;
            melt.originalLabelCenter = label.bounds.getCenter();
            this.moveTokens.push(melt);
        }
        else if (el instanceof GEdge) {
            // In the case of edges (which means an edge control point, as edges cannot be moved as a whole), I need to make a note of the original polyline, and the point of that polyline that's being moved.
            var edge = el;
            var point = this.getControlPointAt(edge, mousePoint);
            if (point != null) {
                var met = new MoveEdgeToken();
                met.edge = edge;
                met.originalPoint = point;
                met.originalPolyline = this.getPolyline(edge.id);
                this.moveTokens.push(met);
            }
        }
    };
    /** This function applies a delta to the positions of the element(s) currently selected for moving. */
    GGraph.prototype.moveElements = function (delta) {
        for (var i in this.moveTokens) {
            var token = this.moveTokens[i];
            if (token instanceof MoveNodeToken) {
                // If I'm moving a node, I'll need to apply the delta to the original center, and set it as the new center.
                var ntoken = token;
                var newBoundaryCenter = ntoken.originalBoundsCenter.add(delta);
                ntoken.node.boundaryCurve.setCenter(newBoundaryCenter);
                // The label too, if there is one.
                if (ntoken.node.label != null) {
                    var newLabelCenter = ntoken.originalBoundsCenter.add(delta);
                    ntoken.node.label.bounds.setCenter(newLabelCenter);
                }
                // Having moved a node, edge routing is required.
                this.checkRouteEdges();
            }
            else if (token instanceof MoveEdgeLabelToken) {
                // If I'm moving an edge, I'll need to apply the delta to the original center, and set it as the new center.
                var ltoken = token;
                var newBoundsCenter = ltoken.originalLabelCenter.add(delta);
                ltoken.label.bounds.setCenter(newBoundsCenter);
            }
            else if (token instanceof MoveEdgeToken) {
                // If I'm moving a control point, I'll need to apply the delta to the original control point, and then replace it in the original polyline. The resulting polyline is the new polyline for the edge.
                var etoken = token;
                var newPoint = etoken.originalPoint.add(delta);
                for (var j = 0; j < etoken.originalPolyline.length; j++)
                    if (etoken.originalPolyline[j].equals(etoken.originalPoint)) {
                        var edgeInternal = this.edgesMap[etoken.edge.id];
                        edgeInternal.polyline = etoken.originalPolyline.map(function (p, k) { return k == j ? newPoint : p; });
                        // Having changed the polyline, I'll need to rebuild the actual edge curve.
                        this.checkRebuildEdge(etoken.edge.id);
                        break;
                    }
            }
        }
    };
    /** Attempts to begin edge routing on the given edge set. If no edge set is provided, gets the edges that are outdated (i.e. the edges that are being affected by current node move operations). Edge routing cannot be started if the worker is already busy; in this case, a new attempt to start will be made when the worker becomes free again. Multiple calls to this function while an edge routing operation is pending will reset the request. */
    GGraph.prototype.checkRouteEdges = function (edgeSet) {
        var edges = edgeSet == null ? this.getOutdatedEdges() : edgeSet;
        if (edges.length > 0) {
            // Remove any already existing callback.
            if (this.delayCheckRouteEdges != null)
                this.workStoppedCallbacks.remove(this.delayCheckRouteEdges);
            this.delayCheckRouteEdges = null;
            if (this.working) {
                // The worker is busy. Register a callback to try again.
                var that = this;
                this.delayCheckRouteEdges = function () { that.checkRouteEdges(edges); };
                this.workStoppedCallbacks.add(this.delayCheckRouteEdges);
            }
            else
                // The worker is available. Start routing.
                this.beginEdgeRouting(edges);
        }
    };
    /** Attempts to begin rebuilding the given edge from its polyline. Edge rebuild cannot be started if the worker is already busy; in this case, a new attempt to start the rebuild will be made when the worker becomes free again. Multiple calls to this function while a rebuild is pending will reset the request. */
    GGraph.prototype.checkRebuildEdge = function (edge) {
        if (this.delayCheckRebuildEdge != null)
            this.workStoppedCallbacks.remove(this.delayCheckRebuildEdge);
        this.delayCheckRebuildEdge = null;
        if (this.working) {
            // The worker is busy. Register a callback to try again.
            var that = this;
            this.delayCheckRebuildEdge = function () { that.checkRebuildEdge(edge); };
            this.workStoppedCallbacks.add(this.delayCheckRebuildEdge);
        }
        else
            // The worker is available. Start routing.
            this.beginRebuildEdgeCurve(edge);
    };
    /** Returns an array of all the edges that are affected by node move operations. */
    GGraph.prototype.getOutdatedEdges = function () {
        // Prepare a dictionary of affected edges. By doing it this way, I avoid duplicate entries in case two or more nodes are being moved.
        var affectedEdges = {};
        for (var t in this.moveTokens) {
            var token = this.moveTokens[t];
            if (token instanceof MoveNodeToken) {
                // Get all of the edges that connect this node.
                var ntoken = token;
                var nEdges = this.getInEdges(ntoken.node.id).concat(this.getOutEdges(ntoken.node.id)).concat(this.getSelfEdges(ntoken.node.id));
                // Add them to the dictionary.
                for (var edge in nEdges)
                    affectedEdges[nEdges[edge]] = true;
            }
        }
        // Convert the dictionary to an array.
        var edges = [];
        for (var e in affectedEdges)
            edges.push(e);
        return edges;
    };
    /** Clear the list of elements that are selected for moving. */
    GGraph.prototype.endMoveElements = function () {
        this.checkRouteEdges();
        this.moveTokens = [];
    };
    /** Simplifies a polyline by removing vertexes that are colinear (or nearly so). */
    GGraph.prototype.removeColinearVertices = function (polyline) {
        for (var i = 1; i < polyline.length - 2; i++) {
            // Get the (signed, doubled) triangle area and compare it with an epsilon.
            var a = GPoint.signedDoubledTriangleArea(polyline[i - 1], polyline[i], polyline[i + 1]);
            // If it's less than that, remove the vertex. This is an approximation, but it's good enough for the purpose of not producing a polyline with many useless control points.
            if (a >= -GGraph.ColinearityEpsilon && a <= GGraph.ColinearityEpsilon)
                polyline.splice(i--, 1);
        }
    };
    /** Creates a polyline for an edge that doesn't have one. Note: generally speaking, this is *not* a round-trip transformation. This is fine. In practice, the polyline will be composed by the start and end points of every segment of the curve, if it is segmented. If it isn't, it'll just be the start and end points. */
    GGraph.prototype.makePolyline = function (edge) {
        var points = [];
        // Push the center of the source node.
        var source = this.nodesMap[edge.source];
        points.push(source.node.boundaryCurve.getCenter());
        // If the curve is segmented...
        if (edge.curve != null && edge.curve.curvetype == "SegmentedCurve") {
            // Push the start of the curve (note that, in general, this is not the same as the center of the source node, due to trimming.
            var scurve = edge.curve;
            points.push(scurve.getStart());
            // Push the end point of each segment (again, the end point of the last segment will not be the same as the center of the target node, due to trimming).
            for (var i = 0; i < scurve.segments.length; i++)
                points.push(scurve.segments[i].getEnd());
        }
        // Push the center of the target node.
        var target = this.nodesMap[edge.target];
        points.push(target.node.boundaryCurve.getCenter());
        // At this point, the polyline often has a lot of points due to edge routing algorithms producing segmented curves with lots of segments. Simplify the polyline.
        this.removeColinearVertices(points);
        return points;
    };
    /** Returns an edge's polyline. Will construct a polyline if not available already. */
    GGraph.prototype.getPolyline = function (edgeID) {
        var edgeInternal = this.edgesMap[edgeID];
        if (edgeInternal.polyline == null)
            edgeInternal.polyline = this.makePolyline(edgeInternal.edge);
        return edgeInternal.polyline;
    };
    /** Adds a control point for an edge, at the given point. The control point will be placed, in the polyline order, between the closest existing control point and the one opposite that. If there is no control point opposing the closest one, it'll be placed right after the closest one. If the closest one is the last one, it'll be placed right before. */
    GGraph.prototype.addEdgeControlPoint = function (edgeID, point) {
        var edgeInternal = this.edgesMap[edgeID];
        // Find the closest control point.
        var iclosest = 0;
        var dclosest = edgeInternal.polyline[0].dist2(point);
        for (var i = 0; i < edgeInternal.polyline.length; i++) {
            var d = edgeInternal.polyline[i].dist2(point);
            if (d < dclosest) {
                iclosest = i;
                dclosest = d;
            }
        }
        // If it's the last one, just put it before (i.e. decrease "iclosest", which at this point will mean "the index of the control point right before the new one").
        if (iclosest == edgeInternal.polyline.length - 1)
            iclosest--;
        else if (iclosest > 0) {
            // If it's neither the last one nor the first one, I need to decide which control point, between the next and the previous, can be considered the "opposite" one. I get the distance from the segment made by the closest and the previous, as a parameter on the segment. If it is far from the extremes, i.e. it is somewhere in the middle, that's the one.
            var par = point.closestParameter(edgeInternal.polyline[iclosest - 1], edgeInternal.polyline[iclosest]);
            if (par > 0.1 && par < 0.9)
                iclosest--;
        }
        // Put the new point in the polyline, at the specified position.
        edgeInternal.polyline.splice(iclosest + 1, 0, point);
        // Begin rebuilding the curve.
        this.beginRebuildEdgeCurve(edgeID);
    };
    /** Removes the specified control point from the edge. The point should be the exact location of an existing control point. You cannot remove the first or last control points. */
    GGraph.prototype.delEdgeControlPoint = function (edgeID, point) {
        var edgeInternal = this.edgesMap[edgeID];
        // Search for the index of the control point in the polyline.
        for (var i = 1; i < edgeInternal.polyline.length - 1; i++)
            if (edgeInternal.polyline[i].equals(point)) {
                // Remove it.
                edgeInternal.polyline.splice(i, 1);
                // Begin rebuilding the curve.
                this.beginRebuildEdgeCurve(edgeID);
                break;
            }
    };
    GGraph.EdgeEditCircleRadius = 8;
    /** If the triangle formed by three vertices has an area that's less than this, it's okay to discard the middle vertex for the purpose of building an edge's polyline. */
    GGraph.ColinearityEpsilon = 50.00;
    return GGraph;
}());
exports.GGraph = GGraph;
//# sourceMappingURL=ggraph.js.map