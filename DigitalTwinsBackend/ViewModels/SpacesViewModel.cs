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
    public class SpacesViewModel
    {
        private IMemoryCache _cache;
        private AuthenticationHelper _auth;

        public String SearchString { get; set; }
        public String SearchType { get; set; }
        public Space SelectedSpaceItem { get; set; }
        public IEnumerable<Space> SpaceList { get; set; }
        public IEnumerable<Space> AncestorSpaceList { get; set; }
        public IEnumerable<Space> ChildrenSpaceList { get; set; }
        public List<Models.Type> SpaceTypeList { get; set; }

        public SpacesViewModel() { }
        public SpacesViewModel(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _auth = new AuthenticationHelper();

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

        private async Task LoadAsync()
        {
            SpaceTypeList = new List<Models.Type>();
            SpaceTypeList.Add(new Models.Type() { Name = "All" });
            SpaceTypeList.AddRange(await DigitalTwinsHelper.GetTypesAsync(Models.Types.SpaceType, _cache, Loggers.SilentLogger, onlyEnabled: true));
        }
    }
}
