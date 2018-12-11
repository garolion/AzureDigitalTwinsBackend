using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;

using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Hubs;
using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Threading;

namespace DigitalTwinsBackend.ViewModels
{
    public class DeviceViewModel
    {
        private IMemoryCache _cache;
        private AuthenticationHelper _auth;

        public Device SelectedDeviceItem { get; set; }
        public IEnumerable<Space> SpaceList { get; set; }
        public IEnumerable<SystemType> DeviceTypeList { get; set; }
        public IEnumerable<SystemType> DeviceSubTypeList { get; set; }
               

        public DeviceViewModel() { }
        public DeviceViewModel(IMemoryCache memoryCache, Guid? id = null)
        {
            _cache = memoryCache;
            _auth = new AuthenticationHelper();

            LoadAsync().Wait();

            if (id != null)
            {
                LoadSelectedSpaceItemAsync((Guid)id).Wait();
            }
        }

        private async Task LoadSelectedSpaceItemAsync(Guid id)
        {
            this.SelectedDeviceItem = await DigitalTwinsHelper.GetDeviceAsync(id, _cache, Loggers.SilentLogger, false);

        }

        private async Task LoadAsync()
        {
            SpaceList = await DigitalTwinsHelper.GetSpacesAsync(_cache, Loggers.SilentLogger);

            DeviceTypeList = await DigitalTwinsHelper.GetTypesAsync(SystemTypes.DeviceType, _cache, Loggers.SilentLogger);
            DeviceSubTypeList = await DigitalTwinsHelper.GetTypesAsync(SystemTypes.DeviceSubtype, _cache, Loggers.SilentLogger);
        }
    }
}
