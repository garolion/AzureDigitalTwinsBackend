using DigitalTwinsBackend.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinsBackend.Models
{
    public enum ParentType
    {
        Space,
        Device,
        Sensor
    }

    public class ContentInfo
    {
        public string Type { get; set; }
        public string FilePath { get; set; }
        public string Version { get; set; }
        public int SizeBytes { get; set; }
        public string LastModifiedUtc { get; set; }
    }

    public class BlobContent : BaseModel
    {
        public string Name { get; set; }
        public Guid ParentId { get; set; }
        public ParentType ParentType { get; set; }
        public string Description { get; set; }
        public string Sharing { get; set; }
        public string Type { get; set; }
        public string Subtype { get; set; }
        public List<ContentInfo> ContentInfos { get; set; }

        public override string Label { get { return Name; } }

        public BlobContent()
        {
            ContentInfos = new List<ContentInfo>();
        }

        public override Dictionary<string, object> ToCreate()
        {
            Dictionary<string, object> createFields = new Dictionary<string, object>();

            createFields.Add("ParentId", ParentId);
            createFields.Add("Name", Name);
            createFields.Add("Description", Description);
            createFields.Add("Sharing", Sharing);
            createFields.Add("Type", Type);
            createFields.Add("Subtype", Subtype);

            return createFields;
        }

        public override Dictionary<string, object> ToUpdate(IMemoryCache memoryCache, out BaseModel updatedElement)
        {
            Dictionary<string, object> changes = new Dictionary<string, object>();

            BlobContent refInCache = null;
            if (Id != Guid.Empty)
            {
                refInCache = CacheHelper.GetBlobContentFromCache(memoryCache, Id);

                if (refInCache != null)
                {
                    if (Name != null && !Name.Equals(refInCache.Name)) changes.Add("Name", Name);
                    if (Description != null && !Description.Equals(refInCache.Description)) changes.Add("Description", Description);
                    if (!Sharing.Equals(refInCache.Sharing)) changes.Add("Sharing", Sharing);
                    if (!Type.Equals(refInCache.Type)) changes.Add("Type", Type);
                    if (!Subtype.Equals(refInCache.Subtype)) changes.Add("Subtype", Subtype);
                }
                else
                {
                    refInCache = this;

                    if (Name != null) changes.Add("Name", Name);
                    if (Description != null) changes.Add("Description", Description);
                    if (Sharing != null) changes.Add("Sharing", Sharing);
                    if (Type != null) changes.Add("Type", Type);
                    if (Subtype != null) changes.Add("Subtype", Subtype);
                }
            }
            updatedElement = refInCache;
            return changes;
        }
    }
}
