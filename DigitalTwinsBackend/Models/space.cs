// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinsBackend.Models
{
    public class Space : BaseModel
    {
        public string Name { get; set; }
        [Display(Name = "Friendy Name")]
        public string FriendlyName { get; set; }
        public string Type { get; set; }
        public int TypeId { get; set; }
        [Display(Name = "Parent space")]
        public Guid ParentSpaceId { get; set; }
        [Display(Name = "Sub Type")]
        public string SubType { get; set; }
        public int SubTypeId { get; set; }
        public string Status { get; set; }
        public int StatusId { get; set; }
        public Space Parent { get; set; }
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
            //Properties = new List<Property>();
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

        public override Dictionary<string, object> ToUpdate(IMemoryCache memoryCache, out BaseModel updatedElement)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();
            Space refInCache = null;

            if (Id != Guid.Empty)
            {
                refInCache = CacheHelper.GetSpaceFromCache(memoryCache, Id);

                if (refInCache != null)
                {
                    if (Name != null && !Name.Equals(refInCache.Name))
                    {
                        changes.Add("Name", Name);
                        refInCache.Name = Name;
                    }
                    if (FriendlyName != null && !FriendlyName.Equals(refInCache.FriendlyName))
                    {
                        changes.Add("FriendlyName", FriendlyName);
                        refInCache.FriendlyName = FriendlyName;
                    }
                    if (Type != null && !Type.Equals(refInCache.Type))
                    {
                        changes.Add("Type", Type);
                        refInCache.Type = Type;
                    }
                    if (SubType != null && !SubType.Equals(refInCache.SubType))
                    {
                        changes.Add("SubType", SubType);
                        refInCache.SubType = SubType;
                    }
                    if (ParentSpaceId != Guid.Empty && !ParentSpaceId.Equals(refInCache.ParentSpaceId))
                    {
                        changes.Add("ParentSpaceId", ParentSpaceId);
                        refInCache.ParentSpaceId = ParentSpaceId;
                    }
                    if (Status != null && !Status.Equals(refInCache.Status))
                    {
                        changes.Add("Status", Status);
                        refInCache.Status = Status;
                    }
                    if (PropertiesHasChanged)
                    {
                        changes.Add("Properties", Properties);
                        refInCache.Properties = Properties;
                    }
                }
                else
                {
                    refInCache = this;

                    if (Name != null) changes.Add("Name", Name);
                    if (FriendlyName != null) changes.Add("FriendlyName", FriendlyName);
                    if (Type != null) changes.Add("Type", Type);
                    if (ParentSpaceId != Guid.Empty) changes.Add("ParentSpaceId", ParentSpaceId);
                    if (SubType != null) changes.Add("SubType", SubType);
                    if (Status != null) changes.Add("Status", Status);
                    if (PropertiesHasChanged) changes.Add("Properties", Properties);
                }
            }
            updatedElement = refInCache;
            return changes;
        }
    }
}