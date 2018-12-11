using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalTwinsBackend.Models;
using DigitalTwinsBackend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DigitalTwinsBackend.Controllers
{
    public class OntologyController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private OntologyViewModel _model;
        private IMemoryCache _cache;

        public OntologyController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }

        public ActionResult List()
        {
            _model = new OntologyViewModel(_cache, null);
            return View(_model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult List(string filterButton)
        {
            object button;
            if (Enum.TryParse(typeof(SystemTypes), filterButton, out button))
            {
                SystemTypes types = (SystemTypes)button;
                _model = new OntologyViewModel(_cache, types);
                return View(_model);
            }

            _model = new OntologyViewModel(_cache, null);
            return View(_model);
        }
    }
}