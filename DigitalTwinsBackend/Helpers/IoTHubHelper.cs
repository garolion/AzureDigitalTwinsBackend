using DigitalTwinsBackend.Models;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace DigitalTwinsBackend.Helpers
{
    public class IoTHubHelper
    {
        private static Random rnd = new Random();

        private static Func<string> CreateGetRandomSensorReading(string sensorDataType, int iteration)
        {
            switch (sensorDataType)
            {
                default:
                    throw new Exception($"Unsupported SensorDataType '{sensorDataType}'.");
                case "Motion":
                    if (iteration % 6 < 3)
                        return () => "false";
                    else
                        return () => "true";

                case "Temperature":
                    return () => rnd.Next(70, 100).ToString(CultureInfo.InvariantCulture);
                case "CarbonDioxide":
                    if (iteration % 6 < 3)
                        return () => rnd.Next(800, 1000).ToString(CultureInfo.InvariantCulture);
                    else
                        return () => rnd.Next(1000, 1100).ToString(CultureInfo.InvariantCulture);
            }
        }

        public static async Task SendEvent(DeviceClient deviceClient, SimulatedSensor sensor)
        {
            var serializer = new DataContractJsonSerializer(typeof(CustomTelemetryMessage));
            var curIteration = 0;

            var getRandomSensorReading = CreateGetRandomSensorReading(sensor.DataType.ToString(), curIteration);
            var telemetryMessage = new CustomTelemetryMessage()
            {
                SensorValue = getRandomSensorReading(),
            };

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, telemetryMessage);
                var byteArray = stream.ToArray();
                Message eventMessage = new Message(byteArray);
                eventMessage.Properties.Add("DigitalTwins-Telemetry", "1.0");
                eventMessage.Properties.Add("DigitalTwins-SensorHardwareId", $"{sensor.HardwareId}");
                eventMessage.Properties.Add("CreationTimeUtc", DateTime.UtcNow.ToString("o"));
                eventMessage.Properties.Add("x-ms-client-request-id", Guid.NewGuid().ToString());

                await FeedbackHelper.Channel.SendMessageAsync(
                    $"\t{DateTime.UtcNow.ToLocalTime()}> Sending message: {Encoding.UTF8.GetString(eventMessage.GetBytes())} " +
                    $"Properties: {{ {eventMessage.Properties.Aggregate(new StringBuilder(), (sb, x) => sb.Append($"'{x.Key}': '{x.Value}',"), sb => sb.ToString())} }}"
                    , MessageType.Info);

                await deviceClient.SendEventAsync(eventMessage);
            }
        }
    }
}
