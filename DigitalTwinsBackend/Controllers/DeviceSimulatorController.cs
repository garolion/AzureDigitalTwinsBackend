using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using DigitalTwinsBackend.ViewModels;
using System.Threading;
using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using DigitalTwinsBackend.Models;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;
using System.Globalization;

namespace DigitalTwinsBackend.Controllers
{
    public class DeviceSimulatorController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IMemoryCache _cache;

        public DeviceSimulatorController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }


        public ActionResult Index()
        {
            // reset sensor list in Cache
            //CacheHelper.AddSimulatedSensorListInCacheAsync(_cache, new List<SimulatedSensor>()).Wait();

            var simulator = new SimulatorViewModel(_cache);

            return View(simulator);

        }

        public ActionResult AddSensor()
        {
            return View();
        }

        public ActionResult Edit(string id)
        {
            var simulator = new SimulatorViewModel(_cache);
            simulator.SelectedSensor = id;

            return View(simulator);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SimulatorAction(SimulatorViewModel model, string action)
        {
            if (action.Equals("Launch") && !CacheHelper.IsInSendingDataState(_cache))
            {
                if (model.DeviceConnectionString.Length == 0)
                {
                    await FeedbackHelper.Channel.SendMessageAsync("No connection string added to connect to Azure IoT Hub");
                    return RedirectToAction(nameof(Index));
                }

                var list = await CacheHelper.GetSimulatedSensorListFromCacheAsync(_cache);

                if (list != null && list.Count > 0)
                {
                    CacheHelper.SetInSendingDataState(_cache, true);

                    try
                    {
                        await SendDataAsync(model.DeviceConnectionString);
                    }
                    catch (Exception)
                    {
                        await FeedbackHelper.Channel.SendMessageAsync("Error during sending");
                    }

                    CacheHelper.SetInSendingDataState(_cache, false);
                }
                else
                {
                    await FeedbackHelper.Channel.SendMessageAsync("No sensor defined to send data");
                }
            }
            else if (action.Equals("Cancel"))
            {
                await FeedbackHelper.Channel.SendMessageAsync("Stopping sending data...");

                CacheHelper.SetInSendingDataState(_cache, false);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddSensor(SimulatedSensor item)
        {
            if (item.HardwareId == null) item.HardwareId = Guid.NewGuid().ToString();

            if (item.DataType == DataTypeEnum.Motion)
            {
                item.InitialValue = true.ToString();
            }
            else if (item.InitialValue == null) item.InitialValue = ((item.MaxValue - item.MinValue) / 2).ToString();

            var list = await CacheHelper.GetSimulatedSensorListFromCacheAsync(_cache);
            list.Add(item);
            await CacheHelper.AddSimulatedSensorListInCacheAsync(_cache, list);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SimulatorViewModel model, string action)
        {
            var list = await CacheHelper.GetSimulatedSensorListFromCacheAsync(_cache);
            var item = list.First(t => t.HardwareId.Equals(model.SelectedSensor));
            var index = list.IndexOf(item);
            list.Remove(item);

            if (action.Equals("Save"))
            {
                list.Insert(index, model.SensorInEdit);
            }

            await CacheHelper.AddSimulatedSensorListInCacheAsync(_cache, list);

            return RedirectToAction(nameof(Index));
        }

        async Task SendDataAsync(string connectionString)
        {
            var list = await CacheHelper.GetSimulatedSensorListFromCacheAsync(_cache);
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(connectionString);

            if (deviceClient == null)
            {
                await FeedbackHelper.Channel.SendMessageAsync("Failed to connect to Azure IoT Hub.");
                return;
            }

            while (CacheHelper.IsInSendingDataState(_cache))
            {
                await FeedbackHelper.Channel.SendMessageAsync("Sending Data...");

                foreach (SimulatedSensor sensor in list)
                {
                    await IoTHubHelper.SendEvent(deviceClient, sensor);
                }

                await Task.Delay(TimeSpan.FromSeconds(ConfigHelper.Config.parameters.SimulatorTimer));
            }

            await FeedbackHelper.Channel.SendMessageAsync("Sending data stopped.");
        }
    }
}