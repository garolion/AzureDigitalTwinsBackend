using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinsBackend.Models
{
    public class Sensor : BaseModel
    {
        [Display(Name = "Device Id")]
        public Guid DeviceId { get; set; }
        public Device Device { get; set; }
        [Display(Name = "Hardware Id")]
        public string HardwareId { get; set; }
        [Display(Name = "Poll rate")]
        public int PollRate { get; set; }
        [Display(Name = "Space Id")]
        public Guid SpaceId { get; set; }
        public Space Space { get; set; }
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

        public override Dictionary<string, object> ToUpdate(IMemoryCache memoryCache, out BaseModel updatedElement)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();
            Sensor refInCache = null;

            if (Id != Guid.Empty)
            {
                refInCache = CacheHelper.GetSensorFromCache(memoryCache, Id);

                if (refInCache != null)
                {
                    if (!DeviceId.Equals(refInCache.DeviceId))
                    {
                        changes.Add("DeviceId", DeviceId);
                        refInCache.DeviceId = DeviceId;
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
                    if (!PollRate.Equals(refInCache.PollRate))
                    {
                        changes.Add("PollRate", PollRate);
                        refInCache.PollRate = PollRate;
                    }
                    if (PortType != null && !PortType.Equals(refInCache.PortType))
                    {
                        changes.Add("PortType", PortType);
                        refInCache.PortType = PortType;
                    }
                    if (DataType != null && !DataType.Equals(refInCache.DataType))
                    {
                        changes.Add("DataType", DataType);
                        refInCache.DataType = DataType;
                    }
                    if (DataUnitType != null && !DataUnitType.Equals(refInCache.DataUnitType))
                    {
                        changes.Add("DataUnitType", DataUnitType);
                        refInCache.DataUnitType = DataUnitType;
                    }
                    if (DataSubtype != null && !DataSubtype.Equals(refInCache.DataSubtype))
                    {
                        changes.Add("DataSubtype", DataSubtype);
                        refInCache.DataSubtype = DataSubtype;
                    }
                }
                else
                {
                    refInCache = this;
                    if (DeviceId != null) changes.Add("DeviceId", DeviceId);
                    if (HardwareId != null) changes.Add("HardwareId", HardwareId);
                    if (SpaceId != null) changes.Add("SpaceId", SpaceId);
                    if (PollRate != 0) changes.Add("PollRate", PollRate);
                    if (PortType != null) changes.Add("PortType", PortType);
                    if (DataType != null) changes.Add("DataType", DataType);
                    if (DataUnitType != null) changes.Add("DataUnitType", DataUnitType);
                    if (DataSubtype != null) changes.Add("DataSubTypeId", DataSubTypeId);
                }
            }
            updatedElement = refInCache;
            return changes;
        }
    }
}