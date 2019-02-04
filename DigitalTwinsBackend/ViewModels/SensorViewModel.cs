using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinsBackend.ViewModels
{
    public class SensorViewModel
    {
        private IMemoryCache _cache;

        public Sensor SelectedSensor { get; set; }
        public IEnumerable<Models.Type> DataTypeList { get; set; }
        public IEnumerable<Models.Type> DataUnitTypeList { get; set; }
        public IEnumerable<Models.Type> DataSubTypeList { get; set; }
        public IEnumerable<Space> SpaceList { get; set; }

        public SensorViewModel() { }
        public SensorViewModel(IMemoryCache memoryCache, Guid? id = null)
        {
            _cache = memoryCache;

            try
            {
                LoadAsync().Wait();

                if (id != null)
                {
                    LoadSelectedSensorAsync((Guid)id).Wait();
                }
            }
            catch (Exception ex)
            {
                FeedbackHelper.Channel.SendMessageAsync($"Error - {ex.Message}", MessageType.Info).Wait();
                FeedbackHelper.Channel.SendMessageAsync($"Please check your settings.", MessageType.Info).Wait();
            }
        }

        private async Task LoadSelectedSensorAsync(Guid id)
        {
            this.SelectedSensor = await DigitalTwinsHelper.GetSensorAsync(id, _cache, Loggers.SilentLogger, false);
        }

        private async Task LoadAsync()
        {
            SpaceList = await DigitalTwinsHelper.GetSpacesAsync(_cache, Loggers.SilentLogger);

            DataTypeList = await DigitalTwinsHelper.GetTypesAsync(Models.Types.SensorDataType, _cache, Loggers.SilentLogger, onlyEnabled: true);
            DataUnitTypeList = await DigitalTwinsHelper.GetTypesAsync(Models.Types.SensorDataUnitType, _cache, Loggers.SilentLogger, onlyEnabled: true);
            DataSubTypeList = await DigitalTwinsHelper.GetTypesAsync(Models.Types.SensorDataSubtype, _cache, Loggers.SilentLogger, onlyEnabled: true);
        }

    }
}
