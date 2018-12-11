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
        public List<SystemType> SpaceTypeList { get; set; }

        public SpacesViewModel() { }
        public SpacesViewModel(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _auth = new AuthenticationHelper();

            LoadAsync().Wait();
        }

        internal async Task LoadAsync()
        {
            SpaceTypeList = new List<SystemType>();
            SpaceTypeList.Add(new SystemType() { Name = "All" });
            SpaceTypeList.AddRange(await DigitalTwinsHelper.GetTypesAsync(SystemTypes.SpaceType, _cache, Loggers.SilentLogger));
        }
    }
}
