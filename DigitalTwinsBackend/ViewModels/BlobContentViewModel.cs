using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;

using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Hubs;
using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Threading;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinsBackend.ViewModels
{
    public class BlobContentViewModel
    {
        private IMemoryCache _cache;
        private AuthenticationHelper _auth;
        private ParentType _parentType;
        public IFormFile File { get; set; }
        public BlobContent SelectedBlobContentItem { get; set; }
        public IEnumerable<Models.Type> BlobContentTypeList { get; set; }
        public IEnumerable<Models.Type> BlobContentSubTypeList { get; set; }

        public BlobContentViewModel() { }

        public BlobContentViewModel(ParentType blobType, IMemoryCache memoryCache, Guid? id = null)
        {
            _cache = memoryCache;
            _auth = new AuthenticationHelper();
            _parentType = blobType;

            try
            {
                LoadAsync(id).Wait();
            }
            catch (Exception ex)
            {
                FeedbackHelper.Channel.SendMessageAsync($"Error - {ex.Message}", MessageType.Info).Wait();
                FeedbackHelper.Channel.SendMessageAsync($"Please check your settings.", MessageType.Info).Wait();

                BlobContentTypeList = new List<Models.Type>();
                BlobContentSubTypeList = new List<Models.Type>();
            }
        }

        private async Task LoadAsync(Guid? id = null)
        {
            switch (_parentType)
            {
                case ParentType.Space:
                    {
                        BlobContentTypeList = await DigitalTwinsHelper.GetTypesAsync(
                            Models.Types.SpaceBlobType, _cache, Loggers.SilentLogger, onlyEnabled: true);
                        BlobContentSubTypeList = await DigitalTwinsHelper.GetTypesAsync(
                            Models.Types.SpaceBlobSubtype, _cache, Loggers.SilentLogger, onlyEnabled: true);
                        break;
                    }
                case ParentType.Device:
                    {
                        BlobContentTypeList = await DigitalTwinsHelper.GetTypesAsync(
                            Models.Types.DeviceBlobType, _cache, Loggers.SilentLogger, onlyEnabled: true);
                        BlobContentSubTypeList = await DigitalTwinsHelper.GetTypesAsync(
                            Models.Types.DeviceBlobSubtype, _cache, Loggers.SilentLogger, onlyEnabled: true);
                        break;
                    }
                case ParentType.Sensor:
                    {
                        BlobContentTypeList = await DigitalTwinsHelper.GetTypesAsync(
                            Models.Types.SensorDataType, _cache, Loggers.SilentLogger, onlyEnabled: true);
                        BlobContentSubTypeList = await DigitalTwinsHelper.GetTypesAsync(
                            Models.Types.DeviceBlobSubtype, _cache, Loggers.SilentLogger, onlyEnabled: true);
                        break;
                    }
            }
            
            if (id != null)
            {
                this.SelectedBlobContentItem = await DigitalTwinsHelper.GetBlobAsync((Guid)id, _cache, Loggers.SilentLogger, true);
            }
            else
            {
                SelectedBlobContentItem = new BlobContent();
            }
            this.SelectedBlobContentItem.ParentType = _parentType;
        }
    }
}
