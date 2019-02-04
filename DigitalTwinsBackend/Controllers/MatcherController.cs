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
        public ActionResult Create(Guid spaceId, Guid udfId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());
            MatcherViewModel model = new MatcherViewModel(_cache, spaceId:spaceId );
            model.UdfId = udfId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MatcherViewModel model, string createButton)
        {
            string previousPage = CacheHelper.GetPreviousPage(_cache);

            if (createButton.Equals("Cancel"))
            {
                return Redirect(previousPage);
            }

            // we load the matcher in cache to restore the conditions previously defined
            var matcher = await DigitalTwinsHelper.GetMatcherAsync(model.SelectedMatcher.Id, _cache, Loggers.SilentLogger, false);
            matcher.Name = model.SelectedMatcher.Name;

            // we create the matcher
            await DigitalTwinsHelper.CreateMatcherAsync(matcher, _cache, Loggers.SilentLogger);

            // we associate the matcher with the UDF
            var udf = await DigitalTwinsHelper.GetUserDefinedFunction(model.UdfId, _cache, Loggers.SilentLogger, false);
            udf.Matchers.Add(matcher);
            await DigitalTwinsHelper.UpdateUserDefinedFunctionAsync(_cache, Loggers.SilentLogger, udf);

            //we remove the matcher from the cache
            CacheHelper.AddInCache(_cache, null, Guid.Empty, Context.Matcher);

            return RedirectToAction("Edit", "UDF", new { id = model.UdfId });
        }

        [HttpGet]
        public ActionResult Edit(Guid matcherId, Guid udfId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());
            MatcherViewModel model = new MatcherViewModel(_cache, id: matcherId);
            model.UdfId = udfId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MatcherViewModel model, string updateButton)
        {
            string previousPage = CacheHelper.GetPreviousPage(_cache);

            if (updateButton.Equals("Cancel"))
            {
                return Redirect(previousPage);
            }

            // we load the matcher in cache to restore the conditions previously defined
            var matcher = await DigitalTwinsHelper.GetMatcherAsync(model.SelectedMatcher.Id, _cache, Loggers.SilentLogger, false);
            matcher.Name = model.SelectedMatcher.Name;

            // we update the matcher
            await DigitalTwinsHelper.UpdateMatcherAsync(matcher, _cache, Loggers.SilentLogger);

            // we associate the matcher with the UDF
            //var udf = await DigitalTwinsHelper.GetUserDefinedFunction(model.UdfId, _cache, Loggers.SilentLogger, false);
            //udf.Matchers.Add(matcher);
            //await DigitalTwinsHelper.UpdateUserDefinedFunctionAsync(_cache, Loggers.SilentLogger, udf);

            //we remove the matcher from the cache
            CacheHelper.AddInCache(_cache, null, Guid.Empty, Context.Matcher);

            return RedirectToAction("Edit", "UDF", new { id = model.UdfId });
        }

        [HttpGet]
        public ActionResult Delete(Guid matcherId, Guid udfId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());
            MatcherViewModel model = new MatcherViewModel(_cache, id: matcherId);
            model.UdfId = udfId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(MatcherViewModel model, string updateButton)
        {
            string previousPage = CacheHelper.GetPreviousPage(_cache);

            if (updateButton.Equals("Cancel"))
            {
                return Redirect(previousPage);
            }

            await DigitalTwinsHelper.DeleteMatcherAsync(model.SelectedMatcher, _cache, Loggers.SilentLogger);

            return RedirectToAction("Edit", "UDF", new { id = model.UdfId });
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

            var matcher = await DigitalTwinsHelper.GetMatcherAsync(model.SelectedMatcher.Id, _cache, Loggers.SilentLogger, false);
            if (matcher != null)
            {
                matcher.Conditions.Add(model.SelectedMatcherCondition);
                CacheHelper.AddInCache(_cache, matcher, matcher.Id, Context.Matcher);
            }         
            return Redirect(previousPage);
        }

        [HttpGet]
        public ActionResult EditCondition(Guid matcherId, Guid conditionId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());
            
            MatcherViewModel model = new MatcherViewModel(_cache, id: matcherId);
            model.SelectedMatcherCondition = model.SelectedMatcher.Conditions.FirstOrDefault(m => m.Id == conditionId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCondition(MatcherViewModel model, string updateButton)
        {
            string previousPage = CacheHelper.GetPreviousPage(_cache);

            if (updateButton.Equals("Cancel"))
            {
                return Redirect(previousPage);
            }

            var matcher = await DigitalTwinsHelper.GetMatcherAsync(model.SelectedMatcher.Id, _cache, Loggers.SilentLogger, false);
            if (matcher != null)
            {
                var index = matcher.Conditions.FindIndex(m => m.Id == model.SelectedMatcherCondition.Id);

                if (updateButton.Equals("Update"))
                {
                    matcher.Conditions[index] = model.SelectedMatcherCondition;
                }

                if (updateButton.Equals("Delete"))
                {
                    matcher.Conditions.RemoveAt(index);
                }

                CacheHelper.AddInCache(_cache, matcher, matcher.Id, Context.Matcher);
            }
            return Redirect(previousPage);
        }

        [HttpGet]
        public async Task<IActionResult> Add(Guid spaceId, Guid udfId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            if (udfId != Guid.Empty)
            {
                CacheHelper.SetObjectId(_cache, udfId);
                Space space = await DigitalTwinsHelper.GetSpaceAsync((Guid)spaceId, _cache, Loggers.SilentLogger);
                var matchers = (await DigitalTwinsHelper.GetMatchersBySpaceId((Guid)spaceId, _cache, Loggers.SilentLogger, false)).ToList<Matcher>();

                var udf = await DigitalTwinsHelper.GetUserDefinedFunction(udfId, _cache, Loggers.SilentLogger, false);

                // we filter to remove all the matchers already associated with the UDF
                IEnumerable<Matcher> mtcs =
                    from mtc in matchers
                    where !udf.Matchers.Any(m => m.Name.Equals(mtc.Name))
                    select mtc;

                return View(mtcs);
            }
            else
            {
                //TODO replace with default view (List)
                return RedirectToAction(nameof(PropertyKeyController.Create));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(List<Matcher> model, string updateButton)
        {
            if (updateButton.Equals("Cancel"))
            {
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }

            try
            {
                UserDefinedFunction udf = await DigitalTwinsHelper.GetUserDefinedFunction(CacheHelper.GetObjectId(_cache), _cache, Loggers.SilentLogger, true);

                if (udf.Matchers == null)
                {
                    udf.Matchers = new System.Collections.ObjectModel.ObservableCollection<Matcher>();
                }

                foreach (Matcher matcher in model)
                {
                    if (matcher.Add)
                    {
                        udf.Matchers.Add(matcher);
                    }
                }

                await DigitalTwinsHelper.UpdateUserDefinedFunctionAsync(_cache, Loggers.SilentLogger, udf);
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                return View();
            }
        }
    }
}