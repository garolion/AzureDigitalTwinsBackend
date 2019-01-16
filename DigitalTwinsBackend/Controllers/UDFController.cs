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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IMemoryCache _cache;

        public UDFController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }


        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Details(Guid id)
        {
            var viewModel = new UDFViewModel();

            var udf = await DigitalTwinsHelper.GetUserDefinedFunction(id, _cache, Loggers.SilentLogger);

            if (udf != null)
            {
                viewModel.UDF = udf;
                viewModel.Content = await DigitalTwinsHelper.GetUserDefinedFunctionContent(udf.Id, _cache, Loggers.SilentLogger);
                //We get the matcher list to add the related conditions

                List<Matcher> matchersWithConditions = new List<Matcher>();
                var matchers = await DigitalTwinsHelper.GetMatchersBySpaceId(udf.SpaceId, _cache, Loggers.SilentLogger);
                foreach(Matcher matcher in viewModel.UDF.Matchers)
                {
                    matchersWithConditions.Add(matchers.First(t => t.Id == matcher.Id));
                }

                viewModel.UDF.Matchers = matchersWithConditions;
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Details(UDFViewModel model, string updateButton)
        {
            try
            {
                if (updateButton.Equals("Update Content"))
                {
                    var udf = await DigitalTwinsHelper.GetUserDefinedFunction(model.UDF.Id, _cache, Loggers.SilentLogger);

                    await DigitalTwinsHelper.UpdateUserDefinedFunction(udf, model.Content, _cache, Loggers.SilentLogger);
                    return await Details(model.UDF.Id);
                }

                return View();
            }
            catch
            {
                return View();
            }
        }
    }
}