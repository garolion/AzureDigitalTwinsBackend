using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Models;
using DigitalTwinsBackend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DigitalTwinsBackend.Controllers
{
    public class UDFController : BaseController
    {
        List<UISpace> spaces;

        public UDFController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }


        [HttpGet]
        public async Task<ActionResult> Details(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            var udf = await DigitalTwinsHelper.GetUserDefinedFunction(id, _cache, Loggers.SilentLogger, false);
            var viewModel = new UDFViewModel(_cache, udf);
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Create(Guid spaceId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            //var udf = await DigitalTwinsHelper.GetUserDefinedFunction(spaceId, _cache, Loggers.SilentLogger);
            var viewModel = new UDFViewModel(_cache);
            viewModel.UDF.SpaceId = spaceId;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UDFViewModel model, string updateButton)
        {
            if (updateButton.Equals("Cancel"))
            {
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }

            try
            {
                string js;

                if (model.UDFFile != null)
                {
                    using (var r = new StreamReader(model.UDFFile.OpenReadStream()))
                    {
                        js = await r.ReadToEndAsync();
                        if (String.IsNullOrWhiteSpace(js))
                        {
                            await FeedbackHelper.Channel.SendMessageAsync($"Error - We cannot read the content of the file {model.UDFFile.FileName}.", MessageType.Info);
                        }
                    }
                }
                else if (model.Content != null)
                {
                    js = model.Content;
                }
                else
                {
                    js = string.Empty;
                }

                var id = await DigitalTwinsHelper.CreateUserDefinedFunctionAsync(_cache, Loggers.SilentLogger, model.UDF, js);

                return RedirectToAction(nameof(Edit), new { id = id });
            }
            catch
            {
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            var udf = await DigitalTwinsHelper.GetUserDefinedFunction(id, _cache, Loggers.SilentLogger,false);
            var viewModel = new UDFViewModel(_cache, udf);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UDFViewModel model, string updateButton)
        {
            if (updateButton.Equals("Cancel"))
            {
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }

            try
            {
                //we retrieve matchers from cache
                var cachedUDF = CacheHelper.GetUDFFromCache(_cache, model.UDF.Id);
                model.UDF.Matchers = cachedUDF.Matchers;

                await DigitalTwinsHelper.UpdateUserDefinedFunctionAsync(_cache, Loggers.SilentLogger, model.UDF, model.Content);
                return RedirectToAction("List", "UDF");
                //return RedirectToAction("Edit", "Space", new { id = model.UDF.SpaceId });
            }
            catch
            {
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
        }
        
        [HttpGet]
        public async Task<ActionResult> Delete(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            var udf = await DigitalTwinsHelper.GetUserDefinedFunction(id, _cache, Loggers.SilentLogger, false);
            var viewModel = new UDFViewModel(_cache, udf);
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(UDFViewModel model, string updateButton)
        {
            if (updateButton.Equals("Cancel"))
            {
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }

            try
            {
                await DigitalTwinsHelper.DeleteUserDefinedFunctionAsync(model.UDF, _cache, Loggers.SilentLogger);
                return RedirectToAction("List", "UDF");
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            spaces = new List<UISpace>
            {
                new UISpace() { Space = new Space() { Name = "Root", Id = Guid.Empty }, MarginLeft = "0" }
            };

            var udfs = await DigitalTwinsHelper.GetUserDefinedFunctions(_cache, Loggers.SilentLogger);
            foreach (var udf in udfs)
            {
                await MergeTree(udf, udf.SpacesHierarchy, 0);
            }

            return View(spaces.Skip(1));
        }

        private async Task MergeTree(UserDefinedFunction udf, IEnumerable<Guid> spacePath, int level)
        {
            level++;
            Guid spaceId = spacePath.First();
            Space space = await DigitalTwinsHelper.GetSpaceAsync(spaceId, _cache, Loggers.SilentLogger);

            if (spacePath.Count() == 1)
            {
                if (!space.UDFs.Exists(u => u.Id == udf.Id))
                {
                    space.UDFs.Add(udf);
                }
            }

            if (!spaces.Exists(s => s.Space.Id == space.Id))
            {
                int index = spaces.FindIndex(s => s.Space.Id == space.ParentSpaceId);
                spaces.Insert(index + 1, new UISpace() { Space = space, MarginLeft = $"{25 * level - 1}px" });
            }
            else
            {
                spaces.First(s => s.Space.Id == space.Id).Space = space;
            }

            if (spacePath.Count() > 1)
            {
                await MergeTree(udf, spacePath.Skip(1), level);
            }
        }
    }
}