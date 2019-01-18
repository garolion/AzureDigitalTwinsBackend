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
    public class DeviceController : BaseController
    {
        public DeviceController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }

        [HttpGet]
        public ActionResult Details(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            DeviceViewModel model = new DeviceViewModel(_cache, id);
            return View(model);
        }

        [HttpGet]
        public ActionResult Create(Guid spaceId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            DeviceViewModel model = new DeviceViewModel(_cache);
            model.SelectedDeviceItem = new Device();
            model.SelectedDeviceItem.SpaceId = spaceId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DeviceViewModel model)
        {
            CacheHelper.ResetMessagesInCache(_cache);

            try
            {
                var id = await DigitalTwinsHelper.CreateDeviceAsync(model.SelectedDeviceItem, _cache, Loggers.SilentLogger);
                await FeedbackHelper.Channel.SendMessageAsync($"Device with id '{id}' successfully created.", MessageType.Info);

                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                return Create(model.SelectedDeviceItem.SpaceId);
            }
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            DeviceViewModel mmodel = new DeviceViewModel(_cache, id);
            return View(mmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DeviceViewModel model)
        {
            try
            {
                await DigitalTwinsHelper.UpdateDeviceAsync(model.SelectedDeviceItem, _cache, Loggers.SilentLogger);
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.InnerException.ToString(), MessageType.Info);
                return View();
            }
        }

        [HttpGet]
        public ActionResult Delete(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            DeviceViewModel mmodel = new DeviceViewModel(_cache, id);
            return View(mmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(DeviceViewModel model)
        {
            CacheHelper.ResetMessagesInCache(_cache);

            try
            {
                await DigitalTwinsHelper.DeleteDeviceAsync(model.SelectedDeviceItem, _cache, Loggers.SilentLogger);
                return Redirect(CacheHelper.GetPreviousPage(_cache));
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.InnerException.ToString(), MessageType.Info);
                return View();
            }
        }
    }
}