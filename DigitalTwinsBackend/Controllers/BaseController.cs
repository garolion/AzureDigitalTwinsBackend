using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Models;
using DigitalTwinsBackend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace DigitalTwinsBackend.Controllers
{
    public class BaseController : Controller
    {
        internal IHttpContextAccessor _httpContextAccessor;
        internal IMemoryCache _cache;

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            SendViewData();
            base.OnActionExecuted(context);
        }

        public void Reset()
        {
            CacheHelper.ResetMessagesInCache(_cache);
        }

        public void SendViewData()
        {
            ViewData["InfoMessages"] = CacheHelper.GetInfoMessagesFromCache(_cache);
            ViewData["APICallMessages"] = CacheHelper.GetAPICallMessagesFromCache(_cache);
        }
    }
}