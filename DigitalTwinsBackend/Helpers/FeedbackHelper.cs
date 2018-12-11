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
        //public static FeedbackHelper Channel = new FeedbackHelper();

        private IHttpContextAccessor _httpContextAccessor;
        private static readonly Lazy<FeedbackHelper> lazy = new Lazy<FeedbackHelper>(() => new FeedbackHelper());
        public static FeedbackHelper Channel { get { return lazy.Value; } }

        public void SetHttpContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SendMessageAsync(string message)
        {
            if (_httpContextAccessor != null)
            {
                var url = "https://" + _httpContextAccessor.HttpContext.Request.Host + "/MessageHub";

                HubConnection connection = new HubConnectionBuilder().WithUrl(url).Build();
                await connection.StartAsync();
                await connection.InvokeAsync("SendMessage", message);
            }
        }
    }
}
