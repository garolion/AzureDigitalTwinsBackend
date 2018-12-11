using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace DigitalTwinsBackend.Models
{
    [DataContract(Name = "CustomTelemetryMessage")]
    public class CustomTelemetryMessage
    {
        [DataMember(Name = "SensorValue")]
        public string SensorValue { get; set; }
    }
}
