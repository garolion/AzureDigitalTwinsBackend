using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public class Sensor
    {
        public Guid Id { get; set; }
        public string DataType { get; set; }
        public int DataTypeId { get; set; }
        public Guid DeviceId { get; set; }
        public string HardwareId { get; set; }
        public int PollRate { get; set; }
        public Guid SpaceId { get; set; }
        public int PortTypeId { get; set; }
        public int DataUnitTypeId { get; set; }
        public string DataUnitType { get; set; }
        public int DataSubTypeId { get; set; }
        public string DataSubtype { get; set; }
        public SpaceValue Value { get; set; }

        public Sensor()
        {
        }
        //    public Sensor(SensorCreate sensor)
        //{
        //    this.DataType = sensor.DataType;
        //    this.DeviceId = sensor.DeviceId;
        //    this.HardwareId = sensor.HardwareId;
        //}


        public Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("DataType", DataType);
            createFields.Add("HardwareId", HardwareId);
            createFields.Add("DeviceId", DeviceId);

            return createFields;
        }

        public Dictionary<string, object> ToUpdate(IMemoryCache memoryCache)
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
                    if (!HardwareId.Equals(oldValue.HardwareId)) changes.Add("HardwareId", HardwareId);
                    if (!SpaceId.Equals(oldValue.SpaceId)) changes.Add("SpaceId", SpaceId);
                    if (!PollRate.Equals(oldValue.PollRate)) changes.Add("PollRate", PollRate);
                    if (!PortTypeId.Equals(oldValue.PortTypeId)) changes.Add("PortTypeId", PortTypeId);
                    if (!DataTypeId.Equals(oldValue.DataTypeId)) changes.Add("DataTypeId", DataTypeId);
                    if (!DataUnitTypeId.Equals(oldValue.DataUnitTypeId)) changes.Add("DataUnitTypeId", DataUnitTypeId);
                    if (!DataSubTypeId.Equals(oldValue.DataSubTypeId)) changes.Add("DataSubTypeId", DataSubTypeId);
                    if (!Value.Equals(oldValue.Value)) changes.Add("Value", Value);
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
                    changes.Add("Value", Value);
                }
            }
            return changes;
        }
    }
}