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

        // GET: UDF/Details/5
        public async Task<ActionResult> Details(Guid id)
        {
            var viewModel = new UDFViewModel();

            var udf = await DigitalTwinsHelper.GetUserDefinedFunctionsByFunctionId(id, _cache, Loggers.SilentLogger);

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

        // GET: UDF/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UDF/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UDF/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UDF/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UDF/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UDF/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}