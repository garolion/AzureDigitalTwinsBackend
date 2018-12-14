// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DigitalTwinsBackend.Helpers
{
    public partial class Api
    {
        public static async Task<bool> UpdateUserDefinedFunction(
            IMemoryCache memoryCache,
            ILogger logger,
            Models.UserDefinedFunction userDefinedFunction,
            string js)
        {
            logger.LogInformation($"Updating UserDefinedFunction with Metadata: {JsonConvert.SerializeObject(userDefinedFunction.ToUpdate(memoryCache), Formatting.Indented)}");
            var displayContent = js.Length > 100 ? js.Substring(0, 100) + "..." : js;
            logger.LogInformation($"Updating UserDefinedFunction with Content: {displayContent}");

            var metadataContent = new StringContent(JsonConvert.SerializeObject(userDefinedFunction.ToUpdate(memoryCache)), Encoding.UTF8, "application/json");
            metadataContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            var multipartContent = new MultipartFormDataContent("userDefinedFunctionBoundary");
            multipartContent.Add(metadataContent, "metadata");
            multipartContent.Add(new StringContent(js), "contents");

            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var response = await httpClient.PatchAsync($"userdefinedfunctions/{userDefinedFunction.Id}", multipartContent);

            return await IsSuccessCall(response, logger);
        }

        public static async Task<bool> UpdateAsync<T>(IMemoryCache memoryCache, ILogger logger, T element) where T : BaseModel
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            logger.LogInformation($"Updating {typeof(T).Name} with Id: {element.Id}");
            var content = JsonConvert.SerializeObject(element.ToUpdate(memoryCache));
            var response = await httpClient.PatchAsync($"{typeof(T).Name.ToLower()}s/{element.Id}", new StringContent(content, Encoding.UTF8, "application/json"));

            return await IsSuccessCall(response, logger);
        }
               

        //public static async Task<bool> UpdateSpaceAsync(
        //    IMemoryCache memoryCache,
        //    ILogger logger,
        //    Models.Space space)
        //{
        //    var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

        //    logger.LogInformation($"Updating Space with Id: {space.Id}");
        //    var content = JsonConvert.SerializeObject(space.ToUpdate(memoryCache));
        //    var response = await httpClient.PatchAsync($"spaces/{space.Id}", new StringContent(content, Encoding.UTF8, "application/json"));

        //    return await IsSuccessCall(response, logger);
        //}

        //public static async Task<bool> UpdateDeviceAsync(
        //    IMemoryCache memoryCache,
        //    ILogger logger,
        //    Models.Device device)
        //{
        //    var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

        //    logger.LogInformation($"Updating Device with Id: {device.Id}");
        //    var content = JsonConvert.SerializeObject(device);
        //    var response = await httpClient.PatchAsync($"devices/{device.Id}", new StringContent(content, Encoding.UTF8, "application/json"));

        //    return await IsSuccessCall(response, logger);
        //}

        //public static async Task<bool> UpdateSensorAsync(
        //    IMemoryCache memoryCache,
        //    ILogger logger,
        //    Models.Sensor sensor)
        //{
        //    var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

        //    logger.LogInformation($"Updating Sensor with Id: {sensor.Id}");
        //    var content = JsonConvert.SerializeObject(sensor);
        //    var response = await httpClient.PatchAsync($"sensors/{sensor.Id}", new StringContent(content, Encoding.UTF8, "application/json"));

        //    return await IsSuccessCall(response, logger);
        //}
    }
}