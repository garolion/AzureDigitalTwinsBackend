using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinsBackend.Models
{
    public class MatcherCondition
    {
        public Guid Id { get; set; }
        public string Target { get; set; }
        public string Path { get; set; }
        public string Value { get; set; }
        public string Comparison { get; set; }
        public bool isTrue { get; set; }

        public Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("Target", Target);
            createFields.Add("Path", Path);
            createFields.Add("Value", Value);
            createFields.Add("Comparison", Comparison);

            return createFields;
        }

        public Dictionary<string, object> ToUpdate(IMemoryCache memoryCache)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();

            changes.Add("Target", Target);
            changes.Add("Path", Path);
            changes.Add("Value", Value);
            changes.Add("Comparison", Comparison);

            return changes;
        }
    }
}
