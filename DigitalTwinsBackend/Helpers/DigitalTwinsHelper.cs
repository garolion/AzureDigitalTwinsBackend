using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DigitalTwinsBackend.Models;
using DigitalTwinsBackend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DigitalTwinsBackend.Helpers
{
    public class DigitalTwinsHelper
    {
        private static List<ProvisionedSpace> spaceResults;

        #region Private methods related to cache management
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

                            if (((Space)cacheElement).ParentSpaceId == Guid.Empty)
                            {
                                var rootSpaces = await GetRootSpacesAsync(memoryCache, Loggers.SilentLogger, true); 
                                await RefreshCacheAsync(memoryCache, rootSpaces, Guid.Empty, false, Context.RootSpaces).ConfigureAwait(false);
                            }
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
                case "Device":
                    {
                        Device device = (Device)cacheElement;
                        CacheHelper.DeleteFromCache(memoryCache, device.Id, Context.Device);

                        if (device.SpaceId != Guid.Empty)
                        {
                            Space space = await GetSpaceAsync(device.SpaceId, memoryCache, Loggers.SilentLogger, true);
                            CacheHelper.AddInCache(memoryCache, space, device.SpaceId, Context.Space);
                        }
                        break;
                    }
                case "Sensor":
                    {
                        var sensor = (Sensor)cacheElement;
                        CacheHelper.DeleteFromCache(memoryCache, sensor.Id, Context.Sensor);

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
        #endregion

        #region Methods related to batch creation (using YAML description)
        public static async Task<IEnumerable<ProvisionedSpace>> CreateSpaces(
             IMemoryCache memoryCache,
             ILogger logger,
             IEnumerable<SpaceDescription> descriptions,
             IEnumerable<IFormFile> udfFiles,
             Guid parentId,
             int level = 1)
        {
            if (level == 1)
            {
                spaceResults = new List<ProvisionedSpace>();
            }

            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            foreach (var description in descriptions)
            {
                var space = await CreateOrPatchSpaceAsync(memoryCache, logger, parentId, description.ToSpace(parentId));

                spaceResults.Add(new ProvisionedSpace()
                {
                    Space = space,
                    MarginLeft = (level * 25).ToString() + "px"
                });

                if (space != null)
                {
                    // This must happen before devices (or anything that could have devices like other spaces)
                    // or the device creation will fail because a resource is required on an ancestor space
                    if (description.resources != null)
                        await CreateResources(memoryCache, logger, description.resources, space.Id);

                    var devices = description.devices != null
                        ? await CreateDevices(memoryCache, logger, description.devices, space.Id)
                        : Array.Empty<Device>();

                    if (description.matchers != null)
                        await CreateMatchers(memoryCache, logger, description.matchers, space.Id);

                    if (description.userdefinedfunctions != null)
                        await CreateUserDefinedFunction(memoryCache, logger, description.userdefinedfunctions, udfFiles, space.Id);

                    if (description.roleassignments != null)
                        await CreateRoleAssignments(memoryCache, logger, description.roleassignments, space.Id);

                    var childSpacesResults = description.spaces != null
                        ? await CreateSpaces(memoryCache, logger, description.spaces, udfFiles, space.Id, level + 1)
                        : Array.Empty<ProvisionedSpace>();

                    var sensors = await Api.FindSensorsOfSpace(httpClient, logger, space.Id);

                    space.Devices = devices;
                    space.Sensors = sensors;
                    space.Children = childSpacesResults.Select(child => child.Space);

                    spaceResults[spaceResults.FindIndex(ps => ps.Space.Id == space.Id)].Space = space;

                    //spaceResults.Add(new ProvisionedSpace()
                    //{
                    //    Space = space,
                    //    MarginLeft = (level * 25).ToString() + "px"
                    //});

                    //space.Devices = devices;
                    //space.Sensors = sensors;
                    //space.Children = childSpacesResults.Select(child => child.Space);


                }
            }

            //spaceResults.Reverse();
            return spaceResults;
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
                var createdId = await Api.CreateAsync<Resource>(httpClient, logger, description.ToResource(spaceId));
                if (createdId != Guid.Empty)
                {
                    // After creation resources might take time to be ready to use so we need
                    // to poll until it is done since downstream operations (like device creation)
                    // may depend on it
                    logger.LogInformation("Polling until resource is no longer in 'Provisioning' state...");
                    while (await Api.IsResourceProvisioning(httpClient, logger, createdId))
                    {
                        await FeedbackHelper.Channel.SendMessageAsync($"IoT Hub still in Provisoning state. Waiting for 5 more seconds...", MessageType.Info);
                        await Task.Delay(5000);
                    }
                }
            }
        }

        private static async Task<IEnumerable<Device>> CreateDevices(
            IMemoryCache memoryCache,
            ILogger logger,
            IEnumerable<DeviceDescription> descriptions,
            Guid spaceId)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (spaceId == Guid.Empty)
                throw new ArgumentException("Devices must have a spaceId");

            var devices = new List<Device>();

            foreach (var description in descriptions)
            {
                var device = await CreateOrPatchDeviceAsync(memoryCache, logger, spaceId, description.ToDevice(spaceId));

                if (device != null)
                {
                    devices.Add(device);

                    if (description.sensors != null)
                        await CreateSensors(memoryCache, logger, description.sensors, device.Id);
                }
            }
            return devices;
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
                await CreateOrPatchSensorAsync(memoryCache, logger, deviceId, description.ToSensor(deviceId));
            }
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
                await CreateOrPatchMatcherAsync(memoryCache, logger, spaceId, description.ToMatcher(spaceId));
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
                        var udf = await Api.FindElementAsync<UserDefinedFunction>(httpClient, logger, $"names={description.objectName}", $"spaceId={spaceId}", "matchers");
                        //var udf = await Api.FindUserDefinedFunction(httpClient, logger, description.objectName, spaceId);
                        objectId = udf != null ? udf.Id : Guid.Empty;
                        break;
                    default:
                        objectId = Guid.Empty;
                        logger.LogError($"roleAssignment with objectName must have known objectIdType but instead has '{description.objectIdType}'");
                        break;
                }

                if (objectId != Guid.Empty)
                {
                    var existingRoleAssigment = await Api.FindElementAsync<RoleAssignment>(httpClient, logger, $"objectid={objectId}", $"path={path}");
                    if (existingRoleAssigment == null)
                    {
                        await Api.CreateAsync<RoleAssignment>(httpClient, logger, description.ToRoleAssignment(objectId, path));
                    }
                    else
                    {
                        await FeedbackHelper.Channel.SendMessageAsync($"RoleAssigment {existingRoleAssigment.Id} was already created for the ObjectId {existingRoleAssigment.ObjectId}.", MessageType.Info);
                    }
                }
                else
                {
                    await FeedbackHelper.Channel.SendMessageAsync("UDF not found. No role assignment created.", MessageType.Info);
                }
            }
        }



        private static async Task CreateUserDefinedFunction(
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
                    await FeedbackHelper.Channel.SendMessageAsync($"Error - No file found for the UDF {description.script}. Check you added a file with this name", MessageType.Info);
                }
                if (file != null)
                {
                    using (var r = new StreamReader(file.OpenReadStream()))
                    {
                        var js = await r.ReadToEndAsync();
                        if (String.IsNullOrWhiteSpace(js))
                        {
                            await FeedbackHelper.Channel.SendMessageAsync($"Error - We cannot read the content of the file {description.script}.", MessageType.Info);
                        }
                        else
                        {
                            await CreateOrPatchUserDefinedFunction(
                                memoryCache,
                                logger,
                                description.ToUserDefinedFunction(spaceId, matchers),
                                js,
                                spaceId,
                                matchers);
                        }
                    }
                }
                else
                {
                    await FeedbackHelper.Channel.SendMessageAsync($"Error - The file '{description.script}' is missing for this UDF, creation ignored.", MessageType.Info);
                }
            }
        }

        private static async Task<Space> CreateOrPatchSpaceAsync(IMemoryCache memoryCache, ILogger logger, Guid parentId, Space spaceToCreateOrPatch)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var existingSpace = await Api.FindSpace(httpClient, logger, spaceToCreateOrPatch.Name, parentId);

            if (existingSpace != null)
            {
                await FeedbackHelper.Channel.SendMessageAsync($"Updating the space {existingSpace.Id} that already exist.", MessageType.Info);

                existingSpace.FriendlyName = spaceToCreateOrPatch.FriendlyName;
                existingSpace.Type = spaceToCreateOrPatch.Type;
                existingSpace.SubType = spaceToCreateOrPatch.SubType;
                await UpdateSpaceAsync(existingSpace, memoryCache, logger, false);

                return existingSpace;
            }

            return await CreateSpaceAsync(spaceToCreateOrPatch, memoryCache, logger, false);
        }

        private static async Task<Device> CreateOrPatchDeviceAsync(IMemoryCache memoryCache, ILogger logger, Guid spaceId, Device deviceToCreateOrPatch)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var existingDevice = await Api.FindElementAsync<Device>(httpClient, logger, $"hardwareIds={deviceToCreateOrPatch.HardwareId}", null, "types");

            if (existingDevice != null)
            {
                await FeedbackHelper.Channel.SendMessageAsync($"Updating the device {existingDevice.Id} that already exist.", MessageType.Info);

                existingDevice.SpaceId = deviceToCreateOrPatch.SpaceId;
                existingDevice.Name = deviceToCreateOrPatch.Name;
                existingDevice.Type = deviceToCreateOrPatch.Type;
                existingDevice.SubType = deviceToCreateOrPatch.SubType;
                await UpdateDeviceAsync(existingDevice, memoryCache, logger, false);

                return existingDevice;
            }

            return await CreateDeviceAsync(deviceToCreateOrPatch, memoryCache, logger, false);
        }

        private static async Task<Guid> CreateOrPatchSensorAsync(IMemoryCache memoryCache, ILogger logger, Guid deviceId, Sensor sensorToCreateOrPatch)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var existingSensor = await Api.FindElementAsync<Sensor>(httpClient, logger, $"hardwareIds={sensorToCreateOrPatch.HardwareId}", $"deviceId={deviceId}", "types");

            if (existingSensor != null)
            {
                await FeedbackHelper.Channel.SendMessageAsync($"Updating the sensor {existingSensor.Id} that already exist.", MessageType.Info);

                existingSensor.DeviceId = sensorToCreateOrPatch.DeviceId;
                existingSensor.Type = sensorToCreateOrPatch.Type;
                existingSensor.DataType = sensorToCreateOrPatch.DataType;
                existingSensor.DataSubtype = sensorToCreateOrPatch.DataSubtype;
                existingSensor.DataUnitType = sensorToCreateOrPatch.DataUnitType;
                await UpdateSensorAsync(existingSensor, memoryCache, logger, false);

                return existingSensor.Id;
            }

            return await CreateSensorAsync(sensorToCreateOrPatch, memoryCache, logger, false);
        }

        private static async Task<Guid> CreateOrPatchMatcherAsync(IMemoryCache memoryCache, ILogger logger, Guid spaceId, Matcher matcherToCreateOrPatch)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var existingMatcher = await Api.FindElementAsync<Matcher>(httpClient, logger, $"names={matcherToCreateOrPatch.Name}", $"spaceId={spaceId}", "conditions");

            if (existingMatcher != null)
            {
                await FeedbackHelper.Channel.SendMessageAsync($"Updating the matcher {existingMatcher.Id} that already exist.", MessageType.Info);

                foreach (var condition in matcherToCreateOrPatch.Conditions)
                {
                    var existingCondition = existingMatcher.Conditions.FirstOrDefault(c => c.Target.Equals(condition.Target));
                    if (existingCondition != null)
                    {
                        condition.Id = existingCondition.Id;
                    }
                }

                existingMatcher.Conditions = matcherToCreateOrPatch.Conditions;
                await UpdateMatcherAsync(existingMatcher, memoryCache, logger);
                return existingMatcher.Id;
            }

            return await CreateMatcherAsync(matcherToCreateOrPatch, memoryCache, logger);
        }

        private static async Task CreateOrPatchUserDefinedFunction(
            IMemoryCache memoryCache,
            ILogger logger,
            UserDefinedFunction userDefinedFunction,
            string js,
            Guid spaceId,
            IEnumerable<Matcher> matchers)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var existingUserDefinedFunction = await Api.FindElementAsync<UserDefinedFunction>(httpClient, logger, $"names={userDefinedFunction.Name}", $"spaceId={spaceId}", "matchers");
            //var userDefinedFunction = await Api.FindUserDefinedFunction(httpClient, logger, description.name, spaceId);

            if (existingUserDefinedFunction != null)
            {
                await FeedbackHelper.Channel.SendMessageAsync($"Updating the UserDefinedFunction {existingUserDefinedFunction.Id} that already exist.", MessageType.Info);

                existingUserDefinedFunction.Name = userDefinedFunction.Name;
                               
                await Api.UpdateUserDefinedFunctionAsync(
                    memoryCache,
                    logger,
                    existingUserDefinedFunction,
                    js);
            }
            else
            {
                await Api.CreateUserDefinedFunctionAsync(
                    httpClient,
                    logger,
                    userDefinedFunction,
                    js);
            }
        }
        #endregion

        #region Space management
        public static async Task<IEnumerable<Space>> SearchSpacesAsync(IMemoryCache memoryCache, ILogger logger, String searchString, int typeId = -1)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            return await Api.FindSpaces(httpClient, logger, searchString, typeId);
        }


        public static async Task<IEnumerable<Space>> GetSpacesAsync(IMemoryCache memoryCache, ILogger logger)
        {
            return await GetSpacesAsync(memoryCache, logger, false);
        }

        private static async Task<IEnumerable<Space>> GetSpacesAsync(IMemoryCache memoryCache, ILogger logger, bool bypassCache)
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
                    maxNumberToGet: 1000, includes: "types,values,properties");

                await RefreshCacheAsync(memoryCache, spaces, Guid.Empty, false, Context.Space).ConfigureAwait(false);
            }

            logger.LogInformation($"GetSpacesAsync: {JsonConvert.SerializeObject(spaces, Formatting.Indented)}");
            return spaces;
        }

        public static async Task<IEnumerable<Space>> GetRootSpacesAsync(IMemoryCache memoryCache, ILogger logger, bool bypassCache)
        {
            IEnumerable<Space> spaces = null;
            if (!bypassCache)
            {
                spaces = CacheHelper.GetRootSpaceListFromCache(memoryCache);
            }
            if (spaces == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

                spaces = await Api.GetSpaces(
                    httpClient, logger,
                    navigation: "maxLevel=1",
                    includes: "types");

                await RefreshCacheAsync(memoryCache, spaces, Guid.Empty, false, Context.RootSpaces).ConfigureAwait(false);
            }

            logger.LogInformation($"GetRootSpacesAsync: {JsonConvert.SerializeObject(spaces, Formatting.Indented)}");
            return spaces;
        }

        public static async Task<Space> GetSpaceAsync(Guid spaceId, IMemoryCache memoryCache, ILogger logger)
        {
            return await GetSpaceAsync(spaceId, memoryCache, logger, false);
        }

        public static async Task<Guid> CreateOrUpdateBlob(
            ParentType blobType,
            BlobContent blobContent,
            IFormFile file,
            IMemoryCache memoryCache,
            ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            var blobs = await GetBlobsAsync(blobContent.ParentId, memoryCache, logger, false);
            var blob = blobs.FirstOrDefault(b => b.Name.Equals(blobContent.Name));
            
            if (blob!=null && blob.Id!=null)
            {
                return blob.Id;
            }

            var id = await Api.CreateBlobAsync(httpClient, logger, blobType, blobContent, file);
            CacheHelper.DeleteFromCache(memoryCache, blobContent.ParentId, Context.Blob);
            return id;
        }

        public static async Task DeleteBlobAsync(BlobContent blob, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            await Api.DeleteBlobAsync(httpClient, logger, blob.ParentType, blob.Id);
            CacheHelper.DeleteFromCache(memoryCache, blob.ParentId, Context.Blob);

            logger.LogInformation($"DeleteBlob: {blob.Id}");
        }

        public static async Task<IEnumerable<BlobContent>> GetBlobsAsync(Guid spaceId, IMemoryCache memoryCache, ILogger logger, bool loadLatestContent)
        {
            IEnumerable<BlobContent> blobs = CacheHelper.GetBlobContentsFromCache(memoryCache, spaceId);

            if (blobs == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
                blobs = await Api.GetBlobsAsync(httpClient, logger, spaceId, includes: "contentInfo,description,types");
                await RefreshCacheAsync(memoryCache, blobs, spaceId, true, Context.Blob).ConfigureAwait(false);

                if (loadLatestContent)
                {
                    foreach (var blob in blobs)
                    {
                        blob.ContentInfos[0].FilePath = await GetBlobContentAsync(blob.Id, memoryCache, logger, false);
                    }
                }
            }

            logger.LogInformation($"GetSpacesAsync: {JsonConvert.SerializeObject(blobs, Formatting.Indented)}");
            return blobs;
        }

        public static async Task<BlobContent> GetBlobAsync(Guid blobId, IMemoryCache memoryCache, ILogger logger, bool loadLatestContent)
        {
            BlobContent blob = CacheHelper.GetBlobContentFromCache(memoryCache, blobId);

            if (blob == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
                blob = await Api.GetBlobAsync(httpClient, logger, blobId, includes: "contentInfo,description,types");
                await RefreshCacheAsync(memoryCache, blob, blobId, true, Context.Blob).ConfigureAwait(false);

                if (loadLatestContent)
                {
                    blob.ContentInfos[0].FilePath = await GetBlobContentAsync(blob.Id, memoryCache, logger, false);
                }
            }

            logger.LogInformation($"GetSpacesAsync: {JsonConvert.SerializeObject(blob, Formatting.Indented)}");
            return blob;
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        public static async Task<String> GetBlobContentAsync(Guid blobId, IMemoryCache memoryCache, ILogger logger, bool loadIfAlreadyExist)
        {
            string fileName = $"/blobs/{blobId}.jpg";

            if (!File.Exists($"wwwroot{fileName}") || loadIfAlreadyExist)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
                var content = await Api.GetLatestBlobContent(httpClient, logger, blobId);

                if (content != null)
                {
                    using (FileStream file = File.Create($"wwwroot{fileName}"))
                    {
                        CopyStream(content, file);
                    }
                }
            }
            return fileName;
        }


        public static async Task<Space> GetSpaceAsync(Guid spaceId, IMemoryCache memoryCache, ILogger logger, bool bypassCache)
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
                    spaceId, includes: "types,values,parent,parenttypes,childspaces,childspacestypes,properties,devices,sensors,users");

                // Add the Space element in cache & refresh Space list in cache
                await RefreshCacheAsync(memoryCache, space, spaceId, false, Context.Space).ConfigureAwait(false);
            }

            logger.LogInformation($"GetSpaceAsync: {spaceId}");
            return space;
        }

        public static async Task<Space> CreateSpaceAsync(Space space, IMemoryCache memoryCache, ILogger logger, bool refreshInCache = true)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            space.Id = await Api.CreateAsync<Space>(httpClient, logger, space);

            if (refreshInCache)
            {
                await RefreshCacheAsync(memoryCache, space, space.Id, true, Context.Space).ConfigureAwait(false);
            }
            logger.LogInformation($"CreateSpaceAsync: {space.Id}");
            return space;
        }

        public static async Task UpdateSpaceAsync(Space space, IMemoryCache memoryCache, ILogger logger, bool refreshInCache = true)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (await Api.UpdateAsync<Space>(memoryCache, logger, space, refreshInCache) && refreshInCache)
            {
                await RefreshCacheAsync(memoryCache, space, space.Id, true, Context.Space).ConfigureAwait(false);
            }
            logger.LogInformation($"UpdateSpaceAsync: {space.Id}");
        }

        public static async Task UpdateSpacePropertiesAsync(Space space, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (await Api.UpdatePropertiesAsync<Space>(memoryCache, logger, space))
            {
                //using (var updatedSpace = await GetSpaceAsync(space.Id, memoryCache, logger, true).ConfigureAwait(false))
                //{
                //    await RefreshCacheAsync(memoryCache, space, space.Id, true, Context.Space);
                //}
                var updatedSpace = await GetSpaceAsync(space.Id, memoryCache, logger, true);
                await RefreshCacheAsync(memoryCache, updatedSpace, updatedSpace.Id, true, Context.Space);
                logger.LogInformation($"UpdateSpacePropertiesAsync: {space.Id}");
            }
        }

        public static async Task<bool> DeleteSpaceAsync(Space space, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (await Api.DeleteAsync<Space>(httpClient, logger, space))
            {
                await RemoveFromCacheAsync(memoryCache, space, space.Id).ConfigureAwait(false);
                logger.LogInformation($"DeleteSpaceAsync: {space.Id}");
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion region


        public static async Task<IEnumerable<Device>> GetDevicesAsync(IMemoryCache memoryCache, ILogger logger)
        {
            return await GetDevicesAsync(memoryCache, logger, false);
        }

        private static async Task<IEnumerable<Device>> GetDevicesAsync(IMemoryCache memoryCache, ILogger logger, bool bypassCache)
        {
            IEnumerable<Device> devices = null;

            if (!bypassCache)
            {
                devices = CacheHelper.GetDeviceListFromCache(memoryCache);
            }

            if (devices == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

                devices = await Api.GetDevices(
                    httpClient, logger,
                    maxNumberToGet: 100, includes: "sensors");

                await RefreshCacheAsync(memoryCache, devices, Guid.Empty, false, Context.Device).ConfigureAwait(false);
            }

            logger.LogInformation($"GetDevices: {JsonConvert.SerializeObject(devices, Formatting.Indented)}");
            return devices;
        }


        public static async Task<Device> GetDeviceAsync(Guid deviceId, IMemoryCache memoryCache, ILogger logger, bool bypassCache)
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
                    deviceId, includes: "sensors,sensorstypes,sensorsvalues,ConnectionString,properties,space,spacetypes");

                await RefreshCacheAsync(memoryCache, device, deviceId, false, Context.Device).ConfigureAwait(false);
            }

            logger.LogInformation($"GetDevice: {device}");
            return device;
        }

        public static async Task<Device> CreateDeviceAsync(Device device, IMemoryCache memoryCache, ILogger logger, bool refreshInCache = true)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var deviceId = await Api.CreateAsync<Device>(httpClient, logger, device);

            device = await Api.GetDevice(httpClient, logger, deviceId, includes: "sensors, sensorsvalues, ConnectionString");
            if (refreshInCache)
            {
                await RefreshCacheAsync(memoryCache, device, deviceId, true, Context.Device).ConfigureAwait(false);
            }

            logger.LogInformation($"CreateDevice: {deviceId}");
            return device;
        }

        public static async Task UpdateDeviceAsync(Device device, IMemoryCache memoryCache, ILogger logger, bool refreshInCache = true)
        {
            if (await Api.UpdateAsync<Device>(memoryCache, logger, device, refreshInCache) && refreshInCache)
            {
                await RefreshCacheAsync(memoryCache, device, device.Id, true, Context.Device).ConfigureAwait(false);
            }
            logger.LogInformation($"UpdateDevice: {device.Id}");
        }

        public static async Task DeleteDeviceAsync(Device device, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            await Api.DeleteAsync<Device>(httpClient, logger, device);
            await RemoveFromCacheAsync(memoryCache, device, device.Id).ConfigureAwait(false);

            logger.LogInformation($"DeleteDevice: {device.Id}");
        }

        public static async Task<Sensor> GetSensorAsync(Guid id, IMemoryCache memoryCache, ILogger logger, bool bypassCache)
        {
            Sensor sensor = null;
            if (!bypassCache)
            {
                sensor = CacheHelper.GetSensorFromCache(memoryCache, id);
            }

            if (sensor == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
                sensor = await Api.GetSensor(httpClient, logger, id, includes: "value,properties,space,spacetypes,types,device,devicetypes");
                await RefreshCacheAsync(memoryCache, sensor, id, false, Context.Sensor).ConfigureAwait(false);
            }

            logger.LogInformation($"GetSensor: {id}");
            return sensor;
        }

        public static async Task<Guid> CreateSensorAsync(Sensor sensor, IMemoryCache memoryCache, ILogger logger, bool refreshInCache = true)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var sensorId = await Api.CreateAsync<Sensor>(httpClient, logger, sensor);

            sensor = await Api.GetSensor(httpClient, logger, sensorId, includes: "value");
            if (refreshInCache)
            {
                await RefreshCacheAsync(memoryCache, sensor, sensorId, true, Context.Sensor).ConfigureAwait(false);
            }

            logger.LogInformation($"CreateSensor: {sensorId}");
            return sensorId;
        }
        public static async Task UpdateSensorAsync(Sensor sensor, IMemoryCache memoryCache, ILogger logger, bool refreshInCache = true)
        {
            if (await Api.UpdateAsync<Sensor>(memoryCache, logger, sensor, refreshInCache) && refreshInCache)
            {
                await RefreshCacheAsync(memoryCache, sensor, sensor.Id, true, Context.Device).ConfigureAwait(false);
            }

            logger.LogInformation($"UpdateSensor: {sensor.Id}");
        }

        public static async Task DeleteSensorAsync(Sensor sensor, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            await Api.DeleteAsync<Sensor>(httpClient, logger, sensor);
            await RemoveFromCacheAsync(memoryCache, sensor, sensor.Id).ConfigureAwait(false);

            logger.LogInformation($"DeleteSensor: {sensor.Id}");
        }


        #region Matcher & UDF management
        public static async Task<IEnumerable<Matcher>> GetMatchersBySpaceId(Guid spaceId, IMemoryCache memoryCache, ILogger logger)
        {
            IEnumerable<Matcher> matchers = CacheHelper.GetMatchersFromCache(memoryCache, Context.Matcher, spaceId);

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

        public static async Task<Guid> CreateMatcherAsync(Matcher matcher, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var matcherId = await Api.CreateAsync<Matcher>(httpClient, logger, matcher);
            matcher.Id = matcherId;

            await RefreshCacheAsync(memoryCache, matcher, matcherId, true, Context.Matcher).ConfigureAwait(false);
            logger.LogInformation($"CreateMatcher: {matcherId}");
            return matcherId;
        }

        public static async Task UpdateMatcherAsync(Matcher matcher, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (await Api.UpdateAsync<Matcher>(memoryCache, logger, matcher))
            {
                await RefreshCacheAsync(memoryCache, matcher, matcher.Id, true, Context.Matcher).ConfigureAwait(false);
                logger.LogInformation($"UpdateMatcher: {matcher.Id}");
            }
        }

        public static async Task<bool> DeleteMatcherAsync(Matcher matcher, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            if (await Api.DeleteAsync<Matcher>(httpClient, logger, matcher))
            {
                await RemoveFromCacheAsync(memoryCache, matcher, matcher.Id).ConfigureAwait(false);
                logger.LogInformation($"DeleteMatcher: {matcher.Id}");
                return true;
            }
            else
            {
                return false;
            }
        }

        public static async Task<IEnumerable<UserDefinedFunction>> GetUDFsBySpaceId(Guid spaceId, IMemoryCache memoryCache, ILogger logger)
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

        public static async Task<UserDefinedFunction> GetUserDefinedFunction(Guid functionId, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

            var userDefinedFunction = await Api.GetUserDefinedFunction(httpClient, logger, functionId, includes: "matchers");

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
        
        public static async Task UpdateUserDefinedFunction(UserDefinedFunction userDefinedFunction, string content, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            await Api.UpdateUserDefinedFunctionAsync(
                memoryCache,
                logger,
                userDefinedFunction,
                content);

            logger.LogInformation($"UpdateUserDefinedFunction: {userDefinedFunction.Id}");
        }
                                 
        #endregion

        public static async Task<IEnumerable<Models.Type>> GetTypesAsync(Types listType, IMemoryCache memoryCache, ILogger logger)
        {
            IEnumerable<Models.Type> types = CacheHelper.GetTypeListFromCache(listType, memoryCache);

            if (types == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

                types = await Api.GetTypes(
                    httpClient, logger,
                    maxNumberToGet: 1000, categories: listType.ToString());

                logger.LogInformation($"GetSpaceTypes: {JsonConvert.SerializeObject(types, Formatting.Indented)}");

                await RefreshCacheAsync(memoryCache, types, listType, false, Context.None).ConfigureAwait(false);
            }

            return types;
        }

        public static async Task<List<Ontology>> GetOntologiesWithTypes(IMemoryCache memoryCache, ILogger logger)
        {
            var cache = CacheHelper.GetOntologyListFromCache(memoryCache);
            List<Ontology> ontologies;

            if (cache == null)
            {
                ontologies = new List<Ontology>();
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);

                var _ontologies = await Api.GetOntologies(httpClient, logger);
                for (int i = 0; i < _ontologies.Count(); i++)
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

        public static async Task<IEnumerable<PropertyKey>> GetPropertyKeysForSpace(Guid spaceId, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var propertyKeys = await Api.FindPropertyKeys(spaceId, httpClient, logger, Scope.Spaces);
            logger.LogInformation($"GetPropertyKeysForSpace: {JsonConvert.SerializeObject(propertyKeys, Formatting.Indented)}");
            return propertyKeys;
        }

        public static async Task<PropertyKey> GetPropertyKeyAsync(string id, IMemoryCache memoryCache, ILogger logger, bool bypassCache)
        {
            PropertyKey propertyKey = null;
            if (!bypassCache)
            {
                propertyKey = CacheHelper.GetPropertyKeyFromCache(memoryCache, id);
            }

            if (propertyKey == null)
            {
                var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
                propertyKey = await Api.GetAsync<PropertyKey>(httpClient, logger, id, includes: "description");
                await RefreshCacheAsync(memoryCache, propertyKey, id, false, Context.PropertyKey).ConfigureAwait(false);
            }

            logger.LogInformation($"GetPropertyKey: {id}");
            return propertyKey;
        }

        public static async Task<int> CreatePropertyKeyAsync(PropertyKey PropertyKey, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            var propertyKeyId = await Api.CreatePropertyKeyAsync(httpClient, logger, PropertyKey);

            PropertyKey = await Api.GetAsync<PropertyKey>(httpClient, logger, propertyKeyId, includes: "description");
            if (propertyKeyId != 0)
            {
                await RefreshCacheAsync(memoryCache, PropertyKey, propertyKeyId, true, Context.PropertyKey).ConfigureAwait(false);
            }

            logger.LogInformation($"CreatePropertyKey: {propertyKeyId}");
            return propertyKeyId;
        }

        public static async Task UpdatePropertyKeyAsync(PropertyKey PropertyKey, IMemoryCache memoryCache, ILogger logger)
        {
            await Api.UpdateAsync<PropertyKey>(memoryCache, logger, PropertyKey);
            await RefreshCacheAsync(memoryCache, PropertyKey, PropertyKey.Id, true, Context.Device).ConfigureAwait(false);

            logger.LogInformation($"UpdatePropertyKey: {PropertyKey.Id}");
        }

        public static async Task DeletePropertyKeyAsync(PropertyKey PropertyKey, IMemoryCache memoryCache, ILogger logger)
        {
            var httpClient = await CacheHelper.GetHttpClientFromCacheAsync(memoryCache, logger);
            await Api.DeleteAsync<PropertyKey>(httpClient, logger, PropertyKey);
            await RemoveFromCacheAsync(memoryCache, PropertyKey, PropertyKey.Id).ConfigureAwait(false);

            logger.LogInformation($"DeletePropertyKey: {PropertyKey.Id}");
        }
    }
}
