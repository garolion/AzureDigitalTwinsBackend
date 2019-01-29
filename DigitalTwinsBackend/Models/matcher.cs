// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public class Matcher : BaseModel
    {
        public string Name { get; set; }
        public Guid SpaceId { get; set; }
        public List<MatcherCondition> Conditions { get; set; }
        public IEnumerable<UserDefinedFunction> UserDefinedFunctions { get; set; }
        public override string Label { get { return Name; } }

        public Matcher()
        {
            Conditions = new List<MatcherCondition>();
        }

        public override Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("Name", Name);
            createFields.Add("SpaceId", SpaceId);
            createFields.Add("Conditions", Conditions);

            return createFields;
        }

        public override Dictionary<string, object> ToUpdate(IMemoryCache memoryCache, out BaseModel updatedElement)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();

            Matcher refInCache = null;
            if (Id != Guid.Empty)
            {
                refInCache = CacheHelper.GetMatcherFromCache(memoryCache, Id);

                if (refInCache != null)
                {
                    if (Name != null && !Name.Equals(refInCache.Name)) changes.Add("Name", Name);
                    if (!SpaceId.Equals(refInCache.SpaceId)) changes.Add("SpaceId", SpaceId);
                    if (!Conditions.Equals(refInCache.Conditions)) changes.Add("Conditions", Conditions);
                    if (!UserDefinedFunctions.Equals(refInCache.UserDefinedFunctions)) changes.Add("UserDefinedFunctions", UserDefinedFunctions);
                }
                else
                {
                    refInCache = this;

                    if (Name != null) changes.Add("Name", Name);
                    if (SpaceId != null) changes.Add("SpaceId", SpaceId);
                    if (Conditions != null) changes.Add("Conditions", Conditions);
                    if (UserDefinedFunctions != null) changes.Add("UserDefinedFunctions", UserDefinedFunctions);
                }
            }
            updatedElement = refInCache;
            return changes;
        }
    }
}