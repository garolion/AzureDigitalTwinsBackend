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
    public class SpaceViewModel
    {
        private IMemoryCache _cache;
        private AuthenticationHelper _auth;
        public Space SelectedSpaceItem { get; set; }
        public List<Space> SpaceList { get; set; }
        public IEnumerable<Models.Type> SpaceTypeList { get; set; }
        public IEnumerable<Models.Type> SpaceSubTypeList { get; set; }
        public IEnumerable<Models.Type> SpaceStatusList { get; set; }
        public IEnumerable<UserDefinedFunction> UDFList { get; set; }
        public IEnumerable<PropertyKey> AvailableProperties { get; set; }

        public SpaceViewModel() { }

        public SpaceViewModel(IMemoryCache memoryCache, Guid? id = null)
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

                //We create empty lists to simplify error management in Views.
                SpaceList = new List<Space>();
                SpaceTypeList = new List<Models.Type>();
                SpaceSubTypeList = new List<Models.Type>();
                SpaceStatusList = new List<Models.Type>();
            }
        }

        private async Task LoadAsync(Guid? id = null)
        {
            SpaceList = new List<Space>();
            SpaceList.Add(new Space() { Id = Guid.Empty, Name = "None" });
            SpaceList.AddRange(await DigitalTwinsHelper.GetSpacesAsync(_cache, Loggers.SilentLogger));

            //SpaceList = await DigitalTwinsHelper.GetSpacesAsync(_cache, Loggers.SilentLogger);

            SpaceTypeList = await DigitalTwinsHelper.GetTypesAsync(Models.Types.SpaceType, _cache, Loggers.SilentLogger);
            SpaceSubTypeList = await DigitalTwinsHelper.GetTypesAsync(Models.Types.SpaceSubtype, _cache, Loggers.SilentLogger);
            SpaceStatusList = await DigitalTwinsHelper.GetTypesAsync(Models.Types.SpaceStatus, _cache, Loggers.SilentLogger);

            if (id != null)
            {
                this.SelectedSpaceItem = await DigitalTwinsHelper.GetSpaceAsync((Guid)id, _cache, Loggers.SilentLogger, false);
                this.UDFList = await DigitalTwinsHelper.GetUDFsBySpaceId((Guid)id, _cache, Loggers.SilentLogger);

                this.AvailableProperties = await DigitalTwinsHelper.GetPropertyKeysForSpace((Guid)id, _cache, Loggers.SilentLogger);
            }
        }
    }
}
