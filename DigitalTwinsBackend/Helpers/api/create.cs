// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DigitalTwinsBackend.Models;
using DigitalTwinsBackend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DigitalTwinsBackend.Helpers
{
    public partial class Api
    {
        public static async Task<Guid> CreateAsync<T>(HttpClient httpClient, ILogger logger, T element) where T : BaseModel
        {
            var fields = element.ToCreate();

            logger.LogInformation($"Creating {element.GetType().Name}: {JsonConvert.SerializeObject(fields, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(fields);
            string domain = element.GetType().Name.ToLower() + "s";

            var response = await httpClient.PostAsync(domain, new StringContent(content, Encoding.UTF8, "application/json"));

            var id = await GetIdFromResponse(response, logger, content);
            if (id != Guid.Empty)
                await FeedbackHelper.Channel.SendMessageAsync($"{element.GetType().Name} '{element.Label}' successfully created with the ID {id}", MessageType.Info);
            return id;
        }
                          
        public static async Task<Guid> CreateUserDefinedFunctionAsync(HttpClient httpClient, ILogger logger, UserDefinedFunction userDefinedFunction, string js)
        {
            logger.LogInformation($"Creating UserDefinedFunction with Metadata: {JsonConvert.SerializeObject(userDefinedFunction.ToCreate(), Formatting.Indented)}");
            var displayContent = js.Length > 100 ? js.Substring(0, 100) + "..." : js;
            logger.LogInformation($"Creating UserDefinedFunction with Content: {displayContent}");

            var metadataContent = new StringContent(JsonConvert.SerializeObject(userDefinedFunction.ToCreate()), Encoding.UTF8, "application/json");
            metadataContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            var multipartContent = new MultipartFormDataContent("userDefinedFunctionBoundary");
            multipartContent.Add(metadataContent, "metadata");
            multipartContent.Add(new StringContent(js), "contents");

            var response = await httpClient.PostAsync("userdefinedfunctions", multipartContent);
            var id = await GetIdFromResponse(response, logger, multipartContent.ToString());
            if (id!= Guid.Empty)
                await FeedbackHelper.Channel.SendMessageAsync($"UserDefinedFunction '{userDefinedFunction.Name}' successfully created with the ID {id}", MessageType.Info);
            return id;
        }

        public static async Task<int> CreatePropertyKeyAsync(HttpClient httpClient, ILogger logger, PropertyKey propertyKey)
        {
            var fields = propertyKey.ToCreate();

            logger.LogInformation($"Creating PropertyKey: {JsonConvert.SerializeObject(fields, Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(fields);

            var response = await httpClient.PostAsync("propertykeys", new StringContent(content, Encoding.UTF8, "application/json"));
                        
            if (await IsSuccessCall(response, logger, content))
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!int.TryParse(responseContent, out var createdId))
                {
                    logger.LogError($"Returned value from POST did not parse into a int: {responseContent}");
                }

                await FeedbackHelper.Channel.SendMessageAsync($"PropertyKey '{propertyKey.Label}' successfully created with the ID {createdId}", MessageType.Info);
                return createdId;
            }
            
            return 0;
        }

        public static async Task<Guid> CreateBlobAsync(HttpClient httpClient, ILogger logger, ParentType blobType, BlobContent blobContent, IFormFile file)
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

            var response = await httpClient.PostAsync($"{blobType}s/blobs", multipartContent);
            var id = await GetIdFromResponse(response, logger, multipartContent.ToString());
            if (id != Guid.Empty)
                await FeedbackHelper.Channel.SendMessageAsync($"{blobType} blob successfully created with the ID {id}", MessageType.Info);
            return id;
        }

        private static async Task<Guid> GetIdFromResponse(HttpResponseMessage response, ILogger logger, string requestBody)
        {
            if (await IsSuccessCall(response, logger, requestBody))
            {
                var content = await response.Content.ReadAsStringAsync();

                if (!Guid.TryParse(content.Substring(1, content.Length - 2), out var createdId))
                {
                    logger.LogError($"Returned value from POST did not parse into a guid: {content}");
                }

                return createdId;
            }

            return Guid.Empty;
        }
    }
}