using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinsBackend.Helpers
{
    public class AppConfig
    {
        [Display(Name = "AAD Instance")]
        public string AADInstance { get; set; }
        [Display(Name = "Client Id")]
        public string ClientId { get; set; }
        public string Resource { get; set; }
        [Display(Name = "Azure Tenant")]
        public string Tenant { get; set; }
        [Display(Name = "Base Url (to your Digital Twins instance)")]
        public string BaseUrl { get; set; }
        [Display(Name = "[Cache] Maximum validity (in seconds) of business objects stored in cache")]
        [Range(3, 300, ErrorMessage = "Time must be between 3 and 300")]
        public int CacheTime { get; set; }
        [Display(Name = "[Device Simulator] Time (in seconds) between two data sending")]
        [Range(1, 180, ErrorMessage = "Time must be between 1 and 180")]
        public int SimulatorTimer { get; set; }
        [Display(Name = "[Device Simulator] Connection String to the device on Azure Iot Hub used to send data")]
        public string DeviceConnectionString { get; set; }
    }

    public class ConfigHelper
    {
        private static readonly string configFilename = "backend.config";
        public AppConfig parameters = new AppConfig();

        private static readonly Lazy<ConfigHelper> lazy = new Lazy<ConfigHelper>(() => new ConfigHelper());
        public static ConfigHelper Config { get { return lazy.Value; } }

        public ConfigHelper()
        {
            var configReader = LoadConfig(configFilename);

            if (configReader != null && configReader.Length > 2)
            {
                parameters = JsonConvert.DeserializeObject<AppConfig>(configReader);
            }
            else
            {
                parameters.AADInstance = "N/A";
                parameters.ClientId = "N/A";
                parameters.Resource = "N/A";
                parameters.Tenant = "N/A";
                parameters.BaseUrl = "N/A";
                parameters.CacheTime = 30;
                parameters.CacheTime = 5;
                parameters.DeviceConnectionString = "";
            }
        }

        private static string LoadConfig(string filename)
            => System.IO.File.Exists(filename) ? System.IO.File.ReadAllText(filename) : null;

        public static void SaveConfig()
        {
            var configString = JsonConvert.SerializeObject(Config.parameters);
            System.IO.File.WriteAllText(configFilename, configString);
        }
    }
}
