using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DigitalTwinsBackend.Models;

namespace DigitalTwinsBackend.ViewModels
{
    public class UDFViewModel
    {
        public UserDefinedFunction UDF { get; set; }

        public string Content { get; set; }

        //public IEnumerable<Matcher> Matchers { get; set; }
    }
}
