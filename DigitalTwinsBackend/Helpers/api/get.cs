// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DigitalTwinsBackend.Helpers
{
    public partial class Api
    {
        public static async Task<IEnumerable<Models.Ontology>> GetOntologies(
            HttpClient httpClient,
            ILogger logger)
        {
            var response = await httpClient.GetAsync($"ontologies");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var ontologies = JsonConvert.DeserializeObject<IEnumerable<Models.Ontology>>(content);
                logger.LogInformation($"Retrieved Ontologies: {JsonConvert.SerializeObject(ontologies, Formatting.Indented)}");
                return ontologies;
            }
            else
            {
                return Array.Empty<Models.Ontology>();
            }
        }

        public static async Task<Models.Ontology> GetOntologyWithTypes(
        HttpClient httpClient,
        ILogger logger,
        int ontologyId)
        {
            var response = await httpClient.GetAsync($"ontologies/{ontologyId}?includes=types");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var ontology = JsonConvert.DeserializeObject<Models.Ontology>(content);
                logger.LogInformation($"Retrieved Ontology: {JsonConvert.SerializeObject(ontology, Formatting.Indented)}");
                return ontology;
            }
            else
            {
                return null;
            }
        }

        public static async Task<IEnumerable<Models.PropertyKey>> GetPropertyKeys(
            HttpClient httpClient,
            ILogger logger)
        {
            var response = await httpClient.GetAsync($"propertykeys");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var propertyKeys = JsonConvert.DeserializeObject<IEnumerable<Models.PropertyKey>>(content);
                logger.LogInformation($"Retrieved PropertyKeys: {JsonConvert.SerializeObject(propertyKeys, Formatting.Indented)}");
                return propertyKeys;
            }
            else
            {
                return Array.Empty<Models.PropertyKey>();
            }
        }

        public static async Task<IEnumerable<Models.PropertyKey>> GetPropertyKeysBySpace(
            Guid spaceId,
            HttpClient httpClient,
            ILogger logger)
        {
            var response = await httpClient.GetAsync($"propertykeys?spaceId={spaceId}&traverse=Up");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var propertyKeys = JsonConvert.DeserializeObject<IEnumerable<Models.PropertyKey>>(content);
                logger.LogInformation($"Retrieved PropertyKeys: {JsonConvert.SerializeObject(propertyKeys, Formatting.Indented)}");
                return propertyKeys;
            }
            else
            {
                return Array.Empty<Models.PropertyKey>();
            }
        }

        public static async Task<Models.Resource> GetResource(
            HttpClient httpClient,
            ILogger logger,
            Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("GetResource requires a non empty guid as id");

            var response = await httpClient.GetAsync($"resources/{id}");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var resource = JsonConvert.DeserializeObject<Models.Resource>(content);
                logger.LogInformation($"Retrieved Resource: {JsonConvert.SerializeObject(resource, Formatting.Indented)}");
                return resource;
            }

            return null;
        }

        public static async Task<Models.Space> GetSpace(
            HttpClient httpClient,
            ILogger logger,
            Guid id,
            string includes = null)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("GetSpace requires a non empty guid as id");

            var response = await httpClient.GetAsync($"spaces/{id}/" + (includes != null ? $"?includes={includes}" : ""));
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var space = JsonConvert.DeserializeObject<Models.Space>(content);
                logger.LogInformation($"Retrieved Space: {JsonConvert.SerializeObject(space, Formatting.Indented)}");
                return space;
            }

            return null;
        }

        public static async Task<Models.Device> GetDevice(
            HttpClient httpClient,
            ILogger logger,
            Guid id,
            string includes = null)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("GetDevice requires a non empty guid as id");

            var response = await httpClient.GetAsync($"devices/{id}/" + (includes != null ? $"?includes={includes}" : ""));
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var device = JsonConvert.DeserializeObject<Models.Device>(content);
                logger.LogInformation($"Retrieved Device: {JsonConvert.SerializeObject(device, Formatting.Indented)}");
                return device;
            }

            return null;
        }

        public static async Task<Models.Sensor> GetSensor(
            HttpClient httpClient,
            ILogger logger,
            Guid id,
            string includes = null)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("GetSensor requires a non empty guid as id");

            var response = await httpClient.GetAsync($"sensors/{id}/" + (includes != null ? $"?includes={includes}" : ""));
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var sensor = JsonConvert.DeserializeObject<Models.Sensor>(content);
                logger.LogInformation($"Retrieved Sensor: {JsonConvert.SerializeObject(sensor, Formatting.Indented)}");
                return sensor;
            }

            return null;
        }

        public static async Task<IEnumerable<Models.Space>> GetSpaces(
            HttpClient httpClient,
            ILogger logger,
            int maxNumberToGet = 1000,
            string navigation = null,
            string includes = null,
            string propertyKey = null)
        {
            var navigationFilter = (navigation != null ? $"{navigation}" : "");
            var includesFilter = (includes != null ? $"includes={includes}" : "");
            var propertyKeyFilter = (propertyKey != null ? $"propertyKey={propertyKey}" : "");
            var topFilter = $"$top={maxNumberToGet}";
            var response = await httpClient.GetAsync($"spaces{MakeQueryParams(new[] { navigationFilter, includesFilter, propertyKeyFilter, topFilter })}");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var spaces = JsonConvert.DeserializeObject<IEnumerable<Models.Space>>(content);
                logger.LogInformation($"Retrieved {spaces.Count()} Spaces");
                return spaces;
            }
            else
            {
                return Array.Empty<Models.Space>();
            }
        }

        public static async Task<IEnumerable<Device>> GetDevices(
            HttpClient httpClient,
            ILogger logger,
            int maxNumberToGet = 100,
            string navigation = null,
            string includes = null,
            string propertyKey = null)
        {
            var navigationFilter = (navigation != null ? $"{navigation}" : "");
            var includesFilter = (includes != null ? $"includes={includes}" : "");
            var propertyKeyFilter = (propertyKey != null ? $"propertyKey={propertyKey}" : "");
            var topFilter = $"$top={maxNumberToGet}";
            var response = await httpClient.GetAsync($"devices{MakeQueryParams(new[] { navigationFilter, includesFilter, propertyKeyFilter, topFilter })}");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var devices = JsonConvert.DeserializeObject<IEnumerable<Models.Device>>(content);
                logger.LogInformation($"Retrieved {devices.Count()} Spaces");
                return devices;
            }
            else
            {
                return Array.Empty<Models.Device>();
            }
        }

        public static async Task<IEnumerable<Models.UserDefinedFunction>> GetUserDefinedFunctions(
            HttpClient httpClient,
            ILogger logger,
            Guid spaceId,
            string includes = null)
        {
            var includesFilter = (includes != null ? $"includes={includes}" : "");

            var response = await httpClient.GetAsync($"userdefinedfunctions?spaceId={spaceId}" + (includes != null ? $"&includes={includes}" : ""));
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var userdefinedfunctions = JsonConvert.DeserializeObject<IEnumerable<Models.UserDefinedFunction>>(content);
                logger.LogInformation($"Retrieved {userdefinedfunctions.Count()} UDF");
                return userdefinedfunctions;
            }
            else
            {
                return Array.Empty<Models.UserDefinedFunction>();
            }
        }

        public static async Task<IEnumerable<Models.Matcher>> GetMatchersBySpaceId(
            HttpClient httpClient,
            ILogger logger,
            Guid id,
            string includes = null)
        {
            var includesFilter = (includes != null ? $"includes={includes}" : "");

            var response = await httpClient.GetAsync($"matchers?spaceId={id}" + (includes != null ? $"&includes={includes}" : ""));
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var matchers = JsonConvert.DeserializeObject<IEnumerable<Models.Matcher>>(content);
                logger.LogInformation($"Retrieved {matchers.Count()} Matchers");
                return matchers;
            }
            else
            {
                return Array.Empty<Models.Matcher>();
            }
        }

        public static async Task<bool> EvaluateIfMatchersIsTrue(
            HttpClient httpClient,
            ILogger logger,
            Guid matcherId,
            string sensorId)
        {
            var response = await httpClient.GetAsync($"matchers/{matcherId}/evaluate/{sensorId}");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var isTrue = JsonConvert.DeserializeObject<bool>(content);

                return isTrue;
            }
            else
            {
                return false;
            }
        }

        public static async Task<UserDefinedFunction> GetUserDefinedFunctionsById(
            HttpClient httpClient,
            ILogger logger,
            Guid id,
            string includes = null)
        {
            var includesFilter = (includes != null ? $"includes={includes}" : "");

            var response = await httpClient.GetAsync($"userdefinedfunctions/{id}" + (includes != null ? $"?includes={includes}" : ""));

            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var userdefinedfunction = JsonConvert.DeserializeObject<Models.UserDefinedFunction>(content);
                return userdefinedfunction;
            }
            else
            {
                return null;
            }
        }

        public static async Task<string> GetUserDefinedFunctionContent(
            HttpClient httpClient,
            ILogger logger,
            Guid functionId)
        {
            var response = await httpClient.GetAsync($"userdefinedfunctions/{functionId}/contents");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                //var userdefinedfunctions = JsonConvert.DeserializeObject<IEnumerable<Models.UserDefinedFunction>>(content);
                logger.LogInformation($"Retrieved UDF content" + content);
                return content;
            }
            else
            {
                return string.Empty;
            }
        }

        //public static async Task<IEnumerable<Models.Sensor>> GetSensorsOfSpace(
        //    HttpClient httpClient,
        //    ILogger logger,
        //    Guid spaceId)
        //{
        //    var response = await httpClient.GetAsync($"sensors?spaceId={spaceId.ToString()}&includes=Types");
        //    if (await IsSuccessCall(response, logger))
        //    {
        //        var content = await response.Content.ReadAsStringAsync();
        //        var sensors = JsonConvert.DeserializeObject<IEnumerable<Models.Sensor>>(content);
        //        logger.LogInformation($"Retrieved {sensors.Count()} Sensors");
        //        return sensors;
        //    }
        //    else
        //    {
        //        return Array.Empty<Models.Sensor>();
        //    }
        //}

        private static string MakeQueryParams(IEnumerable<string> queryParams)
        {
            return queryParams
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select((s, i) => (i == 0 ? '?' : '&') + s)
                .Aggregate((result, cur) => result + cur);
        }

        public static async Task<IEnumerable<Models.Type>> GetTypes(
            HttpClient httpClient,
            ILogger logger,
            int maxNumberToGet = 10,
            string includes = null,
            string categories = null)
        {
            var includesFilter = (includes != null ? $"includes={includes}" : "");
            var categoriesFilter = (categories != null ? $"categories={categories}" : "");

            var response = await httpClient.GetAsync($"types{MakeQueryParams(new[] { includesFilter, categoriesFilter })}");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var types = JsonConvert.DeserializeObject<IEnumerable<Models.Type>>(content);
                logger.LogInformation($"Retrieved {types.Count()} Types");
                return types;
            }
            else
            {
                return Array.Empty<Models.Type>();
            }
        }


        private static async Task<bool> IsSuccessCall(HttpResponseMessage response, ILogger logger)
        {
            return await IsSuccessCall(response, logger, string.Empty);
        }

        private static async Task<bool> IsSuccessCall(HttpResponseMessage response, ILogger logger, string requestBody)
        {
            if (ConfigHelper.Config.parameters.EnableAPICallTrace)
            {
                await FeedbackHelper.Channel.SendMessageAsync(
                    $"{response.RequestMessage.Method.ToString().ToUpper()} - {response.RequestMessage.RequestUri.ToString()}", MessageType.APICall);

                if (requestBody != string.Empty)
                {
                    await FeedbackHelper.Channel.SendMessageAsync($"Body: {requestBody}", MessageType.APICall);
                }
            }

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (ConfigHelper.Config.parameters.EnableVerboseMode && content != null && content.Length > 0)
                {
                    await FeedbackHelper.Channel.SendMessageAsync(
                        $"{response.RequestMessage.Method.ToString().ToUpper()} - {response.RequestMessage.RequestUri.ToString()}", MessageType.Info);

                    if (requestBody != string.Empty)
                    {
                        await FeedbackHelper.Channel.SendMessageAsync($"Body: {requestBody}", MessageType.Info);
                    }
                    
                    await FeedbackHelper.Channel.SendMessageAsync(JsonConvert.SerializeObject(content, Formatting.Indented), MessageType.Info);

                    logger.LogInformation(content);
                }
                return true;
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();

                if (content != null && content.Length > 0)
                {
                    var error = JsonConvert.DeserializeObject<ErrorMessage>(content);
                    await FeedbackHelper.Channel.SendMessageAsync($"Error {error.Error.Code} - {error.Error.Message}", MessageType.Info);
                    logger.LogInformation($"Error {error.Error.Code} - {error.Error.Message}");
                }
                return false;
            }
        }
    }

    public class ErrorMessage
    {
        public InnerError Error { get; set; }
        public string Message { get; set; }
        public string Result { get; set; }
    }

    public class InnerError
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}