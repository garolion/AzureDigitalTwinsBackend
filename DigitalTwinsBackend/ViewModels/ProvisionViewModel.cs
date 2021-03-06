﻿using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DigitalTwinsBackend.ViewModels
{
    public class ProvisionViewModel
    {
        private IMemoryCache _cache;
        private AuthenticationHelper _auth;

        public List<IFormFile> UDFFiles { get; set; }
        public string YamlScript { get; set; }
        public Space RootParent { get; set; }
        public List<Space> SpaceList { get; set; }
        public List<string> Messages { get; set; }

        public IEnumerable<UISpace> CreatedSpaces { get; set; }

        public ProvisionViewModel() { }
        public ProvisionViewModel(IMemoryCache memoryCache)
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

        internal async Task LoadAsync(Guid? id = null)
        {
            SpaceList = new List<Space>();
            SpaceList.Add(new Space() { Name = "None", Id = Guid.Empty });
            SpaceList.AddRange(await DigitalTwinsHelper.GetSpacesAsync(_cache, Loggers.SilentLogger));
        }
    }
}
