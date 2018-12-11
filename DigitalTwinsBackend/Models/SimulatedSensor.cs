using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinsBackend.Models
{
    public enum DataTypeEnum
    {
        Motion,
        Temperature,
        CarbonDioxide
    }

    public class SimulatedSensor
    {
        [Display(Name = "Data Type in Azure Digital Twins")]
        [Required]
        public DataTypeEnum DataType { get; set; }
        [Display(Name = "Hardware Id")]
        [Required]
        public string HardwareId { get; set; }
        [Range(1, 100)]
        [Display(Name = "Min value")]
        public int MinValue { get; set; }
        [Range(1, 100)]
        [Display(Name = "Max value")]
        public int MaxValue { get; set; }
        [Range(1, 100)]
        [Display(Name = "Initial value")]
        public string InitialValue { get; set; }
        public string SensorValue { get; set; }
    }
}
