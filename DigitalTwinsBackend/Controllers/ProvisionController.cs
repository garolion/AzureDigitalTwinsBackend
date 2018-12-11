using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using DigitalTwinsBackend.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using DigitalTwinsBackend.Models;
using YamlDotNet.Serialization;
using DigitalTwinsBackend.Helpers;

namespace DigitalTwinsBackend.Controllers
{
    public class ProvisionController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private SpaceViewModel _model;
        private IMemoryCache _cache;

        public ProvisionController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;

            //_model = new SpaceViewModel(_cache);
        }


        public IActionResult Index()
        {
            var model = new ProvisionViewModel(_cache);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Provision(ProvisionViewModel model)
        {
            CacheHelper.ResetMessagesInCache(_cache);

            IEnumerable<SpaceDescription> spaceCreateDescriptions;
            spaceCreateDescriptions = new Deserializer().Deserialize<IEnumerable<SpaceDescription>>(model.YamlScript);
            var spaces = await DigitalTwinsHelper.CreateSpaces(_cache, Loggers.SilentLogger, spaceCreateDescriptions, model.UDFFiles, model.RootParent.Id);

            model.Messages = CacheHelper.GetMessagesFromCache(_cache);

            return View(model);
        }
    }
}