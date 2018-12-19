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
        public IEnumerable<Property> Properties { get; set; }
        public IEnumerable<Sensor> Sensors { get; set; }

        public override string Label { get { return Name; } }

        public Device()
        {
            Sensors = new List<Sensor>();
        }

        //public Dictionary<string, object> ToCreate()
        public override Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("Name", Name);
            createFields.Add("HardwareId", HardwareId);
            createFields.Add("SpaceId", SpaceId);

            return createFields;
        }

        //public Dictionary<string, object> ToUpdate(IMemoryCache memoryCache)
        public override Dictionary<string, object> ToUpdate(IMemoryCache memoryCache)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();

            Device oldValue = null;
            if (Id != Guid.Empty)
            {
                oldValue = CacheHelper.GetDeviceFromCache(memoryCache, Id);
                //changes.Add("Id", Id);

                if (oldValue != null)
                {
                    if (Name != null && !Name.Equals(oldValue.Name)) changes.Add("Name", Name);
                    if (HardwareId != null && !HardwareId.Equals(oldValue.HardwareId)) changes.Add("HardwareId", HardwareId);
                    if (!SpaceId.Equals(oldValue.SpaceId)) changes.Add("SpaceId", SpaceId);
                    if (!TypeId.Equals(oldValue.TypeId)) changes.Add("TypeId", TypeId);
                    if (!SubTypeId.Equals(oldValue.SubTypeId)) changes.Add("SubTypeId", SubTypeId);
                }
                else
                {
                    changes.Add("Name", Name);
                    changes.Add("HardwareId", HardwareId);
                    changes.Add("SpaceId", SpaceId);
                    changes.Add("Type", Type);
                    changes.Add("SubType", SubType);
                }
            }
            return changes;
        }
    }
}