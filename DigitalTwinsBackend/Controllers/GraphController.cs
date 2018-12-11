using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinsBackend.Controllers
{
    public class GraphController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}