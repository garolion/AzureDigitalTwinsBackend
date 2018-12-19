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
    public class DeviceController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IMemoryCache _cache;
        private DeviceViewModel _model;

        public DeviceController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;

            _model = new DeviceViewModel();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(Guid id)
        {
            _model = new DeviceViewModel(_cache, id);

            return View(_model);
        }

        public ActionResult Create(Guid spaceId)
        {
            _model = new DeviceViewModel(_cache);

            if (spaceId!=Guid.Empty)
            {
                _model.SelectedDeviceItem = new Device();
                _model.SelectedDeviceItem.SpaceId = spaceId;
                CacheHelper.SetContext(_cache, Context.Space);
            }
            else
            {
                CacheHelper.SetContext(_cache, Context.None);
            }

            return View(_model);
        }

        // POST: Device/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DeviceViewModel model)
        {
            CacheHelper.ResetMessagesInCache(_cache);

            try
            {
                var id = await DigitalTwinsHelper.CreateDeviceAsync(ExtractDeviceFromModel(model, true), _cache, Loggers.SilentLogger);
                await FeedbackHelper.Channel.SendMessageAsync($"Device with id '{id}' successfully created.", MessageType.Info);
                
                if (CacheHelper.IsInSpaceEditMode(_cache))
                {
                    CacheHelper.SetContext(_cache, Context.None);
                    return RedirectToAction("Edit", "Spaces", new { id = model.SelectedDeviceItem.SpaceId });
                }
                else
                {
                    return RedirectToAction(nameof(DeviceController.Index));
                }
                
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                //return View();
                return Create(model.SelectedDeviceItem.SpaceId);
            }
        }

        // GET: Device/Edit/5
        public ActionResult Edit(Guid id)
        {
            _model = new DeviceViewModel(_cache, id);
            CacheHelper.SetContext(_cache, Context.Space);

            return View(_model);
        }

        // POST: Device/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DeviceViewModel model)
        {
            CacheHelper.ResetMessagesInCache(_cache);
            var device = ExtractDeviceFromModel(model, false);

            try
            {
                await DigitalTwinsHelper.UpdateDeviceAsync(device, _cache, Loggers.SilentLogger);

                if (CacheHelper.IsInSpaceEditMode(_cache))
                {
                    CacheHelper.SetContext(_cache, Context.None);
                    return RedirectToAction("Edit", "Spaces", new { id = model.SelectedDeviceItem.SpaceId });
                }
                else
                {
                    return RedirectToAction(nameof(DeviceController.Index));
                }
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.InnerException.ToString(), MessageType.Info);
                return View();
            }
        }

        // GET: Device/Delete/5
        public ActionResult Delete(Guid id)
        {
            _model = new DeviceViewModel(_cache, id);
            CacheHelper.SetContext(_cache, Context.Space);

            return View(_model);
        }

        // POST: Device/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(DeviceViewModel model)
        {
            CacheHelper.ResetMessagesInCache(_cache);

            try
            {
                await DigitalTwinsHelper.DeleteDeviceAsync(model.SelectedDeviceItem, _cache, Loggers.SilentLogger);

                if (CacheHelper.IsInSpaceEditMode(_cache))
                {
                    CacheHelper.SetContext(_cache, Context.None);
                    return RedirectToAction("Edit", "Spaces", new { id = model.SelectedDeviceItem.SpaceId });
                }
                else
                {
                    return RedirectToAction(nameof(DeviceController.Index));
                }
            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.InnerException.ToString(), MessageType.Info);
                return View();
            }
        }

        private Device ExtractDeviceFromModel(DeviceViewModel model, bool isInCreate)
        {
            Device device = model.SelectedDeviceItem;

            if (!isInCreate)
            {
                device.TypeId = _model.DeviceTypeList.Single(t => t.Name == device.Type).Id;
                device.SubTypeId = _model.DeviceSubTypeList.Single(t => t.Name == device.SubType).Id;
            }

            return device;
        }
    }
}