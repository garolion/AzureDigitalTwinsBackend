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
    public class ProvisionController : BaseController
    {
        //private readonly IHttpContextAccessor _httpContextAccessor;
        //private SpaceViewModel _model;
        //private IMemoryCache _cache;

        public ProvisionController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;

            //_model = new SpaceViewModel(_cache);
        }


        public IActionResult Index()
        {
            //Reset();
            var model = new ProvisionViewModel(_cache);
            SendViewData();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Provision(ProvisionViewModel model)
        {
            Reset();

            try
            {
                if (model.YamlScript == null)
                    throw new Exception("You must enter a valid YAML Script!");

                IEnumerable<SpaceDescription> spaceCreateDescriptions;
                spaceCreateDescriptions = new Deserializer().Deserialize<IEnumerable<SpaceDescription>>(model.YamlScript);

                model.CreatedSpaces = await DigitalTwinsHelper.CreateSpaces(_cache, Loggers.SilentLogger, spaceCreateDescriptions, model.UDFFiles, model.RootParent.Id);
                model.Messages = CacheHelper.GetInfoMessagesFromCache(_cache);
                SendViewData();
                return View(model);
            }
            catch (Exception ex)
            {
                FeedbackHelper.Channel.SendMessageAsync($"Error - {ex.Message}", MessageType.Info).Wait();
                SendViewData();
                return RedirectToAction(nameof(Index));
            }

        }
    }
}