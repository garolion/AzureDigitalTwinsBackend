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
        public static async Task<bool> UpdateAsync<T>(IMemoryCache memoryCache, ILogger logger, T element) where T : BaseModel
        {
            bool updateWasASuccess = true;
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            logger.LogInformation($"Updating {typeof(T).Name} with Id: {element.Id}");
            BaseModel updatedElement = null;
            var content = JsonConvert.SerializeObject(element.ToUpdate(memoryCache, out updatedElement));

            if (updatedElement != null) CacheHelper.AddInCache(
                memoryCache, 
                updatedElement, 
                updatedElement.Id, (Context)Enum.Parse(typeof(Context), typeof(T).Name));

            if (content.Length > 2)
            {
                var response = await httpClient.PatchAsync($"{typeof(T).Name.ToLower()}s/{element.Id}", 
                    new StringContent(content, Encoding.UTF8, "application/json"));
                updateWasASuccess = await IsSuccessCall(response, logger);
            }

            if (updateWasASuccess & element.PropertiesHasChanged)
            {
                return await UpdatePropertiesAsync(memoryCache, logger, updatedElement);
            }
            return false;
        }

        public static async Task<bool> UpdatePropertiesAsync<T>(IMemoryCache memoryCache, ILogger logger, T element) where T : BaseModel
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            logger.LogInformation($"Adding properties for {typeof(T).Name} with Id: {element.Id}");
            var content = JsonConvert.SerializeObject(element.Properties);
            var response = await httpClient.PutAsync(
                $"{typeof(T).Name.ToLower()}s/{element.Id}/properties", 
                new StringContent(content, Encoding.UTF8, "application/json"));

            return await IsSuccessCall(response, logger);
        }

        public static async Task<bool> UpdateUserDefinedFunctionAsync(
            IMemoryCache memoryCache,
            ILogger logger,
            Models.UserDefinedFunction userDefinedFunction,
            string js)
        {
            Dictionary<string, object> updates = new Dictionary<string, object>();
            BaseModel updatedUDF = null;
            updates = userDefinedFunction.ToUpdate(memoryCache, out updatedUDF);
            logger.LogInformation($"Updating UserDefinedFunction with Metadata: {JsonConvert.SerializeObject(updates, Formatting.Indented)}");

            var displayContent = js.Length > 100 ? js.Substring(0, 100) + "..." : js;
            logger.LogInformation($"Updating UserDefinedFunction with Content: {displayContent}");

            var metadataContent = new StringContent(JsonConvert.SerializeObject(updates), Encoding.UTF8, "application/json");
            metadataContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            var multipartContent = new MultipartFormDataContent("userDefinedFunctionBoundary");
            multipartContent.Add(metadataContent, "metadata");
            multipartContent.Add(new StringContent(js), "contents");

            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var response = await httpClient.PatchAsync($"userdefinedfunctions/{userDefinedFunction.Id}", multipartContent);

            return await IsSuccessCall(response, logger);
        }
    }
}