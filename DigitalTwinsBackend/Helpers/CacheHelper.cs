using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace DigitalTwinsBackend.Helpers
{
    public static class CacheKeys
    {
        public static string Context { get { return "_Context"; } }
        public static string InfoMessages { get { return "_InfoMessages"; } }
        public static string APICallMessages { get { return "_APICallMessages"; } }
        public static string SimulatedSensorList { get { return "_SimulatedSensorList"; } }
        public static string OntologyList { get { return "_OntologyList"; } }
        public static string HttpClient { get { return "_AuthenticationToken"; } }
        public static string IsInSendingDataState { get { return "_IsInSendingDataState"; } }
    }

    public enum Context
    {
        None,
        Space,
        Device,
        Sensor,
        Matcher,
        UDF
    }

    public class CacheHelper
    {
        private static Context context;
        private static string id;

        private static string cacheKey
        {
            get
            {
                return (context == Context.None) ? id : id + "_" + context;
            }
        }

        #region Simulator related parameters & methods
        private static readonly string simulatorConfig = "Simulator.config";

        public static async Task<List<SimulatedSensor>> GetSimulatedSensorListFromCacheAsync(IMemoryCache memoryCache)
        {
            List<SimulatedSensor> list;

            if (!memoryCache.TryGetValue(CacheKeys.SimulatedSensorList, out list))
            {
                list = await LoadSimulatorConfigAsync();
                memoryCache.Set(CacheKeys.SimulatedSensorList, list);
            }
            return list;
        }

        public static async Task AddSimulatedSensorListInCacheAsync(IMemoryCache memoryCache, IEnumerable<SimulatedSensor> list)
        {
            memoryCache.Set(CacheKeys.SimulatedSensorList, list);
            await SaveSimulatorConfigAsync(list);
        }

        private static async Task<List<SimulatedSensor>> LoadSimulatorConfigAsync()
        {
            List<SimulatedSensor> list = new List<SimulatedSensor>();

            if (System.IO.File.Exists(simulatorConfig))
            {
                var content = await System.IO.File.ReadAllTextAsync(simulatorConfig);
                list = JsonConvert.DeserializeObject<List<SimulatedSensor>>(content);
            }
            return list;
        }

        private static async Task SaveSimulatorConfigAsync(IEnumerable<SimulatedSensor> list)
        {
            var listString = JsonConvert.SerializeObject(list);
            await System.IO.File.WriteAllTextAsync(simulatorConfig, listString);
        }

        public static bool IsInSendingDataState(IMemoryCache memoryCache)
        {
            bool isInSendingDataState;

            if (!memoryCache.TryGetValue(CacheKeys.IsInSendingDataState, out isInSendingDataState))
            {
                isInSendingDataState = false;
                memoryCache.Set(CacheKeys.IsInSendingDataState, isInSendingDataState);
            }
            return isInSendingDataState;
        }

        public static void SetInSendingDataState(IMemoryCache memoryCache, bool value)
        {
            memoryCache.Set(CacheKeys.IsInSendingDataState, value);
        }
        #endregion


        internal static void SetContext(IMemoryCache memoryCache, Context context)
        {
            memoryCache.Set(CacheKeys.Context, context);
        }

        internal static bool IsInSpaceEditMode(IMemoryCache memoryCache)
        {
            return IsInContext(memoryCache, Context.Space);
        }

        internal static bool IsInDeviceEditMode(IMemoryCache memoryCache)
        {
            return IsInContext(memoryCache, Context.Device);
        }

        private static bool IsInContext(IMemoryCache memoryCache, Context context)
        {
            bool isInContext = false;

            Object _context = null;
            memoryCache.TryGetValue(CacheKeys.Context, out _context);

            if (_context != null && _context.ToString().Equals(context.ToString()))
            {
                isInContext = true;
            }
            return isInContext;
        }

        internal static void AddInCache(IMemoryCache memoryCache, Object cacheElement, object key, Context scope)
        {
            context = scope;
            id = key.ToString();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(ConfigHelper.Config.parameters.CacheTime));
            //var cacheKey = (scope == Context.None) ? key : key + "_" + scope; 
            
            memoryCache.Set(cacheKey, cacheElement, cacheEntryOptions);
        }

        internal static void ResetMessagesInCache(IMemoryCache memoryCache)
        {
            DeleteFromCache(memoryCache, CacheKeys.APICallMessages, Context.None);
            DeleteFromCache(memoryCache, CacheKeys.InfoMessages, Context.None);
        }

        internal static void AddAPICallMessageInCache(IMemoryCache memoryCache, string message)
        {
            List<string> messages = (List<string>)GetFromCache(memoryCache, CacheKeys.APICallMessages, Context.None);
            if (messages == null)
                messages = new List<string>();

            messages.Add(message);
            AddInCache(memoryCache, messages, CacheKeys.APICallMessages, Context.None);
        }

        internal static List<string> GetAPICallMessagesFromCache(IMemoryCache memoryCache)
        {
            return (List<string>)GetFromCache(memoryCache, CacheKeys.APICallMessages, Context.None);
        }

        internal static void AddInfoMessageInCache(IMemoryCache memoryCache, string message)
        {
            List<string> messages = (List<string>)GetFromCache(memoryCache, CacheKeys.InfoMessages, Context.None);
            if (messages == null)
                messages = new List<string>();

            messages.Add(message);
            AddInCache(memoryCache, messages, CacheKeys.InfoMessages, Context.None);
        }

        internal static List<string> GetInfoMessagesFromCache(IMemoryCache memoryCache)
        {
            return (List<string>)GetFromCache(memoryCache, CacheKeys.InfoMessages, Context.None);
        }

        internal static Object GetFromCache(IMemoryCache memoryCache, object key, Context scope)
        {
            context = scope;
            id = key.ToString();
            //var cacheKey = (scope == Context.None) ? key : key + "_" + scope;

            Object cacheElement = null;
            memoryCache.TryGetValue(cacheKey, out cacheElement);

            return cacheElement;
        }

        internal static void DeleteFromCache(IMemoryCache memoryCache, object key, Context scope)
        {
            context = scope;
            id = key.ToString();
            //var cacheKey = (scope == Context.None) ? key : key + "_" + scope;
            memoryCache.Remove(cacheKey);
        }

        public static async Task<HttpClient> GetHttpClientFromCacheAsync(IMemoryCache memoryCache, ILogger logger)
        {
            HttpClient httpClient;

            if (!memoryCache.TryGetValue(CacheKeys.HttpClient, out httpClient))
            {
                httpClient = await AuthenticationHelper.SetupHttpClient(logger);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(ConfigHelper.Config.parameters.CacheTime));
                memoryCache.Set(CacheKeys.HttpClient, httpClient, cacheEntryOptions);
            }
            return httpClient;
        }

        public static IEnumerable<Space> GetSpaceListFromCache(IMemoryCache memoryCache)
        {
            var list = GetFromCache(memoryCache, Guid.Empty, Context.Space);

            if (list != null)
                return (IEnumerable<Space>)list;
            else
                return null;
        }

        public static IEnumerable<Device> GetDeviceListFromCache(IMemoryCache memoryCache)
        {
            var list = GetFromCache(memoryCache, Guid.Empty, Context.Device);

            if (list != null)
                return (IEnumerable<Device>)list;
            else
                return null;
        }

        public static Space GetSpaceFromCache(IMemoryCache memoryCache, Guid id)
        {
            var space = GetFromCache(memoryCache, id, Context.Space);

            if (space != null)
                return (Space)space;
            else
                return null;
        }

        public static IEnumerable<Matcher> GetMatchersFromCache(IMemoryCache memoryCache, Context context, Guid id)
        {
            var matchers = GetFromCache(memoryCache, id, context);

            if (matchers != null)
                return (IEnumerable<Matcher>)matchers;
            else
                return null;
        }
               
        public static Matcher GetMatcherFromCache(IMemoryCache memoryCache, Guid id)
        {
            var matcher = GetFromCache(memoryCache, id, Context.Matcher);

            if (matcher != null)
                return (Matcher)matcher;
            else
                return null;
        }

        public static IEnumerable<UserDefinedFunction> GetUDFsFromCache(IMemoryCache memoryCache, Context context, Guid id)
        {
            var udfs = GetFromCache(memoryCache, id, context);

            if (udfs != null)
                return (IEnumerable<UserDefinedFunction>)udfs;
            else
                return null;
        }

        public static UserDefinedFunction GetUDFFromCache(IMemoryCache memoryCache, Guid id)
        {
            var udf = GetFromCache(memoryCache, id, Context.UDF);

            if (udf != null)
                return (UserDefinedFunction)udf;
            else
                return null;
        }

        public static Device GetDeviceFromCache(IMemoryCache memoryCache, Guid id)
        {
            var device = GetFromCache(memoryCache, id, Context.Device);

            if (device != null)
                return (Device)device;
            else
                return null;
        }

        public static Sensor GetSensorFromCache(IMemoryCache memoryCache, Guid id)
        {
            var sensor = GetFromCache(memoryCache, id, Context.Sensor);

            if (sensor != null)
                return (Sensor)sensor;
            else
                return null;
        }

        public static IEnumerable<Models.Type> GetTypeListFromCache(Models.Types listType, IMemoryCache memoryCache)
        {
            var types = GetFromCache(memoryCache, listType, Context.None);

            if (types != null)
                return (IEnumerable<Models.Type>)types;
            else
                return null;
        }

        public static IEnumerable<Ontology> GetOntologyListFromCache(IMemoryCache memoryCache)
        {
            var ontologies = GetFromCache(memoryCache, CacheKeys.OntologyList, Context.None);

            if (ontologies != null)
                return (IEnumerable<Ontology>)ontologies;
            else
                return null;
        }
    }
}
