// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public class RoleAssignment
    {
        public Guid Id { get; set; }
        public Guid ObjectId { get; set; }
        public string ObjectIdType { get; set; }
        public string Path { get; set; }
        public Guid RoleId { get; set; }
        public Guid TenantId { get; set; }

        public Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("ObjectId", ObjectId);
            createFields.Add("ObjectIdType", ObjectIdType);
            createFields.Add("Path", Path);
            createFields.Add("RoleId", RoleId);
            createFields.Add("TenantId", TenantId);

            return createFields;
        }
    }
}