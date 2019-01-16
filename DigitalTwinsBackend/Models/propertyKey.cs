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

            PropertyKey oldValue = null;
            if (Id != string.Empty)
            {
                oldValue = CacheHelper.GetPropertyKeyFromCache(memoryCache, Id);

                if (oldValue != null)
                {
                    if (!Name.Equals(oldValue.Name)) changes.Add("Name", Name);
                    if (!PrimitiveDataType.Equals(oldValue.PrimitiveDataType)) changes.Add("PrimitiveDataType", this.PrimitiveDataType.ToString());
                    if (!Category.Equals(oldValue.Category)) changes.Add("Category", Category);
                    if (!Description.Equals(oldValue.Description)) changes.Add("Description", Description);
                    if (!SpaceId.Equals(oldValue.SpaceId)) changes.Add("SpaceId", SpaceId);
                    if (!Scope.Equals(oldValue.Scope)) changes.Add("Scope", Scope.ToString());
                    if (!ValidationData.Equals(oldValue.ValidationData)) changes.Add("ValidationData", ValidationData);
                    if (!Min.Equals(oldValue.Min)) changes.Add("Min", Min);
                    if (!Max.Equals(oldValue.Max)) changes.Add("Max", Max);
                }
                else
                {
                    changes.Add("Name", Name);
                    changes.Add("PrimitiveDataType", this.PrimitiveDataType.ToString());
                    changes.Add("Category", Category);
                    changes.Add("Description", Description);
                    changes.Add("SpaceId", SpaceId);
                    changes.Add("Scope", Scope.ToString());
                    changes.Add("ValidationData", ValidationData);
                    changes.Add("Min", Min);
                    changes.Add("Max", Max);
                }
            }
            updatedElement = null;
            return changes;
        }
    }
}