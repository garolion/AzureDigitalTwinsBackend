// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public class UserDefinedFunctionUpdate
    {
        public Guid Id { get; set; }
        public IEnumerable<string> Matchers { get; set; }
        public string Name { get; set; }
        public string SpaceId { get; set; }
    }
}