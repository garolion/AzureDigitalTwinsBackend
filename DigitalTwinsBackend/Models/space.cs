// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public class Space : BaseModel
    {
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string Type { get; set; }
        public int TypeId { get; set; }
        public Guid ParentSpaceId { get; set; }
        public string SubType { get; set; }
        public int SubTypeId { get; set; }
        public string Status { get; set; }
        public int StatusId { get; set; }
        public Space Parent { get; set; }
        public IEnumerable<Property> Properties { get; set; }
        public IEnumerable<Space> Children { get; set; }
        public IEnumerable<string> SpacePaths { get; set; }
        public IEnumerable<SpaceValue> Values { get; set; }
        public IEnumerable<Resource> Resources { get; set; }
        public IEnumerable<Device> Devices { get; set; }
        public IEnumerable<Sensor> Sensors { get; set; }
        public IEnumerable<string> Users { get; set; }

        public override string Label { get { return Name; } }

        public Space()
        {
            Properties = new List<Property>();
            Children = new List<Space>();
            SpacePaths = new List<string>();
            Values = new List<SpaceValue>();
            Resources = new List<Resource>();
            Devices = new List<Device>();
            Sensors = new List<Sensor>();
            Users = new List<string>();
        }

        public override Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("Name", Name);
            createFields.Add("Type", Type);
            if (ParentSpaceId != Guid.Empty)
            {
                createFields.Add("ParentSpaceId", ParentSpaceId);
            }
            createFields.Add("SubType", SubType);

            return createFields;
        }

        public override Dictionary<string, object> ToUpdate(IMemoryCache memoryCache)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();

            Space oldValue = null;
            if (Id != Guid.Empty)
            {
                oldValue = CacheHelper.GetSpaceFromCache(memoryCache, Id);
                //changes.Add("Id", Id);

                if (oldValue != null)
                {
                    if (Name!=null && !Name.Equals(oldValue.Name)) changes.Add("Name", Name);
                    if (FriendlyName != null && !FriendlyName.Equals(oldValue.FriendlyName)) changes.Add("FriendlyName", FriendlyName);
                    if (!TypeId.Equals(oldValue.TypeId)) changes.Add("TypeId", TypeId);
                    if (ParentSpaceId != Guid.Empty && !ParentSpaceId.Equals(oldValue.ParentSpaceId)) changes.Add("ParentSpaceId", ParentSpaceId);
                    if (!SubTypeId.Equals(oldValue.SubTypeId)) changes.Add("SubTypeId", SubTypeId);
                    if (!StatusId.Equals(oldValue.StatusId)) changes.Add("StatusId", StatusId);
                }
                else
                {
                    changes.Add("Name", Name);
                    changes.Add("FriendlyName", FriendlyName);
                    changes.Add("TypeId", TypeId);
                    if (ParentSpaceId != Guid.Empty)
                        changes.Add("ParentSpaceId", ParentSpaceId);
                    changes.Add("SubType", SubType);
                    changes.Add("Status", Status);
                }
            }
            return changes;
        }
    }
}