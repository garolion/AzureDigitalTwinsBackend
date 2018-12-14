using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Hubs;
using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DigitalTwinsBackend.Helpers
{
    public static class Loggers
    {
        public static ILogger SilentLogger =
            new Microsoft.Extensions.Logging.LoggerFactory()
                .CreateLogger("DigitalTwinsQuickstart");
        public static ILogger ConsoleLogger =
            new Microsoft.Extensions.Logging.LoggerFactory()
                .AddConsole(LogLevel.Trace)
                .CreateLogger("DigitalTwinsQuickstart");
    }

    public sealed class FeedbackHelper
    {
        private IMemoryCache _cache;
        private IHttpContextAccessor _httpContextAccessor;
        private static readonly Lazy<FeedbackHelper> lazy = new Lazy<FeedbackHelper>(() => new FeedbackHelper());
        public static FeedbackHelper Channel { get { return lazy.Value; } }

        public void SetHttpContextAccessor(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SendMessageAsync(string message, MessageType messageType)
        {
            if (_httpContextAccessor != null)
            {
                var url = "https://" + _httpContextAccessor.HttpContext.Request.Host + "/MessageHub";

                HubConnection connection = new HubConnectionBuilder().WithUrl(url).Build();
                await connection.StartAsync();
                await connection.InvokeAsync("SendMessage", message);

                switch (messageType)
                {
                    case MessageType.APICall:
                        {
                            CacheHelper.AddAPICallMessageInCache(_cache, message);
                            break;
                        }
                    case MessageType.Info:
                        {
                            CacheHelper.AddInfoMessageInCache(_cache, message);
                            break;
                        }
                }
            }
        }
    }
           
    public enum MessageType
    {
        Info,
        APICall
    }
}
