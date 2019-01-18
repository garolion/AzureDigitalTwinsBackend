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
        public IEnumerable<Models.Type> DeviceTypeList { get; set; }
        public IEnumerable<Models.Type> DeviceSubTypeList { get; set; }

        public DeviceViewModel() { }
        public DeviceViewModel(IMemoryCache memoryCache, Guid? id = null)
        {
            _cache = memoryCache;
            _auth = new AuthenticationHelper();

            try
            {
                LoadAsync(id).Wait();
            }
            catch (Exception ex)
            {
                FeedbackHelper.Channel.SendMessageAsync($"Error - {ex.Message}", MessageType.Info).Wait();
                FeedbackHelper.Channel.SendMessageAsync($"Please check your settings.", MessageType.Info).Wait();
            }
        }

        private async Task LoadAsync(Guid? id = null)
        {
            SpaceList = await DigitalTwinsHelper.GetSpacesAsync(_cache, Loggers.SilentLogger);
            DeviceTypeList = await DigitalTwinsHelper.GetTypesAsync(Models.Types.DeviceType, _cache, Loggers.SilentLogger);
            DeviceSubTypeList = await DigitalTwinsHelper.GetTypesAsync(Models.Types.DeviceSubtype, _cache, Loggers.SilentLogger);

            if (id != null)
            {
                this.SelectedDeviceItem = await DigitalTwinsHelper.GetDeviceAsync((Guid)id, _cache, Loggers.SilentLogger, false);
            }
        }
    }
}
