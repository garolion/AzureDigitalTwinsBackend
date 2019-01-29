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
using DigitalTwinsBackend.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DigitalTwinsBackend.Controllers
{
    public class MatcherController : BaseController
    {
        public MatcherController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }

        [HttpGet]
        public ActionResult Add(Guid spaceId, Guid udfId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());
            MatcherViewModel model = new MatcherViewModel(_cache, spaceId:spaceId );
            model.UdfId = udfId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(MatcherViewModel model, string createButton)
        {
            string previousPage = CacheHelper.GetPreviousPage(_cache);

            if (createButton.Equals("Cancel"))
            {
                return Redirect(previousPage);
            }

            var matcher = await DigitalTwinsHelper.GetMatcher(model.SelectedMatcher.Id, _cache, Loggers.SilentLogger);
            matcher.Name = model.SelectedMatcher.Name;

            // we create the matcher
            await DigitalTwinsHelper.CreateMatcherAsync(matcher, _cache, Loggers.SilentLogger);

            // we associate the matcher with the UDF
            var udf = await DigitalTwinsHelper.GetUserDefinedFunction(model.UdfId, _cache, Loggers.SilentLogger);
            udf.Matchers.Add(matcher);
            await DigitalTwinsHelper.CreateOrPatchUserDefinedFunctionAsync(_cache, Loggers.SilentLogger, udf);

            return Redirect(previousPage);
        }

        [HttpGet]
        public ActionResult AddCondition()
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());
            MatcherViewModel model = new MatcherViewModel(_cache);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddCondition(MatcherViewModel model, string createButton)
        {
            string previousPage = CacheHelper.GetPreviousPage(_cache);

            var matcher = await DigitalTwinsHelper.GetMatcher(model.SelectedMatcher.Id, _cache, Loggers.SilentLogger);
            if (matcher != null)
            {
                matcher.Conditions.Add(model.SelectedMatcherCondition);
                CacheHelper.AddInCache(_cache, matcher, matcher.Id, Context.Matcher);
            }         
            return Redirect(previousPage);
        }
    }
}