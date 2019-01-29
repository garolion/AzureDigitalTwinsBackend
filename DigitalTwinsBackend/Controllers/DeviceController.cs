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
        List<UISpace> spaces;

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

        [HttpGet]
        public async Task<ActionResult> List()
        {
            CacheHelper.SetPreviousPage(_cache, Request.Headers["Referer"].ToString());

            spaces = new List<UISpace>
            {
                new UISpace() { Space = new Space() { Name = "Root", Id = Guid.Empty }, MarginLeft = "0" }
            };

            var devices = await DigitalTwinsHelper.GetDevicesAsync(_cache, Loggers.SilentLogger);
            foreach(var device in devices)
            {
                await MergeTree(device.Id, device.SpacesHierarchy, 0);
            }

            return View(spaces.Skip(1));
        }

        private async Task MergeTree(Guid deviceId, IEnumerable<Guid> spacePath, int level)
        {
            level++;
            Guid spaceId = spacePath.First();
            Space space = await DigitalTwinsHelper.GetSpaceAsync(spaceId, _cache, Loggers.SilentLogger);

            if (!spaces.Exists(s => s.Space.Id == space.Id))
            {
                int index = spaces.FindIndex(s => s.Space.Id == space.ParentSpaceId);
                spaces.Insert(index + 1, new UISpace() { Space = space, MarginLeft = $"{25 * level-1}px" });
            }

            if (spacePath.Count() > 1)
            {
                await MergeTree(deviceId, spacePath.Skip(1), level);
            }
        }
    }
}