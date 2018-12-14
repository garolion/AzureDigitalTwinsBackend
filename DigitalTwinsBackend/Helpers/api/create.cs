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
        public static async Task<Guid> CreateAsync<T>(HttpClient httpClient, ILogger logger, T element) where T : BaseModel
        {
            logger.LogInformation($"Creating Device: {JsonConvert.SerializeObject(element.ToCreate(), Formatting.Indented)}");
            var content = JsonConvert.SerializeObject(element.ToCreate());
            string domain = element.GetType().Name.ToLower() + "s";

            var response = await httpClient.PostAsync(domain, new StringContent(content, Encoding.UTF8, "application/json"));

            var id = await GetIdFromResponse(response, logger, content);
            if (id != Guid.Empty)
                await FeedbackHelper.Channel.SendMessageAsync($"{element.GetType().Name} '{element.Label}' successfully created with the ID {id}", MessageType.Info);
            return id;
        }
                          
        //public static async Task<Guid> CreateDeviceAsync(HttpClient httpClient, ILogger logger, Device device)
        //{
        //    logger.LogInformation($"Creating Device: {JsonConvert.SerializeObject(device.ToCreate(), Formatting.Indented)}");
        //    var content = JsonConvert.SerializeObject(device.ToCreate());
        //    var response = await httpClient.PostAsync("devices", new StringContent(content, Encoding.UTF8, "application/json"));

        //    var id = await GetIdFromResponse(response, logger, content);
        //    if (id != Guid.Empty)
        //        await FeedbackHelper.Channel.SendMessageAsync($"device '{device.Name}' successfully created with the ID {id}", MessageType.Info);
        //    return id;
        //}

        //public static async Task<Guid> CreateEndpointAsync(HttpClient httpClient, ILogger logger, Endpoint endpointCreate)
        //{
        //    logger.LogInformation($"Creating Endpoint: {JsonConvert.SerializeObject(endpointCreate, Formatting.Indented)}");
        //    var content = JsonConvert.SerializeObject(endpointCreate);
        //    var response = await httpClient.PostAsync("endpoints", new StringContent(content, Encoding.UTF8, "application/json"));

        //    var id = await GetIdFromResponse(response, logger, content);
        //    if (id != Guid.Empty)
        //        await FeedbackHelper.Channel.SendMessageAsync($"endpoint '{endpointCreate.Type}' successfully created with the ID {id}", MessageType.Info);
        //    return id;
        //}

        //public static async Task<Guid> CreateKeyStoreAsync(HttpClient httpClient, ILogger logger, KeyStore keyStoreCreate)
        //{
        //    logger.LogInformation($"Creating KeyStore: {JsonConvert.SerializeObject(keyStoreCreate, Formatting.Indented)}");
        //    var content = JsonConvert.SerializeObject(keyStoreCreate);
        //    var response = await httpClient.PostAsync("keystores", new StringContent(content, Encoding.UTF8, "application/json"));

        //    var id = await GetIdFromResponse(response, logger, content);
        //    if (id != Guid.Empty)
        //        await FeedbackHelper.Channel.SendMessageAsync($"keyStore '{keyStoreCreate.Name}' successfully created with the ID {id}", MessageType.Info);
        //    return id;
        //}

        //public static async Task<Guid> CreateMatcherAsync(HttpClient httpClient, ILogger logger, Matcher matcher)
        //{
        //    logger.LogInformation($"Creating Matcher: {JsonConvert.SerializeObject(matcher.ToCreate(), Formatting.Indented)}");
        //    var content = JsonConvert.SerializeObject(matcher.ToCreate());
        //    var response = await httpClient.PostAsync("matchers", new StringContent(content, Encoding.UTF8, "application/json"));

        //    var id = await GetIdFromResponse(response, logger, content);
        //    if (id != Guid.Empty)
        //        await FeedbackHelper.Channel.SendMessageAsync($"matcher '{matcher.Name}' successfully created with the ID {id}", MessageType.Info);
        //    return id;
        //}

        //public static async Task<Guid> CreateResourceAsync(HttpClient httpClient, ILogger logger, Resource resource)
        //{
        //    logger.LogInformation($"Creating Resource: {JsonConvert.SerializeObject(resource.ToCreate(), Formatting.Indented)}");
        //    var content = JsonConvert.SerializeObject(resource.ToCreate());
        //    var response = await httpClient.PostAsync("resources", new StringContent(content, Encoding.UTF8, "application/json"));

        //    var id = await GetIdFromResponse(response, logger, content);
        //    if (id != Guid.Empty)
        //        await FeedbackHelper.Channel.SendMessageAsync($"resource '{resource.Type}' successfully created with the ID {id}", MessageType.Info);
        //    return id;
        //}

        //public static async Task<Guid> CreateRoleAssignmentAsync(HttpClient httpClient, ILogger logger, RoleAssignment roleAssignment)
        //{
        //    logger.LogInformation($"Creating RoleAssignment: {JsonConvert.SerializeObject(roleAssignment.ToCreate(), Formatting.Indented)}");
        //    var content = JsonConvert.SerializeObject(roleAssignment.ToCreate());
        //    var response = await httpClient.PostAsync("roleassignments", new StringContent(content, Encoding.UTF8, "application/json"));

        //    var id = await GetIdFromResponse(response, logger, content);
        //    if (id != Guid.Empty)
        //        await FeedbackHelper.Channel.SendMessageAsync($"roleAssignment for RoleId '{roleAssignment.RoleId}' successfully created with the ID {id}", MessageType.Info);
        //    return id;
        //}

        //public static async Task<Guid> CreateSensorAsync(HttpClient httpClient, ILogger logger, Sensor sensor)
        //{
        //    logger.LogInformation($"Creating Sensor: {JsonConvert.SerializeObject(sensor.ToCreate(), Formatting.Indented)}");
        //    var content = JsonConvert.SerializeObject(sensor.ToCreate());
        //    var response = await httpClient.PostAsync("sensors", new StringContent(content, Encoding.UTF8, "application/json"));

        //    var id = await GetIdFromResponse(response, logger, content);
        //    if (id != Guid.Empty)
        //        await FeedbackHelper.Channel.SendMessageAsync($"Sensor '{sensor.HardwareId}' successfully created with the ID {id}", MessageType.Info);
        //    return id;
        //}

        //public static async Task<Guid> CreateSpaceAsync(HttpClient httpClient, ILogger logger, Space space)
        //{
        //    logger.LogInformation($"Creating Space: {JsonConvert.SerializeObject(space.ToCreate(), Formatting.Indented)}");
        //    var content = JsonConvert.SerializeObject(space.ToCreate());
        //    var response = await httpClient.PostAsync("spaces", new StringContent(content, Encoding.UTF8, "application/json"));

        //    var id = await GetIdFromResponse(response, logger, content);
        //    if (id != Guid.Empty)
        //        await FeedbackHelper.Channel.SendMessageAsync($"Space '{space.Name}' successfully created with the ID {id}", MessageType.Info);
        //    return id;
        //}

        //public static async Task<Guid> CreatePropertyAsync(HttpClient httpClient, ILogger logger, Guid spaceId, PropertyCreate propertyCreate)
        //{
        //    logger.LogInformation($"Creating Property: {JsonConvert.SerializeObject(propertyCreate, Formatting.Indented)}");
        //    var content = JsonConvert.SerializeObject(propertyCreate);
        //    var response = await httpClient.PostAsync($"spaces/{spaceId.ToString()}/properties", new StringContent(content, Encoding.UTF8, "application/json"));

        //    var id = await GetIdFromResponse(response, logger, content);
        //    if (id != Guid.Empty)
        //        await FeedbackHelper.Channel.SendMessageAsync($"Property '{propertyCreate.Name}' successfully created with the ID {id}", MessageType.Info);
        //    return id;

        //}

        //public static async Task<Guid> CreatePropertyKeyAsync(HttpClient httpClient, ILogger logger, PropertyKeyCreate propertyKeyCreate)
        //{
        //    logger.LogInformation($"Creating PropertyKey: {JsonConvert.SerializeObject(propertyKeyCreate, Formatting.Indented)}");
        //    var content = JsonConvert.SerializeObject(propertyKeyCreate);
        //    var response = await httpClient.PostAsync($"propertykeys", new StringContent(content, Encoding.UTF8, "application/json"));

        //    var id = await GetIdFromResponse(response, logger, content);
        //    if (id != Guid.Empty)
        //        await FeedbackHelper.Channel.SendMessageAsync($"PropertyKey '{propertyKeyCreate.Name}' successfully created with the ID {id}", MessageType.Info);
        //    return id;
        //}

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