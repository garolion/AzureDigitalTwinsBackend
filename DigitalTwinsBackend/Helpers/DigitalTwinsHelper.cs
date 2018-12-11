using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DigitalTwinsBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DigitalTwinsBackend.Helpers
{
    public class DigitalTwinsHelper
    {
        private static async Task RefreshCacheAsync(IMemoryCache memoryCache, Object cacheElement, object id, bool elementHasChanged, Context context)
        {
            CacheHelper.AddInCache(memoryCache, cacheElement, id, context);

            // if the cacheElement is an Model Entity (not a list of elements) we check if we need to update lists
            if (elementHasChanged && cacheElement.GetType().Namespace.Equals("DigitalTwinsBackend.Models"))
            {
                switch (cacheElement.GetType().Name)
                {
                    case "Space":
                        {
                            IEnumerable<Space> spaces = await GetSpacesAsync(memoryCache, Loggers.SilentLogger, true);
                            // we update the full list of spaces with the default Guid key
                            CacheHelper.AddInCache(memoryCache, spaces, Guid.Empty, Context.Space);
                            break;
                        }
                    case "Device":
                        {
                            Device device = await GetDeviceAsync((Guid)id, memoryCache, Loggers.SilentLogger, true);
                            CacheHelper.AddInCache(memoryCache, device, id, context);
                            if (device.SpaceId != Guid.Empty)
                            {
                                Space space = await GetSpaceAsync(device.SpaceId, memoryCache, Loggers.SilentLogger, true);
                                CacheHelper.AddInCache(memoryCache, space, device.SpaceId, Context.Space);
                            }
                            break;
                        }
                    case "Sensor":
                        {
                            Sensor sensor = await GetSensorAsync((Guid)id, memoryCache, Loggers.SilentLogger, true);
                            CacheHelper.AddInCache(memoryCache, sensor, id, context);
                            if (sensor.SpaceId != Guid.Empty)
                            {
                                Space space = await GetSpaceAsync(sensor.SpaceId, memoryCache, Loggers.SilentLogger, true);
                                CacheHelper.AddInCache(memoryCache, space, sensor.SpaceId, Context.Space);

                                Device device = await GetDeviceAsync(sensor.DeviceId, memoryCache, Loggers.SilentLogger, true);
                                CacheHelper.AddInCache(memoryCache, device, sensor.DeviceId, Context.Device);
                            }
                            break;
                        }

                    default:
                        return;
                }
            }
        }

        private static async Task RemoveFromCacheAsync(IMemoryCache memoryCache, Object cacheElement, object id)
        {
            IEnumerable<Space> spaces = null;

            switch (cacheElement.GetType().Name)
            {
                case "Space":
                    CacheHelper.DeleteFromCache(memoryCache, id, Context.None);
                    spaces = await GetSpacesAsync(memoryCache, Loggers.SilentLogger, true);
                    CacheHelper.AddInCache(memoryCache, spaces, id, Context.Space);
                    break;
                default:
                    return;
            }
        }

        public static async Task<IEnumerable<Space>> CreateSpaces(
             IMemoryCache memoryCache,
             ILogger logger,
             IEnumerable<SpaceDescription> descriptions,
             IEnumerable<IFormFile> udfFiles,
             Guid parentId)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            var spaceResults = new List<Space>();
            foreach (var description in descriptions)
            {
                var spaceId = await CreateOrPatchSpace(memoryCache, logger, parentId, description);

                if (spaceId != Guid.Empty)
                {
                    // This must happen before devices (or anyhting that could have devices like other spaces)
                    // or the device create will fail because a resource is required on an ancestor space
                    if (description.resources != null)
                        await CreateResources(memoryCache, logger, description.resources, spaceId);

                    var devices = description.devices != null
                        ? await CreateDevices(memoryCache, logger, description.devices, spaceId)
                        : Array.Empty<Models.Device>();

                    if (description.matchers != null)
                        await CreateMatchers(memoryCache, logger, description.matchers, spaceId);

                    if (description.userdefinedfunctions != null)
                        await CreateUserDefinedFunctions(memoryCache, logger, description.userdefinedfunctions, udfFiles, spaceId);

                    if (description.roleassignments != null)
                        await CreateRoleAssignments(memoryCache, logger, description.roleassignments, spaceId);

                    var childSpacesResults = description.spaces != null
                        ? await CreateSpaces(memoryCache, logger, description.spaces, udfFiles, spaceId)
                        : Array.Empty<Space>();

                    var sensors = await Api.GetSensorsOfSpace(httpClient, logger, spaceId);

                    spaceResults.Add(new Space()
                    {
                        Id = spaceId,
                        Devices = devices.Select(device => new Device()
                        {
                            ConnectionString = device.ConnectionString,
                            HardwareId = device.HardwareId,
                        }),
                        Sensors = sensors.Select(sensor => new Sensor()
                        {
                            DataType = sensor.DataType,
                            HardwareId = sensor.HardwareId,
                        }),
                        Children = childSpacesResults,
                    });
                }
            }

            return spaceResults;
        }

        private static async Task<IEnumerable<Models.Device>> CreateDevices(
            IMemoryCache memoryCache,
            ILogger logger,
            IEnumerable<DeviceDescription> descriptions,
            Guid spaceId)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (spaceId == Guid.Empty)
                throw new ArgumentException("Devices must have a spaceId");

            var devices = new List<Models.Device>();

            foreach (var description in descriptions)
            {
                var device = await CreateOrPatchDevice(memoryCache, logger, spaceId, description);

                if (device != null)
                {
                    devices.Add(device);

                    if (description.sensors != null)
                        await CreateSensors(memoryCache, logger, description.sensors, device.Id);
                }
            }
            return devices;
        }

        private static async Task CreateMatchers(
            IMemoryCache memoryCache,
            ILogger logger,
            IEnumerable<MatcherDescription> descriptions,
            Guid spaceId)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (spaceId == Guid.Empty)
                throw new ArgumentException("Matchers must have a spaceId");

            foreach (var description in descriptions)
            {
                await Api.CreateMatcherAsync(httpClient, logger, description.ToMatcher(spaceId));
            }
        }

        private static async Task CreateResources(
            IMemoryCache memoryCache,
            ILogger logger,
            IEnumerable<ResourceDescription> descriptions,
            Guid spaceId)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (spaceId == Guid.Empty)
                throw new ArgumentException("Resources must have a spaceId");

            foreach (var description in descriptions)
            {
                var createdId = await Api.CreateResourceAsync(httpClient, logger, description.ToResource(spaceId));
                if (createdId != Guid.Empty)
                {
                    // After creation resources might take time to be ready to use so we need
                    // to poll until it is done since downstream operations (like device creation)
                    // may depend on it
                    logger.LogInformation("Polling until resource is no longer in 'Provisioning' state...");
                    while (await Api.IsResourceProvisioning(httpClient, logger, createdId))
                    {
                        await FeedbackHelper.Channel.SendMessageAsync($"IoT Hub still in Provisoning state. Waiting for 5 more seconds...");
                        await Task.Delay(5000);
                    }
                }
            }
        }

        private static async Task CreateRoleAssignments(
            IMemoryCache memoryCache,
            ILogger logger,
            IEnumerable<RoleAssignmentDescription> descriptions,
            Guid spaceId)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (spaceId == Guid.Empty)
                throw new ArgumentException("RoleAssignments must have a spaceId");

            var space = await Api.GetSpace(httpClient, logger, spaceId, includes: "fullpath");

            // A SpacePath is the list of spaces formatted like so: "space1/space2" - where space2 has space1 as a parent
            // When getting SpacePaths of a space itself there is always exactly one path - the path from the root to itself
            // This is not true when getting space paths of other topology items (ie non spaces)
            var path = space.SpacePaths.Single();

            foreach (var description in descriptions)
            {
                Guid objectId;
                switch (description.objectIdType)
                {
                    case "UserDefinedFunctionId":
                        var udf = await Api.FindUserDefinedFunction(httpClient, logger, description.objectName, spaceId);
                        objectId = udf != null ? udf.Id : Guid.Empty;
                        break;
                    default:
                        objectId = Guid.Empty;
                        logger.LogError($"roleAssignment with objectName must have known objectIdType but instead has '{description.objectIdType}'");
                        break;
                }

                if (objectId != Guid.Empty)
                {
                    await Api.CreateRoleAssignmentAsync(httpClient, logger, description.ToRoleAssignment(objectId, path));
                }
                else
                {
                    await FeedbackHelper.Channel.SendMessageAsync("UDF not found. No role assignment created.");
                }
            }
        }

        private static async Task CreateSensors(
            IMemoryCache memoryCache,
            ILogger logger,
            IEnumerable<SensorDescription> descriptions,
            Guid deviceId)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (deviceId == Guid.Empty)
                throw new ArgumentException("Sensors must have a deviceId");

            foreach (var description in descriptions)
            {
                await Api.CreateSensorAsync(httpClient, logger, description.ToSensor(deviceId));
            }
        }

        private static async Task CreateUserDefinedFunctions(
            IMemoryCache memoryCache,
            ILogger logger,
            IEnumerable<UserDefinedFunctionDescription> descriptions,
            IEnumerable<IFormFile> udfFiles,
            Guid spaceId)
        {
            IFormFile file = null;
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (spaceId == Guid.Empty)
                throw new ArgumentException("UserDefinedFunctions must have a spaceId");

            foreach (var description in descriptions)
            {
                var matchers = await Api.FindMatchers(httpClient, logger, description.matcherNames, spaceId);

                try
                {
                    file = udfFiles.First(f => f.FileName.ToLower().Equals(description.script.ToLower()));
                }
                catch (InvalidOperationException)
                {
                    await FeedbackHelper.Channel.SendMessageAsync($"Error - No file found for the UDF {description.script}. Check you added a file with this name");
                }
                if (file != null)
                {
                    using (var r = new StreamReader(file.OpenReadStream()))
                    {
                        var js = await r.ReadToEndAsync();
                        if (String.IsNullOrWhiteSpace(js))
                        {
                            await FeedbackHelper.Channel.SendMessageAsync($"Error - We cannot read the content of the file {description.script}.");
                        }
                        else
                        {
                            await CreateOrPatchUserDefinedFunction(
                                memoryCache,
                                logger,
                                description,
                                js,
                                spaceId,
                                matchers);
                        }
                    }
                }
                else
                {
                    await FeedbackHelper.Channel.SendMessageAsync($"Error - The file '{description.script}' is missing for this UDF, creation ignored.");
                }
            }
        }

        private static async Task<Models.Device> CreateOrPatchDevice(IMemoryCache memoryCache, ILogger logger, Guid spaceId, DeviceDescription description)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            var device = await Api.FindDevice(httpClient, logger, description.hardwareId, spaceId);

            if (device != null)
            {
                await FeedbackHelper.Channel.SendMessageAsync($"Updating the device {device.Id} that already exist.");

                Device deviceToPatch = description.ToDevice(spaceId);
                device.Name = deviceToPatch.Name;
                device.Type = deviceToPatch.Type;
                device.SubType = deviceToPatch.SubType;
                await UpdateDeviceAsync(device, memoryCache, logger);
                               
                return device;
            }

            return await CreateDeviceAsync(description.ToDevice(spaceId), memoryCache, logger);
        }

        private static async Task<Guid> CreateOrPatchSpace(IMemoryCache memoryCache, ILogger logger, Guid parentId, SpaceDescription description)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var existingSpace = await Api.FindSpace(httpClient, logger, description.name, parentId);

            if (existingSpace != null)
            {
                await FeedbackHelper.Channel.SendMessageAsync($"Updating the space {existingSpace.Id} that already exist.");

                Space spaceToPatch = description.ToSpace(parentId);
                existingSpace.FriendlyName = spaceToPatch.FriendlyName;
                existingSpace.Type = spaceToPatch.Type;
                existingSpace.SubType = spaceToPatch.SubType;
                await UpdateSpaceAsync(existingSpace, memoryCache, logger);

                return existingSpace.Id;
            }

            return await CreateSpaceAsync(description.ToSpace(parentId), memoryCache, logger);
        }

        private static async Task CreateOrPatchUserDefinedFunction(
            IMemoryCache memoryCache,
            ILogger logger,
            UserDefinedFunctionDescription description,
            string js,
            Guid spaceId,
            IEnumerable<Matcher> matchers)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var userDefinedFunction = await Api.FindUserDefinedFunction(httpClient, logger, description.name, spaceId);

            if (userDefinedFunction != null)
            {
                await Api.UpdateUserDefinedFunction(
                    memoryCache,
                    logger,
                    description.ToUserDefinedFunctionUpdate(userDefinedFunction.Id, spaceId, matchers),
                    js);
            }

            await Api.CreateUserDefinedFunctionAsync(
                httpClient,
                logger,
                //description.ToUserDefinedFunctionCreate(spaceId, matchers.Select(m => m.Id)),
                description.ToUserDefinedFunctionCreate(spaceId, matchers),
                js);
        }
        #region Space management
        public static async Task<IEnumerable<Models.Space>> SearchSpacesAsync(IMemoryCache memoryCache, ILogger logger, String searchString, int typeId = -1)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            return await Api.FindSpaces(httpClient, logger, searchString, typeId);
        }


        public static async Task<IEnumerable<Models.Space>> GetSpacesAsync(IMemoryCache memoryCache, ILogger logger)
        {
            return await GetSpacesAsync(memoryCache, logger, false);
        }

        private static async Task<IEnumerable<Models.Space>> GetSpacesAsync(IMemoryCache memoryCache, ILogger logger, bool bypassCache)
        {
            IEnumerable<Space> spaces = null;

            if (!bypassCache)
            {
                spaces = CacheHelper.GetSpaceListFromCache(memoryCache);
            }

            if (spaces == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

                spaces = await Api.GetSpaces(
                    httpClient, logger,
                    maxNumberToGet: 100, includes: "types,values,properties");

                await RefreshCacheAsync(memoryCache, spaces, Guid.Empty, false, Context.Space).ConfigureAwait(false);
            }

            logger.LogInformation($"GetSpaces: {JsonConvert.SerializeObject(spaces, Formatting.Indented)}");
            return spaces;
        }

        public static async Task<IEnumerable<Models.Space>> GetRootSpacesAsync(IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            var spaces = await Api.GetSpaces(
                httpClient, logger,
                navigation: "maxLevel=1",
                includes: "types");

            await RefreshCacheAsync(memoryCache, spaces, Guid.Empty, false, Context.Space).ConfigureAwait(false);

            logger.LogInformation($"GetSpaces: {JsonConvert.SerializeObject(spaces, Formatting.Indented)}");
            return spaces;
        }

        public static async Task<Models.Space> GetSpaceAsync(Guid spaceId, IMemoryCache memoryCache, ILogger logger, bool bypassCache)
        {
            Space space = null;

            if (!bypassCache)
            {
                // try get space element from cache
                space = CacheHelper.GetSpaceFromCache(memoryCache, spaceId);
            }

            // if not in cache
            if (space == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

                // get space element from Digital Twins API
                space = await Api.GetSpace(
                    httpClient, logger,
                    spaceId, includes: "types,values,parent,childspaces,childspacestypes,properties,devices,sensors,users");

                // Add the Space element in cache & refresh Space list in cache
                await RefreshCacheAsync(memoryCache, space, spaceId, false, Context.Space).ConfigureAwait(false);
            }

            logger.LogInformation($"GetSpace: {spaceId}");
            return space;
        }

        public static async Task<Guid> CreateSpaceAsync(Models.Space space, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var spaceId = await Api.CreateSpaceAsync(httpClient, logger, space);
            space.Id = spaceId; 
            await RefreshCacheAsync(memoryCache, space, spaceId, true, Context.Space).ConfigureAwait(false);

            logger.LogInformation($"CreateSpace: {spaceId}");
            return spaceId;
        }

        public static async Task UpdateSpaceAsync(Models.Space space, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (await Api.UpdateSpaceAsync(memoryCache, logger, space))
            {
                await RefreshCacheAsync(memoryCache, space, space.Id, true, Context.Space).ConfigureAwait(false);
                logger.LogInformation($"UpdateSpace: {space.Id}");
            }
        }

        public static async Task<bool> DeleteSpaceAsync(Models.Space space, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (await Api.DeleteSpaceAsync(httpClient, logger, space))
            {
                await RemoveFromCacheAsync(memoryCache, space, space.Id).ConfigureAwait(false);
                logger.LogInformation($"DeleteSpace: {space.Id}");
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion region


        public static async Task<Models.Device> GetDeviceAsync(Guid deviceId, IMemoryCache memoryCache, ILogger logger, bool bypassCache)
        {
            Device device = null;
            if (!bypassCache)
            {
                device = CacheHelper.GetDeviceFromCache(memoryCache, deviceId);
            }

            if (device == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

                device = await Api.GetDevice(
                    httpClient, logger,
                    deviceId, includes: "sensors, sensorsvalues, ConnectionString");

                await RefreshCacheAsync(memoryCache, device, deviceId, false, Context.Device).ConfigureAwait(false);
            }

            logger.LogInformation($"GetDevice: {device}");
            return device;
        }

        public static async Task<Device> CreateDeviceAsync(Device device, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var deviceId = await Api.CreateDeviceAsync(httpClient, logger, device);

            device = await Api.GetDevice(httpClient, logger, deviceId, includes: "sensors, sensorsvalues, ConnectionString");
            await RefreshCacheAsync(memoryCache, device, deviceId, true, Context.Device).ConfigureAwait(false);

            logger.LogInformation($"CreateDevice: {deviceId}");
            return device;
        }

        public static async Task UpdateDeviceAsync(Models.Device device, IMemoryCache memoryCache, ILogger logger)
        {
            await Api.UpdateDeviceAsync(memoryCache, logger, device);
            await RefreshCacheAsync(memoryCache, device, device.Id, true, Context.Device).ConfigureAwait(false);

            logger.LogInformation($"UpdateDevice: {device.Id}");
        }

        public static async Task DeleteDeviceAsync(Models.Device device, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            await Api.DeleteDeviceAsync(httpClient, logger, device);
            await RemoveFromCacheAsync(memoryCache, device, device.Id).ConfigureAwait(false);

            logger.LogInformation($"DeleteDevice: {device.Id}");
        }

        public static async Task<Models.Sensor> GetSensorAsync(Guid id, IMemoryCache memoryCache, ILogger logger, bool bypassCache)
        {
            Sensor sensor = null;
            if (!bypassCache)
            {
                sensor = CacheHelper.GetSensorFromCache(memoryCache, id);
            }

            if (sensor == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
                sensor = await Api.GetSensor(httpClient, logger, id, includes: "value");
                await RefreshCacheAsync(memoryCache, sensor, id, false, Context.Sensor).ConfigureAwait(false);
            }

            logger.LogInformation($"GetSensor: {id}");
            return sensor;
        }

        public static async Task<Guid> CreateSensorAsync(Models.Sensor sensor, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var sensorId = await Api.CreateSensorAsync(httpClient, logger, sensor);

            sensor = await Api.GetSensor(httpClient, logger, sensorId, includes: "value");
            await RefreshCacheAsync(memoryCache, sensor, sensorId, true, Context.Sensor).ConfigureAwait(false);

            logger.LogInformation($"CreateSensor: {sensorId}");
            return sensorId;
        }
        public static async Task UpdateSensorAsync(Models.Sensor sensor, IMemoryCache memoryCache, ILogger logger)
        {
            await Api.UpdateSensorAsync(memoryCache, logger, sensor);
            await RefreshCacheAsync(memoryCache, sensor, sensor.Id, true, Context.Device).ConfigureAwait(false);

            logger.LogInformation($"UpdateSensor: {sensor.Id}");
        }

        public static async Task DeleteSensorAsync(Models.Sensor sensor, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            await Api.DeleteSensorAsync(httpClient, logger, sensor);
            await RemoveFromCacheAsync(memoryCache, sensor, sensor.Id).ConfigureAwait(false);

            logger.LogInformation($"DeleteSensor: {sensor.Id}");
        }


        #region Matcher & UDF management
        public static async Task<IEnumerable<Models.Matcher>> GetMatchersBySpaceId(Guid spaceId, IMemoryCache memoryCache, ILogger logger)
        {
            IEnumerable<Models.Matcher> matchers = CacheHelper.GetMatchersFromCache(memoryCache, Context.Matcher, spaceId);

            if (matchers == null)
            {

                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

                matchers = await Api.GetMatchersBySpaceId(
                    httpClient, logger, spaceId,
                    includes: "conditions");

                await RefreshCacheAsync(memoryCache, matchers, spaceId, false, Context.Matcher).ConfigureAwait(false);
            }

            logger.LogInformation($"GetMatchersBySpaceId: {JsonConvert.SerializeObject(matchers, Formatting.Indented)}");
            return matchers;
        }

        public static async Task<IEnumerable<Models.UserDefinedFunction>> GetUDFsBySpaceId(Guid spaceId, IMemoryCache memoryCache, ILogger logger)
        {
            var userDefinedFunctions = CacheHelper.GetUDFsFromCache(memoryCache, Context.UDF, spaceId);


            if (userDefinedFunctions == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

                userDefinedFunctions = await Api.GetUserDefinedFunctions(
                    httpClient, logger, spaceId,
                    includes: "matchers");

                await RefreshCacheAsync(memoryCache, userDefinedFunctions, spaceId, false, Context.UDF).ConfigureAwait(false);
            }

            logger.LogInformation($"GetUserDefinedFunctions: {JsonConvert.SerializeObject(userDefinedFunctions, Formatting.Indented)}");
            return userDefinedFunctions;
        }

        public static async Task<UserDefinedFunction> GetUserDefinedFunctionsByFunctionId(Guid functionId, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            var userDefinedFunction = await Api.GetUserDefinedFunctionsById(
                httpClient, logger, functionId,
                includes: "matchers");

            if (userDefinedFunction != null)
                logger.LogInformation($"GetUserDefinedFunctions: {JsonConvert.SerializeObject(userDefinedFunction, Formatting.Indented)}");

            return userDefinedFunction;
        }

        public static async Task<string> GetUserDefinedFunctionContent(Guid functionId, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var userDefinedFunctions = await Api.GetUserDefinedFunctionContent(httpClient, logger, functionId);
            return userDefinedFunctions;
        }
        #endregion

        public static async Task<IEnumerable<Models.SystemType>> GetTypesAsync(SystemTypes listType, IMemoryCache memoryCache, ILogger logger)
        {
            IEnumerable<SystemType> types = CacheHelper.GetTypeListFromCache(listType, memoryCache);

            if (types == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

                types = await Api.GetTypes(
                    httpClient, logger,
                    maxNumberToGet: 100, categories: listType.ToString());

                logger.LogInformation($"GetSpaceTypes: {JsonConvert.SerializeObject(types, Formatting.Indented)}");

                await RefreshCacheAsync(memoryCache, types, listType, false, Context.None).ConfigureAwait(false);
            }

            return types;
        }

        public static async Task<List<Models.Ontology>> GetOntologiesWithTypes(IMemoryCache memoryCache, ILogger logger)
        {
            var cache = CacheHelper.GetOntologyListFromCache(memoryCache);
            List<Ontology> ontologies;

            if (cache == null)
            {
                ontologies = new List<Ontology>();
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

                var _ontologies = await Api.GetOntologies(httpClient, logger);
                for (int i=0; i< _ontologies.Count(); i++)
                {
                    var item = await Api.GetOntologyWithTypes(httpClient, logger, _ontologies.ElementAt(i).Id);
                    ontologies.Add(item);
                }

                logger.LogInformation($"GetOntologiesWithTypes: {JsonConvert.SerializeObject(ontologies, Formatting.Indented)}");

                await RefreshCacheAsync(memoryCache, ontologies, CacheKeys.OntologyList, false, Context.None).ConfigureAwait(false);
            }
            else
            {
                ontologies = cache.ToList();
            }

            return ontologies;
        }
    }
}
