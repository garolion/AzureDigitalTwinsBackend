"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var G = require("./src/ggraph");
var IDDSVGGraph = require("./src/iddsvggraph");
var graphView = document.getElementById("graphView");
var nodeView = document.getElementById("nodeView");
var graphControl = new IDDSVGGraph(graphView);
var graph = null;
var layeredLayoutCheckBox = document.getElementById("layeredLayoutCheckBox");
var horizontalLayoutCheckBox = document.getElementById("horizontalLayoutCheckBox");
var edgeRoutingSelect = document.getElementById("edgeRoutingSelect");
function updateNodeView(node) {
    nodeView.innerHTML = '';
    var jsonText = document.getElementById("sourceJSON").innerText;
    var igraph = JSON.parse(jsonText);
    var _node = igraph.nodes.filter(function (n) { return n.id == node.id; })[0];
    var fieldId = document.createElement("p");
    fieldId.style.fontWeight = "bold";
    fieldId.appendChild(document.createTextNode("Id"));
    nodeView.appendChild(fieldId);
    var valueId = document.createElement("p");
    valueId.appendChild(document.createTextNode(_node.id));
    valueId.style.marginBottom = "25px";
    nodeView.appendChild(valueId);
    var fieldLabel = document.createElement("p");
    fieldLabel.style.fontWeight = "bold";
    fieldLabel.appendChild(document.createTextNode("Label"));
    nodeView.appendChild(fieldLabel);
    var valueLabel = document.createElement("p");
    valueLabel.appendChild(document.createTextNode(_node.label.content));
    valueLabel.style.marginBottom = "25px";
    nodeView.appendChild(valueLabel);
    var fieldNodeType = document.createElement("p");
    fieldNodeType.style.fontWeight = "bold";
    fieldNodeType.appendChild(document.createTextNode("Node type"));
    nodeView.appendChild(fieldNodeType);
    var valueNodeType = document.createElement("p");
    valueNodeType.appendChild(document.createTextNode(_node.nodeType));
    valueNodeType.style.marginBottom = "25px";
    nodeView.appendChild(valueNodeType);
    var button = document.createElement("p");
    var link = document.createElement("a");
    link.text = "Edit";
    link.href = "/" + _node.nodeType + "/Edit/" + _node.id;
    link.className = "ButtonEdit";
    button.appendChild(link);
    nodeView.appendChild(button);
}
function makeInitialGraph() {
    if (nodeView.innerText.length == 13) {
        makeGraph();
    }
}
function makeGraph() {
    var jsonText = document.getElementById("sourceJSON").innerText;
    graph = G.GGraph.ofJSON(jsonText);
    // ajout d'un noeud Add
    var label = new G.GLabel("Add...");
    var node = new G.GNode({ id: "RootAdd", label: label, fill: "yellow" });
    graph.addNode(node);
    var edge = new G.GEdge({ id: "toRootAdd", source: graph.nodes[0].id, target: node.id });
    graph.addEdge(edge);
    graphControl.setGraph(graph);
    graphControl.onNodeClick = (function (n) { return updateNodeView(n); });
    graph.settings.aspectRatio = graphView.offsetWidth / graphView.offsetHeight;
    graph.settings.layout = layeredLayoutCheckBox.checked ? G.GSettings.sugiyamaLayout : G.GSettings.mdsLayout;
    graph.settings.routing = edgeRoutingSelect.value;
    graph.settings.transformation = horizontalLayoutCheckBox.checked ? G.GPlaneTransformation.ninetyDegreesTransformation : G.GPlaneTransformation.defaultTransformation;
    graph.createNodeBoundariesForSVGInContainer(graphView);
    graph.layoutCallbacks.add(function () { graphControl.drawGraph(); });
    graph.beginLayoutGraph();
}
makeInitialGraph();
document.getElementById("reloadGraphButton").onclick = makeGraph;
//# sourceMappingURL=graph.js.map