// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace DigitalTwinsBackend.Models
{
    public enum Scope
    {
        Spaces,
        Sensors,
        Devices,
        Users,
        None
    }

    public class PropertyKey
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Scope Scope { get; set; }
        public Guid SpaceId { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string PrimitiveDataType { get; set; }
        public string ValidationData { get; set; }
        public string Min { get; set; }
        public string Max { get; set; }
    }
}