using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;

using DigitalTwinsBackend.Helpers;
using DigitalTwinsBackend.Hubs;
using DigitalTwinsBackend.Models;
using System.Diagnostics;

namespace DigitalTwinsBackend.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            FeedbackHelper.Channel.SetHttpContextAccessor(httpContextAccessor);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Spaces()
        {
            ViewData["Message"] = "The page to manage your Spaces from Digital Twins.";

            return View();
        }

        public IActionResult Settings()
        {
            return View(ConfigHelper.Config.parameters);
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Settings(AppConfig parameters)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ConfigHelper.Config.parameters = parameters;
                    ConfigHelper.SaveConfig();

                    await FeedbackHelper.Channel.SendMessageAsync("Application settings have been saved.");
                }
                         
                
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
