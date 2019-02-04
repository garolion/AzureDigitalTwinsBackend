// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DigitalTwinsBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DigitalTwinsBackend.Helpers
{
    public partial class Api
    {
        public static async Task<bool> UpdateAsync<T>(IMemoryCache memoryCache, ILogger logger, T element, bool refreshInCache = true) where T : BaseModel
        {
            bool updateWasASuccess = true;
            BaseModel updatedElement = null;
            var updates = element.ToUpdate(memoryCache, out updatedElement);

            if (updates != null && updates.Count > 0)
            {
                logger.LogInformation($"Updating {typeof(T).Name} with Id: {updatedElement.Id}");

                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
                var content = JsonConvert.SerializeObject(updates);

                if (refreshInCache)
                {
                    CacheHelper.AddInCache(memoryCache, updatedElement, updatedElement.Id, (Context)Enum.Parse(typeof(Context), typeof(T).Name));
                }

                var response = await httpClient.PatchAsync(
                    $"{typeof(T).Name.ToLower()}s/{element.Id}",
                    new StringContent(content, Encoding.UTF8, "application/json"));
                updateWasASuccess = await IsSuccessCall(response, logger);

                if (updateWasASuccess & element.PropertiesHasChanged)
                {
                    await FeedbackHelper.Channel.SendMessageAsync($"{element.GetType().Name} '{element.Label}' successfully updated", MessageType.Info);
                    return await UpdatePropertiesAsync(memoryCache, logger, updatedElement);
                }
            }
            return updateWasASuccess;
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

        public static async Task<bool> UpdateBlobAsync(HttpClient httpClient, ILogger logger, ParentType blobType, BlobContent blobContent, IFormFile file)
        {
            var metadataContent = new StringContent(JsonConvert.SerializeObject(blobContent.ToCreate()), Encoding.UTF8, "application/json");
            metadataContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            var reader = new StreamReader(file.OpenReadStream());
            var subContent = new StreamContent(reader.BaseStream);
            subContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);

            var multipartContent = new MultipartFormDataContent("blobBoundary")
                {
                    { metadataContent, "metadata" },
                    { subContent, "blob" }
                };

            var response = await httpClient.PatchAsync($"{blobType}s/blobs/{blobContent.Id}", multipartContent);
            return await IsSuccessCall(response, logger);
        }
    }
}