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
        [Display(Name = "Azure AD Login service - You should not modify this parameter")]
        public string AADInstance { get; set; }
        [Display(Name = "Azure AD Client Id")]
        public string ClientId { get; set; }
        [Display(Name = "Azure Digital Twins Resource Id - You should not modify this parameter")]
        public string Resource { get; set; }
        [Display(Name = "Azure Tenant")]
        public string Tenant { get; set; }
        [Display(Name = "Base Url (to your Digital Twins instance)")]
        public string BaseUrl { get; set; }
        [Display(Name = "[Cache] Maximum validity (in seconds) of business objects stored in cache")]
        [Range(30, 1800, ErrorMessage = "Time must be between 30 and 1800")]
        public int CacheTime { get; set; }
        [Display(Name = "[Device Simulator] Time (in seconds) between two data sending")]
        [Range(1, 300, ErrorMessage = "Time must be between 1 and 300")]
        public int SimulatorTimer { get; set; }
        [Display(Name = "Trace and Display all API Calls")]
        public bool EnableAPICallTrace { get; set; }
        [Display(Name = "Display details about data exchange with Azure Digital Twins")]
        public bool EnableVerboseMode { get; set; }
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
                parameters.AADInstance = "https://login.microsoftonline.com/";
                parameters.ClientId = "N/A";
                parameters.Resource = "0b07f429-9f4b-4714-9392-cc5e8e80c8b0";
                parameters.Tenant = "N/A";
                parameters.BaseUrl = "https://<DigitalTwinsName>.<Location>.azuresmartspaces.net";
                parameters.CacheTime = 60;
                parameters.CacheTime = 5;
                parameters.EnableAPICallTrace = false;
                parameters.EnableVerboseMode = false;
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
