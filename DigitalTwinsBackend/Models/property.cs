// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace DigitalTwinsBackend.Models
{
    public class Property
    {
        public string DataType { get; set; }
        public bool ShouldSerializeDataType() { return false; }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}