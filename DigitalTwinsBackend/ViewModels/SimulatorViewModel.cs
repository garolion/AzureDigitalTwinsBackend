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
        public Guid SelectedDevice { get; set; }
        public IEnumerable<Device> DeviceList { get; set; }
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

            try
            {
                LoadAsync().Wait();
            }
            catch (Exception ex)
            {
                FeedbackHelper.Channel.SendMessageAsync($"Error - {ex.Message}", MessageType.Info).Wait();
                FeedbackHelper.Channel.SendMessageAsync($"Please check your settings.", MessageType.Info).Wait();
            }
        }

        public async Task LoadAsync()
        {
            SimulatedSensorList = await CacheHelper.GetSimulatedSensorListFromCacheAsync(_cache);

            DeviceList = await DigitalTwinsHelper.GetDevicesAsync(_cache, Loggers.SilentLogger);
        }
    }
}
