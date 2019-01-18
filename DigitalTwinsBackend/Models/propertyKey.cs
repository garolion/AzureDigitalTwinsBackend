// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public enum Scope
    {
        Spaces,
        Sensors,
        Devices,
        Users,
        None
    }

    public enum PrimitiveDataType
    {
        Bool,
        String,
        Long,
        Int,
        Uint,
        DateTime,
        Set,
        Enum,
        Json
    }

    public class PropertyKey : BaseModel
    {
        //used to add the Property to a space in Space Edit mode
        public bool Add { get; set; }

        public new string Id { get; set; }
        public string Name { get; set; }
        public Scope Scope { get; set; }
        public Guid SpaceId { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public PrimitiveDataType PrimitiveDataType { get; set; }
        public string ValidationData { get; set; }
        public string Min { get; set; }
        public string Max { get; set; }

        public override string Label { get { return Name; } }
        
        public override Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("Name", Name);
            createFields.Add("PrimitiveDataType", this.PrimitiveDataType.ToString());
            if (Category != null && Category.Length > 0) createFields.Add("Category", Category);
            if (Description != null && Description.Length > 0) createFields.Add("Description", Description);
            createFields.Add("SpaceId", SpaceId);
            createFields.Add("Scope", Scope.ToString());
            if (ValidationData!=null && ValidationData.Length > 0) createFields.Add("ValidationData", ValidationData);
            if (Min != null && Min.Length > 0) createFields.Add("Min", Min);
            if (Max!=null && Max.Length > 0) createFields.Add("Max", Max);

            return createFields;
        }

        public override Dictionary<string, object> ToUpdate(IMemoryCache memoryCache, out BaseModel updatedElement)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();

            PropertyKey refInCache = null;
            if (Id != string.Empty)
            {
                refInCache = CacheHelper.GetPropertyKeyFromCache(memoryCache, Id);

                if (refInCache != null)
                {
                    if (!Name.Equals(refInCache.Name)) changes.Add("Name", Name);
                    if (!PrimitiveDataType.Equals(refInCache.PrimitiveDataType)) changes.Add("PrimitiveDataType", this.PrimitiveDataType.ToString());
                    if (!Category.Equals(refInCache.Category)) changes.Add("Category", Category);
                    if (!Description.Equals(refInCache.Description)) changes.Add("Description", Description);
                    if (!SpaceId.Equals(refInCache.SpaceId)) changes.Add("SpaceId", SpaceId);
                    if (!Scope.Equals(refInCache.Scope)) changes.Add("Scope", Scope.ToString());
                    if (!ValidationData.Equals(refInCache.ValidationData)) changes.Add("ValidationData", ValidationData);
                    if (!Min.Equals(refInCache.Min)) changes.Add("Min", Min);
                    if (!Max.Equals(refInCache.Max)) changes.Add("Max", Max);
                }
                else
                {
                    refInCache = this;

                    if (Name != null) changes.Add("Name", Name);
                    changes.Add("PrimitiveDataType", this.PrimitiveDataType.ToString());
                    if (Category != null)  changes.Add("Category", Category);
                    if (Description != null)  changes.Add("Description", Description);
                    if (SpaceId != null)  changes.Add("SpaceId", SpaceId);
                    changes.Add("Scope", Scope.ToString());
                    if (ValidationData != null)  changes.Add("ValidationData", ValidationData);
                    if (Min != null) changes.Add("Min", Min);
                    if (Max != null) changes.Add("Max", Max);
                }
            }
            updatedElement = refInCache;
            return changes;
        }
    }
}