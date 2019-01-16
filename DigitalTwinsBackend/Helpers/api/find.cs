// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DigitalTwinsBackend.Helpers
{
    public partial class Api
    {

        // Returns a Device with same hardwareId and spaceId if there is exactly one.
        // Or returns a Sensor with same hardwareId and deviceId if there is exactly one.
        // Otherwise returns null.
        public static async Task<T> FindElementAsync<T>(
            HttpClient httpClient,
            ILogger logger,
            string idFilter,
            string parentFilter,
            string includes = null)
        {
            //var filterHardwareIds = $"hardwareIds={hardwareId}";
            var includesParam = includes != null ? $"&includes={includes}" : "";
            var filter = $"{idFilter}&{parentFilter}{includesParam}";

            var response = await httpClient.GetAsync($"{typeof(T).Name.ToLower()}s?{filter}");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var elements = JsonConvert.DeserializeObject<IReadOnlyCollection<T>>(content);
                var matchingElement = elements.SingleOrDefault();
                if (matchingElement != null)
                {
                    logger.LogInformation($"Retrieved Unique {typeof(T).Name} : {JsonConvert.SerializeObject(matchingElement, Formatting.Indented)}");
                    return matchingElement;
                }
            }
            return default(T);
        }

        // Returns a matcher with same name and spaceId if there is exactly one.
        // Otherwise returns null.
        public static async Task<IEnumerable<Models.Matcher>> FindMatchers(
            HttpClient httpClient,
            ILogger logger,
            IEnumerable<string> names,
            Guid spaceId)
        {
            var commaDelimitedNames = names.Aggregate((string acc, string s) => acc + "," + s);
            var filterNames = $"names={commaDelimitedNames}";
            var filterSpaceId = $"&spaceId={spaceId.ToString()}";
            var filter = $"{filterNames}{filterSpaceId}";

            var response = await httpClient.GetAsync($"matchers?{filter}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var matchers = JsonConvert.DeserializeObject<IReadOnlyCollection<Models.Matcher>>(content);
                if (matchers != null)
                {
                    logger.LogInformation($"Retrieved Unique Matchers using 'names' and 'spaceId': {JsonConvert.SerializeObject(matchers, Formatting.Indented)}");
                    return matchers;
                }
            }
            return null;
        }

        // Returns a space with same name and parentId if there is exactly one
        // that maches that criteria. Otherwise returns null.
        public static async Task<Space> FindSpace(
            HttpClient httpClient,
            ILogger logger,
            string name,
            Guid parentId)
        {
            var filterName = $"Name eq '{name}'";
            var filterParentSpaceId = parentId != Guid.Empty
                ? $"ParentSpaceId eq guid'{parentId}'"
                : $"ParentSpaceId eq null";
            var odataFilter = $"$filter={filterName} and {filterParentSpaceId}";

            var response = await httpClient.GetAsync($"spaces?{odataFilter}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var spaces = JsonConvert.DeserializeObject<IReadOnlyCollection<Models.Space>>(content);
                var matchingSpace = spaces.SingleOrDefault();
                if (matchingSpace != null)
                {
                    logger.LogInformation($"Retrieved Unique Space using 'name' and 'parentSpaceId': {JsonConvert.SerializeObject(matchingSpace, Formatting.Indented)}");
                    return matchingSpace;
                }
            }
            return null;
        }

        public static async Task<IEnumerable<Space>> FindSpaces(
            HttpClient httpClient,
            ILogger logger,
            string name,
            int typeId = -1)
        {
            var filterName = $"substringof('{name}', Name)";
            var filterTypeId = typeId != -1 ? $"and TypeId eq {typeId}" : "";
            var includesFilter = $"&includes=types";

            var odataFilter = $"$filter={filterName} {filterTypeId}";

            var response = await httpClient.GetAsync($"spaces?{odataFilter}{includesFilter}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<Models.Space>>(content);
            }
            return new List<Space>();
        }

 
        public static async Task<IEnumerable<Sensor>> FindSensorsOfSpace(
            HttpClient httpClient,
            ILogger logger,
            Guid spaceId)
        {
            var response = await httpClient.GetAsync($"sensors?spaceId={spaceId.ToString()}&includes=Types");
            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var sensors = JsonConvert.DeserializeObject<IEnumerable<Models.Sensor>>(content);
                logger.LogInformation($"Retrieved {sensors.Count()} Sensors");
                return sensors;
            }
            else
            {
                return Array.Empty<Models.Sensor>();
            }
        }


        public static async Task<IEnumerable<PropertyKey>> FindPropertyKeys(
            Guid spaceId,
            HttpClient httpClient,
            ILogger logger,
            Scope scope = Scope.None)
        {
            var scopeParam = scope != Scope.None ? $"&scope={scope}" : "";
            var response = await httpClient.GetAsync($"propertykeys?spaceId={spaceId.ToString()}&includes=description&traverse=up{scopeParam}");

            if (await IsSuccessCall(response, logger))
            {
                var content = await response.Content.ReadAsStringAsync();
                var propertyKeys = JsonConvert.DeserializeObject<IEnumerable<PropertyKey>>(content);
                logger.LogInformation($"Retrieved {propertyKeys.Count()} PropertyKeys");
                return propertyKeys;
            }
            else
            {
                return Array.Empty<PropertyKey>();
            }
        }
    }
}
