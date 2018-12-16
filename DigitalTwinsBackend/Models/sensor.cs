using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public class Sensor : BaseModel
    {
        public Guid DeviceId { get; set; }
        public string HardwareId { get; set; }
        public int PollRate { get; set; }
        public Guid SpaceId { get; set; }

        public string Type { get; set; }
        public int TypeId { get; set; }

        public string PortType { get; set; }
        public int PortTypeId { get; set; }

        public string DataUnitType { get; set; }
        public int DataUnitTypeId { get; set; }

        public string DataType { get; set; }
        public int DataTypeId { get; set; }

        public string DataSubtype { get; set; }
        public int DataSubTypeId { get; set; }

        public SpaceValue Value { get; set; }

        public override string Label { get { return HardwareId; } }

        public override Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("DataType", DataType);
            createFields.Add("HardwareId", HardwareId);
            createFields.Add("DeviceId", DeviceId);

            return createFields;
        }

        public override Dictionary<string, object> ToUpdate(IMemoryCache memoryCache)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();

            Sensor oldValue = null;
            if (Id != Guid.Empty)
            {
                oldValue = CacheHelper.GetSensorFromCache(memoryCache, Id);
                //changes.Add("Id", Id);

                if (oldValue != null)
                {
                    if (!DeviceId.Equals(oldValue.DeviceId)) changes.Add("DeviceId", DeviceId);
                    if (HardwareId != null && !HardwareId.Equals(oldValue.HardwareId)) changes.Add("HardwareId", HardwareId);
                    if (!SpaceId.Equals(oldValue.SpaceId)) changes.Add("SpaceId", SpaceId);
                    if (!PollRate.Equals(oldValue.PollRate)) changes.Add("PollRate", PollRate);
                    if (!PortTypeId.Equals(oldValue.PortTypeId)) changes.Add("PortTypeId", PortTypeId);
                    if (!DataTypeId.Equals(oldValue.DataTypeId)) changes.Add("DataTypeId", DataTypeId);
                    if (!DataUnitTypeId.Equals(oldValue.DataUnitTypeId)) changes.Add("DataUnitTypeId", DataUnitTypeId);
                    if (!DataSubTypeId.Equals(oldValue.DataSubTypeId)) changes.Add("DataSubTypeId", DataSubTypeId);
                }
                else
                {
                    changes.Add("DeviceId", DeviceId);
                    changes.Add("HardwareId", HardwareId);
                    changes.Add("SpaceId", SpaceId);
                    changes.Add("PollRate", PollRate);
                    changes.Add("PortTypeId", PortTypeId);
                    changes.Add("DataTypeId", DataTypeId);
                    changes.Add("DataUnitTypeId", DataUnitTypeId);
                    changes.Add("DataSubTypeId", DataSubTypeId);
                }
            }
            return changes;
        }
    }
}