// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DigitalTwinsBackend.Models;
using DigitalTwinsBackend.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DigitalTwinsBackend.Helpers
{
    public partial class Api
    {
        public static async Task<bool> DeleteAsync<T>(HttpClient httpClient, ILogger logger, T element) where T : BaseModel
        {
            logger.LogInformation($"Deleting {typeof(T).Name} with Id: {element.Id}");
            var response = await httpClient.DeleteAsync($"{typeof(T).Name.ToLower()}s/{element.Id}");

            await FeedbackHelper.Channel.SendMessageAsync($"{element.GetType().Name} '{element.Label}' successfully deleted", MessageType.Info);

            return await IsSuccessCall(response, logger);
        }

        public static async Task<bool> DeleteBlobAsync(HttpClient httpClient, ILogger logger, ParentType blobType, Guid id)
        {
            logger.LogInformation($"Deleting Blob with Id: {id}");
            var response = await httpClient.DeleteAsync($"{blobType}s/blobs/{id}");

            return await IsSuccessCall(response, logger);
        }
    }
}