using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DigitalTwinsBackend.ViewModels
{
    public class UDFViewModel
    {
        private IMemoryCache _cache;
        public UserDefinedFunction UDF { get; set; }
        public string Content { get; set; }

        public UDFViewModel()
        {
        }

        public UDFViewModel(IMemoryCache memoryCache, UserDefinedFunction udf = null)
        {
            _cache = memoryCache;

            if (udf != null)
            {
                UDF = udf;
                LoadAsync().Wait();
            }
        }

        private async Task LoadAsync()
        {
            Content = await DigitalTwinsHelper.GetUserDefinedFunctionContent(UDF.Id, _cache, Loggers.SilentLogger);

            //We get the matcher list to add the related conditions
            List<Matcher> matchersWithConditions = new List<Matcher>();
            var matchers = await DigitalTwinsHelper.GetMatchersBySpaceId(UDF.SpaceId, _cache, Loggers.SilentLogger);
            foreach (Matcher matcher in UDF.Matchers)
            {
                matchersWithConditions.Add(matchers.First(t => t.Id == matcher.Id));
            }

            UDF.Matchers = matchersWithConditions;
        }
    }
}
