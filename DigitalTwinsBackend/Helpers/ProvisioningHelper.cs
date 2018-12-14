using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinsBackend.Models
{
    public class DeviceDescription
    {
        public string hardwareId { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string subType { get; set; }

        [JsonIgnore]
        public IEnumerable<SensorDescription> sensors { get; set; }
    }

    public class EndpointDescription
    {
        public string type { get; set; }
        public string[] eventTypes { get; set; }
        public string connectionString { get; set; }
        public string secondaryConnectionString { get; set; }
        public string path { get; set; }
    }

    public class KeyStoreDescription
    {
        public string name { get; set; }
    }

    public class MatcherDescription
    {
        public string name { get; set; }
        public string dataTypeValue { get; set; }
    }

    public class ResourceDescription
    {
        public string type { get; set; }
        public string region { get; set; }
    }

    public class RoleAssignmentDescription
    {
        public string objectIdType { get; set; }
        public string objectName { get; set; }
        public Guid roleId { get; set; }
        public Guid tenantId { get; set; }
    }

    public class SensorDescription
    {
        public string hardwareId { get; set; }
        public string type { get; set; }
        public string dataType { get; set; }
        public string dataSubType { get; set; }
        public string dataUnitType { get; set; }

    }

    public class UserDefinedFunctionDescription
    {
        public string name { get; set; }
        public IEnumerable<string> matcherNames { get; set; }
        public string script { get; set; }
    }

    public class SpaceDescription
    {
        public string name { get; set; }
        public string friendlyName { get; set; }
        public string type { get; set; }
        public string subType { get; set; }

        [JsonIgnore]
        public IEnumerable<DeviceDescription> devices { get; set; }

        [JsonIgnore]
        public IEnumerable<KeyStoreDescription> keystores { get; set; }

        [JsonIgnore]
        public IEnumerable<MatcherDescription> matchers { get; set; }

        [JsonIgnore]
        public IEnumerable<RoleAssignmentDescription> roleassignments { get; set; }

        [JsonIgnore]
        public IEnumerable<ResourceDescription> resources { get; set; }

        [JsonIgnore]
        public IEnumerable<SpaceDescription> spaces { get; set; }

        [JsonIgnore]
        public IEnumerable<UserDefinedFunctionDescription> userdefinedfunctions { get; set; }
    }

    public static class DescriptionExtensions
    {
        public static Models.Device ToDevice(this DeviceDescription description, Guid spaceId)
            => new Models.Device()
            {
                HardwareId = description.hardwareId,
                Name = description.name,
                Type = description.type,
                SubType = description.subType,
                SpaceId = spaceId
            };

        public static Models.Endpoint ToEndpointCreate(this EndpointDescription description)
            => new Models.Endpoint()
            {
                ConnectionString = description.connectionString,
                EventTypes = description.eventTypes,
                Path = description.path,
                SecondaryConnectionString = description.secondaryConnectionString,
                Type = description.type,
            };

        public static Models.Matcher ToMatcher(this MatcherDescription description, Guid spaceId)
            => new Models.Matcher()
            {
                Name = description.name,
                SpaceId = spaceId.ToString(),
                Conditions = new[] {
                    new Models.MatcherCondition()
                    {
                        Target = "Sensor",
                        Path = "$.dataType",
                        Value = $"\"{description.dataTypeValue}\"",
                        Comparison = "Equals",
                    }
                }
            };

        public static Models.KeyStore ToKeyStoreCreate(this KeyStoreDescription description, Guid spaceId)
            => new Models.KeyStore()
            {
                Name = description.name,
                SpaceId = spaceId.ToString(),
            };

        public static Models.RoleAssignment ToRoleAssignment(this RoleAssignmentDescription description, Guid objectId, string path)
            => new Models.RoleAssignment()
            {
                ObjectId = objectId,
                ObjectIdType = description.objectIdType,
                Path = path,
                RoleId = description.roleId,
            };

        public static Models.Resource ToResource(this ResourceDescription description, Guid spaceId)
            => new Models.Resource()
            {
                SpaceId = spaceId,
                Type = description.type,
                Region = description.region,
            };

        public static Models.Sensor ToSensor(this SensorDescription description, Guid deviceId)
            => new Models.Sensor()
            {
                DeviceId = deviceId,
                HardwareId = description.hardwareId,
                Type = description.type,
                DataType = description.dataType,
                DataSubtype = description.dataSubType,
                DataUnitType = description.dataUnitType,
            };

        public static Space ToSpace(this SpaceDescription description, Guid parentId)
            => new Space()
            {
                Name = description.name,
                FriendlyName = description.friendlyName,
                ParentSpaceId = parentId != Guid.Empty ? parentId : Guid.Empty,
                Type = description.type,
                SubType = description.subType
            };

        //public static Models.UserDefinedFunction ToUserDefinedFunction(this UserDefinedFunctionDescription description, Guid Id, Guid spaceId, IEnumerable<Models.Matcher> matchers)
        //    => new Models.UserDefinedFunction()
        //    {
        //        Id = Id,
        //        Name = description.name,
        //        SpaceId = spaceId,
        //        Matchers = matchers,
        //    };

        public static Models.UserDefinedFunction ToUserDefinedFunction(this UserDefinedFunctionDescription description, Guid spaceId, IEnumerable<Matcher> matchers)
            => new Models.UserDefinedFunction()
            {
                Name = description.name,
                SpaceId = spaceId,
                Matchers = matchers,
            };

        //public static Models.UserDefinedFunction ToUserDefinedFunctionUpdate(this UserDefinedFunctionDescription description, Guid id, Guid spaceId, IEnumerable<Matcher> matchers)
        //    => new Models.UserDefinedFunction()
        //    {
        //        Id = id,
        //        Name = description.name,
        //        SpaceId = spaceId,
        //        Matchers = matchers,
        //    };
    }
}
