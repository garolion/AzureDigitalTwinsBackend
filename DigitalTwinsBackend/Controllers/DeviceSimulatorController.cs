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
    public class DeviceSimulatorController : BaseController
    {
        //private readonly IHttpContextAccessor _httpContextAccessor;
        //private IMemoryCache _cache;

        public DeviceSimulatorController(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }


        public ActionResult Index()
        {
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
            if (model.SelectedDevice == Guid.Empty)
            {
                await FeedbackHelper.Channel.SendMessageAsync("No connection string added to connect to Azure IoT Hub", MessageType.Info);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var device = await DigitalTwinsHelper.GetDeviceAsync(model.SelectedDevice, _cache, Loggers.SilentLogger, false);

                try
                {
                    if (action.Equals("Launch") && !CacheHelper.IsInSendingDataState(_cache))
                    {


                        var list = await CacheHelper.GetSimulatedSensorListFromCacheAsync(_cache);

                        if (list != null && list.Count > 0)
                        {
                            CacheHelper.SetInSendingDataState(_cache, true);

                            try
                            {
                                await SendDataAsync(device.ConnectionString);
                            }
                            catch (Exception)
                            {
                                await FeedbackHelper.Channel.SendMessageAsync("Error during sending", MessageType.Info);
                            }

                            CacheHelper.SetInSendingDataState(_cache, false);
                        }
                        else
                        {
                            await FeedbackHelper.Channel.SendMessageAsync("No sensor defined to send data", MessageType.Info);
                        }
                    }
                    else if (action.Equals("Stop"))
                    {
                        await FeedbackHelper.Channel.SendMessageAsync("Stopping sending data...", MessageType.Info);

                        CacheHelper.SetInSendingDataState(_cache, false);
                    }
                }
                catch (Exception ex)
                {
                    FeedbackHelper.Channel.SendMessageAsync($"Error - {ex.Message}", MessageType.Info).Wait();
                }

                return RedirectToAction(nameof(Index));
            }
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
                await FeedbackHelper.Channel.SendMessageAsync("Failed to connect to Azure IoT Hub.", MessageType.Info);
                return;
            }

            while (CacheHelper.IsInSendingDataState(_cache))
            {
                await FeedbackHelper.Channel.SendMessageAsync("Sending Data...", MessageType.Info);

                foreach (SimulatedSensor sensor in list)
                {
                    await IoTHubHelper.SendEvent(deviceClient, sensor);
                }

                await Task.Delay(TimeSpan.FromSeconds(ConfigHelper.Config.parameters.SimulatorTimer));
            }

            await FeedbackHelper.Channel.SendMessageAsync("Sending data stopped.", MessageType.Info);
        }
    }
}