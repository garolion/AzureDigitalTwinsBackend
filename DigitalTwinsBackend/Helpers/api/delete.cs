// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DigitalTwinsBackend.Helpers
{
    public partial class Api
    {
        public static async Task<bool> DeleteAsync<T>(HttpClient httpClient, ILogger logger, T element) where T : BaseModel
        {
            logger.LogInformation($"Deleting {typeof(T).Name} with Id: {element.Id}");
            var response = await httpClient.DeleteAsync($"{typeof(T).Name.ToLower()}s/{element.Id}");

            return await IsSuccessCall(response, logger);
        }

        //public static async Task<bool> DeleteSpaceAsync(
        //    HttpClient httpClient,
        //    ILogger logger,
        //    Models.Space space)
        //{
        //    logger.LogInformation($"Deleting Space with Id: {space.Id}");
        //    var response = await httpClient.DeleteAsync($"spaces/{space.Id}");

        //    return await IsSuccessCall(response, logger);
        //}

        //public static async Task<bool> DeleteDeviceAsync(
        //    HttpClient httpClient,
        //    ILogger logger,
        //    Models.Device device)
        //{
        //    logger.LogInformation($"Deleting Device with Id: {device.Id}");
        //    var response = await httpClient.DeleteAsync($"devices/{device.Id}");

        //    return await IsSuccessCall(response, logger);
        //}

        //public static async Task<bool> DeleteSensorAsync(
        //    HttpClient httpClient,
        //    ILogger logger,
        //    Models.Sensor sensor)
        //{
        //    logger.LogInformation($"Deleting Sensor with Id: {sensor.Id}");
        //    var response = await httpClient.DeleteAsync($"sensors/{sensor.Id}");

        //    return await IsSuccessCall(response, logger);
        //}
    }
}