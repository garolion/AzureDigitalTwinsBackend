// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public class Ontology
    {
        public int Id {get; set;}
        public string Name { get; set; }
        public bool Loaded { get; set; }
        public List<SystemType> types { get; set; }
    }
}