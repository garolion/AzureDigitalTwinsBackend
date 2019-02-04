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
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinsBackend.ViewModels
{
    public class PropertyKeyViewModel
    {
        private IMemoryCache _cache;
        private AuthenticationHelper _auth;
        public PropertyKey SelectedPropertyKey { get; set; }
        public IEnumerable<Space> SpaceList { get; set; }
        public List<string> PrimitiveDataTypeList;


        public PropertyKeyViewModel() { }

        public PropertyKeyViewModel(IMemoryCache memoryCache, string id = null)
        {
            _cache = memoryCache;
            _auth = new AuthenticationHelper();

            try
            {
                LoadAsync(id).Wait();
            }
            catch (Exception ex)
            {
                SpaceList = new List<Space>();

                FeedbackHelper.Channel.SendMessageAsync($"Error - {ex.Message}", MessageType.Info).Wait();
                FeedbackHelper.Channel.SendMessageAsync($"Please check your settings.", MessageType.Info).Wait();
            }
        }

        private async Task LoadAsync(string id)
        {
            SpaceList = await DigitalTwinsHelper.GetSpacesAsync(_cache, Loggers.SilentLogger);

            PrimitiveDataTypeList = new List<string>();
            PrimitiveDataTypeList.Add("Bool");
            PrimitiveDataTypeList.Add("String");
            PrimitiveDataTypeList.Add("Long");
            PrimitiveDataTypeList.Add("Int");
            PrimitiveDataTypeList.Add("Uint");
            PrimitiveDataTypeList.Add("DateTime");
            PrimitiveDataTypeList.Add("Set");
            PrimitiveDataTypeList.Add("Enum");
            PrimitiveDataTypeList.Add("Json");

            if (id != null)
            {
                this.SelectedPropertyKey = await DigitalTwinsHelper.GetPropertyKeyAsync(id, _cache, Loggers.SilentLogger, false);
            }
        }
    }
}
