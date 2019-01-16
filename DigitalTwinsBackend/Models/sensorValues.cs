// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace DigitalTwinsBackend.Models
{
    public class SpaceValue
    {
        public string Type;
        public string Value;

        private string timeStamp;
        public string Timestamp
        {
            get { return timeStamp; }
            set
            {
                DateTime dt;
                if (DateTime.TryParse(value, out dt))
                {
                    timeStamp = $"{dt.ToShortDateString()} - {dt.ToShortDateString()}";
                }
                else
                {
                    timeStamp = value;
                }
            }
        }
        public IEnumerable<HistoricalValues> HistoricalValues;
    }
}