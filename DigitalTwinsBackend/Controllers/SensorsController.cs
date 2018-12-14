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
    public class SensorsController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private SensorViewModel _model;
        private IMemoryCache _cache;

        public SensorsController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;

            _model = new SensorViewModel(_cache);
        }

        // GET: Sensors
        public ActionResult Index()
        {
            return View();
        }

        // GET: Sensors/Details/5
        public ActionResult Details(Guid id)
        {
            _model = new SensorViewModel(_cache, id);

            return View(_model);
        }

        // GET: Sensors/Create
        public ActionResult Create(Guid deviceId)
        {
            _model = new SensorViewModel(_cache);

            if (deviceId != Guid.Empty)
            {
                _model.SelectedSensor = new Sensor();
                _model.SelectedSensor.DeviceId = deviceId;
                CacheHelper.SetContext(_cache, Context.Device);
            }
            else
            {
                CacheHelper.SetContext(_cache, Context.None);
            }

            return View(_model);
        }

        // POST: Sensors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SensorViewModel model)
        {
            //SensorCreate sensorCreate = new SensorCreate(ExtractSensorFromModel(model, true));

            try
            {
                var id = await DigitalTwinsHelper.CreateSensorAsync(ExtractSensorFromModel(model, true), _cache, Loggers.SilentLogger);
                await FeedbackHelper.Channel.SendMessageAsync($"Sensor with id '{id}' successfully created.", MessageType.Info);

                if (CacheHelper.IsInDeviceEditMode(_cache))
                {
                    CacheHelper.SetContext(_cache, Context.None);
                    return RedirectToAction("Edit", "Device", new { id = model.SelectedSensor.DeviceId });
                }
                else
                {
                    return RedirectToAction(nameof(DeviceController.Index));
                }

            }
            catch (Exception ex)
            {
                await FeedbackHelper.Channel.SendMessageAsync(ex.Message, MessageType.Info);
                return View();
            }
        }

        public ActionResult Edit(Guid id)
        {
            _model = new SensorViewModel(_cache, id);
            CacheHelper.SetContext(_cache, Context.Device);

            return View(_model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SensorViewModel model)
        {
            var sensor = ExtractSensorFromModel(model, false);

            try
            {
                await DigitalTwinsHelper.UpdateSensorAsync(sensor, _cache, Loggers.SilentLogger);

                if (CacheHelper.IsInDeviceEditMode(_cache))
                {
                    CacheHelper.SetContext(_cache, Context.None);
                    return RedirectToAction("Edit", "Device", new { id = model.SelectedSensor.DeviceId });
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

        // GET: Sensors/Delete/5
        public ActionResult Delete(Guid id)
        {
            _model = new SensorViewModel(_cache, id);
            CacheHelper.SetContext(_cache, Context.Device);

            return View(_model);
        }

        // POST: Sensors/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(SensorViewModel model)
        {
            try
            {
                await DigitalTwinsHelper.DeleteSensorAsync(model.SelectedSensor, _cache, Loggers.SilentLogger);

                if (CacheHelper.IsInDeviceEditMode(_cache))
                {
                    CacheHelper.SetContext(_cache, Context.None);
                    return RedirectToAction("Edit", "Device", new { id = model.SelectedSensor.DeviceId });
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

        private Sensor ExtractSensorFromModel(SensorViewModel model, bool isInCreate)
        {
            Sensor sensor = model.SelectedSensor;

            if (!isInCreate)
            {
                sensor.DataUnitTypeId = _model.DataUnitTypeList.Single(t => t.Name == sensor.DataUnitType).Id;
                sensor.DataSubTypeId = _model.DataSubTypeList.Single(t => t.Name == sensor.DataSubtype).Id;
            }

            return sensor;
        }
    }
}