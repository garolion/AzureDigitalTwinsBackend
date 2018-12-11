// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public class Resource
    {
        public string Id { get; set; }
        public string SpaceId { get; set; }
        public string Region { get; set; }
        public string Type { get; set; }
        public bool IsExternallyCreated { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
        public ResourceProperties Properties { get; set; }
        public int InstanceNum { get; set; }

        public Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("SpaceId", SpaceId);
            createFields.Add("Type", Type);
            createFields.Add("Region", Region);

            return createFields;
        }

        public Dictionary<string, object> ToUpdate(IMemoryCache memoryCache)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();

            changes.Add("SpaceId", SpaceId);
            changes.Add("Type", Type);
            changes.Add("Region", Region);
            changes.Add("Properties", Properties);

            return changes;
        }
    }
}