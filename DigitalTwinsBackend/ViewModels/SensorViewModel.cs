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
        public IEnumerable<SystemType> DataTypeList { get; set; }
        public IEnumerable<SystemType> DataUnitTypeList { get; set; }
        public IEnumerable<SystemType> DataSubTypeList { get; set; }
        public IEnumerable<Space> SpaceList { get; set; }

        public SensorViewModel() { }
        public SensorViewModel(IMemoryCache memoryCache, Guid? id = null)
        {
            _cache = memoryCache;

            LoadAsync().Wait();

            if (id != null)
            {
                LoadSelectedSensorAsync((Guid)id).Wait();
            }
        }

        public async Task LoadSelectedSensorAsync(Guid id)
        {
            this.SelectedSensor = await DigitalTwinsHelper.GetSensorAsync(id, _cache, Loggers.SilentLogger, false);
        }

        public async Task LoadAsync()
        {
            SpaceList = await DigitalTwinsHelper.GetSpacesAsync(_cache, Loggers.SilentLogger);

            DataTypeList = await DigitalTwinsHelper.GetTypesAsync(SystemTypes.SensorDataType, _cache, Loggers.SilentLogger);
            DataUnitTypeList = await DigitalTwinsHelper.GetTypesAsync(SystemTypes.SensorDataUnitType, _cache, Loggers.SilentLogger);
            DataSubTypeList = await DigitalTwinsHelper.GetTypesAsync(SystemTypes.SensorDataSubtype, _cache, Loggers.SilentLogger);
        }

    }
}
