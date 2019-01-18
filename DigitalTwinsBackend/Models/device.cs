// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinsBackend.Models
{
    public class Device : BaseModel
    {
        public string Name { get; set; }
        [Display(Name = "Hardware Id")]
        public string HardwareId { get; set; }
        [Display(Name = "Space Id")]
        public Guid SpaceId { get; set; }
        public Space Space { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; }
        public int SubTypeId { get; set; }
        [Display(Name = "Sub Type")]
        public string SubType { get; set; }
        public string Status { get; set; }
        public string ConnectionString { get; set; }
        public IEnumerable<Sensor> Sensors { get; set; }

        public override string Label { get { return Name; } }

        public Device()
        {
            Sensors = new List<Sensor>();
        }

        public override Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("Name", Name);
            createFields.Add("HardwareId", HardwareId);
            createFields.Add("SpaceId", SpaceId);

            return createFields;
        }

        public override Dictionary<string, object> ToUpdate(IMemoryCache memoryCache, out BaseModel updatedElement)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();

            Device refInCache = null;
            if (Id != Guid.Empty)
            {
                refInCache = CacheHelper.GetDeviceFromCache(memoryCache, Id);

                if (refInCache != null)
                {
                    if (Name != null && !Name.Equals(refInCache.Name))
                    {
                        changes.Add("Name", Name);
                        refInCache.Name = Name;
                    }
                    if (HardwareId != null && !HardwareId.Equals(refInCache.HardwareId))
                    {
                        changes.Add("HardwareId", HardwareId);
                        refInCache.HardwareId = HardwareId;
                    }
                    if (SpaceId != null && !SpaceId.Equals(refInCache.SpaceId))
                    {
                        changes.Add("SpaceId", SpaceId);
                        refInCache.SpaceId = SpaceId;
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
                }
                else
                {
                    refInCache = this;

                    if (Name != null) changes.Add("Name", Name);
                    if (HardwareId != null)  changes.Add("HardwareId", HardwareId);
                    if (SpaceId != null)  changes.Add("SpaceId", SpaceId);
                    if (Type != null)  changes.Add("Type", Type);
                    if (SubType != null)  changes.Add("SubType", SubType);
                }
            }
            updatedElement = refInCache;
            return changes;
        }
    }
}