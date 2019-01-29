// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public class Resource : BaseModel
    {
        public Guid SpaceId { get; set; }
        public string Region { get; set; }
        public string Type { get; set; }
        public bool IsExternallyCreated { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
        public int InstanceNum { get; set; }
        public new ResourceProperties Properties { get; set; }

    public override string Label { get { return Id.ToString(); } }

        public override Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("SpaceId", SpaceId);
            createFields.Add("Type", Type);
            createFields.Add("Region", Region);

            return createFields;
        }

        public override Dictionary<string, object> ToUpdate(IMemoryCache memoryCache, out BaseModel updatedElement)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();

            if (SpaceId != null)  changes.Add("SpaceId", SpaceId);
            if (Type != null) changes.Add("Type", Type);
            if (Region != null) changes.Add("Region", Region);
            if (Properties != null) changes.Add("Properties", Properties);

            updatedElement = null;
            return changes;
        }
    }
}