using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;

namespace DigitalTwinsBackend.Helpers
{
    public class AuthenticationHelper
    {
        //TODO supprimer les contantes ci-dessous
        //static readonly string AADInstance = "https://login.microsoftonline.com/";
        //static readonly string ClientId = "f7400bff-2504-4e5f-8587-eda134e5a70d";
        //static readonly string Resource = "0b07f429-9f4b-4714-9392-cc5e8e80c8b0";
        //static readonly string Tenant = "72f988bf-86f1-41af-91ab-2d7cd011db47";
        //static readonly string BaseUrl = "https://sh360iot-digitaltwins-o3e7wjxah3yqg.northeurope.azuresmartspaces.net/management/api/v1.0/";

        //static string Authority => AADInstance + Tenant;

        //public async Task<HttpClient> SetupHttpClient(ILogger logger)
        //{
        //    var httpClient = new HttpClient()
        //    {
        //        BaseAddress = new Uri(BaseUrl),
        //    };
        //    var accessToken = (await AuthenticationHelper.GetToken(logger, BaseUrl, Authority, Resource, ClientId));

        //    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
        //    return httpClient;
        //}

        //private HttpClient httpClient;

        public static async Task<HttpClient> SetupHttpClient(ILogger logger)
        {
            //if (httpClient != null)
            //{
            //    return httpClient;
            //}
            //else
            //{
            HttpClient httpClient;

                string authority = ConfigHelper.Config.parameters.AADInstance + ConfigHelper.Config.parameters.Tenant;

                httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(ConfigHelper.Config.parameters.BaseUrl),
                };
                var accessToken = (await AuthenticationHelper.GetToken(
                    logger,
                    ConfigHelper.Config.parameters.BaseUrl,
                    authority,
                    ConfigHelper.Config.parameters.Resource,
                    ConfigHelper.Config.parameters.ClientId));

                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                return httpClient;
        }

        // Gets an access token
        // First tries (by making a request) using a cached token and if that
        // fails we generated a new one using device login and cache it.
        internal static async Task<string> GetToken(ILogger logger, string baseUrl, string authority, string resource, string clientId)
        {
            var accessTokenFilename = ".accesstoken";
            var accessToken = ReadAccessTokenFromFile(accessTokenFilename);
            if (accessToken == null || !(await TryRequestWithAccessToken(new Uri(baseUrl), accessToken)))
            {
                accessToken = await AuthenticationHelper.GetNewToken(logger, authority, resource, clientId);
                System.IO.File.WriteAllText(accessTokenFilename, accessToken);
            }

            return accessToken;
        }

        private static async Task<bool> TryRequestWithAccessToken(Uri baseAddress, string accessToken)
        {
            // We create a new httpClient so we can force console logging for this operation
            //var httpClient = new HttpClient(new LoggingHttpHandler(Loggers.ConsoleLogger))
            var httpClient = new HttpClient()
            {
                BaseAddress = baseAddress,
            };
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            await FeedbackHelper.Channel.SendMessageAsync("Checking if previous access token is valid...");

            return (await httpClient.GetAsync("ontologies")).IsSuccessStatusCode;
        }

        private static string ReadAccessTokenFromFile(string filename)
            => System.IO.File.Exists(filename) ? System.IO.File.ReadAllText(filename) : null;

        private static async Task<string> GetNewToken(
            ILogger logger,
            string authority,
            string resource,
            string clientId)
        {
            var authContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(authority);
            return (await GetResultsUsingDeviceCode(authContext, resource, clientId)).AccessToken;
        }

        // This prompts the user to open a browser and input a unique key to authenticate their app
        // This allows dotnet core apps to authorize an application through user credentials without displaying UI.
        private static async Task<AuthenticationResult> GetResultsUsingDeviceCode(AuthenticationContext authContext, string resource, string clientId)
        {
            var codeResult = await authContext.AcquireDeviceCodeAsync(resource, clientId);

            await FeedbackHelper.Channel.SendMessageAsync(codeResult.Message);

            return await authContext.AcquireTokenByDeviceCodeAsync(codeResult);
        }
    }
}