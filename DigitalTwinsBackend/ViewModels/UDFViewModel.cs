using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace DigitalTwinsBackend.ViewModels
{
    public class UDFViewModel
    {
        private IMemoryCache _cache;
        public UserDefinedFunction UDF { get; set; }
        public IEnumerable<Space> SpaceList { get; set; }
        public string Content { get; set; }

        public IFormFile UDFFile { get; set; }

        public UDFViewModel()
        {
        }

        public UDFViewModel(IMemoryCache memoryCache, UserDefinedFunction udf = null)
        {
            _cache = memoryCache;
            LoadAsync(udf).Wait();
        }

        private async Task LoadAsync(UserDefinedFunction udf)
        {
            SpaceList = await DigitalTwinsHelper.GetSpacesAsync(_cache, Loggers.SilentLogger);
            if (udf != null)
            {
                UDF = udf;
                Content = await DigitalTwinsHelper.GetUserDefinedFunctionContent(UDF.Id, _cache, Loggers.SilentLogger);

                //We get the matcher list to add the related conditions
                List<Matcher> matchersWithConditions = new List<Matcher>();
                var matchers = await DigitalTwinsHelper.GetMatchersBySpaceId(UDF.SpaceId, _cache, Loggers.SilentLogger, false);
                foreach (Matcher matcher in UDF.Matchers)
                {
                    matchersWithConditions.Add(matchers.First(t => t.Id == matcher.Id));
                }

                UDF.Matchers = new ObservableCollection<Matcher>(matchersWithConditions);
            }
            else
            {
                UDF = new UserDefinedFunction();
            }
        }
    }
}
