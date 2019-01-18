using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DigitalTwinsBackend.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using DigitalTwinsBackend.Models;
using DigitalTwinsBackend.Helpers;

namespace DigitalTwinsBackend.Controllers
{
    public class SensorController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IMemoryCache _cache;

        public SensorController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;

            SensorViewModel model = new SensorViewModel(_cache);
        }

        [HttpGet]
        public ActionResult Details(Guid id)
        {
            SensorViewModel model = new SensorViewModel(_cache, id);
            return View(model);
        }

        [HttpGet]
        public ActionResult Create(Guid deviceId)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            SensorViewModel model = new SensorViewModel(_cache);
            if (deviceId != Guid.Empty)
            {
                model.SelectedSensor = new Sensor() { DeviceId = deviceId };
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SensorViewModel model)
        {
            try
            {
                var id = await DigitalTwinsHelper.CreateSensorAsync(model.SelectedSensor, _cache, Loggers.SilentLogger);
                await FeedbackHelper.Channel.SendMessageAsync($"Sensor with id '{id}' successfully created.", MessageType.Info);

                return Redirect(CacheHelper.GetPreviousPage(_cache));

            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                return View();
            }
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());
            SensorViewModel model = new SensorViewModel(_cache, id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SensorViewModel model)
        {
            try
            {
                await DigitalTwinsHelper.UpdateSensorAsync(model.SelectedSensor, _cache, Loggers.SilentLogger);

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
            SensorViewModel model = new SensorViewModel(_cache, id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(SensorViewModel model)
        {
            try
            {
                await DigitalTwinsHelper.DeleteSensorAsync(model.SelectedSensor, _cache, Loggers.SilentLogger);

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