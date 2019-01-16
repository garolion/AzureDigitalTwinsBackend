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

            UserDefinedFunction oldValue = null;
            if (Id != Guid.Empty)
            {
                oldValue = CacheHelper.GetUDFFromCache(memoryCache, Id);
                //changes.Add("Id", Id);

                if (oldValue != null)
                {
                    if (Name != null && !Name.Equals(oldValue.Name)) changes.Add("Name", Name);
                    if (!SpaceId.Equals(oldValue.SpaceId)) changes.Add("SpaceId", SpaceId);
                    if (Matchers!= null && !Matchers.Equals(oldValue.Matchers)) changes.Add("Matchers", Matchers);
                }
                else
                {
                    changes.Add("Name", Name);
                    changes.Add("SpaceId", SpaceId);
                    changes.Add("Matchers", GetMatchersIds());
                }
            }
            updatedElement = null;
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