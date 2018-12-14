// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public abstract class BaseModel
    {
        public Guid Id { get; set; }

        public abstract string Label { get; }

        public abstract Dictionary<string, object> ToCreate();

        public abstract Dictionary<string, object> ToUpdate(IMemoryCache memoryCache);


    }
}