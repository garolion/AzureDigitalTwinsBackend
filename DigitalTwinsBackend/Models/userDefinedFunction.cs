// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinsBackend.Models
{
    public class UserDefinedFunction : BaseModel
    {
        public IEnumerable<Matcher> Matchers { get; set; }
        public string Name { get; set; }
        [Display(Name = "Space Id")]
        public Guid SpaceId { get; set; }

        public override string Label { get { return Name; } }

        public UserDefinedFunction()
        {
            Matchers = new List<Matcher>();
        }

        public override Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("Name", Name);
            createFields.Add("SpaceId", SpaceId);
            createFields.Add("Matchers", GetMatchersIds());

            return createFields;
        }

        public override Dictionary<string, object> ToUpdate(IMemoryCache memoryCache, out BaseModel updatedElement)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();

            UserDefinedFunction refInCache = null;
            if (Id != Guid.Empty)
            {
                refInCache = CacheHelper.GetUDFFromCache(memoryCache, Id);

                if (refInCache != null)
                {
                    if (Name != null && !Name.Equals(refInCache.Name)) changes.Add("Name", Name);
                    if (!SpaceId.Equals(refInCache.SpaceId)) changes.Add("SpaceId", SpaceId);
                    if (Matchers!= null && !Matchers.Equals(refInCache.Matchers)) changes.Add("Matchers", Matchers);
                }
                else
                {
                    refInCache = this;

                    if (Name != null) changes.Add("Name", Name);
                    if (SpaceId != null) changes.Add("SpaceId", SpaceId);
                    if (Matchers != null) changes.Add("Matchers", GetMatchersIds());
                }
            }
            updatedElement = refInCache;
            return changes;
        }

        private IEnumerable<Guid> GetMatchersIds()
        {
            List<Guid> matchersId = new List<Guid>();
            foreach (var matcher in Matchers)
            {
                matchersId.Add(matcher.Id);
            }
            return matchersId;
        }

    }
}