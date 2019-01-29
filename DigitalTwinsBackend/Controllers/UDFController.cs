using System;
using System.Collections.Generic;
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
    public class UDFController : Controller
    {
        List<UISpace> spaces;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private IMemoryCache _cache;

        public UDFController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }


        [HttpGet]
        public async Task<ActionResult> Details(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            var udf = await DigitalTwinsHelper.GetUserDefinedFunction(id, _cache, Loggers.SilentLogger);
            var viewModel = new UDFViewModel(_cache, udf);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            var udf = await DigitalTwinsHelper.GetUserDefinedFunction(id, _cache, Loggers.SilentLogger);
            var viewModel = new UDFViewModel(_cache, udf);
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UDFViewModel model, string updateButton)
        {
            try
            {
                if (updateButton.Equals("Cancel"))
                {
                    return Redirect(CacheHelper.GetPreviousPage(_cache));
                }

                //var udf = await DigitalTwinsHelper.GetUserDefinedFunction(model.UDF.Id, _cache, Loggers.SilentLogger);

                await DigitalTwinsHelper.UpdateUserDefinedFunction(model.UDF, model.Content, _cache, Loggers.SilentLogger);
                return Redirect(CacheHelper.GetPreviousPage(_cache));
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