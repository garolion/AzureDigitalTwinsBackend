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
    public class MatcherViewModel
    {
        private IMemoryCache _cache;
        private AuthenticationHelper _auth;

        public Guid UdfId { get; set; }
        public Matcher SelectedMatcher { get; set; }
        public MatcherCondition SelectedMatcherCondition { get; set; }
        public List<string> ConditionTargetList { get; set; }
        public List<string> ConditionComparisonList { get; set; }
        public IEnumerable<Models.Type> SensorDataTypeList { get; set; }
        
        public MatcherViewModel() { }

        public MatcherViewModel(IMemoryCache memoryCache, Guid? id = null, Guid? spaceId = null)
        {
            _cache = memoryCache;
            _auth = new AuthenticationHelper();

            try
            {
                LoadAsync(id, spaceId).Wait();
            }
            catch (Exception ex)
            {
                FeedbackHelper.Channel.SendMessageAsync($"Error - {ex.Message}", MessageType.Info).Wait();
                FeedbackHelper.Channel.SendMessageAsync($"Please check your settings.", MessageType.Info).Wait();
            }
        }

        private async Task LoadAsync(Guid? id = null, Guid? spaceId = null)
        {
            ConditionTargetList = new List<string>
            {
                "Sensor",
                "SensorDevice",
                "SensorSpace"
            };

            ConditionComparisonList = new List<string>
            {
                "Equals",
                "NotEquals",
                "Contains"
            };

            SensorDataTypeList = await DigitalTwinsHelper.GetTypesAsync(Types.SensorDataType, _cache, Loggers.SilentLogger, true);

            if (id != null)
            {
                this.SelectedMatcher = await DigitalTwinsHelper.GetMatcherAsync((Guid)id, _cache, Loggers.SilentLogger, false);
            }
            else
            {
                // if we are back from the creation of a MatcherCondition, we reload the working Matcher 
                Matcher matcher = (Matcher)CacheHelper.GetFromCache(_cache, Guid.Empty, Context.Matcher);
                if (matcher == null)
                {
                    matcher = new Matcher() { SpaceId = (Guid)spaceId };
                    CacheHelper.AddInCache(_cache, matcher, matcher.Id, Context.Matcher);
                }
                this.SelectedMatcher = matcher;
                this.SelectedMatcherCondition = new MatcherCondition();
            }
        }
    }
}
