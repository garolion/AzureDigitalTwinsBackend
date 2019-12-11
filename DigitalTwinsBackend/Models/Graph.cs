// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinsBackend.Models
{
    public enum NodeShape
    {
        rect,
        roundedrect,
        maxroundedrect
    }

    public class Rect
    {
        public int x { get; set; }
        public int y { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Label
    {
        public Rect bounds { get; set; }
        public string content { get; set; }
        public string fill { get; set; }
        public string tooltip { get; set; }
    }

    public class Node
    {
        public Guid id { get; set; }
        public Label label { get; set; }
        public string fill { get; set; }
        public string stroke { get; set; }
        public string nodeType { get; set; }
        public string shape { get; set; }
    }

    public class Edge
    {
        public Guid id { get; set; }
        public string label { get; set; }
        //public string stroke { get; set; }
        public Guid source { get; set; }
        public Guid target { get; set; }
        //public int dask { get; set; }
        //public int thickness { get; set; }
    }
    
    public class UpDownConstraint
    {
        public Guid UpNode { get; set; }
        public Guid DownNode { get; set; }
    }

    public class Settings
    {
        public double AspectRatio { get; set; }
        public List<UpDownConstraint> UpDownConstraints { get; set; }

        public Settings()
        {
            UpDownConstraints = new List<UpDownConstraint>();
        }
    }

    public class Graph
    {
        public List<Node> nodes { get; set; }
        public List<Edge> edges { get; set; }
        //public Settings Settings { get; set; }

        public Graph()
        {
            nodes = new List<Node>();
            edges = new List<Edge>();
        }

    }
}