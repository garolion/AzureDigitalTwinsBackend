using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinsBackend.ViewModels
{
    public class SimulatorViewModel
    {
        IMemoryCache _cache;
        public String DeviceConnectionString { get; set; }

        public List<SimulatedSensor> SimulatedSensorList { get; set; }

        public SimulatedSensor SensorInEdit { get; set; }

        private string selectedSensor;
        public string SelectedSensor
        {
            get
            {
                return selectedSensor;
            }
            set
            {
                selectedSensor = value;

                if (SimulatedSensorList != null)
                {
                    SensorInEdit = SimulatedSensorList.FirstOrDefault(t => t.HardwareId.Equals(value));
                }
            }
        }

        public SimulatorViewModel() { }
        public SimulatorViewModel(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            LoadAsync().Wait();
        }

        public async Task LoadAsync()
        {
            SimulatedSensorList = await CacheHelper.GetSimulatedSensorListFromCacheAsync(_cache);

            DeviceConnectionString = ConfigHelper.Config.parameters.DeviceConnectionString;
        }
    }
}
