using DigitalTwinsBackend.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace DigitalTwinsBackend.Hubs
{
    public class MessageHub : Hub
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IMemoryCache _cache;

        public MessageHub(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = memoryCache;
        }

        public async Task SendMessage(string message)
        {
            CacheHelper.AddMessageInCache(_cache, message);
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
