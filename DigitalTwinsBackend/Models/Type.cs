using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinsBackend.Models
{
    public enum Types
    {
        DeviceType,
        DeviceSubtype,
        DeviceBlobType,
        DeviceBlobSubtype,
        SensorDataType,
        SensorDataSubtype,
        SensorDataUnitType,
        SensorType,
        SensorPortType,
        SpaceBlobType,
        SpaceBlobSubtype,
        SpaceStatus,
        SpaceType,
        SpaceSubtype,
        UserBlobType,
        UserBlobSubtype
    }

    public class Type
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string FriendlyName { get; set; }
        public string Name { get; set; }
        public bool Disabled { get; set; }
        public int LogicalOrder { get; set; }
    }
}
